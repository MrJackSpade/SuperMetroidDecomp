using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.MamaTurtles;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy instruction interpreter: common bank-$A0 instructions, owner-private instruction dispatch,
/// visual selectors and mechanics words, instruction VRAM transfers, and installed extended-frame BG2 writes.
/// </summary>
internal sealed class EnemyInstructionInterpreter
{
    /// <summary>Owns the working BG2 tilemap used by Crocomire's extended frames.</summary>
    private readonly CrocomireBehavior _crocomireBehavior;
    /// <summary>Owns the Mother Brain BG2 working tilemap when that encounter is active.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;
    /// <summary>Provides VRAM for installed BG2 frame and instruction-time tile transfers.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Provides installed visual assets and room scratch memory.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Supplies species-owned private instruction handlers.</summary>
    private readonly IEnemyDispatch _dispatch;

    /// <summary>Captures visual, room, encounter, and species dispatch owners for instruction execution.</summary>
    internal EnemyInstructionInterpreter(CrocomireBehavior crocomireBehavior, MotherBrainEncounter motherBrainEncounter, EnemyPresentation presentation, EnemyRoomBinding room, IEnemyDispatch dispatch)
    {
        _crocomireBehavior = crocomireBehavior;
        _motherBrainEncounter = motherBrainEncounter;
        _presentation = presentation;
        _room = room;
        _dispatch = dispatch;
    }
    /// <summary>
    /// Executes one common enemy instruction. <paramref name="yieldInterpreter"/> reports that
    /// the instruction ended this frame's interpretation (delete, sleep, or wait).
    /// </summary>
    private void ProcessCommonEnemyInstruction(
        RoomEnemySlot slot,
        CommonEnemyInstruction instruction,
        ref ushort cursor,
        out bool yieldInterpreter)
    {
        yieldInterpreter = false;
        switch (instruction)
        {
            case CommonEnemyInstruction.StopScript:
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
                yieldInterpreter = true;
                return;
            case CommonEnemyInstruction.Goto:
                cursor = ReadEnemyInstructionMechanicsWord(
                    slot,
                    unchecked((ushort)(cursor + 2)));
                return;
            case CommonEnemyInstruction.Sleep:
                slot.CurrentInstruction = cursor;
                yieldInterpreter = true;
                return;
            case CommonEnemyInstruction.EnableOffScreenProcessing:
                slot.Properties = slot.Properties.With(EnemyProperties.ProcessOffScreen);
                cursor = unchecked((ushort)(cursor + 2));
                return;
            case CommonEnemyInstruction.DisableOffScreenProcessing:
                slot.Properties = slot.Properties.Without(EnemyProperties.ProcessOffScreen);
                cursor = unchecked((ushort)(cursor + 2));
                return;
            case CommonEnemyInstruction.DecrementTimerAndGoto:
            case CommonEnemyInstruction.DecrementTimerAndGotoDuplicate:
                slot.Timer = slot.Timer.Decrement().Remaining;
                cursor = !slot.Timer.IsZero
                    ? ReadEnemyInstructionMechanicsWord(
                        slot,
                        unchecked((ushort)(cursor + 2)))
                    : unchecked((ushort)(cursor + 4));
                return;
            case CommonEnemyInstruction.SetTimer:
                slot.Timer = NativeCountdown.FromRaw(ReadEnemyInstructionMechanicsWord(
                    slot,
                    unchecked((ushort)(cursor + 2))));
                cursor = unchecked((ushort)(cursor + 4));
                return;
            case CommonEnemyInstruction.WaitFrames:
                slot.InstructionTimer = NativeCountdown.FromRaw(ReadEnemyInstructionMechanicsWord(
                    slot,
                    unchecked((ushort)(cursor + 2))));
                slot.CurrentInstruction = unchecked((ushort)(cursor + 4));
                yieldInterpreter = true;
                return;
            case CommonEnemyInstruction.CopyToVram:
                // Only Torizo's lists copy tiles, and its typed programs carry the transfers (#641).
                throw new InvalidDataException(
                    $"Enemy tile transfer ${slot.Definition.Bank:X2}:${cursor:X4} has no translated owner.");
            default:
                throw new InvalidOperationException(
                    $"Undefined {nameof(CommonEnemyInstruction)} {(int)instruction:X4}.");
        }
    }

