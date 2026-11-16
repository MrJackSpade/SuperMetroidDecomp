using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// Room objects for Samus fixtures whose rooms reach none (#2049): each family fails at the exact one a check reaches
/// without installing it. A check whose terrain reacts through room objects builds real ones
/// (<see cref="RoomObjectFixtures"/>).
/// </summary>
internal static class SamusRoomObjectFixtures
{
    /// <summary>Room objects that fail at whichever family a check reaches.</summary>
    internal static ISamusRoomObjects Uninstalled { get; } = new UninstalledSamusRoomObjects(places: null);

    /// <summary>
    /// Room objects whose quicksand reactions take places from a fresh room's forty, every other family failing where a
    /// check reaches it: for checks whose terrain holds sand, special or bomb blocks, which consult the room's places.
    /// </summary>
    internal static ISamusRoomObjects WithQuicksand() =>
        new UninstalledSamusRoomObjects(new RoomObjectPlaceList(new RoomObjectPlaces()));

    /// <summary>A room with no room objects installed, or only its places and the quicksand that takes them.</summary>
    private sealed class UninstalledSamusRoomObjects(RoomObjectPlaceList? places) : ISamusRoomObjects
    {
        /// <summary>The room's places, or null when the fixture has none.</summary>
        private readonly RoomObjectPlaceList? _places = places;

        /// <summary>The quicksand reactions taking <see cref="_places"/>, or null when the fixture has no places.</summary>
        private readonly QuicksandObjects? _quicksand = places is null ? null : new QuicksandObjects(places);

        /// <inheritdoc/>
        public BlockReactionObjects BlockReactions => throw Missing(nameof(BlockReactions));

        /// <inheritdoc/>
        public IGlobalEventFlags Events => throw Missing(nameof(Events));

        /// <inheritdoc/>
        public RoomObjectSoundQueue Sounds => throw Missing(nameof(Sounds));

        /// <inheritdoc/>
        public IRoomObjectDrawing Drawing => throw Missing(nameof(Drawing));

        /// <inheritdoc/>
        public CollectibleObjects CollectibleObjects => throw Missing(nameof(CollectibleObjects));

        /// <inheritdoc/>
        public StationObjects Stations => throw Missing(nameof(Stations));

        /// <inheritdoc/>
        public DoorObjects DoorObjects => throw Missing(nameof(DoorObjects));

        /// <inheritdoc/>
        public EyeDoorObjects EyeDoors => throw Missing(nameof(EyeDoors));

        /// <inheritdoc/>
        public GreyDoorObjects GreyDoors => throw Missing(nameof(GreyDoors));

        /// <inheritdoc/>
        public GateObjects Gates => throw Missing(nameof(Gates));

        /// <inheritdoc/>
        public SamusEaterObjects SamusEater => throw Missing(nameof(SamusEater));

        /// <inheritdoc/>
        public ScrollObjects Scrolls => throw Missing(nameof(Scrolls));

        /// <inheritdoc/>
        public ProjectileTriggerObjects ProjectileTriggers => throw Missing(nameof(ProjectileTriggers));

        /// <inheritdoc/>
        public QuicksandObjects Quicksand => _quicksand ?? throw Missing(nameof(Quicksand));

        /// <inheritdoc/>
        public IChozoStatueHand ChozoStatueHand => throw Missing(nameof(ChozoStatueHand));

        /// <inheritdoc/>
        public bool EveryPlaceHeld => _places?.IsFull ?? throw Missing(nameof(EveryPlaceHeld));

        /// <inheritdoc/>
        public void DeleteObject(RoomObjectPlace place) => throw Missing(nameof(DeleteObject));

        /// <summary>The failure of a check that reached <paramref name="family"/> without installing it.</summary>
        private static InvalidOperationException Missing(string family) =>
            new($"The Samus fixture installed no room objects, but its terrain reached {family}.");
    }
}
