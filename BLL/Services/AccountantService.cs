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
        private readonly IBaseRepositories<Parent> _parentRepo;
        private readonly IBaseRepositories<Payment> _paymentRepo;
        private readonly IBaseRepositories<StudentParent> _studentParentRepo;
        private readonly IBaseRepositories<SalaryPayment> _salaryPaymentRepo;
        private readonly IBaseRepositories<User> _userRepo;
        private readonly IBaseRepositories<Teacher> _teacherRepo;
        private readonly IBaseRepositories<Supervisor> _supervisorRepo;
        private readonly IBaseRepositories<Accountant> _accountantRepo;
        private readonly IBaseRepositories<DepartmentManager> _managerRepo;

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
            IBaseRepositories<Supervisor> supervisorRepo)
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
            // 1. التدفق البديل: التحقق من وجود رقم العائلة في جدول الأهل مسبقاً
            var allParents = await _parentRepo.GetAllWithIncludeAsync();
            var matchedParent = allParents.FirstOrDefault(p =>
                p.FamilyCardNumber != null &&
                p.FamilyCardNumber.Trim() == dto.FamilyNumber.Trim()
            );

            // إذا لم يكن رقم العائلة موجوداً، نرفض التسجيل فوراً
            if (matchedParent == null)
            {
                throw new InvalidOperationException("عذراً، رقم العائلة هذا غير مسجل في النظام. يجب إنشاء حساب لولي الأمر أولاً قبل تسجيل الأبناء.");
            }

            // 2. فتح ترانزكشن لحماية البيانات في الجداول المتعددة
            var transaction = await _studentRecordRepo.BeginTransactionAsync();
            try
            {
                // أ. إنشاء سجل الشخص (People)
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
                await _personRepo.SaveChangesAsync(); // توليد PersonID

                // ب. إنشاء سجل الطالب (Students) مرتبطاً بالـ PersonID
                var newStudent = new Student
                {
                    PersonId = newPerson.PersonId,
                    MotherName = dto.MotherName.Trim(),
                    Address = dto.HomeAddress.Trim(),
                    Picture = dto.StudentPhotoPath,
                    CreatedAt = DateTime.UtcNow
                };
                await _studentRepo.AddAsync(newStudent);
                await _studentRepo.SaveChangesAsync(); // توليد StudentID

                // ج. إنشاء السجل الأكاديمي السنوي (StudentRecords) مرتبطاً بالـ StudentID
                var newAcademicRecord = new StudentRecord
                {
                    StudentId = newStudent.StudentId,
                    GradeId = dto.GradeID,
                    StudyYear = dto.AcademicYear,
                    YearlyPayment = dto.YearlyPayment,
                };
                await _studentRecordRepo.AddAsync(newAcademicRecord);

                // د. الربط التلقائي في جدول العلاقات (StudentParents) باستخدام الـ ParentID الذي وجدناه
                var newLink = new StudentParent
                {
                    StudentId = newStudent.StudentId,
                    ParentID = matchedParent.Id, // المعرف المالي لولي الأمر
                    RelationshipType = "أب/أم"   // صلة القرابة الافتراضية بناءً على دفتر العائلة
                };
                await _studentParentRepo.AddAsync(newLink);

                // حفظ جميع التغييرات النهائية وتأكيد العملية
                await _studentRecordRepo.SaveChangesAsync();
                await _studentParentRepo.SaveChangesAsync();

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


        public async Task<ParentAccountsDashboardDto> GetParentAccountsGridAsync(string? searchQuery, int page)
        {
            var dashboard = new ParentAccountsDashboardDto();
            const int pageSize = 4; // Renders exactly 4 parent card layout slots as depicted in your UI screenshot

            // 1. Fetch deep parent entity profile graphs, traveling out to user security info blocks
            var baseQueryParents = await _parentRepo.GetAllWithIncludeAsync(
                p => p.Person,
                p => p.Person.Users
            );

            var filteredParents = baseQueryParents.AsEnumerable();

            // 2. Execute dual-criteria searching logic (Matches Full Name OR AccountNumber)
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                string cleanSearch = searchQuery.Trim().ToLower();

                filteredParents = filteredParents.Where(p =>
                    // Search by Name fields
                    p.Person.FirstName.ToLower().Contains(cleanSearch) ||
                    p.Person.SecondName.ToLower().Contains(cleanSearch) ||
                    p.Person.LastName.ToLower().Contains(cleanSearch) ||
                    // Dual Search criteria by System Account Number
                    p.Person.Users.Any(u => u.AccountNumber != null && u.AccountNumber.ToLower().Contains(cleanSearch))
                );
            }

            var matchingList = filteredParents.ToList();

            // 3. Populate framework layout pagination constraints
            dashboard.TotalParentsCount = matchingList.Count;
            dashboard.TotalPages = (int)Math.Ceiling((double)dashboard.TotalParentsCount / pageSize);

            var paginatedData = matchingList
                .OrderBy(p => p.Person.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 4. Map tracked relational data trees straight into screen display models
            foreach (var parent in paginatedData)
            {
                // Pull the registered user profile linked to this person
                var userAccount = parent.Person.Users?.FirstOrDefault();

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

            // 1. Fetch Classrooms and Grades for the filter dropdown component menu
            var allClassRooms = await _classRoomRepo.GetAllWithIncludeAsync(cr => cr.Grade);
            dashboard.AvailableClasses = allClassRooms.Select(cr => new ClassDropdownItemDto
            {
                ClassRoomID = cr.ClassRoomId,
                ClassDisplayName = $"Grade {cr.Grade.GradeNumber} - Section {cr.Section}"
            }).ToList();

            // 2. Fetch current student classroom allocations and total historical payments logged
            var classroomAllocations = await _classroomStudentRepo.GetAllWithIncludeAsync();
            var allPayments = await _paymentRepo.GetAllWithIncludeAsync();

            // 3. Load active StudentRecords linked to personal data trees
            var academicRecords = await _studentRecordRepo.GetAllWithIncludeAsync(
                sr => sr.Student,
                sr => sr.Student.Person,
                sr => sr.Student.Person.Users
            );

            // 4. Track separate global lists to calculate entire school metrics cleanly
            var fullSchoolCalculatedList = new List<InstallmentStudentGridItemDto>();

            foreach (var record in academicRecords)
            {
                // Calculate financial metrics by individual student record
                var studentPayments = allPayments.Where(p => p.StudentRecordId == record.StudentRecordId);
                decimal totalPaid = (decimal)studentPayments.Sum(p => p.PaymentAmount);
                decimal annualFee = (decimal)record.YearlyPayment;
                decimal amountDue = annualFee - totalPaid;
                if (amountDue < 0) amountDue = 0; // Guard clause against overflow anomalies

                string currentStatus = amountDue == 0 ? "PAID" : "UNPAID";

                // Resolve classroom mapping values string representation
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

            // =========================================================================
            // CORRECTED WORKFLOW: CALCULATE TOP METRICS ON ALL COMBINED STUDENTS GLOBAL
            // =========================================================================
            dashboard.TotalAmounts = fullSchoolCalculatedList.Sum(s => s.AnnualFees);
            dashboard.RemainingToPay = fullSchoolCalculatedList.Sum(s => s.AmountDue);
            dashboard.PaymentAmounts = dashboard.TotalAmounts - dashboard.RemainingToPay;

            // 5. Apply Independent Grid View Filters (Search, Class select dropdown, ALL/PAID/UNPAID Chips)
            var filteredQuery = fullSchoolCalculatedList.AsEnumerable();

            // Filter Criteria A: Search by Name Input
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                string cleanSearch = searchName.Trim().ToLower();
                filteredQuery = filteredQuery.Where(s => s.StudentName.ToLower().Contains(cleanSearch));
            }

            // Filter Criteria B: Filter by specific Class Selection list dropdown
            if (classRoomId.HasValue)
            {
                var targetStudentIds = classroomAllocations
                    .Where(ca => ca.ClassRoomId == classRoomId.Value)
                    .Select(ca => ca.StudentId)
                    .ToList();

                filteredQuery = filteredQuery.Where(s => targetStudentIds.Contains(s.StudentID));
            }

            // Filter Criteria C: ALL, PAID, UNPAID horizontal layout chips toggle state selection
            if (!string.IsNullOrWhiteSpace(filterStatus) && !filterStatus.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                filteredQuery = filteredQuery.Where(s => s.Status.Equals(filterStatus, StringComparison.OrdinalIgnoreCase));
            }

            var finalizedFilteredList = filteredQuery.ToList();

            // 6. Paginate the resulting matching layout slice elements
            dashboard.TotalPages = (int)Math.Ceiling((double)finalizedFilteredList.Count / pageSize);
            dashboard.Students = finalizedFilteredList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return dashboard;
        }



        public async Task<StudentPaymentDetailsDto?> GetStudentPaymentDetailsAsync(int studentId)
        {
            // 1. Fetch latest active academic entry tracking row for this student
            var academicRecords = await _studentRecordRepo.GetAllWithIncludeAsync(sr => sr.Student);
            var activeRecord = academicRecords.FirstOrDefault(sr => sr.StudentId == studentId);
            if (activeRecord == null) return null;

            // 2. Load all historical payment transactions recorded against this specific record ID
            var allPayments = await _paymentRepo.GetAllWithIncludeAsync();
            var studentReceipts = allPayments
                .Where(p => p.StudentRecordId == activeRecord.StudentRecordId)
                .OrderBy(p => p.PaymentDate) // Ensures older logs render at top of history feed
                .ToList();

            // 3. Compute live financial constraints balances
            decimal totalFee = (decimal)activeRecord.YearlyPayment;
            decimal totalPaid = (decimal)studentReceipts.Sum(p => p.PaymentAmount);
            decimal balanceDue = totalFee - totalPaid;
            if (balanceDue < 0) balanceDue = 0; // Guard clause parameter protection

            // 4. Populate output object structure
            var details = new StudentPaymentDetailsDto
            {
                TotalFee = totalFee,
                Balance = balanceDue,
                InstallmentSchedule = studentReceipts.Select(p => new InstallmentHistoryItemDto
                {
                    // Formats target time stamps cleanly matching your screenshot representation ("13/8/2022")
                    PaymentDateStr = p.PaymentDate.ToString("d/M/yyyy"),
                    AmountPaid = (decimal)p.PaymentAmount
                }).ToList()
            };

            return details;
        }

        public async Task<StaffSalaryDashboardDto> GetEducationalStaffSalariesAsync()
        {
            var dashboard = new StaffSalaryDashboardDto();

            // تحديد نطاق الشهر الماضي لفحص حالة الدفع التاريخية
            var today = DateTime.Today;
            var lastMonthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
            var lastMonthEnd = new DateTime(today.Year, today.Month, 1).AddDays(-1);

            // جلب سجلات الصرف والـ Users للربط
            var payments = await _salaryPaymentRepo.GetAllWithIncludeAsync();
            var users = await _userRepo.GetAllWithIncludeAsync();
            var lastMonthPayments = payments.Where(p => p.PaymentDate >= lastMonthStart && p.PaymentDate <= lastMonthEnd).ToList();

            // 1. معالجة الأساتذة (Teachers)
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

            // 2. معالجة الموجهين (Supervisors)
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

            // 3. معالجة المحاسبين (Accountants)
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

            // 4. معالجة مدراء الأقسام (Department Managers)
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

    }
}
