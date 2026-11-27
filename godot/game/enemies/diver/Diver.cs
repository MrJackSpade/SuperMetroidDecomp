using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A diver: it hangs still until Samus comes within reach to either side, winds up, and dives straight down, drifting
/// toward her, until it meets the ground. There it burrows in, throws up a burst of debris, and is gone. The Skree and
/// the Metaree are both this scene with their own data files.
/// </summary>
/// <remarks>
/// Its timelines are "idle", "windup", whose cue "ready" releases the dive, and "dive". Its shot "debris" is the
/// debris it throws up, and "debris" in its data lists where each piece starts and how fast.
/// </remarks>
public partial class Diver : Enemy
{
    /// <summary>How near across Samus must come, in pixels, to set it off.</summary>
    [Export, DataValue("reach")]
    public int Reach { get; set; } = 48;

    /// <summary>How far it dives each update, in pixels.</summary>
    [Export, DataValue("dive_speed")]
    public int DiveSpeed { get; set; } = 6;

    /// <summary>
    /// For a diver that dives faster the further below it Samus is: how many pixels of that distance add one pixel to
    /// its dive speed. Zero for one that always dives at the same speed.
    /// </summary>
    [Export, DataValue("distance_per_speed")]
    public int DistancePerSpeed { get; set; }

    /// <summary>How far it drifts toward Samus each update of the dive, in pixels.</summary>
    [Export, DataValue("drift")]
    public int Drift { get; set; } = 1;

    /// <summary>How many updates it burrows for before it is gone.</summary>
    [Export, DataValue("burrow_updates")]
    public int BurrowUpdates { get; set; } = 21;

    /// <summary>How many updates before the end of its burrowing the debris is thrown up.</summary>
    [Export, DataValue("debris_at")]
    public int DebrisAt { get; set; } = 8;

    /// <summary>The sound of the dive beginning, by name.</summary>
    [Export, DataValue("dive_sound")]
    public string DiveSound { get; set; } = "";

    /// <summary>The sound of it meeting the ground, by name.</summary>
    [Export, DataValue("burrow_sound")]
    public string BurrowSound { get; set; } = "";

    private enum Doing
    {
        Hanging,
        WindingUp,
        Diving,
        Burrowing,
    }

    private readonly List<(int Side, Pixels Across, Pixels Up)> _debris = [];
    private Doing _doing;
    private int _diveStep;
    private Countdown _burrowing;
    private bool _ready;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _debris.Clear();
        if (!data.Has("debris"))
            return;
        foreach (DataNode piece in data.List("debris"))
            _debris.Add((piece.Int("side"), piece.Distance("across"), piece.Distance("up")));
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom() => Play("idle");

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "ready")
            _ready = true;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.Hanging:
                if ((Target.X.Whole - Position.X.Whole) is var across && Math.Abs(across) >= Reach)
                    return;
                // How fast it will dive is settled as it starts to wind up, from how far below it Samus is then. The
                // cartridge counts that distance within 65536 and never asks which is lower, so a Samus above it reads
                // as far below, and its first step of the dive would pass the room's bottom.
                _diveStep = DistancePerSpeed > 0 ? ((Target.Y.Whole - Position.Y.Whole) & 0xFFFF) / DistancePerSpeed + DiveSpeed : DiveSpeed;
                _ready = false;
                Play("windup");
                _doing = Doing.WindingUp;
                return;

            case Doing.WindingUp:
                if (!_ready)
                    return;
                Play("dive");
                Sound(DiveSound);
                AlwaysActive = true;
                _doing = Doing.Diving;
                return;

            case Doing.Diving:
                Dive();
                return;

            case Doing.Burrowing:
                _burrowing = _burrowing.Stepped();
                if (_burrowing.IsOver)
                {
                    Vanish();
                    return;
                }

                if (_burrowing.Remaining == DebrisAt)
                    ThrowDebris();
                Position += PixelOffset.FromWhole(0, 1);
                return;

            default:
                throw new InvalidOperationException($"Undefined diver stage {_doing}.");
        }
    }

    /// <inheritdoc/>
    protected override void OnDied() => ThrowDebris();

    /// <summary>Dives one update: straight down by whole pixels, a little toward Samus, until the ground is under it.</summary>
    private void Dive()
    {
        Collider body = Body ?? throw new InvalidOperationException($"The diver \"{Name}\" has no body collider.");
        // Like the cartridge's, it looks only at the row its bottom edge would end in, and a slope there is no ground
        // to it: it dives through slopes, and through anything that would stand between here and that row.
        PixelRect moved = body.RoomRect.MovedBy(0, _diveStep);
        var bottomEdge = new PixelRect(moved.Left, moved.Top + moved.Height - 1, moved.Width, 1);
        // Only the room itself is ground to it: its terrain, and what stands as terrain. Another enemy, frozen or not, is none.
        if (!Room.Mover.IsFree(bottomEdge, this, MoveOptions.Plain with { Slopes = SlopeTreatment.Ignore, StoppedBySolids = false }))
        {
            _burrowing = Countdown.Of(BurrowUpdates);
            _doing = Doing.Burrowing;
            Sound(BurrowSound);
            return;
        }

        int toward = Position.X.Whole < Target.X.Whole ? Drift : -Drift;
        Position += PixelOffset.FromWhole(toward, _diveStep);
    }

    /// <summary>
    /// Throws up the debris from its whole pixels: each listed piece, then each one's mirror image on the other side.
    /// With too few places free in the room the first thrown are the ones made.
    /// </summary>
    private void ThrowDebris()
    {
        foreach ((int side, Pixels across, Pixels up) in _debris)
            Fire("debris", PixelPosition.FromWhole(Position.X.Whole + side, Position.Y.Whole), new PixelVelocity(across, -up));
        foreach ((int side, Pixels across, Pixels up) in _debris)
            Fire("debris", PixelPosition.FromWhole(Position.X.Whole - side, Position.Y.Whole), new PixelVelocity(-across, -up));
    }
}
