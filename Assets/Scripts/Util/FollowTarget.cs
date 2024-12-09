using UnityEngine;

/// <summary>
/// Util class for following a camera's target.
/// </summary>
public class FollowTarget : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject target;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
    }
}
