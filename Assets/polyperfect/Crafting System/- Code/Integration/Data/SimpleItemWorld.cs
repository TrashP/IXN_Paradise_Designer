using System;
using System.Collections;
using System.Collections.Generic;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    public class SimpleItemWorld : IItemWorld,IDataAccessorLookupWithArg<RuntimeID,RuntimeID>
    {
        readonly Dictionary<RuntimeID, IDictionary> categoryDataLookups = new Dictionary<RuntimeID, IDictionary>();
        readonly Dictionary<RuntimeID, HashSet<RuntimeID>> categoryElements = new Dictionary<RuntimeID, HashSet<RuntimeID>>();
        readonly List<RuntimeID> items = new List<RuntimeID>();
        readonly List<RuntimeID> categories = new List<RuntimeID>();
        readonly List<RuntimeID> recipes = new List<RuntimeID>();
        readonly Dictionary<RuntimeID, string> names = new Dictionary<RuntimeID, string>();
        //readonly Dictionary<RuntimeID, RuntimeID> archetypes = new Dictionary<RuntimeID, RuntimeID>();
        //readonly Dictionary<RuntimeID, SimpleRecipe> recipes = new Dictionary<RuntimeID, SimpleRecipe>();
        public IEnumerable<RuntimeID> ItemIDs => items;
        public IEnumerable<RuntimeID> CategoryIDs => categories;
        public IEnumerable<RuntimeID> RecipeIDs => recipes;
        public bool CategoryContains(RuntimeID category, RuntimeID itemID)
        {
            if (categoryElements.ContainsKey(category))
                return categoryElements[category].Contains(itemID);
            Debug.LogError(
                $"Category {category} does not exist in the item world. Make sure it's included in at least one {nameof(ItemWorldFragment)} in the {nameof(ItemWorldReference)}.");
            return false;
        }

        public bool HasItem(RuntimeID id)
        {
            return items.Contains(id);
        }

        public bool HasCategory(RuntimeID id)
        {
            return categoryElements.ContainsKey(id);
        }

        public IEnumerable<RuntimeID> CategoryMembers(RuntimeID categoryID) => categoryElements[categoryID];

        public SimpleItemWorld()
        {
            AddCategoryWithData(StaticCategories.Names,nameof(StaticCategories.Names),names);
            AddCategoryWithData(StaticCategories.Archetypes,nameof(StaticCategories.Archetypes),new Dictionary<RuntimeID,RuntimeID>());
            AddCategoryWithData(StaticCategories.RecipeInputs, nameof(StaticCategories.RecipeInputs),new Dictionary<RuntimeID,IEnumerable<ItemStack>>());
            AddCategoryWithData(StaticCategories.RecipeOutputs, nameof(StaticCategories.RecipeOutputs),new Dictionary<RuntimeID,IEnumerable<ItemStack>>());
        }
        //public IReadOnlyDictionary<RuntimeID, SimpleRecipe> Recipes => recipes;
        public IReadOnlyDictionary<RuntimeID, VALUE> GetReadOnlyAccessor<VALUE>(RuntimeID id)
        {
            if (categoryDataLookups.TryGetValue(id, out var contained))
            {
                if (!(contained is Dictionary<RuntimeID, VALUE> dictionary))
                    throw new ArrayTypeMismatchException($"The collection with ID {id} was not of the expected type {typeof(VALUE).Name}");
                return dictionary;
            }

            Debug.LogError($"No category has the id {id}");
            return null;
        }

        public bool TryGetUntypedAccessor(RuntimeID id, out IDictionary lookup)
        {
            return categoryDataLookups.TryGetValue(id, out lookup);
        }
        
        public IDictionary<RuntimeID, VALUE> GetAccessor<VALUE>(RuntimeID id)
        {
            if (categoryDataLookups.TryGetValue(id, out var contained))
            {
                if (!(contained is Dictionary<RuntimeID, VALUE> dictionary))
                    throw new ArrayTypeMismatchException($"The collection with ID {id} was not of the expected type {typeof(VALUE).Name}");
                return dictionary;
            }

            Debug.LogError($"No category has the id {id}");
            return null;
        }
        
        public void AddItem(RuntimeID item,string name)
        {
            items.Add(item);
            AddItemToCategory(StaticCategories.Names,item,name);
        }

        public void AddCategory(RuntimeID category, string name,IEnumerable<RuntimeID> elements = null)
        {
            categoryDataTypeLookup[category] = null;
            categories.Add(category);
            var idLookup = new HashSet<RuntimeID>();
            categoryElements.Add(category,idLookup);
            if (elements !=null)
                    idLookup.UnionWith(elements);
            names[category] = name;
        }

        public void AddCategoryWithData<T>(RuntimeID category, string name, IDictionary<RuntimeID, T> useOrAddDataLookup)
        {
            AddCategoryWithData(category,name,(IDictionary)useOrAddDataLookup,typeof(T));
        }
        public void AddCategoryWithData(RuntimeID category,string name,IDictionary useOrAddDataLookup,Type dataType)
        {
            categoryDataTypeLookup[category] = dataType;
            if (!categoryElements.ContainsKey(category))
            {
                categories.Add(category);
                categoryElements.Add(category, new HashSet<RuntimeID>());
            }

            if (!categoryDataLookups.ContainsKey(category))
            {                
                categoryDataLookups.Add(category,useOrAddDataLookup);
                foreach (DictionaryEntry item in useOrAddDataLookup)
                    categoryElements[category].Add((RuntimeID)item.Key);
            }
            else
            {
                foreach (DictionaryEntry item in useOrAddDataLookup)
                {
                    if (categoryDataLookups[category].Contains(item.Key))
                    {
                        Debug.LogError($"{name} already contained {item.Key}, skipping");
                        continue;
                    }
                    categoryDataLookups[category].Add(item.Key,item.Value);
                    categoryElements[category].Add((RuntimeID)item.Key);
                }
            }
            names[category] = name;
            
            //propagate to instances
            var toAdd = new List<Tuple<RuntimeID,object>>();
            foreach (DictionaryEntry item in useOrAddDataLookup)
            {
                var originalItemID = (RuntimeID)item.Key;
                if (!instanceLookups.ContainsKey(originalItemID)) 
                    continue;

                foreach (var instance in instanceLookups[originalItemID])
                {
                    if (!CategoryContains(category,instance))
                        toAdd.Add(new Tuple<RuntimeID, object>(instance,item.Value));
                }
            }
            foreach (var (item1,item2) in toAdd)
                AddItemToCategoryUntyped(category, item1, item2);
        }

        public void AddRecipe(RuntimeID id,IEnumerable<ItemStack> inputs,IEnumerable<ItemStack> outputs, string name)
        {
            recipes.Add(id);
            AddItemToCategory(StaticCategories.RecipeInputs,id,inputs);
            AddItemToCategory(StaticCategories.RecipeOutputs,id,outputs);
            names[id] = name;
        }

        public void AddItemToCategory(RuntimeID categoryID, RuntimeID itemID)
        {
            categoryElements[categoryID].Add(itemID);

            if (instanceLookups.ContainsKey(itemID))
            {
                foreach (var instance in instanceLookups[itemID])
                    if (!CategoryContains(categoryID,instance))
                        AddItemToCategory(categoryID,instance);
                    
            }
        }

        public void AddItemToCategory<DATA>(RuntimeID categoryID, RuntimeID itemID, DATA data)
        {
            categoryElements[categoryID].Add(itemID);
            if (categoryDataLookups.TryGetValue(categoryID, out var contained))
            {
                if (!(contained is Dictionary<RuntimeID, DATA> dictionary))
                    throw new ArrayTypeMismatchException($"The collection with ID {categoryID} was not of the expected type {typeof(DATA).Name}");
                dictionary[itemID] = data;
            }
            if (instanceLookups.ContainsKey(itemID))
            {
                foreach (var instance in instanceLookups[itemID])
                    if (!CategoryContains(categoryID,instance))
                        AddItemToCategory(categoryID,instance,data);
                    
            }
        }
        void AddItemToCategoryUntyped(RuntimeID categoryID, RuntimeID itemID, object data)
        {
            categoryElements[categoryID].Add(itemID);
            categoryDataLookups[categoryID].Add(itemID,data);
        }

        //public IReadOnlyDictionary<RuntimeID, HashSet<RuntimeID>> AllInstances=>instanceLookups;
        readonly Dictionary<RuntimeID, HashSet<RuntimeID>> instanceLookups = new Dictionary<RuntimeID, HashSet<RuntimeID>>();
        public void SetInstanceData<T>(RuntimeID category, RuntimeID instance, T data)
        {
            if (!CategoryContains(StaticCategories.Archetypes, instance))
                throw new Exception($"Only data values for instantiated items can be set via {nameof(SetInstanceData)}.");
            if (!categoryElements[category].Contains(instance))
                categoryElements[category].Add(instance);
            categoryDataLookups[category][instance] = data;
        }
        public RuntimeID CreateInstance(RuntimeID archetype)
        {
            if (CategoryContains(StaticCategories.Archetypes, archetype))
                throw new Exception("Instantiating instances is not supported at this time.");
            var instance = RuntimeID.Random();
            
            InitializeInstance(archetype, instance);

            return instance;
        }

        public void InitializeInstance(RuntimeID archetype, RuntimeID instance)
        {
            items.Add(instance);
            AddToInstanceLookup(archetype, instance);
            AddItemToCategory(StaticCategories.Archetypes, instance, archetype);
            foreach (var category in categories)
            {
                if (CategoryContains(category, archetype))
                {
                    if (categoryDataLookups.ContainsKey(category) && categoryDataLookups[category].Contains(archetype))
                        AddItemToCategoryUntyped(category, instance, categoryDataLookups[category][archetype]);
                    else
                        AddItemToCategory(category, instance);
                }
            }
        }

        void AddToInstanceLookup(RuntimeID archetype, RuntimeID instance)
        {
            if (!instanceLookups.ContainsKey(archetype))
                instanceLookups.Add(archetype, new HashSet<RuntimeID>());
            instanceLookups[archetype].Add(instance);
        }

        public void DeleteInstance(RuntimeID instance)
        {
            if (!CategoryContains(StaticCategories.Archetypes, instance))
                throw new Exception($"Only instantiated items can be deleted via {nameof(DeleteInstance)}.");
            instanceLookups[GetReadOnlyAccessor<RuntimeID>(StaticCategories.Archetypes)[instance]].Remove(instance);
            
            foreach (var category in categories)
            {
                if (CategoryContains(category, instance)) 
                    categoryElements[category].Remove(instance);
                if (categoryDataLookups.ContainsKey(category)&&categoryDataLookups[category].Contains(instance))
                    categoryDataLookups[category].Remove(instance);
            }
        }

        Dictionary<RuntimeID, Type> categoryDataTypeLookup = new Dictionary<RuntimeID, Type>();
        public Type GetCategoryDataType(RuntimeID category) => categoryDataTypeLookup[category];
    }
}