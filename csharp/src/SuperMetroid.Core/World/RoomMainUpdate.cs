using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.World;

/// <summary>What a room's main runs against on one gameplay update (#2287).</summary>
internal sealed partial record RoomMainUpdate
{
    /// <summary>The loaded room.</summary>
    public required RoomScope Room { get; init; }

    /// <summary>Where the layers stand, which Crocomire's hidden-wall rumble shakes.</summary>
    public required BackgroundScrollState BackgroundScroll { get; init; }

    /// <summary>The random number this update has already drawn, which the Ceres falling debris samples without drawing another.</summary>
    public required int RandomNumber { get; init; }

    /// <summary>The escape rooms' explosions and shaking.</summary>
    public required EscapeRoomEffects Escape { get; init; }

    /// <summary>Samus, whom the elevatube carries and the elevator shaft's trigger reads.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>The frame clock's counter this update.</summary>
    public required int FrameCounter { get; init; }

    /// <summary>Whether gameplay time is frozen this update.</summary>
    public required bool TimeIsFrozen { get; init; }

    /// <summary>The gameplay display, whose turning floor the elevator shaft's turn publishes for the next frame.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>
    /// Whether this update may take Samus to the Ceres elevator: only while the game is still in main gameplay where room
    /// main runs, main gameplay owning the update and none of its earlier steps having moved the game on (#2534).
    /// </summary>
    public required bool AllowCeresElevatorDeparture { get; init; }
}
