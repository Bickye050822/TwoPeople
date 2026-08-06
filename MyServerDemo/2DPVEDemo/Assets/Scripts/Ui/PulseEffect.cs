using UnityEngine;

public class PulseEffect : MonoBehaviour
{
    public float speed = 3f;
    public float minScale = 0.9f;
    public float maxScale = 1.1f;

    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
        float s = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = baseScale * s;
    }
}
