using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerGUI : MonoBehaviour
{
    TMPro.TextMeshPro _textTarget;
    float timer = 90;
    // Start is called before the first frame update
    void Start()
    {
        _textTarget = GetComponent<TMPro.TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        string minutes = Mathf.Floor(timer / 60).ToString("00");
        string seconds = (timer % 60).ToString("00");
        string ms = ((timer*1000) % 1000).ToString("000");
        _textTarget.SetText("[" + minutes + ":" + seconds + "." + ms + " ]");
    }
}
