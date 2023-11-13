using Assets.GSOT.Scripts.Enums.ApiEnums;
using Assets.GSOT.Scripts.Models.ApiModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApplicationModels
{
    public class ApplicationPlace
    {
        public List<ApplicationScene> Scenes { get; set; }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public double LicensePrice { get; set; }

        public bool IsAvailableInDemoVersion { get; set; }
        public SceneUsingMode SceneUsingMode { get; set; }
        public long? GSOrderProductLicenseId { get; set; }

        public LocalizationDto rectanglePoint1EPSG4326 { get; set; }
        public LocalizationDto rectanglePoint2EPSG4326 { get; set; }
        public int rectangleSecondSideLengthInMeters { get; set; }
        public string SceneGroup1Name { get; set; }
        public string SceneGroup2Name { get; set; }
    }
}
