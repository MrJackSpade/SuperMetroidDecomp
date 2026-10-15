namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The twelve bank-$84 draw lists selected by the reachable Draygon cannon PLM programs.</summary>
internal enum DraygonCannonDraw : ushort
{
    /// <summary>Right shield frame A at $84:9FCD.</summary>
    RightShieldA = 0x9fcd,
    /// <summary>Right shield frame B at $84:9FDD.</summary>
    RightShieldB = 0x9fdd,
    /// <summary>Right damaged frame A at $84:A02D.</summary>
    RightDamagedA = 0xa02d,
    /// <summary>Right damaged frame B at $84:A03D.</summary>
    RightDamagedB = 0xa03d,
    /// <summary>Right damaged frame C at $84:A04D.</summary>
    RightDamagedC = 0xa04d,
    /// <summary>Right damaged frame D at $84:A05D.</summary>
    RightDamagedD = 0xa05d,
    /// <summary>Left shield frame A at $84:A0ED.</summary>
    LeftShieldA = 0xa0ed,
    /// <summary>Left shield frame B at $84:A101.</summary>
    LeftShieldB = 0xa101,
    /// <summary>Left damaged frame A at $84:A165.</summary>
    LeftDamagedA = 0xa165,
    /// <summary>Left damaged frame B at $84:A179.</summary>
    LeftDamagedB = 0xa179,
    /// <summary>Left damaged frame C at $84:A18D.</summary>
    LeftDamagedC = 0xa18d,
    /// <summary>Left damaged frame D at $84:A1A1.</summary>
    LeftDamagedD = 0xa1a1,
}
