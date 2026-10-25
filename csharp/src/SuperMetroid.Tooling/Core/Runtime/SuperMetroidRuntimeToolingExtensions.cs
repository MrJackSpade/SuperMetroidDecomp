using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.Core.Runtime;

/// <summary>Development-tool instance members of <see cref="SuperMetroidRuntime"/>.</summary>
internal static class SuperMetroidRuntimeToolingExtensions
{
    /// <summary>Creates runtimes for hosts that run one runtime without a frontend session.</summary>
    extension(SuperMetroidRuntime)
    {
        /// <summary>
        /// Creates a bare frame-steppable runtime with its own frame clock and Mode 7 queue residue, over the supplied
        /// session's random-number word or, without one, a word of its own; only the initial
        /// palette is bound, and the caller binds whatever catalogs it exercises.
        /// </summary>
        /// <param name="overrides">The host's tester conveniences around the cartridge-authentic runtime (#633).</param>
        /// <param name="initialPaletteArt">Required installed palette image loaded into CGRAM during construction.</param>
        /// <param name="random">The session's random-number word, or null for a runtime that is a session of its own.</param>
        internal static SuperMetroidRuntime Create(
            SuperMetroid.Core.Gameplay.HostGameplayOverrides overrides,
            SuperMetroid.Core.Assets.GameplayBasePaletteCatalog? initialPaletteArt = null,
            SuperMetroid.Core.Progression.CartridgeRandom? random = null) =>
            new(overrides, new SuperMetroid.Core.Frontend.GameArtwork { GameplayBasePalettes = initialPaletteArt },
                new SuperMetroid.Core.Gameplay.FrameClock(), new SuperMetroid.Core.Compatibility.FloorUpdateResidue(),
                random ?? new SuperMetroid.Core.Progression.CartridgeRandom());
    }

    /// <summary>Provides direct room-loading seams for ROM-backed debug and extraction hosts.</summary>
    extension(SuperMetroidRuntime self)
    {
        /// <summary>
        /// Loads one retail room header directly for the ROM-backed debug runner. Gameplay never
        /// calls this seam: normal play must still arrive through a bank-$83 door so placement
        /// and setup code remain authoritative. Keeping the helper internal lets end-to-end
        /// audits exercise the complete runtime, renderer, enemy scheduler, and Samus handlers
        /// in a late room without duplicating several minutes of controller input.
        /// </summary>
        /// <param name="definition">The room.</param>
        /// <param name="camera">The camera the room loads under.</param>
        /// <param name="state">One of the room's states to load in place of the one the session's progress selects.</param>
        internal InitialViewportResult LoadCartridgeRoomForDebug(
            RoomHeaderDefinition definition,
            Position16<RoomPixels> camera = default,
            CartridgeRoomState? state = null)
        {
            if (self.Player.Samus is null)
                throw new InvalidOperationException("Direct debug room loading requires initialized Samus state.");

            CartridgeRoomHeader room = state is null ? self.RoomLoader.LoadCartridgeRoomHeader(definition)
                : RoomStateSelectionDefinitions.GetStates(definition).Contains(state) ? new CartridgeRoomHeader(definition, state)
                : throw new ArgumentException($"{state.Name} is not a state of room {definition.Name}.", nameof(state));
            // The debug seam intentionally supplies an inert synthetic door. Any room whose
            // correctness depends on setup code must instead be audited through its real door;
            // Ceres Ridley's ordinary mode-nine room has no incoming setup routine dependency.
            var door = new DoorDefinition(
                Name: "debug room entry",
                DestinationRoom: definition,
                Orientation: new(DoorDirection.Right, DoorClosingBehavior.None),
                PlmBlock: default,
                DestinationScreen: camera.ToRoomScreen(),
                SamusDistance: 0,
                SetupCode: DoorSetupCode.None);

            // A door arrives with the HUD's characters at their ordinary base, which a direct load from mid-fight in Kraid's
            // room, where his tilemap covers them, would not (#1392).
            if (self.Hud.IsInitialized)
                self.HudSetup.ShowHudCharacters();
            InitialViewportResult viewport = self.RoomLoader.LoadCartridgeRoom(
                door,
                room,
                camera,
                RoomViewportLoadMode.DisplayInitialViewport);
            self.Player.Samus.LiquidPhysics.RoomIdentity = room.Identity;
            SamusPoseMetadata.RefreshCollisionRadii(self.Player.Samus);
            self.Player.Samus.Presentation.PrimeGraphics(self.Player.Samus);
            self.Player.GroundedSamusMovementEnabled = true;
            return viewport;
        }
        /// <summary>
        /// Loads a retail destination through its real bank-$83 header for exhaustive callback
        /// verification. Production reaches the same private loader through door collision.
        /// </summary>
        internal InitialViewportResult LoadCartridgeRoomThroughDoorForVerification(
            DoorDefinition door,
            Position16<RoomPixels> camera = default)
        {
            Ensure.NotNull(door);
            if (self.Player.Samus is null)
                throw new InvalidOperationException("Door verification requires initialized Samus state.");

            CartridgeRoomHeader room = self.RoomLoader.LoadCartridgeRoomHeader(door.DestinationRoom);
            return self.RoomLoader.LoadCartridgeRoom(
                door,
                room,
                camera,
                RoomViewportLoadMode.DisplayInitialViewport,
                new RoomEntry { RunDoorClosingPlm = true });
        }

    }
}
