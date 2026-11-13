using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks the charge flare's typed programs and their playback against the original delay programs (#1624).</summary>
internal static class ChargeFlareDefinitionsVerification
{
    /// <summary>The original loop code: the program goes back by the following byte's count of places.</summary>
    private const byte NativeLoop = 254;

    /// <summary>The original restart code: the program goes back to its first place.</summary>
    private const byte NativeRestart = 255;

    /// <summary>The original table of the three programs' places, just before the programs.</summary>
    private const int NativeProgramTable = 0x90c481;

    /// <summary>
    /// Decodes each component's original delay program into its pictures' waits and the picture its end continues at,
    /// and compares them with the typed animations; then plays every component over repeated loops and from every picture
    /// at each wait boundary, comparing each picture and wait with the original routine's reading of the cartridge.
    /// </summary>
    internal static void VerifyChargeFlareDefinitions(CartridgeImportAddressSpace bus)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        ChargeFlareComponent[] components = Enum.GetValues<ChargeFlareComponent>();
        for (int place = 0; place < components.Length; place++)
        {
            ChargeFlareComponent component = components[place];
            ushort program = RomDataReader.ReadWordFixedBank(cartridge, NativeProgramTable + place * 2);
            var waits = new List<int>();
            int position = 0;
            byte entry;
            while ((entry = Delay(program, position)) is not (NativeLoop or NativeRestart))
            {
                waits.Add(entry);
                position++;
            }
            int continuesAt = entry == NativeRestart ? 0 : position - Delay(program, position + 1);
            IReadOnlyList<AnimationBeat<ChargeFlarePhase, NoAnimationCue>> beats = ChargeFlareAnimationDefinitions.Segments.Beats(component);
            AssertEqual(waits.Count, beats.Count, $"{component} shows the original program's pictures");
            for (int picture = 0; picture < beats.Count; picture++)
            {
                var beat = (AnimationBeat<ChargeFlarePhase, NoAnimationCue>.Frame)beats[picture];
                AssertEqual(new ChargeFlarePhase(picture), beat.Shown, $"{component} picture {picture} is its program's place");
                AssertEqual(waits[picture] + 1, beat.Duration, $"{component} picture {picture} shows for its wait and one update");
            }
            Animation<ChargeFlareComponent> animation = ChargeFlareAnimationDefinitions.Animations[component];
            var loop = (AnimationEnd.Loop)animation.End;
            AssertEqual(continuesAt, animation.Phases[loop.Phase].FirstBeat, $"{component} continues past its last picture where the original program does");
            AssertEqual(beats.Count, animation.Phases[loop.Phase].FirstBeat + animation.Phases[loop.Phase].BeatCount,
                $"{component}'s repeated run ends at its last picture");
        }
        AssertEqual(ChargeFlareAnimationDefinitions.MainFlareLoopStart, ChargeFlareAnimations.MainFlareLoop().Phase,
            "The grapple's flare restarts at the main flare's loop");

        var system = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
        int[] phases = system.Charge.FlarePhases;
        WrappingCountdown[] holds = system.Charge.FlareHolds;
        int ticks = 0, boundaries = 0;
        for (int place = 0; place < components.Length; place++)
        {
            ChargeFlareComponent component = components[place];
            phases[place] = 0; holds[place] = WrappingCountdown.Start(0);
            ushort expectedPhase = 0, expectedHold = 0;
            for (int tick = 0; tick < 1024; tick++)
            {
                Reference(place, ref expectedPhase, ref expectedHold);
                system.Animation.AdvanceFlareComponent(component);
                AssertEqual(new VerifyChargeFlareDefinitionsCase(expectedPhase, expectedHold), new VerifyChargeFlareDefinitionsCase(phases[place], holds[place].RawValue),
                    "Flare playback matches the original program over repeated loops and restarts");
                ticks++;
            }
            IEnumerable<int> starts = Enumerable.Range(0, ChargeFlareAnimations.LastPicture(component) + 1);
            foreach (ushort hold in new ushort[] { 0, 1, 2, 0x8000, 0xffff })
            foreach (int start in starts)
            {
                expectedPhase = (ushort)start;
                phases[place] = start;
                holds[place] = WrappingCountdown.Start(hold);
                expectedHold = hold;
                Reference(place, ref expectedPhase, ref expectedHold);
                system.Animation.AdvanceFlareComponent(component);
                AssertEqual(new VerifyChargeFlareDefinitionsCase(expectedPhase, expectedHold), new VerifyChargeFlareDefinitionsCase(phases[place], holds[place].RawValue),
                    $"{component} picture {start} with wait {hold} moves on as the original program does");
                boundaries++;
            }
        }

