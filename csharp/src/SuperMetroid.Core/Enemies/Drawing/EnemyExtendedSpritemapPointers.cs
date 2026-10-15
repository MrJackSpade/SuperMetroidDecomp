namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>How a 16-bit enemy spritemap pointer marks an extended (multi-part, multibox) spritemap.</summary>
internal static class EnemyExtendedSpritemapPointers
{
    /// <summary>Bit 15 set: native multibox collision accepts only these negative pointers as extended spritemaps.</summary>
    internal static bool IsExtended(ushort pointer) => (pointer & 0x8000) != 0;
}
