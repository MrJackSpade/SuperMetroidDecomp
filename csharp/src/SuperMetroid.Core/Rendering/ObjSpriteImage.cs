using SuperMetroid.Core.Assets;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>Everything the OBJ rasterizer draws for one frame: the OAM image and the whole-frame draws placed among it.</summary>
/// <param name="Oam">Finalized OAM, read through VRAM.</param>
/// <param name="Draws">Whole-frame draws in emission order, read from their own artwork.</param>
public readonly record struct ObjSpriteImage(OamBuffer Oam, ImmutableArray<OrderedSpriteDraw> Draws)
{
    /// <summary>A gameplay sprite frame's OAM image and draws.</summary>
    public static ObjSpriteImage Of(SpriteFrameBuffer sprites)
    {
        Ensure.NotNull(sprites);
        return new(sprites.Oam, sprites.Draws);
    }
}
