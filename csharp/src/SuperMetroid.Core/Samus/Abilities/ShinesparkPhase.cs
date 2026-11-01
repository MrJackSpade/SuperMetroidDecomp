namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Host names for the installed shinespark movement-handler pointers.</summary>
public enum ShinesparkPhase
{
    /// <summary>No shinespark special movement handler is installed; independent palette timers, boost state, and released echo storage may still remain live.</summary>
    Inactive,
    /// <summary>Stored shine admitted by <c>InitializeSamusPose_CrouchingTransition</c>, with a 180-update palette timer; this is not an installed special movement handler.</summary>
    Stored,
    /// <summary><c>SamusMovementHandler_ShinesparkWindup</c>; waits up to 30 movement updates for a direction, defaulting to a vertical launch on countdown expiry.</summary>
    Windup,
    /// <summary><c>SamusMovementHandler_HorizontalShinespark</c>; moves along the facing direction until collision or the sustaining-energy check starts the crash sequence.</summary>
    Horizontal,
    /// <summary><c>SamusMovementHandler_VerticalShinespark</c>; accelerates upward with split 16.16 motion until collision or insufficient sustaining energy.</summary>
    Vertical,
    /// <summary><c>SamusMovementHandler_DiagonalShinespark</c>; runs horizontal then upward movement, retaining the last axis's collision result rather than combining the two.</summary>
    Diagonal,
    /// <summary><c>SamusMovementHandler_ShinesparkCrash_EchoesCircleSamus</c>; expands, rotates, then contracts the paired speed echoes around Samus while maintaining the spark palette timer.</summary>
    Crash,
    /// <summary><c>ShinesparkCrash_EchoesFinishedCirclingSamus</c>; despite the host name, this phase waits 30 updates after the orbit contracts rather than continuing circular motion.</summary>
    CrashEchoCircle,
    /// <summary><c>ShinesparkCrash_Finish</c>; releases crash echoes into fixed projectile slots three/four as capacity permits, expires spark state, and publishes the standing-pose completion request.</summary>
    CrashFinish,
}
