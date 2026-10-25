namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Authoring schema for the timer's palette, digit placement and eleven named sprite compositions; its dictionary and array contents remain caller-mutable.</summary>
public sealed record EscapeTimerPresentationDocument
{
    /// <summary>Schema revision; loading requires <see cref="EscapeTimerPresentationDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Signed horizontal pixel displacement from tens to ones in every digit pair, from -256 through 255; stock value is 8.</summary>
    public required int DigitSpacing { get; init; }
    /// <summary>Exactly Label, Minutes, Seconds and Centiseconds pixel offsets relative to the live timer origin; X is -256..255 and Y is -224..223.</summary>
    public required Dictionary<string, Offset16<SpritePixels>> Anchors { get; init; }
}
