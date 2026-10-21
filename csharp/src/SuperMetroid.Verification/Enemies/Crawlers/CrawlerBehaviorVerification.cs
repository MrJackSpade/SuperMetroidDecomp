using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Crawlers;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks each crawler family's surface animations through the real initializers.</summary>
internal static class CrawlerBehaviorVerification
{
    /// <summary>
    /// For every crawler family and surface, the real initializer starts that surface's animation; its first step
    /// installs the movement function (vertical crawling on the side walls, horizontal on floors and ceilings, with the
    /// orange Zoomer's own functions) and its poses then loop on time. Every pose has installed artwork when supplied.
    /// </summary>
    internal static void VerifyCrawlerBehavior(InstalledEnemyArtwork? artwork = null)
    {
        CrawlerBehaviorFamily[] families =
        [
            new CrawlerBehaviorFamily(CrawlerAnimationFamily.Shared, EnemyDefinitionId.Zoomer, null),
            new CrawlerBehaviorFamily(CrawlerAnimationFamily.Viola, EnemyDefinitionId.Viola, 6),
            new CrawlerBehaviorFamily(CrawlerAnimationFamily.Sciser, EnemyDefinitionId.Sciser, 8),
            new CrawlerBehaviorFamily(CrawlerAnimationFamily.Zero, EnemyDefinitionId.Zero, 10),
            new CrawlerBehaviorFamily(CrawlerAnimationFamily.HZoomer, EnemyDefinitionId.HZoomer, null),
        ];
        foreach ((CrawlerAnimationFamily family, EnemyDefinitionId definition, ushort? speciesOffset) in families)
        foreach (CrawlerSurfaceOrientation orientation in Enum.GetValues<CrawlerSurfaceOrientation>())
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork?.Enemies, BossArtwork = artwork?.Bosses, AreaArtwork = artwork?.Areas };
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(definition, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            // The population parameter, read before initialization, selects the surface; InitAI_Viola reads the property bits.
            slot.CurrentInstruction = (ushort)orientation;
            if (family == CrawlerAnimationFamily.Viola)
                slot.Properties = EnemyPropertyWord.FromNativeWord((ushort)orientation);
            if (family == CrawlerAnimationFamily.HZoomer)
                CrawlersBehavior.InitializeHZoomer(slot);
            else
                CrawlersBehavior.InitializeCrawler(slot, family, speciesOffset);
            CrawlerEnemyState state = enemies.CrawlerStates[0] ?? throw new InvalidOperationException("The crawler has no state.");
            CrawlerProgram program = CrawlerAnimationDefinitions.InitialProgram(family, orientation);
            string context = $"{family} {orientation}";
            AssertEqual(AnimationProgramCursor<CrawlerProgram>.Start(program), state.Program, $"{context} starts its surface animation");

            CrawlerProgram loop = CrawlerAnimations.Programs.Steps(program)
                .Select(step => step switch
                {
                    AnimationProgramStep<CrawlerProgram, CrawlerFrame, CrawlerCue>.FallIntoStep fallInto => (CrawlerProgram?)fallInto.Target,
                    AnimationProgramStep<CrawlerProgram, CrawlerFrame, CrawlerCue>.GoToStep goTo => goTo.Target,
                    _ => null,
                })
                .Single(target => target is not null) ?? throw new InvalidOperationException("A crawler loop names its target.");
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(CrawlerAnimations.Programs, loop, CrawlerFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(CrawlerAnimations.Programs, loop)), $"{context} loop");
            bool sideWall = orientation is CrawlerSurfaceOrientation.UpsideRight or CrawlerSurfaceOrientation.UpsideLeft;
            CrawlerEnemyFunction expected = family == CrawlerAnimationFamily.HZoomer
                ? sideWall ? CrawlerEnemyFunction.HZoomerCrawlingVertically : CrawlerEnemyFunction.HZoomerCrawlingHorizontally
                : sideWall ? CrawlerEnemyFunction.CrawlingVertically : CrawlerEnemyFunction.CrawlingHorizontally;
            AssertEqual(expected, state.Function, $"{context} installs its movement function");
            if (artwork is not null)
                foreach (ushort pose in CrawlerAnimationsConstants.Spritemaps(loop))
                    AssertTrue((artwork.Enemies.Spritemaps ?? throw new InvalidOperationException("Verification requires installed spritemaps.")).TryGet(CrawlerFrameToolingDefinitions.Bank, pose, out _),
                        $"{context} pose ${pose:X4} has installed artwork");
        }
        Console.WriteLine("  Crawler behavior: all five families' four surfaces start, install their movement functions and loop on time.");
    }

    /// <summary>Runs the crawler behavior check and the program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--crawler-instruction-programs")]
    internal static int CrawlerBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCrawlerBehavior), () => VerifyCrawlerBehavior());
        Suite(nameof(CrawlerAnimationsVerification.VerifyCrawlerAnimations), () => CrawlerAnimationsVerification.VerifyCrawlerAnimations(rom));
        return 0;
    }
}
