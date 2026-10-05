using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>Visual OAM compositions selected by the SR388 egg and confused-baby lists.</summary>
internal static class IntroDiscoveryActorSpriteDefinitions
{
    /// <summary>$8C:8D6F, first egg composition.</summary>
    internal const ushort EggStart = 0x8d6f;
    /// <summary>$8C:8F7E, exclusive end of the sixteen consecutive egg compositions.</summary>
    internal const ushort EggEnd = 0x8f7e;
    /// <summary>$8C:8FCB, first confused-baby composition.</summary>
    internal const ushort BabyStart = 0x8fcb;
    /// <summary>$8C:8FE0, exclusive end of the three small confused-baby compositions.</summary>
    internal const ushort BabySmallEnd = 0x8fe0;
    /// <summary>$8C:909D, Ceres large asteroids reused by the second confused-baby list.
    /// The published asset key remains hatched-baby.</summary>
    internal const ushort BabyLarge = 0x909d;
    /// <summary>$8C:90FE, exclusive end of the reused nineteen-part asteroid composition.</summary>
    internal const ushort BabyEnd = 0x90fe;

    /// <summary>Six-part intact record, eight nine-part cracking records, then seven three-part remnants.</summary>
    internal static ushort EggFramePointer(int frame)
    {
        if ((uint)frame >= 16) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(EggStart + (frame > 0 ? 32 : 0) + 47 * Math.Clamp(frame - 1, 0, 8) + 17 * Math.Max(frame - 9, 0));
    }

    /// <summary>Three consecutive one-part confused-baby records, seven bytes each.</summary>
    internal static ushort BabyFramePointer(int frame)
    {
        if ((uint)frame >= 3) throw new ArgumentOutOfRangeException(nameof(frame));
        return (ushort)(BabyStart + 7 * frame);
    }

    internal static IReadOnlyList<IntroDiscoveryActorSpriteFrameDefinition> Frames { get; } = new FrameList();
    private sealed class FrameList : IReadOnlyList<IntroDiscoveryActorSpriteFrameDefinition>
    {
        public int Count => 20;
        public IntroDiscoveryActorSpriteFrameDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                if (index == 19) return new(BabyLarge, "hatched-baby", 19);
                if (index >= 16) return new(BabyFramePointer(index - 16), $"confused-baby-{index - 15}", 1);
                string name = index == 0 ? "egg-intact" : index == 8 ? "egg-hatched"
                    : index < 8 ? $"egg-crack-{index}" : $"egg-remnant-{index - 8}";
                return new(EggFramePointer(index), name, index == 0 ? 6 : index < 9 ? 9 : 3);
            }
        }
        public IEnumerator<IntroDiscoveryActorSpriteFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

internal readonly record struct IntroDiscoveryActorSpriteFrameDefinition(
    ushort Pointer, string Name, int StockPartCount);
