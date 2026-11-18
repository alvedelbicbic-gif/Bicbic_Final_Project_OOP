namespace TechGuildEventManager.Models;

public class RegularMember : GuildMember
{
    public string TechStack { get; set; } = string.Empty;
    public int EventsOrganizedCount { get; set; }

    public RegularMember(int memberId, string fullName, string email, string course, string techStack, int eventsOrganizedCount = 0)
        : base(memberId, fullName, email, course)
    {
        TechStack = techStack;
        EventsOrganizedCount = eventsOrganizedCount;
    }

    public override int CalculateEventPoints(int eventsAttended)
    {
        return (eventsAttended * 10) + (EventsOrganizedCount * 5);
    }
}