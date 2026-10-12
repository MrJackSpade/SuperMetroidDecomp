using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// State shared by the dead-monster family (dead sidehoppers and Tourian corpses): the bank-$A9
/// corpse work words staged in WRAM, the installed corpse tile sheet, and the rot VRAM transfers
/// and dust sound the family publishes each frame.
/// </summary>
internal sealed class DeadMonsterFrame
{
    /// <summary>Stores room.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Initializes a new dead monster frame instance.</summary>
    internal DeadMonsterFrame(EnemyRoomBinding room)
    {
        _room = room;
    }

    /// <summary>Rot transfers queued by this frame's dead-monster rows.</summary>
    internal List<VramWriteEntry> VramTransfers { get; } = [];

    /// <summary>Last library-two dust sound requested by a completed sidehopper or corpse row.</summary>
    public ushort? LastSoundEffect { get; internal set; }

    /// <summary>Drops this frame's transfers and sound request.</summary>
    internal void Clear()
    {
        VramTransfers.Clear();
        LastSoundEffect = null;
    }

    /// <summary>Reads work word.</summary>
    internal ushort ReadWorkWord(int wordOffset) =>
        _room.Scratch.CorpseTiles.ReadWord(wordOffset * 2);

    /// <summary>Writes work word.</summary>
    internal void WriteWorkWord(int wordOffset, ushort value) =>
        _room.Scratch.CorpseTiles.WriteWord(wordOffset * 2, value);

    /// <summary>Appends vram transfers.</summary>
    internal void AppendVramTransfers(ushort vramTablePointer)
    {
        foreach (DeadMonsterVramTransferDefinition record in
                 DeadMonsterRottingDefinitions.ForTransferTable(vramTablePointer))
            VramTransfers.Add(new VramWriteEntry(
                record.SizeInBytes, record.SourceAddress, record.EncodedVramDestination));
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
        audio.QueueLegacySound(LastSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 3);
    }
}
