namespace SuperMetroid.Core.Game;

/// <summary>Five immutable RGB5 sources selected by Kraid's palette handlers.</summary>
public enum KraidPaletteSource
{
    RoomBackdrop,
    InitialTarget,
    Health,
    Secondary,
    DeathArm,
}

/// <summary>Native bank-$A7 Kraid palette addresses and exact source lengths.</summary>
public static class KraidPaletteRomData
{
    /// <summary>$A7:AAA6, initial target colors staged at CGRAM palette eleven.</summary>
    public const int InitialTargetPaletteWords = 0xa7aaa6;

    /// <summary>Nine sixteen-color health bands, including the flash band.</summary>
    public const int HealthBandCount = 9;

    /// <summary>Colors in one native Kraid palette band.</summary>
    public const int BandColors = 16;

    public static int SourceAddress(KraidPaletteSource source) => source switch
    {
        KraidPaletteSource.RoomBackdrop => EnemyRomTablePointers.Kraid.RoomBackgroundPaletteWords,
        KraidPaletteSource.InitialTarget => InitialTargetPaletteWords,
        KraidPaletteSource.Health => EnemyRomTablePointers.Kraid.HealthPaletteWords,
        KraidPaletteSource.Secondary => EnemyRomTablePointers.Kraid.SecondaryPaletteWords,
        KraidPaletteSource.DeathArm => EnemyRomTablePointers.Kraid.DeathArmPaletteWords,
        _ => throw new ArgumentOutOfRangeException(nameof(source)),
    };

    public static int ColorCount(KraidPaletteSource source) => source switch
    {
        KraidPaletteSource.RoomBackdrop or KraidPaletteSource.InitialTarget or
            KraidPaletteSource.DeathArm => BandColors,
        KraidPaletteSource.Health or KraidPaletteSource.Secondary =>
            HealthBandCount * BandColors,
        _ => throw new ArgumentOutOfRangeException(nameof(source)),
    };
}
