using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Runtime;

/// <summary>Development-tool instance members of <see cref="SuperMetroidRuntime"/>.</summary>
internal static class SuperMetroidRuntimeToolingExtensions
{
    /// <summary>Creates runtimes for hosts that run one runtime without a frontend session.</summary>
    extension(SuperMetroidRuntime)
    {
        /// <summary>
        /// Creates a bare frame-steppable runtime with its own frame clock and unauthored Speed Booster cadence, over the supplied
        /// session's random-number word or, without one, a word of its own, and over the supplied session's sound and music
        /// queues or, without them, queues of its own (#2057); the caller binds whatever catalogs it exercises.
        /// </summary>
        /// <param name="assists">The assists the runtime plays under; <see cref="SuperMetroid.Core.Gameplay.GameplayAssists.None"/> for the unassisted game.</param>
        /// <param name="artwork">
        /// The installed catalogs the runtime is built over: the Samus, boss and enemy overread services and the enemy sheets
        /// and frame displays, which every runtime requires.
        /// </param>
        /// <param name="random">The session's random-number word, or null for a runtime that is a session of its own.</param>
        /// <param name="audio">The session's sound and music queues, or null for a runtime that is a session of its own.</param>
        internal static SuperMetroidRuntime Create(
            SuperMetroid.Core.Gameplay.GameplayAssists assists,
            GameplayArtwork artwork,
            SuperMetroid.Core.Compatibility.SharedRandom? random = null,
            AudioQueues? audio = null) =>
            new(assists, artwork,
                new GameplaySessionCarryover
                {
                    Clock = new SuperMetroid.Core.Compatibility.FrameClock(),
                    GameState = new SharedGameStateCarryover
                    {
                        Random = random ?? new SuperMetroid.Core.Compatibility.SharedRandom(),
                        // A tooling runtime powers on as the game does with an empty save file: no valid save, no history.
                        EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.FoundAtPowerOn(
                            validSaveFound: false, gameCompleted: false, powerOnFoundNoValidSave: false),
                        // A tooling runtime's explored maps are its own, as power-on leaves them.
                        StoredExploredMaps = SuperMetroid.Core.Progression.StoredExploredMapLoads.PowerOn(),
                    },
                    Compatibility = new CompatibilityKeptAcrossGames
                    {
                        UnauthoredBoostCadence = new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
                        SaveChoiceInput = new SuperMetroid.Core.Compatibility.SaveChoiceInputCadence(),
                        MessageBoxPauses = new SuperMetroid.Core.Compatibility.MessageBoxPauses(),
                        // A tooling runtime is its session's first game, whose camera starts with no fractions, as power-on leaves them.
                        CameraFractions = new SuperMetroid.Core.Compatibility.CameraFractionsKeptAcrossGames(),
                    },
                    Audio = audio ?? AudioQueues.Standalone(),
                });
    }

    /// <summary>Provides direct room-loading seams for ROM-backed debug and extraction hosts.</summary>
    extension(SuperMetroidRuntime self)
    {
        /// <summary>
        /// Loads one retail room header directly for the ROM-backed debug runner. Gameplay never
        /// calls this seam: normal play must still arrive through a bank-$83 door so placement
        /// and door setup remain authoritative. Keeping the helper internal lets end-to-end
        /// audits exercise the complete runtime, renderer, enemy scheduler, and Samus handlers
        /// in a late room without duplicating several minutes of controller input.
        /// </summary>
        /// <param name="definition">The room.</param>
        /// <param name="camera">The camera the room loads under.</param>
        /// <param name="state">One of the room's states to load in place of the one the session's progress selects.</param>
        internal void LoadCartridgeRoomForDebug(
            RoomDefinition definition,
            Position16<RoomPixels> camera = default,
            RoomStateSettings? state = null)
        {
            if (self.Player.Samus is null)
                throw new InvalidOperationException("Direct debug room loading requires initialized Samus state.");

            SelectedRoom room = state is null ? self.RoomLoader.LoadSelectedRoom(definition)
                : RoomStateLists.GetStates(definition).Contains(state) ? new SelectedRoom(definition, state)
                : throw new ArgumentException($"{state.Name} is not a state of room {definition.Name}.", nameof(state));
            // The debug seam intentionally supplies an inert synthetic door. Any room whose
            // correctness depends on a door setup must instead be audited through its real door;
            // Ceres Ridley's ordinary mode-nine room has no incoming setup routine dependency.
            var door = new DoorDefinition(
                Name: "debug room entry",
                DestinationRoom: definition,
                Orientation: new(DoorDirection.Right, DoorClosingBehavior.None),
                RoomObjectBlock: default,
                DestinationScreen: camera.ToRoomScreen(),
                SamusSpeed: DoorSamusSpeed.None,
                Setup: DoorSetupDefinitions.None);

            self.RoomLoader.LoadRoom(
                door,
                room,
                camera,
                RoomViewportLoadMode.DisplayInitialViewport);
            self.Player.Samus.Ground = room.Definition.Ground;
            SamusPoseMetadata.RefreshCollisionRadii(self.Player.Samus);
            self.Player.GroundedSamusMovementEnabled = true;
        }
        /// <summary>
        /// Loads a retail destination through its real bank-$83 header for exhaustive callback
        /// verification. Production reaches the same private loader through door collision.
        /// </summary>
        internal void LoadCartridgeRoomThroughDoorForVerification(
            DoorDefinition door,
            Position16<RoomPixels> camera = default)
        {
            Ensure.NotNull(door);
            if (self.Player.Samus is null)
                throw new InvalidOperationException("Door verification requires initialized Samus state.");

            SelectedRoom room = self.RoomLoader.LoadSelectedRoom(door.DestinationRoom);
            self.RoomLoader.LoadRoom(
                door,
                room,
                camera,
                RoomViewportLoadMode.DisplayInitialViewport,
                new RoomEntry { RunDoorClosingObject = true });
        }

    }
}
