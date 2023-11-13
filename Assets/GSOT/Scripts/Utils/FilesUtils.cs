using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TriLib;
using UnityEngine;
using System.IO.Compression;
using System.Collections;
using UnityEngine.Video;
using GSOT;
using Assets.GSOT.Scripts.LoadingScripts;
using System.Runtime.InteropServices;

namespace Assets.GSOT.Scripts.Utils
{
    public class FilesUtils : MonoBehaviour
    {
        private static string packagesPath = "";
        private static string soundsPath = "";
        private static string extractsPath = "";
        private static string buttonsPath = "";

        public static string DatabasePath { get; private set; }

        public static void Init()
        {
            Debug.Log("FileUtils  Metoda Init1");
            packagesPath = $"{Application.persistentDataPath}/Packages";
            extractsPath = $"{Application.persistentDataPath}/Models";
            soundsPath = $"{Application.persistentDataPath}/Sounds";
            DatabasePath = $"{Application.persistentDataPath}/Database";
            buttonsPath = $"{Application.persistentDataPath}/Buttons";
            Debug.Log("FileUtils  Metoda Init2");
            if (!Directory.Exists(packagesPath))
            {
                Debug.Log("FileUtils  Metoda Init3");
                Directory.CreateDirectory(packagesPath);
            }
            if (!Directory.Exists(soundsPath))
            {
                Debug.Log("FileUtils  Metoda Init4");
                Directory.CreateDirectory(soundsPath);
            }
            if (!Directory.Exists(DatabasePath))
            {
                Debug.Log("FileUtils  Metoda Init5");
                Directory.CreateDirectory(DatabasePath);
            }
            if (!Directory.Exists(buttonsPath))
            {
                Debug.Log("FileUtils  Metoda Init6");
                Directory.CreateDirectory(buttonsPath);
            }
            if (!Directory.Exists(extractsPath))
            {
                Debug.Log("FileUtils  Metoda Init7");
                Directory.CreateDirectory(extractsPath);
            }
            Debug.Log("FileUtils  Metoda Init8");
        }

        public static void DeleteOldFiles(List<string> filesNeeded)
        {
            Debug.Log("FileUtils  Metoda DeleteOldFiles1");
            if (Directory.Exists(packagesPath))
            {
                Debug.Log("FileUtils  Metoda DeleteOldFiles2");
                foreach (var file in Directory.GetFiles(packagesPath))
                {
                    Debug.Log("FileUtils  Metoda DeleteOldFiles3");
                    var fileName = GetFilenameWithoutExtension(file);
                    var fileExtension = GetFileExtension(file);
                    Debug.Log("FileUtils  Metoda DeleteOldFiles3");
                    if (!filesNeeded.Any(x => x.ToLower() == $"{fileName}{fileExtension.ToLower()}"))
                    {
                        Debug.Log("FileUtils  Metoda DeleteOldFiles4");
                        File.Delete(file);
                    }
                    Debug.Log("FileUtils  Metoda DeleteOldFiles5");
                }
                Debug.Log("FileUtils  Metoda DeleteOldFiles6");
                foreach (var dir in Directory.GetDirectories(extractsPath))
                {
                    Debug.Log("FileUtils  Metoda DeleteOldFiles7");

                    var fileName = Path.GetFileName(dir);
                    Debug.Log("FileUtils  Metoda DeleteOldFiles8");
                    if (!filesNeeded.Any(x => (x.Split('.')[0].ToLower()) == $"{fileName.ToLower()}"))
                    {
                        Debug.Log("FileUtils  Metoda DeleteOldFiles9");
                        Directory.Delete(dir, true);
                        Debug.Log("FileUtils  Metoda DeleteOldFiles10");
                    }
                    Debug.Log("FileUtils  Metoda DeleteOldFiles11");
                }
                Debug.Log("FileUtils  Metoda DeleteOldFiles12");
            }
            Debug.Log("FileUtils  Metoda DeleteOldFiles13");
            if (Directory.Exists(soundsPath))
            {
                Debug.Log("FileUtils  Metoda DeleteOldFiles14");
                foreach (var file in Directory.GetFiles(soundsPath))
                {
                    Debug.Log("FileUtils  Metoda DeleteOldFiles15");
                    var fileName = GetFilenameWithoutExtension(file);
                    var fileExtension = GetFileExtension(file);
                    Debug.Log("FileUtils  Metoda DeleteOldFiles16");
                    if (!filesNeeded.Any(x => x.ToLower() == $"{fileName}{fileExtension.ToLower()}"))
                    {
                        Debug.Log("FileUtils  Metoda DeleteOldFiles17");
                        File.Delete(file);
                        Debug.Log("FileUtils  Metoda DeleteOldFiles18");
                    }
                    Debug.Log("FileUtils  Metoda DeleteOldFiles19");
                }
                Debug.Log("FileUtils  Metoda DeleteOldFiles20");
            }
            Debug.Log("FileUtils  Metoda DeleteOldFiles21");
        }

