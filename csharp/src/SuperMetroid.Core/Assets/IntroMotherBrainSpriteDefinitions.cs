using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>Three visual frames selected by the intro Mother Brain's compiled lists.</summary>
internal static class IntroMotherBrainSpriteDefinitions
{
    /// <summary>$8C:8C00, first nine-part Mother Brain frame.</summary>
    internal const ushort FrameZero = 0x8c00;
    /// <summary>Every retail frame has nine OAM parts; $8C:8C8D is unrelated.</summary>
    internal const int StockPartCount = 9;

    /// <summary>Calculates the bank-$8C pointer for one of the three fixed-size Mother Brain OAM frames.</summary>
    /// <param name="frame">Zero-based frame number from 0 through 2.</param>
    /// <returns>The start address of that frame's OAM data.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> does not identify one of the three frames.</exception>
    internal static ushort FramePointer(int frame)
    {
        if ((uint)frame >= 3) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(FrameZero + frame * (2 + 5 * StockPartCount));
    }
    /// <summary>Lists the three intro Mother Brain frames in animation order with their native data pointers and asset names.</summary>
    internal static IReadOnlyList<IntroMotherBrainSpriteFrameDefinition> Frames { get; } = new FrameList();

    /// <summary>Provides indexed frame metadata without duplicating the fixed-size native pointer calculation.</summary>
    private sealed class FrameList : IReadOnlyList<IntroMotherBrainSpriteFrameDefinition>
    {
        /// <summary>Gets the number of visual frames in the intro sequence.</summary>
        public int Count => 3;

        /// <summary>Creates the metadata entry for a frame in animation order.</summary>
        /// <param name="index">Zero-based frame number from 0 through 2.</param>
        /// <returns>The frame's bank pointer and installed sprite-art identifier.</returns>
        public IntroMotherBrainSpriteFrameDefinition this[int index] =>
            new(FramePointer(index), $"mother-brain-frame-{index}");

        /// <summary>Enumerates frame metadata from the first intro image through the third.</summary>
        /// <returns>An enumerator over the three frame definitions.</returns>
        public IEnumerator<IntroMotherBrainSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/// <summary>Associates one intro Mother Brain OAM frame with its cartridge pointer and installed artwork asset.</summary>
/// <param name="Pointer">Bank-$8C address of the frame's nine-part OAM data.</param>
/// <param name="Name">Stable asset identifier used to load the corresponding sprite artwork.</param>
internal readonly record struct IntroMotherBrainSpriteFrameDefinition(ushort Pointer, string Name);
