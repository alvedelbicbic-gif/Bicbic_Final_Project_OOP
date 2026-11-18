namespace TechGuildEventManager.Models;

public class GuildEvent
{
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string Venue { get; set; } = "";
    public string Organizer { get; set; } = "";
}
