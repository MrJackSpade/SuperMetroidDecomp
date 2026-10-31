namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Cinematic actor slots, expressed as indices rather than byte offsets.</summary>
internal static class EndingSpriteSlots
{
    /// <summary>Allocation scans sixteen slots from the last down to zero.</summary>
    public const int Count = 16;
    /// <summary>The supernova spawns the right stars into slot zero.</summary>
    public const int RightStars = 0;
    /// <summary>The supernova forces the left stars into slot one.</summary>
    public const int LeftStars = 1;
    /// <summary>The finale forces the afterglow into slot three, ahead of both starfields in drawing order.</summary>
    public const int Afterglow = 3;
}
