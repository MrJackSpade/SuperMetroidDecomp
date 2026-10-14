namespace SuperMetroid.Core.Rendering;

/// <summary>Exclusive on-disk composition discriminants. Never renumber existing entries.</summary>
public enum RenderPacketKind : byte
{
    /// <summary>A single solid color fill.</summary>
    Solid = 1,

    /// <summary>A Mode 7 background with objects.</summary>
    Mode7Obj = 2,

    /// <summary>PPU memory plus an ordered list of composed layers.</summary>
    Layered = 3,
}
