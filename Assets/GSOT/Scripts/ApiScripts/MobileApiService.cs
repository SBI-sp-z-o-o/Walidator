using Assets.GSOT.Scripts.LoadingScripts;
using Assets.GSOT.Scripts.Models.ApiModels;
using Assets.GSOT.Scripts.Models.ApiModels.AdvertModels;
using Assets.GSOT.Scripts.Models.ApiModels.EventLog.Models;
using Newtonsoft.Json;
//using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MobileApiService
{
    private const string ApiKey = "160AC0BD-D8D3-4835-804D-0C60FC1F533D";
    private const string ApiUrl = "https://mobileapi.gsot.pl/api/v1";

    public static int MobileAppApiVersion = 1;

    public static string DownloadFileUrl(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }
        return $"{ApiUrl}/File/Download/{fileName}";
    }

    public static List<ModelWithLocationDto> GetModels(string name)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{ApiUrl}/Model/GetModels?Name={name}");
            AddRequiredHeaders(request);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string jsonResponse = reader.ReadToEnd();
            var parsedResponse = JsonConvert.DeserializeObject<ApiResultBaseGeneric<List<ModelWithLocationDto>>>(jsonResponse);
            return parsedResponse.Data;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Ex: {ex.Message}");
            return null;
        }
    }

    public static byte[] DownloadFile(string url)
    {
        WebClient request = new WebClient();
        AddRequiredHeaders(request);
        byte[] responseData = request.DownloadData(url);

        return responseData;
    }

    public static RootObject GetAvailable()
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{ApiUrl}/Place/GetAvailable");
            AddRequiredHeaders(request);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string jsonResponse = reader.ReadToEnd();
            var parsedResponse = JsonConvert.DeserializeObject(jsonResponse, typeof(RootObject)) as RootObject;
            return parsedResponse;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Ex: {ex.Message}");
            return null;
        }
    }

    public static AdvertRoot GetAdverts(float lat, float longi, float alt)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{ApiUrl}/Advert/GetAvailable?LatitudeEPSG4326={lat.ToString(CultureInfo.InvariantCulture)}&LongitudeEPSG4326={longi.ToString(CultureInfo.InvariantCulture)}&AltitudeEPSG4326={alt.ToString(CultureInfo.InvariantCulture)}");
            AddRequiredHeaders(request);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string jsonResponse = reader.ReadToEnd();
            var parsedResponse = JsonConvert.DeserializeObject(jsonResponse, typeof(AdvertRoot)) as AdvertRoot;
            return parsedResponse;
        }
        catch(Exception ex)
        {
            Debug.LogWarning($"Ex: {ex.Message}");
            return null;
        }
    }

    public static SendLicenseModel RegisterCode(string code)
    {
        SendLicenseModel result = new SendLicenseModel()
        {
            Status = 0,
            StatusMessage = Translator.Instance().GetString("Error")
        };
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{ApiUrl}/OrderProductLicense/RegisterCode");
            AddRequiredHeaders(request);
            //request.bo
            string postData = JsonConvert.SerializeObject(new { code });
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = byteArray.Length;
            request.Method = "POST";
            request.ContentType = "application/json";
            using (var stream = request.GetRequestStream())
            {
                stream.Write(byteArray, 0, byteArray.Length);
            }
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result.Status = 1;
                return result;
            }
            else
            {
                return result;
            }
        }
        catch (WebException ex)
        {
            if(ex.Response == null)
            {
                result.StatusMessage = Translator.Instance().GetString("DataDownloadError");
                return result;
            }
            using (var stream = ex.Response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                string message = reader.ReadToEnd();
                var parsedResponse = JsonConvert.DeserializeObject(message, typeof(SendLicenseModel)) as SendLicenseModel;
                result.Status = parsedResponse.Status;
                result.StatusMessage = parsedResponse.StatusMessage;
                return result;
            }
        }
        catch(Exception ex)
        {
            Debug.LogWarning($"Ex: {ex.Message}");
            return result;
        }
    }

   #region EventLog
    public static bool EventLogAdd(EventLogAddModel model)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"{ApiUrl}/EventLog/Add");
            AddRequiredHeaders(request);
            CreatePostRequestBody(request, model);
            
            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Ex: {ex.Message}");
            return false;
        }
    }
    #endregion

    #region Private
    private static void AddRequiredHeaders(HttpWebRequest request)
    {
        if (!string.IsNullOrEmpty(ModelsQueue.Language))
        {
            request.Headers.Add("language", ModelsQueue.Language);
        }
        request.Headers.Add("securityKey", ApiKey);
        request.Headers.Add("deviceUID", ModelsQueue.DeviceId);
        //if (Debug.isDebugBuild)
        //{
        //request.Headers.Add("appGuid", "e3677d03-a3d0-4821-bbef-6c5a24407ff1"); //karol test
        //}
        //else
        //{
        request.Headers.Add("appGuid", "ec271a3f-838f-461b-a348-2101818b9344"); // grunwald
        //request.Headers.Add("appGuid", "c9507a95-11d7-4119-a6da-12615ae20ff7"); // sbi
        //}
    }

    private static void AddRequiredHeaders(WebClient request)
    {
        request.Headers.Add("securityKey", ApiKey);
        request.Headers.Add("deviceUID", ModelsQueue.DeviceId);
    }

    private static void CreatePostRequestBody(HttpWebRequest request, object model)
    {
        string postData = JsonConvert.SerializeObject(model);
        byte[] byteArray = Encoding.UTF8.GetBytes(postData);
        request.ContentLength = byteArray.Length;
        request.Method = "POST";
        request.ContentType = "application/json";
        using (var stream = request.GetRequestStream())
        {
            stream.Write(byteArray, 0, byteArray.Length);
        }
    }
    #endregion
}

public class ApiResultBase
{
    public ApiResultStatus Status { get; set; }

    public string StatusMessage { get; set; }
}

public enum ApiResultStatus
{
    Ok = 1,

    [Description("Internal server error")]
    InternalServerError = 2,

    [Description("Security key not provided")]
    SecurityKeyNotProvided = 3,

    [Description("Incorrect security key")]
    IncorrectSecurityKey = 4
}


public class ApiResultBaseGeneric<T> : ApiResultBase
{
    public T Data { get; set; }
}

public class ModelDto
{
    public long Id { get; set; }

    public string Name { get; set; }
}

public class ModelWithLocationDto : ModelDto
{
    public List<ModelLocationDto> Locations { get; set; }
}

public class ModelLocationDto
{
    public long Id { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public decimal? Altitude { get; set; }
    public int StartTimeInSeconds { get; set; }
    public DateTime? RenderTime { get; set; }
}
