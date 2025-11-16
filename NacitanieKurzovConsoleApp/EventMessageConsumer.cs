using System.Data;
using System.Threading.Channels;
using Dapper;
using Microsoft.Data.SqlClient;

public static class EventMessageConsumer
{
    public static Task StartConsumersAsync(
        Channel<EventMessage>[] channels,
        string connectionString)
    {
        var tasks = new List<Task>(channels.Length);

        for (var partitionIndex = 0; partitionIndex < channels.Length; partitionIndex++)
        {
            var reader = channels[partitionIndex].Reader;
            var localPartitionIndex = partitionIndex;

            tasks.Add(Task.Run(async () =>
            {
                var random = new Random(Random.Shared.Next());

                await foreach (var message in reader.ReadAllAsync())
                {
                    var eventId = message.Event!.ProviderEventID;
                    Console.WriteLine($"Consuming message for EventId {eventId} from partition {localPartitionIndex}.");

                    await ProcessMessageAsync(message, connectionString, random);
                }
            }));
        }

        return Task.WhenAll(tasks);
    }

    private static async Task ProcessMessageAsync(
        EventMessage message,
        string connectionString,
        Random random)
    {
        var eventData = message.Event!;

        var delayMilliseconds = random.Next(0, 10_000);
        await Task.Delay(delayMilliseconds);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        var eventId = await UpsertEventAsync(connection, transaction, eventData);

        await UpsertOddsAsync(connection, transaction, eventId, eventData.OddsList);

        await transaction.CommitAsync();
    }

    private static async Task<int> UpsertEventAsync(
        SqlConnection connection,
        IDbTransaction transaction,
        EventData eventData)
    {
        var existingId = await connection.QuerySingleOrDefaultAsync<int?>(
            new CommandDefinition(
                "SELECT Id FROM dbo.Events WHERE ProviderEventID = @ProviderEventID",
                new { eventData.ProviderEventID },
                transaction));

        if (existingId.HasValue)
        {
            var eventId = existingId.Value;

            await connection.ExecuteAsync(
                new CommandDefinition(
                    @"UPDATE dbo.Events
                      SET EventDate = @EventDate
                      WHERE Id = @Id
                        AND EventDate <> @EventDate;",
                    new
                    {
                        Id = eventId,
                        EventDate = eventData.EventDate
                    },
                    transaction));

            return eventId;
        }

        var insertedId = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                @"INSERT INTO dbo.Events (ProviderEventID, EventName, EventDate)
                  VALUES (@ProviderEventID, @EventName, @EventDate);
                  SELECT CAST(SCOPE_IDENTITY() AS int);",
                new
                {
                    eventData.ProviderEventID,
                    EventName = eventData.EventName,
                    EventDate = eventData.EventDate
                },
                transaction));

        return insertedId;
    }

    private static async Task UpsertOddsAsync(
        SqlConnection connection,
        IDbTransaction transaction,
        int eventId,
        List<OddsItem>? oddsList)
    {
        if (oddsList is null || oddsList.Count == 0)
        {
            return;
        }

        foreach (var odds in oddsList)
        {
            var existingId = await connection.QuerySingleOrDefaultAsync<int?>(
                new CommandDefinition(
                    @"SELECT Id
                      FROM dbo.Odds
                      WHERE ProviderOddsID = @ProviderOddsID;",
                    new { odds.ProviderOddsID },
                    transaction));

            if (existingId.HasValue)
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        @"UPDATE dbo.Odds
                          SET OddsRate = @OddsRate,
                              Status = @Status
                          WHERE Id = @Id
                            AND (OddsRate <> @OddsRate OR Status <> @Status);",
                        new
                        {
                            Id = existingId.Value,
                            OddsRate = odds.OddsRate,
                            Status = odds.Status
                        },
                        transaction));

                continue;
            }

            await connection.ExecuteAsync(
                new CommandDefinition(
                    @"INSERT INTO dbo.Odds (ProviderOddsID, EventId, OddsName, OddsRate, Status)
                      VALUES (@ProviderOddsID, @EventId, @OddsName, @OddsRate, @Status);",
                    new
                    {
                        odds.ProviderOddsID,
                        EventId = eventId,
                        OddsName = odds.OddsName,
                        OddsRate = odds.OddsRate,
                        Status = odds.Status
                    },
                    transaction));
        }
    }
}
