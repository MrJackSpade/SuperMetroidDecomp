using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Where the Ceres self-destruct dispatcher stands in one of its two graphics transfer lists, queued one record per
/// update. <see cref="CeresEscapeVramTransferDefinitions.Records"/> holds the four Japanese overlay records, then the
/// timer-sprite list (two timer and five warning-object transfers), then the timer-background list (five
/// warning-background and three door transfers); a cursor names a record by its position there and stops at its
/// list's end.
/// </summary>
/// <param name="Next">The position in <see cref="CeresEscapeVramTransferDefinitions.Records"/> of the record queued next.</param>
/// <param name="End">The position just past the list's last record; zero for a cursor that was never started.</param>
internal readonly record struct CeresEscapeTransferCursor(int Next, int End)
{
    /// <summary>The timer-sprite list: the timer characters, then the warning text's object characters.</summary>
    internal static CeresEscapeTransferCursor TimerSprites { get; } = new(4, 11);

    /// <summary>The timer-background list: the warning text's background characters, then the door characters.</summary>
    internal static CeresEscapeTransferCursor TimerBackgrounds { get; } = new(11, 19);

    /// <summary>Whether the cursor was started on a list.</summary>
    internal bool IsStarted => End != 0;

    /// <summary>Whether every record of the list has been queued.</summary>
    internal bool IsFinished => IsStarted && Next >= End;

    /// <summary>The record queued next.</summary>
    internal TileTransferDefinition Current => IsStarted && Next < End
        ? CeresEscapeVramTransferDefinitions.Records[Next]
        : throw new InvalidOperationException($"Ceres escape transfer cursor {this} has no current record.");

    /// <summary>The cursor after the current record has been queued.</summary>
    internal CeresEscapeTransferCursor Advance() => this with { Next = Next + 1 };
}
