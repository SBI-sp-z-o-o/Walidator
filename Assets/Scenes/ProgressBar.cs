using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Transform LoadingBar;
    public Transform TextIndicator;
    public Transform TextLoading;
    [SerializeField] 
    public float currentAmount;
    public float TotalTime;


    // Update is called once per frame
    void Update()
    {
        if (currentAmount<= TotalTime)
        {
            TextIndicator.GetComponent<Text>().text = ((int)(Math.Ceiling(currentAmount / TotalTime * 100))).ToString() + "%";
            LoadingBar.GetComponent<Image>().fillAmount = (float)Math.Ceiling(currentAmount / TotalTime*100)/100;
        }


    }
}
