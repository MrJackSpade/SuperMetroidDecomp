using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for enemy projectile $86:B4B1, the old Tourian escape shaft fake-wall
/// explosion, and its list <c>InstList_EnemyProj_OldTourianEscapeShaftFakeWallExplosion</c>
/// at $86:B443-$B46F. Its six small-explosion spritemap operands resolve through extracted
/// presentation art.
/// </summary>
internal static class OldTourianEscapeShaftWallExplosionDefinitions
{
    /// <summary>$86:B4B0, the header's bare-RTS pre-instruction.</summary>
    internal const ushort PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_86B4B0;

    /// <summary>$86:B443, <c>InstList_EnemyProj_OldTourianEscapeShaftFakeWallExplosion_0</c>.</summary>
    internal const ushort InitialInstructionList = 0xb443;

    /// <summary>$86:B449, <c>InstList_EnemyProj_OldTourianEscapeShaftFakeWallExplosion_1</c>, the counted loop.</summary>
    internal const ushort Loop = 0xb449;

    /// <summary>$86:B4A0/$B4A6: the wall-middle X/Y written to position and Var0/Var1.</summary>
    internal const ushort XPosition = 0x0110;
    internal const ushort YPosition = 0x0888;

    /// <summary>$86:B44F-$B451: library-two small explosion $24, Max6.</summary>
    internal static SoundEffectId ExplosionSound => SoundEffectLibrary2Sounds.SmallExplosion;
    internal const byte ExplosionSoundMaximum = 6;

    /// <summary>$86:B452-$B468: six small-explosion poses holding 4, 6, 5, 5, 5 and 6 frames.</summary>
    private static ReadOnlySpan<ushort> FrameDurations => [4, 6, 5, 5, 5, 6];

    private const ushort FirstFrame = 0xb452;

    internal static bool Owns(RoomEnemyProjectileKind kind) =>
        kind == RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion;

    /// <summary>Reads one mechanics word of the list; spritemap operands are presentation.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        switch (address)
        {
            case 0xb443: return (ushort)EnemyProjectileInstruction.ClearPreInstruction;
            case 0xb445: return (ushort)EnemyProjectileInstruction.TimerInY;
            case 0xb447: return 0x0001;
            case Loop: return (ushort)EnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius;
            // $86:B44B: packed bytes $07,$00,$0F,$00 — X mask 7 and Y mask 15, both centred on zero.
            case 0xb44b: return 0x0007;
            case 0xb44d: return 0x000f;
            case 0xb44f: return (ushort)EnemyProjectileInstruction.QueueSoundInY_Lib2_Max6;
            case 0xb46a: return (ushort)EnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero;
            case 0xb46c: return Loop;
            case 0xb46e: return (ushort)EnemyProjectileInstruction.Delete;
        }
        int offset = address - FirstFrame;
        if (offset >= 0 && offset < FrameDurations.Length * 4 && offset % 4 == 0)
            return FrameDurations[offset / 4];
        throw new InvalidDataException(
            $"Old Tourian escape-shaft wall explosion has no mechanics word at $86:{address:X4}.");
    }

    internal static int PresentationWordCount => FrameDurations.Length;

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(FirstFrame + 2 + 4 * index);
    }
}
