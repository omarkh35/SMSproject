using BLL.EntitiesDTOS.Parent;
using BLL.Interfaces;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace SMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize (Roles = "Parents")]
    public class ParentController : ControllerBase
    {
        private readonly IParentService _parentService;

        public ParentController(IParentService parentService)
        {
            _parentService = parentService;
        }

        [HttpGet("MyChildren")]
        public async Task<IActionResult> GetMyChildren()
        {
            try
            {
                var claimUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
                {
                    return Unauthorized(new { message = "THere is a problem with the Identity" });
                }

                var children = await _parentService.GetMyChildrenAsync(parentPersonId);

                return Ok(children);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب قائمة أبناء ولي الأمر.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("ParentHomePage")]
        public async Task<IActionResult> GetParentHomePage()
        {
            try
            {
                var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
                {
                    return Unauthorized(new { message = "User identity error." });
                }

                var dashboardData = await _parentService.GetParentDashboardAsync(parentPersonId);
                return Ok(dashboardData);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب الصفحة الرئيسية لولي الأمر.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("children/{studentId}/bag")]
        public async Task<IActionResult> GetStudentBagDetails(int studentId)
        {
            try
            {
                var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
                {
                    return Unauthorized(new { message = "User identity error." });
                }

                var details = await _parentService.GetStudentBagDetailsAsync(parentPersonId, studentId);

                if (details == null)
                {
                    return Forbid();
                }

                return Ok(details);
            }
             
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب تفاصيل حقيبة الطالب.",
                    details = ex.Message
    });
            }
        }

        [HttpGet("student/{studentId}/weeklyschedule")]
        public async Task<IActionResult> GetStudentWeeklySchedule(int studentId)
        {
            try
            {
                var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
                {
                    return Unauthorized(new { message = "User identity claim parsing fail" });
                }

                string hostUrl = $"{Request.Scheme}://{Request.Host}";

                var schedule = await _parentService.GetStudentWeeklyScheduleAsync(parentPersonId, studentId, hostUrl);

                if (schedule == null)
                {
                    return Forbid();
                }

                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب جدول الدوام الأسبوعي للطالب.",
                    details = ex.Message
                });
            }
        }


        [HttpGet("student/{studentId}/exam-schedule")]
        public async Task<IActionResult> GetStudentExamSchedule(int studentId)
        {
            try
            {
                var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
                {
                    return Unauthorized(new { message = "User identity context is missing." });
                }

                string hostUrl = $"{Request.Scheme}://{Request.Host}";

                var examSchedule = await _parentService.GetStudentExamScheduleAsync(parentPersonId, studentId, hostUrl);

                if (examSchedule == null)
                {
                    return Forbid();
                }

                return Ok(examSchedule);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب جدول امتحانات الطالب.",
                    details = ex.Message
                });
            }
        }


        [HttpGet("student/{studentId}/academic-summary")]
        public async Task<IActionResult> GetStudentAcademicSummary(int studentId)
        {
            try
            {
                var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
                {
                    return Unauthorized(new { message = "User identity claim context parsing failed." });
                }


                var summary = await _parentService.GetStudentAcademicSummaryAsync(parentPersonId, studentId);

                if (summary == null)
                {
                    return Forbid();
                }

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب الملخص الأكاديمي للطالب.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("student/{studentId}/profile")]
        public async Task<IActionResult> GetStudentProfile(int studentId)
        {
            try
            {
                var claimUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
                {
                    return Unauthorized(new { message = "User identity context is missing." });
                }

                string hostUrl = $"{Request.Scheme}://{Request.Host}";


                var profile = await _parentService.GetStudentProfileAsync(parentPersonId, studentId, hostUrl);

                if (profile == null)
                {
                    return Forbid();
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب الملف التعريفي للطالب.",
                    details = ex.Message
                });
            }
        }


        [HttpGet("student/{studentId}/attendance")]
        public async Task<IActionResult> GetStudentAttendance(int studentId, [FromQuery] int? year, [FromQuery] int? month)
        {
            try
            {
                var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
                {
                    return Unauthorized(new { message = "User identity context is missing or corrupted." });
                }

                int filterYear = year ?? DateTime.UtcNow.Year;
                int filterMonth = month ?? DateTime.UtcNow.Month;

                var attendanceData = await _parentService.GetStudentAttendanceCalendarAsync(parentPersonId, studentId, filterYear, filterMonth);

                if (attendanceData == null)
                {
                    return Forbid();
                }

                return Ok(attendanceData);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب سجل حضور الطالب.",
                    details = ex.Message
                });
            }
        }



        [HttpGet("student/{studentId}/subject/{subjectId}/detailes")]
        public async Task<IActionResult> GetSubjectDetailedReport(int studentId, int subjectId)
        {
            try
            {
                var claimUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
                {
                    return Unauthorized(new { message = "User identity context is missing." });
                }

                var reportCard = await _parentService.GetSubjectDetailedReportAsync(parentPersonId, studentId, subjectId);

                if (reportCard == null)
                {
                    return Forbid();
                }

                return Ok(reportCard);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب التقرير التفصيلي للمادة.",
                    details = ex.Message
                });
            }
        }

        [HttpPost("pay")]
        public async Task<IActionResult> MakePayment([FromBody] MakeStudentPaymentRequestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var claimUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
                {
                    return Unauthorized(new { message = "User identity context is missing or invalid." });
                }

                var result = await _parentService.MakeStudentPaymentAsync(parentPersonId, dto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء معالجة عملية الدفع.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("wallet")]
        public async Task<IActionResult> GetWallet()
        {
            try
            {
                var claimUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
                {
                    return Unauthorized(new { message = "User identity context is missing or invalid." });
                }

                var wallet = await _parentService.GetParentWalletAsync(parentPersonId);
                if (wallet == null)
                {
                    return NotFound(new { message = "Parent account not found." });
                }

                return Ok(wallet);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب محفظة ولي الأمر.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("student/{studentId}/payment-status")]
        public async Task<IActionResult> GetStudentPaymentStatus(int studentId)
        {
            try
            {
                var claimUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
                {
                    return Unauthorized(new { message = "User identity context is missing or invalid." });
                }

                var summary = await _parentService.GetStudentPaymentSummaryAsync(parentPersonId, studentId);
                if (summary == null)
                {
                    return Forbid();
                }

                return Ok(summary);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب حالة دفعات الطالب.",
                    details = ex.Message
                });
            }
        }


        [HttpGet("chats")]
        public async Task<IActionResult> GetChatThreads()
        {
            try
            {
                var claimUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
                {
                    return Unauthorized(new { message = "User identity context is missing or invalid." });
                }

                var threads = await _parentService.GetParentChatThreadsAsync(parentPersonId);
                return Ok(threads);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب محادثات ولي الأمر.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("chat-history/{chatRoomId}")]
        public async Task<IActionResult> GetChatHistory(int chatRoomId)
        {
            try
            {
                var claimUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
                {
                    return Unauthorized(new { message = "User identity context is missing or invalid." });
                }

                var messages = await _parentService.GetChatHistoryAsync(parentPersonId, chatRoomId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء جلب سجل المحادثة.",
                    details = ex.Message
                });
            }
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] ParentSendMessageDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var claimUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(claimUserId) || !int.TryParse(claimUserId, out int parentPersonId))
                {
                    return Unauthorized(new { message = "User identity context is missing or invalid." });
                }

                var success = await _parentService.SendMessageAsync(parentPersonId, dto);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to send message or access denied to chat room." });
                }

                return Ok(new { message = "Message sent successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "حدث خطأ أثناء إرسال الرسالة.",
                    details = ex.Message
                });
            }
        }


    }


}