        public static GameObject LoadModelFromFile(string name)
        {
            Debug.Log("FileUtils  Metoda LoadModelFromFile1");
            try
            {
                Debug.Log("FileUtils  Metoda LoadModelFromFile2");
                name = Path.GetFileNameWithoutExtension(name);
                Debug.Log("FileUtils  Metoda LoadModelFromFile3");
                return LoadModelFromBundle(name);
            }
            catch (Exception ex)
            {
                Debug.Log("FileUtils  Metoda LoadModelFromFile4");
                Debug.LogWarning($"Ex: {ex.Message}");
                Debug.Log("[ERROR]: Error in loading model from file.");
                return new GameObject();
            }
        }

        public static GameObject LoadModelFromBundle(string name)
        {
            Debug.Log("FileUtils  Metoda LoadModelFromBundle1");
            var modelFile = Directory.GetFiles($"{extractsPath}/{Path.GetFileNameWithoutExtension(name)}", "*.model").FirstOrDefault();
            Debug.Log("FileUtils  Metoda LoadModelFromBundle2");
            if (modelFile != null)
            {
                Debug.Log("FileUtils  Metoda LoadModelFromBundle3");
                var bundle = AssetBundle.LoadFromFile(modelFile);
                Debug.Log("FileUtils  Metoda LoadModelFromBundle4");
                GameObject model = null;
                Debug.Log("FileUtils  Metoda LoadModelFromBundle5");
                model = bundle.LoadAsset<GameObject>(bundle.GetAllAssetNames().First());
                Debug.Log("FileUtils  Metoda LoadModelFromBundle6");
                if (model != null)
                {
                    Debug.Log("FileUtils  Metoda LoadModelFromBundle7");
                    var animationComponent = model.AddComponent<AnimationListComponent>();
                    Debug.Log("FileUtils  Metoda LoadModelFromBundle8");
                    var renderer = model.AddComponent<SkinnedMeshRenderer>();
                    Debug.Log("FileUtils  Metoda LoadModelFromBundle9");
                    var anim = model.AddComponent<Animation>();
                    Debug.Log("FileUtils  Metoda LoadModelFromBundle10");
                    animationComponent.LoadAnimations(name);
                    Debug.Log("FileUtils  Metoda LoadModelFromBundle11");
                    LoadTextures(model, name);
                    Debug.Log("FileUtils  Metoda LoadModelFromBundle12");
                    bundle.Unload(false);
                    Debug.Log("FileUtils  Metoda LoadModelFromBundle13");
                    return model;
                }
            }
            return null;
        }

