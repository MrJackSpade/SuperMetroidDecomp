using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Installed extended-enemy BG2 presentation dispatch.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Draws the BG2 half of the already resolved visual frame. The common writer
    /// emits its OAM half, which may legitimately be empty for BG2-only poses.
    /// Native new-frame gating and physical collision selectors remain compiled.
    /// Selecting an OAM-only pose performs no BG2 write; it does not invent a clear.
    /// </summary>
    private void ApplyInstalledEnemyBg2Frame(RoomEnemySlot slot, ushort selected)
    {
        ReadOnlyMemory<EnemyBg2TilemapWrite> writes = default;
        string? family = null;
        bool found = false;
        if (slot.EnemyDefinitionPointer == MotherBrainBodyDefinition &&
            slot.Definition.Bank == MotherBrainBodyVisualDefinitions.Bank &&
            MotherBrainBodyVisualDefinitions.HasBg2(selected))
        {
            family = "Mother Brain body";
            found = TileArtwork!.MotherBrainBodyBg2Frames?.TryGet(selected, out writes) == true;
        }
        else if (slot.EnemyDefinitionPointer == CrocomireDefinition &&
            slot.Definition.Bank == CrocomireBodyVisualDefinitions.Bank &&
            CrocomireBodyVisualDefinitions.HasBg2(selected))
        {
            family = "Crocomire body";
            found = TileArtwork!.CrocomireBg2Frames?.TryGet(selected, out writes) == true;
        }
        else if (IsPhantoonPartDefinition(slot.EnemyDefinitionPointer) &&
            slot.Definition.Bank == PhantoonBg2FrameDefinitions.Bank &&
            PhantoonBg2FrameDefinitions.IsFrame(selected))
        {
            family = "Phantoon";
            found = TileArtwork!.PhantoonBg2Frames?.TryGet(selected, out writes) == true;
        }
        else if (IsDraygonDefinition(slot.EnemyDefinitionPointer) &&
            slot.Definition.Bank == DraygonBg2FrameDefinitions.Bank &&
            DraygonBg2FrameDefinitions.IsFrame(selected))
        {
            family = "Draygon";
            found = TileArtwork!.DraygonBg2Frames?.TryGet(selected, out writes) == true;
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
        if (_crocomireDeath is { } death)
            words.CopyTo(death.MutableBg2WorkingTilemap.Slice(destinationWord, words.Length));
        _vram!.ExecuteWordTransfer(words,
            unchecked((ushort)(EnemyBg2FrameLayout.VramBase + destinationWord)),
            wordIncrement: 1);
    }
}
