using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace BLL.EntitiesDTOS.General
{
    public class SchoolInfoDTO
    {
        public int SettingId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public string? SchoolLogo { get; set; }
        public DateTime? LastUpdated { get; set; }
    }


    public class UpdateSchoolInfoDTO
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "اسم المدرسة مطلوب")]
        [System.ComponentModel.DataAnnotations.StringLength(200, MinimumLength = 2, ErrorMessage = "اسم المدرسة يجب أن يكون بين 2 و 200 حرف")]
        public string SchoolName { get; set; } = string.Empty;
        //public string? SchoolLogo { get; set; }

        // ملف الصورة الفعلي عند الرفع المباشر
        public IFormFile? LogoFile { get; set; }
    }

}

