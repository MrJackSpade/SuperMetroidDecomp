namespace SuperMetroid.Core.Assets;

/// <summary>Defines the complete ordered set of opening portrait eye frames.</summary>
public sealed record IntroEyeTilemapDocument
{
    /// <summary>Gets the document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets exactly four frames in blink-selector order.</summary>
    public required IntroEyeTilemapFrame[] Frames { get; init; }
}
