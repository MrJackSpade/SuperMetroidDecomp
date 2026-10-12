using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Runtime;

/// <summary>Development-tool instance members of <see cref="SuperMetroidRuntime"/>.</summary>
internal static class SuperMetroidRuntimeToolingExtensions
{
    extension(SuperMetroidRuntime self)
    {
        /// <summary>
        /// Loads one retail room header directly for the ROM-backed debug runner. Gameplay never
        /// calls this seam: normal play must still arrive through a bank-$83 door so placement
        /// and setup code remain authoritative. Keeping the helper internal lets end-to-end
        /// audits exercise the complete runtime, renderer, enemy scheduler, and Samus handlers
        /// in a late room without duplicating several minutes of controller input.
        /// </summary>
        internal InitialViewportResult LoadCartridgeRoomForDebug(
            ushort roomPointer,
            Position16<RoomPixels> camera = default)
        {
            if (self.Player.Samus is null)
                throw new InvalidOperationException("Direct debug room loading requires initialized Samus state.");

            CartridgeRoomHeader room = self.RoomLoader.LoadCartridgeRoomHeader(roomPointer);
            // The debug seam intentionally supplies an inert synthetic door. Any room whose
            // correctness depends on setup code must instead be audited through its real door;
            // Ceres Ridley's ordinary mode-nine room has no incoming setup routine dependency.
            var door = new CartridgeDoorHeader(
                Pointer: 0,
                DestinationRoomPointer: roomPointer,
                Orientation: new(DoorDirection.Right, DoorClosingBehavior.None),
                PlmBlock: default,
                DestinationScreen: camera.ToRoomScreen(),
                SamusDistance: 0,
                SetupCodePointer: DoorSetupCode.None);

            self.Room.ActiveLoadStation = null;
            self.Room.CeresElevatorArrival = null;
            InitialViewportResult viewport = self.RoomLoader.LoadCartridgeRoom(
                door,
                room,
                camera,
                RoomViewportLoadMode.DisplayInitialViewport);
            self.Player.Samus.LiquidPhysics.RoomIdentity = room.Identity;
            self.Player.Samus.PoseMetadata.RefreshCollisionRadii(self._addressSpace);
            self.Player.Samus.Presentation.PrimeGraphics(self._addressSpace);
            self.Player.GroundedSamusMovementEnabled = true;
            return viewport;
        }
        /// <summary>
        /// Loads a retail destination through its real bank-$83 header for exhaustive callback
        /// verification. Production reaches the same private loader through door collision.
        /// </summary>
        internal InitialViewportResult LoadCartridgeRoomThroughDoorForVerification(
            CartridgeDoorHeader door,
            Position16<RoomPixels> camera = default)
        {
            Ensure.NotNull(door);
            if (self.Player.Samus is null)
                throw new InvalidOperationException("Door verification requires initialized Samus state.");

            CartridgeRoomHeader room = self.RoomLoader.LoadCartridgeRoomHeader(door.DestinationRoomPointer);
            return self.RoomLoader.LoadCartridgeRoom(
                door,
                room,
                camera,
                RoomViewportLoadMode.DisplayInitialViewport,
                runDoorClosingPlm: true);
        }
    }
}
