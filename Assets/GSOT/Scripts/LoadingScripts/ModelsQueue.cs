using Assets.GSOT.Scripts.Models.ApiModels;
using Assets.GSOT.Scripts.Models.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GSOT;
using static InstructionController;

namespace Assets.GSOT.Scripts.LoadingScripts
{
    public static class ModelsQueue
    {
        public static Dictionary<long, List<DataEntry>> SceneQueue = new Dictionary<long, List<DataEntry>>();
        public static Dictionary<long, long> SceneAnimationTimes = new Dictionary<long, long>();
        public static Dictionary<long, List<DataEntry>> Rendered = new Dictionary<long, List<DataEntry>>();
        public static Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
        public static Dictionary<string, Texture2D> Textures2D = new Dictionary<string, Texture2D>();
        public static List<Material> Materials = new List<Material>();


        public static Dictionary<long, List<Audio>> SceneAudio = new Dictionary<long, List<Audio>>();
        public static Dictionary<long, List<Audio>> PlayedAudio = new Dictionary<long, List<Audio>>();

        public static MobileAppConfigurationDto Configuration { get; set; }
        public static List<ApplicationPlace> Places = new List<ApplicationPlace>();
        //public static string
        public static string ActivePlace;
        public static string ActiveScene;
        public static long ActiveSceneId;
        public static string DeviceId;
        //public static string ActiveSceneId;
        public static float HeightCorrection = 0f;
        public static float AzimuthCorrection = 0f;

        //table
        public static bool IsTableScene = false;
        public static bool IsPlaygroundScene = false;
        public static bool TableSceneStart = false;
        public static Vector3 TablePoint1 = new Vector3();
        public static Vector3 TablePoint2 = new Vector3();
        public static Vector3 TablePoint3 = new Vector3();
        public static Vector3 TablePoint4 = new Vector3();
        public static Vector3 ApiPoint1 = new Vector3();
        public static Vector3 ApiPoint2 = new Vector3();
        public static ARLocation.Location TableTransformPosition = new ARLocation.Location();
        public static float TableBCDistance = 0f;
        public static Vector3 TableSceneCenter = new Vector3();

        public static string Language = "pl-PL";
        public static bool SceneStarted = false;
        public static float? TableScale = null;
        public static float? PlaygroundSceneObjectTimelineScale = null;
        public static float? PlaygroundSceneObjectScale = null;


        public static string BackgroundFilePath { get; set; }
        public static string ButtonGroupAFilePath { get; set; }
        public static string ButtonGroupBFilePath { get; set; }
        public static string ButtonGuideFilePath { get; set; }
        public static string ButtonTableFilePath { get; set; }
        public static string ButtonMuseumFilePath { get; set; }
        public static string ButtonPlaygroundFilePath { get; set; }
        public static string ButtonDefaultFilePath { get; set; }
        public static string ButtonCloseFilePath { get; set; }
        public static string ButtonLicenseFilePath { get; set; }
        public static string LogoFilePath { get; set; }

        public static List<PlaceBaseDto> AllPlaces { get; set; } = new List<PlaceBaseDto>();
        public static ARLocation.Location LastLocation { get; set; }
        public static Dictionary<string, string> Messages = new Dictionary<string, string>();
        public static DateTime TimeToShowAd { get; set; }
        public static bool? IsDemo { get; set; }

        public static InstructionType InstructionType { get; set; }
        public static SceneGroupType? BackToScenesType { get; set; }
        

