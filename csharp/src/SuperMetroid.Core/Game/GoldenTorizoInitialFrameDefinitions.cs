namespace SuperMetroid.Core.Game;

/// <summary>Engine-owned collision for the first Golden Torizo extended frame.</summary>
internal static class GoldenTorizoInitialFrameDefinitions
{
    internal const byte Bank = 0xaa;

    /// <summary>Initial seated extended spritemap at $AA:AA30.</summary>
    internal const ushort Frame = 0xaa30;

    /// <summary>Initial seated hitbox list at $AA:8812.</summary>
    internal const ushort HitboxList = 0x8812;

    internal const ushort ComponentCount = 1;
    internal const ushort HitboxCount = 1;
    internal const short Left = -11;
    internal const short Top = -38;
    internal const short Right = 11;
    internal const short Bottom = 39;

    /// <summary>Common Torizo touch AI at $AA:C977.</summary>
    internal const ushort TouchAi = 0xc977;

    /// <summary>Torizo stand-up/sit-down shot AI at $AA:C9C2.</summary>
    internal const ushort ShotAi = 0xc9c2;
}
