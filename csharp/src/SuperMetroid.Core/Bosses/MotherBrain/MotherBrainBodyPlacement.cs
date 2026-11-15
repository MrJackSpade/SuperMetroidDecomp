namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Where Mother Brain's body is placed in the room (#1949, #2031), which shows in place of the room's background.</summary>
public enum MotherBrainBodyPlacement
{
    /// <summary>Not placed: before her rise, the room's own background shows.</summary>
    Unplaced,

    /// <summary>
    /// With her: from her rise on, her body is placed at her position, so each of her body's pictures lies where its
    /// installed frame places it against her.
    /// </summary>
    FollowsBody,

    /// <summary>At the room's top-left corner: once she is gone, where the escape warning written over the background lies.</summary>
    AtRoomCorner,
}
