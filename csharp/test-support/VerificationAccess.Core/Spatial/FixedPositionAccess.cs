namespace SuperMetroid.Core.Spatial;

/// <summary>Fixture codec for constructing native 16.16 positions from WRAM words.</summary>
internal static class FixedPositionAccess
{
    /// <summary>Assembles spatial positions from the four native whole and fractional WRAM words.</summary>
    extension<TSpace>(SuperMetroid.Core.Spatial.FixedPosition<TSpace>) where TSpace : SuperMetroid.Core.Spatial.ISpatialSpace
    {
        /// <summary>Assembles both axes from native whole and subpixel words, as verifier fixtures state them.</summary>
        [SuperMetroid.Core.Spatial.SpatialComponents("Verifier codec: the four WRAM words of one 16.16 position.")]
        internal static SuperMetroid.Core.Spatial.FixedPosition<TSpace> FromStoredParts(ushort xWhole, ushort xFraction, ushort yWhole, ushort yFraction) =>
            new(new SuperMetroid.Core.Spatial.FixedCoordinate(xWhole, xFraction), new SuperMetroid.Core.Spatial.FixedCoordinate(yWhole, yFraction));
    }

    /// <summary>Sets one axis's stored fraction word, as verifier fixtures state a position's state.</summary>
    extension<TSpace>(SuperMetroid.Core.Spatial.FixedPosition<TSpace> position) where TSpace : SuperMetroid.Core.Spatial.ISpatialSpace
    {
        /// <summary>Replaces only the X fraction with the stored word <paramref name="fraction"/>, 0 through 65535.</summary>
        internal SuperMetroid.Core.Spatial.FixedPosition<TSpace> WithFractionX(int fraction) =>
            position.WithX(new SuperMetroid.Core.Spatial.FixedCoordinate(position.X.Whole, fraction));

        /// <summary>Replaces only the Y fraction with the stored word <paramref name="fraction"/>, 0 through 65535.</summary>
        internal SuperMetroid.Core.Spatial.FixedPosition<TSpace> WithFractionY(int fraction) =>
            position.WithY(new SuperMetroid.Core.Spatial.FixedCoordinate(position.Y.Whole, fraction));
    }
}
