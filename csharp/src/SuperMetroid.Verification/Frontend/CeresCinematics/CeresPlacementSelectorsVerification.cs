using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks the import's Ceres actor placement sources against immediate operands in bank $8B.</summary>
internal static class CeresPlacementSelectorsVerification
{
    /// <summary>Checks the five rear-view actors' roles and X and Y native placement immediates.</summary>
    internal static void VerifyCeresRearPlacementSelector(ISnesAddressSpace rom)
    {
        // Original maintained extraction table at d12fa8fa, now keyed by role (#1768). Native initializer operands
        // independently confirm their source views.
        CinematicPlacementSourceDefinition<CeresRearViewActorRole>[] original =
        [
            new(CeresRearViewActorRole.LargeAsteroids, 0xbf23),
            new(CeresRearViewActorRole.StationUnderAttack, 0xbf4d),
            new(CeresRearViewActorRole.SmallAsteroids, 0xbf77),
            new(CeresRearViewActorRole.Vortex, 0xbfb4),
            new(CeresRearViewActorRole.RearStars, 0xbea3),
        ];
        AssertSequenceEqual(original, CeresCinematicSourceDefinitions.RearViewPlacements,
            "the import reads the original rear placement operands under each actor's role");
        AssertSequenceEqual(CeresFlightActorDefinitions.RearViewSpawnOrder, original.Select(source => source.Role),
            "the import writes the rear placements in spawn order");
        foreach (var actual in CeresCinematicSourceDefinitions.RearViewPlacements)
        {
            VerifyPlacementOperand(rom, actual.FirstOperand);
            VerifyPlacementOperand(rom, actual.SecondOperand);
        }
    }

    /// <summary>Checks the six Zebes reveal actors' and the Japanese subtitle's roles and X and Y native placement immediates.</summary>
    internal static void VerifyCeresRevealPlacementSelector(ISnesAddressSpace rom)
    {
        CinematicPlacementSourceDefinition<ZebesRevealActorRole>[] original =
        [
            new(ZebesRevealActorRole.Planet, 0xc83c),
            new(ZebesRevealActorRole.StarsUpperLeft, 0xc944),
            new(ZebesRevealActorRole.StarsUpperRight, 0xc958),
            new(ZebesRevealActorRole.StarsLowerLeft, 0xc96c),
            new(ZebesRevealActorRole.StarsLowerRight, 0xc980),
            new(ZebesRevealActorRole.Title, 0xc993),
            new(ZebesRevealActorRole.JapaneseSubtitle, 0xc9e7),
        ];
        AssertSequenceEqual(original, CeresCinematicSourceDefinitions.ZebesPlacements,
            "the import reads the original reveal placement operands under each actor's role");
        AssertSequenceEqual(CeresDestructionActorDefinitions.RevealSpawnOrder.Append(ZebesRevealActorRole.JapaneseSubtitle),
            original.Select(source => source.Role), "the import writes the reveal placements in spawn order, then the subtitle's");
        foreach (var actual in CeresCinematicSourceDefinitions.ZebesPlacements)
        {
            VerifyPlacementOperand(rom, actual.FirstOperand);
            VerifyPlacementOperand(rom, actual.SecondOperand);
        }
    }

    /// <summary>Checks the three initial destruction actors' roles and the import's X and Y native placement immediates.</summary>
    internal static void VerifyCeresInitialPlacementSelector(ISnesAddressSpace rom)
    {
        CinematicPlacementImmediates<CeresDestructionActorRole>[] original =
        [
            new(CeresDestructionActorRole.LargeAsteroids, 0xbf23, 0xbf29),
            new(CeresDestructionActorRole.SmallAsteroids, 0xbf77, 0xbf7d),
            new(CeresDestructionActorRole.Vortex, 0xbfa6, 0xbfba),
        ];
        AssertSequenceEqual(original, CeresCinematicSourceDefinitions.DestructionPlacements,
            "the import reads the original initial placement operands under each actor's role");
        AssertSequenceEqual(CeresDestructionActorDefinitions.DestructionSpawnOrder, original.Select(source => source.Role),
            "the import writes the initial placements in spawn order");
        foreach (var actual in CeresCinematicSourceDefinitions.DestructionPlacements)
        {
            VerifyPlacementOperand(rom, actual.FirstOperand);
            VerifyPlacementOperand(rom, actual.SecondOperand);
        }
    }

    /// <summary>Requires an immediate LDA before the coordinate the import reads.</summary>
    private static void VerifyPlacementOperand(ISnesAddressSpace rom, int operand)
    {
        int address = 0x8b0000 | operand;
        AssertEqual((byte)0xa9, rom.ReadCartridgeByte(address - 1), "placement immediate LDA");
    }
}
