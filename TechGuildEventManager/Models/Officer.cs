namespace TechGuildEventManager.Models;

public class Officer : GuildMember, Interfaces.IOrganizer
{
    public string AssignedTask { get; set; } = "";
    public string Venue { get; set; } = "";
    public int EventsOrganizedCount { get; set; }
    public int OrganizedEvents { get; set; }

    public Officer()
    {
        MemberType = "Officer";
    }

    public Officer(int memberId, string fullName, string email, string course, int organizedCount = 0)
        : base(memberId, fullName, email, course)
    {
        MemberType = "Officer";
        EventsOrganizedCount = organizedCount;
    }

    public void OrganizeEvent(string task, string venue)
    {
        AssignedTask = task;
        Venue = venue;
        EventsOrganizedCount++;
    }

    public override int CalculateEventPoints(int attendedEvents)
    {
        return (attendedEvents * 10) + (EventsOrganizedCount * 25);
    }
}