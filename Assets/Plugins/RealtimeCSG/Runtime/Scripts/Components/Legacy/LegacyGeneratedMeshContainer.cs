using UnityEngine;

namespace InternalRealtimeCSG
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public sealed class LegacyGeneratedMeshContainer : MonoBehaviour
    {
        void Awake()
        {
            UnityEngine.Object.DestroyImmediate(this.gameObject);
        }
    }
}
