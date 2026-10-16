using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Samus;

/// <summary>The collaborators a <see cref="SamusPoseTransitionPhase"/> is constructed with, built by its <see cref="SamusFramePhases"/> owner.</summary>
internal sealed record SamusPoseTransitionPhaseDependencies
{
    /// <summary>Address space.</summary>
    public required ISnesAddressSpace AddressSpace { get; init; }
    /// <summary>Controller1.</summary>
    public required ControllerInputState Controller1 { get; init; }
    /// <summary>Freeze.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }
    /// <summary>Nmi.</summary>
    public required NmiClock Nmi { get; init; }
    /// <summary>Options.</summary>
    public required GameplayOptions Options { get; init; }
    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }
    /// <summary>Plms.</summary>
    public required RoomPlmSystem Plms { get; init; }
    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }
}
