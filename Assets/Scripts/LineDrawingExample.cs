using UnityEngine;

public class LineDrawingExample : MonoBehaviour
{
    /// <summary>
    /// Starting position of the created line
    /// </summary>
    Vector3 startPosition;

    /// <summary>
    /// GameObject of the created line
    /// </summary>
    GameObject currentLineObject;

    /// <summary>
    /// lineRenderer of the created line
    /// </summary>
    LineRenderer currentLineRenderer;

    /// <summary>
    /// Material of the drawn line.
    /// For my example I used a new Material with "UI/Default" shader
    /// </summary>
    public Material lineMaterial;

    /// <summary>
    /// thickness of the drawn line
    /// </summary>
    public float lineThickness;

    /// <summary>
    /// Canvas that you want to draw on
    /// </summary>
    public Canvas parentCanvas;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrawingLine();
        }
        else if (Input.GetMouseButton(0))
        {
            PreviewLine();
        }
        else if (Input.GetMouseButtonUp(1))
        {
            EndDrawingLine();
        }
    }

    /// <summary>
    /// Returns the current mouseposition relative to the canvas.
    /// Modifies the z-value slightly so that stuff will be rendered in front of UI elements.
    /// </summary>
    /// <returns></returns>
    Vector3 GetMousePosition()
    {
        Vector2 movePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition, parentCanvas.worldCamera,
            out movePos);
        Vector3 positionToReturn = parentCanvas.transform.TransformPoint(movePos);
        positionToReturn.z = parentCanvas.transform.position.z - 0.01f;
        return positionToReturn;
    }

    /// <summary>
    /// Starts drawing the line
    /// Creates a new GameObject at the startPosition and adds a LineRenderer to it
    /// The LineRenderer also gets modified with material and thickness here
    /// </summary>
    void StartDrawingLine()
    {
        startPosition = GetMousePosition();
        currentLineObject = new GameObject();
        currentLineObject.transform.position = startPosition;
        currentLineRenderer = currentLineObject.AddComponent<LineRenderer>();
        currentLineRenderer.material = lineMaterial;
        currentLineRenderer.startWidth = lineThickness;
        currentLineRenderer.endWidth = lineThickness;
    }

    /// <summary>
    /// Updates the LineRenderer Positions
    /// </summary>
    void PreviewLine()
    {
        currentLineRenderer.SetPositions(new Vector3[] { startPosition, GetMousePosition() });
    }

    /// <summary>
    /// Cleans up the used variables as the LineRenderer will not be modified anymore
    /// </summary>
    void EndDrawingLine()
    {
        startPosition = Vector3.zero;
        currentLineObject = null;
        currentLineRenderer = null;
    }
}
