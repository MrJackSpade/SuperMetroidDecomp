using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

// Legacy namespace is persisted in debugger identities; ownership is now platform-neutral.
namespace SuperMetroid.Desktop;

/// <summary>
/// One group of fields a later build added to a serialized type. A debugger state captured before
/// the addition omits every field of the group; restoration applies <see cref="Initialize"/> after
/// all saved fields are set, and the warning names what the old capture cannot supply.
/// </summary>
internal sealed record DebuggerFieldIntroduction(
    string TypeName, string[] Fields, string? Warning, Action<object>? Initialize = null);

/// <summary>
/// Explicit, loss-aware compatibility for older debugger object layouts. A state names each saved
/// field, so an older layout is accepted exactly when every field it omits belongs to a registered
/// introduction that it omits in full. Partial or unregistered omissions are rejected; a value is
/// never inferred from a field count.
/// </summary>
internal static class DebuggerStateFieldMigrations
{
    private const string Suppression = "lacks producer-time sound suppression; retaining its previous unsuppressed publication behavior.";

    internal static readonly DebuggerFieldIntroduction[] Introductions =
    [
        new(typeof(SuperMetroidGameOptions).FullName!, ["<DoorTransitionAutosave>k__BackingField"], null,
            options => Set(options, "<DoorTransitionAutosave>k__BackingField", true)),
        new(typeof(SuperMetroidGameOptions).FullName!, ["<ResetBossesOnLoad>k__BackingField"],
            "Older debugger options predate boss-reset-on-load; leaving it disabled."),
        new(typeof(SuperMetroidGameOptions).FullName!, ["<GrantAllEquipment>k__BackingField", "<UnlockTourian>k__BackingField"],
            "Older debugger options predate full-inventory and Tourian tester settings; leaving both disabled."),
        // Omitted bool/nullable values restore as false/null: no countdown clamp and no
        // ending-time override, matching the capabilities of the original nine-option host.
        new(typeof(SuperMetroidGameOptions).FullName!, ["<PreventEscapeTimeout>k__BackingField", "<EndingTimeOverrideMinutes>k__BackingField"],
            "Older debugger options predate escape-timeout and ending-time overrides; leaving both disabled."),

        new(typeof(TorizoEnemyState).FullName!, ["<PaletteTransition>k__BackingField"],
            "Older Torizo state lacks fade targets/progress; retaining current colors until the next palette target instruction. The old snapshot cannot recover an already-running fade."),
        new("SuperMetroid.Core.Frontend.EndingCreditsState", ["shootingStars"],
            "Older ending state lacks shooting-star records; restarting the native star sequence on the next post-credits step."),

        // Runtime additions: statue owner (5ff0476a), timeout option (fc59514a), escape quake
        // (a74aa6d1), treadmill owner (e361a6b1), Ceres haze ownership (4f3e4bec).
        new(typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime).FullName!, ["_tourianStatues"],
            "Older debugger state predates the statue sequence; it initializes on room entry."),
        new(typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime).FullName!, ["<PreventEscapeTimeout>k__BackingField"],
            "Legacy runtime lacks the escape-timeout option; restoring it disabled."),
        // Constructors are bypassed by graph restoration. No animation existed in this
        // layout; normal room loading selects the next room's objects.
        new(typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime).FullName!, ["<RoomTreadmills>k__BackingField"],
            "Legacy runtime lacks treadmill state; restoring no active treadmill.",
            runtime => Set(runtime, "<RoomTreadmills>k__BackingField", new RoomTreadmillAnimatedTilesState())),
        new(typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime).FullName!, ["<CeresHaze>k__BackingField"],
            "Legacy runtime predates Ceres haze ownership; restoring no active haze.",
            runtime => Set(runtime, "<CeresHaze>k__BackingField", new CeresHazeState())),
        new(typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime).FullName!,
            ["testerInventoryRecipient", "<GrantAllEquipmentEnabled>k__BackingField", "<UnlockTourianEnabled>k__BackingField"],
            "Older runtime predates inventory/Tourian tester policy; restoring disabled options and no inventory recipient."),
        new(typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime).FullName!, ["_roomSpikes"],
            "Older runtime has no horizontal-spike animation; restarting the selected room's spike loop at frame zero."),
        // The door-scrolling IRQ request is set only while a door scroll runs; builds
        // that did not model it never deferred NMI, which false reproduces.
        new(typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime).FullName!, ["_doorScrollingIrqRequestsNmi"],
            "Legacy runtime lacks the door-scrolling IRQ NMI request; restoring no pending request."),

        new(typeof(SamusHorizontalSpeedState).FullName!, ["<EchoSoundFlag>k__BackingField"],
            "Older speed state lacks the echo-sound flag; inferring it from the saved active boost stage. A previously stopped, stale audio loop cannot be inferred from movement state.",
            value =>
            {
                var speed = (SamusHorizontalSpeedState)value;
                speed.EchoSoundFlag = (speed.SpeedBoostCounter & SamusMovementRomData.HorizontalMotion.ActiveSpeedBoostStage) != 0
                    ? (ushort)1 : (ushort)0;
            }),

        new(typeof(SamusState).FullName!, ["<PreviousHealthForHurtCheck>k__BackingField"],
            "Older Samus state lacks draw-time health history; initializing it from saved health without inventing a hurt event.",
            value => ((SamusState)value).PreviousHealthForHurtCheck = ((SamusState)value).Health),
        // 0.2.1 captured neither per-frame pose/camera correction accumulator. Null/zero
        // preserve the saved camera checkpoint until normal movement supplies new corrections.
        new(typeof(SamusState).FullName!, ["_poseCollisionPreviousYPosition", "_poseAlignmentPreviousYDelta"],
            "Older Samus state lacks pose/camera correction accumulators; restoring no pending correction."),
        new(typeof(SamusState).FullName!, ["<BombJumpPoseInputLocked>k__BackingField"],
            "Older Samus state lacks the bomb-jump pose lock; restoring it unlocked."),
        new(typeof(SamusState).FullName!, ["_healthWarning"],
            "Older Samus state lacks the low-health warning latch; it starts inactive until the next admitted native health check."),
        // #342 adds the previously unmodeled WRAM $0E00 latch. Neutral zero avoids inventing a
        // Fire press; the next normal draw/script update populates the native latch.
        new(typeof(SamusState).FullName!, ["<PreviousDrawNewInput>k__BackingField"],
            "Older debugger state has no Samus previous-draw input latch; initializing it to neutral input."),
        new(typeof(SamusState).FullName!, ["<AutoJumpTimer>k__BackingField", "<PreviousDrawHeldInput>k__BackingField", "<AutoJumpInputPending>k__BackingField"],
            "Older Samus state lacks auto-jump history; restoring neutral history and ordinary input handling."),
        new(typeof(SamusState).FullName!, ["<ShinesparkPoseInputLocked>k__BackingField", "<CrystalFlashPoseInputLocked>k__BackingField"],
            "Older Samus state lacks special-movement pose locks; restoring both inactive."),
        new(typeof(SamusState).FullName!, ["_poseHistory"],
            "Older Samus state has no transition pose history; the unavailable history restores as zero until subsequent transitions populate it."),
        new(typeof(SamusState).FullName!, ["<StationaryScriptControlLocked>k__BackingField"],
            "Older Samus state lacks stationary script-handler ownership; retaining its saved input lock, with animation ownership unknown until the next script command."),

        // Old stock captures encode the exact remaining native fade in COLDATA. Older modded
        // captures cannot recover the counterfactual stock elapsed time. Preserve their
        // captured historical remainder rather than resetting the phase or rejecting the state.
        new(typeof(SamusPowerBombExplosionState).FullName!, ["_crystalFlashAfterglowStepsRemaining"],
            "Legacy Crystal Flash state lacks a separate control fade count; preserving its captured remaining component fade once, independently of newly bound colors. For an old modded capture this preserves its historical timing, not a recoverable stock timeline.",
            value =>
            {
                var explosion = (SamusPowerBombExplosionState)value;
                byte remaining = explosion.Phase == PowerBombExplosionPhase.CrystalFlashAfterglow
                    ? Math.Max(explosion.FixedColorRed, Math.Max(explosion.FixedColorGreen, explosion.FixedColorBlue))
                    : (byte)0;
                Set(explosion, "_crystalFlashAfterglowStepsRemaining", remaining);
            }),
        new(typeof(EnemyTileArtworkCatalog).FullName!, ["<MotherBrainBodyBg2Frames>k__BackingField"],
            "Legacy enemy catalog lacks Mother Brain body BG2 presentation; the host must rebind its current installed artwork."),
        new(typeof(SuperMetroidSaveRam).FullName!, ["mutableMemory"],
            "Legacy save manager lacks its typed SRAM capability; rebinding to its existing mutable bus.",
            saveRam => Set(saveRam, "mutableMemory", Get(saveRam, "bus") as SuperMetroid.Core.Hardware.ISnesMutableMemory
                ?? throw new InvalidDataException("Legacy save manager's bus cannot supply active SRAM."))),
        // The old room graph already contains the exact native streaming arrays. Alias those
        // arrays just as a new room with no visual override does.
        new(typeof(RoomLevelData).FullName!, ["_visualStreamingForegroundAllocation", "_visualStreamingBackgroundAllocation"],
            "Legacy room level predates editable visual-layout streams; restoring its native visual allocation.",
            level =>
            {
                Set(level, "_visualStreamingForegroundAllocation", Get(level, "_streamingForegroundAllocation"));
                Set(level, "_visualStreamingBackgroundAllocation", Get(level, "_streamingBackgroundAllocation"));
            }),

        new(typeof(SamusProjectileFrameResult).FullName!, ["<PersistentMemoryCorrupted>k__BackingField"],
            "Legacy projectile result predates native progression-memory corruption; restoring no pending corruption publication."),
        // These values describe a pending host publication, not a new cartridge word. Historical
        // captures cannot recover producer-time suppression; the next producer replaces them.
        new(typeof(SamusProjectileFrameResult).FullName!, ["<QueuedSoundSuppressed>k__BackingField"], "Legacy SamusProjectileFrameResult " + Suppression),
        new(typeof(SamusProjectileFrameResult).FullName!, ["<AdditionalSoundRequests>k__BackingField"], null),
        new(typeof(SamusSoundRequest).FullName!, ["<SoundSuppressed>k__BackingField"], "Legacy SamusSoundRequest " + Suppression),
        new(typeof(EnemySoundRequest).FullName!, ["<SoundSuppressed>k__BackingField"], "Legacy EnemySoundRequest " + Suppression),
        new(typeof(RoomFxSoundRequest).FullName!, ["<SoundSuppressed>k__BackingField"], "Legacy RoomFxSoundRequest " + Suppression),
        new(typeof(PaletteFxSoundRequest).FullName!, ["<SoundSuppressed>k__BackingField"], "Legacy PaletteFxSoundRequest " + Suppression),
        new(typeof(PlmSoundRequest).FullName!, ["<SoundSuppressed>k__BackingField"], "Legacy PlmSoundRequest " + Suppression),
        new(typeof(HudState).FullName!, ["<SelectionSoundSuppressedThisFrame>k__BackingField"], "Legacy HudState " + Suppression),
        new(typeof(SamusBombProjectileSystem).FullName!, ["<SoundSuppressedBeforeProjectileHandling>k__BackingField"], "Legacy SamusBombProjectileSystem " + Suppression),
        new(typeof(SamusShinesparkState).FullName!,
            ["<StoredShineWarningSoundSuppressed>k__BackingField", "<LaunchSoundSuppressed>k__BackingField", "<CrashSoundSuppressed>k__BackingField"],
            "Legacy shinespark requests lack producer-time suppression; retaining historical unsuppressed admission."),
        new(typeof(SamusSuitPickupState).FullName!, ["<TransformationSoundSuppressed>k__BackingField"],
            "Legacy suit pickup lacks producer-time sound suppression; retaining historical admission and any saved entry latch."),
        new(typeof(SamusSuitPickupState).FullName!, ["_transformationSoundPending"],
            "Legacy suit pickup lacks its transformation sound latch; restoring no pending sound."),
        new(typeof(HudState).FullName!, ["<MinimapDisabled>k__BackingField"],
            "Legacy HUD lacks the minimap-disabled latch; restoring the minimap enabled."),

        new(typeof(Bank80SystemState).FullName!, ["<SavedLoadingGameState>k__BackingField"],
            "Legacy bank-$80 state lacks the saved startup dispatcher; restoring ordinary main-game loading.",
            system => ((Bank80SystemState)system).LoadSavedLoadingGameState(SaveLoadingGameStates.MainGame)),
        // Save schema v2 began maintaining the cumulative $7E:D91A counter; older builds kept none.
        new(typeof(Bank80SystemState).FullName!, ["<LoadedItemCount>k__BackingField"],
            "Legacy bank-$80 state lacks the cumulative item-load counter; restarting it at zero."),

        new(typeof(SuperMetroidGame).FullName!, ["spacetimeIntroRestartSlot"],
            "Legacy frontend state predates SpaceTime intro restart ownership; restoring no pending restart."),
        new(typeof(SuperMetroidGame).FullName!, ["menuRandom"],
            "Legacy frontend lacks pre-game RNG history; preserving any loaded runtime RNG, otherwise starting the menu generator at the reset seed."),
        // 3a891459 added the native alternating pause-fade counter. Before it, every fade call
        // changed brightness; zero keeps the first restored call eligible.
        new(typeof(SuperMetroidGame).FullName!, ["pauseFadeCounter"],
            "Legacy frontend lacks pause-fade cadence; restoring the next fade step as immediately eligible."),

        new(typeof(SamusXrayState).FullName!, ["<PendingActivationPose>k__BackingField"],
            "Legacy X-Ray state lacks pending activation; restoring none pending."),
        new(typeof(SamusXrayState).FullName!, ["<OwnsSamusControl>k__BackingField"],
            "Legacy X-Ray state lacks separate Samus-control ownership; reconstructing it from its active/freeze words.",
            value => Set(value, "<OwnsSamusControl>k__BackingField", ((SamusXrayState)value).IsActive && ((SamusXrayState)value).TimeIsFrozen)),
        new(typeof(SamusXrayState).FullName!, ["<SuspendedSubsystems>k__BackingField"],
            "Legacy X-Ray state lacks independent subsystem-disable ownership; reconstructing it from the saved active/freeze words.",
            value => Set(value, "<SuspendedSubsystems>k__BackingField",
                ((SamusXrayState)value).IsActive ? XraySuspendedSubsystems.All : XraySuspendedSubsystems.None)),

        new(typeof(SuperMetroid.Core.Audio.ManagedPcmSampleBank).FullName!, ["loopEntrySources"],
            "Legacy PCM bank lacks cross-source loop routing; retaining its original self-loop behavior until the next bank upload.",
            bank => Set(bank, "loopEntrySources", new Dictionary<byte, byte>())),
        new("SuperMetroid.Core.Audio.ManagedSnesDsp+Voice", ["ReleasedBrrCursor"],
            "Legacy DSP voice lacks silent-release BRR fallback; retaining its saved PCM cursor until a native loop handoff."),
        new(typeof(SamusProjectileSlot).FullName!, ["<AuxiliaryPhase>k__BackingField"],
            "Legacy projectile slot lacks combo auxiliary phase; restoring its initial phase."),
        // Zero keeps the saved native spritemap pointer authoritative until the next
        // projectile instruction selects a new installed bank-$8D visual operand.
        new(typeof(RoomEnemyProjectileSlot).FullName!, ["<PresentationOperandAddress>k__BackingField"],
            "Legacy enemy projectile lacks installed visual-frame identity; retaining its saved native spritemap until the next instruction."),
        new(typeof(SamusProjectileSystem).FullName!, ["<ComboState>k__BackingField"],
            "Legacy projectile owner predates charge-combo state; restoring no combo pending."),
        new(typeof(SamusKinematicsState).FullName!, ["<ProbeContactDamageIndex>k__BackingField"],
            "Legacy kinematics lacks prospective-pose contact mode; retaining live owner lookup."),
        // 571b9a42 implemented Samus Eater capture. Earlier builds did not execute that capture;
        // fresh plant setup supplies the coordinates if the player is subsequently caught.
        new("SuperMetroid.Core.Rooms.RoomPlmSystem+PlmSlot", ["<PlantHeldX>k__BackingField", "<PlantHeldY>k__BackingField"],
            "Legacy PLM slot lacks plant-held coordinates; restoring zero until a new capture setup."),
        new("SuperMetroid.Core.Rooms.RoomPlmSystem", ["_pendingSpeedBoosterPickupContinuation"],
            "Legacy PLM system lacks a pending Speed Booster pickup continuation; restoring none pending."),
        // Before direct retail scroll dispatch, decoded pairs (when present) were authoritative.
        // Otherwise execution rebinds the captured retail program by room argument.
        new("SuperMetroid.Core.Rooms.RoomPlmSystem+ScrollPlmState", ["<CompiledSource>k__BackingField"], null),
        new("SuperMetroid.Core.Rooms.RoomPlmSystem+ScrollPlmState", ["<Program>k__BackingField"],
            "Legacy scroll PLM lacks decoded pairs; rebinding its captured retail program identity without cartridge access."),
        new("SuperMetroid.Core.Rooms.RoomPlmSystem+ScrollPlmState", ["<UseCompiledRetailProgram>k__BackingField"],
            "Legacy scroll PLM predates compiled retail data; restoring its legacy metadata without enabling a cartridge reader."),

        new("SuperMetroid.Core.Frontend.CeresDestructionCinematicState", ["paletteFx"],
            "Legacy Ceres cinematic state lacks engine palette-FX timing; its glow restarts on the next approach frame."),
        new("SuperMetroid.Core.Frontend.CeresDestructionCinematicState", ["zebesTitleActor"],
            "Legacy Ceres cinematic state lacks its Zebes title actor identity; the title actor is recreated when its scene starts."),
        new("SuperMetroid.Core.Frontend.CeresDestructionCinematicState", ["explosionRepeatCountdown"],
            "Legacy Ceres cinematic state lacks the repeating-explosion countdown; it is seeded when the repeating phase starts."),
        // The BG3 HDMA pre-instruction latch was added after the 0.2.0 capture used by #524.
        // False is the exact cold-start state: the next active lava/acid frame installs it.
        new(typeof(RoomLayer3FxState).FullName!, ["lavaAcidBg3PreInstructionInstalled"],
            "Legacy room FX lacks the lava/acid BG3 pre-instruction latch; restoring its cold-start state."),
        // ad3d533e replaced live ROM instruction reads with compiled definitions. The saved
        // native object pointer is the lossless key needed to rebind the compiled definition.
        new("SuperMetroid.Core.Game.RoomFxAnimatedTilesState", ["compiledMechanics"],
            "Legacy room-FX animated tiles lack their compiled mechanics binding; reconstructing it from the captured native object pointer.",
            tiles =>
            {
                RoomFxAnimatedTileMechanicsDefinitions.TryResolve((ushort)(Get(tiles, "objectPointer") ?? (ushort)0),
                    out RoomFxAnimatedTileObjectDefinition? compiled);
                Set(tiles, "compiledMechanics", compiled);
            }),
        // 18f19edc compiled the two treadmill control streams. Their saved native object
        // pointer uniquely identifies the immutable replacement definition.
        new(typeof(WreckedShipTreadmillAnimatedTilesState).FullName!, ["_compiledMechanics"],
            "Legacy Wrecked Ship treadmill lacks its compiled mechanics binding; reconstructing it from the captured native object pointer.",
            treadmill =>
            {
                WreckedShipTreadmillMechanicsDefinitions.TryResolve((ushort)(Get(treadmill, "_objectPointer") ?? (ushort)0),
                    out WreckedShipTreadmillObjectDefinition? compiled);
                Set(treadmill, "_compiledMechanics", compiled);
            }),
        // Old records only had bus sources. Default None retains their exact pending
        // address/count/destination; no image payload or timing is invented.
        new(typeof(SuperMetroid.Core.Hardware.VramWriteEntry).FullName!, ["<AssetId>k__BackingField"], null),

        new("SuperMetroid.Core.Frontend.PauseMenuState", ["mapLabelsBeforeIcons"],
            "Legacy pause state lacks destination-label draw order; order refreshes on the next pause frame."),
        new("SuperMetroid.Core.Frontend.PauseMenuState", ["mapArrows"],
            "Legacy pause state lacks map arrows; counters initialize on artwork rebind and visibility on the next stable map frame."),
        new("SuperMetroid.Core.Frontend.PauseMenuState", ["pauseNmiFrameCounter8"],
            "Legacy pause state lacks reserve fill-flicker NMI phase; restores phase zero until the next accepted frame."),
        new("SuperMetroid.Core.Frontend.PauseMenuState", ["reserveTransferSoundDelay"],
            "Legacy pause state predates manual reserve transfer; restores with no transfer pending."),

        new(typeof(SamusGrappleState).FullName!, ["<PoseChangeAutoFireTimer>k__BackingField"],
            "Legacy grapple state has no pose-change auto-fire timer; unavailable firing age restores expired until the next shot."),
        // c655c9b2 split prospective pose publication from immediate grapple state. Older
        // results could not own either pending handoff, so null is exact.
        new(typeof(GrappleMovementResult).FullName!, ["<PendingDropPose>k__BackingField", "<PendingConnection>k__BackingField"],
            "Legacy grapple result predates deferred drop and connection poses; restoring no pending pose handoff."),
        // b3e5d562 made configured bindings part of message-box state. Earlier builds always
        // used the stock X/B bindings, so reconstruct those exact defaults.
        new(typeof(GameplayMessageBoxState).FullName!, ["_shootBinding", "_runBinding"],
            "Legacy gameplay message box predates configurable bindings; restoring the stock Shoot=X and Run=B bindings.",
            box =>
            {
                Set(box, "_shootBinding", (ushort)SnesButton.X);
                Set(box, "_runBinding", (ushort)SnesButton.B);
            }),
        new(typeof(SamusDraygonGrabbedState).FullName!, ["<MovementHandlerReplaced>k__BackingField"],
            "Legacy Draygon-grab state lacks replacement-handler ownership; retaining its captured active state as the movement owner."),

        new("SuperMetroid.Core.Frontend.IntroCinematicObjectSystem", ["textGlow"],
            "Legacy intro state has no text-glow history; existing glyph ages cannot be recovered. Newly drawn glyphs start native glow normally."),
        new("SuperMetroid.Core.Frontend.IntroCinematicObjectSystem", ["currentEyeFramePointer", "currentEyePackedPosition"],
            "Legacy intro state lacks the current eye frame; the eye draws from its next animation instruction."),
        new("SuperMetroid.Core.Frontend.IntroCinematicObjectSystem",
            ["narrationPage", "narrationCharacterIndex", "narrationInitialMarkerPending", "narrationFinalHoldStarted"],
            "Legacy intro state lacks the narration cursor; restoring no narration page until the next page starts."),

        // 13% intro parity: native INIDISP and fade words, $1A57 per scene, native Rinka slots
        // and the flight's shared music queue.
        new(typeof(IntroCinematicState).FullName!, ["inidisp", "fade"],
            "Legacy intro stores only its brightness level; a fade captured between steps restarts its native step delay.",
            RestoreLegacyIntroFade),
        new(typeof(IntroCinematicState).FullName!, ["flashbackSamusDisplay"], null,
            intro => SetIntroSamusDisplay(intro, "flashbackSamusDisplay",
                Get(intro, "flashbackSamus") is not null &&
                    Get(intro, "flashbackMotherBrain") is { } motherBrain &&
                    (bool)motherBrain.GetType().GetProperty("IsVisible")!.GetValue(motherBrain)!
                    ? IntroSamusDisplaySamusFirst : IntroSamusDisplayHidden)),
        new("SuperMetroid.Core.Frontend.IntroBabyDiscoveryState", ["<SamusDisplay>k__BackingField", "samusHandlersEnded"], null,
            RestoreLegacyBabyDiscoveryDisplay),
        new("SuperMetroid.Core.Frontend.IntroRinkaSystem", ["slots", "spawnsForbidden"], null,
            RestoreLegacyRinkaSlots),
        // The parent intro's fade initializer attaches the shared queue once both are restored.
        new("SuperMetroid.Core.Frontend.IntroCeresFlightState", ["audio"],
            "Legacy Ceres flight never queued its music; a capture during the music wait resumes with an empty queue."),

        new(typeof(PhantoonBlendingState).FullName!, ["<DisplayedMosaic>k__BackingField"],
            "Legacy Phantoon display state lacks MOSAIC history; restores ungrouped until the next accepted NMI."),
        new(typeof(PhantoonEnemyState).FullName!, ["_blending"],
            "Legacy Phantoon state lacks blend HDMA history; setup restarts."),
        new(typeof(PhantoonEnemyState).FullName!, ["_wave"],
            "Legacy Phantoon state lacks wave history; it restores inactive until its next native spawn."),

        new(typeof(SuperMetroid.Core.Rendering.OrdinaryGameplayRegisters).FullName!, ["<Bg2Mosaic>k__BackingField"],
            "Legacy gameplay display registers lack BG2 mosaic; restoring original ungrouped sampling."),
        new(typeof(SuperMetroid.Core.Rendering.OrdinaryGameplayRegisters).FullName!, ["<Windows>k__BackingField", "<MainScreenWindowMask>k__BackingField"],
            "Older gameplay capture has no hardware window registers; restoring disabled windows."),
        new(typeof(SuperMetroid.Core.Rendering.OrdinaryGameplayRegisters).FullName!, ["<Bg2FirstScanline>k__BackingField", "<Bg2EndScanline>k__BackingField"],
            "Older gameplay capture has no BG2 scanline window; the next accepted NMI reconstructs it."),
        new(typeof(SuperMetroid.Core.Runtime.GameplayPpuRenderSnapshot).FullName!, ["<Bg2FirstScanline>k__BackingField", "<Bg2EndScanline>k__BackingField"],
            "Older debugger state has no BG2 scanline window; the next accepted NMI reconstructs it."),
        new(typeof(SuperMetroid.Core.Rendering.OrdinaryGameplayRenderLayer).FullName!, ["mainScreenLayersByLine"],
            "Legacy gameplay layer has no per-scanline main-screen layers; restoring its frame-wide layer selection.",
            layer => Set(layer, "mainScreenLayersByLine", Array.Empty<ushort>())),
        new(typeof(SuperMetroid.Core.Rendering.BgSubscreenAddRenderLayer).FullName!, ["<VerticalScroll>k__BackingField"],
            "Legacy subscreen layer has no vertical-scroll field; restoring its original unscrolled sampling."),
        new(typeof(SuperMetroid.Core.Rendering.Mode7RenderLayer).FullName!, ["<AddBg1Subscreen>k__BackingField"],
            "Legacy Mode 7 layer has no BG1 subscreen addition; retaining its original composition."),
        new(typeof(SuperMetroid.Core.Rendering.Mode7RenderRegisters).FullName!, ["<WrapOutsideMap>k__BackingField"],
            "Legacy Mode 7 snapshot has no wrap control; retaining its original overflow behavior."),
        new(typeof(ScrollBoundaryCamera).FullName!, ["<PreviousSamusPoint>k__BackingField"],
            "Legacy camera has no previous-scroll Samus checkpoint; initializing on its first scrolling pass."),

        new(typeof(FileSelectMenuState).FullName!, ["copyArrowPaletteTimer"],
            "Older file-select state lacks Copy arrow palette timing; restarting its initial delay.",
            menu => Set(menu, "copyArrowPaletteTimer", FileCopyArrowDefinitions.InitialPaletteDelay)),
        new(typeof(FileSelectMenuState).FullName!, ["currentPresentationPage"],
            "Older file-select state lacks its installed-presentation page; reconstructing it from the captured menu phase.",
            menu => RestoreLegacyFileSelectPresentationPage((FileSelectMenuState)menu)),

        // #397 (2026-09-15) added the saved loading dispatcher; save schema v2 (2026-10-04) named
        // three more SRAM words. These records are file-select caches re-read from SRAM.
        new(typeof(SuperMetroidSaveSlot).FullName!, ["<LoadingGameState>k__BackingField"],
            "Older decoded save slot lacks its loading-game dispatcher; restoring ordinary main-game loading.",
            slot => Set(slot, "<LoadingGameState>k__BackingField", SaveLoadingGameStates.MainGame)),
        new(typeof(SuperMetroidSaveSlot).FullName!, ["<ReserveMissiles>k__BackingField", "<JapaneseText>k__BackingField", "<LoadedItemCount>k__BackingField"],
            "Legacy save-slot cache lacks reserve missiles, text language and item-load count; restoring zero/English until SRAM is read again."),

        // 833cc4ee added this dependency for Pseudo Screw contact. EnemyMain supplies the
        // live owner before running any touch callback; null is sufficient between frames.
        new(typeof(RoomEnemySystem).FullName!, ["_samusProjectilesForEnemyFrame"],
            "Legacy enemy state lacks its frame projectile context; the next enemy phase supplies the live owner."),
        new(typeof(RoomEnemySystem).FullName!, ["<TourianEntranceStatueVerticalOffset>k__BackingField", "<TourianStatueWaterY>k__BackingField"],
            "Older debugger state has no statue displacement/water surface; initializing to zero."),
        new(typeof(RoomEnemySystem).FullName!, ["<EnemyDoorTransitionActive>k__BackingField"],
            "Legacy enemy state lacks the enemy door-transition latch; restoring no transition in progress."),

        // v0.4.0 hotfix (#1269) additions. Fields restored as their zero/false/null value need
        // no initializer: that value reproduces the earlier build's behavior.
        new("SuperMetroid.Core.Frontend.CeresDestructionCinematicState", ["initialNmiWaits"], null,
            // Older captures set the scene up at construction; restored as already past $8B:C11B.
            scene => Set(scene, "initialNmiWaits", SuperMetroid.Core.Frontend.CeresDestructionRomData.InitialNmiWaits + 1)),
        // Older boxes had no pre-open or post-close lag frames; none is pending.
        new(typeof(GameplayMessageBoxState).FullName!, ["_lagFramesRemaining"], null),
        new(typeof(GameplayMessageBoxState).FullName!, ["_selectionRedrawWaitPending"],
            "Older message box lacks the cursor-redraw wait; a capture taken right after a save-cursor move resumes one frame early."),
        // The SPC driver's $01-$03/$09-$0B sound-port pipeline, seeded from the last echoes.
        new(typeof(SuperMetroid.Core.Audio.ManagedSpcPlayer).FullName!, ["soundCommandReads", "previousSoundCommandReads"],
            "Legacy SPC state predates the sound-port read pipeline; seeding it from the last echoed commands.",
            player => SeedLegacySoundPortPipeline((SuperMetroid.Core.Audio.ManagedSpcPlayer)player)),
        // Older builds never requested the dead-room column reset; nothing is pending.
        new(typeof(KraidEnemyState).FullName!, ["<Layer1XBlockResetRequested>k__BackingField"], null),
        new(typeof(RoomEnemySystem).FullName!, ["<CameraDistanceIndex>k__BackingField"], null, SeedLegacyCameraDistanceIndex),
        // Recorded by each enemy frame for its draw hooks; clear between frames.
        new(typeof(RoomEnemySystem).FullName!, ["_enemyFrameTimeIsFrozen"], null),
        // A one-frame request between Mother Brain's tube landing and the runtime; a capture is
        // taken between frames, where it is always clear.
        new(typeof(RoomEnemySystem).FullName!, ["<MotherBrainDeletedHdmaObjects>k__BackingField"], null),
        // Door loads now defer camera-dependent init reads to the loader's update. A legacy
        // capture is never mid-door-load with such a read pending; it restores none.
        new(typeof(RoomEnemySystem).FullName!, ["_deferredRinkaSpawnSlots", "_deferLoaderTimeCameraReads"], null,
            enemies => Set(enemies, "_deferredRinkaSpawnSlots", new List<int>())),
        // Older builds mirrored the sequence's flag every call, so the two already agree.
        new(typeof(MotherBrainEnemyState).FullName!, ["<RainbowAppliedSmallPurpleBreathWrites>k__BackingField"], null),
        // Older builds never spawned the small breath, so no capture has one alive.
        new(typeof(MotherBrainEnemyState).FullName!, ["<SmallPurpleBreathActive>k__BackingField"], null),
        // The brain list moved off the head enemy.
        new(typeof(MotherBrainEnemyState).FullName!, ["<BrainInstructionPointer>k__BackingField", "<BrainInstructionTimer>k__BackingField"],
            null, state => MoveLegacyMotherBrainBrainList((MotherBrainEnemyState)state)),
        // Restores as zero, matching the zero latch on the Mother Brain state.
        new(typeof(MotherBrainRainbowBeamAttackSequence).FullName!, ["<SmallPurpleBreathGenerationWriteCount>k__BackingField"], null),
        // Pending previous-fraction writes live within one frame; a capture has none.
        new(typeof(SamusState).FullName!, ["_previousXSubpositionWriteMask", "_previousXSubpositionWriteValue",
            "_previousYSubpositionWriteMask", "_previousYSubpositionWriteValue"], null),
        // Older builds never deleted the liquid HDMA objects; a capture keeps them live.
        new(typeof(RoomLayer3FxState).FullName!, ["liquidHdmaObjectsDeleted"], null),
        new(typeof(RoomEnemySystem).FullName!, ["<GradualColorChange>k__BackingField"], null,
            enemies => Set(enemies, "<GradualColorChange>k__BackingField", new GradualColorChangeCounter())),
        // The retired private transition number supplies this counter; see DebuggerRetiredFieldDefinitions.
        new("SuperMetroid.Core.Frontend.CartridgePaletteTransition", ["numerator"], null),
        // Older builds ran the pre-instruction from the first handler call; a saved
        // transformation resumes with its pre-instruction already installed.
        new(typeof(SamusSuitPickupState).FullName!, ["_preInstructionInstallCallsRemaining"], null),
        // Older builds finished a message box's gameplay frame before the box.
        new(typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime).FullName!, ["_suspendedFrameTail"], null),
        // Older builds applied the loader's elevator placement only when the door finished.
        new(typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime).FullName!, ["_pendingLoaderSamusPlacement"], null),
        // The shared RoomMainASMVar1 replaced the debris and escape timers, which the reader
        // drains as retired fields; it is seeded from the active room main's copy.
        new(typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime).FullName!, ["<RoomMainScratch>k__BackingField"], null,
            runtime => SeedLegacyRoomMainScratch((SuperMetroid.Core.Runtime.SuperMetroidRuntime)runtime)),
        new(typeof(SuperMetroid.Core.Frontend.SuperMetroidGame).FullName!, ["bootMainLoopCarry"],
            "Older frontend lacks the boot main-loop carry; resuming with carry clear, as that build behaved."),
        new(typeof(Bank80SystemState).FullName!, ["<MainGameLoopCarry>k__BackingField"],
            "Older bank-$80 state lacks the main-loop carry; resuming with carry clear, as that build behaved."),
        // Older builds hard-coded the fade reload. Outside a pause fade the native delay word is
        // zero; a capture inside one restores the immediate-step cadence.
        new(typeof(SuperMetroid.Core.Frontend.SuperMetroidGame).FullName!, ["pauseFadeDelay"], null),
        // Door-loader progress and the door music-upload NMI source are host policy; older
        // captures restore the lag-free policy.
        new(typeof(SuperMetroid.Core.Frontend.SuperMetroidGame).FullName!, ["<DoorLoaderProgress>k__BackingField"], null,
            game => Set(game, "<DoorLoaderProgress>k__BackingField", SuperMetroid.Core.Runtime.LagFreeDoorLoaderProgress.Instance)),
        new(typeof(SuperMetroid.Core.Frontend.SuperMetroidGame).FullName!, ["<DoorMusicUploadNmis>k__BackingField"], null,
            game => Set(game, "<DoorMusicUploadNmis>k__BackingField", SuperMetroid.Core.Frontend.LagFreeDoorMusicUploadNmis.Instance)),
        // Older builds left state $09 within its first dispatch, so no capture can be inside the
        // downward-elevator delay.
        new(typeof(SuperMetroid.Core.Frontend.SuperMetroidGame).FullName!, ["waitingForDownwardsElevator", "downwardsElevatorDelayTimer"], null),
        new(typeof(SuperMetroid.Core.Frontend.SuperMetroidGame).FullName!, ["menuNmiFrameCounter", "menuNmiFrameCounter8", "gameLoadingCompletion"],
            "Legacy frontend predates native loading-dispatch and menu NMI-counter ownership; restoring zero menu NMI counts and gameplay fade-in after any pending load."),
        // Older builds folded command six into the plain input lock.
        new(typeof(SamusState).FullName!, ["<RefillStationLocked>k__BackingField"],
            "Older Samus state lacks the station-lock beta; a capture taken at a station animates Samus until release."),
        // A pending previous-X write exists only within one frame's enemy-to-scroll span.
        new(typeof(SamusState).FullName!, ["_previousXPositionWrite"], null),
        new(typeof(RoomLayer3FxState).FullName!, ["lavaSoundTimer"],
            "Legacy room FX predates lava's ambient sound timer; it restarts a full period.",
            fx => Set(fx, "lavaSoundTimer", RoomFxRomData.LavaAcid.AmbientSoundPeriod)),
        // The quarter words are rebuilt from zero by the next firing; a capture taken
        // mid-extension resumes with the clean quarter of its velocity.
        new(typeof(SamusGrappleState).FullName!, ["<XQuarterSubVelocity>k__BackingField", "<XQuarterVelocity>k__BackingField",
            "<YQuarterSubVelocity>k__BackingField", "<YQuarterVelocity>k__BackingField"], null),
        // False is the slow-scroll state of every non-swinging grapple; a swing sets it again.
        new(typeof(SamusGrappleState).FullName!, ["<SlowScrolling>k__BackingField"], null),
        // Older builds applied the release pose immediately; nothing is pending.
        new(typeof(GrappleMovementResult).FullName!, ["<PendingReleasePose>k__BackingField"], null),
        new(typeof(FileSelectMenuState).FullName!, ["screenFade"],
            "Older file-select state lacks native fade timing words; resuming with index two's delay of one.",
            fileSelect =>
            {
                var fade = new ScreenFade();
                fade.SetTiming(1, 1);
                Set(fileSelect, "screenFade", fade);
            }),
    ];

    private static readonly Dictionary<string, DebuggerFieldIntroduction[]> introductionsByType =
        Introductions.GroupBy(introduction => introduction.TypeName).ToDictionary(group => group.Key, group => group.ToArray());

    /// <summary>
    /// Resolves the introductions a saved layout predates from the fields it omits, rejecting any
    /// omission that is not an entire registered introduction of <paramref name="type"/>.
    /// </summary>
    internal static DebuggerFieldIntroduction[] ResolveOmissions(Type type, IReadOnlyCollection<string> omittedFields)
    {
        if (omittedFields.Count == 0) return [];
        DebuggerFieldIntroduction[] registered = introductionsByType.GetValueOrDefault(type.FullName!) ?? [];
        DebuggerFieldIntroduction[] omitted = registered.Where(introduction => introduction.Fields.Any(omittedFields.Contains)).ToArray();
        string[] unexplained = omittedFields.Where(field => !omitted.Any(introduction => introduction.Fields.Contains(field))).ToArray();
        string[] partial = omitted.SelectMany(introduction => introduction.Fields).Where(field => !omittedFields.Contains(field)).ToArray();
        if (unexplained.Length != 0 || partial.Length != 0)
            throw new InvalidDataException(
                $"Serialized {type.FullName} omits fields no registered legacy layout explains. " +
                $"Unregistered omissions: [{string.Join(", ", unexplained)}]; partially omitted introductions retain: [{string.Join(", ", partial)}].");
        return omitted;
    }

    /// <summary>The current fields of a type without the named, registered introductions.</summary>
    internal static FieldInfo[] WithoutIntroductions(Type type, FieldInfo[] current, params string[] introducedFields)
    {
        _ = ResolveOmissions(type, introducedFields);
        return current.Where(field => !introducedFields.Contains(field.Name)).ToArray();
    }

    /// <summary>Applies each omitted introduction's initializer after every saved field is restored.</summary>
    internal static void InitializeOmitted(object instance, IEnumerable<DebuggerFieldIntroduction> omitted)
    {
        foreach (DebuggerFieldIntroduction introduction in omitted)
            introduction.Initialize?.Invoke(instance);
    }

    /// <summary>
    /// Builds the SPC sound-port pipeline for a capture that predates it. Its driver consumed
    /// each CPU write into a $FF sentinel and echoed it at once, so the last echo is the value
    /// both pipeline words held; a still-unconsumed write remains a pending latch value.
    /// </summary>
    private static void SeedLegacySoundPortPipeline(SuperMetroid.Core.Audio.ManagedSpcPlayer player)
    {
        var inputPorts = (byte[])Get(player, "inputPorts")!;
        var reads = new byte[SuperMetroid.Core.Audio.AudioRomData.Queues.SoundLibraryCount];
        var previous = new byte[reads.Length];
        for (int library = 0; library < reads.Length; library++)
        {
            int port = library + SuperMetroid.Core.Audio.AudioRomData.Apu.FirstSoundPort;
            byte echoed = player.ReadPort(port);
            reads[library] = previous[library] = echoed;
            if (inputPorts[port] == byte.MaxValue)
                inputPorts[port] = echoed;
        }
        Set(player, "soundCommandReads", reads);
        Set(player, "previousSoundCommandReads", previous);
    }

    /// <summary>
    /// Older builds ran the brain's list on the head enemy itself. Its cursor becomes the
    /// brain list (restarting the timer) and the head returns to its dummy hitbox list.
    /// </summary>
    private static void MoveLegacyMotherBrainBrainList(MotherBrainEnemyState state)
    {
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Legacy Mother Brain state has no head enemy to carry its brain list.");
        Console.Error.WriteLine("WARNING: Legacy Mother Brain state ran the brain list on the head enemy; the brain restarts its current frame and the head takes its dummy hitbox list.");
        typeof(MotherBrainEnemyState).GetProperty(nameof(MotherBrainEnemyState.BrainInstructionPointer))!
            .SetValue(state, head.CurrentInstruction);
        typeof(MotherBrainEnemyState).GetProperty(nameof(MotherBrainEnemyState.BrainInstructionTimer))!
            .SetValue(state, (ushort)1);
        ushort initialDummy = (ushort)typeof(MotherBrainEnemyState).Assembly
            .GetType("SuperMetroid.Core.Game.MotherBrainBodyInstructionProgramDefinitions", throwOnError: true)!
            .GetField("InitialDummy", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        typeof(RoomEnemySlot).GetProperty(nameof(RoomEnemySlot.CurrentInstruction))!.SetValue(head, initialDummy);
        typeof(RoomEnemySlot).GetProperty(nameof(RoomEnemySlot.InstructionTimer))!.SetValue(head, (ushort)1);
    }

    /// <summary>
    /// Builds the shared RoomMainASMVar1 for a capture that predates it, from the private copy
    /// of whichever room main owned the active room. Earlier rooms' values were not retained.
    /// </summary>
    private static void SeedLegacyRoomMainScratch(SuperMetroid.Core.Runtime.SuperMetroidRuntime runtime)
    {
        var main = runtime.ActiveRoom?.State.MainCallback;
        (object Owner, string Field)? source =
            runtime.CeresElevatorShaft.IsActive ? (runtime.CeresElevatorShaft, "<RotationIndex>k__BackingField") :
            runtime.MaridiaElevatube.IsActive ? (runtime.MaridiaElevatube, "<PositionSubposition>k__BackingField") :
            main == RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris
                ? (runtime, "_ceresFallingDebrisTimer") :
            main is RoomMainCallback.ShakeScreenLightHorizontalAndMediumDiagonal or
                RoomMainCallback.ShakeScreenMediumHorizontalAndStrongDiagonal
                ? (runtime, "_escapeDiagonalFrames") : null;
        ushort var1 = 0;
        if (source is { } owner && !DebuggerRetiredFieldDefinitions.TryGetLegacyWord(owner.Owner, owner.Field, out var1))
            throw new InvalidDataException(
                $"Legacy runtime's active room main lacks its {owner.Field} room-main word.");
        Console.Error.WriteLine(source is null
            ? "WARNING: Legacy runtime predates the shared room-main word; no active room main owned it, so it restores as zero."
            : "WARNING: Legacy runtime predates the shared room-main word; restoring it from the active room main.");
        Set(runtime, "<RoomMainScratch>k__BackingField", new SuperMetroid.Core.Runtime.RoomMainScratchState { Var1 = var1 });
    }

    /// <summary>
    /// Seeds the shared camera distance word from legacy Kraid's private copy. A legacy
    /// Crocomire has no copy; its camera target restores as normal tracking.
    /// </summary>
    private static void SeedLegacyCameraDistanceIndex(object value)
    {
        var enemies = (RoomEnemySystem)value;
        ushort index = 0;
        if (enemies.Kraid is { } legacyKraid &&
            !DebuggerRetiredFieldDefinitions.TryGetLegacyWord(legacyKraid, "<CameraDistanceIndex>k__BackingField", out index))
            throw new InvalidDataException("Legacy Kraid state lacks its camera distance index.");
        if (enemies.Crocomire is not null)
            Console.Error.WriteLine("WARNING: Legacy enemy state predates the shared camera distance index; Crocomire's camera target restores as normal tracking.");
        Set(enemies, "<CameraDistanceIndex>k__BackingField", (CameraDistanceMode)index);
    }

    private static object? Get(object instance, string field) =>
        FindField(instance.GetType(), field).GetValue(instance);

    // $1A57 IntroSamusDisplayFlag values; the Core enum that names them is internal.
    private const short IntroSamusDisplayHidden = 0;
    private const short IntroSamusDisplayObjectsFirst = 1;
    private const short IntroSamusDisplaySamusFirst = -1;

    private static void SetIntroSamusDisplay(object instance, string field, short value) =>
        Set(instance, field, Enum.ToObject(FindField(instance.GetType(), field).FieldType, value));

    /// <summary>
    /// Seeds $51 from the legacy 0-15 level (zero was black, now forced blank) and the fade words
    /// with the native seed of a fade phase, and attaches a restored flight to the intro's queue.
    /// </summary>
    private static void RestoreLegacyIntroFade(object instance)
    {
        var intro = (IntroCinematicState)instance;
        if (!DebuggerRetiredFieldDefinitions.TryGetLegacyValue(intro, "brightness", out object? stored) ||
            stored is not int brightness || brightness is < 0 or > ScreenFade.FullyLit)
            throw new InvalidDataException("Legacy intro brightness is missing or not a 0-15 level.");
        Set(intro, "inidisp", brightness == 0 ? ScreenFade.ForcedBlank : brightness);
        var fade = new ScreenFade();
        ushort seed = intro.Phase switch
        {
            IntroCinematicPhase.FadeInFirstNarration or IntroCinematicPhase.FadeOutFirstNarration or
                IntroCinematicPhase.FadeInPageOne => IntroCinematicRomData.Fade.NarrationSlowFade,
            IntroCinematicPhase.IntroFadeOut => IntroCinematicRomData.Fade.FinishFade,
            _ => 0,
        };
        fade.SetTiming(seed, seed);
        Set(intro, "fade", fade);
        if (Get(intro, "ceresFlight") is { } flight && Get(flight, "audio") is null)
            Set(flight, "audio", Get(intro, "audio") ?? throw new InvalidDataException(
                "Legacy intro with a Ceres flight has no cartridge audio state to attach."));
    }

    /// <summary>
    /// $8B:8682 locked input and stopped Samus together; $1A57 follows the egg's and dancing
    /// baby's deletions and, while the baby dances, the sign of its X velocity.
    /// </summary>
    private static void RestoreLegacyBabyDiscoveryDisplay(object scene)
    {
        Set(scene, "samusHandlersEnded",
            ((SamusState)scene.GetType().GetProperty("Samus")!.GetValue(scene)!).InputLocked);
        object egg = Get(scene, "egg")!;
        object baby = Get(scene, "confusedBaby")!;
        static bool Active(object actor) => (bool)actor.GetType().GetProperty("IsActive")!.GetValue(actor)!;
        ushort babyPreInstruction = (ushort)baby.GetType().GetProperty("PreInstructionPointer")!.GetValue(baby)!;
        ushort dancing = (ushort)typeof(IntroCinematicState).Assembly
            .GetType("SuperMetroid.Core.Frontend.CinematicCodePointers", throwOnError: true)!
            .GetField("PreInstruction_ConfusedBabyMetroid_Dancing")!.GetValue(null)!;
        short display;
        if (!Active(egg) || !Active(baby))
            display = IntroSamusDisplayHidden;
        else if (babyPreInstruction == dancing)
        {
            ushort velocity = (ushort)scene.GetType().GetProperty("BabyXVelocity",
                BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(scene)!;
            display = unchecked((sbyte)(velocity >> 8)) < 0 ? IntroSamusDisplayObjectsFirst : IntroSamusDisplaySamusFirst;
        }
        else
            display = IntroSamusDisplayObjectsFirst;
        SetIntroSamusDisplay(scene, "<SamusDisplay>k__BackingField", display);
    }

    /// <summary>
    /// Places the legacy spawn-ordered Rinkas in the slots the scripted allocation gives them:
    /// the spawner holds 14, the first wave takes 13 and 12, and the second wave arrives after
    /// Rinka zero's hit has freed 13, taking 13 and 11.
    /// </summary>
    private static void RestoreLegacyRinkaSlots(object system)
    {
        if (!DebuggerRetiredFieldDefinitions.TryGetLegacyValue(system, "rinkas", out object? stored) ||
            stored is not System.Collections.IList legacy)
            throw new InvalidDataException("Legacy intro Rinka list is missing.");
        object spawner = Get(system, "spawner")!;
        var slots = (Array)Activator.CreateInstance(FindField(system.GetType(), "slots").FieldType, 15)!;
        static bool Active(object actor) => (bool)actor.GetType().GetProperty("IsActive")!.GetValue(actor)!;
        if (Active(spawner))
            slots.SetValue(spawner, 14);
        int[] scriptedSlots = [13, 12, 13, 11];
        if (legacy.Count > scriptedSlots.Length)
            throw new InvalidDataException($"Legacy intro spawned {legacy.Count} Rinkas; the script spawns four.");
        if (legacy.Count > 2 && Active(legacy[0]!))
            throw new InvalidDataException("Legacy intro spawned the second Rinka wave while Rinka zero still held slot 13.");
        for (int index = 0; index < legacy.Count; index++)
            if (Active(legacy[index]!))
                slots.SetValue(legacy[index], scriptedSlots[index]);
        Set(system, "slots", slots);
    }

    private static void Set(object instance, string field, object? value) =>
        FindField(instance.GetType(), field).SetValue(instance, value);

    private static FieldInfo FindField(Type type, string field)
    {
        for (Type? current = type; current is not null; current = current.BaseType)
            if (current.GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) is { } found)
                return found;
        throw new InvalidDataException($"Legacy migration field {type.FullName}.{field} no longer exists.");
    }

    private static void RestoreLegacyFileSelectPresentationPage(FileSelectMenuState fileSelect)
    {
        FileSelectPhase displayedPhase = fileSelect.Phase == FileSelectPhase.FadeInFromDataManagement
            ? (FileSelectPhase)(Get(fileSelect, "phaseAfterFadeIn") ?? FileSelectPhase.Main)
            : fileSelect.Phase;
        string page = displayedPhase switch
        {
            FileSelectPhase.CopySelectSource => FileSelectPresentationDefinitions.CopySourcePage,
            FileSelectPhase.CopySelectDestination => FileSelectPresentationDefinitions.CopyDestinationPage,
            FileSelectPhase.CopyConfirm => FileSelectPresentationDefinitions.CopyConfirmPage,
            FileSelectPhase.CopyCompleted => FileSelectPresentationDefinitions.CopyCompletedPage,
            FileSelectPhase.ClearSelectSlot => FileSelectPresentationDefinitions.ClearSelectionPage,
            FileSelectPhase.ClearConfirm => FileSelectPresentationDefinitions.ClearConfirmPage,
            FileSelectPhase.ClearCompleted => FileSelectPresentationDefinitions.ClearCompletedPage,
            FileSelectPhase.FadeOutToMain => LegacyDataManagementPage(fileSelect),
            _ => LegacyMainPage(fileSelect),
        };
        Set(fileSelect, "currentPresentationPage", page);
    }

    private static string LegacyDataManagementPage(FileSelectMenuState fileSelect) =>
        string.Equals(Get(fileSelect, "pendingDataMode")?.ToString(), "Copy", StringComparison.Ordinal)
            ? FileSelectPresentationDefinitions.CopySourcePage
            : FileSelectPresentationDefinitions.ClearSelectionPage;

    private static string LegacyMainPage(FileSelectMenuState fileSelect)
    {
        var slots = (Array)(Get(fileSelect, "saveSlots")
            ?? throw new InvalidDataException("Legacy file-select state has no save-slot array."));
        return slots.Cast<object?>().Any(slot => slot is not null)
            ? FileSelectPresentationDefinitions.MainWithDataPage
            : FileSelectPresentationDefinitions.MainEmptyPage;
    }
}
