using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.SceneScripts;
using Assets.GSOT.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTypeController : MonoBehaviour
{
    public Button TableButton;
    public Button MuzeumButton;
    public Button PlayingFieldButton;
    public Image Background;
    public Image Logo;
    public GameObject locationRoot;

    void Start()
    {
        var activePlace = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault();
        var activeScene = activePlace.Scenes.Where(x => x.Id == ModelsQueue.ActiveSceneId).FirstOrDefault();
        if (activeScene != null)
        {
            TableButton.gameObject.SetActive(activeScene.IsAvailableInTableSceneUsingMode);
            if (!activeScene.IsAvailableInPlaygroundScene)
            {
                PlayingFieldButton.gameObject.SetActive(activeScene.IsAvailableInPlaygroundScene);

                if (ModelsQueue.BackToScenesType == Assets.GSOT.Scripts.Models.ApiModels.SceneGroupType.Guide)
                {
                    MuzeumButton.gameObject.SetActive(activeScene.IsAvailableInPlaygroundScene);
                    TableButton.gameObject.transform.position = MuzeumButton.gameObject.transform.position;
                }
            }

            MuzeumButton.gameObject.SetActive(ModelsQueue.BackToScenesType == Assets.GSOT.Scripts.Models.ApiModels.SceneGroupType.Guide);
        }

        var imgConverter = FindObjectOfType<IMG2Sprite>();
        var bg = imgConverter.LoadNewSprite(ModelsQueue.BackgroundFilePath);
        if (bg != null)
        {
            Background.sprite = bg;
        }

        var lo = imgConverter.LoadNewSprite(ModelsQueue.LogoFilePath);
        if (lo != null)
        {
            Logo.sprite = lo;
        }
        //TableButton.gameObject.GetComponentInChildren<Text>().text = Translator.Instance().GetString("Table");
        //MuzeumButton.gameObject.GetComponentInChildren<Text>().text = Translator.Instance().GetString("Ground");
        //PlayingFieldButton.gameObject.GetComponentInChildren<Text>().text = Translator.Instance().GetString("PlayingField");



        var df = imgConverter.LoadNewSprite(ModelsQueue.ButtonDefaultFilePath);
        if (df != null)
        {
            TableButton.image.sprite = df;
            TableButton.transition = Selectable.Transition.None;

            if (ModelsQueue.BackToScenesType == Assets.GSOT.Scripts.Models.ApiModels.SceneGroupType.Guide)
            {
                MuzeumButton.image.sprite = df;
                MuzeumButton.transition = Selectable.Transition.None;
            }
            PlayingFieldButton.image.sprite = df;
            PlayingFieldButton.transition = Selectable.Transition.None;
        }

        var tb = imgConverter.LoadNewSprite(ModelsQueue.ButtonTableFilePath);
        if (tb != null)
        {
            TableButton.image.sprite = tb;
            TableButton.transition = Selectable.Transition.None;
        }
        if (ModelsQueue.BackToScenesType == Assets.GSOT.Scripts.Models.ApiModels.SceneGroupType.Guide)
        {
            var mu = imgConverter.LoadNewSprite(ModelsQueue.ButtonMuseumFilePath);
            if (mu != null)
            {
                MuzeumButton.image.sprite = mu;
                MuzeumButton.transition = Selectable.Transition.None;
            }
        }
        var pg = imgConverter.LoadNewSprite(ModelsQueue.ButtonPlaygroundFilePath);
        if (pg != null)
        {
            PlayingFieldButton.image.sprite = pg;
            PlayingFieldButton.transition = Selectable.Transition.None;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Make sure user is on Android platform
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor)
        {
            // Check if Back was pressed this frame
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StartCoroutine(DeleteButtons());
                StartCoroutine(LoadSceneAsync("ScenesScene"));
            }
        }
    }

    IEnumerator DeleteButtons()
    {
        var buttons = GetComponents<Button>();
        foreach(var b in buttons)
        {
            Destroy(b);
        }

        yield return null;
    }


    IEnumerator LoadSceneAsync(string name)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void SelectTableScene()
    {
        ModelsQueue.IsTableScene = true;
        ModelsQueue.TableSceneStart = false;
        SwitchScene();
    }

    public void SelectGroundScene()
    {
        ModelsQueue.IsTableScene = false;
        SwitchScene();
    }

    public void SelectPlayingField()
    {
        ModelsQueue.InstructionType = InstructionController.InstructionType.Playground;
        if (PlayerPrefs.HasKey(ModelsQueue.InstructionType.ToString()))
        {
            SceneManager.LoadScene("PlayfieldPointsSelectScene");
        }
        else
        {
            SceneManager.LoadScene("InstructionScene");
        }
    }

    private void SwitchScene()
    {
        ModelsQueue.IsPlaygroundScene = false;
        var activePlace = ModelsQueue.Places.Where(x => x.Name == ModelsQueue.ActivePlace).FirstOrDefault();
        TemporaryDatabase.ActiveSceneStartDate = DateTime.Now;
        TemporaryDatabase.ActiveSceneId = ModelsQueue.ActiveSceneId;
        TemporaryDatabase.ActiveScenePlaceId = activePlace.Id;
        TemporaryDatabase.ActiveSceneUsingMode = ModelsQueue.IsTableScene ? Assets.GSOT.Scripts.Enums.ApiEnums.SceneUsingMode.Table : Assets.GSOT.Scripts.Enums.ApiEnums.SceneUsingMode.OriginalArea;
        TemporaryDatabase.ActiveSceneOrderProductLicenseId = activePlace.GSOrderProductLicenseId;

        if (ModelsQueue.IsTableScene)
        {
            ModelsQueue.InstructionType = InstructionController.InstructionType.Table;

            if (PlayerPrefs.HasKey(ModelsQueue.InstructionType.ToString()))
            {
                SceneManager.LoadScene("ModelScene");
            }
            else
            {
                SceneManager.LoadScene("InstructionScene");
            }
        }
        else
        {
            SceneManager.LoadScene("ModelScene");
        }
    }

    public void Help()
    {
        Application.OpenURL(TemporaryDatabase.FaqURL);
    }
}
