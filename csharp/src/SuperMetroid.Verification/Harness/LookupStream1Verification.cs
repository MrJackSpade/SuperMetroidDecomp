using SuperMetroid.TestSupport.SamusNativeCodecs;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Input;
using SuperMetroid.Verification.Areas.Escape.Artwork;
using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Verification.Enemies.Dragon;
using SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Samus.SamusAtmosphereArtworkBoundaryVerification;
using static SuperMetroid.Verification.Samus.Palettes.SamusVisorColorsVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Projectiles.ChargeFlareCompositionsVerification;
using static SuperMetroid.Verification.Areas.Escape.Artwork.EscapeTimerPresentationVerification;
using static SuperMetroid.Verification.Samus.Drawing.SamusAtmosphericAnimationDefinitionsVerification;
using static SuperMetroid.Verification.Samus.SamusContentIdentityVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Hud;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Harness;

using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Cartridge-oracle checks for the first group of compiled gameplay and presentation catalogs.</summary>
internal static class LookupStream1Verification
{
    /// <summary>Checks that the installed world-map BG3 atlas uploads the native characters whole and rejects an invalid pen.</summary>
    internal static void VerifyLookupStream1WorldBackground(ISnesAddressSpace rom)
    {
        byte[] Read(int address, int count) => Enumerable.Range(0, count).Select(i => LookupStream1WorldBackgroundRomByte(rom, address + i)).ToArray();
        byte[] front = Read(MapSourceDefinitions.Menu.ForegroundCharacters, WorldMapArtworkFormat.ForegroundBytes);
        byte[] back = Read(MapSourceDefinitions.Menu.BackgroundCharacters, WorldMapArtworkFormat.BackgroundBytes);
        byte[] frontPixels = SnesGraphics.DecodePlanarTiles(front, 4, 16, out Extent16<ImagePixels> size);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(back, 2, 16, out Extent16<ImagePixels> backSize);
        byte[] Png(byte[] selected, int height, int colors)
        {
            using var png = new MemoryStream(); IndexedPng.Write(png, new(size.X, checked((ushort)height)), selected, SnesGraphics.DiagnosticPalette(colors));
            return png.ToArray();
        }
        byte[] frontPng = Png(frontPixels, size.Y, 16);
        var stock = WorldMapCharacters.Read(new MemoryStream(frontPng), new MemoryStream(Png(pixels, backSize.Y, 4)));
        var vram = new SnesVram(); stock.LoadTo(vram);
        AssertTrue(vram.Bytes.Slice(WorldMapArtworkFormatConstants.ForegroundDestination, front.Length).SequenceEqual(front), "Foreground obligation remains exact and separate");
        AssertTrue(vram.Bytes.Slice(WorldMapArtworkFormatConstants.BackgroundDestination, back.Length).SequenceEqual(back), "Complete BG3 upload holds the installed pixels");
        byte[] invalid = (byte[])pixels.Clone(); invalid[0] = 4;
        AssertThrows<InvalidDataException>(() => WorldMapCharacters.Read(new MemoryStream(frontPng),
            new MemoryStream(Png(invalid, backSize.Y, 16))), "Invalid BG3 pen rejected during import, before upload");
        Console.WriteLine("World BG3: the installed atlas uploads the native characters whole and an invalid pen is rejected.");
    }

