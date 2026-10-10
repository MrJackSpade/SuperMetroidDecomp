using System.Runtime.CompilerServices;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

// Legacy namespace is persisted in debugger identities; ownership is now platform-neutral.
namespace SuperMetroid.Desktop;

/// <summary>Migrates one drained retired field of a restored instance.</summary>
internal delegate void RetiredFieldMigration(object instance, string serializedName, object? value);

/// <summary>
/// Exact historical fields that no longer exist in the current layout. An older debugger state
/// still names them; restoration reads each value and hands it to its migration. Most were removed
/// because nothing read them and are discarded; others carry state that moved and seed it once the
/// graph is complete. Any other unknown field remains a layout mismatch and is rejected.
/// </summary>
internal static class DebuggerRetiredFieldDefinitions
{
    // Legacy values drained from a restored instance, for migrations that run after the
    // owning graph is complete. Weak keys keep this from retaining restored state.
    private static readonly ConditionalWeakTable<object, Dictionary<string, object?>> LegacyValues = new();

    private const string CartridgePaletteTransition = "SuperMetroid.Core.Frontend.CartridgePaletteTransition";
    private const string StationPlmState = "SuperMetroid.Core.Rooms.RoomPlmSystem+StationPlmState";

    /// <summary>(Declaring type full name, field name) of every retired field, with its migration.</summary>
    private static readonly Dictionary<(string Type, string Field), RetiredFieldMigration> Retired = new()
    {
        // The Ceres getaway now runs at room main, after Samus, so `$90:E119` installs
        // its handler directly. A capture taken between the old deferred request and its
        // promotion has no current equivalent: promotion also displaced Samus's movement
        // owners, which a field migration cannot reproduce.
        [(typeof(SamusCeresRidleyEjectionState).FullName!, "<IsPending>k__BackingField")] = (_, _, value) =>
        {
            if (value is not bool pending)
                throw new InvalidDataException("Legacy Ceres Ridley ejection pending flag is not a Boolean.");
            if (pending)
                throw new InvalidDataException(
                    "Legacy snapshot was captured between Ceres Ridley's ejection request and its " +
                    "promotion; that deferred state has no current representation.");
        },
        // RoomMainASMVar1 ($07E1) is now one shared word. These private copies seed it once
        // the runtime graph is complete; see DebuggerStateFieldMigrations.
        [(typeof(SuperMetroidRuntime).FullName!, "_ceresFallingDebrisTimer")] = Remember,
        [(typeof(SuperMetroidRuntime).FullName!, "_escapeDiagonalFrames")] = Remember,
        [(typeof(CeresElevatorShaftRoomMainState).FullName!, "<RotationIndex>k__BackingField")] = Remember,
        [(typeof(MaridiaElevatubeRoomMainState).FullName!, "<PositionSubposition>k__BackingField")] = Remember,
        // PaletteChangeNumerator ($7E:C400) is now one shared counter. A legacy fade keeps
        // its saved progress in a counter of its own; Kraid's restarts from the shared one.
        [(CartridgePaletteTransition, "transitionNumber")] = (instance, _, value) =>
        {
            if (value is not int transitionNumber)
                throw new InvalidDataException("Legacy palette transition number is not an integer.");
            var counter = new GradualColorChangeCounter();
            typeof(GradualColorChangeCounter).GetProperty(nameof(GradualColorChangeCounter.Numerator))!
                .SetValue(counter, checked((ushort)transitionNumber));
            instance.GetType().GetField("numerator",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(instance, counter);
            if (transitionNumber != 0)
                Console.Error.WriteLine("WARNING: Legacy palette fade was captured mid-transition; it resumes on its own counter rather than the shared PaletteChangeNumerator.");
        },
        // CameraDistanceIndex ($0941) is now one shared word on the enemy system. Kraid's
        // private copy seeds it; null meant an already-defeated room, which leaves it zero.
        [(typeof(KraidEnemyState).FullName!, "<CameraDistanceIndex>k__BackingField")] = (instance, name, value) =>
            Remember(instance, name, value ?? (ushort)0),
        [(typeof(KraidEnemyState).FullName!, "<RoomBackgroundFadeStep>k__BackingField")] = (_, _, value) =>
        {
            if (value is not ushort step)
                throw new InvalidDataException("Legacy Kraid background fade step is not a word.");
            if (step != 0)
                Console.Error.WriteLine("WARNING: Legacy Kraid background fade was captured mid-transition; it restarts from the shared PaletteChangeNumerator.");
        },
        // The escape-door dust index was always native word $0FF2, now one
        // DeathAndEscapeExplosionIndex. Fields restore in declaration order, so Phase and the
        // renamed death index are already set: from the door-exploding phase on, the escape
        // cursor was the live value.
        [(typeof(MotherBrainRainbowBeamAttackSequence).FullName!, "<EscapeDoorIndex>k__BackingField")] = (instance, _, value) =>
        {
            if (value is not ushort escapeDoorIndex)
                throw new InvalidDataException("Legacy Mother Brain escape-door index is not a word.");
            var sequence = (MotherBrainRainbowBeamAttackSequence)instance;
            if (sequence.Phase is MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDoorExplodingStartTimer or
                MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBlowUpEscapeDoor or
                MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceKeepEarthquakeGoing)
            {
                typeof(MotherBrainRainbowBeamAttackSequence)
                    .GetField("<DeathAndEscapeExplosionIndex>k__BackingField",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .SetValue(sequence, escapeDoorIndex);
            }
        },
        // A save station now queues its sound and draws its first frame in the PLM pass the
        // confirmation returns into ($84:AFF4-$AFFA). The old one-frame deferral flag is
        // set only in a capture taken on that confirmation frame, which has no current form.
        [(StationPlmState, "<SaveStartSoundPending>k__BackingField")] = (_, _, value) =>
        {
            if (value is not bool pending)
                throw new InvalidDataException("Legacy save-station sound flag is not a Boolean.");
            if (pending)
                throw new InvalidDataException(
                    "Legacy snapshot was captured on a save station's confirmation frame, before " +
                    "its deferred first animation pass; that state has no current representation.");
        },
        // Draygon's turret and goop speeds are the A values each spawn passes to $86:8027,
        // now named constants. The old shared copy has no current meaning.
        [(typeof(DraygonEnemyState).FullName!, "<ProjectileSpeedParameter>k__BackingField")] = (_, _, value) =>
        {
            if (value is not ushort)
                throw new InvalidDataException("Legacy Draygon projectile speed parameter is not a word.");
        },

        // Removed because nothing read them (#1273): the saved value is discarded.
        [("SuperMetroid.Core.Frontend.FileSelectMapAnimations+Arrow", "Program")] = Discard,
        // #627: always equal to Base once visible; arrows draw Base directly.
        [("SuperMetroid.Core.Frontend.FileSelectMapAnimations+Arrow", "Spritemap")] = Discard,
        [("SuperMetroid.Core.Frontend.FileSelectMapScroll", "customButtons")] = Discard,
        [("SuperMetroid.Core.Frontend.GameOverMenuState", "tilemap")] = Discard,
        [("SuperMetroid.Core.Game.AerialMovementResult", "<Horizontal>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.AerialMovementResult", "<Vertical>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutscenePoint", "<XPosition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutscenePoint", "<XSubposition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutscenePoint", "<YPosition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutscenePoint", "<YSubposition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<After>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<Angle>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<Before>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<BrainCollision>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<FunctionTimer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<HealingCompleted>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<Health>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<HyperBeamEnabled>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<InstructionList>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<MotherBrainInterrupted>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<MovementTablePointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<PhaseThreeHandoff>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<SamusAnimationFrozen>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<SamusCrouchingRequested>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<SamusRainbowActivated>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<SamusRainbowDisabled>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<SamusStandingRequested>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<SamusTouchCollision>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<Speed>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<XVelocity>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidCutsceneStepResult", "<YVelocity>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BabyMetroidDeathExplosionRequest", "<PatternIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BlockMoveResult", "<BrokenBombBlock>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BlockMoveResult", "<CeilingSlopeBlock>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BlockMoveResult", "<CollisionBlock>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BlockMoveResult", "<FloorSlopeBlock>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BlockMoveResult", "<PositionAdjustedBySlope>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BombBlockReaction", "<Behavior>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BombBlockReaction", "<BlockX>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BombBlockReaction", "<BlockY>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BombBlockReaction", "<CollisionType>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BombJumpMovementResult", "<Ended>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BombJumpMovementResult", "<Horizontal>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BombJumpMovementResult", "<Started>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BombProjectileFrameResult", "<BlockReactions>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BombProjectileFrameResult", "<BombSpreadStarted>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BombProjectileFrameResult", "<ExplosionStarted>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BombProjectileFrameResult", "<PlacedSlot>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.BombProjectileFrameResult", "<ProjectileDeleted>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.CeresRidleyEjectionResult", "<Ended>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.CeresRidleyEjectionResult", "<Horizontal>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.CeresRidleyEjectionResult", "<Initialized>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.CeresRidleyEjectionResult", "<Vertical>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.CrocomireDeathState", "<RumbleIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.CrocomireDropRequest", "<ItemDropChancesPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.CrocomireDropRequest", "<X>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.CrocomireDropRequest", "<Y>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.CrystalFlashMovementResult", "<Completed>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.CrystalFlashMovementResult", "<ConsumedAmmo>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.CrystalFlashMovementResult", "<PhaseAfterStep>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.CrystalFlashMovementResult", "<RestoredEnergy>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.DrainedSamusMovementResult", "<HitCeiling>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.DrainedSamusMovementResult", "<PhaseAfterStep>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.DrainedSamusMovementResult", "<Vertical>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.DraygonEscapeResult", "<CountedInput>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.DraygonEscapeResult", "<EscapeButtonCounter>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.DraygonEscapeResult", "<NewlyPressedDpad>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.DraygonEscapeResult", "<PoseAfterRelease>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.DraygonEscapeResult", "<PoseBeforeRelease>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.DraygonGrabbedMovementResult", "<Pose>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.DraygonGrabbedMovementResult", "<PreviousSolidVerticalCollisionResult>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.DraygonGrabbedMovementResult", "<SolidVerticalCollisionResult>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.DraygonGrabbedMovementResult", "<XPosition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.DraygonGrabbedMovementResult", "<YPosition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.FakeKraidDropRequest", "<DeathExplosionVariant>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.FakeKraidDropRequest", "<ItemDropChancesPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.FakeKraidDropRequest", "<X>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.FakeKraidDropRequest", "<Y>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.GrappleMovementResult", "<AnchorDisconnected>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.GrappleMovementResult", "<CancelQueued>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.GrappleMovementResult", "<Cancelled>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.GrappleMovementResult", "<Connected>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.GrappleMovementResult", "<DropQueued>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.GrappleMovementResult", "<Dropped>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.GrappleMovementResult", "<LockedInPlace>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.GrappleMovementResult", "<ReleaseQueued>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.GrappleMovementResult", "<Released>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.GrappleMovementResult", "<SpecialAngleHandled>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.GrappleMovementResult", "<WallGrabEntered>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.GrappleMovementResult", "<WallJumpQueued>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.GrappleMovementResult", "<WallJumpWindowOpened>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.GrappleMovementResult", "<WallProbeCollided>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.HyperBeamPaletteFxStepResult", "<Active>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.HyperBeamPaletteFxStepResult", "<CompletedCycles>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.HyperBeamPaletteFxStepResult", "<FrameIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.HyperBeamPaletteFxStepResult", "<InstructionPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.HyperBeamPaletteFxStepResult", "<InstructionTimer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.HyperBeamPaletteFxStepResult", "<PaletteWritten>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.KagoBugDropRequest", "<EnemyDefinitionPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.KagoBugDropRequest", "<EnemyProjectileNativeIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.KagoBugDropRequest", "<X>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.KagoBugDropRequest", "<Y>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.KnockbackMovementResult", "<Ended>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.KnockbackMovementResult", "<Horizontal>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.KnockbackMovementResult", "<ImpactYSpeed>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.KnockbackMovementResult", "<ImpactYSubspeed>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.KnockbackMovementResult", "<Vertical>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.KraidEnemyState", "<Unknown4>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MagdolliteLavaDropRequest", "<EnemyDefinitionPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MagdolliteLavaDropRequest", "<EnemyProjectileNativeIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MagdolliteLavaDropRequest", "<X>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MagdolliteLavaDropRequest", "<Y>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MetroidDropRequest", "<EnemyDefinitionPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MetroidDropRequest", "<SourceSpriteObjectIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MetroidDropRequest", "<XPosition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MetroidDropRequest", "<YPosition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MorphBallMovementResult", "<Horizontal>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainBodyAnimationState", "<Bg2XScroll>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainBodyAnimationState", "<Bg2YScroll>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainBodyAnimationState", "<SpritemapPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainBombDropRequest", "<EnemyDefinitionPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainBombDropRequest", "<X>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainBombDropRequest", "<Y>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainCorpseDustRequest", "<EntryIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainDeathExplosionRequest", "<PatternIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainDeathExplosionRequest", "<XPosition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainDeathExplosionRequest", "<YPosition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainEscapeDoorExplosionRequest", "<PatternIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainForcedSamusMovementResult", "<After>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainForcedSamusMovementResult", "<CameraPreviousPosition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainForcedSamusMovementResult", "<ReachedVerticalBoundary>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainForcedSamusMovementResult", "<ReachedWall>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainForcedSamusMovementResult", "<XVelocity>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainForcedSamusMovementResult", "<YVelocity>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackSequence", "<HeadSpritemapPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackSequence", "<OnionRingTargetAngle>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<AngularWidth>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<EarthquakeTimer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<EarthquakeType>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<FinishOffAttack>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<FunctionTimer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<HeadInstructionList>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<HealthAfter>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<HealthBefore>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<LowerNeckMovementIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<MissilesAfter>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<MissilesBefore>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<NeckAngleDelta>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<Phase3Attack>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<PowerBombsAfter>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<PowerBombsBefore>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<SuperMissilesAfter>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<SuperMissilesBefore>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<TypewriterStepRequested>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<TypewriterTextPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<UnlockedSamus>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowBeamAttackStepResult", "<UpperNeckMovementIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowExplosionRequest", "<SequenceIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.MotherBrainRainbowExplosionRequest", "<SoundEffect>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.PhantoonFlameDropRequest", "<EnemyDefinitionPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.PhantoonFlameDropRequest", "<ItemDropChancesPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.PhantoonFlameDropRequest", "<X>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.PhantoonFlameDropRequest", "<Y>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RidleyEnemyState", "<AttackTableIndex>k__BackingField")] = Discard,
        // Retired with the live $0CEE read at $A6:BD2C; the flag itself is the state.
        [("SuperMetroid.Core.Game.RidleyEnemyState", "<PowerBombReactionLatched>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RidleyEnemyState", "<FireballCooldown>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RidleyEnemyState", "<FireballVolleyCounter>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RidleyEnemyState", "<GrabbedSamusMovementIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RidleyEnemyState", "<GrabbedSamusMovementLagTimer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RidleyEnemyState", "<PogoTargetX>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RidleyEnemyState", "<PreviousSamusX>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RidleyEnemyState", "<SamusMovementDirection>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RoomEnemySpawnSnapshot", "<Health>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RoomEnemySpawnSnapshot", "<Layer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RoomEnemySpawnSnapshot", "<NameWords>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RoomEnemySpawnSnapshot", "<PaletteIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RoomEnemySpawnSnapshot", "<XRadius>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RoomEnemySpawnSnapshot", "<YRadius>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RoomEnemySystem+DeadTourianCorpseProfile", "<DefinitionPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RoomShakeFrameResult", "<Applied>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.RoomShakeFrameResult", "<ShakesEnemies>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonDrawResult", "<Attributes>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonDrawResult", "<DirectionSelector>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonDrawResult", "<Frame>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonDrawResult", "<ScreenX>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonDrawResult", "<ScreenY>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonDrawResult", "<SpriteWritten>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonDrawResult", "<TileSource>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonDrawResult", "<TileUploadQueued>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonUpdateResult", "<CloseFlag>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonUpdateResult", "<DrawingDataPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonUpdateResult", "<DrawingMode>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonUpdateResult", "<FrameAfter>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonUpdateResult", "<FrameBefore>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonUpdateResult", "<HudItemChanged>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonUpdateResult", "<OpenFlag>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusArmCannonUpdateResult", "<TransitionStarted>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusBeamChargePaletteStepResult", "<ChargePaletteIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusBeamChargePaletteStepResult", "<HyperPaletteIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusBeamChargePaletteStepResult", "<PalettePointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusBeamChargePaletteStepResult", "<TimerAfter>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusBeamChargePaletteStepResult", "<TimerBefore>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusDeathSequenceStepResult", "<CounterAfterStep>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusDeathSequenceStepResult", "<ExplosionSpritemapIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusDeathSequenceStepResult", "<IndexAfterStep>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusDeathSequenceStepResult", "<IndexAtStart>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusDeathSequenceStepResult", "<PaletteChanged>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusDeathSequenceStepResult", "<PhaseAtStart>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusDeathSequenceStepResult", "<QueuedSegment>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusDeathSequenceStepResult", "<TimerAfterStep>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusDeathSequenceStepResult", "<WhiteoutChanged>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusHurtFlashPaletteStepResult", "<Action>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusHurtFlashPaletteStepResult", "<CounterAfter>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusHurtFlashPaletteStepResult", "<CounterBefore>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusHurtFlashPaletteStepResult", "<HurtSoundQueued>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusHurtFlashPaletteStepResult", "<PaletteAddress>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusHurtFlashPaletteStepResult", "<Recovery>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusLookupFailurePose", "<Command>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusPoseTransition", "<CurrentPose>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusPoseTransition", "<EntryAddress>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusPoseTransition", "<RequiredHeldInput>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusPoseTransition", "<RequiredNewInput>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusProjectileFrameResult", "<CollisionStartedExplosion>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusProjectileFrameResult", "<FiredSlot>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusProjectileFrameResult", "<ProjectileDeleted>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusProjectileSpawnSnapshot", "<Direction>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusProjectileSpawnSnapshot", "<SlotIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusProjectileSpawnSnapshot", "<XPosition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusProjectileSpawnSnapshot", "<XVelocity>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusProjectileSpawnSnapshot", "<YPosition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusProjectileSpawnSnapshot", "<YVelocity>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusVisorPaletteStepResult", "<Action>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusVisorPaletteStepResult", "<LayerBlendingDefaultConfig>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusVisorPaletteStepResult", "<PackedAfter>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusVisorPaletteStepResult", "<PackedBefore>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusVisorPaletteStepResult", "<SourceByteOffset>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SamusVisorPaletteStepResult", "<WrittenColor>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.ShinesparkMovementResult", "<EndedByCollision>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.ShinesparkMovementResult", "<EndedByLowEnergy>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.ShinesparkMovementResult", "<EnergyDrained>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.ShinesparkMovementResult", "<Horizontal>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.ShinesparkMovementResult", "<PhaseAtStart>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.ShinesparkMovementResult", "<Vertical>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.ShinesparkReleasedEchoClear", "<Angle>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.ShinesparkReleasedEchoClear", "<Axis>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.ShinesparkReleasedEchoClear", "<NativeSlot>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.ShinesparkReleasedEchoClear", "<Radius>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.ShinesparkReleasedEchoClear", "<ScreenX>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.ShinesparkReleasedEchoClear", "<ScreenY>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.ShinesparkReleasedEchoClear", "<XPosition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.ShinesparkReleasedEchoClear", "<YPosition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SolidEnemyCollisionResult", "<DistanceSubposition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SolidEnemyCollisionResult", "<TargetXPosition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SolidEnemyCollisionResult", "<TargetYPosition>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SporeSpawnDropRequest", "<X>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.SporeSpawnDropRequest", "<Y>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.TorizoOrbDropRequest", "<EnemyDefinitionPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.TorizoOrbDropRequest", "<ItemDropChancesPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.TorizoOrbDropRequest", "<X>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.TorizoOrbDropRequest", "<Y>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.XrayBeamStepResult", "<AngleAfterStep>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.XrayBeamStepResult", "<AngleAtStart>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.XrayBeamStepResult", "<PhaseAfterStep>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.XrayBeamStepResult", "<SetupStage>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.XrayBeamStepResult", "<WidthAfterStep>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.XrayBeamStepResult", "<WidthAtStart>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.XrayPoseInputResult", "<Angle>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.XrayPoseInputResult", "<CompletedTurn>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.XrayPoseInputResult", "<Pose>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Game.XrayPoseInputResult", "<StartedTurn>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.CartridgeDoorHeader", "<BitFlags>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.CollectiblePickupEvent", "<BlockIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.CollectiblePickupEvent", "<Presentation>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.CollectiblePickupEvent", "<RoomArgument>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.CollectiblePickupEvent", "<TriggerProjectileType>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<Direction>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<DoorAsmPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<DoorCapXBlock>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<DoorCapYBlock>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<DoorPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<EnemyPopulationPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<EnemyTilesetPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<RoomHeightInScreens>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<RoomStatePointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<RoomWidthInScreens>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<ScreenX>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<ScreenY>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<SkyByteCount>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<SkySourceAddress>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<SkyVramDestination>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LandingSiteEntryState", "<SpawnDistance>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LoadStationEntry", "<DoorBts>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LoadStationEntry", "<ListPointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LoadStationEntry", "<RequestedAreaIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.LoadStationEntry", "<StationIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.PlmTilemapUpdate", "<BlockIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.RoomPlmSystem", "_bombTorizoHandWasDeleted")] = Discard,
        [("SuperMetroid.Core.Rooms.RoomPlmSystem", "_bombTorizoHandWasLoaded")] = Discard,
        [("SuperMetroid.Core.Rooms.RoomPlmSystem", "_motherBrainGlassWasDeleted")] = Discard,
        [("SuperMetroid.Core.Rooms.RoomPlmSystem+EyeDoorPlmState", "<Orientation>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Rooms.TilesetDefinition", "<Pointer>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Runtime.CeresElevatorShaftRoomMainResult", "<IsActive>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Runtime.CeresElevatorShaftRoomMainResult", "<RotationIndex>k__BackingField")] = Discard,
        [("SuperMetroid.Core.Runtime.CeresElevatorShaftRoomMainResult", "<RotationTimer>k__BackingField")] = Discard,
        // The intro now keeps native INIDISP ($51) and the $0723/$0725 fade words; the
        // inidisp/fade introduction derives them from these. Rinkas moved into native
        // slots, rebuilt by the slots introduction. The Ceres flight waits on the real
        // music queue instead of a host countdown, which has no current equivalent.
        [("SuperMetroid.Core.Frontend.IntroCinematicState", "brightness")] = Remember,
        [("SuperMetroid.Core.Frontend.IntroCinematicState", "fadeDelay")] = Discard,
        [("SuperMetroid.Core.Frontend.IntroRinkaSystem", "rinkas")] = Remember,
        [("SuperMetroid.Core.Frontend.IntroCeresFlightState", "musicQueueTimer")] = Discard,
    };

    /// <summary>Returns the migration of a retired serialized field.</summary>
    internal static bool TryGetMigration(Type declaringType, string serializedName, out RetiredFieldMigration migrate) =>
        Retired.TryGetValue((declaringType.FullName!, serializedName), out migrate!);

    /// <summary>Returns a retired word drained from <paramref name="instance"/>, if its capture had one.</summary>
    internal static bool TryGetLegacyWord(object instance, string serializedName, out ushort value)
    {
        value = 0;
        if (!LegacyValues.TryGetValue(instance, out Dictionary<string, object?>? values) ||
            !values.TryGetValue(serializedName, out object? stored))
            return false;
        value = stored as ushort? ?? throw new InvalidDataException(
            $"Legacy {instance.GetType().FullName}.{serializedName} is not a word.");
        return true;
    }

    /// <summary>Returns a retired value of any type drained from <paramref name="instance"/>.</summary>
    internal static bool TryGetLegacyValue(object instance, string serializedName, out object? value)
    {
        value = null;
        return LegacyValues.TryGetValue(instance, out Dictionary<string, object?>? values) &&
            values.TryGetValue(serializedName, out value);
    }

    private static void Remember(object instance, string serializedName, object? value) =>
        LegacyValues.GetOrCreateValue(instance)[serializedName] = value;

    private static void Discard(object instance, string serializedName, object? value) { }
}
