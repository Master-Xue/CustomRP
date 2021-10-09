using UnityEngine;

public abstract class Transformation : MonoBehaviour
{
    public abstract Vector3 Apply(Vector3 point);
    public abstract Matrix4x4 Matrix { get; }
}
