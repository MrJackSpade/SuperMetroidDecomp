using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>The Ceres station's destruction and the approach to Zebes: their timing, motion, floor views and actor slots.</summary>
internal static class CeresDestructionSceneDefinitions
{
    /// <summary>Updates the scene waits before it sets up the station.</summary>
    internal const int InitialFrameWaits = 9;

    /// <summary>The gunship engine's flickering colours, started when the approach to Zebes is set up.</summary>
    internal const PaletteFxObject GunshipEngineFlicker = PaletteFxObject.CutsceneGunshipEngineFlicker;

    /// <summary>
    /// The floor point, in floor pixels, the Ceres explosion's floor turns about before its fade-out; the station's
    /// explosions are also placed relative to it.
    /// </summary>
    internal static Offset16<FloorPixels> CeresFloorCenter => new(52, 48);

    /// <summary>The floor point, in floor pixels, the following Zebes approach's floor turns about.</summary>
    internal static Offset16<FloorPixels> ZebesFloorCenter => new(56, 24);

    /// <summary>The width, in pixels, of the world the approach's actors wrap around horizontally.</summary>
    internal const int WorldWidth = 512;

    /// <summary>The cinematic's shared sprite characters loaded over the start of the station's: 208 of them.</summary>
    internal const int SharedObjectCharacterBytes = 208 * CeresFlightArtworkFormat.ObjectCharacterBytes;

    /// <summary>The tracks the scene plays.</summary>
    internal static class Music
    {
        /// <summary>The station's destruction track when Samus has escaped.</summary>
        internal const int CeresTrack = 7;
        /// <summary>The station's destruction track when Samus is still aboard.</summary>
        internal const int CeresWithSamusTrack = 8;
        /// <summary>The queue delay the destruction track is requested with.</summary>
        internal const int QueueDelay = 14;
    }

    /// <summary>Authored holds and the planet reveal's mosaic fade, in updates.</summary>
    internal static class Timing
    {
        /// <summary>Updates the completed Ceres explosion stays shown.</summary>
        internal const int ExplosionHoldFrames = 192;
        /// <summary>Updates the completed Zebes approach stays shown.</summary>
        internal const int ZebesHoldFrames = 64;
        /// <summary>The planet reveal's starting mosaic size; each fade step lowers it by one until it clears at zero.</summary>
        internal const int InitialMosaicSize = 8;
        /// <summary>The mosaic sizes the screen's mosaic setting holds; lowering the smallest wraps to the largest.</summary>
        internal const int MosaicSizeCount = 16;
    }

    /// <summary>The floor's zoom thresholds and the scenes' fixed-point velocities.</summary>
    internal static class Motion
    {
        /// <summary>The floor's unscaled zoom, in 1/256 units.</summary>
        internal const int IdentityScale = 256;
        /// <summary>The zoom that ends the initial Ceres zoom.</summary>
        internal const int CeresZoomLimit = 640;
        /// <summary>The zoom the Ceres explosion's composition begins at.</summary>
        internal const int CeresExplosionScale = 768;
        /// <summary>The smallest zoom the sequence keeps.</summary>
        internal const int MinimumScale = 16;
        /// <summary>The zoom that ends the near Zebes approach.</summary>
        internal const int ZebesApproachScaleLimit = 1152;
        /// <summary>The zoom that separates the far Zebes approach.</summary>
        internal const int FarZebesScaleLimit = 8192;
        /// <summary>The slow per-update zoom change.</summary>
        internal const int SlowScaleStep = 16;
        /// <summary>The fast per-update zoom change.</summary>
        internal const int FastScaleStep = 32;
        /// <summary>One eighth of a pixel per update, in 1/65536 pixels.</summary>
        internal const int EighthPixel16Point16 = 8192;
        /// <summary>Minus one half pixel per update, in 1/65536 pixels.</summary>
        internal const int NegativeHalfPixel16Point16 = -32768;
        /// <summary>One sixteenth of a pixel per update, in 1/65536 pixels.</summary>
        internal const int SixteenthPixel16Point16 = 4096;
        /// <summary>Minus one quarter pixel per update, in 1/65536 pixels.</summary>
        internal const int NegativeQuarterPixel16Point16 = -16384;
        /// <summary>The fleeing gunship's horizontal speed: a quarter pixel per update, in 1/65536 pixels.</summary>
        internal const int GunshipXSubvelocity = 16384;
        /// <summary>The fleeing gunship's vertical velocity: one sixteenth of a pixel upward per update.</summary>
        internal static SignedSixteenSixteen GunshipYVelocity { get; } = SignedSixteenSixteen.FromSixteenthsOfPixel(-1);
        /// <summary>The floor position, in pixels, that completes the gunship's exit.</summary>
        internal const int FloorExitX = 62;
        /// <summary>The floor's rotation during the Zebes approach: an eighth of a turn.</summary>
        internal static RotationAngle ApproachAngle => RotationAngle.FromTableIndex(32);
    }

    /// <summary>The scene's reserved actor slots, the explosions' palette and the approach's starting scroll.</summary>
    internal static class Sprites
    {
        /// <summary>The large asteroid's slot, the first one allocated.</summary>
        internal const int AsteroidSlot = 15;
        /// <summary>The small asteroids' reserved slot.</summary>
        internal const int SmallAsteroidSlot = 1;
        /// <summary>The vortex's reserved slot.</summary>
        internal const int VortexSlot = 0;
        /// <summary>The slot of the invisible actor that spawns the explosions.</summary>
        internal const int SpawnerSlot = 14;
        /// <summary>The Zebes approach's starting horizontal scroll, in pixels.</summary>
        internal const int ZebesInitialBackgroundX = 128;
    }
}
