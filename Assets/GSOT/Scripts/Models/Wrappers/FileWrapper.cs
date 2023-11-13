using Assets.GSOT.Scripts.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSOT;

namespace Assets.GSOT.Scripts.Models.Wrappers
{
    public class FileWrapper
    {
        //public FileWrapper(ApiModels.Object obj)
        //{
        //    this = obj as FileWrapper;
        //}
        private ApiModels.Object _object;

        public FileWrapper(ApiModels.Object obj)
        {
            _object = obj;
        }

        public DataEntry ToDataEntry()
        {
            return new DataEntry()
            {
                meshId = _object.File.DiscFileName
            };
        }

        public void Save()
        {
            if (_object.Type == Assets.GSOT.Scripts.Models.ApiModels.Type.Model)
            {
                Utils.FilesUtils.SaveModel(MobileApiService.DownloadFileUrl(_object.File.DiscFileName));
            }
            else
            {

                Utils.FilesUtils.SaveSound(MobileApiService.DownloadFileUrl(_object.File.DiscFileName));
            }
        }
    }
}
