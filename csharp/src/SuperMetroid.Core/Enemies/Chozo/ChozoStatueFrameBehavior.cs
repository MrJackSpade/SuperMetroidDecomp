namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The frame behaviours the two retail Chozo-statue variants run.
/// Its values are arbitrary.
/// </summary>
public enum ChozoStatueFrameBehavior
{
    /// <summary>The initializer's frame behaviour, which does nothing.</summary>
    Idle = 1,

    /// <summary>The cleared frame behaviour, which does nothing.</summary>
    Cleared,

    /// <summary>Waits for Lower Norfair's hand-trigger room object.</summary>
    WaitForLowerNorfairHandTrigger,

    /// <summary>Waits for Phantoon's defeat and the Wrecked Ship hand trigger.</summary>
    WaitForWreckedShipHandTrigger,

    /// <summary>The no-op installed while the Wrecked Ship statue walks.</summary>
    Walking,
}
