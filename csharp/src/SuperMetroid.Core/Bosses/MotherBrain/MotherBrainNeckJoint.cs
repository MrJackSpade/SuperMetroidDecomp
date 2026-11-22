namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One joint of Mother Brain's neck (#2314): how far it hangs from its anchor, and where it lies.</summary>
public sealed partial class MotherBrainNeckJoint
{
    /// <summary>Makes the joint <paramref name="definition"/> places.</summary>
    /// <param name="definition">The joint's distance from its anchor and whether Samus can touch it.</param>
    internal MotherBrainNeckJoint(MotherBrainNeckJointDefinition definition)
    {
        Distance = Ensure.AtLeastZero(definition.Distance);
        Exposure = definition.Exposure;
    }

    /// <summary>The joint's pixel distance from its anchor along its half of the neck's angle.</summary>
    public int Distance { get; }

    /// <summary>Whether Samus can touch the joint.</summary>
    internal MotherBrainNeckJointExposure Exposure { get; }

    /// <summary>Where the joint lies in the room; the neck's placement sets it each update the neck is shown.</summary>
    public Position16<RoomPixels> Position { get; internal set; }
}
