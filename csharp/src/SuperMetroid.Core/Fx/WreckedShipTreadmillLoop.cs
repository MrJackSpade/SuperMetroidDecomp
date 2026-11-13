namespace SuperMetroid.Core.Fx;

/// <summary>One direction-specific treadmill definition and boss-gated four-frame loop.</summary>
public sealed class WreckedShipTreadmillLoop
{

    /// <summary>Builds one direction's boss-gated definition.</summary>
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
}
