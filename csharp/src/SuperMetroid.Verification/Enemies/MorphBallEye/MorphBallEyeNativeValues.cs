namespace SuperMetroid.Verification.Enemies.MorphBallEye;

/// <summary>Native values the Morph Ball eye behavior check sets up. Only verification reads them.</summary>
internal static class MorphBallEyeNativeValues
{
    /// <summary>Parameter-two sign bit: a negative parameter makes <c>InitAI_Eye</c> build a mount, its low nibble the direction.</summary>
    internal const ushort MountParameter = 0x8000;
}
