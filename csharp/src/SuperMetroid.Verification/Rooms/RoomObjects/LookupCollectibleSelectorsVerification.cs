using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.CollectibleVisualsVerification;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.Artwork;
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

    /// <summary>
    /// Checks the shot block's reveal and its forming again over the item against the native draws (#1994): the reveal's
    /// three images, then the hiding's two before the whole block it shows as the saved block returns.
    /// </summary>
    internal static void VerifyCollectibleRevealSelector(CartridgeImportAddressSpace rom)
    {
        IReadOnlyList<RoomObjectDraw> reveal = SequenceDraws(CollectibleSequence.ShotBlockReveal);
        IReadOnlyList<RoomObjectDraw> conceal = [.. SequenceDraws(CollectibleSequence.ShotBlockConceal), RoomObjectCollectibleDrawDefinitions.ShotBlockIntact];
        AssertEqual(3, reveal.Count, "the reveal shows three images");
        AssertEqual(3, conceal.Count, "the hiding shows two images before the whole block");
        for (int phase = 0; phase < 3; phase++)
        {
            AssertEqual(ReadCollectibleDrawWord(rom, (ushort)(0xe014 + phase * 4)),
                RoomObjectDrawAddressCodec.Of(reveal[phase]), "Collectible native reveal draw");
            AssertEqual(ReadCollectibleDrawWord(rom, (ushort)(0xe024 + phase * 4)),
                RoomObjectDrawAddressCodec.Of(conceal[phase]), "Collectible native reconceal draw");
        }
    }

    /// <summary>The draws of <paramref name="sequence"/>'s images, in order.</summary>
    private static IReadOnlyList<RoomObjectDraw> SequenceDraws(CollectibleSequence sequence) =>
        [.. CollectibleSequenceAnimations.Segments.Beats(sequence).Select((beat, index) =>
            AssertIs<AnimationBeat<CollectibleImage, NoAnimationCue>.Frame>(beat, $"{sequence} image {index}").Shown.Draw)];

    /// <summary>Checks four tank kinds against native frames regardless of the unused block pair.</summary>
    internal static void VerifyCollectibleTankSelector(CartridgeImportAddressSpace rom)
    {
        // Native exposed tank instruction-list draw operands, independently addressed.
        ushort[] starts = [0xe0a7,0xe0cc,0xe0f1,0xe116];
        for (int kind = 0; kind < starts.Length; kind++)
        {
            foreach (CollectibleLook look in Enum.GetValues<CollectibleLook>())
                foreach (CollectibleBlockPairs.Pair? unusedPair in TankBlockPairs)
                    AssertEqual(ReadCollectibleDrawWord(rom, (ushort)(starts[kind] + (look == CollectibleLook.Primary ? 0 : 4))),
                        RoomObjectDrawAddressCodec.Of(CollectibleObjectDraws.VisibleFrame((InWorldCollectibleKind)kind,look,unusedPair)),
                        "Collectible native tank draw independent of block pair");
        }
    }

    /// <summary>Checks byte-backed dynamic collectible kinds, both looks, the four block pairs, and an item without a pair.</summary>
    internal static void VerifyCollectibleDynamicSelector(CartridgeImportAddressSpace rom)
    {
        // The byte-backed kind domain historically selects dynamic graphics for every value above the four tanks.
        for (int kind = 4; kind <= byte.MaxValue; kind++)
        {
            foreach (CollectibleLook look in Enum.GetValues<CollectibleLook>())
            {
                // The pairs in turn select the table's selectors in order.
                for (int selector = 0; selector < CollectibleBlockPairs.InTurn.Count; selector++)
                    AssertEqual(ReadCollectibleDrawWord(rom, (ushort)((look == CollectibleLook.Primary ? 0xe05f : 0xe077) + selector * 2)),
                        RoomObjectDrawAddressCodec.Of(CollectibleObjectDraws.VisibleFrame((InWorldCollectibleKind)kind,look,CollectibleBlockPairs.InTurn[selector])),
                        "Collectible native block pair draw");
                // A typed pair cannot lie outside the four, so the bound left is an item that took no pair at all.
                AssertThrows<InvalidDataException>(() => CollectibleObjectDraws.VisibleFrame(
                    (InWorldCollectibleKind)kind,look,null), "Collectible item without a block pair");
            }
        }
    }
}
