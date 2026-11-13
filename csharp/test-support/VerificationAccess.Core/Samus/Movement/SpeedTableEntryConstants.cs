namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Cartridge values of the records the cartridge stores <see cref="SamusHorizontalSpeeds"/> in, which only verification reads.</summary>
internal static class SpeedTableEntryConstants
{
    /// <summary>Size of one record in the cartridge's speed tables: three whole/fraction word pairs.</summary>
    public const int ByteCount = 12;
}
