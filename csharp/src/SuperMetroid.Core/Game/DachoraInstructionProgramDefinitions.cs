using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Dachora's body and four echo actors. The eighty-one
/// spritemap selections resolve compiled identities to installed artwork.
/// </summary>
internal abstract class DachoraInstructionProgramDefinitions
{
    /// <summary><c>InstList_Dachora_RunningLeft</c> at $A7:F345.</summary>
    internal const ushort RunningLeft = 0xf345;
    /// <summary><c>InstList_Dachora_RunningLeft_FastAnimation</c> at $A7:F361.</summary>
    internal const ushort RunningLeftFast = 0xf361;
    /// <summary><c>InstList_Dachora_RunningLeft_VeryFastAnimation</c> at $A7:F37D.</summary>
    internal const ushort RunningLeftVeryFast = 0xf37d;
    /// <summary><c>InstList_Dachora_Idling_FacingLeft</c> at $A7:F399.</summary>
    internal const ushort IdleLeft = 0xf399;
    /// <summary><c>InstList_Dachora_Blinking_FacingLeft</c> at $A7:F3C9.</summary>
    internal const ushort BlinkLeft = 0xf3c9;
    /// <summary><c>InstList_Dachora_Echo_FacingLeft</c> at $A7:F3F7.</summary>
    internal const ushort EchoLeft = 0xf3f7;
    /// <summary><c>InstList_Dachora_Falling_FacingLeft</c> at $A7:F3FF.</summary>
    internal const ushort FallingLeft = 0xf3ff;
    /// <summary><c>InstList_Dachora_RunningRight</c> at $A7:F407.</summary>
    internal const ushort RunningRight = 0xf407;
    /// <summary><c>InstList_Dachora_RunningRight_FastAnimation</c> at $A7:F423.</summary>
    internal const ushort RunningRightFast = 0xf423;
    /// <summary><c>InstList_Dachora_RunningRight_VeryFastAnimation</c> at $A7:F43F.</summary>
    internal const ushort RunningRightVeryFast = 0xf43f;
    /// <summary><c>InstList_Dachora_Idling_FacingRight</c> at $A7:F45B.</summary>
    internal const ushort IdleRight = 0xf45b;
    /// <summary><c>InstList_Dachora_Blinking_FacingRight</c> at $A7:F48B.</summary>
    internal const ushort BlinkRight = 0xf48b;
    /// <summary><c>InstList_Dachora_ChargeShinespark_FacingRight</c> at $A7:F4B3.</summary>
    internal const ushort ChargeRight = 0xf4b3;
    /// <summary><c>InstList_Dachora_Echo_FacingRight</c> at $A7:F4B9.</summary>
    internal const ushort EchoRight = 0xf4b9;
    /// <summary><c>InstList_Dachora_Falling_FacingRight</c> at $A7:F4C1.</summary>
    internal const ushort FallingRight = 0xf4c1;

    /// <summary>Native program bank $A7.</summary>
    internal const byte Bank = 0xa7;

    internal static readonly InstructionProgramLayout Layout = new(Bank,
        Origin(0xf345),
        Entry(RunningLeft),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Op((ushort)CommonEnemyInstruction.Goto, RunningLeft),
        Entry(RunningLeftFast),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)CommonEnemyInstruction.Goto, RunningLeftFast),
        Entry(RunningLeftVeryFast),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Op((ushort)CommonEnemyInstruction.Goto, RunningLeftVeryFast),
        Entry(IdleLeft),
        Frame(48),
        Frame(10),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(10),
        Op((ushort)CommonEnemyInstruction.Goto, IdleLeft),
        Entry(BlinkLeft),
        Frame(11),
        Frame(8),
        Frame(8),
        Frame(4),
        Frame(4),
        Frame(4),
        Frame(10),
        Frame(5),
        Frame(11),
        Op((ushort)CommonEnemyInstruction.Goto, BlinkLeft),
        Skip(6),
        Entry(EchoLeft),
        Frame(10),
        Op((ushort)CommonEnemyInstruction.Goto, EchoLeft),
        Entry(FallingLeft),
        Frame(5),
        Op((ushort)CommonEnemyInstruction.Goto, FallingLeft),
        Entry(RunningRight),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Frame(5),
        Op((ushort)CommonEnemyInstruction.Goto, RunningRight),
        Entry(RunningRightFast),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Frame(3),
        Op((ushort)CommonEnemyInstruction.Goto, RunningRightFast),
        Entry(RunningRightVeryFast),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Frame(1),
        Op((ushort)CommonEnemyInstruction.Goto, RunningRightVeryFast),
        Entry(IdleRight),
        Frame(48),
        Frame(10),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(7),
        Frame(10),
        Op((ushort)CommonEnemyInstruction.Goto, IdleRight),
        Entry(BlinkRight),
        Frame(11),
        Frame(8),
        Frame(8),
        Frame(4),
        Frame(4),
        Frame(4),
        Frame(10),
        Frame(5),
        Frame(11),
        Op((ushort)CommonEnemyInstruction.Goto, BlinkRight),
        Entry(ChargeRight),
        Frame(1),
        Op((ushort)CommonEnemyInstruction.Sleep),
        Entry(EchoRight),
        Frame(10),
        Op((ushort)CommonEnemyInstruction.Goto, EchoRight),
        Entry(FallingRight),
        Frame(5),
        Op((ushort)CommonEnemyInstruction.Goto, FallingRight));
    public static int PresentationWordCount => Layout.PresentationSlotCount;

    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);

    internal static ushort ReadMechanicsWord(ushort address) =>
        Layout.TryReadMechanicsWord(address, out ushort value) ? value :
            throw new InvalidDataException(
                $"Dachora instruction mechanics pointer $A7:{address:X4} is not compiled.");
}
