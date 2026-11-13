using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// The state of one Evir actor (also called Mini-Draygon in older source annotations). The game builds each creature
/// from three consecutive enemies: body, arms, and a reusable aimed projectile; each holds one of these.
/// </summary>
public sealed class EvirEnemyState
{
    /// <summary>The side the body faces; the arms and spit copy their body's facing when they position themselves.</summary>
    public EvirFacing Facing { get; internal set; }

    /// <summary>The per-update behaviour this actor runs.</summary>
    public EvirAiFunction? Function { get; internal set; }

    /// <summary>
    /// The body's bobbing countdown, decremented every update and expiring when it steps below zero, which reverses
    /// the bobbing and restarts it at the body's direction-change period.
    /// </summary>
    public int MovementTimer { get; internal set; }

    /// <summary>
    /// The regenerating spit's horizontal offset from the body's mouth, in room pixels: eight pixels away from the
    /// body's installed facing when regeneration starts, then one pixel back towards the mouth per animation step.
    /// </summary>
    public Offset16<RoomPixels> RegenerationOffset { get; internal set; }

    /// <summary>Whether the body is bobbing downward, with <see cref="DownVelocity"/>, rather than upward with <see cref="UpVelocity"/>; reversed each time <see cref="MovementTimer"/> expires.</summary>
    public bool MovingDown { get; internal set; }
    /// <summary>The last installed animation, not its advancing cursor; none forces installation of the requested one.</summary>
    internal EvirProgram? InstalledProgram { get; set; }
    /// <summary>The animation requested by the actor's current facing or projectile phase; a changed request restarts the frame timer at one.</summary>
    internal EvirProgram RequestedProgram { get; set; }
    /// <summary>The animation this actor plays and where it stands in it (#1600), once one is installed.</summary>
    internal AnimationPlayback<EvirProgram>? Playback { get; set; }
    /// <summary>The body's downward vertical velocity per AI update, from the positive half of its linear-speed record.</summary>
    public SignedSixteenSixteen DownVelocity { get; internal set; }
    /// <summary>The body's upward vertical velocity per AI update, from the negative half of its linear-speed record.</summary>
    public SignedSixteenSixteen UpVelocity { get; internal set; }
    /// <summary>The projectile's velocity per AI update, calculated from its launch angle.</summary>
    public FixedVector<RoomPixels> ProjectileVelocity { get; internal set; }
    /// <summary>Whether the projectile is in flight; flight selects the normal projectile animation.</summary>
    public bool InFlight { get; internal set; }
    /// <summary>Whether the projectile is growing back at the mouth: set when distant flight ends and cleared by the regeneration animation's last step.</summary>
    public bool Regenerating { get; internal set; }
}
