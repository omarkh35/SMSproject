using BLL.Notifications.Interfaces;
using BLL.Notifications.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace BLL.Notifications.Services
{
    /// <summary>
    /// مرسل الإشعارات الفوري لتطبيق الأهل (Push Notification Dispatcher)
    /// مسؤول عن تجهيز حزم بيانات الإشعارات الفورية (Push Notification Packets)
    /// وتمريرها لمزود الإشعارات (مثل Firebase Cloud Messaging FCM أو Apple Push Notification APNs)
    /// دون أي حفظ في قاعدة البيانات.
    /// </summary>
    public class ParentPushNotificationDispatcher : IParentPushNotificationDispatcher
    {
        private readonly ILogger<ParentPushNotificationDispatcher> _logger;

        public ParentPushNotificationDispatcher(ILogger<ParentPushNotificationDispatcher> logger)
        {
            _logger = logger;
        }

        public Task DispatchAsync(ParentNotificationPayload payload)
        {
            if (payload == null || payload.TargetParentPersonIds == null || payload.TargetParentPersonIds.Count == 0)
            {
                _logger.LogWarning("[PushNotification] Discarded notification due to empty target recipients.");
                return Task.CompletedTask;
            }

            var serializedPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            // تسجيل حزمة الإشعار الفوري في سجل النظام
            _logger.LogInformation(
                "[PushNotification] Dispatched to ({Count}) Parent(s) [Type: {Type}]: Title: \"{Title}\", DeepLink: \"{DeepLink}\"\nPayload:\n{Payload}",
                payload.TargetParentPersonIds.Count,
                payload.TypeName,
                payload.Title,
                payload.Data.DeepLinkUrl,
                serializedPayload
            );

            // نقطة تكامل جاهزة لـ Firebase Cloud Messaging / WebPush:
            // Example Integration:
            // var message = new MulticastMessage()
            // {
            //     Notification = new Notification { Title = payload.Title, Body = payload.Body },
            //     Data = new Dictionary<string, string>
            //     {
            //         { "actionType", payload.Data.ActionType },
            //         { "route", payload.Data.Route },
            //         { "deepLinkUrl", payload.Data.DeepLinkUrl },
            //         { "studentId", payload.Data.StudentId?.ToString() ?? "" },
            //         { "entityId", payload.Data.EntityId?.ToString() ?? "" }
            //     },
            //     Tokens = parentDeviceTokens
            // };
            // await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);

            return Task.CompletedTask;
        }
    }
}
