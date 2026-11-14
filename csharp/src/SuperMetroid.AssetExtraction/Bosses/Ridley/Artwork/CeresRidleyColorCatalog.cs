using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

/// <summary>
/// The installed Ceres Ridley, shared Norfair Ridley health, and private Baby draw colors, every family kept as installed.
/// </summary>
public sealed class CeresRidleyColorCatalog
{
    /// <summary>The installed start colours: the door's, the container's and the Baby's.</summary>
    private readonly InstalledColourRows start;
    /// <summary>The installed eye-fade shades.</summary>
    private readonly InstalledColourRows eyeFade;
    /// <summary>The installed body-fade shades.</summary>
    private readonly InstalledColourRows bodyFade;
    /// <summary>The installed health bands.</summary>
    private readonly InstalledColourRows health;
    /// <summary>The installed alarm steps.</summary>
    private readonly InstalledColourRows alarm;
    /// <summary>The installed retreat background colours.</summary>
    private readonly InstalledColourRows retreatBg;
    /// <summary>The installed retreat shared colours.</summary>
    private readonly InstalledColourRows retreatShared;
    /// <summary>Each Baby shade's installed colours.</summary>
    private readonly Dictionary<CeresBabyShade, InstalledColourRows> baby;

    /// <summary>Keeps every complete installed colour family.</summary>
    /// <param name="start">The start colours.</param>
    /// <param name="eyeFade">The eye fade's shades.</param>
    /// <param name="bodyFade">The body fade's shades.</param>
    /// <param name="health">The health bands.</param>
    /// <param name="alarm">The alarm steps.</param>
    /// <param name="retreat">The retreat's background and shared colours.</param>
    /// <param name="baby">Each Baby shade's colours.</param>
    /// <exception cref="InvalidDataException">A family is missing a row, a shade or a colour.</exception>
    internal CeresRidleyColorCatalog(RgbColor[] start, RgbColor[][] eyeFade, RgbColor[][] bodyFade, RgbColor[][] health,
        RgbColor[][] alarm, CeresRidleyRetreatColours retreat, CeresBabyShadeColours baby)
    {
        this.start = new InstalledColourRows(start, CeresRidleyColorFormat.StartColorCount, "Ceres Ridley start");
        this.eyeFade = new InstalledColourRows(eyeFade, CeresRidleyColorFormat.EyeFadeShadeCount,
            CeresRidleyColorFormat.EyeFadeColorCount, "Ceres Ridley eye fade");
        this.bodyFade = new InstalledColourRows(bodyFade, CeresRidleyShadeDefinitions.BodyFadeShadeCount,
            CeresRidleyColorFormat.BodyFadeColorCount, "Ceres Ridley body fade");
        this.health = new InstalledColourRows(health, CeresRidleyColorFormat.HealthBandCount,
            CeresRidleyColorFormat.HealthColorCount, "Ceres Ridley health");
        this.alarm = new InstalledColourRows(alarm, CeresRidleyColorFormat.AlarmStepCount,
            CeresRidleyColorFormat.AlarmColorCount, "Ceres Ridley alarm");
        Ensure.NotNull(retreat);
        retreatBg = new InstalledColourRows(retreat.Background, CeresRidleyColorFormat.RetreatBgColorCount, "Ceres Ridley retreat BG");
        retreatShared = new InstalledColourRows(retreat.Shared, CeresRidleyColorFormat.RetreatSharedColorCount,
            "Ceres Ridley retreat shared");
        this.baby = Enum.GetValues<CeresBabyShade>().ToDictionary(shade => shade,
            shade => new InstalledColourRows(baby.Of(shade), CeresRidleyColorFormat.BabyColorCount, $"Ceres Ridley Baby {shade}"));
    }

    /// <summary>One of the fifteen installed colours of Ridley's retreat background.</summary>
    public RgbColor ResolveRetreatBg(int color) => retreatBg.Colour(color);

    /// <summary>One of the eight installed colours the retreat shares with the start's first palette.</summary>
    public RgbColor ResolveRetreatShared(int color) => retreatShared.Colour(color);

    /// <summary>One of the installed start colours.</summary>
    internal RgbColor ResolveStart(int color) => start.Colour(color);

    /// <summary>One installed colour of an eye-fade shade.</summary>
    internal RgbColor ResolveEyeFade(int shade, int color) => eyeFade.Colour(shade, color);

    /// <summary>One installed colour of a body-fade shade.</summary>
    internal RgbColor ResolveBodyFade(int shade, int color) => bodyFade.Colour(shade, color);

    /// <summary>One installed colour of a health band.</summary>
    internal RgbColor ResolveHealth(int band, int color) => health.Colour(band, color);

    /// <summary>One installed colour of an alarm step.</summary>
    internal RgbColor ResolveAlarm(int step, int color) => alarm.Colour(step, color);

    /// <summary>One installed colour of a Baby shade.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The shade is undefined or the colour is outside it.</exception>
    internal RgbColor ResolveBaby(CeresBabyShade shade, int color) => baby.TryGetValue(shade, out InstalledColourRows? colours)
        ? colours.Colour(color)
        : throw new ArgumentOutOfRangeException(nameof(shade), shade, "Undefined Ceres Baby shade.");
}
