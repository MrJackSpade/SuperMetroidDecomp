using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Mother Brain's installed body, leg and corpse death fades, every frame kept as installed.</summary>
public sealed class MotherBrainDeathColorCatalog
{
    /// <summary>The installed body fade.</summary>
    private readonly InstalledColourRows bodyFade;
    /// <summary>The installed rear-leg fade.</summary>
    private readonly InstalledColourRows legFade;
    /// <summary>The installed detached-brain corpse fade.</summary>
    private readonly InstalledColourRows corpseFade;

    /// <summary>Keeps the complete installed fades.</summary>
    /// <param name="bodyFade">Each body fade frame's colours.</param>
    /// <param name="legFade">Each leg fade frame's colours.</param>
    /// <param name="corpseFade">Each corpse fade frame's colours.</param>
    /// <exception cref="InvalidDataException">A fade is missing a frame or a colour.</exception>
    internal MotherBrainDeathColorCatalog(RgbColor[][] bodyFade, RgbColor[][] legFade, RgbColor[][] corpseFade)
    {
        this.bodyFade = new InstalledColourRows(bodyFade, MotherBrainDeathColorFile.BodyFadeFrameCount,
            MotherBrainDeathColorFormat.BodyColorCount, "Mother Brain death body fade");
        this.legFade = new InstalledColourRows(legFade, MotherBrainDeathColorFile.BodyFadeFrameCount,
            MotherBrainDeathColorFormat.BodyColorCount, "Mother Brain death leg fade");
        this.corpseFade = new InstalledColourRows(corpseFade, MotherBrainDeathColorFile.CorpseFadeFrameCount,
            MotherBrainDeathColorFormat.CorpseColorCount, "Mother Brain death corpse fade");
    }

    /// <summary>The body fade's frames.</summary>
    internal int BodyFadeFrameCount => bodyFade.RowCount;

    /// <summary>The corpse fade's frames.</summary>
    internal int CorpseFadeFrameCount => corpseFade.RowCount;

    /// <summary>One installed colour of a body fade frame.</summary>
    public RgbColor BodyColor(int frame, int color) => bodyFade.Colour(frame, color);

    /// <summary>One installed colour of a rear-leg fade frame.</summary>
    public RgbColor LegColor(int frame, int color) => legFade.Colour(frame, color);

    /// <summary>One installed colour of a corpse fade frame.</summary>
    public RgbColor CorpseColor(int frame, int color) => corpseFade.Colour(frame, color);
}
