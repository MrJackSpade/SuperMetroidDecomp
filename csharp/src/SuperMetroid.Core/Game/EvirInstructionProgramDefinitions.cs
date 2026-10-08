namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled timing, callback, timer, and loop control for Evir's body, arms, and
/// regenerating projectile. Interleaved spritemap operands remain live presentation data.
/// </summary>
internal abstract class EvirInstructionProgramDefinitions
{
    /// <summary><c>InstList_Evir_Body_FacingLeft</c> at $A8:86A7.</summary>
    internal const ushort BodyFacingLeft = 0x86a7;
    /// <summary><c>InstList_Evir_Arms_FacingLeft</c> at $A8:86C3.</summary>
    internal const ushort ArmsFacingLeft = 0x86c3;
    /// <summary><c>InstList_Evir_Body_FacingRight</c> at $A8:870B.</summary>
    internal const ushort BodyFacingRight = 0x870b;
    /// <summary><c>InstList_Evir_Arms_FacingRight</c> at $A8:8727.</summary>
    internal const ushort ArmsFacingRight = 0x8727;
    /// <summary><c>InstList_Evir_Projectile_Normal</c> at $A8:876F.</summary>
    internal const ushort ProjectileNormal = 0x876f;
    /// <summary><c>InstList_Evir_Projectile_Regenerating_0</c> at $A8:8775.</summary>
    internal const ushort ProjectileRegenerating = 0x8775;
    /// <summary><c>InstList_Evir_Projectile_Regenerating_1</c> at $A8:877D.</summary>
    internal const ushort ProjectileRegenerationLoop = 0x877d;

    internal const int BodyFrameCount = 6;
    internal const int ArmsFrameCount = 17;

    public static int MechanicsWordCount => 67;
    public static int PresentationWordCount => 49;

    /// <summary>
    /// Both facing halves contain a six-frame body loop and seventeen-frame arm loop.
    /// Frames last ten ticks except the arms' final48-tick rest. The projectile holds
    /// one pose or performs eight regeneration steps followed by a16-tick completion.
    /// </summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        if (index < 54)
        {
            int word = index % 27;
            bool arms = word >= 8;
            if (arms)
                word -= 8;
            int frames = arms ? ArmsFrameCount : BodyFrameCount;
            ushort start = (ushort)((arms ? ArmsFacingLeft : BodyFacingLeft) + 100 * (index / 27));
            if (word < frames)
                return new((ushort)(start + 4 * word), (ushort)(arms && word == frames - 1 ? 48 : 10));
            return new((ushort)(start + 4 * frames + 2 * (word - frames)),
                word == frames ? CommonEnemyInstructionCodes.Goto : start);
        }
        int control = index - 54;
        return control switch
        {
            0 => new(ProjectileNormal, 1),
            1 => new((ushort)(ProjectileNormal + 4), CommonEnemyInstructionCodes.Sleep),
            2 => new(ProjectileRegenerating, EnemyInstructionCodePointers.Instruction_Evir_SetInitialRegenerationXOffset),
            3 => new((ushort)(ProjectileRegenerating + 2), CommonEnemyInstructionCodes.SetTimer),
            4 => new((ushort)(ProjectileRegenerating + 4), 8),
            5 => new((ushort)(ProjectileRegenerating + 6), EnemyInstructionCodePointers.Instruction_Evir_PlaySpitSFX),
            6 => new(ProjectileRegenerationLoop, 8),
            7 => new((ushort)(ProjectileRegenerationLoop + 4), EnemyInstructionCodePointers.Instruction_Evir_AdvanceRegenerationXOffset),
            8 => new((ushort)(ProjectileRegenerationLoop + 6), CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
            9 => new((ushort)(ProjectileRegenerationLoop + 8), ProjectileRegenerationLoop),
            10 => new((ushort)(ProjectileRegenerationLoop + 10), 16),
            11 => new((ushort)(ProjectileRegenerationLoop + 14), EnemyInstructionCodePointers.Instruction_Evir_FinishRegeneration),
            _ => new((ushort)(ProjectileRegenerationLoop + 16), CommonEnemyInstructionCodes.Sleep),
        };
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        if (index < 46)
        {
            int frame = index % 23;
            bool arms = frame >= BodyFrameCount;
            if (arms)
                frame -= BodyFrameCount;
            return (ushort)((arms ? ArmsFacingLeft : BodyFacingLeft) + 100 * (index / 23) + 4 * frame + 2);
        }
        return (ushort)(index == 46 ? ProjectileNormal + 2
            : ProjectileRegenerationLoop + 2 + 10 * (index - 47));
    }
    /// <summary>Returns fixed Evir control or rejects pointers outside its six programs.</summary>
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
            $"Evir instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

}
