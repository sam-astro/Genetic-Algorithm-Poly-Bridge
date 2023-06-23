using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public TMP_Text timeText;
    public int offsetTime = 0;
    void Update()
    {
        float time = Time.realtimeSinceStartup + offsetTime;
        timeText.text =
            ((int)time / 60 / 60).ToString() + "h " +
            ((int)time / 60 % 60).ToString() + "m " +
            ((int)time % 60 % 60).ToString() + "s";
    }
}
