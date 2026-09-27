using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Common cartridge and installed extended-enemy BG2 stream dispatch.</summary>
public sealed partial class RoomEnemySystem
{
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
    /// Ports <c>ProcessExtendedTilemap</c> at $A0:96CA. Each command provides
    /// a WRAM byte destination, word count, and inline tile words; $FFFF ends
    /// the stream. Both the cartridge and installed paths share the final VRAM
    /// effect, and Crocomire also receives its working-image mirror.
    /// </summary>
    private void ProcessExtendedEnemyBg2Tilemap(byte bank, ushort streamPointer)
    {
        int cursor = (bank << 16) | unchecked((ushort)(streamPointer + 2));
        for (int command = 0;
             command < EnemyBg2FrameLayout.MaximumCommandsPerStream; command++)
        {
            ushort destination = ReadWord(_bus!, cursor);
            if (destination == 0xffff)
                return;

            int wordCount = ReadWord(_bus!, cursor + 2);
            if (wordCount is <= 0 or > CrocomireDeathState.Bg2WorkingWordCount)
                throw new InvalidDataException(
                    $"Extended BG2 command ${bank:X2}:{cursor & 0xffff:X4} has invalid word count {wordCount}.");

            int relativeByte = unchecked((ushort)(destination -
                EnemyBg2FrameLayout.WorkingRamBase));
            if ((relativeByte & 1) != 0)
                throw new InvalidDataException(
                    $"Extended BG2 command destination ${destination:X4} is not word aligned.");
            int destinationWord = relativeByte >> 1;
            if (destinationWord < 0 ||
                destinationWord + wordCount > CrocomireDeathState.Bg2WorkingWordCount)
                throw new InvalidDataException(
                    $"Extended BG2 command destination ${destination:X4} exceeds the enemy tilemap buffer.");

            ushort[] words = new ushort[wordCount];
            for (int word = 0; word < wordCount; word++)
                words[word] = ReadWord(_bus!, cursor + 4 + word * 2);

            ApplyExtendedEnemyBg2Words(destinationWord, words);
            cursor += 4 + wordCount * 2;
        }

        throw new InvalidDataException(
            $"Extended BG2 command stream ${bank:X2}:{streamPointer:X4} has no terminator.");
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
