namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The stored form of <see cref="YappingMawCurl"/> that only verification reads (#2555): checks compare the curl with the
/// cartridge's whole and fraction words, which the game holds as one typed curl.
/// </summary>
internal static class YappingMawCurlAccess
{
    /// <summary>Reads a curl's stored form.</summary>
    extension(YappingMawCurl curl)
    {
        /// <summary>The curl in angle units, fraction included, as its fixed-point form holds it.</summary>
        internal SignedSixteenSixteen InFixedPoint => PrivateState.ValueField<YappingMawCurl, SignedSixteenSixteen>(curl, "_units");
    }
}
