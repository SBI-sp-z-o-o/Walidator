using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

public class Translator
{
    private static Translator instance;
    private Translator() { }

    private readonly Dictionary<string, Resource> resources = new Dictionary<string, Resource>();

    public static Translator Instance()
    {
        if(instance == null)
        {
            instance = new Translator();
            instance.GetData("pl-PL");
            instance.GetData("en-US");
        }
        return instance;
    }

    public string GetString(string name)
    {
        return resources[PlayerPrefs.GetString("lang")].Resources.Where(x => x.Key == name).First().Value;
    }

    private void GetData(string language)
    {
        var r = Resources.Load<TextAsset>(language);
        XmlSerializer xs = new XmlSerializer(typeof(Resource));
        Resource re;
        using (TextReader reader = new StringReader(r.text))
        {
            re = (Resource)xs.Deserialize(reader);
            resources.Add(language, re);
        }
    }
}


public class Resource
{
    public List<KeyValuePair<string, string>> Resources { get; set; }

    public Resource()
    {
        Resources = new List<KeyValuePair<string, string>>();
    }

    [Serializable]
    [XmlType(TypeName = "Entry")]
    public struct KeyValuePair<K, V>
    {
        public K Key
        { get; set; }

        public V Value
        { get; set; }

        public KeyValuePair(K key, V value)
        {
            Key = key;
            Value = value;
        }
    }
}
