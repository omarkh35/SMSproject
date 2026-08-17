using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace SMS.Hubs
{
    /// <summary>
    /// قناة الاتصال اللحظي للإشعارات داخل التطبيق (In-App Realtime Notification Hub)
    /// تعتمد على WebSockets وتقوم بربط كل مستخدم بـ PersonId الخاص به القادم من JWT
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var personId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(personId))
            {
                // إضافة العميل لمجموعة خاصة به بمعرف الشخص (PersonId)
                await Groups.AddToGroupAsync(Context.ConnectionId, $"parent_{personId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var personId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(personId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"parent_{personId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
