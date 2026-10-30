namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How Mother Brain's rainbow beam hides its push on Samus from the camera (#1470). Each push moves Samus by a whole
/// number of 1/256ths of a pixel and copies only that coarse part of her new sub-pixel position into the camera's
/// record of where she was, leaving that record's finer part as it was; at an arena edge it copies her whole, cleared,
/// sub-pixel position instead. The camera's speed is the distance between that record and Samus, sub-pixels included,
/// so any fine sub-pixel movement Samus made earlier in the update still reaches the camera instead of being hidden
/// with the push. Its effect on the camera's position is gameplay-observable.
/// </summary>
/// <remarks>
/// Written by the rainbow beam's forced movement during the enemy pass; consulted, and cleared, when the update's
/// scrolling measures how far Samus moved. Evidence: the original routines at <c>$A9:BBFD-$BC75</c> store one byte
/// of the sub-pixel position into the previous-position record and the full word at the arena clamps, and the camera's
/// speed routine subtracts the full sub-pixel words; the 100% movie's fall into the floor clamp established the
/// complete case. The rainbow-beam movement check confirms both cases.
/// </remarks>
[CompatibilityService("Mother Brain's rainbow-beam push hides only the coarse part of its sub-pixel movement from the camera's speed")]
public sealed class RainbowBeamCameraCheckpoint
{
    /// <summary>The horizontal sub-pixel the push recorded this update.</summary>
    private HiddenFraction _horizontal;

    /// <summary>The vertical sub-pixel the push recorded this update.</summary>
    private HiddenFraction _vertical;

    /// <summary>Hides the coarse part of a horizontal push that left Samus's sub-pixel at <paramref name="fraction"/>.</summary>
    /// <param name="fraction">Samus's horizontal sub-pixel position after the push, 0 through 65535.</param>
    public void HideCoarseHorizontalMovement(int fraction) => _horizontal = _horizontal.WithCoarse(fraction);

    /// <summary>Hides the coarse part of a vertical push that left Samus's sub-pixel at <paramref name="fraction"/>.</summary>
    /// <param name="fraction">Samus's vertical sub-pixel position after the push, 0 through 65535.</param>
    public void HideCoarseVerticalMovement(int fraction) => _vertical = _vertical.WithCoarse(fraction);

    /// <summary>Hides a horizontal push completely: the record takes Samus's whole sub-pixel position.</summary>
    /// <param name="fraction">Samus's horizontal sub-pixel position, 0 through 65535.</param>
    public void HideHorizontalMovement(int fraction) => _horizontal = HiddenFraction.Complete(fraction);

    /// <summary>Hides a vertical push completely: the record takes Samus's whole sub-pixel position.</summary>
    /// <param name="fraction">Samus's vertical sub-pixel position, 0 through 65535.</param>
    public void HideVerticalMovement(int fraction) => _vertical = HiddenFraction.Complete(fraction);

    /// <summary>
    /// The camera's record of where Samus was, with this update's hidden pushes applied to its sub-pixels; the
    /// pushes are then forgotten.
    /// </summary>
    /// <param name="checkpoint">Where the camera last recorded Samus.</param>
    public FixedPosition<RoomPixels> Consume(FixedPosition<RoomPixels> checkpoint)
    {
        var result = new FixedPosition<RoomPixels>(
            FixedCoordinate.Of(checkpoint.X.Whole, _horizontal.ApplyTo(checkpoint.X.Fraction)),
            FixedCoordinate.Of(checkpoint.Y.Whole, _vertical.ApplyTo(checkpoint.Y.Fraction)));
        _horizontal = default;
        _vertical = default;
        return result;
    }

    /// <summary>How much of one axis's sub-pixel record the pushes replaced this update.</summary>
    private enum HiddenExtent
    {
        /// <summary>No push this update: the record keeps its sub-pixel.</summary>
        None,

        /// <summary>The record's coarse sub-pixel is replaced; its fine part stays.</summary>
        Coarse,

        /// <summary>The record's whole sub-pixel is replaced.</summary>
        Complete,
    }

    /// <summary>One axis's hidden sub-pixel: what part of the record it replaces, and with what.</summary>
    /// <param name="Extent">How much of the record is replaced.</param>
    /// <param name="Fraction">The replacing sub-pixel, 0 through 65535; only its coarse part counts for a coarse extent.</param>
    private readonly record struct HiddenFraction(HiddenExtent Extent, int Fraction)
    {
        /// <summary>A sub-pixel's coarse steps: the 1/256ths of a pixel the push moves Samus by.</summary>
        private const int FineStepsPerCoarseStep = 256;

        /// <summary>The whole sub-pixel <paramref name="fraction"/> replaces the record's.</summary>
        public static HiddenFraction Complete(int fraction) =>
            new(HiddenExtent.Complete, Ensure.BetweenInclusive(0, ushort.MaxValue, fraction));

        /// <summary>
        /// The coarse part of <paramref name="fraction"/> also replaces the record's; a fine part an earlier complete
        /// replacement this update recorded stays.
        /// </summary>
        public HiddenFraction WithCoarse(int fraction)
        {
            Ensure.BetweenInclusive(0, ushort.MaxValue, fraction);
            return Extent == HiddenExtent.Complete
                ? new(HiddenExtent.Complete, Coarse(fraction) + Fine(Fraction))
                : new(HiddenExtent.Coarse, Coarse(fraction));
        }

        /// <summary>The record's sub-pixel <paramref name="recorded"/> with this replacement applied.</summary>
        public int ApplyTo(int recorded) => Extent switch
        {
            HiddenExtent.None => recorded,
            HiddenExtent.Coarse => Fraction + Fine(recorded),
            HiddenExtent.Complete => Fraction,
            _ => throw new InvalidOperationException($"Undefined hidden extent {Extent}."),
        };

        /// <summary>The whole coarse steps of a sub-pixel position.</summary>
        private static int Coarse(int fraction) => fraction - Fine(fraction);

        /// <summary>What a sub-pixel position holds below its coarse steps.</summary>
        private static int Fine(int fraction) => fraction % FineStepsPerCoarseStep;
    }
}
