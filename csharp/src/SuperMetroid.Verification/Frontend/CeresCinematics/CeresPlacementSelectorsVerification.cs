using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks the import's Ceres actor placement sources against immediate operands in bank $8B.</summary>
internal static class CeresPlacementSelectorsVerification
{
    /// <summary>Checks the five rear-view actors' X and Y native placement immediates.</summary>
    internal static void VerifyCeresRearPlacementSelector(ISnesAddressSpace rom)
    {
        // Original maintained extraction table at d12fa8fa; IDs are asset-format identities,
        // not ROM strings. Native initializer operands independently confirm their source views.
        CinematicPlacementSourceDefinition[] original =
        [
            new("large-asteroid", 0xbf23),
            new("station-under-attack", 0xbf4d),
            new("small-asteroid", 0xbf77),
            new("vortex", 0xbfb4),
            new("rear-stars", 0xbea3),
        ];
        AssertEqual(original.Length, CeresFlightActorDefinitions.RearViewActorCount, "rear placement count");
        for (int i = 0; i < original.Length; i++)
        {
            var actual = CeresCinematicSourceDefinitions.RearViewPlacements[i];
            AssertEqual(original[i], actual, "the import reads the original rear placement operands under the Core actor id");
            VerifyPlacementOperand(rom, actual.FirstOperand);
            VerifyPlacementOperand(rom, actual.SecondOperand);
        }
        foreach (int i in new[] { int.MinValue, -1, original.Length, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => CeresFlightActors.RearViewPlacementId(i), "rear selector bounds");
    }

    /// <summary>Checks the six Zebes reveal actors' and the Japanese subtitle's X and Y native placement immediates.</summary>
    internal static void VerifyCeresRevealPlacementSelector(ISnesAddressSpace rom)
    {
        CinematicPlacementSourceDefinition[] original =
        [
            new("planet", 0xc83c),
            new("stars-upper-left", 0xc944),
            new("stars-upper-right", 0xc958),
            new("stars-lower-left", 0xc96c),
            new("stars-lower-right", 0xc980),
            new("planet-zebes-title", 0xc993),
            new("planet-zebes-subtitle", 0xc9e7),
        ];
        AssertEqual(original.Length, CeresDestructionActorDefinitions.RevealPlacementCount, "reveal placement count");
        for (int i = 0; i < original.Length; i++)
        {
            var actual = CeresCinematicSourceDefinitions.ZebesPlacements[i];
            AssertEqual(original[i], actual, "the import reads the original reveal placement operands under the Core actor id");
            VerifyPlacementOperand(rom, actual.FirstOperand);
            VerifyPlacementOperand(rom, actual.SecondOperand);
        }
        foreach (int i in new[] { int.MinValue, -1, original.Length, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => CeresDestructionActors.ZebesPlacementId(i), "reveal selector bounds");
    }

    /// <summary>Checks the three initial destruction actors' ids and the import's X and Y native placement immediates.</summary>
    internal static void VerifyCeresInitialPlacementSelector(ISnesAddressSpace rom)
    {
        CinematicPlacementImmediates[] original =
        [
            new("large-asteroid", 0xbf23, 0xbf29),
            new("small-asteroid", 0xbf77, 0xbf7d),
            new("vortex", 0xbfa6, 0xbfba),
        ];
        AssertEqual(original.Length, CeresDestructionActorDefinitions.InitialActorCount, "initial placement count");
        for (int i = 0; i < original.Length; i++)
        {
            AssertEqual(original[i].Id, CeresDestructionActors.InitialPlacementId(i), "original initial placement selector");
            CinematicPlacementImmediates actual = CeresCinematicSourceDefinitions.DestructionPlacements[i];
            AssertEqual(original[i], actual, "the import reads the original initial placement operands under the Core actor id");
            VerifyPlacementOperand(rom, actual.FirstOperand);
            VerifyPlacementOperand(rom, actual.SecondOperand);
        }
        foreach (int i in new[] { int.MinValue, -1, original.Length, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => CeresDestructionActors.InitialPlacementId(i), "initial selector bounds");
    }

    /// <summary>Requires an immediate LDA before the coordinate the import reads.</summary>
    private static void VerifyPlacementOperand(ISnesAddressSpace rom, int operand)
    {
        int address = 0x8b0000 | operand;
        AssertEqual((byte)0xa9, rom.ReadCartridgeByte(address - 1), "placement immediate LDA");
    }
}
