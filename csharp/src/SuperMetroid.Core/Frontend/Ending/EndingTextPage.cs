using System.Collections.Immutable;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The post-credits staging page (#1467): the text the scenes have written, as named pictures placed at cells in the order
/// they were written; a page shows the staging page as it was when last shown.
/// </summary>
internal sealed class EndingTextPage
{
    /// <summary>The written text, earliest first.</summary>
    private readonly List<ShownEndingText> written = [];

    /// <summary>The written text, earliest first.</summary>
    internal ImmutableArray<ShownEndingText> Written => [.. written];

    /// <summary>Clears the page.</summary>
    internal void Clear() => written.Clear();

    /// <summary>Writes the picture <paramref name="picture"/> with its upper-left corner in <paramref name="cell"/>.</summary>
    internal void Write(string picture, Position16<TilemapCells> cell) => written.Add(new ShownEndingText(Ensure.NotNull(picture), cell));

    /// <summary>Clears the picture <paramref name="picture"/> wherever it is written.</summary>
    /// <exception cref="InvalidOperationException">The picture is not written.</exception>
    internal void Erase(string picture)
    {
        if (written.RemoveAll(text => text.Picture == picture) == 0)
            throw new InvalidOperationException($"The ending text page holds no {picture}.");
    }

    /// <summary>Clears the picture <paramref name="picture"/> if it is written, as clearing its rows leaves a page without it blank there.</summary>
    internal void EraseIfWritten(string picture) => written.RemoveAll(text => text.Picture == picture);
}
