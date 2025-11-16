using System.Threading.Channels;

public static class PartitionedChannelProcessor
{
    public static Channel<EventMessage>[] CreateChannels(
        int partitionCount = 30,
        int channelCapacity = 20)
    {
        var channels = new Channel<EventMessage>[partitionCount];

        for (var i = 0; i < partitionCount; i++)
        {
            channels[i] = Channel.CreateBounded<EventMessage>(
                new BoundedChannelOptions(channelCapacity)
                {
                    SingleWriter = true,
                    SingleReader = true,
                    FullMode = BoundedChannelFullMode.Wait
                });
        }

        return channels;
    }
}

