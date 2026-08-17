using BLL.Notifications.Events;
using BLL.Notifications.Interfaces;
using BLL.Notifications.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BLL
{
    public static class DepencyInjection
    {
        public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
        {
            // Register Push Notification Dispatcher (zero-DB in-memory push)
            services.AddSingleton<IParentPushNotificationDispatcher, ParentPushNotificationDispatcher>();

            // Register Centralized Event Publisher
            services.AddScoped<INotificationPublisher, NotificationPublisher>();

            // Register Parent Push Notification Event Subscribers
            services.AddScoped<INotificationSubscriber<StudentNoteAddedEvent>, ParentPushNotificationSubscriber>();
            services.AddScoped<INotificationSubscriber<HomeworkAssignedEvent>, ParentPushNotificationSubscriber>();
            services.AddScoped<INotificationSubscriber<MarksReleasedEvent>, ParentPushNotificationSubscriber>();
            services.AddScoped<INotificationSubscriber<ChatMessageSentEvent>, ParentPushNotificationSubscriber>();

            return services;
        }
    }
}

