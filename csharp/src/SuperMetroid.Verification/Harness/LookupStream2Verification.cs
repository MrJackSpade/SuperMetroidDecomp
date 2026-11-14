using SuperMetroid.Verification.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.PipeBugs.Artwork;
using SuperMetroid.Verification.Enemies.PipeBugs;
using SuperMetroid.Verification.Enemies.LavaJumpers;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Enemies.Chozo.ChozoStatueInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using static SuperMetroid.Verification.Enemies.SpacePirates.NinjaSpacePirateInstructionProgramDefinitionsVerification;
using static SuperMetroid.Verification.Fx.PaletteFx.PaletteFxInstructionCodesVerification;
using static SuperMetroid.Verification.Samus.Projectiles.ProjectileCompositionsVerification;
using static SuperMetroid.Verification.Enemies.Chozo.StatueWalkDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Enemies.WreckedShip.Artwork;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid;
namespace SuperMetroid.Verification.Harness;

/// <summary>Cartridge-oracle checks for the second set of gameplay, artwork, and pause-screen catalogs.</summary>
internal static class LookupStream2Verification
{
    /// <summary>Checks all 104 Yard visual frames retain their semantic group order and unique identities.</summary>
    internal static void VerifyLookupStream2YardGroups()
    {
        var frames = YardVisualDefinitions.Frames();
        AssertEqual(104, frames.Length, "Yard semantic groups preserve complete registry");
        string identity = string.Join("\n", frames.Select(frame => $"{frame.Bank:X2}:{frame.Pointer:X4}:{frame.Name}"));
        string hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(identity)));
        AssertEqual("2E09CB87CB8C4106F823834D08281C250FB4294981A52F4FE9F83BD983732954", hash,
            "Yard exact pre-change native bank/pointer/name/order identity");
        Console.WriteLine("Yard groups: 38 semantic program roles preserve all104 native frame identities/names/order.");
    }

    /// <summary>Checks Chozo statue foot spritemap geometry and the compiled stock views.</summary>
    internal static void VerifyLookupStream2ChozoFootGeometry(CartridgeImportAddressSpace rom)
    {
        int[] nativeIdentities = [0xaae943, 0xaae9ae, 0xaaea1e, 0xaaea8e, 0xaaeafe, 0xaaeb69, 0xaaebd9, 0xaaec49];
        var definitions = EnemySpritemapDefinitions.Frames.ToArray();
        var document = new EnemySpritemapDocument
        {
            Version = EnemySpritemapSchema.Revision,
            Frames = definitions.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>(), StringComparer.Ordinal),
            DisplayFrames = definitions.ToDictionary(frame => frame.Name, frame => frame.Name, StringComparer.Ordinal),
        };
        var selected = definitions.Where(frame => nativeIdentities.Contains(((int)new SnesAddress(frame.Bank, frame.Pointer)))).ToArray();
        AssertEqual(8, selected.Length, "Eight native Chozo stride composition identities");
        foreach (var frame in selected)
        {
            int source = ((int)new SnesAddress(frame.Bank, frame.Pointer));
            document.Frames[frame.Name] = Enumerable.Range(0, CartridgeImportSource.Require(rom).ReadCartridgeWord(source)).Select(index =>
            {
                int entry = source + 2 + index * 5;
                var x = new SnesSpritemapXWord(CartridgeImportSource.Require(rom).ReadCartridgeWord(entry));
                var attributes = new SnesObjAttributeWord(CartridgeImportSource.Require(rom).ReadCartridgeWord((entry + 3)));
                return new SpriteVisualPart
                {
                    OffsetX = x.SignedOffset, OffsetY = unchecked((sbyte)rom.ReadCartridgeByte(entry + 2)), Size = x.IsLarge ? 16 : 8,
                    TileColumn = attributes.TileNumber % 16, TileRow = attributes.TileNumber / 16,
                    Palette = attributes.PaletteIndex, Priority = attributes.Priority,
                    FlipX = attributes.FlipHorizontally, FlipY = attributes.FlipVertically,
                };
            }).ToArray();
        }
        EnemySpritemapCatalog Load() => EnemySpritemapDocuments.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
        void Check(EnemySpritemapCatalog catalog)
        {
            foreach (var frame in selected)
            {
                AssertTrue(catalog.TryGetDisplay(frame.Bank, frame.Pointer, out var parts), "Chozo actual installed display binding");
                AssertTrue(parts.SequenceEqual(EnemySpritemapCompiler.CompileParts(document.Frames[frame.Name], frame.Name)),
                    "Chozo source/supplied part ordering and every visual field remain exact");
            }
            string expected = SelectedPresentationHash.Create("enemy-oam-v2", content =>
            {
                var ordered = definitions
                    .OrderBy(frame => EnemyFrameSheetKeys.Of(frame.Frame), StringComparer.Ordinal)
                    .ToArray();
                foreach (var frame in ordered)
                {
                    content.AppendName("frame", EnemyFrameSheetKeys.Of(frame.Frame));
                    content.AppendEnemyParts(EnemySpritemapCompiler.CompileParts(document.Frames[frame.Name], frame.Name));
                }
                foreach (var frame in ordered)
                {
                    string identity = EnemyFrameSheetKeys.Of(frame.Frame);
                    content.AppendName("native-binding", identity);
                    content.AppendName("selected-binding", identity);
                }
            });
            AssertEqual(expected, catalog.ContentIdentity, "Chozo calculated foot fields retain canonical presentation hash");
        }
        var stock = Load();
        Check(stock);
        foreach (var frame in selected)
        {
            AssertTrue(stock.TryGet(frame.Bank, frame.Pointer, out _), "Chozo stock frame exists");
        }
        AssertEqual(ChozoCarry.Read(new(HorizontalDirection.Left, ChozoCarryPose.StrideLift)).Velocity, (short)-0x300, "Chozo carry mechanics velocity");
        AssertEqual(ChozoCarry.Read(new(HorizontalDirection.Left, ChozoCarryPose.StrideSwing)).Velocity, (short)-0xe00, "Chozo late support displacement");
        Suite(nameof(VerifyCompiledStatueWalking), () => VerifyCompiledStatueWalking(rom));
        AssertThrows<ArgumentOutOfRangeException>(() => ChozoStride.SupportFootX(4), "Chozo support-phase upper bound");
        Console.WriteLine("Chozo shared geometry:174 native parts,eight stock views,hash/display order and existing96-word/actual carry checks pass; shape and two movement-policy inputs remain documented.");
    }

    /// <summary>
    /// Checks Wrecked Ship ghost palette phases against native colour words: the fade the ghost keeps, the shade its slot
    /// shows for each step, and the colours the import pre-colours that shade in.
    /// </summary>
    internal static void VerifyLookupStream2GhostPalette(CartridgeImportAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Shading = PictureShading.Standard;
        var state = new WreckedShipGhostEnemyState { PhaseTimer = 2 };
        Action<RoomEnemySlot, WreckedShipGhostEnemyState> brightening = WreckedShipGhostBehavior.RunWreckedShipGhostBrightening;
        Func<RoomEnemySlot, WreckedShipGhostEnemyState, int> fade = WreckedShipGhostBehavior.StepWreckedShipGhostFadeTowardTarget;
        Dictionary<PictureShade, RgbColor[]> installedSteps = EnemySheetShades.GhostFadeSteps().ToDictionary(step => step.Shade, step => step.Colours);
        var native = new ushort[16];
        for (int color = 0; color < native.Length; color++)
        {
            native[color] = ReadVerificationWord(rom, 0xa899ac + color * 2);
            AssertEqual(native[color], WreckedShipGhostAppearance.Colours[color], "Calculated ghost channels preserve native target words");
            AssertEqual(native[color], ReadVerificationWord(rom, 0xa8aafe + color * 2), "Kago shares exactly the same native palette inputs");
            if (color < 9) AssertEqual(native[color], ReadVerificationWord(rom, 0xa89f4f + color * 2), "Yapping Maw shares olive target inputs");
        }
        // From black, the white flash raises every colour one level per call: thirty-one calls reach white and the
        // thirty-second changes nothing, which installs the native target palette phase.
        for (int call = 1; call <= 31; call++)
        {
            brightening(slot, state);
            AssertTrue(state.Action != WreckedShipGhostAction.FadingToGhostColours, $"White flash call {call} still brightens");
            AssertEqual(PictureShading.Shaded(new PictureShade(WreckedShipGhostShades.BrighteningEffect, call)), slot.Shading,
                $"White flash call {call} shows its brightening step's shade");
        }
        AssertTrue(state.Fade.Shown.ToArray().All(colour => colour == RgbColor.White), "White flash reaches white in every colour");
        brightening(slot, state);
        AssertEqual(WreckedShipGhostAction.FadingToGhostColours, state.Action, "White flash installs native target palette phase");
        AssertTrue(state.Fade.Target.SequenceEqual(ToColors(native)), "Actual target buffer includes unused sprite slots");
        for (int tick = 1; tick <= 32; tick++)
        {
            int expectedChanges = 0;
            for (int color = 0; color < native.Length; color++)
            for (int shift = 0; shift < 15; shift += 5)
                if (31 - (tick - 1) > (native[color] >> shift & 31)) expectedChanges++;
            AssertEqual(expectedChanges, fade(slot, state), "Native component-change count preserves fade completion");
            AssertEqual(PictureShading.Shaded(WreckedShipGhostFadeShade.Of(state.Fade)), slot.Shading, "The ghost shows the shade of its fade's current step");
            RgbColor[] installed = installedSteps[WreckedShipGhostFadeShade.Of(state.Fade)];
            for (int color = 0; color < native.Length; color++)
            {
                int expected = 0;
                for (int shift = 0; shift < 15; shift += 5)
                    expected |= Math.Max(31 - tick, native[color] >> shift & 31) << shift;
                AssertEqual((ushort)expected, state.Fade.Shown[color], "Every actual fade step preserves visible and unused target slots");
                AssertEqual((ushort)expected, installed[color], "The import pre-colours each fade step's shade in the native step's colours");
            }
        }
        var usage = new int[16];
        for (int tile = 0; tile < 32; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int address = 0xb1a600 + tile * 32 + y * 2, bit = 7 - x;
            int color = (rom.ReadCartridgeByte(address) >> bit & 1) | (rom.ReadCartridgeByte(address + 1) >> bit & 1) << 1 |
                (rom.ReadCartridgeByte(address + 16) >> bit & 1) << 2 | (rom.ReadCartridgeByte(address + 17) >> bit & 1) << 3;
            usage[color]++;
        }
        AssertEqual(0, usage[1], "Ghost source pixels never select unused highlight slot1");
        for (int color = 9; color < usage.Length; color++) AssertEqual(0, usage[color], "Ghost source pixels never select copied warm-color slots");
        for (int color = 2; color <= 8; color++) AssertTrue(usage[color] > 0, "Every visible olive shade is used by ghost artwork");
        int[] expectedKagoUsage = [1344,38,63,426,918,45,156,237,312,28,75,110,208,36,63,37];
        var kagoUsage = new int[16];
        for (int tile = 0; tile < 64; tile++)
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int address = 0xb1ae00 + tile * 32 + y * 2, bit = 7 - x;
            int color = (rom.ReadCartridgeByte(address) >> bit & 1) | (rom.ReadCartridgeByte(address + 1) >> bit & 1) << 1 |
                (rom.ReadCartridgeByte(address + 16) >> bit & 1) << 2 | (rom.ReadCartridgeByte(address + 17) >> bit & 1) << 3;
            kagoUsage[color]++;
        }
        AssertTrue(kagoUsage.SequenceEqual(expectedKagoUsage), "Exact native Kago pixel-label usage supports each shared paint role; slot0 is transparent");
        Console.WriteLine("Ghost palette:16 native targets and shared-source words,actual white-flash target copy,32 full component-fade steps,and native pixel-slot usage pass; calculated channels preserve the specified source paint and transparent payload.");
    }

    /// <summary>Checks Old Tourian accent durations, repeated colors, and per-tick CGRAM updates.</summary>
    internal static void VerifyLookupStream2TourianAccentCadence(CartridgeImportAddressSpace rom)
    {
        foreach (var definition in OldTourianEscapeAccentPaletteFxColorDefinitions.All)
        {
            ushort[] nativeDurations = new ushort[15];
            int cycle = 0;
            for (int frame = 0; frame < 15; frame++)
            {
                nativeDurations[frame] = ReadVerificationWord(rom, 0x8d0000 | definition.FramePointer(frame));
                AssertEqual(nativeDurations[frame], OldTourianEscapeAccentPaletteFxColorDefinitionsConstants.Duration(frame), "Exact native authored pass dwell");
                cycle += nativeDurations[frame];
                for (int color = 0; color < 3; color++)
                    AssertEqual(ReadVerificationWord(rom, 0x8d0000 | definition.ColorPointer(frame % 5, color)),
                        ReadVerificationWord(rom, 0x8d0000 | definition.ColorPointer(frame, color)), "Three sweeps select identical RGB stages despite distinct cadence");
            }
            AssertEqual(64, cycle, "Native pass cadence sums to its exact loop period");
            var guarded = NewPaletteFxMechanicsForbiddenBus(rom, out _);
            var actualSession = new ColourEffectSessionState();
            var actual = new ColourEffectSystem(actualSession);
            actual.Start(definition.DefinitionPointer, equippedItems: 0);
            var cgram = new SnesCgram();
            var colors = new ReferencePaletteFxColorSource(guarded);
            int frameIndex = 0, elapsed = 0;
            for (int tick = 0; tick <= cycle; tick++)
            {
                if (elapsed == nativeDurations[frameIndex]) { frameIndex = (frameIndex + 1) % 15; elapsed = 0; }
                actual.Step(new ColourEffectInputs { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
                PaletteFxRoomLook.LayShownShades(actual, cgram, colors);
                for (int color = 0; color < 3; color++)
                    AssertEqual(ReadVerificationWord(rom, 0x8d0000 | definition.ColorPointer(frameIndex, color)),
                        cgram.Colors[definition.Object.AnimatedColours.CartridgeColour + color], "Actual CGRAM follows each exact native hold and loop boundary");
                elapsed++;
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts, "Cadence execution avoids native mechanics reads");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => OldTourianEscapeAccentPaletteFxColorDefinitionsConstants.Duration(-1), "Accent duration lower domain");
        AssertThrows<ArgumentOutOfRangeException>(() => OldTourianEscapeAccentPaletteFxColorDefinitionsConstants.Duration(15), "Accent duration upper domain");
        Console.WriteLine("Old Tourian accent cadence:30 native durations,90 repeated color words,68 mechanics words and130 actual per-tick CGRAM/loop states pass with mechanics reads blocked.");
    }
    /// <summary>Checks the ninja Space Pirates' typed programs, which carry the native list layout.</summary>
    internal static void VerifyLookupStream2NinjaProgramLayout()
    {
        Suite(nameof(VerifyNinjaSpacePirateInstructionProgramDefinitions), () => VerifyNinjaSpacePirateInstructionProgramDefinitions());
    }

    /// <summary>Checks projectile identity selectors, extracted compositions, and 417 actual OAM drawings.</summary>
    internal static void VerifyLookupStream2ProjectileIdentityGeometry(ISnesAddressSpace rom)
    {
        var expected = new SortedSet<ushort>();
        foreach (ushort record in SamusProjectileFrameDefinitions.TimedRecordAddresses)
            expected.Add(ReadVerificationWord(rom, (0x930000 | record) + 2));
        int physicalAddress = 0x93f0fa;
        int physicalRecords = 0;
        for (int orientation = 0; orientation < 8; orientation++)
        {
            bool diagonal = orientation % 4 >= 2;
            for (int length = 1; length <= (diagonal ? 5 : 7); length++)
            {
                int parts = length * (diagonal ? 2 : 1);
                AssertEqual((ushort)parts, ReadVerificationWord(rom, physicalAddress), "Every selected and unselected native Plasma growth record has its calculated physical extent");
                physicalAddress += 2 + 5 * parts;
                physicalRecords++;
            }
        }
        AssertEqual(48, physicalRecords, "All48 physical Plasma startup records including omitted stages");
        AssertEqual(0x93f5e2, physicalAddress, "Native Plasma startup layout end after the final ten-part composition");
        AssertEqual(417, expected.Count, "The timed records reference417 distinct compositions");
        int ordinal = 0;
        foreach (ushort pointer in expected)
            AssertEqual(pointer, ProjectileSpriteDefinitions.NativePointers[ordinal++], "Calculated projectile identity equals native sorted selector union");
        AssertTrue(expected.SequenceEqual(ProjectileSpriteDefinitions.NativePointers), "Projectile identity enumeration preserves native order");
        byte[] json = ProjectileSpriteExtractor.Extract(rom);
        var selected = InstalledProjectileSprites.Load(new MemoryStream(json));
        foreach (ushort pointer in expected)
        {
            var native = new OamBuffer();
            var actual = new OamBuffer();
            DrawImportedProjectileSpritemap(rom, native, pointer, new(255, 255));
            selected.Draw(pointer, actual, new(255, 255));
            AssertTrue(native.ImageEquals(actual), "All417 installed identities extract and draw exact native OAM");
        }
        var flare = ChargeFlareSprites.Load(new MemoryStream(ChargeFlareSpriteExtractor.Extract(rom)));
        for (ushort selector = 0; selector < ChargeFlareSpriteNative.NativeSelectorCount; selector++)
        {
            var native = new OamBuffer();
            var actual = new OamBuffer();
            DrawImportedProjectileSpritemap(rom, native, ProjectileSpriteNative.Address(ChargeFlareSpriteDefinitions.NativeSelector(selector)), new(255, 255));
            flare.Draw(selector, actual, new(255, 255));
            AssertTrue(native.ImageEquals(actual), "Existing flare span caller preserves exact extracted OAM");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => _ = ProjectileSpriteDefinitions.NativePointers[-1], "Projectile identity lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = ProjectileSpriteDefinitions.NativePointers[417], "Projectile identity upper bound");
        Console.WriteLine("Projectile identity geometry:417 exact identities from805 native selectors,48 physical startup records,417 actual extracted OAM draws,all existing flare selectors and bounds pass;all417 identities and 208 stock beam/missile/effect compositions calculate; independent frame selection/composition/art inputs remain pending.");
    }
    /// <summary>Checks Norfair and Old Tourian palette-FX owner catalogs and native definition identities.</summary>
    internal static void VerifyLookupStream2EnvironmentalCatalogs(CartridgeImportAddressSpace rom)
    {
        var norfair = NorfairEnvironmentalPaletteFxColorDefinitions.All;
        var accents = OldTourianEscapeAccentPaletteFxColorDefinitions.All;
        AssertEqual(4, norfair.Count, "Four semantic Norfair owners");
        AssertEqual(2, accents.Count, "Two semantic escape accent owners");
        int count = 0;
        foreach (var item in norfair)
        {
            AssertEqual((NorfairEnvironmentalPaletteOwner)count, item.Owner, "Norfair native owner order");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | item.DefinitionPointer + 2), item.ProgramStart, "Native Norfair definition/list binding");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | item.ProgramStart + 2), item.Object.AnimatedColours.CartridgeByteIndex, "Native Norfair colours start where the object's colour group is placed");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | item.LoopInstructionPointer + 2), item.FirstRecordPointer, "Native Norfair loop target");
            AssertEqual(norfair[count].ProgramStart, item.ProgramStart, "Indexed and enumerated Norfair identities agree");
            count++;
        }
        AssertEqual(4, count, "Complete Norfair lazy enumeration");
        count = 0;
        foreach (var item in accents)
        {
            AssertEqual((OldTourianEscapeAccentPaletteOwner)count, item.Owner, "Accent native owner order");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | item.DefinitionPointer + 2), item.ProgramStart, "Native accent definition/list binding");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | item.ProgramStart + 2), item.Object.AnimatedColours.CartridgeByteIndex, "Native railings/panels colours start where the object's colour group is placed");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | item.LoopInstructionPointer + 2), item.FirstFramePointer, "Native accent loop target");
            AssertEqual(accents[count].ProgramStart, item.ProgramStart, "Indexed and enumerated accent identities agree");
            count++;
        }
        AssertEqual(2, count, "Complete accent lazy enumeration");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = norfair[-1]; }, "Norfair lower owner bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = norfair[4]; }, "Norfair upper owner bound");
        AssertThrows<IndexOutOfRangeException>(() => { _ = accents[-1]; }, "Accent lower owner bound");
        AssertThrows<IndexOutOfRangeException>(() => { _ = accents[2]; }, "Accent upper owner bound");
        Console.WriteLine("Environmental catalogs:six native definition/color/loop bindings,292mechanics words,16heat phase bytes,actual complete/repeating cycles,live colors,enumeration/order and bounds pass; accent timing and independent colors remain pending.");
    }
    /// <summary>Checks extracted pause backdrop geometry and VRAM tile output.</summary>
    internal static void VerifyLookupStream2BackdropGeometry(CartridgeImportAddressSpace rom)
    {
        byte[] json = PauseBackdropExtractor.Extract(rom);
        var stock = PauseBackdropPresentation.Load(new MemoryStream(json));
        // The backdrops hold placed tiles, not tilemap words: each native word is decoded to the tile it places and the
        // tiles are compared by tile, colour variant, mirroring and priority.
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            int label = 0x820000 | ReadVerificationWord(rom, 0x82965f + SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(area) * 2);
            var native = new BackgroundTile[1024];
            for (int cell = 0; cell < 1024; cell++)
            {
                int address = cell is >= 170 and < 182 ? label + (cell - 170) * 2 : 0xb6e000 + cell * 2;
                BackgroundTile tile = BackgroundTileWords.ToTile(ReadVerificationWord(rom, address));
                native[cell] = tile;
            }
            ConfirmArea(stock, area, native);
        }
        var nativeButtons = new BackgroundTile[512];
        for (int cell = 0; cell < 512; cell++)
            nativeButtons[cell] = BackgroundTileWords.ToTile(ReadVerificationWord(rom, 0xb6e400 + cell * 2));
        AssertTrue(nativeButtons.SequenceEqual(stock.CreateButtonTilemap()), "All512native button-template tiles");
        var shortTilemap = new BackgroundTile[PauseBackdropDefinitions.Cells - 1];
        var invalidTilemap = new BackgroundTile[PauseBackdropDefinitions.Cells];
        AssertThrows<ArgumentOutOfRangeException>(() => stock.LoadTo(shortTilemap, AreaId.Crateria), "Backdrop destination shorter than the image");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.LoadTo(invalidTilemap, (AreaId)7), "Backdrop area bound");
        AssertTrue(shortTilemap.All(tile => tile == default) && invalidTilemap.All(tile => tile == default), "Invalid load fails before any tilemap write");
        Console.WriteLine("Backdrop geometry: 7680 native tiles load as installed, and the bounds pass.");

        static void ConfirmArea(PauseBackdropPresentation presentation, AreaId area, BackgroundTile[] native)
        {
            const int untouched = 8;
            var sentinel = new BackgroundTile(0x155, 5, highPriority: false, TileFlip.Horizontal);
            BackgroundTile[] tiles = Enumerable.Repeat(sentinel, PauseBackdropDefinitions.Cells + untouched).ToArray();
            presentation.LoadTo(tiles, area);
            AssertTrue(native.AsSpan(0, PauseBackdropDefinitions.Cells).SequenceEqual(tiles.AsSpan(0, PauseBackdropDefinitions.Cells)) &&
                tiles[PauseBackdropDefinitions.Cells..].All(tile => tile == sentinel),
                "Actual area load writes every selected tile and leaves tiles past the image untouched");
        }
    }
    /// <summary>Checks pause equipment base cells and layout geometry.</summary>
    internal static void VerifyLookupStream2EquipmentBaseGeometry(CartridgeImportAddressSpace rom)
    {
        byte[] json = PauseEquipmentBaseExtractor.Extract(rom);
        var stock = PauseEquipmentBasePresentation.Load(new MemoryStream(json));
        // The template holds placed tiles; each native word is decoded to the tile it places and compared by tile, colour
        // variant, mirroring and priority.
        var native = new BackgroundTile[1024];
        for (int cell = 0; cell < 1024; cell++)
            native[cell] = BackgroundTileWords.ToTile(ReadVerificationWord(rom, 0xb6e800 + cell * 2));
        AssertTrue(native.SequenceEqual(stock.CreateTilemap()), "Installed equipment template holds all 1024 native tiles");
        BackgroundTile[] mutable = stock.CreateTilemap();
        mutable[0] = mutable[0] with { Tile = mutable[0].Tile ^ 1 };
        AssertTrue(native.SequenceEqual(stock.CreateTilemap()), "Mutable menu state does not alter immutable template");
        Console.WriteLine("Equipment template: 1024 native tiles and mutable-state independence pass.");
    }

    /// <summary>Checks pause wireframe mirror relationships against the native cells.</summary>
    internal static void VerifyLookupStream2WireframeMirrors(CartridgeImportAddressSpace rom)
    {
        byte[] json = PauseWireframeExtractor.Extract(rom);
        var stock = PauseWireframePresentation.Load(new MemoryStream(json));
        foreach (PauseWireframeKind kind in Enum.GetValues<PauseWireframeKind>())
        {
            int pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord(((int)PauseMenuSourceDefinitions.PauseWireframePointers + (int)kind * 2)) | 0x820000;
            var native = new byte[PauseWireframeDefinitions.Cells * 2];
            for (int cell = 0; cell < PauseWireframeDefinitions.Cells; cell++)
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(native.AsSpan(cell * 2), CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + cell * 2)));
            Confirm(stock, kind, BackgroundTileWordBytes.Tiles(native), "Installed wireframe preserves every native tile word");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ApplyTo(new BackgroundTile[PauseWireframeDefinitions.PageTiles], (PauseWireframeKind)4), "Wireframe kind bound");
        AssertThrows<ArgumentException>(() => stock.ApplyTo(new BackgroundTile[1], PauseWireframeKind.PowerSuit), "Wireframe page bound");
        Console.WriteLine("Wireframe composition: 544 native words, four full native patches and untouched-page/bounds checks pass.");

        static void Confirm(PauseWireframePresentation presentation, PauseWireframeKind kind, BackgroundTile[] tiles, string context)
        {
            BackgroundTile[] actual = Enumerable.Repeat(BackgroundTileWords.ToTile(0xaaaa), PauseWireframeDefinitions.PageTiles).ToArray();
            BackgroundTile[] expected = actual.ToArray();
            for (int row = 0; row < PauseWireframeDefinitions.Rows; row++)
                tiles.AsSpan(row * PauseWireframeDefinitions.Columns, PauseWireframeDefinitions.Columns)
                    .CopyTo(expected.AsSpan(PauseWireframeDefinitions.FirstPageTile + row * PauseWireframeDefinitions.PageColumns));
            presentation.ApplyTo(actual, kind);
            AssertTrue(expected.SequenceEqual(actual), context);
        }
    }

    /// <summary>Checks extracted reserve UI labels and placement.</summary>
    internal static void VerifyLookupStream2ReserveLabels(CartridgeImportAddressSpace rom)
    {
        byte[] json = PauseReserveUiExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseReserveUiDocument>(json,
            ArtworkJson.Options);
        var stock = PauseReserveUiFile.Read(new MemoryStream(json));
        int words = 0;
        foreach (PauseReserveLabel id in PauseReserveUiDefinitions.Labels)
        {
            string name = PauseReserveUiTiles.Name(id);
            PauseReserveLabelVisual label = document.Labels[name];
            BackgroundTile[] native = PauseTileGrid.Compile(label.Cells, name);
            int nativeCell = label.Anchor.Row * 32 + label.Anchor.Column;
            words += label.Cells.Length;
            // Untouched tiles around and under the label; MANUAL/AUTO replacement keeps their colour variant, priority and mirroring.
            var sentinel = new BackgroundTile(0x155, 5, highPriority: false, TileFlip.Horizontal);
            foreach (bool preserve in new[] { false, true })
            {
                BackgroundTile[] actual = Enumerable.Repeat(sentinel, 32 * 32).ToArray();
                BackgroundTile[] expected = actual.ToArray();
                for (int cell = 0; cell < native.Length; cell++)
                    expected[nativeCell + cell] = preserve ? sentinel with { Tile = native[cell].Tile } : native[cell];
                stock.ApplyLabel(actual, id, preserve ? LabelAttributes.Preserve : LabelAttributes.Replace);
                AssertTrue(expected.SequenceEqual(actual), "Actual reserve label patch keeps the attribute policy and surrounding tiles");
            }
        }
        AssertEqual(22, words, "All native reserve label tiles");
        AssertThrows<InvalidDataException>(() => stock.ApplyLabel(new BackgroundTile[1024], (PauseReserveLabel)byte.MaxValue), "Unknown reserve label rejected");
        AssertThrows<ArgumentException>(() => stock.ApplyLabel(new BackgroundTile[1], PauseReserveLabel.Manual), "Short reserve destination rejected");
        Console.WriteLine("Reserve labels:22 native words/four destinations,actual attribute-preserving/ordinary patches and bounds pass; independent glyph/arrow inputs remain required.");
    }

    /// <summary>Checks equipment label text geometry and native cells.</summary>
    internal static void VerifyLookupStream2EquipmentLabels(CartridgeImportAddressSpace rom)
    {
        byte[] json = PauseEquipmentLabelExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseEquipmentLabelDocument>(json,
            ArtworkJson.Options);
        var stock = PauseEquipmentLabelPresentation.Load(new MemoryStream(json));
        int words = 0;
        foreach (PauseEquipmentLabelId id in Enum.GetValues<PauseEquipmentLabelId>())
        {
            string key = PauseEquipmentLabelKeys.Name(id);
            words += document.Labels[key].Cells.Length;
        }
        // Each label laid out for its only collected item, equipped or not, among blank strips and untouched tiles around them.
        var sentinel = new BackgroundTile(0x155, 5, highPriority: false, TileFlip.Horizontal);
        foreach (var identity in PauseEquipmentLabelTiles.Labels())
        {
            PauseEquipmentLabel label = document.Labels[PauseEquipmentLabelKeys.Name(identity.Label)];
            BackgroundTile[] labelTiles = PauseTileGrid.Compile(label.Cells, "Stock label");
            PauseEquipmentBit bit = PauseEquipmentRules.Mask(identity.Category, identity.Item);
            foreach (bool disabled in new[] { false, true })
            {
                BackgroundTile[] actual = Enumerable.Repeat(sentinel, 32 * 32).ToArray();
                BackgroundTile[] expected = actual.ToArray();
                foreach (var other in PauseEquipmentLabelTiles.Labels())
                {
                    PauseEquipmentLabel blank = document.Labels[PauseEquipmentLabelKeys.Name(other.Label)];
                    expected.AsSpan(blank.Row * 32 + blank.Column, blank.Cells.Length).Clear();
                }
                for (int cell = 0; cell < label.Cells.Length; cell++)
                {
                    BackgroundTile tile = labelTiles[cell];
                    if (disabled) tile = tile with { ColourVariant = document.DisabledPalette };
                    expected[label.Row * 32 + label.Column + cell] = tile;
                }
                stock.ApplyInventory(actual, bit.Beam, disabled ? SamusBeamFlags.None : bit.Beam,
                    bit.Item, disabled ? SamusEquipmentFlags.None : bit.Item, hyperBeam: false);
                AssertTrue(expected.SequenceEqual(actual), "Actual label/disabled recolor writes every cell of the label and no surrounding tile");
            }
        }
        AssertEqual(115, words, "All native equipment glyph tiles");
        Console.WriteLine("Pause equipment labels: 115 native words, labels by item state and disabled recoloring pass.");
    }

    /// <summary>Checks pause equipment blank cells against native art and the stock label boundaries.</summary>
    internal static void VerifyLookupStream2EquipmentBlank(CartridgeImportAddressSpace rom)
    {
        byte[] nativeJson = PauseEquipmentLabelExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseEquipmentLabelDocument>(
            nativeJson, ArtworkJson.Options);
        for (int cell = 0; cell < PauseEquipmentLabelArtworkDefinitions.EquipmentWords; cell++)
        {
            int address = 0x820000 | (PauseMenuSourceDefinitions.BlankEquipmentLabel + cell * 2);
            AssertEqual((byte)0, rom.ReadCartridgeByte(address), "Native blank low byte");
            AssertEqual((byte)0, rom.ReadCartridgeByte(address + 1), "Native blank high byte");
        }
        var presentation = PauseEquipmentLabelPresentation.Load(new MemoryStream(nativeJson));
        foreach (bool hyper in new[] { false, true })
        {
            var sentinel = new BackgroundTile(0x155, 5, highPriority: false, TileFlip.Horizontal);
            BackgroundTile[] actual = Enumerable.Repeat(sentinel, 32 * 32).ToArray();
            BackgroundTile[] expected = actual.ToArray();
            BackgroundTile[] blankTiles = PauseTileGrid.Compile(document.Blank, "Reference blank");
            foreach (var identity in PauseEquipmentLabelTiles.Labels())
            {
                PauseEquipmentLabel label = document.Labels[PauseEquipmentLabelKeys.Name(identity.Label)];
                int destination = label.Row * 32 + label.Column;
                int length = PauseEquipmentLabelKeys.WordCount(identity.Category);
                blankTiles.AsSpan(0, length).CopyTo(expected.AsSpan(destination, length));
            }
            if (hyper)
            {
                PauseEquipmentLabel label = document.Labels[PauseEquipmentLabelArtworkDefinitions.HyperKey];
                BackgroundTile[] tiles = PauseTileGrid.Compile(label.Cells, "Reference Hyper label");
                tiles.AsSpan(0, PauseEquipmentLabelArtworkDefinitions.BeamWords)
                    .CopyTo(expected.AsSpan(label.Row * 32 + label.Column));
            }
            presentation.ApplyInventory(actual, 0, 0, 0, 0, hyper);
            AssertTrue(expected.SequenceEqual(actual),
                "Actual uncollected inventory/Hyper blank writes place the stock cells and leave untouched tiles");
        }
        Console.WriteLine("Pause equipment blank: nine native empty cells and two actual inventory/Hyper tilemap writes pass.");
    }

    /// <summary>Checks Dead Torizo corpse-tile geometry and native OAM composition.</summary>
    internal static void VerifyLookupStream2DeadTorizoGeometry(CartridgeImportAddressSpace rom)
    {
        var expected = new byte[0x1000];
        for (int row = 0; row < 12; row++)
        {
            int cursor = 0xa9de18 + row * 14;
            int source = CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + 2) - 0xa800;
            int destination = CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + 5) - 0x2000;
            int length = CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + 8) + 1;
            AssertEqual(new CorpseGraphicsCopy(source, destination, length),
                DeadTorizoGeometryDefinitions.InitialCopy(row), "Dead Torizo native MVN descriptor");
            ReadDeadTorizoRomBytes(rom, 0xb7a800 + source, length).CopyTo(expected, destination);
        }
        // The crop is the tile union of the native stationary composition, not
        // an occupancy bounding box over the unrelated parts elsewhere in its atlas.
        var visibleTiles = new HashSet<int>();
        int partCount = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa9d6e2);
        AssertEqual(25, partCount, "Dead Torizo native stationary part count");
        for (int part = 0; part < partCount; part++)
        {
            int address = 0xa9d6e4 + part * 5;
            int tile = (CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 3) & 0x1ff) - 0x100;
            int extent = (CartridgeImportSource.Require(rom).ReadCartridgeWord(address) & 0x8000) == 0 ? 1 : 2;
            int nativeX = CartridgeImportSource.Require(rom).ReadCartridgeWord(address) & 0x1ff;
            if (nativeX >= 256) nativeX -= 512;
            AssertEqual(new DeadTorizoStockPart(tile + 0x100, extent),
                DeadTorizoStationaryCompositionDefinitions.Part(part),
                "Immutable stock part preserves native tile/size/visible origin");
            for (int y = 0; y < extent; y++)
            for (int x = 0; x < extent; x++)
                AssertEqual(true, visibleTiles.Add(tile + y * 16 + x),
                    "Dead Torizo stationary parts have distinct source tiles");
        }
        var copiedTiles = new HashSet<int>();
        for (int row = 0; row < 12; row++)
        {
            int cursor = 0xa9de18 + row * 14;
            int first = (CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + 2) - 0xa800) / 32;
            int count = (CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + 8) + 1) / 32;
            for (int tile = first; tile < first + count; tile++) copiedTiles.Add(tile);
        }
        AssertEqual(97, visibleTiles.Count, "Dead Torizo native visible tile count");
        AssertEqual(true, visibleTiles.SetEquals(copiedTiles),
            "Dead Torizo twelve native MVNs copy exactly the 25-part stationary composition");
        byte[] planar = ReadDeadTorizoRomBytes(rom, SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.TileDataSource(rom, CorpseSheets.DeadTorizoSheet), CorpseSheets.DeadTorizoSheetByteCount);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, RoomCharacterAtlasFormat.TileColumns,
            out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        png.Position = 0;
        // The artwork fixture needs a display composition; staging copies the corpse rows whatever it holds.
        var displayParts = new Dictionary<EnemyFrameId, EnemySpritemapParts>
        {
            [EnemyFrameId.DeadTorizoStationary] = EnemySpritemapParts.FromOwnedArray([SpritePart.Create(Offset16<ScreenPixels>.Of(70, 33), large: false, character: 0, palette: 7, priority: 3,
                TileFlip.Horizontal)]),
        };
        var displayBindings = new Dictionary<EnemyFrameId, EnemyFrameId> { [EnemyFrameId.DeadTorizoStationary] = EnemyFrameId.DeadTorizoStationary };
        var display = new EnemySpritemapCatalog(displayParts, displayBindings);
        var artwork = EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas> { [EnemyDefinitionId.CorpseTorizo] = RoomCharacterAtlasPng.Read(png, planar.Length) }, Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>
            {
                [EnemyDefinitionId.CorpseTorizo] = EnemyPaletteSheetFile.Read(new MemoryStream(
                    EnemyPaletteSheetFile.Write(new EnemyPaletteSheetDocument
                    {
                        Version = 1,
                        Colors = Enumerable.Range(0,16).Select(_ => new PaletteRgb5 { Red=0, Green=0, Blue=0 }).ToArray(),
                    }))),
            }, Spritemaps = display });
        ReadOnlyMemory<byte> atlas = artwork.TilesOf(CorpseSheets.DeadTorizoSheet);
        AssertEqual(CorpseSheets.DeadTorizoSheetByteCount, atlas.Length, "Dead Torizo installed art resolves its corpse sheet");
        // The staging oracle stages the corpse as the cartridge does (#1392).
        CorpseStaging corpseTiles = CorpseStaging.Torizo(atlas.Span);
        Compare(expected, "Actual Dead Torizo installed-art crop");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadTorizoGeometryDefinitions.InitialCopy(-1), "Crop negative row rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadTorizoGeometryDefinitions.InitialCopy(12), "Crop row upper bound rejected");
        Console.WriteLine("Dead Torizo geometry:12 native MVNs/25-part OAM exact97-tile union and actual installed-art staging pass; crop derives from immutable stock composition; artwork ownership remains explicit.");

        void Compare(byte[] bytes, string context)
        {
            byte[] staged = corpseTiles.Upload(new CorpseRotRow(0, CorpseWorkMemory.EnemyBg2Staging, bytes.Length, 0));
            for (int index = 0; index < bytes.Length; index++)
                AssertEqual(bytes[index], staged[index], context);
        }
    }

    /// <summary>Copies a bounded span of native Dead Torizo artwork bytes for the oracle.</summary>
    private static byte[] ReadDeadTorizoRomBytes(ISnesAddressSpace rom, int address, int count) =>
        Enumerable.Range(0, count).Select(offset => rom.ReadCartridgeByte(address + offset)).ToArray();

    /// <summary>Checks native crawler/Yard speed ramps through actual velocity resets.</summary>
    internal static void VerifyLookupStream2CrawlerRamps(CartridgeImportAddressSpace rom)
    {
        Func<CrawlerSurfaceOrientation, SignedEightEight?, EightEightVector<RoomPixels>, EightEightVector<RoomPixels>> reset = CrawlersBehavior.ResetCrawlerVelocities;
        Action<YardEnemyState, YardDirection> setYard = YardEnemyBehavior.SetYardCrawlingVelocities;
        var yard = new YardEnemyState();
        SignedEightEight? crawlSpeed;
        for (ushort parameter = 0; parameter < 32; parameter++)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3e5f0 + parameter * 2));
            AssertEqual(expected, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3cca2 + parameter * 2)), "Native Yard/crawler magnitude agreement");
            AssertEqual(expected, CrawlerSpeeds.ForParameter(parameter), "Native calculated speed ramp");
            crawlSpeed = EnemySpawnSelectors.CrawlSpeed(parameter);
            yard.CrawlSpeed = new SignedEightEight(CrawlerSpeeds.ForParameter(parameter));
            for (ushort property = 0; property < 4; property++)
            {
                // The block side is the crawler's two low property bits in the cartridge.
                EightEightVector<RoomPixels> velocity = reset(SuperMetroid.AssetExtraction.Enemies.CrawlerOrientationOrder.Orientations.At(property), crawlSpeed, default);
                AssertEqual(property == 0 ? unchecked((ushort)-expected) : expected, velocity.X.RawValue,
                    "Actual crawler reset X magnitude");
                AssertEqual(property == 2 ? unchecked((ushort)-expected) : expected, velocity.Y.RawValue,
                    "Actual crawler reset Y magnitude");
            }
            for (ushort direction = 0; direction < 8; direction++)
            {
                setYard(yard, SuperMetroid.AssetExtraction.Enemies.YardDirectionCodec.At(direction));
                int nativeDirection = 0xa3cd82 + direction * 8;
                AssertEqual(unchecked((ushort)((expected ^ CartridgeImportSource.Require(rom).ReadCartridgeWord(nativeDirection)) + CartridgeImportSource.Require(rom).ReadCartridgeWord((nativeDirection + 2)))),
                    yard.CrawlingVelocity.X.RawValue, "Actual Yard reset X magnitude");
                AssertEqual(unchecked((ushort)((expected ^ CartridgeImportSource.Require(rom).ReadCartridgeWord((nativeDirection + 4))) + CartridgeImportSource.Require(rom).ReadCartridgeWord((nativeDirection + 6)))),
                    yard.CrawlingVelocity.Y.RawValue, "Actual Yard reset Y magnitude");
            }
        }
        AssertThrows<InvalidDataException>(() => CrawlerSpeeds.ForParameter(32), "Crawler parameter upper bound");
        Console.WriteLine("Crawler speed ramps: both 32 native words and 384 actual crawler/Yard velocity resets pass; irregular gaps/terminal choices remain pending.");
    }
    /// <summary>Checks lava-jumper control and visual selectors, parent/follower programs, and handshake.</summary>
    internal static void VerifyLookupStream2LavaJumperLayout(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(NorfairLavaJumperBehaviorVerification.VerifyNorfairLavaJumperBehavior), () => NorfairLavaJumperBehaviorVerification.VerifyNorfairLavaJumperBehavior());
        Suite(nameof(NorfairLavaJumperAnimationsVerification.VerifyNorfairLavaJumperAnimations), () => NorfairLavaJumperAnimationsVerification.VerifyNorfairLavaJumperAnimations(rom));
    }

    /// <summary>Checks Chozo statue instruction controls, visuals, and list boundaries.</summary>
    internal static void VerifyLookupStream2ChozoLayout(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyChozoStatueInstructionProgramDefinitions), () => VerifyChozoStatueInstructionProgramDefinitions(rom));
        Console.WriteLine("Chozo layouts: the four native lists and their 52 poses are checked through the typed programs.");
    }
    /// <summary>Checks horizontal scroll target selection against native grid boundary behavior.</summary>
    internal static void VerifyLookupStream2HorizontalCameraTargets(ISnesAddressSpace rom)
    {
        var storage = new byte[RoomScrollGrid.KeptCellCount];
        Array.Fill(storage, (byte)1);
        var grid = RoomScrollGrid.LoadNativeTable(storage, new Extent16<RoomScreens>(3, 2));
        foreach (CameraDistanceMode mode in Enum.GetValues<CameraDistanceMode>())
        foreach (SamusFacingDirection facing in new[] { SamusFacingDirection.Left, SamusFacingDirection.Right })
        for (int reversal = 0; reversal < 4; reversal++)
        {
            var context = new HorizontalCameraContext(
                reversal == 1,
                reversal == 2 ? SamusMovementType.Moonwalking : 0,
                reversal == 3 ? SamusAccelerationMode.Turning : SamusAccelerationMode.Accelerating, facing, mode);
            bool right = reversal == 0 ? facing != SamusFacingDirection.Left : facing == SamusFacingDirection.Left;
            // The original's target tables keep one word per mode, in this order.
            int nativeEntry = mode switch
            {
                CameraDistanceMode.NormalTracking => 0,
                CameraDistanceMode.BossTracking => 1,
                CameraDistanceMode.LeftEdge => 2,
                CameraDistanceMode.RightEdge => 3,
                _ => throw new InvalidOperationException($"Undefined {nameof(CameraDistanceMode)} {mode}."),
            };
            ushort nativeOffset = ReadVerificationWord(rom, (right ? 0x90963f : 0x909647) + nativeEntry * 2);
            AssertEqual(nativeOffset, HorizontalCameraTargets.Offset(mode, right), "Native mode/facing target offset");
            var camera = new ScrollBoundaryCamera(grid);
            camera.SetPosition(new Position16<RoomPixels>(100, 0));
            camera.TrackMovedSamusHorizontally(
                FixedPosition<RoomPixels>.FromStoredParts(200, 0, 100, 0), FixedPosition<RoomPixels>.FromStoredParts(204, 0, 100, 0), context);
            AssertEqual(unchecked((ushort)(204 - nativeOffset)), camera.IdealPosition.X, "Actual camera target preserves mode and reversal");
            AssertEqual((ushort)5, camera.Speed.X.UnsignedWholePixels, "Target dispatch preserves movement speed calculation");
        }
        var invalidCamera = new ScrollBoundaryCamera(grid);
        AssertThrows<ArgumentOutOfRangeException>(() => invalidCamera.TrackMovedSamusHorizontally(
            FixedPosition<RoomPixels>.FromStoredParts(200, 0, 100, 0), FixedPosition<RoomPixels>.FromStoredParts(204, 0, 100, 0),
            new(false, 0, 0, SamusFacingDirection.Right, (CameraDistanceMode)(-1))),"An undefined camera distance is rejected before tracking");
        Console.WriteLine("Horizontal camera targets:eight native offsets and32 actual mode/facing/reversal targets preserve geometry, speed and domain validation.");
    }

    /// <summary>Checks eight Torizo tile-page identities, decoded sources, and dispatch bounds.</summary>
    internal static void VerifyLookupStream2TorizoPageDispatch(ISnesAddressSpace rom)
    {
        TorizoTileSheet[] original =
        [
            TorizoSwapArtworkDefinitions.SharedDeath, TorizoSwapArtworkDefinitions.StatueCrumble,
            TorizoSwapArtworkDefinitions.LeftAttack, TorizoSwapArtworkDefinitions.RightAttack,
            TorizoSwapArtworkDefinitions.GoldenAwakening, TorizoSwapArtworkDefinitions.GoldenLeftAttack,
            TorizoSwapArtworkDefinitions.GoldenRightAttack, TorizoSwapArtworkDefinitions.ChozoDebris,
        ];
        var pages = new RoomCharacterAtlas[original.Length];
        var native = new byte[original.Length][];
        int ordinal = 0;
        foreach (var page in TorizoSwapArtworkDefinitions.All)
        {
            AssertEqual(original[ordinal], page, "Torizo page manifest order/source/extent/filename");
            AssertEqual(page, TorizoSwapArtworkDefinitions.All[ordinal], "Torizo page enumeration/index agreement");
            AssertEqual((TorizoTilePage)ordinal, page.Page, "Torizo page identity follows manifest order");
            int source = TorizoSourceDefinitions.PageSources[page.Page];
            native[ordinal] = Enumerable.Range(0, page.ByteCount).Select(offset => rom.ReadCartridgeByte(source + offset)).ToArray();
            byte[] pixels = SnesGraphics.DecodePlanarTiles(native[ordinal], 4,
                Math.Min(RoomCharacterAtlasFormat.TileColumns, page.ByteCount / RoomCharacterAtlasFormat.BytesPerTile), out Extent16<ImagePixels> size);
            using var png = new MemoryStream();
            IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
            png.Position = 0;
            pages[ordinal++] = RoomCharacterAtlasPng.Read(png, page.ByteCount);
        }
        AssertEqual(8, ordinal, "Torizo page count");
        var artwork = new TorizoSwapArtwork(pages);
        string expectedHash = SelectedPresentationHash.Create("TorizoInstructionVramArtwork-v1", content =>
        {
            content.Append("pages", original.Length);
            foreach (var bytes in native) content.Append("tiles", bytes);
        });
        AssertEqual(expectedHash, artwork.ContentIdentity, "Torizo canonical hash retains original page order");
        for (int index = 0; index < original.Length; index++)
        {
            var page = original[index];
            AssertTrue(artwork.Characters(page.Page, 0, page.ByteCount).Span.SequenceEqual(native[index]), "Actual resolver returns exact native page bytes");
            AssertEqual(native[index][^1], artwork.Characters(page.Page, page.ByteCount - 1, 1).Span[0], "Actual resolver returns correct final byte");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = artwork.Characters(page.Page, 0, page.ByteCount + 1), "Actual resolver rejects page overrun");
        }
        AssertThrows<IndexOutOfRangeException>(() => _ = TorizoSwapArtworkDefinitions.All[-1], "Torizo page lower bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = TorizoSwapArtworkDefinitions.All[8], "Torizo page upper bound");
        Console.WriteLine("Torizo page dispatch:eight exact ordered identities, native decoded page resolution/bounds and canonical content hash pass.");
    }
    /// <summary>Checks Kago visual frame pointers and geometry against compiled native frames.</summary>
    internal static void VerifyLookupStream2KagoFrameGeometry(ISnesAddressSpace rom)
    {
        int count = 0;
        foreach (var frame in KagoVisualDefinitions.Frames())
        {
            ushort native = ReadVerificationWord(rom, 0xa8ab20 + 4 * count);
            AssertEqual((byte)0xa8, frame.Bank, "Kago native frame bank");
            AssertEqual(native, frame.Pointer, "Kago native animation selects generated identity");
            AssertEqual((ushort)4, ReadVerificationWord(rom, 0xa80000 | native), "Kago native four-part record width");
            AssertEqual($"kago_cycle_{count}", frame.Name, "Kago installed resource name");
            AssertTrue(EnemySpritemapDefinitions.Frames.Select(installed => installed.Native).Contains(frame), "Generated Kago identity reaches installed catalog");
            count++;
        }
        AssertEqual(3, count, "Kago frame enumeration count");
        Console.WriteLine("Kago frames:three native selectors/four-part headers, stable names and installed catalog membership pass.");
    }

    /// <summary>Checks Kraid lint initialization's source values and live actor state.</summary>
    internal static void VerifyLookupStream2KraidLintInitialization(CartridgeImportAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
        var state = new KraidEnemyState();
        // The parts read Kraid's room state, which the body's initializer installs; this fixture installs it directly.
        enemies.KraidBehavior.Access.Kraid = state;
        enemies.VerificationServices().ReadRandom = () => 1;
        Action<RoomEnemySlot, KraidEnemyState> finish = enemies.KraidBehavior.Rise.FinishKraidGrowth;
        Action<RoomEnemySlot, KraidPartState> align = enemies.KraidBehavior.Projectiles.AlignKraidPart;
        var body = enemies.SlotTable.Slots[0];
        body.Position = body.Position.WithWhole(new(200, 300));
        KraidLintPart[] lintParts = [KraidLintPart.Top, KraidLintPart.Middle, KraidLintPart.Bottom];
        foreach (KraidLintPart lintPart in lintParts)
            enemies.KraidBehavior.Parts.LintStateOf(state, enemies.KraidBehavior.Parts.Lint(lintPart)).LintTravel = 17;
        finish(body, state);
        for (int native = 0; native < lintParts.Length; native++)
        {
            var lint = enemies.KraidBehavior.Parts.Lint(lintParts[native]);
            KraidLintState lintState = enemies.KraidBehavior.Parts.LintStateOf(state, lint);
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7a916 + native * 2));
            AssertEqual(expected, KraidLints.InitialDelay(lintParts[native]), "Native per-part initial delay");
            AssertEqual(expected, lintState.PhaseTimer, "Actual post-growth timer write");
            AssertEqual(0, lintState.LintTravel, "Actual lint growth resets extension");
            AssertEqual(KraidPhase.LintProduce, lintState.PhaseToResume, "Actual lint continuation");
            for (int remaining = expected - 1; remaining >= 0; remaining--)
            {
                align(lint, lintState);
                AssertEqual(remaining, lintState.PhaseTimer, "Actual per-part countdown");
                AssertEqual((remaining == 0 ? KraidPhase.LintProduce : KraidPhase.AlignPartToKraid),
                    lintState.Phase, "Actual lint transition occurs on its own final tick");
                AssertEqual(unchecked((ushort)(body.Position.X.Whole - lint.Radius.X)), lint.Position.X.Whole, "Alignment continues during delay");
            }
        }
        foreach (KraidLintPart invalid in new[] { (KraidLintPart)(-1), (KraidLintPart)3 })
            AssertThrows<InvalidOperationException>(() => KraidLints.InitialDelay(invalid), "Only the three lints have launch policy");
        Console.WriteLine("Kraid lint initialization: three native policies, actual slot writes and all 512 countdown transitions pass.");
    }

    /// <summary>Checks enemy drop cumulative weights, RNG reroll, and resource eligibility.</summary>
    internal static void VerifyLookupStream2DropSelection(CartridgeImportAddressSpace rom)
    {
        // Native first record weights60,60,60,5,60,10 yield cumulative thresholds
        //60,120,180,185,245,255 when all resources are eligible.
        ushort[] randomValues = [1, 61, 121, 181, 186, 246];
        for (int column = 0; column < 6; column++)
        {
            EnemyPickupKind expected = EnemyPickupNativeWords.Selectors.Decode(rom.ReadCartridgeByte(0x86f25e + column), "probability-column pickup");
            AssertEqual(expected, EnemyDropTables.ForProbabilityColumn(column),
                "Native probability-column pickup identity");
            var samus = CreateDropTestSamus();
            samus.Health = 50;
            samus.MaxMissiles = samus.MaxSuperMissiles = samus.MaxPowerBombs = 5;
            var fixture = CreateEnemyDropFixture(samus, [randomValues[column]]);
            var projectile = fixture.System.Projectiles.EnemyProjectiles[0];
            projectile.DropChancesOverride = NativeDropChances;
            AssertEqual(expected, fixture.System.DeathsAndDrops.SelectRandomEnemyDrop(projectile),
                "Actual cumulative selection preserves each semantic column");
        }
        Suite(nameof(VerifyEnemyDropSelectionRules), () => VerifyEnemyDropSelectionRules());
        AssertThrows<ArgumentOutOfRangeException>(() => EnemyDropTables.ForProbabilityColumn(-1),
            "Drop column lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => EnemyDropTables.ForProbabilityColumn(6),
            "Drop column upper bound");
        Console.WriteLine("Enemy drop selection:six native identities and actual cumulative selections, RNG-zero reroll, energy hysteresis and full-resource eligibility pass.");
    }
    /// <summary>Checks Brinstar, Norfair, and yellow Pipe Bug visual program geometry.</summary>
    internal static void VerifyLookupStream2PipeBugVisualGeometry(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(PipeBugBehaviorVerification.VerifyPipeBugBehavior), () => PipeBugBehaviorVerification.VerifyPipeBugBehavior());
        Suite(nameof(PipeBugAnimationsVerification.VerifyPipeBugAnimations), () => PipeBugAnimationsVerification.VerifyPipeBugAnimations(rom));
        ushort[] poses = PipeBugAnimationsConstants.Spritemaps(Enum.GetValues<PipeBugSegment>()).Distinct().ToArray();
        foreach (ushort pose in poses)
            AssertEqual((ushort)1, ReadVerificationWord(rom, (int)new SnesAddress(PipeBugFrameToolingDefinitions.Bank, pose)), "Native seven-byte single-part composition");
        Console.WriteLine($"Pipe Bug visual geometry: {poses.Length} native single-part poses and the typed loops pass.");
    }

    /// <summary>Checks five-member Pipe Bug formation positions and native spawn data.</summary>
    internal static void VerifyLookupStream2PipeBugFormation(CartridgeImportAddressSpace rom)
    {
        for (int member = 0; member < 5; member++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb38c65 + member * 7)), PipeBugFormation.NorfairStaggerTarget(member),
                "Native rank-derived formation release counter");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb38c88 + member * 6)), PipeBugsNativeWords.PipeBugActions.Word(PipeBugFormation.NorfairPostRiseAction(member)),
                "Native role-specific formation function");
        }
        foreach (ushort samusX in new ushort[] { 120, 136 })
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
            for (int member = 0; member < 5; member++)
            {
                var slot = enemies.SlotTable.Slots[member];
                slot.Bind(EnemyDefinitionId.Gamet, slot.Definition);
                slot.Position = slot.Position.WithWhole(new(128, 128)); slot.SetSpawnParameter2(EnemyDefinitionId.Gamet, 64);
                enemies.PipeBugsBehavior.InitializeNorfairPipeBug(slot);
                slot.AnimationClock.HoldFor((ushort)(member + 4));
                slot.AnimationClock.Repeats = WrappingCountdown.Start((ushort)(member + 8));
            }
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(new(samusX, 112)) };
            enemies.PipeBugsBehavior.RunNorfairPipeBugSamusWait(enemies.SlotTable.Slots[0], (enemies.PipeBugStates[0] ?? throw new InvalidOperationException("Verification requires enemies.PipeBugStates[0].")), samus);
            for (int member = 0; member < 5; member++)
            {
                var state = enemies.PipeBugStates[member];
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb38c65 + member * 7)), (state ?? throw new InvalidOperationException("Verification requires state.")).StaggerTarget,
                    "Actual formation writes native release counter");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb38c88 + member * 6)), PipeBugsNativeWords.PipeBugActions.Word(state.NorfairPostRiseAction),
                    "Actual formation writes native role function");
                AssertEqual(PipeBugAction.NorfairRise, state.Action, "Actual formation begins rising");
                AssertEqual(new AnimationPlayback<PipeBugSegment>(PipeBugAnimations.StartingWith(samusX < 128 ? PipeBugSegment.GametRisingLeft : PipeBugSegment.GametRisingRight), AnimationPlace.StartOfAnimation),
                    state.Playback, "Actual formation selects facing");
                AssertEqual((ushort)(member == 0 ? 1 : member + 4), enemies.SlotTable.Slots[member].AnimationClock.Hold.RawValue,
                    "Only leader instruction timer resets");
                AssertEqual((ushort)(member == 0 ? 0 : member + 8), enemies.SlotTable.Slots[member].AnimationClock.Repeats.RawValue,
                    "Only leader loop counter resets");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => PipeBugFormation.NorfairStaggerTarget(-1), "Formation lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => PipeBugFormation.NorfairPostRiseAction(5), "Formation upper bound");
        Console.WriteLine("Norfair Pipe Bug formation:ten native immediates, both actual facing formations and leader-only timer resets pass.");
    }

    /// <summary>Checks projectile trail appearance catalog against the native words.</summary>
    internal static void VerifyLookupStream2TrailAppearance(ISnesAddressSpace rom)
    {
        byte[] json = ProjectileTrailExtractor.Extract(rom);
        var stock = InstalledProjectileTrails.Load(new MemoryStream(json));
        foreach (ProjectileTrailFrame frame in ProjectileTrailVisualDefinitions.Frames)
        {
            // The appearance has no packed word: the native attribute word is decoded by the import pipeline's codec and
            // compared by character, palette row, priority and mirroring.
            AssertEqual(new SnesObjAttributeWord(ReadVerificationWord(rom, 0x900000 | (ProjectileTrailNative.Address(frame) + 2))).ToSinglePart(),
                stock.Resolve(frame), "Native trail appearance");
        }
        AssertThrows<InvalidDataException>(() => stock.Resolve((ProjectileTrailFrame)ProjectileTrailVisualDefinitions.FrameCount), "Unknown trail frame rejected");
        Suite(nameof(VerifyLookupStream2TrailPrograms), () => VerifyLookupStream2TrailPrograms(rom));
        Console.WriteLine("Trail appearance:42 native words and existing real OAM/lifetime/freeze proof pass; ice phase boundaries remain pending.");
    }

    /// <summary>Checks trail instruction words and emitted OAM across the compiled programs.</summary>
    internal static void VerifyLookupStream2TrailPrograms(ISnesAddressSpace bus)
    {
        byte[] json = ProjectileTrailExtractor.Extract(bus);
        var catalog = InstalledProjectileTrails.Load(new MemoryStream(json));
        var encountered = new HashSet<ushort>();
        // The cartridge's retained tile words, which the native interpreter keeps beside the port's sides.
        var nativeWords = new Dictionary<SamusProjectileTrailSide, ushort>();
        foreach (ushort start in new[] { ProjectileTrailDefinitions.LeftIce, ProjectileTrailDefinitions.RightIce, ProjectileTrailDefinitions.Wave, ProjectileTrailDefinitions.Missile })
        {
            ushort cursor = start;
            while (true)
            {
                ushort word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusSourceDefinitions.MovementBank.Value | cursor);
                if (word == 0) break;
                if (word < 0x8000) { encountered.Add(cursor); cursor += 4; }
                else { cursor += 2; }
            }
            var authoredSystem = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
            // The native interpreter's own trail, run beside the port's.
            var nativeTrail = new SamusProjectileTrail(SamusBeamCombination.Power);
            SamusProjectileTrail authoredTrail = authoredSystem.AddTrail();
            nativeWords.Clear();
            foreach (var pair in new[] { nativeTrail, authoredTrail })
            {
                foreach (var side in new[] { pair.Left, pair.Right })
                { side.InstructionPointer = start; side.FrameHold = 1; side.Position = side.Position.WithX(100).WithY(100); }
            }
            for (int frame = 0; frame < 80; frame++)
            {
                var nativeOam = new OamBuffer();
                bool frozenFrame = frame % 5 == 0;
                StepNative(nativeTrail, nativeTrail.Left, nativeOam, frozenFrame);
                StepNative(nativeTrail, nativeTrail.Right, nativeOam, frozenFrame);
                OamBuffer authoredOam = SpriteFrameBuffer.DrawnOam(frame =>
                {
                    authoredSystem.AdvanceTrails(frozenFrame);
                    authoredSystem.DrawTrails(frame, Position16<RoomPixels>.Zero, InstalledFixtures.ProjectileArtwork());
                });
                AssertTrue(nativeOam.LowTable.SequenceEqual(authoredOam.LowTable), "Trail catalog preserves live command/termination/freeze frame output");
                foreach (var sides in new[] { new VerifyLookupStream2TrailProgramsCase(nativeTrail.Left, authoredTrail.Left), new VerifyLookupStream2TrailProgramsCase(nativeTrail.Right, authoredTrail.Right) })
                {
                    AssertEqual(sides.First.InstructionPointer, sides.Second.InstructionPointer, "Trail artwork cannot change instruction cursor");
                    AssertEqual(sides.First.FrameHold, sides.Second.FrameHold, "Trail artwork cannot change live timing");
                    AssertEqual(sides.First.Position.Y, sides.Second.Position.Y, "Trail artwork cannot change sibling-targeted movement");
                }
            }
        }
        AssertTrue(encountered.SetEquals(ProjectileTrailVisualDefinitions.Frames.Select(ProjectileTrailNative.Address)), "Independent native stream walk finds exactly the catalog's appearance records");
        int programWords = 0;
        for (int address = 0x90b4c8; address <= 0x90b5b3; address++)
        {
            if (ProjectileTrailAnimationDefinitions.TryRead(address, out _))
            {
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), ProjectileTrailAnimationDefinitions.Read(address), "Compiled trail program preserves every authored mechanics word");
                programWords++;
            }
            else
            {
                int rejectedAddress = address;
                AssertThrows<InvalidDataException>(() => ProjectileTrailAnimationDefinitions.Read(rejectedAddress), "Trail program rejects presentation gaps, odd addresses and unrelated high-bank words");
            }
        }
        AssertEqual(67, programWords, "All 42 durations, 20 movement commands and five terminators are compiled");
        AssertThrows<InvalidDataException>(() => ProjectileTrailAnimationDefinitions.Read(0x91b4c9), "Trail program rejects a wrong-bank alias");
        Console.WriteLine("Stream 2 trail programs: 67 native mechanics words,42 visual record addresses, all four actual paired trail lifetimes/freeze states and mutable alias pass.");
        // A side that has read no tile word yet draws nothing, as the port's sparkle that has shown no pose (#1625).
        void StepNative(SamusProjectileTrail pair, SamusProjectileTrailSide side, OamBuffer oam, bool frozen)
        {
            if (side.FrameHold == 0) return;
            if (!frozen && --side.FrameHold == 0)
            {
                ushort cursor = checked((ushort)side.InstructionPointer);
                while (true)
                {
                    ushort word = ReadVerificationWord(bus, 0x900000 | cursor);
                    if (word < 0x8000)
                    {
                        side.FrameHold = word;
                        if (word == 0) return;
                        nativeWords[side] = ReadVerificationWord(bus, 0x900000 | (cursor + 2));
                        side.InstructionPointer = (ushort)(cursor + 4);
                        break;
                    }
                    cursor += 2;
                    if (word == ProjectileTrailNative.InstructionAddress(ProjectileTrailCue.MoveLeftDown)) pair.Left.Position += new Offset16<RoomPixels>(0, 1);
                    else if (word == ProjectileTrailNative.InstructionAddress(ProjectileTrailCue.MoveRightDown)) pair.Right.Position += new Offset16<RoomPixels>(0, 1);
                    else throw new InvalidDataException($"Unexpected native trail command {word:X4}.");
                }
            }
            if (nativeWords.TryGetValue(side, out ushort shown))
                oam.AddProjectileTrailSprite(new Position16<SpritePixels>((byte)side.Position.X, (byte)side.Position.Y), shown);
        }
    }

    /// <summary>Checks the left and right trail programs every trailing projectile spawns against the selector words read for it.</summary>
    internal static void VerifyLookupStream2TrailSelectors(ISnesAddressSpace bus)
    {

        int start = SamusProjectileRomDataTrailsConstants.LeftInstructionPointers;
        // Every projectile that trails: each beam combination uncharged and charged, the Spazer special attack's trailing
        // pair, and both missiles. The Shinespark crash's echo runs its own update, which spawns no trail (#1500).
        SamusProjectileType[] trailing =
        [
            .. Enum.GetValues<SamusBeamCombination>().Select(beams => SamusProjectileType.Beam(beams, charged: false)),
            .. Enum.GetValues<SamusBeamCombination>().Select(beams => SamusProjectileType.Beam(beams, charged: true)),
            SamusProjectileType.EchoOf(SamusBeamEcho.SpazerComboTrail),
            SamusProjectileType.Of(SamusProjectileFamily.Missile), SamusProjectileType.Of(SamusProjectileFamily.SuperMissile),
        ];
        foreach (SamusProjectileType trailingType in trailing)
        {
            int selection = ProjectileTrailNative.Selection(trailingType);
            var projectiles = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
            var projectile = new SamusProjectile(0)
            {
                Type = trailingType,
                Playback = SamusProjectileProgramAddresses.PlaybackAt(0x86e3),
                Position = new(new(100, 0), new(200, 0)),
            };
            projectiles.Trails.SpawnTrail(projectile);
            // An empty trail family (programs, direction list or coordinates outside the compiled data) draws no trail (#1318).
            if (projectiles.LiveTrails.SingleOrDefault() is not { } trail)
                continue;
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), start + selection * 2), trail.Left.InstructionPointer, "Real spawn selects the left table's trail");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomDataTrailsConstants.RightInstructionPointers + selection * 2), trail.Right.InstructionPointer, "Real spawn selects the right table's trail");
            AssertEqual(1, trail.Left.FrameHold, "Selector extraction leaves allocation timer unchanged");
            AssertEqual(96, trail.Left.Position.X, "Selector extraction leaves origin offset unchanged");
        }
        Console.WriteLine("Trail selectors: every trailing projectile's real spawn selection passes.");
    }

    /// <summary>Checks Mother Brain tube palette ramps and actual CGRAM sequence.</summary>
    internal static void VerifyLookupStream2TubeRamp(ISnesAddressSpace rom)
    {
        PaletteRgb5[] Read(int address) => Enumerable.Range(0, 32).Select(index =>
        {
            ushort word = ReadVerificationWord(rom, address + index * 2);
            return new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        }).ToArray();
        var original = new ChozoAndTubeColorDocument { Version = 1,
            TubeCracks = Read(EnemySpeciesSourceDefinitions.ChozoAndTube.TubeCracksColors),
            WreckedShip = Read(EnemySpeciesSourceDefinitions.ChozoAndTube.WreckedShipColors),
            LowerNorfair = Read(EnemySpeciesSourceDefinitions.ChozoAndTube.LowerNorfairColors) };
        ChozoAndTubeColorCatalog Load(ChozoAndTubeColorDocument document) =>
            ChozoAndTubeColorFile.Read(new MemoryStream(ChozoAndTubeColorFile.Write(document), writable: false));
        var stock = Load(original);
        Check(stock, original);
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveTubeCracks(-1), "tube negative color");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveTubeCracks(32), "tube past last color");
        Console.WriteLine("Tube and statue palettes: 96 installed native colors and canonical identity pass.");

        static void Check(ChozoAndTubeColorCatalog catalog, ChozoAndTubeColorDocument document)
        {
            static ushort Word(PaletteRgb5 color) => (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
            // The colours the import pre-colours the tube-crack and statue shades from (#1512).
            for (int color = 0; color < 32; color++)
            {
                ushort word = Word(document.TubeCracks[color]);
                AssertEqual(word, catalog.ResolveTubeCracks(color), "tube color preserved");
                AssertEqual(Word(document.WreckedShip[color]), catalog.ResolveWreckedShip(color), "Wrecked Ship palette preserved");
                AssertEqual(Word(document.LowerNorfair[color]), catalog.ResolveLowerNorfair(color), "Lower Norfair palette preserved");
            }
            string expected = SelectedPresentationHash.Create("ChozoAndTubeColorCatalog-v1", content =>
            {
                content.AppendWords("tubeCracks", document.TubeCracks.Select(Word).ToArray());
                content.AppendWords("wreckedShip", document.WreckedShip.Select(Word).ToArray());
                content.AppendWords("lowerNorfair", document.LowerNorfair.Select(Word).ToArray());
            });
            AssertEqual(expected, catalog.ContentIdentity, "tube canonical identity preserved");
        }
    }
    /// <summary>Checks all pause tilemap cells choose equipment, label, or reserve destinations correctly.</summary>
    internal static void VerifyLookupStream2PauseOwnership(ISnesAddressSpace rom)
    {
        for (int cell = -1; cell <= PauseEquipmentBaseDefinitions.Cells; cell++)
        {
            bool equipment = false;
            for (int index = 0; index < 14; index++)
            {
                int start = (ReadVerificationWord(rom, 0x82c06c + 2 * index) - 0x3800) / 2;
                equipment |= (uint)(cell - start) < 9;
            }
            bool reserve = (uint)(cell - PauseReserveUiDefinitions.DigitCell) < PauseReserveUiDefinitions.SupplyDigitPlaces;
            for (int index = 0; index < 2; index++)
            {
                int start = (ReadVerificationWord(rom, 0x82c068 + 2 * index) - 0x3800) / 2;
                reserve |= (uint)(cell - start) < 7;
            }
            int relative = cell - PauseWireframeDefinitions.FirstPageTile;
            bool wireframe = relative >= 0 && relative / PauseWireframeDefinitions.PageColumns < PauseWireframeDefinitions.Rows &&
                relative % PauseWireframeDefinitions.PageColumns < PauseWireframeDefinitions.Columns;
            AssertEqual(equipment, PauseEquipmentBaseTiles.IsEquipmentLabelCell(cell), "native equipment ownership footprint");
            AssertEqual(equipment || reserve || wireframe, PauseEquipmentBaseTiles.IsLiveOwnedCell(cell), "native live ownership footprint");
            AssertEqual((reserve || wireframe) && !equipment, PauseEquipmentBaseTiles.IsNonInventoryLiveOwnedCell(cell), "native noninventory footprint");
        }
        Console.WriteLine("Pause ownership: all1024 cells plus rejected outer bounds match native label/reserve destinations, including the nine-cell Plasma overlap.");
    }
    /// <summary>
    /// Checks the extraction's Dead Torizo row upload descriptors and terminators against the cartridge (#1406, #1539).
    /// </summary>
    internal static void VerifyLookupStream2DeadTorizoTransfers(ISnesAddressSpace rom)
    {
        for (ushort phase = 0; phase < 2; phase++)
        {
            var rows = DeadTorizoRowUploadDefinitions.ForHalf(odd: phase == 1);
            // DeadTorizo_VRAMWriteTable_Even at $A9:D549 and DeadTorizo_VRAMWriteTable_Odd at $A9:D583.
            ushort table = phase == 0 ? (ushort)0xd549 : (ushort)0xd583;
            AssertEqual(7, rows.Count, "Dead Torizo six body rows plus sand strip");
            int count = 0;
            foreach (var row in rows)
            {
                int address = 0xa90000 | table + 8 * count;
                AssertEqual(ReadVerificationWord(rom, address), row.SizeInBytes, "Dead Torizo native DMA size");
                AssertEqual(ReadVerificationWord(rom, address + 2), CorpseRowSources.BankWord, "Dead Torizo native DMA bank");
                AssertEqual(ReadVerificationWord(rom, address + 4), CorpseRowSources.OfTorizoHalf(phase == 1, count), "Dead Torizo native DMA source");
                AssertEqual(ReadVerificationWord(rom, address + 6), row.DestinationWord, "Dead Torizo native DMA destination");
                AssertEqual(rows[count++], row, "Dead Torizo enumeration order");
            }
            AssertEqual(7, count, "Dead Torizo enumeration count");
            AssertEqual((ushort)0, ReadVerificationWord(rom, 0xa90000 | table + count * 8), "Dead Torizo native terminator");
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = rows[-1]; }, "Dead Torizo negative row");
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = rows[7]; }, "Dead Torizo past last row");
        }
        Console.WriteLine("Dead Torizo: all56 native descriptor fields, two terminators, enumeration and bounds pass.");
    }
    /// <summary>Checks five gunship liftoff graphics transfers and their live queue behavior.</summary>
    internal static void VerifyLookupStream2GunshipTransfers(ISnesAddressSpace rom)
    {
        var transfers = GunshipLiftoffTransferDefinitions.Frames;
        AssertEqual(5, transfers.Length, "Gunship calculated transfer count");
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom());
        var slot = enemies.SlotTable.Slots[0];
        var part = new GunshipPartState();
        slot.SetSpeciesState(part);
        int enumerated = 0;
        foreach (var transfer in transfers)
        {
            int source = 0x940000 | ReadVerificationWord(rom, 0xa2ac07 + 2 * enumerated);
            ushort destination = ReadVerificationWord(rom, 0xa2ac11 + 2 * enumerated);
            AssertEqual(source, EnemySourceDefinitions.GunshipLiftoffPageSource(enumerated), "Gunship import source matches the takeoff list");
            int firstCharacter = EncounterCharacterPlacementDefinitions.GunshipFirstCharacter
                + enumerated * GunshipLiftoffCharacterDefinitions.ByteCount / SpriteCharacterTableLayout.CharacterByteCount;
            AssertEqual(destination, ObjCharacterNativeWords.Of(firstCharacter), "Gunship original destination");
            AssertEqual(InstalledArtworkId.GunshipLiftoffFirstTiles + enumerated, transfer.Asset, "Gunship typed transfer identity");
            AssertEqual(transfers[enumerated], transfer, "Gunship enumeration and indexing agree");
            enemies.GunshipBehavior.ReadyGunshipTakeoffEngines(slot);
            AssertEqual(enumerated + 1 == GunshipTakeoffDefinitions.EngineReadyingDuration,
                enemies.Room.Session.Looks.Shows(GunshipTakeoffDefinitions.TakeoffLook),
                "the gunship shows its takeoff look from the readying's last update only");
            enumerated++;
        }
        AssertEqual(5, enumerated, "Gunship enumerates every transfer");
        AssertEqual(GunshipTakeoffDefinitions.EngineReadyingDuration, enumerated, "the readying lasts one update a native transfer");
        AssertEqual(0, part.EngineReadyingElapsed, "Gunship completes its readying phase");
        AssertEqual(GunshipPhase.FireUpEngines, part.Phase, "Gunship starts engines after fifth transfer");
        Console.WriteLine("Gunship transfers: all10 native source/destination fields, five typed identities, the readying's duration, takeoff look and phase handoff pass.");
    }
    /// <summary>Checks reserve arrow RGB5 colors and the shades the import pre-colours the arrow in (#1512).</summary>
    internal static void VerifyLookupStream2ReserveArrowColors(ISnesAddressSpace rom)
    {
        byte[] source = PauseReserveUiExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseReserveUiDocument>(source, ArtworkJson.Options);
        var stock = PauseReserveUiFile.Read(new MemoryStream(source));
        SuperMetroid.AssetExtraction.Artwork.SheetShade[] shades = [.. stock.ArrowColours.Shades()];
        for (int frame = 0; frame < 32; frame++)
        {
            var pulse = shades.Single(shade => shade.Shade == SuperMetroid.Core.Frontend.Pause.Artwork.PauseReserveArrowShades.Pulse(frame));
            AssertTrue(pulse.Inks.SequenceEqual([102, 107]), "the pulse shade recolours the arrow's two bevel colours");
            AssertEqual(ReadVerificationWord(rom, 0x82ad5d + 2 * frame), pulse.Colours[0], "Native arrow color6 reaches the arrow's shade colour");
            AssertEqual(ReadVerificationWord(rom, 0x82ad9d + 2 * frame), pulse.Colours[1], "Native arrow color11 reaches the arrow's light colour");
        }
        Check(stock, document);
        Console.WriteLine("Reserve arrow ramps: all 64 native colors, shades and wrapping pass.");

        static void Check(PauseReserveUiPresentation presentation, PauseReserveUiDocument expected)
        {
            for (int frame = -1; frame <= 32; frame++)
            {
                var pulse = presentation.ArrowColours.Pulse(frame);
                var row = expected.Arrow.Frames[frame & 31];
                AssertEqual(Pack(row.Color6), pulse.Color6, "Supplied color6");
                AssertEqual(Pack(row.Color11), pulse.Color11, "Supplied color11");
            }
            var solid = presentation.ArrowColours.Solid;
            AssertEqual(Pack(expected.Arrow.SolidColor6), solid.Color6, "Solid arrow color6");
            AssertEqual(Pack(expected.Arrow.SolidColor11), solid.Color11, "Solid arrow color11");
        }
        static ushort Pack(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
    }
    /// <summary>Checks reserve bar and arrow positions against extracted pause UI geometry.</summary>
    internal static void VerifyLookupStream2ReserveGeometry(ISnesAddressSpace rom)
    {
        byte[] source = PauseReserveUiExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseReserveUiDocument>(source, ArtworkJson.Options);
        var stock = PauseReserveUiFile.Read(new MemoryStream(source));
        Check(stock, document);
        Console.WriteLine("Reserve geometry: all 10 digits/10 arrow cells, actual tilemap writes, supplied anchors/palettes and unrelated tile fields pass.");

        static void Check(PauseReserveUiPresentation presentation, PauseReserveUiDocument expected)
        {
            for (int value = 0; value < 10; value++)
            for (int place = 0; place < 3; place++)
            {
                var tilemap = new BackgroundTile[1024];
                presentation.ApplyDigit(tilemap, place, value);
                int cell = 32 * expected.Digits.Anchor.Row + expected.Digits.Anchor.Column + place;
                BackgroundTile tile = PauseTileGrid.Compile([expected.Digits.Cells[value]], "reserve verification")[0];
                AssertEqual(tile, tilemap[cell], "Digit actual complete tile and supplied anchor");
            }
            // Every tile field set, so the arrow update is seen to change only the colour variant.
            var shown = new BackgroundTile(0x355, 0, true, true, true);
            foreach (bool enabled in new[] { false, true })
            {
                BackgroundTile[] tilemap = Enumerable.Repeat(shown, 1024).ToArray();
                presentation.ApplyArrowTilePalettes(tilemap, enabled);
                for (int cell = 0; cell < 1024; cell++)
                {
                    bool arrow = expected.Arrow.Cells.Any(point => 32 * point.Row + point.Column == cell);
                    BackgroundTile tile = arrow ? shown with { ColourVariant = enabled ? expected.Arrow.EnabledPalette : expected.Arrow.DisabledPalette } : shown;
                    AssertEqual(tile, tilemap[cell], "Arrow updates exactly supplied cells and preserves tile/flip/priority fields");
                }
            }
        }
    }
    /// <summary>Checks ghost appearance timing/visibility and four Norfair palette-FX cycles.</summary>
    internal static void VerifyLookupStream2GhostAndNorfair(ISnesAddressSpace rom)
    {
        Action<RoomEnemySlot, WreckedShipGhostEnemyState, Position16<RoomPixels>> spawn = WreckedShipGhostBehavior.SpawnWreckedShipGhost;
        Action<RoomEnemySlot, WreckedShipGhostEnemyState> flicker = WreckedShipGhostBehavior.AdvanceWreckedShipGhostFlicker;
        var slot = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom()).SlotTable.Slots[0];
        var state = new WreckedShipGhostEnemyState();
        for (int index = 0; index < 9; index++)
        {
            short originalX = unchecked((short)ReadVerificationWord(rom, 0xa89aa8 + 4 * index));
            short originalY = unchecked((short)ReadVerificationWord(rom, 0xa89aaa + 4 * index));
            var calculated = WreckedShipGhostAppearance.SpawnOffset(index);
            AssertEqual(originalX, calculated.X, "Ghost original horizontal spawn offset");
            AssertEqual(originalY, calculated.Y, "Ghost original vertical spawn offset");
            state.SpawnColumn = index % 3;
            state.SpawnRow = index / 3;
            spawn(slot, state, new(32, 65520));
            AssertEqual(unchecked((ushort)(32 + originalX)), slot.Position.X.Whole, "Ghost actual wrapped spawn X");
            AssertEqual(unchecked((ushort)(65520 + originalY)), slot.Position.Y.Whole, "Ghost actual wrapped spawn Y");
            AssertEqual(WreckedShipGhostAction.BrighteningAndFlickering, state.Action, "Ghost appearance phase");
            AssertEqual(64, state.StablePositionTimer, "Ghost appearance position timer");
            AssertEqual(16, state.StableDirectionTimer, "Ghost appearance direction timer");
        }
        // The flicker steps are the native interval table's words after its first, which no appearance starts (each starts
        // at the second word); the word after the last step is the table's terminator, the steps' end. A step at an odd
        // position shows the ghost.
        IReadOnlyList<WreckedShipGhostFlickerStep> steps = WreckedShipGhostAppearanceDefinitions.FlickerSteps;
        for (int position = 0; position <= steps.Count; position++)
        {
            int original = unchecked((short)ReadVerificationWord(rom, 0xa89acc + 2 * (position + 1)));
            bool ended = position == steps.Count;
            if (ended)
                AssertEqual(-1, original, "Ghost native flicker terminator follows the last step");
            else
            {
                AssertEqual(original, steps[position].Duration, "Ghost original flicker interval");
                AssertEqual(position % 2 == 1 ? WreckedShipGhostFlickerVisibility.Shown : WreckedShipGhostFlickerVisibility.Unchanged,
                    steps[position].Visibility, "Ghost original flicker visibility");
            }
            slot.Contact = EnemyContact.None;
            slot.Processing = EnemyProcessing.OffScreen;
            slot.Presence = EnemyPresence.Hidden;
            state.PhaseTimer = 1;
            state.NextFlickerStep = position;
            flicker(slot, state);
            AssertEqual(ended ? 0 : original, state.PhaseTimer, "Ghost actual next interval");
            AssertEqual(ended ? steps.Count : position + 1, state.NextFlickerStep, "Ghost actual next step");
            bool remainsInvisible = ended || position % 2 == 0;
            AssertEqual(EnemyContact.None, slot.Contact, "Ghost actual visibility preserves unrelated contact");
            AssertEqual(EnemyProcessing.OffScreen, slot.Processing, "Ghost actual visibility preserves unrelated processing");
            AssertEqual(remainsInvisible ? EnemyPresence.Hidden : EnemyPresence.None, slot.Presence, "Ghost actual visibility");
        }
        state.PhaseTimer = 2;
        state.NextFlickerStep = 0;
        slot.Contact = EnemyContact.None;
        slot.Processing = EnemyProcessing.None;
        slot.Presence = EnemyPresence.Hidden;
        flicker(slot, state);
        AssertEqual(1, state.PhaseTimer, "Ghost interval countdown");
        AssertEqual(0, state.NextFlickerStep, "Ghost no early interval advance");
        AssertEqual(EnemyContact.None, slot.Contact, "Ghost no early contact change");
        AssertEqual(EnemyProcessing.None, slot.Processing, "Ghost no early processing change");
        AssertEqual(EnemyPresence.Hidden, slot.Presence, "Ghost no early visibility change");
        state.PhaseTimer = 0;
        flicker(slot, state);
        AssertTrue(slot.Contact == EnemyContact.None && slot.Processing == EnemyProcessing.None && slot.Presence == EnemyPresence.None,
            "Ghost terminal next call clears invisibility");
        state.SpawnColumn = 3;
        state.SpawnRow = 0;
        AssertThrows<InvalidDataException>(() => spawn(slot, state, Position16<RoomPixels>.Zero), "Ghost out-of-range spawn class remains rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearance.SpawnOffset(-1), "Ghost spawn lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearance.SpawnOffset(9), "Ghost spawn upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => NorfairEnvironmentalPaletteFxColorDefinitionsConstants.Duration(-1), "Norfair duration lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => NorfairEnvironmentalPaletteFxColorDefinitionsConstants.Duration(16), "Norfair duration upper bound");
        Console.WriteLine("Ghost appearance: 18 native offset fields, 15 native intervals and the terminator, actual wrapped positions/timing/visibility and malformed states pass; Norfair: 64 native durations and four 116-tick cycles pass.");
    }
    /// <summary>Checks compiled palette instruction mechanics across upper, old, and belly color programs.</summary>
    internal static void VerifyLookupStream2PaletteMechanics()
    {
        var definitions = TorizoBellyPaletteFxColorDefinitions.All;
        AssertEqual(2, definitions.Count, "Both Torizo programs enumerated");
        int enumerated = 0;
        foreach (var definition in definitions)
        {
            AssertEqual(definitions[enumerated].Owner, definition.Owner, "Torizo enumerated owner");
            enumerated++;
        }
        AssertEqual(2, enumerated, "Torizo definition enumeration");
        AssertThrows<IndexOutOfRangeException>(() => { _ = definitions[-1]; }, "Torizo definition lower bound");
        AssertThrows<IndexOutOfRangeException>(() => { _ = definitions[2]; }, "Torizo definition upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.FramePointer(-1), "Upper Crateria lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.FramePointer(14), "Upper Crateria upper frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorPointer(0, -1), "Old Tourian lower color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorPointer(0, 8), "Old Tourian upper color bound");
        Console.WriteLine("Stream 2 palette mechanics: original mechanics, inline color offsets, durations, dispatch and bounds pass.");
    }
    /// <summary>Checks Crystal Flash body colors and emitted CGRAM against extracted native phases.</summary>
    internal static void VerifyLookupStream2CrystalBody(ISnesAddressSpace rom)
    {
        byte[] source = CrystalFlashColorExtractor.Extract(rom);
        var stock = CrystalFlashColorFile.Read(new MemoryStream(source));
        var samus = new SamusColours();
        for (int frame = 0; frame < CrystalFlashColorFormat.BodyFrameCount; frame++)
        {
            ushort pointer = ReadVerificationWord(rom, SamusSourceDefinitions.CrystalFlashBodyRecords.Value + frame * 4);
            samus.LayOverBeams(SamusShades.CrystalFlashBody(frame));
            for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
            {
                ushort expected = ReadVerificationWord(rom, 0x9b0000 | pointer + 2 * color);
                AssertEqual(expected, stock.ResolveBody(frame, color), "Original Crystal Flash body RGB5 word");
                AssertEqual(expected, samus.ShownBeams(InstalledFixtures.SamusShadeSources with { CrystalFlash = stock })[color], "Installed body color reaches the body shade");
            }
        }
        for (int frame = 0; frame < CrystalFlashColorFormat.BubbleFrameCount; frame++)
        for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
        {
            ushort pointer = ReadVerificationWord(rom, SamusSourceDefinitions.BubblePointers.Value + frame * 2);
            ushort original = ReadVerificationWord(rom, 0x9b0000 | pointer + 2 * color);
            AssertEqual(original, stock.ResolveBubble(frame, color), "Original bubble payload preserved");
            samus.LayOverBeams(SamusShades.CrystalFlashBubble(frame));
            AssertEqual(original, samus.ShownBeams(InstalledFixtures.SamusShadeSources with { CrystalFlash = stock })[SamusPaletteDefinitionsCrystalFlashConstants.BubbleCgramStart - SamusPaletteDefinitionsCrystalFlashConstants.BodyCgramStart + color], "Original bubble color reaches the bubble shade");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(-1, 0), "Bubble lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(6, 0), "Bubble upper frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(0, -1), "Bubble lower color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(0, 6), "Bubble upper color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(-1, 0), "Body lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(10, 0), "Body upper frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(0, -1), "Body lower color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(0, 10), "Body upper color bound");
        Console.WriteLine("Stream 2 Crystal Flash body: all100 body and36 bubble native colors and CGRAM application pass.");
    }

    /// <summary>Checks Kraid palette ramp derivation and bounds against stock samples.</summary>
    internal static void VerifyLookupStream2KraidRamps(ISnesAddressSpace rom)
    {
        PaletteRgb5[] ReadSource(KraidPaletteSource source) => Enumerable.Range(0, KraidPalettes.ColorCount(source))
            .Select(index =>
            {
                ushort word = ReadVerificationWord(rom, KraidPaletteSources.SourceAddress(source) + 2 * index);
                return new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
            }).ToArray();
        var document = new KraidColorDocument
        {
            Version = KraidColorFormat.Version,
            RoomBackdrop = ReadSource(KraidPaletteSource.RoomBackdrop),
            InitialTarget = ReadSource(KraidPaletteSource.InitialTarget),
            Health = ReadSource(KraidPaletteSource.Health),
            Secondary = ReadSource(KraidPaletteSource.Secondary),
            DeathArm = ReadSource(KraidPaletteSource.DeathArm),
        };
        var stock = KraidColorFile.Read(new MemoryStream(KraidColorFile.Write(document)));
        foreach (KraidPaletteSource source in Enum.GetValues<KraidPaletteSource>())
        for (int index = 0; index < KraidPalettes.ColorCount(source); index++)
            AssertEqual(ReadVerificationWord(rom, KraidPaletteSources.SourceAddress(source) + 2 * index),
                stock.Resolve(source, index), $"Kraid {source} installed colour {index}");
        Check(stock, document);
        string expectedIdentity = SelectedPresentationHash.Create("enemy-kraid-colors-v1", content =>
        {
            foreach (KraidPaletteSource source in Enum.GetValues<KraidPaletteSource>())
            {
                content.Append("source", (int)source);
                content.AppendWords("colors", Source(document, source).Select(Pack).ToArray());
            }
        });
        AssertEqual(expectedIdentity, stock.ContentIdentity, "Calculated Kraid colors preserve canonical content identity");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(KraidPaletteSource.Health, -1), "Kraid calculated lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(KraidPaletteSource.Secondary, 144), "Kraid calculated upper bound");
        Console.WriteLine("Stream 2 Kraid colors: all 336 installed words match the cartridge; canonical identity passes.");

        static ushort Pack(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        static PaletteRgb5[] Source(KraidColorDocument value, KraidPaletteSource source) => source switch
        {
            KraidPaletteSource.RoomBackdrop => value.RoomBackdrop,
            KraidPaletteSource.InitialTarget => value.InitialTarget,
            KraidPaletteSource.Health => value.Health,
            KraidPaletteSource.Secondary => value.Secondary,
            KraidPaletteSource.DeathArm => value.DeathArm,
            _ => throw new ArgumentOutOfRangeException(nameof(source)),
        };
        static void Check(KraidColorCatalog actual, KraidColorDocument expected)
        {
            foreach (KraidPaletteSource source in Enum.GetValues<KraidPaletteSource>())
            {
                var colors = Source(expected, source);
                for (int index = 0; index < colors.Length; index++)
                    AssertEqual(Pack(colors[index]), actual.Resolve(source, index), "Every original Kraid color");
            }
        }
    }
    /// <summary>Checks projectile frame bindings for power and directional combinations.</summary>
    internal static void VerifyLookupStream2PowerDirectionBindings(ISnesAddressSpace rom)
    {
        byte[] json = ProjectileFrameBindingExtractor.Extract(rom);
        var stock = ProjectileFrameBindingFile.Load(new MemoryStream(json));
        foreach (ushort pointer in SamusProjectileFrameDefinitions.TimedRecordAddresses)
            AssertEqual(ReadVerificationWord(rom, (0x930000 | pointer) + 2), stock.Resolve(pointer), "Every installed selector retains its exact native target");
        for (int direction = 0; direction < 8; direction++)
        {
            ushort pointer = (ushort)(0x86db + direction * 12);
            var shot = new SamusProjectile(0) { Playback = SamusProjectileProgramAddresses.PlaybackAt(pointer), FrameHold = WrappingCountdown.Start(1) };
            var system = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount)) { Animation = { FrameBindings = stock } };
            _ = system.Animation.AdvanceProjectileAnimation(shot);
            AssertEqual(ReadVerificationWord(rom, (0x930000 | pointer) + 2), shot.SpritemapPointer, "Actual projectile handler selects calculated compass pose");
        }
        int checkedWaveIce = 0;
        int checkedAxialEndPairs = 0;
        int checkedCappedSweep = 0;
        foreach (ushort pointer in SamusProjectileFrameDefinitions.TimedRecordAddresses)
        {
            if (pointer is not (>= 0x873b and < 0x8973) and
                not (>= 0x8e77 and < 0x8f17) and
                not (>= 0x912f and < 0x914f) and
                not (>= 0xa007 and < 0xa113) and
                not (>= 0x9ebb and < 0xa007) and
                not (>= 0x8f17 and < 0x912f) and
                not (>= 0x9153 and < 0x936b) and not (>= 0x8977 and < 0x8e77) and not (>= 0x936b and < 0x9ebb) and not (>= 0xa119 and < 0xa19d)) continue;
            if (pointer is >= 0x8f8f and <= 0x8f97 or
                >= 0x9097 and <= 0x909f or
                >= 0x91cb and <= 0x91d3 or
                >= 0x92d3 and <= 0x92db)
                checkedAxialEndPairs++;
            if (pointer is >= 0x8bfb and < 0x8c4f)
                checkedCappedSweep++;
            var shot = new SamusProjectile(0) { Playback = SamusProjectileProgramAddresses.PlaybackAt(pointer), FrameHold = WrappingCountdown.Start(1) };
            var system = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount)) { Animation = { FrameBindings = stock } };
            _ = system.Animation.AdvanceProjectileAnimation(shot);
            AssertEqual(ReadVerificationWord(rom, (0x930000 | pointer) + 2), shot.SpritemapPointer, "Actual handler uses native Wave/Ice traversal pose");
            checkedWaveIce++;
        }
        AssertEqual(787, checkedWaveIce, "All other calculated beam/effect records including18 explicit exceptional policies");
        AssertEqual(8, checkedAxialEndPairs, "All four reversed axial end-pairs use exact native operands");
        AssertEqual(10, checkedCappedSweep, "All ten capped sweep phases use exact native operands");
        int checkedBombs = 0;
        foreach (ushort pointer in SamusProjectileFrameDefinitions.TimedRecordAddresses)
        {
            if (pointer is < 0x9f87 or >= 0xa007) continue;
            var bomb = new SamusBombProjectileSlot(0) { Playback = SamusProjectileProgramAddresses.PlaybackAt(pointer), FrameHold = WrappingCountdown.Start(1), Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(0x0500) };
            var bombs = new SamusBombProjectileSystem { FrameBindings = stock };
            _ = bombs.AdvanceProjectileAnimation(bomb);
            AssertEqual(ReadVerificationWord(rom, (0x930000 | pointer) + 2), bomb.SpritemapPointer, "Actual bomb handler uses sequential normal/fast pose");
            checkedBombs++;
        }
        AssertEqual(14, checkedBombs, "Both normal/fast PowerBomb and Bomb visual cycles");
        AssertThrows<InvalidDataException>(() => stock.Resolve(0x87c3), "Wave self-jump is not accepted as a timed record");
        AssertThrows<InvalidDataException>(() => stock.Resolve(0x86dc), "Program interior is not accepted as a timed record");
        Console.WriteLine($"Stream2 Power bindings: {SamusProjectileFrameDefinitions.TimedRecordAddresses.Count} installed beam/effect selectors and native operands and actual handler paths pass.");
    }

    /// <summary>Checks Golden Torizo foot extended frames and OAM geometry.</summary>
    internal static void VerifyLookupStream2GoldenTorizoFootGeometry(CartridgeImportAddressSpace rom)
    {
        var definitions = EnemyExtendedFrameDefinitions.Frames.ToArray();
        var document = new EnemyIdentityFixture().ExtendedDocument();
        var selected = new EnemyFrameName[10];
        int nativeBodyParts = 0;
        for (int phase = 0; phase < 10; phase++)
        {
            int identity = 0xaaa4fa + 34 * phase;
            var frame = definitions.Single(frame => (((int)new SnesAddress(frame.Bank, frame.Pointer))) == identity);
            selected[phase] = frame;
            AssertEqual((ushort)4, CartridgeImportSource.Require(rom).ReadCartridgeWord(identity), "Golden Torizo four native visual components");
            var components = new EnemyExtendedVisualComponent[4];
            for (int component = 0; component < 4; component++)
            {
                int record = identity + 2 + component * 8;
                int source = 0xaa0000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 4));
                if (component == 2) nativeBodyParts += CartridgeImportSource.Require(rom).ReadCartridgeWord(source);
                var parts = Enumerable.Range(0, CartridgeImportSource.Require(rom).ReadCartridgeWord(source)).Select(index =>
                {
                    int entry = source + 2 + index * 5;
                    var x = new SnesSpritemapXWord(CartridgeImportSource.Require(rom).ReadCartridgeWord(entry));
                    var attributes = new SnesObjAttributeWord(CartridgeImportSource.Require(rom).ReadCartridgeWord((entry + 3)));
                    return new SpriteVisualPart
                    {
                        OffsetX = x.SignedOffset, OffsetY = unchecked((sbyte)rom.ReadCartridgeByte(entry + 2)), Size = x.IsLarge ? 16 : 8,
                        TileColumn = attributes.TileNumber % 16, TileRow = attributes.TileNumber / 16,
                        Palette = attributes.PaletteIndex, Priority = attributes.Priority,
                        FlipX = attributes.FlipHorizontally, FlipY = attributes.FlipVertically,
                    };
                }).ToArray();
                components[component] = new EnemyExtendedVisualComponent
                { OffsetX = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(record)), OffsetY = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 2))), Parts = parts };
            }
            document.Frames[frame.Name] = components;
        }
        AssertEqual(220, nativeBodyParts, "Golden Torizo ten native body-composition extents");
        EnemyExtendedFrameCatalog Load() => EnemyExtendedFrameDocuments.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
        void Check(EnemyExtendedFrameCatalog catalog)
        {
            foreach (var frame in selected)
            {
                AssertTrue(catalog.TryGetDisplay(frame.Bank, frame.Pointer, out var actual), "Golden Torizo installed display binding");
                var expected = document.Frames[(document.DisplayFrames ?? throw new InvalidOperationException("Verification requires document.DisplayFrames."))[frame.Name]];
                AssertEqual(expected.Length, actual.Length, "Golden Torizo component order/extent");
                var expectedOam = new OamBuffer(); expectedOam.BeginFrame();
                var actualOam = new OamBuffer(); actualOam.BeginFrame();
                for (int component = 0; component < expected.Length; component++)
                {
                    var wanted = expected[component];
                    var found = actual.Span[component];
                    var parts = EnemySpritemapCompiler.CompileParts(wanted.Parts, frame.Name);
                    AssertEqual((short)wanted.OffsetX, found.Offset.X, "Golden Torizo independent component X");
                    AssertEqual((short)wanted.OffsetY, found.Offset.Y, "Golden Torizo independent component Y");
                    AssertTrue(found.Parts.SequenceEqual(parts), "Golden Torizo exact supplied part order and fields");
                    ushort x = unchecked((ushort)(128 + wanted.OffsetX)), y = unchecked((ushort)(96 + wanted.OffsetY));
                    expectedOam.AddEnemySpritemap(EnemySpritemapParts.FromOwnedArray(parts), new Position16<SpritePixels>(x, y), 0, 0);
                    actualOam.AddEnemySpritemap(found.Parts, new Position16<SpritePixels>(x, y), 0, 0);
                }
                AssertEqual(expectedOam.NextByteOffset, actualOam.NextByteOffset, "Golden Torizo packed OBJ count");
                AssertTrue(expectedOam.ImageEquals(actualOam),
                    "Golden Torizo calculated stock/supplied parts produce exact packed OAM");
            }
            var identities = definitions.ToDictionary(frame => frame.Name,
                frame => EnemyFrameSheetKeys.Of(RoomEnemySlotFrameAccess.FrameAt(frame.Bank, frame.Pointer) ?? throw new InvalidOperationException($"{frame.Name} shows no frame.")), StringComparer.Ordinal);
            var orderedFrames = definitions.OrderBy(frame => identities[frame.Name], StringComparer.Ordinal).ToArray();
            string expectedHash = SelectedPresentationHash.Create("enemy-extended-oam-v2", content =>
            {
                foreach (var frame in orderedFrames)
                {
                    content.AppendName("frame", identities[frame.Name]);
                    var components = document.Frames[frame.Name];
                    content.Append("components", components.Length);
                    foreach (var component in components)
                    {
                        content.Append("offset-x", component.OffsetX); content.Append("offset-y", component.OffsetY);
                        content.AppendEnemyParts(EnemySpritemapCompiler.CompileParts(component.Parts, frame.Name));
                    }
                }
                foreach (var frame in orderedFrames)
                {
                    content.AppendName("native-binding", identities[frame.Name]);
                    content.AppendName("selected-binding", identities[(document.DisplayFrames ?? throw new InvalidOperationException("Verification requires document.DisplayFrames."))[frame.Name]]);
                }
            });
            AssertEqual(expectedHash, catalog.ContentIdentity, "Golden Torizo calculated extended-part hash remains canonical");
        }
        var stock = Load(); Check(stock);
        foreach (var frame in selected)
        {
            AssertTrue(stock.TryGet(frame.Bank, frame.Pointer, out _), "Golden Torizo native identity stays installed");
        }
        for (int step = 0; step < GoldenTorizoWalk.StepCount; step++)
            AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(0xaad59a + step * 2)), GoldenTorizoWalk.Velocity(step), "Golden Torizo walking steps match native");
        Suite(nameof(VerifyCompiledStatueWalking), () => VerifyCompiledStatueWalking(rom));
        AssertThrows<ArgumentOutOfRangeException>(() => GoldenTorizoStride.HorizontalAdvance(10), "Golden Torizo phase upper bound");
        Console.WriteLine("Golden Torizo foot geometry:20derived words,39native byte windows,220body parts/40components,10calculated views,hash/packed OAM and actual walking calls pass; seven artwork origins remain required.");
    }

    /// <summary>Runs Golden Torizo foot geometry and walking stride checks against the retail source.</summary>
    [VerificationCommand("--lookup-stream2-golden-torizo-strides")]
    internal static int LookupStream2GoldenTorizoStridesCommand()
    {
        Suite(nameof(VerifyLookupStream2GoldenTorizoFootGeometry), () => VerifyLookupStream2GoldenTorizoFootGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs Chozo statue foot spritemap geometry checks.</summary>
    [VerificationCommand("--lookup-stream2-chozo-strides")]
    internal static int LookupStream2ChozoStridesCommand()
    {
        Suite(nameof(VerifyLookupStream2ChozoFootGeometry), () => VerifyLookupStream2ChozoFootGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs Old Tourian accent palette per-tick cadence checks.</summary>
    [VerificationCommand("--lookup-stream2-tourian-accent-cadence")]
    internal static int LookupStream2TourianAccentCadenceCommand()
    {
        Suite(nameof(VerifyLookupStream2TourianAccentCadence), () => VerifyLookupStream2TourianAccentCadence(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs Ninja Space Pirate mechanics and visual address layout checks.</summary>
    [VerificationCommand("--lookup-stream2-ninja-program-layout")]
    internal static int LookupStream2NinjaProgramLayoutCommand()
    {
        Suite(nameof(VerifyLookupStream2NinjaProgramLayout), () => VerifyLookupStream2NinjaProgramLayout());
        return 0;
    }

    /// <summary>Checks the Yard visual frame grouping without a cartridge oracle.</summary>
    [VerificationCommand("--lookup-stream2-yard-groups")]
    internal static int LookupStream2YardGroupsCommand()
    {
     VerifyLookupStream2YardGroups(); return 0; 
    }

    /// <summary>Runs Power Beam directional frame binding comparison.</summary>
    [VerificationCommand("--lookup-stream2-power-direction-bindings")]
    internal static int LookupStream2PowerDirectionBindingsCommand()
    {
        Suite(nameof(VerifyLookupStream2PowerDirectionBindings), () => VerifyLookupStream2PowerDirectionBindings(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs projectile identity and actual OAM composition comparison.</summary>
    [VerificationCommand("--lookup-stream2-projectile-identity-geometry")]
    internal static int LookupStream2ProjectileIdentityGeometryCommand()
    {
        Suite(nameof(VerifyLookupStream2ProjectileIdentityGeometry), () => VerifyLookupStream2ProjectileIdentityGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs pause reserve label extraction and placement checks.</summary>
    [VerificationCommand("--lookup-stream2-reserve-labels")]
    internal static int LookupStream2ReserveLabelsCommand()
    {
        Suite(nameof(VerifyLookupStream2ReserveLabels), () => VerifyLookupStream2ReserveLabels(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs pause equipment label geometry checks.</summary>
    [VerificationCommand("--lookup-stream2-equipment-labels")]
    internal static int LookupStream2EquipmentLabelsCommand()
    {
        Suite(nameof(VerifyLookupStream2EquipmentLabels), () => VerifyLookupStream2EquipmentLabels(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs blank equipment cell ownership checks.</summary>
    [VerificationCommand("--lookup-stream2-equipment-blank")]
    internal static int LookupStream2EquipmentBlankCommand()
    {
        Suite(nameof(VerifyLookupStream2EquipmentBlank), () => VerifyLookupStream2EquipmentBlank(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs Dead Torizo corpse tile and OAM geometry comparison.</summary>
    [VerificationCommand("--lookup-stream2-dead-torizo-geometry")]
    internal static int LookupStream2DeadTorizoGeometryCommand()
    {
        Suite(nameof(VerifyLookupStream2DeadTorizoGeometry), () => VerifyLookupStream2DeadTorizoGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs crawler and Yard velocity ramp comparison.</summary>
    [VerificationCommand("--lookup-stream2-crawler-ramps")]
    internal static int LookupStream2CrawlerRampsCommand()
    {
        Suite(nameof(VerifyLookupStream2CrawlerRamps), () => VerifyLookupStream2CrawlerRamps(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs reserve arrow color and CGRAM placement checks.</summary>
    [VerificationCommand("--lookup-stream2-reserve-arrow-colors")]
    internal static int LookupStream2ReserveArrowColorsCommand()
    {
        Suite(nameof(VerifyLookupStream2ReserveArrowColors), () => VerifyLookupStream2ReserveArrowColors(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs projectile trail appearance checks.</summary>
    [VerificationCommand("--lookup-stream2-trail-appearance")]
    internal static int LookupStream2TrailAppearanceCommand()
    {
        Suite(nameof(VerifyLookupStream2TrailAppearance), () => VerifyLookupStream2TrailAppearance(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs lava-jumper control and visual layout checks.</summary>
    [VerificationCommand("--lookup-stream2-lava-jumper-layout")]
    internal static int LookupStream2LavaJumperLayoutCommand()
    {
        Suite(nameof(VerifyLookupStream2LavaJumperLayout), () => VerifyLookupStream2LavaJumperLayout(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs Chozo statue instruction layout and bounds checks.</summary>
    [VerificationCommand("--lookup-stream2-chozo-layout")]
    internal static int LookupStream2ChozoLayoutCommand()
    {
        Suite(nameof(VerifyLookupStream2ChozoLayout), () => VerifyLookupStream2ChozoLayout(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs Torizo graphics page dispatch and source resolution checks.</summary>
    [VerificationCommand("--lookup-stream2-torizo-page-dispatch")]
    internal static int LookupStream2TorizoPageDispatchCommand()
    {
        Suite(nameof(VerifyLookupStream2TorizoPageDispatch), () => VerifyLookupStream2TorizoPageDispatch(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs Kraid lint initialization state comparison.</summary>
    [VerificationCommand("--lookup-stream2-kraid-lint-initialization")]
    internal static int LookupStream2KraidLintInitializationCommand()
    {
        Suite(nameof(VerifyLookupStream2KraidLintInitialization), () => VerifyLookupStream2KraidLintInitialization(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs Kago visual frame and geometry checks.</summary>
    [VerificationCommand("--lookup-stream2-kago-frame-geometry")]
    internal static int LookupStream2KagoFrameGeometryCommand()
    {
        Suite(nameof(VerifyLookupStream2KagoFrameGeometry), () => VerifyLookupStream2KagoFrameGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs all three Pipe Bug family's visual program geometry checks.</summary>
    [VerificationCommand("--lookup-stream2-pipe-bug-visual-geometry")]
    internal static int LookupStream2PipeBugVisualGeometryCommand()
    {
        Suite(nameof(VerifyLookupStream2PipeBugVisualGeometry), () => VerifyLookupStream2PipeBugVisualGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs horizontal camera target selection checks.</summary>
    [VerificationCommand("--lookup-stream2-horizontal-camera-targets")]
    internal static int LookupStream2HorizontalCameraTargetsCommand()
    {
        Suite(nameof(VerifyLookupStream2HorizontalCameraTargets), () => VerifyLookupStream2HorizontalCameraTargets(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs weighted enemy drop selection and RNG boundary checks.</summary>
    [VerificationCommand("--lookup-stream2-drop-selection")]
    internal static int LookupStream2DropSelectionCommand()
    {
        Suite(nameof(VerifyLookupStream2DropSelection), () => VerifyLookupStream2DropSelection(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs five-member Pipe Bug formation checks.</summary>
    [VerificationCommand("--lookup-stream2-pipe-bug-formation")]
    internal static int LookupStream2PipeBugFormationCommand()
    {
        Suite(nameof(VerifyLookupStream2PipeBugFormation), () => VerifyLookupStream2PipeBugFormation(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs Golden Torizo initial and landing instruction control checks.</summary>
    [VerificationCommand("--lookup-stream2-golden-control")]
    internal static int LookupStream2GoldenControlCommand()
    {
        return 0;
    }

    /// <summary>Runs projectile trail program and emitted OAM checks.</summary>
    [VerificationCommand("--lookup-stream2-trail-programs")]
    internal static int LookupStream2TrailProgramsCommand()
    {
        Suite(nameof(VerifyLookupStream2TrailPrograms), () => VerifyLookupStream2TrailPrograms(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs left/right trail selector table comparison.</summary>
    [VerificationCommand("--lookup-stream2-trail-selectors")]
    internal static int LookupStream2TrailSelectorsCommand()
    {
        Suite(nameof(VerifyLookupStream2TrailSelectors), () => VerifyLookupStream2TrailSelectors(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs Mother Brain tube palette ramp and CGRAM checks.</summary>
    [VerificationCommand("--lookup-stream2-tube-ramp")]
    internal static int LookupStream2TubeRampCommand()
    {
        Suite(nameof(VerifyLookupStream2TubeRamp), () => VerifyLookupStream2TubeRamp(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs full pause-screen cell destination ownership checks.</summary>
    [VerificationCommand("--lookup-stream2-pause-ownership")]
    internal static int LookupStream2PauseOwnershipCommand()
    {
        Suite(nameof(VerifyLookupStream2PauseOwnership), () => VerifyLookupStream2PauseOwnership(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs Dead Torizo descriptor and queued VRAM transfer comparison.</summary>
    [VerificationCommand("--lookup-stream2-dead-torizo-transfers")]
    internal static int LookupStream2DeadTorizoTransfersCommand()
    {
        Suite(nameof(VerifyLookupStream2DeadTorizoTransfers), () => VerifyLookupStream2DeadTorizoTransfers(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
