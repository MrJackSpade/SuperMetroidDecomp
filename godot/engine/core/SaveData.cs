using Engine.Actors;

namespace Engine.Core;

/// <summary>
/// The saved form of a game: what has happened, what the player carries, and where they resume. It is what a save
/// slot holds; it does not exist as live state during play.
/// </summary>
public sealed class SaveData
{
    /// <summary>The facts recorded in progress.</summary>
    public List<string> Facts { get; set; } = [];

    /// <summary>The map cells explored, each as "area|column|row".</summary>
    public List<string> Explored { get; set; } = [];

    /// <summary>How many updates of play have passed.</summary>
    public long PlayUpdates { get; set; }

    /// <summary>The equipment the player owns.</summary>
    public List<string> Owned { get; set; } = [];

    /// <summary>The equipment the player has switched on.</summary>
    public List<string> SwitchedOn { get; set; } = [];

    /// <summary>How much of each amount the player has, by name.</summary>
    public Dictionary<string, int> Amounts { get; set; } = [];

    /// <summary>The most of each amount the player can hold, by name.</summary>
    public Dictionary<string, int> Capacities { get; set; } = [];

    /// <summary>The scene of the room the player resumes in.</summary>
    public string Room { get; set; } = "";

    /// <summary>The entry point in that room the player resumes at.</summary>
    public string EntryPoint { get; set; } = "";

    /// <summary>
    /// The game's own settings as they stood when it was saved, by name: what is on. A game keeps with a save the
    /// settings that change how it plays; whoever chooses a save to play gives them back to the options.
    /// </summary>
    public Dictionary<string, bool> Toggles { get; set; } = [];

    /// <summary>The language the game was being played in when it was saved; empty in a save made before this was kept.</summary>
    public string Language { get; set; } = "";

    /// <summary>
    /// The control each action was on when the game was saved, by the action's name, for the actions the game puts on
    /// a control other than their own; empty in a save made before this was kept.
    /// </summary>
    public Dictionary<string, string> Controls { get; set; } = [];

    /// <summary>The saved form of <paramref name="session"/>'s game, to resume in <paramref name="room"/> at <paramref name="entryPoint"/>.</summary>
    public static SaveData Capture(GameSession session, string room, string entryPoint)
    {
        Inventory inventory = session.Player.Inventory;
        return new SaveData
        {
            Facts = [.. session.Progress.Facts.Order(StringComparer.Ordinal)],
            Explored = [.. session.Progress.Explored.Select(cell => $"{cell.Area}|{cell.Column}|{cell.Row}").Order(StringComparer.Ordinal)],
            PlayUpdates = session.Progress.PlayUpdates,
            Owned = [.. inventory.Owned.Order(StringComparer.Ordinal)],
            SwitchedOn = [.. inventory.SwitchedOn.Order(StringComparer.Ordinal)],
            Amounts = inventory.Amounts.ToDictionary(amount => amount, inventory.Amount),
            Capacities = inventory.Amounts.ToDictionary(amount => amount, inventory.Capacity),
            Room = room,
            EntryPoint = entryPoint,
            Toggles = new Dictionary<string, bool>(session.Options.Toggles),
            Language = session.Options.Language,
            Controls = new Dictionary<string, string>(session.Options.Controls),
        };
    }

    /// <summary>The fact recorded when a game is saved to resume at <paramref name="entryPoint"/>: that it has been saved there.</summary>
    public static string SavedAtFact(string entryPoint) => $"saved at: {entryPoint}";

    /// <summary>What this saved game remembers, as progress of its own: for showing a saved game without loading it.</summary>
    public Progress Remembered()
    {
        var progress = new Progress();
        Restore(progress);
        return progress;
    }

    /// <summary>Puts what this saved game remembers into <paramref name="progress"/>.</summary>
    private void Restore(Progress progress)
    {
        foreach (string fact in Facts)
            progress.Record(fact);
        foreach (string cell in Explored)
        {
            string[] parts = cell.Split('|');
            if (parts.Length != 3 || !int.TryParse(parts[1], out int column) || !int.TryParse(parts[2], out int row))
                throw new InvalidDataException($"The save holds an explored cell it cannot read: \"{cell}\".");
            progress.MarkExplored(parts[0], column, row);
        }

        progress.RestorePlayUpdates(PlayUpdates);
    }

    /// <summary>Puts this saved game into <paramref name="session"/>: its progress and its player's inventory.</summary>
    public void ApplyTo(GameSession session)
    {
        Restore(session.Progress);
        Inventory inventory = session.Player.Inventory;
        foreach (string equipment in Owned)
            inventory.Grant(equipment);
        foreach (string equipment in Owned.Except(SwitchedOn))
            inventory.Switch(equipment, false);
        foreach ((string amount, int capacity) in Capacities)
            inventory.SetCapacity(amount, capacity);
        foreach ((string amount, int quantity) in Amounts)
            inventory.Restore(amount, quantity);
    }
}
