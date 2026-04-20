namespace BankingOperationsService.Application.Configuration;

/// <summary>
/// Strongly-typed settings for the DSeries.DOmniBus.Lite messaging library.
/// Bound from kafka.json (section: "DOmniBus").
/// </summary>
public class DOmniBusSettings
{
    public const string SectionName = "DOmniBus";

    public KafkaSettings Kafka { get; set; } = new();
}

/// <summary>
/// Mirrors every property exposed on the DOmniBus.Lite bus config lambda (v1.0.1).
/// Enum-valued options are stored as strings and parsed in ApplicationConfiguration
/// so that kafka.json stays plain-text and environment-agnostic.
/// </summary>
public class KafkaSettings
{
    // -------------------------------------------------------------------------
    // Kafka connection
    // -------------------------------------------------------------------------

    /// <summary>Comma-separated broker list. Default: "localhost:9092"</summary>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>Client id reported to the broker. Default: "DOmniBus.Lite"</summary>
    public string ClientId { get; set; } = "DOmniBus.Lite";

    /// <summary>Consumer group id — must match across replicas. Default: "DOmniBus.Lite"</summary>
    public string GroupId { get; set; } = "DOmniBus.Lite";

    // -------------------------------------------------------------------------
    // Security / auth (Confluent Cloud, MSK SASL, Aiven, etc.)
    // Confluent.Kafka enum names as strings: Plaintext | Ssl | SaslPlaintext | SaslSsl
    // -------------------------------------------------------------------------

    /// <summary>
    /// Confluent.Kafka SecurityProtocol enum name.
    /// Plaintext | Ssl | SaslPlaintext | SaslSsl. Default: "Plaintext"
    /// </summary>
    public string SecurityProtocol { get; set; } = "Plaintext";

    /// <summary>
    /// Confluent.Kafka SaslMechanism enum name.
    /// Plain | ScramSha256 | ScramSha512 | OAuthBearer | Gssapi. Null = not set.
    /// </summary>
    public string? SaslMechanism { get; set; }

    public string? SaslUsername { get; set; }

    public string? SaslPassword { get; set; }

    // -------------------------------------------------------------------------
    // Consumer tuning
    // -------------------------------------------------------------------------

    /// <summary>Parallel worker tasks. 1 = single-threaded (default).</summary>
    public int MaxConcurrency { get; set; } = 1;

    /// <summary>
    /// Confluent.Kafka AutoOffsetReset enum name.
    /// Earliest | Latest | Error. Default: "Earliest"
    /// </summary>
    public string AutoOffsetReset { get; set; } = "Earliest";

    /// <summary>Consumer session timeout in ms. Null = library/broker default.</summary>
    public int? SessionTimeoutMs { get; set; }

    /// <summary>Max poll interval in ms. Null = library/broker default.</summary>
    public int? MaxPollIntervalMs { get; set; }

    // -------------------------------------------------------------------------
    // Producer tuning
    // -------------------------------------------------------------------------

    /// <summary>Enable idempotent producer. Default: true</summary>
    public bool EnableIdempotence { get; set; } = true;

    /// <summary>
    /// Confluent.Kafka Acks enum name.
    /// None | Leader | All. Null = library/broker default.
    /// </summary>
    public string? Acks { get; set; }

    /// <summary>
    /// Confluent.Kafka CompressionType enum name.
    /// None | Gzip | Snappy | Lz4 | Zstd. Null = none.
    /// </summary>
    public string? CompressionType { get; set; }

    // -------------------------------------------------------------------------
    // Dead-letter queue
    // -------------------------------------------------------------------------

    /// <summary>Forward failed messages to a DLQ topic. Default: false</summary>
    public bool EnableDeadLetterQueue { get; set; } = false;

    /// <summary>Suffix appended to the source topic to form the DLQ topic name. Default: ".dlq"</summary>
    public string DeadLetterTopicSuffix { get; set; } = ".dlq";

    // -------------------------------------------------------------------------
    // Topics — convenience dictionary so topic names are not hardcoded in C#.
    // Read these in AddMessaging when calling bus.Map<TMessage, THandler>(topic).
    // -------------------------------------------------------------------------

    /// <summary>Named topic strings keyed by a logical name, e.g. "OrdersCommands": "orders.commands".</summary>
    public Dictionary<string, string> Topics { get; set; } = new();
}
