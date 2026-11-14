using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Spore Spawn's state beyond what every enemy has: the boss's swing angle, stalk anchor and
/// death velocity, each named rather than kept in generic enemy values, with their wrapping arithmetic.
/// </summary>
public sealed class SporeSpawnEnemyState
{
    /// <summary>Binds boss-specific state to its live common enemy slot.</summary>
    internal SporeSpawnEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Spore Spawn's body's and his room's colours (#1512).</summary>
    internal SporeSpawnColours Colours { get; } = new();

    /// <summary>Live common enemy slot owned by the room enemy system; shared by movement, combat, animation, and stalk-projectile owners.</summary>
    public RoomEnemySlot Body { get; }
    /// <summary>Current main-AI movement: the phase of the fight it runs each update.</summary>
    public SporeSpawnMovement Movement { get; internal set; }
    /// <summary>The body's animation and where it stands in it (#1600).</summary>
    internal AnimationPlayback<SporeSpawnBodySegment> BodyPlayback { get; set; }
    /// <summary>
    /// Fixed stalk interpolation anchor in room pixels: the spawn body's X and a Y 72 pixels
    /// above it.
    /// </summary>
    public Position16<RoomPixels> StalkAnchor { get; internal set; }
    /// <summary>
    /// Spawn body position in room pixels, the sway centre, retained before the live spawn
    /// is raised 128 pixels; its <c>XOrigin</c>/<c>YOrigin</c>.
    /// </summary>
    public Position16<RoomPixels> MovementCenter { get; internal set; }
    /// <summary>
    /// Sway phase in 256ths of a turn, starting at three quarters; X samples cosine and Y samples the doubled,
    /// shifted phase. The phase wraps around the turn as it advances, which the sway depends on.
    /// </summary>
    public RotationAngle Angle { get; internal set; }
    /// <summary>
    /// Horizontal sway amplitude in room pixels; vertical amplitude is this value minus sixteen. Set to 48 by the
    /// descent and 64 by the fight-start cue, and widened by eight while below forty, so it stays within 0..64.
    /// </summary>
    public int MaximumXRadius { get; internal set; }
    /// <summary>
    /// Signed sway step per movement update in 256ths of a turn: one from the animation, doubled to two below 400
    /// health with its sign kept, and negated by the first damaging hit of an open phase, so it stays within -2..2.
    /// </summary>
    public int AngleDelta { get; internal set; }
    /// <summary>Whether the ceiling spore emitters are stopped: the animation sets it during protected phases and clears it, and death clears it.</summary>
    public bool SporeGenerationSuppressed { get; internal set; }
    /// <summary>One-hit guard for an open phase: once set, further hits neither reverse the sway nor close the head until the animation clears it.</summary>
    public bool Damaged { get; internal set; }
    /// <summary>Health value last recorded by the guarded damaging-hit palette update; used to avoid reloading an unchanged health palette.</summary>
    public int PreviousHealth { get; internal set; }
    /// <summary>Fixed death-drift direction toward (128,624) in 256ths of a turn: zero is right, a quarter turn up, a half turn left and three quarters down.</summary>
    public RotationAngle DeathAngle { get; internal set; }
    /// <summary>
    /// Unsigned 1/65536-pixel-per-update death-drift magnitude per axis, combining the death
    /// speed and subspeed of X and of Y; <see cref="DeathAngle"/>
    /// supplies each axis's movement sign.
    /// </summary>
    public FixedVector<RoomPixels> DeathVelocityMagnitude { get; internal set; }
    /// <summary>Whether the live fight's scrolling-finished hook still enforces the arena's minimum camera Y; cleared at death and never installed for an already defeated load.</summary>
    public bool ScrollClampHookActive { get; internal set; }
    /// <summary>Guard recording that zero health has already triggered projectile cleanup, the defeated flag, the death animation, and the ceiling-crumble request.</summary>
    public bool DeathStarted { get; internal set; }
    /// <summary>Defeated-area miniboss flag sampled at room initialization; selects the solid dead body and cleared ceiling instead of the live fight setup.</summary>
    public bool LoadedAsDefeated { get; internal set; }

}
