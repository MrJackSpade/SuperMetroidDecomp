namespace SuperMetroid.Core.Hardware;

/// <summary>How VMADD advances after each word of a VRAM port transfer (VMAIN bit 0-1).</summary>
public enum VramIncrement
{
    /// <summary>VMAIN=$80: one word, along a tilemap row or through consecutive character data.</summary>
    Row,

    /// <summary>VMAIN=$81: 32 words, down one 32-wide tilemap column.</summary>
    Column,
}
