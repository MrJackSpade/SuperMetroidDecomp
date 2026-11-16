using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
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
        RoomEnemyPlace body,
        PhantoonEnemyState state,
        SamusState samus)
    {
        PhantoonPatterns.PointPhantoonEyeAtSamus(body, state, samus);
        PhantoonPatterns.MovePhantoonInSwoop(body, state, samus, target: PhantoonSwoopTarget.DeathHeight);
        if (body.Position.X.Whole is >= 96 and < 160)
            state.Phase = PhantoonPhase.DyingFadeInOut;
    }

    /// <summary>Alternates five fade-outs and five fade-ins.</summary>
    internal void RunPhantoonDyingFadeCycles(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        if (PhantoonCadenceDefinitions.DyingFadeIn.IsSetIn(state.Death.FadeCycles))
            _phantoonPresentation.AdvancePhantoonFadeIn(body, state, denominator: 12);
        else
            _patterns.AdvancePhantoonFadeOut(state, denominator: 12);

        if (!state.BodyFade.Complete)
            return;
        state.BodyFade.Complete = false;
        state.Death.FadeCycles++;
        if (state.Death.FadeCycles < 10)
            return;

        state.Phase = PhantoonPhase.DyingExplosions;
        state.PhaseTimer = 15;
        state.Death.NextExplosion = 0;
        state.Death.ExplosionPasses = 0;
    }

    /// <summary>Runs the dying explosions: all thirteen, then the sixth through the thirteenth twice more.</summary>
    internal void RunPhantoonDyingExplosions(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        if (!PhantoonPresentation.TickPhantoonPhaseTimer(state))
            return;

        PhantoonDeathExplosionDefinition explosion = PhantoonDeathExplosions.Read(state.Death.NextExplosion);
        state.PhaseTimer = explosion.Delay;
        Position16<RoomPixels> position = body.Position.Whole + explosion.Offset;

        _projectiles.SpawnDustOrExplosion(position, explosion.Type);
        _audio.RequestPhantoonCombatSound(explosion.Type == MiscDustAnimation.BigExplosion
            ? PhantoonSoundDefinitions.BigDeathExplosionSound
            : PhantoonSoundDefinitions.DeathExplosionSound);

        state.Death.NextExplosion++;
        if (state.Death.NextExplosion < PhantoonDeathExplosionDefinitions.Count)
            return;

        state.Death.NextExplosion = PhantoonDeathExplosionDefinitions.RepeatStart;
        state.Death.ExplosionPasses++;
        if (state.Death.ExplosionPasses >= PhantoonDeathExplosionDefinitions.PassCount)
            state.Phase = PhantoonPhase.BeginFinalWavyDeath;
    }

    /// <summary>Starts the distorted background and pixelation stages after the fatal swoop.</summary>
    internal void BeginPhantoonWavyPixelatedDeath(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        RoomEnemyPlace mouth = state.Mouth;
        state.Wave.Amplitude = SignedSixteenSixteen.Zero;
        state.Wave.AmplitudeShrinking = false;
        state.Wave.PhaseStep = PhantoonWaveDefinitions.PhaseStep;
        state.BodyWave.Begin(PhantoonWaveDefinitions.DeathLength);
        state.Phase = PhantoonPhase.DyingFadeOut;
        state.Death.PixelationSize = PhantoonDeathDefinitions.InitialPixelationSize;
        state.Death.PixelationFinished = false;
        state.PixelationBlockSize = PhantoonDeathDefinitions.InitialPixelationSize;

        // His body stays to show the death. His eye, tentacles and mouth take his body's contact, processing and presence,
        // hidden, not animating and ignoring Samus.
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

    /// <summary>Advances death-wave and pixelation counters before room activation.</summary>
    internal void RunPhantoonWavyPixelatedDeath(
        PhantoonEnemyState state)
    {
        PhantoonPresentation.AdvancePhantoonWaveAmplitude(
            state.Wave,
            PhantoonWaveDefinitions.DeathAmplitudeChange,
            PhantoonWaveDefinitions.DeathAmplitude);

        PhantoonDeathState death = state.Death;
        if (!death.PixelationFinished)
        {
            if (!PhantoonCadenceDefinitions.DeathPixelationGrowsEverySixteenthUpdate.IsDue(_frameContext.FrameCounter))
                return;

            if (death.PixelationSize == PhantoonDeathDefinitions.FinalPixelationSize)
            {
                death.PixelationFinished = true;
                state.BodyFade.Complete = false;
                return;
            }

            death.PixelationSize++;
            state.PixelationBlockSize = death.PixelationSize;
            return;
        }

        _patterns.AdvancePhantoonFadeOut(state, denominator: 12);
        if (state.BodyFade.Complete)
            state.Phase = PhantoonPhase.AlmostDead;
    }


    /// <summary>Hides his whole body, every part showing nothing (#1515), and starts the 60-frame power delay.</summary>
    internal void ClearPhantoonDeathGraphics(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        state.PixelationBlockSize = 1;
        state.Wave.Length = null;
        state.Translucent = false;
        state.BodyVisibilityRequest = PhantoonBodyVisibilityRequest.HideForGood;
        state.Phase = PhantoonPhase.Dead;
        state.PhaseTimer = 60;
        state.BodyFade.Complete = false;
        body.Position = body.Position.WithWhole(new(384, 128));

        _presentation.RequireBossBody().Clear();
    }

    /// <summary>Enables the cleared Wrecked Ship room and its post-boss state.</summary>
    internal void ActivateWreckedShipAfterPhantoon(
        RoomEnemyPlace body,
        PhantoonEnemyState state)
    {
        if (state.PhaseTimer != 0)
        {
            state.PhaseTimer--;
            return;
        }
        if (!PhantoonCadenceDefinitions.PowerReturnStepEveryFourthUpdate.IsDue(_frameContext.FrameCounter))
            return;

        state.BodyFade.Denominator = 12;
        if (!AdvanceWreckedShipPowerOn(state))
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
            _room.Services.Bosses.SetDefeated(DefeatedBosses.AreaBoss);
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
    internal static bool AdvanceWreckedShipPowerOn(PhantoonEnemyState state)
    {
        PhantoonBodyFadeProgress fade = state.BodyFade;
        int denominator = fade.Denominator;
        int numerator = fade.Numerator;
        if (denominator + 1 < numerator)
        {
            fade.Numerator = 0;
            state.Colours.CompletePowerOn();
            return true;
        }
        state.Colours.PowerOn(numerator);
        fade.Numerator = numerator + 1;
        return false;
    }
}
