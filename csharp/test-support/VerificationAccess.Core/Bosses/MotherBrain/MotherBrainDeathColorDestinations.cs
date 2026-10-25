namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Where the cartridge writes Mother Brain's death fades in its colour memory: verification's native view of the colour sets.</summary>
internal static class MotherBrainDeathColorDestinations
{
    /// <summary>$A9:AF77 copies the live brain colors beginning at CGRAM entry 145.</summary>
    public const int BrainColors = 145;
    /// <summary>$A9:AF7D copies those fifteen colors to sprite palette seven, skipping color zero.</summary>
    public const int CorpseColors = 241;
    /// <summary>$AD:E9CC background palette two's first opaque color.</summary>
    public const int BodyColors = 65;
    /// <summary>$AD:E9DD back-leg palette three's first opaque color.</summary>
    public const int LegColors = 177;
}
