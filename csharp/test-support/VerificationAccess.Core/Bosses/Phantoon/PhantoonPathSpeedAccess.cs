namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// The stored words of <see cref="PhantoonPathSpeed"/> that only verification reads (#2555): checks compare the speed with
/// the cartridge's whole and fraction words, which the game holds as one typed speed.
/// </summary>
internal static class PhantoonPathSpeedAccess
{
    /// <summary>Reads a path speed's stored words.</summary>
    extension(PhantoonPathSpeed speed)
    {
        /// <summary>The speed in places an update, fraction included, as its fixed-point form holds it.</summary>
        internal SignedSixteenSixteen InFixedPoint => speed.Places;

        /// <summary>The fraction word.</summary>
        internal ushort Fraction => speed.InFixedPoint.Fraction;

        /// <summary>The signed whole word.</summary>
        internal int WholePixels => speed.InFixedPoint.WholePixels;

        /// <summary>The whole word read unsigned.</summary>
        internal ushort UnsignedWholePixels => speed.InFixedPoint.UnsignedWholePixels;
    }
}
