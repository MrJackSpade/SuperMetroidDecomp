namespace SuperMetroid.Core.Game;

/// <summary>One immutable mechanics-owned word in a translated bank-$86 projectile program.</summary>
internal readonly record struct EnemyProjectileMechanicsWordDefinition(
    ushort Address,
    ushort Value);

/// <summary>
/// One bank-$86 timed program whose frame durations and control flow affect simulation.
/// Spritemap operands deliberately remain outside this definition and continue to come from
/// the cartridge graphics stream.
/// </summary>
internal readonly record struct EnemyProjectileTimedProgramDefinition(
    ushort InitialPointer,
    ushort[] Durations,
    ushort? PrefixInstruction,
    ushort TerminalInstruction,
    ushort? TerminalOperand);

/// <summary>
/// Compiled mechanics words for the translated Mother Brain and shared misc-dust enemy
/// projectile programs in bank $86.
/// </summary>
/// <remarks>
/// Native enemy-projectile lists interleave behavior words with bank-$8D spritemap pointers.
/// Only durations, opcodes, branch targets, and collision-radius operands belong here. Keeping
/// graphics operands ROM-backed preserves editable presentation data while preventing fixed
/// gameplay timing and control flow from depending on a runtime cartridge read.
/// </remarks>
internal static class EnemyProjectileInstructionMechanicsDefinitions
{
    /// <summary><c>$86:C432</c>, Mother Brain blue/onion-ring radius-and-frame program.</summary>
    internal const ushort MotherBrainBlueRingInitial = 0xc432;

    /// <summary><c>$86:C76E</c>, looping Mother Brain bomb animation program.</summary>
    internal const ushort MotherBrainBombInitial = 0xc76e;

    /// <summary><c>$86:CAA4</c>, finite large purple-breath animation program.</summary>
    internal const ushort MotherBrainPurpleBreathInitial = 0xcaa4;

    /// <summary><c>$86:C829</c>, finite Mother Brain rainbow-beam charge program.</summary>
    internal const ushort MotherBrainRainbowBeamChargingInitial = 0xc829;

    /// <summary><c>$86:C8B0</c>, attached Mother Brain drool release program.</summary>
    internal const ushort MotherBrainDroolInitial = 0xc8b0;

    /// <summary><c>$86:C8E1</c>, finite falling-drool splash program.</summary>
    internal const ushort MotherBrainDroolFalling = 0xc8e1;

    /// <summary><c>$86:CA22</c>, looping exploded escape-door fragment program.</summary>
    internal const ushort MotherBrainEscapeDoorFragmentInitial = 0xca22;

    /// <summary><c>$86:CB0D</c>, alternate-language subtitle frame followed by sleep.</summary>
    internal const ushort MotherBrainSubtitleInitial = 0xcb0d;

    /// <summary><c>$86:E152</c>, Mother Brain rainbow-impact explosion program.</summary>
    internal const ushort MotherBrainRainbowExplosionInitial = 0xe152;

    private static readonly ushort[] BlueRingDurations = [0x0010, 0x000a, 0x0008, 0x0007, 0x0006, 0x0005];

    /// <summary>
    /// <c>$86:E42C-$E467</c>'s complete thirty-entry selector domain. The final two
    /// programs are intentionally ordered after the main contiguous block, matching the
    /// cartridge selector table rather than sorting by address.
    /// </summary>
    private static readonly ushort[] MiscDustInitialPointers =
    [
        0xe0ee, 0xe100, 0xe11a, 0xe138, MotherBrainRainbowExplosionInitial, 0xe168, 0xe17e, 0xe198,
        0xe1a6, 0xe1b0, 0xe1c6, 0xe1d8, 0xe1ea, 0xe222, 0xe234, 0xe246,
        0xe258, 0xe266, 0xe2a8, 0xe2ba, 0xe2d4, 0xe2f2, 0xe314, 0xe392,
        0xe3a0, 0xe3c6, 0xe3e8, 0xe40a, 0xe1fc, 0xe208,
    ];

