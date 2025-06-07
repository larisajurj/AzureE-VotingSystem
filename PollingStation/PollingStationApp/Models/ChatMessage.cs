namespace PollingStationApp.Models;

// ChatMessage.cs (or in @code block)
public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsUserMessage { get; set; } 
    public string Sender { get; set; }
}