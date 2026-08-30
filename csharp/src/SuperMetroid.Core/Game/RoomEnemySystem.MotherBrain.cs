using SuperMetroid.Core.Hardware;

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
        byte nmiFrameCounter8)
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
                RunMotherBrainPhaseTwoAscent(state, samus, nmiFrameCounter8);
                return;
            default:
                throw new NotSupportedException(
                    $"Mother Brain body function $A9:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void RunMotherBrainFirstPhase(MotherBrainEnemyState state, SamusState? samus)
    {
        // Before PLM `$D6DE` finishes the nineteen missile increments and its 48-frame tail,
        // event two is clear and `$A9:87E1` performs no phase mutation. Head-vs-Samus and
        // projectile interactions are separate collision passes and remain outside this
        // body-function dispatcher.
        bool glassDestroyed = _hasEvent?.Invoke((int)EventNumber.MotherBrainGlassDestroyed) ?? false;
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
                state.RequestMusic(rawTrack: 6, delayFrames: 8);

                // MotherBrain_SealWall at `$AD:E396` creates both dust puffs before its
                // two identical hardcoded PLMs. Requests remain ordered because the bank
                // $84 allocator searches the same descending pool for each call.
                SpawnRoomGraphicsDustExplosion(248, 72, animationIndex: 9);
                SpawnRoomGraphicsDustExplosion(248, 152, animationIndex: 9);
                state.RequestPlm(blockX: 15, blockY: 4, header: 0xb673);
                state.RequestPlm(blockX: 15, blockY: 9, header: 0xb673);
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
                throw new NotSupportedException(
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
        ushort instruction,
        ref ushort cursor)
    {
        if (!IsMotherBrainDefinition(slot.EnemyDefinitionPointer))
            return false;

        switch (instruction)
        {
            case 0x9b0f: // Instruction_MotherBrain_GotoX: X = next same-bank word.
                cursor = ReadWord(
                    _bus!,
                    (slot.Definition.Bank << 16) | unchecked((ushort)(cursor + 2)));
                return true;
            default:
                return false;
        }
    }

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
