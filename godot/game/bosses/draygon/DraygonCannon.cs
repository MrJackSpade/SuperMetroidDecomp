using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of the cannons in the walls of Draygon's chamber. Behind its shield only what its data lets through harms it:
/// a few missiles, or one super missile. Draygon has it fire at Samus. Broken, it spits sparks: it hurts to touch,
/// and a grapple beam takes hold of it and drains her for as long as she holds on, which is Draygon's undoing.
/// </summary>
/// <remarks>
/// It plays "shield_left" or "shield_right", by the way it faces, "hit_flash_left" or "hit_flash_right", whose cue
/// "shield" puts its shield back, and "sparks_left" or "sparks_right". Broken, it hurts whoever stands on it or
/// presses against it. Its data gives under "picture_offsets" where the corner of its pictures lies from its place
/// when it faces "left" and when it faces "right". Its shot is "shot".
/// </remarks>
public partial class DraygonCannon : Enemy, ILatchable
{
    /// <summary>Which of the chamber's cannons it is, as Draygon's data names the cannon of each of its turns.</summary>
    [Export]
    public string Which { get; set; } = "";

    /// <summary>Whether it is already broken when Samus arrives.</summary>
    [Export]
    public bool StartsBroken { get; set; }

    /// <summary>How much energy it drains each update from Samus while her grapple beam holds it, in hundredths.</summary>
    [Export, DataValue("grapple_drain")]
    public int GrappleDrain { get; set; } = 100;

    private readonly Dictionary<Facing, Vector2I> _pictureOffsets = [];
    private bool _latched;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _pictureOffsets.Clear();
        foreach ((Facing way, string name) in new[] { (Facing.Left, "left"), (Facing.Right, "right") })
        {
            DataNode offset = data.Get("picture_offsets").Get(name);
            _pictureOffsets[way] = new Vector2I(offset.Int("x"), offset.Int("y"));
        }
    }

    /// <summary>Whether it is broken.</summary>
    public bool IsBroken { get; private set; }

    /// <inheritdoc/>
    public bool CanBeLatched => IsBroken;

    /// <summary>Whether the line that holds it took hold of the lower of its two blocks.</summary>
    private bool _latchedLower;

    /// <summary>
    /// Broken, it is two spike blocks, one above the other, and a line holds at the middle of whichever of the two
    /// its end reached, as the cartridge's line holds a block.
    /// </summary>
    public PixelPosition LatchPoint
    {
        get
        {
            int block = Room.Terrain.TileSize;
            return PixelPosition.FromWhole(Position.X.Whole + block / 2, Position.Y.Whole + block / 2 + (_latchedLower ? block : 0));
        }
    }

    /// <inheritdoc/>
    public void Latched(PixelPosition at) =>
        (_latched, _latchedLower) = (true, at.Y.Whole >= Position.Y.Whole + Room.Terrain.TileSize);

    /// <inheritdoc/>
    public void Unlatched() => _latched = false;

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => !IsBroken && base.IsHitBy(side);

    /// <summary>Fires at Samus a shot that appears at <paramref name="from"/>, if it is whole.</summary>
    public void FireAtSamusFrom(PixelPosition from)
    {
        if (IsBroken)
            return;

        // Aimed and split along its aim as the cartridge does, at a whole number of pixels an update.
        int aim = CartridgeAngles.AimAngle(Target.X.Whole - from.X.Whole, Target.Y.Whole - from.Y.Whole);
        Pixels speed = Shot("shot").Speed;
        if (speed.Fraction != 0)
            throw new InvalidOperationException($"The Draygon cannon's shot speed {speed} is not a whole number of pixels.");
        Fire("shot", from, CartridgeAngles.VelocityAlong(aim, speed.Whole));
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
        // Its pictures reach out of the wall, to the side it faces.
        if (Visual is { } visual)
            visual.PictureOffset = _pictureOffsets[Facing];
        if (StartsBroken)
            Break();
        else
            PlayFacing("shield");
    }

    /// <inheritdoc/>
    protected override void OnHurt(Hit hit, Collider part) => PlayFacing("hit_flash");

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit)
    {
        Break();
        return false;
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "shield" && !IsBroken)
            PlayFacing("shield");
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (IsBroken)
            base.Touch(part);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (IsBroken && PlayerRestsAgainstIt() && Solid is { } sparking)
            Touch(sparking);
        if (_latched)
            Room.Player.AddOngoingDamage("draygon_cannon", GrappleDrain);
    }

    /// <summary>Breaks it: its shield is gone for good and it spits sparks.</summary>
    private void Break()
    {
        IsBroken = true;
        PlayFacing("sparks");
    }

    /// <summary>Starts the timeline of a name for the way it faces.</summary>
    private void PlayFacing(string timeline) => Play($"{timeline}_{(Facing == Facing.Right ? "right" : "left")}", restart: true);
}
