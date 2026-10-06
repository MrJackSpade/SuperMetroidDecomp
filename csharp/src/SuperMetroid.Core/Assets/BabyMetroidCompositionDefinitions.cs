namespace SuperMetroid.Core.Assets;

/// <summary>Native Baby Metroid composition identities and record geometry.</summary>
internal static class BabyMetroidCompositionDefinitions
{
    /// <summary>$A9:F9A8/FA40/FAD8: thirty five-byte records in each Baby Metroid pose, arranged as fifteen reflected pairs.</summary>
    internal const int NativePartCount = 30;
    /// <summary>$A9:F9A8, Spritemap_BabyMetroid_0; following two counted records have the same thirty-part extent.</summary>
    internal const int FirstPose = 0xa9f9a8;
    /// <summary>Native two-byte count followed by thirty five-byte OBJ records.</summary>
    internal const int PoseStride = 2 + NativePartCount * 5;
}
