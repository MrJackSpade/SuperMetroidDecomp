namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// What a Chozo statue waits for each frame, when it waits for anything; a statue that waits for nothing has none.
/// Its values are arbitrary.
/// </summary>
public enum ChozoStatueFrameBehavior
{
    /// <summary>Waits for Lower Norfair's hand-trigger room object.</summary>
    WaitForLowerNorfairHandTrigger,

    /// <summary>Waits for Phantoon's defeat and the Wrecked Ship hand trigger.</summary>
    WaitForWreckedShipHandTrigger,
}
