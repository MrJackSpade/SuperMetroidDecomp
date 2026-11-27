using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Engine.Actors;

/// <summary>
/// A projectile its timelines drive. By itself it is a burst that plays out where it is; it is also the base of
/// those that fly their own way. It moves itself, at its own speed, and meets terrain its own way, so its settings
/// let it through terrain. Its timelines' cues turn its harm on and off and move it about.
/// </summary>
/// <remarks>
/// Its data lists under "timelines" the timelines it changes to, by the names its class uses. The cues it knows are
/// "harmless", "harmful", "shots_pass" and "shots_meet" (from then on the other side's shots pass it by, or meet it
/// again as its settings say), "stays" (from then on it stays through every touch of its target, shot down or
/// not), "stop", "end", "front", "behind", "origin" (back to where it began, or last came to rest),
/// "shift:" with pixels across and down, "scatter:" with the width and height of the spread and how far into it the
/// middle lies each way, "sometimes:" with a timeline it turns to one time in four, and "sound:" with a sound's
/// name.
/// </remarks>
public partial class CuedProjectile : Projectile, IHasValues
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

    /// <summary>Whether the other side's shots pass it by for now, whatever its settings have them do on meeting it.</summary>
    private bool _shotsPass;

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => !_shotsPass && base.IsHitBy(side);

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

    /// <summary>Called for a cue that is not one every such projectile knows.</summary>
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
            case "shots_pass":
                _shotsPass = true;
                break;
            case "shots_meet":
                _shotsPass = false;
                break;
            case "stays":
                OutlastsTouch = true;
                break;
            case "stop":
                Moving = false;
                break;
            case "end":
                Leave(room);
                break;
            case "front":
                ZIndex = RoomDepth.InFrontOfThings;
                break;
            case "behind":
                ZIndex = RoomDepth.BehindThings;
                break;
            case "origin":
                Position = Origin;
                break;
            case "shift":
                Position += PixelOffset.FromWhole(Number(parts[1]), Number(parts[2]));
                break;
            case "scatter":
                Position += room.Session.Random.Scatter(Number(parts[1]), Number(parts[2]), Number(parts[3]), Number(parts[4]));
                break;
            case "sometimes":
                // One time in four: when the number drawn lies in the top quarter of its range, its two highest digits.
                if (room.Session.Random.Draw().Part(14, 2) == 3)
                    PlayTimeline(parts[1]);
                break;
            case "sound":
                // All that follows the cue's name is the sound's, a priority it says after a colon of its own too.
                room.Session.Sounds.Request(cue["sound:".Length..], channel: "enemies");
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

    /// <summary>A speed slowed by a drag toward rest, which may carry it past rest.</summary>
    protected static Pixels Dragged(Pixels speed, Pixels drag) => speed.IsNegative ? speed + drag : speed - drag;

    private static int Number(string text) => int.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
}
