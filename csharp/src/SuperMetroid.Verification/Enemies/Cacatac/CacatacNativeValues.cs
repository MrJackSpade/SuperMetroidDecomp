namespace SuperMetroid.Verification.Enemies.Cacatac;

/// <summary>Native Cacatac values the Cacatac behavior check sets up or expects. Only verification reads them.</summary>
internal static class CacatacNativeValues
{
    /// <summary>Library-two sound $34, which <c>Instruction_Cacatac_PlaySpikesSFX</c> at $A2:9F2A queues.</summary>
    internal const ushort SpikeSound = 0x0034;

    /// <summary>A parameter one whose nonzero high byte makes <c>InitAI_Cacatac</c> start upright, with direction zero (left).</summary>
    internal const ushort UpsideUpParameter = 0x0100;
}
