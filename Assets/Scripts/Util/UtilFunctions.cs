using UnityEngine;

/// <summary>
/// Misc. Util functions.
/// </summary>
public class UtilFunctions
{
    /// <summary>
    /// Check if two values are 'close enough' to be equal.
    /// </summary>
    /// <param name="a">Value A</param>
    /// <param name="b">Value B</param>
    /// <param name="epsilon">How different A can be from B.</param>
    /// <returns></returns>
    public static bool NearlyEqual(float a, float b, float epsilon)
    {
        float absA = Mathf.Abs(a);
        float absB = Mathf.Abs(b);
        float diff = Mathf.Abs(a - b);

        if (a == b)
        { // shortcut, handles infinities
            return true;
        }
        else if (a == 0 || b == 0 || absA + absB < float.Epsilon)
        {
            // a or b is zero or both are extremely close to it
            // relative error is less meaningful here
            return diff < (epsilon * float.MinValue);
        }
        else
        {
            return diff < epsilon;
        }
    }

}