    private static readonly EnemyProjectileTimedProgramDefinition[] TimedPrograms =
    [
        new(MotherBrainBombInitial,
            [6, 5, 4, 3, 2, 2, 3, 4, 5],
            null,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY,
            MotherBrainBombInitial),
        new(MotherBrainPurpleBreathInitial,
            [8, 8, 9, 9, 10, 10, 11, 11],
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete,
            null),
        new(MotherBrainRainbowBeamChargingInitial,
            [5, 5, 5, 5, 5, 5],
            null,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete,
            null),
        new(MotherBrainDroolFalling,
            [10, 10, 10, 10],
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete,
            null),
        new(MotherBrainEscapeDoorFragmentInitial,
            [1, 1, 1, 1, 3, 3, 4, 4],
            null,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY,
            MotherBrainEscapeDoorFragmentInitial),
        new(MotherBrainSubtitleInitial,
            [1],
            null,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep,
            null),

        new(0xe0ee, [3, 3, 3, 3], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe100, [5, 4, 3, 3, 3, 3], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe11a, [4, 3, 2, 2, 2, 2, 12], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe138, [4, 6, 5, 5, 5, 6], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe152, [3, 3, 4, 4, 4], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe168, [8, 8, 8, 8, 24], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe17e, [4, 4, 4, 4, 4, 4], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe198, [5, 5, 5], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe1a6, [1, 1], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe1b0, [5, 5, 5, 5, 5], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe1c6, [3, 3, 3, 3], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe1d8, [5, 5, 5, 5], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe1ea, [8, 8, 8, 8], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe222, [8, 8, 8, 8], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe234, [8, 8, 8, 8], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe246, [5, 5, 5, 5], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe258, [16, 16, 16], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe266, [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe2a8, [2, 2, 2, 2], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe2ba, [3, 3, 3, 3, 3, 5], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe2d4, [3, 3, 3, 3, 3, 3, 3], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe2f2, [5, 5, 5, 5, 5, 5, 5, 5], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe314,
            [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
             1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1],
            null,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete,
            null),
        new(0xe392, [1, 1, 1], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe3a0, [8, 8, 8, 8, 8, 8, 8, 8, 8], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe3c6, [1, 1, 1, 1, 1, 1, 1, 1], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe3e8, [16, 16, 16, 16, 16, 16, 16, 16], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe40a, [4, 4, 4, 4, 4, 4, 4, 4], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
        new(0xe1fc, [1, 1], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY, 0xe1fc),
        new(0xe208, [5, 5, 5, 5, 5, 5], null, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete, null),
    ];

    private static readonly EnemyProjectileMechanicsWordDefinition[] MechanicsWords =
        BuildMechanicsWords();

    /// <summary>Number of authored misc-dust list selectors.</summary>
    internal static int MiscDustProgramCount => MiscDustInitialPointers.Length;

    /// <summary>Number of compiled mechanics words retained for exhaustive ROM comparison.</summary>
    internal static int NativeWordCount => MechanicsWords.Length;

    /// <summary>
    /// Returns whether the ordinary room-projectile pool owns a program compiled by this
    /// shared Mother Brain/misc-dust catalog.
    /// </summary>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.MotherBrainBomb or
        RoomEnemyProjectileKind.MotherBrainPurpleBreathBig or
        RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging or
        RoomEnemyProjectileKind.MotherBrainDrool or
        RoomEnemyProjectileKind.MotherBrainDyingDrool or
        RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion or
        RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment or
        RoomEnemyProjectileKind.MotherBrainEscapeSubtitle or
        RoomEnemyProjectileKind.EyeDoorSmoke or
        RoomEnemyProjectileKind.MiscDustExplosion;

    /// <summary>Returns one of the thirty authored misc-dust program entry points.</summary>
    internal static ushort MiscDustInitialPointer(ushort animationIndex)
    {
        if (animationIndex >= MiscDustInitialPointers.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(animationIndex), animationIndex,
                "Bank-$86 misc-dust animation index must be in the native $00..$1D range.");
        }

        return MiscDustInitialPointers[animationIndex];
    }

    /// <summary>
    /// Reads one compiled duration, opcode, branch target, or packed radius operand.
    /// Spritemap addresses and uncatalogued instruction pointers fail explicitly.
    /// </summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWords.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            EnemyProjectileMechanicsWordDefinition candidate = MechanicsWords[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Bank-$86 projectile mechanics pointer ${address:X4} is outside the translated program domain.");
    }

