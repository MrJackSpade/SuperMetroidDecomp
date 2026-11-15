namespace SuperMetroid.Core.Enemies;

/// <summary>What the Tourian statues' unlock effects and decorations need: the statue room as it stands.</summary>
public interface ITourianStatueRoomServices
{
    /// <summary>The statue room's water and the entrance statue's descent.</summary>
    ITourianStatueRoom TourianStatueRoom { get; }
}
