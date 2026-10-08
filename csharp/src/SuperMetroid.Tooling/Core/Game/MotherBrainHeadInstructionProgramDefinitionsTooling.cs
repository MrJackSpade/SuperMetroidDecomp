using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainHeadInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
internal static class MotherBrainHeadInstructionProgramDefinitionsTooling
{
    /// <summary>$A9:9B7F, stretching, recoil, initial, decapitated and drool lists.</summary>
    public const ushort EarlyStart = 0x9b7f;
    /// <summary>$A9:9C63, last head-list word before native code at $A9:9C65.</summary>
    public const ushort EarlyEnd = 0x9c63;
    /// <summary>$A9:9C77, rainbow-beam hold and phase-two neutral lists.</summary>
    public const ushort RainbowAndNeutralPhaseTwoStart = 0x9c77;
    /// <summary>$A9:9CAB, last phase-two neutral branch operand.</summary>
    public const ushort RainbowAndNeutralPhaseTwoEnd = 0x9cab;
    /// <summary>$A9:9CB9, phase-three neutral head list.</summary>
    public const ushort NeutralStart = 0x9cb9;
    /// <summary>$A9:9D0B, last unused phase-three neutral list word.</summary>
    public const ushort NeutralRegionEnd = 0x9d0b;
    /// <summary>$A9:9D25, corpse and phase-two/three onion-ring attack lists.</summary>
    public const ushort CorpseAndRingsStart = 0x9d25;
    /// <summary>$A9:9DF5, last four-ring branch operand before native code.</summary>
    public const ushort CorpseAndRingsEnd = 0x9df5;
    /// <summary>$A9:9ECC, phase-two/three bomb and laser head lists.</summary>
    public const ushort BombAndLaserStart = 0x9ecc;
    /// <summary>$A9:9F44, last laser-list branch operand before native code.</summary>
    public const ushort BombAndLaserEnd = 0x9f44;
    /// <summary>$A9:9F6C, rainbow-beam charging head list.</summary>
    public const ushort RainbowChargeStart = 0x9f6c;
    /// <summary>$A9:9F82, last charging-list loop operand before native code.</summary>
    public const ushort RainbowChargeEnd = 0x9f82;
}
