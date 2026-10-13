namespace SuperMetroid.Core.Game;

/// <summary>Shared bank-$A9 enemy instruction words that Mother Brain bytecode also uses.</summary>
internal static class MotherBrainInstructionCodes
{
    /// <summary><c>Instruction_CommonA9_Sleep</c> at $A9:812F.</summary>
    public const ushort Instruction_CommonA9_Sleep = 0x812f;
}

/// <summary>The ceiling-tube pre-instruction states stored in a top tube projectile's variable zero, in bank $86 order.</summary>
internal enum MotherBrainTopTubeFunction : ushort
{
    /// <summary>$86:CBEA: puffs dust where the tube breaks away, then falls.</summary>
    SpawnDust = 1,
    /// <summary>$86:CC08: falls until it lands.</summary>
    Fall,
}

/// <summary>The falling-tube main functions stored in a tube's VariableA, in bank $A9 order.</summary>
internal enum MotherBrainTubeFunction : ushort
{
    /// <summary><c>Function_MotherBrainTubes_NonMainTube</c> at $A9:8B88.</summary>
    NonMainTube = 1,

    /// <summary><c>Function_MotherBrainTubes_MainTube_WaitingToFall</c> at $A9:8BCB.</summary>
    MainTube_WaitingToFall,

    /// <summary><c>Function_MotherBrainTubes_MainTube_Falling</c> at $A9:8BD6.</summary>
    MainTube_Falling,
}