        public static GameObject LoadVideo(string name, GameObject prefab, DataEntry entry)
        {
            Debug.Log("FileUtils  Metoda LoadVideo1");
            var fileName = name.Split('.')[0];
            var fileExtension = GetFileExtension(name);
            Debug.Log("FileUtils  Metoda LoadVideo2");
            var localFilename = string.Format("{0}/{1}{2}", soundsPath, fileName, fileExtension.ToLower());
            Debug.Log("FileUtils  Metoda LoadVideo3");
            if (entry.Type == Models.ApiModels.Type.Graphic)
            {
                Debug.Log("FileUtils  Metoda LoadVideo4");
                var imgConverter = FindObjectOfType<IMG2Sprite>();
                Debug.Log("FileUtils  Metoda LoadVideo5");
                var image = imgConverter.LoadNewSprite(localFilename);
                Debug.Log("FileUtils  Metoda LoadVideo6");
                //prefab = new GameObject();
                //VideoPlayer vp = prefab.GetComponent<VideoPlayer>();
                //vp.enabled = false;
                if (entry.pathLines[0].GraphicVideoPosition != Models.ApiModels.Timeline.SceneObjectTimelineGraphicVideoPosition.TwoSidesNoFollow)
                {
                    Debug.Log("FileUtils  Metoda LoadVideo7");
                    SpriteRenderer sr = prefab.GetComponentInChildren<SpriteRenderer>();
                    Debug.Log("FileUtils  Metoda LoadVideo8");
                    sr.sprite = image;
                    Debug.Log("FileUtils  Metoda LoadVideo9");
                }
                else
                {
                    Debug.Log("FileUtils  Metoda LoadVideo10");
                    var srs = prefab.GetComponentsInChildren<SpriteRenderer>();
                    Debug.Log("FileUtils  Metoda LoadVideo11");
                    foreach (var sr in srs)
                    {
                        Debug.Log("FileUtils  Metoda LoadVideo12");
                        //image.texture.col
                        sr.sprite = image;
                        //sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
                        Debug.Log("FileUtils  Metoda LoadVideo13");
                    }
                }
                Debug.Log("FileUtils  Metoda LoadVideo14");
            }
            else
            {
                Debug.Log("FileUtils  Metoda LoadVideo15");
                var container = prefab.transform.Find("VideoContainer");
                Debug.Log("FileUtils  Metoda LoadVideo16");
                VideoPlayer vp;
                Debug.Log("FileUtils  Metoda LoadVideo17");
                if (container)
                {
                    Debug.Log("FileUtils  Metoda LoadVideo18");
                    vp = container.GetComponent<VideoPlayer>();
                }
                else
                {
                    Debug.Log("FileUtils  Metoda LoadVideo19");
                    vp = prefab.GetComponent<VideoPlayer>();
                    Debug.Log("FileUtils  Metoda LoadVideo20");
                }
                if (vp != null)
                {
                    Debug.Log("FileUtils  Metoda LoadVideo21");
                    Material mat = new Material(Shader.Find("Standard"));
                    //RenderTexture rt = new RenderTexture(1920, 1080, 1);
                    Debug.Log("FileUtils  Metoda LoadVideo22");
                    vp.url = localFilename;
                    Debug.Log("FileUtils  Metoda LoadVideo23");
                    //vp.clip
                    //var mr = prefab.GetComponent<MeshRenderer>();
                    //vp.renderMode = VideoRenderMode.RenderTexture;
                    //vp.targetTexture = rt;
                    //material.SetColor("", Color.red);
                    //mr.material = new Material(material);
                    //mr.material
                    //mat.SetTexture("Rt", rt);
                    //material.SetTexture("_MainTex", rt);
                    //mat.EnableKeyword("_DETAIL_MULX2");
                    //mat.EnableKeyword("_EMISSION");
                    //mat.SetTexture("_EmissionMap", rt);
                    //mat.SetTexture("_DETAIL_MULX2", rt);
                    //RenderTexture rt2 = (RenderTexture)rt.();
                    //mat.sete
                }
                Debug.Log("FileUtils  Metoda LoadVideo24");
            }
            string jsonString = JsonUtility.ToJson(entry);
            Debug.Log("FileUtils  Metoda LoadVideo28");
            prefab.name = jsonString;
            Debug.Log("FileUtils  Metoda LoadVideo29");
            //DontDestroyOnLoad(prefab);
            Loaded++;
            Debug.Log("FileUtils  Metoda LoadVideo30");
            return prefab;
        }

