var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("Environment variable 'ConnectionStrings__DefaultConnection' not set. Skipping DB initialization.");
    return;
}
else
{
    Console.WriteLine("Initializing database schema...");
    await DatabaseInitializer.EnsureDatabaseSchemaAsync(connectionString);
    Console.WriteLine("Database schema ready.");
}

var channels = PartitionedChannelProcessor.CreateChannels();
Console.WriteLine($"Created {channels.Length} bounded partitions.");

var messages = await JsonMessageLoader.LoadAllAsync();
Console.WriteLine($"Loaded {messages.Count} messages from JSON.");

var consumerTask = EventMessageConsumer.StartConsumersAsync(channels, connectionString);

var partitionCounts = new int[channels.Length];
var index = 0;
foreach (var message in messages)
{
    index++;

    if (message.Event is null)
    {
        continue;
    }

    var eventId = message.Event.ProviderEventID;
    var partitionIndex = EventPartitioner.GetPartitionIndex(eventId, channels.Length);

    partitionCounts[partitionIndex]++;
    Console.WriteLine($"Dispatching message #{index} with EventId {eventId} to partition {partitionIndex}.");

    await channels[partitionIndex].Writer.WriteAsync(message);
}

foreach (var channel in channels)
{
    channel.Writer.Complete();
}

await consumerTask;

var distribution = string.Join(", ", partitionCounts.Select((count, i) => $"{i}:{count}"));
Console.WriteLine($"Partition distribution: {distribution}");

Console.WriteLine("All messages processed.");
