namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Selected visual warning message consumed by the shared escape typewriter, without stored script steps or editable timing.</summary>
/// <param name="Id">Ceres or Zebes identity used to rebind content to saved playback state.</param>
/// <param name="Lines">Ordered text and where each line starts; loaded catalogs expose a read-only compiled sequence, while this record constructor itself does not copy or validate a supplied list.</param>
public sealed record EscapeTypewriterMessage(
    EscapeTypewriterMessageId Id,
    IReadOnlyList<EscapeTypewriterLine> Lines);
