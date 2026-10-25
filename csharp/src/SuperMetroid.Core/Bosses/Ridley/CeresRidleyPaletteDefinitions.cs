namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The shapes of Ceres Ridley's palettes and the rules that select their rows (#1413). Where each palette sits in the
/// cartridge is the import's knowledge (<c>AssetExtraction/CartridgeTables/RidleySourceDefinitions</c>).
/// </summary>
public static class CeresRidleyPaletteDefinitions
{
    /// <summary>The colours Ceres Ridley's initialization loads: the door and Baby container colours and the Baby's initial colours.</summary>
    public const int StartColorCount = 32;

    /// <summary>The Baby's colour rows: resting, horizontal squish, round and vertical squish, selected by its animation.</summary>
    public const int BabyRowCount = 4;
    /// <summary>The colours of one Baby row, excluding the transparent colour zero.</summary>
    public const int BabyColorCount = 15;

    /// <summary>The eye shades, traversed from the last row to the first and then held at the first.</summary>
    public const int EyeFadeRowCount = 16;
    /// <summary>The eye colours of one eye-fade row.</summary>
    public const int EyeFadeColorCount = 3;

    /// <summary>The black-to-visible body shades, one shown every two Ceres Ridley updates.</summary>
    public const int BodyFadeRowCount = 16;
    /// <summary>The body colours of one body-fade row, leaving the eye colours alone.</summary>
    public const int BodyFadeColorCount = 11;

    /// <summary>The damage shades shared by both Ridley encounters: by health in Norfair, by accepted hits in Ceres.</summary>
    public const int HealthRowCount = 3;
    /// <summary>The colours of one damage shade.</summary>
    public const int HealthColorCount = 14;
    /// <summary>The damage shade Ceres Ridley shows from his fiftieth to his sixty-ninth accepted hit while fighting.</summary>
    public const int HealthMidRow = 0;
    /// <summary>
    /// The damage shade Ceres Ridley shows from his seventieth accepted hit on: the cartridge omits the branch after its
    /// ninety-hit comparison, so the middle shade is never shown in Ceres.
    /// </summary>
    public const int HealthLateRow = 2;

    /// <summary>The self-destruct alarm's EMERGENCY-text shades, one every fourth frame, cycling.</summary>
    public const int AlarmRowCount = 16;
    /// <summary>The text colours of one alarm shade, excluding the transparent colour.</summary>
    public const int AlarmColorCount = 3;

    /// <summary>The getaway backdrop's colours one to fifteen, installed when Ridley's real retreat begins.</summary>
    public const int RetreatBgColorCount = 15;
    /// <summary>The retreat colours shared by the getaway's background and Ridley's sprite palette.</summary>
    public const int RetreatSharedColorCount = 8;

    /// <summary>The getaway's zoom shades, selected by the whole part of the getaway zoom (zero to eight).</summary>
    public const int GetawayZoomRowCount = 9;
    /// <summary>The colours of one getaway zoom shade, leaving colour zero alone.</summary>
    public const int GetawayZoomColorCount = 15;
}
