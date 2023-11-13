using Assets.GSOT.Scripts.LoadingScripts;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class DebugController : MonoBehaviour
{
    // Start is called before the first frame update
    public Text debugText;
    public bool Active { get; set; }
    public Dictionary<string, string> Messages
    {
        get
        {
            return ModelsQueue.Messages;
        }
        set
        {
            Messages = value;
        }
    }

    void Start()
    {
        //Messages = new Dictionary<string, string>();
    }

    void Update()
    {
#if DEVELOPMENT_BUILD
        debugText.gameObject.SetActive(true);
        Active = true;
#endif

        if (Active || Application.isEditor)
        {
            debugText.gameObject.SetActive(Active || Application.isEditor);
            Print();
        }
    }
    public void Push(string key, object value)
    {
        if (Messages == null) Messages = new Dictionary<string, string>();
        if (Messages.ContainsKey(key))
        {
            //if (Messages[key].ToString() != value.ToString())
            //{
            //    Messages.Add(key + " " + index.ToString(), value?.ToString());
            //    index++;
            //}
            Messages[key] = value?.ToString();
        }
        else
        {
            Messages.Add(key, value?.ToString());
        }
    }

    private void Print()
    {
        string text = "";
        //text += $"Total allocated memory: {(Profiler.GetTotalAllocatedMemoryLong() / 1024f) / 1024f} MB\n";
        //text += $"Total reserved memory: {(Profiler.GetTotalReservedMemoryLong() / 1024f) / 1024f} MB\n";
        //text += $"Total unused reserved memory: {(Profiler.GetTotalUnusedReservedMemoryLong() / 1024f) / 1024f} MB\n";
        foreach (var key in Messages)
        {
            text += $"{key.Key}: {key.Value} \n";
        }
        if (debugText != null)
            debugText.text = text;
    }

    public void Clear()
    {
        Messages = new Dictionary<string, string>();
    }
}
