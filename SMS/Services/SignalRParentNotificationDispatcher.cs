using BLL.Notifications.Interfaces;
using BLL.Notifications.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SMS.Hubs;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SMS.Services
{
    /// <summary>
    /// مرسل الإشعارات اللحظي داخل التطبيق (In-App SignalR Dispatcher)
    /// يقوم ببث الإشعار وحمولة الـ Deep Linking فوراً لهواتف الأهل المفتوحة عبر WebSockets
    /// بدون أي وسيط خارجي وبدون تخزين في قاعدة البيانات.
    /// </summary>
    public class SignalRParentNotificationDispatcher : IParentPushNotificationDispatcher
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRParentNotificationDispatcher> _logger;

        public SignalRParentNotificationDispatcher(
            IHubContext<NotificationHub> hubContext,
            ILogger<SignalRParentNotificationDispatcher> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task DispatchAsync(ParentNotificationPayload payload)
        {
            if (payload == null || payload.TargetParentPersonIds == null || payload.TargetParentPersonIds.Count == 0)
            {
                _logger.LogWarning("[InApp-Notification] Discarded notification due to empty target recipients.");
                return;
            }

            var targetUserIds = payload.TargetParentPersonIds.Select(id => id.ToString()).ToList();

            _logger.LogInformation(
                "[InApp-SignalR] Dispatching notification '{Title}' [Type: {Type}] to ({Count}) connected parent(s). DeepLink: {DeepLink}",
                payload.Title,
                payload.TypeName,
                targetUserIds.Count,
                payload.Data.DeepLinkUrl
            );

            // إرسال الإشعار اللحظي إلى حسابات أولياء الأمور المستهدفين فقط
            // عن طريق اسم الحدث "ReceiveParentNotification"
            await _hubContext.Clients.Users(targetUserIds).SendAsync("ReceiveParentNotification", payload);
        }
    }
}
