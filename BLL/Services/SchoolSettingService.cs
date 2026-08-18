using BLL.EntitiesDTOS.General;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using System.IO;
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
        private readonly IFileService _fileService;

        public SchoolSettingService(IBaseRepositories<SchoolSetting> schoolSettingRepo, IFileService fileService)
        {
            _schoolSettingRepo = schoolSettingRepo;
            _fileService = fileService;
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

            string? logoPath = setting?.SchoolLogo;

            if (dto.LogoFile != null)
            {
                if (!string.IsNullOrEmpty(setting?.SchoolLogo))
                {
                    _fileService.DeleteFile(setting.SchoolLogo);
                }
                logoPath = await _fileService.SaveFileAsync(dto.LogoFile, "logos");
            }


            if (setting == null)
            {
                setting = new SchoolSetting
                {
                    SchoolName = dto.SchoolName.Trim(),
                    SchoolLogo = logoPath,
                    LastUpdated = DateTime.Now
                };

                await _schoolSettingRepo.AddAsync(setting);
                await _schoolSettingRepo.SaveChangesAsync();
            }
            else
            {
                setting.SchoolName = dto.SchoolName.Trim();
                if (!string.IsNullOrEmpty(logoPath))
                {
                    setting.SchoolLogo = logoPath;
                }
                setting.LastUpdated = DateTime.UtcNow;
                _schoolSettingRepo.UpdateAsync(setting);
                await _schoolSettingRepo.SaveChangesAsync();
            }

            return new SchoolInfoDTO
            {
                SettingId = setting.SettingId,
                SchoolName = setting.SchoolName,
                SchoolLogo = _fileService.GetFullUrl(setting.SchoolLogo),
                LastUpdated = setting.LastUpdated
            };
        }

    }
    }
