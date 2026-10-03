using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCollectibleOrbSelector(SuperMetroidAddressSpace rom)
    {
        for (int phase = 0; phase < 4; phase++)
            AssertEqual(ReadCollectibleVisualWord(rom, (ushort)(0xdfb5 + phase * 4)),
                RoomPlmCollectibleDrawDefinitions.OrbFrame(phase), "Collectible native orb phase draw");
        foreach (int bad in new[] {int.MinValue,-1,4,5,int.MaxValue})
            AssertThrows<InvalidDataException>(() => RoomPlmCollectibleDrawDefinitions.OrbFrame(bad), "Collectible orb phase bounds");
    }

    private static void VerifyCollectibleRevealSelector(SuperMetroidAddressSpace rom)
    {
        for (int phase = 0; phase < 3; phase++)
        {
            AssertEqual(ReadCollectibleVisualWord(rom, (ushort)(0xe014 + phase * 4)),
                RoomPlmCollectibleDrawDefinitions.ShotRevealFrame(phase), "Collectible native reveal draw");
            AssertEqual(ReadCollectibleVisualWord(rom, (ushort)(0xe024 + phase * 4)),
                RoomPlmCollectibleDrawDefinitions.ShotRevealFrame(2 - phase), "Collectible native reconceal draw");
        }
        foreach (int bad in new[] {int.MinValue,-1,3,4,int.MaxValue})
            AssertThrows<InvalidDataException>(() => RoomPlmCollectibleDrawDefinitions.ShotRevealFrame(bad), "Collectible reveal phase bounds");
    }

    private static void VerifyCollectibleTankSelector(SuperMetroidAddressSpace rom)
    {
        // Native exposed tank instruction-list draw operands, independently addressed.
        ushort[] starts = [0xe0a7,0xe0cc,0xe0f1,0xe116];
        for (int kind = 0; kind < starts.Length; kind++)
        {
            for (int phase = 0; phase < 2; phase++)
                foreach (int unusedSlot in new[] {int.MinValue,-1,0,1,2,3,4,int.MaxValue})
                    AssertEqual(ReadCollectibleVisualWord(rom, (ushort)(starts[kind] + phase * 4)),
                        RoomPlmCollectibleDrawDefinitions.VisibleFrame((InWorldCollectibleKind)kind,phase,unusedSlot),
                        "Collectible native tank draw independent of graphics slot");
            foreach (int bad in new[] {int.MinValue,-1,2,3,int.MaxValue})
                AssertThrows<InvalidDataException>(() => RoomPlmCollectibleDrawDefinitions.VisibleFrame(
                    (InWorldCollectibleKind)kind,bad,0), "Collectible tank phase bounds");
        }
    }

    private static void VerifyCollectibleDynamicSelector(SuperMetroidAddressSpace rom)
    {
        // The byte-backed kind domain historically selects dynamic graphics for every value above the four tanks.
        for (int kind = 4; kind <= byte.MaxValue; kind++)
        {
            for (int phase = 0; phase < 2; phase++)
            {
                for (int slot = 0; slot < 4; slot++)
                    AssertEqual(ReadCollectibleVisualWord(rom, (ushort)((phase == 0 ? 0xe05f : 0xe077) + slot * 2)),
                        RoomPlmCollectibleDrawDefinitions.VisibleFrame((InWorldCollectibleKind)kind,phase,slot),
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