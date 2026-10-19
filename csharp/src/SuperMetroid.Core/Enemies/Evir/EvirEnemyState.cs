using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// The state of one Evir actor (also called Mini-Draygon in older source annotations). The cartridge builds each
/// creature from three consecutive enemies: body, arms, and a reusable aimed projectile; each holds one of these.
/// The facing, function, movement countdown and regeneration offset are values the cartridge keeps in the enemy's
/// common record, so they are emptied with that record when the enemy is killed (<see cref="ClearWithRecord"/>);
/// the rest outlive the kill.
/// </summary>
public sealed class EvirEnemyState
{
    /// <summary>The side the body faces; the arms and spit copy their body's facing when they position themselves.</summary>
    public EvirFacing Facing { get; internal set; }

    /// <summary>The per-update behaviour this actor runs; none once a kill has emptied the actor's record.</summary>
    public EvirAiFunction? Function { get; internal set; }

    /// <summary>
    /// The body's bobbing countdown, decremented every update and expiring when it wraps below zero, which reverses
    /// the bobbing and restarts it at the body's direction-change period.
    /// </summary>
    public NativeCountdown MovementTimer { get; internal set; }

    /// <summary>
    /// The regenerating spit's horizontal offset from the body's mouth, in room pixels: eight pixels away from the
    /// body's installed facing when regeneration starts, then one pixel back towards the mouth per animation step.
    /// </summary>
    public Offset16<RoomPixels> RegenerationOffset { get; internal set; }

    /// <summary>Whether the body is bobbing downward, with <see cref="DownVelocity"/>, rather than upward with <see cref="UpVelocity"/>; reversed each time <see cref="MovementTimer"/> expires.</summary>
    public bool MovingDown { get; internal set; }
    /// <summary>The last installed animation, not its advancing cursor; none forces installation of the requested one.</summary>
    internal EvirProgram? InstalledProgram { get; set; }
    /// <summary>The animation requested by the actor's current facing or projectile phase; a changed request restarts the instruction timer at one.</summary>
    internal EvirProgram RequestedProgram { get; set; }
    /// <summary>Where this actor is in its animation.</summary>
    internal AnimationProgramCursor<EvirProgram>? Program { get; set; }
    /// <summary>The body's downward vertical velocity per AI update, from the positive half of its linear-speed record.</summary>
    public SnesSignedSixteenSixteen DownVelocity { get; internal set; }
    /// <summary>The body's upward vertical velocity per AI update, from the negative half of its linear-speed record.</summary>
    public SnesSignedSixteenSixteen UpVelocity { get; internal set; }
    /// <summary>The projectile's velocity per AI update, calculated from its launch angle.</summary>
    public FixedVector<RoomPixels> ProjectileVelocity { get; internal set; }
    /// <summary>Whether the projectile is in flight; flight selects the normal projectile animation.</summary>
    public bool InFlight { get; internal set; }
    /// <summary>Whether the projectile is growing back at the mouth: set when distant flight ends and cleared by the regeneration animation's last step.</summary>
    public bool Regenerating { get; internal set; }

    /// <summary>
    /// Empties the values the cartridge keeps in the enemy's common record, when a kill empties that record: the
    /// facing reads as left, there is no function, and the countdown and regeneration offset are zero. The body's
    /// arms and spit, already scheduled for the update in which it dies, read its facing after this.
    /// </summary>
    internal void ClearWithRecord()
    {
        Facing = EvirFacing.Left;
        Function = null;
        MovementTimer = NativeCountdown.Start(0);
        RegenerationOffset = Offset16<RoomPixels>.Zero;
    }
}
