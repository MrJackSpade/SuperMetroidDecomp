using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$AA Golden Torizo backward-jump landing lists at $AA:CDAF-CDD6.
/// Each landing chooses its foot/facing-specific orb or sonic attack, then
/// returns to the matching walking leg. These are control-flow operands, not
/// editable animation or sprite data.
/// </summary>
internal abstract class GoldenTorizoJumpLandingInstructionProgramDefinitions : IInstructionProgramCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>$AA:CDAF, first backward-jump landing choice for the left-facing, left-foot-forward case.</summary>
    internal const ushort Start = 0xcdaf;

    /// <summary>Four facing and forward-foot cases each contain five attack-choice and return-control words.</summary>
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
            3 => (ushort)CommonEnemyInstruction.Goto,
            _ => facingRight
                ? (rightFootForward ? GoldenTorizoCombatInstructionPointers.WalkingRightLeftLeg
                    : GoldenTorizoCombatInstructionPointers.WalkingRightRightLeg)
                : (rightFootForward ? GoldenTorizoCombatInstructionPointers.WalkingLeftLeftLeg
                    : GoldenTorizoCombatInstructionPointers.WalkingLeftRightLeg),
        };
        return new((ushort)(Start + index * sizeof(ushort)), value);
    }

    /// <summary>Accepts either byte of a landing control word in bank $AA, excluding neighboring animation data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xaa0000)
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
