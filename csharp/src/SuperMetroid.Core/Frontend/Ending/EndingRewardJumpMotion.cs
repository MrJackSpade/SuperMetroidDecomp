namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Where a reward jump's actor stands in its flight (#1903, #2541): Samus's body stands until her jump, rises, falls back
/// and lands; a suited jump's head stands until it flies ahead of her and only rises.
/// </summary>
internal enum EndingRewardJumpMotion
{
    /// <summary>Standing until the jump starts.</summary>
    Standing,

    /// <summary>Rising on the jump until she passes above the screen.</summary>
    Rising,

    /// <summary>Falling back until she lands.</summary>
    Falling,

    /// <summary>Landed, where she stays and shoots.</summary>
    Landed,
}
