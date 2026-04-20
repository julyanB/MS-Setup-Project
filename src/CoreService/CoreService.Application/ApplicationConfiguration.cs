using CoreService.Application.Configuration;
using Confluent.Kafka;
using DOmniBus.Lite;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CoreService.Domain.Enums;
using CoreService.Application.MessageHandler.RequestMetaDataSubsciber.Create;
using CoreService.Application.MessageHandler.RequestMetaDataSubsciber.Update;

namespace CoreService.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .Configure<DOmniBusSettings>(
                configuration.GetSection(DOmniBusSettings.SectionName))
            .AddMessaging(configuration)
            .AddValidatorsFromAssemblyContaining(typeof(ApplicationConfiguration));

    private static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var kafka = configuration
            .GetSection($"{DOmniBusSettings.SectionName}:{nameof(DOmniBusSettings.Kafka)}")
            .Get<KafkaSettings>() ?? new KafkaSettings();

        services.AddDOmniBusLite(bus =>
        {
            SetKafkaSettings(bus, kafka);

            // subscribers
            bus.Map<CreateRequestMetaDataEvent, CreateRequestMetaDataHandler>(
                topic: SubscriberType.RequestMetaData_Create.ToString(),
                wireType: nameof(CreateRequestMetaDataEvent));

            bus.Map<UpdateRequestMetaDataEvent, UpdateRequestMetaDataHandler>(
                topic: SubscriberType.RequestMetaData_Update.ToString(),
                wireType: nameof(UpdateRequestMetaDataEvent));
        });

        return services;
    }

    private static void SetKafkaSettings(DOmniBusLiteOptions bus, KafkaSettings kafka)
    {
        bus.BootstrapServers = kafka.BootstrapServers;
        bus.ClientId = kafka.ClientId;
        bus.GroupId = kafka.GroupId;

        bus.SecurityProtocol = Enum.Parse<SecurityProtocol>(
            kafka.SecurityProtocol, ignoreCase: true);

        if (kafka.SaslMechanism is not null)
        {
            bus.SaslMechanism = Enum.Parse<SaslMechanism>(
                kafka.SaslMechanism, ignoreCase: true);
        }

        if (kafka.SaslUsername is not null)
        {
            bus.SaslUsername = kafka.SaslUsername;
        }

        if (kafka.SaslPassword is not null)
        {
            bus.SaslPassword = kafka.SaslPassword;
        }

        bus.MaxConcurrency = kafka.MaxConcurrency;
        bus.AutoOffsetReset = Enum.Parse<AutoOffsetReset>(
            kafka.AutoOffsetReset, ignoreCase: true);

        if (kafka.SessionTimeoutMs.HasValue)
        {
            bus.SessionTimeoutMs = kafka.SessionTimeoutMs.Value;
        }

        if (kafka.MaxPollIntervalMs.HasValue)
        {
            bus.MaxPollIntervalMs = kafka.MaxPollIntervalMs.Value;
        }

        bus.EnableIdempotence = kafka.EnableIdempotence;

        if (kafka.Acks is not null)
        {
            bus.Acks = Enum.Parse<Acks>(kafka.Acks, ignoreCase: true);
        }

        if (kafka.CompressionType is not null)
        {
            bus.CompressionType = Enum.Parse<CompressionType>(
                kafka.CompressionType, ignoreCase: true);
        }

        bus.EnableDeadLetterQueue = kafka.EnableDeadLetterQueue;
        bus.DeadLetterTopicSuffix = kafka.DeadLetterTopicSuffix;
    }
}
