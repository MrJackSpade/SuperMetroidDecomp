namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The rainbow's fixed sequence length the game steps through; colour content is editable and the import's (#1527).</summary>
/// <remarks>
/// The count stays in Core (#2471): her rainbow's images are made of its frames (<see cref="MotherBrainRainbowImage"/>), as
/// well as the import reading it.
/// </remarks>
public static class MotherBrainRainbowColourFormat
{
    /// <summary>The rainbow colour cycle's ten frames.</summary>
    public const int RainbowFrameCount = 10;
}
