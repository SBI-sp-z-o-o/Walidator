using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.SceneScripts;
using Assets.GSOT.Scripts.Utils;
using GSOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModelsLoader : MonoBehaviour
{
    public Slider ProgressSlider;
    public Text ProgressPercentage;
    public Text SceneText;
    public Image Background;
    public Image Logo;
    public GameObject VideoPrefab;

    int ModelsCount;

    void Start()
    {
        Debug.Log("Metoda ModelsLoader start1");
        var imgConverter = FindObjectOfType<IMG2Sprite>();
        Debug.Log("Metoda ModelsLoader start2");
        var bg = imgConverter.LoadNewSprite(ModelsQueue.BackgroundFilePath);
        Debug.Log("Metoda ModelsLoader start3");
        if (bg != null)
        {
            Debug.Log("Metoda ModelsLoader start4");
            Background.sprite = bg;
        }
        Debug.Log("Metoda ModelsLoader start5");
        var lo = imgConverter.LoadNewSprite(ModelsQueue.LogoFilePath);
        Debug.Log("Metoda ModelsLoader start6");
        if (lo != null)
        {
            Debug.Log("Metoda ModelsLoader start7");
            Logo.sprite = lo;
        }
        Debug.Log("Metoda ModelsLoader start8");
        FilesUtils.Loaded = 0;
        Debug.Log("Metoda ModelsLoader start9");      
   //     SceneText.text = Translator.Instance().GetString("LoadingModels") + "...";
        SceneText.text = "Loading Models...";
        Debug.Log("Metoda ModelsLoader start10");
        var models = ModelsQueue.SceneQueue[ModelsQueue.ActiveSceneId].Select(x => x).Distinct().ToList();
        Debug.Log("Metoda ModelsLoader start11");
        ModelsCount = models.Count;
        Debug.Log("Metoda ModelsLoader start12");

        ProgressSlider.maxValue = 100;
        Debug.Log("Metoda ModelsLoader start13");
        List<string> loading = new List<string>();
        Debug.Log("Metoda ModelsLoader start14");
        foreach (var entry in models)
        {
            Debug.Log("Metoda ModelsLoader start15");
            if (loading.Contains(entry.meshId))
            {
                Debug.Log("Metoda ModelsLoader start16");
                ModelsCount--;
                Debug.Log("Metoda ModelsLoader start17");
                continue;
            }
            Debug.Log("Metoda ModelsLoader start18");
            GameObject Prefab = null;
            Debug.Log("Metoda ModelsLoader start19");
            Prefab = GetDontDestroyOnLoadObjects(entry.meshId);
            Debug.Log("Metoda ModelsLoader start20");
            if (Prefab == null)
            {
                Debug.Log("Metoda ModelsLoader start21");
                try
                {
                    Debug.Log("Metoda ModelsLoader start22");
                    if (entry.Type != Assets.GSOT.Scripts.Models.ApiModels.Type.Model)// GetFileExtension(entry.meshId) != ".zip")
                    {
                        Debug.Log("Metoda ModelsLoader start23");
                        ModelsCount--;
                    }
                    else
                    {
                        Debug.Log("Metoda ModelsLoader start24");
                        loading.Add(entry.meshId);
                        Debug.Log("Metoda ModelsLoader start25");
                        FilesUtils.Loaded++;

                        //StartCoroutine(FilesUtils.LoadModelFromBundleAsync(entry));
                    }
                    Debug.Log("Metoda ModelsLoader start26");
                }
                catch (Exception ex)
                {
                    Debug.Log("Metoda ModelsLoader start27");
                    Debug.LogWarning($"Ex: {ex.Message}");
                    Debug.LogWarning($"[ARLocation#WebMapLoader]: Prefab {entry} not found.");
                    ModelsCount--;
                }
                Debug.Log("Metoda ModelsLoader start28");
            }
            else
            {
                Debug.Log("Metoda ModelsLoader start29");
                ModelsCount--;
            }
            Debug.Log("Metoda ModelsLoader start30");
        }
        Debug.Log("Metoda ModelsLoader start31");
    }

    private static string GetFileExtension(string url)
    {
        Debug.Log("Metoda ModelsLoader GetFileExtension1");
        return TriLib.FileUtils.GetFileExtension(url);
    }


    IEnumerator LoadSceneAsync(string name)
    {
        Debug.Log("Metoda ModelsLoader LoadSceneAsync1");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name);
        Debug.Log("Metoda ModelsLoader LoadSceneAsync2");
        while (!asyncLoad.isDone)
        {
            Debug.Log("Metoda ModelsLoader LoadSceneAsync3");
            yield return null;
            Debug.Log("Metoda ModelsLoader LoadSceneAsync4");
        }
        Debug.Log("Metoda ModelsLoader LoadSceneAsync5");
    }

    bool leaving = false;
    public void Update()
    {
        Debug.Log("Metoda ModelsLoader Update1");
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor)
        {
            Debug.Log("Metoda ModelsLoader Update2");
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Metoda ModelsLoader Update3");
                leaving = true;
                StartCoroutine(LoadSceneAsync("ScenesScene"));
                Debug.Log("Metoda ModelsLoader Update4");
            }
            Debug.Log("Metoda ModelsLoader Update5");
        }
        Debug.Log("Metoda ModelsLoader Update6");
        if (leaving)
        {
            Debug.Log("Metoda ModelsLoader Update7");
            return;
        }
        UpdateProgress();
        Debug.Log("Metoda ModelsLoader Update8");
        if (FilesUtils.Loaded == ModelsCount)
        {
            Debug.Log("Metoda ModelsLoader Update9");
            var activePlace = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault();
            Debug.Log("Metoda ModelsLoader Update10");
            TemporaryDatabase.ActiveSceneStartDate = DateTime.Now;
            TemporaryDatabase.ActiveSceneId = ModelsQueue.ActiveSceneId;
            TemporaryDatabase.ActiveScenePlaceId = activePlace.Id;
            TemporaryDatabase.ActiveSceneUsingMode = activePlace.SceneUsingMode;
            TemporaryDatabase.ActiveSceneOrderProductLicenseId = activePlace.GSOrderProductLicenseId;
            Debug.Log("Metoda ModelsLoader Update11");

            ModelsQueue.InitScene();
            Debug.Log("Metoda ModelsLoader Update12");
            if (ModelsQueue.BackToScenesType == Assets.GSOT.Scripts.Models.ApiModels.SceneGroupType.Guide)
            {
                Debug.Log("Metoda ModelsLoader Update13");
                ModelsQueue.IsTableScene = false;
                ModelsQueue.IsPlaygroundScene = false;
                TemporaryDatabase.ActiveSceneStartDate = DateTime.Now;
                TemporaryDatabase.ActiveSceneId = ModelsQueue.ActiveSceneId;
                TemporaryDatabase.ActiveScenePlaceId = activePlace.Id;
                Debug.Log("Metoda ModelsLoader Update14");
                TemporaryDatabase.ActiveSceneUsingMode = ModelsQueue.IsTableScene ? Assets.GSOT.Scripts.Enums.ApiEnums.SceneUsingMode.Table : Assets.GSOT.Scripts.Enums.ApiEnums.SceneUsingMode.OriginalArea;
                TemporaryDatabase.ActiveSceneOrderProductLicenseId = activePlace.GSOrderProductLicenseId;
                Debug.Log("Metoda ModelsLoader Update15");
                SceneManager.LoadScene("ModelScene");
                Debug.Log("Metoda ModelsLoader Update16");
            }
            else
            {
                Debug.Log("Metoda ModelsLoader Update17");
                SceneManager.LoadScene("SceneTypeScene");
                Debug.Log("Metoda ModelsLoader Update18");
            }
            //Rect camRect = Camera.main.rect;
            //camRect.yMax = 0.9f; // 90% of viewport
            //Camera.main.rect = camRect;
            Debug.Log("Metoda ModelsLoader Update19");
            return;
        }
    }

    private void UpdateProgress()
    {
        var progress = ((float)FilesUtils.Loaded / (float)ModelsCount) * 100;
        ProgressSlider.value = progress;
        if (float.IsNaN(progress))
        {
            ProgressPercentage.text = "0%";
        }
        else
        {
            ProgressPercentage.text = ((int)progress).ToString() + "%";
        }
    }

    public static GameObject GetDontDestroyOnLoadObjects(string name)
    {
        Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects1");
        GameObject temp = null;
        try
        {
            Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects2");
            temp = new GameObject();
            DontDestroyOnLoad(temp);
            Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects3");
            Scene dontDestroyOnLoad = temp.scene;
            Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects4");
            DestroyImmediate(temp);
            Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects5");
            temp = null;
            var dontDestroy = dontDestroyOnLoad.GetRootGameObjects().ToList();
            Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects6");
            foreach (var dd in dontDestroy)
            {
                Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects7");
                if (dd.name[0] != '{')
                {
                    Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects8");
                    continue;
                }
                Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects9");
                try
                {
                    Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects10");
                    var entry = JsonUtility.FromJson<DataEntry>(dd.name);
                    Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects11");
                    if (entry != null)
                    {
                        Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects12");
                        if (entry.meshId == name)
                        {
                            Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects13");
                            return dd;
                        }
                        Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects14");
                    }
                    Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects15");
                }
                catch(Exception ex)
                {
                    Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects16");
                    Debug.LogWarning($"Ex: {ex.Message}");
                }
                Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects17");
            }
            Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects18");
            return null;
            //return dontDestroyOnLoad.GetRootGameObjects().ToList().Where(x => JsonUtility.FromJson<DataEntry>(x.name).meshId == name).FirstOrDefault();
        }
        finally
        {
            Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects19");
            if (temp != null)
            {
                Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects20");
                DestroyImmediate(temp);
                Debug.Log("Metoda ModelsLoader GetDontDestroyOnLoadObjects21");
            }
        }
    }
}
