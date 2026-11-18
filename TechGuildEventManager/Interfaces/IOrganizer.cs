namespace TechGuildEventManager.Interfaces;

public interface IOrganizer
{
    string AssignedTask { get; set; }
    string Venue { get; set; }
    void OrganizeEvent(string task, string venue);
}
