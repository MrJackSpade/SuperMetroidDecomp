using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

// Legacy namespace is persisted in debugger identities; ownership is now platform-neutral.
namespace SuperMetroid.Desktop;

/// <summary>Explicit, loss-aware compatibility rules for older debugger object layouts.</summary>
internal static class DebuggerStateFieldMigrations
{
    /// <summary>
    /// #342 adds the previously unmodeled WRAM $0E00 latch. Older builds never captured
    /// it, so their state cannot supply an exact value. Neutral zero avoids inventing a
    /// Fire press; the next normal draw/script update populates the native latch.
    /// </summary>
    private const string PreviousDrawNewInputField = "<PreviousDrawNewInput>k__BackingField";

    internal static FieldInfo[] SelectSerializedFields(Type type, FieldInfo[] current, int count)
    {
        if (count == current.Length) return current;
        if (type == typeof(SamusProjectileFrameResult) && count <= 7 &&
            current.Any(field => field.Name == "<PersistentMemoryCorrupted>k__BackingField"))
        {
            Console.Error.WriteLine(
                "WARNING: Legacy projectile result predates native progression-memory corruption; restoring no pending corruption publication.");
            return SelectSerializedFields(type, current.Where(field =>
                field.Name != "<PersistentMemoryCorrupted>k__BackingField").ToArray(), count);
        }
        if (type == typeof(Bank80SystemState) && count == current.Length - 1 &&
            current.Any(field => field.Name == "<SavedLoadingGameState>k__BackingField"))
        {
            Console.Error.WriteLine(
                "WARNING: Legacy bank-$80 state lacks the saved startup dispatcher; restoring ordinary main-game loading.");
            return current.Where(field =>
                field.Name != "<SavedLoadingGameState>k__BackingField").ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Frontend.SuperMetroidGame) &&
            count <= 48 &&
            current.Any(field => field.Name == "spacetimeIntroRestartSlot"))
        {
            Console.Error.WriteLine(
                "WARNING: Legacy frontend state predates SpaceTime intro restart ownership; restoring no pending restart.");
            return SelectSerializedFields(type, current.Where(field =>
                field.Name != "spacetimeIntroRestartSlot").ToArray(), count);
        }
        if (type == typeof(SamusXrayState) && count == current.Length - 3 &&
            current.Any(field => field.Name == "<PendingActivationPose>k__BackingField") &&
            current.Any(field => field.Name == "<OwnsSamusControl>k__BackingField") &&
            current.Any(field => field.Name == "<SuspendedSubsystems>k__BackingField"))
        {
            Console.Error.WriteLine("WARNING: Legacy X-Ray state lacks pending activation, separate Samus-control ownership, and subsystem-disable ownership; reconstructing native ownership from its active/freeze words.");
            return current.Where(field => field.Name is not
                "<PendingActivationPose>k__BackingField" and not
                "<OwnsSamusControl>k__BackingField" and not
                "<SuspendedSubsystems>k__BackingField").ToArray();
        }
        if (type == typeof(SamusShinesparkState) && count == current.Length - 3)
        {
            Console.Error.WriteLine("WARNING: Legacy shinespark requests lack producer-time suppression; retaining historical unsuppressed admission.");
            return current.Where(field => field.Name is not "<StoredShineWarningSoundSuppressed>k__BackingField"
                and not "<LaunchSoundSuppressed>k__BackingField" and not "<CrashSoundSuppressed>k__BackingField").ToArray();
        }
        if (type == typeof(SamusXrayState) && count == current.Length - 2 &&
            current.Any(field => field.Name == "<OwnsSamusControl>k__BackingField") &&
            current.Any(field => field.Name == "<SuspendedSubsystems>k__BackingField"))
        {
            Console.Error.WriteLine("WARNING: Legacy X-Ray state lacks separate Samus-control and subsystem-disable ownership; reconstructing both from its active/freeze words.");
            return current.Where(field => field.Name is not
                "<OwnsSamusControl>k__BackingField" and not
                "<SuspendedSubsystems>k__BackingField").ToArray();
        }
        if (type == typeof(SamusXrayState) && count == current.Length - 1 &&
            current.Any(field => field.Name == "<SuspendedSubsystems>k__BackingField"))
        {
            Console.Error.WriteLine("WARNING: Legacy X-Ray state lacks independent subsystem-disable ownership; reconstructing it from the saved active/freeze words.");
            return current.Where(field =>
                field.Name != "<SuspendedSubsystems>k__BackingField").ToArray();
        }
        // These values describe a pending host publication, not a new cartridge word.
        // Historical captures cannot recover producer-time suppression. Preserve their
        // previously unsuppressed admission and let the next producer replace it.
        string? suppressionField = type == typeof(SamusProjectileFrameResult) ? "<QueuedSoundSuppressed>k__BackingField" :
            type == typeof(SamusSoundRequest) ? "<SoundSuppressed>k__BackingField" :
            type == typeof(EnemySoundRequest) ? "<SoundSuppressed>k__BackingField" :
            type == typeof(RoomFxSoundRequest) ? "<SoundSuppressed>k__BackingField" :
            type == typeof(PaletteFxSoundRequest) ? "<SoundSuppressed>k__BackingField" :
            type == typeof(SuperMetroid.Core.Rooms.PlmSoundRequest) ? "<SoundSuppressed>k__BackingField" :
            type == typeof(HudState) ? "<SelectionSoundSuppressedThisFrame>k__BackingField" :
            type == typeof(SamusBombProjectileSystem) ? "<SoundSuppressedBeforeProjectileHandling>k__BackingField" : null;
        if (suppressionField is not null && current.Any(field => field.Name == suppressionField) &&
            (count == current.Length - 1 || type == typeof(SamusProjectileFrameResult) && count == 5 && current.Length == 7))
        {
            Console.Error.WriteLine($"WARNING: Legacy {type.Name} lacks producer-time sound suppression; retaining its previous unsuppressed publication behavior.");
            return SelectSerializedFields(type, current.Where(field => field.Name != suppressionField).ToArray(), count);
        }
        if (type == typeof(SuperMetroid.Core.Audio.ManagedPcmSampleBank) && count == 3 && current.Length == 4)
        {
            Console.Error.WriteLine("WARNING: Legacy PCM bank lacks cross-source loop routing; retaining its original self-loop behavior until the next bank upload.");
            return current.Where(field => field.Name != "loopEntrySources").ToArray();
        }
        if (type.FullName == "SuperMetroid.Core.Audio.ManagedSnesDsp+Voice" && count == 22 && current.Length == 23)
        {
            Console.Error.WriteLine("WARNING: Legacy DSP voice lacks silent-release BRR fallback; retaining its saved PCM cursor until a native loop handoff.");
            return current.Where(field => field.Name != "ReleasedBrrCursor").ToArray();
        }
        if (type == typeof(SamusSuitPickupState) && count is 9 or 10 && current.Length == 11)
        {
            Console.Error.WriteLine("WARNING: Legacy suit pickup lacks producer-time sound suppression; retaining historical admission and any saved entry latch.");
            return current.Where(field => field.Name != "<TransformationSoundSuppressed>k__BackingField" &&
                (count != 9 || field.Name != "_transformationSoundPending")).ToArray();
        }
        if (type == typeof(SamusProjectileFrameResult) && count == 5 && current.Length == 6)
        {
            return current.Where(field => field.Name != "<AdditionalSoundRequests>k__BackingField").ToArray();
        }
        if (type == typeof(SamusProjectileSlot) && count == 19 && current.Length == 20)
        {
            Console.Error.WriteLine("WARNING: Legacy projectile slot lacks combo auxiliary phase; restoring its initial phase.");
            return current.Where(field => field.Name != "<AuxiliaryPhase>k__BackingField").ToArray();
        }
        if (type == typeof(SamusProjectileSystem) && count == 16 && current.Length == 17)
        {
            Console.Error.WriteLine("WARNING: Legacy projectile owner predates charge-combo state; restoring no combo pending.");
            return current.Where(field => field.Name != "<ComboState>k__BackingField").ToArray();
        }
        if (type == typeof(SamusKinematicsState) && count == 22 && current.Length == 23)
        {
            Console.Error.WriteLine("WARNING: Legacy kinematics lacks prospective-pose contact mode; retaining live owner lookup.");
            return current.Where(field => field.Name != "<ProbeContactDamageIndex>k__BackingField").ToArray();
        }
        if (type == typeof(SamusState) && count == current.Length - 12 &&
            current.Any(field => field.Name == "<BombJumpPoseInputLocked>k__BackingField") &&
            current.Any(field => field.Name == PreviousDrawNewInputField))
        {
            // The preserved pre-b944f1b5 player fixture predates the entire input-history
            // family as well as the later pose/camera, warning, pose-history, and script
            // ownership additions. The exact field identities below were read from that
            // fixture; GraphReader still rejects any different 66-field set.
            Console.Error.WriteLine(
                "WARNING: Early Samus layout predates input history and later pose ownership; " +
                "restoring all unavailable transient state neutral.");
            return current.Where(field => field.Name is not
                "_poseCollisionPreviousYPosition" and not
                "_poseAlignmentPreviousYDelta" and not
                "<BombJumpPoseInputLocked>k__BackingField" and not
                "_healthWarning" and not
                PreviousDrawNewInputField and not
                "<AutoJumpTimer>k__BackingField" and not
                "<PreviousDrawHeldInput>k__BackingField" and not
                "<AutoJumpInputPending>k__BackingField" and not
                "<ShinesparkPoseInputLocked>k__BackingField" and not
                "<CrystalFlashPoseInputLocked>k__BackingField" and not
                "_poseHistory" and not
                "<StationaryScriptControlLocked>k__BackingField").ToArray();
        }
        if (type == typeof(SamusState) && count == current.Length - 5 &&
            current.Any(field => field.Name == "<ShinesparkPoseInputLocked>k__BackingField") &&
            current.Any(field => field.Name == "<CrystalFlashPoseInputLocked>k__BackingField"))
        {
            // The 0.2.0/#524 graph predates the two special-movement pose locks, the
            // stationary script lock, and the paired pose/camera correction history. Its
            // actual field identities are validated by GraphReader after this selection;
            // the count alone is never allowed to substitute a different five-field era.
            Console.Error.WriteLine("WARNING: 0.2.0 Samus state lacks special-movement pose locks, stationary script ownership, and pose/camera correction history; restoring all inactive.");
            return current.Where(field => field.Name is not
                "<ShinesparkPoseInputLocked>k__BackingField" and not
                "<CrystalFlashPoseInputLocked>k__BackingField" and not
                "<StationaryScriptControlLocked>k__BackingField" and not
                "_poseCollisionPreviousYPosition" and not
                "_poseAlignmentPreviousYDelta").ToArray();
        }
        if (type == typeof(SamusState) && count == current.Length - 9 &&
            current.Any(field => field.Name == "<BombJumpPoseInputLocked>k__BackingField"))
        {
            // The b944f1b5 capture already has PreviousDrawNewInput, but predates
            // these nine additions. Do not misclassify its independent bomb lock
            // as the older missing draw-input latch in the generic auto-jump path.
            Console.Error.WriteLine("WARNING: Pre-bomb-lock Samus layout restores nine later state additions inactive; retaining its saved previous-draw input.");
            return current.Where(field => field.Name is not "<StationaryScriptControlLocked>k__BackingField"
                and not "_poseCollisionPreviousYPosition" and not "_poseAlignmentPreviousYDelta"
                and not "_healthWarning" and not "_poseHistory"
                and not "<AutoJumpTimer>k__BackingField" and not "<PreviousDrawHeldInput>k__BackingField"
                and not "<AutoJumpInputPending>k__BackingField" and not "<BombJumpPoseInputLocked>k__BackingField").ToArray();
        }
        if (type.FullName == "SuperMetroid.Core.Rooms.RoomPlmSystem+PlmSlot" && count == 20 && current.Length == 22)
        {
            // 571b9a42 implemented Samus Eater capture and added both held-point
            // words. Earlier builds did not execute that capture; fresh plant
            // setup supplies the coordinates if the player is subsequently caught.
            Console.Error.WriteLine("WARNING: Legacy PLM slot lacks plant-held coordinates; restoring zero until a new capture setup.");
            return current.Where(field => field.Name is not "<PlantHeldX>k__BackingField" and not "<PlantHeldY>k__BackingField").ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Frontend.SuperMetroidGame) &&
            count == 47 && current.Length == 48 && current.Any(field => field.Name == "menuRandom"))
        {
            Console.Error.WriteLine("WARNING: Legacy frontend lacks pre-game RNG history; preserving any loaded runtime RNG, otherwise starting the menu generator at the reset seed.");
            return current.Where(field => field.Name != "menuRandom").ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Frontend.SuperMetroidGame) &&
            count == 46 && current.Length == 48 && current.Any(field => field.Name == "pauseFadeCounter"))
        {
            // 3a891459 added the native alternating pause-fade counter. Before it,
            // every fade call changed brightness; zero keeps the first restored
            // call eligible and subsequent calls establish the native cadence.
            // The graph reader still checks every remaining saved name and order.
            Console.Error.WriteLine("WARNING: Legacy frontend lacks pause-fade cadence and pre-game RNG history; restoring the next fade step as immediately eligible and preserving any loaded runtime RNG.");
            return current.Where(field => field.Name is not "pauseFadeCounter" and not "menuRandom").ToArray();
        }
        if (type == typeof(SamusState) && current.Any(field => field.Name == "<StationaryScriptControlLocked>k__BackingField"))
        {
            Console.Error.WriteLine("WARNING: Older Samus state lacks stationary script-handler ownership; retaining its saved input lock, with animation ownership unknown until the next script command.");
            return SelectSerializedFields(type, current.Where(field => field.Name != "<StationaryScriptControlLocked>k__BackingField").ToArray(), count);
        }
        if (type.FullName == "SuperMetroid.Core.Frontend.CeresDestructionCinematicState" && count == current.Length - 1)
        {
            Console.Error.WriteLine("WARNING: Legacy Ceres cinematic state lacks engine palette-FX timing; its glow restarts on the next approach frame.");
            return current.Where(field => field.Name != "paletteFx").ToArray();
        }
        if (type == typeof(RoomLayer3FxState) && count == current.Length - 1 &&
            current.Any(field => field.Name == "lavaAcidBg3PreInstructionInstalled"))
        {
            // The BG3 HDMA pre-instruction latch was added after the 0.2.0 capture used by
            // issue #524. False is the exact cold-start state: the next active lava/acid
            // effect frame installs the pre-instruction before it can execute.
            Console.Error.WriteLine("WARNING: Legacy room FX lacks the lava/acid BG3 pre-instruction latch; restoring its cold-start state.");
            return current.Where(field => field.Name != "lavaAcidBg3PreInstructionInstalled").ToArray();
        }
        if (type.FullName == "SuperMetroid.Core.Game.RoomFxAnimatedTilesState" &&
            count == current.Length - 1 &&
            current.Any(field => field.Name == "compiledMechanics"))
        {
            // ad3d533e replaced live ROM instruction reads with compiled animation
            // definitions. Historical states still contain the native object pointer,
            // which is the lossless key needed to rebind the compiled definition.
            Console.Error.WriteLine(
                "WARNING: Legacy room-FX animated tiles lack their compiled mechanics binding; " +
                "reconstructing it from the captured native object pointer.");
            return current.Where(field => field.Name != "compiledMechanics").ToArray();
        }
        if (type == typeof(WreckedShipTreadmillAnimatedTilesState) &&
            count == current.Length - 1 &&
            current.Any(field => field.Name == "_compiledMechanics"))
        {
            // 18f19edc compiled the two treadmill control streams. Their saved native
            // object pointer uniquely identifies the immutable replacement definition.
            Console.Error.WriteLine(
                "WARNING: Legacy Wrecked Ship treadmill lacks its compiled mechanics binding; " +
                "reconstructing it from the captured native object pointer.");
            return current.Where(field => field.Name != "_compiledMechanics").ToArray();
        }
        if (type == typeof(SamusState) && count <= current.Length - 2 &&
            current.Any(field => field.Name == "_poseCollisionPreviousYPosition") &&
            current.Any(field => field.Name == "_poseAlignmentPreviousYDelta"))
        {
            // 0.2.1 captured neither per-frame pose/camera correction accumulator.
            // Null/zero preserve the saved camera checkpoint until normal movement
            // supplies new corrections. Validate every surviving field name below.
            Console.Error.WriteLine("WARNING: Older Samus state lacks pose/camera correction accumulators; restoring no pending correction.");
            return SelectSerializedFields(type, current.Where(field => field.Name is not
                "_poseCollisionPreviousYPosition" and not "_poseAlignmentPreviousYDelta").ToArray(), count);
        }
        if (type == typeof(SuperMetroid.Core.Hardware.VramWriteEntry) && count == 3 && current.Length == 4)
        {
            // Old records only had bus sources. Default None retains their exact
            // pending address/count/destination; no image payload or timing is invented.
            return current.Where(field => field.Name != "<AssetId>k__BackingField").ToArray();
        }
        if (type == typeof(SamusState) && current.Any(field => field.Name == "_healthWarning"))
        {
            Console.Error.WriteLine("WARNING: Older Samus state lacks the low-health warning latch; it starts inactive until the next admitted native health check.");
            return SelectSerializedFields(type, current.Where(field => field.Name != "_healthWarning").ToArray(), count);
        }
        if (type.FullName == "SuperMetroid.Core.Frontend.PauseMenuState" &&
            current.Any(field => field.Name == "pauseNmiFrameCounter8") &&
            (count == current.Length - 1 || count == current.Length - 2))
        {
            Console.Error.WriteLine("WARNING: Legacy pause state lacks reserve fill-flicker NMI phase; restores phase zero until the next accepted frame.");
            return SelectSerializedFields(type, current.Where(field => field.Name != "pauseNmiFrameCounter8").ToArray(), count);
        }
        if (type.FullName == "SuperMetroid.Core.Frontend.PauseMenuState" && count == current.Length - 1 &&
            current.Any(field => field.Name == "reserveTransferSoundDelay"))
        {
            Console.Error.WriteLine("WARNING: Legacy pause state predates manual reserve transfer; restores with no transfer pending.");
            return current.Where(field => field.Name != "reserveTransferSoundDelay").ToArray();
        }
        if (type == typeof(SamusGrappleState) && count == current.Length - 1)
        {
            Console.Error.WriteLine("WARNING: Legacy grapple state has no pose-change auto-fire timer; unavailable firing age restores expired until the next shot.");
            return current.Where(field => field.Name != "<PoseChangeAutoFireTimer>k__BackingField").ToArray();
        }
        if (type == typeof(GrappleMovementResult) && count == current.Length - 2 &&
            current.Any(field => field.Name == "<PendingDropPose>k__BackingField") &&
            current.Any(field => field.Name == "<PendingConnection>k__BackingField"))
        {
            // c655c9b2 split prospective pose publication from immediate grapple state.
            // Older results could not own either pending handoff, so null is exact.
            Console.Error.WriteLine(
                "WARNING: Legacy grapple result predates deferred drop and connection poses; " +
                "restoring no pending pose handoff.");
            return current.Where(field => field.Name is not
                "<PendingDropPose>k__BackingField" and not
                "<PendingConnection>k__BackingField").ToArray();
        }
        if (type == typeof(GameplayMessageBoxState) && count == current.Length - 2 &&
            current.Any(field => field.Name == "_shootBinding") &&
            current.Any(field => field.Name == "_runBinding"))
        {
            // b3e5d562 made configured controller bindings part of message-box state.
            // Earlier builds always used the stock X/B bindings, so reconstruct those
            // exact defaults rather than leaving uninitialized zero button masks.
            Console.Error.WriteLine(
                "WARNING: Legacy gameplay message box predates configurable bindings; " +
                "restoring the stock Shoot=X and Run=B bindings.");
            return current.Where(field => field.Name is not "_shootBinding" and not "_runBinding").ToArray();
        }
        if (type == typeof(SamusDraygonGrabbedState) && count == current.Length - 1 &&
            current.Any(field => field.Name == "<MovementHandlerReplaced>k__BackingField"))
        {
            Console.Error.WriteLine("WARNING: Legacy Draygon-grab state lacks replacement-handler ownership; retaining its captured active state as the movement owner.");
            return current.Where(field => field.Name != "<MovementHandlerReplaced>k__BackingField").ToArray();
        }
        if (type.FullName == "SuperMetroid.Core.Frontend.IntroCinematicObjectSystem" && count == current.Length - 1)
        {
            Console.Error.WriteLine("WARNING: Legacy intro state has no text-glow history; existing glyph ages cannot be recovered. Newly drawn glyphs start native glow normally.");
            return current.Where(field => field.Name != "textGlow").ToArray();
        }
        if (type == typeof(PhantoonBlendingState) && count == current.Length - 1)
        {
            Console.Error.WriteLine("WARNING: Legacy Phantoon display state lacks MOSAIC history; restores ungrouped until the next accepted NMI.");
            return current.Where(field => field.Name != "<DisplayedMosaic>k__BackingField").ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Rendering.OrdinaryGameplayRegisters) &&
            current.Any(field => field.Name == "<Bg2Mosaic>k__BackingField") && count is 11 or 13 or 15)
        {
            Console.Error.WriteLine("WARNING: Legacy gameplay display registers lack BG2 mosaic; restoring original ungrouped sampling.");
            return SelectSerializedFields(type, current.Where(field => field.Name != "<Bg2Mosaic>k__BackingField").ToArray(), count);
        }
        if (type == typeof(PhantoonEnemyState) &&
            (count == current.Length - 1 || count == current.Length - 2))
        {
            Console.Error.WriteLine("WARNING: Legacy Phantoon state lacks blend HDMA history; setup restarts. Pre-wave states also restore missing wave history inactive until its next native spawn.");
            return current.Where(field => field.Name != "_blending" &&
                (count != current.Length - 2 || field.Name != "_wave")).ToArray();
        }
        if (type == typeof(SamusState) && current.Any(field => field.Name == "_poseHistory") &&
            (count == current.Length - 1 || count == current.Length - 2 ||
             count == current.Length - 4 || count == current.Length - 5))
        {
            Console.Error.WriteLine("WARNING: Older Samus state has no transition pose history; the unavailable history restores as zero until subsequent transitions populate it.");
            // First remove this addition, then apply the explicitly supported older
            // auto-jump / draw-input layouts. Their remaining field identities and
            // ordering are still checked by the graph reader, not inferred silently.
            return SelectSerializedFields(type,
                current.Where(field => field.Name != "_poseHistory").ToArray(), count);
        }
        if (type == typeof(SamusState) && current.Any(field => field.Name == "_poseHistory"))
            throw new InvalidDataException($"Unsupported legacy Samus pose-history layout with {count} fields.");
        if (type == typeof(SamusState) &&
            (count == current.Length - 3 || count == current.Length - 4))
        {
            Console.Error.WriteLine("WARNING: Older Samus state lacks auto-jump history; restoring neutral history and ordinary input handling.");
            bool lacksNewInput = count == current.Length - 4;
            return current.Where(field => field.Name is not "<AutoJumpTimer>k__BackingField"
                and not "<PreviousDrawHeldInput>k__BackingField"
                and not "<AutoJumpInputPending>k__BackingField" &&
                (!lacksNewInput || field.Name != PreviousDrawNewInputField)).ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Rendering.BgSubscreenAddRenderLayer) && count == 6 && current.Length == 7)
        {
            Console.Error.WriteLine("WARNING: Legacy subscreen layer has no vertical-scroll field; restoring its original unscrolled sampling.");
            return current.Where(field => field.Name != "<VerticalScroll>k__BackingField").ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Rendering.Mode7RenderLayer) && count == 2 && current.Length == 3)
        {
            Console.Error.WriteLine("WARNING: Legacy Mode 7 layer has no BG1 subscreen addition; retaining its original composition.");
            return current.Where(field => field.Name != "<AddBg1Subscreen>k__BackingField").ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Rendering.Mode7RenderRegisters) && count == 9 && current.Length == 10)
        {
            Console.Error.WriteLine("WARNING: Legacy Mode 7 snapshot has no wrap control; retaining its original overflow behavior.");
            return current.Where(field => field.Name != "<WrapOutsideMap>k__BackingField").ToArray();
        }
        if (type == typeof(ScrollBoundaryCamera) && count == 11 && current.Length == 12)
        {
            Console.Error.WriteLine("WARNING: Legacy camera has no previous-scroll Samus checkpoint; initializing on its first scrolling pass.");
            return current.Where(field => field.Name != "<PreviousSamusPoint>k__BackingField").ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime) &&
            count is 105 or 106 && current.Length == 110)
        {
            // Additions verified against b944f1b5: statue owner (5ff0476a),
            // timeout option (fc59514a), escape quake (a74aa6d1), treadmill owner (e361a6b1).
            // The 105-field player fixtures additionally predate Ceres haze ownership
            // (4f3e4bec); the 106-field layout already contains it.
            // ReadFields still validates every surviving declaring type/name/order.
            Console.Error.WriteLine(
                "WARNING: Legacy runtime lacks statue, escape-quake, timeout and treadmill state; " +
                "added features restore inactive." +
                (count == 105 ? " It also predates Ceres haze ownership." : ""));
            return current.Where(field => field.Name is not "_tourianStatues"
                and not "_escapeDiagonalFrames"
                and not "<PreventEscapeTimeout>k__BackingField"
                and not "<RoomTreadmills>k__BackingField" &&
                (count == 106 || field.Name != "<CeresHaze>k__BackingField")).ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Frontend.SuperMetroidGameOptions) && count == 9 && current.Length == 11)
        {
            // Both fields were added after the original nine-option host layout.
            // Omitted bool/nullable values restore as false/null: no countdown clamp
            // and no ending-time override, matching the capabilities of that build.
            Console.Error.WriteLine("WARNING: Older debugger options predate escape-timeout and ending-time overrides; leaving both disabled.");
            return current.Where(field => field.Name is not "<PreventEscapeTimeout>k__BackingField"
                and not "<EndingTimeOverrideMinutes>k__BackingField").ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime) && count == current.Length - 1)
        {
            Console.Error.WriteLine("WARNING: Older debugger state predates the statue sequence; it initializes on room entry.");
            return current.Where(field => field.Name != "_tourianStatues").ToArray();
        }
        if (type == typeof(FileSelectMenuState) && count == current.Length - 1 &&
            current.Any(field => field.Name == "currentPresentationPage"))
        {
            Console.Error.WriteLine(
                "WARNING: Older file-select state lacks its installed-presentation page; " +
                "reconstructing it from the captured menu phase.");
            return current.Where(field => field.Name != "currentPresentationPage").ToArray();
        }
        if (type == typeof(SuperMetroidSaveSlot) && count == current.Length - 1 &&
            current.Any(field => field.Name == "<LoadingGameState>k__BackingField"))
        {
            Console.Error.WriteLine(
                "WARNING: Older decoded save slot lacks its loading-game dispatcher; " +
                "restoring ordinary main-game loading.");
            return current.Where(field =>
                field.Name != "<LoadingGameState>k__BackingField").ToArray();
        }
        if (type == typeof(RoomEnemySystem) &&
            (count == current.Length - 1 || count == current.Length - 3) &&
            current.Any(field => field.Name == "_samusProjectilesForEnemyFrame"))
        {
            // 833cc4ee added this dependency for Pseudo Screw contact. EnemyMain
            // supplies the live owner before running any touch callback; null is
            // sufficient between frames. Compose with the older statue layout.
            Console.Error.WriteLine("WARNING: Legacy enemy state lacks its frame projectile context; the next enemy phase supplies the live owner.");
            return SelectSerializedFields(type, current.Where(field => field.Name != "_samusProjectilesForEnemyFrame").ToArray(), count);
        }
        if (type == typeof(RoomEnemySystem) && count == current.Length - 2)
        {
            Console.Error.WriteLine("WARNING: Older debugger state has no statue displacement/water surface; initializing to zero.");
            return current.Where(field => field.Name is not "<TourianEntranceStatueVerticalOffset>k__BackingField"
                and not "<TourianStatueWaterY>k__BackingField").ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Rendering.OrdinaryGameplayRegisters) &&
            count is 11 or 13 && current.Length == 15)
        {
            // Hardware windows added two fields after the scanline-window schema.
            // Keep both historical layouts explicit; never infer missing fields from
            // a count alone or reorder the older fields to fit the newer record.
            Console.Error.WriteLine("WARNING: Older gameplay capture has no hardware window registers; restoring disabled windows.");
            if (count == 11)
                Console.Error.WriteLine("WARNING: Older gameplay capture also has no BG2 scanline window; the next accepted NMI reconstructs it.");
            return current.Where(field => field.Name is not "<Windows>k__BackingField"
                and not "<MainScreenWindowMask>k__BackingField" &&
                (count == 13 || field.Name is not "<Bg2FirstScanline>k__BackingField"
                    and not "<Bg2EndScanline>k__BackingField")).ToArray();
        }
        if (type == typeof(SuperMetroid.Core.Runtime.GameplayPpuRenderSnapshot) && count == 7 && current.Length == 9)
        {
            Console.Error.WriteLine("WARNING: Older debugger state has no BG2 scanline window; the next accepted NMI reconstructs it.");
            return current.Where(field => field.Name is not "<Bg2FirstScanline>k__BackingField" and not "<Bg2EndScanline>k__BackingField").ToArray();
        }
        if (type == typeof(SamusState) && count == current.Length - 1 &&
            current.Any(field => field.Name == PreviousDrawNewInputField))
        {
            Console.Error.WriteLine("WARNING: Older debugger state has no Samus previous-draw input latch; initializing it to neutral input.");
            return current.Where(field => field.Name != PreviousDrawNewInputField).ToArray();
        }
        throw new InvalidDataException($"Serialized {type.FullName} contains {count} fields; this build expects {current.Length}.");
    }

    /// <summary>Initializes fields omitted by explicitly recognized legacy layouts.</summary>
    internal static void InitializeMissingFields(object instance, int serializedCount)
    {
        if (instance is SamusXrayState xray && serializedCount <=
            GetCurrentInstanceFieldCount(typeof(SamusXrayState)) - 2)
        {
            typeof(SamusXrayState)
                .GetField("<OwnsSamusControl>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(xray, xray.IsActive && xray.TimeIsFrozen);
        }
        if (instance is SamusXrayState legacyXray && serializedCount <
            GetCurrentInstanceFieldCount(typeof(SamusXrayState)))
        {
            typeof(SamusXrayState)
                .GetField("<SuspendedSubsystems>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(legacyXray, legacyXray.IsActive
                    ? XraySuspendedSubsystems.All
                    : XraySuspendedSubsystems.None);
        }
        if (instance is SuperMetroid.Core.Audio.ManagedPcmSampleBank && serializedCount == 3)
        {
            typeof(SuperMetroid.Core.Audio.ManagedPcmSampleBank)
                .GetField("loopEntrySources", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(instance, new Dictionary<byte, byte>());
        }
        if (instance is SuperMetroid.Core.Runtime.SuperMetroidRuntime runtime && serializedCount is 105 or 106)
        {
            // Constructors are bypassed by graph restoration. No animation existed
            // in this layout; normal room loading will select the next room's objects.
            typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime)
                .GetField("<RoomTreadmills>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(runtime, new RoomTreadmillAnimatedTilesState());
            if (serializedCount == 105)
            {
                typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime)
                    .GetField("<CeresHaze>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(runtime, new CeresHazeState());
            }
        }
        if (instance is Bank80SystemState system && serializedCount ==
            GetCurrentInstanceFieldCount(typeof(Bank80SystemState)) - 1)
        {
            system.LoadSavedLoadingGameState(SaveLoadingGameStates.MainGame);
        }
        if (instance is FileSelectMenuState fileSelect && serializedCount ==
            GetCurrentInstanceFieldCount(typeof(FileSelectMenuState)) - 1)
        {
            RestoreLegacyFileSelectPresentationPage(fileSelect);
        }
        if (instance is SuperMetroidSaveSlot saveSlot && serializedCount ==
            GetCurrentInstanceFieldCount(typeof(SuperMetroidSaveSlot)) - 1)
        {
            typeof(SuperMetroidSaveSlot)
                .GetField("<LoadingGameState>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(saveSlot, SaveLoadingGameStates.MainGame);
        }
        if (instance.GetType().FullName == "SuperMetroid.Core.Game.RoomFxAnimatedTilesState" &&
            serializedCount == GetCurrentInstanceFieldCount(instance.GetType()) - 1)
        {
            Type type = instance.GetType();
            ushort objectPointer = (ushort)(type.GetField(
                "objectPointer",
                BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance) ?? (ushort)0);
            RoomFxAnimatedTileMechanicsDefinitions.TryResolve(
                objectPointer,
                out RoomFxAnimatedTileObjectDefinition? compiledMechanics);
            type.GetField("compiledMechanics", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(instance, compiledMechanics);
        }
        if (instance is WreckedShipTreadmillAnimatedTilesState treadmill && serializedCount ==
            GetCurrentInstanceFieldCount(typeof(WreckedShipTreadmillAnimatedTilesState)) - 1)
        {
            Type type = typeof(WreckedShipTreadmillAnimatedTilesState);
            ushort objectPointer = (ushort)(type.GetField(
                "_objectPointer",
                BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(treadmill) ?? (ushort)0);
            WreckedShipTreadmillMechanicsDefinitions.TryResolve(
                objectPointer,
                out WreckedShipTreadmillObjectDefinition? compiledMechanics);
            type.GetField("_compiledMechanics", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(treadmill, compiledMechanics);
        }
        if (instance is GameplayMessageBoxState messageBox && serializedCount ==
            GetCurrentInstanceFieldCount(typeof(GameplayMessageBoxState)) - 2)
        {
            Type type = typeof(GameplayMessageBoxState);
            type.GetField("_shootBinding", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(messageBox, (ushort)SnesButton.X);
            type.GetField("_runBinding", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(messageBox, (ushort)SnesButton.B);
        }
    }

    private static void RestoreLegacyFileSelectPresentationPage(FileSelectMenuState fileSelect)
    {
        Type type = typeof(FileSelectMenuState);
        FileSelectPhase displayedPhase = fileSelect.Phase == FileSelectPhase.FadeInFromDataManagement
            ? (FileSelectPhase)(type.GetField(
                "phaseAfterFadeIn",
                BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(fileSelect)
                ?? FileSelectPhase.Main)
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
        type.GetField("currentPresentationPage", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(fileSelect, page);
    }

    private static string LegacyDataManagementPage(FileSelectMenuState fileSelect)
    {
        object? mode = typeof(FileSelectMenuState).GetField(
            "pendingDataMode",
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(fileSelect);
        return string.Equals(mode?.ToString(), "Copy", StringComparison.Ordinal)
            ? FileSelectPresentationDefinitions.CopySourcePage
            : FileSelectPresentationDefinitions.ClearSelectionPage;
    }

    private static string LegacyMainPage(FileSelectMenuState fileSelect)
    {
        var slots = (Array)(typeof(FileSelectMenuState).GetField(
            "saveSlots",
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(fileSelect)
            ?? throw new InvalidDataException("Legacy file-select state has no save-slot array."));
        return slots.Cast<object?>().Any(slot => slot is not null)
            ? FileSelectPresentationDefinitions.MainWithDataPage
            : FileSelectPresentationDefinitions.MainEmptyPage;
    }

    /// <summary>
    /// Counts the same declared instance fields as the graph serializer for migration
    /// comparisons without exposing the serializer's ordering implementation.
    /// </summary>
    private static int GetCurrentInstanceFieldCount(Type type)
    {
        int count = 0;
        for (Type? current = type; current is not null; current = current.BaseType)
        {
            count += current.GetFields(BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Count(field => !field.IsStatic &&
                    !field.IsDefined(typeof(NonSerializedAttribute), false));
        }
        return count;
    }
}
