using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Android;

public class ScreenShot : MonoBehaviour
{
    private const string MediaStoreImagesMediaClass = "android.provider.MediaStore$Images$Media";
    private static AndroidJavaObject _activity;
    public Camera guiCamera;
    public AudioClip sound;


    private void CaptureScreenshot()
    {
        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);


        Camera camera = Camera.main;
        camera.targetTexture = renderTexture;
        camera.Render();
        // mark the render texture as active and read the current pixel data into the Texture2D
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(rect, 0, 0);
        // reset the textures and remove the render texture from the Camera since were done reading the screen data
        camera.targetTexture = null;
        RenderTexture.active = null;
        // get our filename
        string filename = Application.persistentDataPath + "Screenshot" + System.DateTime.Now.Hour + System.DateTime.Now.Minute + System.DateTime.Now.Second + ".png";
        // get file header/data bytes for the specified image format
        byte[] fileHeader = null;
        byte[] fileData = null;
        //Set the format and encode based on it
        fileData = screenShot.EncodeToPNG();
        // create new thread to offload the saving from the main thread
        new System.Threading.Thread(() =>
        {
            var file = System.IO.File.Create(filename);
            if (fileHeader != null)
            {
                file.Write(fileHeader, 0, fileHeader.Length);
            }
            file.Write(fileData, 0, fileData.Length);
            file.Close();
            Debug.Log(string.Format("Screenshot Saved {0}, size {1}", filename, fileData.Length));
        }).Start();
        //Cleanup
        Destroy(renderTexture);
        renderTexture = null;
        screenShot = null;
    }




    public void TakePhoto()
    {
        Debug.Log("Metoda TakePhoto1 ");
        //CaptureScreenshot();

        //StartCoroutine(ScreenshotEncode());

        //working!!!!!!!!!!!!
        StartCoroutine(CaptureScreenshotCoroutine(Screen.width, Screen.height));
        Debug.Log("Metoda TakePhoto2 ");
    }


    #region working version
    public static AndroidJavaObject Activity
    {
        get
        {
            if (_activity == null)
            {
                var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                _activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }
            return _activity;
        }
    }
    private IEnumerator CaptureScreenshotCoroutine(int width, int height)
    {
        yield return new WaitForEndOfFrame();
        Texture2D tex = new Texture2D(width, height);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        yield return tex;

        //new System.Threading.Thread(() =>
        //{
        string path = SaveImageToGallery(tex, "Screenshot" + System.DateTime.Now.Hour + System.DateTime.Now.Minute + System.DateTime.Now.Second, "Description");
        Debug.Log("Picture has been saved at:\n" + path);
        //}).Start();
    }

    public static string SaveImageToGallery(Texture2D texture2D, string title, string description)
    {
        using (var mediaClass = new AndroidJavaClass(MediaStoreImagesMediaClass))
        {
            using (var cr = Activity.Call<AndroidJavaObject>("getContentResolver"))
            {
                var image = Texture2DToAndroidBitmap(texture2D);
                var imageUrl = mediaClass.CallStatic<string>("insertImage", cr, image, title, description);
                return imageUrl;
            }
        }
    }

    public static AndroidJavaObject Texture2DToAndroidBitmap(Texture2D texture2D)
    {
        byte[] encoded = texture2D.EncodeToPNG();
        using (var bf = new AndroidJavaClass("android.graphics.BitmapFactory"))
        {
            return bf.CallStatic<AndroidJavaObject>("decodeByteArray", encoded, 0, encoded.Length);
        }
    }
    #endregion
}