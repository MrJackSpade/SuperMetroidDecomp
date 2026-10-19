namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where the bounded SpaceTime palette-copy overrun's source window starts.</summary>
/// <remarks>
/// With direct-page pointer zero, inherited animation Y=$912F enters the signed
/// $90:AD12 loop at $9131. Subsequent animation records start later in this same
/// window. Y advances by two through $FFFF, then wraps into live low WRAM and
/// exits at $0021. The window's bytes are bank-$80 program observed through bank $00;
/// the import pipeline reads them from the player's cartridge into <see cref="SamusOverreadCatalog"/>.
/// </remarks>
internal static class SpacetimeBeamCopyDefinitions
{
    /// <summary>$00:9131 mirrors $80:9131, the first byte read by the recorded $90:AD16 overrun.</summary>
    internal const int FirstSourceAddress = 0x009131;
}
