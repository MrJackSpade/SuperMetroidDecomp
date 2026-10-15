namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Host names for the installed shinespark movement-handler pointers.</summary>
public enum ShinesparkPhase
{
    /// <summary>No shinespark special movement handler is installed; independent palette timers, boost state, and released echo storage may still remain live.</summary>
    Inactive,
    /// <summary>Stored shine admitted by native <c>InitializeSamusPose_CrouchingTransition</c> at <c>$91:F7B0</c>, with a 180-update palette timer; this is not an installed special movement handler.</summary>
    Stored,
    /// <summary>Native <c>SamusMovementHandler_ShinesparkWindup</c> at <c>$90:D068</c>; waits up to 30 movement updates for a direction, defaulting to a vertical launch on countdown expiry.</summary>
    Windup,
    /// <summary>Native <c>SamusMovementHandler_HorizontalShinespark</c> at <c>$90:D106</c>; moves along the facing direction until collision or the sustaining-energy check starts the crash sequence.</summary>
    Horizontal,
    /// <summary>Native <c>SamusMovementHandler_VerticalShinespark</c> at <c>$90:D0AB</c>; accelerates upward with split 16.16 motion until collision or insufficient sustaining energy.</summary>
    Vertical,
    /// <summary>Native <c>SamusMovementHandler_DiagonalShinespark</c> at <c>$90:D0D7</c>; runs horizontal then upward movement, retaining the last axis's collision result rather than combining the two.</summary>
    Diagonal,
    /// <summary>Native <c>SamusMovementHandler_ShinesparkCrash_EchoesCircleSamus</c> at <c>$90:D346</c>; expands, rotates, then contracts the paired speed echoes around Samus while maintaining the spark palette timer.</summary>
    Crash,
    /// <summary>Native <c>ShinesparkCrash_EchoesFinishedCirclingSamus</c> at <c>$90:D3F3</c>; despite the host name, this phase waits 30 updates after the orbit contracts rather than continuing circular motion.</summary>
    CrashEchoCircle,
    /// <summary>Native <c>ShinesparkCrash_Finish</c> at <c>$90:D40D</c>; releases crash echoes into fixed projectile slots three/four as capacity permits, expires spark state, and publishes the standing-pose completion request.</summary>
    CrashFinish,
}
