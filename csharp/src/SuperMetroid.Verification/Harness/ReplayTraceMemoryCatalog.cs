using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Harness;

using SuperMetroid.Verification.Bosses.Ridley;

/// <summary>
/// The single source of truth for which native WRAM bytes the SMV replay verifiers read from each
/// checkpoint, and therefore which bytes a compact replay trace keeps. Every checkpoint read in
/// <see cref="FullMovieReplayVerification"/> (comparison, sound acknowledgements and the
/// <c>--trace-from</c> diagnostic) lies in <see cref="MovieDesync"/>; every read in
/// <see cref="RidleyFullMovieVerification.VerifyRidleyFullMovie"/> and its draw oracles lies in
/// <see cref="RidleyShowcase"/>. A read outside its layout fails loudly in
/// <see cref="NativeCheckpointMemory"/>; widening a layout changes its hash and requires recompacting
/// the committed traces from their full captures.
/// </summary>
/// <remarks>
/// The initial checkpoint is kept whole (<c>initial.wram.gz</c>), so the one initial-state import
/// (<see cref="NativeSnapshotImportVerification.ImportNativeSnapshot"/>) is not limited by these ranges.
/// </remarks>
internal static class ReplayTraceMemoryCatalog
{
    /// <summary>Bytes in a complete native WRAM image ($7E:0000-$7F:FFFF).</summary>
    public const int WorkRamByteCount = SuperMetroidAddressSpace.WorkRamByteCount;

    /// <summary>$0629-$064F: music queue ($0629-$0640), sound queues ($0643-$064B) and current sounds ($064D-$064F).</summary>
    public const int AudioQueuesStart = 0x0629, AudioQueuesEnd = 0x0650;

    /// <summary>$0943-$0947: escape timer status word and its BCD centiseconds, seconds and minutes bytes.</summary>
    public const int EscapeTimerStart = 0x0943, EscapeTimerEnd = 0x0948;

    /// <summary>$099C: bank-$82 door-transition dispatcher, printed by the movie trace diagnostic.</summary>
    public const int DoorTransitionFunction = 0x099c;

    /// <summary>$0A94/$0A96: Samus animation timer and frame, printed by the movie trace diagnostic.</summary>
    public const int SamusAnimationStart = 0x0a94, SamusAnimationEnd = 0x0a98;

    /// <summary>$0B2C/$0B2E: Samus Y subspeed and speed, printed by the movie trace diagnostic.</summary>
    public const int SamusVerticalSpeedStart = 0x0b2c, SamusVerticalSpeedEnd = 0x0b30;

    /// <summary>$0B42-$0B4B: extra run, base speeds and acceleration mode, printed by the movie trace diagnostic.</summary>
    public const int SamusHorizontalSpeedsStart = 0x0b42, SamusHorizontalSpeedsEnd = 0x0b4c;

    /// <summary>$0B64-$0C2B: the ten-slot X/Y/fraction/radius/velocity/direction/type projectile arrays.</summary>
    public const int SamusProjectileArraysStart = 0x0b64, SamusProjectileArraysEnd = 0x0c2c;

    /// <summary>$0DBA-$0DBF: slope-adjusted flag and total horizontal speed, printed by the movie trace diagnostic.</summary>
    public const int SamusSlopeAndTotalSpeedStart = 0x0dba, SamusSlopeAndTotalSpeedEnd = 0x0dc0;

    /// <summary>Bytes of each enemy slot the movie replay reads: identity through instruction timer ($00-$1D).</summary>
    public const int MovieEnemySlotPrefix = 0x1e;

    /// <summary>$0FA8-$0FAD: slot-zero Ridley AI function and velocities, printed by the movie trace diagnostic.</summary>
    public const int RidleySlotWordsStart = 0x0fa8, RidleySlotWordsEnd = 0x0fae;

    /// <summary>$106A-$106D: Baby Metroid cutscene velocities in slot three, printed by the movie trace diagnostic.</summary>
    public const int BabyMetroidVelocityStart = 0x106a, BabyMetroidVelocityEnd = 0x106e;

    /// <summary>$1B23-$1BB2: enemy-projectile Var1/F, instruction, spritemap and instruction-timer arrays.</summary>
    public const int EnemyProjectileVariableToTimerStart = 0x1b23, EnemyProjectileVariableToTimerEnd = 0x1bb3;

    /// <summary>$1F51: cinematic function, printed by the movie trace diagnostic.</summary>
    public const int CinematicFunction = 0x1f51;

