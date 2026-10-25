using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Debugger-visible state that the cartridge stores beyond Spore Spawn's common enemy words.
/// Naming these fields keeps the boss's angle, stalk anchor, and death velocity from being
/// hidden in unrelated generic variables while retaining their native 16-bit arithmetic.
/// </summary>
public sealed class SporeSpawnEnemyState
{
    /// <summary>Fade-target colours Spore Spawn has staged and the door's fade has not yet taken (#1405).</summary>
    internal readonly List<SporeSpawnStagedColour> stagedTargetColours = [];

    /// <summary>Binds boss-specific state to its live common enemy slot.</summary>
    internal SporeSpawnEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Live common enemy slot owned by the room enemy system; shared by movement, combat, instruction, and stalk-projectile owners.</summary>
    public RoomEnemySlot Body { get; }
    /// <summary>Current bank-$A5 main-AI pointer, corresponding to native variable A at WRAM $0FA8.</summary>
    public SporeSpawnFunction Function { get; internal set; }
    /// <summary>Where the body's typed program resumes (#641).</summary>
    internal AnimationProgramCursor<SporeSpawnBodyProgram> BodyProgram { get; set; }
    /// <summary>
    /// Fixed stalk interpolation anchor in room pixels: the spawn body's X and a Y 72 pixels
    /// above it; native <c>stalkXOrigin</c>/<c>stalkYOrigin</c> at $7E:7808/$7E:780A.
    /// </summary>
    public Position16<RoomPixels> StalkAnchor { get; internal set; }
    /// <summary>
    /// Original body position in room pixels, the sway centre, retained before the live spawn
    /// is raised 128 pixels; native <c>XOrigin</c>/<c>YOrigin</c> at WRAM $0FAC/$0FAE.
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
    /// Signed sway step per movement update in 256ths of a turn: one from the program, doubled to two below 400
    /// health with its sign kept, and negated by the first damaging hit of an open phase, so it stays within -2..2.
    /// </summary>
    public int AngleDelta { get; internal set; }
    /// <summary>Whether the ceiling spore emitters are stopped: the program sets it during protected phases and clears it, and death clears it.</summary>
    public bool SporeGenerationSuppressed { get; internal set; }
    /// <summary>One-hit guard for an open phase: once set, further hits neither reverse the sway nor close the head until the program clears it.</summary>
    public bool Damaged { get; internal set; }
    /// <summary>Health value last recorded by the guarded damaging-hit palette update, native $7E:8800; used to avoid reloading an unchanged health palette.</summary>
    public int PreviousHealth { get; internal set; }
    /// <summary>Fixed death-drift direction toward (128,624) in 256ths of a turn: zero is right, a quarter turn up, a half turn left and three quarters down.</summary>
    public RotationAngle DeathAngle { get; internal set; }
    /// <summary>
    /// Unsigned 16.16 pixels-per-update death-drift magnitude per axis, combining native death
    /// speed/subspeed words $7E:8010/$8012 (X) and $7E:8014/$8016 (Y); <see cref="DeathAngle"/>
    /// supplies each axis's movement sign.
    /// </summary>
    public FixedVector<RoomPixels> DeathVelocityMagnitude { get; internal set; }
    /// <summary>Whether the live fight's scrolling-finished hook still enforces the arena's minimum camera Y; cleared at death and never installed for an already defeated load.</summary>
    public bool ScrollClampHookActive { get; internal set; }
    /// <summary>Host guard recording that zero health has already triggered projectile cleanup, the defeated flag, death instructions, and the ceiling-crumble request.</summary>
    public bool DeathStarted { get; internal set; }
    /// <summary>Whether the death instruction has published its sixteen boss item-drop attempts; separate from individual destroyed-spore drops and actual drop-pool allocation success.</summary>
    public bool DeathDropRequested { get; internal set; }
    /// <summary>Defeated-area miniboss flag sampled at room initialization; selects the solid dead body and cleared ceiling instead of the live fight setup.</summary>
    public bool LoadedAsDefeated { get; internal set; }

    /// <summary>Stages <paramref name="value"/> as the fade target of <paramref name="run"/>'s colour <paramref name="colour"/>.</summary>
    internal void WriteTargetColor(IColourSetRun run, int colour, RgbColor value) => stagedTargetColours.Add(new(run, colour, value));

    /// <summary>Hands every staged colour to <paramref name="write"/> in staging order, then forgets them.</summary>
    internal void ConsumeTargetColors(Action<SporeSpawnStagedColour> write)
    {
        foreach (SporeSpawnStagedColour staged in stagedTargetColours)
            write(staged);
        stagedTargetColours.Clear();
    }
}
