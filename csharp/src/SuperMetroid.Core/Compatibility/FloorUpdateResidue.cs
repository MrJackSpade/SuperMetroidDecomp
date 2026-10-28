namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The descriptors the Ceres Ridley getaway's floor updates leave behind, owned for the session (#1338, #1406). The
/// cartridge queues each floor update as a nine-byte descriptor in WRAM <c>$02D0-$032F</c> (<c>QueueMode7Transfers</c>,
/// <c>$80:8B8B</c>), each followed by a cleared byte, and the NMI handler (<c>$80:8BBA</c>) clears only the first two
/// bytes after copying them, so the rest outlive the update. Samus's running stage five (<c>$91:0303</c>) and the invalid
/// poses' delay programs (<c>$91:0302</c>) read those bytes as delays: the running cadence's stage-five reads in the
/// Ceres SMV established it. The getaway records each update's floor updates here at the point it queued them; the
/// readers consult it whenever Samus's animation reads past its tables.
/// </summary>
[CompatibilityService("bounded over-read: the delays Samus's running stage five and the invalid poses read past their tables")]
[EncodingOwner("the floor update descriptors' residue bytes Samus's animation reads as delays")]
public sealed class FloorUpdateResidue : IFloorUpdateResidue
{
    /// <summary>The residue's bytes.</summary>
    public const int ByteCount = 96;

    /// <summary>
    /// Where the half of an animation window that aliases the residue ends: a delay over-read at a window position below
    /// this reads the residue, and one at or past it reads the installed animation data: the upper half of a
    /// sixteen-bit window position.
    /// </summary>
    public const int ResidueWindowEnd = 32768;

    /// <summary>The bytes one update's descriptor takes.</summary>
    private const int UpdateBytes = 9;

    /// <summary>The descriptors that fit with the cleared byte after the last.</summary>
    private const int UpdateCapacity = (ByteCount - 1) / UpdateBytes;

    /// <summary>The descriptor's leading byte while it is not cleared.</summary>
    private const byte Marker = 0x80;

    /// <summary>The descriptor byte naming where every floor update's tile numbers come from.</summary>
    private const byte TileSourceGroup = 0xa6;

    /// <summary>What each descriptor position last held, or none for one never written.</summary>
    private readonly Descriptor?[] descriptors = new Descriptor?[UpdateCapacity];

    /// <summary>
    /// Records one update's floor updates, in the order queued: each takes the next descriptor position from the first,
    /// the position after the last loses its marker, and the first position's marker and the low byte of its tile source
    /// are cleared once the update's descriptors are applied. An update that makes none leaves the residue as it was.
    /// </summary>
    /// <exception cref="InvalidOperationException">The update makes more floor updates than fit.</exception>
    public void Record(IReadOnlyList<FloorUpdate> updates)
    {
        Ensure.NotNull(updates);
        if (updates.Count == 0)
            return;
        if (updates.Count > UpdateCapacity)
            throw new InvalidOperationException($"{updates.Count} floor updates exceed the {UpdateCapacity} descriptors the residue holds.");
        for (int position = 0; position < updates.Count; position++)
        {
            descriptors[position] = new Descriptor(updates[position], DescriptorClearing.None);
            if (position + 1 < UpdateCapacity && descriptors[position + 1] is { } following)
                descriptors[position + 1] = following.WithMarkerCleared();
        }
        descriptors[0] = (descriptors[0] ?? throw new InvalidOperationException("A floor update run leaves no first descriptor.")) with
        {
            Clearing = DescriptorClearing.MarkerAndSourceLow,
        };
    }

    /// <inheritdoc/>
    public byte ByteAt(int index)
    {
        if ((uint)index >= ByteCount)
            throw new InvalidDataException($"Byte {index} lies outside the {ByteCount}-byte floor update residue.");
        int position = index / UpdateBytes;
        if (position >= UpdateCapacity || descriptors[position] is not { } descriptor)
            return 0;
        FloorUpdate update = descriptor.Update;
        return (index % UpdateBytes) switch
        {
            0 => descriptor.Clearing == DescriptorClearing.None ? Marker : (byte)0,
            1 => descriptor.Clearing == DescriptorClearing.MarkerAndSourceLow ? (byte)0 : Low(update.TileSource),
            2 => High(update.TileSource),
            3 => TileSourceGroup,
            4 => Low(update.TileCount),
            5 => High(update.TileCount),
            6 => Low(update.FloorCell),
            7 => High(update.FloorCell),
            _ => 0,
        };
    }

    /// <summary>The low eight bits of <paramref name="value"/>.</summary>
    private static byte Low(int value) => SnesLittleEndian.LowByte(unchecked((ushort)value));

    /// <summary>The high eight bits of a sixteen-bit <paramref name="value"/>.</summary>
    private static byte High(int value) => SnesLittleEndian.HighByte(unchecked((ushort)value));

    /// <summary>What one descriptor position holds.</summary>
    /// <param name="Update">The floor update written there last.</param>
    /// <param name="Clearing">Which of its bytes have since been cleared.</param>
    private sealed record Descriptor(FloorUpdate Update, DescriptorClearing Clearing)
    {
        /// <summary>This descriptor with its leading byte cleared, keeping whatever else was cleared.</summary>
        internal Descriptor WithMarkerCleared() => Clearing == DescriptorClearing.None ? this with { Clearing = DescriptorClearing.Marker } : this;
    }

    /// <summary>Which bytes of a descriptor a later run has cleared.</summary>
    private enum DescriptorClearing
    {
        /// <summary>None: the descriptor is as its update wrote it.</summary>
        None,

        /// <summary>Its leading byte, which the next run's first descriptor clears.</summary>
        Marker,

        /// <summary>Its leading byte and its tile source's low byte, which a run clears in its first descriptor.</summary>
        MarkerAndSourceLow,
    }
}
