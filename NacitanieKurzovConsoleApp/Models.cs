public sealed class EventMessage
{
    public string MessageID { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public EventData? Event { get; set; }
}

public sealed class EventData
{
    public long ProviderEventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public List<OddsItem> OddsList { get; set; } = new();
}

public sealed class OddsItem
{
    public long ProviderOddsID { get; set; }
    public string OddsName { get; set; } = string.Empty;
    public decimal OddsRate { get; set; }
    public string Status { get; set; } = string.Empty;
}

