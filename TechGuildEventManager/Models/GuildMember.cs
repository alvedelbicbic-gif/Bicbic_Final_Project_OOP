using System.Text.Json.Serialization;

namespace TechGuildEventManager.Models;

[JsonDerivedType(typeof(Officer), typeDiscriminator: "Officer")]
[JsonDerivedType(typeof(RegularMember), typeDiscriminator: "RegularMember")]
[JsonDerivedType(typeof(Alumni), typeDiscriminator: "Alumni")]
public abstract class GuildMember
{
    public int MemberId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Course { get; set; } = "";
    public string MemberType { get; protected set; } = "";

    protected GuildMember() { }

    protected GuildMember(int memberId, string fullName, string email, string course)
    {
        MemberId = memberId;
        FullName = fullName;
        Email = email;
        Course = course;
    }

    public abstract int CalculateEventPoints(int attendedEvents);
}