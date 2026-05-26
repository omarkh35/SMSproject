using BLL.EntitiesDTOS.DepartmentManager;
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
    public class DepartmentManagerService : IDepartmentManagerService
    {
        private readonly IBaseRepositories<ClassroomStudent> _classStudentRepo;
        private readonly IBaseRepositories<ClassroomTeacher> _classTeacherRepo;
        private readonly IBaseRepositories<TeacherSupervisor> _teacherSupervisorRepo;
        private readonly IBaseRepositories<ClassRoom> _classRoomRepo;
        private readonly IBaseRepositories<Teacher> _teacherRepo;

        public DepartmentManagerService(
            IBaseRepositories<ClassRoom> classRoomRepo,
            IBaseRepositories<ClassroomStudent> classStudentRepo,
            IBaseRepositories<ClassroomTeacher> classTeacherRepo,
            IBaseRepositories<TeacherSupervisor> teacherSupervisorRepo,
            IBaseRepositories<Teacher> teacherRepo
        )
        {
            _classRoomRepo = classRoomRepo;
            _classStudentRepo = classStudentRepo;
            _classTeacherRepo = classTeacherRepo;
            _teacherSupervisorRepo = teacherSupervisorRepo;
            _teacherRepo = teacherRepo;
        }

        public async Task<IEnumerable<ClassRoomDto>> GetAllClassRoomsAsync()
        {
            var classes = await _classRoomRepo.GetAllWithIncludeAsync(c => c.ClassroomStudents);

            return classes.Select(c => new ClassRoomDto
            {
                Id = c.ClassRoomId,
                GradeId = c.GradeId,
                Section = c.Section,
                SupervisorId = c.SupervisorId,
                StartYear = c.StartYear,
                CurrentStudentsCount = c.ClassroomStudents.Count
            });
        }

        public async Task<ClassRoomDto> GetClassRoomByIdAsync(int id)
        {
            var c = await _classRoomRepo.GetByIdAsync(id);
            if (c == null) return null;

            return new ClassRoomDto
            {
                Id = c.ClassRoomId,
                GradeId = c.GradeId,
                Section = c.Section,
                SupervisorId = c.SupervisorId,
                StartYear = c.StartYear
            };
        }

        public async Task<ClassRoomDto> CreateClassRoomAsync(ClassRoomCreateDto dto)
        {
            var newClass = new ClassRoom
            {
                GradeId = dto.GradeId,
                Section = dto.Section,
                StartYear = dto.StartYear,
                SupervisorId = dto.SupervisorId
            };
            await _classRoomRepo.AddAsync(newClass);
            await _classRoomRepo.SaveChangesAsync();

            return new ClassRoomDto
            {
                Id = newClass.ClassRoomId,
                GradeId = newClass.GradeId,
                Section = newClass.Section,
                StartYear = newClass.StartYear
            };
        }

        public async Task<bool> UpdateClassRoomAsync(int id, ClassRoomUpdateDto dto)
        {
            var existing = await _classRoomRepo.GetByIdAsync(id);
            if (existing == null) return false;

            existing.StartYear = dto.StartYear ?? existing.StartYear;

            _classRoomRepo.UpdateAsync(existing);
            await _classRoomRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteClassRoomAsync(int id)
        {
            // هون لازم نتحقق انو مدير القسم هو المسؤول عن الصف 
            var existing = await _classRoomRepo.GetByIdAsync(id);
            if (existing == null) return false;

            _classRoomRepo.Delete(existing);
            await _classRoomRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignStudentToClassAsync(StudentToClassDto dto)
        {
            var exists = await _classStudentRepo.GetAllWithIncludeAndFilterAsync(
                cs => cs.StudentId == dto.StudentId && cs.ClassRoomId == dto.ClassRoomId
            );
            if (exists.Any()) return false;

            var link = new ClassroomStudent
            {
                StudentId = dto.StudentId,
                ClassRoomId = dto.ClassRoomId
            };

            await _classStudentRepo.AddAsync(link);
            await _classStudentRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignTeacherToClassAsync(TeacherToClassDto dto)
        {
            var link = new ClassroomTeacher
            {
                TeacherId = dto.TeacherId,
                ClassRoomId = dto.ClassRoomId,
                SubjectId = dto.SubjectId
            };

            await _classTeacherRepo.AddAsync(link);
            await _classTeacherRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignSupervisorToTeacherAsync(TeacherSupervisorDto dto)
        {
            var link = new TeacherSupervisor
            {
                SupervisorId = dto.SupervisorId,
                TeacherId = dto.TeacherId
            };

            await _teacherSupervisorRepo.AddAsync(link);
            await _teacherSupervisorRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveStudentFromClassAsync(int studentId, int classRoomId)
        {
            var links = await _classStudentRepo.GetAllWithIncludeAndFilterAsync(
                cs => cs.StudentId == studentId && cs.ClassRoomId == classRoomId
            );
            var link = links.FirstOrDefault();
            if (link == null) return false;

            _classStudentRepo.Delete(link);
            await _classStudentRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTeacherAssignmentAsync(TeacherToClassDto dto)
        {
           
            var assignment = await _classTeacherRepo.GetByIdAsync(dto.ClassroomTeacherId);

            if (assignment == null) return false;

            assignment.ClassRoomId = dto.ClassRoomId; 
            assignment.SubjectId = dto.SubjectId;     
            assignment.TeacherId = dto.TeacherId;    

            _classTeacherRepo.UpdateAsync(assignment);
            await _classTeacherRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveTeacherFromClassAsync(int teacherId, int classRoomId)
        {
            var assignments = await _classTeacherRepo.GetAllWithIncludeAndFilterAsync(
                ct => ct.TeacherId == teacherId && ct.ClassRoomId == classRoomId
            );

            var assignment = assignments.FirstOrDefault();
            if (assignment == null) return false;

            _classTeacherRepo.Delete(assignment);
            await _classTeacherRepo.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveSupervisorFromTeacherAsync(int supervisorId, int teacherId)
        {
            var relations = await _teacherSupervisorRepo.GetAllWithIncludeAndFilterAsync(
                ts => ts.SupervisorId == supervisorId && ts.TeacherId == teacherId
            );

            var relation = relations.FirstOrDefault();
            if (relation == null) return false;

            _teacherSupervisorRepo.Delete(relation);
            await _teacherSupervisorRepo.SaveChangesAsync();
            return true;
        }
    }
}
