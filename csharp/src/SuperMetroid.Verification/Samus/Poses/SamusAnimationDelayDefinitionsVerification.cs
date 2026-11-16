using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Samus.Movement.RunningCadenceVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Poses;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Samus's typed pose animation programs against the cartridge's (#1473).</summary>
internal static class SamusAnimationDelayDefinitionsVerification
{
    /// <summary>
    /// Decodes every pose record's program from the cartridge and requires the typed animation the record plays, its pose's
    /// or, for a record no pose names, the one a read past an earlier animation reaches (#2048), to hold the same runs: each
    /// run's first frame, its delays and the instruction that ends it.
    /// </summary>
    internal static void VerifySamusAnimationDelayDefinitions(ISnesAddressSpace bus)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        var checkedAnimations = new HashSet<SamusAnimationName>();
        for (int number = 0; number < SamusAnimationProgramCodec.PoseCount; number++)
        {
            if (SamusAnimationProgramCodec.Animation(number) is not { } actual) continue;
            string pose = SamusPoseNumbering.NamesPose(number) ? SamusPoseNumbering.FromNumber(number).ToString() : $"record {number} ({actual.Name})";
            checkedAnimations.Add(actual.Name);
            SamusPoseAnimationStep? StepNumbered(int number) => actual.TryStepAt(SamusAnimationNumbers.Frame(actual, number));
            int nextFrame = 0;
            foreach (SamusAnimationProgramRun run in SamusAnimationProgramCodec.Decode(cartridge, number))
            {
                // Frame numbers between two runs name no step: they held the previous instruction's operands.
                for (; nextFrame < run.FirstFrame; nextFrame++)
                {
                    int gap = nextFrame;
                    AssertTrue(StepNumbered(gap) is null, $"pose {pose} frame {gap} is no step");
                }
                for (int index = 0; index < run.Delays.Count; index++, nextFrame++)
                    AssertEqual<SamusPoseAnimationStep?>(new SamusAnimationDelay(run.Delays[index]), StepNumbered(nextFrame),
                        $"pose {pose} frame {nextFrame} delay");
                AssertEqual<SamusPoseAnimationStep?>(run.End, StepNumbered(nextFrame), $"pose {pose} frame {nextFrame} instruction");
                nextFrame++;
            }
            int past = nextFrame;
            AssertTrue(StepNumbered(past) is null, $"pose {pose} has no step past its program");
        }
        AssertEqual(Enum.GetValues<SamusAnimationName>().Length, checkedAnimations.Count, "every named animation is a cartridge program");

        // The three numbers past the pose records name no program: no gameplay path gives Samus one of them.
        foreach (SamusPoseId adjacent in new[] { SamusPoseId.TallStillRightPose, SamusPoseId.ForwardStandingPose, SamusPoseId.ForwardStillCrouchHeightPose })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusAnimationDelays.AnimationForPose(adjacent),
                $"pose {adjacent} has no animation program");

        VerifyFramesBeyondAnimation(cartridge);

        var flashbackSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Pose = SamusPoseId.FacingLeftNormalPose,
        };
        SamusPresentation.InitializeAnimation(flashbackSamus);
        AssertEqual(SamusPoseId.FacingLeftNormalPose, flashbackSamus.AnimatedPose,
            "Mother Brain flashback pose starts its typed animation program");
        Console.WriteLine("Samus pose animation: every record's typed animation matches the cartridge's runs, delays and instructions.");
    }

    /// <summary>
    /// Requires every named read past an animation the import decodes to reach the frame the cartridge's layout puts there:
    /// a later program, at the frame lying as far before the read frame as that program starts after the read animation's
    /// (#2048), and every named place to land, on every other animation, on the frame its native number names there. The
    /// Mother Brain glitch movie's update 1947 is the recorded case: the right-facing drained standing pose at native frame
    /// 11 plays the left-facing program's native frame 3, which the installation holds (#1473).
    /// </summary>
    private static void VerifyFramesBeyondAnimation(IImportCartridgeSource cartridge)
    {
        var starts = new Dictionary<SamusAnimationName, int>();
        for (int number = 0; number < SamusAnimationProgramCodec.PoseCount; number++)
        {
            if (SamusAnimationProgramCodec.Animation(number) is { } recorded)
                starts.TryAdd(recorded.Name, SamusAnimationProgramCodec.ProgramStart(cartridge, number));
        }
        int reads = 0;
        foreach ((string animationName, IReadOnlyDictionary<string, SamusOverreadDocument.ReadPast> frames) in SamusAnimationLayout.Read(cartridge).ReadsPast())
        {
            SamusAnimationName name = Enum.Parse<SamusAnimationName>(animationName);
            SamusPoseAnimation animation = SamusAnimationDelays.AnimationNamed(name);
            foreach ((string frameName, SamusOverreadDocument.ReadPast read) in frames)
            {
                SamusAnimationName reached = Enum.Parse<SamusAnimationName>(read.Animation);
                SamusPoseAnimation readAnimation = SamusAnimationDelays.AnimationNamed(reached);
                int shift = starts[reached] - starts[name];
                AssertTrue(shift > 0, $"the {name} animation at {frameName} reads a program laid out after its own");
                AssertEqual(SamusAnimationNumbers.Number(animation, animation.FrameNamed(frameName)) - shift,
                    SamusAnimationNumbers.Number(readAnimation, readAnimation.FrameNamed(read.Frame)),
                    $"the {name} animation at {frameName} reads the frame the cartridge lays out there");
                reads++;
            }
        }
        AssertTrue(reads > 0, "the import decodes reads past Samus's animations");
        // A place named for one animation lands, on any other, on the frame its native number names there.
        Dictionary<string, SamusAnimationPlace> places = SamusAnimationPlaces.All.ToDictionary(place => place.Name.ToString());
        foreach ((string placeName, IReadOnlyDictionary<string, string> landings) in SamusAnimationLayout.Read(cartridge).PlaceLandings())
        {
            SamusAnimationPlace place = places[placeName];
            foreach ((string animationName, string frameName) in landings)
            {
                SamusPoseAnimation animation = SamusAnimationDelays.AnimationNamed(Enum.Parse<SamusAnimationName>(animationName));
                AssertEqual(SamusAnimationNumbers.Number(place.Animation, place.Frame), SamusAnimationNumbers.Number(animation, animation.FrameNamed(frameName)),
                    $"place {placeName} lands in the {animationName} animation on the frame its native number names");
            }
        }
        var drained = InstalledFixtures.FramesBeyondAnimation.ReadPast(SamusAnimationName.DrainedStandingRight,
                SamusAnimationNumbers.Frame(SamusPoseId.DrainedStandingRightPose, 11))
            ?? throw new InvalidOperationException("The right-facing drained standing pose reads no program past its own.");
        AssertEqual(SamusAnimationName.DrainedStandingLeft, drained.Animation, "drained standing right reads into drained standing left");
        AssertEqual(SamusAnimationNumbers.Frame(SamusPoseId.DrainedStandingLeftPose, 3), drained.Frame,
            "Mother Brain glitch movie update 1947 plays the left program's frame 3");
    }

    /// <summary>Runs pose-program and running-cadence checks against the retail ROM.</summary>
    [VerificationCommand("--mutable-animation-aliases")]
    internal static int MutableAnimationAliasesCommand()
    {
        string sourceRom = Path.GetFullPath("Super Metroid.smc");
        CartridgeImportAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        Suite(nameof(VerifySamusAnimationDelayDefinitions), () => VerifySamusAnimationDelayDefinitions(bus));
        Suite(nameof(VerifyRunningCadence), () => VerifyRunningCadence(bus));
        return 0;
    }
}
