using BLL.EntitiesDTOS.Accountant;
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
    public class AccountantService : IAccountantService
    {
        private readonly IBaseRepositories<StudentRecord> _studentRecordRepo;
        private readonly IBaseRepositories<ClassRoom> _classRoomRepo;
        private readonly IBaseRepositories<ClassroomStudent> _classroomStudentRepo;
        private readonly IBaseRepositories<Person> _personRepo;
        private readonly IBaseRepositories<Student> _studentRepo;

        public AccountantService(
            IBaseRepositories<StudentRecord> studentRecordRepo,
            IBaseRepositories<ClassRoom> classRoomRepo,
            IBaseRepositories<ClassroomStudent> classroomStudentRepo,
            IBaseRepositories<Person> personRepo,
            IBaseRepositories<Student> studentRepo)
        {
            _studentRecordRepo = studentRecordRepo;
            _classRoomRepo = classRoomRepo;
            _classroomStudentRepo = classroomStudentRepo;
            _personRepo = personRepo;
            _studentRepo = studentRepo;
        }

        public async Task<AccountantDashboardDto> GetMainDashboardGridAsync(string? searchName, int? classRoomId, int page)
        {
            var dashboard = new AccountantDashboardDto();
            const int pageSize = 8; // Renders 8 structured row items per view block

            // 1. Fetch ClassRooms with related Grades to populate the UI dropdown filter component
            var allClassRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            dashboard.AvailableClasses = allClassRooms.Select(cr => new ClassDropdownItemDto
            {
                ClassRoomID = cr.ClassRoomId,
                ClassDisplayName = $"Grade {cr.Grade.GradeNumber} - Section {cr.Section}"
            }).ToList();

            // 2. Load all current class assignment links to avoid inline loop queries
            var classroomAllocations = await _classroomStudentRepo.GetAllWithIncludeAsync();

            // 3. Query academic StudentRecords, joining back to Student and personal info profiles
            var baseQueryRecords = await _studentRecordRepo.GetAllWithIncludeAsync(
                sr => sr.Student,
                sr => sr.Student.Person
            );

            var filteredRecords = baseQueryRecords.AsEnumerable();

            // 4. Evaluate and execute dynamic full text lookup criteria safely
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string cleanSearch = searchName.Trim().ToLower();
                filteredRecords = filteredRecords.Where(sr =>
                    sr.Student.Person.FirstName.ToLower().Contains(cleanSearch) ||
                    sr.Student.Person.SecondName.ToLower().Contains(cleanSearch) ||
                    sr.Student.Person.LastName.ToLower().Contains(cleanSearch)
                );
            }

            // 5. Filter records conditionally if a specific classroom dropdown element is selected
            if (classRoomId.HasValue)
            {
                var targetStudentIds = classroomAllocations
                    .Where(ca => ca.ClassRoomId == classRoomId.Value)
                    .Select(ca => ca.StudentId)
                    .ToList();

                filteredRecords = filteredRecords.Where(sr => targetStudentIds.Contains(sr.StudentId));
            }

            var matchingRecordsList = filteredRecords.ToList();

            // 6. Compute architectural pagination frames
            dashboard.TotalStudentsCount = matchingRecordsList.Count;
            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalStudentsCount / pageSize);

            var paginatedData = matchingRecordsList
                .OrderBy(sr => sr.Student.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 7. Map database properties into the flat UI rendering layer model objects
            foreach (var record in paginatedData)
            {
                string classDisplayStr = "Not Assigned";
                var currentRoomLink = classroomAllocations.FirstOrDefault(ca => ca.StudentId == record.StudentId);

                if (currentRoomLink != null)
                {
                    var matchedRoom = allClassRooms.FirstOrDefault(cr => cr.ClassRoomId == currentRoomLink.ClassRoomId);
                    if (matchedRoom != null)
                    {
                        classDisplayStr = $"Grade {matchedRoom.Grade.GradeNumber} - Section {matchedRoom.Section}";
                    }
                }

                string combinedFullName = $"{record.Student.Person.FirstName} {record.Student.Person.SecondName} {record.Student.Person.LastName}".Replace("  ", " ").Trim();

                dashboard.Students.Add(new StudentGridItemDto
                {
                    StudentID = record.StudentId,
                    FullName = combinedFullName,
                    MotherName = record.Student.MotherName,
                    ClassAndSection = classDisplayStr,
                    AnnualFee = (decimal)record.YearlyPayment // Handles money SQL translation safely
                });
            }

            return dashboard;
        }




        public async Task<bool> RegisterNewStudentAsync(StudentRegistrationDto dto)
        {
            // Use transaction to safeguard sequential insertions across the three hierarchical tables
            var transaction = await _studentRecordRepo.BeginTransactionAsync();
            try
            {
                // 1. Create and Insert base Person record
                var newPerson = new Person
                {
                    FirstName = dto.FirstName.Trim(),
                    SecondName = dto.FatherName.Trim(),
                    LastName = dto.FamilyName.Trim(),
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _personRepo.AddAsync(newPerson);
                await _personRepo.SaveChangesAsync(); // Generates newPerson.PersonID

                // 2. Create and Insert Student Profile linked directly to the PersonID
                var newStudent = new Student
                {
                    PersonId = newPerson.PersonId, // CORRECT ROOT: Student points to Person
                    MotherName = dto.MotherName.Trim(),
                    Address = dto.HomeAddress.Trim(),
                    Picture = dto.StudentPhotoPath,
                    CreatedAt = DateTime.UtcNow
                };
                await _studentRepo.AddAsync(newStudent);
                await _studentRepo.SaveChangesAsync(); // Generates newStudent.StudentID

                // 3. Create and Insert Academic Study Record Block linked directly to the StudentID
                var newAcademicRecord = new StudentRecord
                {
                    StudentId = newStudent.StudentId, // CORRECT ROOT: StudentRecord points to Student
                    GradeId = dto.GradeID,
                    StudyYear = dto.AcademicYear,
                    YearlyPayment = dto.YearlyPayment
                };
                await _studentRecordRepo.AddAsync(newAcademicRecord);
                await _studentRecordRepo.SaveChangesAsync();

                await _studentRecordRepo.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _studentRecordRepo.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<StudentDetailsFormDto?> GetStudentDetailsForFormAsync(int studentId)
        {
            // Fetch latest active academic entry track record for student mapping values
            var academicHistory = await _studentRecordRepo.GetAllWithIncludeAsync(
                sr => sr.Student,
                sr => sr.Student.Person
            );

            var currentRecord = academicHistory.FirstOrDefault(sr => sr.StudentId == studentId);
            if (currentRecord == null) return null;

            // Optional: pull structural tracking from StudentParents table if family cards are tracked

            return new StudentDetailsFormDto
            {
                StudentID = currentRecord.StudentId,
                FirstName = currentRecord.Student.Person.FirstName,
                FatherName = currentRecord.Student.Person.SecondName,
                FamilyName = currentRecord.Student.Person.LastName,
                MotherName = currentRecord.Student.MotherName,
                DateOfBirth = currentRecord.Student.Person.DateOfBirth,
                Gender = currentRecord.Student.Person.Gender,
                StudentPhotoPath = currentRecord.Student.Picture,
                HomeAddress = currentRecord.Student.Address,
                GradeID = currentRecord.GradeId,
                AcademicYear = currentRecord.StudyYear
            };
        }

        public async Task<bool> UpdateStudentRegistrationAsync(int studentId, StudentRegistrationDto dto)
        {
            var transaction = await _studentRecordRepo.BeginTransactionAsync();
            try
            {
                var records = await _studentRecordRepo.GetAllWithIncludeAsync(sr => sr.Student, sr => sr.Student.Person);
                var activeRecord = records.FirstOrDefault(sr => sr.StudentId == studentId);
                if (activeRecord == null) return false;

                // Mutate Person profiles tracking values
                activeRecord.Student.Person.FirstName = dto.FirstName.Trim();
                activeRecord.Student.Person.SecondName = dto.FatherName.Trim();
                activeRecord.Student.Person.LastName = dto.FamilyName.Trim();
                activeRecord.Student.Person.DateOfBirth = dto.DateOfBirth;
                activeRecord.Student.Person.Gender = dto.Gender;

                // Mutate Student explicit records values
                activeRecord.Student.MotherName = dto.MotherName.Trim();
                activeRecord.Student.Address = dto.HomeAddress.Trim();
                if (dto.StudentPhotoPath != null) activeRecord.Student.Picture = dto.StudentPhotoPath;

                // Mutate current dynamic academic year fields safely
                activeRecord.GradeId = dto.GradeID;
                activeRecord.StudyYear = dto.AcademicYear;

                _studentRecordRepo.UpdateAsync(activeRecord);
                await _studentRecordRepo.SaveChangesAsync();
                await _studentRecordRepo.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _studentRecordRepo.RollbackTransactionAsync();
                return false;
            }
        }

        public async Task<bool> DeleteStudentRecordWorkflowAsync(int studentId)
        {
            var transaction = await _studentRecordRepo.BeginTransactionAsync();
            try
            {
                var records = await _studentRecordRepo.GetAllWithIncludeAsync(sr => sr.Student);
                var studentRecordsList = records.Where(sr => sr.StudentId == studentId).ToList();

                // 1. Remove academic record entries matching target student
                foreach (var r in studentRecordsList)
                {
                    _studentRecordRepo.Delete(r); // Adjust standard delete based on repo structure
                }

                // 2. Cascade down to clear out base entities cleanly or flags dependent on system preference
                await _studentRecordRepo.SaveChangesAsync();
                await _studentRecordRepo.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _studentRecordRepo.RollbackTransactionAsync();
                return false;
            }
        }


    }
}
