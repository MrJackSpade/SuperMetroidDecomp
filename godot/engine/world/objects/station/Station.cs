using Engine.Actors;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>
/// A station: it holds the player while it works, then grants or records its result. What it does is a setting: save
/// the game, reveal the area's map, or refill listed inventory amounts.
/// </summary>
public partial class Station : RoomObject
{
    /// <summary>What a station does when it has finished working.</summary>
    public enum Does
    {
        /// <summary>It asks for the game to be saved, resuming at this station.</summary>
        Saves,

        /// <summary>It reveals the map of the room's area.</summary>
        RevealsMap,

        /// <summary>It fills the listed inventory amounts.</summary>
        Refills,
    }

    /// <summary>What the station does.</summary>
    [Export]
    public Does Kind { get; set; }

    /// <summary>The inventory amounts a refilling station fills, by name.</summary>
    [Export]
    public string[] Refills { get; set; } = [];

    /// <summary>The station's width and height, in pixels; its top-left is where it stands.</summary>
    [Export]
    public Vector2I Size { get; set; } = new(32, 48);

    /// <summary>How many updates the station works for.</summary>
    [Export]
    public int WorkUpdates { get; set; } = 60;

    private Countdown _working = Countdown.Over;
    private bool _busy;
    private bool _mustLeave;

    /// <summary>The fact recorded when an area's map is revealed, for <paramref name="area"/>.</summary>
    public static string MapRevealedFact(string area) => $"map revealed: {area}";

    /// <summary>Whether the station is holding the player.</summary>
    public bool IsWorking => _busy;

    /// <summary>The region of the room the station covers.</summary>
    public PixelRect Area => new(Position.X.Whole, Position.Y.Whole, Size.X, Size.Y);

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Player player = Room.Player;
        bool inside = Area.Overlaps(player.BodyRect);
        if (_busy)
        {
            _working = _working.Stepped();
            if (_working.IsRunning)
                return;
            _busy = false;
            _mustLeave = true;
            Finish(player);
            player.Release();
            return;
        }

        if (!inside)
        {
            _mustLeave = false;
            return;
        }

        if (_mustLeave || player.State is not StandingState || !CanBeUsed(player))
            return;
        _busy = true;
        _working = Countdown.Of(WorkUpdates);
        player.UseStation();
    }

    /// <summary>Whether the station has anything to do for the player; a refill station does nothing for one who is full.</summary>
    private bool CanBeUsed(Player player) =>
        Kind != Does.Refills || Refills.Any(amount => !player.Inventory.IsFull(amount));

    /// <summary>Grants or records the station's result.</summary>
    private void Finish(Player player)
    {
        switch (Kind)
        {
            case Does.Saves:
                Room.Session.RequestSave(Room.SceneFilePath, Name);
                break;
            case Does.RevealsMap:
                if (Room.Area.Length == 0)
                    throw new InvalidOperationException($"The map station \"{Name}\" is in a room that is in no area.");
                Room.Session.Progress.Record(MapRevealedFact(Room.Area));
                break;
            case Does.Refills:
                foreach (string amount in Refills)
                    player.Inventory.Fill(amount);
                break;
            default:
                throw new InvalidOperationException($"Undefined station kind {Kind}.");
        }
    }
}