        // A place outside an animation is no picture of it: only the Hyper Beam's backward count reaches one, and the ordinary
        // flare never plays again once Samus has the Hyper Beam, so the port plays no further from it (#1624).
        foreach ((ChargeFlareComponent component, int phase) in new[]
                 {
                     (ChargeFlareComponent.MainFlare, 30), (ChargeFlareComponent.MainFlare, ushort.MaxValue - 7),
                     (ChargeFlareComponent.MainFlare, ushort.MaxValue - 1), (ChargeFlareComponent.MainFlare, ushort.MaxValue),
                     (ChargeFlareComponent.SlowSparks, 6), (ChargeFlareComponent.FastSparks, ushort.MaxValue - 1),
                 })
        {
            AssertThrows<InvalidOperationException>(() => ChargeFlareAnimations.Next(component, phase),
                $"{component} picture {phase} is outside its animation");
        }
        // The Hyper Beam's flare counts each component's picture back three updates at a time, fast sparks first, and the
        // fast sparks reaching their first picture end the flare; from its own start and from an ordinary flare's pictures
        // the Baby's gift catches mid-charge, the count matches the original routine, places wrapped past zero included.
        int hyperTicks = 0;
        foreach ((int[] startPhases, ushort[] startHolds) in new[]
                 {
                     (new[] { 29, 5, 5 }, new ushort[] { 3, 3, 3 }),
                     (new[] { 0, 0, 0 }, new ushort[] { 3, 5, 4 }),
                     (new[] { 17, 0, 3 }, new ushort[] { 1, 0, 2 }),
                     (new[] { 2, 1, 0 }, new ushort[] { 0, 0xffff, 0x8000 }),
                 })
        {
            var hyper = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
            var samus = new SuperMetroid.Core.Samus.SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
                SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), hyper.Charge) { HyperBeamEnabled = true };
            hyper.Charge.StartHyperBeamFlare();
            ushort[] expectedPhases = [.. startPhases.Select(phase => (ushort)phase)], expectedHolds = [.. startHolds];
            for (int place = 0; place < 3; place++)
            {
                hyper.Charge.FlarePhases[place] = startPhases[place];
                hyper.Charge.FlareHolds[place] = WrappingCountdown.Start(startHolds[place]);
            }
            bool expectedLit = true;
            for (int tick = 0; tick < 800 && expectedLit; tick++)
            {
                for (int place = 2; place >= 0; place--)
                {
                    expectedHolds[place] = unchecked((ushort)(expectedHolds[place] - 1));
                    if (expectedHolds[place] != 0 && (short)expectedHolds[place] >= 0)
                        continue;
                    bool last = expectedPhases[place] == 1;
                    expectedPhases[place] = unchecked((ushort)(expectedPhases[place] - 1));
                    if (!last)
                        expectedHolds[place] = 3;
                    else if (place == 2)
                        expectedLit = false;
                }
                ChargeFlareComponents shown = hyper.AdvanceChargeFlare(samus);
                AssertEqual(new ChargeFlareComponents(3, Descending: true), shown, "The Hyper Beam's flare shows every component, fast sparks first");
                AssertTrue(expectedPhases.Select(phase => (int)phase).SequenceEqual(hyper.Charge.FlarePhases)
                    && expectedHolds.SequenceEqual(hyper.Charge.FlareHolds.Select(hold => hold.RawValue))
                    && expectedLit == hyper.Charge.HyperBeamFlare,
                    $"Hyper Beam flare from [{string.Join(", ", startPhases)}] matches the original count on tick {tick}");
                hyperTicks++;
            }
        }

        // Each component's picture for each facing is the composition the original selector table names at the component's
        // run start plus the picture; every other place 0 through 65535, which only the Hyper Beam's count reaches, shows
        // nothing, not the next run's picture the original draw showed there (a presentation quirk, #1624).
        ushort[] nativeTable = [.. Enumerable.Range(0, SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteNative.NativeSelectorCount)
            .Select(selector => RomDataReader.ReadWordFixedBank(cartridge, SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteDefinitionsConstants.SelectorTable + selector * 2))];
        int pictures = 0;
        foreach (bool facingLeft in new[] { false, true })
        foreach (ChargeFlareComponent component in components)
        {
            int runStart = component switch
            {
                ChargeFlareComponent.MainFlare => 0,
                ChargeFlareComponent.SlowSparks => facingLeft ? 42 : 30,
                ChargeFlareComponent.FastSparks => facingLeft ? 48 : 36,
                _ => throw new InvalidOperationException($"Undefined {nameof(ChargeFlareComponent)} {component}."),
            };
            int runLength = ChargeFlareAnimations.LastPicture(component) + 1;
            for (int phase = 0; phase <= ushort.MaxValue; phase++)
            {
                SuperMetroid.Core.Samus.Artwork.ProjectileSpriteId? picture = SuperMetroid.Core.Samus.Artwork.ChargeFlarePictures.Of(component, facingLeft, phase);
                if (phase >= runLength)
                    AssertTrue(picture is null, $"{component} place {phase} facing left {facingLeft} is none of its pictures and shows nothing");
                else
                    AssertEqual(nativeTable[runStart + phase], picture is { } named ? SuperMetroid.Core.Samus.Artwork.ProjectileSpriteNative.Address(named) : (ushort)0,
                        $"{component} place {phase} facing left {facingLeft} shows the original selector's composition");
                pictures++;
            }
        }
        Console.WriteLine($"Charge flare: three original programs decode to the typed pictures, waits and loops; {ticks} loop ticks, {boundaries} boundary advances and {hyperTicks} Hyper Beam ticks match the original playback; {pictures} pictures match the original selector table.");

        byte Delay(ushort program, int selected) => bus.ReadCartridgeByte(0x900000 | unchecked((ushort)(program + selected)));

        void Reference(int place, ref ushort phase, ref ushort hold)
        {
            hold = unchecked((ushort)(hold - 1));
            if ((short)hold >= 0) return;
            ushort program = RomDataReader.ReadWordFixedBank(cartridge, NativeProgramTable + place * 2);
            phase = unchecked((ushort)(phase + 1));
            byte delay = Delay(program, phase);
            if (delay == NativeRestart) { phase = 0; delay = Delay(program, phase); }
            else if (delay == NativeLoop) { phase = unchecked((ushort)(phase - Delay(program, phase + 1))); delay = Delay(program, phase); }
            hold = delay;
        }
    }

    /// <summary>Runs the charge flare cadence definitions against the retail cartridge.</summary>
    [VerificationCommand("--charge-flare-definitions")]
    internal static int ChargeFlareDefinitionsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyChargeFlareDefinitions), () => VerifyChargeFlareDefinitions(rom));
        return 0;
    }
}
