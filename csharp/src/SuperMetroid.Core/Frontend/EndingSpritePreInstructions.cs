namespace SuperMetroid.Core.Frontend;

/// <summary>Native bank-$8B pre-instructions for ending cinematic actors.</summary>
internal static class EndingSpritePreInstructions
{
    /// <summary>$8B:F35A waits for the flyaway function before initializing star motion.</summary>
    public const ushort WaitForFlyaway = 0xf35a;
    /// <summary>$8B:F375 accelerates stars horizontally and falls through to the text-handoff deletion check.</summary>
    public const ushort MoveFlyawayStars = 0xf375;
}
