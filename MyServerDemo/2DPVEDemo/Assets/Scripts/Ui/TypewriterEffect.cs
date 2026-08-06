using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    public float delay = 0.05f;        // 每个字的间隔
    public bool playOnStart = true;
    private Text txt;
    private string fullText;

    void Start()
    {
        txt = GetComponent<Text>();
        if (txt == null) return;
        fullText = txt.text;
        if (playOnStart) StartType();
    }

    public void StartType()
    {
        StopAllCoroutines();
        StartCoroutine(Type());
    }

    IEnumerator Type()
    {
        txt.text = "";
        for (int i = 0; i < fullText.Length; i++)
        {
            txt.text += fullText[i];
            yield return new WaitForSeconds(delay);
        }
    }
}
