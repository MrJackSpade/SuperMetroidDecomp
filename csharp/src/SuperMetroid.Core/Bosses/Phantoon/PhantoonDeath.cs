using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's death: the fatal swoop, the fades, explosions and wavy pixelation, and Wrecked Ship's power returning.</summary>
[RoomLifetime]
internal sealed class PhantoonDeath
{
    /// <summary>Room services and background state used by the Wrecked Ship encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the starting, casual, rain, rage, and spiral flame actors.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Applies body colours and the background changes.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>The current enemy pass's clocks and flags.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Handles Phantoon's damage, death, and flame drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Phantoon's fades, health colors, wave amplitude and animations, shared by every phase of the fight.</summary>
    private readonly PhantoonPresentation _phantoonPresentation;
    /// <summary>Phantoon's attack rounds: figure eights, eye tracking, swoops, flame rain and rage.</summary>
    private readonly PhantoonPatterns _patterns;
    /// <summary>Receives the death's explosion sounds, dying cry and music change.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>The bosses' results of the pass, where the death asks for the restored door.</summary>
    private readonly EnemyPassEvents _passEvents;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal PhantoonDeath(PhantoonDeathDependencies dependencies)
    {
        _audio = dependencies.Audio;
        _passEvents = dependencies.PassEvents;
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
        RoomEnemyPlace body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PhantoonPatterns.PointPhantoonEyeAtSamus(body, state, samus);
        PhantoonPatterns.MovePhantoonInSwoop(body, state, samus, target: PhantoonSwoopTarget.DeathHeight);
        if (body.Position.X.Whole is >= PhantoonArenaDefinitions.DeathBandLeftX and < PhantoonArenaDefinitions.DeathBandRightX)
            state.Phase = PhantoonPhase.DyingFadeInOut;
    }

    /// <summary>Alternates fade-outs and fade-ins (<see cref="PhantoonArenaDefinitions.DyingFadeCycles"/>).</summary>
    internal void RunPhantoonDyingFadeCycles(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        if (PhantoonCadenceDefinitions.DyingFadeIn.IsInSecondHalfAt(state.Death.FadeCycles))
            _phantoonPresentation.AdvancePhantoonFadeIn(body, state, steps: PhantoonColorDefinitions.BodyFadeSteps);
        else
            _patterns.AdvancePhantoonFadeOut(state, steps: PhantoonColorDefinitions.BodyFadeSteps);

        if (!state.BodyFade.IsFinished)
            return;
        state.BodyFade.Resume();
        state.Death.FadeCycles++;
        if (state.Death.FadeCycles < PhantoonArenaDefinitions.DyingFadeCycles)
            return;

        state.Phase = PhantoonPhase.DyingExplosions;
        state.PhaseWait.LastFor(PhantoonArenaDefinitions.FirstDyingExplosionUpdates);
        state.Death.NextExplosion = PhantoonDeathExplosions.First;
        state.Death.ExplosionPasses = 0;
    }

    /// <summary>Runs the dying explosions: the opening explosions, then the repeated explosions (<see cref="PhantoonDeathExplosionDefinitions.PassCount"/>).</summary>
    internal void RunPhantoonDyingExplosions(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        if (!state.PhaseWait.StepRunsOut())
            return;

        PhantoonDeathExplosionDefinition explosion = state.Death.NextExplosion.Entry;
        state.PhaseWait.LastFor(explosion.Delay);
        Position16<RoomPixels> position = body.Position.Whole + explosion.Offset;

        _projectiles.SpawnDustOrExplosion(position, explosion.Type);
        _audio.RequestPhantoonCombatSound(PhantoonSoundDefinitions.DeathExplosionSounds.TryGetValue(explosion.Type, out SoundEffectId sound)
            ? sound
            : PhantoonSoundDefinitions.DeathExplosionSound);

        if (state.Death.NextExplosion.Following is { } next)
        {
            state.Death.NextExplosion = next;
            return;
        }

        state.Death.NextExplosion = PhantoonDeathExplosions.FirstRepeated;
        state.Death.ExplosionPasses++;
        if (state.Death.ExplosionPasses >= PhantoonDeathExplosionDefinitions.PassCount)
            state.Phase = PhantoonPhase.BeginFinalWavyDeath;
    }

    /// <summary>Starts his body's wave and pixelation after the fatal swoop.</summary>
    internal void BeginPhantoonWavyPixelatedDeath(PhantoonEnemyState state)
    {
        RoomEnemyPlace mouth = state.Mouth;
        state.Wave.Amplitude = UnsignedSixteenSixteen.Zero;
        state.Wave.AmplitudeShrinking = false;
        state.Wave.PhaseStep = PhantoonWaveDefinitions.PhaseStep;
        state.BodyWave.Begin(PhantoonWaveDefinitions.DeathLength);
        state.Phase = PhantoonPhase.DyingFadeOut;
        state.Death.PixelationSize = PhantoonDeathDefinitions.InitialPixelationSize;
        state.Death.Stage = PhantoonDeathStage.Pixelating;

        // His body stays to show the death; his eye, tentacles and mouth are hidden (#2257).
        HideDyingPart(state.Eye);
        HideDyingPart(state.Tentacles);
        HideDyingPart(mouth);
        _audio.RequestPhantoonCombatSound(PhantoonSoundDefinitions.DyingCry);
    }

