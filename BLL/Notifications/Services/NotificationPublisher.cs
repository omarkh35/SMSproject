using BLL.Notifications.Events;
using BLL.Notifications.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BLL.Notifications.Services
{
    /// <summary>
    /// ناشر الأحداث المركزي (Centralized Domain Event Publisher)
    /// يقوم بتوزيع الأحداث المحدثة فورياً على المشتركين المسجلين في الـ DI Container
    /// بنمط Publish-Subscribe مفصول بالكامل عن قاعدة البيانات
    /// </summary>
    public class NotificationPublisher : INotificationPublisher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationPublisher> _logger;

        public NotificationPublisher(IServiceProvider serviceProvider, ILogger<NotificationPublisher> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : INotificationEvent
        {
            if (domainEvent == null) return;

            var subscribers = _serviceProvider.GetServices<INotificationSubscriber<TEvent>>();

            foreach (var subscriber in subscribers)
            {
                try
                {
                    await subscriber.HandleAsync(domainEvent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "[NotificationPublisher] Error occurred while executing subscriber {Subscriber} for event {Event}",
                        subscriber.GetType().Name,
                        typeof(TEvent).Name
                    );
                }
            }
        }
    }
}
