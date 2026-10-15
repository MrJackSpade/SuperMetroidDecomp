namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The random bits that spread a dying corpse's dust clouds.</summary>
internal static class CorpseDustDefinitions
{
    /// <summary>A Tourian or Sidehopper corpse's dust spread: $1A selects 0-26 pixels, even.</summary>
    internal const int SpreadBits = 0x001a;

    /// <summary>Dead Torizo's dust spread: 0-31 pixels.</summary>
    internal const int TorizoSpreadBits = 0x001f;
}
