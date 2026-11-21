namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Where the X-ray scope stands (#2560): off, or live with each reachable combination of whether it holds Samus's pose
/// input and movement and whether time is frozen under it. Automatic reserve recovery takes the hold and lets time run
/// apart from the beam, which is how the game's G-Mode states arise; each is its own member.
/// </summary>
public enum XrayScopeMode
{
    /// <summary>No X-ray scope is live.</summary>
    Off,

    /// <summary>
    /// The beam is live and time is frozen, but X-ray does not hold Samus: from the setup until the pose update commits
    /// her X-ray pose, for good when a higher-priority pose supersedes it (X-Mode), and once reserve recovery takes the hold.
    /// </summary>
    BeamFreezesTime,

    /// <summary>The beam is live, time is frozen and X-ray holds Samus's pose input and movement: the scope in use.</summary>
    HoldsSamus,

    /// <summary>
    /// X-ray holds Samus but time runs: reserve recovery under Mother Brain's rainbow-beam hold, which keeps X-ray's hold,
    /// let time run again.
    /// </summary>
    HoldsSamusTimeRunning,

    /// <summary>
    /// Reserve Mode: reserve recovery took X-ray's hold and let time run under the still-live finishing beam, which then
    /// cannot end itself (direct G-Mode, the greyout).
    /// </summary>
    ReserveMode,
}
