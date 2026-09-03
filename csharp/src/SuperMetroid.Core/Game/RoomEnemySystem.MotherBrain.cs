using SuperMetroid.Core.Hardware;

using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Retail Mother Brain room load and first-phase body/head dispatch from
/// <c>$A9:8687-$A9:881C</c>. The fight is intentionally modeled as its two cartridge enemy
/// records and the post-enemy drawing hook they install, rather than as one synthetic actor.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort MotherBrainBodyDefinition = 0xec7f;
    private const ushort MotherBrainHeadDefinition = 0xec3f;
    private const ushort MotherBrainFallingTubeDefinition = 0xecff;
    private const ushort MotherBrainBlankBg2Tile = 0x0338;
    private const ushort MotherBrainBg2VramBase = 0x4800;
    private const int MotherBrainBg2WordCount = 0x0800;
    private const int MotherBrainGlassShardPalette = 0xa99514;
    private const int MotherBrainTubeProjectilePalette = 0xa994f4;
    private const ushort MotherBrainInitialDummyInstruction = 0x9c13;
    private const ushort MotherBrainInitialHeadInstruction = 0x9c21;

    private MotherBrainEnemyState? _motherBrain;
    private Action? _incrementMotherBrainGlassRoomArgument;

    /// <summary>The typed multipart encounter while Mother Brain's retail population is loaded.</summary>
    public MotherBrainEnemyState? MotherBrain => _motherBrain;

    private static bool IsMotherBrainDefinition(ushort definition) =>
        definition is MotherBrainBodyDefinition or MotherBrainHeadDefinition;

    private void ResetMotherBrainRoomState() => _motherBrain = null;

    /// <summary>Ports <c>InitAI_MotherBrainBody</c> at <c>$A9:8687</c>.</summary>
    private void InitializeMotherBrainBody(RoomEnemySlot body)
    {
        if (body.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Mother Brain's body requires native slot zero, not slot {body.SlotIndex}.");
        }

        // `$7E:2000-$2FFF` is the enemy BG2 staging surface; the renderer-visible mirror is
        // VRAM word `$4800`. The native loop writes all `$800` words, including the second
        // off-screen tilemap page, before either Mother Brain record can execute.
        ushort[] clearedTilemap = new ushort[MotherBrainBg2WordCount];
        Array.Fill(clearedTilemap, MotherBrainBlankBg2Tile);
        _vram!.ExecuteWordTransfer(clearedTilemap, MotherBrainBg2VramBase, wordIncrement: 1);

        body.CurrentInstruction = MotherBrainInitialDummyInstruction;
        body.InstructionTimer = 1;
        body.VramTilesIndex = 0;
        body.Properties = unchecked((ushort)(body.Properties | 0x1500));
        body.PaletteIndex = 0;

        // Both source labels include transparent color zero. `$A9:86B3/$86C0` deliberately
        // begin at +2 and copy only colors 1..15 into the otherwise unrelated room palette
        // ranges used by glass shards and tube projectiles.
        _cgram!.LoadFromBus(
            _bus!,
            MotherBrainGlassShardPalette,
            colorCount: 15,
            destinationIndex: 0x0162 / 2);
        _cgram.LoadFromBus(
            _bus!,
            MotherBrainTubeProjectilePalette,
            colorCount: 15,
            destinationIndex: 0x01e2 / 2);

        _motherBrain = new MotherBrainEnemyState(body)
        {
            Form = 0,
            EnableUnpauseHook = false,
            HitboxesEnabled = 2,
            BrainFunction = MotherBrainBrainFunction.SetupBrainToBeDrawn,
            Function = MotherBrainBodyFunction.FirstPhase,
            FxEntry = 1,
            BackgroundTilemapPrepared = true,
        };
        _motherBrain.RecordInitialTurretRequests();
        SpawnMotherBrainInitialTurrets();
    }

    /// <summary>Ports <c>InitAI_MotherBrainHead</c> at <c>$A9:8705</c>.</summary>
    private void InitializeMotherBrainHead(RoomEnemySlot head)
    {
        MotherBrainEnemyState state = _motherBrain ??
            throw new InvalidDataException("Mother Brain's head appeared before its body record.");
        if (head.SlotIndex != 1)
        {
            throw new InvalidDataException(
                $"Mother Brain's head requires native slot one, not slot {head.SlotIndex}.");
        }

        // The cartridge seeds the complete future corpse effect during room load, not when
        // Mother Brain dies. Keeping this eager call preserves the exact WRAM state visible
        // throughout every earlier phase and prevents the death sequence from inventing it.
        state.CorpseRotting.Initialize(_bus!);

        head.Health = 0x0bb8;
        head.CurrentInstruction = MotherBrainInitialHeadInstruction;
        head.InstructionTimer = 1;
        head.VramTilesIndex = 0;
        head.Properties = unchecked((ushort)(head.Properties | 0x1100));
        head.PaletteIndex = 0x0200;
        state.Head = head;
        state.NeckPaletteIndex = 0x0200;
        state.BrainPaletteIndex = 0x0200;

        // SetupMotherBrainHeadNormalPalette installs a ten-frame timer. Palette table
        // interpolation belongs to the later damage/phase slice, but the state producer is
        // already real here and therefore starts with the cartridge's exact value.
        state.BrainPaletteTimer = 0x000a;
    }

    /// <summary>Ports the body main/hurt entry at <c>$A9:873E</c> for phase one.</summary>
    private void RunMotherBrainBodyMain(
        RoomEnemySlot body,
        SamusState? samus,
        byte nmiFrameCounter8,
        SamusBombProjectileSystem? sharedProjectiles)
    {
        MotherBrainEnemyState state = RequireCompleteMotherBrainState(body);

        // `$A9:873E` advances the independent room-palette instruction list before it
        // dispatches the body function. Keeping that order is visible on the exact frame
        // the fake-death flash begins and later when the main tube stops the loop.
        RunMotherBrainRoomPalette(state);
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
                RunMotherBrainPhaseTwoAscent(
                    state,
                    samus,
                    nmiFrameCounter8,
                    sharedProjectiles);
                return;
            default:
                throw new InvalidDataException(
                    $"Mother Brain body function $A9:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void RunMotherBrainFirstPhase(MotherBrainEnemyState state, SamusState? samus)
    {
        // Before PLM `$D6DE` finishes the nineteen missile increments and its 48-frame tail,
        // event two is clear and `$A9:87E1` performs no phase mutation. Head-vs-Samus and
        // projectile interactions are separate collision passes and remain outside this
        // body-function dispatcher.
        bool glassDestroyed = RequireEvent(EventNumber.MotherBrainGlassDestroyed);
        if (glassDestroyed)
        {
            state.BrainMainShakeTimer = EarthquakeTimer;
            if (samus is not null && samus.XPosition < 0x00ec && state.Head!.Health == 0)
            {
                // This is the exact first-phase exit at `$A9:87FE-$8814`. The following
                // function remains represented by its native pointer so the next frame
                // enters the real timed descent rather than jumping to standing form.
                state.DeleteTurretsAndRinkas = true;
                state.Form = 1;
                state.RequestMusic(MusicCommand.SelectTrack(6), MusicCommandDelay.EightFrames);

                // MotherBrain_SealWall at `$AD:E396` creates both dust puffs before its
                // two identical hardcoded PLMs. Requests remain ordered because the bank
                // $84 allocator searches the same descending pool for each call.
                SpawnRoomGraphicsDustExplosion(248, 72, animationIndex: 9);
                SpawnRoomGraphicsDustExplosion(248, 152, animationIndex: 9);
                state.RequestPlm(
                    blockX: 15,
                    blockY: 4,
                    header: RoomPlmHeaders.FillMotherBrainsWall);
                state.RequestPlm(
                    blockX: 15,
                    blockY: 9,
                    header: RoomPlmHeaders.FillMotherBrainsWall);
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
                head.XPosition = state.NeckSegment4.X;
                head.YPosition = unchecked((ushort)(state.NeckSegment4.Y - 21));
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
    private void DrawMotherBrainHook(OamBuffer oam, ushort cameraX, ushort cameraY)
    {
        MotherBrainEnemyState? state = _motherBrain;
        RoomEnemySlot? head = state?.Head;
        if (state?.DrawBrain != true || head is null || head.SpritemapPointer == 0)
            return;

        (short brainShakeX, short brainShakeY) = GetMotherBrainBrainShake(state, head);
        DrawMotherBrainWorldSpritemap(
            oam,
            head.SpritemapPointer,
            unchecked((ushort)(head.XPosition + brainShakeX)),
            unchecked((ushort)(head.YPosition + brainShakeY)),
            state.BrainPaletteIndex,
            head.VramTilesIndex,
            cameraX,
            cameraY);

        if (!state.DrawNeck || state.Body.Properties.HasAny(EnemyProperties.Invisible))
            return;

        // `$A9:930C-$9354` emits the joint nearest the brain first and the body-side joint
        // last. Equal-priority overlap therefore follows the same low-OAM precedence.
        int shakeIndex = (head.FlashTimer & 6) >> 1;
        short neckShakeX = MotherBrainShakeXOffsets[shakeIndex];
        short neckShakeY = MotherBrainShakeYOffsets[shakeIndex];
        MotherBrainNeckPoint[] segments =
        [
            state.NeckSegment0,
            state.NeckSegment1,
            state.NeckSegment2,
            state.NeckSegment3,
            state.NeckSegment4,
        ];
        for (int index = segments.Length - 1; index >= 0; index--)
        {
            MotherBrainNeckPoint segment = segments[index];
            DrawMotherBrainWorldSpritemap(
                oam,
                spritemapPointer: 0xa694,
                unchecked((ushort)(segment.X + neckShakeX)),
                unchecked((ushort)(segment.Y + neckShakeY)),
                state.NeckPaletteIndex,
                head.VramTilesIndex,
                cameraX,
                cameraY);
        }
    }

    private static readonly short[] MotherBrainShakeXOffsets = [0, -1, 0, 1];
    private static readonly short[] MotherBrainShakeYOffsets = [0, 1, -1, 1];

    private void DrawMotherBrainWorldSpritemap(
        OamBuffer oam,
        ushort spritemapPointer,
        ushort worldX,
        ushort worldY,
        ushort paletteIndex,
        ushort vramTilesIndex,
        ushort cameraX,
        ushort cameraY)
    {
        // Mother Brain's private `$A9:93EE` writer rejects an entire map whose center begins
        // above the screen. The generic writer intentionally wraps, so preserve that unique
        // vertical gate here before sharing its byte-exact map decoder.
        ushort screenY = unchecked((ushort)(worldY - cameraY));
        if (unchecked((short)screenY) < 0)
            return;

        oam.AddEnemySpritemap(
            _bus!,
            bank: 0xa9,
            spritemapPointer,
            unchecked((ushort)(worldX - cameraX)),
            screenY,
            paletteIndex,
            vramTilesIndex);
    }

    private static (short X, short Y) GetMotherBrainBrainShake(
        MotherBrainEnemyState state,
        RoomEnemySlot head)
    {
        ushort shake;
        if (state.BrainMainShakeTimer != 0)
        {
            state.BrainMainShakeTimer = unchecked((ushort)(state.BrainMainShakeTimer - 1));
            shake = state.BrainMainShakeTimer;
        }
        else
        {
            shake = head.FlashTimer != 0 ? head.FlashTimer : head.ShakeTimer;
        }

        int index = (shake & 6) >> 1;
        return (MotherBrainShakeXOffsets[index], MotherBrainShakeYOffsets[index]);
    }

    private void StepMotherBrainNeck(MotherBrainEnemyState state, SamusState samus)
    {
        if (state.NeckMovementEnabled)
        {
            ushort lowerAngle = state.LowerNeckAngle;
            ushort upperAngle = state.UpperNeckAngle;
            ushort lowerIndex = state.LowerNeckMovementIndex;
            ushort upperIndex = state.UpperNeckMovementIndex;
            MotherBrainNeckKinematics.StepAngles(
                ref lowerAngle,
                ref upperAngle,
                ref lowerIndex,
                ref upperIndex,
                state.NeckAngleDelta,
                state.Head!.YPosition,
                samus.YPosition);
            state.LowerNeckAngle = lowerAngle;
            state.UpperNeckAngle = upperAngle;
            state.LowerNeckMovementIndex = lowerIndex;
            state.UpperNeckMovementIndex = upperIndex;
        }

        MotherBrainNeckGeometry geometry = MotherBrainNeckKinematics.CalculateGeometry(
            _bus!,
            state.Body.XPosition,
            state.Body.YPosition,
            state.LowerNeckAngle,
            state.UpperNeckAngle,
            state.NeckSegment0Distance,
            state.NeckSegment1Distance,
            state.NeckSegment2Distance,
            state.NeckSegment3Distance,
            state.NeckSegment4Distance);
        state.NeckSegment0 = geometry.Segment0;
        state.NeckSegment1 = geometry.Segment1;
        state.NeckSegment2 = geometry.Segment2;
        state.NeckSegment3 = geometry.Segment3;
        state.NeckSegment4 = geometry.Segment4;
    }

    /// <summary>Handles Mother Brain's private instruction opcodes used by phase-one art.</summary>
    private bool TryProcessMotherBrainInstruction(
        RoomEnemySlot slot,
        SamusState? samus,
        ushort instruction,
        ref ushort cursor)
    {
        if (!IsMotherBrainDefinition(slot.EnemyDefinitionPointer))
            return false;

        switch (instruction)
        {
            case MotherBrainInstructionCodes.Instruction_MotherBrain_GotoX:
                cursor = ReadWord(
                    _bus!,
                    (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                return true;

            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_EnableNeckMovement_GotoX:
                RequireCompleteMotherBrainState(slot).NeckMovementEnabled = true;
                cursor = ReadWord(
                    _bus!,
                    (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                return true;

            // `$95B6-$95F2` are the six posture-transition displacements. Each command
            // moves the body vertically, counter-scrolls BG2 by the opposite amount, and
            // derives BG2 X from the body's new origin plus its authored horizontal bias.
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy10_ScrollLeftBy4:
                MoveMotherBrainBodyWithScrollBias(-10, 4);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy16_ScrollLeftBy4:
                MoveMotherBrainBodyWithScrollBias(-16, 4);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy12_ScrollRightBy2:
                MoveMotherBrainBodyWithScrollBias(-12, -2);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy12_ScrollLeftBy4:
                MoveMotherBrainBodyWithScrollBias(12, 4);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy16_ScrollRightBy2:
                MoveMotherBrainBodyWithScrollBias(16, -2);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy10_ScrollRightBy2:
                MoveMotherBrainBodyWithScrollBias(10, -2);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            // The walk-cycle comments in the historical disassembly describe visual
            // motion and are occasionally opposite the literal signed Y operand. These
            // cases preserve the actual additions performed by `$9579`, not the labels.
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_ScrollRightBy1:
                MoveMotherBrainBody(1, -2);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyRightBy2:
                MoveMotherBrainBody(2, 0);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy1:
                MoveMotherBrainBody(0, 1);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy1_RightBy3_Footstep:
                RunMotherBrainFootstep();
                MoveMotherBrainBody(3, 1);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy15:
                MoveMotherBrainBody(15, -2);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy6:
                MoveMotherBrainBody(6, -4);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy2:
                MoveMotherBrainBody(-2, 4);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep:
                RunMotherBrainFootstep();
                MoveMotherBrainBody(-1, 2);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep_d:
                RunMotherBrainFootstep();
                MoveMotherBrainBody(-1, 2);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyLeftBy2:
                MoveMotherBrainBody(-2, 0);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy1:
                MoveMotherBrainBody(0, -1);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy1_LeftBy3:
                MoveMotherBrainBody(-3, -1);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy15_Footstep:
                RunMotherBrainFootstep();
                MoveMotherBrainBody(-15, 2);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy6:
                MoveMotherBrainBody(-6, 4);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy2:
                MoveMotherBrainBody(2, -4);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy1:
                MoveMotherBrainBody(1, -2);
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            // Pose is shared encounter state, not a property of the displayed map. Body AI
            // waits on these exact values while the instruction list continues independently.
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding:
                RequireCompleteMotherBrainState(slot).Pose = MotherBrainBodyPose.Standing;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToWalking:
                RequireCompleteMotherBrainState(slot).Pose = MotherBrainBodyPose.Walking;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouching:
                RequireCompleteMotherBrainState(slot).Pose = MotherBrainBodyPose.Crouched;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouchingTransition:
                RequireCompleteMotherBrainState(slot).Pose = MotherBrainBodyPose.CrouchingTransition;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToDeathBeamMode:
                RequireCompleteMotherBrainState(slot).Pose = MotherBrainBodyPose.DeathBeam;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToLeaningDown:
                RequireCompleteMotherBrainState(slot).Pose = MotherBrainBodyPose.LeaningDown;
                cursor = unchecked((ushort)(cursor + 2));
                return true;

            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_DisableNeckMovement:
                RequireCompleteMotherBrainState(slot).NeckMovementEnabled = false;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_QueueSoundX_Lib2_Max6:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                state.LastSoundEffect = ReadWord(
                    _bus!,
                    (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            }
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_QueueSoundX_Lib3_Max6:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                state.LastSoundEffectLibrary3 = ReadWord(
                    _bus!,
                    (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            }
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_SpawnDroolProjectile:
                SpawnMotherBrainDrool(RequireCompleteMotherBrainState(slot));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_SpawnPurpleBreathBigProjectile:
                SpawnMotherBrainPurpleBreathBig(RequireCompleteMotherBrainState(slot));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_SetMainShakeTimerTo50:
                RequireCompleteMotherBrainState(slot).BrainMainShakeTimer = 50;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_SpawnDustCloudExplosionProj:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                short xOffset = unchecked((short)ReadWord(
                    _bus!,
                    (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2))));
                short yOffset = unchecked((short)ReadWord(
                    _bus!,
                    (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 4))));
                ushort animationIndex = ReadWord(
                    _bus!,
                    (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 6)));
                SpawnRoomGraphicsDustExplosion(
                    unchecked((ushort)(state.Body.XPosition + xOffset)),
                    unchecked((ushort)(state.Body.YPosition + yOffset)),
                    animationIndex);
                cursor = unchecked((ushort)(cursor + 8));
                return true;
            }
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_SpawnDeathBeamProjectile:
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
            case MotherBrainInstructionCodes.Instruction_MotherBrainBody_IncrementDeathBeamAttackPhase:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                state.HandBeamPhase = unchecked((MotherBrainHandBeamPhase)(
                    (ushort)state.HandBeamPhase + 1));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_MaybeGotoNeutralPhase2:
                cursor = RequireRandomNumber() < 0xf000
                    ? (ushort)0x9c9f
                    : unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_GotoDyingDroolInstList:
            {
                // `$A9:9C65-$9C76` returns its destination in X rather than storing an
                // operand beside the opcode. The low twelve random bits select `$9C47`
                // only for `$FE0-$FFF`; every other value returns the one-frame `$9C5F`
                // loop. Assigning the cursor directly preserves that control convention.
                ushort random = RequireRandomNumber();
                cursor = (random & 0x0fff) >= 0x0fe0
                    ? (ushort)0x9c47
                    : (ushort)0x9c5f;
                return true;
            }
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_IncBabyMetroidAttackCounter:
                RequireLiveMotherBrainRainbowSequence(slot).
                    IncrementLiveBabyMetroidAttackCounter();
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_ResetBabyMetroidAttackCounter:
                RequireLiveMotherBrainRainbowSequence(slot).
                    ResetLiveBabyMetroidAttackCounter();
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_AimOnionRingsAtBabyMetroid:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                BabyMetroidCutsceneState baby = state.BabyMetroid ??
                    throw new InvalidOperationException(
                        "Mother Brain's Baby-targeting opcode ran without the Baby actor.");
                AimMotherBrainOnionRingsAt(state, baby.XPosition, baby.YPosition);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_AimOnionRingsAtSamus:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                SamusState target = samus ?? throw new InvalidOperationException(
                    "Mother Brain onion-ring aiming requires the active Samus actor.");
                AimMotherBrainOnionRingsAt(state, target.XPosition, target.YPosition);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_QueueBabyMetroidAttackSFX:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                MotherBrainRainbowBeamAttackSequence sequence =
                    RequireLiveMotherBrainRainbowSequence(slot);
                if (sequence.BabyMetroidAttackCounter != 0x000b)
                    state.LastSoundEffect = 0x006f;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_MaybeGotoNeutralPhase3:
                // Retail contains an unconditional BRA where the adjacent commentary might
                // suggest a carry branch. Low-twelve values below `$EC0` replace X with the
                // hold origin; the remaining values simply continue after this opcode.
                cursor = (RequireRandomNumber() & 0x0fff) < 0x0ec0
                    ? (ushort)0x9cd1
                    : unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_SpawnOnionRingsProjectile:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                SpawnMotherBrainOnionRing(state, unchecked((byte)state.OnionRingsTargetAngle));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_SpawnBombProjectileWithParamX:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                ushort afterburnCount = ReadWord(
                    _bus!,
                    (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                SpawnMotherBrainBomb(state, afterburnCount);
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            }
            case MotherBrainInstructionCodes.InstList_MotherBrainHead_SpawnLaserProjectile:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                RoomEnemySlot head = state.Head!;
                state.NeckMovementEnabled = false;

                // `$A9:9F4E-$9F66` writes an explicit mouth coordinate into the common
                // bank-$86 spawn scratch words, selects direction one (right), and invokes
                // definition `$A17B`. The source header remains the head record, so shared
                // initializer `$86:A009` obtains Mother Brain's authored contact damage and
                // her parameter-one speed flag without any encounter-specific substitute.
                RoomEnemyProjectileSlot? laser = SpawnPirateMotherBrainLaser(
                    head,
                    unchecked((ushort)(head.XPosition + 0x0010)),
                    unchecked((ushort)(head.YPosition + 0x0004)),
                    movingRight: true);
                if (laser is not null)
                    state.LastSoundEffect = PirateMotherBrainLaserSound;

                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_SpawnRainbowBeamChargingProj:
                SpawnMotherBrainRainbowChargingProjectile(
                    RequireCompleteMotherBrainState(slot));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case MotherBrainInstructionCodes.Instruction_MotherBrainHead_SetupEffectsForRainbowBeamCharge:
            {
                MotherBrainEnemyState state = RequireCompleteMotherBrainState(slot);
                state.SmallPurpleBreathGenerationEnabled = false;
                state.BrainPaletteTimer = 0x0202;
                state.LastSoundEffect = 0x007f;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            }
            default:
                return false;
        }
    }

    /// <summary>
    /// Shared `$A9:9E37/$9E5B` tail. Both target selectors subtract the same mouth offsets,
    /// call the cartridge angle routine, rotate by `$80`, and clamp the launch arc to
    /// `$10..$48`; only the source coordinate differs.
    /// </summary>
    private static void AimMotherBrainOnionRingsAt(
        MotherBrainEnemyState state,
        ushort targetX,
        ushort targetY)
    {
        RoomEnemySlot head = state.Head!;
        short deltaX = unchecked((short)(targetX - head.XPosition - 0x000a));
        short deltaY = unchecked((short)(targetY - head.YPosition - 0x0010));
        byte angle = unchecked((byte)(0x80 - CalculateCartridgeAngle(deltaX, deltaY)));

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

    private MotherBrainRainbowBeamAttackSequence RequireLiveMotherBrainRainbowSequence(
        RoomEnemySlot slot) =>
        RequireCompleteMotherBrainState(slot).RainbowBeamSequence ??
        throw new InvalidDataException(
            "Mother Brain head bytecode requires the live rainbow/Baby sequence.");

    private MotherBrainEnemyState RequireCompleteMotherBrainState(RoomEnemySlot slot)
    {
        MotherBrainEnemyState state = _motherBrain ??
            throw new InvalidDataException("Mother Brain record has no shared encounter state.");
        if (state.Head is null)
            throw new InvalidDataException("Mother Brain's body/head population is incomplete.");
        if (!ReferenceEquals(slot, state.Body) && !ReferenceEquals(slot, state.Head))
            throw new InvalidDataException("A non-Mother-Brain slot entered the Mother Brain dispatcher.");
        return state;
    }
}
