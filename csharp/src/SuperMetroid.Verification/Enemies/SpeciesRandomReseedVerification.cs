using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Confirms each species reseeds the shared random numbers through its named reseed (#2374): the seed its production
/// placement or activation writes is the one the cartridge's routine loads, and the Sidehopper's placement draws once
/// after it.
/// </summary>
internal static class SpeciesRandomReseedVerification
{
    /// <summary>The cartridge place of the seed the Sidehopper placement routine loads (the operand at A3:AB0C).</summary>
    private const int HopperSeedOperand = 0xa3ab0d;

    /// <summary>The cartridge place of the seed the Polyp placement routine loads (the operand at A2:B588).</summary>
    private const int PolypSeedOperand = 0xa2b589;

    /// <summary>The cartridge place of the seed the Beetom placement routine loads (the operand at A8:B798).</summary>
    private const int BeetomSeedOperand = 0xa8b799;

    /// <summary>Runs every species' reseeding setup and compares the seed it wrote with the cartridge's.</summary>
    internal static void VerifySpeciesRandomReseeds(CartridgeImportAddressSpace rom)
    {
        int Word(int address) => CartridgeImportSource.Require(rom).ReadCartridgeWord(address);

        var shared = new SharedRandom();
        shared.Reseed(SpeciesRandomReseed.PolypPlacement);
        AssertEqual(Word(PolypSeedOperand), shared.Read(), "the shared random numbers read back a species' seed");

        var seeds = new List<int>();
        int draws = 0;
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().SeedRandom = value => seeds.Add(value);
        enemies.VerificationServices().NextRandom = () =>
        {
            draws++;
            return 0;
        };
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];

        place.Bind(EnemyDefinitionId.Sidehopper, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        place.SetSpawnSettings(new HopperSpawnSettings(HopperSurface.Floor));
        enemies.HoppersBehavior.InitializeHopper(place, place.HeldOccupant());
        AssertEqual(new ReseedCase([Word(HopperSeedOperand)], 1), new ReseedCase([.. seeds], draws),
            "a Sidehopper's placement reseeds with the cartridge's seed and draws once");

        seeds.Clear();
        draws = 0;
        place.Bind(EnemyDefinitionId.Polyp, place.KeptDefinition);
        place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        enemies.PolypBehavior.InitializePolyp(place);
        AssertEqual(new ReseedCase([Word(PolypSeedOperand)], 0), new ReseedCase([.. seeds], draws),
            "a Polyp's placement reseeds with the cartridge's seed");

        SamusState samus = NewSamus();
        seeds.Clear();
        place.Bind(EnemyDefinitionId.Beetom, place.KeptDefinition);
        enemies.BeetomBehavior.InitializeBeetom(place, samus, 0);
        AssertEqual(new ReseedCase([Word(BeetomSeedOperand)], 0), new ReseedCase([.. seeds], draws),
            "a Beetom's placement reseeds with the cartridge's seed");
        Console.WriteLine("  Species reseeds: Sidehopper, Polyp and Beetom write the cartridge's seeds.");
    }

    /// <summary>A fresh standing Samus for the species that read her.</summary>
    private static SamusState NewSamus() =>
        new(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new UnauthoredBoostCadence(),
            SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), InstalledFixtures.SamusArtwork());

    /// <summary>The seeds one setup wrote, in order, and the values it drew.</summary>
    private sealed record ReseedCase(int[] Seeds, int Draws)
    {
        /// <inheritdoc/>
        public bool Equals(ReseedCase? other) => other is not null && Seeds.SequenceEqual(other.Seeds) && Draws == other.Draws;

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Seeds.Length, Draws);

        /// <inheritdoc/>
        public override string ToString() => $"seeds [{string.Join(", ", Seeds)}], draws {Draws}";
    }
}
