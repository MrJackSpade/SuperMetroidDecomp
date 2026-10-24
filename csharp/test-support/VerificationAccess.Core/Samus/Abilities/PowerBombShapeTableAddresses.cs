namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Where the cartridge keeps the Power Bomb's authored ellipse frames in bank <c>$88</c>, which only verification reads.</summary>
internal static class PowerBombShapeTableAddresses
{
    /// <summary>The cartridge's shape-table bounds.</summary>
    extension(PowerBombExplosionDefinitions)
    {
        /// <summary>First yellow ellipse definition in bank <c>$88</c>.</summary>
        internal static ushort FirstYellowShape => 0x9f06;

        /// <summary>Exclusive end of yellow ellipse definitions.</summary>
        internal static ushort YellowShapeEnd => 0xa206;

        /// <summary>First white ellipse definition in bank <c>$88</c>.</summary>
        internal static ushort FirstWhiteShape => 0x9246;

        /// <summary>Exclusive end of white ellipse definitions.</summary>
        internal static ushort WhiteShapeEnd => 0x9f06;
    }
}