        public static int Loaded = 0;
        public static IEnumerator LoadModelFromBundleAsync(DataEntry entry)
        {
            Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync1");
            var modelFile = Directory.GetFiles($"{extractsPath}/{Path.GetFileNameWithoutExtension(entry.meshId)}", "*.model").FirstOrDefault();
            Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync2");
            var bundleLoadRequest = AssetBundle.LoadFromFileAsync(modelFile);
            Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync3");
            yield return bundleLoadRequest;
            Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync4");
            if (modelFile != null)
            {
                Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync5");
                var myLoadedAssetBundle = bundleLoadRequest.assetBundle;
                Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync6");
                if (myLoadedAssetBundle == null)
                {
                    Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync7");
                    Loaded++;

                    yield break;
                    Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync8");
                }
                Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync9");
                var assetLoadRequest = myLoadedAssetBundle.LoadAssetAsync<GameObject>(myLoadedAssetBundle.GetAllAssetNames().First());
                Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync10");
                yield return assetLoadRequest;
                Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync11");

                //var bundle = AssetBundle.LoadFromFile(modelFile);
                GameObject model = null;
                Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync12");
                model = assetLoadRequest.asset as GameObject;
                Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync13");
                if (model != null)
                {
                    Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync14");
                    var animationComponent = model.AddComponent<AnimationListComponent>();
                    Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync15");
                    var renderer = model.AddComponent<SkinnedMeshRenderer>();
                    Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync16");
                    var anim = model.AddComponent<Animation>();
                    Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync17");
                    animationComponent.LoadAnimations(entry.meshId);
                    Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync18");
                    LoadTextures(model, entry.meshId);
                    Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync19");
                    myLoadedAssetBundle.Unload(false);
                    Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync20");

                    model.name = JsonUtility.ToJson(entry);
                    Debug.Log("FileUtils  Metoda LoadModelFromBundleAsync21");
                    // DontDestroyOnLoad(model);
                }
            }
            Loaded++;
        }
        public static void LoadTextures(GameObject model, string name)
        {
            Debug.Log("FileUtils  Metoda LoadTextures1");
            //return;
            var parts = Directory.GetDirectories($@"{extractsPath}/{Path.GetFileNameWithoutExtension(name)}").Select(Path.GetFileName);
            Debug.Log("FileUtils  Metoda LoadTextures2");
            foreach (var part in parts)
            {
                Debug.Log("FileUtils  Metoda LoadTextures3");
                LoadTextures(model, Path.GetFileNameWithoutExtension(name), part);
                Debug.Log("FileUtils  Metoda LoadTextures4");
            }
        }

        static Texture2D Atex = new Texture2D(4, 4, TextureFormat.DXT1, false);
        static Texture2D Ntex = new Texture2D(4, 4, TextureFormat.DXT1, false);
        static Texture2D Mtex = new Texture2D(4, 4, TextureFormat.DXT1, false);
        static Texture2D Rtex = new Texture2D(4, 4, TextureFormat.DXT1, false);


        public static bool IsFileLocked(string filePath)
        {
            try
            {
                using (File.Open(filePath, FileMode.Open)) { }
            }
            catch (IOException e)
            {
                var errorCode = Marshal.GetHRForException(e) & ((1 << 16) - 1);

                return errorCode == 32 || errorCode == 33;
            }

            return false;
        }

        private static void infoOpliku(string plik)
        {
            if (File.Exists(plik))
            {
                Debug.Log($"Plik tekstury {plik} istnieje");
                if (IsFileLocked(plik)) Debug.Log($"Plik {plik} jest zablokowany");
                else
                    Debug.Log($"Plik {plik} jest niezablokowany");


            }
            else Debug.Log($"Plik tekstury {plik} NIE ISTENIEJE");
        }

