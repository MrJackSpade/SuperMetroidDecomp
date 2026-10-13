namespace SuperMetroid.Core.Game;

/// <summary>The random gate of Mother Brain's low-X forward walk.</summary>
public static class MotherBrainForwardWalkDefinitions
{
    /// <summary>$A9:C6F3 masks the shared random word to twelve bits before the forward-walk gate.</summary>
    public const ushort ForwardWalkRandomMask = 0x0fff;
    /// <summary>$A9:C6F6 admits the top 64 values of the masked random word for the low-X forward-walk branch.</summary>
    public const ushort ForwardWalkRandomThreshold = 0x0fc0;
}
