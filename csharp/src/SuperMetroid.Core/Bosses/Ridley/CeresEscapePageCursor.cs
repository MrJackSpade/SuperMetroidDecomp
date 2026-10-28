using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Where the Ceres self-destruct stands in one of its two page lists, one page an update (#1406): the pages of the list
/// shown so far.
/// </summary>
/// <param name="List">The list; none for a cursor that was never started.</param>
/// <param name="Shown">The pages of the list shown so far.</param>
internal readonly record struct CeresEscapePageCursor(CeresEscapePageList List, int Shown)
{
    /// <summary>The timer-sprite list from its first page.</summary>
    internal static CeresEscapePageCursor Objects { get; } = new(CeresEscapePageList.Objects, 0);

    /// <summary>The background list from its first page.</summary>
    internal static CeresEscapePageCursor Backgrounds { get; } = new(CeresEscapePageList.Backgrounds, 0);

    /// <summary>Whether the cursor was started on a list.</summary>
    internal bool IsStarted => List != CeresEscapePageList.None;

    /// <summary>Whether every page of the list has been shown.</summary>
    internal bool IsFinished => IsStarted && Shown >= PageCount;

    /// <summary>The pages the list shows.</summary>
    /// <exception cref="InvalidOperationException">The cursor was never started.</exception>
    private int PageCount => List switch
    {
        CeresEscapePageList.Objects => CeresEscapeObjectPages.ObjectPages.Count,
        CeresEscapePageList.Backgrounds => CeresEscapePageDefinitions.BackgroundPageUpdates,
        CeresEscapePageList.None => throw new InvalidOperationException("A Ceres escape page cursor that was never started has no pages."),
        _ => throw new InvalidOperationException($"Undefined {nameof(CeresEscapePageList)} {List}."),
    };

    /// <summary>The cursor after the next page has been shown.</summary>
    internal CeresEscapePageCursor Advance() => this with { Shown = Shown + 1 };
}
