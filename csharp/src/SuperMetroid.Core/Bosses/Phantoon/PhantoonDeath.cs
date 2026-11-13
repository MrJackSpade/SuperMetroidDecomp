using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's death: the fatal swoop, the fades, explosions and wavy mosaic, and Wrecked Ship's power returning.</summary>
[RoomLifetime]
internal sealed class PhantoonDeath
{
    /// <summary>Room services and background state used by the Wrecked Ship encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the starting, casual, rain, rage, and spiral flame actors.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Applies body palettes and the background changes.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies Samus state for eye tracking and attack selection.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Handles Phantoon's damage, death, and flame drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Phantoon's fades, health colors, wave amplitude, animations and phase timer, shared by every phase of the fight.</summary>
    private readonly PhantoonPresentation _phantoonPresentation;
    /// <summary>Phantoon's attack rounds: figure eights, eye tracking, swoops, flame rain and rage.</summary>
    private readonly PhantoonPatterns _patterns;
    /// <summary>Receives the death's explosion sounds, dying cry and music change.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>The bosses' results of the pass, where the death asks for the restored door.</summary>
    private readonly BossFrameResultBuilder _frameResults;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonDeath(PhantoonDeathDependencies dependencies)
    {
        _audio = dependencies.Audio;
        _frameResults = dependencies.FrameResults;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _phantoonPresentation = dependencies.PhantoonPresentation;
        _patterns = dependencies.Patterns;
    }

    /// <summary>Tracks Samus during the fatal swoop and advances once the body reaches the center band.</summary>
    internal static void RunPhantoonFatalSwoop(
        RoomEnemySlot body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PhantoonPatterns.PointPhantoonEyeAtSamus(body, state, samus);
        PhantoonPatterns.MovePhantoonInSwoop(body, state, samus, target: PhantoonSwoopTarget.DeathHeight);
        if (body.Position.X.Whole is >= 96 and < 160)
            state.BodyState.Phase = PhantoonPhase.DyingFadeInOut;
    }

    /// <summary>Alternates five fade-outs and five fade-ins.</summary>
    internal void RunPhantoonDyingFadeCycles(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (PhantoonCadenceDefinitions.DyingFadeIn.IsSetIn(state.EyeState.DyingFadeCycles))
            _phantoonPresentation.AdvancePhantoonFadeIn(body, state, denominator: 12);
        else
            _patterns.AdvancePhantoonFadeOut(state, denominator: 12);

        if (!state.EyeState.FadeComplete)
            return;
        state.EyeState.FadeComplete = false;
        state.EyeState.DyingFadeCycles = WrappingCounter.IncrementCount(state.EyeState.DyingFadeCycles);
        if (state.EyeState.DyingFadeCycles < 10)
            return;

        state.BodyState.Phase = PhantoonPhase.DyingExplosions;
        state.BodyState.PhaseTimer = 15;
        state.TentaclesState.ExplosionIndex = 0;
        state.BodyState.ExplosionPass = 0;
    }

    /// <summary>Runs the literal 13-entry-then-5..12-twice explosion schedule.</summary>
    internal void RunPhantoonDyingExplosions(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonPhaseTimer(state.BodyState))
            return;

        PhantoonDeathExplosionDefinition explosion = PhantoonDeathExplosions.Read(state.TentaclesState.ExplosionIndex);
        state.BodyState.PhaseTimer = explosion.Delay;
        Position16<RoomPixels> position = body.Position.Whole + explosion.Offset;

        _projectiles.SpawnRoomGraphicsDustExplosion(position, explosion.Type);
        _audio.RequestPhantoonCombatSound(explosion.Type == MiscDustAnimation.BigExplosion
            ? PhantoonSoundDefinitions.BigDeathExplosionSound
            : PhantoonSoundDefinitions.DeathExplosionSound);

        state.TentaclesState.ExplosionIndex = WrappingCounter.IncrementCount(state.TentaclesState.ExplosionIndex);
        if (state.TentaclesState.ExplosionIndex < PhantoonDeathExplosionDefinitions.Count)
            return;

