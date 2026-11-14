using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>
/// The runtime's state as the services one room's enemy AI calls (#633): the shared
/// RNG, events and defeated bosses, the room's scroll zones, Samus's control lock, Mother Brain's room
/// effects and the active room-object population. Defeated bosses belong to the room's area.
/// </summary>
[RoomLifetime]
internal sealed class RoomEnemyRuntimeServices(
    SharedGameState system,
    ActiveRoomState room,
    SamusFrameState player,
    RoomObjectSystem roomObjects,
    GameplayDisplayState display,
    RoomAreaProgress progress) : IRoomEnemyServices,
    IEnemyRandomNumbers, IRoomScrollZoneAccess, ISamusControlLock,
    IMotherBrainRoomEffects, IRoomObjectPresence
{
    /// <summary>Exposes the system-owned RNG to enemy AI without a raw system-state dependency.</summary>
    public IEnemyRandomNumbers Random => this;
    /// <summary>Exposes persistent global event bits to enemy AI.</summary>
    public IGlobalEventFlags Events => progress;
    /// <summary>Exposes the persistent Zebetite count to the Zebetites.</summary>
    public IZebetiteProgress ZebetiteProgress => progress;
    /// <summary>Exposes defeated bosses for the active room's area.</summary>
    public IAreaBossFlags Bosses => progress;
    /// <summary>Exposes active room-object presence to enemy AI.</summary>
    public IRoomObjectPresence RoomObjects => this;
    /// <summary>Exposes the room camera's scroll-zone storage.</summary>
    public IRoomScrollZoneAccess RoomScroll => this;
    /// <summary>Exposes Samus's script control lock to enemy AI.</summary>
    public ISamusControlLock SamusControls => this;
    /// <summary>Exposes Mother Brain's glass, blending, and the background room effects.</summary>
    public IMotherBrainRoomEffects MotherBrain => this;

    /// <summary>Advances the shared system RNG once for an enemy request.</summary>
    public int Generate() => system.NextRandom();

    /// <summary>Reads the current random number without drawing a new one.</summary>
    public int Read() => system.RandomNumber;

    /// <summary>Installs an enemy-supplied seed into the shared system RNG.</summary>
    public void Seed(int value) => system.SetRandomNumber(value);

    /// <inheritdoc/>
    RoomScrollState IRoomScrollZoneAccess.Read(Position16<RoomScreens> screen) => room.LoadedCamera.Scrolls.ReadScreen(screen);

    /// <inheritdoc/>
    public void Write(Position16<RoomScreens> screen, RoomScrollState state) => room.LoadedCamera.Scrolls.SetLogicalState(screen, state);

    /// <summary>Releases Samus's stationary script lock and reopens the player's grounded-movement gate.</summary>
    public void EnableControls()
    {
        ControlledSamus.Control.Unlock();
        player.GroundedSamusMovementEnabled = true;
    }

    /// <summary>Locks Samus stationary and closes the player's grounded-movement gate.</summary>
    public void DisableControls()
    {
        ControlledSamus.Control.LockStationary();
        player.GroundedSamusMovementEnabled = false;
    }

    /// <summary>The Samus actor a Chozo statue's control lock acts on.</summary>
    private SamusState ControlledSamus => player.Samus ?? throw new InvalidOperationException(
        "Chozo control lock requires an active Samus actor.");

    /// <summary>Advances the Mother Brain glass room-object argument after an enemy-driven phase event.</summary>
    public void IncrementGlassRoomArgument() => roomObjects.MotherBrainGlass.IncrementMotherBrainGlassRoomArgument();

    /// <inheritdoc />
    public void ShowFloorFrame(CeresFloorAnimationFrame frame) =>
        (room.LoadedScope.Floor ?? throw new InvalidOperationException("Only a room that shows a turning floor has its parts to animate."))
            .Show(frame);

    /// <summary>Checks whether a requested Bomb Torizo hand room object remains active.</summary>
    public bool BombTorizoHandRemains => roomObjects.BombTorizoHand.HandRemains;
}
