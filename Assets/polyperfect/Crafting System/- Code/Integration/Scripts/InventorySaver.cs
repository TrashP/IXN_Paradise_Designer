using System;
using System.Collections.Generic;
using Polyperfect.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Polyperfect.Crafting.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

namespace Polyperfect.Crafting.Integration
{
    [RequireComponent(typeof(BaseItemStackInventory))]
    public class InventorySaver : ItemUserBase
    {
        public override string __Usage => "Easy save and load of inventories.";

        [Serializable]
        public class SerializedInventory
        {
            [SerializeField] public List<ItemStack> Items = new List<ItemStack>();
            [SerializeField] public List<SerializedInstance> Instances = new List<SerializedInstance>();
        }
        [Serializable]
        public class SerializedInstance
        {
            [SerializeField] public RuntimeID ID;
            [SerializeField] public RuntimeID[] Categories;
            [SerializeField] public string[] SerializedData;
        }

        public string FileName = "MyInventory";
        public string FileExtension = "inv";

        public bool LoadOnStart = true;
        [FormerlySerializedAs("SaveOnQuit")] public bool SaveOnDestroy = true;
        public LoadMode LoadMethod = LoadMode.Replace;

        IItemWorld cachedWorld;
        public enum LoadMode
        {
            Replace,
            Add
        }

        const string INVENTORY_FOLDER = "Inventories";

        BaseItemStackInventory _attached;
        bool initialized;

        protected void Start()
        {
            cachedWorld = World;
            _attached = GetComponent<BaseItemStackInventory>();
            initialized = true;
            if (LoadOnStart)
                LoadFromFile();
        }

        public void LoadFromFile() => LoadFromFile(GetPath());

        public void LoadFromFile(string path)
        {
            if (!File.Exists(path))
                return;
            var json = File.ReadAllText(path);
            var serialized = JsonUtility.FromJson<SerializedInventory>(json);
            if (ValidateWorld(cachedWorld))
                LoadInstancesIntoWorld(serialized.Instances,cachedWorld as SimpleItemWorld);
            if (LoadMethod == LoadMode.Replace)
                _attached.ExtractAll();
            _attached.InsertPossible(serialized.Items);
        }

        public void SaveToFile() => SaveToFile(GetPath());

        public void SaveToFile(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory ?? throw new InvalidOperationException());
            var serialized = CreateSaveObject(cachedWorld,_attached.Peek());
            var json = JsonUtility.ToJson(serialized);
            File.WriteAllText(path, json);
        }

        public static SerializedInventory CreateSaveObject(IItemWorld world,IEnumerable<ItemStack> inventory)
        {
            var serialized = new SerializedInventory();
            serialized.Items.AddRange(inventory);
            var instanceList = serialized.Instances;
            if (ValidateWorld(world))
            {
                var directWorld = (SimpleItemWorld)world;
                foreach (var instance in serialized.Items.Where(i => world.CategoryContains(StaticCategories.Archetypes, i.ID)).Select(i => i.ID))
                {
                    var saveInstance = new SerializedInstance();
                    saveInstance.ID = instance;
                    var categories = world.CategoryIDs.Where(c => world.CategoryContains(c, instance)).ToArray();
                    saveInstance.Categories = categories;
                    saveInstance.SerializedData = categories.Select(c =>
                        directWorld.TryGetUntypedAccessor(c, out var lookup) ? Serialize(directWorld.GetCategoryDataType(c), lookup[instance]) : "").ToArray();
                    instanceList.Add(saveInstance);

                }
            }
            else
            {
                Debug.LogError($"There must be an Item World in the scene that is a {nameof(SimpleItemWorld)} in order to use the instance saving capabilities of the {nameof(InventorySaver)} script.");
            }

            return serialized;
        }

        public string GetPath() => Path.ChangeExtension(Path.Combine(Application.persistentDataPath, INVENTORY_FOLDER, FileName), FileExtension);

        void OnDisable()
        {
            if (initialized && SaveOnDestroy)
                SaveToFile();
        }

        //The following is necessary because Unity cannot serialize basic value types like int or string directly, they need some sort of wrapper.
        //However, a Generic wrapper is not doable since the types are not known ahead of time, and an object wrapper just kicks the can down and 
        //still doesn't work. Also needed to avoid additional dependencies from something like Newtonsoft Json
        public static readonly Dictionary<Type, Func<string, object>> DeserializationFunctions = new Dictionary<Type, Func<string, object>>();
        public static readonly Dictionary<Type, Func<object,string>> SerializationFunctions = new Dictionary<Type, Func<object, string>>();
        public static object Deserialize(Type targetType, string str) => DeserializationFunctions.TryGetValue(targetType, out var func) ? func(str) : null;

        public static T Deserialize<T>(string str) => (T)Deserialize(typeof(T), str);

        public static bool IsDataSerializable(Type type) => type != null && SerializationFunctions.ContainsKey(type);
        public static string Serialize(Type targetType, object obj) => SerializationFunctions.TryGetValue(targetType,out var func) ? func(obj) : null;
        public static string Serialize<T>(T obj) => Serialize(typeof(T), obj);

        public static void RegisterSerializeDeserializePair(Type type, Func<object, string> serialize, Func<string, object> deserialize)
        {
            DeserializationFunctions.Add(type,deserialize);
            SerializationFunctions.Add(type,serialize);
        }
        public static void LoadInstancesIntoWorld(IEnumerable<SerializedInstance> loadedData,SimpleItemWorld world)
        {
            Profiler.BeginSample(nameof(LoadInstancesIntoWorld));
            foreach (var item in loadedData)
            {
                var instanceID = item.ID;
                var indexOfArchetypeData = item.Categories.IndexOf(StaticCategories.Archetypes);
                world.InitializeInstance(Deserialize<RuntimeID>(item.SerializedData[indexOfArchetypeData]),instanceID);
                for (var i = 0; i < item.Categories.Length; i++)
                {
                    var currentCategory = item.Categories[i];
                    try
                    {
                        var categoryType = world.GetCategoryDataType(currentCategory);
                        if (IsDataSerializable(categoryType) && world.TryGetUntypedAccessor(currentCategory, out var lookup))
                            lookup[instanceID] = Deserialize(categoryType, item.SerializedData[i]);
                        else if (!world.CategoryContains(currentCategory, instanceID))
                            world.AddItemToCategory(currentCategory, instanceID);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"On {item.Categories[i]} caught error\n{e} ");
                    }
                }
            }
            Profiler.EndSample();
        }
        static InventorySaver()
        {
            RegisterSerializeDeserializePair(
                typeof(float),
                o=>((float)o).ToString(CultureInfo.InvariantCulture),
                s=>float.Parse(s));
            RegisterSerializeDeserializePair(
                typeof(int),
                o=>((int)o).ToString(CultureInfo.InvariantCulture),
                s=>int.Parse(s));
            RegisterSerializeDeserializePair(
                typeof(string),
                o=>(string)o,
                s=>s);
            RegisterSerializeDeserializePair(
                typeof(RuntimeID),
                o=>((RuntimeID)o).NumericID.ToString(CultureInfo.InvariantCulture),
                s=>new RuntimeID(long.Parse(s)));
        }
        
        
        static bool ValidateWorld(IItemWorld world)
        {
            return world is SimpleItemWorld;
        }
    }
    
}