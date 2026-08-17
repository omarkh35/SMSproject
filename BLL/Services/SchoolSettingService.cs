using BLL.EntitiesDTOS.General;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class SchoolSettingService : ISchoolSettingService
    {
        private readonly IBaseRepositories<SchoolSetting> _schoolSettingRepo;

        public SchoolSettingService(IBaseRepositories<SchoolSetting> schoolSettingRepo)
        {
            _schoolSettingRepo = schoolSettingRepo;
        }

        public async Task<SchoolInfoDTO?> GetSchoolInfoAsync()
        {
            var settings = await _schoolSettingRepo.GetAllAsync();
            var setting = settings.FirstOrDefault();

            if (setting == null)
            {
                return new SchoolInfoDTO
                {
                    SettingId = 0,
                    SchoolName = "MEDAD",
                    SchoolLogo = null,
                    LastUpdated = null
                };
            }

            return new SchoolInfoDTO
            {
                SettingId = setting.SettingId,
                SchoolName = setting.SchoolName ?? string.Empty,
                SchoolLogo = setting.SchoolLogo,
                LastUpdated = setting.LastUpdated
            };
        }

        public async Task<SchoolInfoDTO> UpdateSchoolInfoAsync(UpdateSchoolInfoDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "بيانات المدرسة مطلوبة.");

            var settings = await _schoolSettingRepo.GetAllAsync();
            var setting = settings.FirstOrDefault();

            if (setting == null)
            {
                // إذا لم يكن هناك سجل سابق، يتم إنشاء سجل جديد بالقيم المدخلة
                setting = new SchoolSetting
                {
                    SchoolName = dto.SchoolName.Trim(),
                    SchoolLogo = dto.SchoolLogo,
                    LastUpdated = DateTime.Now
                };

                await _schoolSettingRepo.AddAsync(setting);
                await _schoolSettingRepo.SaveChangesAsync();
            }
            else
            {
                // تحديث السجل الموجود مسبقاً
                setting.SchoolName = dto.SchoolName.Trim();
                setting.SchoolLogo = dto.SchoolLogo;
                setting.LastUpdated = DateTime.Now;

                _schoolSettingRepo.UpdateAsync(setting);
                await _schoolSettingRepo.SaveChangesAsync();
            }

            return new SchoolInfoDTO
            {
                SettingId = setting.SettingId,
                SchoolName = setting.SchoolName ?? string.Empty,
                SchoolLogo = setting.SchoolLogo,
                LastUpdated = setting.LastUpdated
            };
        }

    }
    }
