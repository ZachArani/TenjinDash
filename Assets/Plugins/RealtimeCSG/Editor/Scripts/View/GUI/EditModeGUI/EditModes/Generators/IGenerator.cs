using RealtimeCSG.Foundation;
using UnityEditor;
using UnityEngine;

namespace RealtimeCSG
{
    internal interface IGenerator
    {
        bool HaveBrushes { get; }
        bool CanCommit { get; }

        CSGOperationType CurrentCSGOperationType { get; set; }

        void Init();

        bool HotKeyReleased();

        bool UndoRedoPerformed();
        void PerformDeselectAll();

        void HandleEvents(SceneView sceneView, Rect sceneRect);

        bool OnShowGUI(bool isSceneGUI);
        void StartGUI();
        void FinishGUI();

        void DoCancel();
        void DoCommit();

        void OnDefaultMaterialModified();
    }
}
