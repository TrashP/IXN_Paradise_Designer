using System.Collections.Generic;
using UnityEngine;

namespace MagicSystem
{
    public class MagicEnabler : MonoBehaviour
    {
        [SerializeField]
        private List<BaseMagic> magicModules = new List<BaseMagic>();

        public IReadOnlyList<BaseMagic> Modules => magicModules;

        public void AddModule(BaseMagic module)
        {
            if (module != null && !magicModules.Contains(module))
                magicModules.Add(module);
        }
        public T GetMagic<T>() where T : BaseMagic
        {
            foreach (var module in magicModules)
            {
                if (module is T matched)
                    return matched;
            }
            return null;
        }
    }

}
