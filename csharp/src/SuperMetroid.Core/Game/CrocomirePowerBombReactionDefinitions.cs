namespace SuperMetroid.Core.Game;

/// <summary>
/// Engine-owned power-bomb reactions from the component selectors of Crocomire's
/// fifty bank-$A4 body frames ($BFC4..$CAC4). Editable OAM cannot change mouth admission.
/// </summary>
internal static class CrocomirePowerBombReactionDefinitions
{
    internal static ushort ForFrame(ushort frame) => frame switch
    {
        0xbfc4 or 0xbff6 or 0xc028 or 0xc05a or 0xc08c or 0xc0be or 0xc0f0 or 0xc122 or 0xc154 or 0xc186 or 0xc1b8 or 0xc1ea or 0xc2ec or 0xc326 or 0xc360 or 0xc39a or 0xc3d4 or 0xc40e or 0xc448 or 0xc47a or 0xc4ac or 0xc4de or 0xc510 or 0xc542 or 0xc574 or 0xc6a4 or 0xc922 or 0xca7e or 0xca88 or 0xca92 or 0xca9c or 0xcaa6 or 0xcab0 or 0xcaba or 0xcac4 =>
            CrocomireInstructionProgramDefinitions.PowerBombReactionMouthNotOpen,
        0xc5ae or 0xc8e8 =>
            CrocomireInstructionProgramDefinitions.PowerBombReactionMouthPartiallyOpen,
        0xc5e8 or 0xc752 or 0xc78c or 0xc7c6 or 0xc800 or 0xc83a or 0xc874 or 0xc8ae or 0xc95c or 0xc996 or 0xc9d0 or 0xca0a or 0xca44 =>
            CrocomireInstructionProgramDefinitions.PowerBombReactionMouthFullyOpen,
        _ => throw new InvalidDataException($"Crocomire power-bomb frame $A4:{frame:X4} has no compiled reaction."),
    };
}
