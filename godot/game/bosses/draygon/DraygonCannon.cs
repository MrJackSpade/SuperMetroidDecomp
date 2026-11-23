using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of the cannons in the walls of Draygon's chamber. Behind its shield only what its data lets through harms it:
/// a few missiles, or one super missile. Draygon has it fire at Samus. Broken, it spits sparks: it hurts to touch,
/// and a grapple beam takes hold of it and drains her for as long as she holds on, which is Draygon's undoing.
/// </summary>
/// <remarks>
/// It plays "shield_left" or "shield_right", by the way it faces, "hit_flash_left" or "hit_flash_right", whose cue
/// "shield" puts its shield back, and "sparks_left" or "sparks_right". Its hitbox "Sparks" is what hurts to touch once
/// it is broken. Its shot is "shot".
/// </remarks>
public partial class DraygonCannon : Enemy, ILatchable
{
    /// <summary>Which of Draygon's turns fires it, from nothing up; one with no turn, below nothing, never fires.</summary>
    [Export]
    public int Slot { get; set; } = -1;

    /// <summary>Whether it is already broken when Samus arrives.</summary>
    [Export]
    public bool StartsBroken { get; set; }

    /// <summary>Where its shots appear, from its place, in pixels.</summary>
    [Export]
    public Vector2I Muzzle { get; set; }

    /// <summary>How much energy it drains each update from Samus while her grapple beam holds it, in hundredths.</summary>
    [Export, DataValue("grapple_drain")]
    public int GrappleDrain { get; set; } = 100;

    private bool _latched;

    /// <summary>Whether it is broken.</summary>
    public bool IsBroken { get; private set; }

    /// <inheritdoc/>
    public bool CanBeLatched => IsBroken;

    /// <inheritdoc/>
    public PixelPosition LatchPoint => Position;

    /// <inheritdoc/>
    public void Latched() => _latched = true;

    /// <inheritdoc/>
    public void Unlatched() => _latched = false;

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => !IsBroken && base.IsHitBy(side);

    /// <summary>Fires at Samus, if it is whole.</summary>
    public void FireAtSamus()
    {
        if (!IsBroken)
            FireAtPlayer("shot", Position + PixelOffset.FromWhole(Muzzle.X, Muzzle.Y));
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        AlwaysActive = true;
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
