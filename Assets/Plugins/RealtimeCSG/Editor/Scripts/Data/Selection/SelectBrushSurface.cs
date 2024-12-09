using RealtimeCSG.Components;
using RealtimeCSG.Legacy;

namespace RealtimeCSG
{
    internal sealed class SelectedBrushSurface
    {
        public SelectedBrushSurface(CSGBrush _brush, int _surfaceIndex, CSGPlane _surfacePlane)
        {
            brush = _brush; surfaceIndex = _surfaceIndex; surfacePlane = _surfacePlane;
        }
        public SelectedBrushSurface(CSGBrush _brush, int _surfaceIndex, CSGPlane? _surfacePlane = null)
        {
            brush = _brush; surfaceIndex = _surfaceIndex; surfacePlane = _surfacePlane;
        }
        public CSGBrush brush;
        public int surfaceIndex;
        public CSGPlane? surfacePlane;
    }
}
