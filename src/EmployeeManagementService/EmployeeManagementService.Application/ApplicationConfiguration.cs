using EmployeeManagementService.Application.Configuration;
using EmployeeManagementService.Application.Features.Identity.Commands.CreateUser;
using EmployeeManagementService.Application.Features.Identity.Commands.LoginUser;
using Confluent.Kafka;
using DOmniBus.Lite;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EmployeeManagementService.Application.MessageEmitters.RequestMetaDataEmitter.Create;
using EmployeeManagementService.Domain.Enums;
using EmployeeManagementService.Application.MessageEmitters.RequestMetaDataEmitter.Update;

namespace EmployeeManagementService.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
        => services
            .Configure<ApplicationSettings>(
                configuration.GetSection(nameof(ApplicationSettings)),
                options => options.BindNonPublicProperties = true)
            .Configure<DOmniBusSettings>(
                configuration.GetSection(DOmniBusSettings.SectionName))
            .AddTransient<CreateUserService>()
            .AddTransient<LoginUserService>()
            .AddMessaging(configuration)
            .AddValidatorsFromAssemblyContaining<ApplicationSettings>();

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

            // emitters
            bus.Map<CreateRequestMetaDataEvent>(
                topic: EmitterType.RequestMetaData_Create.ToString(),
                wireType: nameof(CreateRequestMetaDataEvent));

            bus.Map<UpdateRequestMetaDataEvent>(
                topic: EmitterType.RequestMetaData_Update.ToString(),
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
