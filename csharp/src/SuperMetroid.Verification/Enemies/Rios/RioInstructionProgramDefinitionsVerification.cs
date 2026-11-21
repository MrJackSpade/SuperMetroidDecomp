using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Rios;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Spatial;

/// <summary>Checks the Rio's typed idle, swoop and cooldown animations.</summary>
internal static class RioInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail cartridge data for the Rio animation checks.</summary>
    internal static void VerifyRioInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyRioInstructionProgramDefinitions), () => VerifyRioInstructionProgramDefinitions(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>
    /// The real initializer starts idling with nothing installed; the idle program runs into the post-swoop idle and back;
    /// both one-shot swoop programs end by telling main AI the animation has finished; the second swoop phase loops; the
    /// program oracle matches bank $A2.
    /// </summary>
    internal static void VerifyRioInstructionProgramDefinitions(CartridgeImportAddressSpace rom)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace rio = enemies.Places.InUpdateOrder[0];
        rio.Bind(EnemyDefinitionId.Rio, rio.KeptDefinition);
        rio.Bind(rio.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        // The room spawn primes the instruction timer before the species initializer runs.
        rio.AnimationClock.StepOnNextUpdate();
        RioBehavior.InitializeRio(rio);

        // Idle shows its tucked wingbeat, then runs into the post-swoop hover's tucked wingbeat three times and its hanging
        // wingbeat twice, which return to idle (#2451, #2512).
        IEnumerable<ushort> idlePass = RioAnimationsConstants.Spritemaps(RioSegment.TuckedWingbeat,
            RioSegment.TuckedWingbeat, RioSegment.TuckedWingbeat, RioSegment.TuckedWingbeat, RioSegment.HangingWingbeat, RioSegment.HangingWingbeat);
        AssertEqual(EnemyInstructionRunner.ExpectedFrames([.. idlePass, RioAnimationsConstants.Spritemaps(RioSegment.TuckedWingbeat).First()], Enumerable.Repeat(4, 13)),
            EnemyInstructionRunner.ShownFrames(rio, 12 * 4 + 1),
            "idle runs into the post-swoop idle and returns");

        // Samus coming within the trigger distance sets the Rio swooping on its next update: its opening shows its five
        // poses, three updates each. The swoop's loop and cooldown, which its dive and landing lead into, are matched to
        // their native lists by the Rio's animation check.
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        samus.Position = FixedPosition<RoomPixels>.FromWhole(new(rio.Position.X.Whole, rio.Position.Y.Whole + 0x40));
        var swoop = new List<string>();
        for (int update = 0; update < 5 * 3; update++)
        {
            rio.RunUpdate(samus);
            if (rio.AnimationClock.ShowsNewFrame)
                swoop.Add($"{update}:{rio.SpritemapPointer:X4}");
        }
        AssertEqual(EnemyInstructionRunner.ExpectedFrames(RioAnimationsConstants.Spritemaps(RioSegment.SwoopOpening), Enumerable.Repeat(3, 5)),
            string.Join(",", swoop),
            "Rio's swoop opening shows its five poses");

        Suite(nameof(RioAnimationsVerification.VerifyRioAnimations), () => RioAnimationsVerification.VerifyRioAnimations(rom));
        Console.WriteLine(
            "Rio: the real initializer, idle fallthrough and return, and the swoop opening Samus sets off pass.");
    }

    /// <summary>Runs the Rio animation verification command.</summary>
    [VerificationCommand("--rio-instruction-mechanics")]
    internal static int RioInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyRioInstructionProgramDefinitions), () => VerifyRioInstructionProgramDefinitions());
        return 0;
    }
}
