namespace SuperMetroid.Core.Compatibility;

/// <summary>Verification access to <see cref="DraygonRegrabSpiralLift"/>: a center a check states, built as the fight builds one.</summary>
internal static class DraygonRegrabSpiralLiftAccess
{
    /// <summary>States a spiral center through the fight's own steps.</summary>
    extension(DraygonRegrabSpiralLift)
    {
        /// <summary>
        /// A center at <paramref name="center"/>, whose horizontal position is whole: a fight's first center lifted by the
        /// vertical part of a pixel <paramref name="center"/> carries, then a grab's spiral started on its whole position,
        /// which keeps that part.
        /// </summary>
        internal static DraygonRegrabSpiralLift At(FixedPosition<RoomPixels> center) =>
            default(DraygonRegrabSpiralLift).Lifted(SignedSixteenSixteen.FromParts(0, center.Y.Fraction)).StartSpiral(center.Whole);
    }
}
