using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DescriptionComponent : MonoBehaviour
{
    // Start is called before the first frame update
    public string Description;
    AnimationComponent animationComponent;
    void Start()
    {

        animationComponent = FindObjectOfType<AnimationComponent>();//.fi.gameObject.GetComponentInChildren<AnimationComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
