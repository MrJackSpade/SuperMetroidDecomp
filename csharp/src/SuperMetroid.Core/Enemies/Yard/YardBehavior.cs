namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>
/// Yard behavior word at <c>$7E:7810,X</c> (<c>Yard.behavior</c>). Bank <c>$A3</c> stores
/// only these six values and compares them for look-at, solidity, touch and airborne decisions.
/// </summary>
public enum YardBehavior
{
    /// <summary>Zero: normal crawling (<c>$A3:CE1C</c>, <c>$A3:D2D0</c>, <c>$A3:D41C</c>).</summary>
    NormalCrawling = 0,

    /// <summary>One: aggressive crawling after a dropped Yard lands (<c>$A3:D2BA</c>).</summary>
    AggressiveCrawling = 1,

    /// <summary>Two: hiding in its shell (<c>$A3:CF04</c>).</summary>
    Hiding = 2,

    /// <summary>Three: dropped and falling (<c>$A3:D171</c>).</summary>
    Dropped = 3,

    /// <summary>Four: kicked by Samus (<c>$A3:D4A2</c>).</summary>
    Kicked = 4,

    /// <summary>Five: launched by a beam (<c>$A3:D55B</c>).</summary>
    BeamLaunched = 5,
}