    /// <summary>
    /// Offers a non-common instruction word to the owner handlers. The owner predicates are
    /// disjoint, so at most one handler can accept the word; the order mirrors the original
    /// single-switch interpreter. Returns false when no owner translates the word.
    /// </summary>
    private bool TryProcessOwnedEnemyInstruction(RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter)
    {
        foreach (EnemyOwnedInstructionHandler handler in _dispatch.Ai.OwnedInstructions)
        {
            if (handler(slot, context, word, ref cursor, out yieldInterpreter))
                return true;
        }
        yieldInterpreter = false;
        return false;
    }
    /// <summary>
    /// Draws the BG2 half of the already resolved visual frame. The common writer
    /// emits its OAM half, which may legitimately be empty for BG2-only poses.
    /// Native new-frame gating and physical collision selectors remain compiled.
    /// Selecting an OAM-only pose performs no BG2 write; it does not invent a clear.
    /// </summary>
    internal void ApplyInstalledEnemyBg2Frame(RoomEnemySlot slot, ushort selected)
    {
        ReadOnlyMemory<EnemyBg2TilemapWrite> writes = default;
        string? family = null;
        bool found = false;
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainBody &&
            slot.Definition.Bank == MotherBrainBodyVisualDefinitions.Bank &&
            MotherBrainBodyVisualDefinitions.HasBg2(selected))
        {
            family = "Mother Brain body";
            found = _room.InstalledArtwork.MotherBrainBodyBg2Frames?.TryGet(selected, out writes) == true;
        }
        else if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire &&
            slot.Definition.Bank == CrocomireBodyVisualDefinitions.Bank &&
            CrocomireBodyVisualDefinitions.HasBg2(selected))
        {
            family = "Crocomire body";
            found = _room.InstalledArtwork.CrocomireBg2Frames?.TryGet(selected, out writes) == true;
        }
        else if (PhantoonBehavior.IsPhantoonPartDefinition(slot.EnemyDefinitionPointer) &&
            slot.Definition.Bank == PhantoonBg2FrameDefinitions.Bank &&
            PhantoonBg2FrameDefinitions.IsFrame(selected))
        {
            family = "Phantoon";
            found = _room.InstalledArtwork.PhantoonBg2Frames?.TryGet(selected, out writes) == true;
        }
        else if (DraygonBehavior.IsDraygonDefinition(slot.EnemyDefinitionPointer) &&
            slot.Definition.Bank == DraygonBg2FrameDefinitions.Bank &&
            DraygonBg2FrameDefinitions.IsFrame(selected))
        {
            family = "Draygon";
            found = _room.InstalledArtwork.DraygonBg2Frames?.TryGet(selected, out writes) == true;
        }
        if (family is null)
            return;
        if (!found)
            throw new InvalidDataException(
                $"Installed {family} frame ${slot.Definition.Bank:X2}:{selected:X4} has no BG2 presentation.");
        if (slot.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame))
            foreach (EnemyBg2TilemapWrite write in writes.Span)
                ApplyExtendedEnemyBg2Words(write.DestinationWord, write.Tiles.Span);
    }

    /// <summary>
    /// Common $A0:96CA destination semantics for cartridge and installed
    /// tilemap streams. Only the source of the visual words changes.
    /// </summary>
    private void ApplyExtendedEnemyBg2Words(int destinationWord, ReadOnlySpan<ushort> words)
    {
        if (destinationWord < 0 || words.Length == 0 ||
            destinationWord + words.Length > CrocomireDeathState.Bg2WorkingWordCount)
            throw new InvalidDataException("Extended BG2 write exceeds the enemy tilemap buffer.");
        if (_crocomireBehavior.CrocomireDeath is { } death)
            words.CopyTo(death.MutableBg2WorkingTilemap.Slice(destinationWord, words.Length));
        if (_motherBrainEncounter.MotherBrain is not null)
        {
            WorkRamWordTable bg2 = _room.Scratch.EnemyBg2Tilemap;
            for (int index = 0; index < words.Length; index++)
                bg2[destinationWord + index] = words[index];
        }
        new EnemyVram(_presentation.Vram).EnemyBg2Tilemap.Write(words, destinationWord);
    }

    /// <summary>Resolves a frame operand through its enemy family's compiled visual selector catalog.</summary>
    internal static ushort ReadEnemyVisualSelector(RoomEnemySlot slot, ushort operandAddress)
    {
        if (EnemySpritemapDefinitions.TryFrameAt(
                slot.EnemyDefinitionPointer, operandAddress, out ushort frame))
            return frame;
        if (CompiledEnemyVisualSelectors.TryGet(slot.Definition.Bank,
                operandAddress, out ushort selected))
            return selected;
        throw new InvalidDataException(
            $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} has no compiled visual selector " +
            $"${slot.Definition.Bank:X2}:${operandAddress:X4}.");
    }

    /// <summary>Advances one enemy animation timer and interprets commands until a frame or yielding opcode.</summary>
    internal void ProcessInstructions(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level,
        Position16<RoomPixels> camera,
        ushort controllerInput)
    {
        // Migrated owners run typed animations in place of their native instruction lists (#641).
        if (_dispatch.Ai.HasTypedAnimation(slot))
        {
            _dispatch.Ai.AdvanceTypedAnimation(slot, new EnemyInstructionContext
                { Samus = samus, Level = level, Camera = camera, ControllerInput = controllerInput });
            return;
        }

        ushort oldTimer = slot.InstructionTimer.RawValue;
        slot.InstructionTimer = slot.InstructionTimer.Decrement().Remaining;
        if (oldTimer != 1)
        {
            slot.ExtraProperties = slot.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        ushort cursor = slot.CurrentInstruction;
        for (int commandCount = 0; commandCount < 64; commandCount++)
        {
            ushort word = ReadEnemyInstructionMechanicsWord(slot, cursor);
            if ((word & 0x8000) == 0)
            {
                slot.InstructionTimer = NativeCountdown.FromRaw(word);
                slot.SpritemapPointer = ReadEnemyVisualSelector(slot,
                    unchecked((ushort)(cursor + 2)));
                slot.CurrentInstruction = unchecked((ushort)(cursor + 4));
                slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
                return;
            }

            // Common bank-$A0 commands behave identically for every owner and no owner's
            // private opcode shares their offsets, so they are decoded first.
            if (Enum.IsDefined((CommonEnemyInstruction)word))
            {
                ProcessCommonEnemyInstruction(
                    slot,
                    (CommonEnemyInstruction)word,
                    ref cursor,
                    out bool yieldAfterCommon);
                if (yieldAfterCommon)
                    return;
                continue;
            }

            if (!TryProcessOwnedEnemyInstruction(slot, new EnemyInstructionContext { Samus = samus, Level = level, Camera = camera, ControllerInput = controllerInput }, word, ref cursor, out bool yieldAfterOwned))
            {
                throw new InvalidDataException(
                    $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} instruction " +
                    $"${slot.Definition.Bank:X2}:{cursor:X4} opcode ${word:X4} is not translated.");
            }
            if (yieldAfterOwned)
                return;
        }

        throw new InvalidDataException(
            $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} instruction list exceeded 64 commands without a frame.");
    }

    /// <summary>
    /// Resolves simulation-owned enemy instruction words from compiled definitions.
    /// Presentation selectors resolve separately to installed artwork identities.
    /// </summary>
    internal static ushort ReadEnemyInstructionMechanicsWord(RoomEnemySlot slot, ushort address)
    {
        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Shaktool)
            return ShaktoolInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Chozo)
            return ChozoStatueInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.MiniKraid)
            return FakeKraidInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Yard)
            return YardInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (WalkingSpacePirateBehavior.IsWalkingSpacePirateDefinition(slot.EnemyDefinitionPointer))
        {
            return WalkingSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(
                address);
        }

        if (WallSpacePirateBehavior.IsWallSpacePirateDefinition(slot.EnemyDefinitionPointer))
        {
            return WallSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(
                address);
        }

        if (NinjaSpacePirateBehavior.IsNinjaSpacePirateDefinition(slot.EnemyDefinitionPointer))
        {
            return NinjaSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(
                address);
        }

        if (WorkRobotBehavior.IsWorkRobotDefinition(slot.EnemyDefinitionPointer))
            return WorkRobotInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.TourianStatue)
        {
            return TourianEntranceStatueInstructionProgramDefinitions.ReadMechanicsWord(
                address);
        }

        if (slot.EnemyDefinitionPointer is
            EnemyDefinitionId.MamaTurtle or
            EnemyDefinitionId.BabyTurtle)
        {
            return MamaTurtleInstructionProgramDefinitions.ReadMechanicsWord(address);
        }

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Hibashi)
            return HibashiInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Magdollite)
            return MagdolliteInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (KiHunterBehavior.IsKiHunterDefinition(slot.EnemyDefinitionPointer))
            return KiHunterInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Etecoon)
            return EtecoonInstructionProgramDefinitions.ReadMechanicsWord(address);

        if (slot.EnemyDefinitionPointer == EnemyDefinitionId.Dachora)
            return DachoraInstructionProgramDefinitions.ReadMechanicsWord(address);


        throw new InvalidDataException(
            $"Enemy ${(int)slot.EnemyDefinitionPointer:X4} instruction mechanics pointer " +
            $"${slot.Definition.Bank:X2}:{address:X4} has no compiled owner.");
    }
}