    /// <summary>$8026: Ridley getaway Mode 7 table index, printed by the movie trace diagnostic.</summary>
    public const int RidleyGetawayIndex = 0x8026;

    /// <summary>$0370-$058F: native low and high OAM staging tables.</summary>
    public const int OamStart = 0x0370, OamEnd = 0x0590;

    /// <summary>$07F7-$08F6: live current-area explored-map plane.</summary>
    public const int LiveExploredMapEnd = 0x08f7;

    /// <summary>$09C0-$09D7: reserve mode, health, ammunition, selected HUD item and reserve energy.</summary>
    public const int SamusResourcesStart = 0x09c0, SamusResourcesEnd = 0x09d8;

    /// <summary>$0A1C-$0A27: pose, movement type, previous and last-different pose/movement words.</summary>
    public const int SamusPoseHistoryStart = 0x0a1c, SamusPoseHistoryEnd = 0x0a28;

    /// <summary>$0A42-$0A61: Samus handlers, slope enable, hurt flash, damage, knockback and input handler words.</summary>
    public const int SamusHandlersStart = 0x0a42, SamusHandlersEnd = 0x0a62;

    /// <summary>$0A66-$0A6F: speed divisor, shine timer, speed table and contact damage index.</summary>
    public const int SamusSpeedSelectionStart = 0x0a66, SamusSpeedSelectionEnd = 0x0a70;

    /// <summary>$0A94-$0A9D: animation timer, frame and liquid animation frame buffer.</summary>
    public const int SamusAnimationBufferEnd = 0x0a9e;

    /// <summary>$0AA6-$0AAD: arm-cannon flags, frame, toggle and drawing mode.</summary>
    public const int CannonStateEnd = 0x0aae;

    /// <summary>$0AD2-$0B4C: liquid type, atmospherics, position, radii, sprite origin, cameras, speeds and deceleration byte.</summary>
    public const int SamusMotionStart = 0x0ad2, SamusMotionEnd = 0x0b4d;

    /// <summary>$0B56-$0CD5: extra displacement, shot direction, charge palette, projectile arrays and firing counters.</summary>
    public const int SamusFiringStart = 0x0b56, SamusFiringEnd = 0x0cd6;

    /// <summary>$0DA2-$0DC3: camera speed, projectile inheritance, slope flag, total speed and charge words.</summary>
    public const int CameraSpeedToChargeEnd = 0x0dc4;

    /// <summary>$18A8-$18AD: invincibility, knockback and projectile-interaction timers.</summary>
    public const int SamusTimersStart = 0x18a8, SamusTimersEnd = 0x18ae;

    /// <summary>$1997-$1C1E: every enemy-projectile word array from identity through variable G.</summary>
    public const int EnemyProjectileArraysEnd = 0x1c1f;

    /// <summary>$1C37-$1E66: PLM header through family-variable arrays.</summary>
    public const int PlmArraysEnd = 0x1e67;

    /// <summary>$2000-$20AB: Ridley tail controller words and the seven tail records.</summary>
    public const int RidleyTailControllerEnd = 0x20ac;

    /// <summary>$7800-$783D: Ridley's extra enemy variables.</summary>
    public const int RidleyExtraEnd = 0x783e;

    /// <summary>$C000-$C1FF: the 256-color native palette buffer.</summary>
    public const int PaletteBufferEnd = 0xc200;

    /// <summary>$D658-$D7BF: the ten eighteen-slot projectile-trail arrays.</summary>
    public const int TrailArraysEnd = 0xd7c0;

    /// <summary>$D820-$D913: events, boss bits, collected items, opened doors and station markers.</summary>
    public const int ProgressBitsEnd = 0xd914;

    /// <summary>$DE1C-$DF5B: PLM instruction timer, link instruction and extra variable arrays.</summary>
    public const int PlmTimersEnd = 0xdf5c;

    /// <summary>$F380-$F433: enemy-projectile collision option, source enemy header and killed-enemy arrays.</summary>
    public const int EnemyProjectileDropArraysEnd = 0xf434;

    /// <summary>Ridley's tail is seven ten-word records from $7E:2020.</summary>
    public const int RidleyTailSegmentCount = 7;

    /// <summary>Bytes in each decompressed level buffer: $7F:0002 collision words, then $7F:6402 BTS.</summary>
    public const int LevelWordBytes = 0x6400, BtsBytes = 0x3200;

