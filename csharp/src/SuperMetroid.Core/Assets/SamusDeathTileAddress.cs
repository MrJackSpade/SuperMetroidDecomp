namespace SuperMetroid.Core.Assets;

/// <summary>Bank-$9B tile addresses with a named relation inside the five uploaded Samus death pages.</summary>
internal enum SamusDeathTileAddress
{
    /// <summary>$9B:8900: unused transparent padding tile.</summary>
    TransparentPadding0 = 0x9b8900,

    /// <summary>$9B:8A60: unused transparent padding tile.</summary>
    TransparentPadding1 = 0x9b8a60,

    /// <summary>$9B:8B60: unused transparent padding tile.</summary>
    TransparentPadding2 = 0x9b8b60,

    /// <summary>$9B:8D20: unused transparent padding tile.</summary>
    TransparentPadding3 = 0x9b8d20,

    /// <summary>$9B:9240: unused transparent padding tile.</summary>
    TransparentPadding4 = 0x9b9240,

    /// <summary>$9B:92E0: unused transparent padding tile.</summary>
    TransparentPadding5 = 0x9b92e0,

    /// <summary>$9B:8F60: phase-seven hair tip at (6,-28), a repeat of <see cref="HairTipSource"/>.</summary>
    RepeatedHairTip = 0x9b8f60,

    /// <summary>$9B:8CC0: the phase-six upper-right OBJ quadrant that the phase-seven hair tip repeats.</summary>
    HairTipSource = 0x9b8cc0,

    /// <summary>$9B:9080: outer arm at (-10,-12) shared by late phases six and eight, a repeat of <see cref="ArmEdgeSource"/>.</summary>
    RepeatedArmEdge = 0x9b9080,

    /// <summary>$9B:8F80: the outer-arm tile that <see cref="RepeatedArmEdge"/> repeats.</summary>
    ArmEdgeSource = 0x9b8f80,

    /// <summary>$9B:93A0: torso patch at (-2,-12), the phase-eight lower-left OBJ quadrant, a repeat of <see cref="TorsoSource"/>.</summary>
    RepeatedTorso = 0x9b93a0,

    /// <summary>$9B:90A0: the phase-six upper-left torso tile that <see cref="RepeatedTorso"/> repeats.</summary>
    TorsoSource = 0x9b90a0,
}
