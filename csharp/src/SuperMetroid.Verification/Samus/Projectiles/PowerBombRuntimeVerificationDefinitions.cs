namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Retail identities and fixture coordinates for Power Bomb renderer integration.</summary>
internal static class PowerBombRuntimeVerificationDefinitions
{
    /// <summary>
    /// Alpha Power Bomb room <c>$01/$26</c> at <c>$8F:A3AE</c>, where the recorded
    /// issue-#288 playthrough demonstrated an invisible Power Bomb explosion.
    /// </summary>
    public const ushort AlphaPowerBombRoomHeader = 0xa3ae;

    /// <summary>Screen-space X center used by the controlled active HDMA owner.</summary>
    public const ushort ScreenCenterX = 0x0080;

    /// <summary>Screen-space Y center below the 32-pixel gameplay HUD.</summary>
    public const ushort ScreenCenterY = 0x0080;
}
