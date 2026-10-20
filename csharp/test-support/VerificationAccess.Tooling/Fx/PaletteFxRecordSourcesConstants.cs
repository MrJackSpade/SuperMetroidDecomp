namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Cartridge values and helpers of <see cref="PaletteFxRecordSources"/> that only verification reads.</summary>
internal static class PaletteFxRecordSourcesConstants
{
    /// <summary>The records the programs show.</summary>
    internal static int RecordCount => PaletteFxRecordSources.Addresses.Count;
}
