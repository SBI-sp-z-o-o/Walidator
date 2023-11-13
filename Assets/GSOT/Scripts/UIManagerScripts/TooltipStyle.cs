using UnityEngine;

public class TooltipStyle : MonoBehaviour
{
    private void Start()
    {
        // Subscribe to the event preparation of tooltip style.
        OnlineMapsGUITooltipDrawer.OnPrepareTooltipStyle += OnPrepareTooltipStyle;
        //OnlineMapsGUITooltipDrawer.
    }

    private void OnPrepareTooltipStyle(ref GUIStyle style)
    {
        // Change the style settings.
        style.fontSize = Screen.width / 30;
        //style.normal.background = null;
        //style.onNormal.
    }
}
