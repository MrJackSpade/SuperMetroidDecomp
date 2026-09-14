using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Game;

/// <summary>Fixed trail timing/commands in bank $90; appearance words remain external presentation.</summary>
public static class ProjectileTrailProgramDefinitions
{
    /// <summary>$90:B4C9/B523/B585/B59F/B5B1: the empty, ice-left, ice-right, wave and missile terminators.</summary>
    private static ReadOnlySpan<ushort> Ends => [0xb4c9, 0xb523, 0xb585, 0xb59f, 0xb5b1];
    /// <summary>$90:B4E3..B51D: inline MoveLeftProjectileTrailDownOnePixel commands in the left ice list.</summary>
    private static ReadOnlySpan<ushort> LeftMoves => [0xb4e3, 0xb4ed, 0xb4f3, 0xb4f9, 0xb4ff, 0xb505, 0xb50b, 0xb511, 0xb517, 0xb51d];
    /// <summary>$90:B545..B57F: inline MoveRightProjectileTrailDownOnePixel commands in the right ice list.</summary>
    private static ReadOnlySpan<ushort> RightMoves => [0xb545, 0xb54f, 0xb555, 0xb55b, 0xb561, 0xb567, 0xb56d, 0xb573, 0xb579, 0xb57f];
    /// <summary>$90:B51F/B581 are the ice lists' four-frame tails; wave/missile frames all last four ticks.</summary>
    private static ReadOnlySpan<ushort> FourTickFrames => [0xb51f, 0xb581, 0xb58f, 0xb593, 0xb597, 0xb59b, 0xb5a1, 0xb5a5, 0xb5a9, 0xb5ad];

    public static bool TryRead(int address, out ushort word)
    {
        word = 0;
        if ((address & ~0xffff) != SamusProjectileRomData.Banks.Movement) return false;
        ushort pointer = (ushort)address;
        if (Ends.Contains(pointer)) return true;
        if (LeftMoves.Contains(pointer)) { word = SamusProjectileRomData.Trails.MoveLeftDown; return true; }
        if (RightMoves.Contains(pointer)) { word = SamusProjectileRomData.Trails.MoveRightDown; return true; }
        if (!ProjectileTrailVisualDefinitions.Frames.Contains(pointer)) return false;
        word = (ushort)(FourTickFrames.Contains(pointer) ? 4 : 1);
        return true;
    }

    public static ushort Read(ISnesAddressSpace bus, int address) => TryRead(address, out ushort word)
        ? word : RomDataReader.ReadWordFixedBank(bus, address);
}