    /// <summary>Returns one compiled address/value pair for exhaustive verification.</summary>
    internal static EnemyProjectileMechanicsWordDefinition NativeWord(int index)
    {
        if ((uint)index >= MechanicsWords.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return MechanicsWords[index];
    }

    /// <summary>Tests whether one canonical CPU-bus byte belongs to a compiled word.</summary>
    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0x860000)
            return false;

        ushort wordAddress = unchecked((ushort)(address & 0xffff));
        for (int index = 0; index < MechanicsWords.Length; index++)
        {
            ushort start = MechanicsWords[index].Address;
            if (wordAddress == start || wordAddress == unchecked((ushort)(start + 1)))
                return true;
        }

        return false;
    }

    private static EnemyProjectileMechanicsWordDefinition[] BuildMechanicsWords()
    {
        var words = new List<EnemyProjectileMechanicsWordDefinition>();

        // `$86:C432-C462` alternates radius opcodes, packed byte operands, timed frames,
        // and a final sleep. Frame spritemaps at duration+2 remain presentation-owned.
        ushort bluePointer = MotherBrainBlueRingInitial;
        for (ushort radius = 1; radius <= BlueRingDurations.Length; radius++)
        {
            words.Add(new(bluePointer,
                EnemyProjectileCodePointers.Instruction_EnemyProjectile_XYRadiusInY));
            words.Add(new(unchecked((ushort)(bluePointer + 2)),
                unchecked((ushort)(radius | radius << 8))));
            words.Add(new(unchecked((ushort)(bluePointer + 4)), BlueRingDurations[radius - 1]));
            bluePointer = unchecked((ushort)(bluePointer + 8));
        }
        words.Add(new(bluePointer,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep));

        // `$86:C8B0-C8CE` displays five attached maps, switches to the falling
        // pre-instruction, lowers the actor twelve pixels, displays its released map, and
        // sleeps. The separate odd-addressed `$C8E1` splash is described in TimedPrograms.
        ushort droolPointer = MotherBrainDroolInitial;
        for (int frame = 0; frame < 5; frame++)
        {
            words.Add(new(droolPointer, 10));
            droolPointer = unchecked((ushort)(droolPointer + 4));
        }
        words.Add(new(droolPointer,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY));
        words.Add(new(unchecked((ushort)(droolPointer + 2)),
            EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_MotherBrainsDrool_Falling));
        words.Add(new(unchecked((ushort)(droolPointer + 4)),
            EnemyProjectileCodePointers.Instruction_EnemyProj_MotherBrainsDrool_MoveDownCPixels));
        words.Add(new(unchecked((ushort)(droolPointer + 6)), 10));
        words.Add(new(unchecked((ushort)(droolPointer + 10)),
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep));

        foreach (EnemyProjectileTimedProgramDefinition program in TimedPrograms)
        {
            ushort pointer = program.InitialPointer;
            if (program.PrefixInstruction is { } prefix)
            {
                words.Add(new(pointer, prefix));
                pointer = unchecked((ushort)(pointer + 2));
            }

            foreach (ushort duration in program.Durations)
            {
                words.Add(new(pointer, duration));
                pointer = unchecked((ushort)(pointer + 4));
            }

            words.Add(new(pointer, program.TerminalInstruction));
            if (program.TerminalOperand is { } operand)
                words.Add(new(unchecked((ushort)(pointer + 2)), operand));
        }

        words.Sort(static (left, right) => left.Address.CompareTo(right.Address));
        for (int index = 1; index < words.Count; index++)
        {
            if (words[index - 1].Address == words[index].Address)
            {
                throw new InvalidDataException(
                    $"Duplicate compiled bank-$86 projectile word ${words[index].Address:X4}.");
            }
        }

        return [.. words];
    }
}
