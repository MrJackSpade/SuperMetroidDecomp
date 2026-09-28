namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoLeftFootOrbMechanicsWord(
    ushort Address, ushort Value);

/// <summary>
/// Golden Torizo's callable left-foot-forward Chozo-orb attack at
/// $AA:CC57-CC98. Twenty-three control words remain engine-owned while ten
/// interleaved extended-frame selectors are editable presentation.
/// </summary>
internal static class GoldenTorizoLeftFootOrbInstructionProgramDefinitions
{
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_0</c> at $AA:CC57.</summary>
    internal const ushort Start = 0xcc57;
    /// <summary>First byte after the left-foot-forward orb list, $AA:CC99.</summary>
    internal const ushort End = GoldenTorizoRightOrbInstructionProgramDefinitions.Start;
    /// <summary><c>InstList_GoldenTorizo_SpewChozoOrb_FacingLeft_LeftFootFwd_1</c> at $AA:CC77.</summary>
    private const ushort ShotLoop = 0xcc77;
    /// <summary><c>Function_GoldenTorizo_Movement_Attacking</c> at $AA:D5ED.</summary>
    private const ushort AttackingMovement = 0xd5ed;
    /// <summary><c>Function_GoldenTorizo_Movement_Walking</c> at $AA:D5F1.</summary>
    private const ushort WalkingMovement = 0xd5f1;

    private static readonly GoldenTorizoLeftFootOrbMechanicsWord[] Words =
    [
        new(0xcc57, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xcc59, AttackingMovement),
        new(0xcc5b, 0x0006),
        new(0xcc5f, 0x0003),
        new(0xcc63, 0x0003),
        new(0xcc67, 0x0003),
        new(0xcc6b, 0x0003),
        new(0xcc6f, 0x0006),
        new(0xcc73, CommonEnemyInstructionCodes.SetTimer),
        new(0xcc75, 0x0006),
        new(0xcc77, TorizoInstructionCodes.Instruction_Torizo_PlayShotTorizoSFX),
        new(0xcc79, TorizoInstructionCodes.Instruction_GoldenTorizo_SpawnChozoOrbs),
        new(0xcc7b, CommonEnemyInstructionCodes.WaitFrames),
        new(0xcc7d, 0x0006),
        new(0xcc7f, CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate),
        new(0xcc81, ShotLoop),
        new(0xcc83, 0x0003),
        new(0xcc87, 0x0003),
        new(0xcc8b, 0x0003),
        new(0xcc8f, 0x0003),
        new(0xcc93, TorizoInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY),
        new(0xcc95, WalkingMovement),
        new(0xcc97, TorizoInstructionCodes.Instruction_Torizo_Return),
    ];

    private static readonly ushort[] PresentationWords =
    [
        0xcc5d, 0xcc61, 0xcc65, 0xcc69, 0xcc6d,
        0xcc71, 0xcc85, 0xcc89, 0xcc8d, 0xcc91,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static GoldenTorizoLeftFootOrbMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (GoldenTorizoLeftFootOrbMechanicsWord word in Words)
        {
            if (word.Address != address) continue;
            value = word.Value;
            return true;
        }
        value = 0;
        return false;
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000)
            return false;
        ushort offset = unchecked((ushort)address);
        foreach (GoldenTorizoLeftFootOrbMechanicsWord word in Words)
            if (offset == word.Address ||
                offset == unchecked((ushort)(word.Address + 1)))
                return true;
        return false;
    }
}
