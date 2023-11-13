using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GSOT.Scripts.Models.ApiModels
{
    public class ApplicationDto
    {
        public File BackgroundFile { get; set; }
        public File ButtonGroupAFile { get; set; }
        public File ButtonGroupBFile { get; set; }
        public File ButtonGuideFile { get; set; }
        public File ButtonMuseumFile { get; set; }
        public File ButtonPlaygroundFile { get; set; }
        public File ButtonTableFile { get; set; }
        public File ButtonDefaultFile { get; set; }
        public File ButtonCloseFile { get; set; }
        public File ButtonLicenseFile { get; set; }
        public File LogoFile { get; set; }
        public int MobileAppMinVersion { get; set; }
    }
}
