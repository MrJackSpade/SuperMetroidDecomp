using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The four bounded bank-$84 instruction-list ranges used by the translated
/// Chozo statue terrain PLMs. Adjacent native setup and callback machine code
/// is deliberately excluded; draw-list payloads are compiled separately.
/// </summary>
internal static class ChozoStatuePlmProgramDefinitions
{
    /// <summary>Crumbling plug animation/control list, $84:D0F6-D107.</summary>
    internal const ushort CrumblePlugStart = 0xd0f6;
    /// <summary>Last byte before the crumbling plug's $84:D108 setup routine.</summary>
    internal const ushort CrumblePlugEnd = 0xd107;
    /// <summary>Lower Norfair hand event/acid list, $84:D13F-D154.</summary>
    internal const ushort LowerNorfairHandStart = 0xd13f;
    /// <summary>Last byte before the hand's $84:D155 native callback.</summary>
    internal const ushort LowerNorfairHandEnd = 0xd154;
    /// <summary>Wrecked Ship clear-slope list, $84:D3CF-D3D6.</summary>
    internal const ushort ClearSlopeStart = 0xd3cf;
    /// <summary>Last byte before native slope transform $84:D3D7.</summary>
    internal const ushort ClearSlopeEnd = 0xd3d6;
    /// <summary>Wrecked Ship block-slope list, $84:D3EC-D3F3.</summary>
    internal const ushort BlockSlopeStart = 0xd3ec;
    /// <summary>Last byte before native spike restore $84:D3F4.</summary>
    internal const ushort BlockSlopeEnd = 0xd3f3;

    /// <summary>$84:D14D: restore lowered acid and clear the hand on event-set room entry.</summary>
    private const ushort RestoreLoweredAcid = 0xd14d;

    /// <summary>The four bounded instruction lists, each owning one inclusive bank-$84 byte range.</summary>
    private enum ProgramList
    {
        /// <summary>$84:D0F6-D107, the crumbling plug list.</summary>
        CrumblePlug,
        /// <summary>$84:D13F-D154, the Lower Norfair hand list.</summary>
        LowerNorfairHand,
        /// <summary>$84:D3CF-D3D6, the Wrecked Ship clear-slope list.</summary>
        ClearSlope,
        /// <summary>$84:D3EC-D3F3, the Wrecked Ship block-slope list.</summary>
        BlockSlope,
    }

    private static readonly ProgramList[] ProgramLists = Enum.GetValues<ProgramList>();

    private static ushort StartOf(ProgramList list) => list switch
    {
        ProgramList.CrumblePlug => CrumblePlugStart,
        ProgramList.LowerNorfairHand => LowerNorfairHandStart,
        ProgramList.ClearSlope => ClearSlopeStart,
        ProgramList.BlockSlope => BlockSlopeStart,
        _ => throw new InvalidOperationException($"Undefined {nameof(ProgramList)} {(int)list}."),
    };

    private static ushort EndOf(ProgramList list) => list switch
    {
        ProgramList.CrumblePlug => CrumblePlugEnd,
        ProgramList.LowerNorfairHand => LowerNorfairHandEnd,
        ProgramList.ClearSlope => ClearSlopeEnd,
        ProgramList.BlockSlope => BlockSlopeEnd,
        _ => throw new InvalidOperationException($"Undefined {nameof(ProgramList)} {(int)list}."),
    };

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = 0;
        if (!TryLocate(address, out ProgramList list) || address == EndOf(list)) return false;
        int offset = address - StartOf(list);
        value = (ushort)(ByteAt(list, offset) | ByteAt(list, offset + 1) << 8);
        return true;
    }
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        value = 0;
        if (!TryLocate(address, out ProgramList list)) return false;
        value = ByteAt(list, address - StartOf(list));
        return true;
    }
    private static bool TryLocate(ushort address, out ProgramList list)
    {
        foreach (ProgramList candidate in ProgramLists)
        {
            if (address >= StartOf(candidate) && address <= EndOf(candidate))
            {
                list = candidate;
                return true;
            }
        }
        list = default;
        return false;
    }
    private static byte ByteAt(ProgramList list, int offset) =>
        (byte)(WordAt(list, offset & ~1) >> ((offset & 1) * 8));

    /// <summary>
    /// Native crumble uses four six-byte draw records with holds 4/4/4/1.
    /// The hand branches on lowered acid, otherwise sleeps under its trigger callback.
    /// Slope programs draw once, apply their named transform and delete.
    /// Canonical words are calculated here; byte projection preserves all 52 overlaps.
    /// </summary>
    private static ushort WordAt(ProgramList list, int offset)
    {
        switch (list)
        {
            case ProgramList.CrumblePlug:
            {
                if (offset == 16) return (ushort)RoomPlmInstruction.Delete;
                int frame = offset / 4;
                return offset % 4 == 0 ? (ushort)(frame == 3 ? 1 : 4) :
                    (ushort)(RoomPlmShotBlockDrawDefinitions.SingleFrame0 + frame * 6);
            }
            case ProgramList.LowerNorfairHand:
                return offset switch
                {
                    0 => (ushort)RoomPlmInstruction.GotoIfEventSet,
                    2 => (ushort)EventNumber.LowerNorfairChozoLoweredAcid,
                    4 => RestoreLoweredAcid,
                    6 => (ushort)RoomPlmInstruction.InstallPreInstruction,
                    8 => ChozoStatuePlmRomData.WaitForLowerNorfairHand,
                    10 => (ushort)RoomPlmInstruction.Sleep,
                    14 => (ushort)RoomPlmInstruction.SetLoweredAcidHeight,
                    16 => 1,
                    18 => (ushort)ChozoStatueDraw.LowerNorfairClearedHand,
                    _ => (ushort)RoomPlmInstruction.Delete,
                };
            case ProgramList.ClearSlope:
            case ProgramList.BlockSlope:
            {
                bool clear = list == ProgramList.ClearSlope;
                return offset switch
                {
                    0 => 1,
                    2 => clear ? (ushort)ChozoStatueDraw.ClearSlopeAccess : (ushort)ChozoStatueDraw.BlockSlopeAccess,
                    4 => clear ? (ushort)RoomPlmInstruction.TransformSpikesToSlopes : (ushort)RoomPlmInstruction.RevertSlopesToSpikes,
                    _ => (ushort)RoomPlmInstruction.Delete,
                };
            }
            default:
                throw new InvalidOperationException($"Undefined {nameof(ProgramList)} {(int)list}.");
        }
    }
}
