namespace SuperMetroid.Core.Rooms;

/// <summary>Destination progression within a native 32-character-wide BG tilemap page, distinct from the camera axis that caused the update.</summary>
public enum TilemapDmaDirection
{
    /// <summary>Horizontal character sequence: VMAIN=$80 and destination address increments by one VRAM word after each transferred word.</summary>
    Row,
    /// <summary>Vertical character sequence: VMAIN=$81 and destination address increments by 32 VRAM words after each transferred word.</summary>
    Column,
}
