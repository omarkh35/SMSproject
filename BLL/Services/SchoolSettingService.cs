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

            // 1. جلب الإعدادات الحالية المتوفرة في قاعدة البيانات
            var settings = await _schoolSettingRepo.GetAllAsync();
            var setting = settings.FirstOrDefault();

            string? logoPath = setting?.SchoolLogo;

            // 2. معالجة رفع الشعار الجديد محلياً بالـ GUID والمسار المادي للمشروع
            if (dto.LogoFile != null && dto.LogoFile.Length > 0)
            {
                // أ. مسح ملف الشعار القديم ماديّاً من جهازك لتوفير مساحة القرص
                if (!string.IsNullOrEmpty(setting?.SchoolLogo))
                {
                    string oldPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", setting.SchoolLogo);
                    if (File.Exists(oldPhysicalPath))
                    {
                        File.Delete(oldPhysicalPath);
                    }
                }

                // ب. توليد اسم فريد مئة بالمئة للشعار الجديد باستخدام الـ GUID
                string fileExtension = Path.GetExtension(dto.LogoFile.FileName);
                string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";

                // ج. تثبيت ممر المجلد المحلي بداخل wwwroot/uploads/logos
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "logos");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string physicalFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                // د. حفظ ملف الشعار الفعلي على القرص الصلب محلياً
                using (var fileStream = new FileStream(physicalFilePath, FileMode.Create))
                {
                    await dto.LogoFile.CopyToAsync(fileStream);
                }

                // هـ. استخلاص المسار النسبي الآمن للتخزين بجدول قاعدة البيانات
                logoPath = $"uploads/logos/{uniqueFileName}";
            }

            // 3. مزامنة البيانات وحفظ الإجراء التعديلي (مطابقة صريحة لـ SettingID الكبير للسكافولدينج)
            if (setting == null)
            {
                setting = new SchoolSetting
                {
                    SchoolName = dto.SchoolName.Trim(),
                    SchoolLogo = logoPath,
                    LastUpdated = DateTime.UtcNow // توحيد التوقيت العالمي لمنع تضارب الخادم
                };
                await _schoolSettingRepo.AddAsync(setting);
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
            }

            await _schoolSettingRepo.SaveChangesAsync();

            // 4. صياغة الرد المرجوع بالمسار النسبي ليقوم الـ Controller بدمج الـ Host معه ديناميكياً
            return new SchoolInfoDTO
            {
                SettingId = setting.SettingId, // مطابقة الحروف الكبيرة للسكافولدينج SettingID لسكريبت الداتابيز
                SchoolName = setting.SchoolName,
                SchoolLogo = setting.SchoolLogo ?? "uploads/logos/default_logo.png",
                LastUpdated = setting.LastUpdated
            };
        }

    }
    }
