using SuperMetroid.Core.Input;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy instruction interpreter: common bank-$A0 instructions, owner-private instruction dispatch,
/// visual selectors and mechanics words, instruction VRAM transfers, and installed extended-frame BG2 writes.
/// </summary>
[RoomLifetime]
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

    /// <summary>Captures visual, room and encounter owners for instruction execution.</summary>
    internal EnemyInstructionInterpreter(CrocomireBehavior crocomireBehavior, MotherBrainEncounter motherBrainEncounter, EnemyPresentation presentation, EnemyRoomBinding room)
    {
        _crocomireBehavior = crocomireBehavior;
        _motherBrainEncounter = motherBrainEncounter;
        _presentation = presentation;
        _room = room;
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
        if (slot.Entity is IEnemyBg2Frames owner)
        {
            if (owner.FindBg2Frame(selected, _room) is { } lookup)
                (family, found, writes) = lookup;
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
            StagingWordView bg2 = _room.Staging.EnemyBg2Tilemap;
            for (int index = 0; index < words.Length; index++)
                bg2[destinationWord + index] = words[index];
        }
        // A boss that owns the enemy BG2 image draws into it (#1353). Mother Brain only stages her body until her
        // phase-two transition attaches the image (#1356), as native uploads nothing before then.
        if (_presentation.Bg2Image is { } image)
            image.Write(destinationWord, words);
        else if (_motherBrainEncounter.MotherBrain is null)
            new EnemyVram(_presentation.Vram).EnemyBg2Tilemap.Write(words, destinationWord);
    }

    /// <summary>
    /// Advances one enemy's typed animation (#641). Every enemy that processes instructions runs a typed program; one
    /// without is a missing conversion and fails here rather than reading cartridge instruction lists.
    /// </summary>
    internal static void ProcessInstructions(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level,
        Position16<RoomPixels> camera,
        SnesButton controllerInput)
    {
        if ((slot.Entity as IEnemyTypedAnimation)?.FindTypedAnimation() is not { } advance)
        {
            throw new InvalidDataException(
                $"Enemy {slot.EnemyDefinitionPointer} processes instructions but has no typed animation.");
        }
        advance(slot, new EnemyInstructionContext
            { Samus = samus, Level = level, Camera = camera, ControllerInput = controllerInput });
    }
}
