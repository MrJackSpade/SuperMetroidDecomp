namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Visual block indexes synthesized by bank-$84 PLM setup routines rather than read from
/// room level data. These are low ten-bit block-definition indexes, not complete packed
/// level words or bank-$84 draw-list pointers.
/// </summary>
internal static class RoomPlmVisualBlockDefinitions
{
    /// <summary>
    /// Bomb-block parent $058, synthesized by $84:CE83 on successful Samus contact.
    /// The live word immediately becomes this air-type visual; the restore word keeps
    /// the original collision nibble with the same replacement visual index.
    /// </summary>
    public const ushort CollisionBombParent = 0x0058;

    /// <summary>
    /// Speed-block parent visual block <c>$0B6</c>, synthesized by setup
    /// <c>$84:CDEA</c> before its authored animation list replaces it.
    /// </summary>
    public const ushort SpeedBoosterParent = 0x00b6;

    /// <summary>
    /// Contact-crumble parent visual block `$0BC`, written by setup `$84:CE37` before its
    /// authored animation list replaces it.
    /// </summary>
    public const ushort ContactCrumbleParent = 0x00bc;

    /// <summary>Visual block $052: the shot block a beam break restores after its respawn delay.</summary>
    public const ushort ShotBlockParent = 0x0052;
    /// <summary>Visual block $057: the power-bomb block a power bomb restores after its respawn delay.</summary>
    public const ushort PowerBombBlockParent = 0x0057;
    /// <summary>Visual block $09F: the Super Missile block a Super Missile restores after its respawn delay.</summary>
    public const ushort SuperMissileBlockParent = 0x009f;
}
