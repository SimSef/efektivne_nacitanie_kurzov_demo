public static class EventPartitioner
{
    public static int GetPartitionIndex(long providerEventId, int partitionCount)
    {
        var hash = (int)(providerEventId % partitionCount);

        if (hash < 0)
        {
            hash += partitionCount;
        }

        return hash;
    }
}

