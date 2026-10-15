namespace SuperMetroid.Core.Areas.Escape.Artwork;

/// <summary>Authoring schema for the timer's palette, digit placement and eleven named sprite compositions; its dictionary and array contents remain caller-mutable.</summary>
public sealed record EscapeTimerPresentationDocument
{
    /// <summary>Schema revision; loading requires <see cref="EscapeTimerPresentationDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Shared OBJ palette selector, 0 through 7, inherited by parts with a null palette; the cartridge timer uses selector 5.</summary>
    public required int Palette { get; init; }
    /// <summary>Signed horizontal pixel displacement from tens to ones in every digit pair, from -256 through 255; stock value is 8.</summary>
    public required int DigitSpacing { get; init; }
    /// <summary>Exactly Label, Minutes, Seconds and Centiseconds pixel offsets relative to the live timer origin; X is -256..255 and Y is -224..223.</summary>
    public required Dictionary<string, Offset16<OamPixels>> Anchors { get; init; }
    /// <summary>Exactly Label and Digit.0 through Digit.9; each nonnull array contains zero to 128 ordered, nonnull OAM parts. The stock Label also supplies both digit separators.</summary>
    public required Dictionary<string, EscapeTimerVisualPart[]> Frames { get; init; }
}
