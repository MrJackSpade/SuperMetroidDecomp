using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One Kraid part's own state beside its enemy slot: the body, arm, three lints, foot and two fingernails each have
/// one, in that slot order.
/// </summary>
public sealed class KraidPartState
{
    /// <summary>Where the part's typed program resumes; it replaces the native instruction pointer.</summary>
    internal AnimationProgramCursor<KraidPartProgram> Cursor { get; set; }

    /// <summary>
    /// Gets the function a part's function timer installs when it expires. The cartridge keeps it in the same word as
    /// <see cref="ThinkTimer"/>; no part reads one after writing the other (the foot writes its think timer whenever it
    /// starts a second-phase walk and its next function whenever it starts a timed phase), so each is held on its own.
    /// </summary>
    public KraidAiFunction NextFunction { get; internal set; }

    /// <summary>Gets the second-phase foot's think timer, which expires when its decrement reaches zero.</summary>
    public NativeCountdown ThinkTimer { get; internal set; }

    /// <summary>
    /// Gets whether this fingernail's last launch was the fixed-left spawn rather than the body-side one. A nail
    /// launches body-side whenever its sibling's last launch was fixed-left.
    /// </summary>
    public bool AlternateSpawn { get; internal set; }
}
