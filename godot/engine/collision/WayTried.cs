namespace Engine.Collision;

/// <summary>How much of its way a move tries against what could stop it.</summary>
public enum WayTried
{
    /// <summary>
    /// Every pixel of the way. The move stops flush against the first thing it comes to, at the start of the pixel it
    /// stops in, and never carries part of a pixel into what stopped it.
    /// </summary>
    EveryPixel,

    /// <summary>
    /// Only where the move would end. Solids are tried first: the first registered that the body would overlap there,
    /// and does not overlap already, stops the move against itself, part of a pixel and all, and terrain is then not
    /// tried. Otherwise only the terrain the body's leading edge would end in stops it: what lies on the way, short
    /// of the end, is passed, and a move that ends beside something, part of a pixel short of it, is not stopped.
    /// Stopped by terrain, the body rests against the tile its edge would have ended in, at the far end of its pixel
    /// when it was going right or down and at the pixel's start when going left or up. A move across sloped ground
    /// that holds things up by their middle is not stopped by it: once moved, the body is put where the slope's
    /// surface is under its middle, however far that is.
    /// </summary>
    WhereItEnds,
}
