using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using System.Linq;

public class UpdatePlayerMiniMap : MonoBehaviour
{
    // Start is called before the first frame update

    public UILineRenderer line;
    public NewAgent posInfo;
    RectTransform rect;
    void Start()
    {
        rect = gameObject.GetComponent<RectTransform>();
        
    }

    // Update is called once per frame
    void Update()
    {
        //Get goal information
        Vector3 lastPoint = posInfo.points.ElementAt(posInfo.points.Count - 1);
        float distance = (posInfo.transform.position - lastPoint).magnitude / (posInfo.currentPoint - lastPoint).magnitude;
        float currentX = 0;
        float currentY = 0;

            currentX = lastPoint.x + (posInfo.currentPoint.x - lastPoint.x) * distance;
            currentY = lastPoint.z + (posInfo.currentPoint.z - lastPoint.z) * distance;


        rect.anchoredPosition3D = new Vector3(currentX, currentY, 0);
    }
}
