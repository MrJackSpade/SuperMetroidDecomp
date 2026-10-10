namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$AA Golden Torizo backward-jump landing lists at $AA:CDAF-CDD6.
/// Each landing chooses its foot/facing-specific orb or sonic attack, then
/// returns to the matching walking leg. These are control-flow operands, not
/// editable animation or sprite data.
/// </summary>
internal abstract class GoldenTorizoJumpLandingInstructionProgramDefinitions : IInstructionProgramCatalog, ICompiledMechanicsByteProbe
{
    internal const ushort Start = 0xcdaf;

    public static int MechanicsWordCount => 4 * 5;

    /// <summary>
    /// $AA:CDAF-CDD6: each of four facing/forward-foot cases calls an orb or sonic
    /// attack and returns to the opposite moving leg. Five words comprise each case.
    /// </summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int landing = index / 5;
        bool facingRight = landing >= 2;
        bool rightFootForward = ((landing & 1) != 0) != facingRight;
        ushort value = (index % 5) switch
        {
            0 => (ushort)TorizoInstruction.Instruction_GoldenTorizo_CallY_OrY2_ForAttack,
            1 => facingRight
                ? (rightFootForward ? GoldenTorizoRightOrbInstructionProgramDefinitions.Start
                    : GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Start)
                : (rightFootForward ? GoldenTorizoLeftOrbInstructionProgramDefinitions.Start
                    : GoldenTorizoLeftOrbInstructionProgramDefinitions.LeftFootForward),
            2 => facingRight
                ? (rightFootForward ? GoldenTorizoRightSonicInstructionProgramDefinitions.RightFootForward
                    : GoldenTorizoRightSonicInstructionProgramDefinitions.Start)
                : (rightFootForward
                    ? TorizoInstructionProgramDefinitions.InstList_GoldenTorizo_SonicBooms_FacingLeft_RightFootFwd_0
                    : TorizoInstructionProgramDefinitions.InstList_GoldenTorizo_SonicBooms_FacingLeft_LeftFootFwd_0),
            3 => CommonEnemyInstructionCodes.Goto,
            _ => facingRight
                ? (rightFootForward ? GoldenTorizoCombatInstructionPointers.WalkingRightLeftLeg
                    : GoldenTorizoCombatInstructionPointers.WalkingRightRightLeg)
                : (rightFootForward ? GoldenTorizoCombatInstructionPointers.WalkingLeftLeftLeg
                    : GoldenTorizoCombatInstructionPointers.WalkingLeftRightLeg),
        };
        return new((ushort)(Start + index * sizeof(ushort)), value);
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000)
            return false;
        ushort offset = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            InstructionMechanicsWord word = MechanicsWord(index);
            if (offset == word.Address || offset == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
