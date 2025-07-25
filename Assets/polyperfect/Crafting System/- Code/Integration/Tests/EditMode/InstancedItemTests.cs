using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration.Tests
{
    public class InstancedItemTests
    {
        [Test]
        public void GetOriginalFromInstance()
        {
            var world = new SimpleItemWorld();
            var originalItem = RuntimeID.Random();
            var instance = world.CreateInstance(originalItem);
            Assert.AreEqual(originalItem,world.GetArchetypeFromInstance(instance));
        }
        [Test]
        public void InstancesHaveBaseValues()
        {
            var world = new SimpleItemWorld();
            var originalItem = RuntimeID.Random();
            var categoryWithData = RuntimeID.Random();
            var categoryWithoutData = RuntimeID.Random();
            world.AddItem(originalItem,"Gold");
            world.AddCategoryWithData(categoryWithData,"MyCategory",new Dictionary<RuntimeID,string>());
            world.AddCategory(categoryWithoutData,"WithoutData");
            world.AddItemToCategory(categoryWithData,originalItem,"Original Item");
            world.AddItemToCategory(categoryWithoutData,originalItem);
            var instance = world.CreateInstance(originalItem);
            var accessor = world.GetReadOnlyAccessor<string>(categoryWithData);
            Assert.AreEqual("Original Item",accessor[originalItem]);
            Assert.AreEqual("Original Item" ,accessor[instance]);
            Assert.AreEqual("Gold",world.GetName(originalItem));
            Assert.AreEqual("Gold",world.GetName(instance));
            Assert.IsTrue(world.CategoryContains(categoryWithoutData,instance));
        }
        
        [Test]
        public void SetValuesPersistAndDontAffectOriginal()
        {
            var world = new SimpleItemWorld();
            var originalItem = RuntimeID.Random();
            var category = RuntimeID.Random();
            world.AddCategoryWithData(category,"MyCategory",new Dictionary<RuntimeID,string>());
            world.AddItemToCategory(category,originalItem,"Original Item");
            var instance = world.CreateInstance(originalItem);
            world.SetInstanceData(category,instance,"Instantiated Item");
            var accessor = world.GetReadOnlyAccessor<string>(category);
            Assert.AreEqual("Original Item",accessor[originalItem]);
            Assert.AreEqual("Instantiated Item" ,accessor[instance]);
        }
        [Test]
        public void InstancesAreDeleted()
        {
            var world = new SimpleItemWorld();
            var originalItem = RuntimeID.Random();
            var category = RuntimeID.Random();
            var categoryWithoutData = RuntimeID.Random();
            world.AddCategoryWithData(category,"MyCategory",new Dictionary<RuntimeID,string>());
            world.AddCategory(categoryWithoutData,"WithoutData");
            world.AddItemToCategory(category,originalItem,"Original Item");
            world.AddItemToCategory(categoryWithoutData,originalItem);
            var instance = world.CreateInstance(originalItem);
            Assert.AreEqual(2,world.GetReadOnlyAccessor<string>(category).Count);
            Assert.AreEqual(1,world.GetReadOnlyAccessor<RuntimeID>(StaticCategories.Archetypes).Count);
            Assert.IsTrue(world.CategoryContains(categoryWithoutData,instance));
            world.DeleteInstance(instance);
            Assert.AreEqual(1 ,world.GetReadOnlyAccessor<string>(category).Count);
            Assert.AreEqual(0,world.GetReadOnlyAccessor<RuntimeID>(StaticCategories.Archetypes).Count);
            Assert.IsFalse(world.CategoryContains(categoryWithoutData,instance));
        }
        
        [Test]
        public void CategoriesAddedAfterHaveDataPropagated()
        {
            var world = new SimpleItemWorld();
            var originalItem = RuntimeID.Random();
            var individuallyAddedCategory = RuntimeID.Random();
            var massAddedCategory = RuntimeID.Random();
            var massDictionary = new Dictionary<RuntimeID,string>();
            massDictionary.Add(originalItem,"Original Item Mass");
            var instance = world.CreateInstance(originalItem);
            world.AddCategoryWithData(individuallyAddedCategory,"MyCategory",new Dictionary<RuntimeID,string>());
            world.AddCategoryWithData(massAddedCategory,"MyCategoryMass",massDictionary);
            world.AddItemToCategory(individuallyAddedCategory,originalItem,"Original Item");
            var individualAccessor = world.GetReadOnlyAccessor<string>(individuallyAddedCategory);
            var massAccessor = world.GetReadOnlyAccessor<string>(massAddedCategory);
            Assert.AreEqual("Original Item Mass",massAccessor[originalItem]);
            Assert.AreEqual("Original Item Mass" ,massAccessor[instance]);
            Assert.AreEqual("Original Item",individualAccessor[originalItem]);
            Assert.AreEqual("Original Item" ,individualAccessor[instance]);
        }

        [Test]
        public void InstanceSerialization()
        {
            var world = new SimpleItemWorld();
            var archetype = new RuntimeID(25);
            var category = new RuntimeID(692);
            var categoryWithData = new RuntimeID(4209);
            var otherCategory = new RuntimeID(23);
            var unhandleableCategory = new RuntimeID(3902);
            world.AddItem(archetype,"Archetype");
            world.AddCategory(category,"MyCategory");
            world.AddCategory(otherCategory,"OtherCategory");
            world.AddCategoryWithData(categoryWithData,"CategoryWithData", new Dictionary<RuntimeID,string>());
            world.AddItemToCategory(category,archetype);
            world.AddCategoryWithData(unhandleableCategory,"Unhandleable",new Dictionary<RuntimeID,Sprite>());
            world.AddItemToCategory(unhandleableCategory,archetype);
            
            var instance = world.CreateInstance(archetype);
            world.SetInstanceData(categoryWithData,instance,"MyValidData");
            var data = InventorySaver.CreateSaveObject(world,world.ItemIDs.Select(i=>new ItemStack(i,1)));
            var savedInstance = data.Instances[0];
            
            Assert.AreEqual(instance,savedInstance.ID);
            Assert.IsTrue(savedInstance.Categories.Contains(category));
            Assert.IsTrue(savedInstance.Categories.Contains(StaticCategories.Archetypes));
            Assert.IsFalse(savedInstance.Categories.Contains(otherCategory));
            Assert.IsTrue(savedInstance.Categories.Contains(categoryWithData));
            Assert.AreEqual(archetype,InventorySaver.Deserialize<RuntimeID>(savedInstance.SerializedData[savedInstance.Categories.IndexOf(StaticCategories.Archetypes)]));
            Assert.AreEqual("MyValidData",InventorySaver.Deserialize<string>(savedInstance.SerializedData[savedInstance.Categories.IndexOf(categoryWithData)]));
        }
        [Test]
        public void InstanceDeserialization()
        {
            var world = new SimpleItemWorld();
            var archetype = new RuntimeID(25);
            var category = new RuntimeID(692);
            var otherCategory = new RuntimeID(23);
            var categoryWithData = new RuntimeID(4209);
            var unhandleableCategory = new RuntimeID(3902);
            world.AddItem(archetype,"Archetype");
            world.AddCategory(category,"MyCategory");
            world.AddCategory(otherCategory,"OtherCategory");
            world.AddCategoryWithData(categoryWithData,"CategoryWithData", new Dictionary<RuntimeID,string>());
            world.AddItemToCategory(category,archetype);
            world.AddCategoryWithData(unhandleableCategory,"Unhandleable",new Dictionary<RuntimeID,Sprite>());
            world.AddItemToCategory(unhandleableCategory,archetype);
            var instance = world.CreateInstance(archetype);
            world.SetInstanceData(categoryWithData,instance,"MyValidData");
            var data = InventorySaver.CreateSaveObject(world,world.ItemIDs.Select(i=>new ItemStack(i,1)));
            var dataJson = JsonUtility.ToJson(data);

            var loadedData = JsonUtility.FromJson<InventorySaver.SerializedInventory>(dataJson);
            var newWorld = new SimpleItemWorld();
            newWorld.AddItem(archetype,"Archetype");
            newWorld.AddCategory(category,"MyCategory");
            newWorld.AddCategory(otherCategory,"OtherCategory");
            newWorld.AddCategoryWithData(categoryWithData,"CategoryWithData", new Dictionary<RuntimeID,string>());
            newWorld.AddItemToCategory(category,archetype);
            newWorld.AddCategoryWithData(unhandleableCategory,"Unhandleable",new Dictionary<RuntimeID,Sprite>());
            newWorld.AddItemToCategory(unhandleableCategory,archetype);

            InventorySaver.LoadInstancesIntoWorld(loadedData.Instances,newWorld);
            Assert.AreEqual("MyValidData",newWorld.GetReadOnlyAccessor<string>(categoryWithData)[instance]);
            Assert.AreEqual(archetype,newWorld.GetReadOnlyAccessor<RuntimeID>(StaticCategories.Archetypes)[instance]);
        }
        [Test]
        public void SerializeTests()
        {
            Assert.AreEqual(5,ReverseTranslate(5));
            Assert.AreEqual(3f,ReverseTranslate(3f));
            Assert.AreEqual("Have a cookie",ReverseTranslate("Have a cookie"));
        }

        static T ReverseTranslate<T>(T obj) => InventorySaver.Deserialize<T>(InventorySaver.Serialize(obj));
    }
}