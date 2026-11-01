namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Named host equivalents for the otherwise opaque drain pose/handler state.</summary>
public enum DrainedSamusPhase
{
    /// <summary>No drained movement/pose lock is owned by this state, either before setup, after release art completes, or after another owner replaces the movement handler.</summary>
    Inactive,
    /// <summary>Samus command five or $18 has installed left-facing knockback pose $54 and locked input via the shared rainbow-beam setup.</summary>
    RainbowBeamLocked,
    /// <summary>Controller zero has selected drained crouching pose $E8/$E9 at animation index two with zero base/Y speed; animation command $F7 has not yet installed falling movement.</summary>
    WaitingForFallingCommand,
    /// <summary>Animation command $F7 has installed the drained falling handler, which advances shared gravity/Y movement and block collision until a solid collision restores normal movement.</summary>
    Falling,
    /// <summary>The falling handler encountered a solid collision, cleared Y speed, and selected animation index seven with an eight-update timer without changing pose or Y direction.</summary>
    OnFloor,
    /// <summary>Controller one has selected facing-dependent standing drained pose $EA/$EB at animation index zero with a sixteen-update timer.</summary>
    Standing,
    /// <summary>Controller four has selected facing-dependent crouching drained pose $E8/$E9 at animation index eight with a sixteen-update timer.</summary>
    Crouching,
    /// <summary>Controller two has started drained release art, refreshed collision radii, and cleared base/Y speed; the drain lock remains until the release animation's $FD command completes.</summary>
    Releasing,
}
