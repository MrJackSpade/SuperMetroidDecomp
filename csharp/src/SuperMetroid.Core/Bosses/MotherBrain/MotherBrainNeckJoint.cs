namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One joint of Mother Brain's neck (#2314): how far it hangs from its anchor, and where it lies.</summary>
public sealed class MotherBrainNeckJoint
{
    /// <summary>Makes a joint hanging <paramref name="distance"/> pixels from its anchor.</summary>
    /// <param name="distance">The joint's pixel distance from its anchor.</param>
    internal MotherBrainNeckJoint(int distance) => Distance = Ensure.AtLeastZero(distance);

    /// <summary>The joint's pixel distance from its anchor along its half of the neck's angle.</summary>
    public int Distance { get; }

    /// <summary>Where the joint lies in the room; the neck's placement sets it each update the neck is shown.</summary>
    public Position16<RoomPixels> Position { get; internal set; }
}
