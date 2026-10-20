namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Which of Mother Brain's health-tinted OBJ palettes a colour belongs to.</summary>
internal enum MotherBrainPalettePart
{
    /// <summary>The body palette.</summary>
    Body,

    /// <summary>The rear-leg palette, which tints two steps further at every damaged state.</summary>
    BackLeg,
}