        public static void LoadTextures(GameObject model, string name, string part)
        {
            Debug.Log("FileUtils  Metoda LoadTextures5");
            //return;
            List<GameObject> nodes = new List<GameObject>();
            Debug.Log("FileUtils  Metoda LoadTextures6");

            GetChildNodes(model, part, nodes);
            Debug.Log("FileUtils  Metoda LoadTextures7");
            foreach (var x in nodes)
            {
                Debug.Log("FileUtils  Metoda LoadTextures8");
                if (ModelsQueue.Textures2D.ContainsKey($@"{extractsPath}/{name}/{part}/a.png"))
                {
                    Debug.Log("FileUtils  Metoda LoadTextures9");
                    Atex = ModelsQueue.Textures2D[$@"{extractsPath}/{name}/{part}/a.png"];
                    Ntex = ModelsQueue.Textures2D[$@"{extractsPath}/{name}/{part}/n.png"];
                    Mtex = ModelsQueue.Textures2D[$@"{extractsPath}/{name}/{part}/m.png"];
                    Rtex = ModelsQueue.Textures2D[$@"{extractsPath}/{name}/{part}/r.png"];
                    Debug.Log("FileUtils  Metoda LoadTextures10");
                }
                else
                {
                    Debug.Log("FileUtils  Metoda LoadTextures11");
                  
                    Atex = new Texture2D(4, 4, TextureFormat.DXT1, false);
                    Ntex = new Texture2D(4, 4, TextureFormat.DXT1, false);
                    Mtex = new Texture2D(4, 4, TextureFormat.DXT1, false);
                    Rtex = new Texture2D(4, 4, TextureFormat.DXT1, false);

                    Debug.Log("FileUtils  Metoda LoadTextures12");

                    if (Directory.Exists($@"{extractsPath}/{name}/{part}")) Debug.Log("Katalog z teksturami istnieje"); else Debug.Log("Katalog z teksturami NIE ISTNIEJE");

                    infoOpliku($@"{extractsPath}/{name}/{part}/a.png");
                    infoOpliku($@"{extractsPath}/{name}/{part}/n.png");
                    infoOpliku($@"{extractsPath}/{name}/{part}/m.png");
                    infoOpliku($@"{extractsPath}/{name}/{part}/r.png");


                    //Debug.Log($" ----------- Ladowanie Atex={Atex.LoadImage(File.ReadAllBytes($@"{extractsPath}/{name}/{part}/a.png"))}");
                    //Debug.Log("FileUtils  Metoda LoadTextures12_1");
                    //Debug.Log($" ----------- Ladowanie Ntex={Ntex.LoadImage(File.ReadAllBytes($@"{extractsPath}/{name}/{part}/n.png"))}");
                    //Debug.Log("FileUtils  Metoda LoadTextures12_2");
                    //Debug.Log($" ----------- Ladowanie Mtex={Mtex.LoadImage(File.ReadAllBytes($@"{extractsPath}/{name}/{part}/m.png"))}");
                    //Debug.Log("FileUtils  Metoda LoadTextures12_3");
                    //Debug.Log($" ----------- Ladowanie Rtex={Rtex.LoadImage(File.ReadAllBytes($@"{extractsPath}/{name}/{part}/r.png"))}");
                    //Debug.Log("FileUtils  Metoda LoadTextures13");

                    Atex= Resources.Load("testowyobrazek.PNG") as Texture2D;
                    Ntex= Resources.Load("testowyobrazek.PNG") as Texture2D;
                    Mtex= Resources.Load("testowyobrazek.PNG") as Texture2D;
                    Rtex= Resources.Load("testowyobrazek.PNG") as Texture2D;

                    ModelsQueue.Textures2D.Add($@"{extractsPath}/{name}/{part}/a.png", Atex);
                    ModelsQueue.Textures2D.Add($@"{extractsPath}/{name}/{part}/n.png", Ntex);
                    ModelsQueue.Textures2D.Add($@"{extractsPath}/{name}/{part}/m.png", Mtex);
                    ModelsQueue.Textures2D.Add($@"{extractsPath}/{name}/{part}/r.png", Rtex);
                    Debug.Log("FileUtils  Metoda LoadTextures14");
                }
                Debug.Log("FileUtils  Metoda LoadTextures15");
                Renderer meshRenderer = x.GetComponent<SkinnedMeshRenderer>();
                Debug.Log("FileUtils  Metoda LoadTextures16");
                if (meshRenderer == null)
                {
                    Debug.Log("FileUtils  Metoda LoadTextures17");
                    meshRenderer = x.GetComponent<MeshRenderer>();
                    Debug.Log("FileUtils  Metoda LoadTextures18");
                }
                Debug.Log("FileUtils  Metoda LoadTextures19");
                if (meshRenderer == null)
                {
                    Debug.Log("FileUtils  Metoda LoadTextures20");
                    continue;
                }
                Debug.Log("FileUtils  Metoda LoadTextures21");
                Material material = new Material(Shader.Find("Autodesk Interactive"));
                Debug.Log("FileUtils  Metoda LoadTextures22");
                material.SetTexture("_MainTex", Atex);
                material.SetTexture("_MetallicGlossMap", Mtex);
                material.SetTexture("_SpecGlossMap", Rtex);
                material.SetTexture("_BumpMap", Ntex);
                material.EnableKeyword("_NORMALMAP");
                material.EnableKeyword("_METALLICGLOSSMAP");
                material.EnableKeyword("_SPECGLOSSMAP");
                Debug.Log("FileUtils  Metoda LoadTextures23");
                meshRenderer.material = material;
                ModelsQueue.Materials.Add(material);
                Debug.Log("FileUtils  Metoda LoadTextures24");
            }
        }


