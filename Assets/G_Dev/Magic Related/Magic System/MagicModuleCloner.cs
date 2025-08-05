using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace MagicSystem
{
    public static class MagicModuleCloner
    {
        public static void CloneMagicModules(GameObject source, GameObject target)
        {
            var sourceEnabler = source.GetComponent<MagicEnabler>();
            if (!sourceEnabler)
            {
                Debug.LogWarning($"[MagicClone] Source {source.name} has no MagicEnabler.");
                return;
            }

            var targetEnabler = target.GetComponent<MagicEnabler>();
            if (!targetEnabler)
                targetEnabler = target.AddComponent<MagicEnabler>();

            int count = 0;

            foreach (var originalModule in sourceEnabler.Modules) 
            {
                if (originalModule == null)
                    continue;

                var type = originalModule.GetType();
                var newModule = target.AddComponent(type) as BaseMagic;

             
                var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var fields = type.GetFields(flags);

                foreach (var field in fields)
                {
                    if (field.IsNotSerialized) continue;
                    var value = field.GetValue(originalModule);
                    field.SetValue(newModule, value);
                }

                targetEnabler.AddModule(newModule); 
            }

            Debug.Log($"[MagicClone] Cloned {count} magic module(s) from '{source.name}' to '{target.name}'.");
        }
    }
}
