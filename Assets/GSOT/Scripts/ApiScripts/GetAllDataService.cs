using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.Models.ApiModels;
using Assets.GSOT.Scripts.Models.ApplicationModels;
using Assets.GSOT.Scripts.Models.Wrappers;
using Assets.GSOT.Scripts.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.GSOT.Scripts.ApiScripts
{
    public class GetAllDataService
    {
        private string _getAvailableDataResultPath = Path.Combine(FilesUtils.DatabasePath, "GetAllData.json");
        public int ObjectsToDownload = 0;
        public int Downloaded = 0;
        public bool GetAllData()
        {
            ModelsQueue.ClearAll();
            try
            {
                bool result = true;

                var data = MobileApiService.GetAvailable();

                if (data != null)
                {
                    if (MobileApiService.MobileAppApiVersion < data.Data.Application.MobileAppMinVersion)
                    {
                        try
                        {
                            AndroidMessageService.ShowAndroidToastMessage("Twoja aplikacja jest nieauktualna - w celu poprawnego dzialania aplikacji pobierz jej aktualizacje");
                        }
                        catch { }
                        data = GetAvailableDataResultSaved();
                    }
                    SavetGetAvailableDataResult(data);
                }
                else
                {
                    result = false;
                    data = GetAvailableDataResultSaved();

                    if (data == null)
                    {
                        return false;
                    }
                }


                if (data.Data.Objects.Count > 0)
                {
                    this.ObjectsToDownload = data.Data.Objects.Count;

                    foreach (var obj in data.Data.Objects)
                    {
                        var fw = new FileWrapper(obj);
                        fw.Save();
                        Downloaded++;
                    }
                }

                ModelsQueue.Configuration = data.Data.Configuration;

                List<string> allFiles = data.Data.Objects.Select(x => x.File.DiscFileName).ToList();

                allFiles.Add(data.Data.Application.BackgroundFile?.DiscFileName);
                allFiles.Add(data.Data.Application.ButtonGroupAFile?.DiscFileName);
                allFiles.Add(data.Data.Application.ButtonGroupBFile?.DiscFileName);
                allFiles.Add(data.Data.Application.ButtonGuideFile?.DiscFileName);
                allFiles.Add(data.Data.Application.ButtonTableFile?.DiscFileName);
                allFiles.Add(data.Data.Application.ButtonMuseumFile?.DiscFileName);
                allFiles.Add(data.Data.Application.ButtonPlaygroundFile?.DiscFileName);
                allFiles.Add(data.Data.Application.ButtonDefaultFile?.DiscFileName);
                allFiles.Add(data.Data.Application.ButtonCloseFile?.DiscFileName);
                allFiles.Add(data.Data.Application.ButtonLicenseFile?.DiscFileName);
                allFiles.Add(data.Data.Application.LogoFile?.DiscFileName);
                allFiles.AddRange(data.Data.Places[0].Scenes.Select(x => x.ThemeMusicFile?.DiscFileName));
                allFiles.AddRange(data.Data.Places[0].Scenes.Select(x => x.LogoFile?.DiscFileName));
                allFiles.AddRange(data.Data.Places[0].Scenes.Select(x => x.RectangleFile?.DiscFileName));


                Assets.GSOT.Scripts.Utils.FilesUtils.DeleteOldFiles(allFiles.Where(x => !string.IsNullOrEmpty(x)).ToList());

                ModelsQueue.BackgroundFilePath = FilesUtils.SaveImage(MobileApiService.DownloadFileUrl(data.Data.Application.BackgroundFile?.DiscFileName));
                ModelsQueue.ButtonGroupAFilePath = FilesUtils.SaveImage(MobileApiService.DownloadFileUrl(data.Data.Application.ButtonGroupAFile?.DiscFileName));
                ModelsQueue.ButtonGroupBFilePath = FilesUtils.SaveImage(MobileApiService.DownloadFileUrl(data.Data.Application.ButtonGroupBFile?.DiscFileName));
                ModelsQueue.ButtonGuideFilePath = FilesUtils.SaveImage(MobileApiService.DownloadFileUrl(data.Data.Application.ButtonGuideFile?.DiscFileName));
                ModelsQueue.ButtonTableFilePath = FilesUtils.SaveImage(MobileApiService.DownloadFileUrl(data.Data.Application.ButtonTableFile?.DiscFileName));
                ModelsQueue.ButtonMuseumFilePath = FilesUtils.SaveImage(MobileApiService.DownloadFileUrl(data.Data.Application.ButtonMuseumFile?.DiscFileName));
                ModelsQueue.ButtonPlaygroundFilePath = FilesUtils.SaveImage(MobileApiService.DownloadFileUrl(data.Data.Application.ButtonPlaygroundFile?.DiscFileName));
                ModelsQueue.ButtonDefaultFilePath = FilesUtils.SaveImage(MobileApiService.DownloadFileUrl(data.Data.Application.ButtonDefaultFile?.DiscFileName));
                ModelsQueue.ButtonCloseFilePath = FilesUtils.SaveImage(MobileApiService.DownloadFileUrl(data.Data.Application.ButtonCloseFile?.DiscFileName));
                ModelsQueue.ButtonLicenseFilePath = FilesUtils.SaveImage(MobileApiService.DownloadFileUrl(data.Data.Application.ButtonLicenseFile?.DiscFileName));
                ModelsQueue.LogoFilePath = FilesUtils.SaveImage(MobileApiService.DownloadFileUrl(data.Data.Application.LogoFile?.DiscFileName));

                DateTime now = DateTime.Now;
                foreach (var place in data.Data.Places)
                {
                    if (!place.Scenes.Any())
                    {
                        continue;
                    }
                    ModelsQueue.Places.Add(new ApplicationPlace()
                    {
                        Name = place.Name,
                        Description = place.Description,
                        LicensePrice = place.LicensePrice,
                        IsAvailableInDemoVersion = place.IsAvailableInDemoVersion,
                        Id = place.Id,
                        Scenes = place.Scenes.Where(x => x.SceneObjects.Any()).Select(x => new ApplicationScene()
                        {
                            Id = x.Id,
                            Name = x.Name,
                            DemoVersionTimeInSeconds = x.DemoVersionTimeInSeconds,
                            Description = x.Description,
                            GroupType = x.GroupType,
                            GroundColor = x.GroundColor,
                            ButtonImage = Utils.FilesUtils.SaveImage(MobileApiService.DownloadFileUrl(x.LogoFile?.DiscFileName)),
                            ThemeMusic = Utils.FilesUtils.SaveSound(MobileApiService.DownloadFileUrl(x.ThemeMusicFile?.DiscFileName)),
                            RectangleFile = Utils.FilesUtils.SaveImage(MobileApiService.DownloadFileUrl(x.RectangleFile?.DiscFileName)),
                            FirstSideLength = x.FirstSideLength,
                            SecondSideLength = x.SecondSideLength,
                            IsAvailableInPlaygroundScene = x.IsAvailableInPlaygroundScene,
                            IsAvailableInTableSceneUsingMode = x.IsAvailableInTableSceneUsingMode,
                            Scale = x.Scale,
                            RLengthInMeters = x.RLengthInMeters,
                            PlaygroundSceneObjectTimelineScale = x.PlaygroundSceneObjectTimelineScale,
                            PlaygroundObjectScale = x.PlaygroundObjectScale,
                            GroundColorAlpha = x.GroundColorAlpha
                        }).ToList(),
                        SceneUsingMode = place.SceneUsingMode,
                        GSOrderProductLicenseId = place.GSOrderProductLicenseId,
                        SceneGroup1Name = place.SceneGroup1Name,
                        SceneGroup2Name = place.SceneGroup2Name,
                        rectanglePoint1EPSG4326 = place.rectanglePoint1EPSG4326,
                        rectanglePoint2EPSG4326 = place.rectanglePoint2EPSG4326
                    });
                    foreach (var scene in place.Scenes)
                    {
                        if (!scene.SceneObjects.Any() || !scene.SceneObjects.SelectMany(x => x.Timelines).Any())
                        {
                            continue;
                        }

                        var longestTimeline = scene.SceneObjects.SelectMany(x => x.Timelines)
                            .Select(x => (long?)x.EndTimeInSeconds)
                            .DefaultIfEmpty()
                            .Max();

                        if (!ModelsQueue.SceneAnimationTimes.ContainsKey(scene.Id))
                        {
                            ModelsQueue.SceneAnimationTimes.Add(scene.Id, longestTimeline ?? 0);
                        }
                        else if (longestTimeline.HasValue && ModelsQueue.SceneAnimationTimes[scene.Id] < longestTimeline.Value)
                        {
                            ModelsQueue.SceneAnimationTimes[scene.Id] = longestTimeline.Value;
                        }
                        foreach (var obj in scene.SceneObjects)
                        {
                            if (obj.Timelines.Any())
                            {
                                var gsObj = data.Data.Objects.Where(x => x.Id == obj.GsObjectId).FirstOrDefault();

                                if (gsObj != null)
                                {
                                    if (gsObj.Type != Assets.GSOT.Scripts.Models.ApiModels.Type.Sound)
                                    {
                                        try
                                        {
                                            var cv = new SceneObjectConverter(obj);
                                            var entry = cv.ToDataEntry(gsObj.Type);
                                            entry.SecondsToRender = obj.Timelines.OrderBy(x => x.StartTimeInSeconds).First().StartTimeInSeconds;
                                            var model = data.Data.Objects.Where(x => x.Id.ToString() == entry.meshId).FirstOrDefault();
                                            entry.meshId = model.File.DiscFileName;
                                            //entry.description = model.Description;
                                            entry.name = model.Name;
                                            ModelsQueue.Insert(entry);
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.LogWarning($"Ex: {ex.Message}");
                                        }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(gsObj.File.DiscFileName))
                                        {
                                            Audio a = new Audio()
                                            {
                                                Id = obj.Id,
                                                FileName = gsObj.File.DiscFileName,
                                                StartTimeSeconds = obj.Timelines.First().StartTimeInSeconds,
                                                SceneId = obj.GsSceneId,
                                                Timelines = obj.Timelines.Select(x =>
                                                    new AudioTimeline()
                                                    {
                                                        StartTimeInSeconds = x.StartTimeInSeconds,
                                                        EndTimeInSeconds = x.EndTimeInSeconds
                                                    }).ToList()
                                            };
                                            ModelsQueue.InsertAudio(a);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                ModelsQueue.AllPlaces.AddRange(data.Data.AllPlaces);

                return result;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Ex: {ex.Message}");
                return false;
            }
        }

        #region Private
        private void SavetGetAvailableDataResult(RootObject data)
        {
            DatatabaseItemWrapper itemToSave = new DatatabaseItemWrapper()
            {
                InsertDate = DateTime.Now,
                RootObject = data
            };
            System.IO.File.WriteAllText(_getAvailableDataResultPath, JsonConvert.SerializeObject(itemToSave));
        }

        private RootObject GetAvailableDataResultSaved()
        {
            DatatabaseItemWrapper result = System.IO.File.Exists(_getAvailableDataResultPath)
                ? JsonConvert.DeserializeObject<DatatabaseItemWrapper>(System.IO.File.ReadAllText(_getAvailableDataResultPath))
                : null;
            if (result != null && result.RootObject.Data.Configuration.MobileAppMaxOfflineHours < (DateTime.Now - result.InsertDate).TotalHours)
            {
                result = null;
            }
            return result?.RootObject;
        }
        #endregion
    }
}
