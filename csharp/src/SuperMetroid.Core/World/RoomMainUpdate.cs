using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World;

/// <summary>What a room's main runs against on one gameplay update (#2287).</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal sealed record RoomMainUpdate
{
    /// <summary>The loaded room.</summary>
    public required RoomScope Room { get; init; }

    /// <summary>The room mains' work that reads the session: the Crocomire rumble's layers and the falling debris's random number.</summary>
    public required RoomMainRoutines Mains { get; init; }

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

    /// <summary>The working value room mains leave for the following room's.</summary>
    public required RoomMainCarryover Carryover { get; init; }

    /// <summary>Whether this update may take Samus to the Ceres elevator: only main gameplay does.</summary>
    public required bool AllowCeresElevatorDeparture { get; init; }
}
