using BLL.EntitiesDTOS.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface ISchoolSettingService
    {
        Task<SchoolInfoDTO?> GetSchoolInfoAsync();
        Task<SchoolInfoDTO> UpdateSchoolInfoAsync(UpdateSchoolInfoDTO dto);

    }
}
