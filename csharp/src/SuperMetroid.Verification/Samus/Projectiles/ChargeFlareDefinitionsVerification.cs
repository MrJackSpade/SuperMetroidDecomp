using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Verification.Harness;
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
            IReadOnlyList<AnimationBeat<ChargeFlarePicture, NoAnimationCue>> beats = ChargeFlareAnimationDefinitions.Pictures(component);
            AssertEqual(waits.Count, beats.Count, $"{component} shows the original program's pictures");
            for (int picture = 0; picture < beats.Count; picture++)
            {
                var beat = (AnimationBeat<ChargeFlarePicture, NoAnimationCue>.Frame)beats[picture];
                AssertEqual(waits[picture] + 1, beat.Duration, $"{component} picture {picture} shows for its wait and one update");
            }
            Animation<ChargeFlareSegment> animation = ChargeFlareAnimationDefinitions.Animations[component];
            var loop = (AnimationEnd.Loop)animation.End;
            AnimationPhase<ChargeFlareSegment> repeated = animation.Phases[loop.Phase];
            int repeatedFrom = animation.Phases.Take(loop.Phase).Sum(phase => phase.BeatCount * phase.Plays) + repeated.FirstBeat;
            AssertEqual(continuesAt, repeatedFrom, $"{component} continues past its last picture where the original program does");
            AssertEqual(beats.Count, repeatedFrom + repeated.BeatCount * repeated.Plays, $"{component}'s repeated run ends at its last picture");
            if (component == ChargeFlareComponent.MainFlare)
            {
                var grappleFlare = new ChargeFlareComponentState(component);
                grappleFlare.RestartAtLoop();
                AssertEqual<int?>(continuesAt, grappleFlare.NativePicture,
                    "The grapple's flare restarts at the main flare's loop, where the original program continues");
            }
        }

        var system = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        int ticks = 0, boundaries = 0;
        for (int place = 0; place < components.Length; place++)
        {
            ChargeFlareComponent component = components[place];
            ChargeFlareComponentState flare = system.Charge.Flare(component);
            flare.ShowNative(0, WrappingCountdown.Start(0));
            ushort expectedPhase = 0, expectedHold = 0;
            for (int tick = 0; tick < 1024; tick++)
            {
                Reference(place, ref expectedPhase, ref expectedHold);
                system.Animation.AdvanceFlareComponent(component);
                AssertEqual(new VerifyChargeFlareDefinitionsCase(expectedPhase, expectedHold), new VerifyChargeFlareDefinitionsCase(ShownPicture(flare), flare.NativeWait.RawValue),
                    "Flare playback matches the original program over repeated loops and restarts");
                ticks++;
            }
            // The ordinary flare's waits run from a picture's whole wait down to zero; it never leaves one run past zero.
            IEnumerable<int> starts = Enumerable.Range(0, ChargeFlareAnimationDefinitions.PictureCount(component));
            foreach (ushort hold in new ushort[] { 0, 1, 2 })
            foreach (int start in starts)
            {
                expectedPhase = (ushort)start;
                flare.ShowNative(start, WrappingCountdown.Start(hold));
                expectedHold = hold;
                Reference(place, ref expectedPhase, ref expectedHold);
                system.Animation.AdvanceFlareComponent(component);
                AssertEqual(new VerifyChargeFlareDefinitionsCase(expectedPhase, expectedHold), new VerifyChargeFlareDefinitionsCase(ShownPicture(flare), flare.NativeWait.RawValue),
                    $"{component} picture {start} with wait {hold} moves on as the original program does");
                boundaries++;
            }
        }

        // The Hyper Beam's flare counts each component's picture back three updates at a time, fast sparks first, and the
        // fast sparks reaching their first picture end the flare. From its own start, and from ordinary flare pictures and
        // waits the Baby's gift catches mid-charge, each update's shown pictures and the flare's end match the original
        // routine, which counts its wait word to zero and runs a component's place past its first picture (#2396: the
        // check compares what each component shows, where it compared the wait word the port no longer keeps).
        int hyperTicks = 0;
        foreach ((bool shot, int[] startPhases, ushort[] startHolds) in new[]
                 {
                     (true, new[] { 29, 5, 5 }, new ushort[] { 3, 3, 3 }),
                     (false, new[] { 0, 0, 0 }, new ushort[] { 3, 5, 4 }),
                     (false, new[] { 17, 0, 3 }, new ushort[] { 1, 0, 2 }),
                     (false, new[] { 2, 1, 0 }, new ushort[] { 0, 1, 2 }),
                 })
        {
            var hyper = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
            var samus = new SuperMetroid.Core.Samus.SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
                SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), hyper.Charge, artwork: global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { HyperBeamEnabled = true };
            if (shot)
            {
                hyper.Charge.StartHyperBeamFlare();
            }
            else
            {
                // An ordinary charge the Baby's gift catches: the count still runs and each component stands where the
                // ordinary flare left it.
                hyper.Charge.FlareCounter = ChargeFlareAnimationDefinitions.SparksShownFrom;
                for (int place = 0; place < 3; place++)
                    hyper.Charge.Flare(components[place]).ShowNative(startPhases[place], WrappingCountdown.Start(startHolds[place]));
            }
            ushort[] expectedPhases = [.. startPhases.Select(phase => (ushort)phase)], expectedHolds = [.. startHolds];
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
                AssertEqual(ChargeFlareComponents.HyperBeamFlare, shown, "The Hyper Beam's flare shows every component, fast sparks first");
                // A place the original count wrapped past zero is the port's component stepped back past its first picture.
                AssertTrue(components.Select((component, place) => expectedPhases[place] < ChargeFlareAnimationDefinitions.PictureCount(component) ? expectedPhases[place] : (int?)null)
                        .SequenceEqual(components.Select(component => hyper.Charge.Flare(component).NativePicture))
                    && expectedLit == hyper.Charge.IsCharging,
                    $"Hyper Beam flare from [{string.Join(", ", startPhases)}] matches the original count on tick {tick}");
                hyperTicks++;
            }
        }

        // Each component's picture for each facing is the composition the original selector table names at the component's
        // run start plus the picture; a place past the run is none of the component's pictures. A component the Hyper
        // Beam's count runs past its first picture shows nothing, not the next run's picture the original draw showed
        // there (a presentation quirk, #1624, #2114).
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
            int runLength = ChargeFlareAnimationDefinitions.PictureCount(component);
            for (int phase = 0; phase < runLength; phase++)
            {
                SuperMetroid.Core.Samus.Artwork.ProjectileSpriteId picture = SuperMetroid.Core.Samus.Artwork.ChargeFlarePictures.Of(component, facingLeft, ChargeFlareAnimationDefinitions.PictureNumbered(component, phase));
                AssertEqual(nativeTable[runStart + phase], SuperMetroid.Core.Samus.Artwork.ProjectileSpriteNative.Address(picture),
                    $"{component} place {phase} facing left {facingLeft} shows the original selector's composition");
                pictures++;
            }
            ChargeFlarePicture another = component == ChargeFlareComponent.MainFlare ? ChargeFlarePicture.SparksWidest : ChargeFlarePicture.Ember;
            AssertThrows<ArgumentOutOfRangeException>(() => SuperMetroid.Core.Samus.Artwork.ChargeFlarePictures.Of(component, facingLeft, another),
                $"{component} facing left {facingLeft} shows none of another component's pictures ({another})");
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

        // The ordinary flare plays only through its components' pictures: none steps past its first.
        static int ShownPicture(ChargeFlareComponentState flare) =>
            flare.NativePicture ?? throw new InvalidOperationException($"{flare.Component} stepped past its first picture in ordinary playback.");
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
