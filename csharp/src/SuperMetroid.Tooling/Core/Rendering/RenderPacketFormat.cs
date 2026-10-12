namespace SuperMetroid.Core.Rendering;

/// <summary>Stable identities and allocation limits of the portable .smframe fixture format.</summary>
public static class RenderPacketFormat
{
    /// <summary>Eight-byte marker checked before the reader interprets a packet payload.</summary>
    internal static ReadOnlySpan<byte> Signature => "SMFRAME\0"u8;
    /// <summary>Current writer revision; readers retain older feature thresholds below.</summary>
    internal const ushort Version = 29;
    /// <summary>First revision carrying per-line subscreen scroll.</summary>
    internal const ushort SubscreenLineScrollVersion = 29;
    /// <summary>First revision carrying gameplay main-screen HDMA state.</summary>
    internal const ushort GameplayMainScreenHdmaVersion = 28;
    /// <summary>First revision combining gameplay subscreen composition in one packet.</summary>
    internal const ushort CombinedGameplaySubscreenVersion = 27;
    /// <summary>First revision carrying gameplay mosaic parameters.</summary>
    internal const ushort GameplayMosaicVersion = 26;
    /// <summary>First revision adding BG2 to gameplay subscreen composition.</summary>
    internal const ushort Bg2GameplaySubscreenVersion = 25;
    /// <summary>First revision carrying a scrolled subscreen layer.</summary>
    internal const ushort ScrolledSubscreenVersion = 24;
    /// <summary>First revision supporting Mode 7 BG1 addition in composition.</summary>
    internal const ushort Mode7Bg1AddVersion = 23;
    /// <summary>First revision encoding the Mode 7 wrap choice.</summary>
    internal const ushort Mode7WrapVersion = 22;
    /// <summary>First revision carrying gameplay window composition.</summary>
    internal const ushort GameplayWindowVersion = 21;
    /// <summary>First revision combining object priority with fixed color.</summary>
    internal const ushort ObjPriorityFixedColorVersion = 20;
    /// <summary>First revision combining ordinary objects with fixed color.</summary>
    internal const ushort ObjFixedColorVersion = 19;
    /// <summary>First revision composing main-screen objects over a subscreen.</summary>
    internal const ushort SubscreenMainObjectsVersion = 18;
    /// <summary>First revision storing subscreen object rendering.</summary>
    internal const ushort SubscreenObjectsVersion = 17;
    /// <summary>First revision adding four-bit BG artwork on the subscreen.</summary>
    internal const ushort Bg4SubscreenAddVersion = 16;
    /// <summary>First revision supporting Mode 7 and object subtraction.</summary>
    internal const ushort Mode7ObjSubtractVersion = 15;
    /// <summary>First revision adding object pixels from the subscreen.</summary>
    internal const ushort ObjSubscreenAddVersion = 14;
    /// <summary>First revision carrying gameplay X-Ray rendering.</summary>
    internal const ushort XrayGameplayVersion = 13;
    /// <summary>First revision carrying title gradient composition.</summary>
    internal const ushort TitleGradientVersion = 11;
    /// <summary>Oldest packet revision accepted by the portable capture reader.</summary>
    internal const ushort FirstSupportedVersion = 1;
    /// <summary>First revision allowing Mode 7 as an ordered layer.</summary>
    internal const ushort Mode7LayerVersion = 2;
    /// <summary>First revision carrying a fixed-color layer.</summary>
    internal const ushort FixedColorLayerVersion = 3;
    /// <summary>First revision carrying a clipped BG2 viewport layer.</summary>
    internal const ushort Bg2ViewportLayerVersion = 4;
    /// <summary>First revision encoding ordinary gameplay layer composition.</summary>
    internal const ushort OrdinaryGameplayLayerVersion = 5;
    /// <summary>First revision carrying scanline color changes.</summary>
    internal const ushort ScanlineColorLayerVersion = 6;
    /// <summary>First revision serializing message-box composition.</summary>
    internal const ushort MessageLayerVersion = 7;
    /// <summary>First revision carrying background color math.</summary>
    internal const ushort BgColorMathLayerVersion = 8;
    /// <summary>First revision encoding the gameplay Mode 7 layer.</summary>
    internal const ushort Mode7GameplayLayerVersion = 9;
    /// <summary>First revision composing a child scene through a rectangular window.</summary>
    internal const ushort WindowedSceneLayerVersion = 10;
    /// <summary>Rejects corrupt operation counts before allocating lists; normal scenes use at most ten layers and two outer brightness passes.</summary>
    internal const int MaximumOperations = 1024;
    /// <summary>Upper bound for a serialized packet before the reader allocates its payload.</summary>
    internal const int MaximumPacketBytes = 4 * 1024 * 1024;
}

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

/// <summary>Unfiltered plane or one tile-priority bit value; not combinable flags.</summary>
internal enum RenderPacketPriority : byte
{
    /// <summary>Includes both native tile-priority groups.</summary>
    All = 0,
    /// <summary>Includes only tiles whose priority bit is clear.</summary>
    Low = 1,
    /// <summary>Includes only tiles whose priority bit is set.</summary>
    High = 2,
}
