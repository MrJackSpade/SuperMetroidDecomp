namespace SuperMetroid.Core.Rendering;

/// <summary>Stable identities and allocation limits of the portable .smframe fixture format.</summary>
internal static class RenderPacketFormat
{
    internal static ReadOnlySpan<byte> Signature => "SMFRAME\0"u8;
    internal const ushort Version = 19;
    internal const ushort ObjFixedColorVersion = 19;
    internal const ushort SubscreenMainObjectsVersion = 18;
    internal const ushort SubscreenObjectsVersion = 17;
    internal const ushort Bg4SubscreenAddVersion = 16;
    internal const ushort Mode7ObjSubtractVersion = 15;
    internal const ushort ObjSubscreenAddVersion = 14;
    internal const ushort XrayGameplayVersion = 13;
    internal const ushort XrayWindowVersion = 12;
    internal const ushort TitleGradientVersion = 11;
    internal const ushort FirstSupportedVersion = 1;
    internal const ushort Mode7LayerVersion = 2;
    internal const ushort FixedColorLayerVersion = 3;
    internal const ushort Bg2ViewportLayerVersion = 4;
    internal const ushort OrdinaryGameplayLayerVersion = 5;
    internal const ushort ScanlineColorLayerVersion = 6;
    internal const ushort MessageLayerVersion = 7;
    internal const ushort BgColorMathLayerVersion = 8;
    internal const ushort Mode7GameplayLayerVersion = 9;
    internal const ushort WindowedSceneLayerVersion = 10;
    // Limits reject hostile/corrupt lengths before allocating lists. Current scene
    // packets have ten or fewer layers and at most two outer brightness passes.
    internal const int MaximumOperations = 1024;
    internal const int MaximumPacketBytes = 4 * 1024 * 1024;
}

/// <summary>Exclusive on-disk composition discriminants. Never renumber existing entries.</summary>
internal enum RenderPacketKind : byte { Solid = 1, Mode7Obj = 2, Layered = 3 }

/// <summary>Exclusive on-disk layer discriminants. Never renumber existing entries.</summary>
internal enum RenderPacketLayerKind : byte { Obj = 1, ObjPriority = 2, Bg4Bpp = 3, Bg2Bpp = 4, Mode7 = 5, FixedColorAdd = 6, Bg2Viewport = 7, OrdinaryGameplay = 8, ScanlineColorAdd = 9, MessageBox = 10, BgColorMath = 11, Mode7Gameplay = 12, BgSubscreenAdd = 13, WindowedScene = 14, XrayWindow = 15, XrayGameplay = 16, ObjSubscreenAdd = 17, Mode7ObjSubtract = 18, Bg4SubscreenAdd = 19 }

/// <summary>Unfiltered plane or one tile-priority bit value; not combinable flags.</summary>
internal enum RenderPacketPriority : byte { All = 0, Low = 1, High = 2 }
