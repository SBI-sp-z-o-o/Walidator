using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewController : MonoBehaviour
{
    Vector3 startPosition;
    ScrollRect rect;
    public bool Enabled = false;

    public ScrollRect MyScrollRect;
    public RectTransform Content;

    void Start()
    {
        var children = this.gameObject.GetComponentsInChildren<Button>().ToList();
        this.transform.localPosition = new Vector3(transform.localPosition.x, -children.Count * (100 + children.Count * 3), transform.localPosition.z);

        Canvas.ForceUpdateCanvases();
        //MyScrollRect.content.localPosition = MyScrollRect.GetSnapToPositionToBringChildIntoView(someChild);
        //contentPanel.anchoredPosition =
        //    (Vector2)scrollRect.transform.InverseTransformPoint(contentPanel.position)
        //    - (Vector2)scrollRect.transform.InverseTransformPoint(target.position);

        MyScrollRect.content.localPosition = GetSnapToPositionToBringChildIntoView(MyScrollRect, children[0].GetComponent<RectTransform>());


        rect = FindObjectOfType<ScrollRect>();
    }

    void Update()
    {
        var children = this.gameObject.GetComponentsInChildren<Button>().ToList();
        if(!Enabled && gameObject.activeSelf)
        {
            Enabled = true;
            //this.transform.localPosition = new Vector3(transform.localPosition.x, -children * (100 + children * 3), transform.localPosition.z);
            MyScrollRect.content.localPosition = GetSnapToPositionToBringChildIntoView(MyScrollRect, children[0].GetComponent<RectTransform>());
        }
        if (startPosition == Vector3.zero)
        {
            startPosition = this.transform.localPosition;
        }

        if (children.Count <= 2)
        {
            this.transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
        }
        else
        {
            if (this.transform.localPosition.y < -children.Count * 120)
            {
                var c = this.gameObject.GetComponentsInChildren<Button>().ToList();
                //this.rect.trans.offsetMax = new Vector2(rt.offsetMax.x, -top);
                this.transform.localPosition = new Vector3(transform.localPosition.x, (-children.Count * (100 + children.Count * 3)), transform.localPosition.z);
                if (rect)
                {
                    rect.StopMovement();
                }
                //this.transform.localPosition = referencePositionText.transform.position;
            }
            if (this.transform.localPosition.y > children.Count * 120)
            {
                this.transform.localPosition = new Vector3(transform.localPosition.x, children.Count * (100 + children.Count * 3), transform.localPosition.z);
                if (rect)
                {
                    rect.StopMovement();
                }
            }
        }
    }


    public static Vector2 GetSnapToPositionToBringChildIntoView(ScrollRect instance, RectTransform child)
    {
        Canvas.ForceUpdateCanvases();
        Vector2 viewportLocalPosition = instance.viewport.localPosition;
        Vector2 childLocalPosition = child.localPosition;
        Vector2 result = new Vector2(
            0,
            0 - (viewportLocalPosition.y + childLocalPosition.y)
        );
        return result;
    }
}
