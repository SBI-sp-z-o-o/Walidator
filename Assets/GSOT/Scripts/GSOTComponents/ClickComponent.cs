using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickComponent : MonoBehaviour, IPointerClickHandler
{
    public string Url { get; set; }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(Url))
        {
            Application.OpenURL(Url);
        }
    }
}
