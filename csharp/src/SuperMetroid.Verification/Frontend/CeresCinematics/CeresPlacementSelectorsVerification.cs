using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.CeresCinematics;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks compiled Ceres actor placements against immediate operands in bank $8B.</summary>
internal static class CeresPlacementSelectorsVerification
{
    /// <summary>Checks the five rear-view actors and their X and Y native placement words.</summary>
    internal static void VerifyCeresRearPlacementSelector(ISnesAddressSpace rom)
    {
        // Original maintained extraction table at d12fa8fa; IDs are asset-format identities,
        // not ROM strings. Native initializer operands independently confirm their source views.
        (string Id, ushort XAddress, ushort YAddress)[] original =
        [
            ("large-asteroid", 0xbf23, 0xbf29),
            ("station-under-attack", 0xbf4d, 0xbf53),
            ("small-asteroid", 0xbf77, 0xbf7d),
            ("vortex", 0xbfb4, 0xbfba),
            ("rear-stars", 0xbea3, 0xbea9),
        ];
        AssertEqual(original.Length, CeresFlightActorDefinitions.RearViewActorCount, "rear placement count");
        for (int i = 0; i < original.Length; i++)
        {
            var actual = CeresFlightActorDefinitions.RearViewPlacementSource(i);
            AssertEqual(original[i], actual, "original rear placement selector");
            var actor = CeresFlightActorDefinitions.RearViewActor(i);
            VerifyPlacementOperand(rom, actual.XAddress, actor.Position.X);
            VerifyPlacementOperand(rom, actual.YAddress, actor.Position.Y);
        }
        foreach (int i in new[] { int.MinValue, -1, original.Length, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => CeresFlightActorDefinitions.RearViewPlacementSource(i), "rear selector bounds");
    }

    /// <summary>Checks six Zebes reveal actors and their X and Y native placement words.</summary>
    internal static void VerifyCeresRevealPlacementSelector(ISnesAddressSpace rom)
    {
        (string Id, ushort XAddress, ushort YAddress)[] original =
        [
            ("planet", 0xc83c, 0xc842),
            ("stars-upper-left", 0xc944, 0xc94a),
            ("stars-upper-right", 0xc958, 0xc95e),
            ("stars-lower-left", 0xc96c, 0xc972),
            ("stars-lower-right", 0xc980, 0xc986),
            ("planet-zebes-title", 0xc993, 0xc999),
        ];
        AssertEqual(original.Length, CeresDestructionActorDefinitions.ZebesActorCount, "reveal placement count");
        for (int i = 0; i < original.Length; i++)
        {
            var actual = CeresDestructionActorDefinitions.ZebesPlacementSource(i);
            AssertEqual(original[i], actual, "original reveal placement selector");
            var actor = CeresDestructionActorDefinitions.ZebesActor(i);
            VerifyPlacementOperand(rom, actual.XAddress, actor.Position.X);
            VerifyPlacementOperand(rom, actual.YAddress, actor.Position.Y);
        }
        foreach (int i in new[] { int.MinValue, -1, original.Length, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => CeresDestructionActorDefinitions.ZebesPlacementSource(i), "reveal selector bounds");
    }

    /// <summary>Checks the three initial destruction actors before the reveal phase.</summary>
    internal static void VerifyCeresInitialPlacementSelector()
    {
        string[] original = ["large-asteroid", "small-asteroid", "vortex"];
        AssertEqual(original.Length, CeresDestructionActorDefinitions.InitialActorCount, "initial placement count");
        for (int i = 0; i < original.Length; i++)
            AssertEqual(original[i], CeresDestructionActorDefinitions.InitialPlacementId(i), "original initial placement selector");
        foreach (int i in new[] { int.MinValue, -1, original.Length, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => CeresDestructionActorDefinitions.InitialPlacementId(i), "initial selector bounds");
    }

    /// <summary>Requires an immediate LDA before the coordinate and compares its retail word.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres placement operands")]
    private static void VerifyPlacementOperand(ISnesAddressSpace rom, ushort operand, ushort coordinate)
    {
        int address = 0x8b0000 | operand;
        AssertEqual((byte)0xa9, rom.ReadCartridgeByte(address - 1), "placement immediate LDA");
        AssertEqual(coordinate, CartridgeImportSource.Require(rom).ReadCartridgeWord(address), "native placement operand");
    }
}
