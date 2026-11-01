using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// A copy of where the game is and what Samus is doing at one moment, read without running the game. A host takes one
/// before an update and another when the update fails, to report what the update started from and how far it got.
/// </summary>
public sealed class GameplaySnapshot
{
    /// <summary>The number of the last update the game ran.</summary>
    public required int FrameNumber { get; init; }

    /// <summary>The phase the game is in.</summary>
    public required SuperMetroidGameState GameState { get; init; }

    /// <summary>The active room's name, or null before gameplay.</summary>
    public string? Room { get; init; }

    /// <summary>The active room state's name, or null before gameplay.</summary>
    public string? RoomState { get; init; }

    /// <summary>The name of the door the active room was entered through, or null when there is none.</summary>
    public string? Door { get; init; }

    /// <summary>What Samus and the player's controller are doing, or null before gameplay has introduced Samus.</summary>
    public SamusSnapshot? Samus { get; init; }
}
