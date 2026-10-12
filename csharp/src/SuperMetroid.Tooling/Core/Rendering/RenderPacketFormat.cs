namespace SuperMetroid.Core.Rendering;

/// <summary>Stable identities and allocation limits of the portable .smframe fixture format.</summary>
public static class RenderPacketFormat
{
    /// <summary>Gets signature.</summary>
    internal static ReadOnlySpan<byte> Signature => "SMFRAME\0"u8;
    /// <summary>Defines version.</summary>
    internal const ushort Version = 29;
    /// <summary>Defines subscreen line scroll version.</summary>
    internal const ushort SubscreenLineScrollVersion = 29;
    /// <summary>Defines gameplay main screen hdma version.</summary>
    internal const ushort GameplayMainScreenHdmaVersion = 28;
    /// <summary>Defines combined gameplay subscreen version.</summary>
    internal const ushort CombinedGameplaySubscreenVersion = 27;
    /// <summary>Defines gameplay mosaic version.</summary>
    internal const ushort GameplayMosaicVersion = 26;
    /// <summary>Defines bg2 gameplay subscreen version.</summary>
    internal const ushort Bg2GameplaySubscreenVersion = 25;
    /// <summary>Defines scrolled subscreen version.</summary>
    internal const ushort ScrolledSubscreenVersion = 24;
    /// <summary>Defines mode7 bg1 add version.</summary>
    internal const ushort Mode7Bg1AddVersion = 23;
    /// <summary>Defines mode7 wrap version.</summary>
    internal const ushort Mode7WrapVersion = 22;
    /// <summary>Defines gameplay window version.</summary>
    internal const ushort GameplayWindowVersion = 21;
    /// <summary>Defines obj priority fixed color version.</summary>
    internal const ushort ObjPriorityFixedColorVersion = 20;
    /// <summary>Defines obj fixed color version.</summary>
    internal const ushort ObjFixedColorVersion = 19;
    /// <summary>Defines subscreen main objects version.</summary>
    internal const ushort SubscreenMainObjectsVersion = 18;
    /// <summary>Defines subscreen objects version.</summary>
    internal const ushort SubscreenObjectsVersion = 17;
    /// <summary>Defines bg4 subscreen add version.</summary>
    internal const ushort Bg4SubscreenAddVersion = 16;
    /// <summary>Defines mode7 obj subtract version.</summary>
    internal const ushort Mode7ObjSubtractVersion = 15;
    /// <summary>Defines obj subscreen add version.</summary>
    internal const ushort ObjSubscreenAddVersion = 14;
    /// <summary>Defines xray gameplay version.</summary>
    internal const ushort XrayGameplayVersion = 13;
    /// <summary>Defines title gradient version.</summary>
    internal const ushort TitleGradientVersion = 11;
    /// <summary>Defines first supported version.</summary>
    internal const ushort FirstSupportedVersion = 1;
    /// <summary>Defines mode7 layer version.</summary>
    internal const ushort Mode7LayerVersion = 2;
    /// <summary>Defines fixed color layer version.</summary>
    internal const ushort FixedColorLayerVersion = 3;
    /// <summary>Defines bg2 viewport layer version.</summary>
    internal const ushort Bg2ViewportLayerVersion = 4;
    /// <summary>Defines ordinary gameplay layer version.</summary>
    internal const ushort OrdinaryGameplayLayerVersion = 5;
    /// <summary>Defines scanline color layer version.</summary>
    internal const ushort ScanlineColorLayerVersion = 6;
    /// <summary>Defines message layer version.</summary>
    internal const ushort MessageLayerVersion = 7;
    /// <summary>Defines bg color math layer version.</summary>
    internal const ushort BgColorMathLayerVersion = 8;
    /// <summary>Defines mode7 gameplay layer version.</summary>
    internal const ushort Mode7GameplayLayerVersion = 9;
    /// <summary>Defines windowed scene layer version.</summary>
    internal const ushort WindowedSceneLayerVersion = 10;
    // Limits reject hostile/corrupt lengths before allocating lists. Current scene
    // packets have ten or fewer layers and at most two outer brightness passes.
    /// <summary>Defines maximum operations.</summary>
    internal const int MaximumOperations = 1024;
    /// <summary>Defines maximum packet bytes.</summary>
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
internal enum RenderPacketLayerKind : byte { /** <summary>Identifies obj.</summary> */ Obj = 1, /** <summary>Identifies obj priority.</summary> */ ObjPriority = 2, /** <summary>Identifies bg4 bpp.</summary> */ Bg4Bpp = 3, /** <summary>Identifies bg2 bpp.</summary> */ Bg2Bpp = 4, /** <summary>Identifies mode7.</summary> */ Mode7 = 5, /** <summary>Identifies fixed color add.</summary> */ FixedColorAdd = 6, /** <summary>Identifies bg2 viewport.</summary> */ Bg2Viewport = 7, /** <summary>Identifies ordinary gameplay.</summary> */ OrdinaryGameplay = 8, /** <summary>Identifies scanline color add.</summary> */ ScanlineColorAdd = 9, /** <summary>Identifies message box.</summary> */ MessageBox = 10, /** <summary>Identifies bg color math.</summary> */ BgColorMath = 11, /** <summary>Identifies mode7 gameplay.</summary> */ Mode7Gameplay = 12, /** <summary>Identifies bg subscreen add.</summary> */ BgSubscreenAdd = 13, /** <summary>Identifies windowed scene.</summary> */ WindowedScene = 14, /** <summary>Identifies xray gameplay.</summary> */ XrayGameplay = 16, /** <summary>Identifies obj subscreen add.</summary> */ ObjSubscreenAdd = 17, /** <summary>Identifies mode7 obj subtract.</summary> */ Mode7ObjSubtract = 18, /** <summary>Identifies bg4 subscreen add.</summary> */ Bg4SubscreenAdd = 19, /** <summary>Identifies mode7 bg1 add.</summary> */ Mode7Bg1Add = 20, /** <summary>Identifies scrolled bg4 subscreen add.</summary> */ ScrolledBg4SubscreenAdd = 21 }

/// <summary>Unfiltered plane or one tile-priority bit value; not combinable flags.</summary>
internal enum RenderPacketPriority : byte { /** <summary>Identifies all.</summary> */ All = 0, /** <summary>Identifies low.</summary> */ Low = 1, /** <summary>Identifies high.</summary> */ High = 2 }
