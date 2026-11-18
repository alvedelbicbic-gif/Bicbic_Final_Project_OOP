namespace TechGuildEventManager.Models;

public class Alumni : GuildMember
{
    public string TechStack { get; set; } = string.Empty;
    public int EventsOrganizedCount { get; set; }

    public Alumni(int memberId, string fullName, string email, string course, string techStack, int eventsOrganizedCount = 0)
        : base(memberId, fullName, email, course)
    {
        TechStack = techStack;
        EventsOrganizedCount = eventsOrganizedCount;
    }

    public override int CalculateEventPoints(int eventsAttended)
    {
        return (eventsAttended * 15) + (EventsOrganizedCount * 5);
    }
}