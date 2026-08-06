using UnityEngine;

public class SpriteAnimEffect : MonoBehaviour
{
    public Sprite[] frames;
    public float fps = 10;
    private SpriteRenderer sr;
    private float timer;
    private int index;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (frames.Length > 0) Destroy(gameObject, frames.Length / fps + 0.1f);
    }

    void Update()
    {
        if (sr == null || frames.Length == 0) return;
        timer += Time.deltaTime;
        if (timer >= 1f / fps)
        {
            timer = 0;
            index = (index + 1) % frames.Length;
            sr.sprite = frames[index];
        }
    }
}
