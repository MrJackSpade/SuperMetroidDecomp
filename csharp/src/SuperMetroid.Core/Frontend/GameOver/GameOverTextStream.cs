namespace SuperMetroid.Core.Frontend.GameOver;

/// <summary>One text element's bank-$81 command stream and the page cell it starts on.</summary>
/// <param name="Element">The screen element the stream draws.</param>
/// <param name="Origin">The page cell of the element's first tile.</param>
/// <param name="Description">Human-readable text-element identity for extraction diagnostics, not the encoded tile-word payload.</param>
public readonly record struct GameOverTextStream(
    GameOverTextElement Element,
    Position16<TilemapCells> Origin,
    string Description);
