namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The definition data of Mother Brain's rainbow beam: where it leaves her mouth and the lines it may cross. Its colours
/// are installed (#1557).
/// </summary>
public static class MotherBrainBeamDefinitions
{
    /// <summary>How far right of the head's X the beam's apex is, before wrapping to the 256-pixel line.</summary>
    public const int MouthXOffset = 14;

    /// <summary>How far below the head's Y the beam's apex is.</summary>
    public const int MouthYOffset = 5;

    /// <summary>The first screen line the beam may cross, below the HUD's thirty-two lines; the apex must lie strictly below it.</summary>
    public const int FirstLine = 32;

    /// <summary>The line below the last the beam may cross (exclusive); the apex must lie strictly above it.</summary>
    public const int EndLine = 232;
}
