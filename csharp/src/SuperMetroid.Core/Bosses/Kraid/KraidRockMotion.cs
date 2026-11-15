namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// How one of Kraid's rocks moves itself (#1843).
/// </summary>
internal enum KraidRockMotion
{
    /// <summary>Flies one of Kraid's rocks with room collision, drag and gravity.</summary>
    Flying,

    /// <summary>Drops a ceiling rock with vertical room collision and gravity.</summary>
    CeilingRockFalling,

    /// <summary>Switches a shot spat rock to the common explosion colours.</summary>
    ShotRockRecolouring,
}
