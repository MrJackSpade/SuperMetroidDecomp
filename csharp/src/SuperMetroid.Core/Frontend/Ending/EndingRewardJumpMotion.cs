namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>How a reward jump's actor moves (#1903): Samus's body rises and falls back, and a suited jump's head only rises.</summary>
internal enum EndingRewardJumpMotion
{
    /// <summary>Rising on the jump until she passes above the screen.</summary>
    Rising,

    /// <summary>Falling back until she lands.</summary>
    Falling,
}
