using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.CollectibleVisualsVerification;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Checks collectible PLM visual selectors and phase bounds against native draw operands.</summary>
internal static class LookupCollectibleSelectorsVerification
{
    /// <summary>Every block pair a tank's draw is checked against, and none: a tank's frames are its kind's own blocks.</summary>
    private static readonly CollectibleBlockPairs.Pair?[] TankBlockPairs = [null, .. CollectibleBlockPairs.InTurn];

    /// <summary>Checks the orb animation's four images, in order, against the native orb draws.</summary>
    internal static void VerifyCollectibleOrbSelector(CartridgeImportAddressSpace rom)
    {
        IReadOnlyList<AnimationBeat<CollectibleDraw, NoAnimationCue>> images = CollectibleOrbAnimation.Segments.Beats(CollectibleOrbSegment.Pulsing);
        AssertEqual(4, images.Count, "the orb's animation shows four images");
        for (int phase = 0; phase < images.Count; phase++)
        {
            var image = AssertIs<AnimationBeat<CollectibleDraw, NoAnimationCue>.Frame>(images[phase], $"orb image {phase}");
            AssertEqual(ReadCollectibleDrawWord(rom, (ushort)(0xdfb5 + phase * 4)),
                RoomObjectDrawAddressCodec.Of(RoomObjectDraw.Of(image.Shown)), "Collectible native orb phase draw");
        }
        AssertEqual(AnimationEnd.LoopsFrom(0), CollectibleOrbAnimation.Pulsing.End, "the orb's animation starts again after its fourth image");
    }

    /// <summary>Checks reveal and reverse conceal frames for shot collectibles.</summary>
    internal static void VerifyCollectibleRevealSelector(CartridgeImportAddressSpace rom)
    {
        for (int phase = 0; phase < 3; phase++)
        {
            AssertEqual(ReadCollectibleDrawWord(rom, (ushort)(0xe014 + phase * 4)),
                RoomObjectDrawAddressCodec.Of(CollectibleObjectDraws.ShotRevealFrame(phase)), "Collectible native reveal draw");
            AssertEqual(ReadCollectibleDrawWord(rom, (ushort)(0xe024 + phase * 4)),
                RoomObjectDrawAddressCodec.Of(CollectibleObjectDraws.ShotRevealFrame(2 - phase)), "Collectible native reconceal draw");
        }
        foreach (int bad in new[] {int.MinValue,-1,3,4,int.MaxValue})
            AssertThrows<InvalidDataException>(() => CollectibleObjectDraws.ShotRevealFrame(bad), "Collectible reveal phase bounds");
    }

    /// <summary>Checks four tank kinds against native frames regardless of the unused block pair.</summary>
    internal static void VerifyCollectibleTankSelector(CartridgeImportAddressSpace rom)
    {
        // Native exposed tank instruction-list draw operands, independently addressed.
        ushort[] starts = [0xe0a7,0xe0cc,0xe0f1,0xe116];
        for (int kind = 0; kind < starts.Length; kind++)
        {
            for (int phase = 0; phase < 2; phase++)
                foreach (CollectibleBlockPairs.Pair? unusedPair in TankBlockPairs)
                    AssertEqual(ReadCollectibleDrawWord(rom, (ushort)(starts[kind] + phase * 4)),
                        RoomObjectDrawAddressCodec.Of(CollectibleObjectDraws.VisibleFrame((InWorldCollectibleKind)kind,phase,unusedPair)),
                        "Collectible native tank draw independent of block pair");
            foreach (int bad in new[] {int.MinValue,-1,2,3,int.MaxValue})
                AssertThrows<InvalidDataException>(() => CollectibleObjectDraws.VisibleFrame(
                    (InWorldCollectibleKind)kind,bad,CollectibleBlockPairs.Pair.First), "Collectible tank phase bounds");
        }
    }

    /// <summary>Checks byte-backed dynamic collectible kinds, both phases, the four block pairs, and invalid bounds.</summary>
    internal static void VerifyCollectibleDynamicSelector(CartridgeImportAddressSpace rom)
    {
        // The byte-backed kind domain historically selects dynamic graphics for every value above the four tanks.
        for (int kind = 4; kind <= byte.MaxValue; kind++)
        {
            for (int phase = 0; phase < 2; phase++)
            {
                // The pairs in turn select the table's selectors in order.
                for (int selector = 0; selector < CollectibleBlockPairs.InTurn.Count; selector++)
                    AssertEqual(ReadCollectibleDrawWord(rom, (ushort)((phase == 0 ? 0xe05f : 0xe077) + selector * 2)),
                        RoomObjectDrawAddressCodec.Of(CollectibleObjectDraws.VisibleFrame((InWorldCollectibleKind)kind,phase,CollectibleBlockPairs.InTurn[selector])),
                        "Collectible native block pair draw");
                // A typed pair cannot lie outside the four, so the bound left is an item that took no pair at all.
                AssertThrows<InvalidDataException>(() => CollectibleObjectDraws.VisibleFrame(
                    (InWorldCollectibleKind)kind,phase,null), "Collectible item without a block pair");
            }
            foreach (int bad in new[] {int.MinValue,-1,2,3,int.MaxValue})
                AssertThrows<InvalidDataException>(() => CollectibleObjectDraws.VisibleFrame(
                    (InWorldCollectibleKind)kind,bad,CollectibleBlockPairs.Pair.First), "Collectible dynamic phase bounds");
        }
    }
}
