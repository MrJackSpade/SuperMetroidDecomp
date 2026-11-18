using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Runtime;

/// <summary>Verification access to the kind of frame a published <see cref="GameplayDisplayFrame"/> latched (#2498).</summary>
internal static class GameplayDisplayFrameAccess
{
    /// <summary>The published frame's latch as the kind of frame verification expects.</summary>
    extension(GameplayDisplayFrame self)
    {
        /// <summary>The room's frame the published frame latched.</summary>
        /// <exception cref="InvalidOperationException">The frame latched no room's frame.</exception>
        internal RoomFrameLatch RoomLatch => self.Latched as RoomFrameLatch
            ?? throw new InvalidOperationException($"The published frame latched {self.Latched.GetType().Name}, not a room's frame.");

        /// <summary>The ordinary room frame the published frame latched.</summary>
        /// <exception cref="InvalidOperationException">The frame latched another kind of frame.</exception>
        internal OrdinaryRoomFrameLatch OrdinaryLatch => self.Latched as OrdinaryRoomFrameLatch
            ?? throw new InvalidOperationException($"The published frame latched {self.Latched.GetType().Name}, not an ordinary room's frame.");

        /// <summary>The published ordinary frame's base: the room's scene and the HUD's layer.</summary>
        /// <exception cref="InvalidOperationException">The frame latched another kind of frame.</exception>
        internal OrdinaryGameplayBase OrdinaryBase => GameplayDisplayCapture.CaptureOrdinaryBase(self.OrdinaryLatch, self.FrameEnd);

        /// <summary>The published ordinary frame's base as a scene of its own: the room's scene, then the HUD's layer.</summary>
        /// <exception cref="InvalidOperationException">The frame latched another kind of frame.</exception>
        internal LayeredScene OrdinaryBaseScene => new(self.Paint, [self.OrdinaryBase.Room, self.OrdinaryBase.Hud], DisplayBrightness.Full);

        /// <summary>The power bomb's live window inputs drawn in the colour the frame latched.</summary>
        internal PowerBombRenderSnapshot PowerBomb => GameplayDisplayCapture.PowerBomb(self.FrameEnd, self.RoomLatch);

        /// <summary>The suit pickup's live light beam drawn in the colour the frame latched, or null while inactive.</summary>
        internal SamusSuitPickupFlash? SuitPickup => GameplayDisplayCapture.SuitPickup(self.FrameEnd, self.RoomLatch);
    }
}
