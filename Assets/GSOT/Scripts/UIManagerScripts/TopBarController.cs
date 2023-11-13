using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopBarController : MonoBehaviour
{
    public GameObject TopBar;
    private float offset = 0.1f;

    void Update()
    {
        TopBar.transform.position = transform.position + transform.forward * offset + new Vector3(0, 0.07f, 0);
        TopBar.transform.rotation = new Quaternion(0.0f, transform.rotation.y, 0.0f, transform.rotation.w);
        TopBar.transform.localScale = new Vector3(0.5f, 0.1f);
    }
}
