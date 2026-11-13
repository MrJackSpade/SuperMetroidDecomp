
namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// Immutable control words and byte operands for the Landing Site's lightning program.
/// </summary>
/// <remarks>
/// Its colour words remain presentation data; timer setup, timing, branches, and targets are compiled.
/// </remarks>
public static class CrateriaLightningPaletteFxColorDefinitions
{
    /// <summary>The Landing Site program whose flash colors light eight of the background's colours.</summary>
    public static CrateriaLightningPaletteFxColorDefinition Surface { get; } =
        new()
        {
            ColorsPerFrame = 8,
            NeutralCount = 1,
            ColorFrames = new(FirstRecord: 344, FrameCount: 12 + 1, ColorsPerFrame: 8),
        };
}
