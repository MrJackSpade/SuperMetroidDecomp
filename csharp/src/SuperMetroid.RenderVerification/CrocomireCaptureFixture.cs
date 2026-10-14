namespace SuperMetroid.RenderVerification;

/// <summary>Retail room and actor positions that trigger and complete Crocomire's death sequence.</summary>
internal static class CrocomireCaptureFixture
{
    /// <summary>Retail bank-$8F Crocomire room header, shared with the production death audit.</summary>
    internal const ushort Room = 0xa98d;
    /// <summary>Right arena viewport used by the retail Crocomire audit.</summary>
    internal const ushort CameraX = 0x0400;
    /// <summary>Stationary audit actor's initial world X.</summary>
    internal const ushort InitialSamusX = 0x0440;
    /// <summary>$A4:8D5E bridge threshold that starts the native death dispatcher.</summary>
    internal const ushort BridgeThreshold = 0x0640;
    /// <summary>Left-side actor position satisfying the native wall-return gate.</summary>
    internal const ushort ReturnSamusX = 0x0270;
}
