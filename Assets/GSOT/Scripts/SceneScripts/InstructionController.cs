using Assets.GSOT.Scripts.LoadingScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class InstructionController : MonoBehaviour
{
    public VideoClip GuideClip;
    public VideoClip TableClip;

    public VideoPlayer Player;
    public Text PlayerTime;
    public ProgressBar Progress;
    public Toggle DontShow;
    bool autoPlay = true;

    public GameObject playButton;
    public GameObject pauseButton;


    void Start()
    {
        playButton.SetActive(false);
        if (autoPlay)
            switch (ModelsQueue.InstructionType)
            {
                case InstructionType.Playground:
                    Player.clip = GuideClip;
                    Player.Play();
                    Progress.TotalTime = (float)Player.length;
                    break;
                case InstructionType.Table:
                    Player.clip = TableClip;
                    Player.Play();
                    Progress.TotalTime = (float)Player.length;

                    break;
                default:
                    Player.clip = TableClip;
                    Player.Play();
                    Progress.TotalTime = (float)Player.length;
                    break;
            }
    }

    public void Play()
    {
        
        Player.Play();
    }

    public void Pause()
    {
        if (Player.isPlaying)
        {
            Player.Pause();
            pauseButton.SetActive(false);
            playButton.SetActive(true);
            end = false;
        }
    }

    public void Resume()
    {
        if (Player.isPaused)
        {
            Player.Play();
            pauseButton.SetActive(true);
            playButton.SetActive(false);
            end = false;
        }
    }
    public void Repeat()
    {
        Player.Stop();
        Player.Play();
        end = false;
    }

    public void Backward()
    {
        if (Player.time + 5f < (long)Player.length)
        {
            Player.time = 0f;
            Player.Play();
        }
        else
        {
            Player.time -= 5.0f;
            Player.Play();
        }
        end = false;
    }
    private bool end = false;
    public void Forward()
    {
        if(Player.time + 5f >= (long)Player.length)
        {
            Player.time = Player.length;
            end = true;
        }
        else
        {
            Player.time += 5.0f;
        }
    }

    public void GoToScene()
    {
        if (DontShow.isOn)
        {
            PlayerPrefs.SetString(ModelsQueue.InstructionType.ToString(), "true");
        }
        switch (ModelsQueue.InstructionType)
        {
            case InstructionType.Playground:
                SceneManager.LoadScene("PlayfieldPointsSelectScene");
                break;
            case InstructionType.Table:
                SceneManager.LoadScene("ModelScene");
                break;
        }
    }

    void Update()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WindowsEditor)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene("SceneTypeScene");
            }
        }
        //PlayerTime.text = Player.clockTime.ToString();
        if (end)
        {
            //Progress.TotalTime = 1;
            Progress.TotalTime = 1;
            Progress.currentAmount = 1;
            //Progress.currentAmount = 1;
        }
        else
        {
            Progress.TotalTime = (float)Player.length;
            Progress.currentAmount = (float)Player.time;
            if (Player.time>=Player.length-0.1) GoToScene();
        }
    }

    public enum InstructionType
    {
        Playground = 1,
        Table = 2
    }
}
