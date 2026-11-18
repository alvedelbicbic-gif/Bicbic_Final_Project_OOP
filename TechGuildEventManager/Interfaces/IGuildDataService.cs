
using TechGuildEventManager.Models;

namespace TechGuildEventManager.Interfaces;

public interface IGuildDataService
{
    List<GuildMember> Members { get; }

    List<GuildEvent> Events { get; }

    List<TechCompetition> Competitions { get; }

    void LoadAllData();

    void SaveAllData();
}
