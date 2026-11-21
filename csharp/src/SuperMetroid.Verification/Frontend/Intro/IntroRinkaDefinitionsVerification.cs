using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;

/// <summary>Checks intro Rinka actor and motion tables through their two-wave spawn sequence.</summary>
internal static class IntroRinkaDefinitionsVerification
{
    /// <summary>Compares native definitions, animation, four spawns, and explosion cleanup without source rereads.</summary>
    internal static void VerifyIntroRinkaDefinitions()
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        // Native six-byte definition $8B:CF21 (Rinka); the spawner's at $8B:CF27 is the port's launch schedule. Its first
        // word, the initialization callback, has no port counterpart.
        IntroRinkaDefinitionsActor[] actors =
        [
            new IntroRinkaDefinitionsActor(IntroRinkaDefinitions.RinkaActor, 0xcf21),
        ];
        for (int index = 0; index < actors.Length; index++)
        {
            CinematicActorDefinition<IntroRinkaAnimation> actual = actors[index].Actor;
            int address = IntroRinkaDefinitions.NativeBank | actors[index].NativePointer;
            AssertEqual(retail.ReadCartridgeWord((address + 2)), CinematicActorNativeMotions.NoMotion,
                $"intro Rinka actor {index} routine: it holds still until its animation starts it moving");
            AssertEqual(retail.ReadCartridgeWord((address + 4)), CinematicSpriteNativeListAddresses.Start(actual.Animation),
                $"intro Rinka actor {index} instruction list");
        }

        IntroRinkaLaunch[] launches = [.. IntroRinkaDefinitions.StrikingPair, .. IntroRinkaDefinitions.MissingPair];
        AssertEqual(IntroRinkaDefinitions.RinkaCount, launches.Length, "the spawner launches four Rinkas");
        for (int launch = 0; launch < launches.Length; launch++)
        {
            IntroRinkaLaunch actual = launches[launch];
            AssertEqual(retail.ReadCartridgeWord((IntroRinkaDefinitions.InitialXReferenceAddress + launch * 2)),
                actual.Position.X, $"intro Rinka {launch} initial X");
            AssertEqual(unchecked((ushort)(retail.ReadCartridgeWord((IntroRinkaDefinitions.InitialYReferenceAddress + launch * 2)) - 8)),
                actual.Position.Y, $"intro Rinka {launch} biased initial Y");
            // The cartridge adds a half-pixel fraction to each Rinka's signed whole-pixel sideways word.
            AssertEqual(SignedSixteenSixteen.FromParts(unchecked((short)retail.ReadCartridgeWord((IntroRinkaDefinitions.XWholeVelocityReferenceAddress + launch * 2))), 0x8000),
                actual.Drift, $"intro Rinka {launch} sideways drift");
            AssertEqual(launch == 0 ? IntroRinkaFlight.StrikesSamus : IntroRinkaFlight.MissesSamus,
                actual.Flight, $"intro Rinka {launch} strikes Samus only when launched first");
        }

        var guarded = NewIntroRinkaDefinitionGuard(retail);
        var system = new IntroRinkaSystem();
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(0x0200, 0x0100)),
        };
        for (int frame = 0; frame < 220; frame++)
        {
            system.Step(samus, motherBrainExploding: false);
            int expectedCount = frame < 74 ? 0 : frame < 202 ? 2 : 4;
            AssertEqual(expectedCount, system.LiveRinkas.Count,
                $"intro Rinka two-wave spawn count at frame {frame}");
            // The spawner updates before the Rinkas, so each new Rinka steps in its spawn call (capture update 1793).
            if (frame >= 74)
            {
                int relative = frame - 74;
                ushort expectedSprite = relative < 30
                    ? (ushort)(0x8c8d + relative / 10 * 0x16)
                    : ((relative - 30) / 10 % 4) switch
                    {
                        0 or 2 => (ushort)0x8ca3,
                        1 => (ushort)0x8c8d,
                        _ => (ushort)0x8cb9,
                    };
                AssertEqual(expectedSprite, CinematicSpritePoseSpritemaps.Of(system.HittingRinka.Shown),
                    $"intro first Rinka selects its native frame at call {frame}");
            }
        }
        AssertEqual(IntroRinkaDefinitions.RinkaCount, system.LiveRinkas.Count,
            "intro Rinka spawner allocates both native waves");
        AssertEqual(IntroRinkaDefinitions.RinkaCount, system.ActiveCount,
            "all intro Rinkas remain active before Mother Brain explodes");

        for (int frame = 0; frame < 100; frame++)
            system.Step(samus, motherBrainExploding: true);
        AssertEqual(0, system.ActiveCount,
            "all intro Rinkas retire after Mother Brain begins exploding");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "intro Rinka production path never rereads compiled definitions, lists or physical tables");

        Console.WriteLine(
            "  Intro Rinka definitions: six actor, twelve physical and 48 instruction bytes match; both timed spawn waves, animation and cleanup are ROM-table independent.");
    }

    /// <summary>Forbids runtime access to migrated actor, instruction, and physical tables.</summary>
    private static CartridgeReadGuard NewIntroRinkaDefinitionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => address is >= 0x8bcf21 and < 0x8bcf2d or
                >= 0x8bcdeb and < 0x8bce1b or
                >= 0x8bb8b5 and < 0x8bb8c5 or
                >= 0x8bb985 and < 0x8bb98d,
            address => $"Intro Rinka reread compiled byte ${address:X6}.");

    /// <summary>Runs the intro Rinka definition and two-wave behavior comparison.</summary>
    [VerificationCommand("--intro-rinka-definitions")]
    internal static int IntroRinkaDefinitionsCommand()
    {
        Suite(nameof(VerifyIntroRinkaDefinitions), () => VerifyIntroRinkaDefinitions());
        return 0;
    }
}
