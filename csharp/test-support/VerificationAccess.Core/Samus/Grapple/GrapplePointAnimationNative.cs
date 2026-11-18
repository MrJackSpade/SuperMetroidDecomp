using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// Verification's native view of the grapple endpoint and rope animations: the bank-$9A tiles each endpoint frame shows,
/// and the endpoint and rope as the original's frame number and countdown (#2388).
/// </summary>
internal static class GrapplePointAnimationNative
{
    /// <summary>Bank $9A, which holds the grapple character graphics.</summary>
    internal const int CharacterDataBank = 0x9a0000;

    /// <summary>$9B:C342: the bank-$9A tile source of <paramref name="frame"/>, from $9A:8200 in $0200-byte frames within the bank.</summary>
    internal static int SourceAddress(int frame) => 0x9a0000 | unchecked((ushort)(0x8200 + frame * 0x200));

    /// <summary>Reads and sets the endpoint and rope as the original counts them.</summary>
    extension(SamusGrappleState self)
    {
        /// <summary>The endpoint frame shown, counted from the first of its four.</summary>
        internal int PointAnimationFrame
        {
            get => GrappleSheetDefinitions.Endpoint.ToList().IndexOf(self.Endpoint.Showing);
            set => self.Endpoint.ShowAt(GrappleAnimations.Endpoint, new(0, Ensure.BetweenInclusive(0, GrappleSheetDefinitions.Endpoint.Count - 1, value)),
                self.Endpoint.UpdatesLeft);
        }

        /// <summary>
        /// The endpoint's countdown as the original keeps it: the draws its frame still shows less one, so the draw that
        /// takes it below zero moves the endpoint on; a countdown already below zero moves it on at the next draw.
        /// </summary>
        internal WrappingCountdown PointAnimationTimer
        {
            get => WrappingCountdown.Start(self.Endpoint.UpdatesLeft - 1);
            set => self.Endpoint.ShowAt(GrappleAnimations.Endpoint, new(0, self.PointAnimationFrame), value.IsNegative ? 1 : value.RawValue + 1);
        }

        /// <summary>The rope draws its pattern's picture still shows for, counting the next.</summary>
        internal int RopeAnimationTimer => self.Rope.UpdatesLeft;
    }
}
