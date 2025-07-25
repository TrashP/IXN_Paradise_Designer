using System;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Common;
using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    [CustomEditor(typeof(ItemStackConstraintCollection),true,isFallback = true)]
    public class ItemStackConstraintCollectionDrawer : Editor
    {
        
        static List<Type> types;
        public override VisualElement CreateInspectorGUI()
        {
            TryInitTypes();
            var ve = new VisualElement();
            Populate(ve);
            
            return ve;
        }

        void Populate(VisualElement ve)
        {
            ve.Clear();
            var constraintProp = serializedObject.FindProperty(nameof(ItemStackConstraintCollection.Constraints));

            if (constraintProp.arraySize > 0)
            {
                var arrayIterator = constraintProp.GetArrayElementAtIndex(0);
                var index = -1;
                do
                {
                    index++;
                    var noHoist = index;
                    var container = new VisualElement().SetRow();

                    var serializedProperty = arrayIterator.Copy();
                    serializedProperty.isExpanded = true;
                    var propertyField = new PropertyField(serializedProperty)
                    {
                        label = serializedProperty.managedReferenceFullTypename.Substring(serializedProperty.managedReferenceFullTypename.LastIndexOf('.') + 1),
                        name = "PropertyField:" + arrayIterator.propertyPath
                    }.SetGrow();
                    container.Add(propertyField);
                    container.Add(new Button(()=>
                    {
                        constraintProp.DeleteArrayElementAtIndex(noHoist);
                        serializedObject.ApplyModifiedProperties();
                        Populate(ve);
                    }) { text = "-" });
                    ve.Add(container);
                    container.style.marginBottom = 16f;
                } while (arrayIterator.NextVisible(false));
            }

            var addButton = new Label("Add Constraint");
            addButton.AddToClassList("unity-button");
            var cmm = new ContextualMenuManipulator(e =>
            {
                foreach (var item in types)
                {
                    var nohoist = item;
                    e.menu.AppendAction(nohoist.Name, a =>
                    {
                        constraintProp.InsertArrayElementAtIndex(constraintProp.arraySize);
                        var elem = constraintProp.GetArrayElementAtIndex(constraintProp.arraySize - 1);
                        elem.managedReferenceValue = Activator.CreateInstance(nohoist);
                        serializedObject.ApplyModifiedProperties();
                        Populate(ve);
                    });
                }
            });
            cmm.activators.Clear();
            cmm.activators.Add(new ManipulatorActivationFilter(){button = MouseButton.LeftMouse});
            addButton.AddManipulator(cmm);
            ve.Add(addButton);
            ve.Bind(serializedObject);
        }
        
        static void TryInitTypes()
        {
            if (types != null)
                return;
            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes()).Where(t=>t.IsSubclassOf(typeof(StackInsertionConstraint))).ToList();
        }
    }
}