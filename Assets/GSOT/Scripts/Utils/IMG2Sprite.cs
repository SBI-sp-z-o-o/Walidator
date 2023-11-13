using UnityEngine;
using System.Collections;
using System.IO;
using Assets.GSOT.Scripts.LoadingScripts;

public class IMG2Sprite : MonoBehaviour
{
    private static IMG2Sprite _instance;
    private string persistentPath = "";// = Application.persistentDataPath;
    public static IMG2Sprite instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<IMG2Sprite>();
                _instance.persistentPath = Application.persistentDataPath;
            }
            return _instance;
        }
    }

    public Sprite LoadNewSprite(string FilePath = null, float PixelsPerUnit = 100.0f)
    {
        Debug.Log("Metoda ImageToSprite LoadNewSprite1 ");
        if (FilePath == null) return null;
        Debug.Log("Metoda ImageToSprite LoadNewSprite2 ");
        if (ModelsQueue.Sprites.ContainsKey(FilePath))
        {
            Debug.Log("Metoda ImageToSprite LoadNewSprite3");
            return ModelsQueue.Sprites[FilePath];
        }
        Debug.Log("Metoda ImageToSprite LoadNewSprite4");
        Sprite NewSprite;
        Texture2D SpriteTexture = LoadTexture(FilePath);
        NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);
        Debug.Log("Metoda ImageToSprite LoadNewSprite5");
        ModelsQueue.Sprites.Add(FilePath, NewSprite);
        Debug.Log("Metoda ImageToSprite LoadNewSprite6");
        return NewSprite;
    }

    public Texture2D LoadTexture(string FilePath)
    {

        Debug.Log("Metoda ImageToSprite LoadTexture1");
        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            Debug.Log("Metoda ImageToSprite LoadTexture2");
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);
            if (Tex2D.LoadImage(FileData))
            {
                Debug.Log("Metoda ImageToSprite LoadTexture3");
                return Tex2D;
            }
        }
        Debug.Log("Metoda ImageToSprite LoadTexture3");
        return null;
    }
}