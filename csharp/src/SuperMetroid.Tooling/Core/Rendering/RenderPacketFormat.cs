namespace SuperMetroid.Core.Rendering;

/// <summary>Stable identities and allocation limits of the portable .smframe fixture format.</summary>
internal static class RenderPacketFormat
{
    /// <summary>Eight-byte file marker used to distinguish render fixtures from other data.</summary>
    internal static ReadOnlySpan<byte> Signature => "SMFRAME\0"u8;
    /// <summary>Newest packet schema emitted by the current writer.</summary>
    internal const ushort Version = 29;
    /// <summary>Version that added per-scanline scroll values to the subscreen layer.</summary>
    internal const ushort SubscreenLineScrollVersion = 29;
    /// <summary>Version that recorded gameplay main-screen HDMA state.</summary>
    internal const ushort GameplayMainScreenHdmaVersion = 28;
    /// <summary>Version that combined gameplay main- and subscreen composition data.</summary>
    internal const ushort CombinedGameplaySubscreenVersion = 27;
    /// <summary>Version that added gameplay mosaic parameters.</summary>
    internal const ushort GameplayMosaicVersion = 26;
    /// <summary>Version that added BG2 gameplay subscreen composition.</summary>
    internal const ushort Bg2GameplaySubscreenVersion = 25;
    /// <summary>Version that added scroll offsets to subscreen composition.</summary>
    internal const ushort ScrolledSubscreenVersion = 24;
    /// <summary>Version that added BG1 additive blending for gameplay Mode 7.</summary>
    internal const ushort Mode7Bg1AddVersion = 23;
    /// <summary>Version that recorded Mode 7 wrap behavior.</summary>
    internal const ushort Mode7WrapVersion = 22;
    /// <summary>Version that added window-mask state to gameplay composition.</summary>
    internal const ushort GameplayWindowVersion = 21;
    /// <summary>Version that added OBJ priority and fixed-color composition.</summary>
    internal const ushort ObjPriorityFixedColorVersion = 20;
    /// <summary>Version that added fixed-color composition for OBJ layers.</summary>
    internal const ushort ObjFixedColorVersion = 19;
    /// <summary>Version that separated main-screen objects from subscreen composition.</summary>
    internal const ushort SubscreenMainObjectsVersion = 18;
    /// <summary>Version that added objects to the subscreen layer.</summary>
    internal const ushort SubscreenObjectsVersion = 17;
    /// <summary>Version that added BG4 subscreen color addition.</summary>
    internal const ushort Bg4SubscreenAddVersion = 16;
    /// <summary>Version that added subtractive Mode 7 object blending.</summary>
    internal const ushort Mode7ObjSubtractVersion = 15;
    /// <summary>Version that added OBJ subscreen color addition.</summary>
    internal const ushort ObjSubscreenAddVersion = 14;
    /// <summary>Version that added gameplay X-Ray composition metadata.</summary>
    internal const ushort XrayGameplayVersion = 13;
    /// <summary>Version that added the title-screen gradient layer.</summary>
    internal const ushort TitleGradientVersion = 11;
    /// <summary>Oldest packet version accepted by the reader.</summary>
    internal const ushort FirstSupportedVersion = 1;
    /// <summary>Version that introduced the standalone Mode 7 layer.</summary>
    internal const ushort Mode7LayerVersion = 2;
    /// <summary>Version that introduced fixed-color layer data.</summary>
    internal const ushort FixedColorLayerVersion = 3;
    /// <summary>Version that introduced the BG2 viewport layer.</summary>
    internal const ushort Bg2ViewportLayerVersion = 4;
    /// <summary>Version that introduced ordinary gameplay layer composition.</summary>
    internal const ushort OrdinaryGameplayLayerVersion = 5;
    /// <summary>Version that introduced scanline color-add data.</summary>
    internal const ushort ScanlineColorLayerVersion = 6;
    /// <summary>Version that introduced message-box layer data.</summary>
    internal const ushort MessageLayerVersion = 7;
    /// <summary>Version that introduced background color-math composition.</summary>
    internal const ushort BgColorMathLayerVersion = 8;
    /// <summary>Version that introduced gameplay Mode 7 layer composition.</summary>
    internal const ushort Mode7GameplayLayerVersion = 9;
    /// <summary>Version that introduced windowed-scene composition.</summary>
    internal const ushort WindowedSceneLayerVersion = 10;
    // Limits reject hostile/corrupt lengths before allocating lists. Current scene
    // packets have ten or fewer layers and at most two outer brightness passes.
    /// <summary>Maximum decoded operation count accepted before rejecting a packet.</summary>
    internal const int MaximumOperations = 1024;
    /// <summary>Maximum serialized packet size accepted before allocating payload storage.</summary>
    internal const int MaximumPacketBytes = 4 * 1024 * 1024;
}

