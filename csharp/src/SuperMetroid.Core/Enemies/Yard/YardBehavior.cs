namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// Yard's behavior. The game uses
/// only these six values and compares them for look-at, solidity, touch and airborne decisions.
/// </summary>
public enum YardBehavior
{
    /// <summary>Zero: normal crawling.</summary>
    NormalCrawling = 0,

    /// <summary>One: aggressive crawling after a dropped Yard lands.</summary>
    AggressiveCrawling = 1,

    /// <summary>Two: hiding in its shell.</summary>
    Hiding = 2,

    /// <summary>Three: dropped and falling.</summary>
    Dropped = 3,

    /// <summary>Four: kicked by Samus.</summary>
    Kicked = 4,

    /// <summary>Five: launched by a beam.</summary>
    BeamLaunched = 5,
}
