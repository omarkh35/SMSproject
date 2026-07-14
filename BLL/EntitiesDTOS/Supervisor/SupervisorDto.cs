using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.EntitiesDTOS.Supervisor
{
    public class SupervisorMainDashboardDto
    {
        // Metric Cards (Top)
        public int TotalStudentsCount { get; set; }
        public int ClassesCount { get; set; }
        public int PresentTodayCount { get; set; }
        public int AbsentTodayCount { get; set; }

        // The Dropdown Combo Box Data List
        public List<SupervisorClassDropdownDto> SupervisedClasses { get; set; } = new();

        // Main List Grid: Real-time exception notification tracker feed
        public List<AbsentTodayGridItemDto> ExceptionFeed { get; set; } = new();
    }

    public class SupervisorClassDropdownDto
    {
        public int ClassRoomID { get; set; }
        public string ClassDisplayName { get; set; } // e.g., "Seventh - Section First"
    }

    public class AbsentTodayGridItemDto
    {
        public string FullName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string Status { get; set; } // "Absent" or "Late"
    }

    public class ClassRollCallDto
    {
        public string StatusMessage { get; set; } // "Attendance taken" or "Attendance was not taken yet"
        public bool IsAttendanceTaken { get; set; }
        public List<RollCallStudentItemDto> Students { get; set; } = new();
    }

    public class RollCallStudentItemDto
    {
        public int StudentID { get; set; }
        public string FullName { get; set; }
        public string CurrentStatus { get; set; } // "Present", "Absent", "Late", or "Not Set"
    }

    public class SupervisorTaskDto
    {
        public long TaskID { get; set; }
        public string TaskDescription { get; set; }
        public bool IsDone { get; set; }
        public DateTime DueDate { get; set; }
        public int? ClassRoomID { get; set; }
        public byte PriorityLevel { get; set; }
    }

    public class CreateTaskDto
    {
        public string TaskDescription { get; set; }
        public DateTime DueDate { get; set; }
        public int? ClassRoomID { get; set; } // Nullable as per your checkbox image
        public byte PriorityLevel { get; set; } // e.g., 1 = Low, 2 = Medium, 3 = High
    }

    public class AttendanceSheetLoadDto
    {
        public bool IsAlreadyRecordedToday { get; set; }
        public List<StudentAttendanceRowDto> Students { get; set; } = new();
        public List<TeacherAttendanceRowDto> Teachers { get; set; } = new();
    }

    public class StudentAttendanceRowDto
    {
        public int StudentID { get; set; }
        public string FullName { get; set; }
        public string Note { get; set; } // Empty string or pre-saved text like "SICK"
        public byte Status { get; set; }  // 1=Present, 2=Absent, 3=Late, 4=Excused
    }

    public class TeacherAttendanceRowDto
    {
        public int TeacherID { get; set; }
        public string FullName { get; set; }
        public string Note { get; set; }
        public byte Status { get; set; } // 1=Present, 2=Absent, 3=On Leave
    }

    // Payload sent back when clicking "Save the Attendance Sheet"
    public class SaveAttendanceSheetDto
    {
        public int ClassRoomID { get; set; }
        public List<StudentAttendanceRowDto> StudentRecords { get; set; } = new();
        public List<TeacherAttendanceRowDto> TeacherRecords { get; set; } = new();
    }

    public class AnnouncementManagementPageDto
    {
        // For the "Addressed to" dropdown combo box selection list
        public List<AnnouncementTargetDropdownDto> TargetOptions { get; set; } = new();
        // For the "Published Advertisements" grid feed at the bottom
        public List<PublishedAnnouncementItemDto> MyPublishedAnnouncements { get; set; } = new();
    }

    public class AnnouncementTargetDropdownDto
    {
        public int? ClassRoomID { get; set; } // Null if it represents "All classes"
        public string DisplayName { get; set; }  // e.g., "All classes" or "7th Grade / Class 1"
    }

    public class PublishedAnnouncementItemDto
    {
        public int AnnouncementID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string TargetAudienceDisplay { get; set; } // e.g., "All Classes" or "7th Grade - Section 1"
        public DateTime? CreatedAt { get; set; }
    }

    public class CreateAnnouncementRequestDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsGeneral { get; set; } 
        public List<int> TargetClassRoomIDs { get; set; } = new(); 
    }


    public class SupervisorClassCardDto
    {
        public int ClassRoomID { get; set; }
        public string ClassName { get; set; }          // e.g., "7th grade / first"
        public int NumberOfStudents { get; set; }      // Real-time counter
        public string ClassAverage { get; set; }       // e.g., "79%" or "N/A"
        public string SemesterExamScheduleUrl { get; set; } // Path to exam schedule image
        public string WeeklyWorkScheduleUrl { get; set; }   // Path to weekly schedule image
    }

    public class SupervisorStudentGridDto
    {
        public int StudentID { get; set; }
        public string FullName { get; set; }
        public string ClassName { get; set; }     // e.g., "8th"
        public string SectionName { get; set; }   // e.g., "first"
        public string GPA { get; set; }           // Calculated percentage string (e.g., "70 %")
        public string AttendanceRate { get; set; } // Calculated percentage string (e.g., "86 %")
    }

    public class StudentDetailsPageDto
    {
        // 1. Profile Header
        public int StudentID { get; set; }
        public string FullName { get; set; }
        public string ParentPhoneNumber { get; set; }
        public string ClassAndSection { get; set; }
        public string TotalGPA { get; set; }

        // 2. Marks Breakdown List
        public List<StudentDetailsMarkItemDto> MarksList { get; set; } = new();

        // 3. Attendance Calendar Feeder Matrix
        public List<CalendarAttendanceDayDto> CalendarLogs { get; set; } = new();
    }

    public class StudentDetailsMarkItemDto
    {
        public string SubjectName { get; set; }
        public decimal AchievedScore { get; set; }
        public decimal MaximumScore { get; set; }
        public string DisplayScore => $"{AchievedScore:G29} / {MaximumScore:G29}"; // Cleans trailing decimal zeroes
    }

    public class CalendarAttendanceDayDto
    {
        public int DayNumber { get; set; } // e.g., 14
        public byte StatusType { get; set; } // 1=Present (Green), 2=Absent (Red), 3=Late (Yellow), 4=Leave with parent (Black)
    }

    public class SupervisorTeacherSidebarDto
    {
        public int TeacherID { get; set; }
        public string FullName { get; set; }
        public string ClassesDisplay { get; set; }  // e.g., "SEVENTH / FIRST / THIRD"
        public string PhoneNumber { get; set; }
        public string SubjectsDisplay { get; set; } // e.g., "SCIENCE - PHYSICS"
    }

    // Right Details Pane DTO
    public class TeacherDetailsPaneDto
    {
        public int TeacherID { get; set; }
        public string FullName { get; set; }
        public string ClassesDisplay { get; set; }
        public string PhoneNumber { get; set; }
        public string SubjectsDisplay { get; set; }
        public string WeeklyWorkScheduleUrl { get; set; } // For the "Click to view" schedule card button

        // Attendance Heatmap Feeder Matrix
        public List<CalendarAttendanceDayDto> AttendanceCalendar { get; set; } = new();
    }

    public class SaveStudentAttendanceDto
    {
        public int ClassRoomID { get; set; }
        public List<StudentAttendanceItemDto> StudentRecords { get; set; } = new();
    }

    public class StudentAttendanceItemDto
    {
        public int StudentID { get; set; }
        public byte Status { get; set; } // 1=Present, 2=Absent, etc.
        public string? Note { get; set; }
    }

    public class SaveTeacherAttendanceDto
    {
        public List<TeacherAttendanceItemDto> TeacherRecords { get; set; } = new();
    }

    public class TeacherAttendanceItemDto
    {
        public int TeacherID { get; set; }
        public bool IsPresent { get; set; } // Matches your database boolean flag
        public int? MissedPeriodsCount { get; set; } // Your brand new nullable database column
    }

    public class ChatThreadDto
    {
        public int ChatRoomID { get; set; }
        public int ParentPersonID { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime? LastMessageTime { get; set; }
    }

    public class ChatMessageDto
    {
        public long MessageID { get; set; } // Matches your bigint column type
        public int SenderPersonID { get; set; }
        public string MessageContent { get; set; } = string.Empty;
        public DateTime? SentAt { get; set; }
        public DateTime? ReadAt { get; set; } // Matches your nullable datetime2 column
        public bool IsMe { get; set; }
    }

    public class SendMessageDto
    {
        public int ChatRoomID { get; set; }
        public string MessageContent { get; set; } = string.Empty;
    }



}
