using BLL.EntitiesDTOS.Accountant;
using BLL.Interfaces;
using DAL.Entities;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
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
        private readonly IBaseRepositories<Parent> _parentRepo;
        private readonly IBaseRepositories<Payment> _paymentRepo;
        private readonly IBaseRepositories<StudentParent> _studentParentRepo;
        private readonly IBaseRepositories<SalaryPayment> _salaryPaymentRepo;
        private readonly IBaseRepositories<User> _userRepo;
        private readonly IBaseRepositories<Teacher> _teacherRepo;
        private readonly IBaseRepositories<Supervisor> _supervisorRepo;
        private readonly IBaseRepositories<Accountant> _accountantRepo;
        private readonly IBaseRepositories<DepartmentManager> _managerRepo;
        private readonly IBaseRepositories<Role> _roleRepo;
        private readonly IBaseRepositories<Grade> _gradeRepo;
        private readonly IFileService _fileService;

        public AccountantService(
            IBaseRepositories<StudentRecord> studentRecordRepo,
            IBaseRepositories<ClassRoom> classRoomRepo,
            IBaseRepositories<ClassroomStudent> classroomStudentRepo,
            IBaseRepositories<Person> personRepo,
            IBaseRepositories<Student> studentRepo,
            IBaseRepositories<Parent> parentRepo,
            IBaseRepositories<Payment> paymentRepo,
            IBaseRepositories<StudentParent> studentParentRepo,
            IBaseRepositories<DepartmentManager> managerRepo,
            IBaseRepositories<SalaryPayment> salaryPaymentRepo,
            IBaseRepositories<User> userRepo,
            IBaseRepositories<Teacher> teacherRepo,
            IBaseRepositories<Accountant> accountantRepo,
            IBaseRepositories<Supervisor> supervisorRepo,
            IBaseRepositories<Role> roleRepo,
            IBaseRepositories<Grade> gradeRepo,
            IFileService fileService
            )
        {
            _studentRecordRepo = studentRecordRepo;
            _classRoomRepo = classRoomRepo;
            _classroomStudentRepo = classroomStudentRepo;
            _personRepo = personRepo;
            _studentRepo = studentRepo;
            _parentRepo = parentRepo;
            _paymentRepo = paymentRepo;
            _studentParentRepo = studentParentRepo;
            _managerRepo = managerRepo;
            _accountantRepo = accountantRepo;
            _salaryPaymentRepo = salaryPaymentRepo;
            _userRepo = userRepo;
            _teacherRepo = teacherRepo;
            _supervisorRepo = supervisorRepo;
            _roleRepo = roleRepo;
            _gradeRepo = gradeRepo;
            _fileService = fileService;
        }

        public async Task<AccountantDashboardDto> GetMainDashboardGridAsync(string? searchName, int? classRoomId, int page)
        {
            var dashboard = new AccountantDashboardDto();
            const int pageSize = 8; 

            var allClassRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            dashboard.AvailableClasses = allClassRooms.Select(cr => new ClassDropdownItemDto
            {
                ClassRoomID = cr.ClassRoomId,
                ClassDisplayName = $"Grade {cr.Grade.GradeNumber} - Section {cr.Section}"
            }).ToList();

            var classroomAllocations = await _classroomStudentRepo.GetAllWithIncludeAsync();

            var baseQueryRecords = await _studentRecordRepo.GetAllWithIncludeAsync(
                sr => sr.Student,
                sr => sr.Student.Person
            );

            var filteredRecords = baseQueryRecords.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string cleanSearch = searchName.Trim().ToLower();
                filteredRecords = filteredRecords.Where(sr =>
                    sr.Student.Person.FirstName.ToLower().Contains(cleanSearch) ||
                    sr.Student.Person.SecondName.ToLower().Contains(cleanSearch) ||
                    sr.Student.Person.LastName.ToLower().Contains(cleanSearch)
                );
            }

            if (classRoomId.HasValue)
            {
                var targetStudentIds = classroomAllocations
                    .Where(ca => ca.ClassRoomId == classRoomId.Value)
                    .Select(ca => ca.StudentId)
                    .ToList();

                filteredRecords = filteredRecords.Where(sr => targetStudentIds.Contains(sr.StudentId));
            }

            var matchingRecordsList = filteredRecords.ToList();

            dashboard.TotalStudentsCount = matchingRecordsList.Count;
            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalStudentsCount / pageSize);

            var paginatedData = matchingRecordsList
                .OrderBy(sr => sr.Student.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

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
                    AnnualFee = (decimal)record.YearlyPayment 
                });
            }

            return dashboard;
        }




        public async Task<bool> RegisterNewStudentAsync(StudentRegistrationDto dto)
        {
            var allParents = await _parentRepo.GetAllWithIncludeAsync();
            var matchedParent = allParents.FirstOrDefault(p =>
                p.FamilyCardNumber != null &&
                p.FamilyCardNumber.Trim() == dto.FamilyNumber.Trim()
            );

            if (matchedParent == null)
            {
                throw new InvalidOperationException("عذراً، رقم العائلة هذا غير مسجل في النظام. يجب إنشاء حساب لولي الأمر أولاً قبل تسجيل الأبناء.");
            }


            var targetGrade = await _gradeRepo.GetByIdAsync(dto.GradeID);
            if (targetGrade == null)
            {
                throw new ArgumentException($"الصف الدراسي المحدد برقم ({dto.GradeID}) غير موجود في النظام. يرجى اختيار صف دراسي صحيح.");
            }

            string? savedPhotoPath = dto.StudentPhotoPath;
            if (dto.StudentPhotoFile != null)
            {
                savedPhotoPath = await _fileService.SaveFileAsync(dto.StudentPhotoFile, "students");
            }

            var transaction = await _studentRecordRepo.BeginTransactionAsync();
            try
            {
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
                await _personRepo.SaveChangesAsync(); 

                var newStudent = new Student
                {
                    PersonId = newPerson.PersonId,
                    MotherName = dto.MotherName.Trim(),
                    Address = dto.HomeAddress.Trim(),
                    Picture = dto.StudentPhotoPath,
                    CreatedAt = DateTime.UtcNow
                };
                await _studentRepo.AddAsync(newStudent);
                await _studentRepo.SaveChangesAsync();

                var newAcademicRecord = new StudentRecord
                {
                    StudentId = newStudent.StudentId,
                    GradeId = dto.GradeID,
                    StudyYear = dto.AcademicYear,
                    YearlyPayment = dto.YearlyPayment,
                };
                await _studentRecordRepo.AddAsync(newAcademicRecord);

                var newLink = new StudentParent
                {
                    StudentId = newStudent.StudentId,
                    ParentID = matchedParent.Id, 
                    RelationshipType = "Father"  
                };
                await _studentParentRepo.AddAsync(newLink);

                await _studentRecordRepo.SaveChangesAsync();
                await _studentParentRepo.SaveChangesAsync();

                await _studentRecordRepo.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _studentRecordRepo.RollbackTransactionAsync();
                if (dto.StudentPhotoFile != null && !string.IsNullOrEmpty(savedPhotoPath))
                {
                    _fileService.DeleteFile(savedPhotoPath);
                }
                return false;
            }
        }

        public async Task<StudentDetailsFormDto?> GetStudentDetailsForFormAsync(int studentId)
        {
            var academicHistory = await _studentRecordRepo.GetAllWithIncludeAsync(
                sr => sr.Student,
                sr => sr.Student.Person
            );

            var currentRecord = academicHistory.FirstOrDefault(sr => sr.StudentId == studentId);
            if (currentRecord == null) return null;


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

                activeRecord.Student.Person.FirstName = dto.FirstName.Trim();
                activeRecord.Student.Person.SecondName = dto.FatherName.Trim();
                activeRecord.Student.Person.LastName = dto.FamilyName.Trim();
                activeRecord.Student.Person.DateOfBirth = dto.DateOfBirth;
                activeRecord.Student.Person.Gender = dto.Gender;

                activeRecord.Student.MotherName = dto.MotherName.Trim();
                activeRecord.Student.Address = dto.HomeAddress.Trim();
                if (dto.StudentPhotoPath != null) activeRecord.Student.Picture = dto.StudentPhotoPath;

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

                foreach (var r in studentRecordsList)
                {
                    _studentRecordRepo.Delete(r); 
                }

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


        public async Task<ParentAccountsDashboardDto> GetParentAccountsGridAsync(string? searchQuery, int page)
        {
            var dashboard = new ParentAccountsDashboardDto();
            const int pageSize = 4; 

            var baseQueryParents = await _parentRepo.GetAllWithIncludeAsync(p => p.Person);

            var allUsers = await _userRepo.GetAllAsync();

            var filteredParents = baseQueryParents.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                string cleanSearch = searchQuery.Trim().ToLower();

                filteredParents = filteredParents.Where(p =>
                {
                    bool nameMatch = p.Person.FirstName.ToLower().Contains(cleanSearch) ||
                                     p.Person.SecondName.ToLower().Contains(cleanSearch) ||
                                     p.Person.LastName.ToLower().Contains(cleanSearch);

                    var matchedUser = allUsers.FirstOrDefault(u => u.PersonId == p.PersonId);
                    bool accountMatch = matchedUser != null &&
                                        matchedUser.AccountNumber != null &&
                                        matchedUser.AccountNumber.ToLower().Contains(cleanSearch);

                    return nameMatch || accountMatch;
                });
            }

            var matchingList = filteredParents.ToList();

            dashboard.TotalParentsCount = matchingList.Count;
            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalParentsCount / pageSize);

            var paginatedData = matchingList
                .OrderBy(p => p.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var parent in paginatedData)
            {
                var userAccount = allUsers.FirstOrDefault(u => u.PersonId == parent.PersonId);

                string parentCombinedName = $"{parent.Person.FirstName} {parent.Person.SecondName} {parent.Person.LastName}".Replace("  ", " ").Trim();

                dashboard.Parents.Add(new ParentGridItemDto
                {
                    ParentId = parent.Id,
                    ParentName = parentCombinedName,
                    Email = userAccount?.Email ?? "No Email Registered",
                    PhoneNumber = userAccount?.PhoneNumber ?? "No Contact Number",
                    AccountNumber = userAccount?.AccountNumber ?? "N/A"
                });
            }

            return dashboard;
        }


        public async Task<InstallmentTrackingDashboardDto> GetInstallmentTrackingGridAsync(string? filterStatus, string? searchName, int? classRoomId, int page)
        {
            var dashboard = new InstallmentTrackingDashboardDto();
            const int pageSize = 4; // Renders 4 rows per page layout

            var allClassRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            dashboard.AvailableClasses = allClassRooms.Select(cr => new ClassDropdownItemDto
            {
                ClassRoomID = cr.ClassRoomId,
                ClassDisplayName = $"Grade {cr.Grade.GradeNumber} - Section {cr.Section}"
            }).ToList();

            var classroomAllocations = await _classroomStudentRepo.GetAllWithIncludeAsync();
            var allPayments = await _paymentRepo.GetAllWithIncludeAsync();

            var academicRecords = await _studentRecordRepo.GetAllWithIncludeAsync(
                sr => sr.Student,
                sr => sr.Student.Person,
                sr => sr.Student.Person.Users
            );

            var fullSchoolCalculatedList = new List<InstallmentStudentGridItemDto>();

            foreach (var record in academicRecords)
            {
                var studentPayments = allPayments.Where(p => p.StudentRecordId == record.StudentRecordId);
                decimal totalPaid = (decimal)studentPayments.Sum(p => p.PaymentAmount);
                decimal annualFee = (decimal)record.YearlyPayment;
                decimal amountDue = annualFee - totalPaid;
                if (amountDue < 0) amountDue = 0; 

                string currentStatus = amountDue == 0 ? "PAID" : "UNPAID";

                string classDisplayStr = "Not Assigned";
                var currentRoomLink = classroomAllocations.FirstOrDefault(ca => ca.StudentId == record.StudentId);
                if (currentRoomLink != null)
                {
                    var matchedRoom = allClassRooms.FirstOrDefault(cr => cr.ClassRoomId == currentRoomLink.ClassRoomId);
                    if (matchedRoom != null) classDisplayStr = $"Grade {matchedRoom.Grade.GradeNumber}";
                }

                var userContact = record.Student.Person.Users?.FirstOrDefault();
                string fullName = $"{record.Student.Person.FirstName} {record.Student.Person.SecondName} {record.Student.Person.LastName}".Replace("  ", " ").Trim();

                fullSchoolCalculatedList.Add(new InstallmentStudentGridItemDto
                {
                    StudentID = record.StudentId,
                    StudentName = fullName,
                    Contact = userContact?.PhoneNumber ?? "No Active Contact",
                    Class = classDisplayStr,
                    AnnualFees = annualFee,
                    AmountDue = amountDue,
                    Status = currentStatus
                });
            }

           
            dashboard.TotalAmounts = fullSchoolCalculatedList.Sum(s => s.AnnualFees);
            dashboard.RemainingToPay = fullSchoolCalculatedList.Sum(s => s.AmountDue);
            dashboard.PaymentAmounts = dashboard.TotalAmounts - dashboard.RemainingToPay;

            var filteredQuery = fullSchoolCalculatedList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string cleanSearch = searchName.Trim().ToLower();
                filteredQuery = filteredQuery.Where(s => s.StudentName.ToLower().Contains(cleanSearch));
            }

            if (classRoomId.HasValue)
            {
                var targetStudentIds = classroomAllocations
                    .Where(ca => ca.ClassRoomId == classRoomId.Value)
                    .Select(ca => ca.StudentId)
                    .ToList();

                filteredQuery = filteredQuery.Where(s => targetStudentIds.Contains(s.StudentID));
            }

            if (!string.IsNullOrWhiteSpace(filterStatus) && !filterStatus.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                filteredQuery = filteredQuery.Where(s => s.Status.Equals(filterStatus, StringComparison.OrdinalIgnoreCase));
            }

            var finalizedFilteredList = filteredQuery.ToList();

            dashboard.TotalPages = (int)Math.Ceiling((double)finalizedFilteredList.Count / pageSize);
            dashboard.Students = finalizedFilteredList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return dashboard;
        }



        public async Task<StudentPaymentDetailsDto?> GetStudentPaymentDetailsAsync(int studentId)
        {
            var academicRecords = await _studentRecordRepo.GetAllWithIncludeAsync(sr => sr.Student);
            var activeRecord = academicRecords.FirstOrDefault(sr => sr.StudentId == studentId);
            if (activeRecord == null) return null;

            var allPayments = await _paymentRepo.GetAllWithIncludeAsync();
            var studentReceipts = allPayments
                .Where(p => p.StudentRecordId == activeRecord.StudentRecordId)
                .OrderBy(p => p.PaymentDate) 
                .ToList();

            decimal totalFee = (decimal)activeRecord.YearlyPayment;
            decimal totalPaid = (decimal)studentReceipts.Sum(p => p.PaymentAmount);
            decimal balanceDue = totalFee - totalPaid;
            if (balanceDue < 0) balanceDue = 0; 
            var details = new StudentPaymentDetailsDto
            {
                TotalFee = totalFee,
                Balance = balanceDue,
                InstallmentSchedule = studentReceipts.Select(p => new InstallmentHistoryItemDto
                {
                    PaymentDateStr = p.PaymentDate.ToString("d/M/yyyy"),
                    AmountPaid = (decimal)p.PaymentAmount
                }).ToList()
            };

            return details;
        }

        public async Task<StaffSalaryDashboardDto> GetEducationalStaffSalariesAsync()
        {
            var dashboard = new StaffSalaryDashboardDto();

            var today = DateTime.Today;
            var lastMonthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
            var lastMonthEnd = new DateTime(today.Year, today.Month, 1).AddDays(-1);

            var payments = await _salaryPaymentRepo.GetAllWithIncludeAsync();
            var users = await _userRepo.GetAllWithIncludeAsync();
            var lastMonthPayments = payments.Where(p => p.PaymentDate >= lastMonthStart && p.PaymentDate <= lastMonthEnd).ToList();

            var teachers = await _teacherRepo.GetAllWithIncludeAsync(t => t.Person);
            foreach (var t in teachers)
            {
                var user = users.FirstOrDefault(u => u.PersonId == t.PersonId);
                bool isPaid = user != null && lastMonthPayments.Any(p => p.EmployeeId == user.UserId);
                decimal baseSalary = (t.WeeklyClasses ?? 0) * (decimal)(t.SalaryPerClass ?? 0);

                dashboard.StaffMembers.Add(new StaffSalaryGridItemDto
                {
                    PersonID = t.PersonId,
                    FullName = $"{t.Person.FirstName} {t.Person.SecondName} {t.Person.LastName}".Replace("  ", " ").Trim(),
                    Role = "teacher",
                    WorkHours = t.WeeklyClasses?.ToString() ?? "0",
                    PayPerHour = t.SalaryPerClass.HasValue ? $"{t.SalaryPerClass.Value}$" : "0$",
                    Status = isPaid ? "paid" : "unpaid",
                    NetSalary = baseSalary
                });
            }

            var supervisors = await _supervisorRepo.GetAllWithIncludeAsync(s => s.Person);
            foreach (var s in supervisors)
            {
                var user = users.FirstOrDefault(u => u.PersonId == s.PersonId);
                bool isPaid = user != null && lastMonthPayments.Any(p => p.EmployeeId == user.UserId);

                dashboard.StaffMembers.Add(new StaffSalaryGridItemDto
                {
                    PersonID = s.PersonId,
                    FullName = $"{s.Person.FirstName} {s.Person.SecondName} {s.Person.LastName}".Replace("  ", " ").Trim(),
                    Role = "Supervisor",
                    WorkHours = "-",
                    PayPerHour = "-",
                    Status = isPaid ? "paid" : "unpaid",
                    NetSalary = (decimal)(s.Salary ?? 0)
                });
            }

            var accountants = await _accountantRepo.GetAllWithIncludeAsync(a => a.Person);
            foreach (var a in accountants)
            {
                var user = users.FirstOrDefault(u => u.PersonId == a.PersonId);
                bool isPaid = user != null && lastMonthPayments.Any(p => p.EmployeeId == user.UserId);

                dashboard.StaffMembers.Add(new StaffSalaryGridItemDto
                {
                    PersonID = a.PersonId,
                    FullName = $"{a.Person.FirstName} {a.Person.SecondName} {a.Person.LastName}".Replace("  ", " ").Trim(),
                    Role = "Accountant",
                    WorkHours = "-",
                    PayPerHour = "-",
                    Status = isPaid ? "paid" : "unpaid",
                    NetSalary = (decimal)(a.Salary ?? 0)
                });
            }

            var managers = await _managerRepo.GetAllWithIncludeAsync(m => m.Person);
            foreach (var m in managers)
            {
                var user = users.FirstOrDefault(u => u.PersonId == m.PersonId);
                bool isPaid = user != null && lastMonthPayments.Any(p => p.EmployeeId == user.UserId);

                dashboard.StaffMembers.Add(new StaffSalaryGridItemDto
                {
                    PersonID = m.PersonId,
                    FullName = $"{m.Person.FirstName} {m.Person.SecondName} {m.Person.LastName}".Replace("  ", " ").Trim(),
                    Role = "Department Head",
                    WorkHours = "-",
                    PayPerHour = "-",
                    Status = isPaid ? "paid" : "unpaid",
                    NetSalary = (decimal)(m.Salary ?? 0)
                });
            }

            return dashboard;
        }


        public async Task<ParentCreatedResponseDto> RegisterNewParentAsync(ParentRegistrationDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "بيانات ولي الأمر غير مكتملة.");
            }

            string cleanFamilyCard = dto.FamilyCardNumber?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(cleanFamilyCard))
            {
                throw new ArgumentException("رقم دفتر العائلة مطلوب وهو حقل فريد.");
            }

            var allParents = await _parentRepo.GetAllAsync();
            bool familyCardExists = allParents.Any(p =>
                !string.IsNullOrWhiteSpace(p.FamilyCardNumber) &&
                p.FamilyCardNumber.Trim().Equals(cleanFamilyCard, StringComparison.OrdinalIgnoreCase));

            if (familyCardExists)
            {
                throw new InvalidOperationException($"رقم العائلة '{cleanFamilyCard}' مسجل مسبقاً في النظام لولي أمر آخر.");
            }

            string cleanPhone = dto.PhoneNumber?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(cleanPhone))
            {
                var allUsers = await _userRepo.GetAllAsync();
                if (allUsers.Any(u => !string.IsNullOrWhiteSpace(u.PhoneNumber) && u.PhoneNumber.Trim() == cleanPhone))
                {
                    throw new InvalidOperationException($"رقم الهاتف '{cleanPhone}' مسجل مسبقاً في النظام لمستخدم آخر.");
                }

                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    string cleanEmail = dto.Email.Trim().ToLower();
                    if (allUsers.Any(u => !string.IsNullOrWhiteSpace(u.Email) && u.Email.Trim().ToLower() == cleanEmail))
                    {
                        throw new InvalidOperationException($"البريد الإلكتروني '{dto.Email}' مسجل مسبقاً في النظام لمستخدم آخر.");
                    }
                }
            }

            int parentRoleId = await GetParentRoleIdAsync();

            var transaction = await _personRepo.BeginTransactionAsync();
            try
            {
                string sqlCommand = "SELECT CAST(NEXT VALUE FOR [dbo].[Seq_UserAccountNumber] AS NVARCHAR(8))";
                string generatedAccountNumber = await _classRoomRepo.ExecuteRawSqlScalarAsync<string>(sqlCommand);

                var newPerson = new Person
                {
                    FirstName = dto.FirstName.Trim(),
                    SecondName = string.IsNullOrWhiteSpace(dto.SecondName) ? string.Empty : dto.SecondName.Trim(),
                    LastName = dto.LastName.Trim(),
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _personRepo.AddAsync(newPerson);
                await _personRepo.SaveChangesAsync();

                string? hashedPassword = null;
                if (!string.IsNullOrWhiteSpace(dto.Password))
                {
                    hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password.Trim());
                }

                var newUser = new User
                {
                    PersonId = newPerson.PersonId,
                    UserRoleId = parentRoleId,
                    PhoneNumber = cleanPhone,
                    Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLower(),
                    HashPassword = hashedPassword,
                    AccountNumber = generatedAccountNumber
                };
                await _userRepo.AddAsync(newUser);
                await _userRepo.SaveChangesAsync();

                var newParent = new Parent
                {
                    PersonId = newPerson.PersonId,
                    FamilyCardNumber = cleanFamilyCard,
                    WalletBalance = 0m 
                };
                await _parentRepo.AddAsync(newParent);
                await _parentRepo.SaveChangesAsync();

                await _personRepo.CommitTransactionAsync();

                string combinedFullName = $"{newPerson.FirstName} {newPerson.SecondName} {newPerson.LastName}".Replace("  ", " ").Trim();

                return new ParentCreatedResponseDto
                {
                    ParentId = newParent.Id,
                    PersonId = newPerson.PersonId,
                    FullName = combinedFullName,
                    PhoneNumber = newUser.PhoneNumber,
                    Email = newUser.Email,
                    AccountNumber = generatedAccountNumber,
                    FamilyCardNumber = newParent.FamilyCardNumber,
                    WalletBalance = 0m,
                    CreatedAt = newPerson.CreatedAt,
                    Message = "تم تسجيل ولي الأمر بنجاح في النظام وتوليد رقم الحساب."
                };
            }
            catch
            {
                await _personRepo.RollbackTransactionAsync();
                throw;
            }
        }

     
        public async Task<ParentWalletTopUpResponseDto> TopUpParentWalletAsync(ParentWalletTopUpDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "بيانات شحن المحفظة غير مكتملة.");
            }

            if (dto.Amount <= 0)
            {
                throw new ArgumentException("يجب أن يكون المبلغ المضاف إلى المحفظة أكبر من الصفر.");
            }

            var parents = await _parentRepo.GetAllWithIncludeAsync(p => p.Person);
            var parent = parents.FirstOrDefault(p => p.Id == dto.ParentId || p.PersonId == dto.ParentId);

            if (parent == null)
            {
                throw new KeyNotFoundException($"لم يتم العثور على سجل ولي الأمر بالمعرّف الممرر: {dto.ParentId}.");
            }

            var transaction = await _parentRepo.BeginTransactionAsync();
            try
            {
                decimal previousBalance = parent.WalletBalance;
                parent.WalletBalance += dto.Amount;

                _parentRepo.UpdateAsync(parent);
                await _parentRepo.SaveChangesAsync();

                await _parentRepo.CommitTransactionAsync();

                var allUsers = await _userRepo.GetAllAsync();
                var user = allUsers.FirstOrDefault(u => u.PersonId == parent.PersonId);

                string combinedFullName = parent.Person != null
                    ? $"{parent.Person.FirstName} {parent.Person.SecondName} {parent.Person.LastName}".Replace("  ", " ").Trim()
                    : "ولي أمر";

                return new ParentWalletTopUpResponseDto
                {
                    ParentId = parent.Id,
                    PersonId = parent.PersonId,
                    ParentFullName = combinedFullName,
                    AccountNumber = user?.AccountNumber ?? "N/A",
                    FamilyCardNumber = parent.FamilyCardNumber ?? "N/A",
                    PreviousBalance = previousBalance,
                    AddedAmount = dto.Amount,
                    CurrentBalance = parent.WalletBalance,
                    TransactionDate = DateTime.UtcNow,
                    Message = $"تمت إضافة مبلغ {dto.Amount:N2} إلى محفظة ولي الأمر بنجاح. الرصيد الحالي: {parent.WalletBalance:N2}."
                };
            }
            catch
            {
                await _parentRepo.RollbackTransactionAsync();
                throw;
            }
        }

    
        private async Task<int> GetParentRoleIdAsync()
        {
            try
            {
                var roles = await _roleRepo.GetAllAsync();
                var parentRole = roles.FirstOrDefault(r =>
                    !string.IsNullOrWhiteSpace(r.RoleName) &&
                    (r.RoleName.Equals("Parent", StringComparison.OrdinalIgnoreCase) ||
                     r.RoleName.Equals("ولي أمر", StringComparison.OrdinalIgnoreCase) ||
                     r.RoleName.Equals("أب", StringComparison.OrdinalIgnoreCase) ||
                     r.RoleName.ToLower().Contains("parent")));

                if (parentRole != null)
                {
                    return parentRole.RoleId;
                }

                return roles.FirstOrDefault()?.RoleId ?? 5;
            }
            catch
            {
                return 5;
            }
        }


    }
}
