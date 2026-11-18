using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// What a frame boundary before the gameplay runtime's first room latches (#2498): no room shows, only the HUD's images
/// as gameplay last placed them.
/// </summary>
/// <param name="Hud">The HUD's images this frame boundary latched.</param>
public sealed record LatchBeforeFirstRoom(ShownHud Hud) : GameplayDisplayLatch(Hud)
{
    /// <summary>The latch in effect before any frame boundary: no room, and no HUD image placed.</summary>
    public static LatchBeforeFirstRoom NothingLatched { get; } = new(ShownHud.Nothing);
}
