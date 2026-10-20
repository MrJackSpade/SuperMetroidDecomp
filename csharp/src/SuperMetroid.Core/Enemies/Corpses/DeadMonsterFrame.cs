using System.Collections.Immutable;
﻿using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// State shared by the dead-monster family (dead sidehoppers and Tourian corpses): the bank-$A9
/// corpse work words staged in WRAM, the installed corpse tile sheet, and the rot VRAM transfers
/// and dust sound the family publishes each frame.
/// </summary>
internal sealed class DeadMonsterFrame
{
    /// <summary>Room scratch memory and installed artwork shared by corpse-family actors.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Connects dead-monster work words and rot transfers to the active room.</summary>
    internal DeadMonsterFrame(EnemyRoomBinding room)
    {
        _room = room;
    }

    /// <summary>Rot transfers queued by this frame's dead-monster rows.</summary>
    internal List<VramWriteEntry> VramTransfers { get; } = [];

    /// <summary>The dust sound requested by a completed sidehopper or corpse row this frame, or null.</summary>
    public SoundEffectId? LastSoundEffect { get; internal set; }

    /// <summary>Drops this frame's transfers and sound request.</summary>
    internal void Clear()
    {
        VramTransfers.Clear();
        LastSoundEffect = null;
    }

    /// <summary>Reads a word from the bank-$A9 corpse work area using a word index.</summary>
    internal ushort ReadWorkWord(int wordOffset) =>
        _room.Staging.CorpseTiles.ReadWord(wordOffset * 2);

    /// <summary>Writes a word into the corpse work area using the same word indexing as native rot tables.</summary>
    internal void WriteWorkWord(int wordOffset, ushort value) =>
        _room.Staging.CorpseTiles.WriteWord(wordOffset * 2, value);

    /// <summary>Appends the source, size, and destination of each rot DMA record of a corpse's upload list.</summary>
    internal void AppendVramTransfers(ImmutableArray<DeadMonsterVramTransferDefinition> transfers)
    {
        foreach (DeadMonsterVramTransferDefinition record in transfers)
            VramTransfers.Add(new VramWriteEntry(
                record.SizeInBytes, record.SourceAddress, record.Destination));
    }

    /// <summary>
    /// Zoomer, Ripper and Skree copy from the shared installed bank-$B7 source sheet.
    /// Resolving their editable ED7F PNG is mandatory before publishing an owner or
    /// initializing its mutable rot table. There is no cartridge fallback.
    /// </summary>
    internal ReadOnlySpan<byte> InstalledCorpseTiles()
    {
        if (_room.Artwork is null)
            throw new InvalidDataException("Dead Tourian corpses require installed artwork.");
        if (!_room.Artwork.TryResolve(DeadTourianCorpseArtworkDefinitions.SourceAddress,
                DeadTourianCorpseArtworkDefinitions.ByteCount, out ReadOnlyMemory<byte> tiles))
            throw new InvalidDataException(
                "Installed Tourian corpse sheet enemy-ed7f-tiles.png is missing or has the wrong size.");
        return tiles.Span;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastSoundEffect is SoundEffectId sound)
            audio.QueueEnemySound(sound, maximumQueued: 3);
    }
}
