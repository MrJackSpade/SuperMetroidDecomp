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
    /// <param name="head">The head's place.</param>
    /// <param name="state">The encounter her body began, which the head joins and holds from here (#2218).</param>
    internal static MotherBrainEnemyState InitializeMotherBrainHead(RoomEnemyPlace head, MotherBrainEnemyState state)
    {

        head.Health = MotherBrainFightDefinitions.FirstPhaseHealth;
        // The head shows her brain's hit area from its next animation update on (#2220); the visible brain runs a
        // separate animation.
        head.AnimationClock.StepOnNextUpdate();
        head.Contact = head.Contact.With(EnemyContact.BlocksPlasmaBeam);
        head.Presence = head.Presence.With(EnemyPresence.Hidden);
        MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.WholeHeadMouthClosed);
        return state;
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

    /// <summary>Runs the head main/hurt entry and its brain action's choice of whether her brain and neck are drawn.</summary>
    internal static void RunMotherBrainHeadMain(RoomEnemyPlace head, MotherBrainEnemyState state, SamusState samus)
    {
        // Her brain's action decides whether her own draw shows her neck with her brain.
        switch (state.BrainAction)
        {
            case MotherBrainBrainAction.ShowsBrainAlone:
                state.BrainActionDrawn = MotherBrainBrainAction.ShowsBrainAlone;
                return;
            case MotherBrainBrainAction.ShowsBrainOnNeck:
                StepMotherBrainNeck(
                    state,
                    samus);
                head.Position = head.Position.WithWhole(state.Neck.BrainAnchor.Position + MotherBrainNeckJointDefinitions.BrainAboveAnchor);
                state.BrainActionDrawn = MotherBrainBrainAction.ShowsBrainOnNeck;
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain has no behaviour for brain action {state.BrainAction}.");
        }
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

        // The neck draw emits the joint nearest the brain first and the body-side joint
        // last. Equal-priority overlap therefore follows the same drawing precedence.
        int shakeIndex = MotherBrainCadenceDefinitions.NeckShakeStep.Of(head.HurtFlash.UpdatesLeft);
        Offset16<RoomPixels> neckShake = MotherBrainShakeDefinitions.Offsets[shakeIndex];
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
    internal void DrawMotherBrainWorldFrame(
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
    /// Her late fight's volleys carry what their own cues act on (#2471, #2554): her volley at Samus the late fight, whose
    /// count of attacks on the Baby it clears and whose cry it reads; her volley at the Baby the arrived Baby it counts,
    /// aims at and cries at.
    /// </remarks>
    internal EnemyFrameId? AdvanceMotherBrainBrain(MotherBrainEnemyState state, SamusState samus) =>
        PlayMotherBrainBrain(state, samus)?.Frame;

    /// <summary>
    /// Plays her brain at her brain's draw as <see cref="AdvanceMotherBrainBrain"/> does, returning the picture it shows and
    /// whether her brain's shake, which a shown picture counts down, was still shaking; null when the brain is not live.
    /// </summary>
    private MotherBrainBrainPicture? PlayMotherBrainBrain(MotherBrainEnemyState state, SamusState samus)
    {
        EnemyFrameId? shown = state.Brain.AdvanceAtDraw(_frameContext.TimeIsFrozen, new BrainCues(this, state, samus));
        return shown is { } frame ? new MotherBrainBrainPicture(frame, state.BrainShake.Step()) : null;
    }

    /// <summary>A picture her brain's draw shows and whether her brain's shake counted one of its updates off for it.</summary>
    /// <param name="Frame">The picture.</param>
    /// <param name="Shook">Whether her brain's shake was still shaking when the picture counted an update off it.</param>
    private readonly record struct MotherBrainBrainPicture(EnemyFrameId Frame, bool Shook);

    /// <summary>The cues of her brain's draw over <paramref name="state"/>, which read <paramref name="samus"/>.</summary>
    /// <param name="head">The head whose projectiles, sounds and chances the cues use.</param>
    /// <param name="state">Her encounter, on which every brain animation's shared cues act.</param>
    /// <param name="samus">Samus, at whom her head aims.</param>
    private sealed class BrainCues(MotherBrainHead head, MotherBrainEnemyState state, SamusState samus) : IMotherBrainBrainCues
    {
        /// <inheritdoc/>
        public MotherBrainHeadSegment? RunHeadCue(MotherBrainBrainCue<MotherBrainHeadChanceCueKind> cue)
        {
            switch (cue)
            {
                case MotherBrainBrainCue<MotherBrainHeadChanceCueKind>.Shared shared:
                    head.ApplyMotherBrainHeadCue(state, samus, shared.Cue);
                    return null;
                // The neutral hold holds the phase-two neutral pose again on fifteen samples in sixteen.
                case MotherBrainBrainCue<MotherBrainHeadChanceCueKind>.Own { Kind: MotherBrainHeadChanceCueKind.MaybeHoldNeutralPhase2 }:
                    return MotherBrainRandomDefinitions.NeutralPhase2Holds.IsTakenBy(head._room.Random.Read()) ? MotherBrainHeadSegment.NeutralPhase2Hold : null;
                // The hold's chance alone decides whether the phase-three pose holds again.
                case MotherBrainBrainCue<MotherBrainHeadChanceCueKind>.Own { Kind: MotherBrainHeadChanceCueKind.MaybeHoldNeutralPhase3 }:
                    return head.HoldsNeutralPhase3Again() ? MotherBrainHeadSegment.NeutralPhase3Hold : null;
                // The drool hold returns its destination: only the burst's chance bursts into more drool; every other sample
                // keeps the one-frame loop.
                case MotherBrainBrainCue<MotherBrainHeadChanceCueKind>.Own { Kind: MotherBrainHeadChanceCueKind.MaybeDroolAgain }:
                    return MotherBrainRandomDefinitions.DroolBursts.IsTakenBy(head._room.Random.Read()) ? MotherBrainHeadSegment.DyingDroolBurst : null;
                default:
                    throw new InvalidOperationException($"Undefined Mother Brain head cue {cue}.");
            }
        }

        /// <inheritdoc/>
        public MotherBrainLateFightHeadSegment? RunLateFightCue(MotherBrainLateFightState lateFight, MotherBrainBrainCue<MotherBrainLateFightCueKind> cue)
        {
            switch (cue)
            {
                case MotherBrainBrainCue<MotherBrainLateFightCueKind>.Shared shared:
                    head.ApplyMotherBrainHeadCue(lateFight.Encounter, samus, shared.Cue);
                    return null;
                case MotherBrainBrainCue<MotherBrainLateFightCueKind>.Own { Kind: MotherBrainLateFightCueKind.ResetBabyMetroidAttacks }:
                    lateFight.ResetBabyMetroidAttacks();
                    return null;
                case MotherBrainBrainCue<MotherBrainLateFightCueKind>.Own { Kind: MotherBrainLateFightCueKind.QueueBabyMetroidAttackSound }:
                    head.QueueBabyMetroidAttackCry(lateFight);
                    return null;
                case MotherBrainBrainCue<MotherBrainLateFightCueKind>.Own { Kind: MotherBrainLateFightCueKind.MaybeHoldNeutralPhase3 }:
                    return head.HoldsNeutralPhase3Again() ? MotherBrainLateFightHeadSegment.NeutralPhase3Hold : null;
                default:
                    throw new InvalidOperationException($"Undefined Mother Brain late-fight cue {cue}.");
            }
        }

        /// <inheritdoc/>
        public MotherBrainBabyAttackSegment? RunBabyAttackCue(BabyMetroidCutsceneState baby, MotherBrainBrainCue<MotherBrainBabyAttackCueKind> cue)
        {
            MotherBrainLateFightState lateFight = baby.LateFight.State;
            switch (cue)
            {
                case MotherBrainBrainCue<MotherBrainBabyAttackCueKind>.Shared shared:
                    head.ApplyMotherBrainHeadCue(lateFight.Encounter, samus, shared.Cue);
                    return null;
                case MotherBrainBrainCue<MotherBrainBabyAttackCueKind>.Own { Kind: MotherBrainBabyAttackCueKind.CountBabyMetroidAttack }:
                    lateFight.CountBabyMetroidAttack();
                    return null;
                case MotherBrainBrainCue<MotherBrainBabyAttackCueKind>.Own { Kind: MotherBrainBabyAttackCueKind.AimOnionRingsAtBabyMetroid }:
                    AimMotherBrainOnionRingsAt(lateFight.Encounter, baby.Position.Whole);
                    return null;
                case MotherBrainBrainCue<MotherBrainBabyAttackCueKind>.Own { Kind: MotherBrainBabyAttackCueKind.QueueBabyMetroidAttackSound }:
                    head.QueueBabyMetroidAttackCry(lateFight);
                    return null;
                case MotherBrainBrainCue<MotherBrainBabyAttackCueKind>.Own { Kind: MotherBrainBabyAttackCueKind.MaybeHoldNeutralPhase3 }:
                    return head.HoldsNeutralPhase3Again() ? MotherBrainBabyAttackSegment.NeutralPhase3Hold : null;
                default:
                    throw new InvalidOperationException($"Undefined Mother Brain Baby-attack cue {cue}.");
            }
        }
    }

    /// <summary>Takes the phase-three neutral hold's chance, which alone decides whether the pose holds again.</summary>
    private bool HoldsNeutralPhase3Again() => MotherBrainRandomDefinitions.NeutralPhase3Holds.IsTakenBy(_room.Random.Read());

    /// <summary>Queues her attack cry, which the attack cry step skips once <paramref name="lateFight"/>'s attack counter reaches eleven.</summary>
    private void QueueBabyMetroidAttackCry(MotherBrainLateFightState lateFight)
    {
        if (lateFight.BabyMetroidAttackCounter != MotherBrainFightDefinitions.SilentBabyAttackCount)
            _audio.QueueEnemySound(MotherBrainSoundDefinitions.AttackCry, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Carries out a cue every brain animation raises to the same effect on <paramref name="state"/>.</summary>
    /// <remarks>
    /// The brain animation advances in her brain's draw, after the enemy update has queued the frame's enemy
    /// sounds, so its sounds go straight to the enemy audio queue at their limits (#1391).
    /// </remarks>
    private void ApplyMotherBrainHeadCue(MotherBrainEnemyState state, SamusState samus, MotherBrainHeadCue cue)
    {
        switch (cue)
        {
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.ShakeBrain }:
                state.BrainShake.ShakeFor(MotherBrainFightDefinitions.BrainShakeUpdates);
                return;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.SpawnDrool }:
                SpawnMotherBrainDrool(state);
                return;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.SpawnBigPurpleBreath }:
                SpawnMotherBrainPurpleBreathBig(state);
                return;
            case MotherBrainHeadCue.PlaySound playSound:
                _audio.QueueEnemySound(playSound.Effect, priority: SoundRequestPriority.Normal);
                return;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.DisableNeckMovement }:
                state.NeckMovementEnabled = false;
                return;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.EnableNeckMovement }:
                state.NeckMovementEnabled = true;
                return;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.AimOnionRingsAtSamus }:
                AimMotherBrainOnionRingsAt(state, samus.Position.Whole);
                return;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.SpawnOnionRing }:
                SpawnMotherBrainOnionRing(state, state.OnionRingsTargetAngle);
                return;
            case MotherBrainHeadCue.SpawnBomb bomb:
                SpawnMotherBrainBomb(state, bomb.Afterburns);
                return;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.SpawnLaser }:
                SpawnMotherBrainMouthLaser(state);
                return;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.PrepareRainbowBeamCharge }:
                state.SmallPurpleBreathGenerationEnabled = false;
                _audio.QueueEnemySound(MotherBrainSoundDefinitions.RainbowBeamCharge, priority: SoundRequestPriority.Normal);
                return;
            case MotherBrainHeadCue.Signal { Kind: MotherBrainHeadCueKind.SpawnRainbowBeamChargeParticle }:
                SpawnMotherBrainRainbowChargingProjectile(state);
                return;
            default:
                throw new InvalidOperationException($"Undefined Mother Brain head cue {cue}.");
        }
    }

    /// <summary>Fires the mouth laser, freezing the neck.</summary>
    private void SpawnMotherBrainMouthLaser(MotherBrainEnemyState state)
    {
        RoomEnemyPlace head = state.Head;
        state.NeckMovementEnabled = false;

        // The laser leaves her mouth, right of and below her head, travelling right. It is the head's
        // shot, so it deals Mother Brain's contact damage at her laser's speed.
        EnemyProjectile? laser = _roomActors.SpawnPirateLaser(
            head,
            head.Position.Whole + MotherBrainFightDefinitions.BeamOffset,
            direction: HorizontalDirection.Right);
        if (laser is not null)
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.PirateMotherBrainLaserSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>
    /// Aims the onion rings at a target. Both target selectors subtract the same mouth offsets,
    /// take the angle from those offsets, rotate by a half turn, and clamp the launch arc to
    /// <see cref="MotherBrainOnionRingDefinitions.LowestAim"/> through <see cref="MotherBrainOnionRingDefinitions.HighestAim"/>; only the source coordinate differs.
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
        var behavior = _projectileAttacks.Bomb(new MotherBrainBombState(state, MotherBrainBombBounceDefinitions.BounceSpeed, afterburnCount));
        EnemyProjectile? bomb = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.MotherBrainBomb, behavior, SpriteGraphics.SharedIn(state.Colours.OwnShading));
        if (bomb is null)
            return;

        bomb.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectSegment.Bomb, behavior));
        RoomEnemyPlace head = state.Head;
        bomb.Position = bomb.Position.WithWhole(head.Position.Whole + MotherBrainFightDefinitions.BombOffset);
        bomb.Position = bomb.Position.WithoutFractions();
        bomb.Velocity = new(MotherBrainBombBounceDefinitions.LaunchSpeedX, MotherBrainBombBounceDefinitions.LaunchSpeedY);
        state.RecordBombSpawned();
    }

    /// <summary>
    /// Runs the head's onion-ring step and the ring's setup. The ring takes its enemy-projectile place after the twelve
    /// room turrets and all lingering drool have theirs, so no ring appears when every place is held.
    /// </summary>
    internal void SpawnMotherBrainOnionRing(
        MotherBrainEnemyState state,
        TrigonometryAngle angle)
    {
        var behavior = _projectileAttacks.OnionRing(new MotherBrainOnionRingState(state, MotherBrainOnionRingDefinitions.MouthWaitUpdates));
        EnemyProjectile? ring = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.MotherBrainOnionRing, behavior, SpriteGraphics.SharedIn(state.Colours.OwnShading));
        if (ring is null)
            return;

        ring.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectSegment.OnionRing, behavior));
        ring.Velocity = ring.Velocity.WithX(EnemyMath.SineSpeed(MotherBrainOnionRingDefinitions.Speed, angle));
        ring.Velocity = ring.Velocity.WithY(EnemyMath.SineSpeed(
            MotherBrainOnionRingDefinitions.Speed,
            angle.TurnedBy(TrigonometryAngle.QuarterTurn)));

        // The ring's setup pins it to the mouth in the same update, before the first of its eight delayed attachment
        // frames.
        MotherBrainProjectiles.PinMotherBrainOnionRingToMouth(ring, state);
    }

    /// <summary>Runs the head's drool step and the drool's setup.</summary>
    internal void SpawnMotherBrainDrool(MotherBrainEnemyState state)
    {
        if (!state.DroolGenerationEnabled)
            return;

        Offset16<RoomPixels> mouth = state.NextDroolMouthPoint.Entry;
        state.NextDroolMouthPoint = state.NextDroolMouthPoint.NextInCycle;

        var behavior = _projectileAttacks.Drool(droolState);
        EnemyProjectile? drool = _projectiles.AllocateEnemyProjectile(kind, behavior, SpriteGraphics.SharedStandard);
        if (drool is null)
            return;

        // The game itself picks the drool by how fast her neck turns (#2471), and no phase stands behind that choice: her
        // phase-two laser's quick aim and steady fire, her fake death's landing, her third phase's bob and her late fight's
        // fast throws all drool the dying drool, her slow sways and rises the ordinary one. Only the neck's turn tells them apart, so it stays the
        // decision.
        RoomEnemyProjectileKind kind = state.NeckTurnPerUpdate < MotherBrainNeckTurnDefinitions.DyingDrool
            ? RoomEnemyProjectileKind.MotherBrainDrool
            : RoomEnemyProjectileKind.MotherBrainDyingDrool;
        var droolState = new MotherBrainDroolState(state, mouth);
        drool.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectSegment.Drool, behavior));

        // Spawning the drool attaches it to the head at once, so its position is already
        // right before the later projectile pass repeats the same attachment.
        MotherBrainProjectiles.MotherBrainProjectileBehavior.HoldMotherBrainDroolAtMouth(drool, droolState);
    }

    /// <summary>Runs the head's big purple breath step and the breath's setup.</summary>
    private void SpawnMotherBrainPurpleBreathBig(MotherBrainEnemyState state)
    {
        var behavior = _projectileAttacks.WithEncounter(new MotherBrainProjectileState(state));
        EnemyProjectile? breath = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.MotherBrainPurpleBreathBig, behavior, SpriteGraphics.SharedStandard);
        if (breath is null)
            return;

        breath.StartAnimation(new MotherBrainBreathAnimation(MotherBrainBreathSegment.PurpleBreathBig, behavior));
        RoomEnemyPlace head = state.Head;
        breath.Position = breath.Position.WithWhole(head.Position.Whole + MotherBrainFightDefinitions.BreathOffset);
    }

    /// <summary>Runs the small-breath spawn and the breath's setup.</summary>
    private void SpawnMotherBrainPurpleBreathSmall(MotherBrainEnemyState state)
    {
        var behavior = _projectileAttacks.WithEncounter(new MotherBrainProjectileState(state));
        EnemyProjectile? breath = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall, behavior, SpriteGraphics.SharedStandard);
        if (breath is null)
            return;

        breath.StartAnimation(new MotherBrainBreathAnimation(MotherBrainBreathSegment.PurpleBreathSmall, behavior));
        RoomEnemyPlace head = state.Head;
        breath.Position = breath.Position.WithWhole(head.Position.Whole + MotherBrainFightDefinitions.BreathOffset);
        state.SmallPurpleBreathActive = true;
    }

    /// <summary>Spawns the rainbow beam's charge glow, pinned to her head in the update it spawns.</summary>
    internal void SpawnMotherBrainRainbowChargingProjectile(MotherBrainEnemyState state)
    {
        var behavior = _projectileAttacks.RainbowCharge(encounter);
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging, behavior, SpriteGraphics.SharedStandard);
        if (projectile is null)
            return;

        var encounter = new MotherBrainProjectileState(state);
        projectile.StartAnimation(new MotherBrainEffectAnimation(MotherBrainEffectSegment.RainbowBeamCharging, behavior));
        projectile.Velocity = PixelVelocity<RoomPixels>.Zero;
        projectile.Position = projectile.Position.WithoutFractions();
        MotherBrainProjectiles.MotherBrainProjectileBehavior.PinMotherBrainRainbowCharge(projectile, encounter);
    }
}
