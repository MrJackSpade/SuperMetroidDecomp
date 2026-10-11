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
            if (self.Samus is null)
                throw new InvalidOperationException("Direct debug room loading requires initialized Samus state.");

            CartridgeRoomHeader room = self.LoadCartridgeRoomHeader(roomPointer);
            // The debug seam intentionally supplies an inert synthetic door. Any room whose
            // correctness depends on setup code must instead be audited through its real door;
            // Ceres Ridley's ordinary mode-nine room has no incoming setup routine dependency.
            var door = new CartridgeDoorHeader(
                Pointer: 0,
                DestinationRoomPointer: roomPointer,
                Orientation: new(DoorDirection.Right, DoorClosingBehavior.None),
                PlmBlock: Point32<RoomBlocks>.Zero,
                DestinationScreen: new(camera.X >> 8, camera.Y >> 8),
                SamusDistance: 0,
                SetupCodePointer: DoorSetupCode.None);

            self.ActiveLoadStation = null;
            self.CeresElevatorArrival = null;
            InitialViewportResult viewport = self.LoadCartridgeRoom(
                door,
                room,
                camera.X,
                camera.Y,
                RoomViewportLoadMode.DisplayInitialViewport);
            self.Samus.LiquidPhysics.RoomIdentity = room.Identity;
            self.Samus.RefreshCollisionRadii(self._addressSpace);
            self.Samus.PrimeGraphics(self._addressSpace);
            self.GroundedSamusMovementEnabled = true;
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
            ArgumentNullException.ThrowIfNull(door);
            if (self.Samus is null)
                throw new InvalidOperationException("Door verification requires initialized Samus state.");

            CartridgeRoomHeader room = self.LoadCartridgeRoomHeader(door.DestinationRoomPointer);
            return self.LoadCartridgeRoom(
                door,
                room,
                camera.X,
                camera.Y,
                RoomViewportLoadMode.DisplayInitialViewport,
                runDoorClosingPlm: true);
        }
    }
}
