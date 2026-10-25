namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Cartridge sources of the opening cinematic that only verification reads.</summary>
internal static class IntroCinematicCheckSources
{
    /// <summary>$95:D713, the second Japanese narration font.</summary>
    public const int JapaneseFontTwo = 0x95d713;
    /// <summary>$8C:BEC3, the Mother Brain flashback room's level data.</summary>
    public const int MotherBrainLevelData = 0x8cbec3;
    /// <summary>Bank $8B, which holds the cinematic instruction streams.</summary>
    public const int CinematicCode = 0x8b0000;
}
