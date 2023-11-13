using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.Models.ApplicationModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.GSOT.Scripts.SceneScripts
{
    public class AudioController : MonoBehaviour
    {
        public string audioName;

        [Header("Audio Stuff")]
        public AudioSource audioSource;
        public AudioClip audioClip;
        public string soundPath;
        public Audio CurrentAudio;
        private bool initialized = false;
        private Stopwatch stopwatch;
        private DateTime audioStartTime;
        private bool restarted = false;
        private bool paused = false;
        private void Start()
        {
        }

        public void InitAudio(Audio audio)
        {
            if (audio != null)
            {
                if (!string.IsNullOrEmpty(audio.FileName))
                {
                    CurrentAudio = audio;
                    this.audioName = audio.FileName;
                    audioSource = gameObject.AddComponent<AudioSource>();
                    soundPath = "file://" + Application.persistentDataPath + "/Sounds/";
                    StartCoroutine(LoadAudio());
                    DontDestroyOnLoad(this.gameObject);
                }
            }
        }

        private void Update()
        {
            if (CurrentAudio != null && !restarted && audioSource.clip != null)
            {
                DateTime now = DateTime.Now;
                var currentTimeline = CurrentAudio.Timelines.Where(x => x.StartTime.Ticks < now.Ticks && x.EndTime.Ticks > now.Ticks).FirstOrDefault();
                if (!audioSource.isPlaying && currentTimeline != null && !paused)
                {
                    audioSource.Play();
                }
                if (currentTimeline == null)
                {
                    audioSource.Pause();
                    return;
                }
                if (paused)
                {
                    audioSource.Pause();
                    //paused = true;
                    AudioListener.pause = true;
                }
                else if(!audioSource.isPlaying || AudioListener.pause)
                {
                    audioSource.Play();
                    AudioListener.pause = false;
                    //paused = false;
                }
            }
        }

        public void Play()
        {
            if (CurrentAudio == null)
            {
                return;
            }
            stopwatch.Stop();
            foreach (var timeline in CurrentAudio.Timelines)
            {
                timeline.StartTime = timeline.StartTime.AddSeconds(stopwatch.Elapsed.TotalSeconds);
                timeline.EndTime = timeline.EndTime.AddSeconds(stopwatch.Elapsed.TotalSeconds);
            }
            restarted = false;
            paused = false;
        }

        public void Stop()
        {
            if (CurrentAudio == null)
            {
                return;
            }
            stopwatch = new Stopwatch();
            stopwatch.Start();
            AudioListener.pause = true;
            paused = true;
            restarted = true;
        }

        private void Init()
        {
            if (CurrentAudio != null)
            {
                DateTime now = DateTime.Now;
                foreach (var timeline in CurrentAudio.Timelines)
                {
                    timeline.StartTime = now.AddSeconds(timeline.StartTimeInSeconds);
                    timeline.EndTime = now.AddSeconds(timeline.EndTimeInSeconds);
                }
                initialized = true;
            }
        }

        private void Awake()
        {
        }

        private IEnumerator LoadAudio()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            WWW request = GetAudioFromFile(soundPath, audioName);
#pragma warning restore CS0618 // Type or member is obsolete
            yield return request;

            audioClip = request.GetAudioClip();
            audioClip.name = audioName;

            PlayAudioFile();
        }

        private void PlayAudioFile()
        {
            audioSource.clip = audioClip;
            audioSource.loop = true;

            if (!initialized)
            {
                Init();
            }
        }

#pragma warning disable CS0618 // Type or member is obsolete
        private WWW GetAudioFromFile(string path, string filename)
        {
            string audioToLoad = string.Format(path + "{0}", filename);
            WWW request = new WWW(audioToLoad);
            return request;
        }
#pragma warning restore CS0618 // Type or member is obsolete

        private void OnDestroy()
        {
            audioClip.UnloadAudioData();
        }
    }
}
