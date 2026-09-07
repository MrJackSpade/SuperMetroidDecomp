namespace SuperMetroid.Core.Rendering;

/// <summary>Stable identities and allocation limits of the portable .smframe fixture format.</summary>
internal static class RenderPacketFormat
{
    internal static ReadOnlySpan<byte> Signature => "SMFRAME\0"u8;
    internal const ushort Version = 1;
    // Limits reject hostile/corrupt lengths before allocating lists. Current scene
    // packets have ten or fewer layers and at most two outer brightness passes.
    internal const int MaximumOperations = 1024;
    internal const int MaximumPacketBytes = 4 * 1024 * 1024;
}

/// <summary>Exclusive on-disk composition discriminants. Never renumber existing entries.</summary>
internal enum RenderPacketKind : byte { Solid = 1, Mode7Obj = 2, Layered = 3 }

/// <summary>Exclusive on-disk layer discriminants. Never renumber existing entries.</summary>
internal enum RenderPacketLayerKind : byte { Obj = 1, ObjPriority = 2, Bg4Bpp = 3, Bg2Bpp = 4 }

/// <summary>Unfiltered plane or one tile-priority bit value; not combinable flags.</summary>
internal enum RenderPacketPriority : byte { All = 0, Low = 1, High = 2 }
