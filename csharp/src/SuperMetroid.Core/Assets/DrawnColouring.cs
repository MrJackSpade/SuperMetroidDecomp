namespace SuperMetroid.Core.Assets;

/// <summary>
/// The colours a draw shows in place of the ones its image was coloured with (#1464): what they are, and the placement
/// the load current when the draw was published made, a room's graphics set or a frontend screen. The placement is the
/// draw's because a frame is published after its draws, and a room load in between places colourings anew; it is an
/// immutable record of that load, so the draw reaches no live gameplay state.
/// </summary>
public sealed record DrawnColouring
{
    /// <summary>Keeps what the draw shows and the load that places it.</summary>
    /// <param name="shows">What colours the draw shows.</param>
    /// <param name="placement">The load placing them in the sprite sets when the draw was published.</param>
    public DrawnColouring(SpriteColouring shows, SpriteColourPlacement placement)
    {
        Shows = Ensure.NotNull(shows);
        Placement = Ensure.NotNull(placement);
    }

    /// <summary>What colours the draw shows.</summary>
    public SpriteColouring Shows { get; }

    /// <summary>The load placing them in the sprite sets when the draw was published.</summary>
    public SpriteColourPlacement Placement { get; }
}
