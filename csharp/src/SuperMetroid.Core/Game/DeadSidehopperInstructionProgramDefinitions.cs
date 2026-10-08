using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the sidehopper corpse's hopping, idle, and
/// corpse programs. Their eleven spritemap operands remain live cartridge data.
/// </summary>
internal abstract class DeadSidehopperInstructionProgramDefinitions
{
    /// <summary><c>InstList_CorpseSidehopper_Alive_Hopping</c> at $A9:ECAC.</summary>
    internal const ushort AliveHopping = 0xecac;

    /// <summary><c>InstList_CorpseSidehopper_Alive_Idle</c> at $A9:ECE3.</summary>
    internal const ushort AliveIdle = 0xece3;

    /// <summary><c>InstList_CorpseSidehopper_Alive_Corpse</c> at $A9:ECE9.</summary>
    internal const ushort AliveCorpse = 0xece9;

    /// <summary><c>InstList_CorpseSidehopper_Alive_Dead</c> at $A9:ECEF.</summary>
    internal const ushort InitiallyDead = 0xecef;

    /// <summary>The embedded <c>Instruction_SidehopperCorpse_EndHop</c> word at $A9:ECCC.</summary>
    internal const ushort EndHopOpcode = 0xeccc;

    /// <summary>The terminal common sleep word of the hopping program at $A9:ECCE.</summary>
    internal const ushort HoppingSleepOpcode = 0xecce;

    /// <summary><c>Instruction_SidehopperCorpse_EndHop</c> at $A9:ECD0.</summary>
    private const ushort SidehopperCorpseEndHop = 0xecd0;

    /// <summary>Native program bank $A9.</summary>
    internal const byte Bank = 0xa9;

    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xecac),
        Entry(AliveHopping),
        Frame(2),
        Frame(4),
        Frame(5),
        Frame(48),
        Frame(5),
        Frame(4),
        Frame(5),
        Frame(4),
        Entry(EndHopOpcode),
        Op(SidehopperCorpseEndHop),
        Entry(HoppingSleepOpcode),
        Op(CommonEnemyInstructionCodes.Sleep),
        Origin(0xece3),
        Entry(AliveIdle),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(AliveCorpse),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep),
        Entry(InitiallyDead),
        Frame(1),
        Op(CommonEnemyInstructionCodes.Sleep));

    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Dead sidehopper instruction mechanics pointer $A9:{address:X4} is not compiled.");
}
