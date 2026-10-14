namespace SuperMetroid.Core.Rendering;

/// <summary>Exclusive on-disk layer discriminants. Never renumber existing entries.</summary>
internal enum RenderPacketLayerKind : byte
{
    /// <summary>Ordinary object pixels from the main screen.</summary>
    Obj = 1,
    /// <summary>Object pixels filtered to one native priority level.</summary>
    ObjPriority = 2,
    /// <summary>Four-bit background tilemap composition.</summary>
    Bg4Bpp = 3,
    /// <summary>Two-bit background tilemap composition.</summary>
    Bg2Bpp = 4,
    /// <summary>Affine Mode 7 background composition.</summary>
    Mode7 = 5,
    /// <summary>Additive fixed-color composition pass.</summary>
    FixedColorAdd = 6,
    /// <summary>BG2 visible through a bounded viewport.</summary>
    Bg2Viewport = 7,
    /// <summary>Gameplay background and object composition as one layer.</summary>
    OrdinaryGameplay = 8,
    /// <summary>Fixed-color additions selected per scanline.</summary>
    ScanlineColorAdd = 9,
    /// <summary>Message-box pixels composed over the scene.</summary>
    MessageBox = 10,
    /// <summary>Native background color-math operation.</summary>
    BgColorMath = 11,
    /// <summary>Gameplay-specific Mode 7 composition.</summary>
    Mode7Gameplay = 12,
    /// <summary>Background pixels added from the subscreen.</summary>
    BgSubscreenAdd = 13,
    /// <summary>Child scene clipped to an on-screen rectangle.</summary>
    WindowedScene = 14,
    /// <summary>Gameplay X-Ray layer composition.</summary>
    XrayGameplay = 16,
    /// <summary>Object pixels added from the subscreen.</summary>
    ObjSubscreenAdd = 17,
    /// <summary>Subtractive Mode 7 and object composition.</summary>
    Mode7ObjSubtract = 18,
    /// <summary>Four-bit background added from the subscreen.</summary>
    Bg4SubscreenAdd = 19,
    /// <summary>Mode 7 BG1 pixels added to the composition.</summary>
    Mode7Bg1Add = 20,
    /// <summary>Four-bit subscreen background with line-dependent scrolling.</summary>
    ScrolledBg4SubscreenAdd = 21,
}
