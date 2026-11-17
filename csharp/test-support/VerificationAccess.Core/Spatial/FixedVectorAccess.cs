namespace SuperMetroid.Core.Spatial;

/// <summary>Fixture codec for constructing native 16.16 displacements from WRAM words.</summary>
internal static class FixedVectorAccess
{
    /// <summary>Assembles spatial displacements from the four native whole and fractional WRAM words.</summary>
    extension<TSpace>(SuperMetroid.Core.Spatial.FixedVector<TSpace>) where TSpace : SuperMetroid.Core.Spatial.ISpatialSpace
    {
        /// <summary>Assembles both axes from whole and fraction words, each 0 through 65535, as verifier fixtures state them.</summary>
        [SuperMetroid.Core.Spatial.SpatialComponents("Verifier codec: the four WRAM words of one 16.16 displacement.")]
        internal static SuperMetroid.Core.Spatial.FixedVector<TSpace> FromStoredParts(int xWhole, int xFraction, int yWhole, int yFraction) =>
            new(SignedSixteenSixteen.FromParts(CoordinateAxis.AsSigned(Ensure.BetweenInclusive(0, ushort.MaxValue, xWhole)), xFraction),
                SignedSixteenSixteen.FromParts(CoordinateAxis.AsSigned(Ensure.BetweenInclusive(0, ushort.MaxValue, yWhole)), yFraction));
    }
}