    /// <summary>
    /// The five playthrough movies' layout (<c>--full-playthrough-movie</c>, <c>--lsmv-playthrough-movie</c>,
    /// <c>--drunk-playthrough-movie</c>, <c>--mother-brain-glitch-movie</c>, <c>--lowc-playthrough-movie</c>): the desynchronization
    /// comparison, the sound-acknowledgement states and the <c>--trace-from</c> diagnostic fields.
    /// </summary>
    public static ReplayTraceLayout MovieDesync { get; } = new("movie-desync",
    [
        WorkRamRange.Words(MovieDesyncMemory.NmiCounter),
        WorkRamRange.Words(MovieDesyncMemory.Random),
        WorkRamRange.Between(AudioQueuesStart, AudioQueuesEnd),
        WorkRamRange.Words(MovieDesyncMemory.Room),
        WorkRamRange.Between(MovieDesyncMemory.CameraXSubposition, MovieDesyncMemory.CameraY + sizeof(ushort)),
        WorkRamRange.Words(NativeSnapshotMemory.CeresStatus),
        WorkRamRange.Between(EscapeTimerStart, EscapeTimerEnd),
        WorkRamRange.Words(MovieDesyncMemory.GameState),
        WorkRamRange.Words(DoorTransitionFunction),
        WorkRamRange.Between(MovieDesyncMemory.EquippedItems, MovieDesyncMemory.CollectedBeams + sizeof(ushort)),
        WorkRamRange.Between(MovieDesyncMemory.Health, MovieDesyncMemory.Reserve + sizeof(ushort)),
        WorkRamRange.Words(MovieDesyncMemory.SamusPose),
        WorkRamRange.Between(SamusAnimationStart, SamusAnimationEnd),
        WorkRamRange.Between(MovieDesyncMemory.SamusX, MovieDesyncMemory.SamusYFraction + sizeof(ushort)),
        WorkRamRange.Between(SamusVerticalSpeedStart, SamusVerticalSpeedEnd),
        WorkRamRange.Between(SamusHorizontalSpeedsStart, SamusHorizontalSpeedsEnd),
        WorkRamRange.Between(SamusProjectileArraysStart, SamusProjectileArraysEnd),
        WorkRamRange.Between(SamusSlopeAndTotalSpeedStart, SamusSlopeAndTotalSpeedEnd),
        .. Enumerable.Range(0, EnemySlotTable.MaximumEnemyCount).Select(slot =>
            new WorkRamRange(MovieDesyncMemory.EnemyBase + slot * MovieDesyncMemory.EnemyStride, MovieEnemySlotPrefix)),
        WorkRamRange.Between(RidleySlotWordsStart, RidleySlotWordsEnd),
        WorkRamRange.Between(BabyMetroidVelocityStart, BabyMetroidVelocityEnd),
        WorkRamRange.Words(MovieDesyncMemory.EnemyProjectileIds, MovieDesyncMemory.EnemyProjectileSlots),
        WorkRamRange.Words(MovieDesyncMemory.EnemyProjectileX, MovieDesyncMemory.EnemyProjectileSlots),
        WorkRamRange.Words(MovieDesyncMemory.EnemyProjectileY, MovieDesyncMemory.EnemyProjectileSlots),
        WorkRamRange.Between(EnemyProjectileVariableToTimerStart, EnemyProjectileVariableToTimerEnd),
        WorkRamRange.Words(CinematicFunction),
        WorkRamRange.Words(NativeSnapshotMemory.RidleyTailFunctionIndex),
        new(MovieDesyncMemory.RidleyTailSegments, MovieDesyncMemory.RidleyTailSegmentStride * RidleyTailSegmentCount),
        WorkRamRange.Words(RidleyGetawayIndex),
    ]);

