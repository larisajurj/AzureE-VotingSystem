namespace PollingStationAPI.Data.Models;

public class Booth
{
   public required string Id { get; set; }
   public string? Status { get; set; }
   public string? SessionId { get; set; }
}
