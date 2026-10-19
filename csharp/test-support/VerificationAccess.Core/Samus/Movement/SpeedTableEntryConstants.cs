namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Cartridge values and helpers of <see cref="SpeedTableEntry"/> that only verification reads.</summary>
internal static class SpeedTableEntryConstants
{
    /// <summary>Size of one record in the cartridge's speed tables: three whole/fraction word pairs.</summary>
    public const int ByteCount = 12;
}