    /// <summary>
    /// The Ridley showcase movie's layout (<c>--ridley-full-movie</c>): every Samus, camera, HUD, map,
    /// palette, enemy, Ridley, PLM, projectile, trail and level-buffer field it compares, plus the
    /// OAM and Samus words its cartridge draw oracles read.
    /// </summary>
    public static ReplayTraceLayout RidleyShowcase { get; } = new("ridley-showcase",
    [
        WorkRamRange.Between(OamStart, OamEnd),
        WorkRamRange.Words(NativeSnapshotMemory.PowerBombExplosionStatus),
        WorkRamRange.Between(NativeSnapshotMemory.NmiCounterByte, NativeSnapshotMemory.NmiCounter + sizeof(ushort)),
        WorkRamRange.Words(NativeSnapshotMemory.Random),
        WorkRamRange.Words(NativeSnapshotMemory.MinimapDisabled),
        WorkRamRange.Words(NativeSnapshotMemory.EnemyDoorTransition),
        WorkRamRange.Words(NativeSnapshotMemory.Room),
        WorkRamRange.Words(NativeSnapshotMemory.CurrentArea),
        WorkRamRange.Between(NativeSnapshotMemory.LiveExploredMap, LiveExploredMapEnd),
        WorkRamRange.Between(NativeSnapshotMemory.CameraXFraction, NativeSnapshotMemory.CameraY + sizeof(ushort)),
        WorkRamRange.Words(NativeSnapshotMemory.CeresStatus),
        WorkRamRange.Words(NativeSnapshotMemory.GameState),
        WorkRamRange.Words(NativeSnapshotMemory.DoorFunction),
        WorkRamRange.Between(NativeSnapshotMemory.Items, NativeSnapshotMemory.CollectedBeams + sizeof(ushort)),
        WorkRamRange.Between(SamusResourcesStart, SamusResourcesEnd),
        WorkRamRange.Words(NativeSnapshotMemory.AutoCancelHudItemIndex),
        WorkRamRange.Words(NativeSnapshotMemory.PreviousHudSelection),
        WorkRamRange.Words(NativeSnapshotMemory.SamusPreviousHealthForFlash),
        WorkRamRange.Between(SamusPoseHistoryStart, SamusPoseHistoryEnd),
        WorkRamRange.Between(SamusHandlersStart, SamusHandlersEnd),
        WorkRamRange.Between(SamusSpeedSelectionStart, SamusSpeedSelectionEnd),
        WorkRamRange.Words(NativeSnapshotMemory.HyperBeam),
        WorkRamRange.Between(NativeSnapshotMemory.AnimationTimer, SamusAnimationBufferEnd),
        WorkRamRange.Between(NativeSnapshotMemory.CannonFlags, CannonStateEnd),
        WorkRamRange.Between(NativeSnapshotMemory.SamusTopSpritemap, NativeSnapshotMemory.SamusBottomSpritemap + sizeof(ushort)),
        WorkRamRange.Between(SamusMotionStart, SamusMotionEnd),
        WorkRamRange.Between(SamusFiringStart, SamusFiringEnd),
        WorkRamRange.Words(NativeSnapshotMemory.PowerBombArmedFlag),
        WorkRamRange.Between(NativeSnapshotMemory.CameraSpeedX, CameraSpeedToChargeEnd),
        WorkRamRange.Words(NativeSnapshotMemory.SamusFilteredHeld, 2),
        new(NativeSnapshotMemory.EnemyBase, EnemySlotTable.MaximumEnemyCount * EnemySlotTable.EnemyIndexStride),
        WorkRamRange.Words(NativeSnapshotMemory.SamusSolidEnemyIndices, 4),
        WorkRamRange.Between(SamusTimersStart, SamusTimersEnd),
        WorkRamRange.Words(NativeSnapshotMemory.AcidSurface),
        WorkRamRange.Words(NativeSnapshotMemory.TidePhase),
        WorkRamRange.Between(NativeSnapshotMemory.EnemyProjectileId, EnemyProjectileArraysEnd),
        WorkRamRange.Between(NativeSnapshotMemory.PlmHeaders, PlmArraysEnd),
        WorkRamRange.Between(NativeSnapshotMemory.RidleyTailFunctionIndex, RidleyTailControllerEnd),
        WorkRamRange.Between(NativeSnapshotMemory.RidleySwoopTimer, RidleyExtraEnd),
        WorkRamRange.Between(NativeSnapshotMemory.PaletteBuffer, PaletteBufferEnd),
        WorkRamRange.Words(NativeSnapshotMemory.HudTilemap, HudState.MutableTileCount),
        WorkRamRange.Between(NativeSnapshotMemory.ScrollStorage,
            NativeSnapshotMemory.SavedExploredMaps + SharedGameState.ExploredMapAreaCount * SharedGameState.ExploredMapBytesPerArea),
        WorkRamRange.Between(NativeSnapshotMemory.TrailLeftInstructionTimer, TrailArraysEnd),
        WorkRamRange.Between(NativeSnapshotMemory.Events, ProgressBitsEnd),
        WorkRamRange.Between(NativeSnapshotMemory.PlmInstructionTimer, PlmTimersEnd),
        WorkRamRange.Between(NativeSnapshotMemory.EnemyProjectileCollisionOption, EnemyProjectileDropArraysEnd),
        new(NativeSnapshotMemory.Level, LevelWordBytes + BtsBytes),
    ]);
}
