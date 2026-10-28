using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>One direction-specific treadmill header and boss-gated four-frame loop.</summary>
public sealed class WreckedShipTreadmillLoop
{

    /// <summary>Builds one direction's boss-gated header.</summary>
    internal WreckedShipTreadmillLoop(
        WreckedShipTreadmillDirection direction,
        AnimatedTileObject animatedObject)
    {
        Direction = direction;
        AnimatedObject = animatedObject;
    }

    /// <summary>The physical direction selected by the entrance door setup.</summary>
    public WreckedShipTreadmillDirection Direction { get; }

    /// <summary>The animated-tile object running this direction's loop.</summary>
    public AnimatedTileObject AnimatedObject { get; }

    /// <summary>
    /// The picture loop frame <paramref name="frameIndex"/>, 0 through 3, shows: both directions show the rightward
    /// treadmill's four pictures, the leftward loop in reverse order.
    /// </summary>
    public AnimatedTileImage Image(int frameIndex)
    {
        if ((uint)frameIndex >= WreckedShipTreadmillMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frameIndex));
        return new(AnimatedTileObject.WreckedShipTreadmillRightwards,
            Direction == WreckedShipTreadmillDirection.Rightwards
                ? frameIndex : WreckedShipTreadmillMechanicsDefinitions.FrameCount - 1 - frameIndex);
    }
}
