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
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.SpriteObjects;
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
using static SuperMetroid.Verification.Harness.InstalledArtworkBindingVerification;
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
            var slot = enemies.SlotTable.Slots[0];
            // The typed programs (#641) replacing the native lists at $B3:E964, $B3:E9D0 and $B3:EA34.
            EscapeDachoraEnemyState state = enemies.EscapeDachoraStates[0] ?? throw new InvalidOperationException("The Dachora fixture has no state.");
            state.Playback = new(EscapeDachoraAnimations.StartingWith(program == 0 ? EscapeDachoraProgram.RunningAroundLowTide
                : program == 1 ? EscapeDachoraProgram.RunningAroundHighTide : EscapeDachoraProgram.RunningForEscape), AnimationPlace.StartOfAnimation);
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
            int records = program == 2 ? 19 : 60, expectedX = slot.Position.X.Whole, calls = 0;
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
                    AssertEqual((ushort)(duration - held), slot.FrameHold.RawValue, "Every uninterrupted escape Dachora exposure tick");
                    AssertEqual(visual, slot.SpritemapPointer, "Native escape Dachora pose order");
                    AssertEqual(unchecked((ushort)expectedX), slot.Position.X.Whole, "Native callback displacement occurs only at pose completion");
                }
            }
            AssertEqual(program == 0 ? 180 : program == 1 ? 120 : 163, calls, "Exact full native pacing/departure duration");
            EnemyInstructionRunner.Process(arguments);
            expectedX += Word(0xb3eadf);
            AssertEqual(unchecked((ushort)expectedX), slot.Position.X.Whole, "Final movement callback is preserved");
            ushort next = program == 0 ? (ushort)0xe968 : program == 1 ? (ushort)0xe9d4 : (ushort)0xea80;
            AssertEqual(Word(0xb30000 | (next + 2)), slot.SpritemapPointer, "Pacing reversal/maximum speed loop shows the native record's pose");
            AssertEqual(Word(0xb30000 | next), slot.FrameHold.RawValue, "Loop reload keeps native cadence");
            AssertEqual(0, guard.ForbiddenReadAttempts, "Escape Dachora mechanics avoid cartridge reads");
        }
        Console.WriteLine("Escape Dachora cadence:139 native records,463 uninterrupted ticks,two complete pacing round trips and full accelerating departure pass.");
    }

    /// <summary>A room enemy system holding one initialized escape Dachora in slot zero.</summary>
    private static RoomEnemySystem CreateEscapeDachoraProgramSystem()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().HasEvent = _ => false;
        RoomEnemySlot dachora = enemies.SlotTable.Slots[0];
        dachora.Bind(EnemyDefinitionId.DachoraEscape, dachora.Definition);
        dachora.Bind(dachora.DefinitionId, default(RoomEnemyDefinition));
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
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            Action<RoomEnemySlot> initialize = enemies.PowampBehavior.InitializePowamp;
            var balloon = enemies.SlotTable.Slots[0];
            balloon.Bind(EnemyDefinitionId.Powamp, balloon.Definition);
            balloon.Bind(balloon.DefinitionId, default(RoomEnemyDefinition));
            balloon.SetSpawnParameter1(EnemyDefinitionId.Powamp, 1);
            initialize(balloon);
            var body = enemies.SlotTable.Slots[1];
            body.Bind(EnemyDefinitionId.Powamp, body.Definition);
            body.Bind(body.DefinitionId, default(RoomEnemyDefinition));
            body.SetSpawnSettings(new PowampSpawnSettings(PowampPart.Body, GrappleTravelDistance: 0));
            initialize(body);
            body.Position = body.Position.WithWholeY(100);
            var slot = isBody ? body : balloon;
            PowampEnemyState slotState = slot.RequireSpeciesState<PowampEnemyState>();
            slotState.Playback = new(PowampAnimations.StartingWith(root switch
            {
                0xc163 => PowampProgram.BodyFast,
                0xc173 => PowampProgram.BodySlow,
                0xc183 => PowampProgram.BalloonInflate0,
                _ => PowampProgram.BalloonStartSinking,
            }), AnimationPlace.StartOfAnimation);
            var pair = new PowampBalloon(balloon, balloon.RequireSpeciesState<PowampEnemyState>());
            slot.FrameHold = WrappingCountdown.Start(1);
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
            int calls = 0;
            for (int pose = 0; pose < 3; pose++)
            {
                ushort address = (ushort)(root + pose * 4), duration = Word(0xa80000 | address);
                ushort sprite = Word(0xa80000 | (address + 2));
                for (int held = 0; held < duration; held++)
                {
                    EnemyInstructionRunner.Process(arguments); calls++;
                    AssertEqual((ushort)(duration - held), slot.FrameHold.RawValue, "Every uninterrupted Powamp hold tick");
                    AssertEqual(sprite, slot.SpritemapPointer, "Native Powamp held visual");
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
                AssertEqual(Word(0xa80000 | (root + 2)), slot.SpritemapPointer, "Actual Powamp loop repeats first pose");
                AssertEqual(Word(0xa80000 | root), slot.FrameHold.RawValue, "Powamp loop first hold reload");
            }
            else
            {
                AssertEqual((ushort)0, slot.FrameHold.RawValue, "Powamp Sleep follows full terminal hold");
                ushort terminalY = balloon.Position.Y.Whole;
                PowampBehavior.AlignPowampBalloonY(body, pair);
                AssertEqual(terminalY, balloon.Position.Y.Whole, "Sleep preserves terminal balloon collision center");
                EnemyInstructionRunner.Process(arguments);
                AssertEqual(ushort.MaxValue, slot.FrameHold.RawValue, "Sleeping Powamp retains native timer underflow");
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
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().NextRandom = () => { randomCalls++; return 5; };
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Metroid, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.FrameHold = WrappingCountdown.Start(1);
            enemies.SlotTable.Slots[0].SetSpeciesState(new MetroidEnemyState(new RoomSpriteObject(RoomSpriteObjectKind.MetroidOuterBodyA, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard), new RoomSpriteObject(RoomSpriteObjectKind.MetroidOuterBodyB, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard))
            {
                Playback = new(MetroidAnimations.StartingWith(chasing ? MetroidProgram.ChasingSamus : MetroidProgram.DrainingSamus), AnimationPlace.StartOfAnimation),
            });
            object?[] arguments = [slot, null, null, Position16<RoomPixels>.Zero, ControllerButton.None];
            for (int record = 0; record < records; record++)
            {
                ushort address = (ushort)(root + record * 4);
                ushort duration = Word(0xa30000 | address), visual = Word(0xa30000 | (address + 2));
                EnemyInstructionRunner.Process(arguments); calls++;
                AssertEqual(duration, slot.FrameHold.RawValue, "Actual Metroid duration reload");
                AssertEqual(visual, slot.SpritemapPointer, "Actual native inside-pose identity");
                for (int held = 1; held < duration; held++)
                {
                    EnemyInstructionRunner.Process(arguments); calls++;
                    AssertEqual((ushort)(duration - held), slot.FrameHold.RawValue, "Every actual Metroid exposure tick");
                    AssertEqual(visual, slot.SpritemapPointer, "Held Metroid inside pose stays selected");
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
            AssertEqual(Word(0xa30000 | (root + 2)), slot.SpritemapPointer, "Callback loops to first timed record");
            AssertEqual(Word(0xa30000 | root), slot.FrameHold.RawValue, "Loop reload preserves first native hold");
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
            AssertEqual(count, SamusParticleAnimations.StepCount(kind), "Named atmospheric frame domain matches native source");
            for (int frame = 0; frame < count; frame++)
                AssertEqual(Word(pointer + frame * 2), SamusParticleAnimations.StepUpdates(kind, frame), "Calculated atmospheric cadence preserves every native duration");
            AssertThrows<InvalidDataException>(() => SamusParticleAnimations.StepUpdates(kind, count), "Atmospheric first out-of-range frame still rejects");
            AssertThrows<InvalidDataException>(() => SamusParticleAnimations.StepUpdates(kind, byte.MaxValue), "Atmospheric byte-max frame still rejects");
        }
        AssertTrue(SamusAtmosphericEffectWordCodec.KindNumbered(0) is null, "Effect number zero is an empty place");
        foreach (int invalid in new[] { 2, 8, byte.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusAtmosphericEffectWordCodec.KindNumbered(invalid), "An unstarted or out-of-domain atmospheric type names no particle");
        foreach (int invalid in new[] { -1, Enum.GetValues<SamusParticleKind>().Length })
        {
            AssertThrows<InvalidDataException>(() => SamusParticleAnimations.StepCount((SamusParticleKind)invalid), "An undefined particle kind rejects");
            AssertThrows<InvalidDataException>(() => SamusParticleAnimations.StepUpdates((SamusParticleKind)invalid, 0), "An undefined particle kind fails before step selection");
        }
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
            timer.SelectRunningPhase(EscapeTimerPhase.InPlace);
            timer.SetRemaining(EscapeTimerClock.Time(0, 1, 50));
            byte correction = LookupStream1TimerCadenceRomByte(rom, 0x809eec + frame);
            int expected = EscapeTimerClock.Time(0, 1, 50) - correction;
            bool expired = timer.Process(frame);
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
            AssertEqual(expected, EscapeTimerCadence.Centiseconds((ushort)index), "Every native rational cadence sample");
            AssertEqual(expected, EscapeTimerCadence.Centiseconds((ushort)(index + 128)), "Native128-frame wrap");
            AssertEqual(expected, EscapeTimerCadence.Centiseconds((ushort)(index + 0xff80)), "Native ignores high NMI bits");
            var timer = new EscapeTimer();
            timer.SelectRunningPhase(EscapeTimerPhase.InPlace);
            timer.SetRemaining(EscapeTimerClock.Time(0, 1, 50));
            AssertTrue(!timer.Process((ushort)index), "Calculated cadence stays non-expiring");
            AssertEqual(EscapeTimerClock.Time(0, 1, 50) - expected, timer.RemainingCentiseconds, "Actual Process consumes each native correction");
        }
        AssertEqual(213, total, "Native full-period centisecond budget");
        foreach (ushort frame in new ushort[] { 124, 125 })
        {
            var timer = new EscapeTimer();
            timer.SelectRunningPhase(EscapeTimerPhase.InPlace);
            timer.SetRemaining(EscapeTimerClock.Time(0, 0, 2));
            bool expired = timer.Process(frame);
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
        byte[] Transfer(EscapeTimerCharacters value) => EscapeTimerTilePageTransfers.Resolve(value, InstalledArtworkId.EscapeTimerFirstTiles).Span
            .ConcatToArray(EscapeTimerTilePageTransfers.Resolve(value, InstalledArtworkId.EscapeTimerSecondTiles).Span);
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
            ((byte[])[.. EscapeTimerTilePageTransfers.Resolve(stock, InstalledArtworkId.EscapeTimerFirstTiles).Span, .. EscapeTimerTilePageTransfers.Resolve(stock, InstalledArtworkId.EscapeTimerSecondTiles).Span]).SequenceEqual(native),
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
    /// <summary>Checks the installed Samus body frame components and their canonical hash against native data.</summary>
    internal static void VerifyLookupStream1XrayBodyFrames(ISnesAddressSpace rom)
    {
        int count = SamusBodySources.FrameCount * 4;
        byte[] native = Enumerable.Range(0, count).Select(index => LookupStream1XrayBodyFramesRomByte(rom, 0x92db48 + index)).ToArray();
        ushort[] Words(int address, int length) => Enumerable.Range(0, length).Select(index =>
            LittleEndianBytes.Word(LookupStream1XrayBodyFramesRomByte(rom, address + index * 2), LookupStream1XrayBodyFramesRomByte(rom, address + index * 2 + 1))).ToArray();
        ushort[] top = Words(0x92d91e, 13), bottom = Words(0x92d938, 11), poses = Words(0x92d94e, 253);
        ushort[] starts = top.Concat(bottom).Order().ToArray();
        SamusBodyTileTransfer[][] Groups(ushort[] pointers) => pointers.Select(pointer =>
        {
            int next = Array.IndexOf(starts, pointer) + 1;
            int end = next == starts.Length ? 0xd7d3 : starts[next];
            return Enumerable.Range(0, (end - pointer) / 7).Select(_ => new SamusBodyTileTransfer(32, 0, new byte[32])).ToArray();
        }).ToArray();
        var upper = Groups(top); var lower = Groups(bottom);
        var template = CreateSamusIdentityFixture();
        SamusBodyFrameSelection[] Decode(byte[] data) => Enumerable.Range(0, count / 4).Select(index =>
            new SamusBodyFrameSelection(data[index * 4], data[index * 4 + 1], data[index * 4 + 2], data[index * 4 + 3])).ToArray();
        SamusBodySources Create(byte[] data) => SamusBodyCharacterArtwork.Create(new SamusBodyArtworkParts { TopSetStarts = SamusBodyNative.Records(top), BottomSetStarts = SamusBodyNative.Records(bottom), PoseFirstFrames = SamusBodyNative.Frames(poses), Placements = template.Placements, Frames = Decode(data), Spritemaps = template.Spritemaps, Atmosphere = template.Atmosphere, DeathSteps = template.DeathSteps, ArmCannon = template.ArmCannon }, upper, lower).Body;
        var stock = Create(native);
        AssertTrue(stock.Frames.SequenceEqual(Decode(native)), "All supplied frame components preserved");
        AssertEqual(CanonicalBodyHash(template, top, bottom, SamusBodyNative.Frames(poses), Decode(native)), stock.ContentIdentity, "Original canonical frame hash");
        AssertThrows<InvalidDataException>(() => stock.Gameplay.OriginRise(SamusPoseNumbering.FromNumber(0xfd), 0), "Original pose bound preserved");
        Console.WriteLine($"Body frame basis:{count} native components preserved, canonical hash and pose bound pass.");
    }

    /// <summary>Reads native X-Ray body selection words for the pose-frame oracle.</summary>
    private static byte LookupStream1XrayBodyFramesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks each body pose's upper/lower frame pointers and canonical stock identity.</summary>
    internal static void VerifyLookupStream1BodyPosePointers(ISnesAddressSpace rom)
    {
        ushort[] native = Enumerable.Range(0, 253).Select(pose => LittleEndianBytes.Word(LookupStream1BodyPosePointersRomByte(rom, 0x92d94e + pose * 2), LookupStream1BodyPosePointersRomByte(rom, 0x92d94f + pose * 2))).ToArray();
        SamusBodyCharacterArtwork templateCharacters = CreateSamusIdentityCharacters();
        SamusBodySources template = templateCharacters.Body;
        ushort[] top = template.TopSetPointers.ToArray(), bottom = template.BottomSetPointers.ToArray();
        var topGroups = Enumerable.Range(0, 13).Select(set => templateCharacters.TopSet(set).ToArray()).ToArray();
        var bottomGroups = Enumerable.Range(0, 11).Select(set => templateCharacters.BottomSet(set).ToArray()).ToArray();
        var frames = Enumerable.Range(0, SamusBodySources.FrameCount)
            .Select(index => new SamusBodyFrameSelection((byte)(index % 13), 0, (byte)(index % 11), 0)).ToArray();
        SamusBodySources Create(ushort[] selected) => SamusBodyCharacterArtwork.Create(new SamusBodyArtworkParts { TopSetStarts = SamusBodyNative.Records(top), BottomSetStarts = SamusBodyNative.Records(bottom), PoseFirstFrames = SamusBodyNative.Frames(selected), Placements = template.Placements, Frames = frames, Spritemaps = template.Spritemaps, Atmosphere = template.Atmosphere, DeathSteps = template.DeathSteps, ArmCannon = template.ArmCannon }, topGroups, bottomGroups).Body;
        SamusBodySources stock = Create(native);
        AssertEqual(CanonicalBodyHash(template, top, bottom, SamusBodyNative.Frames(native), frames), stock.ContentIdentity, "Exact canonical stock pose hash");
        AssertTrue(stock.Frames.SequenceEqual(frames), "Independent frame payload remains intact");
        for (int pose = 0; pose < 253; pose++)
        {
            if (!SamusPoseNumbering.NamesPose(pose)) continue;
            int index = (native[pose] - 0xdb48) / 4;
            AssertEqual(frames[index], stock.Selection(SamusPoseNumbering.FromNumber(pose), 0), "Import selects supplied frame payload through the native frame list");
        }
        foreach (SamusPoseId pose in new[] { SamusPoseNumbering.FromNumber(0xfd), SamusPoseNumbering.FromNumber(0xfe), SamusPoseNumbering.FromNumber(0xff) })
        {
            AssertThrows<InvalidDataException>(() => stock.Gameplay.OriginRise(pose, 0), "Installed pose domain stays bounded");
        }
        ushort[] invalid = (ushort[])native.Clone(); invalid[0] = SamusBodyNative.FramePointer(SamusBodySources.FrameCount);
        AssertThrows<InvalidDataException>(() => Create(invalid), "A pose frame list past the frames is rejected");
        IReadOnlyDictionary<SamusPoseId, ushort> posePointers = stock.PosePointers;
        AssertTrue(posePointers.Count == SamusPoseNumbers.RecordedPoses.Count &&
            SamusPoseNumbers.RecordedPoses.All(pose => posePointers[pose] == native[SamusPoseNumbers.Number(pose)]),
            "Every pose with a record installs its native frame list under its name, and no record no pose names is installed");
        Console.WriteLine("Body pose pointers:233 named native frame lists, supplied frame resolution, bounds and the exact canonical hash pass.");
    }

    /// <summary>Reads native Samus body pose pointer bytes from the cartridge table.</summary>
    private static byte LookupStream1BodyPosePointersRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Recreates the content hash from sorted body allocation, pose, frame, and tile data.</summary>
    private static string CanonicalBodyHash(SamusBodySources template, ushort[] upper, ushort[] lower, IReadOnlyDictionary<SamusPoseId, int> poseFirstFrames, SamusBodyFrameSelection[] frames)
    {
        return SelectedPresentationHash.Create("samus-body-v6", content =>
        {
            foreach (int start in SamusBodyNative.Records(upper)) content.Append("top set start", start);
            foreach (int start in SamusBodyNative.Records(lower)) content.Append("bottom set start", start);
            foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses) content.Append("pose first frame", poseFirstFrames[pose]);
            foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses)
            {
                SamusBodyPosePlacement placement = template.Placements[pose];
                content.Append("graphics y offset", placement.GraphicsYOffset);
                if (placement.FrameRises is not { } rises) continue;
                content.Append("frame rise count", rises.Length);
                foreach (int rise in rises) content.Append("frame rise", rise);
            }
            foreach (SamusBodyFrameSelection frame in frames)
            {
                content.Append("top set", frame.TopSet); content.Append("top position", frame.TopPosition);
                content.Append("bottom set", frame.BottomSet); content.Append("bottom position", frame.BottomPosition);
            }
            content.Append("spritemaps", Convert.FromHexString(template.Spritemaps.ContentIdentity));
            content.Append("atmosphere", Convert.FromHexString(template.Atmosphere.ContentIdentity));
            content.Append("death steps", Convert.FromHexString(template.DeathSteps.ContentIdentity));
            content.Append("arm cannon", Convert.FromHexString(template.ArmCannon.ContentIdentity));
        });
    }

    /// <summary>Checks body tile-set allocation roots and the canonical stock identity.</summary>
    internal static void VerifyLookupStream1BodySetPointers(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1BodySetPointersRomByte(rom, address), LookupStream1BodySetPointersRomByte(rom, address + 1));
        ushort[] top = Enumerable.Range(0, 13).Select(set => Word(0x92d91e + set * 2)).ToArray();
        ushort[] bottom = Enumerable.Range(0, 11).Select(set => Word(0x92d938 + set * 2)).ToArray();
        var physical = top.Select((pointer, set) => new Physical(Upper:true, Set:set, Pointer:pointer))
            .Concat(bottom.Select((pointer, set) => new Physical(Upper:false, Set:set, Pointer:pointer))).OrderBy(value => value.Pointer).ToArray();
        SamusBodyCharacterArtwork templateCharacters = CreateSamusIdentityCharacters();
        SamusBodySources template = templateCharacters.Body;
        SamusBodyTileTransfer sample = templateCharacters.TopSet(0)[0];
        var topGroups = new SamusBodyTileTransfer[13][]; var bottomGroups = new SamusBodyTileTransfer[11][];
        for (int order = 0; order < physical.Length; order++)
        {
            var item = physical[order]; int end = order + 1 == physical.Length ? 0xd7d3 : physical[order + 1].Pointer;
            (item.Upper ? topGroups : bottomGroups)[item.Set] = Enumerable.Repeat(sample, (end - item.Pointer) / 7).ToArray();
        }
        SamusBodySources Create(ushort[] upper, ushort[] lower, SamusBodyTileTransfer[][] upperGroups, SamusBodyTileTransfer[][] lowerGroups) => SamusBodyCharacterArtwork.Create(new SamusBodyArtworkParts { TopSetStarts = SamusBodyNative.Records(upper), BottomSetStarts = SamusBodyNative.Records(lower), PoseFirstFrames = template.PoseFirstFrames.ToDictionary(), Placements = template.Placements, Frames = template.Frames.ToArray(), Spritemaps = template.Spritemaps, Atmosphere = template.Atmosphere, DeathSteps = template.DeathSteps, ArmCannon = template.ArmCannon }, upperGroups, lowerGroups).Body;
        // Independent oracle for the pre-conversion canonical serialization order (520c810c4).
        string Canonical(ushort[] upper, ushort[] lower, SamusBodyTileTransfer[][] upperGroups, SamusBodyTileTransfer[][] lowerGroups) =>
            CanonicalBodyHash(template, upper, lower, template.PoseFirstFrames, template.Frames.ToArray());
        SamusBodySources stock = Create(top, bottom, topGroups, bottomGroups);
        AssertEqual(Canonical(top, bottom, topGroups, bottomGroups), stock.ContentIdentity, "Exact pre-conversion canonical stock hash");
        AssertTrue(stock.TopSetPointers.SequenceEqual(top) && stock.BottomSetPointers.SequenceEqual(bottom), "Stock pointer snapshots match native");
        AssertThrows<InvalidDataException>(() => stock.DefinitionRecord(SamusBodyHalf.Top, 13, 0), "Installed top selector retains rejection");
        AssertThrows<InvalidDataException>(() => stock.DefinitionRecord(SamusBodyHalf.Bottom, 11, 0), "Installed bottom selector retains rejection");
        Console.WriteLine("Body allocation pointers:24 native group starts, selector bounds and the exact canonical stock hash pass.");
    }

    /// <summary>Reads native upper and lower body tile-set pointer bytes.</summary>
    private static byte LookupStream1BodySetPointersRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks visual body Y origins across all poses without changing projectile origins.</summary>
    internal static void VerifyLookupStream1BodyGraphicsOrigins(ISnesAddressSpace rom)
    {
        int[] native = Enumerable.Range(0, 253).Select(pose => (int)unchecked((sbyte)LookupStream1BodyGraphicsOriginsRomByte(rom, 0x91b62d + pose * 8))).ToArray();
        SamusBodySources template = CreateSamusIdentityFixture();
        SamusBodySources stock = new SamusBodySources(new SamusBodyArtworkParts { TopSetStarts = template.TopSetStarts.ToArray(), BottomSetStarts = template.BottomSetStarts.ToArray(), PoseFirstFrames = template.PoseFirstFrames.ToDictionary(), Placements = template.Placements.ToDictionary(pose => pose.Key, pose => pose.Value with { GraphicsYOffset = native[SamusPoseNumbers.Number(pose.Key)] }), Frames = template.Frames.ToArray(), Spritemaps = template.Spritemaps, Atmosphere = template.Atmosphere, DeathSteps = template.DeathSteps, ArmCannon = template.ArmCannon });
        InstalledSamusBody installed = SamusBodyArtworkFiles.Load(RepositoryInstallation.Installation.SamusBodyDirectory);
        for (int pose = 0; pose < 253; pose++)
        {
            if (!SamusPoseNumbering.NamesPose(pose)) continue;
            AssertEqual(native[pose], stock.Gameplay.GraphicsYOffset(SamusPoseNumbering.FromNumber(pose)), "Installed origin is the supplied placement");
            AssertEqual(native[pose], installed.Gameplay.GraphicsYOffset(SamusPoseNumbering.FromNumber(pose)), "The import installs the native real-pose byte as the pose's origin");
            AssertEqual(unchecked((byte)native[pose]), SamusPoseProjectileOrigins.ReadYOffset(SamusPoseNumbering.FromNumber(pose)), "Physical projectile origin matches the native byte");
        }
        foreach (SamusPoseId pose in new[] { SamusPoseNumbering.FromNumber(0xfd), SamusPoseNumbering.FromNumber(0xfe), SamusPoseNumbering.FromNumber(0xff) })
            AssertThrows<InvalidDataException>(() => stock.Gameplay.GraphicsYOffset(pose), "Installed graphics excludes adjacent instruction poses");
        Console.WriteLine("Body graphics origins:253 native installed origins, physical projectile origins and bounds pass.");
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
            // The import bakes each frame's placement from the table the drained draw indexes by the frame (#1517).
            AssertTrue(stock.Placements[pose].FrameRises is { } rises && rises.SequenceEqual(native.Select(offset => -offset)),
                "Every drained crouch frame installs the negated native offset its frame indexes");
            for (int frame = 0; frame < 32; frame++)
            {
                if (stock.Gameplay.TryOriginRise(pose, frame, out int rise))
                    AssertEqual(-native[frame], rise, "Installed drained frame carries its native placement");
            }
            AssertTrue(!stock.Gameplay.TryOriginRise(pose, 32, out _), "A frame past the drained table has no placement");
        }
        SamusShownFrames shown = SamusShownFrames.Of(InstalledFixtures.AnimationContinuation);
        int draws = 0;
        for (ushort frame = 0; frame < 32; frame++)
        {
            // Only the frames the drained crouch can show are drawn; the draw rejects the others (#1513).
            if (!shown.TryPictureFrame(SamusPoseId.DrainedCrouchingLeftPose, frame, out _)) continue;
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = SamusPoseId.DrainedCrouchingLeftPose, AnimationFrame = frame, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
            samus.Presentation.BindBodyFixture(stockArt);
            samus.Presentation.Draw(samus, new SpriteFrameBuffer(), new(0, 0));
            // A frame outside the crouch's own pictures is placed as the picture it draws (#1513).
            int picture = shown.PictureFrame(SamusPoseId.DrainedCrouchingLeftPose, frame);
            AssertEqual(unchecked((ushort)(128 + native[picture])), samus.Presentation.SpritemapPosition.Y,
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
                var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = pose, AnimationFrame = frame, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
                samus.Presentation.BindBodyFixture(stockArt);
                samus.Presentation.Draw(samus, new SpriteFrameBuffer(), new(0, 0));
                AssertEqual(unchecked((ushort)(128 + native[(SamusPoseNumbers.Number(pose) - 0x35) * 2 + frame])), samus.Presentation.SpritemapPosition.Y,
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
            AssertTrue(stock.Placements[pose].FrameRises is { } rises && rises.SequenceEqual(landing.Skip(row * 4)),
                "Each landing frame installs the unsigned native byte its frame indexes");
            AssertTrue(!stock.Gameplay.TryOriginRise(pose, 16 - row * 4, out _), "A landing frame past the table has no placement");
        }
        for (int row = 0; row < 12; row++)
        {
            SamusPoseId pose = SamusPoseNumbering.FromNumber(0x35 + row);
            AssertTrue(stock.Placements[pose].FrameRises is { } rises && rises.SequenceEqual(posture.Skip(row * 2).Select(offset => -offset)),
                "Each posture-transition frame installs the negated native offset its frame indexes");
            AssertTrue(!stock.Gameplay.TryOriginRise(pose, 24 - row * 2, out _), "A posture frame past the table has no placement");
        }
        SamusShownFrames shown = SamusShownFrames.Of(InstalledFixtures.AnimationContinuation);
        for (int index = 0; index < 16; index++)
        {
            SamusPoseId landingPose = SamusPoseNumbering.FromNumber(0xa4 + index / 4);
            // Only the landing frames the pose can show are drawn; the draw rejects the others (#1513).
            if (!shown.TryPictureFrame(landingPose, index % 4, out _))
                continue;
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                Pose = landingPose, AnimationFrame = (ushort)(index % 4),
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            };
            samus.Presentation.BindBodyFixture(stock);
            samus.Presentation.Draw(samus, new SpriteFrameBuffer(), cameraPosition: new(0, 0));
            // The game subtracts a word whose high byte the drawn position never shows; the body sits by the low byte (#1500).
            AssertEqual(unchecked((ushort)(128 - landing[index])), samus.Presentation.SpritemapPosition.Y, "Actual rendering places every landing frame by its own byte");
            AssertEqual((ushort)128, samus.Position.Y.Whole, "Visual landing correction does not change physical position");
        }
        Console.WriteLine("Body facing offsets:16 landing and 24 posture native placements, landing draws and bounds pass.");
    }

    /// <summary>Reads native landing and posture offset words for Samus placement.</summary>
    private static byte LookupStream1BodyFacingOffsetsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks upper/lower OAM base selectors, the stock hash, and invalid poses.</summary>
    internal static void VerifyLookupStream1BodyOamBases(ISnesAddressSpace rom)
    {
        ushort[] ReadWords(int address, int count) => Enumerable.Range(0, count)
            .Select(index => LittleEndianBytes.Word(LookupStream1BodyOamBasesRomByte(rom, address + index * 2), LookupStream1BodyOamBasesRomByte(rom, address + index * 2 + 1))).ToArray();
        ushort[] top = ReadWords(0x929263, 253), bottom = ReadWords(0x92945d, 253), pointers = ReadWords(0x92808d, 2096);
        SamusSpritemapId?[] entries = SamusSpritemapNative.Entries(pointers);
        // Marker parts confirm selected supplied payload identity, not native artwork: every bit of the pointer lands in
        // the marker's character, palette row, priority or mirroring, so distinct pointers give distinct markers.
        static SpritePart Marker(ushort pointer) =>
            SpritePart.Create(Offset16<ScreenPixels>.Of(unchecked((sbyte)pointer), unchecked((sbyte)(pointer >> 8))), (pointer & 1) != 0, pointer & 511,
                (pointer >> 9) & 7, (pointer >> 12) & 3,
                ((pointer & 0x4000) != 0 ? TileFlip.Horizontal : TileFlip.None) | ((pointer & 0x8000) != 0 ? TileFlip.Vertical : TileFlip.None));
        SamusSpritemapDefinition[] definitions = [.. SamusSpritemapNative.Compositions(pointers)
            .Select(pointer => new SamusSpritemapDefinition([Marker(pointer)]))];
        SamusSpritemapSources Create(ushort[] upper, ushort[] lower) => new(SamusSpritemapNative.Bases(upper), SamusSpritemapNative.Bases(lower), entries, definitions);
        var stock = Create(top, bottom);
        string Canonical(ushort[] upper, ushort[] lower) => SelectedPresentationHash.Create("samus-spritemaps-v4", content =>
        {
            foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses) content.Append("top base", upper[SamusPoseNumbers.Number(pose)]);
            foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses) content.Append("bottom base", lower[SamusPoseNumbers.Number(pose)]);
            foreach (SamusSpritemapId? entry in entries) content.Append("entry", entry?.Ordinal ?? -1);
            foreach (SamusSpritemapDefinition definition in definitions)
            {
                content.Append("part count", definition.Parts.Length);
                foreach (SpritePart part in definition.Parts) AppendCanonicalSpritePart(content, part);
            }
        });
        void CheckSelection(SamusSpritemapSources catalog, ushort selected)
        {
            bool found = catalog.TryGet(selected, out SamusSpritemapDefinition? definition);
            AssertEqual(pointers[selected] != 0, found, "Native zero pointer draws nothing");
            if (found)
                AssertEqual(Marker(pointers[selected]),
                    (definition ?? throw new InvalidOperationException("Verification requires definition.")).Parts.Single(),
                    "Actual selector resolves independent marker payload");
        }
        // The installed semantic selection is compared with the native table: a pose half's first animation frame shows
        // the composition of the entry its native base names.
        static SamusSpritemapId? Shown(SamusSpritemapSources catalog, SamusBodyHalfFrame frame) =>
            catalog.TryGetComposition(frame, out SamusSpritemapId composition) ? composition : null;
        AssertEqual(Canonical(top, bottom), stock.ContentIdentity, "Exact original canonical stock OAM hash");
        for (int half = 0; half < 2; half++)
        for (int pose = 0; pose < 253; pose++)
        {
            if (!SamusPoseNumbering.NamesPose(pose)) continue;
            ushort expected = half == 0 ? top[pose] : bottom[pose];
            var firstFrame = new SamusBodyHalfFrame(SamusPoseNumbering.FromNumber(pose), half == 0 ? SamusBodyHalf.Top : SamusBodyHalf.Bottom, 0);
            AssertEqual(expected, half == 0 ? stock.TopBase(SamusPoseNumbering.FromNumber(pose)) : stock.BottomBase(SamusPoseNumbering.FromNumber(pose)), "Import-side native base selection");
            AssertEqual(entries[expected], Shown(stock, firstFrame), "A pose half's first frame shows its native base entry's composition");
            CheckSelection(stock, expected);
        }
        foreach (SamusPoseId pose in new[] { SamusPoseNumbering.FromNumber(0xfd), SamusPoseNumbering.FromNumber(0xfe), SamusPoseNumbering.FromNumber(0xff) })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.TopBase(pose), "Original installed top domain exception");
            AssertThrows<ArgumentOutOfRangeException>(() => stock.BottomBase(pose), "Original installed bottom domain exception");
            AssertThrows<ArgumentOutOfRangeException>(() => stock.TryGetComposition(new SamusBodyHalfFrame(pose, SamusBodyHalf.Top, 0), out _),
                "A pose without a record has no installed pictures");
        }
        ushort[] invalid = (ushort[])top.Clone(); invalid[0] = 2096;
        AssertThrows<InvalidDataException>(() => Create(invalid, bottom), "A base past the entries is rejected");
        AssertTrue(stock.TopBases.Count == SamusPoseNumbers.RecordedPoses.Count && stock.BottomBases.Count == SamusPoseNumbers.RecordedPoses.Count &&
            SamusPoseNumbers.RecordedPoses.All(pose => stock.TopBases[pose] == top[SamusPoseNumbers.Number(pose)] && stock.BottomBases[pose] == bottom[SamusPoseNumbers.Number(pose)]),
            "Stock bases match native, each under its pose's name");
        Console.WriteLine("Samus OAM bases:506 native bases mapped to pose-half first frames, actual supplied payload selection, the exact canonical hash and original bounds pass.");
    }

    /// <summary>Reads native Samus top and bottom OAM base words.</summary>
    private static byte LookupStream1BodyOamBasesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
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
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        SamusArmCannonSourceArtwork Load(SamusArmCannonArtworkDocument value) => SamusArmCannonSourceArtwork.FromPlacement(
            SamusArmCannonSourceArtwork.LoadPlacement(new MemoryStream(SamusArmCannonSourceArtwork.Write(value))));
        var stock = Load(document);
        ushort Native(int pose) => LittleEndianBytes.Word(LookupStream1CannonPosesRomByte(rom, 0x90C7DF + pose * 2), LookupStream1CannonPosesRomByte(rom, 0x90C7E0 + pose * 2));
        string Identity(SamusArmCannonArtworkDocument value) => SelectedPresentationHash.Create("samus-arm-cannon-v5", hash =>
        {
            foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses) hash.Append("pose descriptor", value.PoseDescriptors[pose]);
            hash.Append("drawing data", value.DrawingData.Select(item => (byte)item).ToArray());
            foreach (SpriteVisualPart appearance in value.CoverAppearances)
                AppendCanonicalSpritePart(hash, CannonCoverAppearance(appearance.TileRow, appearance.TileColumn,
                    appearance.Palette ?? 0, appearance.Priority, appearance.FlipX, appearance.FlipY));
            foreach (int[] direction in value.CoverTiles)
                foreach (int tile in direction) hash.Append("cover tile", tile);
        });
        AssertEqual(253, SamusBodySources.PoseCount, "cannon original 506-byte pointer extent");
        AssertEqual(Identity(document), stock.ContentIdentity, "cannon canonical native identity unchanged");
        AssertEqual(SamusPoseNumbers.RecordedPoses.Count, document.PoseDescriptors.Count, "cannon installs one descriptor per pose with a record, and none for a record no pose names");
        foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses)
            AssertEqual(Native(SamusPoseNumbers.Number(pose)), stock.PoseDrawingData(pose), "cannon installed native pose dispatch");
        foreach (SamusPoseId pose in new[] { SamusPoseId.FirstUnauthoredPose, SamusPoseId.SecondUnauthoredPose, SamusPoseId.LastUnauthoredPose })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.PoseDrawingData(pose), "cannon installed pose domain excludes the poses past the records");
        Console.WriteLine("Cannon poses: 233 named installed descriptors, the canonical stock hash and exact FD-FF rejection pass.");
    }

    /// <summary>Reads the native pose-to-cannon pointer table.</summary>
    private static byte LookupStream1CannonPosesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>
    /// Checks that the three numbers past the pose records name no program (#1473): the cartridge's table words for them
    /// would be the first program's delay bytes, naming low-bank memory rather than a program, and no gameplay path gives
    /// Samus one of those numbers, so the port gives them no animation.
    /// </summary>
    internal static void VerifyLookupStream1AnimationAliases(CartridgeImportAddressSpace bus)
    {
        for (int pose = SamusAnimationProgramCodec.PoseCount; pose <= byte.MaxValue; pose++)
        {
            ushort word = bus.ReadCartridgeWord(SamusAnimationDelayDefinitions.PointerTableAddress + pose * sizeof(ushort));
            AssertTrue(word < 0x8000, $"pose ${pose:X2}'s table word ${word:X4} names low-bank memory, not a program");
            SamusPoseId adjacent = SamusPoseNumbering.FromNumber(pose);
            AssertThrows<ArgumentOutOfRangeException>(() => SamusAnimationDelays.ProgramForPose(adjacent),
                $"pose ${pose:X2} has no animation program");
        }
        Console.WriteLine("Animation aliases: the three numbers past the pose records name low-bank memory and have no program.");
    }

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
            AssertEqual(new Offset16<SpritePixels>(unchecked((short)Word(anchorImmediates[(int)anchor])), 0), stock.anchors[anchor], "Installed timer anchor matches native renderer immediate");
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
        var grapple = ChargeFlarePlacementFile.LoadGrapple(new MemoryStream(ChargeFlarePlacementFile.WriteGrapple(grappleDocument)));
        foreach (ChargeFlarePlacement placement in ChargeFlarePlacements.Placements(ChargeFlareSource.Grapple))
        {
            int row = SamusProjectileDirectionCodec.FlarePlacementRow(placement.Aim);
            var expected = new ChargeFlareOffset { X = Word((placement.Running ? 0x9bc19a : 0x9bc14a) + row * 2),
                Y = Word((placement.Running ? 0x9bc1ae : 0x9bc15e) + row * 2) };
            AssertEqual(expected.Offset, grapple.Resolve(placement.Running, placement.Aim), "Installed Grapple defaults match native");
        }
        AssertEqual(20, ChargeFlarePlacements.Placements(ChargeFlareSource.Grapple).Count(), "The Grapple flare is placed at ten muzzles in each movement mode");
        // A flare aim naming neither a muzzle nor a turn is no placement.
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(false, default), "Flare placement catalog bounds");
        AssertThrows<ArgumentOutOfRangeException>(() => grapple.Resolve(true, default), "Grapple catalog bounds");
        // No pose places a flare while turning running, or a Grapple flare while turning (#1500).
        foreach (SamusTurningAim turning in Enum.GetValues<SamusTurningAim>())
        {
            SamusFlareAim aim = SamusFlareAim.WhileTurning(turning);
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(true, aim), "Running flare placement rejects turning placements");
            AssertThrows<ArgumentOutOfRangeException>(() => grapple.Resolve(false, aim), "Grapple flare placement has no turning placements");
        }
        var compositions = ChargeFlareSprites.Load(new MemoryStream(ChargeFlareSpriteExtractor.Extract(rom)));
        var body = CreateSamusIdentityFixture();
        var system = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
        var draw = new Action<SpriteFrameBuffer, SamusState, Position16<RoomPixels>, ChargeFlareComponent, TurningFloorTransform?, ChargeFlareArtwork>(system.Animation.DrawFlareComponent);
        ProjectileDrawArtwork flareObjects = InstalledFixtures.ProjectileArtwork();
        foreach (SamusPoseId pose in new[]
        {
            SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose,
            SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose
        })
        {
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = pose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
            samus.Presentation.BindBodyFixture(body);
            int direction = LookupStream1FlarePlacementRomByte(rom, 0x91b629 + SamusPoseNumbers.Number(pose) * 8 + 3);
            bool running = pose is SamusPoseId.MovingRightNormalPose or SamusPoseId.MovingLeftNormalPose;
            short x = Word((running ? 0x90c1dc : 0x90c1a8) + direction * 2);
            short y = Word((running ? 0x90c1f0 : 0x90c1c2) + direction * 2);
            var expected = new OamBuffer();
            DrawImportedFlareSpritemap((CartridgeImportAddressSpace)rom, expected, 0, new((ushort)(100 + x), unchecked((ushort)(100 + y - unchecked((byte)body.Gameplay.GraphicsYOffset(pose))))));
            // The flare's picture may be laid out once for several projectile sheet frames whose pictures are identical
            // (#1513), so the draw is matched against each composition that picture stands for.
            Action<SpriteFrameBuffer> drawFlare = frame => draw(frame, samus, Position16<RoomPixels>.Zero, ChargeFlareComponent.MainFlare, null,
                new ChargeFlareArtwork { Placement = stock, Objects = flareObjects });
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
            AssertEqual(LookupStream1RomByte(rom, 0x91b62b + pose * 8), SamusPoseNumbers.Number(SamusPoseDispatch.ReadNoInputPose(SamusPoseNumbering.FromNumber(pose))), "Native named-pose no-input case");
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
        Suite(nameof(VerifyLookupStream1CadencePrograms), () => VerifyLookupStream1CadencePrograms(rom));
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1RomByte(rom, address), LookupStream1RomByte(rom, address + 1));
        foreach (SamusLiquidMedium medium in Enum.GetValues<SamusLiquidMedium>())
        {
            for (int variant = 0; variant < 4; variant++)
            {
                var actual = SamusVerticalMotion.Launch(medium, (variant & 1) != 0, ((variant & 2) != 0) ? SamusLaunchKind.WallJump : SamusLaunchKind.Jump);
                int address = 0x909eb9 + 12 * variant + 2 * (int)medium;
                AssertEqual(Word(address), actual.Whole, "stream1 native jump whole speed");
                AssertEqual(Word(address + 6), actual.Fraction, "stream1 native jump fractional speed");
            }
            var hurt = SamusVerticalMotion.Knockback(medium);
            AssertEqual(Word(0x909ee9 + 2 * (int)medium), hurt.Whole, "stream1 native knockback whole speed");
            AssertEqual(Word(0x909eef + 2 * (int)medium), hurt.Fraction, "stream1 native knockback fractional speed");
            var bomb = SamusVerticalMotion.BombJump(medium);
            AssertEqual(Word(0x909ef5 + 2 * (int)medium), bomb.Whole, "stream1 native bomb whole speed");
            AssertEqual(Word(0x909efb + 2 * (int)medium), bomb.Fraction, "stream1 native bomb fractional speed");
            var gravity = SamusVerticalMotion.Gravity(medium);
            AssertEqual(Word(0x909ea7 + 2 * (int)medium), gravity.Whole, "stream1 native whole gravity");
            AssertEqual(Word(0x909ea1 + 2 * (int)medium), gravity.Fraction, "stream1 native fractional gravity");
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
        for (int phase = 0; phase < 12; phase++)
            AssertEqual(Word(0xa8c1a1 + 2 * phase), PowampMotion.WiggleOffset(phase), "Powamp native wiggle triangle");
        for (int pose = 0; pose < 3; pose++)
        {
            AssertEqual(Word(0xa8c277 + 2 * pose), PowampMotion.BalloonOffset(pose, false), "Powamp native rising balloon offset");
            AssertEqual(Word(0xa8c27d + 2 * pose), PowampMotion.BalloonOffset(pose, true), "Powamp native sinking balloon offset");
        }
        for (int direction = 0; direction < 8; direction++)
        {
            AssertEqual(Word(0x86d21a + 2 * direction), PowampMotion.SpikeAcceleration(direction).X.InTwoHundredFiftySixthsOfPixel, "Powamp native spike X acceleration");
            AssertEqual(Word(0x86d22a + 2 * direction), PowampMotion.SpikeAcceleration(direction).Y.InTwoHundredFiftySixthsOfPixel, "Powamp native spike Y acceleration");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 12, int.MaxValue })
            AssertThrows<InvalidDataException>(() => PowampMotion.WiggleOffset(invalid), "Powamp wiggle domain");
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
            for (int sinking = 0; sinking < 2; sinking++)
                AssertThrows<ArgumentOutOfRangeException>(() => PowampMotion.BalloonOffset(invalid, sinking != 0), "Powamp balloon pose domain");
        foreach (int invalid in new[] { int.MinValue, -1, 8, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => PowampMotion.SpikeAcceleration(invalid), "Powamp spike direction domain");
        }
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
            AssertEqual(new TrigonometryAngle(Word(0xa8d871 + 2 * direction.ToAimIndex())), BullMotion.ShotAngle(direction), "stream1 Bull compass angle");
        AssertThrows<InvalidDataException>(() => EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Bull, PopulationWords.Parameters(13, 0)), "stream1 Bull invalid interval");
        for (byte index = 0; index < 6; index++)
            AssertEqual(Word(0xa29f36 + 2 * index), CacatacTravelDistance(index), "stream1 Cacatac patrol blocks");
        foreach (byte invalid in new byte[] { 6, byte.MaxValue })
            AssertThrows<InvalidDataException>(() => CacatacTravelDistance(invalid), "stream1 Cacatac invalid patrol selector");
        for (ushort direction = 0; direction < 20; direction += 2)
            AssertEqual(Word(0x86d96a + direction), CacatacProjectileNativeListAddresses.Entry(CacatacSpikes.Program(
                CacatacNativeWords.SpikeDirections.Decode(direction, "Cacatac spike direction"))), "stream1 Cacatac named direction program");
        foreach (ushort invalid in new ushort[] { 1, 19, 20, ushort.MaxValue })
            AssertEqual(false, CacatacNativeWords.SpikeDirections.TryDecode(invalid, out _), "stream1 Cacatac invalid direction selector");
    }

    /// <summary>Reads native Bull and related enemy motion words.</summary>
    private static byte LookupStream1EnemyMovementRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>The patrol distance the import decodes from a Cacatac's travel selector.</summary>
    private static int CacatacTravelDistance(int selector) =>
        ((CacatacSpawnSettings)EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Cacatac, PopulationWords.Parameters(0, selector))).TravelDistance;
    /// <summary>Checks the installed waits below the charge flare's programs against the table naming the programs.</summary>
    internal static void VerifyLookupStream1CadencePrograms(ISnesAddressSpace rom)
    {
        // A main-flare place wrapping below its first picture reads the table naming the programs: the three program
        // words, which hold each program's place in the bank. The installed waits must be those bytes.
        var below = InstalledFixtures.SamusOverreads.Projectiles.ChargeFlareDelays;
        int[] programStarts = [0, 32, 39];
        int places = SuperMetroid.Core.Compatibility.ChargeFlareDelaysBelowPrograms.Places;
        for (int distance = 1; distance <= places; distance++)
        {
            int tableByte = places - distance;
            ushort programWord = (ushort)(SamusSourceDefinitions.ChargeFlarePrograms.Offset + programStarts[tableByte / sizeof(ushort)]);
            byte expected = tableByte % sizeof(ushort) == 0 ? LittleEndianBytes.LowByte(programWord) : LittleEndianBytes.HighByte(programWord);
            AssertEqual(LookupStream1CadenceProgramsRomByte(rom, SamusSourceDefinitions.ChargeFlarePrograms.Value - distance), expected,
                "stream1 flare program table names each program's place");
            AssertEqual(expected, below.DelayBelow(distance), "stream1 installed flare wait below the programs");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => below.DelayBelow(places + 1),
            "stream1 flare read below the program table is rejected");
    }

    /// <summary>Reads native charge-flare and animation cadence instructions.</summary>
    private static byte LookupStream1CadenceProgramsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks the beam initializer's launch speeds against the bank-$90 speed words it reads, overreads included.</summary>
    internal static void VerifyLookupStream1ProjectileMotion(ISnesAddressSpace rom)
    {
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1ProjectileMotionRomByte(rom, address), LookupStream1ProjectileMotionRomByte(rom, address + 1));
        var initialize = new Action<ProjectileLaunchInheritance, SamusProjectile>((records, launched) =>
            BeamFiring.InitializePowerBeamVelocity(records, launched, InstalledFixtures.SamusOverreads.Projectiles.UnauthoredBeamFiring));
        var stationary = new ProjectileLaunchInheritance();
        for (ushort combination = 0; combination < 16; combination++)
        for (ushort direction = 0; direction < 10; direction++)
        {
            var slot = new SamusProjectile(0) { Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(combination), Direction = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeDirection(direction) };
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
            var launch = SamusBombSpreadLaunches.ForSlot(index);
            AssertEqual(Word(0x90d8cf + index * 2), launch.FuseTimer, "spread native fuse");
            AssertEqual(Word(0x90d8d9 + index * 2), launch.XVelocity.LaunchWord, "spread native direction/magnitude X");
            AssertEqual(Word(0x90d8e3 + index * 2), launch.YSpeed.Whole, "spread native whole Y");
            AssertEqual(Word(0x90d8ed + index * 2), launch.YSpeed.Fraction, "spread native fraction Y");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 5, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => SamusBombSpreadLaunches.ForSlot(invalid), "spread native slot rejection");
        for (int index = 0; index < 22; index++)
        {
            var frame = HibashiEruption.ActivityFrame(index);
            AssertEqual(Word(0xa68dbb + index * 2), frame.YOffset, "Hibashi native collision rise");
            AssertEqual(Word(0xa68de7 + index * 2), frame.YRadius, "Hibashi native collision radius");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 22, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => HibashiEruption.ActivityFrame(invalid), "Hibashi native frame rejection");
        for (ushort selector = 0; selector < 6; selector++)
        {
            AssertEqual(Word(0xa2e5ef + selector * 2), NativeEnemyAnimationOracle.EntryOf(DragonNativeListAddresses.Entries, DragonPhases.Program(DragonNativeWords.AnimationSelectors.Decode(selector, "Dragon animation ordinal"))), "Dragon native phase/facing program");
            var role = EscapeEtecoons.Initialization(SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.EscapeEtecoonRole(selector));
            int offset = selector & ~1;
            AssertEqual(Word(0xb3e718 + offset), role.Position.X, "Etecoon native role X");
            AssertEqual(Word(0xb3e71e + offset), role.Position.Y, "Etecoon native role Y");
            AssertEqual(Word(0xb3e724 + offset), AnimalsNativeWords.EscapeEtecoonFrameBehaviors.Word(role.FrameBehavior), "Etecoon native role action");
            AssertEqual(Word(0xb3e72a + offset), EscapeEtecoonNativeListAddresses.Entries.Single(entry => entry.Program == role.Program).Entry, "Etecoon native role program");
            AssertEqual(Word(0xb3e730 + offset), role.Velocity.X.RawValue, "Etecoon native role speed");
        }
        foreach (ushort invalid in new ushort[] { 6, 7, ushort.MaxValue })
        {
            AssertEqual(false, DragonNativeWords.AnimationSelectors.TryDecode(invalid, out _), "Dragon invalid selector rejection");
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
        foreach (SamusMovementType movement in movements)
            AssertEqual(LookupStream1DeathDefinitionsRomByte(rom, 0x9bb420 + SamusMovementTypeNativeWords.Byte(movement)), SamusDeathSequenceFrames.InitialFrame(movement), "native movement death phase");
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
            var eventNumber = SuperMetroid.Core.Rooms.RoomObjects.MetroidsClearedEvents.ResolveEvent(argument / 2);
            if (argument < 18)
            {
                AssertTrue(eventNumber is null, "nine no-op quota identities have no event");
                AssertEqual((byte)0x60, LookupStream1PuyoAndQuotaRomByte(rom, 0x840000 | pointer), "native distinct no-op RTS");
            }
            else AssertEqual(Word(0x840000 | (pointer + 9)), (eventNumber ?? throw new InvalidOperationException("Verification requires eventNumber.")).Number(), "native quota observer event operand");
        }
        foreach (ushort invalid in new ushort[] { 1, 17, 25, 26, ushort.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => SuperMetroid.Core.Rooms.RoomObjects.RoomObjectFrameBehaviorNativeRoutines.MetroidsCleared(invalid), "quota pointer argument domain");
            AssertThrows<InvalidDataException>(() =>
            {
                if (SuperMetroid.Core.Rooms.RoomObjects.RoomObjectRoomArgumentWords.Decode(
                    SuperMetroid.Core.Rooms.RoomObjects.RoomObjectKind.MetroidRoomClearance, invalid)
                    is SuperMetroid.Core.Rooms.RoomObjects.MetroidRoomArgument metroids)
                    _ = SuperMetroid.Core.Rooms.RoomObjects.MetroidsClearedEvents.ResolveEvent(metroids.Entry);
            }, "quota event argument domain");
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
        AssertThrows<ArgumentOutOfRangeException>(() => SamusHudObservations.MovementHandler(undefinedMovement), "HUD movement exact domain");
        AssertThrows<InvalidDataException>(() => SamusAtmosphericEffects.WaterSplashFor(undefinedMovement), "splash exact movement domain");
        foreach (ushort invalid in new ushort[] { 10, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => SamusAtmosphericEffects.IsRunningFootContact(invalid), "foot-contact exact frame domain");
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
            int nativeIndex = Array.IndexOf(expected, (ushort)word);
            AssertEqual(nativeIndex, sources.IndexOf((ushort)word), "Calculated cannon reverse index preserves complete word domain");
            AssertEqual(nativeIndex >= 0, sources.Contains((ushort)word), "Calculated cannon membership rejects interior addresses");
        }
        foreach (int invalid in new[] { int.MinValue, -1, expected.Length, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = sources[invalid], "Calculated cannon source index bounds preserve array contract");
        byte[] json = File.ReadAllBytes(Path.Combine(CartridgeImportContent.ImportData(GameInstallationLayout.SamusBodyDirectoryName), SamusArmCannonArtworkFormat.ArtworkFileName));
        byte[] png = File.ReadAllBytes(Path.Combine(CartridgeImportContent.ImportData(GameInstallationLayout.SamusBodyDirectoryName), SamusArmCannonArtworkFormat.TileFileName));
        SamusArmCannonCoverCharacters cover = InstalledSamusArmCannonArtworkToolingConstants.LoadWithCover(new MemoryStream(json), new MemoryStream(png));
        SamusArmCannonSourceArtwork stock = cover.Cannon;
        AssertEqual(ReferenceIdentity(json), stock.ContentIdentity, "Calculated cannon selectors preserve canonical native content identity");
        Suite(nameof(VerifySelectors), () => VerifySelectors(stock));
        foreach (int invalid in new[] { int.MinValue, -1, 10, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.CoverAppearance(invalid), "Cannon cover appearance bounds remain exact");
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
                AssertEqual(new SnesObjAttributeWord(Word(0x90c791 + direction * 2)).ToSinglePart(), catalog.CoverAppearance(direction), "Installed cover appearances match the native attribute words");
                int list = 0x900000 | Word(0x90c7a5 + direction * 2);
                for (int frame = 0; frame < 4; frame++)
                    AssertEqual(Word(list + frame * 2), catalog.TileSource(direction, frame), "Calculated cannon orientation/frame preserves native selections");
            }
        }

        static string ReferenceIdentity(byte[] jsonBytes)
        {
            using var document = System.Text.Json.JsonDocument.Parse(jsonBytes);
            var root = document.RootElement;
            return SelectedPresentationHash.Create("samus-arm-cannon-v5", content =>
            {
                System.Text.Json.JsonElement descriptors = root.GetProperty("poseDescriptors");
                foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses) content.Append("pose descriptor", descriptors.GetProperty(pose.ToString()).GetInt32());
                content.Append("drawing data", root.GetProperty("drawingData").EnumerateArray().Select(value => (byte)value.GetInt32()).ToArray());
                foreach (var appearance in root.GetProperty("coverAppearances").EnumerateArray())
                {
                    int palette = appearance.TryGetProperty("palette", out var paletteValue) && paletteValue.ValueKind != System.Text.Json.JsonValueKind.Null
                        ? paletteValue.GetInt32() : 0;
                    AppendCanonicalSpritePart(content, CannonCoverAppearance(
                        appearance.GetProperty("tileRow").GetInt32(), appearance.GetProperty("tileColumn").GetInt32(),
                        palette, appearance.GetProperty("priority").GetInt32(),
                        appearance.GetProperty("flipX").GetBoolean(), appearance.GetProperty("flipY").GetBoolean()));
                }
                foreach (var direction in root.GetProperty("coverTiles").EnumerateArray())
                    foreach (var tile in direction.EnumerateArray()) content.Append("cover tile", tile.GetInt32());
            });
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
        content.Append("priority", part.Priority);
    }

    /// <summary>A cover appearance's single small piece at the origin, from its document fields.</summary>
    private static SpritePart CannonCoverAppearance(int tileRow, int tileColumn, int palette, int priority, bool flipHorizontally, bool flipVertically) =>
        SpritePart.Single(tileRow * SpriteArtwork.CharactersPerRow + tileColumn, palette, priority,
            (flipHorizontally ? TileFlip.Horizontal : TileFlip.None) | (flipVertically ? TileFlip.Vertical : TileFlip.None));
    /// <summary>Checks arm-cannon drawing commands and extracted placement through native OAM results.</summary>
    internal static void VerifyLookupStream1CannonDrawingControls(ISnesAddressSpace rom)
    {
        byte[] json = File.ReadAllBytes(Path.Combine(CartridgeImportContent.ImportData(GameInstallationLayout.SamusBodyDirectoryName), SamusArmCannonArtworkFormat.ArtworkFileName));
        var document = RequiredJson.Deserialize<SamusArmCannonArtworkDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        SamusArmCannonSourceArtwork Load(SamusArmCannonArtworkDocument value) => SamusArmCannonSourceArtwork.FromPlacement(
            SamusArmCannonSourceArtwork.LoadPlacement(new MemoryStream(SamusArmCannonSourceArtwork.Write(value))));
        var stock = Load(document);
        const int start = 0xC9D9, count = 608;
        for (int index = 0; index < count; index++)
        {
            ushort address = (ushort)(start + index);
            AssertEqual(LookupStream1CannonDrawingControlsRomByte(rom, 0x900000 | address), stock.ReadDrawingByteAt(address), "cannon complete installed byte window");
        }
        foreach (ushort address in new ushort[] { 0, start - 1, start + count, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => stock.ReadDrawingByteAt(address), "cannon drawing bounds remain exact");
        Console.WriteLine("Cannon drawing: the 608-byte installed window holds the cartridge's bytes and its bounds are exact.");
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
        string Identity(SpritePart[] first, SpritePart[] second) => SelectedPresentationHash.Create("InstalledSamusAtmosphericArtwork.v2", hash =>
        {
            hash.Append("type one", first.Length);
            foreach (SpritePart part in first) AppendCanonicalSpritePart(hash, part);
            hash.Append("shared type four", second.Length);
            foreach (SpritePart part in second) AppendCanonicalSpritePart(hash, part);
        });
        AssertEqual(Identity(one, shared), stock.ContentIdentity, "atmospheric canonical selected identity unchanged");
        for (int kind = 0; kind < 2; kind++)
        for (int frame = 0; frame < 4; frame++)
        {
            AssertTrue(stock.TryResolve(kind == 0 ? 1 : 4, frame, out SpritePart installed), "atmospheric installed frame resolves");
            AssertEqual((kind == 0 ? one : shared)[frame], installed, "atmospheric installed appearance matches native");
        }
        AssertTrue(!stock.TryResolve(2, 0, out _), "atmospheric null pointer remains outside installed domain");
        AssertTrue(!stock.TryResolve(1, 4, out _), "atmospheric next frame rejected");
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
        int Stored(EscapeTypewriterProgram program) => ((EscapeTypewriterPresentation.LineSequence)program.Lines).overrides.Count;
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1EscapeTextRomByte(rom, address), LookupStream1EscapeTextRomByte(rom, address + 1));
        int totalFrames = 0;
        foreach (EscapeTypewriterProgramId id in new[] { EscapeTypewriterProgramId.Ceres, EscapeTypewriterProgramId.Zebes })
        {
            var program = stock.Get(id);
            var original = document.Programs[id.ToString()].Lines;
            AssertEqual(original.Length, EscapeTypewriterLines.LineCount(id), "escape text required line grouping");
            AssertEqual(0, Stored(program), "escape text stock contains no sampled line overrides");
            for (int index = 0; index < original.Length; index++)
            {
                var expected = new EscapeTypewriterLine(original[index].Start, original[index].Text);
                AssertEqual(expected, EscapeTypewriterLines.Line(id, index), "escape text direct original placement and wording");
                AssertEqual(expected, program.Lines[index], "escape text installed line");
            }
            // Independently interpret original control words and characters; the production
            // side receives only the installed calculated view and no cartridge reader.
            ushort tileBase = id == EscapeTypewriterProgramId.Ceres ? (ushort)0x3582 : (ushort)0x2610;
            // The program's words are VRAM tilemap words; the port types at the pixel of the layer that tilemap word's cell shows at.
            int tilemapWord = id == EscapeTypewriterProgramId.Ceres ? GameplayVramLayout.Bg1TilemapWord : GameplayVramLayout.Bg2TilemapWord;
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
                AssertEqual(done, actual.StepInto(actualVram), "escape text native completion");
                AssertEqual(destination - tilemapWord, SuperMetroid.Core.Rooms.Backgrounds.TwoScreenTilemapCells.IndexOf(actual.Corner), "escape text native cell");
                AssertEqual(delay, actual.Delay, "escape text native delay");
                AssertEqual(timer, actual.DelayTimer, "escape text native countdown");
                AssertEqual(glyphs, actual.GlyphsWritten, "escape text native glyph count");
                AssertEqual(click, actual.ClickRequested, "escape text native click order");
                AssertTrue(expectedVram.ImageEquals(actualVram), "escape text native per-character VRAM");
                totalFrames++;
            }
            AssertTrue(done, "escape text native program terminates");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = program.Lines[-1], "escape text negative line");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = program.Lines[original.Length], "escape text next line");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Get(EscapeTypewriterProgramId.None), "escape text rejects absent program");
        Console.WriteLine($"Escape text: five direct native lines, zero stock overrides and {totalFrames} actual native-oracle frames pass.");
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
        AssertTrue(SamusProjectileRecordDefinitions.TimedRecordAddresses.SequenceEqual(expected), "Exact calculated pointer domain/order");
        AssertEqual(expected.Count, SamusProjectileRecordDefinitions.TimedRecordAddresses.Count, "Calculated pointer view count");
        for (int index = 0; index < expected.Count; index++)
        {
            ushort pointer = expected[index];
            AssertEqual(pointer, SamusProjectileRecordDefinitions.TimedRecordAddresses[index], "Calculated indexed pointer");
            Extent16<RoomPixels> radius = SamusProjectileProgramsVerification.RecordAt(pointer).Radius;
            AssertEqual(Word(0x930000 | (pointer + 4)), LittleEndianBytes.Word((byte)radius.X, (byte)radius.Y), "Typed record radii are the native bytes");
        }
        var document = new ProjectileFrameBindingDocument
        {
            Version = ProjectileFrameBindingFormat.Version,
            Frames = expected.ToDictionary(pointer => ProjectileFrameBindingFormat.FrameNameAt(pointer),
                pointer => ProjectileSpriteDefinitions.Name(Word(0x930000 | (pointer + 2)))),
        };
        var guard = NewLookupFlareForbiddenBus();
        int interpreted = 0, bombInterpreted = 0;
        var bindings = ProjectileFrameBindingFile.Load(new MemoryStream(ProjectileFrameBindingFile.Write(document)));
        var shots = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount)) { Animation = { FrameBindings = bindings } };
        var bombs = new SamusBombProjectileSystem { FrameBindings = bindings };
        foreach (ushort pointer in expected)
        {
            ushort duration = Word(0x930000 | pointer);
            ushort sprite = Word(0x930000 | (pointer + 2));
            var slot = new SamusProjectile(0) { FrameHold = WrappingCountdown.Start(1), Playback = SamusProjectileProgramAddresses.PlaybackAt(pointer) };
            AssertTrue(!shots.Animation.RunProjectileInstructionHandler(slot), "Actual native timed frame survives");
            AssertEqual(duration, slot.FrameHold.RawValue, "Actual timer unchanged");
            AssertEqual(sprite, slot.SpritemapPointer, "Installed artwork selection");
            AssertEqual(LookupStream1ProjectileRadiusLayoutRomByte(rom, 0x930000 | (pointer + 4)), slot.Radius.X, "Actual X radius");
            AssertEqual(LookupStream1ProjectileRadiusLayoutRomByte(rom, 0x930000 | (pointer + 5)), slot.Radius.Y, "Actual Y radius");
            AssertEqual<int?>(Word(0x930000 | (pointer + 6)), ProjectileTrails.GetTrailAnimationFrame(slot), "Actual trail phase unchanged");
            AssertEqual(unchecked((ushort)(pointer + 8)), slot.InstructionPointer, "Actual record progression unchanged");
            interpreted++;
            if (duration == 0) continue; // Existing bomb contract excludes zero-duration timed records.
            var bomb = new SamusBombProjectileSlot(0) { FrameHold = WrappingCountdown.Start(1), Playback = SamusProjectileProgramAddresses.PlaybackAt(pointer), Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(0x0500) };
            AssertTrue(!bombs.RunProjectileInstructionHandler(bomb), "Actual bomb timed frame survives");
            AssertEqual(duration, bomb.FrameHold.RawValue, "Actual bomb timer unchanged");
            AssertEqual(sprite, bomb.SpritemapPointer, "Installed bomb artwork selection");
            AssertEqual(LookupStream1ProjectileRadiusLayoutRomByte(rom, 0x930000 | (pointer + 4)), bomb.Radius.X, "Actual bomb X radius");
            AssertEqual(LookupStream1ProjectileRadiusLayoutRomByte(rom, 0x930000 | (pointer + 5)), bomb.Radius.Y, "Actual bomb Y radius");
            AssertEqual(unchecked((ushort)(pointer + 8)), bomb.InstructionPointer, "Actual bomb record progression");
            bombInterpreted++;
        }
        AssertThrows<ArgumentOutOfRangeException>(() => _ = SamusProjectileRecordDefinitions.TimedRecordAddresses[-1], "Lower indexed view rejection");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = SamusProjectileRecordDefinitions.TimedRecordAddresses[expected.Count], "Upper indexed view rejection");
        AssertEqual(Extent16<RoomPixels>.Zero, SamusProjectilePrograms.InitialRadius(null), "A shot with no list (the safe Murder Beam) has zero radii");
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
        var document = new ProjectileFrameBindingDocument
        {
            Version = ProjectileFrameBindingFormat.Version,
            Frames = records.ToDictionary(pointer => ProjectileFrameBindingFormat.FrameNameAt(pointer),
                pointer => ProjectileSpriteDefinitions.Name(Word(0x930000 | (pointer + 2)))),
        };
        var bindings = ProjectileFrameBindingFile.Load(new MemoryStream(ProjectileFrameBindingFile.Write(document)));
        var guard = NewLookupFlareForbiddenBus();
        int actualTicks = 0, loops = 0, deletions = 0;
        foreach (var program in programs)
        {
            var shots = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount)) { Animation = { FrameBindings = bindings } };
            var bombs = new SamusBombProjectileSystem { FrameBindings = bindings };
            var slot = new SamusProjectile(0) { FrameHold = WrappingCountdown.Start(1), Playback = SamusProjectileProgramAddresses.PlaybackAt(program.Start), Damage = 30 };
            var bomb = new SamusBombProjectileSlot(0) { FrameHold = WrappingCountdown.Start(1), Playback = SamusProjectileProgramAddresses.PlaybackAt(program.Start), Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(0x0500) };
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
                        timer = word; sprite = Word(0x930000 | (pointer + 2));
                        x = LookupStream1ProjectileProgramsRomByte(rom, 0x930000 | (pointer + 4)); y = LookupStream1ProjectileProgramsRomByte(rom, 0x930000 | (pointer + 5));
                        trail = Word(0x930000 | (pointer + 6)); pointer += 8;
                    }
                }
                AssertEqual(deleted, shots.Animation.RunProjectileInstructionHandler(slot), "Exact native projectile deletion tick");
                AssertEqual(deleted, bombs.RunProjectileInstructionHandler(bomb), "Exact native bomb deletion tick");
                actualTicks++;
                if (deleted)
                {
                    AssertEqual(0, slot.Damage, "Projectile deletion clears occupancy");
                    AssertTrue(bomb.Type.IsNone, "Bomb deletion clears occupancy");
                    deletions++; finished = true; break;
                }
                AssertEqual(new VerifyLookupStream1ProjectileProgramsCase(timer, pointer, sprite, x, y, trail),
                    new VerifyLookupStream1ProjectileProgramsCase(slot.FrameHold.RawValue, slot.InstructionPointer, slot.SpritemapPointer, slot.Radius.X, slot.Radius.Y,
                        ProjectileTrails.GetTrailAnimationFrame(slot) ?? throw new InvalidOperationException("A played projectile shows a record.")),
                    "Every scheduled projectile exposure/physical envelope/trail phase matches native");
                AssertEqual(new LookupStream1VerifyLookupStream1ProjectileProgramsCase(timer, pointer, sprite, x, y),
                    new LookupStream1VerifyLookupStream1ProjectileProgramsCase(bomb.FrameHold.RawValue, bomb.InstructionPointer, bomb.SpritemapPointer, bomb.Radius.X, bomb.Radius.Y),
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
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        SamusArmCannonSourceArtwork Load(SamusArmCannonArtworkDocument value) => SamusArmCannonSourceArtwork.FromPlacement(
            SamusArmCannonSourceArtwork.LoadPlacement(new MemoryStream(SamusArmCannonSourceArtwork.Write(value))));
        var stock = body.ArmCannon;
        AssertEqual(Load(document).ContentIdentity, stock.ContentIdentity, "Body binding preserves canonical cannon identity");
        for (int index = 0; index < document.DrawingData.Length; index++)
        {
            ushort address = (ushort)(0xc9d9 + index);
            AssertEqual(LookupStream1CannonPlacementRomByte(rom, 0x900000 | address), stock.ReadDrawingByteAt(address), "Every installed native cannon byte");
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
                var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Pose = pose, AnimationFrame = frame, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), SelectedHudItem = HudItem.Missile };
                samus.Presentation.BindBodyFixture(body);
                var cannon = new SamusArmCannonState { Artwork = stock.Gameplay };
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
        // the pose shows that the drawing window holds, and no displacement at any other frame; the installed document reads
        // back as written.
        SamusShownFrames shown = SamusShownFrames.Of(InstalledFixtures.AnimationContinuation);
        SamusArmCannonLayout installed = stock.Installed(InstalledFixtures.AnimationContinuation);
        int installedFrames = 0;
        foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses)
        {
            SamusArmCannonPlacement decoded = stock.Gameplay.Placement(pose), placed = installed.Gameplay.Placement(pose);
            AssertEqual(decoded.DrawOrder, placed.DrawOrder, $"Installed cannon draw order of {pose}");
            AssertEqual(decoded.FirstDirection, placed.FirstDirection, $"Installed cannon direction of {pose}");
            AssertEqual(decoded.LaterDirection, placed.LaterDirection, $"Installed cannon later direction of {pose}");
            for (int frame = 0; frame <= byte.MaxValue; frame++)
            {
                if (shown.TryPictureFrame(pose, frame, out _) && decoded.Offsets.ContainsKey(frame))
                {
                    AssertEqual(decoded.Offset(frame), placed.Offset(frame), $"Installed cannon displacement of {pose} at frame {frame}");
                    installedFrames++;
                }
                else
                {
                    int unshown = frame;
                    AssertThrows<InvalidDataException>(() => placed.Offset(unshown), $"No installed cannon displacement of {pose} at unshown frame {frame}");
                }
            }
        }
        AssertEqual(installed.ContentIdentity, SamusArmCannonLayout.Read(new MemoryStream(installed.Write())).ContentIdentity,
            "Installed cannon layout reads back as written");
        Console.WriteLine($"Cannon placement:608 extracted native bytes,the canonical hash, {draws} actual OAM draws and {installedFrames} installed pose frames pass.");
    }

    /// <summary>Reads Samus arm-cannon placement source words.</summary>
    private static byte LookupStream1CannonPlacementRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Checks Samus spritemap pointer roots and the installed OAM definitions.</summary>
    internal static void VerifyLookupStream1OamPointers(ISnesAddressSpace rom)
    {
        var stock = InstalledFixtures.SamusBody.Spritemaps;
        ushort[] nativePointers = Enumerable.Range(0, 2096).Select(index =>
            LittleEndianBytes.Word(LookupStream1OamPointersRomByte(rom, 0x92808d + index * 2), LookupStream1OamPointersRomByte(rom, 0x92808e + index * 2))).ToArray();
        SamusSpritemapId?[] native = SamusSpritemapNative.Entries(nativePointers);
        int compositionCount = stock.Definitions.Count;
        AssertEqual(SamusSpritemapNative.Compositions(nativePointers).Length, compositionCount, "One installed composition per referenced native record");
        AssertTrue(stock.Entries.SequenceEqual(native), "All native pointer observations preserved");
        // Every pose half's animation frame shows the composition of the native entry its base and frame index, for
        // every frame the body sheets cover that the table holds; a frame past those is outside installed art.
        int poseFrames = 0;
        foreach (int pose in Enumerable.Range(0, SamusBodySources.PoseCount).Where(SamusPoseNumbering.NamesPose))
        foreach (SamusBodyHalf half in new[] { SamusBodyHalf.Top, SamusBodyHalf.Bottom })
        for (int frame = 0; ; frame++)
        {
            var poseFrame = new SamusBodyHalfFrame(SamusPoseNumbering.FromNumber(pose), half, frame);
            int index = stock.EntryIndex(poseFrame);
            if (frame == SamusBodySheetDefinitions.AnimationFrames || index >= SamusSpritemapSources.EntryCount)
            {
                AssertThrows<InvalidDataException>(() => stock.TryGetComposition(poseFrame, out _), "A frame past the covered frames is outside installed art");
                break;
            }
            AssertEqual(native[index], stock.TryGetComposition(poseFrame, out SamusSpritemapId composition) ? composition : null,
                $"Pose {pose:X2} {half} frame {frame} shows native entry {index:X4}");
            poseFrames++;
        }
        IReadOnlyDictionary<SamusPoseId, int> top = stock.TopBases, bottom = stock.BottomBases;
        SamusSpritemapDefinition[] definitions = stock.Definitions.ToArray();
        string Canonical(SamusSpritemapId?[] entries) => SelectedPresentationHash.Create("samus-spritemaps-v4", content =>
        {
            foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses) content.Append("top base", top[pose]);
            foreach (SamusPoseId pose in SamusPoseNumbers.RecordedPoses) content.Append("bottom base", bottom[pose]);
            foreach (SamusSpritemapId? entry in entries) content.Append("entry", entry?.Ordinal ?? -1);
            foreach (var map in definitions)
            {
                content.Append("part count", map.Parts.Length);
                foreach (SpritePart part in map.Parts) AppendCanonicalSpritePart(content, part);
            }
        });
        AssertEqual(Canonical(native), stock.ContentIdentity, "Original native OAM canonical hash");
        AssertThrows<InvalidDataException>(() => stock.TryGet(2096, out _), "Original upper index rejection");
        AssertThrows<InvalidDataException>(() => stock.TryGet(ushort.MaxValue, out _), "Original full ushort rejection");
        VerifySamusZeroOamSelectorDrawsNothing(stock);
        Console.WriteLine($"Samus OAM pointers:2096 native selectors, {poseFrames} pose-half frames mapped to their entries,the canonical hash,the zero selector and domain checks pass.");
    }

    /// <summary>Reads the native Samus spritemap OAM pointer table.</summary>
    private static byte LookupStream1OamPointersRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>
    /// Checks that a zero OAM pointer-table entry draws nothing: it would dereference $92:0000 direct-page scratch,
    /// which the port exempts (owner decision on #1318).
    /// </summary>
    private static void VerifySamusZeroOamSelectorDrawsNothing(SamusSpritemapSources stock)
    {
        OamBuffer oam = SamusSpritemapDrawsAccess.SamusSpritemapOam(3, new Position16<ScreenPixels>(127, 131),
            SpriteFrameTestArtwork.Spritemaps(stock), new SpriteSource(SpriteSourceKind.Samus));
        OamBuffer empty = SpriteFrameBuffer.DrawnOam(_ => { });
        AssertTrue(oam.LowTable.SequenceEqual(empty.LowTable) && oam.HighTable.SequenceEqual(empty.HighTable),
            "a zero selector draws no OAM entry through the actual renderer");
    }

    /// <summary>Checks the installed body tile pixels against the cartridge and the canonical stock hash.</summary>
    internal static void VerifyLookupStream1BodyPixels(ISnesAddressSpace rom)
    {
        string directory = CartridgeImportContent.Component(GameInstallationLayout.SamusBodyDirectoryName);
        var bodyArt = SamusBodyArtworkFiles.LoadCharacters(directory, CartridgeImportContent.ImportData(GameInstallationLayout.SamusBodyDirectoryName));
        var body = bodyArt.Body;
        var top = Enumerable.Range(0, 13).Select(set => bodyArt.TopSet(set).ToArray()).ToArray();
        var bottom = Enumerable.Range(0, 11).Select(set => bodyArt.BottomSet(set).ToArray()).ToArray();
        int bytes = 0;
        for (int half = 0; half < 2; half++)
        {
            bool upper = half == 0;
            var groups = upper ? top : bottom;
            for (int set = 0; set < groups.Length; set++)
            for (int position = 0; position < groups[set].Length; position++)
            {
                int glyphSource = LookupStream1BodySource(rom, upper, set, position);
                byte[] image = groups[set][position].Planar.ToArray();
                for (int index = 0; index < image.Length; index++)
                {
                    AssertEqual(LookupStream1BodyPixelsRomByte(rom, glyphSource + index), image[index], "Every native body pixel byte");
                    bytes++;
                }
            }
        }
        AssertEqual(130464, bytes, "Complete native body planar domain");
        AssertEqual(CanonicalBodyHash(body, body.TopSetPointers.ToArray(), body.BottomSetPointers.ToArray(), body.PoseFirstFrames, body.Frames.ToArray()), body.ContentIdentity, "Original native body hash");
        Console.WriteLine($"Body pixels: {bytes} installed native bytes and the canonical hash pass.");
    }

    /// <summary>Where the cartridge stores body record <paramref name="position"/> of set <paramref name="set"/>'s planar bytes, read from its native descriptor.</summary>
    private static int LookupStream1BodySource(ISnesAddressSpace rom, bool upper, int set, int position)
    {
        ushort Word(int at) => (ushort)(rom.ReadCartridgeByte(at) | rom.ReadCartridgeByte(at + 1) << 8);
        int address = 0x920000 | (Word((upper ? 0x92d91e : 0x92d938) + set * 2) + position * 7);
        return (int)new SnesAddress(rom.ReadCartridgeByte(address + 2), Word(address));
    }

    /// <summary>Reads planar Samus body tile bytes before derived pixel expansion.</summary>
    private static byte LookupStream1BodyPixelsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
    /// <summary>Checks body tile DMA definitions and actual native-sized VRAM transfer output.</summary>
    internal static void VerifyLookupStream1BodyTransfers(ISnesAddressSpace rom)
    {
        string directory = CartridgeImportContent.Component(GameInstallationLayout.SamusBodyDirectoryName);
        var bodyArt = SamusBodyArtworkFiles.LoadCharacters(directory, CartridgeImportContent.ImportData(GameInstallationLayout.SamusBodyDirectoryName));
        var body = bodyArt.Body;
        ushort Word(int address) => LittleEndianBytes.Word(LookupStream1BodyTransfersRomByte(rom, address), LookupStream1BodyTransfersRomByte(rom, address + 1));
        var top = Enumerable.Range(0, 13).Select(set => bodyArt.TopSet(set).ToArray()).ToArray();
        var bottom = Enumerable.Range(0, 11).Select(set => bodyArt.BottomSet(set).ToArray()).ToArray();
        int records = 0;
        for (int half = 0; half < 2; half++)
        {
            bool upper = half == 0;
            var groups = upper ? top : bottom;
            for (int set = 0; set < groups.Length; set++)
            for (int position = 0; position < groups[set].Length; position++)
            {
                var value = groups[set][position];
                int address = 0x920000 | (Word((upper ? 0x92d91e : 0x92d938) + set * 2) + position * 7);
                int source = (int)new SnesAddress(LookupStream1BodyTransfersRomByte(rom, address + 2), Word(address));
                ushort first = Word(address + 3), second = Word(address + 5);
                AssertEqual(first, value.FirstSize, "Installed native first row");
                AssertEqual(second, value.SecondSize, "Installed native remaining row");
                byte[] pixels = value.Planar.ToArray();
                for (int index = 0; index < pixels.Length; index++) AssertEqual(LookupStream1BodyTransfersRomByte(rom, source + index), pixels[index], "Separate native pixel payload preserved");
                records++;
            }
        }
        AssertEqual(435, records, "Complete original metadata domain");
        AssertEqual(CanonicalBodyHash(body, body.TopSetPointers.ToArray(), body.BottomSetPointers.ToArray(), body.PoseFirstFrames, body.Frames.ToArray()),
            body.ContentIdentity, "Exact canonical stock metadata hash");
        // These exact native selectors include the known physical cross-group windows.
        var guard = NewCartridgeReadGuard(rom);
        int transfers = 0;
        foreach (var selected in new VerifyLookupStream1BodyTransfersCase[] { new VerifyLookupStream1BodyTransfersCase(SamusPoseId.FacingRightNormalPose, 0), new VerifyLookupStream1BodyTransfersCase(SamusPoseId.CompactAirborneFallingRightPose, 6), new VerifyLookupStream1BodyTransfersCase(SamusPoseId.CompactAirborneFallingRightPose, 7), new VerifyLookupStream1BodyTransfersCase(SamusPoseId.CompactAirborneFallingLeftPose, 8), new VerifyLookupStream1BodyTransfersCase(SamusPoseId.DeathSequenceLeftPose, 0) })
        {
            // The half pictures the import composes the frame's whole picture of (#1585).
            SamusBodyFramePictures pictures = body.Pictures(selected.Pose, selected.Phase);
            int shownTop = 0x920000 | SamusBodyNative.RecordPointer(SamusBodySources.Record(pictures.Top));
            int? shownBottom = pictures.Bottom is { } lowerPicture ? 0x920000 | SamusBodyNative.RecordPointer(SamusBodySources.Record(lowerPicture)) : null;
            int frameAddress = 0x920000 | (Word(0x92d94e + SamusPoseNumbers.Number(selected.Pose) * 2) + selected.Phase * 4);
            int topAddress = 0x920000 | (Word(0x92d91e + LookupStream1BodyTransfersRomByte(rom, frameAddress) * 2) + LookupStream1BodyTransfersRomByte(rom, frameAddress + 1) * 7);
            AssertEqual(topAddress, shownTop, "Native physical upper selection including cross-group position");
            bool namesBottom = LookupStream1BodyTransfersRomByte(rom, frameAddress + 2) != 255;
            if (namesBottom)
                AssertEqual(0x920000 | (Word(0x92d938 + LookupStream1BodyTransfersRomByte(rom, frameAddress + 2) * 2) + LookupStream1BodyTransfersRomByte(rom, frameAddress + 3) * 7),
                    shownBottom, "Native physical lower selection including cross-group position");
            else
                AssertEqual(null, shownBottom, "A selection naming no lower half composes none");
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
            Copy(shownTop, 0);
            if (shownBottom is int bottomAddress) Copy(bottomAddress, SpriteArtwork.CharactersPerRow / 2);
            AssertTrue(bodyArt.BodyArtwork(pictures.Top, pictures.Bottom).Characters.SequenceEqual(expected),
                "Composed body pictures match the native split copies and physical cross-group aliases");
            transfers++;
        }
        Console.WriteLine($"Body transfers:{records} direct native records,zero stock metadata overrides,canonical hash and{transfers} actual split DMA/cross-group selections pass.");
    }

    /// <summary>Reads native Samus body DMA source and transfer words.</summary>
    private static byte LookupStream1BodyTransfersRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

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

    /// <summary>Runs the charge flare cadence program lookups against the retail cartridge.</summary>
    [VerificationCommand("--lookup-stream-1-cadence-programs")]
    internal static int LookupStream1CadenceProgramsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyLookupStream1CadencePrograms), () => VerifyLookupStream1CadencePrograms(rom));
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
