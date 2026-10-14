using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="SamusHudRomData"/> that only verification reads.</summary>
internal static class SamusHudRomDataConstants
{
    /// <summary>$90:DD05 movement-type HUD handler pointer table.</summary>
    public const int MovementHandlers = 0x90dd05;
    /// <summary>$90:DDAA posture-transition flags: zero enters the standard handler.</summary>
    public const int TransitionFlags = 0x90ddaa;

    /// <summary>Provides HUD movement handlers and transition flags for Samus status display checks.</summary>
    extension(SamusHudRomData)
    {
        /// <inheritdoc cref="SamusHudRomDataConstants.MovementHandlers"/>
        internal static int MovementHandlers => SamusHudRomDataConstants.MovementHandlers;
        /// <inheritdoc cref="SamusHudRomDataConstants.TransitionFlags"/>
        internal static int TransitionFlags => SamusHudRomDataConstants.TransitionFlags;
    }
}
