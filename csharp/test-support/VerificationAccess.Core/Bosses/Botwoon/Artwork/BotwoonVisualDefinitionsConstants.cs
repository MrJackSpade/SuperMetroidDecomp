using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.Botwoon.Artwork;

/// <summary>Cartridge values and helpers of <see cref="BotwoonVisualDefinitions"/> that only verification reads.</summary>
internal static class BotwoonVisualDefinitionsConstants
{
    /// <summary>Botwoon's head OAM bank, $B3.</summary>
    internal const byte Bank = 0xb3;
    /// <summary>The installed head spritemap for <paramref name="frame"/>; the hidden frame is the shared empty map.</summary>
    internal static ushort HeadSpritemap(BotwoonHeadFrame frame)
    {
        if (!frame.Visible)
            return CommonEnemyEmptyExtendedFrameToolingDefinitions.EmptySpritemap;
        return frame.MouthOpen ? OpenMouth(frame.Facing) : ClosedMouth(frame.Facing);
    }
    /// <summary>The sixteen visible head frames as installed artwork, closed then open, in physical order.</summary>
    internal static NativeEnemySpritemapDefinition[] Frames() =>
    [
        .. BotwoonVisualToolingDefinitions.PhysicalOrder.Select(BotwoonHeadFrame.Closed).Concat(BotwoonVisualToolingDefinitions.PhysicalOrder.Select(BotwoonHeadFrame.Open))
            .Select((frame, index) => new NativeEnemySpritemapDefinition(Bank, HeadSpritemap(frame), $"botwoon_head_{index:D2}")),
    ];
    /// <summary>Selects the bank-$B3 closed-mouth spritemap, skipping the unused down-facing-left map.</summary>
    internal static ushort ClosedMouth(BotwoonFacing facing) => facing switch
    {
        // Spritemaps_Botwoon_MouthClosed_Priority2_Aiming* at $B3:E329-$E389; the unused down-facing-left map is skipped.
        BotwoonFacing.UpLeft => 0xe329,
        BotwoonFacing.Left => 0xe335,
        BotwoonFacing.DownLeft => 0xe341,
        BotwoonFacing.Down => 0xe359,
        BotwoonFacing.DownRight => 0xe365,
        BotwoonFacing.Right => 0xe371,
        BotwoonFacing.UpRight => 0xe37d,
        BotwoonFacing.Up => 0xe389,
        _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, "Undefined Botwoon facing."),
    };
    /// <summary>Selects the bank-$B3 open-mouth spritemap; diagonal facings carry a third OAM piece.</summary>
    internal static ushort OpenMouth(BotwoonFacing facing) => facing switch
    {
        // Spritemaps_Botwoon_MouthOpen_Priority2_Aiming* at $B3:E3A1-$E415; diagonal maps carry a third piece.
        BotwoonFacing.UpLeft => 0xe3a1,
        BotwoonFacing.Left => 0xe3b2,
        BotwoonFacing.DownLeft => 0xe3be,
        BotwoonFacing.Down => 0xe3db,
        BotwoonFacing.DownRight => 0xe3e7,
        BotwoonFacing.Right => 0xe3f8,
        BotwoonFacing.UpRight => 0xe404,
        BotwoonFacing.Up => 0xe415,
        _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, "Undefined Botwoon facing."),
    };
}
