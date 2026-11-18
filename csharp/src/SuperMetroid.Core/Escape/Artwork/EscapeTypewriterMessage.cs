namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Selected visual warning message consumed by the shared escape typewriter, without stored script steps or editable timing.</summary>
/// <param name="Id">Which warning this is, Ceres's or Zebes's: text installed mid-warning must be the same warning's.</param>
/// <param name="Lines">Ordered text and where each line starts, as installed; this record constructor itself does not copy or validate a supplied list.</param>
public sealed record EscapeTypewriterMessage(
    EscapeTypewriterMessageId Id,
    IReadOnlyList<EscapeTypewriterLine> Lines);
