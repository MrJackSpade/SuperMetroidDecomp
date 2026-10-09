namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Ceres Ridley's fireball, center-afterburn, directional-afterburn,
/// and final-impact programs. Interleaved spritemap operands remain live cartridge data.
/// </summary>
internal abstract class CeresRidleyProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_RidleysFireball_0</c> at $86:9552.</summary>
    /// <remarks>
    /// The $86:9642 projectile header enters here. The bounded $86:9552..9573
    /// control stream clears its pre-instruction, holds the first pose for four
    /// ticks, installs the moving pre-instruction at $940E, then holds that pose
    /// for four more ticks. Its continuation is <see cref="FireballLoop"/>.
    /// The eleven control words match the pinned NTSC J/U v1.0 ROM; the six
    /// interleaved spritemap operands remain live presentation data.
    /// </remarks>
    internal const ushort Fireball = 0x9552;

    /// <summary><c>InstList_EnemyProjectile_RidleysFireball_1</c> at $86:9560.</summary>
    /// <remarks>
    /// Four consecutive two-tick frames at $9560, $9564, $9568, and $956C
    /// end in a $81AB go-to whose operand is this entry address. Thus the
    /// animation cycles through exactly these four frames until a collision
    /// changes its instruction pointer; it cannot fall through to $9574.
    /// </remarks>
    internal const ushort FireballLoop = 0x9560;

    /// <summary><c>InstList_EnemyProjectile_Afterburn_Final</c> at $86:9574.</summary>
    /// <remarks>
    /// A directional afterburn's collision path resets its instruction pointer
    /// here. The bounded $86:9574..958B stream clears the pre-instruction,
    /// displays five successive five-tick poses, then executes the $8154
    /// delete instruction. Its seven control words match the pinned NTSC
    /// J/U v1.0 ROM. The five intervening spritemap pointers are presentation
    /// operands, not control words.
    /// </remarks>
    internal const ushort AfterburnFinal = 0x9574;

    /// <summary><c>InstList_EnemyProjectile_HorizontalAfterburn_Center</c> at $86:95A0.</summary>
    /// <remarks>
    /// The $86:9650 projectile header selects this bounded $95A0..95B9
    /// stream. It clears the pre-instruction, holds the first pose for five
    /// ticks, calls $95BA to spawn right and left afterburn children, then
    /// displays four more five-tick poses before $8154 deletes the center.
    /// Its eight control words match the pinned NTSC J/U v1.0 ROM. The five
    /// interleaved spritemap pointers remain live presentation operands.
    /// </remarks>
    internal const ushort HorizontalCenter = 0x95a0;

    /// <summary><c>InstList_EnemyProjectile_VerticalAfterburn_Center</c> at $86:95D3.</summary>
    /// <remarks>
    /// The $86:965E header selects this $95D3..95EC stream. Every control
    /// address is its <see cref="HorizontalCenter"/> counterpart plus $33;
    /// every value is identical except the spawn callback $95BA becomes
    /// $95ED, also plus $33. Thus one five-tick pose precedes spawning up
    /// and down children, four five-tick poses follow, and $8154 deletes the
    /// center. All eight words match the pinned NTSC J/U v1.0 ROM.
    /// </remarks>
    internal const ushort VerticalCenter = 0x95d3;

    /// <summary><c>InstList_EnemyProjectile_Afterburn</c> at $86:9606.</summary>
    /// <remarks>
    /// The $86:966C/$967A/$9688/$9696 right, left, up, and down projectile
    /// headers all enter this bounded $9606..961F stream. It clears the
    /// pre-instruction, holds one pose for five ticks, calls $9620 to decrement
    /// the remaining-afterburn byte and spawn the same directional kind when
    /// its signed result is nonnegative, then holds four further five-tick
    /// poses and executes $8154 delete. All eight control words match the
    /// pinned NTSC J/U v1.0 ROM. Its five spritemap operands stay live.
    /// </remarks>
    internal const ushort DirectionalAfterburn = 0x9606;

    /// <summary>Number of interleaved presentation operands that select separately installed spritemap artwork.</summary>
    public static int PresentationWordCount => 26;

    /// <summary>Calculated locations of the interleaved bank-$86 spritemap operands.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        if (index < 6)
            return (ushort)(index == 0 ? Fireball + 4 : index == 1 ? Fireball + 12 : FireballLoop + 2 + (index - 2) * 4);
        if (index < 11)
            return (ushort)(AfterburnFinal + 4 + (index - 6) * 4);
        int frame = (index - 11) % 5;
        return (ushort)(SpawnProgram((index - 11) / 5) + 4 + frame * 4 + (frame == 0 ? 0 : 2));
    }

    /// <summary>Selects the native instruction-list start for a center-spawn or directional afterburn program.</summary>
    /// <param name="kind">Program selector: horizontal center, vertical center, or directional afterburn.</param>
    /// <returns>The bank-$86 start address for the selected program.</returns>
    /// <exception cref="IndexOutOfRangeException">The selector is not one of the three compiled program kinds.</exception>
    internal static ushort SpawnProgram(int kind) => kind switch
    {
        0 => HorizontalCenter,
        1 => VerticalCenter,
        2 => DirectionalAfterburn,
        _ => throw new IndexOutOfRangeException(),
    };
    /// <summary>Tests whether the compiled Ridley projectile programs own the requested projectile kind.</summary>
    /// <param name="kind">Projectile kind to classify.</param>
    /// <returns>True for the fireball and the horizontal or vertical afterburn variants.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.CeresRidleyFireball or
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter or
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter or
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight or
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft or
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp or
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown;

    /// <summary>Reads a compiled control-flow or timing word by its bank-$86 address.</summary>
    /// <param name="address">Address of the requested mechanics word.</param>
    /// <returns>The instruction or operand value represented at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not part of a compiled Ridley projectile mechanics stream.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value))
            return value;
        throw new InvalidDataException(
            $"Ceres Ridley projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    /// <summary>Attempts to resolve an address in the bounded fireball, center-afterburn, or directional-afterburn control streams.</summary>
    /// <param name="address">Bank-$86 address to look up.</param>
    /// <param name="value">Receives the compiled mechanics word when the address is owned.</param>
    /// <returns>True when the address identifies a compiled mechanics word; presentation operands are intentionally excluded.</returns>
    internal static bool TryRead(int address, out ushort value)
    {
        int offset = address - Fireball;
        if (offset is 0 or 2 or 6 or 8 or 10)
        {
            value = offset switch
            {
                0 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction,
                6 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY,
                8 => EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_RidleyFireball,
                _ => 4,
            };
            return true;
        }
        offset = address - FireballLoop;
        if (offset >= 0 && offset <= 12 && offset % 4 == 0)
        {
            value = 2;
            return true;
        }
        if (offset is 16 or 18)
        {
            value = offset == 16 ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY : FireballLoop;
            return true;
        }
        if (TryAfterburn(address - AfterburnFinal, false, 0, out value))
            return true;
        for (int kind = 0; kind < 3; kind++)
        {
            ushort callback = kind switch
            {
                0 => EnemyProjectileCodePointers.Instruction_Spawn_HorizontalAfterburn_EnemyProjectiles,
                1 => EnemyProjectileCodePointers.Instruction_Spawn_VerticalAfterburn_EnemyProjectiles,
                _ => EnemyProjectileCodePointers.Instruction_SpawnNext_Afterburn_EnemyProjectile,
            };
            if (TryAfterburn(address - SpawnProgram(kind), true, callback, out value))
                return true;
        }
        value = 0;
        return false;
    }

    /// <summary>Resolves a control word in an afterburn stream, accounting for its optional child-spawn instruction.</summary>
    /// <param name="offset">Address relative to the selected afterburn list start.</param>
    /// <param name="spawns">Whether the stream includes a spawn callback before its timed frames.</param>
    /// <param name="callback">Compiled callback word used by the spawn instruction.</param>
    /// <param name="value">Receives the mechanics word when the offset is part of the control stream.</param>
    /// <returns>True when the offset is a control word; spritemap presentation addresses are not reported.</returns>
    private static bool TryAfterburn(int offset, bool spawns, ushort callback, out ushort value)
    {
        if (offset == 0)
        {
            value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction;
            return true;
        }
        if (spawns && offset == 6)
        {
            value = callback;
            return true;
        }
        if (spawns && offset >= 8)
            offset -= 2;
        if (offset >= 2 && offset <= 18 && (offset - 2) % 4 == 0)
        {
            value = 5;
            return true;
        }
        value = EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete;
        return offset == 22;
    }
}
