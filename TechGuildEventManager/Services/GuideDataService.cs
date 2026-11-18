
using System.Text.Json;
using TechGuildEventManager.Interfaces;
using TechGuildEventManager.Models;

namespace TechGuildEventManager.Services;

public class GuildDataService : IGuildDataService
{
    public List<GuildMember> Members { get; } = new();

    public List<GuildEvent> Events { get; } = new();

    public List<TechCompetition> Competitions { get; } = new();

    private readonly string folderPath;

    private readonly JsonSerializerOptions options = new()
    {
        WriteIndented = true
    };

    public GuildDataService()
    {
        folderPath = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "TechGuildEventManager");
    }

    public void LoadAllData()
    {
        Directory.CreateDirectory(folderPath);

        string membersPath = Path.Combine(folderPath, "members.json");
        string eventsPath = Path.Combine(folderPath, "events.json");
        string competitionsPath = Path.Combine(folderPath, "competitions.json");

        if (File.Exists(membersPath))
        {
            var data = JsonSerializer.Deserialize<List<GuildMember>>(
                File.ReadAllText(membersPath));

            if (data != null)
            {
                Members.Clear();
                Members.AddRange(data);
            }
        }

        if (File.Exists(eventsPath))
        {
            var data = JsonSerializer.Deserialize<List<GuildEvent>>(
                File.ReadAllText(eventsPath));

            if (data != null)
            {
                Events.Clear();
                Events.AddRange(data);
            }
        }

        if (File.Exists(competitionsPath))
        {
            var data = JsonSerializer.Deserialize<List<TechCompetition>>(
                File.ReadAllText(competitionsPath));

            if (data != null)
            {
                Competitions.Clear();
                Competitions.AddRange(data);
            }
        }
    }

    public void SaveAllData()
    {
        Directory.CreateDirectory(folderPath);

        File.WriteAllText(
            Path.Combine(folderPath, "members.json"),
            JsonSerializer.Serialize(Members, options));

        File.WriteAllText(
            Path.Combine(folderPath, "events.json"),
            JsonSerializer.Serialize(Events, options));

        File.WriteAllText(
            Path.Combine(folderPath, "competitions.json"),
            JsonSerializer.Serialize(Competitions, options));
    }
}
