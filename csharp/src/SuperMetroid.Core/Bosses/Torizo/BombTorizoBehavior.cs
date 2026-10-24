using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The Bomb Torizo species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class BombTorizoBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. BombTorizoPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new BombTorizoEntity(this, slot, common);

    /// <summary>Supplies the live room services, boss flag, random state, and artwork for both statue variants.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the eighteen-slot enemy projectile pool used by Torizo attacks.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Queues the statue-break sound with the enemy audio limits.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Exposes the two live CGRAM rows occupied by Torizo's body.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Provides the NMI counter used by Torizo instruction choices.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Spawns the orb drops and the death scatter after the authored sequences.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Bomb and Golden Torizo's body: its behaviour flags, palette, program start and the low-health drool it triggers.</summary>
    internal TorizoBody Body { get; }

    /// <summary>Bomb and Golden Torizo's setup and movement: walking, jumps, falls and air transitions under gravity.</summary>
    internal TorizoMovement Movement { get; }

    /// <summary>Bomb and Golden Torizo's state lookups, facing, posture and the function word their branches test.</summary>
    internal TorizoEncounterAccess Access { get; }

    /// <summary>Bomb and Golden Torizo's projectiles: drool, swipes, sonic booms, Chozo orbs, eggs, super missiles, eye beams and explosions.</summary>
    internal TorizoAttacks Attacks { get; }

    /// <summary>The branches Bomb and Golden Torizo's programs take on Samus's position, and their walk steps.</summary>
    internal TorizoChoices Choices { get; }

    /// <summary>Bomb and Golden Torizo's hitboxes, touch and hurt, and what Samus's weapons do to them.</summary>
    internal TorizoCombat Combat { get; }
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;



    /// <summary>Connects the Bomb and Golden Torizo callbacks to their room-scoped services.</summary>
    internal BombTorizoBehavior(BombTorizoBehaviorDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _audio = dependencies.Audio;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        Access = new TorizoEncounterAccess();
        Attacks = new TorizoAttacks(_room, _projectiles, _audio, _deathsAndDrops, Access);
        Body = new TorizoBody(_room, _presentation, Access, Attacks);
        Movement = new TorizoMovement(_room, dependencies.Terrain, dependencies.Earthquake, _presentation, Body, Access);
        Choices = new TorizoChoices(_room, dependencies.Terrain, Access);
        Combat = new TorizoCombat(_room, _presentation, dependencies.Collision, Body, Access);
    }
    /// <summary>Health below which the head-break list interrupts active Bomb Torizo AI.</summary>
    internal const ushort BombTorizoHeadExplosionHealth = 350;
    /// <summary>Health below which the later core-break list interrupts active AI.</summary>
    internal const ushort BombTorizoCoreExplosionHealth = 100;

    /// <summary>Last delayed music request emitted by awakening/death bytecode.</summary>
    public DelayedMusicRequest? LastBombTorizoMusicRequest { get; private set; }

    /// <summary>Creates the per-statue gradual color transition against the live CGRAM image on first use.</summary>
    private CartridgePaletteTransition GetTorizoPaletteTransition(TorizoEnemyState state) =>
        state.PaletteTransition ??= new CartridgePaletteTransition(
            new ColourImage(TorizoBodyColours), TorizoPaletteDefinitions.FadeDenominator, _presentation.GradualColorChange);

    /// <summary>The body and belly colours the statue's fades change.</summary>
    private ColourRun TorizoBodyColours => _presentation.Cgram.SpriteSets.Sets(1, 2);

    /// <summary>Sets target colors beginning at the first body entry for the authored palette fade.</summary>
    private void SetTorizoPaletteTarget(TorizoEnemyState state, ReadOnlySpan<Bgr555> colors)
    {
        colors.CopyTo(GetTorizoPaletteTransition(state).Target.Of(TorizoBodyColours));
    }

    /// <summary>Ports <c>Torizo_Main</c> and its <c>toriz_var_E</c> dispatcher.</summary>
    private void RunBombTorizoMain(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        switch (state.Function)
        {
            case TorizoFunction.Idle:
                return;

            case TorizoFunction.Falling:
                Body.MaybeSpawnBombTorizoLowHealthDrool(torizo);
                Movement.ApplyBombTorizoGravity(torizo, state, TorizoMovement.RequireBombTorizoLevel(level));
                return;

            case TorizoFunction.WaitForHandTrigger:
                // $AA:C6C6 scans all forty PLM headers for $D6EA. A supplied PLM owner is
                // authoritative; standalone enemy audits omit it and therefore model the
                // already-collected item/removed hand trigger.
                torizo.Properties = torizo.Properties.With(EnemyProperties.SolidToSamus);
                if (_room.Services.Plms.HasActiveHeader(PlmHeaderId.BombTorizoHand))
                    return;

                LastBombTorizoMusicRequest = new DelayedMusicRequest(
                    MusicCommand.SelectTrack(6),
                    MusicCommandDelay.EightFrames);
                torizo.Properties = torizo.Properties.Without(EnemyProperties.SolidToSamus);
                TorizoBody.StartTorizoBodyProgram(torizo, state, AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoAnimations.AfterSleep(state.BodyProgram.Program)));
                state.AwakeningReleased = true;
                return;

            case TorizoFunction.Active:
                Movement.RunBombTorizoActiveState(torizo, state, level);
                return;

            case TorizoFunction.GoldenWaitForSamus:
                // $AA:D5C2 advances the list operand only after Samus has entered the
                // lower-right trigger rectangle. This is the authored statue wake-up gate.
                if (samus is not null && samus.Position.Y.Whole > 320 && samus.Position.X.Whole > 368)
                {
                    TorizoBody.StartTorizoBodyProgram(torizo, state, AnimationProgramCursor<TorizoBodyProgram>.Start(TorizoAnimations.AfterSleep(state.BodyProgram.Program)));
                }
                return;

            case TorizoFunction.GoldenGravity:
                Body.MaybeSpawnBombTorizoLowHealthDrool(torizo);
                Movement.ApplyBombTorizoGravity(torizo, state, TorizoMovement.RequireBombTorizoLevel(level));
                return;

            case TorizoFunction.GoldenPreInstruction:
                Body.MaybeSpawnBombTorizoLowHealthDrool(torizo);
                Movement.RunBombTorizoPreInstruction(torizo, state, level);
                return;

            default:
                throw new InvalidDataException(
                    $"Torizo main function $AA:{(int)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Counts consecutive Samus space-jump poses before running the shared statue dispatcher.</summary>
    private void RunGoldenTorizoMain(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        // GoldTorizo_Main counts consecutive space-jump poses for its anti-stall leap
        // decision, then calls the same toriz_var_E dispatcher as Bomb Torizo.
        if (samus?.Pose is SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose)
            state.CountSamusSpaceJumpFrame();
        else
            state.SamusSpaceJumpFrames = 0;
        RunBombTorizoMain(torizo, state, samus, level);
    }

    /// <summary>Records the area boss bit, scatters sixteen drops in the variant's arena, and requests post-fight music.</summary>
    private void FinishBombTorizoDeath(TorizoEnemyState state)
    {
        _room.Services.Bosses.SetDefeated(BossBits.AreaTorizo);
        state.BossBitSet = true;

        // The two statues share header $EEFF's chance table but use different native
        // arena rectangles: Bomb Torizo uses the 128x64 upper room region, while Golden
        // Torizo uses a 256x64 strip much lower in its room.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.BombTorizo,
            count: 16,
            xBase: state.IsGolden ? (ushort)128 : (ushort)64,
            xMask: state.IsGolden ? TorizoRandomDefinitions.GoldenDropXMask : TorizoRandomDefinitions.BombDropXMask,
            yBase: state.IsGolden ? (ushort)288 : (ushort)96,
            yMask: TorizoRandomDefinitions.DropYMask);
        state.ItemDropRequested = true;
        LastBombTorizoMusicRequest = new DelayedMusicRequest(
            MusicCommand.SelectTrack(3),
            MusicCommandDelay.EightFrames);
    }

    /// <summary>Replaces Torizo's posture bits, keeping parameter 1's low thirteen bits.</summary>
    private void SetPosture(RoomEnemySlot torizo, TorizoPostureFlags posture) =>
        Access.RequireBombTorizoState(torizo).Posture = posture;

    /// <summary>Clears <paramref name="flags"/>.</summary>
    private void ClearBehavior(RoomEnemySlot torizo, TorizoBehaviorFlags flags) => Access.RequireBombTorizoState(torizo).Behavior &= ~flags;

    /// <summary>Queues <paramref name="sound"/> through the shared enemy queue.</summary>
    private void PlayTorizoSound(SoundEffectId sound)
    {
        _audio.QueueEnemySound(sound, maximumQueued: 6);
    }

    /// <summary>Ports BombTorizo's main AI.</summary>
    internal void RunBombTorizoMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunBombTorizoMain(slot, Access.RequireBombTorizoState(slot), frame.Samus, frame.Level);
    }

    /// <summary>Ports GoldenTorizo's main AI.</summary>
    internal void RunGoldenTorizoMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunGoldenTorizoMain(
            slot,
            Access.RequireBombTorizoState(slot),
            frame.Samus,
            frame.Level);
    }

    /// <summary>Advances BombTorizo's typed animation.</summary>
    internal void AdvanceBombTorizoTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context)
    {
        AdvanceTorizoAnimation(slot, new TorizoInstructionInputs { Samus = context.Samus, Level = context.Level, ControllerInput = context.ControllerInput, FrameCounterLow = unchecked((byte)_frameContext.NmiFrameCounter) });
    }
    /// <summary>Runs Bomb or Golden Torizo's body program where the enemy instruction interpreter would.</summary>
    internal void AdvanceTorizoAnimation(RoomEnemySlot torizo, TorizoInstructionInputs inputs)
    {
        if (!EnemyProgramAnimation.Expire(torizo))
            return;
        TorizoEnemyState state = Access.RequireBombTorizoState(torizo);
        AnimationProgramCursor<TorizoBodyProgram> cursor = state.BodyProgram;
        // The death and wake repeats count the body's timer word, as the native timer commands do.
        EnemyProgramAnimation.RunTo(torizo, TorizoAnimations.Programs, ref cursor, TorizoFrameDefinitions.ShownFrames.Of,
            (cue, continuation) => ApplyTorizoCue(torizo, state, inputs, cue, continuation));
        state.BodyProgram = cursor;
    }

    /// <summary>
    /// Shows artwork swap <paramref name="swap"/> under Torizo's poses at this update (#1406): from now on the frames drawn
    /// over its characters picture it, over every swap shown there before.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed boss artwork has no Torizo artwork swaps.</exception>
    private void ShowTorizoArtwork(TorizoArtworkSwap swap)
    {
        _ = _bossArtwork.Artwork?.TorizoSwaps ?? throw new InvalidDataException(
            "Torizo's artwork swaps require installed artwork.");
        _presentation.ObjCharacters.Record(TorizoArtworkSwapDefinitions.Uploads[swap.Ordinal]);
    }

    /// <summary>
    /// Carries out one cue of Bomb or Golden Torizo's body program, returning the cursor a branch, call or return
    /// continues at, or null to run on. <paramref name="continuation"/> is the cursor just past the cue: the return
    /// point a call saves. <see cref="TorizoInstructionInputs.FrameCounterLow"/> is the low byte of
    /// <c>NMI_FrameCounter</c> ($05B6), which both attack choosers ($AA:C5B9, $AA:D53B) add; only its bit 3 survives.
    /// </summary>
    internal AnimationProgramCursor<TorizoBodyProgram>? ApplyTorizoCue(
        RoomEnemySlot torizo,
        TorizoEnemyState state,
        TorizoInstructionInputs inputs,
        TorizoCue cue,
        AnimationProgramCursor<TorizoBodyProgram> continuation)
    {
        switch (cue)
        {
            case TorizoCue.SetPreInstruction setPreInstruction:
                state.PreInstruction = setPreInstruction.PreInstruction;
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.ClearPreInstruction }:
                state.PreInstruction = TorizoPreInstruction.Idle;
                return null;

            case TorizoCue.SetFunction setFunction:
                state.Function = setFunction.Function;
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.BlowUpGut }:
                Combat.SetBehavior(torizo, TorizoBehaviorFlags.GutBlownUp);
                for (int piece = 0; piece < 6; piece++)
                    Attacks.SpawnBombTorizoLowHealthDrool(torizo);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.MarkFaceBlownUp }:
                Combat.SetBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.MakeVisible }:
                torizo.Properties = torizo.Properties.Without(EnemyProperties.Invisible);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.MakeInvisible }:
                torizo.Properties = torizo.Properties.With(EnemyProperties.Invisible);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.FadeToBlack }:
                SetTorizoPaletteTarget(state, new Bgr555[32]);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.FinishDeath }:
                FinishBombTorizoDeath(state);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.AdvancePaletteFade }:
                GetTorizoPaletteTransition(state).Step();
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.FadeToNormalColors }:
                SetTorizoPaletteTarget(state, TorizoPaletteDefinitions.Normal);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.StartBombTorizoFight }:
                LastBombTorizoMusicRequest = new DelayedMusicRequest(
                    MusicCommand.SelectTrack(5),
                    MusicCommandDelay.EightFrames);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.NoOperation }:
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.LockAnimation }:
                state.AnimationLocked = true;
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.UnlockAnimation }:
                state.AnimationLocked = false;
                return null;

            case TorizoCue.Choice { Kind: TorizoChoiceKind.GoToIfFaceBlownUpElseIfGolden } choice:
            {
                if (Body.HasBehavior(torizo, TorizoBehaviorFlags.FaceBlownUp))
                    return AnimationProgramCursor<TorizoBodyProgram>.Start(choice.Target);
                // The Bomb variant runs on past both targets.
                return state.IsGolden ? AnimationProgramCursor<TorizoBodyProgram>.Start(choice.Alternate) : null;
            }

            case TorizoCue.Branch { Kind: TorizoBranchKind.Link } branch:
                state.ReturnCursor = AnimationProgramCursor<TorizoBodyProgram>.Start(branch.Target);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.Return }:
                return state.ReturnCursor;

            case TorizoCue.Command { Kind: TorizoCueKind.ResumeInterrupted }:
                return state.InterruptedCursor;

            case TorizoCue.ParameterizedSpawn { Kind: TorizoParameterizedSpawnKind.SpawnLowHealthExplosions } parameterizedSpawn:
                for (int explosion = 0; explosion < 6; explosion++)
                    Attacks.SpawnBombTorizoLowHealthExplosion(torizo, parameterizedSpawn.Parameter);
                // The program then waits forty updates, as the native callback's sleep does.
                torizo.FlashTimer = WrappingCountdown.Start(40);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.SpawnDeathExplosion }:
                Attacks.SpawnBombTorizoDeathExplosion(torizo);
                // The program then waits one update, as the native callback's sleep does.
                torizo.FlashTimer = WrappingCountdown.Start(1);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.SpawnLandingDust }:
                Attacks.SpawnBombTorizoLandingDust(torizo, foot: HorizontalDirection.Right);
                Attacks.SpawnBombTorizoLandingDust(torizo, foot: HorizontalDirection.Left);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.SpawnLowHealthDrool }:
                if (torizo.Health < BombTorizoHeadExplosionHealth)
                    Body.SpawnBombTorizoInitialDrool(torizo);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.SetTurningAround }:
                SetPosture(torizo, Access.Posture(torizo) | TorizoPostureFlags.TurningAround);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.SteppedLeftWithLeftFoot }:
                SetPosture(torizo, TorizoPostureFlags.None);
                state.CountDecisionStep();
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.SteppedRightWithRightFoot }:
                SetPosture(torizo, TorizoPostureFlags.FacingRight);
                state.CountDecisionStep();
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.SteppedLeftWithRightFoot }:
                SetPosture(torizo, TorizoPostureFlags.OppositeLegForward);
                state.CountDecisionStep();
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.SteppedRightWithLeftFoot }:
                SetPosture(torizo, TorizoPostureFlags.FacingRight | TorizoPostureFlags.OppositeLegForward);
                state.CountDecisionStep();
                return null;

            case TorizoCue.Posture { Kind: TorizoPostureKind.StandUp } posture:
                ApplyBombTorizoMapOffset(torizo, posture.MapOffset, move: BombTorizoPostureMove.StandUp);
                return null;

            case TorizoCue.Posture { Kind: TorizoPostureKind.SitDown } posture:
                ApplyBombTorizoMapOffset(torizo, posture.MapOffset, move: BombTorizoPostureMove.SitDown);
                return null;

            case TorizoCue.Walk { Kind: TorizoWalkKind.WalkBombTorizo } walk:
                return Choices.ProcessBombTorizoWalkInstruction(torizo, state, inputs.Samus, TorizoMovement.RequireBombTorizoLevel(inputs.Level), new TorizoWalkStep { TableOffset = walk.TableOffset, CollisionFacingRight = TorizoBodyProgram.FacingLeftTurningLeft, CollisionFacingLeft = TorizoBodyProgram.FacingRightTurningRight });

            case TorizoCue.Walk { Kind: TorizoWalkKind.WalkFacelessBombTorizo } walk:
                return Choices.ProcessBombTorizoWalkInstruction(torizo, state, inputs.Samus, TorizoMovement.RequireBombTorizoLevel(inputs.Level), new TorizoWalkStep { TableOffset = walk.TableOffset, CollisionFacingRight = TorizoBodyProgram.FacingLeftFacelessTurningLeft, CollisionFacingLeft = TorizoBodyProgram.FacingRightFacelessTurningRight });

            case TorizoCue.Branch { Kind: TorizoBranchKind.GoToIfRising } branch:
                return state.Velocity.Y.SignedRawValue < 0 ? AnimationProgramCursor<TorizoBodyProgram>.Start(branch.Target) : null;

            case TorizoCue.Branch { Kind: TorizoBranchKind.CallIfSamusCloseBehind } branch:
                return Choices.SelectBombTorizoCloseBehindBranch(
                    torizo,
                    state,
                    inputs.Samus,
                    continuation,
                    branch.Target);

            case TorizoCue.Branch { Kind: TorizoBranchKind.JumpBackIfSamusCloseInFront } branch:
                return Choices.SelectBombTorizoCloseFrontJump(
                    torizo,
                    state,
                    inputs.Samus,
                    branch.Target);

            case TorizoCue.Choice { Kind: TorizoChoiceKind.CallBombTorizoAttack } choice:
            {
                if (inputs.Samus is null)
                    throw new InvalidOperationException("Bomb Torizo attack selection requires Samus state.");
                state.ReturnCursor = continuation;
                bool chooseFirst = inputs.Samus.Missiles < 5 ||
                    ((inputs.FrameCounterLow + (inputs.Samus.Position.X.Whole & 1) + (inputs.Samus.Position.X.Whole >> 1)) & 8) != 0;
                return AnimationProgramCursor<TorizoBodyProgram>.Start(chooseFirst ? choice.Target : choice.Alternate);
            }

            case TorizoCue.Command { Kind: TorizoCueKind.SpawnBombTorizoChozoOrbs }:
                Attacks.SpawnBombTorizoChozoOrb(torizo);
                Attacks.SpawnBombTorizoChozoOrb(torizo);
                Attacks.SpawnBombTorizoChozoOrb(torizo);
                return null;

            case TorizoCue.ParameterizedSpawn { Kind: TorizoParameterizedSpawnKind.SpawnBombTorizoSonicBoom } parameterizedSpawn:
                Attacks.SpawnBombTorizoSonicBoom(torizo, parameterizedSpawn.Parameter);
                return null;

            case TorizoCue.ParameterizedSpawn { Kind: TorizoParameterizedSpawnKind.SpawnGoldenTorizoSonicBoom } parameterizedSpawn:
                Attacks.SpawnGoldenTorizoSonicBoom(torizo, parameterizedSpawn.Parameter);
                return null;

            case TorizoCue.ParameterizedSpawn { Kind: TorizoParameterizedSpawnKind.SpawnExplosiveSwipe } parameterizedSpawn:
                Attacks.SpawnBombTorizoExplosiveSwipe(torizo, parameterizedSpawn.Parameter);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.PlayShotSound }:
                PlayTorizoSound(SoundEffectLibrary2Sounds.ShotTorizo);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.PlayFootstepSound }:
                PlayTorizoSound(SoundEffectLibrary2Sounds.TorizoFootsteps);
                return null;

            case TorizoCue.Branch { Kind: TorizoBranchKind.GoToIfAirborne } branch:
                return torizo.Position.Y.Whole == 375 ? null : AnimationProgramCursor<TorizoBodyProgram>.Start(branch.Target);

            case TorizoCue.Command { Kind: TorizoCueKind.LoadGoldenTorizoPalettes }:
                SetTorizoPaletteTarget(state, TorizoPaletteDefinitions.Golden);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.StartGoldenTorizoFight }:
                LastBombTorizoMusicRequest = new DelayedMusicRequest(
                    MusicCommand.SelectTrack(5),
                    MusicCommandDelay.EightFrames);
                torizo.Radius = new(18, 48);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.ClearCaughtSuperMissile }:
                ClearBehavior(torizo, TorizoBehaviorFlags.CaughtSuperMissile);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.SpawnEgg }:
                Attacks.SpawnGoldenTorizoEgg(torizo);
                return null;

            case TorizoCue.Branch { Kind: TorizoBranchKind.GoToIfEyeBeamActive } branch:
                return _projectiles.EnemyProjectiles.Any(projectile =>
                        projectile.Kind == RoomEnemyProjectileKind.GoldenTorizoEgg)
                    ? AnimationProgramCursor<TorizoBodyProgram>.Start(branch.Target)
                    : null;

            case TorizoCue.Command { Kind: TorizoCueKind.DisableEyeBeamExplosions }:
                state.EyeBeamExplosionsEnabled = false;
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.EnableEyeBeamExplosions }:
                state.EyeBeamExplosionsEnabled = true;
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.UnmarkStunned }:
                ClearBehavior(torizo, TorizoBehaviorFlags.Stunned);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.PlayEggReleasedSound }:
                PlayTorizoSound(SoundEffectLibrary2Sounds.GoldenTorizoEggReleased);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.PlayLaserSound }:
                PlayTorizoSound(SoundEffectLibrary2Sounds.GoldenTorizoLaser);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.PlaySonicBoomSound }:
                PlayTorizoSound(SoundEffectLibrary2Sounds.TorizoSonicBoom);
                return null;

            case TorizoCue.Command { Kind: TorizoCueKind.SpawnSuperMissile }:
                Attacks.SpawnGoldenTorizoSuperMissile(torizo);
                return null;

            case TorizoCue.Branch { Kind: TorizoBranchKind.GoToIfSamusMorphedBehind } branch:
                return Choices.SelectGoldenTorizoMorphBallBranch(
                    torizo,
                    state,
                    inputs.Samus,
                    branch.Target);

            case TorizoCue.ParameterizedSpawn { Kind: TorizoParameterizedSpawnKind.SpawnEyeBeam } parameterizedSpawn:
                Attacks.SpawnGoldenTorizoEyeBeam(torizo, parameterizedSpawn.Parameter);
                return null;

            case TorizoCue.Branch { Kind: TorizoBranchKind.CallSometimesIfSamusMorphedInFront } branch:
                return Choices.SelectGoldenTorizoMediumRangeBranch(
                    torizo,
                    state,
                    inputs.Samus,
                    continuation,
                    branch.Target);

            case TorizoCue.Branch { Kind: TorizoBranchKind.CallSometimesIfHealthLow } branch:
                if (torizo.Health > 1928 || (_room.Random.Generate() & TorizoRandomDefinitions.LowHealthCallMask) != 0)
                    return null;
                state.DecisionCounter = 0;
                state.ReturnCursor = continuation;
                return AnimationProgramCursor<TorizoBodyProgram>.Start(branch.Target);

            case TorizoCue.Branch { Kind: TorizoBranchKind.CallIfStunHealthHigh } branch:
                if (torizo.Health <= 10800 ||
                    !Body.HasBehavior(torizo, TorizoBehaviorFlags.Stunned))
                    return null;
                state.ReturnCursor = continuation;
                return AnimationProgramCursor<TorizoBodyProgram>.Start(branch.Target);

            case TorizoCue.Branch { Kind: TorizoBranchKind.JumpForwardIfSamusFar } branch:
                return Choices.SelectGoldenTorizoSpaceJumpCounter(
                    torizo,
                    state,
                    inputs.Samus,
                    inputs.ControllerInput,
                    branch.Target);

            case TorizoCue.Command { Kind: TorizoCueKind.SpawnGoldenTorizoChozoOrbs }:
                Attacks.SpawnGoldenTorizoChozoOrb(torizo);
                return null;

            case TorizoCue.Branch { Kind: TorizoBranchKind.JumpBackIfSamusCloseInFrontGolden } branch:
                return Choices.SelectGoldenTorizoRepeatedJump(
                    torizo,
                    state,
                    inputs.Samus,
                    branch.Target);

            case TorizoCue.Choice { Kind: TorizoChoiceKind.CallGoldenTorizoAttack } choice:
            {
                SamusState activeSamus = RequireGoldenTorizoSamus(inputs.Samus);
                state.ReturnCursor = continuation;
                bool chooseFirst = activeSamus.Missiles < 32 ||
                    ((inputs.FrameCounterLow + (activeSamus.Position.X.Whole & 1) +
                        (activeSamus.Position.X.Whole >> 1)) & 8) != 0;
                return AnimationProgramCursor<TorizoBodyProgram>.Start(chooseFirst ? choice.Target : choice.Alternate);
            }

            case TorizoCue.Walk { Kind: TorizoWalkKind.WalkGoldenTorizo } walk:
                return Choices.ProcessGoldenTorizoWalkInstruction(
                    torizo,
                    state,
                    inputs.Samus,
                    TorizoMovement.RequireBombTorizoLevel(inputs.Level),
                    walk.TableOffset);

            case TorizoCue.ShowArtwork showArtwork:
                ShowTorizoArtwork(showArtwork.Swap);
                return null;

            default:
                throw new InvalidOperationException($"Undefined {nameof(TorizoCueKind)} {cue}.");
        }
    }

    /// <summary>Requires the active Samus actor for distance and attack decisions in Golden bytecode.</summary>
    private static SamusState RequireGoldenTorizoSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Golden Torizo instruction selection requires the active Samus actor.");

    /// <summary>Applies or reverses the posture offset indexed by a Bomb Torizo movement table operand.</summary>
    internal static void ApplyBombTorizoMapOffset(
        RoomEnemySlot torizo,
        int tableOffset,
        BombTorizoPostureMove move)
    {
        Offset16<RoomPixels> displacement = BombTorizoMovementDefinitions.Posture(tableOffset);
        torizo.Position += move == BombTorizoPostureMove.SitDown ? -displacement : displacement;
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastBombTorizoMusicRequest = null;
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        audio.QueueLegacyMusic(LastBombTorizoMusicRequest?.Command, LastBombTorizoMusicRequest?.Delay);
    }
}
