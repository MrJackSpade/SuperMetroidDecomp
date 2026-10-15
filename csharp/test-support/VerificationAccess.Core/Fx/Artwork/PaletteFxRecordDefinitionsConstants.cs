using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Cartridge values and helpers of <see cref="PaletteFxRecordDefinitions"/> that only verification reads.</summary>
internal static class PaletteFxRecordDefinitionsConstants
{
    /// <summary>The records the programs show.</summary>
    internal static int RecordCount => PaletteFxRecordDefinitions.Records.Length;
}
