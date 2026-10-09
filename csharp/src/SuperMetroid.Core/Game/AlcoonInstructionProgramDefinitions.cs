namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Alcoon's walking, fire-volley, and airborne
/// programs. Their interleaved spritemap operands select installed presentation frames.
/// </summary>
internal abstract class AlcoonInstructionProgramDefinitions
{
    /// <summary><c>InstList_Alcoon_FacingLeft_Walking_0</c> at $A8:DBE7.</summary>
    internal const ushort WalkingLeft = 0xdbe7;
    /// <summary>The first timed frame in the left-walking loop at $A8:DBE9.</summary>
    internal const ushort WalkingLeftFirstFrame = 0xdbe9;
    /// <summary><c>InstList_Alcoon_FacingLeft_SpawnFireballs</c> at $A8:DC03.</summary>
    internal const ushort FireLeft = 0xdc03;
    /// <summary><c>InstList_Alcoon_FacingLeft_Airborne_LookingUp</c> at $A8:DC4B.</summary>
    internal const ushort AirborneLeftLookingUp = 0xdc4b;
    /// <summary><c>InstList_Alcoon_FacingLeft_Airborne_LookingForward</c> at $A8:DC51.</summary>
    internal const ushort AirborneLeftLookingForward = 0xdc51;
    /// <summary><c>InstList_Alcoon_FacingRight_Walking_0</c> at $A8:DC57.</summary>
    internal const ushort WalkingRight = 0xdc57;
    /// <summary>The first timed frame in the right-walking loop at $A8:DC59.</summary>
    internal const ushort WalkingRightFirstFrame = 0xdc59;
    /// <summary><c>InstList_Alcoon_FacingRight_SpawnFireballs</c> at $A8:DC73.</summary>
    internal const ushort FireRight = 0xdc73;
    /// <summary><c>InstList_Alcoon_FacingRight_Airborne_LookingUp</c> at $A8:DCBB.</summary>
    internal const ushort AirborneRightLookingUp = 0xdcbb;
    /// <summary><c>InstList_Alcoon_FacingRight_Airborne_LookingForward</c> at $A8:DCC1.</summary>
    internal const ushort AirborneRightLookingForward = 0xdcc1;

    /// <summary>Identifies aligned words in the compiled Alcoon program that are visual operands rather than mechanics.</summary>
    /// <param name="address">The bank-$A8 address of the candidate instruction word.</param>
    /// <returns><see langword="true"/> when the address is in the compiled span and not a mechanics word.</returns>
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - WalkingLeft;
        // Every aligned word in this complete program span is either control or visual.
        return (uint)offset < 224 && (offset & 1) == 0 && !TryReadMechanicsWord(address, out _);
    }

    /// <summary>Returns a compiled engine-control or timing word from Alcoon's instruction programs.</summary>
    /// <param name="address">The bank-$A8 address to resolve.</param>
    /// <returns>The mechanics word stored at the address.</returns>
    /// <exception cref="InvalidDataException">The address does not identify a compiled mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address) =>
        TryReadMechanicsWord(address, out ushort value) ? value : throw new InvalidDataException(
            $"Alcoon instruction mechanics pointer $A8:{address:X4} is not compiled.");

    /// <summary>Attempts to resolve a compiled engine-control or timing word without throwing for unrecognized addresses.</summary>
    /// <param name="address">The bank-$A8 address to inspect.</param>
    /// <param name="value">Receives the mechanics word when the address is recognized; otherwise receives zero.</param>
    /// <returns><see langword="true"/> when the address contains a compiled mechanics word.</returns>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        value = 0;
        int offset = address - WalkingLeft;
        if ((uint)offset >= 224 || (offset & 1) != 0) return false;
        int local = offset % 112;
        if (local < 24)
        {
            if (local % 6 == 0)
                value = local == 18
                    ? EnemyInstructionCodePointers.Instruction_Alcoon_DecrementStepCounter_MoveHorizontally
                    : EnemyInstructionCodePointers.Instruction_Alcoon_MoveHorizontally_TurnIfWallCollision;
            else if (local % 6 == 2) value = 10;
            else return false;
            return true;
        }
        if (local < 28)
        {
            value = local == 24 ? CommonEnemyInstructionCodes.Goto
                : offset < 112 ? WalkingLeft : WalkingRight;
            return true;
        }
        if (local < 94)
        {
            int volley = (local - 28) / 22;
            int stage = (local - 28) % 22;
            // Extend wing, open mouth, aim, wind up, fire, recover; repeat for three arcs.
            value = stage switch
            {
                0 => (ushort)(volley == 0 ? 20 : 10),
                4 => 9,
                8 => 16,
                12 => 3,
                16 => volley switch
                {
                    0 => EnemyInstructionCodePointers.Instruction_Alcoon_SpawnAlcoonFireballHorizontally,
                    1 => EnemyInstructionCodePointers.Instruction_Alcoon_SpawnAlcoonFireballUpward,
                    _ => EnemyInstructionCodePointers.Instruction_Alcoon_SpawnAlcoonFireballDownward,
                },
                18 => (ushort)(volley == 2 ? 40 : 10),
                _ => 0,
            };
            return value != 0;
        }
        value = local switch
        {
            94 => EnemyInstructionCodePointers.Instruction_Alcoon_StartWalking,
            96 => 1, // Native trailing frame is skipped by StartWalking, but remains readable.
            100 or 106 => 0x7fff,
            104 or 110 => CommonEnemyInstructionCodes.Sleep,
            _ => 0,
        };
        return value != 0;
    }
}
