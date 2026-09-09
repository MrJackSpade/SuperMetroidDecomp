namespace SuperMetroid.Core.Frontend;

/// <summary>Native bank-$8B cinematic actor slots, expressed as indices rather than byte offsets.</summary>
internal static class EndingSpriteSlots
{
    /// <summary>$8B:938A scans sixteen two-byte slots from byte offset 30 down to zero.</summary>
    public const int Count = 16;
    /// <summary>$8B:F2B7 uses SpawnCinematicSpriteObjectToR18 with byte offset 0 for right stars.</summary>
    public const int RightStars = 0;
    /// <summary>$8B:F2B7 forces left stars into byte offset 2.</summary>
    public const int LeftStars = 1;
    /// <summary>$8B:F2FA forces the afterglow into byte offset 6, ahead of both starfields in OAM.</summary>
    public const int Afterglow = 3;
}
