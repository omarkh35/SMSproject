using BLL.EntitiesDTOS.SchoolAdmin;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class SchoolAdminService : ISchoolAdminService
    {
        private readonly IBaseRepositories<Subject> _subjectRepo;
        private readonly IBaseRepositories<DepartmentManager> _managerRepo;
        private readonly IBaseRepositories<Person> _personRepo;
        private readonly IBaseRepositories<Supervisor> _supervisorRepo;
        private readonly IBaseRepositories<GradeSubject> _gradeSubjectRepo;
        public SchoolAdminService(
            IBaseRepositories<Subject> subjectRepo,
            IBaseRepositories<DepartmentManager> managerRepo,
            IBaseRepositories<Person> personRepo,
            IBaseRepositories<Supervisor> supervisorRepo,
            IBaseRepositories<GradeSubject> gradeSubjectRepo
            )
        {
            _subjectRepo = subjectRepo;
            _managerRepo = managerRepo;
            _personRepo = personRepo;
            _supervisorRepo = supervisorRepo;
            _gradeSubjectRepo = gradeSubjectRepo;
        }
        public async Task<SubjectDto> CreateSubjectAsync(SubjectCreateDto dto)
        {
            var subject = new Subject
            {
                SubjectName = dto.SubjectName.ToUpper()
            };
            await _subjectRepo.AddAsync(subject);
            await _subjectRepo.SaveChangesAsync();
            return new SubjectDto
            {
                Id = subject.SubjectId,
                SubjectName = subject.SubjectName
            };
        }

        public async Task<IEnumerable<SubjectDto>> GetAllSubjectsAsync()
        {
            var subjects = await _subjectRepo.GetAllAsync();
            return subjects.Select(s => new SubjectDto
            {
                Id = s.SubjectId,
                SubjectName = s.SubjectName,
            });
        }

        public async Task<bool> UpdateSubjectAsync(int id, SubjectUpdateDto dto)
        {
            var subject = await _subjectRepo.GetByIdAsync(id);
            if (subject == null) return false;
            subject.SubjectName = dto.SubjectName;
            _subjectRepo.UpdateAsync(subject);
            await _subjectRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSubjectAsync(int id)
        {
            var subject = await _subjectRepo.GetByIdAsync(id);
            if (subject == null) return false;
            _subjectRepo.Delete(subject);
            await _subjectRepo.SaveChangesAsync();
            return true;
        }

        public async Task<GradeSubjectDto> AssignSubjectToGradeAsync(GradeSubjectDto dto)
        {
            var gradeSubject = new GradeSubject
            {
                GradeId = dto.GradeId,
                SubjectId = dto.SubjectId
            };

            await _gradeSubjectRepo.AddAsync(gradeSubject);
            await _gradeSubjectRepo.SaveChangesAsync();

            return dto;
        }

        public async Task<bool> RemoveSubjectFromGradeAsync(int gradeId, int subjectId)
        {
            var links = await _gradeSubjectRepo.GetAllWithIncludeAndFilterAsync(
                gs => gs.GradeId == gradeId && gs.SubjectId == subjectId
            );

            var link = links.FirstOrDefault();
            if (link == null) return false;

            _gradeSubjectRepo.Delete(link);
            await _gradeSubjectRepo.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<StaffDto>> GetAllDepartmentManagersAsync()
        {
            var managers = await _managerRepo.GetAllWithIncludeAndFilterAsync(
                m => true,
                m => m.Person
            );

            return managers.Select(m => new StaffDto
            {
                Id = m.DepartmentManagerId,
                PersonId = m.PersonId,
                FullName = $"{m.Person.FirstName} {m.Person.LastName}",
                Salary = m.Salary,
                Role = "DEPARTMENT MANAGER"
            }).ToList();
        }
        public async Task<StaffDto> AddDepartmentManagerAsync(DepartmentManagerCreateDto dto)
        {
            var manager = new DepartmentManager
            {
                PersonId = dto.PersonId,
                Salary = dto.Salary
            };
            await _managerRepo.AddAsync(manager);
            await _managerRepo.SaveChangesAsync();
            var person = await _personRepo.GetByIdAsync(dto.PersonId);
            return new StaffDto
            {
                Id = manager.DepartmentManagerId,
                PersonId = manager.PersonId,
                FullName = $"{person?.FirstName} {person?.LastName}",
                Salary = (decimal)manager.Salary,
                Role = "DEPARTMENT MANAGER"
            };
        }

        public async Task<bool> UpdateDepartmentManagerAsync(int id, StaffUpdateDto dto)
        {
            var manager = await _managerRepo.GetByIdAsync(id);
            if (manager == null) return false;

            manager.Salary = dto.Salary;

            _managerRepo.UpdateAsync(manager);
            await _managerRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteDepartmentManagerAsync(int id)
        {
            var manager = await _managerRepo.GetByIdAsync(id);
            if (manager == null) return false;

            _managerRepo.Delete(manager);
            await _managerRepo.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<StaffDto>> GetAllSupervisorsAsync()
        {
            var supervisors = await _supervisorRepo.GetAllWithIncludeAndFilterAsync(
                s => true,
                s => s.Person,
                s => s.DepartmentManager.Person
            );

            return supervisors.Select(s => new StaffDto
            {
                Id = s.SupervisorId,
                PersonId = s.PersonId,
                FullName = $"{s.Person?.FirstName} {s.Person?.LastName}",
                Salary = s.Salary,
                Role = "SUPERVISOR",
                DepartmentManagerName = s.DepartmentManager != null
                    ? $"{s.DepartmentManager.Person?.FirstName} {s.DepartmentManager.Person?.LastName}"
                    : "NOT ASSIGNED"
            }).ToList();
        }

        public async Task<StaffDto> AddSupervisorAsync(SupervisorCreateDto dto)
        {
            var supervisor = new Supervisor
            {
                PersonId = dto.PersonId,
                Salary = (decimal)dto.Salary,
                DepartmentManagerId = dto.DepartmentManagerId
            };
            await _supervisorRepo.AddAsync(supervisor);
            await _supervisorRepo.SaveChangesAsync();
            var person = await _personRepo.GetByIdAsync(dto.PersonId);
            return new StaffDto
            {
                Id = supervisor.SupervisorId,
                PersonId = supervisor.PersonId,
                FullName = $"{person?.FirstName} {person?.LastName}",
                Salary = (decimal)supervisor.Salary,
                Role = "SUPERVISOR"
            };
        }
        public async Task<bool> UpdateSupervisorAsync(int id, StaffUpdateDto dto)
        {
            var supervisor = await _supervisorRepo.GetByIdAsync(id);
            if (supervisor == null) return false;

            supervisor.Salary = dto.Salary;

            _supervisorRepo.UpdateAsync(supervisor);
            await _supervisorRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSupervisorAsync(int id)
        {
            var supervisor = await _supervisorRepo.GetByIdAsync(id);
            if (supervisor == null) return false;

            _supervisorRepo.Delete(supervisor);
            await _supervisorRepo.SaveChangesAsync();
            return true;
        }
    }
}
