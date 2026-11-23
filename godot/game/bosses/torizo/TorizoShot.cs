using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// Something a Torizo throws, spews or leaves: by itself a burst that plays out where it is, and the base of those
/// that fly. It moves itself, at its own speed, and meets terrain its own way, so its settings let it through
/// terrain. Its timelines' cues turn its harm on and off and move it about.
/// </summary>
/// <remarks>
/// Its data lists under "timelines" the timelines it changes to, by the names its class uses. The cues it knows are
/// "harmless", "harmful", "stop", "end", "front", "behind", "origin" (back to where it began, or last came to rest),
/// "rise" (up four pixels), "scatter:" with the width and height of the spread and how far into it the middle lies
/// each way, "smoke:" with a timeline it turns to one time in four, and "sound:" with a sound's name.
/// </remarks>
public partial class TorizoShot : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    private readonly Dictionary<string, string> _timelines = new(StringComparer.Ordinal);
    private bool _harmless;

    /// <summary>The speed it moves itself at.</summary>
    protected PixelVelocity Speed { get; set; }

    /// <summary>Whether it is moving itself at the moment.</summary>
    protected bool Moving { get; set; } = true;

    /// <summary>Where it began, or last came to rest.</summary>
    protected PixelPosition Origin { get; set; }

    /// <inheritdoc/>
    public virtual void ReadValues(DataNode data)
    {
        _timelines.Clear();
        if (!data.Has("timelines"))
            return;
        foreach ((string name, DataNode file) in data.Get("timelines").Entries())
            _timelines[name] = file.AsText();
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    protected override bool DeliversHitNow => !_harmless && base.DeliversHitNow;

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        DataValues.Fill(this, room.Content);
        // The engine does not travel it: it keeps its speed to itself.
        Speed = Velocity;
        Velocity = PixelVelocity.Zero;
        Origin = Position;
    }

    /// <inheritdoc/>
    protected sealed override void BeforeTravel(Room room)
    {
        if (Moving)
            Move(room);
    }

    /// <summary>Moves it for one update. A burst stays where it is.</summary>
    protected virtual void Move(Room room)
    {
    }

    /// <summary>Called for a cue that is not one every Torizo shot knows.</summary>
    protected virtual void OnOwnCue(Room room, string[] cue) =>
        throw new InvalidOperationException($"A {GetType().Name} does not know the cue \"{string.Join(':', cue)}\".");

    /// <inheritdoc/>
    protected sealed override void OnCue(Room room, string cue)
    {
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "harmless":
                _harmless = true;
                break;
            case "harmful":
                _harmless = false;
                break;
            case "stop":
                Moving = false;
                break;
            case "end":
                Leave(room);
                break;
            case "front":
                ZIndex = 1;
                break;
            case "behind":
                ZIndex = -1;
                break;
            case "origin":
                Position = Origin;
                break;
            case "rise":
                Position -= PixelOffset.FromWhole(0, 4);
                break;
            case "scatter":
                Scatter(room, Number(parts[1]), Number(parts[2]), Number(parts[3]), Number(parts[4]));
                break;
            case "smoke":
                if (room.Session.Random.Draw().Part(0, 2) == 3)
                    PlayTimeline(parts[1]);
                break;
            case "sound":
                room.Session.Sounds.Request(parts[1], channel: "enemies");
                break;
            default:
                OnOwnCue(room, parts);
                break;
        }
    }

    /// <summary>Changes it to one of the timelines its data names.</summary>
    protected void ShowTimeline(string name) => PlayTimeline(_timelines.TryGetValue(name, out string? file)
        ? file
        : throw new InvalidOperationException($"The data of a {GetType().Name} names no timeline \"{name}\"."));

    /// <summary>Stops it where it is, harmless, on the timeline of its meeting something.</summary>
    protected void Burst(string timeline)
    {
        _harmless = true;
        Moving = false;
        ShowTimeline(timeline);
    }

    /// <summary>Whether a speed has run away with it: one so fast is taken out of the room.</summary>
    protected static bool IsRunaway(Pixels speed) => speed >= Pixels.FromWhole(16);

    /// <summary>The height at which something bursts on the floor of the block it is in.</summary>
    protected int FloorBurstY => Position.Y.Whole / 16 * 16 + 6;

    /// <summary>A speed slowed by a drag toward rest, which may carry it past rest.</summary>
    protected static Pixels Dragged(Pixels speed, Pixels drag) => speed.IsNegative ? speed + drag : speed - drag;

    /// <summary>Throws it a chance distance from where it is, either way across and up or down.</summary>
    private void Scatter(Room room, int width, int height, int middleX, int middleY)
    {
        RandomDraw signs = room.Session.Random.Draw();
        int across = ChanceDistance(room, width, middleX), down = ChanceDistance(room, height, middleY);
        Position += PixelOffset.FromWhole(signs.Part(0, 1) == 1 ? -across : across, signs.Part(8, 1) == 1 ? -down : down);
    }

    /// <summary>A chance distance within a spread, measured from its middle; a draw short of the middle is drawn again.</summary>
    private static int ChanceDistance(Room room, int span, int middle)
    {
        while (true)
        {
            int distance = (int)(room.Session.Random.Draw().Number % (uint)span) - middle;
            if (distance >= 0)
                return distance;
        }
    }

    private static int Number(string text) => int.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
}
