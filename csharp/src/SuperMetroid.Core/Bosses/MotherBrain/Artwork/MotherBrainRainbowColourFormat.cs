namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The rainbow's fixed sequence length the game steps through; colour content is editable and the import's (#1527).</summary>
public static class MotherBrainRainbowColourFormat
{
    /// <summary>The rainbow colour cycle's ten frames.</summary>
    public const int RainbowFrameCount = 10;

    /// <summary>
    /// The frame her second phase's death colours begin at (#2470): the cycle's last four frames are those colours, which
    /// the drained beam leaves her showing as it runs out; the six before them are the beam's own.
    /// </summary>
    public const int SecondPhaseDeathFrame = 6;
}