        public static void GetChildNodes(GameObject parent, string name, List<GameObject> list)
        {
            Debug.Log("FileUtils  Metoda GetChildNodes1");
            foreach (Transform child in parent.transform)
            {
                Debug.Log("FileUtils  Metoda GetChildNodes2");
                if (child.gameObject.name.Contains(name))
                {
                    Debug.Log("FileUtils  Metoda GetChildNodes3");
                    list.Add(child.gameObject);
                    Debug.Log("FileUtils  Metoda GetChildNodes4");
                }
                Debug.Log("FileUtils  Metoda GetChildNodes5");
                GetChildNodes(child.gameObject, name, list);
                Debug.Log("FileUtils  Metoda GetChildNodes6");
            }
        }

        public static void SaveModel(string url)
        {
            Debug.Log("FileUtils  Metoda SaveModel1");
            var fileName = GetFilenameWithoutExtension(url);
            Debug.Log("FileUtils  Metoda SaveModel2");
            var fileExtension = GetFileExtension(url);
            Debug.Log("FileUtils  Metoda SaveModel3");
            var localFilename = string.Format("{0}/{1}{2}", packagesPath, fileName, fileExtension.ToLower());
            Debug.Log("FileUtils  Metoda SaveModel4");

            if (!File.Exists(localFilename))
            {
                Debug.Log("FileUtils  Metoda SaveModel5");
                var response = MobileApiService.DownloadFile(url);
                Debug.Log("FileUtils  Metoda SaveModel6");
                File.WriteAllBytes(localFilename, response);
                Debug.Log("FileUtils  Metoda SaveModel7");
                if (GetFileExtension(url).ToLower() == ".zip")
                {
                    Debug.Log("FileUtils  Metoda SaveModel8");
                    Extract($"{fileName}{fileExtension}");
                    Debug.Log("FileUtils  Metoda SaveModel9");
                }
            }
        }

        public static string SaveImage(string file)
        {
            Debug.Log("FileUtils  Metoda SaveImage1");
            if (string.IsNullOrEmpty(file))
            {
                Debug.Log("FileUtils  Metoda SaveImage2");
                return null;
            }
            Debug.Log("FileUtils  Metoda SaveImage3");
            var fileName = GetFilenameWithoutExtension(file);
            Debug.Log("FileUtils  Metoda SaveImage4");
            var fileExtension = GetFileExtension(file);
            Debug.Log("FileUtils  Metoda SaveImage5");
            var localFilename = string.Format("{0}/{1}{2}", buttonsPath, fileName, fileExtension.ToLower());
            Debug.Log("FileUtils  Metoda SaveImage6");
            if (!File.Exists(localFilename))
            {
                Debug.Log("FileUtils  Metoda SaveImage7");
                var response = MobileApiService.DownloadFile(file);
                Debug.Log("FileUtils  Metoda SaveImage8");
                File.WriteAllBytes(localFilename, response);
                Debug.Log("FileUtils  Metoda SaveImage9");
                if (GetFileExtension(file).ToLower() == ".zip")
                {
                    Debug.Log("FileUtils  Metoda SaveImage10");
                    Extract($"{fileName}{fileExtension}");
                    Debug.Log("FileUtils  Metoda SaveImage11");
                }
                Debug.Log("FileUtils  Metoda SaveImage12");
            }
            Debug.Log("FileUtils  Metoda SaveImage13");
            return localFilename;
        }

