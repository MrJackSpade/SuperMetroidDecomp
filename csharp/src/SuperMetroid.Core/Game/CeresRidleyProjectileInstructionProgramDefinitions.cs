namespace SuperMetroid.Core.Game;

/// <summary>One compiled Ceres Ridley projectile mechanics word at its bank-$86 address.</summary>
internal readonly record struct CeresRidleyProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Ceres Ridley's fireball, center-afterburn, directional-afterburn,
/// and final-impact programs. Interleaved spritemap operands remain live cartridge data.
/// </summary>
internal static class CeresRidleyProjectileInstructionProgramDefinitions
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
    internal const ushort HorizontalCenter = 0x95a0;

    /// <summary><c>InstList_EnemyProjectile_VerticalAfterburn_Center</c> at $86:95D3.</summary>
    internal const ushort VerticalCenter = 0x95d3;

    /// <summary><c>InstList_EnemyProjectile_Afterburn</c> at $86:9606.</summary>
    internal const ushort DirectionalAfterburn = 0x9606;

    private static readonly CeresRidleyProjectileInstructionMechanicsWord[] Words =
    [
        new(Fireball, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0x9554, 0x0004),
        new(0x9558, EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
        new(0x955a, EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_RidleyFireball),
        new(0x955c, 0x0004),
        new(FireballLoop, 0x0002),
        new(0x9564, 0x0002),
        new(0x9568, 0x0002),
        new(0x956c, 0x0002),
        new(0x9570, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY),
        new(0x9572, FireballLoop),

        new(AfterburnFinal,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0x9576, 0x0005),
        new(0x957a, 0x0005),
        new(0x957e, 0x0005),
        new(0x9582, 0x0005),
        new(0x9586, 0x0005),
        new(0x958a, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),

        new(HorizontalCenter,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0x95a2, 0x0005),
        new(0x95a6,
            EnemyProjectileCodePointers.Instruction_Spawn_HorizontalAfterburn_EnemyProjectiles),
        new(0x95a8, 0x0005),
        new(0x95ac, 0x0005),
        new(0x95b0, 0x0005),
        new(0x95b4, 0x0005),
        new(0x95b8, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),

        new(VerticalCenter,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0x95d5, 0x0005),
        new(0x95d9,
            EnemyProjectileCodePointers.Instruction_Spawn_VerticalAfterburn_EnemyProjectiles),
        new(0x95db, 0x0005),
        new(0x95df, 0x0005),
        new(0x95e3, 0x0005),
        new(0x95e7, 0x0005),
        new(0x95eb, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),

        new(DirectionalAfterburn,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
        new(0x9608, 0x0005),
        new(0x960c,
            EnemyProjectileCodePointers.Instruction_SpawnNext_Afterburn_EnemyProjectile),
        new(0x960e, 0x0005),
        new(0x9612, 0x0005),
        new(0x9616, 0x0005),
        new(0x961a, 0x0005),
        new(0x961e, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
    ];

    /// <summary>Addresses of live bank-$86 spritemap operands in the compiled programs.</summary>
    /// <remarks>
    /// The six fireball operands at $9556, $955E, and $9562..956E read
    /// $80CA, $80CA, $80CA, $80D1, $80D8, and $80DF in the pinned ROM.
    /// Their four distinct bank-$8D targets are contiguous seven-byte records:
    /// a one-component count followed by one five-byte component, so target
    /// address = $80CA + 7 * frame index for frame indices zero through three.
    /// The repeated first target supplies the two four-tick setup poses and
    /// first loop pose. The component tile/attribute words differ by frame;
    /// retain these authored pointers and payloads as live presentation data.
    /// The five final-impact operands at $9578, $957C, $9580, $9584, and
    /// $9588 point to $8D:80E6 + 7 * frame index, for indices zero through
    /// four. Each target again has a one-component count and five-byte
    /// component, ending just before $8109. Their coordinates and tile words
    /// vary across frames, so these authored presentation records also stay
    /// live rather than becoming compiled control words.
    /// </remarks>
    private static readonly ushort[] PresentationWords =
    [
        0x9556, 0x955e, 0x9562, 0x9566, 0x956a, 0x956e,
        0x9578, 0x957c, 0x9580, 0x9584, 0x9588,
        0x95a4, 0x95aa, 0x95ae, 0x95b2, 0x95b6,
        0x95d7, 0x95dd, 0x95e1, 0x95e5, 0x95e9,
        0x960a, 0x9610, 0x9614, 0x9618, 0x961c,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static CeresRidleyProjectileInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.CeresRidleyFireball or
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter or
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter or
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight or
        RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft or
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp or
        RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            CeresRidleyProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Ceres Ridley projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