    /// <summary>Reads a source BG3 byte for the world-map artwork oracle.</summary>
    private static byte LookupStream1WorldBackgroundRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks death atlas source-byte relations, the stock hash, and the explosion's character order.</summary>
    internal static void VerifyLookupStream1DeathPixels(ISnesAddressSpace rom)
    {
        // $9B:8000: the explosion's characters, in the order its spritemaps name them.
        const int explosionCharacters = 0x9b8000;
        byte[] native = Enumerable.Range(0, SamusDeathTileAtlasFormat.TotalByteCount)
            .Select(index => LookupStream1DeathPixelsRomByte(rom, explosionCharacters + SamusDeathTileAtlasFormat.ExplosionByte(index))).ToArray();
        SamusDeathTileAtlas Create(byte[] planar)
        {
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, 8, out Extent16<ImagePixels> size);
            using var png = new MemoryStream();
            IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
            png.Position = 0;
            return SamusDeathTileAtlas.Load(png);
        }
        void Check(SamusDeathTileAtlas art, byte[] expected)
        {
            ReadOnlySpan<byte> explosion = art.Explosion.Characters;
            for (int index = 0; index < expected.Length; index++)
                AssertEqual(expected[index], explosion[SamusDeathTileAtlasFormat.ExplosionByte(index)], "Exact independent explosion character bytes");
            AssertEqual(SelectedPresentationHash.Create(nameof(SamusDeathTileAtlas),
                content => content.Append("death characters", expected)), art.ContentIdentity, "Exact pre-conversion death hash");
        }
        var stock = Create(native);
        Check(stock, native);
        AssertTrue(stock.Explosion.Characters.SequenceEqual(Enumerable.Range(0, SamusDeathTileAtlasFormat.TotalByteCount)
                .Select(offset => LookupStream1DeathPixelsRomByte(rom, explosionCharacters + offset)).ToArray()),
            "Stock explosion characters equal the native characters in spritemap order");
        Console.WriteLine("Death atlas:5120 installed native bytes,the canonical stock hash and explosion character order pass.");
    }

    /// <summary>Reads a native Samus death tile byte for exact atlas comparison.</summary>
    private static byte LookupStream1DeathPixelsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks every escape Dachora duration, pose, displacement, and uninterrupted update against bank $B3.</summary>
    internal static void VerifyLookupStream1EscapeDachoraCadence(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1EscapeDachoraCadenceRomByte(rom, address), LookupStream1EscapeDachoraCadenceRomByte(rom, address + 1));
        foreach (int program in new[] { 0, 1, 2 })
        {
            var guard = new CartridgeReadGuard(rom).Forbid(address => address is >= 0xb3e964 and < 0xb3eaa8,
                address => $"Production read a native escape-Dachora list byte ${address:X6}.");
            var enemies = CreateEscapeDachoraProgramSystem();
            var place = enemies.Places.InUpdateOrder[0];
            // The typed programs (#641) replacing the native lists at $B3:E964, $B3:E9D0 and $B3:EA34.
            EscapeDachoraEnemyState state = enemies.EscapeDachoraStates[0] ?? throw new InvalidOperationException("The Dachora fixture has no state.");
            state.Playback = new(program == 0 ? EscapeDachoraAnimations.RunningAroundLowTide
                : program == 1 ? EscapeDachoraAnimations.RunningAroundHighTide : EscapeDachoraAnimationsConstants.StartingWith(EscapeDachoraSegment.RunningForEscape), AnimationPlace.StartOfAnimation);
            object?[] arguments = [place, null, null, Position16<RoomPixels>.Zero, GameplayAction.None];
            int records = program == 2 ? 19 : 60, expectedX = place.Position.X.Whole, calls = 0;
            for (int record = 0; record < records; record++)
            {
                int address;
                if (program == 2)
                {
                    address = record == 0 ? 0xea34 : 0xea38 + (record - 1) * 6;
                    if (record > 1) expectedX += Word(0xb3eadf);
                }
                else
                {
                    int direction = record / 30;
                    int start = program == 0 ? direction == 0 ? 0xe968 : 0xe99c : direction == 0 ? 0xe9d4 : 0xea04;
                    address = start + (record % 6) * 6;
                    if (record != 0) expectedX += (record <= 30 ? -1 : 1) * Word(0xb3eadf);
                }
                ushort duration = Word(0xb30000 | address), visual = Word(0xb30000 | (address + 2));
                for (int held = 0; held < duration; held++)
                {
                    EnemyInstructionRunner.Process(arguments); calls++;
                    AssertEqual((ushort)(duration - held), place.AnimationClock.Hold, "Every uninterrupted escape Dachora exposure tick");
                    AssertEqual(visual, place.SpritemapPointer, "Native escape Dachora pose order");
                    AssertEqual(unchecked((ushort)expectedX), place.Position.X.Whole, "Native callback displacement occurs only at pose completion");
                }
            }
            AssertEqual(program == 0 ? 180 : program == 1 ? 120 : 163, calls, "Exact full native pacing/departure duration");
            EnemyInstructionRunner.Process(arguments);
            expectedX += Word(0xb3eadf);
            AssertEqual(unchecked((ushort)expectedX), place.Position.X.Whole, "Final movement callback is preserved");
            ushort next = program == 0 ? (ushort)0xe968 : program == 1 ? (ushort)0xe9d4 : (ushort)0xea80;
            AssertEqual(Word(0xb30000 | (next + 2)), place.SpritemapPointer, "Pacing reversal/maximum speed loop shows the native record's pose");
            AssertEqual(Word(0xb30000 | next), place.AnimationClock.Hold, "Loop reload keeps native cadence");
            AssertEqual(0, guard.ForbiddenReadAttempts, "Escape Dachora mechanics avoid cartridge reads");
        }
        Console.WriteLine("Escape Dachora cadence:139 native records,463 uninterrupted ticks,two complete pacing round trips and full accelerating departure pass.");
    }

    /// <summary>A room enemy system holding one initialized escape Dachora in slot zero.</summary>
    private static RoomEnemySystem CreateEscapeDachoraProgramSystem()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().HasEvent = _ => false;
        RoomEnemyPlace dachora = enemies.Places.InUpdateOrder[0];
        dachora.Bind(EnemyDefinitionId.DachoraEscape, dachora.KeptDefinition);
        dachora.Bind(dachora.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
        dachora.Position = dachora.Position.WithWhole(new(256, 200));
        enemies.EscapeAnimalsBehavior.InitializeEscapeDachora(dachora);
        return enemies;
    }

    /// <summary>Reads a native escape Dachora timing or movement byte for the cadence oracle.</summary>
    private static byte LookupStream1EscapeDachoraCadenceRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks Powamp body and balloon holds, loops, sleep, and native Y offsets through the live instruction runner.</summary>
    internal static void VerifyLookupStream1PowampCadence(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1PowampCadenceRomByte(rom, address), LookupStream1PowampCadenceRomByte(rom, address + 1));
        foreach (ushort root in new ushort[] { 0xc163, 0xc173, 0xc183, 0xc191 })
        {
            bool isBody = root < 0xc183;
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            // The balloon and its body are placed as their room places them, whose settings name the part each is (#2163).
            var balloon = enemies.Places.InUpdateOrder[0];
            balloon.SetSpawnParameter1(EnemyDefinitionId.Powamp, 1);
            balloon.Bind(EnemyDefinitionId.Powamp, balloon.KeptDefinition);
            balloon.Bind(balloon.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            PowampBehavior.InitializePowampBalloon(balloon, balloon.HeldOccupant().SpawnSettings<PowampBalloonSpawnSettings>());
            var body = enemies.Places.InUpdateOrder[1];
            body.BindPlaced(EnemyDefinitionId.Powamp, new PowampBodySpawnSettings(PowampContact.Accepts));
            body.Bind(body.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            PowampBehavior.InitializePowampBody(body);
            body.Position = body.Position.WithWholeY(100);
            var place = isBody ? body : balloon;
            PowampSegment program = root switch
            {
                0xc163 => PowampSegment.BodyFast,
                0xc173 => PowampSegment.BodySlow,
                0xc183 => PowampSegment.BalloonInflatingFromDeflated,
                _ => PowampSegment.BalloonStartSinking,
            };
            if (isBody)
                body.RequireSpeciesState<PowampBodyState>().Playback = new(PowampAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            else
                PowampBehavior.StartPowampBalloonAnimation(balloon, balloon.RequireSpeciesState<PowampBalloonState>(), program);
            var pair = new PowampBalloon(balloon, balloon.RequireSpeciesState<PowampBalloonState>(), balloon.HeldOccupant());
            place.AnimationClock.StepOnNextUpdate();
            object?[] arguments = [place, null, null, Position16<RoomPixels>.Zero, GameplayAction.None];
            int calls = 0;
            for (int pose = 0; pose < 3; pose++)
            {
                ushort address = (ushort)(root + pose * 4), duration = Word(0xa80000 | address);
                ushort sprite = Word(0xa80000 | (address + 2));
                for (int held = 0; held < duration; held++)
                {
                    EnemyInstructionRunner.Process(arguments); calls++;
                    // A balloon's last pose is shown for good, its clock stopped while the native timer counts it down (#2454).
                    SuperMetroid.Verification.Enemies.NativeEnemyProjectileParity.AssertClockHold((ushort)(duration - held), place.AnimationClock,
                        $"Every uninterrupted Powamp hold tick (program {root:X4}, pose {pose}, tick {held})");
                    AssertEqual(sprite, place.SpritemapPointer, "Native Powamp held visual");
                    if (!isBody)
                    {
                        PowampBehavior.AlignPowampBalloonY(body, pair);
                        int nativeOffset = unchecked((short)Word(0xa80000 | ((root == 0xc183 ? 0xc277 : 0xc27d) + pose * 2)));
                        AssertEqual((ushort)(100 + nativeOffset), balloon.Position.Y.Whole, "Native pose-driven balloon collision center");
                    }
                }
            }
            AssertEqual(root == 0xc163 ? 15 : root == 0xc173 ? 27 : 167, calls, "Native Powamp sequence duration");
            EnemyInstructionRunner.Process(arguments);
            if (isBody)
            {
                AssertEqual(Word(0xa80000 | (root + 2)), place.SpritemapPointer, "Actual Powamp loop repeats first pose");
                AssertEqual(Word(0xa80000 | root), place.AnimationClock.Hold, "Powamp loop first hold reload");
            }
            else
            {
                AssertEqual(0, place.AnimationClock.Hold, "Powamp Sleep follows full terminal hold");
                ushort terminalY = balloon.Position.Y.Whole;
                PowampBehavior.AlignPowampBalloonY(body, pair);
                AssertEqual(terminalY, balloon.Position.Y.Whole, "Sleep preserves terminal balloon collision center");
                EnemyInstructionRunner.Process(arguments);
                AssertTrue(place.AnimationClock.IsStopped, "Sleeping Powamp keeps its clock stopped");
            }
        }
        Console.WriteLine("Powamp cadence:12 native holds,376 uninterrupted exposure ticks,both Goto/Sleep boundaries and native balloon centers pass.");
    }

    /// <summary>Reads bank-$A8 Powamp timing, artwork, and balloon-offset bytes for comparison.</summary>
    private static byte LookupStream1PowampCadenceRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks Metroid chase/idle pulse holds, loop callbacks, sound, and RNG ownership.</summary>
    internal static void VerifyLookupStream1MetroidPulse(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1MetroidPulseRomByte(rom, address), LookupStream1MetroidPulseRomByte(rom, address + 1));
        foreach (bool chasing in new[] { true, false })
        {
            ushort root = chasing ? (ushort)0xe9cf : (ushort)0xea25;
            int records = chasing ? 20 : 5, randomCalls = 0, calls = 0;
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().NextRandom = () => { randomCalls++; return 5; };
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.Metroid, place.KeptDefinition);
            place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            // The Metroid is set up as its room's population sets it up, then plays the pulse its chase or drain starts.
            place.SetSpawnSettings(new MetroidSpawnSettings(IceDamageToFreeze: 0));
            place.SetUpAsPlaced();
            place.RequireSpeciesState<MetroidEnemyState>().Playback =
                new(MetroidAnimations.StartingWith(chasing ? MetroidSegment.ChasingSamus : MetroidSegment.DrainingSamus), AnimationPlace.StartOfAnimation);
            place.AnimationClock.StepOnNextUpdate();
            randomCalls = 0;
            object?[] arguments = [place, null, null, Position16<RoomPixels>.Zero, GameplayAction.None];
            for (int record = 0; record < records; record++)
            {
                ushort address = (ushort)(root + record * 4);
                ushort duration = Word(0xa30000 | address), visual = Word(0xa30000 | (address + 2));
                EnemyInstructionRunner.Process(arguments); calls++;
                AssertEqual(duration, place.AnimationClock.Hold, "Actual Metroid duration reload");
                AssertEqual(visual, place.SpritemapPointer, "Actual native inside-pose identity");
                for (int held = 1; held < duration; held++)
                {
                    EnemyInstructionRunner.Process(arguments); calls++;
                    AssertEqual((ushort)(duration - held), place.AnimationClock.Hold, "Every actual Metroid exposure tick");
                    AssertEqual(visual, place.SpritemapPointer, "Held Metroid inside pose stays selected");
                }
                AssertEqual(0, randomCalls, "No Metroid RNG call before complete pulse loop");
                AssertTrue(enemies.Audio.LatestSound is null, "No Metroid callback before loop boundary");
            }
            AssertEqual(chasing ? 256 : 64, calls, "Exact native pulse-loop length");
            EnemyInstructionRunner.Process(arguments);
            AssertEqual(chasing ? 1 : 0, randomCalls, "Exact Metroid callback RNG ownership");
            ushort expectedSound = chasing ? Word(0xa3ead6 + 5 * 2) : Word(0xa3eaa8);
            AssertEqual(SoundEffectCommands.DecodeOptional(SoundEffectLibrary.Enemies, expectedSound),
                enemies.Audio.LatestSound, "Native loop callback sound decodes to the sound queued");
            AssertEqual(Word(0xa30000 | (root + 2)), place.SpritemapPointer, "Callback loops to first timed record");
            AssertEqual(Word(0xa30000 | root), place.AnimationClock.Hold, "Loop reload preserves first native hold");
        }
        Console.WriteLine("Metroid pulse:25 native records,320 actual exposure ticks,two callback boundaries and exact sound/RNG ownership pass.");
    }

    /// <summary>Reads native Metroid pulse words and callback sound choices.</summary>
    private static byte LookupStream1MetroidPulseRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks all atmospheric animation durations and rejected type/frame boundaries.</summary>
    internal static void VerifyLookupStream1AtmosphericCadence(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1AtmosphericCadenceRomByte(rom, address), LookupStream1AtmosphericCadenceRomByte(rom, address + 1));
        // Every effect number a producer starts names a particle kind (six and seven both dust); two names none.
        foreach (int type in new[] { 1, 3, 4, 5, 6, 7 })
        {
            SamusParticleKind kind = SamusAtmosphericEffectWordCodec.KindNumbered(type) ?? throw new InvalidDataException($"Effect number {type} names no particle.");
            int pointer = 0x900000 | Word(0x908b93 + type * 2);
            int count = Word(0x908bef + type * 2);
            AssertEqual(count, PictureCount(kind), "Named atmospheric frame domain matches native source");
            // Each picture shows for the native duration of the step before it, the first for its own (#2388).
            for (int frame = 0; frame < count; frame++)
                AssertEqual(Word(pointer + Math.Max(frame - 1, 0) * 2), PictureUpdates(kind, frame), "Atmospheric pictures show every handed-on native duration");
        }
        AssertTrue(SamusAtmosphericEffectWordCodec.KindNumbered(0) is null, "Effect number zero is an empty place");
        foreach (int invalid in new[] { 2, 8, byte.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusAtmosphericEffectWordCodec.KindNumbered(invalid), "An unstarted or out-of-domain atmospheric type names no particle");
        Suite(nameof(VerifyProductionAtmosphericCadence), () => VerifyProductionAtmosphericCadence());
    }

    /// <summary>Reads bank-$90 atmospheric timer tables for the cadence oracle.</summary>
    private static byte LookupStream1AtmosphericCadenceRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks every 128-frame escape timer correction and the two expiration-boundary frames.</summary>
    internal static void VerifyLookupStream1TimerCadence(ISnesAddressSpace rom)
    {
        var failures = new List<string>();
        foreach (ushort frame in new ushort[] { 124, 125 })
        {
            var timer = new EscapeTimer();
            timer.RunIntoPlace();
            timer.SetRemaining(EscapeTimerClock.Time(0, 1, 50));
            byte correction = LookupStream1TimerCadenceRomByte(rom, 0x809eec + frame);
            int expected = EscapeTimerClock.Time(0, 1, 50) - correction;
            bool expired = timer.Process(frame, holdAtOneSecond: false);
            if (timer.RemainingCentiseconds != expected)
                failures.Add("NMI " + frame + ": native decrement " + correction + " expects " + expected + ", actual " + timer.RemainingCentiseconds);
            AssertTrue(!expired && timer.Readout.Seconds == 1 && timer.Readout.Minutes == 0, "Exact cadence fixture preserves non-expiring whole time");
        }
        AssertTrue(failures.Count == 0, string.Join("; ", failures));
        int total = 0;
        for (int index = 0; index < 128; index++)
        {
            byte expected = LookupStream1TimerCadenceRomByte(rom, 0x809eec + index);
            total += expected;
            AssertEqual(expected, EscapeTimerCadence.Centiseconds((ushort)index), "Every authored cadence step matches the native table");
            AssertEqual(expected, EscapeTimerCadence.Centiseconds((ushort)(index + 128)), "Native128-frame wrap");
            AssertEqual(expected, EscapeTimerCadence.Centiseconds((ushort)(index + 0xff80)), "Native ignores high NMI bits");
            var timer = new EscapeTimer();
            timer.RunIntoPlace();
            timer.SetRemaining(EscapeTimerClock.Time(0, 1, 50));
            AssertTrue(!timer.Process((ushort)index, holdAtOneSecond: false), "The authored cadence stays non-expiring");
            AssertEqual(EscapeTimerClock.Time(0, 1, 50) - expected, timer.RemainingCentiseconds, "Actual Process consumes each native correction");
        }
        AssertEqual(213, total, "Native full-period centisecond budget");
        foreach (ushort frame in new ushort[] { 124, 125 })
        {
            var timer = new EscapeTimer();
            timer.RunIntoPlace();
            timer.SetRemaining(EscapeTimerClock.Time(0, 0, 2));
            bool expired = timer.Process(frame, holdAtOneSecond: false);
            AssertEqual(frame == 124, expired, "Corrected native expiration phase");
            AssertEqual(frame == 124 ? 0 : 1, timer.RemainingCentiseconds, "Exact corrected expiration centisecond");
        }
        Console.WriteLine("Timer cadence:128 native corrections,384 phase observations,128 actual countdown updates and exact124/125 expiration pass.");
    }

    /// <summary>Reads the native centisecond correction table in bank $80.</summary>
    private static byte LookupStream1TimerCadenceRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks escape timer glyph geometry and encoded VRAM output.</summary>
    internal static void VerifyLookupStream1TimerGlyphs(ISnesAddressSpace rom)
    {
        byte[] png = EscapeTimerTileAtlasExtractor.Extract(rom);
        byte[] native = Enumerable.Range(0, 800).Select(index => LookupStream1TimerGlyphsRomByte(rom, 0xb0c000 + index)).ToArray();
        var stock = EscapeTimerCharacters.Read(new MemoryStream(png));
        byte[] Transfer(EscapeTimerCharacters value) => EscapeTimerTilePageTransfers.Resolve(value, EscapeTimerTilePage.First).Span
            .ConcatToArray(EscapeTimerTilePageTransfers.Resolve(value, EscapeTimerTilePage.Second).Span);
        AssertTrue(Transfer(stock).SequenceEqual(native), "Both reconstructed timer pages match all800 native planar bytes");
        var queue = new VramWriteQueue(); stock.QueueTo(queue);
        AssertEqual(2, queue.Entries.Count, "Original two-page queue count");
        AssertEqual(512, queue.Entries[0].SizeInBytes, "First page size");
        AssertEqual((ushort)0x7e00, queue.Entries[0].Destination.Encoded, "First page destination");
        AssertEqual(InstalledArtworkId.EscapeTimerFirstTiles, queue.Entries[0].AssetId, "First typed page identity");
        AssertEqual(288, queue.Entries[1].SizeInBytes, "Second page size");
        AssertEqual((ushort)0x7f00, queue.Entries[1].Destination.Encoded, "Second page destination");
        AssertEqual(InstalledArtworkId.EscapeTimerSecondTiles, queue.Entries[1].AssetId, "Second typed page identity");
        var uploads = new SpriteArtworkPlacements();
        AssertTrue(stock.TryRecordNativeTransfer(uploads, 0xb0c000, 512, 0x7e00) && stock.TryRecordNativeTransfer(uploads, 0xb0c200, 288, 0x7f00), "Both synchronous native uploads accepted");
        AssertTrue(uploads.Uploads.Select(upload => upload.FirstByte).SequenceEqual([0x3c00, 0x3e00]) &&
            ((byte[])[.. EscapeTimerTilePageTransfers.Resolve(stock, EscapeTimerTilePage.First).Span, .. EscapeTimerTilePageTransfers.Resolve(stock, EscapeTimerTilePage.Second).Span]).SequenceEqual(native),
            "The two synchronous pages show the native bytes where they land in the OBJ characters");
        AssertTrue(stock.TryResolve(0xb0c000, 512, out var first) && first.Span.SequenceEqual(native.AsSpan(0, 512)), "First restored native page resolution");
        AssertTrue(stock.TryResolve(0xb0c200, 288, out var second) && second.Span.SequenceEqual(native.AsSpan(512)), "Second restored native page resolution");
        AssertTrue(!stock.TryResolve(0xb0c000, 511, out _), "Mismatched native page rejected");
        AssertTrue(!stock.TryRecordNativeTransfer(new SpriteArtworkPlacements(), 0xb0c200, 288, 0x7e00), "Mismatched synchronous destination rejected");
        AssertTrue(Transfer(stock).SequenceEqual(native), "Stock timer remains immutable");
        Console.WriteLine("Timer font:836 direct outline pixels,312 shared/reflected pixels,275 retained fill positions/four deviations,173 calculated digit/T/I/E basis fill sites,800 native planar bytes and exact two-page queue/VRAM uploads pass.");
    }

    /// <summary>Reads native escape timer glyph bytes for pixel and transfer comparison.</summary>
    private static byte LookupStream1TimerGlyphsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks visual body Y origins across all poses without changing projectile origins.</summary>
    internal static void VerifyLookupStream1BodyGraphicsOrigins(ISnesAddressSpace rom)
    {
        int[] native = Enumerable.Range(0, 253).Select(pose => (int)unchecked((sbyte)LookupStream1BodyGraphicsOriginsRomByte(rom, 0x91b62d + pose * 8))).ToArray();
        InstalledSamusBody installed = SamusBodyArtworkFiles.Load(RepositoryInstallation.Installation.SamusBodyDirectory);
        for (int pose = 0; pose < 253; pose++)
        {
            if (!SamusPoseNumbering.NamesPose(pose)) continue;
            AssertEqual(native[pose], installed.Gameplay.ArtOrigin(SamusPoseNumbering.FromNumber(pose)), "The import installs the native real-pose byte as the pose's origin");
            // The drained poses' origin is their signed correction, four below the centre; the original's unsigned read of
            // it, which nothing fired from those poses reaches, is gone (#1928).
            int expectedOrigin = SamusPoseNumbering.FromNumber(pose) is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose
                or SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose
                ? native[pose]
                : unchecked((byte)native[pose]);
            AssertEqual(expectedOrigin, SamusPoseTraits.Of(SamusPoseNumbering.FromNumber(pose)).ProjectileHeight, "Physical projectile origin matches the native byte");
        }
        Console.WriteLine("Body graphics origins:253 native installed origins and physical projectile origins pass.");
    }

    /// <summary>Reads native Samus visual-origin offsets for all supported poses.</summary>
    private static byte LookupStream1BodyGraphicsOriginsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the drained crouch's installed frame placements against the native table, and their actual draws.</summary>
    internal static void VerifyLookupStream1DrainedGeometry(ISnesAddressSpace rom)
    {
        string directory = CartridgeImportContent.Component(GameInstallationLayout.SamusBodyDirectoryName);
        SamusBodyCharacterArtwork stockArt = SamusBodyArtworkFiles.LoadCharacters(directory, CartridgeImportContent.ImportData(GameInstallationLayout.SamusBodyDirectoryName));
        SamusBodySources stock = stockArt.Body;
        int[] native = Enumerable.Range(0, 32).Select(index => (int)unchecked((sbyte)LookupStream1DrainedGeometryRomByte(rom, 0x908def + index))).ToArray();
        foreach (SamusPoseId pose in new[] { SamusPoseId.DrainedCrouchingRightPose, SamusPoseId.DrainedCrouchingLeftPose })
        {
            // The import decodes each frame's placement from the table the drained draw indexes by the frame (#1517).
            foreach ((int frame, SamusBodyDecodedFrame decoded) in stock.Frames[pose])
            {
                AssertTrue(frame < 32, "A frame past the drained table has no placement, so the import decodes none");
                AssertEqual(-native[frame], decoded.OriginRise, "Every decoded drained crouch frame carries the negated native offset its frame indexes");
            }
        }
        SamusShownFrames shown = InstalledFixtures.SamusShownFrameAnalysis.Installed();
        int draws = 0;
        for (ushort frame = 0; frame < 32; frame++)
        {
            // Only the frames the drained crouch can show are drawn; the draw rejects the others (#1513).
            if (!shown.TryPictureFrame(SamusPoseId.DrainedCrouchingLeftPose, SamusAnimationNumbers.Frame(SamusPoseId.DrainedCrouchingLeftPose, frame), out _)) continue;
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Pose = SamusPoseId.DrainedCrouchingLeftPose, AnimationFrame = SamusAnimationNumbers.Frame(SamusPoseId.DrainedCrouchingLeftPose, frame), Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
            samus.BindBodyFixture(stockArt);
            samus.Presentation.DrawWithoutFlicker(samus, new SpriteFrameBuffer(), new(0, 0));
            // A frame outside the crouch's own pictures is placed as the picture it draws (#1513).
            int picture = SamusAnimationNumbers.Number(SamusPoseId.DrainedCrouchingLeftPose, shown.PictureFrame(SamusPoseId.DrainedCrouchingLeftPose, SamusAnimationNumbers.Frame(SamusPoseId.DrainedCrouchingLeftPose, frame)));
            AssertEqual(128 + native[picture], (int)samus.Presentation.DrawOrigin.Y,
                "Actual drained drawing uses the exact signed native correction of the picture it draws");
            AssertEqual((ushort)128, samus.Position.Y.Whole, "Drawing preserves physical center");
            draws++;
        }
        Console.WriteLine($"Drained geometry:32 native placements,{draws} actual draws and bounds pass.");
    }

    /// <summary>Reads native drained Samus pose offsets for geometry comparison.</summary>
    private static byte LookupStream1DrainedGeometryRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks morph-transition posture offsets through actual draws.</summary>
    internal static void VerifyLookupStream1PostureGeometry(ISnesAddressSpace rom)
    {
        string directory = CartridgeImportContent.Component(GameInstallationLayout.SamusBodyDirectoryName);
        SamusBodyCharacterArtwork stockArt = SamusBodyArtworkFiles.LoadCharacters(directory, CartridgeImportContent.ImportData(GameInstallationLayout.SamusBodyDirectoryName));
        sbyte[] native = Enumerable.Range(0, 24).Select(index => unchecked((sbyte)LookupStream1PostureGeometryRomByte(rom, 0x908d80 + index))).ToArray();
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.CrouchingTransitionRightPose, SamusPoseId.CrouchingTransitionLeftPose,
            SamusPoseId.MorphingTransitionRightPose, SamusPoseId.MorphingTransitionLeftPose,
            SamusPoseId.StandingTransitionRightPose, SamusPoseId.StandingTransitionLeftPose,
            SamusPoseId.UnmorphingTransitionRightPose, SamusPoseId.UnmorphingTransitionLeftPose
        })
        {
            int count = pose is SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionLeftPose or SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionLeftPose ? 1 : 2;
            for (ushort frame = 0; frame < count; frame++)
            {
                var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Pose = pose, AnimationFrame = SamusAnimationNumbers.Frame(pose, frame), Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
                samus.BindBodyFixture(stockArt);
                samus.Presentation.DrawWithoutFlicker(samus, new SpriteFrameBuffer(), new(0, 0));
                AssertEqual(128 + native[(SamusPoseNumbers.Number(pose) - 0x35) * 2 + frame], (int)samus.Presentation.DrawOrigin.Y,
                    "Actual transition rendering preserves native support correction");
                AssertEqual((ushort)128, samus.Position.Y.Whole, "Posture art never mutates physical center");
            }
        }
        Console.WriteLine("Posture geometry:12 actual draw cases place the native offsets.");
    }

    /// <summary>Reads native Samus posture Y offsets for the geometry oracle.</summary>
    private static byte LookupStream1PostureGeometryRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks the landing and posture-transition poses' installed frame placements against the native tables, and the landing draws.</summary>
    internal static void VerifyLookupStream1BodyFacingOffsets(ISnesAddressSpace rom)
    {
        int[] landing = Enumerable.Range(0, 16).Select(index => (int)LookupStream1BodyFacingOffsetsRomByte(rom, 0x908d28 + index)).ToArray();
        int[] posture = Enumerable.Range(0, 24).Select(index => (int)unchecked((sbyte)LookupStream1BodyFacingOffsetsRomByte(rom, 0x908d80 + index))).ToArray();
        SamusBodySources stock = InstalledFixtures.SamusBody;
        // The import bakes each frame's placement from the row the draw indexes: four landing bytes, two posture bytes per
        // pose, plus the frame, reading on into the next pose's row up to the table's end (#1517).
        for (int row = 0; row < 4; row++)
        {
            SamusPoseId pose = SamusPoseNumbering.FromNumber(0xa4 + row);
            foreach ((int frame, SamusBodyDecodedFrame decoded) in stock.Frames[pose])
            {
                AssertTrue(frame < 16 - row * 4, "A landing frame past the table has no placement, so the import decodes none");
                AssertEqual(landing[row * 4 + frame], decoded.OriginRise, "Each decoded landing frame carries the unsigned native byte its frame indexes");
            }
        }
        for (int row = 0; row < 12; row++)
        {
            // The glitch-ball and instant morphing records name no pose (#2048): no path gives Samus their numbers, so the
            // draw never indexes their rows by them. The earlier poses' frames still read on into those rows, as their checks show.
            if (!SamusPoseNumbering.NamesPose(0x35 + row))
                continue;
            SamusPoseId pose = SamusPoseNumbering.FromNumber(0x35 + row);
            foreach ((int frame, SamusBodyDecodedFrame decoded) in stock.Frames[pose])
            {
                AssertTrue(frame < 24 - row * 2, "A posture frame past the table has no placement, so the import decodes none");
                AssertEqual(-posture[row * 2 + frame], decoded.OriginRise, "Each decoded posture-transition frame carries the negated native offset its frame indexes");
            }
        }
        SamusShownFrames shown = InstalledFixtures.SamusShownFrameAnalysis.Installed();
        for (int index = 0; index < 16; index++)
        {
            SamusPoseId landingPose = SamusPoseNumbering.FromNumber(0xa4 + index / 4);
            // Only the landing frames the pose can show are drawn; the draw rejects the others (#1513).
            if (!shown.TryPictureFrame(landingPose, SamusAnimationNumbers.Frame(landingPose, index % 4), out _))
                continue;
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
            {
                Pose = landingPose, AnimationFrame = SamusAnimationNumbers.Frame(landingPose, (ushort)(index % 4)),
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            };
            samus.BindBodyFixture(stock);
            samus.Presentation.DrawWithoutFlicker(samus, new SpriteFrameBuffer(), cameraPosition: new(0, 0));
            // The game subtracts a word whose high byte the drawn position never shows; the body sits by the low byte (#1500).
            AssertEqual(128 - landing[index], (int)samus.Presentation.DrawOrigin.Y, "Actual rendering places every landing frame by its own byte");
            AssertEqual((ushort)128, samus.Position.Y.Whole, "Visual landing correction does not change physical position");
        }
        Console.WriteLine("Body facing offsets:16 landing and 24 posture native placements, landing draws and bounds pass.");
    }

    /// <summary>Reads native landing and posture offset words for Samus placement.</summary>
    private static byte LookupStream1BodyFacingOffsetsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the installed intro and hurt palettes keep the stock colours.</summary>
    internal static void VerifyLookupStream1HurtBlend(ISnesAddressSpace rom)
    {
        ushort[] native = Enumerable.Range(0, 32).Select(index =>
            LittleEndianBytes.Word(LookupStream1HurtBlendRomByte(rom, 0x9ba380 + index * 2), LookupStream1HurtBlendRomByte(rom, 0x9ba381 + index * 2))).ToArray();
        PaletteRgb5 Color(ushort word) => new() { Red = word & 31, Green = (word >> 5) & 31, Blue = (word >> 10) & 31 };
        InstalledSamusHurtColors Create(ushort[] supplied) => SamusHurtColorFile.Read(new MemoryStream(SamusHurtColorFile.Write(
            new SamusHurtColorDocument { Version = 1, Hurt = supplied.Take(16).Select(Color).ToArray(), Intro = supplied.Skip(16).Select(Color).ToArray() })));
        var stock = Create(native);
        for (int index = 0; index < 32; index++)
            AssertEqual(native[index], stock.Resolve(index < 16 ? SamusHurtColorVariant.Hurt : SamusHurtColorVariant.Intro, index % 16), "Stock color matches native");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve((SamusHurtColorVariant)2, 0), "Variant rejection preserved");
        foreach (int index in new[] { -1, 16 })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(SamusHurtColorVariant.Intro, index), "Color bounds preserved");
        Console.WriteLine("Hurt/intro:32 native colors kept as installed and the variant and colour bounds pass.");
    }

    /// <summary>Reads native intro and hurt RGB5 palette words.</summary>
    private static byte LookupStream1HurtBlendRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);






    /// <summary>Checks arm-cannon pose pointers against native selections.</summary>
    internal static void VerifyLookupStream1CannonPoses(ISnesAddressSpace rom)
    {
        byte[] json = File.ReadAllBytes(Path.Combine(CartridgeImportContent.ImportData(GameInstallationLayout.SamusBodyDirectoryName), SamusArmCannonArtworkFormat.ArtworkFileName));
        var document = RequiredJson.Deserialize<SamusArmCannonArtworkDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new SamusPoseIdJsonConverter(),
                new SamusProjectileDirectionJsonConverter(), new SamusArmCannonCoverOrientationJsonConverter() } });
        SamusArmCannonSourceArtwork Load(SamusArmCannonArtworkDocument value) => SamusArmCannonSourceArtwork.FromPlacement(
            SamusArmCannonSourceArtwork.LoadPlacement(new MemoryStream(SamusArmCannonSourceArtwork.Write(value))));
        var stock = Load(document);
        ushort Native(int pose) => LittleEndianBytes.Word(LookupStream1CannonPosesRomByte(rom, 0x90C7DF + pose * 2), LookupStream1CannonPosesRomByte(rom, 0x90C7E0 + pose * 2));
        AssertEqual(253, SamusBodyTableDecoding.PoseCount, "cannon original 506-byte pointer extent");
        foreach (SamusProjectileDirection direction in SamusProjectileDirectionCodec.InAimOrder)
        {
            SpriteVisualPiece appearance = document.CoverAppearances[direction];
            AssertEqual(CannonCoverAppearance(appearance.TileRow, appearance.TileColumn, appearance.Palette, appearance.Depth, appearance.FlipX, appearance.FlipY),
                stock.CoverAppearance(direction), $"cannon {direction} cover appearance loads as its document places it");
        }
        AssertEqual(SamusPoseNumbers.RecordedPoses.Count, document.PoseDescriptors.Count, "cannon installs one descriptor per pose with a record, and none for a record no pose names");
        foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses)
            AssertEqual(Native(SamusPoseNumbers.Number(pose)), document.PoseDrawingData(pose), "cannon installed native pose dispatch");
        Console.WriteLine("Cannon poses: 233 named installed descriptors and the canonical stock hash pass.");
    }

    /// <summary>Reads the native pose-to-cannon pointer table.</summary>
    private static byte LookupStream1CannonPosesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks HUD posture admission and grapple/non-grapple observations over all pose bytes.</summary>
    internal static void VerifyLookupStream1HudPosture(ISnesAddressSpace rom)
    {
        for (int pose = 0; pose < 256; pose++)
        {
            if (!SamusPoseNumbering.NamesPose(pose)) continue;
            // The admission's answer is whether the byte it reads for the pose is zero; the decoded service must agree with
            // the cartridge's flags and the code stored around them for every pose.
            if (pose < 0xDB)
                AssertEqual(LookupStream1HudPostureRomByte(rom, 0x90DDAA + pose - 0x35) == 0,
                    SamusHudObservations.PostureAdmitsWeapons(SamusPoseNumbering.FromNumber(pose), InstalledFixtures.SamusOverreads.Projectiles.PostureAdmission),
                    $"HUD exact bounded posture admission {pose:X2}");
            else
                AssertThrows<ArgumentOutOfRangeException>(() => SamusHudObservations.PostureAdmitsWeapons(SamusPoseNumbering.FromNumber(pose), InstalledFixtures.SamusOverreads.Projectiles.PostureAdmission),
                    "HUD rejects prefiltered posture observation");
            foreach (bool grapple in new[] { false, true })
            {
                bool expected = pose >= 0xF1 || (pose < 0xDB && (LookupStream1HudPostureRomByte(rom, 0x90DDAA + pose - 0x35) == 0 || grapple));
                AssertEqual(expected, SamusHudInput.PostureTransitionAdmitsWeapons(SamusPoseNumbering.FromNumber(pose), grapple, InstalledFixtures.SamusOverreads.Projectiles.PostureAdmission),
                    $"HUD actual admission {pose:X2}, Grapple={grapple}");
            }
        }
        Console.WriteLine("HUD posture: twelve real flags plus the bounded instruction bytes and every admission branch of the poses and rejection domain match native.");
    }

    /// <summary>Reads native HUD posture observation bytes.</summary>
    private static byte LookupStream1HudPostureRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks the escape timer's anchors and its sheet draws against the native sprites.</summary>
    internal static void VerifyLookupStream1TimerLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1TimerLayoutRomByte(rom, address), LookupStream1TimerLayoutRomByte(rom, address + 1));
        byte[] json = EscapeTimerPresentationExtractor.Extract();
        var stock = EscapeTimerPresentationFile.Load(new MemoryStream(json));
        int[] anchorImmediates = [0x809f73, 0x809f7c, 0x809f85, 0x809f8e];
        foreach (EscapeTimerAnchor anchor in EscapeTimerLayoutDefinitions.Anchors)
            AssertEqual(new Offset16<ScreenPixels>(unchecked((short)Word(anchorImmediates[(int)anchor])), 0), stock.anchors[anchor], "Installed timer anchor matches native renderer immediate");
        SpriteSheet sheet = EscapeTimerDrawChecks.Sheet(rom);
        SpriteArtwork characters = EscapeTimerCharacters.Read(new MemoryStream(
            SuperMetroid.AssetExtraction.Areas.Escape.EscapeTimerTileAtlasExtractor.Extract(rom))).Artwork;
        var timer = new EscapeTimer(); timer.Clear();
        for (int digit = 0; digit < 10; digit++)
        {
            timer.SetRemaining(EscapeTimerClock.Time(digit * 11, 0, digit * 11));
            var native = new OamBuffer();
            DrawImportedEscapeTimer(rom, timer, native);
            EscapeTimerDrawChecks.AssertSamePixels(native, EscapeTimerDrawChecks.Draw(timer, stock, sheet), characters, rom,
                $"Decimal glyph {digit} paints the native sprites' pixels");
        }
        Console.WriteLine("Escape timer layout: 4 installed native anchors and 10 decimal glyphs pixel-exact from the sheet pass.");
    }

    /// <summary>Reads native escape timer OAM layout bytes.</summary>
    private static byte LookupStream1TimerLayoutRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks flare origin words and emitted OAM.</summary>
    internal static void VerifyLookupStream1FlarePlacement(ISnesAddressSpace rom)
    {
        short Word(int address) => unchecked((short)(LittleEndianBytes.Word(LookupStream1FlarePlacementRomByte(rom, address), LookupStream1FlarePlacementRomByte(rom, address + 1))));
        byte[] json = ChargeFlarePlacementExtractor.Extract(rom);
        var stock = ChargeFlarePlacementFile.Load(new MemoryStream(json));
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        foreach (ChargeFlarePlacement placement in ChargeFlarePlacements.Placements(ChargeFlareSource.Beam))
        {
            int row = SamusProjectileDirectionCodec.FlarePlacementRow(placement.Aim);
            var expected = new ChargeFlareOffset { X = Word((placement.Running ? 0x90c1dc : 0x90c1a8) + 2 * row),
                Y = Word((placement.Running ? 0x90c1f0 : 0x90c1c2) + 2 * row) };
            AssertEqual(expected.Offset, stock.Resolve(placement.Running, placement.Aim), "Installed flare default matches native");
        }
        AssertEqual(23, ChargeFlarePlacements.Placements(ChargeFlareSource.Beam).Count(),
            "The beam flare is placed at ten muzzles in each movement mode and three turning placements standing");
        var grappleDocument = RequiredJson.Deserialize<ChargeFlarePlacementDocument>(GrappleFlarePlacementExtractor.Extract(rom), options);
        var grapple = ChargeFlarePlacementFile.LoadGrapple(new MemoryStream(ChargeFlarePlacementFile.Write(grappleDocument)));
        foreach (ChargeFlarePlacement placement in ChargeFlarePlacements.Placements(ChargeFlareSource.Grapple))
        {
            int row = SamusProjectileDirectionCodec.FlarePlacementRow(placement.Aim);
            var expected = new ChargeFlareOffset { X = Word((placement.Running ? 0x9bc19a : 0x9bc14a) + row * 2),
                Y = Word((placement.Running ? 0x9bc1ae : 0x9bc15e) + row * 2) };
            AssertEqual(expected.Offset, grapple.Resolve(placement.Running, placement.Aim), "Installed Grapple defaults match native");
        }
        AssertEqual(20, ChargeFlarePlacements.Placements(ChargeFlareSource.Grapple).Count(), "The Grapple flare is placed at ten muzzles in each movement mode");
        // No pose places a flare while turning running, or a Grapple flare while turning (#1500).
        foreach (SamusTurningAim turning in Enum.GetValues<SamusTurningAim>())
        {
            SamusFlareAim aim = new SamusFlareAim.WhileTurning(turning);
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(true, aim), "Running flare placement rejects turning placements");
            AssertThrows<ArgumentOutOfRangeException>(() => grapple.Resolve(false, aim), "Grapple flare placement has no turning placements");
        }
        var compositions = ChargeFlareSprites.Load(new MemoryStream(ChargeFlareSpriteExtractor.Extract(rom)));
        var body = CreateSamusIdentityFixture();
        var system = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        var draw = new Action<SpriteFrameBuffer, SamusState, Position16<RoomPixels>, ChargeFlareComponent, TurningFloorTransform?, ChargeFlareArtwork>(system.Animation.DrawFlareComponent);
        ProjectileDrawArtwork flareSheets = InstalledFixtures.ProjectileArtwork();
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose
        })
        {
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
            samus.BindBodyFixture(body);
            int direction = LookupStream1FlarePlacementRomByte(rom, 0x91b629 + SamusPoseNumbers.Number(pose) * 8 + 3);
            bool running = pose is SamusPoseId.MovingRightNormalPose or SamusPoseId.MovingLeftNormalPose;
            short x = Word((running ? 0x90c1dc : 0x90c1a8) + direction * 2);
            short y = Word((running ? 0x90c1f0 : 0x90c1c2) + direction * 2);
            var expected = new OamBuffer();
            DrawImportedFlareSpritemap((CartridgeImportAddressSpace)rom, expected, 0, new((ushort)(100 + x), unchecked((ushort)(100 + y - unchecked((byte)body.Gameplay.ArtOrigin(pose))))));
            // The flare's picture may be laid out once for several projectile sheet frames whose pictures are identical
            // (#1513), so the draw is matched against each composition that picture stands for.
            Action<SpriteFrameBuffer> drawFlare = frame => draw(frame, samus, Position16<RoomPixels>.Zero, ChargeFlareComponent.MainFlare, null,
                new ChargeFlareArtwork { Placement = stock, Sheets = flareSheets });
            OamBuffer actual = SpriteFrameBuffer.DrawnOam(drawFlare);
            AssertTrue(actual.LastFinalizedSpriteCount > 0, "Focused flare placement fixture emits its native sprite");
            AssertTrue(SpriteFrameBufferAccess.DrawnStandsFor(drawFlare, expected),
                $"Actual flare draw stands for the independently positioned native sprite, {pose}: {expected.FirstDifference(actual)}");
        }
        Console.WriteLine("Beam/Grapple flare origins:128 direct native words, zero stock overrides, bounded aliases and four actual OAM fixtures pass.");
    }

    /// <summary>Reads native beam and grapple flare placement bytes.</summary>
    private static byte LookupStream1FlarePlacementRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Creates a fixture bus that rejects cartridge reads during flare OAM drawing.</summary>
    private static CartridgeReadGuard NewLookupFlareForbiddenBus() =>
        CartridgeReadGuard.Unbacked().RejectWrites(address => $"Unexpected flare write {address:X6}");
    /// <summary>Runs the first lookup suite and checks native pose facing, movement, and selected catalog words.</summary>
    internal static void VerifyLookupStream1(ISnesAddressSpace rom)
    {
        for (int pose = 0; pose <= byte.MaxValue; pose++)
        {
            if (!SamusPoseNumbering.NamesPose(pose)) continue;
            AssertEqual(LookupStream1RomByte(rom, 0x91b629 + pose * 8), SamusPoseRecordCodecTooling.FacingByte(SamusPoseNumbering.FromNumber(pose)), "Native named-pose facing case");
            AssertEqual(LookupStream1RomByte(rom, 0x91b62a + pose * 8), SamusPoseRecordCodecTooling.MovementByte(SamusPoseNumbering.FromNumber(pose)), "Native named-pose raw movement case");
            AssertEqual(LookupStream1RomByte(rom, 0x91b62b + pose * 8), SamusNoInputPoseNumbers.Number(SamusPoseTraits.Of(SamusPoseNumbering.FromNumber(pose)).NoInput), "Native named-pose no-input case");
        }
        Suite(nameof(VerifyLookupStream1SamusPolicyDomains), () => VerifyLookupStream1SamusPolicyDomains(rom));
        Suite(nameof(VerifyLookupStream1MetroidLayout), () => VerifyLookupStream1MetroidLayout(rom));
        Suite(nameof(VerifyLookupStream1PuyoAndQuota), () => VerifyLookupStream1PuyoAndQuota(rom));
        Suite(nameof(VerifyLookupStream1VisualCatalogs), () => VerifyLookupStream1VisualCatalogs(rom));
        Suite(nameof(VerifyLookupStream1DeathDefinitions), () => VerifyLookupStream1DeathDefinitions(rom));
        Suite(nameof(VerifyLookupStream1Sciser), () => VerifyLookupStream1Sciser(rom));
        Suite(nameof(VerifyLookupStream1PowampMotion), () => VerifyLookupStream1PowampMotion(rom));
        Suite(nameof(VerifyLookupStream1CommonFrames), () => VerifyLookupStream1CommonFrames(rom));
        Suite(nameof(VerifyLookupStream1EnemyMovement), () => VerifyLookupStream1EnemyMovement(rom));
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1RomByte(rom, address), LookupStream1RomByte(rom, address + 1));
        foreach (SamusLiquidMedium medium in Enum.GetValues<SamusLiquidMedium>())
        {
            for (int variant = 0; variant < 4; variant++)
            {
                var actual = SamusVerticalMotion.Launch(medium, (variant & 1) != 0, ((variant & 2) != 0) ? SamusLaunchKind.WallJump : SamusLaunchKind.Jump);
                int address = 0x909eb9 + 12 * variant + 2 * SamusLiquidMediumCodec.ToNative(medium);
                AssertEqual(Word(address), actual.Whole, "stream1 native jump whole speed");
                AssertEqual(Word(address + 6), actual.Fraction, "stream1 native jump fractional speed");
            }
            var hurt = SamusVerticalMotion.Knockback(medium);
            AssertEqual(Word(0x909ee9 + 2 * SamusLiquidMediumCodec.ToNative(medium)), hurt.Whole, "stream1 native knockback whole speed");
            AssertEqual(Word(0x909eef + 2 * SamusLiquidMediumCodec.ToNative(medium)), hurt.Fraction, "stream1 native knockback fractional speed");
            var bomb = SamusVerticalMotion.BombJump(medium);
            AssertEqual(Word(0x909ef5 + 2 * SamusLiquidMediumCodec.ToNative(medium)), bomb.Whole, "stream1 native bomb whole speed");
            AssertEqual(Word(0x909efb + 2 * SamusLiquidMediumCodec.ToNative(medium)), bomb.Fraction, "stream1 native bomb fractional speed");
            var gravity = SamusVerticalMotion.Gravity(medium);
            AssertEqual(Word(0x909ea7 + 2 * SamusLiquidMediumCodec.ToNative(medium)), gravity.Whole, "stream1 native whole gravity");
            AssertEqual(Word(0x909ea1 + 2 * SamusLiquidMediumCodec.ToNative(medium)), gravity.Fraction, "stream1 native fractional gravity");
        }
        foreach (SamusLiquidMedium invalid in new[] { (SamusLiquidMedium)3, (SamusLiquidMedium)ushort.MaxValue })
        {
            for (int variant = 0; variant < 4; variant++)
                AssertThrows<InvalidOperationException>(() => SamusVerticalMotion.Launch(invalid, (variant & 1) != 0, ((variant & 2) != 0) ? SamusLaunchKind.WallJump : SamusLaunchKind.Jump), "stream1 invalid launch medium");
            AssertThrows<InvalidOperationException>(() => SamusVerticalMotion.Knockback(invalid), "stream1 invalid hurt medium");
            AssertThrows<InvalidOperationException>(() => SamusVerticalMotion.BombJump(invalid), "stream1 invalid bomb medium");
            AssertThrows<InvalidOperationException>(() => SamusVerticalMotion.Gravity(invalid), "stream1 invalid gravity medium");
        }
    }

    /// <summary>Reads source bytes for the broad pose and gameplay lookup comparison.</summary>
    private static byte LookupStream1RomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks Powamp wiggle offsets and balloon/body motion tables.</summary>
    internal static void VerifyLookupStream1PowampMotion(ISnesAddressSpace rom)
    {
        short Word(int address) => unchecked((short)(LittleEndianBytes.Word(LookupStream1PowampMotionRomByte(rom, address), LookupStream1PowampMotionRomByte(rom, address + 1))));
        // The wiggle is the cartridge's twelve offsets in order, the last followed by the first (#2375).
        AssertEqual(12, PowampWiggle.All.Count, "Powamp wiggle offsets");
        for (int phase = 0; phase < PowampWiggle.All.Count; phase++)
        {
            AssertEqual(Word(0xa8c1a1 + 2 * phase), PowampWiggle.All[phase].Offset, "Powamp native wiggle triangle");
            AssertEqual(PowampWiggle.All[(phase + 1) % 12], PowampWiggle.All[phase].Next, "Powamp wiggle takes the next offset");
        }
        AssertEqual(PowampWiggle.All[0], PowampWiggle.First, "Powamp wiggle starts at its centre");
        // The cartridge lists the balloon's height for each pose in the order each sequence shows them (#1845).
        for (int pose = 0; pose < 3; pose++)
        {
            AssertEqual(Word(0xa8c277 + 2 * pose), PowampMotion.BalloonHeight(new PowampBalloonInflation(PowampBalloonSequence.Inflating, pose + 1).Pose),
                "Powamp native rising balloon offset");
            AssertEqual(Word(0xa8c27d + 2 * pose), PowampMotion.BalloonHeight(new PowampBalloonInflation(PowampBalloonSequence.Sinking, pose + 1).Pose),
                "Powamp native sinking balloon offset");
        }
        CompassDirection[] spikeDirections = SuperMetroid.Verification.Enemies.Powamp.PowampSpikeCartridgeOrder.Directions;
        for (int place = 0; place < spikeDirections.Length; place++)
        {
            CompassDirection direction = spikeDirections[place];
            AssertEqual(Word(0x86d21a + 2 * place), PowampMotion.SpikeAcceleration(direction).X.InTwoHundredFiftySixthsOfPixel, $"Powamp native spike {direction} X acceleration");
            AssertEqual(Word(0x86d22a + 2 * place), PowampMotion.SpikeAcceleration(direction).Y.InTwoHundredFiftySixthsOfPixel, $"Powamp native spike {direction} Y acceleration");
        }
        AssertTrue(PowampMotionDefinitions.SpikeFiringOrder.SequenceEqual(spikeDirections.Reverse()),
            "Powamp fires its spikes from the cartridge table's last direction back to its first");
        AssertThrows<ArgumentOutOfRangeException>(() => PowampMotion.BalloonHeight((PowampBalloonPose)3), "Powamp balloon pose domain");
        AssertThrows<InvalidDataException>(() => PowampMotion.SpikeAcceleration((CompassDirection)spikeDirections.Length), "Powamp spike direction domain");
    }

    /// <summary>Reads Powamp native wiggle and balloon offset words.</summary>
    private static byte LookupStream1PowampMotionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks compiled Bull movement intervals and other enemy motion values.</summary>
    internal static void VerifyLookupStream1EnemyMovement(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1EnemyMovementRomByte(rom, address), LookupStream1EnemyMovementRomByte(rom, address + 1));
        for (ushort index = 0; index < 13; index++)
        {
            BullSpeedIntervals intervals = ((BullSpawnSettings)EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Bull, PopulationWords.Parameters(index, 0))).Intervals;
            AssertEqual(Word(0xa8d895 + 4 * index), intervals.Acceleration, "stream1 Bull acceleration interval");
            AssertEqual(Word(0xa8d897 + 4 * index), intervals.Deceleration, "stream1 Bull deceleration interval");
        }
        foreach (SamusProjectileDirection direction in SamusProjectileDirectionCodec.InAimOrder)
            AssertEqual(new TrigonometryAngle(Word(0xa8d871 + 2 * direction.ToAimIndex())), BullMovementDefinitions.ShotAngles.For(direction), "stream1 Bull compass angle");
        AssertThrows<InvalidDataException>(() => EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Bull, PopulationWords.Parameters(13, 0)), "stream1 Bull invalid interval");
        for (byte index = 0; index < 6; index++)
            AssertEqual(Word(0xa29f36 + 2 * index), CacatacTravelDistance(index), "stream1 Cacatac patrol blocks");
        foreach (byte invalid in new byte[] { 6, byte.MaxValue })
            AssertThrows<InvalidDataException>(() => CacatacTravelDistance(invalid), "stream1 Cacatac invalid patrol selector");
        for (ushort direction = 0; direction < 20; direction += 2)
            AssertEqual(Word(0x86d96a + direction), CacatacProjectileNativeListAddresses.Entry(
                CacatacNativeWords.SpikeDirections.Decode(direction, "Cacatac spike direction")), "stream1 Cacatac named direction program");
        foreach (ushort invalid in new ushort[] { 1, 19, 20, ushort.MaxValue })
            AssertEqual(false, CacatacNativeWords.SpikeDirections.TryDecode(invalid, out _), "stream1 Cacatac invalid direction selector");
    }

    /// <summary>Reads native Bull and related enemy motion words.</summary>
    private static byte LookupStream1EnemyMovementRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>The patrol distance the import decodes from a Cacatac's travel selector.</summary>
    private static int CacatacTravelDistance(int selector) =>
        ((CacatacSpawnSettings)EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Cacatac, PopulationWords.Parameters(0, selector))).TravelDistance;
    /// <summary>Checks the beam initializer's launch speeds against the bank-$90 speed words it reads, overreads included.</summary>
    internal static void VerifyLookupStream1ProjectileMotion(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1ProjectileMotionRomByte(rom, address), LookupStream1ProjectileMotionRomByte(rom, address + 1));
        var initialize = new Action<ProjectileLaunchInheritance, SamusProjectile>((records, launched) =>
            BeamFiring.InitializePowerBeamVelocity(records, launched.HeldShot(), InstalledFixtures.SamusOverreads.Projectiles.UnauthoredBeamFiring));
        var stationary = new ProjectileLaunchInheritance();
        for (ushort combination = 0; combination < 16; combination++)
        for (ushort direction = 0; direction < 10; direction++)
        {
            var slot = new SamusProjectile(0);
            slot.OccupyAsFired(SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(combination));
            slot.HeldShot().Direction = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeDirection(direction);
            initialize(stationary, slot);
            int speed = unchecked((short)Word(0x90c2d1 + combination * 4 + (direction is 1 or 3 or 6 or 8 ? 2 : 0)));
            AssertEqual(unchecked((short)(direction is 1 or 2 or 3 ? speed : direction is 6 or 7 or 8 ? -speed : 0)), slot.Velocity.X.InTwoHundredFiftySixthsOfPixel, "stream1 actual initializer X preserves adjacent missile-data overread");
            AssertEqual(unchecked((short)(direction is 0 or 1 or 8 or 9 ? -speed : direction is 3 or 4 or 5 or 6 ? speed : 0)), slot.Velocity.Y.InTwoHundredFiftySixthsOfPixel, "stream1 actual initializer Y preserves adjacent missile-data overread");
        }
    }

    /// <summary>Reads bank-$90 projectile speed and motion words.</summary>
    private static byte LookupStream1ProjectileMotionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks the full bank-$93 projectile selection table against compiled words.</summary>
    internal static void VerifyLookupStream1Selection(ISnesAddressSpace rom)
    {
        SamusProjectileProgramsVerification.VerifySelectionWords(
            address => LittleEndianBytes.Word(LookupStream1SelectionRomByte(rom, address), LookupStream1SelectionRomByte(rom, address + 1)),
            "stream1 original projectile selection");
    }

    /// <summary>Reads bank-$93 projectile selector words.</summary>
    private static byte LookupStream1SelectionRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks bomb-spread launch roles and their native slot-specific values.</summary>
    internal static void VerifyLookupStream1LaunchRoles(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1LaunchRolesRomByte(rom, address), LookupStream1LaunchRolesRomByte(rom, address + 1));
        for (int index = 0; index < 5; index++)
        {
            var launch = SamusBombSpreadLaunchDefinitions.LeftToRight[index];
            AssertEqual(Word(0x90d8cf + index * 2), launch.FuseTimer, "spread native fuse");
            AssertEqual(BombSpreadLaunchWords.Unpack(Word(0x90d8d9 + index * 2)), launch.HorizontalVelocity, "spread native direction/magnitude X");
            // The game negates the speed word alone and keeps the subspeed as the launch velocity's fraction (#2459).
            AssertEqual(SignedSixteenSixteen.FromParts(-Word(0x90d8e3 + index * 2), Word(0x90d8ed + index * 2)), launch.VerticalVelocity,
                "spread native negated whole Y with its fraction");
        }
        HibashiEruptionStep[] hibashiSteps = HibashiAnimationsToolingConstants.EruptionSteps();
        for (int index = 0; index < hibashiSteps.Length; index++)
        {
            var frame = hibashiSteps[index];
            AssertEqual(Word(0xa68dbb + index * 2), frame.YOffset, "Hibashi native collision rise");
            AssertEqual(Word(0xa68de7 + index * 2), frame.YRadius, "Hibashi native collision radius");
        }
        for (ushort selector = 0; selector < 6; selector++)
        {
            AssertEqual(Word(0xa2e5ef + selector * 2), NativeEnemyAnimationOracle.EntryOf(DragonNativeListAddresses.Entries, DragonNativeWords.Animations.Decode(selector, "Dragon animation ordinal")), "Dragon native phase/facing program");
            var role = EscapeEtecoons.Initialization(SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.EscapeEtecoonRole(selector));
            int offset = selector & ~1;
            AssertEqual(Word(0xb3e718 + offset), role.Position.X, "Etecoon native role X");
            AssertEqual(Word(0xb3e71e + offset), role.Position.Y, "Etecoon native role Y");
            AssertEqual(Word(0xb3e724 + offset), AnimalsNativeWords.EscapeEtecoonActions.Word(role.Action), "Etecoon native role action");
            AssertEqual(Word(0xb3e72a + offset), EscapeEtecoonNativeListAddresses.Entries.Single(entry => entry.Program == role.Animation).Entry, "Etecoon native role program");
            AssertEqual(Word(0xb3e730 + offset), (role.Direction == HorizontalDirection.Left ? role.Speed.Negated() : role.Speed).RawValue, "Etecoon native role speed");
        }
        foreach (ushort invalid in new ushort[] { 6, 7, ushort.MaxValue })
        {
            AssertEqual(false, DragonNativeWords.Animations.TryDecode(invalid, out _), "Dragon invalid selector rejection");
            AssertThrows<InvalidDataException>(() => EscapeEtecoons.Initialization(SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.EscapeEtecoonRole(invalid)), "Etecoon invalid role rejection");
        }
    }

    /// <summary>Reads native bomb-spread launch values by slot.</summary>
    private static byte LookupStream1LaunchRolesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks empty extended enemy frames across their supported native banks.</summary>
    private static void VerifyLookupStream1CommonFrames(ISnesAddressSpace rom)
    {
        byte[] originalBanks = [0xa0, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3];
        AssertTrue(originalBanks.SequenceEqual(CommonEnemyEmptyFrameDefinitionsTooling.SupportedBanks), "common empty-frame original bank order");
        for (int bank = 0; bank <= byte.MaxValue; bank++)
        {
            bool expected = originalBanks.Contains((byte)bank);
            AssertEqual(expected, CommonEnemyEmptyFrameToolingDefinitionsConstants.HasFrame((byte)bank, 0x804f), "common empty extended-frame exact bank domain");
            AssertEqual(expected, CommonEnemyEmptyFrameToolingDefinitions.HasEmptySpritemap((byte)bank, 0x804d), "common empty OAM exact bank domain");
            AssertTrue(!CommonEnemyEmptyFrameToolingDefinitionsConstants.HasFrame((byte)bank, 0x8050), "common empty frame excludes neighboring pointer");
            if (expected)
            {
                AssertEqual((byte)0, LookupStream1CommonFramesRomByte(rom, ((int)new SnesAddress(checked((byte)bank), checked(0x804d)))), "native common OAM has zero components");
                AssertEqual((byte)1, LookupStream1CommonFramesRomByte(rom, ((int)new SnesAddress(checked((byte)bank), checked(0x804f)))), "native common extended frame has one component");
            }
        }
        AssertEqual((ushort)NativeEnemyProjectileInstruction.Delete, LittleEndianBytes.Word(LookupStream1CommonFramesRomByte(rom, 0x8684fc), LookupStream1CommonFramesRomByte(rom, 0x8684fd)), "shared projectile delete native instruction");
    }

    /// <summary>Reads empty extended enemy frame headers across supported banks.</summary>
    private static byte LookupStream1CommonFramesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks Sciser directional visual frames against native pointer and spritemap data.</summary>
    private static void VerifyLookupStream1Sciser(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1SciserRomByte(rom, address), LookupStream1SciserRomByte(rom, address + 1));
        var frames = SuperMetroid.Core.Enemies.Artwork.SciserVisualDefinitions.Frames();
        AssertEqual(12, frames.Length, "Sciser visual catalog size");
        string[] names = ["right", "left", "down", "up"];
        for (int surface = 0; surface < 4; surface++)
        for (int frame = 0; frame < 3; frame++)
        {
            var actual = frames[surface * 3 + frame];
            ushort pointer = Word(0xa39681 + surface * 24 + frame * 4);
            AssertEqual(pointer, actual.Pointer, "native Sciser visual identity");
            AssertEqual((byte)0xa3, actual.Bank, "Sciser bank");
            AssertEqual($"sciser_upside_{names[surface]}_{frame}", actual.Name, "Sciser stable editable name");
            AssertEqual((ushort)4, Word(0xa30000 | pointer), "native Sciser four-object record size");
        }
    }

    /// <summary>Reads native Sciser directional frames and spritemap pointers.</summary>
    private static byte LookupStream1SciserRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks death-sequence initial frames and movement-type data against native tables.</summary>
    private static void VerifyLookupStream1DeathDefinitions(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1DeathDefinitionsRomByte(rom, address), LookupStream1DeathDefinitionsRomByte(rom, address + 1));
        SamusMovementType[] movements = Enum.GetValues<SamusMovementType>();
        AssertEqual(SamusMovementTypeNumbers.Named.Count(), movements.Length, "death retail movement domain: every movement type a pose selects");
        // The cartridge numbers the death pose's frames through its whole animation; the port names a segment's frame (#2530).
        foreach (SamusMovementType movement in movements)
        foreach (SamusPoseId deathPose in new[] { SamusPoseId.DeathSequenceRightPose, SamusPoseId.DeathSequenceLeftPose })
            AssertEqual(LookupStream1DeathDefinitionsRomByte(rom, 0x9bb420 + SamusMovementTypeNativeWords.Byte(movement)),
                SamusAnimationNumbers.Number(deathPose, SamusDeathSequenceFrames.InitialFrame(movement)), $"native movement death phase of {movement} in {deathPose}");
        // The cartridge uploads the explosion as five pages; the atlas keeps them in that upload order.
        for (int page = 0; page < SamusDeathTileAtlasFormat.SegmentCount; page++)
        {
            int explosionPage = SamusDeathTileAtlasFormat.ExplosionPageOfAtlasPage(page);
            AssertEqual(0x9b8000 + explosionPage * SamusDeathTileAtlasFormat.PageByteCount, 0x9b0000 | Word(0x9bb7bf + page * 2),
                "native death page source is the atlas page's explosion page");
            AssertEqual(0x6000 + explosionPage * SamusDeathTileAtlasFormat.PageByteCount / 2, (int)Word(0x9bb7c9 + page * 2),
                "native death page destination is the atlas page's explosion page");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 5, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = SamusDeathTileAtlasFormat.ExplosionPageOfAtlasPage(invalid), "death page exact domain");
    }

    /// <summary>Reads native Samus death-sequence selection data.</summary>
    private static byte LookupStream1DeathDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks charge-flare sprite selectors and extracted visual records against native pointers.</summary>
    internal static void VerifyLookupStream1VisualCatalogs(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1VisualCatalogsRomByte(rom, address), LookupStream1VisualCatalogsRomByte(rom, address + 1));
       var nativePointers = new List<ushort>();
        byte[] json = ChargeFlareSpriteExtractor.Extract(rom);
        var stock = ChargeFlareSprites.Load(new MemoryStream(json));
        for (ushort selector = 0; selector < 54; selector++)
        {
            ushort pointer = Word(0x93a1a1 + selector * 2);
            AssertEqual(pointer, ProjectileSpriteNative.Address(ChargeFlareSpriteDefinitions.NativeSelector(selector)), "native charge flare phased selector");
            if (!nativePointers.Contains(pointer)) nativePointers.Add(pointer);
            var expected = new OamBuffer();
            var actual = new OamBuffer();
            DrawImportedFlareSpritemap((CartridgeImportAddressSpace)rom, expected, selector, new(100, 100));
            stock.Draw(selector, actual, new(100, 100));
            AssertTrue(expected.ImageEquals(actual),
                "calculated flare selector draws exact native composition");
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, "calculated flare native OAM cursor");
        }
        AssertEqual(28, nativePointers.Count, "native unique flare identity count");
        AssertTrue(nativePointers.SequenceEqual(ChargeFlareSpriteDefinitions.NativePointers), "native flare identity order preserved");
        for (int index = 0; index < nativePointers.Count; index++)
            AssertEqual((ushort)(index < 3 ? 1 : index == 3 ? 4 : 3), Word(0x930000 | nativePointers[index]), "native flare object record sizes");
        foreach (int invalid in new[] { int.MinValue, -1, 54, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = ChargeFlareSpriteDefinitions.NativeSelector(invalid), "charge flare selector bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 28, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = ChargeFlareSpriteDefinitions.NativePointers[invalid], "flare pointer bounds");
        for (int digit = 0; digit < 10; digit++)
        {
            ushort pointer = EscapeTimerPresentationDefinitions.DigitSpritemapPointer(digit);
            AssertEqual(Word(0x809fd4 + digit * 2), pointer, "native escape timer digit pointer");
            AssertEqual((ushort)2, Word(0x800000 | pointer), "native two-object digit record");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 10, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EscapeTimerPresentationDefinitions.DigitSpritemapPointer(invalid), "escape digit bounds");
        var dragon = DragonVisualDefinitions.Frames();
        ushort[] operands = [0xe59d, 0xe5a3, 0xe5a7, 0xe5af, 0xe5b5, 0xe5b9, 0xe5c1, 0xe5c5, 0xe5c9, 0xe5d9, 0xe5dd, 0xe5e1];
        string[] names = ["body_idle_left", "wing_left_0", "wing_left_1", "body_idle_right", "wing_right_0", "wing_right_1",
            "body_attack_left_0", "body_attack_left_1", "body_attack_left_2", "body_attack_right_0", "body_attack_right_1", "body_attack_right_2"];
        AssertEqual(12, dragon.Length, "Dragon visual identity count");
        for (int index = 0; index < dragon.Length; index++)
        {
            AssertEqual(Word(0xa20000 | operands[index]), dragon[index].Pointer, "native Dragon visual pointer");
            AssertEqual((byte)0xa2, dragon[index].Bank, "Dragon visual bank");
            AssertEqual("dragon_" + names[index], dragon[index].Name, "Dragon stable editable identity");
            AssertEqual((ushort)(index is 1 or 2 or 4 or 5 ? 1 : 8), Word(0xa20000 | dragon[index].Pointer), "native Dragon record size");
        }
    }

    /// <summary>Reads native charge-flare visual selector and sprite data.</summary>
    private static byte LookupStream1VisualCatalogsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks Puyo program data and the native projectile quota selector table.</summary>
    private static void VerifyLookupStream1PuyoAndQuota(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1PuyoAndQuotaRomByte(rom, address), LookupStream1PuyoAndQuotaRomByte(rom, address + 1));
        for (ushort argument = 0; argument <= 24; argument += 2)
        {
            ushort pointer = Word(0x84db28 + argument);
            AssertEqual(pointer, SuperMetroid.Core.Rooms.RoomObjects.RoomObjectFrameBehaviorNativeRoutines.MetroidsCleared(argument), "native Metroid quota dispatcher identity");
            if (argument < 18)
            {
                // A no-op entry's placement is an escape room's held place, which marks no event (#2426).
                AssertTrue(SuperMetroid.Core.Rooms.RoomObjects.RoomObjectRoomArgumentWords.PlacedKind(
                        SuperMetroid.Core.Rooms.RoomObjects.RoomObjectKind.MetroidRoomClearance, argument)
                    == SuperMetroid.Core.Rooms.RoomObjects.RoomObjectKind.EscapeRoomHeldPlace, "nine no-op quota identities place a held place");
                AssertEqual((byte)0x60, LookupStream1PuyoAndQuotaRomByte(rom, 0x840000 | pointer), "native distinct no-op RTS");
                continue;
            }
            var eventNumber = (SuperMetroid.Core.Rooms.RoomObjects.RoomObjectRoomArgumentWords.Decode(
                SuperMetroid.Core.Rooms.RoomObjects.RoomObjectKind.MetroidRoomClearance, argument)
                as SuperMetroid.Core.Rooms.RoomObjects.WatchedMetroidRoom
                ?? throw new InvalidOperationException($"Metroids-cleared argument {argument} decodes to no watch.")).Cleared;
            AssertEqual(Word(0x840000 | (pointer + 9)), eventNumber.Number(), "native quota observer event operand");
        }
        foreach (ushort invalid in new ushort[] { 1, 17, 25, 26, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => SuperMetroid.Core.Rooms.RoomObjects.RoomObjectFrameBehaviorNativeRoutines.MetroidsCleared(invalid), "quota pointer argument domain");
            AssertThrows<InvalidDataException>(() => SuperMetroid.Core.Rooms.RoomObjects.RoomObjectRoomArgumentWords.Decode(
                SuperMetroid.Core.Rooms.RoomObjects.RoomObjectKind.MetroidRoomClearance, invalid), "quota event argument domain");
        }
    }

    /// <summary>Reads Puyo instruction and projectile quota source words.</summary>
    private static byte LookupStream1PuyoAndQuotaRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks Metroid body frames and instruction presentation layout.</summary>
    private static void VerifyLookupStream1MetroidLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1MetroidLayoutRomByte(rom, address), LookupStream1MetroidLayoutRomByte(rom, address + 1));
        var frames = MetroidVisualDefinitions.Frames();
        AssertEqual(4, frames.Length, "Metroid body identities");
        for (int frame = 0; frame < 4; frame++)
        {
            AssertEqual(Word(0xa3e9d1 + frame * 4), frames[frame].Pointer, "native Metroid body identity");
            AssertEqual((byte)0xa3, frames[frame].Bank, "Metroid body bank");
            AssertEqual($"metroid_body_{frame}", frames[frame].Name, "Metroid stable editable name");
            AssertEqual((ushort)(frame == 1 ? 6 : 8), Word(0xa30000 | frames[frame].Pointer), "native Metroid OAM record size");
        }
    }

    /// <summary>Reads native Metroid frame and instruction presentation data.</summary>
    private static byte LookupStream1MetroidLayoutRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks Samus pose policy tables across the complete byte-valued pose domain.</summary>
    private static void VerifyLookupStream1SamusPolicyDomains(ISnesAddressSpace rom)
    {
        // The crash echo departures, including the drained poses that read past their table, are checked by the
        // Shinespark family's own crash departure check (#1500).
        foreach (byte invalid in new byte[] { 28, byte.MaxValue })
            AssertEqual(false, SamusMovementTypeNativeWords.Types.TryDecode(invalid, out _), "movement byte exact domain");
        var undefinedMovement = (SamusMovementType)(-1);
        AssertThrows<ArgumentOutOfRangeException>(() => SamusHudObservations.SelectionMode(undefinedMovement), "HUD movement exact domain");
        AssertThrows<InvalidDataException>(() => SamusAtmosphericEffects.WaterSplashFor(undefinedMovement), "splash exact movement domain");
        foreach (ushort invalid in new ushort[] { 10, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusAtmosphericEffects.IsRunningFootContact(new SamusAnimationFrame(SamusAnimationSegmentName.Run, invalid)), "foot-contact exact frame domain");
        foreach (var (header, room) in new[] { new VerifyLookupStream1SamusPolicyDomainsCase(0x91f8, 0), new VerifyLookupStream1SamusPolicyDomainsCase(0x93fe, 5), new VerifyLookupStream1SamusPolicyDomainsCase(0x948c, 7), new VerifyLookupStream1SamusPolicyDomainsCase(0x94fd, 9), new VerifyLookupStream1SamusPolicyDomainsCase(0x9552, 10), new VerifyLookupStream1SamusPolicyDomainsCase(0x957d, 11), new VerifyLookupStream1SamusPolicyDomainsCase(0x95a8, 12), new VerifyLookupStream1SamusPolicyDomainsCase(0x95ff, 14) })
            AssertEqual((byte)room, LookupStream1SamusPolicyDomainsRomByte(rom, 0x8f0000 | header), "native atmospheric room header identity");
    }

    /// <summary>Reads native pose policy bytes, including the special drained poses.</summary>
    private static byte LookupStream1SamusPolicyDomainsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks arm-cannon tile source words and stock source selection across directions.</summary>
    internal static void VerifyLookupStream1ArmCannonTileSources(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1ArmCannonTileSourcesRomByte(rom, address), LookupStream1ArmCannonTileSourcesRomByte(rom, address + 1));
        var nativeSources = new SortedSet<ushort>();
        for (int direction = 0; direction < 10; direction++)
        {
            int list = 0x900000 | Word(0x90c7a5 + direction * 2);
            for (int frame = 1; frame < 4; frame++) nativeSources.Add(Word(list + frame * 2));
        }
        ushort[] expected = nativeSources.ToArray();
        var sources = SamusArmCannonArtworkFormat.TileSourcePointers;
        AssertEqual(expected.Length, sources.Count, "Calculated cannon source count matches distinct native list operands");
        AssertTrue(expected.SequenceEqual(sources), "Calculated cannon source enumeration retains original ascending tile order");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], sources[index], "Calculated cannon source indexing retains native identity");
        for (int word = 0; word <= ushort.MaxValue; word++)
        {
            bool native = Array.IndexOf(expected, (ushort)word) >= 0;
            AssertEqual(native, ArmCannonTileSources.TryDecode(word, out _), "Calculated cannon decoding preserves complete word domain");
            AssertEqual(native, sources.Contains((ushort)word), "Calculated cannon membership rejects interior addresses");
        }
        foreach (int invalid in new[] { int.MinValue, -1, expected.Length, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = sources[invalid], "Calculated cannon source index bounds preserve array contract");
        byte[] json = File.ReadAllBytes(Path.Combine(CartridgeImportContent.ImportData(GameInstallationLayout.SamusBodyDirectoryName), SamusArmCannonArtworkFormat.ArtworkFileName));
        byte[] png = File.ReadAllBytes(Path.Combine(CartridgeImportContent.ImportData(GameInstallationLayout.SamusBodyDirectoryName), SamusArmCannonArtworkFormat.TileFileName));
        SamusArmCannonCoverCharacters cover = InstalledSamusArmCannonArtworkToolingConstants.LoadWithCover(new MemoryStream(json), new MemoryStream(png));
        SamusArmCannonSourceArtwork stock = cover.Cannon;
        Suite(nameof(VerifySelectors), () => VerifySelectors(stock));
        foreach (int invalid in new[] { int.MinValue, -1, 10, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.CoverAppearance(SuperMetroid.AssetExtraction.Samus.SamusProjectileDirectionCodec.FromAimIndex(invalid)), "Cannon cover appearance bounds remain exact");
            AssertThrows<ArgumentOutOfRangeException>(() => stock.TileSource(invalid, 0), "Cannon tile direction bounds remain exact");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.TileSource(0, invalid), "Cannon cover frame bounds remain exact");
        Verify(cover);
        AssertTrue(!cover.TryResolveTile(0x9b0000 | expected[0], 32, out _), "Cannon transfer retains native bank boundary");
        AssertTrue(!cover.TryResolveTile(0x9a0000 | expected[0], 31, out _), "Cannon transfer retains native byte-count boundary");
        AssertTrue(!cover.TryResolveTile((0x9a0000 | expected[0]) + 1, 32, out _), "Cannon transfer rejects interior source address");

        void VerifySelectors(SamusArmCannonSourceArtwork catalog)
        {
            for (int direction = 0; direction < 10; direction++)
            {
                // The cover appearance has no packed word: its character, palette row, priority and mirroring are
                // compared with the native attribute word decoded by the import pipeline's codec.
                AssertEqual(new SnesObjAttributeWord(Word(0x90c791 + direction * 2)).ToSinglePart(), catalog.CoverAppearance(SuperMetroid.AssetExtraction.Samus.SamusProjectileDirectionCodec.FromAimIndex(direction)), "Installed cover appearances match the native attribute words");
                int list = 0x900000 | Word(0x90c7a5 + direction * 2);
                for (int frame = 0; frame < 4; frame++)
                    AssertEqual(Word(list + frame * 2), catalog.TileSource(direction, frame), "Calculated cannon orientation/frame preserves native selections");
            }
        }

        void Verify(SamusArmCannonCoverCharacters catalog)
        {
            for (int index = 0; index < expected.Length; index++)
            {
                int source = 0x9a0000 | expected[index];
                AssertTrue(catalog.TryResolveTile(source, 32, out var tile), "Actual cannon transfer resolves every calculated identity");
                for (int offset = 0; offset < 32; offset++)
                    AssertEqual(LookupStream1ArmCannonTileSourcesRomByte(rom, source + offset), tile.Span[offset], "Calculated identity retains original tile bytes");
            }
        }
    }

    /// <summary>Reads native arm-cannon tile source pointers by direction.</summary>
    private static byte LookupStream1ArmCannonTileSourcesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>
    /// Appends one sprite piece in the canonical field order every Samus artwork content identity frames its pieces
    /// with: offset, size, character, both mirrorings, palette row and priority.
    /// </summary>
    private static void AppendCanonicalSpritePart(SelectedPresentationHash content, SpritePart part)
    {
        content.Append("x", part.Offset.X);
        content.Append("y", part.Offset.Y);
        content.Append("large", part.Large ? 1 : 0);
        content.Append("character", part.Character);
        content.Append("flip-horizontal", part.FlipsHorizontally ? 1 : 0);
        content.Append("flip-vertical", part.FlipsVertically ? 1 : 0);
        content.Append("palette", part.Palette);
        content.AppendName("depth", SuperMetroid.Core.Assets.ActorDepthKeys.Installed.Of(part.Depth));
    }

    /// <summary>A cover appearance's single small piece at the origin, from its document fields.</summary>
    private static SpritePart CannonCoverAppearance(int tileRow, int tileColumn, int palette, SuperMetroid.Core.Assets.ActorDepth depth, bool flipHorizontally, bool flipVertically) =>
        SpritePart.Single(tileRow * SpriteArtwork.CharactersPerRow + tileColumn, palette, depth,
            (flipHorizontally ? PictureFlip.Horizontal : PictureFlip.None) | (flipVertically ? PictureFlip.Vertical : PictureFlip.None));
    /// <summary>Checks arm-cannon drawing commands and extracted placement through native OAM results.</summary>
    internal static void VerifyLookupStream1CannonDrawingControls(ISnesAddressSpace rom)
    {
        byte[] json = File.ReadAllBytes(Path.Combine(CartridgeImportContent.ImportData(GameInstallationLayout.SamusBodyDirectoryName), SamusArmCannonArtworkFormat.ArtworkFileName));
        var document = RequiredJson.Deserialize<SamusArmCannonArtworkDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new SamusPoseIdJsonConverter(),
                new SamusProjectileDirectionJsonConverter(), new SamusArmCannonCoverOrientationJsonConverter() } });
        SamusArmCannonSourceArtwork Load(SamusArmCannonArtworkDocument value) => SamusArmCannonSourceArtwork.FromPlacement(
            SamusArmCannonSourceArtwork.LoadPlacement(new MemoryStream(SamusArmCannonSourceArtwork.Write(value))));
        var stock = Load(document);
        const int start = 0xC9D9, count = SamusArmCannonArtworkFormat.DrawingDataByteCount;
        for (int index = 0; index < count; index++)
        {
            ushort address = (ushort)(start + index);
            AssertEqual(LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | address), document.ReadDrawingByteAt(address), "cannon complete installed byte window");
        }
        foreach (ushort address in new ushort[] { 0, start - 1, start + count, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => document.ReadDrawingByteAt(address), "cannon drawing bounds remain exact");
        Console.WriteLine($"Cannon drawing: the {count}-byte extracted window holds the cartridge's bytes and its bounds are exact.");
    }

    /// <summary>Reads native arm-cannon drawing control and placement words.</summary>
    private static byte LookupStream1CannonDrawingControlsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks atmospheric sprite attributes and shared frame words.</summary>
    internal static void VerifyLookupStream1AtmosphericAttributes(ISnesAddressSpace rom)
    {
        // The appearances are domain pieces; the native words are decoded by the import pipeline's attribute codec and
        // compared by character, palette row, priority and mirroring.
        ushort[] NativeWords(int address) => Enumerable.Range(0, 4).Select(frame =>
            LittleEndianBytes.Word(LookupStream1AtmosphericAttributesRomByte(rom, address + frame * 2), LookupStream1AtmosphericAttributesRomByte(rom, address + frame * 2 + 1))).ToArray();
        ushort[] oneWords = NativeWords(0x908C0F), sharedWords = NativeWords(0x908C17);
        SpritePart[] one = [.. oneWords.Select(word => new SnesObjAttributeWord(word).ToSinglePart())];
        SpritePart[] shared = [.. sharedWords.Select(word => new SnesObjAttributeWord(word).ToSinglePart())];
        var stock = new SamusAtmosphericSourceArtwork(one, shared);
        for (int kind = 0; kind < 2; kind++)
        for (int frame = 0; frame < 4; frame++)
        {
            AssertTrue(stock.TryResolve(kind == 0 ? SamusAtmosphericPictures.FootstepSplash : SamusAtmosphericPictures.LavaAndDust, frame, out SpritePart installed), "atmospheric installed frame resolves");
            AssertEqual((kind == 0 ? one : shared)[frame], installed, "atmospheric installed appearance matches native");
        }
        AssertTrue(!SamusAtmosphericTypeCodec.TryDecode(2, out _), "atmospheric null pointer remains outside installed domain");
        AssertTrue(!stock.TryResolve(SamusAtmosphericPictures.FootstepSplash, 4, out _), "atmospheric next frame rejected");
        Console.WriteLine("Atmospheric attributes: eight installed native appearances and the canonical stock identity pass.");
    }

    /// <summary>Reads native atmospheric sprite attribute words.</summary>
    private static byte LookupStream1AtmosphericAttributesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks extracted escape typewriter glyph and placement data against native text drawing.</summary>
    internal static void VerifyLookupStream1EscapeText(ISnesAddressSpace rom)
    {
        byte[] json = EscapeTypewriterExtractor.Extract(rom);
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var document = RequiredJson.Deserialize<EscapeTypewriterDocument>(json, options);
        EscapeTypewriterPresentation Load(EscapeTypewriterDocument value)
        {
            using var output = new MemoryStream();
            EscapeTypewriterExtractor.Write(output, value);
            output.Position = 0;
            return EscapeTypewriterPresentationFile.Load(output);
        }
        var stock = Load(document);
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1EscapeTextRomByte(rom, address), LookupStream1EscapeTextRomByte(rom, address + 1));
        int totalFrames = 0;
        foreach (EscapeTypewriterMessageId id in new[] { EscapeTypewriterMessageId.Ceres, EscapeTypewriterMessageId.Zebes })
        {
            var program = stock.Get(id);
            var original = document.Messages[id.ToString()].Lines;
            AssertEqual(original.Length, program.Lines.Count, "escape text installed line grouping");
            for (int index = 0; index < original.Length; index++)
                AssertEqual(new EscapeTypewriterLine(original[index].Start, original[index].Text), program.Lines[index], "escape text installed line");
            // Independently interpret original control words and characters; the production
            // side receives only the installed calculated view and no cartridge reader.
            ushort tileBase = id == EscapeTypewriterMessageId.Ceres ? (ushort)0x3582 : (ushort)0x2610;
            // The program's words are VRAM tilemap words; the port types at the pixel of the layer that tilemap word's cell shows at.
            int tilemapWord = id == EscapeTypewriterMessageId.Ceres ? GameplayVramLayout.Bg1TilemapWord : GameplayVramLayout.Bg2TilemapWord;
            var actual = new EscapeTypewriterState(program);
            var actualVram = new SnesVram(); var expectedVram = new SnesVram();
            int cursor = EscapeTypewriterDefinitions.SourceAddress(id), glyphs = 0;
            ushort destination = 0, delay = 0, timer = 0;
            bool done = false;
            for (int frame = 0; !done && frame < 1000; frame++)
            {
                bool click = false;
                if (timer != 0) timer--;
                else
                {
                    timer = delay;
                    while (true)
                    {
                        ushort command = Word(cursor);
                        if (command == 0) { done = true; break; }
                        if (command == 1) { delay = Word(cursor + 2); cursor += 4; continue; }
                        if (command == 13) { destination = Word(cursor + 2); cursor += 4; continue; }
                        byte character = LookupStream1EscapeTextRomByte(rom, cursor++);
                        if (character != ' ')
                        {
                            int glyph = character == '!' ? '[' : character;
                            expectedVram.ExecuteWordTransfer([(ushort)(tileBase + glyph - 'A')], checked((ushort)(destination - tilemapWord)), 1);
                            click = ++glyphs % 2 == 0;
                        }
                        destination++;
                        break;
                    }
                }
                // The typewriter's schedule shows in which update types each character: the oracle's reloaded countdown
                // and glyph count are compared through the cell, click and image of every update, not as counters.
                EscapeTypewriterStep step = actual.StepInto(actualVram);
                AssertEqual(done, step is EscapeTypewriterStep.Complete, "escape text native completion");
                AssertEqual(destination - tilemapWord, SuperMetroid.Core.Rooms.Backgrounds.TwoScreenTilemapCells.IndexOf(actual.Corner), "escape text native cell");
                AssertEqual(click, step is EscapeTypewriterStep.TypedWithClick, "escape text native click order");
                AssertTrue(expectedVram.ImageEquals(actualVram), "escape text native per-character VRAM");
                totalFrames++;
            }
            AssertTrue(done, "escape text native program terminates");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Get((EscapeTypewriterMessageId)(-1)), "escape text rejects absent program");
        Console.WriteLine($"Escape text: five installed native lines and {totalFrames} actual native-oracle frames pass.");
    }

    /// <summary>Reads escape typewriter glyph and presentation bytes.</summary>
    private static byte LookupStream1EscapeTextRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks Samus visor color extraction keeps the native colours.</summary>
    internal static void VerifyLookupStream1VisorColors(ISnesAddressSpace rom)
    {
        var document = RequiredJson.Deserialize<SamusVisorColorDocument>(
            SamusVisorColorExtractor.Extract(rom), new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        InstalledSamusVisorColors Load(SamusVisorColorDocument value) =>
            SamusVisorColorFile.Read(new MemoryStream(SamusVisorColorFile.Write(value)));
        var stock = Load(document);
        ushort Native(int index)
        {
            int address = SamusSourceDefinitions.VisorColors.Value + index * 2;
            return LittleEndianBytes.Word(LookupStream1VisorColorsRomByte(rom, address), LookupStream1VisorColorsRomByte(rom, address + 1));
        }
        for (int index = 0; index < SamusVisorColorFormat.ColorCount; index++)
            AssertEqual(Native(index), stock.Resolve(index), $"visor native colour {index} is kept as installed");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(-1), "visor rejects negative index");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(6), "visor rejects next index");
        Console.WriteLine("Visor: six native colours kept as installed and the index bounds pass.");
    }

    /// <summary>Reads native Samus visor color words.</summary>
    private static byte LookupStream1VisorColorsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks that the installed world-map foreground atlas uploads the native characters whole.</summary>
    internal static void VerifyLookupStream1WorldForeground(ISnesAddressSpace rom)
    {
        byte[] Read(int address, int count) => Enumerable.Range(0, count).Select(i => LookupStream1WorldForegroundRomByte(rom, address + i)).ToArray();
        byte[] front = Read(MapSourceDefinitions.Menu.ForegroundCharacters, WorldMapArtworkFormat.ForegroundBytes);
        byte[] back = Read(MapSourceDefinitions.Menu.BackgroundCharacters, WorldMapArtworkFormat.BackgroundBytes);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(front, 4, 16, out Extent16<ImagePixels> size);
        byte[] backPixels = SnesGraphics.DecodePlanarTiles(back, 2, 16, out Extent16<ImagePixels> backSize);
        byte[] Png(byte[] selected, int selectedHeight, int colors)
        {
            using var png = new MemoryStream(); IndexedPng.Write(png, new(size.X, checked((ushort)selectedHeight)), selected, SnesGraphics.DiagnosticPalette(colors));
            return png.ToArray();
        }
        var stock = WorldMapCharacters.Read(new MemoryStream(Png(pixels, size.Y, 16)), new MemoryStream(Png(backPixels, backSize.Y, 4)));
        var vram = new SnesVram(); stock.LoadTo(vram);
        AssertTrue(vram.Bytes.Slice(WorldMapArtworkFormatConstants.ForegroundDestination, front.Length).SequenceEqual(front), "Complete foreground upload holds the installed pixels");
        AssertTrue(vram.Bytes.Slice(WorldMapArtworkFormatConstants.BackgroundDestination, back.Length).SequenceEqual(back), "Completed BG3 remains exact");
        Console.WriteLine("World foreground: the installed atlas uploads the native characters whole.");
    }

    /// <summary>Reads world-map BG3 foreground tile bytes.</summary>
    private static byte LookupStream1WorldForegroundRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks projectile collision-radius words across the timed instruction layouts.</summary>
    internal static void VerifyLookupStream1ProjectileRadiusLayout(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1ProjectileRadiusLayoutRomByte(rom, address), LookupStream1ProjectileRadiusLayoutRomByte(rom, address + 1));
        var expected = new List<ushort>();
        for (int address = 0x9386db; address < 0x93a1a1;)
        {
            if (UnusedProjectileListEnd(address) is { } unusedEnd)
            {
                address = unusedEnd;
                continue;
            }
            ushort word = Word(address);
            if (address == 0x93a117)
            {
                AssertEqual((ushort)0, word, "Native intervening empty spritemap record");
                address += 2;
            }
            else if ((word & 0x8000) == 0)
            {
                expected.Add(unchecked((ushort)address));
                address += 8;
            }
            else if (word == (ushort)SamusProjectileInstruction.GoTo) address += 4;
            else
            {
                AssertEqual((ushort)SamusProjectileInstruction.Delete, word, "Only native terminal delete remains");
                address += 2;
            }
        }
        AssertEqual(795, expected.Count, "Independent native timed-record count outside the two unused lists");
        AssertTrue(SamusProjectileFrameDefinitions.TimedRecordAddresses.SequenceEqual(expected), "Exact calculated pointer domain/order");
        AssertEqual(expected.Count, SamusProjectileFrameDefinitions.TimedRecordAddresses.Count, "Calculated pointer view count");
        for (int index = 0; index < expected.Count; index++)
        {
            ushort pointer = expected[index];
            AssertEqual(pointer, SamusProjectileFrameDefinitions.TimedRecordAddresses[index], "Calculated indexed pointer");
            Extent16<RoomPixels> radius = SamusProjectileFrameDefinitions.Radius(SamusProjectileProgramsVerification.RecordAt(pointer));
            AssertEqual(Word(0x930000 | (pointer + 4)), LittleEndianBytes.Word((byte)radius.X, (byte)radius.Y), "Typed record radii are the native bytes");
        }
        ProjectileFrameBindingDocument document = NativeFrameBindings(rom, expected);
        var guard = NewLookupFlareForbiddenBus();
        int interpreted = 0, bombInterpreted = 0;
        var bindings = ProjectileFrameBindingFile.Load(new MemoryStream(ProjectileFrameBindingFile.Write(document)));
        var shots = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: bindings), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        var bombs = new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: bindings));
        foreach (ushort pointer in expected)
        {
            ushort duration = Word(0x930000 | pointer);
            // A record storing a composition again, byte for byte, shows that composition (#2295).
            ushort sprite = global::SuperMetroid.Core.Samus.Artwork.ProjectileSpriteNative.FirstStored(Word(0x930000 | (pointer + 2)));
            var slot = SuperMetroid.Core.Samus.Projectiles.SamusProjectileFixtureOccupancy.ShotPlace(SuperMetroid.Core.Progression.SamusProjectileType.Beam(SuperMetroid.Core.Progression.SamusBeamCombination.Power, charged: false));
            slot.HeldShot().Animation = SamusProjectileProgramAddresses.PlayerAt(pointer);
            shots.Animation.AdvanceProjectileAnimation(slot.HeldShot(), slot.Animation ?? throw new InvalidOperationException("The shot plays no animation."));
            AssertTrue(slot.IsActive, "Actual native timed frame survives");
            // A list repeating one still picture is held: no timer, the place at the list's end (#2347).
            bool still = SamusProjectileProgramAddresses.ShowsStillPicture(pointer);
            AssertEqual<int?>(still ? null : duration, slot.NativeFrameHold, "Actual timer unchanged, a still picture held");
            AssertEqual(sprite, slot.SpritemapPointer, "Installed artwork selection");
            AssertEqual(LookupStream1ProjectileRadiusLayoutRomByte(rom, 0x930000 | (pointer + 4)), slot.Radius.X, "Actual X radius");
            AssertEqual(LookupStream1ProjectileRadiusLayoutRomByte(rom, 0x930000 | (pointer + 5)), slot.Radius.Y, "Actual Y radius");
            AssertEqual<SamusProjectileFrameId?>(SamusProjectileProgramsVerification.RecordAt(pointer), ProjectileTrails.GetTrailFrame(slot), "Actual trail frame is the record's");
            AssertEqual(still ? SamusProjectileProgramAddresses.ListEndOf(pointer) : unchecked((ushort)(pointer + 8)), slot.InstructionPointer, "Actual record progression unchanged");
            interpreted++;
            if (duration == 0 || still) continue; // Existing bomb contract excludes zero-duration timed records; no bomb holds a still picture.
            var bomb = new SamusBomb(0);
            bomb.OccupyAsPlaced(SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(0x0500));
            bomb.HeldBomb().Animation = SamusProjectileProgramAddresses.PlayerAt(pointer);
            bombs.AdvanceProjectileAnimation(bomb.HeldBomb());
            AssertTrue(bomb.IsActive, "Actual bomb timed frame survives");
            AssertEqual(duration, bomb.NativeFrameHold, "Actual bomb timer unchanged");
            AssertEqual(sprite, bomb.SpritemapPointer, "Installed bomb artwork selection");
            AssertEqual(LookupStream1ProjectileRadiusLayoutRomByte(rom, 0x930000 | (pointer + 4)), bomb.Radius.X, "Actual bomb X radius");
            AssertEqual(LookupStream1ProjectileRadiusLayoutRomByte(rom, 0x930000 | (pointer + 5)), bomb.Radius.Y, "Actual bomb Y radius");
            AssertEqual(unchecked((ushort)(pointer + 8)), bomb.InstructionPointer, "Actual bomb record progression");
            bombInterpreted++;
        }
        AssertThrows<ArgumentOutOfRangeException>(() => _ = SamusProjectileFrameDefinitions.TimedRecordAddresses[-1], "Lower indexed view rejection");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = SamusProjectileFrameDefinitions.TimedRecordAddresses[expected.Count], "Upper indexed view rejection");
        AssertEqual(Extent16<RoomPixels>.Zero, SamusProjectilePlayback.InitialRadius(null), "A shot with no list (the safe Murder Beam) has zero radii");
        Console.WriteLine($"Projectile radii:{expected.Count} native calculated records/{2 * expected.Count} bytes,{interpreted} actual projectile/{bombInterpreted} bomb records with zero reads; exact domain/order/bounds pass.");
    }

    /// <summary>
    /// The end of the unused bank-$93 list starting at <paramref name="address"/>, or null when none starts there: the lists
    /// of the two unused echo-table projectiles, which no header the port keeps names and the port does not define.
    /// </summary>
    private static int? UnusedProjectileListEnd(int address) => address switch
    {
        0x93a0f3 => 0x93a117,
        0x93a16d => 0x93a1a1,
        _ => null,
    };

    /// <summary>
    /// The bindings the cartridge's timed records give: each frame's composition, read from every record that shows it,
    /// which all store the same one (#2274).
    /// </summary>
    private static ProjectileFrameBindingDocument NativeFrameBindings(ISnesAddressSpace rom, IEnumerable<ushort> records)
    {
        var frames = new Dictionary<string, string>();
        var unpictured = new HashSet<string>();
        foreach (ushort pointer in records)
        {
            int sprite = 0x930000 | (pointer + 2);
            string name = ProjectileFrameBindingFormat.FrameNameAt(pointer);
            int selected = LittleEndianBytes.Word(rom.ReadCartridgeByte(sprite), rom.ReadCartridgeByte(sprite + 1));
            if (selected == global::SuperMetroid.Core.Samus.Artwork.ProjectileSpriteNative.EmptyRecordAddress)
            {
                // A record selecting the empty record draws nothing (#2558).
                AssertTrue(!frames.ContainsKey(name), $"every record that shows {name} draws nothing");
                unpictured.Add(name);
                continue;
            }
            string composition = ProjectileSpriteDefinitions.Name(selected);
            AssertTrue(!unpictured.Contains(name), $"every record that shows {name} stores its composition");
            if (!frames.TryAdd(name, composition))
                AssertEqual(frames[name], composition, $"every record that shows {name} stores its composition");
        }
        return new ProjectileFrameBindingDocument
        {
            Version = ProjectileFrameBindingFormat.Version,
            Frames = frames,
            Unpictured = [.. unpictured],
        };
    }

    /// <summary>Reads native projectile radius words from bank-$93 programs.</summary>
    private static byte LookupStream1ProjectileRadiusLayoutRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Traverses projectile programs, comparing timed records and mechanics ownership with bank $93.</summary>
    internal static void VerifyLookupStream1ProjectilePrograms(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1ProjectileProgramsRomByte(rom, address), LookupStream1ProjectileProgramsRomByte(rom, address + 1));
        var programs = new List<LookupStream1Program>();
        var records = new List<ushort>();
        var mechanics = new HashSet<int>();
        int programStart = 0x9386db, ticks = 1;
        for (int address = programStart; address < 0x93a1a1;)
        {
            if (UnusedProjectileListEnd(address) is { } unusedEnd) { address = unusedEnd; programStart = address; continue; }
            if (address == 0x93a117) { address += 2; programStart = address; continue; }
            ushort word = Word(address);
            mechanics.Add(address);
            if ((word & 0x8000) == 0)
            {
                AssertTrue(word > 0, "Native projectile durations are positive");
                mechanics.Add(address + 6);
                records.Add(unchecked((ushort)address));
                ticks += word;
                address += 8;
            }
            else
            {
                if (word == 0x8239) { mechanics.Add(address + 2); address += 4; }
                else { AssertEqual((ushort)0x822f, word, "Native terminal deletion opcode"); address += 2; }
                programs.Add(new LookupStream1Program(unchecked((ushort)programStart), ticks));
                programStart = address; ticks = 1;
            }
        }
        AssertEqual(795, records.Count, "Original timed-record scope outside the two unused lists");
        AssertEqual(1792, mechanics.Count, "Original mechanics-word scope outside the two unused lists");
        AssertEqual(103, programs.Count, "Independent native complete program domains outside the two unused lists");
        ProjectileFrameBindingDocument document = NativeFrameBindings(rom, records);
        var bindings = ProjectileFrameBindingFile.Load(new MemoryStream(ProjectileFrameBindingFile.Write(document)));
        _ = NewLookupFlareForbiddenBus();
        int actualTicks = 0, loops = 0, deletions = 0;
        foreach (var program in programs)
        {
            var shots = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: bindings), new global::SuperMetroid.Core.Fx.RoomEarthquake());
            var bombs = new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: bindings));
            var slot = SuperMetroid.Core.Samus.Projectiles.SamusProjectileFixtureOccupancy.ShotPlace(SuperMetroid.Core.Progression.SamusProjectileType.Beam(SuperMetroid.Core.Progression.SamusBeamCombination.Power, charged: false));
            slot.HeldShot().Animation = SamusProjectileProgramAddresses.PlayerAt(program.Start);
            var bomb = new SamusBomb(0);
            bomb.OccupyAsPlaced(SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(0x0500));
            bomb.HeldBomb().Animation = SamusProjectileProgramAddresses.PlayerAt(program.Start);
            // No bomb shows a still picture, which a shot holds (#2347): the bomb owner plays only the lists that hold none.
            bool bombPlays = !SamusProjectileProgramAddresses.HoldsStillPicture(bomb.Playback?.Animation
                ?? throw new InvalidOperationException($"Projectile list position ${program.Start:X4} names no playback."));
            ushort timer = 1, pointer = program.Start, sprite = 0, x = 0, y = 0, trail = 0;
            var visited = new HashSet<ushort>();
            bool finished = false;
            for (int tick = 0; tick < program.Ticks; tick++)
            {
                bool deleted = false, looped = false;
                timer--;
                if (timer == 0)
                {
                    ushort word = Word(0x930000 | pointer);
                    if (word == 0x8239)
                    {
                        pointer = Word(0x930000 | (pointer + 2));
                        word = Word(0x930000 | pointer);
                    }
                    if (word == 0x822f) deleted = true;
                    else
                    {
                        AssertTrue(word is > 0 and < 0x8000, "Native command reaches a timed frame");
                        looped = !visited.Add(pointer);
                        timer = word; sprite = global::SuperMetroid.Core.Samus.Artwork.ProjectileSpriteNative.FirstStored(Word(0x930000 | (pointer + 2)));
                        x = LookupStream1ProjectileProgramsRomByte(rom, 0x930000 | (pointer + 4)); y = LookupStream1ProjectileProgramsRomByte(rom, 0x930000 | (pointer + 5));
                        trail = pointer; pointer += 8;
                    }
                }
                shots.Animation.AdvanceProjectileAnimation(slot.HeldShot(), slot.Animation ?? throw new InvalidOperationException("The shot plays no animation."));
                AssertEqual(deleted, !slot.IsActive, "Exact native projectile deletion tick");
                if (bombPlays)
                {
                    bombs.AdvanceProjectileAnimation(bomb.HeldBomb());
                    AssertEqual(deleted, !bomb.IsActive, "Exact native bomb deletion tick");
                }
                actualTicks++;
                if (deleted)
                {
                    AssertTrue(!slot.IsOccupied, "Projectile deletion clears occupancy");
                    AssertTrue(!bombPlays || !bomb.IsOccupied, "Bomb deletion clears occupancy");
                    deletions++; finished = true; break;
                }
                // A trail reads the frame the record just shown shows; a frame shown again stands at its first record (#2274).
                // A shot holding a still picture keeps it with no timer while the list re-shows it, its place the list's end (#2347).
                bool held = slot.NativeFrameHold is null;
                AssertEqual(new VerifyLookupStream1ProjectileProgramsCase(held ? (ushort)0 : timer, held ? SamusProjectileProgramAddresses.ListEndOf(trail) : pointer, sprite, x, y,
                        SamusProjectileProgramAddresses.RecordAddress(global::SuperMetroid.Core.Samus.Artwork.ProjectileSpriteNative.Frame(trail))),
                    new VerifyLookupStream1ProjectileProgramsCase(checked((ushort)(slot.NativeFrameHold ?? 0)), slot.InstructionPointer, slot.SpritemapPointer, slot.Radius.X, slot.Radius.Y,
                        SamusProjectileProgramAddresses.RecordAddress(ProjectileTrails.GetTrailFrame(slot) ?? throw new InvalidOperationException("A played projectile shows a record."))),
                    "Every scheduled projectile exposure/physical envelope/trail phase matches native");
                if (bombPlays)
                    AssertEqual(new LookupStream1VerifyLookupStream1ProjectileProgramsCase(timer, pointer, sprite, x, y),
                        new LookupStream1VerifyLookupStream1ProjectileProgramsCase(checked((ushort)bomb.NativeFrameHold), bomb.InstructionPointer, bomb.SpritemapPointer, bomb.Radius.X, bomb.Radius.Y),
                        "Every scheduled bomb exposure/physical envelope matches native");
                if (looped) { loops++; finished = true; break; }
            }
            AssertTrue(finished, "Complete native startup and first loop or terminal deletion confirmed");
        }
        Console.WriteLine($"Projectile programs:{mechanics.Count} native words,{records.Count} records,{programs.Count} complete programs,{actualTicks} exact ticks per owner,{loops} loop returns/{deletions} terminal deletions, zero runtime reads; original domain preserved.");
    }

    /// <summary>Reads native projectile program words for control and hold comparison.</summary>
    private static byte LookupStream1ProjectileProgramsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks cannon placement relative to Samus body poses.</summary>
    internal static void VerifyLookupStream1CannonPlacement(ISnesAddressSpace rom)
    {
        SamusBodySources body = InstalledFixtures.SamusBody;
        byte[] json = File.ReadAllBytes(Path.Combine(CartridgeImportContent.ImportData(GameInstallationLayout.SamusBodyDirectoryName), SamusArmCannonArtworkFormat.ArtworkFileName));
        var document = RequiredJson.Deserialize<SamusArmCannonArtworkDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new SamusPoseIdJsonConverter(),
                new SamusProjectileDirectionJsonConverter(), new SamusArmCannonCoverOrientationJsonConverter() } });
        SamusArmCannonSourceArtwork Load(SamusArmCannonArtworkDocument value) => SamusArmCannonSourceArtwork.FromPlacement(
            SamusArmCannonSourceArtwork.LoadPlacement(new MemoryStream(SamusArmCannonSourceArtwork.Write(value))));
        var stock = body.ArmCannon;
        SamusArmCannonSourceArtwork loaded = Load(document);
        foreach (SamusProjectileDirection direction in SamusProjectileDirectionCodec.InAimOrder)
            AssertEqual(loaded.CoverAppearance(direction), stock.CoverAppearance(direction), $"Body binding keeps the cannon document's {direction} cover");
        for (int index = 0; index < document.DrawingData.Length; index++)
        {
            ushort address = (ushort)(0xc9d9 + index);
            AssertEqual(LookupStream1CannonPlacementRomByte(rom, 0x900000 | address), document.ReadDrawingByteAt(address), "Every installed native cannon byte");
        }
        int draws = 0;
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.FacingRightNormalPose, SamusPoseId.StandingAimUpRightPose,
            SamusPoseId.StandingAimDiagonalUpRightPose, SamusPoseId.StandingAimDiagonalDownRightPose,
            SamusPoseId.MovingRightGunExtendedPose, SamusPoseId.MovingLeftGunExtendedPose,
            SamusPoseId.RunningAimDiagonalUpRightPose, SamusPoseId.RunningAimDiagonalUpLeftPose,
            SamusPoseId.RunningAimDiagonalDownRightPose, SamusPoseId.RunningAimDiagonalDownLeftPose,
            SamusPoseId.NormalJumpAimUpRightPose, SamusPoseId.NormalJumpAimUpLeftPose,
            SamusPoseId.NormalJumpAimDownRightPose, SamusPoseId.NormalJumpAimDownLeftPose,
            SamusPoseId.FallingAimUpRightPose, SamusPoseId.FallingAimUpLeftPose,
            SamusPoseId.FallingAimDownRightPose, SamusPoseId.FallingAimDownLeftPose,
            SamusPoseId.MoonwalkFacingLeftPose, SamusPoseId.MoonwalkFacingRightPose,
            SamusPoseId.NeutralJumpTransitionRightPose, SamusPoseId.CrouchingAimDiagonalUpRightPose,
            SamusPoseId.CrouchingAimDiagonalUpLeftPose, SamusPoseId.MoonwalkAimUpLeftPose,
            SamusPoseId.MoonwalkAimUpRightPose, SamusPoseId.NormalLandingRightPose,
            SamusPoseId.SpinLandingRightPose
        })
        {
            int count = pose == SamusPoseId.NormalLandingRightPose ? 2 : pose == SamusPoseId.SpinLandingRightPose ? 3 : pose is SamusPoseId.StandingAimUpRightPose or SamusPoseId.NormalJumpAimUpRightPose or SamusPoseId.NormalJumpAimUpLeftPose or SamusPoseId.NormalJumpAimDownRightPose or SamusPoseId.NormalJumpAimDownLeftPose or SamusPoseId.FallingAimUpRightPose or SamusPoseId.FallingAimUpLeftPose or SamusPoseId.FallingAimDownRightPose or SamusPoseId.FallingAimDownLeftPose ? 2 : 1;
            for (ushort frame = 0; frame < count; frame++)
            {
                var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Pose = pose, AnimationFrame = SamusAnimationNumbers.Frame(pose, frame), Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), SelectedHudItem = HudItem.Missile };
                samus.BindBodyFixture(body);
                var cannon = new SamusArmCannonState(InstalledFixtures.SamusArtwork(armCannon: stock.Gameplay));
                cannon.Update(samus); cannon.Update(samus);
                int pointer = LittleEndianBytes.Word(LookupStream1CannonPlacementRomByte(rom, 0x90c7df + SamusPoseNumbers.Number(pose) * 2), LookupStream1CannonPlacementRomByte(rom, 0x90c7e0 + SamusPoseNumbers.Number(pose) * 2));
                int offset = (LookupStream1CannonPlacementRomByte(rom, 0x900000 | pointer) & 128) != 0 ? 4 : 2;
                int x = unchecked((sbyte)LookupStream1CannonPlacementRomByte(rom, 0x900000 | (pointer + offset + frame * 2)));
                int y = unchecked((sbyte)LookupStream1CannonPlacementRomByte(rom, 0x900000 | (pointer + offset + frame * 2 + 1)));
                int graphics = unchecked((sbyte)LookupStream1CannonPlacementRomByte(rom, 0x91b629 + SamusPoseNumbers.Number(pose) * 8 + 4));
                OamBuffer cannonOam = SpriteFrameBuffer.DrawnOam(frame => cannon.Draw(frame, samus, Position16<RoomPixels>.Zero, 0));
                AssertEqual(1, cannonOam.LastFinalizedSpriteCount, "Actual cover draw publication");
                OamEntry cannonEntry = cannonOam.GetEntry(0);
                AssertEqual(128 + x, cannonEntry.Position.X, "Actual native cover X");
                AssertEqual(128 + y - graphics, (int)cannonEntry.Position.Y, "Actual native cover Y including pose origin");
                draws++;
            }
        }
        // The installation holds each pose's typed placement (#1638): the descriptor's own reading at every animation frame
        // the pose shows, which the drawing window holds and which is the pair of bytes the cartridge's cover draw reads for
        // that frame, and no displacement at any other frame; the installed document reads back as written.
        SamusShownFrames shown = InstalledFixtures.SamusShownFrameAnalysis.Installed();
        SamusArmCannonLayout installed = stock.Installed(InstalledFixtures.SamusShownFrameAnalysis.Installed());
        int installedFrames = 0;
        foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses)
        {
            SamusArmCannonPlacement decoded = stock.Gameplay.Placement(pose), placed = installed.Gameplay.Placement(pose);
            AssertEqual(decoded.DrawOrder, placed.DrawOrder, $"Installed cannon draw order of {pose}");
            AssertEqual(decoded.FirstDirection, placed.FirstDirection, $"Installed cannon direction of {pose}");
            AssertEqual(decoded.LaterDirection, placed.LaterDirection, $"Installed cannon later direction of {pose}");
            int pointer = LittleEndianBytes.Word(LookupStream1CannonPlacementRomByte(rom, 0x90c7df + SamusPoseNumbers.Number(pose) * 2), LookupStream1CannonPlacementRomByte(rom, 0x90c7e0 + SamusPoseNumbers.Number(pose) * 2));
            int displacements = pointer + ((LookupStream1CannonPlacementRomByte(rom, 0x900000 | pointer) & 128) != 0 ? 4 : 2);
            for (int frame = 0; frame <= byte.MaxValue; frame++)
            {
                if (shown.TryPictureFrame(pose, SamusAnimationNumbers.Frame(pose, frame), out _))
                {
                    AssertTrue(decoded.Offsets.ContainsKey(SamusAnimationNumbers.Frame(pose, frame)), $"The drawing window holds the cannon displacement of {pose} at shown frame {frame}");
                    AssertEqual(decoded.OffsetNumbered(pose, frame), placed.OffsetNumbered(pose, frame), $"Installed cannon displacement of {pose} at frame {frame}");
                    AssertEqual(Offset16<RoomPixels>.Of(unchecked((sbyte)LookupStream1CannonPlacementRomByte(rom, 0x900000 | (displacements + frame * 2))),
                            unchecked((sbyte)LookupStream1CannonPlacementRomByte(rom, 0x900000 | (displacements + frame * 2 + 1)))),
                        placed.OffsetNumbered(pose, frame), $"Installed cannon displacement of {pose} at frame {frame} is the pair the cartridge reads");
                    installedFrames++;
                }
                else
                {
                    int unshown = frame;
                    AssertThrows<InvalidDataException>(() => placed.OffsetNumbered(pose, unshown), $"No installed cannon displacement of {pose} at unshown frame {frame}");
                }
            }
        }
        AssertEqual(installed.ContentIdentity, SamusArmCannonLayout.Read(new MemoryStream(installed.Write())).ContentIdentity,
            "Installed cannon layout reads back as written");
        Console.WriteLine($"Cannon placement: {document.DrawingData.Length} extracted native bytes, the canonical hash, {draws} actual OAM draws and {installedFrames} installed pose frames pass.");
    }

    /// <summary>Reads Samus arm-cannon placement source words.</summary>
    private static byte LookupStream1CannonPlacementRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks that composed body pictures hold the characters the cartridge's split body DMA copies for their frames.</summary>
    internal static void VerifyLookupStream1BodyTransfers(ISnesAddressSpace rom)
    {
        string directory = CartridgeImportContent.Component(GameInstallationLayout.SamusBodyDirectoryName);
        var bodyArt = SamusBodyArtworkFiles.LoadCharacters(directory, CartridgeImportContent.ImportData(GameInstallationLayout.SamusBodyDirectoryName));
        var body = bodyArt.Body;
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1BodyTransfersRomByte(rom, address), LookupStream1BodyTransfersRomByte(rom, address + 1));
        int transfers = 0;
        // These exact native selectors include the known physical cross-group windows.
        foreach (var selected in new VerifyLookupStream1BodyTransfersCase[] { new VerifyLookupStream1BodyTransfersCase(SamusPoseId.FacingRightNormalPose, 0), new VerifyLookupStream1BodyTransfersCase(SamusPoseId.DeathSequenceLeftPose, 0) })
        {
            // The half pictures the import composes the frame's whole picture of (#1585).
            SamusBodyFramePictures pictures = body.Pictures(selected.Pose, selected.Phase);
            int frameAddress = 0x920000 | (Word(0x92d94e + SamusPoseNumbers.Number(selected.Pose) * 2) + selected.Phase * 4);
            int topAddress = 0x920000 | (Word(0x92d91e + LookupStream1BodyTransfersRomByte(rom, frameAddress) * 2) + LookupStream1BodyTransfersRomByte(rom, frameAddress + 1) * 7);
            bool namesBottom = LookupStream1BodyTransfersRomByte(rom, frameAddress + 2) != 255;
            int? bottomAddress = namesBottom
                ? 0x920000 | (Word(0x92d938 + LookupStream1BodyTransfersRomByte(rom, frameAddress + 2) * 2) + LookupStream1BodyTransfersRomByte(rom, frameAddress + 3) * 7)
                : null;
            AssertEqual(namesBottom, pictures.Bottom is not null, "A selection naming no lower half composes none");
            // The shown body characters equal the native split copies: each definition's first part fills a half row
            // (top from character 0, bottom from character 8) and its second part the half row a full row below.
            var expected = new byte[SamusBodyCharacterArtwork.BodyCharacterCount * SpriteArtwork.CharacterByteCount];
            void Copy(int address, int firstCharacter)
            {
                int source = (int)new SnesAddress(LookupStream1BodyTransfersRomByte(rom, address + 2), Word(address));
                int first = Word(address + 3), second = Word(address + 5);
                for (int index = 0; index < first; index++)
                    expected[firstCharacter * SpriteArtwork.CharacterByteCount + index] = LookupStream1BodyTransfersRomByte(rom, source + index);
                for (int index = 0; index < second; index++)
                    expected[(firstCharacter + SpriteArtwork.CharactersPerRow) * SpriteArtwork.CharacterByteCount + index] =
                        LookupStream1BodyTransfersRomByte(rom, source + first + index);
            }
            Copy(topAddress, 0);
            if (bottomAddress is int lower) Copy(lower, SpriteArtwork.CharactersPerRow / 2);
            AssertTrue(bodyArt.BodyArtwork(pictures.Top, pictures.Bottom).Characters.SequenceEqual(expected),
                "Composed body pictures match the native split copies and physical cross-group aliases");
            transfers++;
        }
        Console.WriteLine($"Body transfers:{transfers} actual split DMA/cross-group selections pass.");
    }

    /// <summary>Reads native Samus body DMA source and transfer words.</summary>
    private static byte LookupStream1BodyTransfersRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>
    /// The stream-one oracle's suites that no other registered suite runs, each its own suite of the full run and selectable
    /// by name (#1757). The stream-one group ran them, with the stream's other checks, as one command over the time limit;
    /// those other checks are registered suites of their own, or run inside one.
    /// </summary>
    internal static FullRunSuitesVerification.RegisteredSuite[] LookupStream1Suites() =>
    [
        new(nameof(VerifyLookupStream1), () => VerifyLookupStream1(LoadStream1Oracle())),
        new(nameof(VerifyLookupStream1ProjectileMotion), () => VerifyLookupStream1ProjectileMotion(LoadStream1Oracle())),
        new(nameof(VerifyLookupStream1Selection), () => VerifyLookupStream1Selection(LoadStream1Oracle())),
        new(nameof(VerifyLookupStream1LaunchRoles), () => VerifyLookupStream1LaunchRoles(LoadStream1Oracle())),
        new(nameof(BeamCallbackTablesVerification.VerifyBeamCallbackInitializers), () => BeamCallbackTablesVerification.VerifyBeamCallbackInitializers()),
        new(nameof(SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectPopulationVerification.VerifyMetroidsClearedStateObject),
            () => SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectPopulationVerification.VerifyMetroidsClearedStateObject(new TestAddressSpace())),
    ];

    /// <summary>The retail cartridge, confirmed to be the revision the stream-one oracle reads.</summary>
    private static SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace LoadStream1Oracle()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Stream 1 oracle revision");
        return rom;
    }

    /// <summary>Runs the focused Samus death-transfer definitions oracle with the retail cartridge.</summary>
    [VerificationCommand("--lookup-stream-1-death-definitions")]
    internal static int LookupStream1DeathDefinitionsCommand()
    {
        Suite(nameof(VerifyLookupStream1DeathDefinitions), () => VerifyLookupStream1DeathDefinitions(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the focused escape Dachora full-cadence oracle with the retail cartridge.</summary>
    [VerificationCommand("--lookup-stream-1-escape-dachora-cadence")]
    internal static int LookupStream1EscapeDachoraCadenceCommand()
    {
        Suite(nameof(VerifyLookupStream1EscapeDachoraCadence), () => VerifyLookupStream1EscapeDachoraCadence(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the focused Powamp body and balloon cadence oracle.</summary>
    [VerificationCommand("--lookup-stream-1-powamp-cadence")]
    internal static int LookupStream1PowampCadenceCommand()
    {
        Suite(nameof(VerifyLookupStream1PowampCadence), () => VerifyLookupStream1PowampCadence(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs arm-cannon pose pointer extraction checks.</summary>
    [VerificationCommand("--lookup-stream-1-cannon-poses")]
    internal static int LookupStream1CannonPosesCommand()
    {
        Suite(nameof(VerifyLookupStream1CannonPoses), () => VerifyLookupStream1CannonPoses(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the HUD posture selector check over the full byte pose domain.</summary>
    [VerificationCommand("--lookup-stream-1-hud-posture")]
    internal static int LookupStream1HudPostureCommand()
    {
        Suite(nameof(VerifyLookupStream1HudPosture), () => VerifyLookupStream1HudPosture(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs visor color extraction and the independent visor color behavior suite.</summary>
    [VerificationCommand("--lookup-stream-1-visor-colors")]
    internal static int LookupStream1VisorColorsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyLookupStream1VisorColors), () => VerifyLookupStream1VisorColors(rom));
        Suite(nameof(VerifySamusVisorColors), () => VerifySamusVisorColors());
        return 0;
    }

    /// <summary>Runs the escape typewriter presentation oracle against the retail cartridge.</summary>
    [VerificationCommand("--lookup-stream-1-escape-text")]
    internal static int LookupStream1EscapeTextCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyLookupStream1EscapeText), () => VerifyLookupStream1EscapeText(rom));
        return 0;
    }

    /// <summary>Runs atmospheric sprite attribute comparison and its artwork boundary suite.</summary>
    [VerificationCommand("--lookup-stream-1-atmospheric-attributes")]
    internal static int LookupStream1AtmosphericAttributesCommand()
    {
        string sourceRom = Path.GetFullPath("Super Metroid.smc");
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        Suite(nameof(VerifyLookupStream1AtmosphericAttributes), () => VerifyLookupStream1AtmosphericAttributes(rom));
        Suite(nameof(VerifySamusAtmosphereArtworkBoundary), () => VerifySamusAtmosphereArtworkBoundary(sourceRom));
        return 0;
    }

    /// <summary>Runs cannon drawing controls and pose-pointer comparison together.</summary>
    [VerificationCommand("--lookup-stream-1-cannon-drawing")]
    internal static int LookupStream1CannonDrawingCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyLookupStream1CannonDrawingControls), () => VerifyLookupStream1CannonDrawingControls(rom));
        Suite(nameof(VerifyLookupStream1CannonPoses), () => VerifyLookupStream1CannonPoses(rom));
        return 0;
    }
}
