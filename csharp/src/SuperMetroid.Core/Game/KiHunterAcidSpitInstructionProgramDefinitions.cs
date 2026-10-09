namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for both KiHunter acid-spit introductions and their shared splash.
/// Interleaved spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class KiHunterAcidSpitInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_KiHunterAcidSpit_Left</c> at $86:CF34.</summary>
    internal const ushort Left = 0xcf34;

    /// <summary><c>InstList_EnemyProjectile_KiHunterAcidSpit_HitFloor</c> at $86:CF56.</summary>
    internal const ushort HitFloor = 0xcf56;

    /// <summary><c>InstList_EnemyProjectile_KiHunterAcidSpit_Right</c> at $86:CF6E.</summary>
    internal const ushort Right = 0xcf6e;

    /// <summary>
    /// $86:CF34-CF44 and mirrored $86:CF6E-CF7E introduction holds. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.
    /// The splash holds below remain calculated.
    /// </summary>
    private static readonly ushort[] IntroductionHolds = [3, 3, 4, 3, 1];

    /// <summary>Number of compiled mechanics words across the two spit introductions and their shared floor splash.</summary>
    public static int MechanicsWordCount => 27;

    /// <summary>Number of spritemap operands whose pointers are resolved through the extracted presentation data.</summary>
    public static int PresentationWordCount => 19;

    /// <summary>
    /// Seven poses per facing install movement after posefive, then sleep.
    /// The shared floor splash clears movement, displays five frames and deletes.
    /// Splash holds descend12,10,10,8,8: a two-tick reduction every two phases.
    /// </summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index is >= 10 and < 17)
        {
            int local = index - 10;
            return local switch
            {
                0 => new(HitFloor, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
                6 => new(HitFloor + 2 + 5 * 4, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
                _ => new((ushort)(HitFloor + 2 + 4 * (local - 1)), (ushort)(12 - 2 * (local / 2))),
            };
        }
        bool right = index >= 17;
        int part = right ? index - 17 : index;
        int start = right ? Right : Left;
        if (part < 5) return new((ushort)(start + part * 4), IntroductionHolds[part]);
        return part switch
        {
            5 => new((ushort)(start + 5 * 4), EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
            6 => new((ushort)(start + 5 * 4 + 2), right
                ? EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KiHunterAcid_Right
                : EnemyProjectileCodePointers.PreInstruction_EnemyProjectile_KiHunterAcid_Left),
            7 or 8 => new((ushort)(start + 6 * 4 + (part - 7) * 4), 1),
            _ => new((ushort)(start + 8 * 4), EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
        };
    }

    /// <summary>Maps an operand ordinal to its address in the left, right, or shared floor-splash instruction list.</summary>
    /// <param name="index">Zero-based ordinal among all presentation operands in these instruction lists.</param>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index is >= 7 and < 12) return (ushort)(HitFloor + 4 + (index - 7) * 4);
        int pose = index >= 12 ? index - 12 : index;
        int start = index >= 12 ? Right : Left;
        return (ushort)(start + 2 + pose * 4 + (pose >= 5 ? 4 : 0));
    }
    /// <summary>Determines whether this compiled program set handles one of the two KiHunter acid-spit projectiles.</summary>
    /// <param name="kind">Projectile kind to check against the left- and right-facing spit programs.</param>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.KiHunterAcidSpitLeft or
        RoomEnemyProjectileKind.KiHunterAcidSpitRight;

    /// <summary>Finds the compiled mechanics value stored at a bank-relative instruction address.</summary>
    /// <param name="address">Instruction address to resolve within the compiled KiHunter acid-spit mechanics.</param>
    /// <returns>The mechanics word associated with <paramref name="address"/>.</returns>
    /// <exception cref="InvalidDataException">The address is not represented by a compiled mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"KiHunter acid-spit instruction mechanics pointer $86:{address:X4} is not compiled.");
    }
}
