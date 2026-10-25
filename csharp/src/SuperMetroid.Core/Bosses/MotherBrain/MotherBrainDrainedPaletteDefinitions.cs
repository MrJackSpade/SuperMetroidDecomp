namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The shape of Mother Brain's drained-to-grey and revival colour fades.</summary>
public static class MotherBrainDrainedPaletteDefinitions
{
    /// <summary>The body and brain colours each drained-to-grey image rewrites.</summary>
    public const int DrainedColors = 15;

    /// <summary>The body and brain colours each revival image rewrites.</summary>
    public const int RevivalColors = 13;

    /// <summary>The rear-leg colours each drained or revival image rewrites after the body and brain colours.</summary>
    public const int BackLegCount = 5;

    /// <summary>The rainbow rear-leg colour the fades' first rear-leg colour repeats: the fades start three inks into the rainbow's rear-leg set.</summary>
    public const int RearSourceColor = 3;

    /// <summary>The rainbow rear-leg colour the drained image's trailing colour repeats.</summary>
    public const int TrailingRearSourceColor = 13;

    /// <summary>The revival image whose lower-face tissue shade holds the preceding image's shade for one step.</summary>
    public const int RevivalTissueHoldFrame = 2;

    /// <summary>The body tissue colour (zero-based, ten) that holds its preceding shade on <see cref="RevivalTissueHoldFrame"/>; it paints the lower face and neck.</summary>
    public const int RevivalHeldTissueColor = 10;
}
