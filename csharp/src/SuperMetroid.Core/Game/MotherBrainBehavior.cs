using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Mother Brain species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class MotherBrainBehavior : IEnemySpecies
{
    /// <summary>Routes Mother Brain's AI and projectile handlers through the room dispatch tables.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Provides room scratch memory, tiles, palette state, and effects used by the encounter.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns the bombs, glass, beams, turrets, and other encounter projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Publishes screen shake during the encounter's collapse and attacks.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues the encounter's music and sound effects in enemy update order.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Retains the native body, head, and falling-tube slot layout.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Draws encounter artwork and applies installed palette data.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Supplies Samus and camera state for the current enemy frame.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Resolves the head, body, projectile, and Samus contact bounds.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Applies the encounter's damage and death/drop rules.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;

    /// <summary>Coordinates the Baby Metroid transfer and rainbow-beam rescue sequence.</summary>
    private readonly BabyMetroidCutsceneBehavior _babyMetroidCutsceneBehavior;

    /// <summary>Provides Rinka behavior activated by the encounter.</summary>
    private readonly RinkaBehavior _rinkaBehavior;

    /// <summary>Provides the space-pirate behavior selected during encounter setup.</summary>
    private readonly WalkingSpacePirateBehavior _walkingSpacePirateBehavior;
    /// <summary>Provides shared projectile movement used by the encounter.</summary>
    private readonly RidleyProjectiles _ridleyProjectiles;
    /// <summary>Tracks room completion and boss status across the fight.</summary>
    private readonly EnemyRoomStatus _roomStatus;

    /// <summary>Binds the encounter's room services while keeping one state owner for body and head.</summary>
    internal MotherBrainBehavior(MotherBrainBehaviorDependencies dependencies)
    {
        _dispatch = dependencies.Dispatch;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        _frameContext = dependencies.FrameContext;
        _collision = dependencies.Collision;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _motherBrainEncounter = dependencies.MotherBrainEncounter;
        _babyMetroidCutsceneBehavior = dependencies.BabyMetroidCutsceneBehavior;
        _rinkaBehavior = dependencies.RinkaBehavior;
        _walkingSpacePirateBehavior = dependencies.WalkingSpacePirateBehavior;
        _ridleyProjectiles = dependencies.RidleyProjectiles;
        _roomStatus = dependencies.RoomStatus;
    }

    /// <summary>Native initial head instruction list at <c>$A9:9C21</c>.</summary>
    private const ushort MotherBrainInitialHeadInstruction = 0x9c21;


    /// <summary>Recognizes the two enemy definitions that share the encounter state.</summary>
    private static bool IsMotherBrainDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.MotherBrainBody or EnemyDefinitionId.MotherBrainHead;

    /// <summary>Discards the prior room's shared body/head state before enemy initialization.</summary>
    internal void ResetMotherBrainRoomState() => _motherBrainEncounter.MotherBrain = null;

    /// <summary>Ports <c>InitAI_MotherBrainBody</c> at <c>$A9:8687</c>.</summary>
    private void InitializeMotherBrainBody(RoomEnemySlot body)
    {
        if (body.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Mother Brain's body requires native slot zero, not slot {body.SlotIndex}.");
        }

        // Prepare the future body image without replacing the visible pipe background.
        // The phase-two transition owns the first transfer from staging into VRAM.
        WorkRamWordTable bg2 = _room.Scratch.EnemyBg2Tilemap;
        for (int index = 0; index < MotherBrainBg2Definitions.ClearWordCount; index++)
            bg2[index] = MotherBrainBg2Definitions.BlankTile;

        body.CurrentInstruction = MotherBrainBodyInstructionProgramDefinitions.InitialDummy;
        body.InstructionTimer = NativeCountdown.Start(1);
        body.VramTilesIndex = 0;
        body.Properties = body.Properties.With(
            EnemyProperties.BlocksPlasmaBeam |
            EnemyProperties.IgnoreSamusCollision |
            EnemyProperties.Invisible);
        body.PaletteIndex = 0;

        // Both source labels include transparent color zero. `$A9:86B3/$86C0` deliberately
        // begin at +2 and copy only colors 1..15 into the otherwise unrelated room palette
        // ranges used by glass shards and tube projectiles.
        LoadMotherBrainRoomEntryColors();

        _motherBrainEncounter.MotherBrain = new MotherBrainEnemyState(body)
        {
            Form = 0,
            EnableUnpauseHook = false,
            HitboxesEnabled = 2,
            BrainFunction = MotherBrainBrainFunction.SetupBrainToBeDrawn,
            Function = MotherBrainBodyFunction.FirstPhase,
            FxEntry = 1,
            BackgroundTilemapPrepared = true,
            EnemyBg2TilemapSize = MotherBrainBg2Definitions.InitialTransferByteCount,
        };
        _motherBrainEncounter.MotherBrain.RecordInitialTurretRequests();
        SpawnMotherBrainInitialTurrets();
    }

    /// <summary>Applies the installed room-entry palette for glass and tube artwork.</summary>
    internal void LoadMotherBrainRoomEntryColors()
    {
        (_presentation.MotherBrainRoomColors ?? throw new InvalidOperationException(
            "Mother Brain room requires installed color artwork."))
            .ApplyRoomEntry(_presentation._cgram!);
    }

    /// <summary>Ports <c>InitAI_MotherBrainHead</c> at <c>$A9:8705</c>.</summary>
    private void InitializeMotherBrainHead(RoomEnemySlot head)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ??
            throw new InvalidDataException("Mother Brain's head appeared before its body record.");
        if (head.SlotIndex != 1)
        {
            throw new InvalidDataException(
                $"Mother Brain's head requires native slot one, not slot {head.SlotIndex}.");
        }

        // The cartridge seeds the complete future corpse effect during room load, not when
        // Mother Brain dies. Keeping this eager call preserves the exact WRAM state visible
        // throughout every earlier phase and prevents the death sequence from inventing it.
        state.CorpseRotting.Initialize(_room.Bus!, _room.MotherBrainCorpseArtwork);

        head.Health = 0x0bb8;
        // `$A9:8714` gives the head enemy the dummy list for life: its $A320 frame is the
        // head's collision hitbox. The visible brain runs a separate list (`$A9:8734`).
        head.CurrentInstruction = MotherBrainBodyInstructionProgramDefinitions.InitialDummy;
        head.InstructionTimer = NativeCountdown.Start(1);
        head.VramTilesIndex = 0;
        head.Properties = head.Properties.With(
            EnemyProperties.BlocksPlasmaBeam | EnemyProperties.Invisible);
        head.PaletteIndex = EnemyPaletteBits.Palette1;
        state.Head = head;
        state.NeckPaletteIndex = EnemyPaletteBits.Palette1;
        state.BrainPaletteIndex = EnemyPaletteBits.Palette1;
        SetMotherBrainBrainInstructionList(state, MotherBrainInitialHeadInstruction);

        // SetupMotherBrainHeadNormalPalette installs a ten-frame timer. Palette table
        // interpolation belongs to the later damage/phase slice, but the state producer is
        // already real here and therefore starts with the cartridge's exact value.
        state.BrainPaletteTimer = 0x000a;
    }

    /// <summary>Ports the common body main/hurt entry at <c>$A9:873E</c>.</summary>
    internal void RunMotherBrainBodyMain(
        RoomEnemySlot body,
        SamusState? samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        MotherBrainEnemyState state = RequireCompleteMotherBrainState(body);

        // `$A9:873E` advances the independent room-palette instruction list before it
        // dispatches the body function. Keeping that order is visible on the exact frame
        // the fake-death flash begins and later when the main tube stops the loop.
        RunMotherBrainRoomPalette(state);
        RunMotherBrainBodyFunction(state, samus, sharedProjectiles);
        // Every body function returns through the common contact tail. The first-phase
        // function also has its own authored contact call; retain both native sites.
        if (samus is not null)
            ResolveMotherBrainSamusCollision(state, samus);
    }

    /// <summary>Dispatches the active body phase while preserving the phase-specific timer and attack order.</summary>
    private void RunMotherBrainBodyFunction(
        MotherBrainEnemyState state,
        SamusState? samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        switch (state.Function)
        {
            case MotherBrainBodyFunction.FirstPhase:
                RunMotherBrainFirstPhase(state, samus);
                return;
            case MotherBrainBodyFunction.FakeDeathDescentInitialPause:
            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock:
            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeMusic:
            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeUnlock:
            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeFlash:
            case MotherBrainBodyFunction.FakeDeathDescentFadeToGray:
            case MotherBrainBodyFunction.FakeDeathDescentCollapseTubes:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows4And5:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows6And7:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows8And9:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRowsAAndB:
            case MotherBrainBodyFunction.FakeDeathAscentDrawRowsCAndD:
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics:
                RunMotherBrainFakeDeath(state, samus);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain:
            case MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense:
            case MotherBrainBodyFunction.FakeDeathAscentPrepareForRising:
            case MotherBrainBodyFunction.FakeDeathAscentLoadLegTiles:
            case MotherBrainBodyFunction.FakeDeathAscentContinuePausing:
            case MotherBrainBodyFunction.FakeDeathAscentStartMusicAndEarthquake:
            case MotherBrainBodyFunction.FakeDeathAscentRaiseMotherBrain:
            case MotherBrainBodyFunction.FakeDeathAscentWaitUntilUncrouched:
            case MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray:
            case MotherBrainBodyFunction.SecondPhaseStretchingShakeHead:
            case MotherBrainBodyFunction.SecondPhaseStretchingBringHeadUp:
            case MotherBrainBodyFunction.SecondPhaseStretchingFinish:
            case MotherBrainBodyFunction.SecondPhaseThinking:
            case MotherBrainBodyFunction.SecondPhaseTryAttack:
            case MotherBrainBodyFunction.SecondPhaseBombDecideWalking:
            case MotherBrainBodyFunction.SecondPhaseBombWalkingBackwards:
            case MotherBrainBodyFunction.SecondPhaseBombCrouch:
            case MotherBrainBodyFunction.SecondPhaseBombFired:
            case MotherBrainBodyFunction.SecondPhaseBombStandUp:
            case MotherBrainBodyFunction.SecondPhaseLaserPositionHeadQuickly:
            case MotherBrainBodyFunction.SecondPhaseLaserPositionHeadSlowlyAndFire:
            case MotherBrainBodyFunction.SecondPhaseLaserFinishAttack:
            case MotherBrainBodyFunction.SecondPhaseHandBeam:
            case MotherBrainBodyFunction.SecondPhaseRainbowExtendNeck:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartCharging:
            case MotherBrainBodyFunction.SecondPhaseRainbowRetractNeck:
            case MotherBrainBodyFunction.SecondPhaseRainbowWaitForCharge:
            case MotherBrainBodyFunction.SecondPhaseRainbowExtendNeckDown:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartFiring:
            case MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall:
            case MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus:
            case MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus:
            case MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring:
            case MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall:
            case MotherBrainBodyFunction.SecondPhaseRainbowWaitForSamusToLand:
            case MotherBrainBodyFunction.SecondPhaseRainbowLowerHead:
            case MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOff:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffStandUp:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffAdmire:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffChargeFinalBeam:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffLoadBabyTiles:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam:
            case MotherBrainBodyFunction.SecondPhaseFinalRainbowBeamHolding:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyRainbowBeamRunOut:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyMoveToBackOfRoom:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyGoIntoLowPowerMode:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyPrepareTransitionToGrey:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyTransitionToGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveInanimateGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveShowSignsOfLife:
            case MotherBrainBodyFunction.SecondPhaseReviveTransitionFromGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveWakeUp:
            case MotherBrainBodyFunction.SecondPhaseReviveWakeUpStretch:
            case MotherBrainBodyFunction.SecondPhaseReviveWalkUpToBaby:
            case MotherBrainBodyFunction.SecondPhaseRevivePrepareNeckForBabyDeath:
            case MotherBrainBodyFunction.SecondPhaseReviveFinishPreparingForBabyDeath:
            case MotherBrainBodyFunction.SecondPhaseMurderBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseMurderBabyAttackCooldown:
            case MotherBrainBodyFunction.SecondPhasePrepareForFinalBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseExecuteFinalBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseFinalBabyAttackHolding:
            case MotherBrainBodyFunction.ThirdPhaseRecoverMakeSomeDistance:
            case MotherBrainBodyFunction.ThirdPhaseRecoverSetupForFighting:
            case MotherBrainBodyFunction.ThirdPhaseFightingMain:
            case MotherBrainBodyFunction.ThirdPhaseFightingAttackCooldown:
            case MotherBrainBodyFunction.ThirdPhaseDeathMoveToBackOfRoom:
            case MotherBrainBodyFunction.ThirdPhaseDeathIdleWhilstExploding:
            case MotherBrainBodyFunction.ThirdPhaseDeathStumbleToMiddleOfRoom:
            case MotherBrainBodyFunction.ThirdPhaseDeathDisableBrainEffects:
            case MotherBrainBodyFunction.ThirdPhaseDeathSetupBodyFadeOut:
            case MotherBrainBodyFunction.ThirdPhaseDeathFadeOutBody:
            case MotherBrainBodyFunction.ThirdPhaseDeathFinalFewExplosions:
            case MotherBrainBodyFunction.ThirdPhaseDeathRealizeDecapitation:
            case MotherBrainBodyFunction.ThirdPhaseDeathBrainFallsToGround:
            case MotherBrainBodyFunction.ThirdPhaseDeathLoadCorpseTiles:
            case MotherBrainBodyFunction.ThirdPhaseDeathSetupFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseTipsOver:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseRotsAway:
            case MotherBrainBodyFunction.ThirdPhaseDeath20FrameDelay:
            case MotherBrainBodyFunction.ThirdPhaseDeathLoadEscapeTimerTiles:
            case MotherBrainBodyFunction.ThirdPhaseDeathStartEscape:
            case MotherBrainBodyFunction.ThirdPhaseDeathSpawnTimeBombSetSubtitle:
            case MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText:
            case MotherBrainBodyFunction.ThirdPhaseDeathDoorExplodingStartTimer:
            case MotherBrainBodyFunction.ThirdPhaseDeathBlowUpEscapeDoor:
            case MotherBrainBodyFunction.ThirdPhaseDeathKeepEarthquakeGoing:
                RunMotherBrainPhaseTwoAscent(
                    state,
                    samus,
                    sharedProjectiles);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain body function $A9:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Updates the glass-bound first phase and advances its scripted reaction to Samus.</summary>
    private void RunMotherBrainFirstPhase(MotherBrainEnemyState state, SamusState? samus)
    {
        // Before PLM `$D6DE` finishes the nineteen missile increments and its 48-frame tail,
        // event two is clear and `$A9:87E1` performs no phase mutation. Head-vs-Samus and
        // projectile interactions are separate collision passes and remain outside this
        // body-function dispatcher.
        bool glassDestroyed = _room.Services.Events.Has(EventNumber.MotherBrainGlassDestroyed);
        if (glassDestroyed)
        {
            state.BrainMainShakeTimer = NativeCountdown.FromRaw(_earthquake.EarthquakeTimer.RawValue);
            if (samus is not null && samus.Position.X.Whole < 0x00ec && state.Head!.Health == 0)
            {
                // This is the exact first-phase exit at `$A9:87FE-$8814`. The following
                // function remains represented by its native pointer so the next frame
                // enters the real timed descent rather than jumping to standing form.
                state.DeleteTurretsAndRinkas = true;
                // Mother Brain and Rinka alias native slot-zero variable $1D. Publish the
                // shared word before later enemy slots can keep drawing replaced tile art.
                _rinkaBehavior._rinkaTerminationFlag = 1;
                state.Form = 1;
                state.RequestMusic(MusicCommand.SelectTrack(6), MusicCommandDelay.EightFrames);

                // MotherBrain_SealWall at `$AD:E396` creates both dust puffs before its
                // two identical hardcoded PLMs. Requests remain ordered because the bank
                // $84 allocator searches the same descending pool for each call.
                _projectiles.SpawnRoomGraphicsDustExplosion(new(248, 72), animationIndex: 9);
                _projectiles.SpawnRoomGraphicsDustExplosion(new(248, 152), animationIndex: 9);
                state.RequestPlm(new(15, 4), PlmHeaderId.FillMotherBrainsWall);
                state.RequestPlm(new(15, 9), PlmHeaderId.FillMotherBrainsWall);
                state.Function = MotherBrainBodyFunction.FakeDeathDescentInitialPause;
            }
        }

        // `$A9:87E1` always falls through to the encounter's custom rectangle walker,
        // including the frame that arms fake death. This collision is not represented by
        // either enemy header's ordinary radius and must stay in the body dispatcher.
        if (samus is not null)
            ResolveMotherBrainSamusCollision(state, samus);
    }

    /// <summary>Ports the head main/hurt entry and its first-phase draw-hook selector.</summary>
    private void RunMotherBrainHeadMain(RoomEnemySlot head, SamusState? samus)
    {
        MotherBrainEnemyState state = RequireCompleteMotherBrainState(head);

        // `$A9:878B` first restores the global enemy-graphics-drawn hook to RTL. Only an
        // intentionally invisible head dispatches the shared brain function and replaces it.
        state.DrawBrain = false;
        state.DrawNeck = false;
        if (!head.Properties.HasAny(EnemyProperties.Invisible))
            return;

        switch (state.BrainFunction)
        {
            case MotherBrainBrainFunction.SetupBrainToBeDrawn:
                state.DrawBrain = true;
                return;
            case MotherBrainBrainFunction.SetupBrainAndNeckToBeDrawn:
                StepMotherBrainNeck(
                    state,
                    samus ?? throw new InvalidOperationException(
                        "Mother Brain's articulated neck requires the live Samus actor."));
                head.Position = head.Position.WithWhole(new(state.NeckSegment4.X, unchecked((ushort)(state.NeckSegment4.Y - 21))));
                state.DrawBrain = true;
                state.DrawNeck = true;
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain brain function $A9:{(ushort)state.BrainFunction:X4} is not translated.");
        }
    }

    /// <summary>
    /// Executes the hook installed by <c>$A9:87D0</c> after the ordinary enemy queues have
    /// been written. The source is an ordinary bank-$A9 spritemap even though the population
    /// record carries extended-spritemap bit <c>$0004</c>; routing it through the generic
    /// extended parser is precisely the corruption the cartridge hook avoids.
    /// </summary>
    internal void DrawMotherBrainHook(OamBuffer oam, Position16<RoomPixels> camera)
    {
        MotherBrainEnemyState? state = _motherBrainEncounter.MotherBrain;
        RoomEnemySlot? head = state?.Head;
        if (state?.DrawBrain != true || head is null)
            return;
        // `$A9:9357-$936B` samples (never advances) the RNG before reading the brain frame,
        // so the small breath can spawn even on a draw that then shows no brain.
        if (state.SmallPurpleBreathGenerationEnabled && !state.SmallPurpleBreathActive &&
            (_room.Random.Read() & 0x8000) == 0)
            SpawnMotherBrainPurpleBreathSmall(state);
        // `$A9:936F` advances the brain list here, after every enemy's AI, and returns out
        // of the whole draw when the list is not live.
        if (AdvanceMotherBrainBrainInstructions(state) is not ushort brainSpritemap)
            return;

        Offset16<RoomPixels> brainShake = GetMotherBrainBrainShake(state, head);
        // The caller must cull before the nine-bit OAM coordinate loses world-space bits.
        // Otherwise a brain half a room to the left reappears in the approach hallway.
        if (unchecked((short)(head.Position.X.Whole + brainShake.X +
                MotherBrainDrawData.BrainLeftCullMargin - camera.X)) >= 0)
        DrawMotherBrainWorldSpritemap(
            oam,
            brainSpritemap,
            head.Position.Whole + brainShake,
            // The custom draw hook bypasses ordinary enemy palette selection. Retail
            // still selects the shared white hurt palette on odd flash-timer frames.
            (head.FlashTimer.RawValue & 1) != 0 ? (ushort)0 : state.BrainPaletteIndex,
            head.VramTilesIndex,
            camera);

        if (!state.DrawNeck || state.Body.Properties.HasAny(EnemyProperties.Invisible))
            return;

        // `$A9:930C-$9354` emits the joint nearest the brain first and the body-side joint
        // last. Equal-priority overlap therefore follows the same low-OAM precedence.
        int shakeIndex = (head.FlashTimer.RawValue & 6) >> 1;
        Offset16<RoomPixels> neckShake = MotherBrainShakeOffsets[shakeIndex];
        Position16<RoomPixels>[] segments =
        [
            state.NeckSegment0,
            state.NeckSegment1,
            state.NeckSegment2,
            state.NeckSegment3,
            state.NeckSegment4,
        ];
        for (int index = segments.Length - 1; index >= 0; index--)
        {
            Position16<RoomPixels> segment = segments[index];
            DrawMotherBrainWorldSpritemap(
                oam,
                spritemapPointer: 0xa694,
                segment + neckShake,
                state.NeckPaletteIndex,
                head.VramTilesIndex,
                camera);
        }
    }

    /// <summary>Four authored jitter offsets selected by the low shake-state bits.</summary>
    private static readonly Offset16<RoomPixels>[] MotherBrainShakeOffsets =
        [new(0, 0), new(-1, 1), new(0, -1), new(1, 1)];

    /// <summary>Draws a selected world-space Mother Brain frame with palette and camera offsets.</summary>
    internal void DrawMotherBrainWorldSpritemap(
        OamBuffer oam,
        ushort spritemapPointer,
        Position16<RoomPixels> world,
        ushort paletteIndex,
        ushort vramTilesIndex,
        Position16<RoomPixels> camera)
    {
        // Mother Brain's private `$A9:93EE` writer rejects an entire map whose center begins
        // above the screen. The generic writer intentionally wraps, so preserve that unique
        // vertical gate here before sharing its byte-exact map decoder.
        Position16<ScreenPixels> screen = world.ToScreen(camera);
        if (unchecked((short)screen.Y) < 0)
            return;

        _presentation.DrawEnemySpritemap(oam, new EnemySpritemapDraw { Bank = MotherBrainVisualDefinitions.Bank, Pointer = spritemapPointer, Origin = screen.ToOam(), PaletteBits = paletteIndex, BaseTileIndex = vramTilesIndex });
    }

    /// <summary>Derives the head's current shake offset from the encounter's animation state.</summary>
    private static Offset16<RoomPixels> GetMotherBrainBrainShake(
        MotherBrainEnemyState state,
        RoomEnemySlot head)
    {
        ushort shake;
        if (!state.BrainMainShakeTimer.IsZero)
        {
            state.BrainMainShakeTimer = state.BrainMainShakeTimer.Decrement().Remaining;
            shake = state.BrainMainShakeTimer.RawValue;
        }
        else
        {
            shake = !head.FlashTimer.IsZero ? head.FlashTimer.RawValue : head.ShakeTimer.RawValue;
        }

        int index = (shake & 6) >> 1;
        return MotherBrainShakeOffsets[index];
    }

    /// <summary>Moves neck segments toward the head and updates their attachment to Samus's position.</summary>
    private static void StepMotherBrainNeck(MotherBrainEnemyState state, SamusState samus)
    {
        if (state.NeckMovementEnabled)
        {
            ushort lowerAngle = state.LowerNeckAngle;
            ushort upperAngle = state.UpperNeckAngle;
            MotherBrainNeckMovement lowerIndex = state.LowerNeckMovementIndex;
            MotherBrainNeckMovement upperIndex = state.UpperNeckMovementIndex;
            MotherBrainNeckKinematics.StepAngles(
                ref lowerAngle,
                ref upperAngle,
                ref lowerIndex,
                ref upperIndex,
                state.NeckAngleDelta,
                state.Head!.Position.Y.Whole,
                samus.Position.Y.Whole);
            state.LowerNeckAngle = lowerAngle;
            state.UpperNeckAngle = upperAngle;
            state.LowerNeckMovementIndex = lowerIndex;
            state.UpperNeckMovementIndex = upperIndex;
        }

        MotherBrainNeckGeometry geometry = MotherBrainNeckKinematics.CalculateGeometry(state.Body.Position.Whole, state.LowerNeckAngle, state.UpperNeckAngle, new MotherBrainNeckSegmentDistances { Segment0Distance = state.NeckSegment0Distance, Segment1Distance = state.NeckSegment1Distance, Segment2Distance = state.NeckSegment2Distance, Segment3Distance = state.NeckSegment3Distance, Segment4Distance = state.NeckSegment4Distance });
        state.NeckSegment0 = geometry.Segment0;
        state.NeckSegment1 = geometry.Segment1;
        state.NeckSegment2 = geometry.Segment2;
        state.NeckSegment3 = geometry.Segment3;
        state.NeckSegment4 = geometry.Segment4;
    }

    /// <summary>Ports <c>SetMotherBrainHeadInstList</c> ($A9:C447) for the brain's own list.</summary>
    private static void SetMotherBrainBrainInstructionList(MotherBrainEnemyState state, ushort pointer)
    {
        state.BrainInstructionPointer = pointer;
        state.BrainInstructionTimer = 1;
    }

    /// <summary>
    /// Ports <c>GetMotherBrainHeadSpritemapPointerInY</c> ($A9:92AF): returns the brain frame
    /// to draw, or null when the list is not live. With time frozen it only reads the current
    /// frame. Otherwise a frame holds while its duration minus the timer is not negative
    /// (CMP/BPL), the timer counting up; past it, commands run until the next frame, which
    /// resets the timer to one.
    /// </summary>
    internal ushort? AdvanceMotherBrainBrainInstructions(MotherBrainEnemyState state)
    {
        RoomEnemySlot head = state.Head ?? throw new InvalidOperationException(
            "Mother Brain's brain list ran without its head enemy.");
        ushort pointer = state.BrainInstructionPointer;
        if ((pointer & 0x8000) == 0)
            return null;
        if (_frameContext._enemyFrameTimeIsFrozen)
            return EnemyInstructionInterpreter.ReadEnemyVisualSelector(head, unchecked((ushort)(pointer + 2)));

        ushort word = EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(head, pointer);
        if ((word & 0x8000) == 0)
        {
            if (unchecked((short)(word - state.BrainInstructionTimer)) >= 0)
            {
                state.BrainInstructionTimer = unchecked((ushort)(state.BrainInstructionTimer + 1));
                return EnemyInstructionInterpreter.ReadEnemyVisualSelector(head, unchecked((ushort)(pointer + 2)));
            }
            pointer = unchecked((ushort)(pointer + 4));
        }
        while (true)
        {
            word = EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(head, pointer);
            if ((word & 0x8000) == 0)
            {
                state.BrainInstructionTimer = 1;
                state.BrainInstructionPointer = pointer;
                return EnemyInstructionInterpreter.ReadEnemyVisualSelector(head, unchecked((ushort)(pointer + 2)));
            }
            ushort cursor = pointer;
            if (!TryProcessMotherBrainInstruction(head, _deathsAndDrops._samusForEnemyDrops, word, ref cursor))
            {
                throw new InvalidDataException(
                    $"Mother Brain brain-list opcode $A9:{word:X4} at $A9:{pointer:X4} is not translated.");
            }
            pointer = cursor;
        }
    }

    /// <summary>Handles Mother Brain's private instruction opcodes used by phase-one art.</summary>
    private bool TryProcessMotherBrainInstruction(
        RoomEnemySlot slot,
        SamusState? samus,
        ushort instruction,
        ref ushort cursor)
    {
        // The shared interpreter hands every remaining negative word here; only
        // Mother Brain's private opcodes belong to this handler.
        if (!IsMotherBrainDefinition(slot.EnemyDefinitionPointer) ||
            !Enum.IsDefined((MotherBrainInstruction)instruction))
            return false;

        switch ((MotherBrainInstruction)instruction)
        {
            case MotherBrainInstruction.MotherBrain_GotoX:
                cursor = EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                    slot, unchecked((ushort)(cursor + 2)));
                return true;

            case MotherBrainInstruction.MotherBrainHead_EnableNeckMovement_GotoX:
                RequireCompleteMotherBrainState(slot).NeckMovementEnabled = true;
                cursor = EnemyInstructionInterpreter.ReadEnemyInstructionMechanicsWord(
                    slot, unchecked((ushort)(cursor + 2)));
                return true;

            // `$95B6-$95F2` are the six posture-transition displacements. Each command
            // moves the body vertically, counter-scrolls BG2 by the opposite amount, and
            // derives BG2 X from the body's new origin plus its authored horizontal bias.
            case MotherBrainInstruction.MotherBrainBody_MoveBodyUpBy10_ScrollLeftBy4:
                MoveMotherBrainBodyWithScrollBias(-10, 4);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyUpBy16_ScrollLeftBy4:
                MoveMotherBrainBodyWithScrollBias(-16, 4);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyUpBy12_ScrollRightBy2:
                MoveMotherBrainBodyWithScrollBias(-12, -2);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyDownBy12_ScrollLeftBy4:
                MoveMotherBrainBodyWithScrollBias(12, 4);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyDownBy16_ScrollRightBy2:
                MoveMotherBrainBodyWithScrollBias(16, -2);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyDownBy10_ScrollRightBy2:
                MoveMotherBrainBodyWithScrollBias(10, -2);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            // The walk-cycle comments in the historical disassembly describe visual
            // motion and are occasionally opposite the literal signed Y operand. These
            // cases preserve the actual additions performed by `$9579`, not the labels.
            case MotherBrainInstruction.MotherBrainBody_MoveBodyUpBy2_ScrollRightBy1:
                MoveMotherBrainBody(new(1, -2));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyRightBy2:
                MoveMotherBrainBody(new(2, 0));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyUpBy1:
                MoveMotherBrainBody(new(0, 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyUpBy1_RightBy3_Footstep:
                RunMotherBrainFootstep();
                MoveMotherBrainBody(new(3, 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyDownBy2_RightBy15:
                MoveMotherBrainBody(new(15, -2));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyDownBy4_RightBy6:
                MoveMotherBrainBody(new(6, -4));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyUpBy4_LeftBy2:
                MoveMotherBrainBody(new(-2, 4));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep:
                RunMotherBrainFootstep();
                MoveMotherBrainBody(new(-1, 2));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep_d:
                RunMotherBrainFootstep();
                MoveMotherBrainBody(new(-1, 2));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyLeftBy2:
                MoveMotherBrainBody(new(-2, 0));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyDownBy1:
                MoveMotherBrainBody(new(0, -1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyDownBy1_LeftBy3:
                MoveMotherBrainBody(new(-3, -1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyUpBy2_LeftBy15_Footstep:
                RunMotherBrainFootstep();
                MoveMotherBrainBody(new(-15, 2));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyUpBy4_LeftBy6:
                MoveMotherBrainBody(new(-6, 4));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyDownBy4_RightBy2:
                MoveMotherBrainBody(new(2, -4));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_MoveBodyDownBy2_RightBy1:
                MoveMotherBrainBody(new(1, -2));
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            // Pose is shared encounter state, not a property of the displayed map. Body AI
            // waits on these exact values while the instruction list continues independently.
            case MotherBrainInstruction.MotherBrainBody_SetPoseToStanding:
                RequireCompleteMotherBrainState(slot).Pose = MotherBrainBodyPose.Standing;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_SetPoseToWalking:
                RequireCompleteMotherBrainState(slot).Pose = MotherBrainBodyPose.Walking;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_SetPoseToCrouching:
                RequireCompleteMotherBrainState(slot).Pose = MotherBrainBodyPose.Crouched;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_SetPoseToCrouchingTransition:
                RequireCompleteMotherBrainState(slot).Pose = MotherBrainBodyPose.CrouchingTransition;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_SetPoseToDeathBeamMode:
                RequireCompleteMotherBrainState(slot).Pose = MotherBrainBodyPose.DeathBeam;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_SetPoseToLeaningDown:
                RequireCompleteMotherBrainState(slot).Pose = MotherBrainBodyPose.LeaningDown;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case MotherBrainInstruction.MotherBrainHead_DisableNeckMovement:
                RequireCompleteMotherBrainState(slot).NeckMovementEnabled = false;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib2_Max6:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                state.LastSoundEffect = MotherBrainHeadInstructionProgramDefinitions.ReadWord(
                    unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            }
            case MotherBrainInstruction.MotherBrainHead_QueueSoundX_Lib3_Max6:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                state.LastSoundEffectLibrary3 = MotherBrainHeadInstructionProgramDefinitions.ReadWord(
                    unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            }
            case MotherBrainInstruction.MotherBrainHead_SpawnDroolProjectile:
                SpawnMotherBrainDrool(RequireCompleteMotherBrainState(slot));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainHead_SpawnPurpleBreathBigProjectile:
                SpawnMotherBrainPurpleBreathBig(RequireCompleteMotherBrainState(slot));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainHead_SetMainShakeTimerTo50:
                RequireCompleteMotherBrainState(slot).BrainMainShakeTimer = NativeCountdown.Start(50);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainBody_SpawnDustCloudExplosionProj:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                short xOffset = unchecked((short)
                    MotherBrainHandBeamBodyInstructionDefinitions.ReadMechanicsWord(
                        unchecked((ushort)(cursor + 2))));
                short yOffset = unchecked((short)
                    MotherBrainHandBeamBodyInstructionDefinitions.ReadMechanicsWord(
                        unchecked((ushort)(cursor + 4))));
                ushort animationIndex =
                    MotherBrainHandBeamBodyInstructionDefinitions.ReadMechanicsWord(
                        unchecked((ushort)(cursor + 6)));
                _projectiles.SpawnRoomGraphicsDustExplosion(
                    new(unchecked((ushort)(state.Body.Position.X.Whole + xOffset)), unchecked((ushort)(state.Body.Position.Y.Whole + yOffset))),
                    animationIndex);
                cursor = unchecked((ushort)(cursor + 8));
                return true;
            }
            case MotherBrainInstruction.MotherBrainBody_SpawnDeathBeamProjectile:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Mother Brain hand-beam aiming requires the active Samus actor.");
                }
                state.LastSoundEffect = 0x0063;
                SpawnMotherBrainHandBeamCharging(state, samus);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            case MotherBrainInstruction.MotherBrainBody_IncrementDeathBeamAttackPhase:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                state.HandBeamPhase = MotherBrainHandBeamPhases.Next(state.HandBeamPhase);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            case MotherBrainInstruction.MotherBrainHead_MaybeGotoNeutralPhase2:
                cursor = _room.Random.Read() < 0xf000
                    ? (ushort)0x9c9f
                    : unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainHead_GotoDyingDroolInstList:
            {
                // `$A9:9C65-$9C76` returns its destination in X rather than storing an
                // operand beside the opcode. The low twelve random bits select `$9C47`
                // only for `$FE0-$FFF`; every other value returns the one-frame `$9C5F`
                // loop. Assigning the cursor directly preserves that control convention.
                ushort random = _room.Random.Read();
                cursor = (random & 0x0fff) >= 0x0fe0
                    ? (ushort)0x9c47
                    : (ushort)0x9c5f;
                return true;
            }
            case MotherBrainInstruction.MotherBrainHead_IncBabyMetroidAttackCounter:
                RequireLiveMotherBrainRainbowSequence(slot).
                    IncrementLiveBabyMetroidAttackCounter();
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainHead_ResetBabyMetroidAttackCounter:
                RequireLiveMotherBrainRainbowSequence(slot).
                    ResetLiveBabyMetroidAttackCounter();
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainHead_AimOnionRingsAtBabyMetroid:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                BabyMetroidCutsceneState baby = state.BabyMetroid ??
                    throw new InvalidOperationException(
                        "Mother Brain's Baby-targeting opcode ran without the Baby actor.");
                AimMotherBrainOnionRingsAt(state, baby.Position.Whole);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            case MotherBrainInstruction.MotherBrainHead_AimOnionRingsAtSamus:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                SamusState target = samus ?? throw new InvalidOperationException(
                    "Mother Brain onion-ring aiming requires the active Samus actor.");
                AimMotherBrainOnionRingsAt(state, target.Position.Whole);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            case MotherBrainInstruction.MotherBrainHead_QueueBabyMetroidAttackSFX:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                MotherBrainRainbowBeamAttackSequence sequence =
                    RequireLiveMotherBrainRainbowSequence(slot);
                if (sequence.BabyMetroidAttackCounter != 0x000b)
                    state.LastSoundEffect = 0x006f;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            case MotherBrainInstruction.MotherBrainHead_MaybeGotoNeutralPhase3:
                // Retail contains an unconditional BRA where the adjacent commentary might
                // suggest a carry branch. Low-twelve values below `$EC0` replace X with the
                // hold origin; the remaining values simply continue after this opcode.
                cursor = (_room.Random.Read() & 0x0fff) < 0x0ec0
                    ? (ushort)0x9cd1
                    : unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainHead_SpawnOnionRingsProjectile:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                SpawnMotherBrainOnionRing(state, unchecked((byte)state.OnionRingsTargetAngle));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            case MotherBrainInstruction.MotherBrainHead_SpawnBombProjectileWithParamX:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                ushort afterburnCount = MotherBrainHeadInstructionProgramDefinitions.ReadWord(
                    unchecked((ushort)(cursor + 2)));
                SpawnMotherBrainBomb(state, afterburnCount);
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            }
            case MotherBrainInstruction.InstList_MotherBrainHead_SpawnLaserProjectile:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                RoomEnemySlot head = state.Head!;
                state.NeckMovementEnabled = false;

                // `$A9:9F4E-$9F66` writes an explicit mouth coordinate into the common
                // bank-$86 spawn scratch words, selects direction one (right), and invokes
                // definition `$A17B`. The source header remains the head record, so shared
                // initializer `$86:A009` obtains Mother Brain's authored contact damage and
                // her parameter-one speed flag without any encounter-specific substitute.
                RoomEnemyProjectileSlot? laser = _walkingSpacePirateBehavior.SpawnPirateMotherBrainLaser(
                    head,
                    head.Position.Whole + new Offset16<RoomPixels>(0x0010, 0x0004),
                    movingRight: true);
                if (laser is not null)
                    state.LastSoundEffect = WalkingSpacePirateBehavior.PirateMotherBrainLaserSound;

                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            case MotherBrainInstruction.MotherBrainHead_SpawnRainbowBeamChargingProj:
                // The spawn reads no encounter state, but the instruction still requires one.
                _ = RequireCompleteMotherBrainState(slot);
                SpawnMotherBrainRainbowChargingProjectile();
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstruction.MotherBrainHead_SetupEffectsForRainbowBeamCharge:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                state.SmallPurpleBreathGenerationEnabled = false;
                state.BrainPaletteTimer = 0x0202;
                state.LastSoundEffect = 0x007f;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            default:
                throw new InvalidOperationException($"Unhandled MotherBrainInstruction {(MotherBrainInstruction)instruction}.");
        }
    }

    /// <summary>
    /// Shared `$A9:9E37/$9E5B` tail. Both target selectors subtract the same mouth offsets,
    /// call the cartridge angle routine, rotate by `$80`, and clamp the launch arc to
    /// `$10..$48`; only the source coordinate differs.
    /// </summary>
    private static void AimMotherBrainOnionRingsAt(
        MotherBrainEnemyState state,
        Position16<RoomPixels> target)
    {
        RoomEnemySlot head = state.Head!;
        short deltaX = unchecked((short)(target.X - head.Position.X.Whole - 0x000a));
        short deltaY = unchecked((short)(target.Y - head.Position.Y.Whole - 0x0010));
        byte angle = unchecked((byte)(0x80 - EnemyMath.CalculateCartridgeAngle(new(deltaX, deltaY))));

        // `$9E86-$9E98` performs two signed-flag comparisons in 8-bit mode. The
        // wraparound half maps $C0..FF and $00..0F to $10; the opposite half maps
        // $48..BF to $48. Only $10..47 survives unchanged.
        state.OnionRingsTargetAngle = angle switch
        {
            >= 0x10 and < 0x48 => angle,
            >= 0x48 and < 0xc0 => 0x0048,
            _ => 0x0010,
        };
    }

    /// <summary>Requires the active rainbow attack sequence for live phase transitions.</summary>
    private MotherBrainRainbowBeamAttackSequence RequireLiveMotherBrainRainbowSequence(
        RoomEnemySlot slot) =>
        RequireCompleteMotherBrainState(slot).RainbowBeamSequence ??
        throw new InvalidDataException(
            "Mother Brain head bytecode requires the live rainbow/Baby sequence.");

    /// <summary>Requires a body and head pair initialized in their native enemy slots.</summary>
    private MotherBrainEnemyState RequireCompleteMotherBrainState(RoomEnemySlot slot)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ??
            throw new InvalidDataException("Mother Brain record has no shared encounter state.");
        if (state.Head is null)
            throw new InvalidDataException("Mother Brain's body/head population is incomplete.");
        if (!ReferenceEquals(slot, state.Body) && !ReferenceEquals(slot, state.Head))
            throw new InvalidDataException("A non-Mother-Brain slot entered the Mother Brain dispatcher.");
        return state;
    }

    /// <summary>Installs the handlers used when Samus's bombs contact Mother Brain.</summary>
    internal void RegisterMotherBrainBombReactions(EnemyAiTable table)
    {
        table.AddBombSpecial(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainHead &&
                                  c.SelectedShotAi == EnemyInteractionCallback.MotherBrainHeadShot,
            c => ResolveMotherBrainHeadNormalBomb(c.Enemy, c.Bomb));
    }

    /// <summary>Registers the encounter-owned bombs, glass, beams, rings, turrets, tubes, and effects.</summary>
    internal void RegisterMotherBrainProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunMotherBrainTurretPreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.MotherBrainsTurrets);

        table.RegisterPreInstruction(context =>
        {
            RunMotherBrainTurretBulletPreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.MotherBrainsTurretBullets);

        table.RegisterPreInstruction(context =>
        {
            RunMotherBrainGlassShardPreInstruction(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainGlassShattering_Shard);

        table.RegisterPreInstruction(context =>
        {
            RunMotherBrainTopTubePreInstruction(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainsTubeFalling);

        table.RegisterPreInstruction(context =>
        {
            RunMotherBrainAttachedDroolPreInstruction(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainsDrool);

        table.RegisterPreInstruction(context =>
        {
            RunMotherBrainFallingDroolPreInstruction(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainsDrool_Falling);

        table.RegisterPreInstruction(context =>
        {
            RunMotherBrainOnionRingPreInstruction(
                context.Projectile,
                context.Samus,
                context.Camera.X);
        }, EnemyProjectilePreInstruction.MotherBrainsOnionRings);

        table.RegisterPreInstruction(context =>
        {
            RunMotherBrainBombPreInstruction(context.Projectile, context.SamusBombs);
        }, EnemyProjectilePreInstruction.MotherBrainsBomb);

        table.RegisterPreInstruction(context =>
        {
            RunMotherBrainRainbowChargingPreInstruction(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainRainbowBeam_Charging);

        table.RegisterPreInstruction(context =>
        {
            RunMotherBrainRainbowExplosionPreInstruction(context.Projectile, context.Samus);
        }, EnemyProjectilePreInstruction.MotherBrainsRainbowBeamExplosion);

        table.RegisterPreInstruction(context =>
        {
            RunMotherBrainDeathExplosion(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainDeathExplosionPreInstruction);

        table.RegisterPreInstruction(context =>
        {
            RunMotherBrainDoorFragment(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainDeathDoorFragmentPreInstruction);

        table.RegisterPreInstruction(context =>
        {
            PinMotherBrainEscapeSubtitle(context.Projectile);
        }, EnemyProjectilePreInstruction.MotherBrainDeathSubtitlePreInstruction);

        table.RegisterInstruction(EnemyProjectileInstruction.MotherBrainPurpleBreath_Inactive, projectile => projectile.Kind == RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall,
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                (_motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
                    "Mother Brain's small purple breath ran without its encounter state."))
                    .SmallPurpleBreathActive = false;
                cursor = unchecked((ushort)(cursor + 2));
                return EnemyProjectileInstructionOutcome.Continue;
            });

        table.RegisterInstruction(EnemyProjectileInstruction.CallExternalFunctionInY, null,
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                if (!MotherBrainHandBeamInstructionProgramDefinitions.Owns(projectile.Kind))
                {
                    throw new InvalidDataException(
                        $"Enemy projectile external function at $86:{cursor:X4} " +
                        $"is not translated for {projectile.Kind}.");
                }

                int externalFunction =
                    MotherBrainHandBeamInstructionProgramDefinitions.ReadExternalFunction(
                        cursor);
                switch (externalFunction)
                {
                    case MotherBrainHandBeamInstructionProgramDefinitions.SpawnNextCallback:
                        SpawnMotherBrainHandBeamFired(projectile.Variable0);
                        break;
                    default:
                        throw new InvalidDataException(
                            $"Mother Brain hand-beam callback ${externalFunction:X6} " +
                            $"at $86:{cursor:X4} is not translated.");
                }
                cursor = unchecked((ushort)(cursor + 5));
                return EnemyProjectileInstructionOutcome.Continue;
            });
    }

    /// <summary>Binds body and head initialization to their native enemy definitions.</summary>
    internal void RegisterMotherBrainInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_MotherBrainBody, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainBody, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeMotherBrainBody(slot);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_MotherBrainHead, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainHead, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeMotherBrainHead(slot);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_MotherBrainTubes, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainTubes, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeMotherBrainFallingTube(slot);
        });
    }

    /// <summary>Binds body and head frame updates to their native enemy definitions.</summary>
    internal void RegisterMotherBrainMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_HurtAI_MotherBrainBody, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainBody, (slot, frame, nmiFrameCounter8) =>
        {
            RunMotherBrainBodyMain(slot, frame.Samus, frame.SharedProjectiles);
        });

        table.RegisterMain(EnemyAiRoutine.MainAI_HurtAI_MotherBrainHead, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainHead, (slot, frame, nmiFrameCounter8) =>
        {
            RunMotherBrainHeadMain(slot, frame.Samus);
        });

        table.RegisterMain(EnemyAiRoutine.MainAI_MotherBrainTubes, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainTubes, (slot, frame, nmiFrameCounter8) =>
        {
            RunMotherBrainFallingTubeMain(slot);
        });
    }

    /// <summary>Binds encounter-specific enemy instruction commands to their handlers.</summary>
    internal void RegisterMotherBrainOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessMotherBrainInstruction(slot, context.Samus, word, ref cursor);
        });
    }

    /// <summary>Binds head and body shot responses for each combat phase.</summary>
    internal void RegisterMotherBrainShotReactions(EnemyAiTable table)
    {

        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainBody &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.MotherBrainBodyShot)
        {
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                // Every body rectangle's shot callback is `$A9:B503`, exactly CreateDudShot;
                // only the dummy root's head-callback list could select anything else.
                if (c.HitboxShotAi != EnemyInteractionCallback.MotherBrainBodyShot)
                {
                    throw new InvalidDataException(
                        $"Mother Brain's body selected shot AI $A9:{c.HitboxShotAiPointer:X4} from " +
                        $"frame $A9:{c.Enemy.SpritemapPointer:X4}; only $A9:B503 is ported.");
                }
                // The common collision prelude still owns Super-Missile quake first.
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                _projectiles.CreateEnemyProjectileDudShot(c.Projectile);
                return true;
            },
        });

        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainHead &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.MotherBrainHeadShot)
        {
            BeforeSuperMissileQuake = (in EnemyShotContext c) =>
            {
                ResolveMotherBrainHeadShot(c.Enemy, c.Projectile, c.Projectiles, c.ProjectileType, c.ProjectileDamage);
                return true;
            },
        });
    }
    /// <summary>Ports initializer <c>$86:C482-C4C7</c>.</summary>
    private void SpawnMotherBrainBomb(
        MotherBrainEnemyState state,
        ushort afterburnCount)
    {
        RoomEnemyProjectileSlot? bomb = _projectiles.AllocateEnemyProjectile();
        if (bomb is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            bomb,
            RoomEnemyProjectileKind.MotherBrainBomb,
            graphicsIndex: 0x0400);
        RoomEnemySlot head = state.Head!;
        bomb.Position = bomb.Position.WithWhole(new(unchecked((ushort)(head.Position.X.Whole + 0x000c)), unchecked((ushort)(head.Position.Y.Whole + 0x0010))));

        // The initializer's 8-bit store writes parameter LOW into the low byte of X
        // subposition. Every 8.8 motion delta has a zero low byte, so that value naturally
        // survives fractional motion and becomes the terminal Ridley-afterburn count.
        bomb.Position = bomb.Position.WithFractionX(unchecked((byte)afterburnCount)).WithFractionY(0);
        bomb.Velocity = new(new SnesSignedEightEight(0x00e0), new SnesSignedEightEight(0x0100));
        bomb.Variable0 = 0x0070; // Horizontal speed restored after each floor bounce.
        bomb.Variable1 = 0;      // Byte offset into the ten-word acceleration table.
        bomb.DirectionParameter = afterburnCount;
        state.BombCounter = unchecked((ushort)(state.BombCounter + 1));
    }

    /// <summary>Ports pre-instruction <c>$86:C4C8-C604</c>.</summary>
    internal void RunMotherBrainBombPreInstruction(
        RoomEnemyProjectileSlot bomb,
        SamusBombProjectileSystem? samusBombs)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain bomb ran without its multipart encounter state.");
        if (TryDestroyMotherBrainBombWithSamusBomb(bomb, state, samusBombs))
            return;

        ushort acceleration;
        if (bomb.Variable1 == 0)
        {
            // Only the initial fall applies two units of horizontal drag. Native takes the
            // absolute value, subtracts, clamps by the wrapped sign flag, then restores sign.
            bool movingLeft = (bomb.Velocity.X.RawValue & 0x8000) != 0;
            ushort magnitude = movingLeft
                ? unchecked((ushort)-bomb.Velocity.X.RawValue)
                : bomb.Velocity.X.RawValue;
            ushort slowed = unchecked((ushort)(magnitude - 2));
            if ((slowed & 0x8000) != 0)
                slowed = 0;
            bomb.Velocity = bomb.Velocity.WithX(new SnesSignedEightEight(movingLeft ? unchecked((ushort)-slowed) : slowed));
            acceleration = MotherBrainBombBounceDefinitions.FallAcceleration;
        }
        else
        {
            int accelerationIndex = bomb.Variable1 >> 1;
            if ((uint)accelerationIndex >= MotherBrainBombBounceDefinitions.StageCount)
            {
                throw new InvalidDataException(
                    $"Mother Brain bomb bounce-table offset ${bomb.Variable1:X4} is outside " +
                    "$86:C550-C563.");
            }

            acceleration = MotherBrainBombBounceDefinitions.YAcceleration(accelerationIndex);
            if (acceleration == 0)
            {
                ExpireMotherBrainBomb(bomb, state);
                return;
            }
        }

        if (MoveMotherBrainBomb(bomb, acceleration))
            bomb.Variable1 = unchecked((ushort)(bomb.Variable1 + 2));
    }

    /// <summary>Consumes an overlapping Samus bomb and resolves the Mother Brain bomb explosion.</summary>
    private bool TryDestroyMotherBrainBombWithSamusBomb(
        RoomEnemyProjectileSlot bomb,
        MotherBrainEnemyState state,
        SamusBombProjectileSystem? samusBombs)
    {
        if (samusBombs is null || samusBombs.BombCounter == 0)
            return false;

        foreach (SamusBombProjectileSlot samusBomb in samusBombs.Slots)
        {
            if (samusBomb.PackedType.Family != SamusProjectileFamily.Bomb ||
                !samusBomb.BombTimer.IsZero ||
                !MotherBrainBombStrictOverlap(
                    bomb.Position.Whole,
                    bomb.Radius,
                    samusBomb.Position.Whole,
                    samusBomb.Radius))
            {
                continue;
            }

            ushort x = bomb.Position.X.Whole;
            ushort y = bomb.Position.Y.Whole;
            DeleteMotherBrainBomb(bomb, state);
            _projectiles.SpawnRoomGraphicsDustExplosion(new(x, y), animationIndex: 9);
            state.LastBombDropRequest = new MotherBrainBombDropRequest();
            _deathsAndDrops.SpawnEnemyDropFromEnemyHeader(
                new(x, y),
                state.Head!.EnemyDefinitionPointer);
            return true;
        }

        return false;
    }

    /// <summary>Advances a bomb through its native trajectory and reports arena-boundary expiry.</summary>
    private static bool MoveMotherBrainBomb(
        RoomEnemyProjectileSlot bomb,
        ushort acceleration)
    {
        bomb.Velocity = bomb.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(bomb.Velocity.Y.RawValue + acceleration))));
        bomb.Position = bomb.Position.Advance(bomb.Velocity.ToSixteenSixteen());

        // Both boundaries are signed CMP/BMI tests. X reflects without clamping; the floor
        // clamps Y to $D0 and resets the two authored bounce velocities.
        if (unchecked((short)(bomb.Position.X.Whole - 0x00f0)) >= 0)
            bomb.Velocity = bomb.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-bomb.Velocity.X.RawValue)));
        if (unchecked((short)(bomb.Position.Y.Whole - 0x00d0)) < 0)
            return false;

        bomb.Position = bomb.Position.WithWholeY(0x00d0);
        bomb.Velocity = new(new SnesSignedEightEight((bomb.Velocity.X.RawValue & 0x8000) != 0
            ? unchecked((ushort)-bomb.Variable0)
            : bomb.Variable0), new SnesSignedEightEight(0xfe00));
        return true;
    }

    /// <summary>Removes an expired bomb and emits the associated explosion effect.</summary>
    private void ExpireMotherBrainBomb(
        RoomEnemyProjectileSlot bomb,
        MotherBrainEnemyState state)
    {
        Position16<RoomPixels> impact = bomb.Position.Whole;
        byte afterburnCount = unchecked((byte)bomb.Position.X.Fraction);
        DeleteMotherBrainBomb(bomb, state);

        // `$C50B-$C54B` clears the source slot before both allocations. They therefore use
        // the normal descending shared-pool competition, including legitimate same-slot
        // replacement that the outer scheduler will continue processing on this pass.
        _ridleyProjectiles.SpawnAfterburnCenter(
            RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter,
            impact,
            afterburnCount);
        _projectiles.SpawnRoomGraphicsDustExplosion(impact, animationIndex: 3);
        state.LastSoundEffectLibrary3 = 0x0013;
    }

    /// <summary>Clears a bomb slot when its native lifetime or collision ends.</summary>
    private static void DeleteMotherBrainBomb(
        RoomEnemyProjectileSlot bomb,
        MotherBrainEnemyState state)
    {
        state.BombCounter = NativeWordCounter.Decrement(state.BombCounter).Value;
        bomb.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
        bomb.Clear();
    }

    /// <summary>Uses strict hitbox intersection for bomb-to-bomb contact.</summary>
    private static bool MotherBrainBombStrictOverlap(
        Position16<RoomPixels> first,
        Extent16<RoomPixels> firstRadius,
        Position16<RoomPixels> second,
        Extent16<RoomPixels> secondRadius) =>
        EnemyMath.WrappedMagnitude(unchecked((ushort)(first.X - second.X))) <
            firstRadius.X + secondRadius.X &&
        EnemyMath.WrappedMagnitude(unchecked((ushort)(first.Y - second.Y))) <
            firstRadius.Y + secondRadius.Y;
    /// <summary>
    /// Ports <c>$A9:B3B6-$B454</c>. Mother Brain owns a bit-selected set of asymmetric
    /// rectangles whose side extents are interpreted relative to Samus, rather than an
    /// ordinary enemy radius. Phase one enables only the brain list; later phases also
    /// admit the body and three independently positioned neck segments.
    /// </summary>
    private static bool ResolveMotherBrainSamusCollision(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        ushort enabled = state.HitboxesEnabled;
        if ((enabled & 1) != 0 && ResolveMotherBrainSamusCollisionPart(
                state,
                samus,
                MotherBrainContactPart.Body,
                state.Body.Position.Whole))
        {
            return true;
        }

        enabled >>= 1;
        RoomEnemySlot head = state.Head!;
        if ((enabled & 1) != 0 && ResolveMotherBrainSamusCollisionPart(
                state,
                samus,
                MotherBrainContactPart.Brain,
                head.Position.Whole))
        {
            return true;
        }

        enabled >>= 1;
        if ((enabled & 1) != 0)
        {
            // Native tests only middle joints one, two, and three. Joint zero is buried in
            // the body and joint four is already covered by the separate brain rectangles.
            Position16<RoomPixels>[] collisionSegments =
                [state.NeckSegment1, state.NeckSegment2, state.NeckSegment3];
            foreach (Position16<RoomPixels> segment in collisionSegments)
            {
                if (ResolveMotherBrainSamusCollisionPart(
                        state,
                        samus,
                        MotherBrainContactPart.Neck,
                        segment))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>Checks a single head or body contact region and applies the phase-specific collision response.</summary>
    internal static bool ResolveMotherBrainSamusCollisionPart(
        MotherBrainEnemyState state,
        SamusState samus,
        MotherBrainContactPart contactPart,
        Position16<RoomPixels> origin)
    {
        Offset16<RoomPixels> fromOrigin = samus.Position.Whole - origin;
        foreach (MotherBrainContactHitbox hitbox in
            MotherBrainContactHitboxDefinitions.Get(contactPart))
        {
            bool belowOrigin = fromOrigin.Y >= 0;
            int yDistance = Math.Abs(fromOrigin.Y);
            short yExtent = belowOrigin ? hitbox.Bottom : hitbox.Top;
            int yOverlap = samus.Kinematics.Radius.Y + Math.Abs((int)yExtent) - yDistance;
            if (yOverlap < 0)
                continue;

            bool rightOfOrigin = fromOrigin.X >= 0;
            int xDistance = Math.Abs(fromOrigin.X);
            short xExtent = rightOfOrigin ? hitbox.Right : hitbox.Left;
            int xOverlap = samus.Kinematics.Radius.X + Math.Abs((int)xExtent) - xDistance;
            if (xOverlap < 0)
                continue;

            // Native clamps shallow contact UP to four pixels. The resulting words are
            // consumed by Samus movement later in the frame, so overwrite rather than add.
            samus.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromWholePixels(new(unchecked((short)Math.Max(xOverlap, 4)), 4));
            samus.InvincibilityTimer = NativeCountdown.Start(96);
            samus.KnockbackTimer = NativeCountdown.Start(5);
            samus.KnockbackXDirection = 1;
            if (unchecked((short)(samus.Position.Y.Whole - 192)) < 0)
                samus.Kinematics.YDirection = SamusVerticalDirection.Down;

            // Only contact to the right of body X+24 calls MotherBrain_HurtSamus. Contact
            // elsewhere still installs the displacement and timers above without damage.
            if (unchecked((short)(state.Body.Position.X.Whole + 24 - samus.Position.X.Whole)) < 0)
                DamageSamusFromMotherBrainBody(state.Body, samus);
            return true;
        }
        return false;
    }

    /// <summary>Applies body contact damage while preserving Samus's invulnerability timing.</summary>
    private static void DamageSamusFromMotherBrainBody(RoomEnemySlot body, SamusState samus)
    {
        ushort damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
            ? unchecked((ushort)(body.Definition.Damage >> 2))
            : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                ? unchecked((ushort)(body.Definition.Damage >> 1))
                : body.Definition.Damage;
        samus.Health = samus.Health <= damage
            ? (ushort)0
            : unchecked((ushort)(samus.Health - damage));
        samus.InvincibilityTimer = NativeCountdown.Start(96);
        samus.KnockbackTimer = NativeCountdown.Start(5);
        samus.KnockbackXDirection = samus.Position.X.Whole >= body.Position.X.Whole ? (ushort)1 : (ushort)0;
    }

    /// <summary>
    /// Ports the first-form branch of head shot AI <c>$A9:B507</c>. The glass encounter
    /// accepts only missile families; beams pass through without being converted into an
    /// impact, while every accepted missile increments the room PLM argument before common
    /// vulnerability damage is applied without the ordinary enemy-deletion tail.
    /// </summary>
    private void ResolveMotherBrainHeadShot(
        RoomEnemySlot head,
        SamusProjectileSlot projectile,
        SamusProjectileSystem projectiles,
        SamusProjectileTypeWord projectileType,
        ushort projectileDamage)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain head shot AI ran without its multipart encounter state.");
        if (!ReferenceEquals(state.Head, head))
        {
            throw new InvalidOperationException(
                "Mother Brain head shot AI ran for a slot not linked to the loaded body.");
        }
        SamusProjectileFamily family = projectileType.Family;
        if (state.Form != 0)
        {
            ResolveMotherBrainLaterFormHeadShot(
                state,
                head,
                projectile,
                projectiles,
                projectileType,
                projectileDamage);
            return;
        }

        // The bank-$A0 walker has marked every overlapping family before this callback;
        // the next projectile pass consumes the marker, removing a Super Missile's pair too.
        projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(
            projectile.SlotIndex,
            head.Properties.HasAny(EnemyProperties.BlocksPlasmaBeam) ||
                !projectileType.BeamCombination.HasPlasma);
        // `$A9:B519` lets only missiles and Super Missiles reach the glass.
        if (family is not (SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile))
            return;

        _room.Services.MotherBrain.IncrementGlassRoomArgument();
        _audio.QueueEnemySound(SoundEffectLibrary2Sounds.MotherBrainGlassHit, maximumQueued: 6);

        // `$B52D-$B539` deliberately alternates odd/even flash parity when a second hit
        // arrives during the existing flash. The common no-death damage helper then owns
        // health, hurt-AI, and the final ROM-authored hurt duration.
        head.FlashTimer = NativeCountdown.FromRaw(!head.FlashTimer.IsZero && (head.FlashTimer.RawValue & 1) != 0
            ? (ushort)14
            : (ushort)13);

        byte vulnerability = EnemyCollision.ReadProjectileVulnerability(head, projectileType);
        if (vulnerability == 0xff)
        {
            head.FrozenTimer = NativeCountdown.Start(400);
            head.AiHandlerBits = unchecked((ushort)(head.AiHandlerBits | 0x0004));
            head.InvincibilityTimer = NativeCountdown.Start(10);
            return;
        }

        int damage = (projectileDamage >> 1) * (vulnerability & 0x7f);
        if (damage == 0)
        {
            _projectiles.CreateEnemyProjectileDudShot(projectile);
            return;
        }

        ushort hurtTime = head.HurtAiTime == 0 ? (ushort)4 : head.HurtAiTime;
        head.FlashTimer = NativeCountdown.FromRaw(unchecked((ushort)(hurtTime + 8)));
        head.AiHandlerBits = unchecked((ushort)(head.AiHandlerBits | 0x0002));
        head.Health = damage >= head.Health
            ? (ushort)0
            : unchecked((ushort)(head.Health - damage));
    }

    /// <summary>
    /// Ports <c>$A9:B54E-B5C4</c> plus the phase-two/three common no-death damage tail.
    /// Form one (the fake-death interval) still runs the recoil bookkeeping but converts the
    /// projectile into a dud. Forms two and later apply ordinary vulnerability damage while
    /// deliberately leaving the multipart boss records alive at zero health for body AI.
    /// </summary>
    internal void ResolveMotherBrainLaterFormHeadShot(
        MotherBrainEnemyState state,
        RoomEnemySlot head,
        SamusProjectileSlot projectile,
        SamusProjectileSystem projectiles,
        SamusProjectileTypeWord projectileType,
        ushort projectileDamage)
    {
        // The bank-$A0 walker marked the projectile before dispatching this callback.
        projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(
            projectile.SlotIndex,
            head.Properties.HasAny(EnemyProperties.BlocksPlasmaBeam) ||
                !projectileType.BeamCombination.HasPlasma);

        // `$B58E` indexes its reaction table with the type word's high byte masked to three
        // bits, so missile explosions (`$08`) alias beams exactly as natively.
        var reactionProjectile = (MotherBrainProjectileType)((SnesLittleEndian.HighByte(projectileType.Raw)) & 7);
        if (state.RainbowBeamSequence is { } sequence)
        {
            // Once attached, the rainbow/Baby/phase-three state machine owns walk counter
            // `$7E:780E` and republishes it every body turn, so every reaction must reach it;
            // a write to the encounter projection alone is overwritten on the next turn.
            // `$B5BA/$B5BD` clears the body function timer on recoil underflow and `$B5C0`
            // publishes the counter immediately, before common projectile damage runs.
            sequence.ApplyPhase2Or3ShotReaction(reactionProjectile);
            state.FunctionTimer = sequence.FunctionTimer;
            state.WalkCounter = sequence.Phase3WalkCounter;
        }
        else
        {
            MotherBrainShotReactionResult reaction =
                MotherBrainShotReaction.Resolve(state.Form, reactionProjectile, state.WalkCounter);
            if (reaction.HyperBeamRecoil)
            {
                throw new InvalidOperationException(
                    "Mother Brain received a form-four beam without its phase-three state owner.");
            }
            state.WalkCounter = reaction.WalkCounter;
        }

        if (state.Form == 1)
        {
            _projectiles.CreateEnemyProjectileDudShot(projectile);
            return;
        }

        // The native callback tail-calls common no-death shot damage. In particular,
        // charged beams use the separate charged vulnerability byte after the ordinary
        // beam freeze check; Hyper would otherwise inherit the immune plasma entry.
        EnemyCollision.NormalShotVulnerability vulnerability = EnemyCollision.ReadNormalShotVulnerability(head, projectileType);
        if (vulnerability.FreezeImmediately)
        {
            head.FrozenTimer = NativeCountdown.Start(400);
            head.AiHandlerBits = unchecked((ushort)(head.AiHandlerBits | 0x0004));
            head.InvincibilityTimer = NativeCountdown.Start(10);
            return;
        }

        int damage = (projectileDamage >> 1) * vulnerability.Multiplier;
        if (damage == 0)
        {
            _projectiles.CreateEnemyProjectileDudShot(projectile);
            return;
        }

        ushort hurtTime = head.HurtAiTime == 0 ? (ushort)4 : head.HurtAiTime;
        head.FlashTimer = NativeCountdown.FromRaw(unchecked((ushort)(hurtTime + 8)));
        head.AiHandlerBits = unchecked((ushort)(head.AiHandlerBits | 0x0002));
        // The shared native no-death tail still grants Plasma's hit immunity.
        // A surviving penetrating beam cannot damage this head again every frame.
        if (projectileType.BeamCombination.HasPlasma)
            head.InvincibilityTimer = NativeCountdown.Start(EnemyShotTiming.PlasmaInvincibilityFrames);
        head.Health = damage >= head.Health
            ? (ushort)0
            : unchecked((ushort)(head.Health - damage));
    }

    /// <summary>
    /// Ports family <c>$0500</c> through Mother Brain brain callback <c>$A9:B507</c>.
    /// The first form's eight-entry projectile-family table rejects bombs outright. Later
    /// forms always run <c>$B562</c>: family five selects reaction zero, subtracting
    /// <c>$0100</c> from the walk/recoil accumulator with a signed-zero clamp. Fake-death
    /// form one then creates only a dud, while forms two and later enter common no-death
    /// damage. The bank-$A0 multibox caller already owns the bomb collision mark.
    /// </summary>
    private void ResolveMotherBrainHeadNormalBomb(
        RoomEnemySlot head,
        SamusBombProjectileSlot bomb)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain head normal-bomb AI ran without its multipart encounter state.");
        if (!ReferenceEquals(state.Head, head))
        {
            throw new InvalidOperationException(
                "Mother Brain head normal-bomb AI ran for a slot not linked to the loaded body.");
        }

        if (state.Form == 0)
            return;

        state.WalkCounter = state.WalkCounter < 0x0100
            ? (ushort)0
            : unchecked((ushort)(state.WalkCounter - 0x0100));
        if (state.Form == 1)
            return;

        _collision.ApplyCommonNormalBombDamage(head, bomb, runGenericDeath: false);
    }
    /// <summary>Consumes death actor events exactly once, alongside the live rainbow adapter.</summary>
    internal void ApplyLiveMotherBrainDeath(MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackSequence sequence, MotherBrainRainbowBeamAttackStepResult step)
    {
        foreach (var explosion in step.DeathExplosions)
        {
            SpawnMotherBrainDeathExplosion(explosion);
            state.LastSoundEffectLibrary3 = explosion.SoundEffect;
        }
        if (step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDisableBrainEffects &&
            step.PhaseAfter != step.PhaseBefore)
        {
            _presentation.BossPalettes.MotherBrainCorpse.CopyFrom(_presentation.BossPalettes.MotherBrainBrain.Colors);
            state.BrainPaletteIndex = sequence.BrainPaletteIndex;
            state.BrainPaletteHandlingEnabled = false;
            state.DroolGenerationEnabled = false;
        }
        bool bodyFade = step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody ||
            step.PhaseAfter == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody;
        if (bodyFade)
        {
            // ADF41C flickers the body and BG2 together on every call, not just palette ticks.
            state.DeathBg2Hidden = (state.Body.FrameCounter & 1) == 0;
            state.Body.Properties = state.DeathBg2Hidden
                ? state.Body.Properties.With(EnemyProperties.Invisible)
                : state.Body.Properties.Without(EnemyProperties.Invisible);
            if (step.PaletteRequested)
                LoadMotherBrainDeathBodyFade(sequence.GreyTransitionCounter - 1);
        }
        if (sequence.EnemyBg2TilemapClearRequested && !state.DeathBg2Cleared)
        {
            state.DeathBg2Cleared = true;
            state.DeathBg2Hidden = false;
            state.Body.Properties = sequence.BodyProperties;
            var words = new ushort[MotherBrainDeathRomData.Bg2LastByte / 2 + 1];
            Array.Fill(words, MotherBrainDeathRomData.EmptyBodyTile);
            WorkRamWordTable bg2 = _room.Scratch.EnemyBg2Tilemap;
            for (int i = 0; i < words.Length; i++)
                bg2[i] = words[i];
            new EnemyVram(_presentation._vram!).EnemyBg2Tilemap.Write(words);
            state.EnemyBg2TilemapSize = MotherBrainDeathRomData.Bg2LastByte;
            state.EnemyBg2TilemapTransferRequested = true;
        }
        if (sequence.BrainDrawSetupRequested)
        {
            // Decapitation transfers position ownership from the articulated neck to the
            // death sequence. Otherwise the next head AI call overwrites falling Y again.
            state.BrainFunction = MotherBrainBrainFunction.SetupBrainToBeDrawn;
            state.Head!.Position = state.Head!.Position.WithWhole(sequence.BrainPosition);
        }
        if (step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeToGrey && step.PaletteRequested)
            LoadMotherBrainDeathCorpseFade(sequence.GreyTransitionCounter - 1);
        foreach (var transfer in step.CorpseRottingVramTransfers) _motherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(transfer);
        foreach (var dust in step.CorpseDustRequests)
        {
            _projectiles.SpawnRoomGraphicsDustExplosion(dust.Position, dust.ProjectileParameter);
            // $A9:B252 queues the rot dust through QueueSound_Lib2_Max3.
            if (dust.SoundEffectQueued)
                _audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, dust.SoundEffect), maximumQueued: 3);
        }
        if (step.MusicStopQueued) state.RequestMusic(MusicCommand.Stop, MusicCommandDelay.EightFrames);
        if (step.EscapeMusicQueued) state.RequestMusic(MusicCommand.LoadData(MotherBrainDeathRomData.EscapeMusicData), MusicCommandDelay.EightFrames);
        if (step.EscapeMusicTrackQueued) state.RequestMusic(MusicCommand.SelectTrack(MotherBrainDeathRomData.EscapeMusicTrack), MusicCommandDelay.EightFrames);
        foreach (var transfer in step.EscapeSequenceTileTransfers) _motherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(transfer);
        if (step.EscapeTypewriterSetupRequested)
        {
            state.EnableUnpauseHook = sequence.MotherBrainUnpauseHookEnabled;
            state.EscapeTypewriter = _presentation.EscapeTypewriterPresentation is { } presentation
                ? new(presentation.Get(EscapeTypewriterProgramId.Zebes), EscapeTypewriterRomData.ZebesTileBase)
                : new(EscapeTypewriterRomData.ZebesText, EscapeTypewriterRomData.ZebesTileBase);
            _presentation.BossPalettes.EscapeTypewriterText.CopyFrom(_presentation.BossPalettes.EscapeTypewriterSource.Colors);
            state.Bg2Scroll = Position16<Bg2Pixels>.Zero;
        }
        if (step.ExplodedDoorPaletteRequested)
            LoadMotherBrainDeathDoorPalette();
        if (step.EscapeDoorExplosion is { } doorDust)
        {
            _projectiles.SpawnRoomGraphicsDustExplosion(doorDust.Position, doorDust.ProjectileParameter);
            // $A9:B346 queues the door explosion through QueueSound_Lib2_Max3.
            _audio.QueueEnemySound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, doorDust.SoundEffect), maximumQueued: 3);
        }
        if (step.EscapeDoorPlm is { } doorPlm)
            state.RequestPlm(doorPlm.Block, doorPlm.PlmEntry);
        foreach (var request in step.EscapeDoorParticleSpawns) SpawnMotherBrainDoorFragment(request.Parameter);
        if (step.TimeBombSetSubtitleSpawnRequested)
        {
            var subtitle = _projectiles.AllocateEnemyProjectile();
            if (subtitle is not null)
            {
                EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(subtitle, RoomEnemyProjectileKind.MotherBrainEscapeSubtitle, graphicsIndex: 0);
                PinMotherBrainEscapeSubtitle(subtitle);
            }
        }
        if (step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround &&
            step.PhaseAfter == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadCorpseTiles ||
            step.EscapeMusicTrackQueued || step.EarthquakeTimerRefreshed)
        {
            _earthquake.EarthquakeType = sequence.EarthquakeType;
            _earthquake.EarthquakeTimer = NativeCountdown.FromRaw(sequence.EarthquakeTimer);
        }
    }

    /// <summary>Applies the requested body-color row during the death fade.</summary>
    internal void LoadMotherBrainDeathBodyFade(int frame)
    {
        if (_room.Artwork?.MotherBrainDeathColors is { } colors)
        {
            for (int color = 0; color < MotherBrainDeathRomData.BodyColorCount; color++)
            {
                Bgr555 shared = colors.BodyColor(frame, color);
                _presentation.BossPalettes.MotherBrainBody.Write(color, shared);
                _presentation.BossPalettes.MotherBrainBrain.Write(color, shared);
                _presentation.BossPalettes.MotherBrainLegs.Write(color, colors.LegColor(frame, color));
            }
            return;
        }
        throw new InvalidOperationException("Mother Brain death requires installed fade colors.");
    }

    /// <summary>Applies the requested corpse-color row during the death fade.</summary>
    internal void LoadMotherBrainDeathCorpseFade(int frame)
    {
        if (_room.Artwork?.MotherBrainDeathColors is { } colors)
        {
            for (int color = 0; color < MotherBrainDeathRomData.CorpseColorCount; color++)
                _presentation.BossPalettes.MotherBrainCorpse.Write(color, colors.CorpseColor(frame, color));
            return;
        }
        throw new InvalidOperationException("Mother Brain corpse fade requires installed colors.");
    }

    /// <summary>Restores the room door colors required after Mother Brain dies.</summary>
    internal void LoadMotherBrainDeathDoorPalette()
    {
        if (_room.Artwork?.MotherBrainDeathColors is { } colors)
        {
            for (int color = 0; color < MotherBrainDeathRomData.BodyColorCount; color++)
                _presentation.BossPalettes.MotherBrainBrain.Write(color, colors.ExplodedDoorColor(color));
            return;
        }
        throw new InvalidOperationException("Mother Brain exploded door requires installed colors.");
    }

    /// <summary>Places a death explosion with the requested position and visual phase.</summary>
    internal void SpawnMotherBrainDeathExplosion(MotherBrainDeathExplosionRequest request)
    {
        ushort instructionList =
            MotherBrainDeathExplosionDefinitions.InstructionList(request.ProjectileParameter);
        var projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null) return; // Native shared-pool exhaustion drops the spawn.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, RoomEnemyProjectileKind.MotherBrainDeathExplosion, graphicsIndex: 0);
        projectile.InstructionPointer = instructionList;
        projectile.InstructionTimer = NativeCountdown.Start(1);
        // The native initializer keeps the body-relative offset in the velocity words.
        projectile.Velocity = new EightEightVector<RoomPixels>(request.Offset.X, request.Offset.Y);
        RunMotherBrainDeathExplosion(projectile);
    }

    /// <summary>Advances a death explosion projectile through its installed program.</summary>
    internal void RunMotherBrainDeathExplosion(RoomEnemyProjectileSlot projectile)
    {
        var body = _motherBrainEncounter.MotherBrain?.Body ?? throw new InvalidOperationException("Body-relative death explosion has no Mother Brain owner.");
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(body.Position.X.Whole + projectile.Velocity.X.RawValue)), unchecked((ushort)(body.Position.Y.Whole + projectile.Velocity.Y.RawValue))));
    }

    /// <summary>Spawns a falling door fragment from the native parameter identity.</summary>
    internal void SpawnMotherBrainDoorFragment(ushort parameter)
    {
        MotherBrainDoorFragmentDefinition definition =
            MotherBrainDoorFragmentDefinitions.ForParameter(parameter);
        var fragment = _projectiles.AllocateEnemyProjectile();
        if (fragment is null) return;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(fragment, RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment, graphicsIndex: 0);
        fragment.Position = fragment.Position.WithWhole(new Position16<RoomPixels>(MotherBrainDeathRomData.DoorX, MotherBrainDeathRomData.DoorY) + definition.Offset);
        fragment.Velocity = definition.Velocity;
        fragment.Variable0 = MotherBrainDeathRomData.DoorFragmentLifetime;
    }

    /// <summary>Updates a door fragment's fall and expiration after the room collapse.</summary>
    internal void RunMotherBrainDoorFragment(RoomEnemyProjectileSlot fragment)
    {
        bool negative = unchecked((short)fragment.Velocity.X.RawValue) < 0;
        ushort magnitude = negative ? unchecked((ushort)-fragment.Velocity.X.RawValue) : fragment.Velocity.X.RawValue;
        short slowed = unchecked((short)(magnitude - MotherBrainDeathRomData.DoorFragmentDrag));
        int speed = Math.Max(0, (int)slowed);
        fragment.Velocity = fragment.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(negative ? -speed : speed))));
        fragment.Velocity = fragment.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(fragment.Velocity.Y.RawValue + MotherBrainDeathRomData.DoorFragmentGravity))));
        fragment.Position = fragment.Position.Advance(fragment.Velocity.ToSixteenSixteen());
        fragment.Variable0--;
        if (unchecked((short)fragment.Variable0) >= 0) return;
        ushort x = fragment.Position.X.Whole, y = unchecked((ushort)(fragment.Position.Y.Whole - MotherBrainDeathRomData.DoorDustYOffset));
        fragment.Clear();
        _projectiles.SpawnRoomGraphicsDustExplosion(new(x, y), MotherBrainDeathRomData.DoorDustParameter);
    }

    /// <summary>Keeps the escape subtitle projectile fixed to its screen position.</summary>
    internal static void PinMotherBrainEscapeSubtitle(RoomEnemyProjectileSlot subtitle)
    {
        subtitle.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
        subtitle.Position = subtitle.Position.WithWhole(new(MotherBrainDeathRomData.SubtitleX, MotherBrainDeathRomData.SubtitleY));
    }
    /// <summary>Native sequence of explosion positions used by the fake-death collapse.</summary>
    private static readonly (ushort X, ushort Y)[] MotherBrainFakeDeathExplosionPositions =
    [
        (136, 116),
        (120, 132),
        (124, 90),
        (138, 146),
        (120, 52),
        (124, 170),
        (138, 72),
        (120, 206),
    ];

    /// <summary>
    /// $A9:8B5D/8B67/8B71: collision radii and landing floor selected by piece role. The two
    /// outer and two inner pieces share their values; the main tube has its own floor.
    /// </summary>
    internal static (ushort XRadius, ushort YRadius, ushort Floor) MotherBrainFallingTubeBody(int tubeIndex) =>
        tubeIndex switch
        {
            0 or 1 => (16, 32, 0x00f8),
            2 or 3 => (8, 24, 0x00f0),
            4 => (16, 32, 0x00f6),
            _ => throw new InvalidDataException($"Mother Brain falling tube index {tubeIndex} is outside its five pieces."),
        };
    /// <summary>$A9:8C61: authored smoke puff X offsets cycled while a tube lands.</summary>
    private static readonly int[] MotherBrainFallingTubeSmokeXOffsets = [-8, 2, -4, 6];

    /// <summary>Dispatches the body pointer after the phase-one function changes to $881D.</summary>
    internal void RunMotherBrainFakeDeath(MotherBrainEnemyState state, SamusState? samus)
    {
        switch (state.Function)
        {
            case MotherBrainBodyFunction.FakeDeathDescentInitialPause:
                // `$A9:881D` installs the following countdown and falls through to it. The
                // first observable value is therefore 63, not the host-friendly value 64.
                state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock;
                state.FunctionTimer = NativeCountdown.Start(64);
                RunMotherBrainPauseBeforeLock(state, samus);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeLock:
                RunMotherBrainPauseBeforeLock(state, samus);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeMusic:
                if (!DecrementMotherBrainTimerPastZero(state))
                    return;
                state.RequestMusic(MusicCommand.Stop, MusicCommandDelay.EightFrames);
                state.RequestMusic(MusicCommand.LoadData(0x21), MusicCommandDelay.EightFrames);
                state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeUnlock;
                state.FunctionTimer = NativeCountdown.Start(12);
                RunMotherBrainFakeDeath(state, samus);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeUnlock:
                if (!DecrementMotherBrainTimerPastZero(state))
                    return;
                // $A9:8874: Samus command one restores the normal handler pair.
                samus?.ScriptLocks.SetStationaryScriptControlLock(false);
                state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeFlash;
                state.FunctionTimer = NativeCountdown.Start(8);
                RunMotherBrainFakeDeath(state, samus);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentPauseBeforeFlash:
                if (!DecrementMotherBrainTimerPastZero(state))
                    return;

                // `$A9:88A0-$88AF` starts the looping room flash, selects FX entry two,
                // arms the head's independent tube sequence, and clears both fade words.
                state.RoomPaletteInstructionPointer =
                    MotherBrainRoomPaletteProgramDefinitions.FlashStart;
                state.RoomPaletteInstructionTimer = 1;
                state.FxEntry = 2;
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.WaitForFourFreeProjectileSlots;
                state.TubeCollapseTimer = NativeCountdown.Start(0);
                state.Function = MotherBrainBodyFunction.FakeDeathDescentFadeToGray;
                state.FunctionTimer = NativeCountdown.Start(0);
                state.GrayFadeIndex = 0;
                state.RequestPlm(new(14, 2), PlmHeaderId.ClearMotherBrainCeilingBlock);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentFadeToGray:
                RunMotherBrainFadeToGray(state);
                return;

            case MotherBrainBodyFunction.FakeDeathDescentCollapseTubes:
                RunMotherBrainTubeCollapse(state);
                RunMotherBrainFakeDeathExplosion(state);
                return;

            case MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3:
                RequestMotherBrainRoomRows(
                    state,
                    PlmHeaderId.MotherBrainsBackgroundRow2,
                    PlmHeaderId.MotherBrainsBackgroundRow3,
                    MotherBrainBodyFunction.FakeDeathAscentDrawRows4And5);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows4And5:
                RequestMotherBrainRoomRows(
                    state,
                    PlmHeaderId.MotherBrainsBackgroundRow4,
                    PlmHeaderId.MotherBrainsBackgroundRow5,
                    MotherBrainBodyFunction.FakeDeathAscentDrawRows6And7);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows6And7:
                RequestMotherBrainRoomRows(
                    state,
                    PlmHeaderId.MotherBrainsBackgroundRow6,
                    PlmHeaderId.MotherBrainsBackgroundRow7,
                    MotherBrainBodyFunction.FakeDeathAscentDrawRows8And9);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawRows8And9:
                RequestMotherBrainRoomRows(
                    state,
                    PlmHeaderId.MotherBrainsBackgroundRow8,
                    PlmHeaderId.MotherBrainsBackgroundRow9,
                    MotherBrainBodyFunction.FakeDeathAscentDrawRowsAAndB);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawRowsAAndB:
                RequestMotherBrainRoomRows(
                    state,
                    PlmHeaderId.MotherBrainsBackgroundRowA,
                    PlmHeaderId.MotherBrainsBackgroundRowB,
                    MotherBrainBodyFunction.FakeDeathAscentDrawRowsCAndD);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentDrawRowsCAndD:
                RequestMotherBrainRoomRows(
                    state,
                    PlmHeaderId.MotherBrainsBackgroundRowC,
                    PlmHeaderId.MotherBrainsBackgroundRowD,
                    MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Graphics:
                SetupMotherBrainPhaseTwoGraphics(state);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain fake-death function $A9:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Waits through the fake-death pause before control is locked for the next phase.</summary>
    private void RunMotherBrainPauseBeforeLock(MotherBrainEnemyState state, SamusState? samus)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        // $A9:8836 loads TWO bytes, masks with $00FF, then stores TWO bytes. This
        // preserves scroll zero and clears scroll one, locking the arena to screen zero.
        SamusState requiredSamus = samus ?? throw new InvalidOperationException(
            "Mother Brain's fake-death input lock requires the active Samus state.");
        // $A9:8832 runs Samus command zero, the stationary lock: its alpha handler places
        // no bombs and fires nothing new while the arena drains.
        requiredSamus.ScriptLocks.SetStationaryScriptControlLock(true);
        _room.Services.RoomScroll.Write(1, RoomScrollState.RedBoundary);

        state.Function = MotherBrainBodyFunction.FakeDeathDescentPauseBeforeMusic;
        state.FunctionTimer = NativeCountdown.Start(32);
        RunMotherBrainFakeDeath(state, samus);
    }

    /// <summary>
    /// Native timers expire only after unsigned decrement produces a negative signed word.
    /// A zero timer therefore expires immediately, while a value N remains live for N ticks.
    /// </summary>
    private static bool DecrementMotherBrainTimerPastZero(MotherBrainEnemyState state)
    {
        NativeWordCounterStep timer = state.FunctionTimer.Decrement();
        state.FunctionTimer = timer.Remaining;
        return timer.IsNegative;
    }

    /// <summary>Steps Mother Brain's fake-death gray fade and advances when its palette cycle completes.</summary>
    internal void RunMotherBrainFadeToGray(MotherBrainEnemyState state)
    {
        NativeWordCounterStep timer = state.FunctionTimer.Decrement();
        state.FunctionTimer = timer.Remaining;
        if (timer.IsNegative)
        {
            state.FunctionTimer = NativeCountdown.Start(8);
            ushort step = state.GrayFadeIndex++;
            if (step == MotherBrainFakeDeathPaletteRomData.FrameCount)
            {
                state.Function = MotherBrainBodyFunction.FakeDeathDescentCollapseTubes;
            }
            else
            {
                // Fake-death fade replaces colors 145..147 only. Copying a convenient
                // whole palette would visibly alter unrelated room art and is not what
                // `$AD:ED5A` does.
                if (_presentation.MotherBrainRainbowColors is { } colors)
                    colors.ApplyFakeDeathToGrey(_presentation._cgram!, step);
                else throw new InvalidOperationException(
                    "Mother Brain fake-death fade requires installed colors.");
            }
        }

        // `$A9:88CF` is an unconditional tail-call. Tube sequencing and dust therefore
        // begin during the grey fade rather than waiting for the eighth palette entry.
        RunMotherBrainTubeCollapse(state);
        RunMotherBrainFakeDeathExplosion(state);
    }

    /// <summary>Executes Mother Brain's independent bank-$A9 room-palette bytecode.</summary>
    internal void RunMotherBrainRoomPalette(MotherBrainEnemyState state)
    {
        if (state.RoomPaletteInstructionPointer == 0)
            return;

        for (int commandCount = 0; commandCount < 16; commandCount++)
        {
            ushort pointer = state.RoomPaletteInstructionPointer;
            ushort command =
                MotherBrainRoomPaletteProgramDefinitions.ReadMechanicsWord(pointer);
            if ((command & 0x8000) != 0)
            {
                if (command != MotherBrainRoomPaletteProgramDefinitions.GotoInstruction)
                {
                    throw new InvalidDataException(
                        $"Mother Brain room-palette opcode $A9:{command:X4} is not translated.");
                }

                state.RoomPaletteInstructionPointer =
                    MotherBrainRoomPaletteProgramDefinitions.ReadMechanicsWord(
                        unchecked((ushort)(pointer + 2)));
                state.RoomPaletteInstructionTimer = 0;
                continue;
            }

            if (command == 0)
            {
                state.RoomPaletteInstructionPointer = 0;
                state.RoomPaletteInstructionTimer = 0;
                return;
            }

            if (state.RoomPaletteInstructionTimer == 0)
            {
                state.RoomPaletteInstructionTimer = 1;
                ApplyMotherBrainRoomPalette(pointer);
                return;
            }

            if (state.RoomPaletteInstructionTimer != command)
            {
                state.RoomPaletteInstructionTimer++;
                ApplyMotherBrainRoomPalette(pointer);
                return;
            }

            state.RoomPaletteInstructionPointer = unchecked((ushort)(pointer + 4));
            state.RoomPaletteInstructionTimer = 0;
        }

        throw new InvalidDataException(
            "Mother Brain room-palette list exceeded 16 commands without selecting a timed entry.");
    }

    /// <summary>Copies the selected timed room-palette entry into CGRAM.</summary>
    private void ApplyMotherBrainRoomPalette(ushort timedEntryPointer)
    {
        (_presentation.MotherBrainRoomColors ?? throw new InvalidOperationException(
            "Mother Brain room flash requires installed colors."))
            .ApplyFlash(_presentation._cgram!, timedEntryPointer);
    }

    /// <summary>Stops the palette effect and restores the encounter's steady room colors.</summary>
    internal void StopMotherBrainRoomPalette(MotherBrainEnemyState state)
    {
        state.RoomPaletteInstructionPointer = 0;
        state.RoomPaletteInstructionTimer = 0;
        (_presentation.MotherBrainRoomColors ?? throw new InvalidOperationException(
            "Mother Brain room finale requires installed colors."))
            .ApplyFinal(_presentation._cgram!);
    }

    /// <summary>Emits timed explosions while the first-phase body appears to die.</summary>
    private void RunMotherBrainFakeDeathExplosion(MotherBrainEnemyState state)
    {
        NativeWordCounterStep timer = state.FakeDeathExplosionTimer.Decrement();
        state.FakeDeathExplosionTimer = timer.Remaining;
        if (timer.IsNonNegative)
            return;

        state.FakeDeathExplosionTimer = NativeCountdown.Start(8);
        state.FakeDeathExplosionIndex = unchecked((ushort)((state.FakeDeathExplosionIndex - 1) & 7));
        (ushort x, ushort y) = MotherBrainFakeDeathExplosionPositions[state.FakeDeathExplosionIndex];

        // `$A9:8904` samples the existing RNG word and never advances it. A host random
        // call here would desynchronize every later Rinka and phase-two attack decision.
        ushort sampledRandom = _room.Random.Read();
        ushort animation = sampledRandom < 0x4000 ? (ushort)12 : (ushort)3;
        _projectiles.SpawnRoomGraphicsDustExplosion(new(x, y), animation);
        state.LastSoundEffect = 0x24;
    }

    // Every spawn after the first ($A9:8983-$8ACB) begins with DEC timer : BPL return, so
    // each waits out the $20 (or $02) its preceding clear step set.
    /// <summary>Schedules glass and tube collapse phases with their authored waits.</summary>
    internal void RunMotherBrainTubeCollapse(MotherBrainEnemyState state)
    {
        switch (state.TubeCollapseFunction)
        {
            case MotherBrainTubeCollapseFunction.WaitForFourFreeProjectileSlots:
                if (_projectiles.EnemyProjectiles.Count(projectile => !projectile.IsActive) < 4)
                    return;
                SpawnMotherBrainFallingTube(MotherBrainFallingTubePopulationDefinitions.BottomLeft);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearBottomLeftTube;
                return;

            case MotherBrainTubeCollapseFunction.ClearBottomLeftTube:
                RequestMotherBrainTubePlm(state, new(5, 9),
                    PlmHeaderId.ClearMotherBrainBottomLeftTube,
                    MotherBrainTubeCollapseFunction.SpawnTopRightTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnTopRightTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopRightTube, new(152, 47));
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearCeilingColumn9;
                return;
            case MotherBrainTubeCollapseFunction.ClearCeilingColumn9:
                RequestMotherBrainTubePlm(state, new(9, 2),
                    PlmHeaderId.ClearMotherBrainCeilingBlock,
                    MotherBrainTubeCollapseFunction.SpawnTopLeftTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnTopLeftTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopLeftTube, new(104, 47));
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearCeilingColumn6;
                return;
            case MotherBrainTubeCollapseFunction.ClearCeilingColumn6:
                RequestMotherBrainTubePlm(state, new(6, 2),
                    PlmHeaderId.ClearMotherBrainCeilingBlock,
                    MotherBrainTubeCollapseFunction.SpawnBottomRightTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnBottomRightTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainFallingTube(MotherBrainFallingTubePopulationDefinitions.BottomRight);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearBottomRightTube;
                return;
            case MotherBrainTubeCollapseFunction.ClearBottomRightTube:
                RequestMotherBrainTubePlm(state, new(10, 9),
                    PlmHeaderId.ClearMotherBrainBottomRightTube,
                    MotherBrainTubeCollapseFunction.SpawnBottomMiddleLeftTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnBottomMiddleLeftTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainFallingTube(MotherBrainFallingTubePopulationDefinitions.BottomMiddleLeft);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearBottomMiddleLeftTube;
                return;
            case MotherBrainTubeCollapseFunction.ClearBottomMiddleLeftTube:
                RequestMotherBrainTubePlm(state, new(6, 10),
                    PlmHeaderId.ClearMotherBrainBottomMiddleSideTube,
                    MotherBrainTubeCollapseFunction.SpawnTopMiddleLeftTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnTopMiddleLeftTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube, new(120, 59));
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearCeilingColumn7;
                return;
            case MotherBrainTubeCollapseFunction.ClearCeilingColumn7:
                RequestMotherBrainTubePlm(state, new(7, 2),
                    PlmHeaderId.ClearMotherBrainCeilingTube,
                    MotherBrainTubeCollapseFunction.SpawnTopMiddleRightTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnTopMiddleRightTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainTopTube(RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube, new(136, 59));
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearCeilingColumn8;
                return;
            case MotherBrainTubeCollapseFunction.ClearCeilingColumn8:
                RequestMotherBrainTubePlm(state, new(8, 2),
                    PlmHeaderId.ClearMotherBrainCeilingTube,
                    MotherBrainTubeCollapseFunction.SpawnBottomMiddleRightTube, 32);
                return;
            case MotherBrainTubeCollapseFunction.SpawnBottomMiddleRightTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainFallingTube(MotherBrainFallingTubePopulationDefinitions.BottomMiddleRight);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearBottomMiddleRightTube;
                return;
            case MotherBrainTubeCollapseFunction.ClearBottomMiddleRightTube:
                RequestMotherBrainTubePlm(state, new(9, 10),
                    PlmHeaderId.ClearMotherBrainBottomMiddleSideTube,
                    MotherBrainTubeCollapseFunction.SpawnMainTube, 2);
                return;
            case MotherBrainTubeCollapseFunction.SpawnMainTube:
                if (!DecrementMotherBrainTubeTimerPastZero(state))
                    return;
                SpawnMotherBrainFallingTube(MotherBrainFallingTubePopulationDefinitions.Main);
                state.TubeCollapseFunction = MotherBrainTubeCollapseFunction.ClearBottomMiddleTubes;
                return;
            case MotherBrainTubeCollapseFunction.ClearBottomMiddleTubes:
                RequestMotherBrainTubePlm(state, new(7, 7),
                    PlmHeaderId.ClearMotherBrainBottomMiddleTubes,
                    MotherBrainTubeCollapseFunction.Finished, 0);
                return;
            case MotherBrainTubeCollapseFunction.Finished:
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain tube function $A9:{(ushort)state.TubeCollapseFunction:X4} is not translated.");
        }
    }

    /// <summary>Implements the tube phase's wraparound countdown transition.</summary>
    private static bool DecrementMotherBrainTubeTimerPastZero(MotherBrainEnemyState state)
    {
        NativeWordCounterStep timer = state.TubeCollapseTimer.Decrement();
        state.TubeCollapseTimer = timer.Remaining;
        return timer.IsNegative;
    }

    /// <summary>Queues the room tile change at the tube's native location.</summary>
    private static void RequestMotherBrainTubePlm(
        MotherBrainEnemyState state,
        Position16<RoomBlocks> block,
        PlmHeaderId header,
        MotherBrainTubeCollapseFunction next,
        ushort timer)
    {
        state.RequestPlm(block, header);
        state.TubeCollapseFunction = next;
        state.TubeCollapseTimer = NativeCountdown.Start(timer);
    }

    /// <summary>Spawns the top tube projectile for the scripted collapse.</summary>
    internal void SpawnMotherBrainTopTube(
        RoomEnemyProjectileKind kind,
        Position16<RoomPixels> position)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, graphicsIndex: EnemyPaletteBits.Palette7);
        projectile.Position = projectile.Position.WithWhole(position);
        projectile.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
        projectile.Variable0 = 0xcbea;
    }

    /// <summary>Runs pre-instruction $86:CBE7 for the four falling ceiling-tube actors.</summary>
    internal void RunMotherBrainTopTubePreInstruction(RoomEnemyProjectileSlot projectile)
    {
        if (projectile.Variable0 == 0xcbea)
        {
            _projectiles.SpawnRoomGraphicsDustExplosion(
                new(projectile.Position.X.Whole, unchecked((ushort)(projectile.Position.Y.Whole + 8))),
                animationIndex: 9);
            projectile.Variable0 = 0xcc08;
        }
        else if (projectile.Variable0 != 0xcc08)
        {
            throw new InvalidDataException(
                $"Mother Brain ceiling tube function $86:{projectile.Variable0:X4} is not translated.");
        }

        projectile.Velocity = projectile.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(projectile.Velocity.Y.RawValue + 6))));
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        if (projectile.Position.Y.Whole < 0x00d0)
            return;

        ushort x = projectile.Position.X.Whole;
        ushort y = projectile.Position.Y.Whole;
        projectile.Clear();
        _projectiles.SpawnRoomGraphicsDustExplosion(new(x, y), animationIndex: 12);
    }

    /// <summary>Creates a falling tube enemy from its population pointer.</summary>
    internal void SpawnMotherBrainFallingTube(ushort populationPointer)
    {
        int slotIndex = Array.FindIndex(_slotTable._slots, slot => slot.EnemyDefinitionPointer == 0);
        if (slotIndex < 0)
            return; // SpawnEnemy silently fails when all 32 native records are occupied.

        // Authored verifier buses may supply constructed records. A retail runtime
        // instead resolves the fixed cutscene placement without touching bank $A9.
        RoomEnemyPopulationRecord population = _room.Bus is IRoomEnemyFallingTubeFixtureSource fixture
            ? fixture.ReadFallingTubePopulation(populationPointer)
            : MotherBrainFallingTubePopulationDefinitions.Get(populationPointer);
        if (population.DefinitionPointer != EnemyDefinitionId.MotherBrainTubes)
        {
            throw new InvalidDataException(
                $"Mother Brain tube record $A9:{populationPointer:X4} names enemy " +
                $"${(int)population.DefinitionPointer:X4}, not $ECFF.");
        }

        RoomEnemySlot tube = _slotTable._slots[slotIndex];
        RoomEnemyDefinition definition = EnemyPopulationLoader.ResolveRoomEnemyDefinition(_room.Bus!, population.DefinitionPointer);
        _dispatch.InitializeSlotFromDefinition(tube, population, definition);
        _dispatch.RunInitializationAi(tube);
        tube.SpritemapPointer = tube.Properties.HasAny(EnemyProperties.ProcessInstructions)
            ? (ushort)0x804d
            : (ushort)0;
        _slotTable.EnemyCount = unchecked((ushort)Math.Max(_slotTable.EnemyCount, slotIndex + 1));
        _slotTable.FirstFreeEnemyIndex = unchecked((ushort)((slotIndex + 1) * RoomEnemySystem.NativeSlotSize));
        if (_motherBrainEncounter.MotherBrain is not null)
            _motherBrainEncounter.MotherBrain.SpawnedFallingTubeCount++;
    }

    /// <summary>Ports falling-tube initialization $A9:8B35.</summary>
    internal static void InitializeMotherBrainFallingTube(RoomEnemySlot tube)
    {
        int tubeIndex = tube.Parameter1 / 2;
        (ushort xRadius, ushort yRadius, ushort floor) = MotherBrainFallingTubeBody(tubeIndex);
        tube.Radius = new(xRadius, yRadius);
        tube.VariableA = tubeIndex == 4 ? (ushort)0x8bcb : (ushort)0x8b88;
        tube.VariableB = floor;
        tube.VariableC = 0;
        tube.VariableD = 0;
        tube.VariableE = 0;
    }

    /// <summary>Ports the $A9:8B85 function-pointer dispatcher for physical tube enemies.</summary>
    internal void RunMotherBrainFallingTubeMain(RoomEnemySlot tube)
    {
        switch (ClosedNativeWords.Decode<MotherBrainTubeFunction>(tube.VariableA, "Mother Brain tube function"))
        {
            case MotherBrainTubeFunction.NonMainTube:
                FallMotherBrainTube(tube, mainTube: false);
                return;
            case MotherBrainTubeFunction.MainTube_WaitingToFall:
                tube.Parameter2 = unchecked((ushort)(tube.Parameter2 - 1));
                if (unchecked((short)tube.Parameter2) >= 0)
                    return;
                tube.VariableA = 0x8bd6;
                FallMotherBrainTube(tube, mainTube: true);
                return;
            case MotherBrainTubeFunction.MainTube_Falling:
                FallMotherBrainTube(tube, mainTube: true);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain falling tube function $A9:{tube.VariableA:X4} is not translated.");
        }
    }

    /// <summary>Moves a falling tube, with a distinct path for the main tube.</summary>
    private void FallMotherBrainTube(RoomEnemySlot tube, bool mainTube)
    {
        tube.VariableC = unchecked((ushort)(tube.VariableC + 6));
        tube.Position = tube.Position.AdvanceY(new SnesSignedEightEight(tube.VariableC).ToSixteenSixteenDelta());

        if (!mainTube)
        {
            if (tube.Position.Y.Whole >= tube.VariableB)
                ExplodeMotherBrainTube(tube);
            else
                SpawnMotherBrainTubeSmoke(tube);
            return;
        }

        // Hiding the tube and moving the brain are independent native branches.
        // The brain follows the tube throughout its descent, including before hiding.
        if (tube.Position.Y.Whole >= 0x00f4)
            tube.Properties = tube.Properties.With(EnemyProperties.Invisible);
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ??
            throw new InvalidOperationException("A falling main tube has no Mother Brain encounter state.");
        ushort headY = unchecked((ushort)(tube.Position.Y.Whole - 56));
        state.Head!.Position = state.Head!.Position.WithWholeY(headY);
        if (headY < 196)
        {
            SpawnMotherBrainTubeSmoke(tube);
            return;
        }

        StopMotherBrainRoomPalette(state);
        _earthquake.EarthquakeType = 0x0019;
        _earthquake.EarthquakeTimer = NativeCountdown.Start(0x0020);
        // `$A9:8C0C/$8C0F` zero the channel words of HDMA objects 0-3, deleting the room's
        // liquid scroll objects (and any X-ray object) for the rest of the encounter.
        _motherBrainEncounter.MotherBrainDeletedHdmaObjects = true;
        state.Head.Position = state.Head.Position.WithWholeY(196);
        state.Body.Position = state.Body.Position.WithWhole(new(59, 279));

        // `$A9:903F` is part of the main-tube landing path, not phase-two setup. These
        // lengths and starting angles must exist before `$87A2` gets installed several
        // frames later or the first visible neck frame collapses every joint onto zero.
        state.NeckSegment0Distance = 2;
        state.NeckSegment1Distance = 10;
        state.NeckSegment2Distance = 20;
        state.NeckSegment3Distance = 10;
        state.NeckSegment4Distance = 20;
        state.LowerNeckAngle = 0x4800;
        state.UpperNeckAngle = 0x5000;
        state.NeckAngleDelta = 0x0100;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3;
        ExplodeMotherBrainTube(tube);
    }

    /// <summary>Emits smoke at the falling tube's current position.</summary>
    private void SpawnMotherBrainTubeSmoke(RoomEnemySlot tube)
    {
        tube.VariableD = unchecked((ushort)(tube.VariableD - 1));
        if (unchecked((short)tube.VariableD) >= 0)
            return;

        tube.VariableD = 8;
        tube.VariableE = unchecked((ushort)((tube.VariableE + 1) & 3));
        _projectiles.SpawnRoomGraphicsDustExplosion(
            new(unchecked((ushort)(tube.Position.X.Whole + MotherBrainFallingTubeSmokeXOffsets[tube.VariableE])), 208),
            animationIndex: 9);
    }

    /// <summary>Finishes a falling tube with its explosion and tile update.</summary>
    private void ExplodeMotherBrainTube(RoomEnemySlot tube)
    {
        ushort x = tube.Position.X.Whole;
        ushort y = tube.Position.Y.Whole;
        tube.Properties = tube.Properties.With(EnemyProperties.Deleted);
        _projectiles.SpawnRoomGraphicsDustExplosion(new(x, y), animationIndex: 3);
        if (_motherBrainEncounter.MotherBrain is not null)
            _motherBrainEncounter.MotherBrain.LastSoundEffect = SoundEffectLibrary2Sounds.SmallExplosion.Value;
    }

    /// <summary>Queues the BG2 room rows revealed by the tube collapse.</summary>
    private static void RequestMotherBrainRoomRows(
        MotherBrainEnemyState state,
        PlmHeaderId firstHeader,
        PlmHeaderId secondHeader,
        MotherBrainBodyFunction next)
    {
        byte firstRow = state.Function switch
        {
            MotherBrainBodyFunction.FakeDeathAscentDrawRows2And3 => 2,
            MotherBrainBodyFunction.FakeDeathAscentDrawRows4And5 => 4,
            MotherBrainBodyFunction.FakeDeathAscentDrawRows6And7 => 6,
            MotherBrainBodyFunction.FakeDeathAscentDrawRows8And9 => 8,
            MotherBrainBodyFunction.FakeDeathAscentDrawRowsAAndB => 10,
            MotherBrainBodyFunction.FakeDeathAscentDrawRowsCAndD => 12,
            _ => throw new InvalidOperationException("Mother Brain row draw called outside fake ascent."),
        };
        state.RequestPlm(new(2, firstRow), firstHeader);
        state.RequestPlm(new(2, unchecked((byte)(firstRow + 1))), secondHeader);
        state.Function = next;
    }

    /// <summary>Stages the phase-two body tilemap and requests its first VRAM transfer.</summary>
    internal void SetupMotherBrainPhaseTwoGraphics(MotherBrainEnemyState state)
    {
        // `$A9:8D11` installs colors 1..15 of the attack and back-leg palettes. Color zero
        // belongs to the room backdrop and must remain untouched.
        (_presentation.MotherBrainRoomColors ?? throw new InvalidOperationException(
            "Mother Brain phase two requires installed room colors."))
            .ApplyPhaseTwoInitial(_presentation._cgram!);
        state.EnableUnpauseHook = true;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain;
        // The native transition now requests the prepared enemy image, replacing the
        // room's pipe background before the articulated body becomes visible.
        var words = new ushort[state.EnemyBg2TilemapSize / 2];
        WorkRamWordTable bg2 = _room.Scratch.EnemyBg2Tilemap;
        for (int index = 0; index < words.Length; index++)
            words[index] = bg2[index];
        new EnemyVram(_presentation._vram!).EnemyBg2Tilemap.Write(words);
        state.EnemyBg2TilemapTransferRequested = true;
    }
    // `$86:CDD0-$CDE1` indexes the 256-word sine cycle by the already-even angle
    // offset in X: horizontal motion reads cosine base `$A0:B443`, while vertical
    // motion reads sine base `$A0:B3C3`. Expressing both through the sine base lets
    // the X call add the native quarter-turn (`+64` samples) exactly once. Using
    // `$B443` as this base silently shifted both components and made late angles read
    // unrelated ROM data beyond the table.

    /// <summary>
    /// Consumes one request emitted by PLM instruction <c>$D30B</c>. The request retains the
    /// executing PLM's block origin because initializer <c>$CDC5</c> calls
    /// <c>CalculatePlmBlockCoords(plm_id)</c> after the shared projectile allocator runs.
    /// </summary>
    public void SpawnMotherBrainGlassProjectile(MotherBrainGlassProjectileRequest request)
    {
        _room.EnsureLoaded();
        if (request.DefinitionPointer != (ushort)RoomEnemyProjectileKind.MotherBrainGlassShard)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                $"Mother Brain glass requested unsupported projectile " +
                $"$86:{request.DefinitionPointer:X4}.");
        }
        if (request.Parameter is not (0 or 2 or 4))
        {
            throw new InvalidDataException(
                $"Mother Brain glass shard parameter ${request.Parameter:X4} is not a " +
                "cartridge X/Y offset-table index.");
        }

        RoomEnemyProjectileSlot? shard = _projectiles.AllocateEnemyProjectile();
        if (shard is null)
            return; // SpawnEprojInner silently loses actors when all eighteen slots are full.

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            shard,
            RoomEnemyProjectileKind.MotherBrainGlassShard,
            graphicsIndex: 0);

        // `$86:CDC5` doubles the complete RNG word and masks it to an even nine-bit angle.
        // The low eight-bit angle selects signed sine/cosine velocities; bits 5..8 choose
        // one of sixteen ROM-authored shard animation lists.
        ushort doubledRandom = unchecked((ushort)(_room.Random.Generate() * 2));
        ushort angleOffset = unchecked((ushort)(doubledRandom & 0x01fe));
        int angle = angleOffset >> 1;
        shard.Variable0 = angleOffset;
        shard.Velocity = shard.Velocity.WithX(new SnesSignedEightEight(ReadSignedSineSample(angle + 64)));
        shard.Velocity = shard.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(
            unchecked((short)ReadSignedSineSample(angle)) * 4))));
        int animationIndex = ((angleOffset >> 4) & 0x001e) >> 1;
        shard.InstructionPointer = MotherBrainGlassShardDefinitions
            .InstructionPointer(unchecked((ushort)animationIndex));
        shard.InstructionTimer = NativeCountdown.Start(1);
        shard.GraphicsIndex = MotherBrainGlassShardDefinitions.GraphicsIndex;

        MotherBrainGlassShardPlacement placement =
            MotherBrainGlassShardDefinitions.Placement(request.Parameter);
        shard.Position = shard.Position.WithWhole(request.PlmBlock.ToRoomPixels() + placement.Offset);

        // Position jitter consumes two additional RNG samples in this order. It is not
        // cosmetic randomness: later turret and sparkle behavior observes the advanced seed.
        shard.Position = shard.Position.WithWholeX(unchecked((ushort)(
            shard.Position.X.Whole + (_room.Random.Generate() & 0x000f) - 8)));
        shard.Position = shard.Position.WithWholeY(unchecked((ushort)(
            shard.Position.Y.Whole + (_room.Random.Generate() & 0x000f) - 8)));
    }

    /// <summary>Ports glass-shard pre-instruction <c>$86:CE9B</c>.</summary>
    internal void RunMotherBrainGlassShardPreInstruction(RoomEnemyProjectileSlot shard)
    {
        shard.Position = shard.Position.Advance(shard.Velocity.ToSixteenSixteen());

        // Native tests the whole-coordinate high byte rather than camera visibility. A shard
        // is deleted as soon as wrapped Y leaves page zero; X may freely leave the viewport.
        if (SnesLittleEndian.HighByte(shard.Position.Y.Whole) != 0)
        {
            shard.Clear();
            return;
        }

        shard.Velocity = shard.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(shard.Velocity.Y.RawValue + 0x0020))));

        // The masked test succeeds for 1/16 of samples. The condition itself consumes one
        // random value every live shard frame; a successful sparkle consumes two more below.
        if ((_room.Random.Generate() & 0x0420) == 0)
            SpawnMotherBrainGlassSparkle(shard);
    }

    /// <summary>Ports sparkle initializer <c>$86:CE6D</c> using the spawning shard as parameter.</summary>
    internal void SpawnMotherBrainGlassSparkle(RoomEnemyProjectileSlot source)
    {
        RoomEnemyProjectileSlot? sparkle = _projectiles.AllocateEnemyProjectile();
        if (sparkle is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            sparkle,
            RoomEnemyProjectileKind.MotherBrainGlassSparkle,
            graphicsIndex: 0);
        sparkle.Position = sparkle.Position.WithWhole(new(unchecked((ushort)(
            source.Position.X.Whole + (_room.Random.Generate() & 0x001f) - 16)), unchecked((ushort)(
            source.Position.Y.Whole + (_room.Random.Generate() & 0x001f) - 16))));
        sparkle.GraphicsIndex = MotherBrainGlassShardDefinitions.GraphicsIndex;
    }

    /// <summary>Reads the native signed sine table as its raw two-byte sample.</summary>
    internal static ushort ReadSignedSineSample(int index) =>
        unchecked((ushort)EnemyTrigonometryTables.SignedNegativeCosineWord(index));
    /// <summary>Native body animation at <c>$A9:9A42</c> for the hand-beam windup.</summary>
    private const ushort MotherBrainHandBeamBodyInstruction = 0x9a42;
    /// <summary>Native walking animation at <c>$A9:9852</c> for the hand-beam approach.</summary>
    private const ushort MotherBrainHandBeamWalkInstruction = 0x9852;

    /// <summary>Ports the four-entry body dispatcher at <c>$A9:B87D-B8EA</c>.</summary>
    private static void RunMotherBrainHandBeamAttack(MotherBrainEnemyState state)
    {
        switch (state.HandBeamPhase)
        {
            case MotherBrainHandBeamPhase.BackUp:
                if (!TryWalkMotherBrainBackwardsForHandBeam(state))
                    return;
                state.LowerNeckMovementIndex = MotherBrainNeckMovement.Raise;
                state.UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
                state.HandBeamPhase = MotherBrainHandBeamPhase.WaitForBombs;
                return;

            case MotherBrainHandBeamPhase.WaitForBombs:
                // Bombs occupy the same eighteen-slot projectile pool. Native waits for
                // every active shell before starting the visually dense recursive beam.
                if (state.BombCounter != 0)
                    return;
                SetMotherBrainInstructionList(
                    state.Body,
                    MotherBrainHandBeamBodyInstruction);
                state.HandBeamPhase = MotherBrainHandBeamPhase.Firing;
                return;

            case MotherBrainHandBeamPhase.Firing:
                // `$A9:B8C8` is intentionally just RTS. Body bytecode at $9A42 owns the
                // charge effects, projectile emission, 240-frame hold, and phase advance.
                return;

            case MotherBrainHandBeamPhase.Finish:
                SetMotherBrainBrainInstructionList(
                    state,
                    MotherBrainNeutralPhaseTwoHeadInstruction);
                state.LowerNeckMovementIndex = MotherBrainNeckMovement.BobDown;
                state.UpperNeckMovementIndex = MotherBrainNeckMovement.BobUp;
                state.HandBeamPhase = MotherBrainHandBeamPhase.BackUp;
                state.Function = MotherBrainBodyFunction.SecondPhaseThinking;
                return;

            default:
                throw new InvalidDataException(
                    $"Mother Brain hand-beam phase {(ushort)state.HandBeamPhase} is " +
                    "outside the four-entry cartridge dispatcher.");
        }
    }

    /// <summary>
    /// Literal <c>MakeMotherBrainWalkBackwards($28, $08)</c>. Offset eight selects the slow
    /// authored gait; the helper's hard floor at X=$30 can report arrival before target $28.
    /// </summary>
    private static bool TryWalkMotherBrainBackwardsForHandBeam(
        MotherBrainEnemyState state)
    {
        ushort targetMinusBody = unchecked((ushort)(0x0028 - state.Body.Position.X.Whole));
        if ((targetMinusBody & 0x8000) == 0)
            return true;
        if (state.Pose != MotherBrainBodyPose.Standing)
            return false;
        if (unchecked((short)(state.Body.Position.X.Whole - 0x0030)) < 0)
            return true;

        SetMotherBrainInstructionList(
            state.Body,
            MotherBrainHandBeamWalkInstruction);
        return false;
    }

    /// <summary>
    /// Ports charging initializer <c>$86:C605</c>. The body record—not the articulated
    /// head—is the source: its extended map puts the hand at (+$40,-$30).
    /// </summary>
    internal void SpawnMotherBrainHandBeamCharging(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        RoomEnemyProjectileSlot? beam = _projectiles.AllocateEnemyProjectile();
        if (beam is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            beam,
            RoomEnemyProjectileKind.MotherBrainHandBeamCharging,
            graphicsIndex: 0x0040);
        beam.Variable0 = 0;
        beam.Variable1 = 0;
        beam.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
        beam.Position = beam.Position.WithFractionX(0).WithFractionY(0);
        beam.GraphicsIndex = 0x0400;
        beam.Position = beam.Position.WithWhole(new(unchecked((ushort)(state.Body.Position.X.Whole + 0x0040)), unchecked((ushort)(state.Body.Position.Y.Whole - 0x0030))));

        state.HandBeamNextPosition = FixedPosition<RoomPixels>.FromWhole(beam.Position.Whole);

        byte angle = unchecked((byte)(
            0x80 - EnemyMath.CalculateCartridgeAngle(samus.Position.Whole - beam.Position.Whole)));
        state.HandBeamNextAngle = angle;
        state.HandBeamNextVelocity = new EightEightVector<RoomPixels>(
            new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(0x0c00, angle)),
            new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(0x0c00, unchecked((byte)(angle + 0x40)))));

        beam.InstructionPointer = MotherBrainHandBeamInstructionProgramDefinitions.Initial;
        beam.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>
    /// Ports fired-child initializer <c>$86:C684-C75C</c>. Each child first advances the
    /// shared aimed cursor, then receives a randomized secondary displacement used only for
    /// its spawn frame. Surviving actors become stationary animated emitters; out-of-bounds
    /// children turn into the cartridge's large impact dust and earthquake.
    /// </summary>
    internal void SpawnMotherBrainHandBeamFired(ushort parentParameter)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain hand-beam bytecode ran without its encounter state.");
        RoomEnemyProjectileSlot? beam = _projectiles.AllocateEnemyProjectile();
        if (beam is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            beam,
            RoomEnemyProjectileKind.MotherBrainHandBeamFired,
            graphicsIndex: 0);
        beam.Position = state.HandBeamNextPosition;
        beam.Velocity = state.HandBeamNextVelocity;

        beam.Position = beam.Position.Advance(beam.Velocity.ToSixteenSixteen());
        state.HandBeamNextPosition = beam.Position;

        ushort angleRandom = _room.Random.Generate();
        byte scatterAngle = unchecked((byte)(
            state.HandBeamNextAngle + unchecked((byte)angleRandom)));
        ushort speedRandom = _room.Random.Generate();
        ushort scatterSpeed = unchecked((ushort)(speedRandom & 0x0700));
        beam.Velocity = beam.Velocity.WithX(new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(scatterSpeed, scatterAngle)));
        beam.Velocity = beam.Velocity.WithY(new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(
            scatterSpeed,
            unchecked((byte)(scatterAngle + 0x40)))));
        beam.Position = beam.Position.Advance(beam.Velocity.ToSixteenSixteen());

        if (IsMotherBrainHandBeamOutsideArena(beam))
        {
            ushort impactX = beam.Position.X.Whole;
            ushort impactY = beam.Position.Y.Whole;
            beam.Clear();
            _projectiles.SpawnRoomGraphicsDustExplosion(
                new(impactX, impactY),
                animationIndex: 0x001d);
            state.LastSoundEffectLibrary3 = 0x0013;
            _earthquake.EarthquakeTimer = NativeCountdown.Start(0x000a);
            _earthquake.EarthquakeType = 0x0005;
            return;
        }

        beam.Variable0 = unchecked((ushort)((parentParameter + 1) & 3));
        beam.Variable1 = 0;
        beam.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
        beam.InstructionPointer = MotherBrainHandBeamInstructionProgramDefinitions.Initial;
        beam.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Detects when a fired hand beam has crossed the fight arena's bounds.</summary>
    private static bool IsMotherBrainHandBeamOutsideArena(
        RoomEnemyProjectileSlot beam) =>
        unchecked((short)(beam.Position.Y.Whole - 0x0022)) < 0 ||
        unchecked((short)(beam.Position.Y.Whole - 0x00ce)) >= 0 ||
        unchecked((short)(beam.Position.X.Whole - 0x0002)) < 0 ||
        unchecked((short)(beam.Position.X.Whole - 0x00ee)) >= 0;
    /// <summary>Native crouched body list at <c>$A9:9A02</c> used for bomb attacks.</summary>
    private const ushort MotherBrainCrouchedInstruction = 0x9a02;
    /// <summary>Native slow uncrouch body list at <c>$A9:99AA</c>.</summary>
    private const ushort MotherBrainSlowUncrouchInstruction = 0x99aa;
    /// <summary>Native head-stretch list at <c>$A9:9B7F</c> during the phase-two rise.</summary>
    private const ushort MotherBrainStretchingHeadInstruction = 0x9b7f;
    /// <summary>Native neutral head list at <c>$A9:9C87</c> between phase-two attacks.</summary>
    private const ushort MotherBrainNeutralPhaseTwoHeadInstruction = 0x9c87;
    /// <summary>Native four-ring attack head list at <c>$A9:9D3D</c>.</summary>
    private const ushort MotherBrainFourOnionRingsInstruction = 0x9d3d;
    /// <summary>Native bomb-attack head list at <c>$A9:9ECC</c>.</summary>
    private const ushort MotherBrainBombPhaseTwoHeadInstruction = 0x9ecc;
    /// <summary>Native laser-attack head list at <c>$A9:9F34</c>.</summary>
    private const ushort MotherBrainLaserHeadInstruction = 0x9f34;

    /// <summary>Native X positions cycled when Mother Brain's ascent emits floor dust.</summary>
    private static readonly ushort[] MotherBrainAscentDustXPositions =
        [0x003d, 0x0054, 0x0020, 0x0035, 0x005a, 0x0043, 0x0067, 0x0029];

    /// <summary>Dispatches $A9:8D49-$8F45 without collapsing native same-frame fallthroughs.</summary>
    private void RunMotherBrainPhaseTwoAscent(
        MotherBrainEnemyState state,
        SamusState? samus,
        SamusBombProjectileSystem? sharedProjectiles = null)
    {
        switch (state.Function)
        {
            case MotherBrainBodyFunction.FakeDeathAscentSetupPhase2Brain:
                SetupMotherBrainPhaseTwoBrain(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense:
                PauseBeforeMotherBrainRises(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentPrepareForRising:
                PrepareMotherBrainForRising(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentLoadLegTiles:
                LoadMotherBrainLegTiles(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentContinuePausing:
                ContinueMotherBrainAscentPause(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentStartMusicAndEarthquake:
                StartMotherBrainAscent(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentRaiseMotherBrain:
                RaiseMotherBrain(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentWaitUntilUncrouched:
                WaitForMotherBrainToFinishUncrouching(state);
                return;
            case MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray:
                TransitionMotherBrainFromGray(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseStretchingShakeHead:
                ShakeMotherBrainHeadMenacingly(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseStretchingBringHeadUp:
                BringMotherBrainHeadBackUp(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseStretchingFinish:
                FinishMotherBrainStretching(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseThinking:
                RunMotherBrainSecondPhaseThinking(state, samus, sharedProjectiles);
                return;
            case MotherBrainBodyFunction.SecondPhaseTryAttack:
                RunMotherBrainSecondPhaseTryAttack(state, samus);
                return;
            case MotherBrainBodyFunction.SecondPhaseBombDecideWalking:
                DecideMotherBrainBombWalking(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseBombWalkingBackwards:
                ContinueMotherBrainBombWalk(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseBombCrouch:
                CrouchMotherBrainForBomb(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseBombFired:
                WaitAfterMotherBrainBomb(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseBombStandUp:
                StandMotherBrainAfterBomb(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseLaserPositionHeadQuickly:
                PositionMotherBrainHeadQuicklyForLaser(
                    state,
                    RequireMotherBrainCombatSamus(samus));
                return;
            case MotherBrainBodyFunction.SecondPhaseLaserPositionHeadSlowlyAndFire:
                PositionMotherBrainHeadSlowlyAndFireLaser(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseLaserFinishAttack:
                FinishMotherBrainLaserAttack(state, samus);
                return;
            case MotherBrainBodyFunction.SecondPhaseHandBeam:
                RunMotherBrainHandBeamAttack(state);
                return;
            case MotherBrainBodyFunction.SecondPhaseRainbowExtendNeck:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartCharging:
            case MotherBrainBodyFunction.SecondPhaseRainbowRetractNeck:
            case MotherBrainBodyFunction.SecondPhaseRainbowWaitForCharge:
            case MotherBrainBodyFunction.SecondPhaseRainbowExtendNeckDown:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartFiring:
            case MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall:
            case MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay:
            case MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus:
            case MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus:
            case MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring:
            case MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall:
            case MotherBrainBodyFunction.SecondPhaseRainbowWaitForSamusToLand:
            case MotherBrainBodyFunction.SecondPhaseRainbowLowerHead:
            case MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOff:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffStandUp:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffAdmire:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffChargeFinalBeam:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffLoadBabyTiles:
            case MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam:
            case MotherBrainBodyFunction.SecondPhaseFinalRainbowBeamHolding:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyRainbowBeamRunOut:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyMoveToBackOfRoom:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyGoIntoLowPowerMode:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyPrepareTransitionToGrey:
            case MotherBrainBodyFunction.SecondPhaseDrainedByBabyTransitionToGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveInanimateGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveShowSignsOfLife:
            case MotherBrainBodyFunction.SecondPhaseReviveTransitionFromGrey:
            case MotherBrainBodyFunction.SecondPhaseReviveWakeUp:
            case MotherBrainBodyFunction.SecondPhaseReviveWakeUpStretch:
            case MotherBrainBodyFunction.SecondPhaseReviveWalkUpToBaby:
            case MotherBrainBodyFunction.SecondPhaseRevivePrepareNeckForBabyDeath:
            case MotherBrainBodyFunction.SecondPhaseReviveFinishPreparingForBabyDeath:
            case MotherBrainBodyFunction.SecondPhaseMurderBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseMurderBabyAttackCooldown:
            case MotherBrainBodyFunction.SecondPhasePrepareForFinalBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseExecuteFinalBabyAttack:
            case MotherBrainBodyFunction.SecondPhaseFinalBabyAttackHolding:
            case MotherBrainBodyFunction.ThirdPhaseRecoverMakeSomeDistance:
            case MotherBrainBodyFunction.ThirdPhaseRecoverSetupForFighting:
            case MotherBrainBodyFunction.ThirdPhaseFightingMain:
            case MotherBrainBodyFunction.ThirdPhaseFightingAttackCooldown:
            case MotherBrainBodyFunction.ThirdPhaseDeathMoveToBackOfRoom:
            case MotherBrainBodyFunction.ThirdPhaseDeathIdleWhilstExploding:
            case MotherBrainBodyFunction.ThirdPhaseDeathStumbleToMiddleOfRoom:
            case MotherBrainBodyFunction.ThirdPhaseDeathDisableBrainEffects:
            case MotherBrainBodyFunction.ThirdPhaseDeathSetupBodyFadeOut:
            case MotherBrainBodyFunction.ThirdPhaseDeathFadeOutBody:
            case MotherBrainBodyFunction.ThirdPhaseDeathFinalFewExplosions:
            case MotherBrainBodyFunction.ThirdPhaseDeathRealizeDecapitation:
            case MotherBrainBodyFunction.ThirdPhaseDeathBrainFallsToGround:
            case MotherBrainBodyFunction.ThirdPhaseDeathLoadCorpseTiles:
            case MotherBrainBodyFunction.ThirdPhaseDeathSetupFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseTipsOver:
            case MotherBrainBodyFunction.ThirdPhaseDeathCorpseRotsAway:
            case MotherBrainBodyFunction.ThirdPhaseDeath20FrameDelay:
            case MotherBrainBodyFunction.ThirdPhaseDeathLoadEscapeTimerTiles:
            case MotherBrainBodyFunction.ThirdPhaseDeathStartEscape:
            case MotherBrainBodyFunction.ThirdPhaseDeathSpawnTimeBombSetSubtitle:
            case MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText:
            case MotherBrainBodyFunction.ThirdPhaseDeathDoorExplodingStartTimer:
            case MotherBrainBodyFunction.ThirdPhaseDeathBlowUpEscapeDoor:
            case MotherBrainBodyFunction.ThirdPhaseDeathKeepEarthquakeGoing:
                RunLiveMotherBrainRainbowBeam(
                    state,
                    RequireMotherBrainCombatSamus(samus),
                    unchecked((byte)state.Body.FrameCounter),
                    sharedProjectiles);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain phase-two function $A9:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Attaches the phase-two head and prepares its first animation and position.</summary>
    private void SetupMotherBrainPhaseTwoBrain(
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
        state.Head!.Properties =
            state.Head.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        state.Head.Health = 0x4650;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentPauseForSuspense;
        state.FunctionTimer = NativeCountdown.Start(0x0080);

        // `$8D49` falls directly into `$8D79`; the freshly written timer is observed as
        // $007F before another enemy slot can run.
        PauseBeforeMotherBrainRises(state);
    }

    /// <summary>Holds the phase-two body before its scripted ascent begins.</summary>
    private void PauseBeforeMotherBrainRises(
        MotherBrainEnemyState state)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        state.Function = MotherBrainBodyFunction.FakeDeathAscentPrepareForRising;
        state.FunctionTimer = NativeCountdown.Start(0x0020);
        PrepareMotherBrainForRising(state);
    }

    /// <summary>Stages the body and head state used by the rise from the collapsed room.</summary>
    internal void PrepareMotherBrainForRising(
        MotherBrainEnemyState state)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        // Bank $88 installs a fixed per-scanline main-screen table that prevents the newly
        // loaded leg sprites from appearing over the floor. The renderer consumes this
        // typed lifetime rather than pretending it is a body-visibility flag.
        state.RisingHdmaActive = true;
        state.Head!.Properties = state.Head.Properties.With(EnemyProperties.Invisible);
        SetMotherBrainBrainInstructionList(state, MotherBrainInitialHeadInstruction);
        state.Function = MotherBrainBodyFunction.FakeDeathAscentLoadLegTiles;
        state.FunctionTimer = NativeCountdown.Start(0x0100);

        // The first $0200-byte transfer is queued on the same frame as HDMA creation.
        LoadMotherBrainLegTiles(state);
    }

    /// <summary>Loads the leg tiles needed before the phase-two body becomes visible.</summary>
    private void LoadMotherBrainLegTiles(
        MotherBrainEnemyState state)
    {
        if (!ProcessMotherBrainSpriteTileTransfer(state))
        {
            return;
        }

        state.Function = MotherBrainBodyFunction.FakeDeathAscentContinuePausing;

        // `$8DBD` has no return between the state write and `$8DC3`; the still-$0100 timer
        // loses its first tick on the final DMA frame.
        ContinueMotherBrainAscentPause(state);
    }

    /// <summary>Drains the pending body sprite transfer and reports whether more VRAM work remains.</summary>
    internal bool ProcessMotherBrainSpriteTileTransfer(MotherBrainEnemyState state)
    {
        int index = state.SpriteTileTransferEntryPointer == 0
            ? 0
            : MotherBrainLegTileTransferDefinitions.IndexOf(
                state.SpriteTileTransferEntryPointer);
        MotherBrainSpriteTileTransferRequest transfer =
            MotherBrainLegTileTransferDefinitions.Get(index);
        _motherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(transfer);

        // Native checks the next record's zero terminator on the same call as
        // page eleven. Preserve that same-frame completion and pointer reset.
        if (index + 1 == MotherBrainLegTileTransferDefinitions.PageCount)
        {
            state.SpriteTileTransferEntryPointer = 0;
            return true;
        }

        state.SpriteTileTransferEntryPointer = unchecked((ushort)(
            MotherBrainLegTileTransferDefinitions.NativeListPointer +
            (index + 1) * MotherBrainLegTileTransferDefinitions.RecordByteCount));
        return false;
    }

    /// <summary>Consumes the pre-rise wait and advances to body ascent.</summary>
    private void ContinueMotherBrainAscentPause(MotherBrainEnemyState state)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        state.Body.Position = state.Body.Position.WithWhole(new(0x003b, 0x0117));
        PublishMotherBrainBg2Scroll(state, new(0xffe5, 0xff27));
        state.HitboxesEnabled = 7;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentStartMusicAndEarthquake;
    }

    /// <summary>Switches the body into the rising state after its setup pause.</summary>
    private void StartMotherBrainAscent(MotherBrainEnemyState state)
    {
        SetMotherBrainInstructionList(state.Body, MotherBrainCrouchedInstruction);
        state.Head!.InstructionTimer = NativeCountdown.Start(1);
        state.Body.Properties = state.Body.Properties.Without(EnemyProperties.Invisible);
        state.Body.Position = state.Body.Position.WithWhole(new(0x003b, 0x0117));
        PublishMotherBrainBg2Scroll(state, new(0xffe5, 0xff27));
        state.RequestMusic(MusicCommand.SelectTrack(5), MusicCommandDelay.EightFrames);
        _earthquake.EarthquakeType = 2;
        _earthquake.EarthquakeTimer = NativeCountdown.Start(0x0100);
        state.NeckAngleDelta = 0x0050;
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
        if ((_frameContext._enemyFrameNmiFrameCounter & 3) != 0)
            return;

        SpawnMotherBrainAscentDust(state);
        state.Bg2Scroll = state.Bg2Scroll + new Offset16<Bg2Pixels>(0, 2);
        state.Body.Position = state.Body.Position.WithWholeY(unchecked((ushort)(state.Body.Position.Y.Whole - 2)));
        PublishMotherBrainBg2Scroll(state, state.Bg2Scroll);
        if (state.Body.Position.Y.Whole >= 0x00bd)
            return;

        state.EnemyBg2TilemapSize = 0x0140;
        state.EnemyBg2TilemapTransferRequested = true;
        state.Body.Position = state.Body.Position.WithWholeY(0x00bc);
        _earthquake.EarthquakeTimer = NativeCountdown.Start(0);
        state.RisingHdmaActive = false;
        SetMotherBrainInstructionList(state.Body, MotherBrainSlowUncrouchInstruction);
        state.Function = MotherBrainBodyFunction.FakeDeathAscentWaitUntilUncrouched;
    }

    /// <summary>Emits dust at the feet as the body rises from the floor.</summary>
    private void SpawnMotherBrainAscentDust(MotherBrainEnemyState state)
    {
        NativeWordCounterStep timer = state.BodySubFunctionTimer.Decrement();
        state.BodySubFunctionTimer = timer.Remaining;
        if (timer.IsNegative)
            state.BodySubFunctionTimer = NativeCountdown.Start(7);

        ushort x = MotherBrainAscentDustXPositions[state.BodySubFunctionTimer.RawValue];
        ushort random = _room.Random.Read();
        ushort animation = (random & 0x0100) == 0 ? (ushort)9 : (ushort)0x12;
        _projectiles.SpawnRoomGraphicsDustExplosion(new(x, 0x00d4), animation);
        state.LastSoundEffect = 0x0029;
    }

    /// <summary>Holds the body phase until its slow uncrouch animation ends.</summary>
    private static void WaitForMotherBrainToFinishUncrouching(MotherBrainEnemyState state)
    {
        if (state.Pose != MotherBrainBodyPose.Standing)
            return;

        state.GrayTransitionCounter = 0;
        state.Function = MotherBrainBodyFunction.FakeDeathAscentTransitionFromGray;
        state.FunctionTimer = NativeCountdown.Start(0);
    }

    /// <summary>Restores the combat palette after the gray fake-death phase.</summary>
    internal void TransitionMotherBrainFromGray(MotherBrainEnemyState state)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        state.FunctionTimer = NativeCountdown.Start(4);
        ushort paletteStep = state.GrayTransitionCounter++;
        if (paletteStep < MotherBrainFakeDeathPaletteRomData.FrameCount)
        {
            // Fake-death restoration changes only brain sprite colors one through three.
            if (_presentation.MotherBrainRainbowColors is { } colors)
                colors.ApplyFakeDeathFromGrey(_presentation._cgram!, paletteStep);
            else throw new InvalidOperationException(
                "Mother Brain revival requires installed colors.");
            return;
        }

        state.BrainPaletteHandlingEnabled = true;
        state.Form = 2;
        state.DroolGenerationEnabled = true;
        state.LowerNeckMovementIndex = MotherBrainNeckMovement.Lower;
        state.UpperNeckMovementIndex = MotherBrainNeckMovement.Lower;
        state.NeckAngleDelta = 0x0500;
        state.Function = MotherBrainBodyFunction.SecondPhaseStretchingShakeHead;
        state.FunctionTimer = NativeCountdown.Start(0x0017);
    }

    /// <summary>Applies the phase-two head shake before normal combat begins.</summary>
    private static void ShakeMotherBrainHeadMenacingly(
        MotherBrainEnemyState state)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        SetMotherBrainBrainInstructionList(state, MotherBrainStretchingHeadInstruction);
        state.Function = MotherBrainBodyFunction.SecondPhaseStretchingBringHeadUp;
        state.NeckAngleDelta = 0x0040;
        state.FunctionTimer = NativeCountdown.Start(0x0100);
        BringMotherBrainHeadBackUp(state);
    }

    /// <summary>Returns the head from its lowered pose after the stretch.</summary>
    private static void BringMotherBrainHeadBackUp(
        MotherBrainEnemyState state)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        state.LowerNeckMovementIndex = MotherBrainNeckMovement.BobDown;
        state.UpperNeckMovementIndex = MotherBrainNeckMovement.BobUp;
        state.Function = MotherBrainBodyFunction.SecondPhaseStretchingFinish;
        state.FunctionTimer = NativeCountdown.Start(0x0040);
        FinishMotherBrainStretching(state);
    }

    /// <summary>Switches the head to its neutral phase-two instruction list.</summary>
    private static void FinishMotherBrainStretching(MotherBrainEnemyState state)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        state.SmallPurpleBreathGenerationEnabled = true;
        state.Function = MotherBrainBodyFunction.SecondPhaseThinking;
    }

    /// <summary>Advances the between-attacks delay and chooses when another attack may start.</summary>
    private void RunMotherBrainSecondPhaseThinking(
        MotherBrainEnemyState state,
        SamusState? samus,
        SamusBombProjectileSystem? sharedProjectiles = null)
    {
        if (state.Head!.Health == 0)
        {
            StartLiveMotherBrainRainbowBeam(
                state,
                RequireMotherBrainCombatSamus(samus),
                sharedProjectiles);
            return;
        }
        if (state.Pose != MotherBrainBodyPose.Standing)
            return;

        ushort random = _room.Random.Read();
        if (state.Head.Health >= 0x1194)
        {
            if (random < 0x1000)
            {
                // `$B605` installs the attack dispatcher and returns. It does not execute
                // phase zero until the body's next enemy-AI call.
                state.Function = MotherBrainBodyFunction.SecondPhaseTryAttack;
                return;
            }
            HandleMotherBrainWalking(state);
            return;
        }

        if (random < 0x2000)
        {
            HandleMotherBrainWalking(state);
            return;
        }
        if (random >= 0xa000)
        {
            state.Function = MotherBrainBodyFunction.SecondPhaseTryAttack;
            return;
        }
        // `$B644` installs the ordinary red hand-beam dispatcher. It does not execute phase
        // zero until the next body AI call; `$B8F1` belongs to the separate rainbow beam
        // entered only when health reaches zero.
        state.Function = MotherBrainBodyFunction.SecondPhaseHandBeam;
    }

    /// <summary>
    /// Ports the three-entry body dispatcher at <c>$A9:B64B-B780</c>. The selected head
    /// list is ordinary enemy bytecode, so attacks remain synchronized with native map
    /// durations instead of being represented by host timers.
    /// </summary>
    private void RunMotherBrainSecondPhaseTryAttack(
        MotherBrainEnemyState state,
        SamusState? samus)
    {
        switch (state.AttackPhase)
        {
            case MotherBrainAttackPhase.ChooseAttack:
                ChooseMotherBrainSecondPhaseAttack(state, RequireMotherBrainCombatSamus(samus));
                return;

            case MotherBrainAttackPhase.Cooldown:
                // `$B764` decrements before testing. The phase-zero initializer always
                // writes $40, but unchecked arithmetic retains the native zero-underflow
                // behavior if a debugger edits this word while stopped here.
                state.AttackCooldown = state.AttackCooldown.Decrement().Remaining;
                if (state.AttackCooldown.IsZero)
                    state.AttackPhase = MotherBrainAttackPhase.EndAttack;
                return;

            case MotherBrainAttackPhase.EndAttack:
                state.AttackPhase = MotherBrainAttackPhase.ChooseAttack;
                state.Function = MotherBrainBodyFunction.SecondPhaseThinking;
                return;

            default:
                throw new InvalidDataException(
                    $"Mother Brain attack phase {(ushort)state.AttackPhase} is outside the " +
                    "three-entry cartridge dispatcher.");
        }
    }

    /// <summary>Ports <c>$A9:B65A-B72B</c>, including its unusual stack-return strategy.</summary>
    private void ChooseMotherBrainSecondPhaseAttack(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        state.AttackCooldown = NativeCountdown.Start(0x0040);
        state.AttackPhase = MotherBrainAttackPhase.Cooldown;

        byte randomLow = unchecked((byte)_room.Random.Read());
        SamusMovementType movementType = samus.PoseMetadata.ReadMovementType(_room.Bus!);
        if ((byte)movementType >= 0x1c)
        {
            throw new InvalidDataException(
                $"Mother Brain attack strategy cannot index Samus movement type ${(byte)movementType:X2}.");
        }

        if (IsMotherBrainAirAttackMovement(movementType))
        {
            // The native helper removes its caller's return address: every airborne route
            // commits immediately and skips the proximity/fallback thresholds entirely.
            if (randomLow >= 0x80)
            {
                SetMotherBrainBrainInstructionList(
                    state,
                    MotherBrainFourOnionRingsInstruction);
                return;
            }

            // `$B702` discards the strategy helper's return address, then installs and
            // jumps to `$B80E`. Calling the first body state here reproduces that same-frame
            // tail call rather than introducing an otherwise visible idle frame.
            state.Function = MotherBrainBodyFunction.SecondPhaseLaserPositionHeadQuickly;
            PositionMotherBrainHeadQuicklyForLaser(state, samus);
            return;
        }

        // Grounded movement types attempt a bomb on the upper half of the current random
        // byte. An already-live bomb falls back to the normal threshold table.
        if (randomLow >= 0x80 && state.BombCounter < 1)
        {
            state.Function = MotherBrainBodyFunction.SecondPhaseBombDecideWalking;
            DecideMotherBrainBombWalking(state);
            return;
        }

        ushort verticalDistance = EnemyMath.WrappedMagnitude(unchecked((ushort)(
            state.Head!.Position.Y.Whole + 4 - samus.Position.Y.Whole)));
        ReadOnlySpan<byte> thresholds = verticalDistance < 0x20
            ? [0x10, 0x20, 0xd0]
            : [0x40, 0x80, 0xc0];
        int choice = randomLow < thresholds[0]
            ? 0
            : randomLow < thresholds[1]
                ? 1
                : randomLow < thresholds[2]
                    ? 2
                    : 3;

        switch (choice)
        {
            case 0:
                SetMotherBrainBrainInstructionList(
                    state,
                    MotherBrainNeutralPhaseTwoHeadInstruction);
                return;
            case 1:
                SetMotherBrainBrainInstructionList(
                    state,
                    MotherBrainFourOnionRingsInstruction);
                return;
            case 2:
                state.Function = MotherBrainBodyFunction.SecondPhaseLaserPositionHeadQuickly;
                PositionMotherBrainHeadQuicklyForLaser(state, samus);
                return;
            case 3 when state.BombCounter >= 1:
                // `$B6B9` returns without changing the current head list when one bomb is
                // already alive. The body still spends the full cooldown in phase one.
                return;
            default:
                state.Function = MotherBrainBodyFunction.SecondPhaseBombDecideWalking;
                DecideMotherBrainBombWalking(state);
                return;
        }
    }

    /// <summary>Identifies Samus movement types considered airborne by the attack chooser.</summary>
    private static bool IsMotherBrainAirAttackMovement(SamusMovementType movementType) =>
        movementType is
            SamusMovementType.NormalJumping or
            SamusMovementType.SpinJumping or
            SamusMovementType.Falling or
            SamusMovementType.MorphBallFalling or
            SamusMovementType.UnusedGlitchBallAlternate or
            SamusMovementType.Unused0B or
            SamusMovementType.Unused0C or
            SamusMovementType.Unused0D or
            SamusMovementType.SpringBallInAir or
            SamusMovementType.SpringBallFalling or
            SamusMovementType.WallJumping;

    /// <summary>Ports <c>$A9:B781-B7AB</c>, including its immediate posture decision.</summary>
    private void DecideMotherBrainBombWalking(MotherBrainEnemyState state)
    {
        ushort random = _room.Random.Read();
        if (random < 0xff80)
        {
            ushort targetX = random >= 0x6000 ? (ushort)0x0040 : (ushort)0x0060;
            ushort targetMinusBody = unchecked((ushort)(targetX - state.Body.Position.X.Whole));
            if ((targetMinusBody & 0x8000) != 0)
            {
                state.BodyTargetXPosition = targetX;
                if (!TryWalkMotherBrainBackwards(state, targetX))
                {
                    state.Function = MotherBrainBodyFunction.SecondPhaseBombWalkingBackwards;
                    return;
                }
            }
        }

        DecideWhetherMotherBrainCrouchesForBomb(state);
    }

    /// <summary>Ports stored body state <c>$A9:B7AC</c>.</summary>
    private void ContinueMotherBrainBombWalk(MotherBrainEnemyState state)
    {
        if (TryWalkMotherBrainBackwards(state, state.BodyTargetXPosition))
            DecideWhetherMotherBrainCrouchesForBomb(state);
    }

    /// <summary>
    /// Ports the fallthrough at <c>$A9:B7B7</c>. This routine advances the global RNG—unlike
    /// attack selection—and fires immediately on the lower half; the upper half enters the
    /// ordinary slow-crouch body list before firing.
    /// </summary>
    private void DecideWhetherMotherBrainCrouchesForBomb(MotherBrainEnemyState state)
    {
        if (_room.Random.Generate() < 0x8000)
        {
            FireMotherBrainBomb(state);
            return;
        }

        state.Function = MotherBrainBodyFunction.SecondPhaseBombCrouch;
        CrouchMotherBrainForBomb(state);
    }

    /// <summary>Ports stored body state <c>$A9:B7C6</c>.</summary>
    private static void CrouchMotherBrainForBomb(MotherBrainEnemyState state)
    {
        if (TryMakeMotherBrainCrouch(state))
            FireMotherBrainBomb(state);
    }

    /// <summary>Ports the unstored firing tail at <c>$A9:B7CB</c>.</summary>
    private static void FireMotherBrainBomb(MotherBrainEnemyState state)
    {
        SetMotherBrainBrainInstructionList(state, MotherBrainBombPhaseTwoHeadInstruction);
        state.Function = MotherBrainBodyFunction.SecondPhaseBombFired;
        state.FunctionTimer = NativeCountdown.Start(0x002c);
    }

    /// <summary>Ports stored body state <c>$A9:B7E8</c> and its stand-up fallthrough.</summary>
    private static void WaitAfterMotherBrainBomb(MotherBrainEnemyState state)
    {
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (!state.FunctionTimer.IsNegative)
            return;

        if (TryMakeMotherBrainStand(state))
        {
            FinishMotherBrainBombAttack(state);
            return;
        }

        state.Function = MotherBrainBodyFunction.SecondPhaseBombStandUp;

        // `$B7F2` falls directly into `$B7F8`, so a crouched body calls the helper twice on
        // this frame. Reinstalling the same list/timer is observable to a debugger and must
        // not be collapsed into a host-only transition delay.
        StandMotherBrainAfterBomb(state);
    }

    /// <summary>Ports stored body state <c>$A9:B7F8</c>.</summary>
    private static void StandMotherBrainAfterBomb(MotherBrainEnemyState state)
    {
        if (TryMakeMotherBrainStand(state))
            FinishMotherBrainBombAttack(state);
    }

    /// <summary>Returns the body and head to their neutral thinking phase after a bomb.</summary>
    private static void FinishMotherBrainBombAttack(MotherBrainEnemyState state) =>
        state.Function = MotherBrainBodyFunction.SecondPhaseThinking;

    /// <summary>Literal target/pose/list behavior of <c>$A9:C647</c> at delay-table offset six.</summary>
    private static bool TryWalkMotherBrainBackwards(
        MotherBrainEnemyState state,
        ushort targetX)
    {
        ushort targetMinusBody = unchecked((ushort)(targetX - state.Body.Position.X.Whole));
        if ((targetMinusBody & 0x8000) == 0)
            return true;
        if (state.Pose != MotherBrainBodyPose.Standing)
            return false;
        if (unchecked((short)(state.Body.Position.X.Whole - 0x0030)) < 0)
            return true;

        SetMotherBrainInstructionList(state.Body, 0x9900); // Backwards, medium.
        return false;
    }

    /// <summary>Literal pose/list behavior of <c>$A9:C68E</c>.</summary>
    private static bool TryMakeMotherBrainCrouch(MotherBrainEnemyState state)
    {
        if (state.Pose == MotherBrainBodyPose.Crouched)
            return true;
        if (state.Pose == MotherBrainBodyPose.Standing)
            // The static crouched list is only appropriate during ascent initialization.
            // Combat must first lower the body so the subsequent stand reverses that move.
            SetMotherBrainInstructionList(state.Body, MotherBrainBodyInstructionLists.CrouchSlow);
        return false;
    }

    /// <summary>Literal pose/list behavior of <c>$A9:C670</c>.</summary>
    private static bool TryMakeMotherBrainStand(MotherBrainEnemyState state)
    {
        switch (state.Pose)
        {
            case MotherBrainBodyPose.Standing:
                return true;
            case MotherBrainBodyPose.Crouched:
                SetMotherBrainInstructionList(state.Body, 0x99c6);
                return false;
            case MotherBrainBodyPose.LeaningDown:
                SetMotherBrainInstructionList(state.Body, 0x99e2);
                return false;
            case MotherBrainBodyPose.Walking:
            case MotherBrainBodyPose.CrouchingTransition:
            case MotherBrainBodyPose.DeathBeam:
                return false;
            default:
                throw new InvalidOperationException($"Undefined MotherBrainBodyPose {state.Pose}.");
        }
    }

    /// <summary>Requires a live Samus state for combat-only phase logic.</summary>
    private static SamusState RequireMotherBrainCombatSamus(SamusState? samus) =>
        samus ?? throw new InvalidOperationException(
            "Mother Brain phase-two attack selection requires the active Samus actor.");

    /// <summary>
    /// Ports body state <c>$A9:B80E</c>. Both neck joints initially turn by two angle units
    /// per frame toward one of two authored target indices chosen from the head/Samus Y
    /// ordering; the head instruction list itself is deliberately left untouched here.
    /// </summary>
    private static void PositionMotherBrainHeadQuicklyForLaser(
        MotherBrainEnemyState state,
        SamusState samus)
    {
        RoomEnemySlot head = state.Head!;

        // CMP/BPL tests the sign bit of the wrapped 16-bit subtraction. Normal arena
        // coordinates cannot overflow signed range, but expressing the native flag test
        // explicitly keeps debugger-edited/wrapped coordinates cartridge-faithful too.
        ushort headMinusSamus = unchecked((ushort)(head.Position.Y.Whole - samus.Position.Y.Whole));
        MotherBrainNeckMovement targetIndex = (headMinusSamus & 0x8000) == 0
            ? MotherBrainNeckMovement.Raise
            : MotherBrainNeckMovement.Lower;
        state.LowerNeckMovementIndex = targetIndex;
        state.UpperNeckMovementIndex = targetIndex;
        state.NeckAngleDelta = 0x0200;
        state.Function = MotherBrainBodyFunction.SecondPhaseLaserPositionHeadSlowlyAndFire;
        state.FunctionTimer = NativeCountdown.Start(0x0004);
    }

    /// <summary>
    /// Ports <c>$A9:B839</c>. Its unsigned timer expires only after decrementing through
    /// zero to <c>$FFFF</c>, at which point neck rotation slows and ordinary head bytecode
    /// owns the muzzle animation and projectile spawn.
    /// </summary>
    private static void PositionMotherBrainHeadSlowlyAndFireLaser(
        MotherBrainEnemyState state)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        state.NeckAngleDelta = (state.NeckAngleDelta & 0x8000) == 0
            ? (ushort)0x0100
            : (ushort)0xff00;
        SetMotherBrainBrainInstructionList(state, MotherBrainLaserHeadInstruction);
        state.Function = MotherBrainBodyFunction.SecondPhaseLaserFinishAttack;
        state.FunctionTimer = NativeCountdown.Start(0x0010);
    }

    /// <summary>
    /// Ports <c>$A9:B863</c>, including its same-frame jump back into thinking. The attack
    /// phase/cooldown words intentionally remain as selected: the laser body state replaces
    /// `$B64B`, so the native chain does not pass through that dispatcher's end phase.
    /// </summary>
    private void FinishMotherBrainLaserAttack(
        MotherBrainEnemyState state,
        SamusState? samus)
    {
        if (!DecrementMotherBrainTimerPastZero(state))
            return;

        state.LowerNeckMovementIndex = MotherBrainNeckMovement.BobUp;
        state.UpperNeckMovementIndex = MotherBrainNeckMovement.BobUp;
        state.Function = MotherBrainBodyFunction.SecondPhaseThinking;
        RunMotherBrainSecondPhaseThinking(state, samus);
    }

    /// <summary>Advances the chosen walk direction, animation, and footstep cadence.</summary>
    private void HandleMotherBrainWalking(MotherBrainEnemyState state)
    {
        // `$A9:C6B8` alters only the walk accumulator and installs a ROM-authored body list;
        // actual displacement remains bytecode-owned, which also keeps BG2 scroll aligned.
        if (state.Pose != MotherBrainBodyPose.Standing)
            return;
        if (state.WalkCounter == 0)
        {
            state.WalkCounter = 1;
            if (state.Body.Position.X.Whole >= 0x0030)
            {
                SetMotherBrainInstructionList(state.Body, MotherBrainBodyInstructionLists.WalkBackwardsFast);
                return;
            }
        }
        else
        {
            state.WalkCounter = unchecked((ushort)(state.WalkCounter + 6));
            if (state.WalkCounter >= 0x0100)
            {
                StartMotherBrainForwardWalk(state);
                return;
            }
            if (state.Body.Position.X.Whole >= 0x0030)
                return;
        }

        // The cartridge samples the current RNG word; it does not advance the generator.
        if ((_room.Random.Read() & MotherBrainBodyInstructionLists.ForwardWalkRandomMask) >=
            MotherBrainBodyInstructionLists.ForwardWalkRandomThreshold)
            StartMotherBrainForwardWalk(state);
    }

    /// <summary>Starts the body animation and state for a forward step.</summary>
    private static void StartMotherBrainForwardWalk(MotherBrainEnemyState state)
    {
        state.WalkCounter = 0x0080;
        if (state.Body.Position.X.Whole < 0x0080)
            SetMotherBrainInstructionList(state.Body, MotherBrainBodyInstructionLists.WalkForwardsMedium);
    }

    /// <summary>
    /// Literal body/BG2 movement helper at <c>$A9:9579</c>. The signed Y operand is added
    /// to the actor and subtracted from the background, keeping the large BG2 body art
    /// rigidly attached to its small OBJ hitbox record. X movement happens before this call
    /// in the cartridge, so the derived scroll must sample the already-updated body X.
    /// </summary>
    private void MoveMotherBrainBody(Offset16<RoomPixels> displacement)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain body bytecode ran without its multipart encounter state.");
        state.Body.Position += displacement;
        PublishMotherBrainBg2Scroll(state, new(
            unchecked((ushort)(0x0022 - state.Body.Position.X.Whole)),
            unchecked((ushort)(state.Bg2Scroll.Y - displacement.Y))));
    }

    /// <summary>
    /// Posture-transition variant at <c>$A9:9552</c>. Its X register is a scroll bias, not
    /// actor displacement; only Y changes the physical body coordinate.
    /// </summary>
    private void MoveMotherBrainBodyWithScrollBias(
        short yDisplacement,
        short horizontalScrollBias)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain posture bytecode ran without its multipart encounter state.");
        state.Body.Position = state.Body.Position.WithWholeY(unchecked((ushort)(state.Body.Position.Y.Whole + yDisplacement)));
        PublishMotherBrainBg2Scroll(state, new(
            unchecked((ushort)(0x0022 + horizontalScrollBias - state.Body.Position.X.Whole)),
            unchecked((ushort)(state.Bg2Scroll.Y - yDisplacement))));
    }

    /// <summary>Shared four-frame earthquake produced by each planted walking frame.</summary>
    private void RunMotherBrainFootstep()
    {
        _earthquake.EarthquakeType = 1;
        _earthquake.EarthquakeTimer = NativeCountdown.Start(4);

        // Form three additionally queues library-three sound $16. Phase two normally never
        // reaches that branch, but retaining it here makes the shared gait opcode complete.
        if (_motherBrainEncounter.MotherBrain?.Form == 3)
            _motherBrainEncounter.MotherBrain.LastSoundEffectLibrary3 = SoundEffectLibrary3Sounds.MotherBrainFormThreeFootstep.Value;
    }

    /// <summary>Restarts a slot's native animation list with its initial timer.</summary>
    internal static void SetMotherBrainInstructionList(RoomEnemySlot slot, ushort pointer)
    {
        slot.CurrentInstruction = pointer;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Publishes the encounter's BG2 offset for the current body motion.</summary>
    private void PublishMotherBrainBg2Scroll(
        MotherBrainEnemyState state,
        Position16<Bg2Pixels> scroll)
    {
        state.Bg2Scroll = scroll;
        state.HasBg2ScrollOverride = true;
        _room.Services.MotherBrain.SetBg2Scroll(scroll);
    }
    /// <summary>Six authored drool spawn offsets from the brain's center.</summary>
    private static readonly Offset16<RoomPixels>[] MotherBrainDroolOffsets =
        [new(6, 20), new(14, 18), new(8, 23), new(10, 19), new(11, 25), new(12, 18)];

    /// <summary>
    /// Ports head opcode <c>$A9:9E29</c> and initializer <c>$86:C2F3</c>. The shared
    /// allocator deliberately runs after the twelve room turrets and all lingering drool,
    /// so pool saturation drops the ring exactly as it does on hardware.
    /// </summary>
    internal void SpawnMotherBrainOnionRing(
        MotherBrainEnemyState state,
        byte angle)
    {
        RoomEnemyProjectileSlot? ring = _projectiles.AllocateEnemyProjectile();
        if (ring is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            ring,
            RoomEnemyProjectileKind.MotherBrainOnionRing,
            graphicsIndex: 0x0400);
        ring.Variable0 = 8;
        ring.Variable1 = 0;
        ring.DirectionParameter = angle;
        ring.Velocity = ring.Velocity.WithX(new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(0x0450, angle)));
        ring.Velocity = ring.Velocity.WithY(new SnesSignedEightEight(EnemyMath.MultiplyCartridgeSinCos(
            0x0450,
            unchecked((byte)(angle + 0x40)))));

        // `$C2F3` falls through into `$C320`, making the mouth position observable before
        // the projectile scheduler performs the first of eight delayed attachment frames.
        PinMotherBrainOnionRingToMouth(ring, state);
    }

    /// <summary>Ports head opcode <c>$A9:9B3C</c> and initializer <c>$86:C843</c>.</summary>
    internal void SpawnMotherBrainDrool(MotherBrainEnemyState state)
    {
        if (!state.DroolGenerationEnabled)
            return;

        ushort parameter = unchecked((ushort)(state.DroolProjectileParameter + 1));
        if (parameter >= 6)
            parameter = 0;
        state.DroolProjectileParameter = parameter;

        RoomEnemyProjectileSlot? drool = _projectiles.AllocateEnemyProjectile();
        if (drool is null)
            return;

        RoomEnemyProjectileKind kind = state.NeckAngleDelta < 0x0080
            ? RoomEnemyProjectileKind.MotherBrainDrool
            : RoomEnemyProjectileKind.MotherBrainDyingDrool;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(drool, kind, graphicsIndex: 0);
        drool.Variable0 = parameter;

        // SpawnEnemyProjectile runs initializer C843 immediately. It falls through into
        // the attached pre-instruction, so debugger-visible coordinates are valid before
        // the later global projectile pass repeats the same attachment calculation.
        RunMotherBrainAttachedDroolPreInstruction(drool);
    }

    /// <summary>Ports head opcode <c>$A9:9B6D</c> and initializer <c>$86:CA6A</c>.</summary>
    private void SpawnMotherBrainPurpleBreathBig(MotherBrainEnemyState state)
    {
        RoomEnemyProjectileSlot? breath = _projectiles.AllocateEnemyProjectile();
        if (breath is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            breath,
            RoomEnemyProjectileKind.MotherBrainPurpleBreathBig,
            graphicsIndex: 0);
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain purple breath requires the linked head record.");
        breath.Position = breath.Position.WithWhole(new(unchecked((ushort)(head.Position.X.Whole + 6)), unchecked((ushort)(head.Position.Y.Whole + 0x0010))));
    }

    /// <summary>Ports the small-breath spawn at <c>$A9:9368</c> and initializer <c>$86:CA83</c>.</summary>
    private void SpawnMotherBrainPurpleBreathSmall(MotherBrainEnemyState state)
    {
        RoomEnemyProjectileSlot? breath = _projectiles.AllocateEnemyProjectile();
        if (breath is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            breath,
            RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall,
            graphicsIndex: 0);
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain purple breath requires the linked head record.");
        breath.Position = breath.Position.WithWhole(new(unchecked((ushort)(head.Position.X.Whole + 6)), unchecked((ushort)(head.Position.Y.Whole + 0x0010))));
        state.SmallPurpleBreathActive = true;
    }

    /// <summary>Exact six-position mouth attachment table at <c>$86:C86E-C885</c>.</summary>
    internal void RunMotherBrainAttachedDroolPreInstruction(
        RoomEnemyProjectileSlot drool)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain drool ran without its multipart encounter state.");
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain drool requires the linked head record.");
        int parameter = drool.Variable0;
        if ((uint)parameter >= 6)
        {
            throw new InvalidDataException(
                $"Mother Brain drool offset parameter {parameter} is outside 0..5.");
        }

        drool.Position = drool.Position.WithWhole(head.Position.Whole + MotherBrainDroolOffsets[parameter]);
        drool.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
    }

    /// <summary>
    /// Ports <c>$86:C886-C8AF</c>. The native actor uses signed 8.8 velocity, adds twelve
    /// units of gravity per frame, and switches to its four-map splash at world Y $D7.
    /// </summary>
    internal static void RunMotherBrainFallingDroolPreInstruction(
        RoomEnemyProjectileSlot drool)
    {
        drool.Velocity = drool.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(drool.Velocity.Y.RawValue + 0x000c))));
        drool.Position = drool.Position.AdvanceY(drool.Velocity.Y.ToSixteenSixteenDelta());
        if (drool.Position.Y.Whole < 0x00d7)
            return;

        drool.Position = drool.Position.WithWholeY(unchecked((ushort)(drool.Position.Y.Whole - 4)));
        drool.InstructionPointer = EnemyProjectileInstructionLists.MotherBrainDroolFalling;
        drool.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>
    /// Ports <c>$86:C335-C431</c>. A live Baby slot is tested before Samus and the room,
    /// matching the private projectile's cross-enemy collision order during phase three.
    /// </summary>
    internal void RunMotherBrainOnionRingPreInstruction(
        RoomEnemyProjectileSlot ring,
        SamusState? samus,
        ushort cameraX)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ?? throw new InvalidOperationException(
            "Mother Brain onion ring ran without its multipart encounter state.");

        if (ring.Variable0 != 0)
        {
            ring.Variable0 = unchecked((ushort)(ring.Variable0 - 1));
            PinMotherBrainOnionRingToMouth(ring, state);
            return;
        }

        ring.Position = ring.Position.Advance(ring.Velocity.ToSixteenSixteen());

        BabyMetroidCutsceneState? baby = state.BabyMetroid;
        if (baby is { IsDeleted: false })
        {
            // `$C3B1-$C3C8` deletes every later ring as soon as the still-registered Baby
            // reaches zero health. Native clears only the projectile ID on this branch.
            if (baby.Health == 0)
            {
                ring.Kind = RoomEnemyProjectileKind.None;
                return;
            }

            if (MotherBrainOnionRingOverlapsBaby(ring, baby))
            {
                state.PendingBabyCryCount = unchecked((ushort)(state.PendingBabyCryCount + 1));
                _ = baby.ApplyMotherBrainOnionRingHit();
                // `$86:C399-$C3A5` writes the Baby slot's Enemy.health from this projectile
                // pass; it must not wait for the Baby's next AI turn to publish it.
                RoomEnemySlot babySlot = state.BabyMetroidSlot ?? throw new InvalidDataException(
                    "A Mother Brain onion ring hit the Baby without its physical slot.");
                babySlot.Health = baby.Health;
                ExplodeMotherBrainOnionRing(ring, state, ring.Position.Whole);
                return;
            }
        }

        // This private collision path intentionally ignores Samus's current invincibility
        // timer. It writes a fresh $60 only after applying suit-divided damage.
        if (samus is not null && MotherBrainOnionRingOverlapsSamus(ring, samus))
        {
            Position16<RoomPixels> ringPosition = ring.Position.Whole;
            ushort damage = samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
                ? (ushort)(0x0050 >> 2)
                : samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                    ? (ushort)(0x0050 >> 1)
                    : (ushort)0x0050;
            samus.Health = samus.Health <= damage
                ? (ushort)0
                : unchecked((ushort)(samus.Health - damage));
            samus.InvincibilityTimer = NativeCountdown.Start(0x0060);
            samus.KnockbackTimer = NativeCountdown.Start(5);
            samus.KnockbackXDirection = unchecked((short)(samus.Position.X.Whole - ringPosition.X)) >= 0
                ? (ushort)1
                : (ushort)0;
            ExplodeMotherBrainOnionRing(ring, state, ringPosition);
            return;
        }

        short signedY = unchecked((short)ring.Position.Y.Whole);
        short signedX = unchecked((short)ring.Position.X.Whole);
        short screenX = unchecked((short)(ring.Position.X.Whole - cameraX));
        if (signedY < 0x20 || ring.Position.Y.Whole >= 0x00d8 || signedX < 0 ||
            screenX < 0 || screenX >= 0x00f8)
        {
            _earthquake.EarthquakeTimer = NativeCountdown.Start(10);
            _earthquake.EarthquakeType = 5;
            ExplodeMotherBrainOnionRing(ring, state, ring.Position.Whole);
        }
    }

    /// <summary>Positions an unlaunched ring at the moving head's mouth.</summary>
    private static void PinMotherBrainOnionRingToMouth(
        RoomEnemyProjectileSlot ring,
        MotherBrainEnemyState state)
    {
        RoomEnemySlot head = state.Head ?? throw new InvalidDataException(
            "Mother Brain onion ring requires the linked head record.");
        ring.Position = ring.Position.WithWhole(new(unchecked((ushort)(head.Position.X.Whole + 0x000a)), unchecked((ushort)(head.Position.Y.Whole + 0x0010))));
    }

    /// <summary>Checks ring contact against Samus's current hitbox.</summary>
    private static bool MotherBrainOnionRingOverlapsSamus(
        RoomEnemyProjectileSlot ring,
        SamusState samus) =>
        EnemyMath.WrappedMagnitude(unchecked((ushort)(ring.Position.X.Whole - samus.Position.X.Whole))) <
            ring.Radius.X + samus.Kinematics.Radius.X &&
        EnemyMath.WrappedMagnitude(unchecked((ushort)(ring.Position.Y.Whole - samus.Position.Y.Whole))) <
            ring.Radius.Y + samus.Kinematics.Radius.Y;

    /// <summary>Checks ring contact against the Baby Metroid's current hitbox.</summary>
    private static bool MotherBrainOnionRingOverlapsBaby(
        RoomEnemyProjectileSlot ring,
        BabyMetroidCutsceneState baby) =>
        EnemyMath.WrappedMagnitude(unchecked((ushort)(ring.Position.X.Whole - baby.Position.X.Whole))) <
            ring.Radius.X + BabyMetroidCutsceneState.XHitboxRadius &&
        EnemyMath.WrappedMagnitude(unchecked((ushort)(ring.Position.Y.Whole - baby.Position.Y.Whole))) <
            ring.Radius.Y + BabyMetroidCutsceneState.YHitboxRadius;

    /// <summary>Ends a ring on contact and spawns its explosion effect.</summary>
    private void ExplodeMotherBrainOnionRing(
        RoomEnemyProjectileSlot ring,
        MotherBrainEnemyState state,
        Position16<RoomPixels> position)
    {
        // `$C410` clears the source before allocating parameter-three room dust. If the
        // allocator reuses this physical slot, the outer projectile scheduler will advance
        // the newborn dust list during the same pass, matching EprojRunOne's original X.
        ring.Clear();
        _projectiles.SpawnRoomGraphicsDustExplosion(position, animationIndex: 3);
        state.LastSoundEffectLibrary3 = 0x0013;
    }
    /// <summary>
    /// Ports the twelve calls to <c>SpawnEprojWithRoomGfx($C17E, A=0..$B)</c> made by
    /// <c>InitAI_MotherBrainBody</c>. Allocation scans from native index $22 down, so request
    /// zero lands in host slot 17 and request $B lands in slot 6.
    /// </summary>
    private void SpawnMotherBrainInitialTurrets()
    {
        for (ushort parameter = 0; parameter < 12; parameter++)
            SpawnMotherBrainTurret(parameter);
    }

    /// <summary>Ports <c>InitAI_EnemyProjectile_MotherBrainsTurrets</c> at $86:BE4F.</summary>
    internal void SpawnMotherBrainTurret(ushort parameter)
    {
        if (parameter >= 12)
            throw new ArgumentOutOfRangeException(nameof(parameter));

        RoomEnemyProjectileSlot? turret = _projectiles.AllocateEnemyProjectile();
        if (turret is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            turret,
            RoomEnemyProjectileKind.MotherBrainRoomTurret,
            MotherBrainTurretDefinitions.GraphicsIndex);

        MotherBrainTurretDefinition definition =
            MotherBrainTurretDefinitions.ForTurret(parameter);
        MotherBrainTurretDirection direction = definition.InitialDirection;
        MotherBrainTurretDirectionDefinition directionDefinition =
            MotherBrainTurretDefinitions.ForDirection(direction);
        turret.DirectionParameter = parameter;
        turret.Position = turret.Position.WithWhole(definition.Position);

        // The turret is stationary, so the initializer intentionally repurposes both
        // subposition words. X-subposition is a bank-$86 pointer to this turret's allowed
        // direction bytes. Y-subposition packs direction in the low byte and signed
        // rotation delta (+1 initially) in the high byte.
        turret.Position = turret.Position.WithFractionX(definition.AllowedRotationPointer).WithFractionY(unchecked((ushort)(0x0100 | (byte)direction)));
        turret.InstructionPointer = directionDefinition.InstructionPointer;
        turret.InstructionTimer = NativeCountdown.Start(1);

        // X/Y velocity are likewise timers for a stationary turret. Each initializer makes
        // two independent calls to the cartridge RNG and clamps their low bytes upward.
        ResetMotherBrainTurretRotationTimer(turret);
        ResetMotherBrainTurretCooldown(turret);
    }

    /// <summary>Ports <c>PreInstruction_EnemyProjectile_MotherBrainsTurrets</c>.</summary>
    internal void RunMotherBrainTurretPreInstruction(
        RoomEnemyProjectileSlot turret,
        Position16<RoomPixels> camera)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ??
            throw new InvalidDataException("A Mother Brain turret has no owning encounter state.");
        bool onScreen = MotherBrainTurretIsOnScreen(turret, camera);

        // Off-screen turrets deliberately freeze both timers until scrolled into view. Once
        // the shared deletion flag is set they disappear silently off-screen, but visible
        // turrets replace themselves with dust animation parameter $C.
        if (!onScreen)
        {
            if (state.DeleteTurretsAndRinkas)
                turret.Clear();
            return;
        }
        if (state.DeleteTurretsAndRinkas)
        {
            ushort x = turret.Position.X.Whole;
            ushort y = turret.Position.Y.Whole;
            turret.Clear();
            _projectiles.SpawnRoomGraphicsDustExplosion(new(x, y), animationIndex: 0x000c);
            return;
        }

        turret.Velocity = turret.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)(turret.Velocity.X.RawValue - 1))));
        if (turret.Velocity.X.RawValue == 0)
        {
            ResetMotherBrainTurretRotationTimer(turret);
            SelectNextMotherBrainTurretDirection(turret);
            MotherBrainTurretDirection direction =
                (MotherBrainTurretDirection)unchecked((byte)turret.Position.Y.Fraction);
            turret.InstructionPointer = MotherBrainTurretDefinitions
                .ForDirection(direction).InstructionPointer;
            turret.InstructionTimer = NativeCountdown.Start(1);
        }

        turret.Velocity = turret.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(turret.Velocity.Y.RawValue - 1))));
        if (turret.Velocity.Y.RawValue == 0)
        {
            ResetMotherBrainTurretCooldown(turret);
            SpawnMotherBrainTurretBullet(turret);
        }
    }

    /// <summary>Ports the signed direction-delta and allowed-rotation test at $86:C050.</summary>
    internal static void SelectNextMotherBrainTurretDirection(RoomEnemyProjectileSlot turret)
    {
        byte currentDirection = unchecked((byte)turret.Position.Y.Fraction);
        sbyte rotationDelta = unchecked((sbyte)SnesLittleEndian.HighByte(turret.Position.Y.Fraction));
        byte candidate = unchecked((byte)((currentDirection + rotationDelta) & 7));
        bool allowed = MotherBrainTurretDefinitions.IsRotationAllowed(
            turret.Position.X.Fraction,
            (MotherBrainTurretDirection)candidate);

        if (allowed)
        {
            turret.Position = turret.Position.WithFractionY(SnesLittleEndian.Word(candidate, SnesLittleEndian.HighByte(turret.Position.Y.Fraction)));
            return;
        }

        // A disallowed next step reverses the signed high-byte delta and immediately takes
        // one step in the new direction. The cartridge does not perform a second mask test.
        rotationDelta = unchecked((sbyte)-rotationDelta);
        byte reversedDirection = unchecked((byte)(currentDirection + rotationDelta));
        turret.Position = turret.Position.WithFractionY(unchecked((ushort)(
            SnesLittleEndian.Word(reversedDirection, unchecked((byte)rotationDelta)))));
    }

    /// <summary>Restarts the turret's wait before it chooses another firing direction.</summary>
    private void ResetMotherBrainTurretRotationTimer(RoomEnemyProjectileSlot turret)
    {
        ushort sample = unchecked((byte)_room.Random.Generate());
        turret.Velocity = turret.Velocity.WithX(new SnesSignedEightEight(Math.Max(sample, MotherBrainTurretDefinitions.MinimumRotationDelay)));
    }

    /// <summary>Restarts the turret's pause between fired bullets.</summary>
    private void ResetMotherBrainTurretCooldown(RoomEnemyProjectileSlot turret)
    {
        ushort sample = unchecked((byte)_room.Random.Generate());
        turret.Velocity = turret.Velocity.WithY(new SnesSignedEightEight(Math.Max(sample, MotherBrainTurretDefinitions.MinimumFiringCooldown)));
    }

    /// <summary>Ports the asymmetric viewport bounds at $86:C0B4.</summary>
    private static bool MotherBrainTurretIsOnScreen(
        RoomEnemyProjectileSlot turret,
        Position16<RoomPixels> camera)
    {
        if (unchecked((short)turret.Position.Y.Whole) < 0 ||
            unchecked((short)(turret.Position.Y.Whole + 0x0010 - camera.Y)) < 0 ||
            unchecked((ushort)(turret.Position.Y.Whole + 0x0010 - camera.Y)) >= 0x0100)
        {
            return false;
        }

        return unchecked((short)turret.Position.X.Whole) >= 0 &&
            unchecked((short)(turret.Position.X.Whole + 4 - camera.X)) >= 0 &&
            unchecked((ushort)(turret.Position.X.Whole + 4 - camera.X)) < 0x0108;
    }

    /// <summary>Ports turret-bullet initialization at $86:BF59.</summary>
    internal void SpawnMotherBrainTurretBullet(RoomEnemyProjectileSlot turret)
    {
        RoomEnemyProjectileSlot? bullet = _projectiles.AllocateEnemyProjectile();
        if (bullet is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            bullet,
            RoomEnemyProjectileKind.MotherBrainRoomTurretBullet,
            MotherBrainTurretDefinitions.GraphicsIndex);
        MotherBrainTurretDirection direction =
            (MotherBrainTurretDirection)unchecked((byte)turret.Position.Y.Fraction);
        MotherBrainTurretDirectionDefinition definition =
            MotherBrainTurretDefinitions.ForDirection(direction);
        bullet.DirectionParameter = (byte)direction;
        bullet.Variable0 = unchecked((ushort)((byte)direction * 2));
        bullet.Variable1 = 0;
        bullet.Velocity = definition.BulletVelocity;
        bullet.Position = bullet.Position.WithWhole(turret.Position.Whole + definition.BulletOffset);
    }

    /// <summary>Ports the bullet property flicker, 8.8 movement, and point collision.</summary>
    internal static void RunMotherBrainTurretBulletPreInstruction(
        RoomEnemyProjectileSlot bullet,
        RoomLevelData level)
    {
        // Property $8000 is toggled before the projectile-vs-projectile pass every frame,
        // reproducing the bullet's alternating vulnerability rather than a visual alpha hack.
        bullet.BlocksSamusProjectiles = !bullet.BlocksSamusProjectiles;
        bullet.Position = bullet.Position.Advance(bullet.Velocity.ToSixteenSixteen());

        if (EnemyProjectileEngine.NonAirBlockAt(level, bullet.Position.Whole))
            bullet.Clear();
    }
    /// <summary>
    /// Executes `$A9:B605-$B610`'s zero-health tail call into `$B8EB`. Setup happens on the
    /// same body-AI call, but the freshly installed `$B91A` timer is not decremented until
    /// Mother Brain's next enemy turn.
    /// </summary>
    internal void StartLiveMotherBrainRainbowBeam(
        MotherBrainEnemyState state,
        SamusState samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        var sequence = new MotherBrainRainbowBeamAttackSequence(_room.MotherBrainCorpseArtwork);
        SynchronizeLiveMotherBrainRainbowActor(state, sequence);
        // Word $0FF2 keeps the fake-death ascent dust's last value; the death explosions
        // start from it. Word $0FF0 then holds a bank-$A9 sub-function pointer, always
        // negative, so its first DEC takes the same immediate branch as the sequence's zero.
        sequence.InheritDeathAndEscapeExplosionIndex(state.BodySubFunctionTimer.RawValue);
        sequence.StartAttackCycle();
        state.RainbowBeamSequence = sequence;
        state.RainbowPaletteCursor = 0;
        state.RainbowAppliedHeadInstructionList = 0;
        state.RainbowAppliedSmallPurpleBreathWrites = 0;
        ApplyLiveMotherBrainRainbowState(
            state,
            sequence,
            step: null,
            samus,
            sharedProjectiles);
    }

    /// <summary>Runs one native body-function call through the shared verified sequence.</summary>
    private void RunLiveMotherBrainRainbowBeam(
        MotherBrainEnemyState state,
        SamusState samus,
        byte enemyFrameCounter,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        MotherBrainRainbowBeamAttackSequence sequence = state.RainbowBeamSequence ??
            throw new InvalidOperationException(
                "Mother Brain entered a rainbow function without its live sequence owner.");

        // Body bytecode and articulated-head AI ran through the physical room records on
        // previous frames. Import those words before dispatch so walk completion and neck
        // aiming are decided from the same state the 65816 would read from WRAM.
        SynchronizeLiveMotherBrainRainbowActor(state, sequence);

        bool typewriterFinished = false;
        if (sequence.Phase is MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText or
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSpawnTimeBombSetSubtitle)
        {
            var text = state.EscapeTypewriter ?? throw new InvalidOperationException("Mother Brain escape text was not initialized.");
            typewriterFinished = text.Step(_presentation._vram!);
            if (text.ClickRequested) state.LastSoundEffectLibrary3 = EscapeTypewriterRomData.ZebesClick;
        }
        MotherBrainRainbowBeamAttackStepResult step = sequence.Step(_room.Bus, samus, new RainbowBeamAttackFrame { EnemyFrameCounter = enemyFrameCounter, MainEnemyExecutionCounter = _frameContext._randomEnemyCounter, PowerBombActive = sharedProjectiles?.PowerBombExplosion.Status != 0, RandomNumberSeed = _room.Random.Read(), NextRandomNumber = _room.Random.Generate, AlternateEscapeText = _roomStatus.JapaneseText, TypewriterFinished = typewriterFinished, GlobalEarthquakeTimer = _earthquake.EarthquakeTimer.RawValue });
        state.LastRainbowBeamStep = step;
        ApplyLiveMotherBrainRainbowState(state, sequence, step, samus, sharedProjectiles);
    }

    /// <summary>Copies rainbow-beam phase state into the live actor slot.</summary>
    private static void SynchronizeLiveMotherBrainRainbowActor(
        MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackSequence sequence)
    {
        RoomEnemySlot head = state.Head ?? throw new InvalidOperationException(
            "Mother Brain's rainbow beam requires the physical head slot.");
        sequence.SynchronizeLiveActor(new MotherBrainLiveActorSnapshot { Body = state.Body.Position.Whole, BodyPose = state.Pose, Form = state.Form, BodyProperties = state.Body.Properties, BodyExtraProperties = state.Body.ExtraProperties, Brain = head.Position.Whole, BrainHealth = head.Health, BrainProperties = head.Properties, BrainExtraProperties = head.ExtraProperties, LowerNeckAngle = state.LowerNeckAngle, UpperNeckAngle = state.UpperNeckAngle, NeckMovementEnabled = state.NeckMovementEnabled, LowerNeckMovementIndex = state.LowerNeckMovementIndex, UpperNeckMovementIndex = state.UpperNeckMovementIndex, BombCounter = state.BombCounter, HitboxesEnabled = state.HitboxesEnabled != 0 });
    }

    /// <summary>
    /// Publishes only values produced by the current body-AI call. In particular, cooldown
    /// and earthquake timers are event writes rather than continuously mirrored values;
    /// continuously copying the sequence's retained word would undo the global owners'
    /// normal per-frame decrements.
    /// </summary>
    private void ApplyLiveMotherBrainRainbowState(
        MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackSequence sequence,
        MotherBrainRainbowBeamAttackStepResult? step,
        SamusState samus,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        RoomEnemySlot head = state.Head!;

        // `$A9:C447` resets the physical list timer only when body AI requests a new list.
        // The dedicated latch is essential because CurrentInstruction advances away from
        // the list origin while the request itself remains unchanged.
        if (sequence.HeadInstructionList != 0 &&
            (step is { HeadInstructionListRequested: true } ||
             sequence.HeadInstructionList != state.RainbowAppliedHeadInstructionList))
        {
            SetMotherBrainBrainInstructionList(state, sequence.HeadInstructionList);
            state.RainbowAppliedHeadInstructionList = sequence.HeadInstructionList;
        }

        if (step is { } result &&
            (result.BodyWalkRequested || result.BodyPostureRequested))
        {
            ushort bodyInstruction = sequence.RequestedBodyInstructionList;
            if (bodyInstruction == 0)
            {
                throw new InvalidDataException(
                    "Rainbow body AI requested animation without an instruction pointer.");
            }
            SetMotherBrainInstructionList(state.Body, bodyInstruction);
        }

        state.Function = MapLiveMotherBrainRainbowFunction(sequence.Phase);
        state.FunctionTimer = sequence.FunctionTimer;
        // `$7E:8026` remains one shared native word across phase two and phase three. The
        // reusable sequence owns phase-three walking/recoil decisions, while the public
        // encounter state is the debugger-facing WRAM projection used by shot callbacks.
        // Republish it after every body call so either view observes the exact same counter.
        state.WalkCounter = sequence.Phase3WalkCounter;
        state.NeckAngleDelta = sequence.NeckAngleDelta;
        state.NeckMovementEnabled = sequence.NeckMovementEnabled != 0;
        state.LowerNeckMovementIndex = sequence.LowerNeckMovementIndex;
        state.UpperNeckMovementIndex = sequence.UpperNeckMovementIndex;
        if (sequence.Phase3NeckPhase == MotherBrainPhase3NeckPhase.HyperBeamRecoil &&
            sequence.Phase3NeckFunctionTimer.RawValue == 0x000a)
        {
            // `$C395` loads `$32` during the one-shot setup call, immediately before the
            // fallthrough decrements neck timer `$0B` to `$0A`. The physical draw hook owns
            // subsequent shake decrements, so publish this event only on that exact setup
            // result rather than copying the sequence's retained word every enemy frame.
            state.BrainMainShakeTimer = NativeCountdown.Start(0x0032);
        }
        state.Form = sequence.Body.Form;
        state.BodySubFunctionTimer = sequence.DeathAndEscapeExplosionIndex;
        state.Body.Properties = sequence.BodyProperties;
        state.Body.ExtraProperties = sequence.BodyProperties2;
        head.Health = sequence.BrainHealth;
        head.Properties = sequence.BrainProperties;
        head.ExtraProperties = sequence.BrainProperties2;
        // The live word begins at two, while the reusable sequence needs only its Boolean
        // branch meaning. Preserve every nonzero cartridge value; only a translated clear
        // is allowed to collapse it to zero.
        if (!sequence.HitboxesEnabled)
            state.HitboxesEnabled = 0;
        if (sequence.SmallPurpleBreathGenerationWriteCount != state.RainbowAppliedSmallPurpleBreathWrites)
        {
            state.SmallPurpleBreathGenerationEnabled = sequence.SmallPurpleBreathGenerationEnabled;
            state.RainbowAppliedSmallPurpleBreathWrites = sequence.SmallPurpleBreathGenerationWriteCount;
        }
        state.DroolGenerationEnabled = sequence.DroolGenerationEnabled;

        // These words are the renderer-facing output of the bank-$88 HDMA object. The beam
        // renderer is deliberately fed from this single translated aim calculation.
        state.RainbowBeamHdmaActive = sequence.HdmaActive;
        state.RainbowBeamAngle = sequence.RainbowBeamAngle;
        state.RainbowBeamAngularWidth = sequence.AngularWidth;

        if (step is not { } current)
            return;

        state.RainbowBeamPaletteRequested = current.PaletteRequested;
        if (current.PhaseBefore == MotherBrainRainbowBeamAttackPhase.FireFinalRainbowBeam &&
            current.PhaseAfter == MotherBrainRainbowBeamAttackPhase.FinalRainbowBeamHolding)
            state.RainbowPaletteCursor = 0;
        ApplyMotherBrainRainbowPalette(state, current);
        // $A9:D1E4 runs after body AI. Its health handler is enabled during revival,
        // but $AD:E3D5 waits until the corpse-state word reaches final-combat state 2.
        if (sequence.HealthBasedPaletteHandlingEnabled && sequence.Phase2CorpseState >= 2)
            MotherBrainHealthPalette.Apply(_presentation._cgram!, state.Head!.Health, _presentation.MotherBrainHealthColors);
        ApplyLiveMotherBrainDeath(state, sequence, current);
        state.LastRainbowBeamExplosion = current.Explosion;
        if (current.SoundQueued)
            // A request is not invariably a start: the painful-walk stage-six handoff
            // stops the same looping voice. Preserve the command selected by the sequence.
            state.LastSoundEffectLibrary1 = sequence.RainbowBeamSoundPlaying
                ? MotherBrainSoundCommands.StartRainbowDrain
                : MotherBrainSoundCommands.StopRainbowDrain;
        if (current.ChargeSoundQueued || current.FinalBeamSoundQueued)
            state.LastSoundEffect = 0x0071;
        if (current.Explosion is not null)
            state.LastSoundEffect = 0x0024;

        // `$BA0F` seeds the short quake, `$BA27` replaces it with the full drain duration,
        // and `$BA93` clears it when the beam contracts below its minimum width.
        if (current.PhaseBefore == MotherBrainRainbowBeamAttackPhase.OneFrameDelay &&
            current.PhaseAfter == MotherBrainRainbowBeamAttackPhase.StartDrainingSamus)
        {
            _earthquake.EarthquakeType = 8;
            _earthquake.EarthquakeTimer = NativeCountdown.Start(8);
        }
        else if (current.PhaseBefore == MotherBrainRainbowBeamAttackPhase.StartDrainingSamus)
        {
            _earthquake.EarthquakeType = 8;
            _earthquake.EarthquakeTimer = NativeCountdown.Start(0x012b);
        }
        else if (current.PhaseBefore == MotherBrainRainbowBeamAttackPhase.FinishFiring &&
            current.PhaseAfter == MotherBrainRainbowBeamAttackPhase.LetSamusFall)
        {
            _earthquake.EarthquakeTimer = NativeCountdown.Start(0);
            state.LastSoundEffectLibrary1 = 0x0002;
        }

        if (sharedProjectiles is not null)
        {
            if (current.PhaseBefore == MotherBrainRainbowBeamAttackPhase.WaitForCharge &&
                current.PhaseAfter == MotherBrainRainbowBeamAttackPhase.StartFiring)
            {
                sharedProjectiles.SetSharedCooldown(8);
            }
            else if (current.PhaseBefore == MotherBrainRainbowBeamAttackPhase.StartFiring &&
                current.PhaseAfter == MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall)
            {
                sharedProjectiles.SetSharedCooldown(0);
            }
            else if (current.PhaseBefore == MotherBrainRainbowBeamAttackPhase.FinishFiring &&
                current.PhaseAfter == MotherBrainRainbowBeamAttackPhase.LetSamusFall)
            {
                sharedProjectiles.SetSharedCooldown(8);
            }
        }

        if (current.SpriteTileTransfer is { } transfer)
            _motherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(transfer);
        if (current.BabySpawnRequested)
            _babyMetroidCutsceneBehavior.SpawnMotherBrainBabyMetroid(state);
        if (current.Explosion is { } explosion)
        {
            // `$BC76` runs before `$B9F1/$BA0C/$BA4F` moves Samus. The reusable sequence
            // captures that pre-move point in its typed movement result, so use it for the
            // initializer and let `$C94C` attach to the current position on the later bank-
            // $86 pass. Finish-firing has no movement and therefore uses the unchanged actor.
            Position16<RoomPixels> samusBeforeMovement = current.Movement?.Before.Whole ?? samus.Position.Whole;
            SpawnMotherBrainRainbowExplosion(samusBeforeMovement, explosion);
        }
    }

    /// <summary>Ports `$86:C80A-$C828`, including the initializer's same-call pin.</summary>
    internal void SpawnMotherBrainRainbowChargingProjectile()
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging,
            graphicsIndex: 0);
        projectile.Velocity = new(new SnesSignedEightEight(0), new SnesSignedEightEight(0));
        projectile.Position = projectile.Position.WithFractionX(0).WithFractionY(0);
        projectile.GraphicsIndex = 0;
        RunMotherBrainRainbowChargingPreInstruction(projectile);
    }

    /// <summary>Pins the charging effect to the head while the beam winds up.</summary>
    internal void RunMotherBrainRainbowChargingPreInstruction(
        RoomEnemyProjectileSlot projectile)
    {
        RoomEnemySlot head = _motherBrainEncounter.MotherBrain?.Head ?? throw new InvalidOperationException(
            "Mother Brain rainbow charge ran without the physical head slot.");
        projectile.Position = projectile.Position.WithWhole(head.Position.Whole);
    }

    /// <summary>Ports `$86:C92F-$C960`; the signed offsets remain in velocity words.</summary>
    internal void SpawnMotherBrainRainbowExplosion(
        Position16<RoomPixels> samusBeforeMovement,
        MotherBrainRainbowExplosionRequest request)
    {
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion,
            graphicsIndex: 0);
        projectile.Velocity = new EightEightVector<RoomPixels>(request.Offset.X, request.Offset.Y);
        projectile.GraphicsIndex = 0;
        projectile.Position = projectile.Position.WithWhole(samusBeforeMovement + request.Offset);
    }

    /// <summary>Advances the rainbow impact effect through its timed lifetime.</summary>
    internal static void RunMotherBrainRainbowExplosionPreInstruction(
        RoomEnemyProjectileSlot projectile,
        SamusState? samus)
    {
        SamusState target = samus ?? throw new InvalidOperationException(
            "Mother Brain rainbow explosion requires the live Samus actor.");
        projectile.Position = projectile.Position.WithWhole(new(unchecked((ushort)(target.Position.X.Whole + projectile.Velocity.X.RawValue)), unchecked((ushort)(target.Position.Y.Whole + projectile.Velocity.Y.RawValue))));
    }

    /// <summary>Maps rainbow sequence phases to the matching body AI phase.</summary>
    internal static MotherBrainBodyFunction MapLiveMotherBrainRainbowFunction(
        MotherBrainRainbowBeamAttackPhase phase) => phase switch
        {
            MotherBrainRainbowBeamAttackPhase.RepeatAttack =>
                MotherBrainBodyFunction.SecondPhaseRainbowExtendNeck,
            MotherBrainRainbowBeamAttackPhase.StartCharging =>
                MotherBrainBodyFunction.SecondPhaseRainbowStartCharging,
            MotherBrainRainbowBeamAttackPhase.RetractNeck =>
                MotherBrainBodyFunction.SecondPhaseRainbowRetractNeck,
            MotherBrainRainbowBeamAttackPhase.WaitForCharge =>
                MotherBrainBodyFunction.SecondPhaseRainbowWaitForCharge,
            MotherBrainRainbowBeamAttackPhase.ExtendNeckDown =>
                MotherBrainBodyFunction.SecondPhaseRainbowExtendNeckDown,
            MotherBrainRainbowBeamAttackPhase.StartFiring =>
                MotherBrainBodyFunction.SecondPhaseRainbowStartFiring,
            MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall =>
                MotherBrainBodyFunction.SecondPhaseRainbowMoveSamusTowardWall,
            MotherBrainRainbowBeamAttackPhase.OneFrameDelay =>
                MotherBrainBodyFunction.SecondPhaseRainbowOneFrameDelay,
            MotherBrainRainbowBeamAttackPhase.StartDrainingSamus =>
                MotherBrainBodyFunction.SecondPhaseRainbowStartDrainingSamus,
            MotherBrainRainbowBeamAttackPhase.DrainingSamus =>
                MotherBrainBodyFunction.SecondPhaseRainbowDrainingSamus,
            MotherBrainRainbowBeamAttackPhase.FinishFiring =>
                MotherBrainBodyFunction.SecondPhaseRainbowFinishFiring,
            MotherBrainRainbowBeamAttackPhase.LetSamusFall =>
                MotherBrainBodyFunction.SecondPhaseRainbowLetSamusFall,
            MotherBrainRainbowBeamAttackPhase.WaitForSamusToLand =>
                MotherBrainBodyFunction.SecondPhaseRainbowWaitForSamusToLand,
            MotherBrainRainbowBeamAttackPhase.LowerHead =>
                MotherBrainBodyFunction.SecondPhaseRainbowLowerHead,
            MotherBrainRainbowBeamAttackPhase.DecideNextAction =>
                MotherBrainBodyFunction.SecondPhaseRainbowDecideNextAction,
            MotherBrainRainbowBeamAttackPhase.FinishSamusOff =>
                MotherBrainBodyFunction.SecondPhaseFinishSamusOff,
            MotherBrainRainbowBeamAttackPhase.FinishStandUp =>
                MotherBrainBodyFunction.SecondPhaseFinishSamusOffStandUp,
            MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone =>
                MotherBrainBodyFunction.SecondPhaseFinishSamusOffAdmire,
            MotherBrainRainbowBeamAttackPhase.ChargeFinalRainbowBeam =>
                MotherBrainBodyFunction.SecondPhaseFinishSamusOffChargeFinalBeam,
            MotherBrainRainbowBeamAttackPhase.LoadBabyMetroidTiles =>
                MotherBrainBodyFunction.SecondPhaseFinishSamusOffLoadBabyTiles,
            MotherBrainRainbowBeamAttackPhase.FireFinalRainbowBeam =>
                MotherBrainBodyFunction.SecondPhaseFinishSamusOffFireFinalBeam,
            MotherBrainRainbowBeamAttackPhase.FinalRainbowBeamHolding =>
                MotherBrainBodyFunction.SecondPhaseFinalRainbowBeamHolding,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTakenAback =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyTakenAback,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyRegainBalance,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyFiringRainbowBeam,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRainbowBeamRunOut =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyRainbowBeamRunOut,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidMoveToBackOfRoom =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyMoveToBackOfRoom,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyGoIntoLowPowerMode,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidPrepareTransitionToGrey =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyPrepareTransitionToGrey,
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey =>
                MotherBrainBodyFunction.SecondPhaseDrainedByBabyTransitionToGrey,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfInanimateGrey =>
                MotherBrainBodyFunction.SecondPhaseReviveInanimateGrey,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfShowSignsOfLife =>
                MotherBrainBodyFunction.SecondPhaseReviveShowSignsOfLife,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfTransitionFromGrey =>
                MotherBrainBodyFunction.SecondPhaseReviveTransitionFromGrey,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUp =>
                MotherBrainBodyFunction.SecondPhaseReviveWakeUp,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWakeUpStretch =>
                MotherBrainBodyFunction.SecondPhaseReviveWakeUpStretch,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfWalkUpToBabyMetroid =>
                MotherBrainBodyFunction.SecondPhaseReviveWalkUpToBaby,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfPrepareNeckForBabyMetroidDeath =>
                MotherBrainBodyFunction.SecondPhaseRevivePrepareNeckForBabyDeath,
            MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfFinishPreparingForBabyMetroidDeath =>
                MotherBrainBodyFunction.SecondPhaseReviveFinishPreparingForBabyDeath,
            MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttack =>
                MotherBrainBodyFunction.SecondPhaseMurderBabyAttack,
            MotherBrainRainbowBeamAttackPhase.Phase2MurderBabyMetroidAttackCooldown =>
                MotherBrainBodyFunction.SecondPhaseMurderBabyAttackCooldown,
            MotherBrainRainbowBeamAttackPhase.PrepareForFinalBabyMetroidAttack =>
                MotherBrainBodyFunction.SecondPhasePrepareForFinalBabyAttack,
            MotherBrainRainbowBeamAttackPhase.ExecuteFinalBabyMetroidAttack =>
                MotherBrainBodyFunction.SecondPhaseExecuteFinalBabyAttack,
            MotherBrainRainbowBeamAttackPhase.FinalBabyMetroidAttackHolding =>
                MotherBrainBodyFunction.SecondPhaseFinalBabyAttackHolding,
            MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneMakeSomeDistance =>
                MotherBrainBodyFunction.ThirdPhaseRecoverMakeSomeDistance,
            MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneSetupForFighting =>
                MotherBrainBodyFunction.ThirdPhaseRecoverSetupForFighting,
            MotherBrainRainbowBeamAttackPhase.Phase3FightingMain =>
                MotherBrainBodyFunction.ThirdPhaseFightingMain,
            MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown =>
                MotherBrainBodyFunction.ThirdPhaseFightingAttackCooldown,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceMoveToBackOfRoom =>
                MotherBrainBodyFunction.ThirdPhaseDeathMoveToBackOfRoom,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceIdleWhilstExploding =>
                MotherBrainBodyFunction.ThirdPhaseDeathIdleWhilstExploding,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStumbleToMiddleOfRoom =>
                MotherBrainBodyFunction.ThirdPhaseDeathStumbleToMiddleOfRoom,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDisableBrainEffects =>
                MotherBrainBodyFunction.ThirdPhaseDeathDisableBrainEffects,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupBodyFadeOut =>
                MotherBrainBodyFunction.ThirdPhaseDeathSetupBodyFadeOut,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody =>
                MotherBrainBodyFunction.ThirdPhaseDeathFadeOutBody,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFinalFewExplosions =>
                MotherBrainBodyFunction.ThirdPhaseDeathFinalFewExplosions,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceRealizeDecapitation =>
                MotherBrainBodyFunction.ThirdPhaseDeathRealizeDecapitation,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround =>
                MotherBrainBodyFunction.ThirdPhaseDeathBrainFallsToGround,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadCorpseTiles =>
                MotherBrainBodyFunction.ThirdPhaseDeathLoadCorpseTiles,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupFadeToGrey =>
                MotherBrainBodyFunction.ThirdPhaseDeathSetupFadeToGrey,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeToGrey =>
                MotherBrainBodyFunction.ThirdPhaseDeathFadeToGrey,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseTipsOver =>
                MotherBrainBodyFunction.ThirdPhaseDeathCorpseTipsOver,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseRotsAway =>
                MotherBrainBodyFunction.ThirdPhaseDeathCorpseRotsAway,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequence20FrameDelay =>
                MotherBrainBodyFunction.ThirdPhaseDeath20FrameDelay,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadEscapeTimerTiles =>
                MotherBrainBodyFunction.ThirdPhaseDeathLoadEscapeTimerTiles,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStartEscape =>
                MotherBrainBodyFunction.ThirdPhaseDeathStartEscape,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSpawnTimeBombSetSubtitle =>
                MotherBrainBodyFunction.ThirdPhaseDeathSpawnTimeBombSetSubtitle,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText =>
                MotherBrainBodyFunction.ThirdPhaseDeathTypeOutZebesEscapeText,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDoorExplodingStartTimer =>
                MotherBrainBodyFunction.ThirdPhaseDeathDoorExplodingStartTimer,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBlowUpEscapeDoor =>
                MotherBrainBodyFunction.ThirdPhaseDeathBlowUpEscapeDoor,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceKeepEarthquakeGoing =>
                MotherBrainBodyFunction.ThirdPhaseDeathKeepEarthquakeGoing,
            _ => throw new InvalidDataException(
                $"Live Mother Brain rainbow phase {phase} is not attached to a room function yet."),
        };
    /// <summary>Runs $A9:BCFD at the body-frame cadence, then $BD1D's three literal copies.</summary>
    private void ApplyMotherBrainRainbowPalette(MotherBrainEnemyState state,
        MotherBrainRainbowBeamAttackStepResult step)
    {
        bool draining = step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey;
        bool reviving = step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfTransitionFromGrey;
        if ((draining || reviving) && step.PaletteRequested)
        {
            // The sequence increments before publishing this request. Keep the distinct
            // native copy lengths: revival intentionally preserves two brain/body colors.
            int index = state.RainbowBeamSequence!.GreyTransitionCounter - 1;
            var installed = _presentation.MotherBrainRainbowColors ?? throw new InvalidDataException(
                "Mother Brain's grey transition requires installed palette artwork.");
            if (draining) installed.ApplyToGrey(_room.Bus!, _presentation._cgram!, index);
            else installed.ApplyFromGrey(_room.Bus!, _presentation._cgram!, index);
            return;
        }
        if (step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.FinishFiring &&
            step.PhaseAfter == MotherBrainRainbowBeamAttackPhase.LetSamusFall)
        {
            var installed = _presentation.MotherBrainRainbowColors ?? throw new InvalidDataException(
                "Mother Brain's normal palette requires installed artwork.");
            installed.ApplyNormal(_presentation._cgram!);
            return;
        }
        if (step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam &&
            step.PhaseAfter == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRainbowBeamRunOut)
        {
            var installed = _presentation.MotherBrainRainbowColors ?? throw new InvalidDataException(
                "Mother Brain's drained palette requires installed artwork.");
            installed.ApplyRainbow(_presentation._cgram!, MotherBrainRainbowPaletteRomData.DrainedPointerOffset / sizeof(ushort));
            return;
        }
        bool rainbowPhase = step.PhaseBefore is
            MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall or
            MotherBrainRainbowBeamAttackPhase.OneFrameDelay or
            MotherBrainRainbowBeamAttackPhase.StartDrainingSamus or
            MotherBrainRainbowBeamAttackPhase.DrainingSamus or
            MotherBrainRainbowBeamAttackPhase.FinishFiring or
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance or
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam;
        if (!rainbowPhase || !step.PaletteRequested || (state.Body.FrameCounter & 2) == 0)
            return;

        var rainbow = _presentation.MotherBrainRainbowColors ?? throw new InvalidDataException(
            "Mother Brain's rainbow phase requires installed palette artwork.");
        if ((state.RainbowPaletteCursor & 1) != 0 ||
            state.RainbowPaletteCursor > MotherBrainRainbowPaletteFormat.RainbowFrameCount * sizeof(ushort))
            throw new InvalidDataException($"Mother Brain rainbow cursor ${state.RainbowPaletteCursor:X4} is outside the compiled loop.");
        int frame = state.RainbowPaletteCursor / sizeof(ushort);
        if (frame == MotherBrainRainbowPaletteFormat.RainbowFrameCount)
        {
            state.RainbowPaletteCursor = 0;
            frame = 0;
        }
        state.RainbowPaletteCursor += sizeof(ushort);
        rainbow.ApplyRainbow(_presentation._cgram!, frame);
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetMotherBrainRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        _motherBrainEncounter.MotherBrain?.BeginFrame();
    }


    /// <summary>Registers the head-touch reaction for the active body/head pair.</summary>
    internal void RegisterMotherBrainHeadTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainHead &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.MotherBrainHeadTouch,
            context =>
            {
                // `$A9:B5C6` is not ordinary touch damage. A spin-jumping Samus toggles
                // the head's visible flash length between the cartridge's 13/14-frame
                // cadence; every other movement type returns without changing either actor.
                RoomEnemySlot slot = context.Slot;
                if (context.Samus.PoseMetadata.ReadMovementType(_room.Bus!) == SamusMovementType.SpinJumping)
                    slot.FlashTimer = NativeCountdown.FromRaw(!slot.FlashTimer.IsZero && (slot.FlashTimer.RawValue & 1) != 0
                        ? (ushort)14
                        : (ushort)13);
            });
    }


    // `$A9:B503` is `CreateDudShot`. The multibox bomb walker has already marked the physical explosion, and the
    // body callback performs no health, phase, or multipart-state work of its own.
    /// <summary>Registers body bomb reactions in the native AI dispatch table.</summary>
    internal static void RegisterMotherBrainBodyBombReactions(EnemyAiTable table)
    {
        table.AddBombSpecial(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainBody &&
                                  c.SelectedShotAi == EnemyInteractionCallback.MotherBrainBodyShot,
            static _ => { });
    }
}
