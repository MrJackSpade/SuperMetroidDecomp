namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>One text element's bank-$81 command stream and its BG1 byte destination.</summary>
/// <param name="Element">The screen element the stream draws.</param>
/// <param name="DestinationByteOffset">BG1-page-relative byte offset of the initial tile; row and column are encoded as 64 times Y plus twice X.</param>
/// <param name="Description">Human-readable text-element identity for extraction diagnostics, not the encoded tile-word payload.</param>
public readonly record struct GameOverTextStream(
    GameOverTextElement Element,
    int DestinationByteOffset,
    string Description);
