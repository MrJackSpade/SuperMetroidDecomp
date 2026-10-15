namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Defines the text and tilemap placement of one staff-credit line.</summary>
public sealed record CreditsLineDocument
{
    /// <summary>Gets the stable line identifier matched against the native cadence definition.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the text compiled with the line's predefined font style.</summary>
    public required string Text { get; init; }

    /// <summary>Gets the zero-based starting column in the 32-word tilemap row.</summary>
    public required int Column { get; init; }

    /// <summary>Gets the BG palette selector encoded into the compiled tilemap words.</summary>
    public required int Palette { get; init; }
}
