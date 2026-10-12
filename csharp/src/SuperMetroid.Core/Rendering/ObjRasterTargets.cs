using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The caller-owned rasters an OBJ pass writes (#633): the winning colour per pixel, the winning
/// sprite's priority when the pass resolves priorities, and its palette when a later colour-math
/// pass needs ownership. Absent rasters are empty spans.
/// </summary>
public readonly ref struct ObjRasterTargets
{
    /// <summary>Binds the rasters one OBJ pass writes.</summary>
    public ObjRasterTargets(Span<Rgba32> pixels, Span<byte> priorities = default, Span<byte> palettes = default)
    {
        Pixels = pixels;
        Priorities = priorities;
        Palettes = palettes;
    }

    /// <summary>Winning OBJ colour per pixel.</summary>
    public Span<Rgba32> Pixels { get; }

    /// <summary>Winning OBJ priority per pixel, or empty when the pass selects one priority plane.</summary>
    public Span<byte> Priorities { get; }

    /// <summary>Winning OBJ palette per pixel, or empty when no colour-math pass reads ownership.</summary>
    public Span<byte> Palettes { get; }
}
