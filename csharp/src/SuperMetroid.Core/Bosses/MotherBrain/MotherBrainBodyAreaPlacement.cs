namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Where Mother Brain's body's area lies in the room (#1949), which shows in place of the room's background.</summary>
public enum MotherBrainBodyAreaPlacement
{
    /// <summary>Not placed: before her rise, the room's own background shows.</summary>
    Unplaced,

    /// <summary>
    /// With her: from her rise on, her body's area lies at <see cref="MotherBrainBodyAreaDefinitions.OffsetFromBody"/> from
    /// her, so each of her body's pictures lies where its frame places it against her.
    /// </summary>
    FollowsBody,

    /// <summary>At the room's top-left corner: once she is gone, where the escape warning shows.</summary>
    AtRoomCorner,
}
