namespace SuperMetroid.Core.Game;

/// <summary>Native Work Robot palette-color geometry; cadence remains separately compiled.</summary>
public static class WorkRobotPaletteRomData
{
    /// <summary>Four RGB5 color words precede the timer word in each $A8:CCC1 record.</summary>
    public const int ColorCount = 4;

    /// <summary>The graphics-drawn hook writes OBJ palette colors nine through twelve.</summary>
    public const int FirstAnimatedColor = 9;
}
