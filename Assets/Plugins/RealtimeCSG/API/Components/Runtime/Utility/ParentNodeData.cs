using System;

namespace RealtimeCSG
{
#if UNITY_EDITOR
    [Serializable]
    public sealed class ParentNodeData : HierarchyItem
    {
        public bool ChildrenModified = true;

        public override void Reset()
        {
            base.Reset();
            ChildrenModified = true;
        }
    }
#endif
}