        state.TentaclesState.ExplosionIndex = PhantoonDeathExplosionDefinitions.RepeatStart;
        state.BodyState.ExplosionPass = WrappingCounter.IncrementCount(state.BodyState.ExplosionPass);
        if (state.BodyState.ExplosionPass >= PhantoonDeathExplosionDefinitions.PassCount)
            state.BodyState.Phase = PhantoonPhase.BeginFinalWavyDeath;
    }

    /// <summary>Starts the distorted background and mosaic stages after the fatal swoop.</summary>
    internal void BeginPhantoonWavyMosaicDeath(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        RoomEnemySlot mouth = state.Mouth;
        state.MouthState.WaveAmplitude = 0;
        state.MouthState.WaveAmplitudeShrinking = false;
        state.MouthState.WavyPhaseDelta = PhantoonBehavior.PhantoonWavyPhaseDelta;
        state.BodyWave.Begin(PhantoonWaveDefinitions.DeathLength);
        state.BodyState.Phase = PhantoonPhase.DyingFadeOut;
        state.EyeState.DeathMosaicSize = PhantoonDeathDefinitions.InitialMosaicSize;
        state.EyeState.DeathMosaicFinished = false;
        state.MosaicBlockSize = PhantoonDeathDefinitions.InitialMosaicSize;

        // The body remains the background producer. Eye/tentacle/mouth become invisible and ignore
        // Samus while preserving every unrelated raw property bit exactly as the death step does.
        EnemyContact hiddenPartContact = body.Contact.With(EnemyContact.IgnoresSamus);
        EnemyProcessing hiddenPartProcessing = body.Processing.Without(EnemyProcessing.Animates);
        EnemyPresence hiddenPartPresence = body.Presence.With(EnemyPresence.Hidden);
        state.Eye.Contact = hiddenPartContact;
        state.Eye.Processing = hiddenPartProcessing;
        state.Eye.Presence = hiddenPartPresence;
        state.Tentacles.Contact = hiddenPartContact;
        state.Tentacles.Processing = hiddenPartProcessing;
        state.Tentacles.Presence = hiddenPartPresence;
        mouth.Contact = hiddenPartContact;
        mouth.Processing = hiddenPartProcessing;
        mouth.Presence = hiddenPartPresence;
        _audio.RequestPhantoonCombatSound(PhantoonSoundDefinitions.DyingCry);
    }

    /// <summary>Advances death-wave and mosaic counters before room activation.</summary>
    internal void RunPhantoonWavyMosaicDeath(
        PhantoonEnemyState state)
    {
        PhantoonPresentation.AdvancePhantoonWaveAmplitude(
            state.MouthState,
            PhantoonBehavior.PhantoonDeathWaveDelta,
            PhantoonBehavior.PhantoonDeathWaveMaximum);

        PhantoonEyeState eyePart = state.EyeState;
        if (!eyePart.DeathMosaicFinished)
        {
            if (!PhantoonCadenceDefinitions.DeathMosaicGrowsEverySixteenthUpdate.IsDue(_frameContext.FrameCounter))
                return;

            if (eyePart.DeathMosaicSize == PhantoonDeathDefinitions.FinalMosaicSize)
            {
                eyePart.DeathMosaicFinished = true;
                eyePart.FadeComplete = false;
                return;
            }

            eyePart.DeathMosaicSize++;
            state.MosaicBlockSize = eyePart.DeathMosaicSize;
            return;
        }

        _patterns.AdvancePhantoonFadeOut(state, denominator: 12);
        if (state.EyeState.FadeComplete)
            state.BodyState.Phase = PhantoonPhase.AlmostDead;
    }


    /// <summary>Hides his whole body, every part showing nothing (#1515), and starts the 60-frame power delay.</summary>
    internal void ClearPhantoonDeathGraphics(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        state.MosaicBlockSize = 1;
        state.EyeState.WaveLength = null;
        state.Translucent = false;
        state.MouthState.BodyLayer = PhantoonBodyLayer.Cleared;
        state.BodyState.Phase = PhantoonPhase.Dead;
        state.BodyState.PhaseTimer = 60;
        state.EyeState.FadeComplete = false;
        body.Position = body.Position.WithWhole(new(384, 128));

        _presentation.RequireBossBody().Clear();
    }

    /// <summary>Enables the cleared Wrecked Ship room and its post-boss state.</summary>
    internal void ActivateWreckedShipAfterPhantoon(
        RoomEnemySlot body,
        PhantoonEnemyState state)
    {
        if (state.BodyState.PhaseTimer != 0)
        {
            state.BodyState.PhaseTimer--;
            return;
        }
        if (!PhantoonCadenceDefinitions.PowerReturnStepEveryFourthUpdate.IsDue(_frameContext.FrameCounter))
            return;

        state.EyeState.FadeDenominator = 12;
        if (!AdvanceWreckedShipPowerPalette(state, state.EyeState))
            return;

        // The death drops scatter sixteen room-graphics pickups over the visible boss arena.
        // The coordinates are absolute room coordinates, not offsets from Phantoon's body.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.PhantoonBody,
            count: 16,
            xBase: 64,
            xScatter: EnemyDropScatterDefinitions.NarrowArenaHorizontalScatter,
            yBase: 96,
            yScatter: EnemyDropScatterDefinitions.ArenaBandVerticalScatter);
        // The body is deleted, and every other part takes the body's properties.
        body.Presence = body.Presence.With(EnemyPresence.Deleted);
        state.Eye.Contact = body.Contact;
        state.Eye.Processing = body.Processing;
        state.Eye.Presence = body.Presence;
        state.Tentacles.Contact = body.Contact;
        state.Tentacles.Processing = body.Processing;
        state.Tentacles.Presence = body.Presence;
        state.Mouth.Contact = body.Contact;
        state.Mouth.Processing = body.Processing;
        state.Mouth.Presence = body.Presence;
        if (!state.BossDefeatPersisted)
        {
            _room.Services.Bosses.SetDefeated(BossBits.AreaBoss);
            state.BossDefeatPersisted = true;
        }
        _frameResults.PhantoonDoor = PhantoonDoorChange.RestoredDoor;
        _audio.RequestPhantoonMusic(MusicRequest.SelectTrack(MusicTrack.Elevator));
    }

    /// <summary>
    /// Takes one step of the Wrecked Ship's power coming on over the room (#1512): every colour but Phantoon's body's fades
    /// toward its powered colour, which the room shows as the import's shade of each step.
    /// </summary>
    /// <returns>Whether the power had finished coming on.</returns>
    internal static bool AdvanceWreckedShipPowerPalette(PhantoonEnemyState state, PhantoonEyeState eye)
    {
        int denominator = eye.FadeDenominator;
        int numerator = eye.FadeNumerator;
        if (denominator + 1 < numerator)
        {
            eye.FadeNumerator = 0;
            state.Colours.CompletePowerOn();
            return true;
        }
        state.Colours.PowerOn(numerator);
        eye.FadeNumerator = numerator + 1;
        return false;
    }
}
