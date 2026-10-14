using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="TourianStatueRomData"/> that only verification reads.</summary>
internal static class TourianStatueRomDataConstants
{
    /// <summary>$86:BA6A eye glow projectile definition.</summary>
    public const ushort EyeGlow = 0xba6a;
    /// <summary>$86:BA94 ascending soul projectile definition.</summary>
    public const ushort Soul = 0xba94;

    /// <summary>Provides Tourian statue eye glow and soul asset pointers for import checks.</summary>
    extension(TourianStatueRomData)
    {
        /// <inheritdoc cref="TourianStatueRomDataConstants.EyeGlow"/>
        internal static ushort EyeGlow => TourianStatueRomDataConstants.EyeGlow;
        /// <inheritdoc cref="TourianStatueRomDataConstants.Soul"/>
        internal static ushort Soul => TourianStatueRomDataConstants.Soul;
    }
}
