using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Samus.Movement.RunningCadenceVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
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
            int namedRuns = 0;
            var unnamedRuns = new List<SamusAnimationUnnamedRun>();
            foreach (SamusAnimationProgramRun run in SamusAnimationProgramCodec.Decode(cartridge, number))
            {
                // Frame numbers between two runs name no step: they held the previous instruction's operands.
                for (; nextFrame < run.FirstFrame; nextFrame++)
                {
                    int gap = nextFrame;
                    AssertTrue(StepNumbered(gap) is null, $"pose {pose} frame {gap} is no step");
                }
                SamusAnimationFrame first = SamusAnimationNumbers.Frame(actual, run.FirstFrame);
                if (first.Frame != 0)
                {
                    // A run that starts no named segment lies past the previous segment's cue: the import's unnamed run,
                    // whose frames name no step of the animation (#2395).
                    for (int frame = run.FirstFrame; frame <= run.FirstFrame + run.Delays.Count; frame++)
                        AssertTrue(StepNumbered(frame) is null, $"pose {pose} frame {frame} of an unnamed run is no step");
                    unnamedRuns.Add(new SamusAnimationUnnamedRun(actual.Name, first.Segment, run.Delays.Count, run.End));
                    nextFrame = run.FirstFrame + run.Delays.Count + 1;
                    continue;
                }
                namedRuns++;
                for (int index = 0; index < run.Delays.Count; index++, nextFrame++)
                    AssertEqual<SamusPoseAnimationStep?>(new SamusAnimationDelay(run.Delays[index]), StepNumbered(nextFrame),
                        $"pose {pose} frame {nextFrame} delay");
                AssertEqual<SamusPoseAnimationStep?>(run.End, StepNumbered(nextFrame), $"pose {pose} frame {nextFrame} instruction");
                nextFrame++;
            }
            // A lead and the segment it plays on into are one run of the cartridge's (#2530).
            AssertEqual(actual.Segments.Count(segment => segment.End is not null), namedRuns, $"pose {pose}: every named segment ending in a cue closes one of the cartridge's runs");
            AssertSequenceEqual(SamusAnimationUnnamedRunDefinitions.All.Where(run => run.Animation == actual.Name).ToArray(), unnamedRuns.ToArray(),
                $"pose {pose}: the import's unnamed runs are the cartridge's runs no segment names");
            int past = nextFrame;
            AssertTrue(StepNumbered(past) is null, $"pose {pose} has no step past its program");
        }
        AssertEqual(Enum.GetValues<SamusAnimationName>().Length, checkedAnimations.Count, "every named animation is a cartridge program");

        // Every repeat plays a named segment again from its first frame; the frames an animation shows once before it are a
        // lead of their own (#2530). The frame after the start is the one the cartridge numbers one, across a lead too.
        foreach (SamusPoseAnimation animation in SamusAnimationDelayDefinitions.All)
        {
            foreach (SamusAnimationSegment segment in animation.Segments)
            {
                if (segment.End is SamusAnimationRepeat repeat)
                    AssertEqual(0, repeat.Target.Frame, $"the {animation.Name} animation's {segment.Name} repeat plays {repeat.Target.Segment} from its first frame");
            }
            AssertEqual(1, SamusAnimationNumbers.Number(animation, animation.AfterStart), $"the {animation.Name} animation's frame after its start");
        }

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

        // Every frame after a segment's cue goes on to the frame the cartridge numbers next, whether the next value the cue
        // carries, an unnamed run's frame or the next segment's first frame (#2395).
        int stepsPastCues = 0;
        foreach (SamusPoseAnimation animation in SamusAnimationDelayDefinitions.All)
        {
            for (int index = 0; index < animation.Segments.Count - 1; index++)
            {
                SamusAnimationSegment segment = animation.Segments[index];
                if (segment.End is null)
                    continue;
                SamusAnimationFrame frame = new(segment.Name, segment.CueFrame);
                SamusAnimationFrame nextStart = new(animation.Segments[index + 1].Name, 0);
                while (frame != nextStart)
                {
                    SamusAnimationFrame after = animation.Next(frame, InstalledFixtures.FramesBeyondAnimation);
                    AssertEqual(SamusAnimationNumbers.Number(animation, frame) + 1, SamusAnimationNumbers.Number(animation, after),
                        $"the {animation.Name} animation goes on from {animation.NameOf(frame)} to the frame the cartridge numbers next");
                    frame = after;
                    stepsPastCues++;
                }
            }
        }
        AssertTrue(stepsPastCues > 0, "Samus's animations go on past their segments' cues");
        AssertEqual(new SamusAnimationFrame(SamusAnimationSegmentName.RecoilHold, 0),
            SamusAnimationDelayDefinitions.KnockbackAnimation.Next(new SamusAnimationFrame(SamusAnimationSegmentName.Recoil, 10), InstalledFixtures.FramesBeyondAnimation),
            "the knockback's recoil hold follows its slow recoil, which no segment names");
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
