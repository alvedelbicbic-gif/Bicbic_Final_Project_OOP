namespace TechGuildEventManager.Models;

public class TechCompetition
{
    public int CompetitionId { get; set; }
    public string CompetitionName { get; set; } = "";
    public string ParticipantName { get; set; } = "";
    public string TechStack { get; set; } = "";
    public int Rank { get; set; }
}
