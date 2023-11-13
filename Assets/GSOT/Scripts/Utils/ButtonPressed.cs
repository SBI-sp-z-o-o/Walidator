using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPressed : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool ButtonIsPressed;

    public void OnPointerDown(PointerEventData eventData)
    {
        ButtonIsPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ButtonIsPressed = false;
    }
}
