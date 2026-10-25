using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The definition data of Mother Brain's rainbow beam: where it leaves her mouth, the lines it may cross and its colour.</summary>
public static class MotherBrainBeamDefinitions
{
    /// <summary>The colour the beam adds on the first update it shows: red 0, green 7, blue 15.</summary>
    public static RgbColor InitialColor => new(0, 7, 15);

    /// <summary>The hue steps of the beam's colour wheel between consecutive cycle colours: the beam shows every other hue.</summary>
    public const int HueStepsPerColor = 2;

    /// <summary>How far right of the head's X the beam's apex is, before wrapping to the 256-pixel line.</summary>
    public const int MouthXOffset = 14;

    /// <summary>How far below the head's Y the beam's apex is.</summary>
    public const int MouthYOffset = 5;

    /// <summary>The first screen line the beam may cross, below the HUD's thirty-two lines; the apex must lie strictly below it.</summary>
    public const int FirstLine = 32;

    /// <summary>The line below the last the beam may cross (exclusive); the apex must lie strictly above it.</summary>
    public const int EndLine = 232;
}