/// <summary>Exclusive on-disk composition discriminants. Never renumber existing entries.</summary>
internal enum RenderPacketKind : byte
{
    /// <summary>Composition represented by one flat solid-color image.</summary>
    Solid = 1,
    /// <summary>Composition containing a Mode 7 background and objects.</summary>
    Mode7Obj = 2,
    /// <summary>Composition assembled from the ordered layer records that follow.</summary>
    Layered = 3,
}

/// <summary>Exclusive on-disk layer discriminants. Never renumber existing entries.</summary>
internal enum RenderPacketLayerKind : byte
{
    /// <summary>Object layer with no priority filtering.</summary>
    Obj = 1,
    /// <summary>Object layer filtered by the encoded tile-priority selection.</summary>
    ObjPriority = 2,
    /// <summary>Four-bit background tile layer.</summary>
    Bg4Bpp = 3,
    /// <summary>Two-bit background tile layer.</summary>
    Bg2Bpp = 4,
    /// <summary>Standalone Mode 7 background layer.</summary>
    Mode7 = 5,
    /// <summary>Layer that adds the fixed color to selected pixels.</summary>
    FixedColorAdd = 6,
    /// <summary>BG2 layer drawn through the viewport bounds.</summary>
    Bg2Viewport = 7,
    /// <summary>Ordinary gameplay layer with its recorded screen configuration.</summary>
    OrdinaryGameplay = 8,
    /// <summary>Per-scanline color-add contribution.</summary>
    ScanlineColorAdd = 9,
    /// <summary>Message box and its presentation pixels.</summary>
    MessageBox = 10,
    /// <summary>Background color-math layer.</summary>
    BgColorMath = 11,
    /// <summary>Mode 7 layer using gameplay screen configuration.</summary>
    Mode7Gameplay = 12,
    /// <summary>Background contribution blended through the subscreen.</summary>
    BgSubscreenAdd = 13,
    /// <summary>Scene composition clipped by window-mask state.</summary>
    WindowedScene = 14,
    /// <summary>Gameplay X-Ray overlay and associated composition state.</summary>
    XrayGameplay = 16,
    /// <summary>Object contribution blended through the subscreen.</summary>
    ObjSubscreenAdd = 17,
    /// <summary>Mode 7 object contribution using subtraction.</summary>
    Mode7ObjSubtract = 18,
    /// <summary>Four-bit background subscreen contribution using addition.</summary>
    Bg4SubscreenAdd = 19,
    /// <summary>BG1 contribution blended with Mode 7 additive color math.</summary>
    Mode7Bg1Add = 20,
    /// <summary>Scrolled four-bit background subscreen additive contribution.</summary>
    ScrolledBg4SubscreenAdd = 21,
}

/// <summary>Unfiltered plane or one tile-priority bit value; not combinable flags.</summary>
internal enum RenderPacketPriority : byte
{
    /// <summary>Include pixels from both tile-priority classes.</summary>
    All = 0,
    /// <summary>Include only low-priority tile pixels.</summary>
    Low = 1,
    /// <summary>Include only high-priority tile pixels.</summary>
    High = 2,
}
