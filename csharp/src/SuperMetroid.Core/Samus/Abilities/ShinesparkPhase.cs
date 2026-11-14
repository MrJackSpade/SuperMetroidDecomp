namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The shinespark's movement phases, one per movement handler.</summary>
public enum ShinesparkPhase
{
    /// <summary>No shinespark special movement handler is installed; independent palette timers, boost state, and released echo storage may still remain live.</summary>
    Inactive,
    /// <summary>Stored shine admitted by crouching at full boost, with a 180-update palette timer; this is not an installed special movement handler.</summary>
    Stored,
    /// <summary>The windup; waits up to 30 movement updates for a direction, defaulting to a vertical launch on countdown expiry.</summary>
    Windup,
    /// <summary>The horizontal spark; moves along the facing direction until collision or the sustaining-energy check starts the crash sequence.</summary>
    Horizontal,
    /// <summary>The vertical spark; accelerates upward with split 1/65536-pixel motion until collision or insufficient sustaining energy.</summary>
    Vertical,
    /// <summary>The diagonal spark; runs horizontal then upward movement, retaining the last axis's collision result rather than combining the two.</summary>
    Diagonal,
    /// <summary>The crash; expands, rotates, then contracts the paired speed echoes around Samus while maintaining the spark palette timer.</summary>
    Crash,
    /// <summary>The crash after the echoes finish circling; despite the member name, this phase waits 30 updates after the orbit contracts rather than continuing circular motion.</summary>
    CrashEchoCircle,
    /// <summary>The crash's end; releases its echoes on Samus's shots as her shot count leaves room, expires spark state, and publishes the standing-pose completion request.</summary>
    CrashFinish,
}
