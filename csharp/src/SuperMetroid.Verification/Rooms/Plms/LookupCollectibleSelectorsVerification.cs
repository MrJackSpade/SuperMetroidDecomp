using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Plms.CollectibleVisualsVerification;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>Checks collectible PLM visual selectors and phase bounds against native draw operands.</summary>
internal static class LookupCollectibleSelectorsVerification
{
    /// <summary>Checks the four orb animation phases and rejects invalid phase indices.</summary>
    internal static void VerifyCollectibleOrbSelector(SuperMetroidAddressSpace rom)
    {
        for (int phase = 0; phase < 4; phase++)
            AssertEqual(ReadCollectibleVisualWord(rom, (ushort)(0xdfb5 + phase * 4)),
                PlmDrawAddressCodec.Of(RoomPlmCollectibleDrawDefinitions.OrbFrame(phase)), "Collectible native orb phase draw");
        foreach (int bad in new[] {int.MinValue,-1,4,5,int.MaxValue})
            AssertThrows<InvalidDataException>(() => RoomPlmCollectibleDrawDefinitions.OrbFrame(bad), "Collectible orb phase bounds");
    }

    /// <summary>Checks reveal and reverse conceal frames for shot collectibles.</summary>
    internal static void VerifyCollectibleRevealSelector(SuperMetroidAddressSpace rom)
    {
        for (int phase = 0; phase < 3; phase++)
        {
            AssertEqual(ReadCollectibleVisualWord(rom, (ushort)(0xe014 + phase * 4)),
                PlmDrawAddressCodec.Of(RoomPlmCollectibleDrawDefinitions.ShotRevealFrame(phase)), "Collectible native reveal draw");
            AssertEqual(ReadCollectibleVisualWord(rom, (ushort)(0xe024 + phase * 4)),
                PlmDrawAddressCodec.Of(RoomPlmCollectibleDrawDefinitions.ShotRevealFrame(2 - phase)), "Collectible native reconceal draw");
        }
        foreach (int bad in new[] {int.MinValue,-1,3,4,int.MaxValue})
            AssertThrows<InvalidDataException>(() => RoomPlmCollectibleDrawDefinitions.ShotRevealFrame(bad), "Collectible reveal phase bounds");
    }

    /// <summary>Checks four tank kinds against native frames regardless of unused dynamic graphics slots.</summary>
    internal static void VerifyCollectibleTankSelector(SuperMetroidAddressSpace rom)
    {
        // Native exposed tank instruction-list draw operands, independently addressed.
        ushort[] starts = [0xe0a7,0xe0cc,0xe0f1,0xe116];
        for (int kind = 0; kind < starts.Length; kind++)
        {
            for (int phase = 0; phase < 2; phase++)
                foreach (int unusedSlot in new[] {int.MinValue,-1,0,1,2,3,4,int.MaxValue})
                    AssertEqual(ReadCollectibleVisualWord(rom, (ushort)(starts[kind] + phase * 4)),
                        PlmDrawAddressCodec.Of(RoomPlmCollectibleDrawDefinitions.VisibleFrame((InWorldCollectibleKind)kind,phase,unusedSlot)),
                        "Collectible native tank draw independent of graphics slot");
            foreach (int bad in new[] {int.MinValue,-1,2,3,int.MaxValue})
                AssertThrows<InvalidDataException>(() => RoomPlmCollectibleDrawDefinitions.VisibleFrame(
                    (InWorldCollectibleKind)kind,bad,0), "Collectible tank phase bounds");
        }
    }

    /// <summary>Checks byte-backed dynamic collectible kinds, both phases, four graphics slots, and invalid bounds.</summary>
    internal static void VerifyCollectibleDynamicSelector(SuperMetroidAddressSpace rom)
    {
        // The byte-backed kind domain historically selects dynamic graphics for every value above the four tanks.
        for (int kind = 4; kind <= byte.MaxValue; kind++)
        {
            for (int phase = 0; phase < 2; phase++)
            {
                for (int slot = 0; slot < 4; slot++)
                    AssertEqual(ReadCollectibleVisualWord(rom, (ushort)((phase == 0 ? 0xe05f : 0xe077) + slot * 2)),
                        PlmDrawAddressCodec.Of(RoomPlmCollectibleDrawDefinitions.VisibleFrame((InWorldCollectibleKind)kind,phase,slot)),
                        "Collectible native allocated dynamic draw");
                foreach (int bad in new[] {int.MinValue,-1,4,5,int.MaxValue})
                    AssertThrows<InvalidDataException>(() => RoomPlmCollectibleDrawDefinitions.VisibleFrame(
                        (InWorldCollectibleKind)kind,phase,bad), "Collectible dynamic slot bounds");
            }
            foreach (int bad in new[] {int.MinValue,-1,2,3,int.MaxValue})
                AssertThrows<InvalidDataException>(() => RoomPlmCollectibleDrawDefinitions.VisibleFrame(
                    (InWorldCollectibleKind)kind,bad,0), "Collectible dynamic phase bounds");
        }
    }
}
