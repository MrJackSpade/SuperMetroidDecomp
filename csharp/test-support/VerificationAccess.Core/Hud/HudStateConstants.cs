
namespace SuperMetroid.Core.Hud;

/// <summary>Cartridge values of <see cref="HudState"/> that only verification reads.</summary>
internal static class HudStateConstants
{
    /// <summary>$7E:C608, native mutable HUD tilemap buffer.</summary>
    public const int WorkRamAddress = 0x7ec608;

    /// <summary>Exposes the native HUD tilemap buffer address to verification callers.</summary>
    extension(HudState)
    {
        /// <inheritdoc cref="HudStateConstants.WorkRamAddress"/>
        internal static int WorkRamAddress => HudStateConstants.WorkRamAddress;
    }
}
