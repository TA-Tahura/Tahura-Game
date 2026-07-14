using UnityEngine;

/// Kamera mengikuti pemain di sumbu X, dibatasi lebar background.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float minX = -9.6f;
    public float maxX = 9.6f;
    public float smooth = 8f;

    void LateUpdate()
    {
        if (target == null) return;
        var p = transform.position;
        float wanted = Mathf.Clamp(target.position.x, minX, maxX);
        p.x = Mathf.Lerp(p.x, wanted, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        transform.position = p;
    }

    public void SnapTo(float x)
    {
        var p = transform.position;
        p.x = Mathf.Clamp(x, minX, maxX);
        transform.position = p;
    }
}
