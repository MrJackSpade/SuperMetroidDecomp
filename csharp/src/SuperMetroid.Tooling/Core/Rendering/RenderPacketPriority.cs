namespace SuperMetroid.Core.Rendering;

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
