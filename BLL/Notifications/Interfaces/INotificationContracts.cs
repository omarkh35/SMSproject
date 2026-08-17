using BLL.Notifications.Events;
using BLL.Notifications.Models;
using System.Threading.Tasks;

namespace BLL.Notifications.Interfaces
{
    /// <summary>
    /// ناشر الأحداث (Publisher) لنشر أحداث النظام فور وقوعها في الطبقات الخدمية
    /// </summary>
    public interface INotificationPublisher
    {
        Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : INotificationEvent;
    }

    /// <summary>
    /// معالج الأحداث (Subscriber) للاشتراك في حدث معين ومعالجته
    /// </summary>
    public interface INotificationSubscriber<TEvent> where TEvent : INotificationEvent
    {
        Task HandleAsync(TEvent domainEvent);
    }

    /// <summary>
    /// خدمة إرسال الإشعارات لتطبيق الأهل (Push Notification Dispatcher / Gateway)
    /// بدون تخزين في قاعدة البيانات - يتم الإرسال الفوري لـ FCM / APNs / WebSockets
    /// </summary>
    public interface IParentPushNotificationDispatcher
    {
        Task DispatchAsync(ParentNotificationPayload payload);
    }
}
