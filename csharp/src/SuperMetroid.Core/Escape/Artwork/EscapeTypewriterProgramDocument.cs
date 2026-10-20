namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Editable ordered lines for one escape warning; the owning dictionary supplies its scenario identity.</summary>
public sealed record EscapeTypewriterProgramDocument
{
    /// <summary>Nonempty caller-owned mutable line array in typewriter order; stock Ceres has three lines and stock Zebes two, but authored line counts may differ.</summary>
    public required EscapeTypewriterLineDocument[] Lines { get; init; }
}
