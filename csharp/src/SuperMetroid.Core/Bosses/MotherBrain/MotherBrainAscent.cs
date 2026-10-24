using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain rising into her second form: its graphics, brain setup, the rise and the menacing head shake.</summary>
[RoomLifetime]
internal sealed class MotherBrainAscent
{
    /// <summary>Provides room scratch memory, tiles, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Draws encounter artwork and applies installed palette data.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies Samus and camera state for the current enemy frame.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainAscent(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomEarthquake earthquake, EnemyPresentation presentation, EnemyFrameContext frameContext, MotherBrainEncounter motherBrainEncounter, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _room = room;
        _projectiles = projectiles;
        _earthquake = earthquake;
        _presentation = presentation;
        _frameContext = frameContext;
        _motherBrainEncounter = motherBrainEncounter;
    }

    /// <summary>Stages the phase-two body tilemap and requests its first VRAM transfer.</summary>
    internal void SetupMotherBrainPhaseTwoGraphics(MotherBrainEnemyState state)
    {
        // `$A9:8D11` installs colors 1..15 of the attack and back-leg palettes. Color zero
        // belongs to the room backdrop and must remain untouched.
        (_bossArtwork.MotherBrainRoomColors ?? throw new InvalidOperationException(
            "Mother Brain phase two requires installed room colors."))
            .ApplyPhaseTwoInitial(_presentation.Cgram);
        state.EnableUnpauseHook = true;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain;
        // The native transition now requests the prepared enemy image, replacing the
        // room's pipe background before the articulated body becomes visible. Its one
        // 32x32 screen is the whole BG2 map she shows; she owns that image from here (#1356).
        var words = new ushort[state.EnemyBg2TilemapSize / 2];
        ushort[] bg2 = _room.Staging.EnemyBg2Tilemap;
        for (int index = 0; index < words.Length; index++)
            words[index] = bg2[index];
        var image = new EnemyBg2Image(MotherBrainBg2Definitions.BlankTile);
        image.Write(0, words);
        _presentation.AttachBg2Image(image);
        state.EnemyBg2TilemapTransferRequested = true;
    }

    /// <summary>Attaches the phase-two head and prepares its first animation and position.</summary>
    internal void SetupMotherBrainPhaseTwoBrain(
        MotherBrainEnemyState state)
    {
        // `$34` is the room's verified Mother Brain phase-two color-math configuration.
        state.LayerBlendingDefaultConfig = LayerBlendingConfiguration.MotherBrainPhaseTwo;
        _room.Services.MotherBrain.SetLayerBlendingDefault(state.LayerBlendingDefaultConfig);
        state.BrainFunction = MotherBrainBrainFunction.SetupBrainAndNeckToBeDrawn;

        // Both records become tangible before the suspense pause, but remain invisible for
        // their separate native reasons: the body waits for `$8DEC`, while the head's $0100
        // bit deliberately selects the custom articulated draw hook.
        state.Body.Properties =
            state.Body.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.Head.Properties =
            state.Head.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.Head.Health = 18000;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense;
        state.FunctionTimer = WrappingCountdown.Start(128);

        // `$8D49` falls directly into `$8D79`; the freshly written timer is observed as
        // $007F before another enemy slot can run.
        PauseBeforeMotherBrainRises(state);
    }

    /// <summary>Holds the phase-two body before its scripted ascent begins.</summary>
    internal void PauseBeforeMotherBrainRises(
        MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        state.Function = MotherBrainBodyFunction.FakeDeathAscentPrepareForRising;
        state.FunctionTimer = WrappingCountdown.Start(32);
        PrepareMotherBrainForRising(state);
    }

    /// <summary>Stages the body and head state used by the rise from the collapsed room.</summary>
    internal void PrepareMotherBrainForRising(
        MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        // Bank $88 installs a fixed per-scanline main-screen table that prevents the newly
        // loaded leg sprites from appearing over the floor. The renderer consumes this
        // typed lifetime rather than pretending it is a body-visibility flag.
        state.RisingHdmaActive = true;
        state.Head.Properties = state.Head.Properties.With(EnemyProperties.Invisible);
        MotherBrainEncounterAccess.SetMotherBrainBrainProgram(state, MotherBrainHeadProgram.Initial);
        state.Function = MotherBrainBodyFunction.FakeDeathAscentLoadLegTiles;
        state.FunctionTimer = WrappingCountdown.Start(256);

        // The first leg page shows on the same frame as HDMA creation.
        LoadMotherBrainLegTiles(state);
    }

    /// <summary>Loads the leg tiles needed before the phase-two body becomes visible.</summary>
    internal void LoadMotherBrainLegTiles(
        MotherBrainEnemyState state)
    {
        if (!ShowNextLegPage(state))
        {
            return;
        }

        state.Function = MotherBrainBodyFunction.FakeDeathAscentContinuePausing;

        // `$8DBD` has no return between the state write and `$8DC3`; the still-$0100 timer
        // loses its first tick on the final DMA frame.
        ContinueMotherBrainAscentPause(state);
    }

    /// <summary>
    /// Shows the next of the leg-loading pages, one per call, and reports whether that was the last: the eight leg pages,
    /// then the four attack pages the legs had covered.
    /// </summary>
    internal bool ShowNextLegPage(MotherBrainEnemyState state)
    {
        int index = state.NextLegTilePage;
        int legPages = MotherBrainSpritePage.PageCount(MotherBrainSpritePageSheet.Legs);
        _motherBrainEncounter.ShowSpritePage(index < legPages
            ? MotherBrainSpritePage.Of(MotherBrainSpritePageSheet.Legs, index)
            : MotherBrainSpritePage.Of(MotherBrainSpritePageSheet.Attack, index - legPages));

        // Native checks the next record's zero terminator on the same call as
        // page eleven. Preserve that same-frame completion and list reset.
        if (index + 1 == MotherBrainEnemyState.LegLoadPageCount)
        {
            state.NextLegTilePage = 0;
            return true;
        }

        state.NextLegTilePage = index + 1;
        return false;
    }

    /// <summary>Consumes the pre-rise wait and advances to body ascent.</summary>
    internal static void ContinueMotherBrainAscentPause(MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        state.Body.Position = state.Body.Position.WithWhole(new(59, 279));
        MotherBrainBody.PublishMotherBrainBg2Scroll(state, new(unchecked((ushort)-27), unchecked((ushort)-217)));
        state.HitboxesEnabled = MotherBrainHitboxParts.Body | MotherBrainHitboxParts.Brain | MotherBrainHitboxParts.Neck;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentStartMusicAndEarthquake;
    }

    /// <summary>Switches the body into the rising state after its setup pause.</summary>
    internal void StartMotherBrainAscent(MotherBrainEnemyState state)
    {
        MotherBrainEncounterAccess.SetMotherBrainBodyProgram(state, MotherBrainBodyProgram.Crouched);
        state.Head.InstructionTimer = WrappingCountdown.Start(1);
        state.Body.Properties = state.Body.Properties.Without(EnemyProperties.Invisible);
        state.Body.Position = state.Body.Position.WithWhole(new(59, 279));
        MotherBrainBody.PublishMotherBrainBg2Scroll(state, new(unchecked((ushort)-27), unchecked((ushort)-217)));
        state.RequestMusic(MusicRequest.SelectTrack(5), MusicQueueDelay.Shortest);
        _earthquake.EarthquakeType = 2;
        _earthquake.EarthquakeTimer = WrappingCountdown.Start(256);
        state.NeckAngleDelta = 80;
        state.NeckMovementEnabled = true;
        state.LowerNeckMovementIndex = MotherBrainNeckMovement.Raise;
        state.UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentRaiseMotherBrain;
    }

    /// <summary>Moves the body upward through the authored rise while updating its head attachment.</summary>
    internal void RaiseMotherBrain(MotherBrainEnemyState state)
    {
        // `$A9:8E4D` gates on NMI_FrameCounter ($05B6), not the separate 8-bit $05B5; the
        // two advance together but are not aligned, so their low two bits differ.
        if ((_frameContext.NmiFrameCounter & 3) != 0)
            return;

        SpawnMotherBrainAscentDust(state);
        state.Bg2Scroll = state.Bg2Scroll + new Offset16<Bg2Pixels>(0, 2);
        state.Body.Position = state.Body.Position.WithWholeY(unchecked((ushort)(state.Body.Position.Y.Whole - 2)));
        MotherBrainBody.PublishMotherBrainBg2Scroll(state, state.Bg2Scroll);
        if (state.Body.Position.Y.Whole >= 189)
            return;

        state.EnemyBg2TilemapSize = 320;
        state.EnemyBg2TilemapTransferRequested = true;
        state.Body.Position = state.Body.Position.WithWholeY(188);
        _earthquake.EarthquakeTimer = WrappingCountdown.Start(0);
        state.RisingHdmaActive = false;
        MotherBrainEncounterAccess.SetMotherBrainBodyProgram(state, MotherBrainBodyProgram.StandAfterCrouchSlow);
        state.Function = MotherBrainBodyFunction.FakeDeathAscentWaitUntilUncrouched;
    }

    /// <summary>Emits dust at the feet as the body rises from the floor.</summary>
    private void SpawnMotherBrainAscentDust(MotherBrainEnemyState state)
    {
        CountdownStep timer = state.BodySubFunctionTimer.Decrement();
        state.BodySubFunctionTimer = timer.Remaining;
        if (timer.IsNegative)
            state.BodySubFunctionTimer = WrappingCountdown.Start(7);

        ushort x = MotherBrainBehavior.MotherBrainAscentDustXPositions[state.BodySubFunctionTimer.RawValue];
        int random = _room.Random.Read();
        MiscDustAnimation animation = (SnesLittleEndian.HighByte(random) & 1) == 0 ? MiscDustAnimation.SmallDustCloud : MiscDustAnimation.ShortBigDustCloud;
        _projectiles.SpawnRoomGraphicsDustExplosion(new(x, 212), animation);
        state.LastSoundEffect = MotherBrainSoundDefinitions.AscentDust;
    }

    /// <summary>Holds the body phase until its slow uncrouch animation ends.</summary>
    internal static void WaitForMotherBrainToFinishUncrouching(MotherBrainEnemyState state)
    {
        if (state.Pose != MotherBrainBodyPose.Standing)
            return;

        state.GrayTransitionCounter = 0;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray;
        state.FunctionTimer = WrappingCountdown.Start(0);
    }

    /// <summary>Applies the phase-two head shake before normal combat begins.</summary>
    internal static void ShakeMotherBrainHeadMenacingly(
        MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        MotherBrainEncounterAccess.SetMotherBrainBrainProgram(state, MotherBrainHeadProgram.StretchingPhase2);
        state.Function = MotherBrainBodyFunction.SecondPhaseStretchingBringHeadUp;
        state.NeckAngleDelta = 64;
        state.FunctionTimer = WrappingCountdown.Start(256);
        BringMotherBrainHeadBackUp(state);
    }

    /// <summary>Returns the head from its lowered pose after the stretch.</summary>
    internal static void BringMotherBrainHeadBackUp(
        MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        state.LowerNeckMovementIndex = MotherBrainNeckMovement.BobDown;
        state.UpperNeckMovementIndex = MotherBrainNeckMovement.BobUp;
        state.Function = MotherBrainBodyFunction.SecondPhaseStretchingFinish;
        state.FunctionTimer = WrappingCountdown.Start(64);
        FinishMotherBrainStretching(state);
    }

    /// <summary>Switches the head to its neutral phase-two instruction list.</summary>
    internal static void FinishMotherBrainStretching(MotherBrainEnemyState state)
    {
        if (!MotherBrainEncounterAccess.DecrementMotherBrainTimerPastZero(state))
            return;

        state.SmallPurpleBreathGenerationEnabled = true;
        state.Function = MotherBrainBodyFunction.SecondPhaseThinking;
    }
}
