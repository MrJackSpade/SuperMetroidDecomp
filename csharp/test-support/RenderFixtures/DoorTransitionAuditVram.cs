/// <summary>VRAM regions inspected by the door-transition visual regression.</summary>
internal static class DoorTransitionAuditVram
{
    /// <summary>First word of the live two-page BG1 room tilemap at VRAM $5000.</summary>
    public const ushort Bg1TilemapFirstWord = 0x5000;

    /// <summary>Two 32x32 BG1 tilemap pages, expressed as 16-bit words.</summary>
    public const ushort Bg1TilemapWordCount = 0x0800;
}
