#region Namespaces

using UnityEngine;
using System.Collections;

using UnityEngine.UI;

#endregion

public class AnimationComponent : MonoBehaviour
{
    public GameObject DescriptionWindow;

    public void PlayInAnimation()
    {
        DescriptionWindow.SetActive(true);
    }

    public void PlayOutAnimation()
    {
        DescriptionWindow.SetActive(false);
    }
}
