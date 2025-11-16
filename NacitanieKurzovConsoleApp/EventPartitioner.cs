public static class EventPartitioner
{
    public static int GetPartitionIndex(long providerEventId, int partitionCount)
    {
        return (int)(providerEventId % partitionCount);
    }
}