        public static void Extract(string name)
        {
            Debug.Log("FileUtils  Metoda Extract1");
            ZipFile.ExtractToDirectory($"{packagesPath}/{name}", $"{extractsPath}/{Path.GetFileNameWithoutExtension(name)}");
            Debug.Log("FileUtils  Metoda Extract2");

        }

        public static string SaveSound(string url)
        {
            Debug.Log("FileUtils  Metoda SaveSound1");

            if (string.IsNullOrEmpty(url))
            {
                Debug.Log("FileUtils  Metoda SaveSound2");
                return null;
            }
            Debug.Log("FileUtils  Metoda SaveSound3");
            var fileName = GetFilenameWithoutExtension(url);
            Debug.Log("FileUtils  Metoda SaveSound4");
            var fileExtension = GetFileExtension(url);
            Debug.Log("FileUtils  Metoda SaveSound5");
            var localFilename = string.Format("{0}/{1}{2}", soundsPath, fileName, fileExtension.ToLower());
            Debug.Log("FileUtils  Metoda SaveSound6");
            if (!File.Exists(localFilename))
            {
                Debug.Log("FileUtils  Metoda SaveSound7");
                var response = MobileApiService.DownloadFile(url);
                Debug.Log("FileUtils  Metoda SaveSound8");
                File.WriteAllBytes(localFilename, response);
                Debug.Log("FileUtils  Metoda SaveSound9");
            }
            Debug.Log("FileUtils  Metoda SaveSound10");
            return localFilename;
        }

        public static List<AnimationClip> GetAnimations(string name)
        {
            Debug.Log("FileUtils  Metoda GetAnimations1");
            List<AnimationClip> animations = new List<AnimationClip>();
            Debug.Log("FileUtils  Metoda GetAnimations2");
            var animFile = Directory.GetFiles($"{extractsPath}/{Path.GetFileNameWithoutExtension(name)}", "*.anim").FirstOrDefault();
            Debug.Log("FileUtils  Metoda GetAnimations3");
            if (animFile == null) return new List<AnimationClip>();
            Debug.Log("FileUtils  Metoda GetAnimations4");
            var bundle = AssetBundle.LoadFromFile(animFile);
            Debug.Log("FileUtils  Metoda GetAnimations5");
            if (bundle == null) return new List<AnimationClip>();
            Debug.Log("FileUtils  Metoda GetAnimations6");
            var animationsNames = bundle.GetAllAssetNames();
            Debug.Log("FileUtils  Metoda GetAnimations7");

            for (int i = 0; i < animationsNames.Length; i++)
            {
                Debug.Log("FileUtils  Metoda GetAnimations8");
                AnimationClip animationClip = bundle.LoadAsset(animationsNames[i]) as AnimationClip;
                Debug.Log("FileUtils  Metoda GetAnimations9");
                animations.Add(animationClip);
                Debug.Log("FileUtils  Metoda GetAnimations10");
            }
            Debug.Log("FileUtils  Metoda GetAnimations11");
            bundle.Unload(false);
            Debug.Log("FileUtils  Metoda GetAnimations12");
            return animations;
        }

        #region Helpers
        public static string GetFilenameWithoutExtension(string url)
        {
            return TriLib.FileUtils.GetFilenameWithoutExtension(url);
        }

        public static string GetFileExtension(string url)
        {
            return TriLib.FileUtils.GetFileExtension(url);
        }
        #endregion
    }
}