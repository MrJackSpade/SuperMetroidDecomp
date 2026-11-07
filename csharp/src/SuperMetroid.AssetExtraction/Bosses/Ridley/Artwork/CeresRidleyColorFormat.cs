namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>Filename and schema version of the installed Ridley/Baby RGB5 palette asset.</summary>
public static class CeresRidleyColorFormat
{
    /// <summary>Resource-root JSON filename consumed by the presentation catalog loader.</summary>
    public const string FileName = "ceres-ridley-colors.json";

    /// <summary>Schema version five, requiring the complete startup, fade, health, retreat, alarm and Baby palette families, the Baby's colours named by shade, with no step-ordered Baby field.</summary>
    public const int Version = 5;

    /// <summary>The colours Ceres Ridley's initialization loads: the door and Baby container colours and the Baby's initial colours.</summary>
    public const int StartColorCount = 32;

    /// <summary>The colours of one Baby shade, excluding the transparent colour zero.</summary>
    public const int BabyColorCount = 15;

    /// <summary>The eye shades, traversed from the last to the first and then held at the first.</summary>
    public const int EyeFadeShadeCount = 16;

    /// <summary>The eye colours of one eye-fade shade.</summary>
    public const int EyeFadeColorCount = 3;

    /// <summary>The body colours of one body-fade shade, leaving the eye colours alone.</summary>
    public const int BodyFadeColorCount = 11;

    /// <summary>The health bands shared by both Ridley encounters, each with its own damage shade: entered by health in Norfair, by accepted hits in Ceres.</summary>
    public const int HealthBandCount = 3;

    /// <summary>The colours of one health band's damage shade.</summary>
    public const int HealthColorCount = 14;

    /// <summary>The self-destruct alarm's EMERGENCY-text steps, one every fourth frame, cycling.</summary>
    public const int AlarmStepCount = 16;

    /// <summary>The text colours of one alarm step, excluding the transparent colour.</summary>
    public const int AlarmColorCount = 3;

    /// <summary>The getaway backdrop's colours one to fifteen, installed when Ridley's real retreat begins.</summary>
    public const int RetreatBgColorCount = 15;

    /// <summary>The retreat colours shared by the getaway's background and Ridley's sprite palette.</summary>
    public const int RetreatSharedColorCount = 8;

    /// <summary>The colours of one getaway zoom step, leaving colour zero alone.</summary>
    public const int GetawayZoomColorCount = 15;
}
