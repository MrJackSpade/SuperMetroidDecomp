using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>
/// The runtime's state as the services one room's enemy AI calls (#633): the shared
/// RNG, events and defeated bosses, the room's scroll zones, Samus's control lock, Mother Brain's room
/// effects and the room's room-object population. Defeated bosses belong to the room's area; the scroll zones are those of
/// the room the services are built for (#2486), and the control lock acts on the introduced Samus.
/// </summary>
[RoomLifetime]
internal sealed partial class RoomEnemyRuntimeServices(
    SharedGameState system,
    RoomScrollGrid scrolls,
    SamusFrameState player,
    RoomObjectSystem roomObjects,
    GameplayDisplayState display,
    RoomAreaProgress progress) : IRoomEnemyServices,
    IEnemyRandomNumbers, IRoomScrollZoneAccess, ISamusControlLock,
    IMotherBrainRoomEffects, IRoomObjectPresence
{
    /// <summary>Exposes the system-owned RNG to enemy AI without a raw system-state dependency.</summary>
    public IEnemyRandomNumbers Random => this;
    /// <summary>The story events that have happened, which the enemies read and mark.</summary>
    public IStoryEvents Events => progress;
    /// <summary>Exposes the persistent Zebetite count to the Zebetites.</summary>
    public IZebetiteProgress ZebetiteProgress => progress;
    /// <summary>Exposes defeated bosses for the active room's area.</summary>
    public IDefeatedBossesRecorder Bosses => progress;
    /// <summary>Exposes active room-object presence to enemy AI.</summary>
    public IRoomObjectPresence RoomObjects => this;
    /// <summary>Exposes how the camera may scroll over each of the room's screens, which enemies read and change.</summary>
    public IRoomScrollZoneAccess RoomScroll => this;
    /// <summary>Exposes Samus's script control lock to enemy AI.</summary>
    public ISamusControlLock SamusControls => this;
    /// <summary>Exposes Mother Brain's glass, blending, and the background room effects.</summary>
    public IMotherBrainRoomEffects MotherBrain => this;

    /// <summary>Advances the shared system RNG once for an enemy request.</summary>
    public int Generate() => system.NextRandom();

    /// <summary>Advances the shared system RNG once for an enemy draw whose value decides nothing.</summary>
    public void Advance() => system.Random.Advance();

    /// <summary>Reads the current random number without drawing a new one.</summary>
    public int Read() => system.RandomNumber;

    /// <summary>Overwrites the shared random numbers with a species' seed.</summary>
    public void Reseed(SpeciesRandomReseed reseed) => system.Random.Reseed(reseed);

    /// <inheritdoc/>
    public void Write(Position16<RoomScreens> screen, RoomScrollState state) => scrolls.SetRoomScreen(screen, state);

    /// <summary>Releases Samus's stationary script lock and reopens the player's grounded-movement gate.</summary>
    public void EnableControls()
    {
        player.IntroducedSamus.Control.Unlock();
        player.GroundedSamusMovementEnabled = true;
    }

    /// <summary>Locks Samus stationary and closes the player's grounded-movement gate.</summary>
    public void DisableControls()
    {
        player.IntroducedSamus.Control.LockStationary();
        player.GroundedSamusMovementEnabled = false;
    }

    /// <inheritdoc/>
    public void CountHeadShotOnGlass() => roomObjects.MotherBrainGlass.CountHeadShotOnGlass();

    /// <inheritdoc/>
    public void ConfirmGlassLoaded() => roomObjects.MotherBrainGlass.ConfirmGlassLoaded();

    /// <summary>Checks whether a requested Bomb Torizo hand room object remains active.</summary>
    public bool BombTorizoHandRemains => roomObjects.BombTorizoHand.HandRemains;
}
