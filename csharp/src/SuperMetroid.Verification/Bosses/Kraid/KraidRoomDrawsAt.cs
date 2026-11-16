using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// The Kraid room's draws as the cartridge stores them, for oracle checks that start from an address: a crumbling block's
/// breakup stages and the pictures and clears it leaves are two families (#2127), stored side by side.
/// </summary>
internal static class KraidRoomDrawsAt
{
    /// <summary>The Kraid room draw of either family stored at <paramref name="address"/>, or a draw no family owns.</summary>
    internal static RoomObjectDraw Draw(int address) =>
        NativeObjectDraws.IsDrawAt<KraidRoomObjectDraws.CrumbleDraw>(address)
            ? NativeObjectDraws.At<KraidRoomObjectDraws.CrumbleDraw>(address)
            : NativeObjectDraws.At<KraidRoomDraw>(address);

    /// <summary>Whether <paramref name="draw"/> is a frame of either Kraid room family, and the block shape it draws.</summary>
    internal static bool TryShape(RoomObjectDraw draw, [MaybeNullWhen(false)] out RoomBlockShape shape) =>
        RoomObjectDrawIdentity.TryShape(KraidRoomObjectDraws.CrumbleFamily, draw, out shape)
            || RoomObjectDrawIdentity.TryShape(KraidRoomObjectDraws.Family, draw, out shape);
}