        public static void PushDebugMessage(string key, object value)
        {
            if (Messages == null) Messages = new Dictionary<string, string>();
            if (Messages.ContainsKey(key))
            {
                Messages[key] = value?.ToString();
            }
            else
            {
                Messages.Add(key, value?.ToString());
            }
        }
        public static void ClearAll()
        {
            SceneQueue.Clear();
            Rendered.Clear();
            SceneAudio.Clear();
            PlayedAudio.Clear();
            Places.Clear();
            AllPlaces.Clear();
            ActivePlace = null;
            ActiveScene = null;
        }
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
    (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static void Restart(long sceneId)
        {
            if (Rendered.ContainsKey(sceneId))
            {
                SceneQueue[sceneId].AddRange(Rendered[sceneId]);
                SceneQueue[sceneId] = SceneQueue[sceneId].DistinctBy(x => x.id).ToList();
                Rendered[sceneId].Clear();
            }

            if (PlayedAudio.ContainsKey(sceneId))
            {
                SceneAudio[sceneId].AddRange(PlayedAudio[sceneId]);
                PlayedAudio[sceneId].Clear();
            }
            TableSceneStart = false;
            TableScale = null;
            SceneStarted = false;
            TablePoint1 = new Vector3();
            TablePoint2 = new Vector3();
            TablePoint3 = new Vector3();
            TablePoint4 = new Vector3();
            ApiPoint1 = new Vector3();
            ApiPoint2 = new Vector3();
            PlaygroundSceneObjectScale = null;
            PlaygroundSceneObjectTimelineScale = null;

            Time.timeScale = 1;
        }

        public static void Insert(DataEntry entry)
        {
            if (!SceneQueue.ContainsKey(entry.sceneId))
            {
                SceneQueue.Add(entry.sceneId, new List<DataEntry>());
            }
            if (!Rendered.ContainsKey(entry.sceneId))
            {
                Rendered.Add(entry.sceneId, new List<DataEntry>());
            }
            SceneQueue[entry.sceneId].Add(entry);
        }

        public static void InsertAudio(Audio audio)
        {
            if (!SceneAudio.ContainsKey(audio.SceneId))
            {
                SceneAudio.Add(audio.SceneId, new List<Audio>());
            }
            if (!PlayedAudio.ContainsKey(audio.SceneId))
            {
                PlayedAudio.Add(audio.SceneId, new List<Audio>());
            }
            SceneAudio[audio.SceneId].Add(audio);
        }

        public static bool IsNotEmpty()
        {
            return SceneQueue.ContainsKey(ActiveSceneId) && SceneQueue[ActiveSceneId]?.Count != 0;
        }

        public static void InitScene()
        {
            DateTime now = DateTime.Now;
            SceneStarted = false;
            if (SceneQueue.ContainsKey(ActiveSceneId))
            {
                foreach (var entry in SceneQueue[ActiveSceneId])
                {
                    entry.timeToRender = now.AddSeconds(entry.SecondsToRender);
                }
            }
            if (SceneAudio.ContainsKey(ActiveSceneId))
            {
                foreach (var entry in SceneAudio[ActiveSceneId])
                {
                    entry.StartTime = now.AddSeconds(entry.StartTimeSeconds);
                }
            }
        }

        public static Audio GetAudio()
        {
            var audio = SceneAudio[ActiveSceneId];
            if (audio != null)
            {
                var now = DateTime.Now;
                var toReturn = SceneAudio[ActiveSceneId].FirstOrDefault();//.Where(x => x.StartTime <= now).FirstOrDefault();
                if (toReturn != null)
                {
                    PlayedAudio[ActiveSceneId].Add(toReturn);
                    SceneAudio[ActiveSceneId].Remove(toReturn);

                    return toReturn;
                }
            }
            return null;
        }

        public static bool HasAudioToPlay()
        {
            if (!SceneAudio.ContainsKey(ActiveSceneId))
            {
                return false;
            }
            return SceneAudio[ActiveSceneId].Any();
        }

        public static List<DataEntry> ModelsToRender()
        {
            if (!SceneQueue.ContainsKey(ActiveSceneId))
            {
                return new List<DataEntry>();
            }
            var now = DateTime.Now;
            var toReturn = SceneQueue[ActiveSceneId].Where(x => x.timeToRender <= now).ToList();
            SceneQueue[ActiveSceneId].RemoveAll(x => toReturn.Any(y => y.id == x.id));
            toReturn.RemoveAll(x => Rendered[ActiveSceneId].Any(y => y.id == x.id));
            Rendered[ActiveSceneId].AddRange(toReturn);
            return toReturn;
        }

        public static long GetSceneAnimationLength()
        {
            if (SceneAnimationTimes.ContainsKey(ActiveSceneId))
                return SceneAnimationTimes[ActiveSceneId];
            return 0;
        }
    }
}
