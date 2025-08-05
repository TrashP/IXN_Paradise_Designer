using UnityEngine;

namespace MagicSystem
{
    public abstract class BaseMagic : MonoBehaviour
    {
        public abstract void Execute(Vector3 position);
    }
}
