using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Installed extended-enemy BG2 presentation dispatch.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Draws the selected mixed body's BG2 half; the common draw emits its OAM half.</summary>
    private void ApplyInstalledMotherBrainBodyBg2(RoomEnemySlot slot)
    {
        if (slot.EnemyDefinitionPointer != MotherBrainBodyDefinition ||
            TileArtwork?.ExtendedFrames is not { } frames)
            return;
        ushort selected = frames.GetDisplayPointer(slot.Definition.Bank, slot.SpritemapPointer);
        if (!MotherBrainBodyVisualDefinitions.HasBg2(selected))
            return;
        if (TileArtwork.MotherBrainBodyBg2Frames?.TryGet(selected,
                out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) != true)
            throw new InvalidDataException(
                $"Installed Mother Brain body frame $A9:{selected:X4} has no BG2 presentation.");
        if (slot.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame))
            foreach (EnemyBg2TilemapWrite write in writes.Span)
                ApplyExtendedEnemyBg2Words(write.DestinationWord, write.Tiles.Span);
    }

    /// <summary>
    /// Crocomire's native extended roots interleave ordinary OAM and BG2
    /// streams. Apply only their installed BG2 half here; the common installed
    /// extended-frame draw immediately afterward emits their OAM half.
    /// </summary>
    private void ApplyInstalledCrocomireBodyBg2(RoomEnemySlot slot)
    {
        if (slot.EnemyDefinitionPointer != CrocomireDefinition ||
            slot.Definition.Bank != CrocomireBodyVisualDefinitions.Bank ||
            !CrocomireBodyVisualDefinitions.HasBg2(slot.SpritemapPointer) ||
            TileArtwork?.ExtendedFrames?.TryGetDisplay(slot.Definition.Bank,
                slot.SpritemapPointer, out _) != true)
            return;
        if (TileArtwork.CrocomireBg2Frames?.TryGet(slot.SpritemapPointer,
                out ReadOnlyMemory<EnemyBg2TilemapWrite> writes) != true)
            throw new InvalidDataException(
                $"Installed Crocomire body frame $A4:{slot.SpritemapPointer:X4} has no BG2 presentation.");
        if (slot.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame))
            foreach (EnemyBg2TilemapWrite write in writes.Span)
                ApplyExtendedEnemyBg2Words(write.DestinationWord, write.Tiles.Span);
    }

    /// <summary>
    /// Executes an installed extended BG2 visual stream for the converted boss
    /// families. The native new-frame bit gates writes; collision remains on
    /// the separate gameplay path and is never read from editable visual JSON.
    /// </summary>
    private bool TryDrawInstalledEnemyBg2Frame(RoomEnemySlot slot)
    {
        if (TileArtwork is null)
            return false;
        ReadOnlyMemory<EnemyBg2TilemapWrite> writes;
        if (slot.Definition.Bank == PhantoonBg2FrameDefinitions.Bank)
        {
            if (TileArtwork.PhantoonBg2Frames?.TryGet(slot.SpritemapPointer,
                    out writes) != true)
                return false;
        }
        else if (slot.Definition.Bank == DraygonBg2FrameDefinitions.Bank)
        {
            if (TileArtwork.DraygonBg2Frames?.TryGet(slot.SpritemapPointer,
                    out writes) != true)
                return false;
        }
        else
            return false;

        if (slot.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame))
            foreach (EnemyBg2TilemapWrite write in writes.Span)
                ApplyExtendedEnemyBg2Words(write.DestinationWord, write.Tiles.Span);
        return true;
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
