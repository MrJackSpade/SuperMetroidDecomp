namespace SuperMetroid.Core.Game;

/// <summary>NTSC bank-$AA acid-motion operands used by the Lower Norfair statue.</summary>
internal static class ChozoStatueFxData
{
    /// <summary>$AA:E429, Instruction_Chozo_StartLoweringAcid: delay before motion.</summary>
    public const ushort LoweringDelay = 0x20;

    /// <summary>$AA:E42F, Instruction_Chozo_StartLoweringAcid: downward 8.8 velocity.</summary>
    public const ushort LoweringVelocity = 0x40;
}