    /// <summary>Hides one of his parts as his body dissolves: it ignores Samus, stops animating and shows nothing.</summary>
    private static void HideDyingPart(RoomEnemyPlace part)
    {
        part.Contact = part.Contact.With(EnemyContact.IgnoresSamus);
        part.Processing = part.Processing.Without(EnemyProcessing.Animates);
        part.Presence = part.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Advances death-wave and pixelation counters before room activation.</summary>
    internal void RunPhantoonWavyPixelatedDeath(
        PhantoonEnemyState state)
    {
        PhantoonPresentation.AdvancePhantoonWaveAmplitude(
            state.Wave,
            PhantoonWaveDefinitions.DeathAmplitudeChange,
            PhantoonWaveDefinitions.DeathAmplitude);

        PhantoonDeathState death = state.Death;
        if (death.Stage == PhantoonDeathStage.Pixelating)
        {
            if (!PhantoonCadenceDefinitions.DeathPixelationGrows.IsDue(_frameContext.FrameCounter))
                return;

            if (death.PixelationSize == PhantoonDeathDefinitions.FinalPixelationSize)
            {
                death.Stage = PhantoonDeathStage.FadingOut;
                state.BodyFade.Resume();
                return;
            }

            death.PixelationSize++;
            return;
        }

        _patterns.AdvancePhantoonFadeOut(state, steps: PhantoonColorDefinitions.BodyFadeSteps);
        if (state.BodyFade.IsFinished)
            state.Phase = PhantoonPhase.AlmostDead;
    }


    /// <summary>Hides his whole body, every part showing nothing (#1515), and starts the power's delay.</summary>
    internal static void ClearPhantoonDeathGraphics(PhantoonEnemyState state)
    {
        state.Death.PixelationSize = PhantoonDeathDefinitions.InitialPixelationSize;
        state.Wave.End();
        state.Translucent = false;
        state.BodyVisibilityRequest = PhantoonBodyVisibilityRequest.HideForGood;
        state.Phase = PhantoonPhase.Dead;
        state.PhaseWait.LastFor(PhantoonArenaDefinitions.PowerReturnDelayUpdates);
        state.BodyFade.Resume();

        state.BodyParts.Clear();
    }

    /// <summary>Enables the cleared Wrecked Ship room and its post-boss state.</summary>
    internal void ActivateWreckedShipAfterPhantoon(
        RoomEnemyPlace body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        if (state.PhaseWait.StepKeepsRunning())
            return;
        if (!PhantoonCadenceDefinitions.PowerReturnStep.IsDue(_frameContext.FrameCounter))
            return;

        if (!AdvanceWreckedShipPowerOn(state))
            return;

        // The death drops scatter energy and ammunition pickups over the visible boss arena. The coordinates are
        // absolute room coordinates, not offsets from Phantoon's body.
        _deathsAndDrops.SpawnEnemyDropScatter(
            EnemyDefinitionId.PhantoonBody,
            count: PhantoonArenaDefinitions.DeathDropCount,
            xBase: PhantoonArenaDefinitions.DeathDropOrigin.X,
            xScatter: EnemyDropScatterDefinitions.NarrowArenaHorizontalScatter,
            yBase: PhantoonArenaDefinitions.DeathDropOrigin.Y,
            yScatter: EnemyDropScatterDefinitions.ArenaBandVerticalScatter,
            samus: samus);
        // His body and his three parts are deleted (#2257).
        body.Presence = body.Presence.With(EnemyPresence.Deleted);
        state.Eye.Presence = state.Eye.Presence.With(EnemyPresence.Deleted);
        state.Tentacles.Presence = state.Tentacles.Presence.With(EnemyPresence.Deleted);
        state.Mouth.Presence = state.Mouth.Presence.With(EnemyPresence.Deleted);
        if (!state.BossDefeatPersisted)
        {
            _room.Services.Bosses.SetDefeated(DefeatedBosses.AreaBoss);
            state.BossDefeatPersisted = true;
        }
        _passEvents.Publish(new PhantoonDoorChanged(PhantoonDoorChange.RestoredDoor));
        _audio.RequestPhantoonMusic(MusicRequest.SelectTrack(MusicTrack.Elevator));
    }

    /// <summary>
    /// Takes one step of the Wrecked Ship's power coming on over the room (#1512, #2251), its own fade with its own
    /// progress: every colour but Phantoon's body's fades toward its powered colour, a typed fade the room shows.
    /// </summary>
    /// <returns>Whether the power had finished coming on.</returns>
    private static bool AdvanceWreckedShipPowerOn(PhantoonEnemyState state)
    {
        if (state.ShipPowerOn.TakeStep(PhantoonColorDefinitions.PowerOnSteps) is { } step)
        {
            state.Colours.PowerOn(step);
            return false;
        }
        state.Colours.CompletePowerOn();
        return true;
    }
}
