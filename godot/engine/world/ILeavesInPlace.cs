namespace Engine.World;

/// <summary>
/// Something that says what it leaves in the place it holds among a room's places as it goes
/// (<see cref="PlaceLeftover"/>). Whatever holds a place and does not say leaves where it is, with the speed it found
/// in the place.
/// </summary>
public interface ILeavesInPlace
{
    /// <summary>What it leaves, as it stands now; null to leave where it is with the speed it found.</summary>
    PlaceLeftover? LeftInPlace { get; }
}
