namespace SuperMetroid.Core.Game;

internal readonly record struct GoldenTorizoJumpLandingMechanicsWord(
    ushort Address, ushort Value);

/// <summary>
/// Bank-$AA Golden Torizo backward-jump landing lists at $AA:CDAF-CDD6.
/// Each landing chooses its foot/facing-specific orb or sonic attack, then
/// returns to the matching walking leg. These are control-flow operands, not
/// editable animation or sprite data.
/// </summary>
internal static class GoldenTorizoJumpLandingInstructionProgramDefinitions
{
    internal const ushort Start = 0xcdaf;
    internal const ushort End = 0xcdd7;

    private static readonly GoldenTorizoJumpLandingMechanicsWord[] Words =
    [
        new(0xcdaf, TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_OrY2_ForAttack),
        new(0xcdb1, GoldenTorizoLeftOrbInstructionProgramDefinitions.LeftFootForward),
        new(0xcdb3, 0xcbed),
        new(0xcdb5, CommonEnemyInstructionCodes.Goto),
        new(0xcdb7, GoldenTorizoCombatInstructionPointers.WalkingLeftRightLeg),
        new(0xcdb9, TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_OrY2_ForAttack),
        new(0xcdbb, GoldenTorizoLeftOrbInstructionProgramDefinitions.Start),
        new(0xcdbd, 0xcb83),
        new(0xcdbf, CommonEnemyInstructionCodes.Goto),
        new(0xcdc1, GoldenTorizoCombatInstructionPointers.WalkingLeftLeftLeg),
        new(0xcdc3, TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_OrY2_ForAttack),
        new(0xcdc5, 0xcc99),
        new(0xcdc7, 0xcd45),
        new(0xcdc9, CommonEnemyInstructionCodes.Goto),
        new(0xcdcb, GoldenTorizoCombatInstructionPointers.WalkingRightLeftLeg),
        new(0xcdcd, TorizoInstructionCodes.Instruction_GoldenTorizo_CallY_OrY2_ForAttack),
        new(0xcdcf, 0xcc57),
        new(0xcdd1, 0xccdb),
        new(0xcdd3, CommonEnemyInstructionCodes.Goto),
        new(0xcdd5, GoldenTorizoCombatInstructionPointers.WalkingRightRightLeg),
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static GoldenTorizoJumpLandingMechanicsWord MechanicsWord(int index) => Words[index];

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        foreach (GoldenTorizoJumpLandingMechanicsWord word in Words)
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
        foreach (GoldenTorizoJumpLandingMechanicsWord word in Words)
            if (offset == word.Address ||
                offset == unchecked((ushort)(word.Address + 1)))
                return true;
        return false;
    }
}
