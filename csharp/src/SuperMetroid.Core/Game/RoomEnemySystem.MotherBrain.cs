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
    private void RunMotherBrainBodyMain(RoomEnemySlot body, SamusState? samus)
    {
        MotherBrainEnemyState state = RequireCompleteMotherBrainState(body);
        switch (state.Function)
        {
            case MotherBrainBodyFunction.FirstPhase:
                RunMotherBrainFirstPhase(state, samus);
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
        if (!glassDestroyed)
            return;

        state.BrainMainShakeTimer = EarthquakeTimer;
        if (samus is null || samus.XPosition >= 0x00ec || state.Head!.Health != 0)
            return;

        // This is the exact first-phase exit at `$A9:87FE-$8814`. The following function is
        // deliberately represented by its native pointer; its timed descent/tube sequence is
        // the next encounter slice rather than an invented jump straight to standing form.
        state.DeleteTurretsAndRinkas = true;
        state.Form = 1;
        state.Function = MotherBrainBodyFunction.FakeDeathDescentInitialPause;
        throw new NotSupportedException(
            "Mother Brain reached fake-death descent $A9:881D; that phase is not translated yet.");
    }

    /// <summary>Ports the head main/hurt entry and its first-phase draw-hook selector.</summary>
    private void RunMotherBrainHeadMain(RoomEnemySlot head)
    {
        MotherBrainEnemyState state = RequireCompleteMotherBrainState(head);

        // `$A9:878B` first restores the global enemy-graphics-drawn hook to RTL. Only an
        // intentionally invisible head dispatches the shared brain function and replaces it.
        state.DrawBrain = false;
        if (!head.Properties.HasAny(EnemyProperties.Invisible))
            return;

        switch (state.BrainFunction)
        {
            case MotherBrainBrainFunction.SetupBrainToBeDrawn:
                state.DrawBrain = true;
                return;
            case MotherBrainBrainFunction.SetupBrainAndNeckToBeDrawn:
                throw new NotSupportedException(
                    "Mother Brain neck draw function $A9:87A2 is not translated yet.");
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

        oam.AddEnemySpritemap(
            _bus!,
            head.Definition.Bank,
            head.SpritemapPointer,
            unchecked((ushort)(head.XPosition - cameraX)),
            unchecked((ushort)(head.YPosition - cameraY)),
            state.BrainPaletteIndex,
            head.VramTilesIndex);
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
