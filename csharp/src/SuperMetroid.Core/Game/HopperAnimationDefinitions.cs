namespace SuperMetroid.Core.Game;

/// <summary>Animation dispatch shared by Sidehoppers and Dessgeegas.</summary>
internal static class HopperAnimationDefinitions
{
    /// <summary>$A3:AAC2-AAE1: selects the species/size program for its orientation and movement phase.</summary>
    internal static ushort InstructionList(ushort variantIndex, bool upsideDown, bool jumping) =>
        HopperInstructionProgramDefinitions.InstructionList(variantIndex, upsideDown, jumping);
}
