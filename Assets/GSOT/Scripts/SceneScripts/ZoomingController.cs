using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoomingController : MonoBehaviour
{
    private ModelSceneMenuController controller;
    public List<GameObject> Models => controller?.models;

    float touchesPrevPosDiff, touchesCurPosDif, zoomModifier;
    Vector2 firstTouchPrev, secondTouchPrev;

    // Start is called before the first frame update
    void Start()
    {
        controller = FindObjectOfType<ModelSceneMenuController>();
    }


    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount == 2)
        {
            Debug.Log("Metoda Zooming metoda upadate1");
            Touch first = Input.GetTouch(0);
            Touch second = Input.GetTouch(1);

            firstTouchPrev = first.position - first.deltaPosition;
            secondTouchPrev = second.position - second.deltaPosition;

            touchesPrevPosDiff = (firstTouchPrev - secondTouchPrev).magnitude;
            touchesCurPosDif = (first.position - second.position).magnitude;

            zoomModifier = (first.deltaPosition - second.deltaPosition).magnitude * 0.001f;

            if (touchesPrevPosDiff < touchesCurPosDif)
            {
                foreach(var x in Models)
                {
                    if (x.transform.localScale.x < 10f)
                        x.transform.localScale = new Vector3(x.transform.localScale.x + zoomModifier, x.transform.localScale.y + zoomModifier, x.transform.localScale.z + zoomModifier);

                    if (x.transform.localScale.x < 0.05f)
                    {
                        x.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                    }
                    if (x.transform.localScale.x > 10f)
                    {
                        x.transform.localScale = new Vector3(3f, 3f, 3f);
                    }
                }
            }
            if (touchesPrevPosDiff > touchesCurPosDif)
            {
                foreach (var x in Models)
                {
                    if (x.transform.localScale.x > 0.05f)
                        x.transform.localScale = new Vector3(x.transform.localScale.x - zoomModifier, x.transform.localScale.y - zoomModifier, x.transform.localScale.z - zoomModifier);

                    if (x.transform.localScale.x < 0.05f)
                    {
                        x.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                    }
                    if (x.transform.localScale.x > 10f)
                    {
                        x.transform.localScale = new Vector3(3f, 3f, 3f);
                    }
                }
            }
        }
    }
}
