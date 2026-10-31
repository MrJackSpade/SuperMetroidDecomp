using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>The Alcoon species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class AlcoonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Alcoon];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new AlcoonEntity(this, slot, common);

    /// <summary>Provides RNG for the randomized post-volley walk count.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Resolves Alcoon's jump landing and its walking collisions.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Spawns the three directed fireball variants.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Receives the species' end-of-update sound requests.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects Alcoon's per-room AI to collision, RNG, and projectile services.</summary>
    internal AlcoonBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _terrain = terrain;
        _projectiles = projectiles;
    }

    /// <summary>
    /// Carries out one cue of the Alcoon's animations: spitting a fireball, starting to walk, or stepping. Starting to walk
    /// always continues at the walk for the Alcoon's facing, and a step that meets a wall turns it round to the other walk.
    /// </summary>
    private AlcoonProgram? ApplyAlcoonCue(RoomEnemySlot slot, AlcoonEnemyState state, RoomLevelData? level, AlcoonCue cue)
    {
        switch (cue.Kind)
        {
            case AlcoonCueKind.FireUpward:
                SpawnAlcoonFireball(slot, yVelocityTableByteOffset: 0);
                _audio.RequestSoundAtUpdateEnd(EnemySoundSource.Alcoon, EnemySoundEffectDefinitions.AlcoonFireSound, priority: SoundRequestPriority.Normal);
                return null;
            case AlcoonCueKind.FireHorizontally:
                SpawnAlcoonFireball(slot, yVelocityTableByteOffset: 2);
                _audio.RequestSoundAtUpdateEnd(EnemySoundSource.Alcoon, EnemySoundEffectDefinitions.AlcoonFireSound, priority: SoundRequestPriority.Normal);
                return null;
            case AlcoonCueKind.FireDownward:
                SpawnAlcoonFireball(slot, yVelocityTableByteOffset: 4);
                _audio.RequestSoundAtUpdateEnd(EnemySoundSource.Alcoon, EnemySoundEffectDefinitions.AlcoonFireSound, priority: SoundRequestPriority.Normal);
                return null;
            case AlcoonCueKind.StartWalking:
                return StartAlcoonWalking(state);
            case AlcoonCueKind.Step:
                if (!state.StepCounter.IsZero)
                    state.StepCounter = state.StepCounter.Decrement().Remaining;
                return MoveAlcoonHorizontally(slot, state, level);
            case AlcoonCueKind.Move:
                return MoveAlcoonHorizontally(slot, state, level);
            default:
                throw new InvalidOperationException($"Undefined {nameof(AlcoonCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Maximum horizontal distance for activation, after the landing-height check.</summary>
    private const int AlcoonEmergeXDistance = 80;
    /// <summary>Distance from spawn that causes an emerged Alcoon to jump back underground.</summary>
    private const int AlcoonHideXDistance = 112;
    /// <summary>Guards the initializer's simulated ascent and floor search against invalid room geometry.</summary>
    private const int AlcoonJumpSimulationLimit = 4096;

    /// <summary>Ports <c>InitAI_Alcoon</c> at <c>$A8:DCCD</c>, including its floor search.</summary>
    private void InitializeAlcoon(RoomEnemySlot slot, RoomLevelData? level)
    {
        if (level is null)
        {
            throw new InvalidOperationException(
                "Alcoon initialization requires the room's collision layer to simulate its landing.");
        }

        var state = new AlcoonEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        state.StepCounter = WrappingCountdown.Start(0);
        state.SpawnPosition = slot.Position.Whole;
        slot.Properties = slot.Properties.With(EnemyProperties.ProcessInstructions);
        state.Program = AnimationProgramCursor<AlcoonProgram>.Start(AlcoonProgram.WalkingLeft);
        state.Function = AlcoonEnemyFunction.WaitingForSamus;
        SetupAlcoonJumpMovement(state);

        // The original initializer really executes every half-pixel-accelerated ascent
        // step, then every collision-aware descent step. It ultimately restores only the
        // whole Y coordinate; the collision-produced subposition deliberately survives.
        int iterations = 0;
        do
        {
            AddAlcoonYVelocity(slot, state);
            AccelerateAlcoonY(state);
            GuardAlcoonJumpSimulation(++iterations, slot);
        }
        while (state.Velocity.Y.WholePixels < 0);

        while (true)
        {
            int displacement = state.Velocity.Y.RawValue;
            if (_terrain.MoveEnemyVertically(level, slot, displacement))
                break;
            AccelerateAlcoonY(state);
            GuardAlcoonJumpSimulation(++iterations, slot);
        }

        state.LandingPosition = slot.Position.Whole;
        slot.Position = slot.Position.WithWholeY(state.SpawnPosition.Y);
    }

    /// <summary>Ports <c>MainAI_Alcoon</c> and all seven indirect targets.</summary>
    private void RunAlcoonMain(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        switch (state.Function)
        {
            case AlcoonEnemyFunction.WaitingForSamus:
                WaitForAlcoonActivation(slot, state, samus);
                return;
            case AlcoonEnemyFunction.EmergingRising:
                RunAlcoonEmergingRise(slot, state);
                return;
            case AlcoonEnemyFunction.EmergingFalling:
                RunAlcoonEmergingFall(slot, state, samus, level);
                return;
            case AlcoonEnemyFunction.WalkingAndFiring:
                RunEmergedAlcoon(slot, state, samus, level);
                return;
            case AlcoonEnemyFunction.WaitingForFireAnimation:
                return;
            case AlcoonEnemyFunction.HidingRising:
                RunAlcoonHidingRise(slot, state);
                return;
            case AlcoonEnemyFunction.HidingFalling:
                RunAlcoonHidingFall(slot, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Alcoon function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Starts Alcoon's jump when Samus nears its predicted landing point.</summary>
    private void WaitForAlcoonActivation(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Alcoon proximity AI requires the active Samus actor.");

        if (CoordinateAxis.Distance(state.LandingPosition.Y, samus.Position.Y.Whole) >= 32)
            return;

        int signedXDistance = CoordinateAxis.Wrap(samus.Position.X.Whole - slot.Position.X.Whole);
        if (EnemyMath.WrappedMagnitude(signedXDistance) >= AlcoonEmergeXDistance)
            return;

        SetupAlcoonJumpMovement(state);
        if (CoordinateAxis.AsSigned(signedXDistance) < 0)
        {
            state.Velocity = state.Velocity.WithWholeX(-2);
            InstallAlcoonInstruction(slot, state, AlcoonProgram.AirborneLeftLookingUp);
        }
        else
        {
            state.Velocity = state.Velocity.WithWholeX(2);
            InstallAlcoonInstruction(slot, state, AlcoonProgram.AirborneRightLookingUp);
        }
        state.Function = AlcoonEnemyFunction.EmergingRising;
        _audio.RequestSoundAtUpdateEnd(EnemySoundSource.Alcoon, EnemySoundEffectDefinitions.AlcoonEmergeSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Integrates the emerging ascent until vertical velocity ceases to be negative.</summary>
    private static void RunAlcoonEmergingRise(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        AccelerateAlcoonY(state);
        if (state.Velocity.Y.WholePixels < 0)
            return;

        state.Function = AlcoonEnemyFunction.EmergingFalling;
        InstallAlcoonInstruction(
            slot,
            state,
            state.Velocity.X.WholePixels < 0
                ? AlcoonProgram.AirborneLeftLookingForward
                : AlcoonProgram.AirborneRightLookingForward);
    }

    /// <summary>Applies collision-aware descent, then chooses the initial walking direction toward Samus.</summary>
    private void RunAlcoonEmergingFall(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Alcoon landing direction requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Alcoon falling AI requires room collision data.");

        int displacement = state.Velocity.Y.RawValue;
        if (!_terrain.MoveEnemyVertically(level, slot, displacement))
        {
            AccelerateAlcoonY(state);
            return;
        }

        int signedXDistance = CoordinateAxis.Wrap(samus.Position.X.Whole - slot.Position.X.Whole);
        if (CoordinateAxis.AsSigned(signedXDistance) < 0)
        {
            state.Velocity = state.Velocity.WithWholeX(-2);
            InstallAlcoonInstruction(slot, state, AlcoonProgram.WalkingLeft);
        }
        else
        {
            state.Velocity = state.Velocity.WithWholeX(2);
            InstallAlcoonInstruction(slot, state, AlcoonProgram.WalkingRight);
        }
        state.Function = AlcoonEnemyFunction.WalkingAndFiring;
        state.StepCounter = WrappingCountdown.Start(1);
    }

    /// <summary>Maintains floor contact, starts a retreat at the spawn-distance limit, or begins a forward volley.</summary>
    private void RunEmergedAlcoon(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Alcoon firing AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Alcoon walking AI requires room collision data.");

        // This is a real two-pixel downward move, not merely a floor probe. On ordinary
        // floors it collides and keeps Alcoon aligned; if the floor vanishes it lets the
        // walking actor fall by the same amount before the remainder of main AI runs.
        _terrain.MoveEnemyVertically(level, slot, 2 << 16);

        int spawnDistance = CoordinateAxis.Wrap(state.SpawnPosition.X - slot.Position.X.Whole);
        if (EnemyMath.WrappedMagnitude(spawnDistance) >= AlcoonHideXDistance)
        {
            state.Function = AlcoonEnemyFunction.HidingRising;
            state.Velocity = state.Velocity.WithY(SignedSixteenSixteen.FromParts(-4, 0));
            InstallAlcoonInstruction(
                slot,
                state,
                state.Velocity.X.WholePixels < 0
                    ? AlcoonProgram.AirborneLeftLookingForward
                    : AlcoonProgram.AirborneRightLookingForward);
            return;
        }

        if (!state.StepCounter.IsZero)
            return;

        bool samusIsLeft = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0;
        bool movingLeft = state.Velocity.X.WholePixels < 0;
        if (samusIsLeft != movingLeft)
            return;

        InstallAlcoonInstruction(
            slot,
            state,
            movingLeft
                ? AlcoonProgram.FireLeft
                : AlcoonProgram.FireRight);
        state.Function = AlcoonEnemyFunction.WaitingForFireAnimation;
    }

    /// <summary>Starts the retreat arc upward until gravity reverses its Y velocity.</summary>
    private static void RunAlcoonHidingRise(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        AccelerateAlcoonY(state);
        if (state.Velocity.Y.WholePixels >= 0)
            state.Function = AlcoonEnemyFunction.HidingFalling;
    }

    /// <summary>Returns the retreating actor to its whole-pixel spawn position while retaining subpositions.</summary>
    private static void RunAlcoonHidingFall(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        AddAlcoonYVelocity(slot, state);
        if (CoordinateAxis.Difference(slot.Position.Y.Whole, state.SpawnPosition.Y) < 0)
        {
            AccelerateAlcoonY(state);
            return;
        }

        // Like the original, the reset snaps only whole coordinates. Both subpositions and
        // the last animation map remain live while the actor waits underground.
        slot.Position = slot.Position.WithWhole(state.SpawnPosition);
        state.Function = AlcoonEnemyFunction.WaitingForSamus;
    }

    /// <summary>Resets the jump to a twelve-pixel upward velocity and half-pixel acceleration.</summary>
    private static void SetupAlcoonJumpMovement(AlcoonEnemyState state)
    {
        state.Velocity = new FixedVector<RoomPixels>(
            SignedSixteenSixteen.Zero, SignedSixteenSixteen.FromParts(-12, 0));
        // The cartridge also clears a record word here that nothing reads.
        state.YAcceleration = SignedSixteenSixteen.FromRaw(SixteenSixteen.FromWhole(1) / 2);
    }

    /// <summary>Adds the per-slot 16.16 acceleration to Alcoon's vertical velocity.</summary>
    private static void AccelerateAlcoonY(AlcoonEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + state.YAcceleration);
    }

    /// <summary>Integrates vertical velocity without collision during airborne ascent or retreat.</summary>
    private static void AddAlcoonYVelocity(RoomEnemySlot slot, AlcoonEnemyState state)
    {
        slot.Position = slot.Position.AdvanceY(state.Velocity.Y);
    }

    /// <summary>Rejects an initializer that cannot find a landing floor within the simulation bound.</summary>
    private static void GuardAlcoonJumpSimulation(int iterations, RoomEnemySlot slot)
    {
        if (iterations > AlcoonJumpSimulationLimit)
        {
            throw new InvalidDataException(
                $"Alcoon slot {slot.SlotIndex} did not find a landing floor within " +
                $"{AlcoonJumpSimulationLimit} initialization steps.");
        }
    }

    /// <summary>Switches Alcoon's animation so its first step runs on the next update.</summary>
    private static void InstallAlcoonInstruction(RoomEnemySlot slot, AlcoonEnemyState state, AlcoonProgram program)
    {
        state.Program = AnimationProgramCursor<AlcoonProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>Implements Alcoon instruction <c>$A8:DF3F</c> and returns the walk the animation continues at.</summary>
    private AlcoonProgram StartAlcoonWalking(AlcoonEnemyState state)
    {
        state.Function = AlcoonEnemyFunction.WalkingAndFiring;
        Func<int> readRandomNumber = _room.Random.Read;
        int count = WordWrap.Unsigned(readRandomNumber() & 3);
        state.StepCounter = WrappingCountdown.FromRaw(count == 0 ? 2 : count);
        return state.Velocity.X.WholePixels < 0
            ? AlcoonProgram.WalkingLeft
            : AlcoonProgram.WalkingRight;
    }

    /// <summary>
    /// Implements both movement bytecodes at <c>$A8:DF63/$DF71</c>: the animation runs on unless a wall turns the Alcoon
    /// round, when it continues at the other walk's first pose.
    /// </summary>
    private AlcoonProgram? MoveAlcoonHorizontally(
        RoomEnemySlot slot,
        AlcoonEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Alcoon movement instruction requires room collision data.");

        int displacement = state.Velocity.X.RawValue;
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
        {
            EnemyTerrain.AlignEnemyYWithNonSquareSlope(level, slot);
            return null;
        }

        int oldVelocity = state.Velocity.X.WholePixels;
        state.Velocity = state.Velocity.WithWholeX(WordWrap.Signed(-oldVelocity));
        return oldVelocity >= 0
            ? AlcoonProgram.WalkingLeftLoop
            : AlcoonProgram.WalkingRightLoop;
    }

    /// <summary>Connects Alcoon fireballs to their bank-$86 movement pre-instruction.</summary>
    internal static void AddAlcoonProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            RunAlcoonFireballPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.AlcoonFireball);
    }

    /// <summary>Ports Alcoon's initialization AI.</summary>
    internal void RunAlcoonInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        InitializeAlcoon(slot, surroundings.Level);
    }

    /// <summary>Ports Alcoon's main AI.</summary>
    internal void RunAlcoonMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunAlcoonMain(slot, slot.RequireSpeciesState<AlcoonEnemyState>(), frame.Samus, frame.Level);
    }

    /// <summary>Advances Alcoon's typed animation.</summary>
    internal void AdvanceAlcoonTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext context) =>
        EnemyProgramAnimation.Advance(slot, AlcoonAnimations.Programs, AlcoonFrameDefinitions.ShownFrames.Of,
            static (AlcoonEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyAlcoonCue(slot, state, context.Level, cue));

    /// <summary>Initial horizontal fireball speed in native 8.8 units.</summary>
    private const int AlcoonFireballGraphicsHorizontalSpeed = 1024;
    /// <summary>Horizontal speed at which fireball deceleration stops.</summary>
    private const int AlcoonFireballTerminalHorizontalSpeed = 512;
    /// <summary>Native 8.8 horizontal speed reduction applied per fireball update.</summary>
    private const int AlcoonFireballHorizontalDeceleration = 64;

    /// <summary>
    /// Ports <c>InitAI_EnemyProjectile_AlcoonFireball</c> at <c>$86:9EB2</c>. The parameter
    /// is a byte offset (0, 2, or 4), exactly as passed in A by Alcoon's three instructions.
    /// </summary>
    internal void SpawnAlcoonFireball(RoomEnemySlot alcoon, int yVelocityTableByteOffset)
    {
        if (yVelocityTableByteOffset is not (0 or 2 or 4))
        {
            throw new InvalidDataException(
                $"Alcoon fireball Y-velocity byte offset {yVelocityTableByteOffset} is invalid.");
        }

        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        AlcoonEnemyState state = alcoon.RequireSpeciesState<AlcoonEnemyState>();
        bool movingLeft = state.Velocity.X.WholePixels < 0;

        // SpawnEnemyProjectileY_ParameterA_XGraphics copies the complete seven-word $9E90
        // definition before initializer $9EB2 touches position, subposition, and velocity.
        // Keeping that division exact makes radii, damage, persistence, shot blocking,
        // animation, and pre-instruction cartridge-owned instead of duplicating them here.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.AlcoonFireball,
            alcoon.Graphics);
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(new(CoordinateAxis.Wrap(alcoon.Position.X.Whole + (movingLeft ? -16 : 16)), CoordinateAxis.Wrap(alcoon.Position.Y.Whole - 12)));
        projectile.Velocity = projectile.Velocity.WithX(new SignedEightEight(movingLeft
            ? WordWrap.Unsigned(-AlcoonFireballGraphicsHorizontalSpeed)
            : AlcoonFireballGraphicsHorizontalSpeed));
        projectile.Velocity = projectile.Velocity.WithY(new SignedEightEight(EnemyFireballLaunches.AlcoonYVelocity(yVelocityTableByteOffset)));
    }

    /// <summary>Ports Alcoon fireball pre-instruction <c>$86:9EFF-$9F40</c>.</summary>
    internal static void RunAlcoonFireballPreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        // Unlike Ridley's fireball, this definition checks vertical collision first. If
        // either axis collides, the native routine clears the projectile ID immediately and
        // never applies horizontal deceleration on that frame.
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal))
        {
            projectile.Clear();
            return;
        }

        int velocity = projectile.Velocity.X.SignedRawValue;
        if (velocity < 0)
        {
            velocity = WordWrap.Signed(velocity + AlcoonFireballHorizontalDeceleration);
            if (velocity >= unchecked(-AlcoonFireballTerminalHorizontalSpeed))
                velocity = unchecked(-AlcoonFireballTerminalHorizontalSpeed);
        }
        else
        {
            velocity = WordWrap.Signed(velocity - AlcoonFireballHorizontalDeceleration);
            if (velocity < AlcoonFireballTerminalHorizontalSpeed)
                velocity = WordWrap.Signed(AlcoonFireballTerminalHorizontalSpeed);
        }
        projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.Wrapped(velocity));
    }
}
