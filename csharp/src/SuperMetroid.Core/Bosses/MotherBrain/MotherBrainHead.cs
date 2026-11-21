using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's head and brain: their setup, neck, animations, cues and drawing.</summary>
[RoomLifetime]
internal sealed class MotherBrainHead
{
    /// <summary>Provides room services, pictures, colour state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The room's enemy places, in which the head finds her body listed right before it.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues the encounter's music and sound effects in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Draws encounter artwork and applies installed colours.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>The current enemy pass's clocks and flags.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>The other actors in the room: the Baby, the Rinkas, the Pirate laser and the afterburn.</summary>
    private readonly IMotherBrainRoomActors _roomActors;
    /// <summary>Mother Brain's projectiles: bombs, onion rings, drool, lasers, hand beams, turret shots and the tank's glass.</summary>
    private readonly MotherBrainProjectiles _projectileAttacks;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainHead(MotherBrainHeadDependencies dependencies)
    {
        _room = dependencies.Room;
        _places = dependencies.Places;
        _projectiles = dependencies.Projectiles;
        _audio = dependencies.Audio;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _roomActors = dependencies.RoomActors;
        _projectileAttacks = dependencies.ProjectileAttacks;
    }

    /// <summary>Sets Mother Brain's head up when the room spawns it.</summary>
    internal void InitializeMotherBrainHead(RoomEnemyPlace head)
    {
        // The head joins the encounter her body began; it holds it from here (#2218).
        MotherBrainEnemyState state = _places.Member(head, MotherBrainFightDefinitions.Listing, EnemyDefinitionId.MotherBrainHead, EnemyDefinitionId.MotherBrainBody)
            .RequireSpeciesState<MotherBrainEnemyState>();
        head.SetSpeciesState(state);

        head.Health = MotherBrainFightDefinitions.FirstPhaseHealth;
        // The head shows her brain's hit area from its next animation update on (#2220); the visible brain runs a
        // separate animation.
        head.AnimationClock.StepOnNextUpdate();
        head.Contact = head.Contact.With(EnemyContact.BlocksPlasmaBeam);
        head.Presence = head.Presence.With(EnemyPresence.Hidden);
        MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.WholeHeadMouthClosed);
    }

    /// <summary>
    /// The head place's animation update (#2220): its first shows her brain's hit area for good, which is what Samus and
    /// her shots meet there; the visible brain runs its own animation in her brain's draw.
    /// </summary>
    internal static void AdvanceMotherBrainHeadPlace(RoomEnemyPlace head)
    {
        if (head.AnimationClock.Expire())
            ShowBrainHitArea(head);
    }

    /// <summary>
    /// Shows her brain's hit area on <paramref name="place"/> for good, its clock stopped (#2454).
    /// </summary>
    internal static void ShowBrainHitArea(RoomEnemyPlace place)
    {
        place.Frame = EnemyFrameId.MotherBrainBrainHitArea;
        place.AnimationClock.ShowForGood();
    }

    /// <summary>What <see cref="RunMotherBrainHeadMain"/> does for each <see cref="MotherBrainBrainAction"/> member, each member's own arm.</summary>
    private static IdentityTable<MotherBrainBrainAction, Action<RoomEnemyPlace, MotherBrainEnemyState, SamusState>> RunMotherBrainHeadMainForMotherBrainBrainAction => field ??= new(new Dictionary<MotherBrainBrainAction, Action<RoomEnemyPlace, MotherBrainEnemyState, SamusState>>
    {
        [MotherBrainBrainAction.ShowsBrainAlone] = static (head, state, samus) => state.BrainActionDrawn = MotherBrainBrainAction.ShowsBrainAlone,
        [MotherBrainBrainAction.ShowsBrainOnNeck] = static (head, state, samus) =>
        {
            StepMotherBrainNeck(
                state,
                samus);
            head.Position = head.Position.WithWhole(state.Neck.BrainAnchor.Position + MotherBrainNeckJointDefinitions.BrainAboveAnchor);
            state.BrainActionDrawn = MotherBrainBrainAction.ShowsBrainOnNeck;
        },
    });

    /// <summary>Chooses whether her brain and neck are drawn and places her head on her neck.</summary>
    internal static void RunMotherBrainHeadMain(RoomEnemyPlace head, MotherBrainEnemyState state, SamusState samus)
    {
        // Her brain's action decides whether her own draw shows her neck with her brain.
        RunMotherBrainHeadMainForMotherBrainBrainAction[state.BrainAction](head, state, samus);
    }

    /// <summary>
    /// Draws her brain, and her neck when it is shown, after every queued enemy has been drawn. The brain is shown by
    /// her own draw rather than the common enemy draw, although her population entry collides by its frames' hitboxes.
    /// </summary>
    /// <param name="state">The room's Mother Brain, as her body attached her to the room (#2194), or none before she sets up.</param>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="camera">Camera position (viewport top-left) in whole room pixels.</param>
    /// <param name="samus">Samus, whom the brain animation's cues read.</param>
    internal void DrawMotherBrainBrainAndNeck(MotherBrainEnemyState? state, SpriteFrameBuffer sprites, Position16<RoomPixels> camera, SamusState samus)
    {
        RoomEnemyPlace? head = state?.Head;
        if (state?.DrawBrain != true || head is null)
            return;
        // The brain draw samples (never advances) the RNG before reading the brain frame,
        // so the small breath can spawn even on a draw that then shows no brain.
        if (state.SmallPurpleBreathGenerationEnabled && !state.SmallPurpleBreathActive &&
            BossRandomRolls.InLowerHalf(_room.Random.Read()))
            SpawnMotherBrainPurpleBreathSmall(state);
        // The brain's animation plays at this draw, after every enemy's AI (MotherBrainBrainAnimation), and the whole draw
        // ends when the animation is not live.
        if (PlayMotherBrainBrain(state, samus) is not { } picture)
            return;

        EnemyFrameId brainFrame = picture.Frame;
        Offset16<RoomPixels> brainShake = GetMotherBrainBrainShake(state, head, picture.Shook);
        // The brain is published wherever it lies; the display clips it to the view (#1520).
        DrawMotherBrainWorldFrame(
            sprites,
            new SpriteSource(SpriteSourceKind.MotherBrainBrain),
            new MotherBrainWorldFrame(
                brainFrame,
                head.Position.Whole + brainShake,
                // Her brain's own draw bypasses ordinary enemy colour selection. The game
                // still selects the shared white hurt colours on odd flash-timer frames.
                MotherBrainCadenceDefinitions.HurtFlashWhite.IsSetIn(head.HurtFlash.UpdatesLeft) ? EnemyShades.HurtFlashLook : state.Colours.OwnShading),
            camera,
            ShownCorpseRows(state));

        if (!state.DrawNeck || state.Body.Presence.HasAny(EnemyPresence.Hidden))
            return;

        // The joints are drawn from the brain down to her body, so the joints nearer her brain lie over the others.
        Offset16<RoomPixels> neckShake = MotherBrainShake.OffsetOn(MotherBrainCadenceDefinitions.NeckShakeStep, head.HurtFlash.UpdatesLeft);
        foreach (MotherBrainNeckJoint joint in state.Neck.Joints.Reverse())
        {
            DrawMotherBrainWorldFrame(
                sprites,
                new SpriteSource(SpriteSourceKind.MotherBrainNeck),
                new MotherBrainWorldFrame(MotherBrainVisualDefinitions.NeckJointFrame, joint.Position + neckShake,
                    state.Colours.OwnShading),
                camera,
                ShownCorpseRows(state));
        }
    }

    /// <summary>
    /// The rows <paramref name="state"/>'s corpse shows as its own rot has left them, which her corpse's frame is drawn
    /// whole through (#1539); none while her rainbow beam sequence has not begun.
    /// </summary>
    private static CorpseRotRows? ShownCorpseRows(MotherBrainEnemyState state) =>
        state.RainbowBeamSequence?.ThirdPhaseDeath.CorpseRotting.Shown;

    /// <summary>Publishes a selected world-space Mother Brain frame from the head's sheet with colours and camera offsets.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="source">The part shown.</param>
    /// <param name="frame">The frame, where it lies and its shading.</param>
    /// <param name="camera">Camera position (viewport top-left) in whole room pixels.</param>
    /// <param name="corpseRows">The rows her rotting corpse shows, or none while she shows no rotting corpse.</param>
    private void DrawMotherBrainWorldFrame(
        SpriteFrameBuffer sprites,
        SpriteSource source,
        MotherBrainWorldFrame frame,
        Position16<RoomPixels> camera,
        CorpseRotRows? corpseRows)
    {
        // The frame is published wherever it lies, above the screen included; the display clips it (#1587).
        Offset16<ScreenPixels> screen = frame.World.RelativeToCamera(camera);

        // The head's sheet pictures each frame as the encounter's artwork changes leave it (#1392); her corpse's frame is drawn
        // whole through her corpse's own rows as its rot leaves them (#1539).
        _presentation.PublishSheetFrame(sprites, source, new EnemySheetDraw(EnemyDefinitionId.MotherBrainHead, EnemySheetDefinitions.FrameName(frame.Frame), screen)
            { Shading = frame.Shading, RotRows = corpseRows });
    }

    /// <summary>
    /// Where her brain's shake places it at this draw: her brain's own shake, which this draw's play of her brain counted
    /// down, while it was shaking (<paramref name="brainShook"/>); otherwise the shake her hurt flash or her shake timer paces.
    /// </summary>
    private static Offset16<RoomPixels> GetMotherBrainBrainShake(
        MotherBrainEnemyState state,
        RoomEnemyPlace head,
        bool brainShook)
    {
        if (brainShook)
            return state.BrainShake.Offset;

        int paced = head.HurtFlash.IsRunning ? head.HurtFlash.UpdatesLeft : head.Shake.UpdatesLeft;
        return MotherBrainShakeDefinitions.Offsets[MotherBrainCadenceDefinitions.BrainShakeStep.Of(paced)];
    }

    /// <summary>Moves neck segments toward the head and updates their attachment to Samus's position.</summary>
    private static void StepMotherBrainNeck(MotherBrainEnemyState state, SamusState samus)
    {
        if (state.NeckMovementEnabled)
        {
            RotationAngle lowerAngle = state.LowerNeckAngle;
            RotationAngle upperAngle = state.UpperNeckAngle;
            MotherBrainNeckMovement lowerMovement = state.LowerNeckMovement;
            MotherBrainNeckMovement upperMovement = state.UpperNeckMovement;
            MotherBrainNeckKinematics.StepAngles(
                ref lowerAngle,
                ref upperAngle,
                ref lowerMovement,
                ref upperMovement,
                state.NeckTurnPerUpdate,
                state.Head.Position.Y.Whole,
                samus.Position.Y.Whole);
            state.LowerNeckAngle = lowerAngle;
            state.UpperNeckAngle = upperAngle;
            state.LowerNeckMovement = lowerMovement;
            state.UpperNeckMovement = upperMovement;
        }

        MotherBrainNeckKinematics.PlaceJoints(state.Neck, state.Body.Position.Whole, state.LowerNeckAngle, state.UpperNeckAngle);
    }

    /// <summary>
    /// Plays her brain at her brain's draw (<see cref="MotherBrainBrainAnimation"/>) and returns the picture it shows, or
    /// null when the brain is not live; its cues act on <paramref name="state"/> and read <paramref name="samus"/>. A
    /// brain that shows a picture counts its own shake down at the same draw (#2469).
    /// </summary>
    /// <remarks>
    /// Once her late fight has begun its own cues act on it (#2471): her attacks on the Baby count, aim at and cry at the
    /// Baby it made.
    /// </remarks>
    internal EnemyFrameId? AdvanceMotherBrainBrain(MotherBrainEnemyState state, SamusState samus) =>
        PlayMotherBrainBrain(state, samus)?.Frame;

    /// <summary>
    /// Plays her brain at her brain's draw as <see cref="AdvanceMotherBrainBrain"/> does, returning the picture it shows and
    /// whether her brain's shake, which a shown picture counts down, was still shaking; null when the brain is not live.
    /// </summary>
    private MotherBrainBrainPicture? PlayMotherBrainBrain(MotherBrainEnemyState state, SamusState samus)
    {
        EnemyFrameId? shown = state.Brain.AdvanceAtDraw(_frameContext.TimeIsFrozen, cue => (state.RainbowBeamSequence is { } lateFight
                ? ApplyLateFightHeadCue(lateFight, samus, cue)
                : ApplyMotherBrainHeadCue(state, samus, cue)) is { } segment
            ? new AnimationContinuation<MotherBrainHeadSegment>(MotherBrainAnimations.HeadStartingWith(segment), AnimationPlace.StartOfAnimation)
            : null);
        return shown is { } frame ? new MotherBrainBrainPicture(frame, state.BrainShake.Step()) : null;
    }

    /// <summary>A picture her brain's draw shows and whether her brain's shake counted one of its updates off for it.</summary>
    /// <param name="Frame">The picture.</param>
    /// <param name="Shook">Whether her brain's shake was still shaking when the picture counted an update off it.</param>
    private readonly record struct MotherBrainBrainPicture(EnemyFrameId Frame, bool Shook);

    /// <summary>
    /// Carries out one event of a brain animation in her late fight: the cues of her attacks on the Baby act on
    /// <paramref name="lateFight"/>, and every other cue as before it began.
    /// </summary>
    /// <returns>The segment whose animation a random hold or drool event continues at; otherwise null.</returns>
    private MotherBrainHeadSegment? ApplyLateFightHeadCue(MotherBrainRainbowBeamAttackSequence lateFight, SamusState samus, MotherBrainHeadCue cue)
    {
        switch (cue)
        {
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.AimOnionRingsAtBabyMetroid }:
            {
                // Her brain plays its attack on the Baby only once the final beam's spawn has made it.
                BabyMetroidCutsceneState baby = lateFight.Baby ??
                    throw new InvalidOperationException("Mother Brain's brain aimed its onion rings at a Baby the final beam's spawn never made.");
                AimMotherBrainOnionRingsAt(lateFight.Encounter, baby.Position.Whole);
                return null;
            }
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.CountBabyMetroidAttack }:
                lateFight.IncrementLiveBabyMetroidAttackCounter();
                return null;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.ResetBabyMetroidAttacks }:
                lateFight.ResetLiveBabyMetroidAttackCounter();
                return null;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.QueueBabyMetroidAttackSound }:
                // The attack cry step skips the attack cry once the attack counter reaches the silent attack.
                if (lateFight.Revival.BabyMetroidAttackCounter != MotherBrainFightDefinitions.SilentBabyAttackCount)
                    _audio.QueueEnemySound(MotherBrainSoundDefinitions.AttackCry, priority: SoundRequestPriority.Normal);
                return null;
            default:
                return ApplyMotherBrainHeadCue(lateFight.Encounter, samus, cue);
        }
    }

    /// <summary>Carries out one event of a brain animation.</summary>
    /// <returns>The segment whose animation a random hold or drool event continues at; otherwise null.</returns>
    /// <remarks>
    /// The brain animation advances in her brain's draw, after the enemy update has queued the frame's enemy
    /// sounds, so its sounds go straight to the enemy audio queue at their limits (#1391).
    /// </remarks>
    private MotherBrainHeadSegment? ApplyMotherBrainHeadCue(MotherBrainEnemyState state, SamusState samus, MotherBrainHeadCue cue) =>
        cue.Match(ApplyMotherBrainHeadCueCases, new ApplyMotherBrainHeadCueContext(state, samus, this));

    /// <summary>What <see cref="ApplyMotherBrainHeadCue"/> does on each <see cref="MotherBrainHeadCue"/>, built once (#2553).</summary>
    private static MotherBrainHeadCueCases<ApplyMotherBrainHeadCueContext, MotherBrainHeadSegment?> ApplyMotherBrainHeadCueCases { get; } = new(
            PlaySound: static (playSound, context) =>
            {
                context.Owner._audio.QueueEnemySound(playSound.Effect, priority: SoundRequestPriority.Normal);
                return (MotherBrainHeadSegment?)null;
            },
            SpawnBomb: static (bomb, context) =>
            {
                context.Owner.SpawnMotherBrainBomb(context.State, bomb.Afterburns);
                return null;
            },
            Signal: static (signal, context) => context.Owner.HeadSignals[signal.Kind](context.State, context.Samus));

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="State">The state the cases read.</param>
    /// <param name="Samus">The samus the cases read.</param>
    /// <param name="Owner">The <see cref="MotherBrainHead"/> reacting.</param>
    private readonly record struct ApplyMotherBrainHeadCueContext(MotherBrainEnemyState State, SamusState Samus, MotherBrainHead Owner);

    /// <summary>
    /// What each of her brain's signals does before her late fight, and the segment it continues at, or null to run on.
    /// Only the animations of her attacks on the Baby, which her late fight starts, raise the Baby's cues
    /// (ApplyLateFightHeadCue), so one raised before it fails.
    /// </summary>
    private IdentityTable<MotherBrainHeadCueKind, Func<MotherBrainEnemyState, SamusState, MotherBrainHeadSegment?>> HeadSignals => field ??=
        new(new Dictionary<MotherBrainHeadCueKind, Func<MotherBrainEnemyState, SamusState, MotherBrainHeadSegment?>>
        {
            [MotherBrainHeadCueKind.ShakeBrain] = static (state, _) =>
            {
                state.BrainShake.ShakeFor(MotherBrainFightDefinitions.BrainShakeUpdates);
                return null;
            },
            [MotherBrainHeadCueKind.SpawnDrool] = (state, _) =>
            {
                SpawnMotherBrainDrool(state);
                return null;
            },
            [MotherBrainHeadCueKind.SpawnBigPurpleBreath] = (state, _) =>
            {
                SpawnMotherBrainPurpleBreathBig(state);
                return null;
            },
            [MotherBrainHeadCueKind.DisableNeckMovement] = static (state, _) =>
            {
                state.NeckMovementEnabled = false;
                return null;
            },
            [MotherBrainHeadCueKind.EnableNeckMovement] = static (state, _) =>
            {
                state.NeckMovementEnabled = true;
                return null;
            },
            [MotherBrainHeadCueKind.AimOnionRingsAtSamus] = (state, samus) =>
            {
                AimMotherBrainOnionRingsAt(state, samus.Position.Whole);
                return null;
            },
            [MotherBrainHeadCueKind.SpawnOnionRing] = (state, _) =>
            {
                SpawnMotherBrainOnionRing(state, state.OnionRingsTargetAngle);
                return null;
            },
            [MotherBrainHeadCueKind.SpawnLaser] = (state, _) =>
            {
                SpawnMotherBrainMouthLaser(state);
                return null;
            },
            [MotherBrainHeadCueKind.AimOnionRingsAtBabyMetroid] = static (_, _) => throw LateFightCueBeforeItBegan(MotherBrainHeadCueKind.AimOnionRingsAtBabyMetroid),
            [MotherBrainHeadCueKind.CountBabyMetroidAttack] = static (_, _) => throw LateFightCueBeforeItBegan(MotherBrainHeadCueKind.CountBabyMetroidAttack),
            [MotherBrainHeadCueKind.ResetBabyMetroidAttacks] = static (_, _) => throw LateFightCueBeforeItBegan(MotherBrainHeadCueKind.ResetBabyMetroidAttacks),
            [MotherBrainHeadCueKind.QueueBabyMetroidAttackSound] = static (_, _) => throw LateFightCueBeforeItBegan(MotherBrainHeadCueKind.QueueBabyMetroidAttackSound),
            [MotherBrainHeadCueKind.PrepareRainbowBeamCharge] = (state, _) =>
            {
                state.SmallPurpleBreathGenerationEnabled = false;
                _audio.QueueEnemySound(MotherBrainSoundDefinitions.RainbowBeamCharge, priority: SoundRequestPriority.Normal);
                return null;
            },
            [MotherBrainHeadCueKind.SpawnRainbowBeamChargeParticle] = (state, _) =>
            {
                SpawnMotherBrainRainbowChargingProjectile(state);
                return null;
            },
            // The neutral hold holds the phase-two neutral pose again on most samples.
            [MotherBrainHeadCueKind.MaybeHoldNeutralPhase2] = (_, _) =>
                MotherBrainRandomDefinitions.NeutralPhase2Holds.IsTakenBy(_room.Random.Read()) ? MotherBrainHeadSegment.NeutralPhase2Hold : null,
            // The hold's chance alone decides whether the phase-three pose holds again.
            [MotherBrainHeadCueKind.MaybeHoldNeutralPhase3] = (_, _) =>
                MotherBrainRandomDefinitions.NeutralPhase3Holds.IsTakenBy(_room.Random.Read()) ? MotherBrainHeadSegment.NeutralPhase3Hold : null,
            // The drool hold returns its destination: only the burst's chance bursts into more drool; every other sample
            // keeps the one-frame loop.
            [MotherBrainHeadCueKind.MaybeDroolAgain] = (_, _) =>
                MotherBrainRandomDefinitions.DroolBursts.IsTakenBy(_room.Random.Read()) ? MotherBrainHeadSegment.DyingDroolBurst : null,
        });

    /// <summary>The failure of a cue of her late fight that her brain raised before it began.</summary>
    private static InvalidOperationException LateFightCueBeforeItBegan(MotherBrainHeadCueKind kind) =>
        new($"Mother Brain's brain raised {kind}, a cue of her late fight, before it began.");

    /// <summary>Fires the mouth laser, freezing the neck.</summary>
    private void SpawnMotherBrainMouthLaser(MotherBrainEnemyState state)
    {
        RoomEnemyPlace head = state.Head;
        state.NeckMovementEnabled = false;

        // The laser leaves her mouth, right of and below her head, travelling right, at her laser's speed. Her brain fires
        // it while it is drawn, after every enemy of the room has been drawn, and the laser takes the damage of the enemy
        // drawn last. Only her phase-two fight fires it, when her body is the one enemy the room draws: her brain is drawn
        // apart from the room's enemies, and her Rinkas are gone. So it deals her body's definition's damage, whatever an
        // installation gives it.
        EnemyProjectile? laser = _roomActors.SpawnPirateLaser(
            state.BodyOccupant,
            head.Position.Whole + MotherBrainFightDefinitions.BeamOffset,
            direction: HorizontalDirection.Right);
        if (laser is not null)
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.PirateMotherBrainLaserSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// Aims the onion rings from her mouth at a target: the angle from her mouth to the target, a quarter turn on,
    /// clamped to <see cref="MotherBrainOnionRingDefinitions.LowestAim"/> through
    /// <see cref="MotherBrainOnionRingDefinitions.HighestAim"/>.
    /// </summary>
    private static void AimMotherBrainOnionRingsAt(
        MotherBrainEnemyState state,
        Position16<RoomPixels> target)
    {
        RoomEnemyPlace head = state.Head;
        Position16<RoomPixels> mouth = head.Position.Whole + MotherBrainFightDefinitions.MouthOffset;
        TrigonometryAngle angle = EnemyMath.CalculateAimAngle(target - mouth).TurnedBy(TrigonometryAngle.QuarterTurn);

        // The clamp splits the turn in two halves. The
        // half from three quarters of a turn round past zero maps to the lowest aim; the opposite half maps
        // to the highest. Only the aims between them survive unchanged.
        state.OnionRingsTargetAngle =
            angle >= MotherBrainOnionRingDefinitions.LowestAim && angle < MotherBrainOnionRingDefinitions.HighestAim ? angle
            : angle >= MotherBrainOnionRingDefinitions.HighestAim && angle < MotherBrainOnionRingDefinitions.ClampSplit ? MotherBrainOnionRingDefinitions.HighestAim
            : MotherBrainOnionRingDefinitions.LowestAim;
    }

    /// <summary>Runs the bomb's setup.</summary>
    private void SpawnMotherBrainBomb(
        MotherBrainEnemyState state,
        int afterburnCount)
    {
        EnemyProjectile? bomb = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.MotherBrainBomb, _projectileAttacks.Projectile, graphics: SpriteGraphics.SharedIn(state.Colours.OwnShading), new MotherBrainEffectAnimation(_projectileAttacks.Projectile, MotherBrainEffectSegment.Bomb));
        if (bomb is null)
            return;

        RoomEnemyPlace head = state.Head;
        bomb.Position = bomb.Position.WithWhole(head.Position.Whole + MotherBrainFightDefinitions.BombOffset);
        bomb.Position = bomb.Position.WithoutFractions();
        bomb.Velocity = new(MotherBrainBombBounceDefinitions.LaunchSpeedX, MotherBrainBombBounceDefinitions.LaunchSpeedY);
        var bombState = new MotherBrainBombState(state, MotherBrainBombBounceDefinitions.BounceSpeed, afterburnCount);
        _projectileAttacks.Projectile.Bombs.Start(bomb, bombState);
        state.RecordBombSpawned(bombState);
    }

    /// <summary>
    /// Runs the head's onion-ring step and the ring's setup. The ring takes its enemy-projectile place after the
    /// room turrets and all lingering drool have theirs, so no ring appears when every place is held.
    /// </summary>
    private void SpawnMotherBrainOnionRing(
        MotherBrainEnemyState state,
        TrigonometryAngle angle)
    {
        EnemyProjectile? ring = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.MotherBrainOnionRing, _projectileAttacks.OnionRing, graphics: SpriteGraphics.SharedIn(state.Colours.OwnShading), new MotherBrainEffectAnimation(_projectileAttacks.OnionRing, MotherBrainEffectSegment.OnionRing));
        if (ring is null)
            return;

        _projectileAttacks.OnionRing.OnionRings.Start(ring, new MotherBrainOnionRingState(state, MotherBrainOnionRingDefinitions.MouthWaitUpdates));
        ring.Velocity = ring.Velocity.WithX(EnemyMath.SineSpeed(MotherBrainOnionRingDefinitions.Speed, angle));
        ring.Velocity = ring.Velocity.WithY(EnemyMath.SineSpeed(
            MotherBrainOnionRingDefinitions.Speed,
            angle.TurnedBy(TrigonometryAngle.QuarterTurn)));

        // The ring's setup pins it to the mouth in the same update, before the first of its delayed attachment
        // frames (MotherBrainOnionRingDefinitions.MouthWaitUpdates).
        MotherBrainProjectiles.PinMotherBrainOnionRingToMouth(ring, state);
    }

    /// <summary>Runs the head's drool step and the drool's setup.</summary>
    private void SpawnMotherBrainDrool(MotherBrainEnemyState state)
    {
        if (!state.DroolGenerationEnabled)
            return;

        Offset16<RoomPixels> mouth = state.NextDroolMouthPoint.Entry;
        state.NextDroolMouthPoint = state.NextDroolMouthPoint.NextInCycle;

        // The game itself picks the drool by how fast her neck turns (#2471), and no phase stands behind that choice: her
        // phase-two laser's quick aim and steady fire, her fake death's landing, her third phase's bob and her late fight's
        // fast throws all drool the dying drool, her slow sways and rises the ordinary one. Only the neck's turn tells them apart, so it stays the
        // decision.
        RoomEnemyProjectileKind kind = state.NeckTurnPerUpdate < MotherBrainNeckTurnDefinitions.DyingDrool
            ? RoomEnemyProjectileKind.MotherBrainDrool
            : RoomEnemyProjectileKind.MotherBrainDyingDrool;
        EnemyProjectile? drool = _projectiles.AllocateEnemyProjectile(kind, _projectileAttacks.Projectile, graphics: SpriteGraphics.SharedStandard, new MotherBrainEffectAnimation(_projectileAttacks.Projectile, MotherBrainEffectSegment.Drool));
        if (drool is null)
            return;

        _projectileAttacks.Projectile.Drools.Start(drool, new MotherBrainDroolState(state, mouth));

        // Spawning the drool attaches it to the head at once, so its position is already
        // right before the later projectile pass repeats the same attachment.
        _projectileAttacks.Projectile.HoldMotherBrainDroolAtMouth(drool);
    }

    /// <summary>Runs the head's big purple breath step and the breath's setup.</summary>
    private void SpawnMotherBrainPurpleBreathBig(MotherBrainEnemyState state) =>
        _ = SpawnMotherBrainPurpleBreath(state, RoomEnemyProjectileKind.MotherBrainPurpleBreathBig, MotherBrainEffectSegment.PurpleBreathBig);

    /// <summary>Runs the small-breath spawn and the breath's setup; a small breath that spawns is her one small breath.</summary>
    private void SpawnMotherBrainPurpleBreathSmall(MotherBrainEnemyState state)
    {
        if (SpawnMotherBrainPurpleBreath(state, RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall, MotherBrainEffectSegment.PurpleBreathSmall))
            state.SmallPurpleBreathActive = true;
    }

    /// <summary>
    /// Spawns a purple breath of <paramref name="kind"/> playing <paramref name="animation"/> at her mouth, the breath offset
    /// from her head; it returns whether a projectile was free for it.
    /// </summary>
    private bool SpawnMotherBrainPurpleBreath(MotherBrainEnemyState state, RoomEnemyProjectileKind kind, MotherBrainEffectSegment animation)
    {
        EnemyProjectile? breath = _projectiles.AllocateEnemyProjectile(kind, _projectileAttacks.Projectile, graphics: SpriteGraphics.SharedStandard,
            new MotherBrainEffectAnimation(_projectileAttacks.Projectile, animation));
        if (breath is null)
            return false;

        _projectileAttacks.Projectile.MotherBrains.Start(breath, new MotherBrainProjectileState(state));
        RoomEnemyPlace head = state.Head;
        breath.Position = breath.Position.WithWhole(head.Position.Whole + MotherBrainFightDefinitions.BreathOffset);
        return true;
    }

    /// <summary>Spawns the rainbow beam's charge glow, pinned to her head in the update it spawns.</summary>
    private void SpawnMotherBrainRainbowChargingProjectile(MotherBrainEnemyState state)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging, _projectileAttacks.Projectile, graphics: SpriteGraphics.SharedStandard, new MotherBrainEffectAnimation(_projectileAttacks.Projectile, MotherBrainEffectSegment.RainbowBeamCharging));
        if (projectile is null)
            return;

        _projectileAttacks.Projectile.MotherBrains.Start(projectile, new MotherBrainProjectileState(state));
        projectile.Velocity = PixelVelocity<RoomPixels>.Zero;
        projectile.Position = projectile.Position.WithoutFractions();
        _projectileAttacks.Projectile.PinMotherBrainRainbowCharge(projectile);
    }
}
