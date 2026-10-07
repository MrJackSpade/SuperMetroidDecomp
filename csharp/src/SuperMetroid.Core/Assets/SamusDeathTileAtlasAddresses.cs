namespace SuperMetroid.Core.Assets;

/// <summary>Bank-$9B tile addresses with a named relation inside the five uploaded Samus death pages.</summary>
internal static class SamusDeathTileAtlasAddresses
{
    /// <summary>$9B:8900: unused transparent padding tile.</summary>
    internal const int TransparentPadding0 = 0x9b8900;

    /// <summary>$9B:8A60: unused transparent padding tile.</summary>
    internal const int TransparentPadding1 = 0x9b8a60;

    /// <summary>$9B:8B60: unused transparent padding tile.</summary>
    internal const int TransparentPadding2 = 0x9b8b60;

    /// <summary>$9B:8D20: unused transparent padding tile.</summary>
    internal const int TransparentPadding3 = 0x9b8d20;

    /// <summary>$9B:9240: unused transparent padding tile.</summary>
    internal const int TransparentPadding4 = 0x9b9240;

    /// <summary>$9B:92E0: unused transparent padding tile.</summary>
    internal const int TransparentPadding5 = 0x9b92e0;

    /// <summary>$9B:8F60: phase-seven hair tip at (6,-28), a repeat of <see cref="HairTipSource"/>.</summary>
    internal const int RepeatedHairTip = 0x9b8f60;

    /// <summary>$9B:8CC0: the phase-six upper-right OBJ quadrant that the phase-seven hair tip repeats.</summary>
    internal const int HairTipSource = 0x9b8cc0;

    /// <summary>$9B:9080: outer arm at (-10,-12) shared by late phases six and eight, a repeat of <see cref="ArmEdgeSource"/>.</summary>
    internal const int RepeatedArmEdge = 0x9b9080;

    /// <summary>$9B:8F80: the outer-arm tile that <see cref="RepeatedArmEdge"/> repeats.</summary>
    internal const int ArmEdgeSource = 0x9b8f80;

    /// <summary>$9B:93A0: torso patch at (-2,-12), the phase-eight lower-left OBJ quadrant, a repeat of <see cref="TorsoSource"/>.</summary>
    internal const int RepeatedTorso = 0x9b93a0;

    /// <summary>$9B:90A0: the phase-six upper-left torso tile that <see cref="RepeatedTorso"/> repeats.</summary>
    internal const int TorsoSource = 0x9b90a0;
}
