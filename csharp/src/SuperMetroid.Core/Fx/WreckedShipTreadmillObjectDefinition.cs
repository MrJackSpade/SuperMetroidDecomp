namespace SuperMetroid.Core.Fx;

/// <summary>One direction-specific treadmill header and boss-gated four-frame loop.</summary>
public sealed class WreckedShipTreadmillObjectDefinition
{

    /// <summary>Builds one direction's boss-gated header.</summary>
    internal WreckedShipTreadmillObjectDefinition(
        WreckedShipTreadmillDirection direction,
        AnimatedTileObject animatedObject)
    {
        Direction = direction;
        AnimatedObject = animatedObject;
    }

    /// <summary>The physical direction selected by the entrance door setup.</summary>
    public WreckedShipTreadmillDirection Direction { get; }

    /// <summary>The object-header address within bank $87.</summary>
    public AnimatedTileObject AnimatedObject { get; }

    /// <summary>
    /// The artwork identity of loop frame <paramref name="frameIndex"/>, 0 through 3: the leftward loop shows the
    /// rightward loop's art in reverse order.
    /// </summary>
    public int FrameSource(int frameIndex) =>
        WreckedShipTreadmillRomData.FrameSource(
            Direction == WreckedShipTreadmillDirection.Rightwards
                ? frameIndex : WreckedShipTreadmillMechanicsDefinitions.FrameCount - 1 - frameIndex);
}
