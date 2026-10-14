namespace SuperMetroid.Core.Spatial;

/// <summary>Fixture codec for constructing native 16.16 positions from WRAM words.</summary>
internal static class FixedPositionAccess
{
    /// <summary>Assembles spatial positions from the four native whole and fractional WRAM words.</summary>
    extension<TSpace>(SuperMetroid.Core.Spatial.FixedPosition<TSpace>) where TSpace : SuperMetroid.Core.Spatial.ISpatialSpace
    {
        /// <summary>Assembles both axes from native whole and subpixel words, as verifier fixtures state them.</summary>
        [SuperMetroid.Core.Spatial.SpatialComponents("Verifier codec: the four WRAM words of one 16.16 position.")]
        internal static SuperMetroid.Core.Spatial.FixedPosition<TSpace> FromWords(ushort xWhole, ushort xFraction, ushort yWhole, ushort yFraction) =>
            new(new SuperMetroid.Core.Hardware.SnesFixedPosition(xWhole, xFraction), new SuperMetroid.Core.Hardware.SnesFixedPosition(yWhole, yFraction));
    }
}
