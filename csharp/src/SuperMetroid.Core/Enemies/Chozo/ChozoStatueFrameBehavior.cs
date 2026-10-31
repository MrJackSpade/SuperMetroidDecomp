namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The pre-instruction routines the two retail Chozo-statue variants run.
/// Its values are arbitrary.
/// </summary>
public enum ChozoStatueFrameBehavior
{
    /// <summary>The initializer's frame behaviour, which does nothing.</summary>
    Idle = 1,

    /// <summary>The cleared frame behaviour, which does nothing.</summary>
    Cleared,

    /// <summary>Waits for Lower Norfair's hand-trigger PLM.</summary>
    WaitForLowerNorfairHandTrigger,

    /// <summary>Waits for Phantoon's boss bit and the Wrecked Ship hand trigger.</summary>
    WaitForWreckedShipHandTrigger,

    /// <summary>The no-op installed while the Wrecked Ship statue walks.</summary>
    Walking,
}
