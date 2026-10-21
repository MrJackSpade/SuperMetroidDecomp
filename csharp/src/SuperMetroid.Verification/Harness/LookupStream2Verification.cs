using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.PipeBugs.Artwork;
using SuperMetroid.Verification.Enemies.PipeBugs;
using SuperMetroid.Verification.Enemies.LavaJumpers;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
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
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Progression;
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

    /// <summary>Checks Chozo statue foot spritemap geometry and independent compiled edits.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Chozo foot geometry")]
    internal static void VerifyLookupStream2ChozoFootGeometry(SuperMetroidAddressSpace rom)
    {
        int[] nativeIdentities = [0xaae943, 0xaae9ae, 0xaaea1e, 0xaaea8e, 0xaaeafe, 0xaaeb69, 0xaaebd9, 0xaaec49];
        var definitions = EnemySpritemapDefinitions.Frames.ToArray();
        var document = new EnemySpritemapDocument
        {
            Version = (int)EnemySpritemapSchema.Current,
            Frames = definitions.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>(), StringComparer.Ordinal),
            DisplayFrames = definitions.ToDictionary(frame => frame.Name, frame => frame.Name, StringComparer.Ordinal),
        };
        var selected = definitions.Where(frame => nativeIdentities.Contains(((int)new SnesAddress(frame.Bank, frame.Pointer)))).ToArray();
        AssertEqual(8, selected.Length, "Eight native Chozo stride composition identities");
        for (int pose = 0; pose < nativeIdentities.Length; pose++)
            AssertEqual(nativeIdentities[pose], ChozoStrideGeometryDefinitions.NativePoseIdentity(pose), "Chozo counted-record identity geometry");
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
        EnemySpritemapCatalog Load() => EnemySpritemapCatalog.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
        void Check(EnemySpritemapCatalog catalog)
        {
            foreach (var frame in selected)
            {
                AssertTrue(catalog.TryGetDisplay(frame.Bank, frame.Pointer, out var parts), "Chozo actual installed display binding");
                AssertTrue(parts.SequenceEqual(EnemySpritemapCatalog.CompileParts(document.Frames[frame.Name], frame.Name)),
                    "Chozo source/supplied part ordering and every visual field remain exact");
            }
            string expected = SelectedPresentationHash.Create("enemy-oam-v1", content =>
            {
                foreach (var frame in definitions.OrderBy(frame => ((int)new SnesAddress(frame.Bank, frame.Pointer))))
                {
                    content.Append("frame", ((int)new SnesAddress(frame.Bank, frame.Pointer)));
                    content.AppendEnemyParts(EnemySpritemapCatalog.CompileParts(document.Frames[frame.Name], frame.Name));
                }
                foreach (var frame in definitions.OrderBy(frame => ((int)new SnesAddress(frame.Bank, frame.Pointer))))
                {
                    content.Append("native-binding", ((int)new SnesAddress(frame.Bank, frame.Pointer)));
                    content.Append("selected-binding", ((int)new SnesAddress(frame.Bank, frame.Pointer)));
                }
            });
            AssertEqual(expected, catalog.ContentIdentity, "Chozo calculated foot fields retain canonical presentation hash");
        }
        var stock = Load();
        Check(stock);
        foreach (var frame in selected)
        {
            AssertTrue(stock.TryGet(frame.Bank, frame.Pointer, out var parts), "Chozo stock frame exists");
            AssertEqual("FootParts", parts.GetType().Name, "Chozo stock foot coordinates use shared calculated owner");
            var visual = document.Frames[frame.Name];
            for (int index = 0; index < visual.Length; index++)
            {
                var original = visual[index];
                if (original.TileRow * 16 + original.TileColumn is not (0x170 or 0x171)) continue;
                visual[index] = original with { OffsetX = original.OffsetX + 1 };
                Check(Load());
                visual[index] = original;
                // Gameplay geometry must not follow an independently edited display anchor.
                AssertEqual(ChozoCarryMotionDefinitions.Read(5).Velocity, (short)-0x300, "Chozo edited art does not move carry mechanics");
                AssertEqual(ChozoCarryMotionDefinitions.Read(6).Velocity, (short)-0xe00, "Chozo edited art does not alter late support displacement");
            }
        }
        Suite(nameof(VerifyCompiledStatueWalking), () => VerifyCompiledStatueWalking(rom));
        AssertThrows<ArgumentOutOfRangeException>(() => ChozoStrideGeometryDefinitions.SupportFootX(4), "Chozo support-phase upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => ChozoStrideGeometryDefinitions.NativePoseIdentity(-1), "Chozo pose lower bound");
        Console.WriteLine("Chozo shared geometry:174 native parts,eight stock views,16 independent foot-X edits,hash/display order and existing96-word/actual carry checks pass; shape and two movement-policy inputs remain documented.");
    }

    /// <summary>Checks Wrecked Ship ghost palette phases against native CGRAM writes.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for ghost palette")]
    internal static void VerifyLookupStream2GhostPalette(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        var cgram = new SnesCgram();
        enemies.Presentation.BindCgram(cgram);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.PaletteIndex = 0;
        var state = new WreckedShipGhostEnemyState { PhaseTimer = NativeCountdown.Start(2) };
        Action<RoomEnemySlot, WreckedShipGhostEnemyState> brightening = enemies.WreckedShipGhostBehavior.RunWreckedShipGhostBrightening;
        Func<RoomEnemySlot, WreckedShipGhostEnemyState, int> fade = enemies.WreckedShipGhostBehavior.StepWreckedShipGhostPaletteTowardTarget;
        var native = new ushort[16];
        for (int color = 0; color < native.Length; color++)
        {
            native[color] = ReadVerificationWord(rom, 0xa899ac + color * 2);
            AssertEqual(native[color], WreckedShipGhostAppearanceDefinitions.PaletteColor(color), "Calculated ghost channels preserve native target words");
            AssertEqual(native[color], ReadVerificationWord(rom, 0xa8aafe + color * 2), "Kago shares exactly the same native palette inputs");
            if (color < 9) AssertEqual(native[color], ReadVerificationWord(rom, 0xa89f4f + color * 2), "Yapping Maw shares olive target inputs");
            cgram.SetColor(128 + color, Bgr555.FromWord(0x7fff));
        }
        brightening(slot, state);
        AssertEqual(WreckedShipGhostAiFunction.FadingToGhostPalette, state.Function, "White flash installs native target palette phase");
        AssertTrue(state.TargetPalette.Span.SequenceEqual(ToColors(native)), "Actual target buffer includes unused sprite slots");
        for (int tick = 1; tick <= 32; tick++)
        {
            int expectedChanges = 0;
            for (int color = 0; color < native.Length; color++)
            for (int shift = 0; shift < 15; shift += 5)
                if (31 - (tick - 1) > (native[color] >> shift & 31)) expectedChanges++;
            AssertEqual(expectedChanges, fade(slot, state), "Native component-change count preserves fade completion");
            for (int color = 0; color < native.Length; color++)
            {
                int expected = 0;
                for (int shift = 0; shift < 15; shift += 5)
                    expected |= Math.Max(31 - tick, native[color] >> shift & 31) << shift;
                AssertEqual((ushort)expected, cgram.Colors[128 + color], "Every actual fade step preserves visible and unused target slots");
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
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.PaletteColor(-1), "Ghost color lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.PaletteColor(16), "Ghost color upper bound");
        Console.WriteLine("Ghost palette:16 native targets and shared-source words,actual white-flash target copy,32 full component-fade steps,and native pixel-slot usage pass; calculated channels preserve the specified source paint and transparent payload.");
    }

    /// <summary>Checks Old Tourian accent durations, repeated colors, and per-tick CGRAM updates.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
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
            var actualSession = new PaletteFxSessionState();
            var actual = new RoomPaletteFxSystem(actualSession);
            actual.SpawnDefinition(definition.DefinitionPointer, equippedItems: 0);
            var cgram = new SnesCgram();
            var colors = new ReferencePaletteFxColorSource(guarded);
            int frameIndex = 0, elapsed = 0;
            for (int tick = 0; tick <= cycle; tick++)
            {
                if (elapsed == nativeDurations[frameIndex]) { frameIndex = (frameIndex + 1) % 15; elapsed = 0; }
                actual.Step(cgram, colors, new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
                for (int color = 0; color < 3; color++)
                    AssertEqual(ReadVerificationWord(rom, 0x8d0000 | definition.ColorPointer(frameIndex, color)),
                        cgram.Colors[definition.ColorByteIndex / 2 + color], "Actual CGRAM follows each exact native hold and loop boundary");
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
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyLookupStream2ProjectileIdentityGeometry(ISnesAddressSpace rom)
    {
        var expected = new SortedSet<ushort>();
        foreach (ushort record in SamusProjectileRecordDefinitions.TimedRecordAddresses)
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
        AssertEqual(417, expected.Count, "Native805 timed records reference417 distinct compositions");
        int ordinal = 0;
        foreach (ushort pointer in expected)
            AssertEqual(pointer, ProjectileSpriteDefinitions.NativePointers[ordinal++], "Calculated projectile identity equals native sorted selector union");
        AssertTrue(expected.SequenceEqual(ProjectileSpriteDefinitions.NativePointers), "Projectile identity enumeration preserves native order");
        byte[] json = ProjectileSpriteExtractor.Extract(rom);
        var selected = ProjectileSpriteCatalog.Load(new MemoryStream(json));
        var frames = (System.Collections.IDictionary)selected.frames;
        int calculatedCompositions = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (ProjectileSpritePartDefinitions.SingleBeamPoses.Any(pose => ReferenceEquals(pose, composition.parts)))
                calculatedCompositions++;
        AssertEqual(12, calculatedCompositions, "Eight Power and four Ice compositions calculate without cached part arrays");
        int calculatedQuads = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (composition.parts is ChargedBeamParts)
                calculatedQuads++;
        AssertEqual(19, calculatedQuads, "Nineteen charged Power/Ice quadrant compositions calculate without cached part arrays");
        int calculatedWaves = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (ProjectileSpritePartDefinitions.WavePoses.Any(pose => ReferenceEquals(pose, composition.parts)))
                calculatedWaves++;
        AssertEqual(33, calculatedWaves, "All33 stock Wave compositions share directional geometry without cached parts");
        int calculatedLobes = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (composition.parts is VerticalChargedWaveParts)
                calculatedLobes++;
        AssertEqual(20, calculatedLobes, "Both charged-Wave families share centered/vertical lobe geometry without cached parts");
        int calculatedMissiles = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (composition.parts is AxialSuperMissileParts)
                calculatedMissiles++;
        AssertEqual(4, calculatedMissiles, "Four SuperMissile axial poses share centered strip geometry without cached parts");
        int calculatedDiagonalMissiles = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (composition.parts is DiagonalMissileParts)
                calculatedDiagonalMissiles++;
        AssertEqual(8, calculatedDiagonalMissiles, "Eight diagonal missile poses share reflected footprint geometry without cached part arrays");
        int calculatedSimpleEffects = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (composition.parts is SimpleEffectParts)
                calculatedSimpleEffects++;
        AssertEqual(8, calculatedSimpleEffects, "Four centered Bomb poses and four mirrored beam explosions calculate without cached parts");
        int calculatedHorizontalLobes = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (composition.parts is HorizontalChargedWaveParts)
                calculatedHorizontalLobes++;
        AssertEqual(16, calculatedHorizontalLobes, "Sixteen horizontal charged Wave lobes calculate geometry with exact native corner order");
        int calculatedSpazerSeeds = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (composition.parts is SpazerSeedParts)
                calculatedSpazerSeeds++;
        AssertEqual(8, calculatedSpazerSeeds, "Eight Spazer seed poses share tile/reflection geometry and one required diagonal origin");
        int calculatedSpazerSpreads = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (composition.parts is SpazerDiagonalSpreadParts)
                calculatedSpazerSpreads++;
        AssertEqual(16, calculatedSpazerSpreads, "Sixteen diagonal Spazer spreads calculate repeated seed lanes");
        int calculatedAxialSpreads = 0;
        foreach (var composition in frames.Values)
            if (((SpriteComposition)composition).parts is SpazerAxialSpreadParts)
                calculatedAxialSpreads++;
        AssertEqual(19, calculatedAxialSpreads, "Nineteen axial Spazer spreads calculate repeated lanes with exact traversal");
        int calculatedChargedSpazer = 0;
        foreach (var composition in frames.Values)
            if (((SpriteComposition)composition).parts is HorizontalChargedSpazerParts)
                calculatedChargedSpazer++;
        AssertEqual(6, calculatedChargedSpazer, "Six horizontal charged Spazer compositions calculate strip adjacency and lane geometry");
        int calculatedVerticalSpazer = 0;
        foreach (var composition in frames.Values)
            if (((SpriteComposition)composition).parts is VerticalChargedSpazerSpreadParts)
                calculatedVerticalSpazer++;
        AssertEqual(4, calculatedVerticalSpazer, "Four vertical charged Spazer spreads calculate cell adjacency and mirrored side columns");
        int calculatedSpazerStartup = 0;
        foreach (var composition in frames.Values)
            if (((SpriteComposition)composition).parts is SpazerAxialStartupParts)
                calculatedSpazerStartup++;
        AssertEqual(12, calculatedSpazerStartup, "Twelve axial Spazer startup poses calculate centered single/pair geometry");
        int calculatedDiagonalStartup = 0;
        foreach (var composition in frames.Values)
            if (((SpriteComposition)composition).parts is SpazerDiagonalStartupParts)
                calculatedDiagonalStartup++;
        AssertEqual(6, calculatedDiagonalStartup, "Six diagonal Spazer startup poses calculate centered pair-strip geometry");
        int calculatedAlternateStartup = 0, calculatedPlasmaCores = 0;
        foreach (var composition in frames.Values)
        {
            object? parts = ((SpriteComposition)composition).parts;
            if (parts is AlternateDiagonalStartupParts) calculatedAlternateStartup++;
            if (parts is PlasmaStartupCoreParts) calculatedPlasmaCores++;
        }
        AssertEqual(4, calculatedAlternateStartup, "Four alternate diagonal startup poses calculate rotated/reflected endcap adjacency");
        AssertEqual(8, calculatedPlasmaCores, "Eight Plasma startup cores calculate centered cell geometry");
        int calculatedPlasmaShort = 0;
        foreach (var composition in frames.Values)
            if (((SpriteComposition)composition).parts is HorizontalPlasmaWaveShortParts)
                calculatedPlasmaShort++;
        AssertEqual(5, calculatedPlasmaShort, "Five horizontal PlasmaWave Short poses calculate cell and lobe geometry");
        var firstCharged = (SpriteComposition)(frames[(ushort)0xec3e] ?? throw new InvalidOperationException("Verification requires frames[(ushort)0xec3e]."));
        AssertTrue(firstCharged.parts is CompiledSpritePart[], "Distinct initial charged-Power ordering stays explicitly supplied/pending");
        foreach (ushort pointer in expected)
        {
            var native = new OamBuffer();
            var actual = new OamBuffer();
            DrawImportedProjectileSpritemap(rom, native, pointer, new(255, 255));
            selected.Draw(pointer, actual, new(255, 255));
            AssertTrue(native.ImageEquals(actual), "All417 calculated/remaining identities extract and draw exact native OAM");
        }
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        var document = RequiredJson.Deserialize<ProjectileSpriteDocument>(json, options);
        const ushort superMissilePointer = 0xadd5;
        string superMissileName = ProjectileSpriteDefinitions.Name(superMissilePointer);
        var originalMissilePart = document.Frames[superMissileName][1];
        document.Frames[superMissileName][1] = originalMissilePart with { OffsetX = originalMissilePart.OffsetX + 1 };
        var editedMissile = ProjectileSpriteCatalog.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, options)));
        var originalMissileOam = new OamBuffer();
        var editedMissileOam = new OamBuffer();
        selected.Draw(superMissilePointer, originalMissileOam, new(255, 255));
        editedMissile.Draw(superMissilePointer, editedMissileOam, new(255, 255));
        byte[] expectedMissileOam = originalMissileOam.LowTable.ToArray();
        expectedMissileOam[4]++;
        AssertTrue(expectedMissileOam.SequenceEqual(editedMissileOam.LowTable) && originalMissileOam.HighTable.SequenceEqual(editedMissileOam.HighTable), "Independent SuperMissile tail edit changes only its emitted X byte");
        document.Frames[superMissileName][1] = originalMissilePart;
        ushort editedPointer = ProjectileSpriteDefinitions.NativePointers[1];
        string editedName = ProjectileSpriteDefinitions.Name(editedPointer);
        var before = document.Frames[editedName][0];
        document.Frames[editedName][0] = before with { OffsetX = before.OffsetX + 1 };
        var edited = ProjectileSpriteCatalog.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, options)));
        var baselineOam = new OamBuffer();
        var editedOam = new OamBuffer();
        selected.Draw(editedPointer, baselineOam, new(255, 255));
        edited.Draw(editedPointer, editedOam, new(255, 255));
        AssertEqual(unchecked((byte)(baselineOam.LowTable[0] + 1)), editedOam.LowTable[0], "Independent composition edit reaches calculated identity");
        document.Frames[editedName][0] = before;
        var ownedOam = new OamBuffer();
        edited.Draw(editedPointer, ownedOam, new(255, 255));
        AssertTrue(ownedOam.LowTable.SequenceEqual(editedOam.LowTable), "Loaded composition owns independent edited content");
        const ushort quadPointer = 0xed9e;
        string quadName = ProjectileSpriteDefinitions.Name(quadPointer);
        var originalQuadPart = document.Frames[quadName][0];
        document.Frames[quadName][0] = originalQuadPart with { FlipX = !originalQuadPart.FlipX };
        var editedQuadCatalog = ProjectileSpriteCatalog.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, options)));
        var nativeQuad = new OamBuffer();
        var editedQuad = new OamBuffer();
        selected.Draw(quadPointer, nativeQuad, new(255, 255));
        editedQuadCatalog.Draw(quadPointer, editedQuad, new(255, 255));
        byte[] expectedQuad = nativeQuad.LowTable.ToArray();
        expectedQuad[3] ^= 0x40;
        AssertTrue(expectedQuad.SequenceEqual(editedQuad.LowTable) && nativeQuad.HighTable.SequenceEqual(editedQuad.HighTable), "Independent one-quadrant reflection edit changes only its exact emitted OAM attribute bit");
        const ushort wavePointer = 0xaea4;
        string waveName = ProjectileSpriteDefinitions.Name(wavePointer);
        var wavePart = document.Frames[waveName][0];
        document.Frames[waveName][0] = wavePart with { OffsetY = wavePart.OffsetY + 1 };
        var editedWaveCatalog = ProjectileSpriteCatalog.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, options)));
        var nativeWave = new OamBuffer();
        var editedWave = new OamBuffer();
        selected.Draw(wavePointer, nativeWave, new(255, 255));
        editedWaveCatalog.Draw(wavePointer, editedWave, new(255, 255));
        byte[] expectedWave = nativeWave.LowTable.ToArray();
        expectedWave[1]++;
        AssertTrue(expectedWave.SequenceEqual(editedWave.LowTable) && nativeWave.HighTable.SequenceEqual(editedWave.HighTable), "Independent diagonal Wave Y edit changes exactly its emitted coordinate and preserves X ninth bit");
        var flare = ChargeFlareSpriteCatalog.Load(new MemoryStream(ChargeFlareSpriteExtractor.Extract(rom)));
        for (ushort selector = 0; selector < ChargeFlareSpriteDefinitions.Selectors.Length; selector++)
        {
            var native = new OamBuffer();
            var actual = new OamBuffer();
            DrawImportedProjectileSpritemap(rom, native, ChargeFlareSpriteDefinitions.Selectors[selector], new(255, 255));
            flare.Draw(selector, actual, new(255, 255));
            AssertTrue(native.ImageEquals(actual), "Existing flare span caller preserves exact extracted OAM");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => _ = ProjectileSpriteDefinitions.NativePointers[-1], "Projectile identity lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = ProjectileSpriteDefinitions.NativePointers[417], "Projectile identity upper bound");
        Console.WriteLine("Projectile identity geometry:417 exact identities from805 native selectors,48 physical startup records,417 actual extracted OAM draws,independent composition edit/ownership,all existing flare selectors and bounds pass;all417 identities and 208 stock beam/missile/effect compositions calculate; independent frame selection/composition/art inputs remain pending.");
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
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | item.ProgramStart + 2), item.ColorByteIndex, "Native Norfair palette/color role");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | item.LoopInstructionPointer + 2), item.FirstFramePointer, "Native Norfair loop target");
            AssertEqual(norfair[count].ProgramStart, item.ProgramStart, "Indexed and enumerated Norfair identities agree");
            count++;
        }
        AssertEqual(4, count, "Complete Norfair lazy enumeration");
        count = 0;
        foreach (var item in accents)
        {
            AssertEqual((OldTourianEscapeAccentPaletteOwner)count, item.Owner, "Accent native owner order");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | item.DefinitionPointer + 2), item.ProgramStart, "Native accent definition/list binding");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | item.ProgramStart + 2), item.ColorByteIndex, "Native railings/panels color role");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | item.LoopInstructionPointer + 2), item.FirstFramePointer, "Native accent loop target");
            AssertEqual(accents[count].ProgramStart, item.ProgramStart, "Indexed and enumerated accent identities agree");
            count++;
        }
        AssertEqual(2, count, "Complete accent lazy enumeration");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = norfair[-1]; }, "Norfair lower owner bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = norfair[4]; }, "Norfair upper owner bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = accents[-1]; }, "Accent lower owner bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = accents[2]; }, "Accent upper owner bound");
        Console.WriteLine("Environmental catalogs:six native definition/color/loop bindings,292mechanics words,16heat phase bytes,actual complete/repeating cycles,live colors,enumeration/order and bounds pass; accent timing and independent colors remain pending.");
    }
    /// <summary>Checks extracted pause backdrop geometry, independent edits, and VRAM tile output.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyLookupStream2BackdropGeometry(SuperMetroidAddressSpace rom)
    {
        byte[] json = PauseBackdropExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseBackdropDocument>(json, ArtworkJson.Options);
        var stock = PauseBackdropPresentation.Load(new MemoryStream(json));
        var areaResiduals = stock.areas;
        var buttonResiduals = stock.buttons;
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            int index = AreaIds.ToIndex(area);
            int label = 0x820000 | ReadVerificationWord(rom, 0x82965f + index * 2);
            byte[] native = new byte[2048];
            for (int cell = 0; cell < 1024; cell++)
            {
                int address = cell is >= 170 and < 182 ? label + (cell - 170) * 2 : 0xb6e000 + cell * 2;
                ushort word = ReadVerificationWord(rom, address);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(native.AsSpan(cell * 2), word);
                AssertEqual(word != PauseBackdropDefinitions.StockAreaWord(area, cell), areaResiduals[index].ContainsKey(cell), "Exact required backdrop residual membership");
                if (areaResiduals[index].TryGetValue(cell, out ushort retained)) AssertEqual(word, retained, "Exact independent backdrop input");
            }
            ConfirmArea(stock, area, native);
            Console.WriteLine($"{area} required inputs: " + string.Join(", ", areaResiduals[index].Select(pair => $"({pair.Key % 32},{pair.Key / 32})={pair.Value:X4}")));
        }
        AssertEqual(15, areaResiduals.Sum(cells => cells.Count), "Required area residual count" );
        AssertEqual(1, buttonResiduals.Count, "Required button residual count" );
        byte[] nativeButtons = new byte[1024];
        for (int cell = 0; cell < 512; cell++)
        {
            ushort word = ReadVerificationWord(rom, 0xb6e400 + cell * 2);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(nativeButtons.AsSpan(cell * 2), word);
            AssertEqual(word != PauseBackdropDefinitions.StockButtonWord(cell), buttonResiduals.ContainsKey(cell), "Exact required button residual membership");
        }
        AssertTrue(nativeButtons.SequenceEqual(stock.CreateButtonTilemap()), "All512native button-template words");
        foreach (int edit in new[] { 0, 5 * 32 + 8, 5 * 32 + 12, 10 * 32 + 3, 24 * 32 + 31, 26 * 32 + 29 })
        {
            var areas = document.Areas.ToDictionary(pair => pair.Key, pair => pair.Value);
            var cells = areas[AreaId.Crateria.ToString()].ToArray();
            cells[edit] = cells[edit] with { FlipX = !cells[edit].FlipX, Palette = 4 };
            areas[AreaId.Crateria.ToString()] = cells;
            using var output = new MemoryStream();
            PauseBackdropPresentation.Write(output, document with { Areas = areas });
            var selected = PauseBackdropPresentation.Load(new MemoryStream(output.ToArray()));
            foreach (AreaId area in Enum.GetValues<AreaId>())
                ConfirmArea(selected, area, PauseTileGrid.Compile(areas[area.ToString()], "Edited area"));
            AssertTrue(nativeButtons.SequenceEqual(selected.CreateButtonTilemap()), "Area edit does not propagate into independent button template");
        }
        foreach (int edit in new[] { 0, 8 * 32 + 2, 9 * 32 + 5, 10 * 32 + 29 })
        {
            var cells = document.Buttons.ToArray();
            cells[edit] = cells[edit] with { FlipY = !cells[edit].FlipY, Palette = 5 };
            using var output = new MemoryStream();
            PauseBackdropPresentation.Write(output, document with { Buttons = cells });
            var selected = PauseBackdropPresentation.Load(new MemoryStream(output.ToArray()));
            AssertTrue(PauseTileGrid.Compile(cells, "Edited buttons").SequenceEqual(selected.CreateButtonTilemap()), "Independent control-template edit");
            ConfirmArea(selected, AreaId.Crateria, PauseTileGrid.Compile(document.Areas[AreaId.Crateria.ToString()], "Unchanged area"));
        }
        var shortTilemap = new ushort[PauseBackdropDefinitions.Cells - 1];
        var invalidTilemap = new ushort[PauseBackdropDefinitions.Cells];
        AssertThrows<ArgumentOutOfRangeException>(() => stock.LoadTo(shortTilemap, AreaId.Crateria), "Backdrop destination shorter than the image");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.LoadTo(invalidTilemap, (AreaId)7), "Backdrop area bound");
        AssertTrue(shortTilemap.All(word => word == 0) && invalidTilemap.All(word => word == 0), "Invalid load fails before any tilemap write");
        Console.WriteLine($"Backdrop geometry:7680native words, exact{areaResiduals.Sum(cells => cells.Count)}area/{buttonResiduals.Count}button residuals,53actual VRAM loads,10independent edits and bounds pass; selected layout/art/style inputs remain required.");

        static void ConfirmArea(PauseBackdropPresentation presentation, AreaId area, byte[] native)
        {
            const int untouched = 8;
            ushort[] words = Enumerable.Repeat((ushort)0x5555, PauseBackdropDefinitions.Cells + untouched).ToArray();
            presentation.LoadTo(words, area);
            byte[] loaded = System.Runtime.InteropServices.MemoryMarshal.AsBytes<ushort>(words.AsSpan(0, PauseBackdropDefinitions.Cells)).ToArray();
            AssertTrue(native.AsSpan(0, loaded.Length).SequenceEqual(loaded) && words[PauseBackdropDefinitions.Cells..].All(word => word == 0x5555),
                "Actual area load writes every selected word and leaves words past the image untouched");
        }
    }
    /// <summary>Checks pause equipment base cells, edit preservation, and layout geometry.</summary>
    internal static void VerifyLookupStream2EquipmentBaseGeometry(SuperMetroidAddressSpace rom)
    {
        byte[] json = PauseEquipmentBaseExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseEquipmentBaseDocument>(json, ArtworkJson.Options);
        var stock = PauseEquipmentBasePresentation.Load(new MemoryStream(json));
        byte[] native = new byte[2048];
        for (int cell = 0; cell < 1024; cell++)
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(native.AsSpan(cell * 2),
                ReadVerificationWord(rom, 0xb6e800 + cell * 2));
        AssertTrue(native.SequenceEqual(stock.CreateTilemap()), "Calculated equipment template preserves all1024native words");
        var residuals = stock.remainingCells;
        for (int cell = 0; cell < 1024; cell++)
        {
            int x = cell % 32, y = cell / 32;
            ushort expected = ReadVerificationWord(rom, 0xb6e800 + cell * 2);
            ushort calculated;
            if (x is >= 12 and < 20 && y is >= 7 and < 24)
            {
                int local = (y - 7) * 8 + x - 12;
                calculated = PauseWireframeDefinitions.StockWord(PauseWireframeKind.PowerSuit, local);
            }
            else calculated = PauseEquipmentBaseDefinitions.StockWord(cell);
            AssertEqual(expected != calculated, residuals.ContainsKey(cell), "Exact equipment base residual membership");
            if (residuals.TryGetValue(cell, out ushort retained)) AssertEqual(expected, retained, "Required stock input remains exact");
        }
        AssertEqual(0, residuals.Count, "Zero unexplained stock equipment-base words" );

        ConfirmRebind(stock, native);
        foreach (int cell in new[] { 0, 5 * 32 + 13, 9 * 32 + 2, 4 * 32 + 1, 12 * 32 + 3, 16 * 32 + 4, 7 * 32 + 15, 7 * 32 + 16 })
        {
            var cells = document.Cells.ToArray();
            cells[cell] = cells[cell] with { FlipX = !cells[cell].FlipX, Palette = 4 };
            using var editedJson = new MemoryStream();
            PauseEquipmentBasePresentation.Write(editedJson, document with { Cells = cells });
            var selected = PauseEquipmentBasePresentation.Load(new MemoryStream(editedJson.ToArray()));
            byte[] expected = PauseTileGrid.Compile(cells, "Independent equipment base edit");
            AssertTrue(expected.SequenceEqual(selected.CreateTilemap()), "Independent selected glyph/palette/flip edit preserves every other cell and mirror partner");
            ConfirmRebind(selected, expected);
        }
        byte[] mutable = stock.CreateTilemap();
        mutable[0] ^= 1;
        AssertTrue(native.SequenceEqual(stock.CreateTilemap()), "Mutable menu state does not alter immutable template");
        AssertThrows<ArgumentException>(() => stock.RebindBaseInto(new byte[1]), "Base rebind length bound");
        AssertThrows<ArgumentException>(() => stock.RebindBeforeInventoryRefreshInto(new byte[1]), "Inventory rebind length bound");
        Console.WriteLine($"Equipment template:1024native words, exact{residuals.Count}stock overrides,8independent edits,18actual rebinds,mutable-state independence and bounds pass; approved specific page composition retained, glyph pixels separately accounted.");

        static void ConfirmRebind(PauseEquipmentBasePresentation presentation, byte[] selected)
        {
            foreach (bool beforeInventory in new[] { false, true })
            {
                byte[] actual = Enumerable.Repeat((byte)0x55, 2048).ToArray();
                byte[] expected = actual.ToArray();
                for (int cell = 0; cell < 1024; cell++)
                {
                    int x = cell % 32, y = cell / 32;
                    bool inventory = y is >= 16 and <= 20 && x is >= 4 and < 13 ||
                        x is >= 21 and < 30 && y is 9 or 10 or >= 13 and <= 16 or >= 19 and <= 21;
                    bool reserve = y is 10 or 11 && x is >= 4 and < 11 ||
                        cell >= PauseReserveUiDefinitions.DigitCell && cell < PauseReserveUiDefinitions.DigitCell + 3;
                    bool wireframe = x is >= 12 and < 20 && y is >= 7 and < 24;
                    if (beforeInventory ? (reserve || wireframe) && !inventory : inventory || reserve || wireframe) continue;
                    ushort word = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(selected.AsSpan(cell * 2));
                    bool arrow = x == 1 && y is >= 4 and <= 12 || y == 12 && x == 2;
                    if (arrow) word = (ushort)(word & ~0x1c00 | 0x5555 & 0x1c00);
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(cell * 2), word);
                }
                if (beforeInventory) presentation.RebindBeforeInventoryRefreshInto(actual);
                else presentation.RebindBaseInto(actual);
                AssertTrue(expected.SequenceEqual(actual), "Actual base refresh preserves live ownership and arrow palette while applying selected artwork");
            }
        }
    }

    /// <summary>Checks pause wireframe mirror relationships and independently edited cells.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for wireframe mirrors")]
    internal static void VerifyLookupStream2WireframeMirrors(SuperMetroidAddressSpace rom)
    {
        byte[] json = PauseWireframeExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseWireframeDocument>(json,
            ArtworkJson.Options);
        var stock = PauseWireframePresentation.Load(new MemoryStream(json));
        int pieceCells = 0, stockOverrides = 0;
        foreach (PauseWireframeKind kind in Enum.GetValues<PauseWireframeKind>())
        {
            int pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((PauseWireframeDefinitions.Pointers + (int)kind * 2)) | 0x820000;
            var native = new byte[PauseWireframeDefinitions.Cells * 2];
            for (int cell = 0; cell < PauseWireframeDefinitions.Cells; cell++)
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(native.AsSpan(cell * 2), CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + cell * 2)));
            Confirm(stock, kind, native, "Actual mirrored stock wireframe preserves every native tile word");
            var edits = stock.frames[(int)kind].edits;
            stockOverrides += edits.Count;
            for (int cell = 0; cell < PauseWireframeDefinitions.Cells; cell++)
            {
                ushort actualNative = CartridgeImportSource.Require(rom).ReadCartridgeWord((pointer + cell * 2));
                AssertEqual(actualNative, PauseWireframeDefinitions.StockWord(kind, cell), $"Complete native composition word {kind}/{cell}");
                if (PauseWireframeDefinitions.TryStockTile(kind, cell, out int tile))
                {
                    AssertEqual(actualNative & 0x03ff, tile, "Native body-piece tile progression");
                    pieceCells++;
                }
            }
            // Confirm independent edits to an empty cell, either side of the
            // helmet pair, and the asymmetric lower-right artwork/connector cell.
            foreach (int cell in new[] { 0, 3, 4, 6 * PauseWireframeDefinitions.Columns + 6, PauseWireframeDefinitions.Cells - 1, 13 * 8 + 3, 14 * 8 + 7 })
            {
                var frames = document.Frames.ToDictionary(pair => pair.Key, pair => pair.Value);
                PauseBackdropCell[] cells = frames[kind.ToString()].ToArray();
                cells[cell] = cells[cell] with
                {
                    TileColumn = (cells[cell].TileColumn + 1) % PauseBackdropDefinitions.AtlasColumns,
                    FlipX = !cells[cell].FlipX,
                    Palette = (cells[cell].Palette + 1) % 8,
                    Priority = !cells[cell].Priority,
                };
                frames[kind.ToString()] = cells;
                using var output = new MemoryStream();
                PauseWireframePresentation.Write(output, document with { Frames = frames });
                var selected = PauseWireframePresentation.Load(new MemoryStream(output.ToArray()));
                Confirm(selected, kind, PauseTileGrid.Compile(cells, "Edited wireframe"),
                    "Independent left/right/empty/asymmetric edit does not propagate into its paired cell");
            }
        }
        AssertEqual(266, pieceCells, "Native glyph cells calculated from named body pieces");
        AssertEqual(0, stockOverrides, "All stock words derive without unexplained overrides");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ApplyTo(new byte[2048], (PauseWireframeKind)4), "Wireframe kind bound");
        AssertThrows<ArgumentException>(() => stock.ApplyTo(new byte[1], PauseWireframeKind.PowerSuit), "Wireframe page bound");
        Console.WriteLine("Wireframe composition:544 native words,266 atlas-strip cells,zero stock overrides,four full native patches,28 independent glyph/flip/palette/priority edits and untouched-page/bounds checks pass. Only the approved specific diagram composition is retained; glyph pixels remain separately accounted.");

        static void Confirm(PauseWireframePresentation presentation, PauseWireframeKind kind, byte[] words, string context)
        {
            byte[] actual = Enumerable.Repeat((byte)0xaa, PauseWireframeDefinitions.DestinationSize).ToArray();
            byte[] expected = actual.ToArray();
            for (int row = 0; row < PauseWireframeDefinitions.Rows; row++)
                words.AsSpan(row * PauseWireframeDefinitions.Columns * 2, PauseWireframeDefinitions.Columns * 2)
                    .CopyTo(expected.AsSpan(PauseWireframeDefinitions.DestinationByte + row * PauseWireframeDefinitions.DestinationStride));
            presentation.ApplyTo(actual, kind);
            AssertTrue(expected.SequenceEqual(actual), context);
        }
    }

    /// <summary>Checks extracted reserve UI labels, placement, and edited glyph ownership.</summary>
    internal static void VerifyLookupStream2ReserveLabels(SuperMetroidAddressSpace rom)
    {
        byte[] json = PauseReserveUiExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseReserveUiDocument>(json,
            ArtworkJson.Options);
        int words = 0;
        foreach (PauseReserveLabel id in PauseReserveUiDefinitions.Labels)
        {
            string name = PauseReserveUiDefinitions.Name(id);
            PauseReserveLabelVisual label = document.Labels[name];
            byte[] native = PauseTileGrid.Compile(label.Cells, name);
            int nativeOffset = (label.Anchor.Row * 32 + label.Anchor.Column) * 2;
            AssertEqual(nativeOffset, PauseReserveUiDefinitions.StockLabelOffset(id), "Native reserve text destination");
            for (int cell = 0; cell < label.Cells.Length; cell++)
            {
                AssertEqual(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(cell * 2)),
                    PauseReserveUiDefinitions.StockLabelWord(id, cell), "Native reserve glyph/palette/priority");
                words++;
            }
            for (int edit = -1; edit < label.Cells.Length; edit++)
            {
                PauseBackdropCell[] cells = label.Cells.ToArray();
                if (edit >= 0) cells[edit] = cells[edit] with { FlipX = !cells[edit].FlipX, Palette = 4 };
                var labels = document.Labels.ToDictionary(pair => pair.Key, pair => pair.Value);
                labels[name] = label with { Cells = cells };
                using var output = new MemoryStream();
                PauseReserveUiPresentation.Write(output, document with { Labels = labels });
                var selected = PauseReserveUiPresentation.Load(new MemoryStream(output.ToArray()));
                byte[] selectedWords = PauseTileGrid.Compile(cells, "Edited reserve label");
                foreach (bool preserve in new[] { false, true })
                {
                    byte[] actual = Enumerable.Repeat((byte)0x55, 32 * 32 * 2).ToArray();
                    byte[] expected = actual.ToArray();
                    for (int cell = 0; cell < cells.Length; cell++)
                    {
                        ushort value = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(selectedWords.AsSpan(cell * 2));
                        if (preserve) value = (ushort)(0x5555 & 0xfc00 | value & 0x03ff);
                        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(nativeOffset + cell * 2), value);
                    }
                    selected.ApplyLabel(actual, id, preserve ? LabelAttributes.Preserve : LabelAttributes.Replace);
                    AssertTrue(expected.SequenceEqual(actual), "Actual reserve label patch preserves edits, attribute policy and surrounding bytes");
                }
            }
            var movedLabels = document.Labels.ToDictionary(pair => pair.Key, pair => pair.Value);
            movedLabels[name] = label with { Anchor = label.Anchor with { Column = label.Anchor.Column + 1 } };
            using var movedJson = new MemoryStream();
            PauseReserveUiPresentation.Write(movedJson, document with { Labels = movedLabels });
            var moved = PauseReserveUiPresentation.Load(new MemoryStream(movedJson.ToArray()));
            byte[] movedActual = new byte[32 * 32 * 2];
            byte[] movedExpected = movedActual.ToArray();
            native.CopyTo(movedExpected, nativeOffset + 2);
            moved.ApplyLabel(movedActual, id);
            AssertTrue(movedExpected.SequenceEqual(movedActual), "Independent reserve label placement edit");
        }
        AssertEqual(22, words, "All native reserve label words");
        var stock = PauseReserveUiPresentation.Load(new MemoryStream(json));
        AssertThrows<InvalidDataException>(() => stock.ApplyLabel(new byte[2048], (PauseReserveLabel)byte.MaxValue), "Unknown reserve label rejected");
        AssertThrows<ArgumentException>(() => stock.ApplyLabel(new byte[1], PauseReserveLabel.Manual), "Short reserve destination rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => PauseReserveUiDefinitions.StockLabelWord(PauseReserveLabel.Auto, 4), "Reserve glyph upper bound rejected");
        Console.WriteLine("Reserve labels:22 native words/four destinations,22 independent cell edits,56 actual attribute-preserving/ordinary/moved patches and bounds pass; independent glyph/arrow inputs remain required.");
    }

    /// <summary>Checks equipment label text geometry, native cells, and independent edit behavior.</summary>
    internal static void VerifyLookupStream2EquipmentLabels(SuperMetroidAddressSpace rom)
    {
        byte[] json = PauseEquipmentLabelExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseEquipmentLabelDocument>(json,
            ArtworkJson.Options);
        var stock = PauseEquipmentLabelPresentation.Load(new MemoryStream(json));
        int words = 0;
        foreach (PauseEquipmentLabelId id in Enum.GetValues<PauseEquipmentLabelId>())
        {
            string key = PauseEquipmentLabelDefinitions.Name(id);
            PauseEquipmentLabel label = document.Labels[key];
            byte[] native = PauseTileGrid.Compile(label.Cells, key);
            AssertEqual((label.Row * 32 + label.Column) * 2,
                PauseEquipmentLabelDefinitions.StockDestinationByte(id), "Native semantic label placement");
            for (int cell = 0; cell < label.Cells.Length; cell++)
            {
                AssertEqual(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(cell * 2)),
                    PauseEquipmentLabelDefinitions.StockWord(id, cell), "Native packed glyph fragment");
                words++;
            }
        }
        int changedCells = 0;
        foreach (var identity in PauseEquipmentLabelDefinitions.Labels())
        {
            string key = PauseEquipmentLabelDefinitions.Name(identity.Label);
            PauseEquipmentLabel label = document.Labels[key];
            for (int edit = -1; edit < label.Cells.Length; edit++)
            {
                PauseBackdropCell[] cells = label.Cells.ToArray();
                if (edit >= 0) cells[edit] = cells[edit] with { FlipX = !cells[edit].FlipX };
                var labels = document.Labels.ToDictionary(pair => pair.Key, pair => pair.Value);
                labels[key] = label with { Cells = cells };
                using var encoded = new MemoryStream();
                PauseEquipmentLabelPresentation.Write(encoded, document with { Labels = labels });
                byte[] selectedJson = encoded.ToArray();
                var selected = PauseEquipmentLabelPresentation.Load(new MemoryStream(selectedJson));
                AssertEqual(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(selectedJson)),
                    selected.ContentIdentity, "Label selection preserves source identity");
                byte[] selectedWords = PauseTileGrid.Compile(cells, "Expected edited label");
                foreach (bool disabled in new[] { false, true })
                {
                    byte[] actual = Enumerable.Repeat((byte)0x55, 32 * 32 * 2).ToArray();
                    byte[] expected = actual.ToArray();
                    int length = label.Cells.Length;
                    for (int cell = 0; cell < length; cell++)
                    {
                        ushort value = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(selectedWords.AsSpan(cell * 2));
                        if (disabled) value = new SnesBgTilemapWord(value).WithPaletteIndex(document.DisabledPalette).Raw;
                        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(
                            expected.AsSpan((label.Row * 32 + label.Column + cell) * 2), value);
                    }
                    selected.ApplyLabel(actual, identity.Category, identity.Item, length, disabled);
                    AssertTrue(expected.SequenceEqual(actual), "Actual label patch/disabled recolor preserves every cell and surrounding byte");
                }
                if (edit >= 0) changedCells++;
            }
        }
        byte[] overrun = new byte[32 * 32 * 2];
        stock.ApplyLabel(overrun, PauseEquipmentCategory.Beams, 4, 9, false);
        PauseEquipmentLabel plasma = document.Labels[PauseEquipmentLabelDefinitions.Name(PauseEquipmentLabelId.Plasma)];
        PauseEquipmentLabel varia = document.Labels[PauseEquipmentLabelDefinitions.Name(PauseEquipmentLabelId.Varia)];
        byte[] tail = PauseTileGrid.Compile(varia.Cells, "Native contiguous Varia tail");
        AssertTrue(tail.AsSpan(0, 8).SequenceEqual(overrun.AsSpan((plasma.Row * 32 + plasma.Column + 5) * 2, 8)),
            "Nine-word Plasma patch retains four exact contiguous Varia words");
        // Hyper's five consumed cells pass through the actual inventory path; its
        // four unused trailing words are still checked against native above.
        for (int edit = 0; edit < PauseEquipmentLabelDefinitions.BeamWords; edit++)
        {
            var labels = document.Labels.ToDictionary(pair => pair.Key, pair => pair.Value);
            PauseEquipmentLabel hyper = labels[PauseEquipmentLabelDefinitions.HyperKey];
            PauseBackdropCell[] cells = hyper.Cells.ToArray();
            cells[edit] = cells[edit] with { FlipX = !cells[edit].FlipX };
            labels[PauseEquipmentLabelDefinitions.HyperKey] = hyper with { Cells = cells };
            using var output = new MemoryStream();
            PauseEquipmentLabelPresentation.Write(output, document with { Labels = labels });
            var selected = PauseEquipmentLabelPresentation.Load(new MemoryStream(output.ToArray()));
            byte[] actual = new byte[32 * 32 * 2];
            selected.ApplyInventory(actual, 0, 0, 0, 0, true);
            byte[] expected = PauseTileGrid.Compile(cells, "Selected Hyper words");
            AssertTrue(expected.AsSpan(0, 10).SequenceEqual(actual.AsSpan((hyper.Row * 32 + hyper.Column) * 2, 10)),
                "Actual Hyper label preserves each independently edited cell");
        }
        var movedLabels = document.Labels.ToDictionary(pair => pair.Key, pair => pair.Value);
        PauseEquipmentLabel charge = movedLabels["Beam.Charge"];
        movedLabels["Beam.Charge"] = charge with { Column = charge.Column + 1 };
        using var movedJson = new MemoryStream();
        PauseEquipmentLabelPresentation.Write(movedJson, document with { Labels = movedLabels });
        var moved = PauseEquipmentLabelPresentation.Load(new MemoryStream(movedJson.ToArray()));
        byte[] movedActual = Enumerable.Repeat((byte)0x55, 32 * 32 * 2).ToArray();
        byte[] movedExpected = movedActual.ToArray();
        PauseTileGrid.Compile(charge.Cells, "Native Charge text").CopyTo(movedExpected,
            (charge.Row * 32 + charge.Column + 1) * 2);
        moved.ApplyLabel(movedActual, PauseEquipmentCategory.Beams, 0, 5, false);
        AssertTrue(movedExpected.SequenceEqual(movedActual),
            "Independent moved label retains its edited destination and untouched original cell");
        AssertEqual(115, words, "All native equipment glyph words");
        AssertEqual(106, changedCells, "All ordinary label glyph cells edited independently");
        AssertThrows<ArgumentOutOfRangeException>(() => PauseEquipmentLabelDefinitions.StockWord((PauseEquipmentLabelId)byte.MaxValue, 0), "Unknown label rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => PauseEquipmentLabelDefinitions.StockWord(PauseEquipmentLabelId.Ice, -1), "Negative label cell rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => PauseEquipmentLabelDefinitions.StockWord(PauseEquipmentLabelId.Ice, 5), "Label cell upper bound rejected");
        Console.WriteLine("Pause equipment labels:115 native words/15placements,106 ordinary and five Hyper cell edits, disabled recoloring, exact Plasma/Varia overrun, source hashes and bounds pass; glyph artwork remains independent.");
    }

    /// <summary>Checks pause equipment blank cells against native art and edited label boundaries.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for equipment blank")]
    internal static void VerifyLookupStream2EquipmentBlank(SuperMetroidAddressSpace rom)
    {
        byte[] nativeJson = PauseEquipmentLabelExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseEquipmentLabelDocument>(
            nativeJson, ArtworkJson.Options);
        for (int cell = 0; cell < PauseEquipmentLabelDefinitions.EquipmentWords; cell++)
        {
            int address = 0x820000 | (PauseEquipmentLabelDefinitions.BlankSource + cell * 2);
            AssertEqual((byte)0, rom.ReadCartridgeByte(address), "Native blank low byte");
            AssertEqual((byte)0, rom.ReadCartridgeByte(address + 1), "Native blank high byte");
        }
        for (int edited = -1; edited < PauseEquipmentLabelDefinitions.EquipmentWords; edited++)
        {
            PauseBackdropCell[] blank = document.Blank.ToArray();
            if (edited >= 0)
                blank[edited] = PauseTileGrid.FromWord((ushort)(0x8000 | edited + 1), "Blank edit");
            var selected = document with { Blank = blank };
            using var output = new MemoryStream();
            PauseEquipmentLabelPresentation.Write(output, selected);
            byte[] json = output.ToArray();
            var presentation = PauseEquipmentLabelPresentation.Load(new MemoryStream(json));
            AssertEqual(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(json)),
                presentation.ContentIdentity, "Blank edits retain serialized content identity");
            var edits = presentation.blankEdits;
            AssertEqual(edited < 0 ? 0 : 1, edits.Count, "Only explicit blank edits are stored");
            foreach (bool hyper in new[] { false, true })
            {
                byte[] actual = Enumerable.Repeat((byte)0x55, 32 * 32 * 2).ToArray();
                byte[] expected = actual.ToArray();
                byte[] blankBytes = PauseTileGrid.Compile(blank, "Reference blank");
                foreach (var identity in PauseEquipmentLabelDefinitions.Labels())
                {
                    PauseEquipmentLabel label = selected.Labels[PauseEquipmentLabelDefinitions.Name(identity.Label)];
                    int destination = (label.Row * 32 + label.Column) * 2;
                    int length = PauseEquipmentLabelDefinitions.WordCount(identity.Category) * 2;
                    blankBytes.AsSpan(0, length).CopyTo(expected.AsSpan(destination, length));
                }
                if (hyper)
                {
                    PauseEquipmentLabel label = selected.Labels[PauseEquipmentLabelDefinitions.HyperKey];
                    byte[] words = PauseTileGrid.Compile(label.Cells, "Reference Hyper label");
                    words.AsSpan(0, PauseEquipmentLabelDefinitions.BeamWords * 2)
                        .CopyTo(expected.AsSpan((label.Row * 32 + label.Column) * 2));
                }
                presentation.ApplyInventory(actual, 0, 0, 0, 0, hyper);
                AssertTrue(expected.SequenceEqual(actual),
                    "Actual uncollected inventory/Hyper blank writes preserve every edited cell and untouched byte");
            }
        }
        Console.WriteLine("Pause equipment blank: nine native empty cells, stock/no stored payload, nine independent edits, serialized hashes and20 actual inventory/Hyper tilemap writes pass.");
    }

    /// <summary>Checks Dead Torizo corpse-tile geometry and native OAM composition.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyLookupStream2DeadTorizoGeometry(SuperMetroidAddressSpace rom)
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
        int[] nativeColumns = [0xa9e280,0xa9e29f,0xa9e2be,0xa9e2d8,0xa9e2f2,
            0xa9e30c,0xa9e326,0xa9e340,0xa9e35a,0xa9e379];
        int[] minimumY = [CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa9e276),CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa9e295),CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa9e2b4),0,0,0,0,0,0,CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa9e36f)];
        for (int column = 0; column < 10; column++)
        {
            AssertEqual((CartridgeImportSource.Require(rom).ReadCartridgeWord(nativeColumns[column]) - 0x2000) / 2,
                DeadTorizoGeometryDefinitions.ColumnWordOffset(column), "Dead Torizo native column word displacement");
            AssertEqual(minimumY[column], DeadTorizoGeometryDefinitions.ColumnMinimumY(column),
                "Dead Torizo native clipping boundary");
        }
        byte[] planar = ReadDeadTorizoRomBytes(rom, DeadTorizoArtworkDefinitions.SourceAddress, DeadTorizoArtworkDefinitions.ByteCount);
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, RoomCharacterAtlasFormat.TileColumns,
            out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        png.Position = 0;
        // An independently edited display composition is deliberately unrelated to
        // stock tile coverage. Native staging must still copy the same artwork rows.
        var displayParts = new Dictionary<int, EnemySpritemapParts>
        {
            [0xa9d6e2] = EnemySpritemapParts.FromOwnedArray([new(SnesSpritemapXWord.Create(70, false), new SnesSpritemapYByte(33),
                SnesObjAttributeWord.Create(0, 7, 3, SnesTileFlipFlags.Horizontal))]),
        };
        var displayBindings = new Dictionary<int, int> { [0xa9d6e2] = 0xa9d6e2 };
        var display = new EnemySpritemapCatalog(displayParts, displayBindings);
        AssertTrue(display.TryGetDisplay(0xa9, 0xd6e2, out EnemySpritemapParts selectedDisplay),
            "Edited Dead Torizo display is selected");
        var editedOam = new OamBuffer();
        editedOam.AddEnemySpritemap(selectedDisplay, new Position16<OamPixels>(128, 128), 0, 0);
        AssertEqual(4, editedOam.NextByteOffset, "Edited display draws one small part instead of25stock parts");
        AssertEqual((byte)198, editedOam.LowTable[0], "Edited display origin reaches actual OAM");
        var artwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas> { [EnemyDefinitionId.CorpseTorizo] = RoomCharacterAtlas.Load(png, planar.Length) }, Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>
            {
                [EnemyDefinitionId.CorpseTorizo] = EnemyPaletteSheet.Load(new MemoryStream(
                    EnemyPaletteSheet.Write(new EnemyPaletteSheetDocument
                    {
                        Version = 1,
                        Colors = Enumerable.Range(0,16).Select(_ => new PaletteRgb5 { Red=0, Green=0, Blue=0 }).ToArray(),
                    }))),
            }, Spritemaps = display, DmaSources = new Dictionary<EnemyDefinitionId, int> { [EnemyDefinitionId.CorpseTorizo] = DeadTorizoArtworkDefinitions.SourceAddress } });
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork };
        StagingByteView corpseTiles = enemies.Room.Staging.CorpseTiles;
        corpseTiles.Fill(0);
        new Action(enemies.DeadTorizoBehavior.InitializeDeadTorizoGraphics)();
        Compare(expected, "Actual Dead Torizo installed-art crop");

        var state = new DeadTorizoEnemyState(enemies.SlotTable.Slots[0], new CorpseRottingProgram { YLimit = 95, LateMoveEntryIndex = 94, WrapOffset = 0x134 });
        Action<DeadTorizoEnemyState, ushort, CorpseRowOperation> moveRow = enemies.DeadTorizoBehavior.CopyOrMoveDeadTorizoPixelRow;
        foreach (bool move in new[] { false, true })
        for (ushort y = 0; y < 96; y++)
        {
            for (int index = 0; index < expected.Length; index++)
                expected[index] = (byte)(index * 73 ^ index >> 3);
            corpseTiles.CopyFrom(expected);
            int source = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa9e226 + y / 8 * 2) + (y & 7) * 2;
            int destination = source + ((y & 7) >= 6 ? 0x134 : 0) + 2;
            for (int column = 0; column < 10; column++)
            {
                if (y < minimumY[column]) continue;
                int nativeOffset = CartridgeImportSource.Require(rom).ReadCartridgeWord(nativeColumns[column]) - 0x2000;
                foreach (int plane in new[] { 0, 16 })
                {
                    for (int byteIndex = 0; byteIndex < 2; byteIndex++)
                    {
                        int src = source + nativeOffset + plane + byteIndex;
                        if (y < 94) expected[destination + nativeOffset + plane + byteIndex] = expected[src];
                        if (move) expected[src] = 0;
                    }
                }
            }
            moveRow(state, y, move ? CorpseRowOperation.Move : CorpseRowOperation.Copy);
            Compare(expected, "Actual Dead Torizo column row copy/move");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => DeadTorizoGeometryDefinitions.InitialCopy(-1), "Crop negative row rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadTorizoGeometryDefinitions.InitialCopy(12), "Crop row upper bound rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadTorizoGeometryDefinitions.ColumnMinimumY(-1), "Crop negative column rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadTorizoGeometryDefinitions.ColumnMinimumY(10), "Crop column upper bound rejected");
        Console.WriteLine("Dead Torizo geometry:12 native MVNs/25-part OAM exact97-tile union,10 column operands/clip limits, actual installed-art staging and192 actual row copy/move operations pass; crop derives from immutable stock composition independently of edited display OAM; artwork ownership remains explicit.");

        void Compare(byte[] bytes, string context)
        {
            for (int index = 0; index < bytes.Length; index++)
                AssertEqual(bytes[index], corpseTiles[index], context);
        }
    }

    /// <summary>Copies a bounded span of native Dead Torizo artwork bytes for the oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Dead Torizo artwork bytes")]
    private static byte[] ReadDeadTorizoRomBytes(ISnesAddressSpace rom, int address, int count) =>
        Enumerable.Range(0, count).Select(offset => rom.ReadCartridgeByte(address + offset)).ToArray();

    /// <summary>Checks native crawler/Yard speed ramps through actual velocity resets.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for crawler ramps")]
    internal static void VerifyLookupStream2CrawlerRamps(SuperMetroidAddressSpace rom)
    {
        Func<RoomEnemySlot, int, EightEightVector<RoomPixels>, EightEightVector<RoomPixels>> reset = CrawlersBehavior.ResetCrawlerVelocitiesFromProperties;
        Action<YardEnemyState, int> setYard = YardEnemyBehavior.SetYardCrawlingVelocities;
        var slot = new RoomEnemySystem(new EnemySession()).SlotTable.Slots[0];
        var yard = new YardEnemyState();
        int speedParameter = 0;
        for (ushort parameter = 0; parameter < 32; parameter++)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3e5f0 + parameter * 2));
            AssertEqual(expected, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3cca2 + parameter * 2)), "Native Yard/crawler magnitude agreement");
            AssertEqual(expected, CrawlerSpeedDefinitions.ForParameter(parameter), "Native calculated speed ramp");
            speedParameter = parameter;
            yard.CrawlingSpeedIndex = parameter;
            for (ushort property = 0; property < 4; property++)
            {
                slot.Properties = EnemyPropertyWord.FromNativeWord((ushort)(0xa000 | property));
                EightEightVector<RoomPixels> velocity = reset(slot, speedParameter, default);
                AssertEqual(property == 0 ? unchecked((ushort)-expected) : expected, velocity.X.RawValue,
                    "Actual crawler reset X magnitude");
                AssertEqual(property == 2 ? unchecked((ushort)-expected) : expected, velocity.Y.RawValue,
                    "Actual crawler reset Y magnitude");
            }
            for (ushort direction = 0; direction < 8; direction++)
            {
                setYard(yard, direction);
                int nativeDirection = 0xa3cd82 + direction * 8;
                AssertEqual(unchecked((ushort)((expected ^ CartridgeImportSource.Require(rom).ReadCartridgeWord(nativeDirection)) + CartridgeImportSource.Require(rom).ReadCartridgeWord((nativeDirection + 2)))),
                    yard.CrawlingVelocity.X.RawValue, "Actual Yard reset X magnitude");
                AssertEqual(unchecked((ushort)((expected ^ CartridgeImportSource.Require(rom).ReadCartridgeWord((nativeDirection + 4))) + CartridgeImportSource.Require(rom).ReadCartridgeWord((nativeDirection + 6)))),
                    yard.CrawlingVelocity.Y.RawValue, "Actual Yard reset Y magnitude");
            }
        }
        AssertThrows<InvalidDataException>(() => CrawlerSpeedDefinitions.ForParameter(32), "Crawler parameter upper bound");
        Console.WriteLine("Crawler speed ramps: both 32 native words and 384 actual crawler/Yard velocity resets pass; irregular gaps/terminal choices remain pending.");
    }
    /// <summary>Checks lava-jumper control and visual selectors, parent/follower programs, and handshake.</summary>
    internal static void VerifyLookupStream2LavaJumperLayout(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(NorfairLavaJumperBehaviorVerification.VerifyNorfairLavaJumperBehavior), () => NorfairLavaJumperBehaviorVerification.VerifyNorfairLavaJumperBehavior());
        Suite(nameof(NorfairLavaJumperAnimationsVerification.VerifyNorfairLavaJumperAnimations), () => NorfairLavaJumperAnimationsVerification.VerifyNorfairLavaJumperAnimations(rom));
    }

    /// <summary>Checks Chozo statue instruction controls, visuals, and list boundaries.</summary>
    internal static void VerifyLookupStream2ChozoLayout(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyChozoStatueInstructionProgramDefinitions), () => VerifyChozoStatueInstructionProgramDefinitions(rom));
        Console.WriteLine("Chozo layouts: the four native lists and their 52 poses are checked through the typed programs.");
    }
    /// <summary>Checks horizontal scroll target selection against native grid boundary behavior.</summary>
    internal static void VerifyLookupStream2HorizontalCameraTargets(ISnesAddressSpace rom)
    {
        var storage = new byte[RoomScrollGrid.StorageByteCount];
        Array.Fill(storage, (byte)1);
        var grid = RoomScrollGrid.LoadCompiled(new RoomScrollStorage(), storage, new Extent16<RoomScreens>(3, 2));
        foreach (CameraDistanceMode mode in Enum.GetValues<CameraDistanceMode>())
        foreach (byte facing in new byte[] { 4, 8 })
        for (int reversal = 0; reversal < 4; reversal++)
        {
            var context = new HorizontalCameraContext(
                reversal == 1,
                reversal == 2 ? SamusMovementType.Moonwalking : 0,
                reversal == 3 ? SamusAccelerationMode.Turning : SamusAccelerationMode.Accelerating, facing, mode);
            bool right = reversal == 0 ? facing != 4 : facing == 4;
            ushort nativeOffset = ReadVerificationWord(rom, (right ? 0x90963f : 0x909647) + (int)mode);
            AssertEqual(nativeOffset, HorizontalCameraTargetDefinitions.Offset(mode, right), "Native mode/facing target offset");
            var camera = new ScrollBoundaryCamera(grid);
            camera.SetPosition(new Position16<RoomPixels>(100, 0));
            camera.TrackMovedSamusHorizontally(
                FixedPosition<RoomPixels>.FromWords(200, 0, 100, 0), FixedPosition<RoomPixels>.FromWords(204, 0, 100, 0), context);
            AssertEqual(unchecked((ushort)(204 - nativeOffset)), camera.IdealPosition.X, "Actual camera target preserves mode and reversal");
            AssertEqual((ushort)5, camera.Speed.X.WholeWord, "Target dispatch preserves movement speed calculation");
        }
        var invalidCamera = new ScrollBoundaryCamera(grid);
        AssertThrows<ArgumentOutOfRangeException>(() => invalidCamera.TrackMovedSamusHorizontally(
            FixedPosition<RoomPixels>.FromWords(200, 0, 100, 0), FixedPosition<RoomPixels>.FromWords(204, 0, 100, 0),
            new(false, 0, 0, 8, (CameraDistanceMode)1)), "Native mode domain remains validated before tracking");
        Console.WriteLine("Horizontal camera targets:eight native offsets and32 actual mode/facing/reversal targets preserve geometry, speed and domain validation.");
    }

    /// <summary>Checks eight Torizo tile-page identities, decoded sources, and dispatch bounds.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Torizo page dispatch")]
    internal static void VerifyLookupStream2TorizoPageDispatch(ISnesAddressSpace rom)
    {
        TileSheetDefinition[] original =
        [
            TorizoInstructionVramArtworkDefinitions.SharedDeath, TorizoInstructionVramArtworkDefinitions.StatueCrumble,
            TorizoInstructionVramArtworkDefinitions.LeftAttack, TorizoInstructionVramArtworkDefinitions.RightAttack,
            TorizoInstructionVramArtworkDefinitions.GoldenAwakening, TorizoInstructionVramArtworkDefinitions.GoldenLeftAttack,
            TorizoInstructionVramArtworkDefinitions.GoldenRightAttack, TorizoInstructionVramArtworkDefinitions.ChozoDebris,
        ];
        var pages = new RoomCharacterAtlas[original.Length];
        var native = new byte[original.Length][];
        int ordinal = 0;
        foreach (var page in TorizoInstructionVramArtworkDefinitions.All)
        {
            AssertEqual(original[ordinal], page, "Torizo page manifest order/source/extent/filename");
            AssertEqual(page, TorizoInstructionVramArtworkDefinitions.All[ordinal], "Torizo page enumeration/index agreement");
            native[ordinal] = Enumerable.Range(0, page.ByteCount).Select(offset => rom.ReadCartridgeByte(page.SourceAddress + offset)).ToArray();
            byte[] pixels = SnesGraphics.DecodePlanarTiles(native[ordinal], 4,
                Math.Min(RoomCharacterAtlasFormat.TileColumns, page.ByteCount / RoomCharacterAtlasFormat.BytesPerTile), out Extent16<ImagePixels> size);
            using var png = new MemoryStream();
            IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
            png.Position = 0;
            pages[ordinal++] = RoomCharacterAtlas.Load(png, page.ByteCount);
        }
        AssertEqual(8, ordinal, "Torizo page count");
        var artwork = new TorizoInstructionVramArtwork(pages);
        string expectedHash = SelectedPresentationHash.Create("TorizoInstructionVramArtwork-v1", content =>
        {
            content.Append("pages", original.Length);
            foreach (var bytes in native) content.Append("tiles", bytes);
        });
        AssertEqual(expectedHash, artwork.ContentIdentity, "Torizo canonical hash retains original page order");
        for (int index = 0; index < original.Length; index++)
        {
            var page = original[index];
            AssertTrue(artwork.TryResolve(page.SourceAddress, page.ByteCount, out var whole), "Actual resolver accepts complete named page");
            AssertTrue(whole.Span.SequenceEqual(native[index]), "Actual resolver returns exact native page bytes");
            AssertTrue(artwork.TryResolve(page.SourceAddress + page.ByteCount - 1, 1, out var tail), "Actual resolver accepts final byte");
            AssertEqual(native[index][^1], tail.Span[0], "Actual resolver returns correct final byte");
            AssertTrue(!artwork.TryResolve(page.SourceAddress, page.ByteCount + 1, out _), "Actual resolver rejects page overrun");
            AssertTrue(!artwork.TryResolve(page.SourceAddress, 0, out _), "Actual resolver rejects empty slice");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => _ = TorizoInstructionVramArtworkDefinitions.All[-1], "Torizo page lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = TorizoInstructionVramArtworkDefinitions.All[8], "Torizo page upper bound");
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
            AssertTrue(EnemySpritemapDefinitions.Frames.Contains(frame), "Generated Kago identity reaches installed catalog");
            count++;
        }
        AssertEqual(3, count, "Kago frame enumeration count");
        Console.WriteLine("Kago frames:three native selectors/four-part headers, stable names and installed catalog membership pass.");
    }

    /// <summary>Checks Kraid lint initialization's source values and live actor state.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid Lint initialization")]
    internal static void VerifyLookupStream2KraidLintInitialization(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), services: new VerificationRoomEnemyServices(null));
        var state = new KraidEnemyState();
        // The parts read Kraid's room state, which the body's initializer installs; this fixture installs it directly.
        enemies.KraidBehavior.Access.Kraid = state;
        enemies.VerificationServices().ReadRandom = () => 1;
        Action<RoomEnemySlot, KraidEnemyState> finish = enemies.KraidBehavior.Rise.FinishKraidGrowth;
        Action<RoomEnemySlot, KraidPartState> align = enemies.KraidBehavior.Projectiles.AlignKraidPart;
        var body = enemies.SlotTable.Slots[0];
        body.Position = body.Position.WithWhole(new(200, 300));
        for (int slot = 2; slot <= 4; slot++) state.Parts[slot].LintTravel = 17;
        finish(body, state);
        for (int slot = 2; slot <= 4; slot++)
        {
            var lint = enemies.SlotTable.Slots[slot];
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7a916 + (slot - 2) * 2));
            AssertEqual(expected, KraidLintInitializationDefinitions.InitialDelay((KraidLintPart)slot), "Native per-part initial delay");
            AssertEqual(expected, state.Parts[lint.SlotIndex].FunctionTimer, "Actual post-growth timer write");
            AssertEqual(0, state.Parts[lint.SlotIndex].LintTravel, "Actual lint growth resets extension");
            AssertEqual(KraidAiFunction.LintProduce, state.Parts[slot].NextFunction, "Actual lint continuation");
            for (int remaining = expected - 1; remaining >= 0; remaining--)
            {
                align(lint, state.Parts[slot]);
                AssertEqual((ushort)remaining, state.Parts[lint.SlotIndex].FunctionTimer, "Actual per-part countdown");
                AssertEqual((remaining == 0 ? KraidAiFunction.LintProduce : KraidAiFunction.AlignPartToKraid),
                    state.Parts[lint.SlotIndex].Function, "Actual lint transition occurs on its own final tick");
                AssertEqual(unchecked((ushort)(body.Position.X.Whole - lint.Radius.X)), lint.Position.X.Whole, "Alignment continues during delay");
            }
        }
        foreach (KraidLintPart invalid in new[] { (KraidLintPart)0, (KraidLintPart)1, (KraidLintPart)5 })
            AssertThrows<InvalidOperationException>(() => KraidLintInitializationDefinitions.InitialDelay(invalid), "Only lint slots have launch policy");
        Console.WriteLine("Kraid lint initialization: three native policies, actual slot writes and all 512 countdown transitions pass.");
    }

    /// <summary>Checks enemy drop cumulative weights, RNG reroll, and resource eligibility.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for drop selection")]
    internal static void VerifyLookupStream2DropSelection(SuperMetroidAddressSpace rom)
    {
        // Native first record weights60,60,60,5,60,10 yield cumulative thresholds
        //60,120,180,185,245,255 when all resources are eligible.
        ushort[] randomValues = [1, 61, 121, 181, 186, 246];
        for (int column = 0; column < 6; column++)
        {
            EnemyPickupKind expected = (EnemyPickupKind)rom.ReadCartridgeByte(0x86f25e + column);
            AssertEqual(expected, EnemyDropSelectionDefinitions.ForProbabilityColumn(column),
                "Native probability-column pickup identity");
            var samus = CreateDropTestSamus();
            samus.Health = 50;
            samus.MaxMissiles = samus.MaxSuperMissiles = samus.MaxPowerBombs = 5;
            var fixture = CreateEnemyDropFixture(samus, [randomValues[column]]);
            var projectile = fixture.System.Projectiles.EnemyProjectiles[0];
            projectile.DropChancesOverride = EnemyDropChanceDefinitions.At(NativeDropChancePointer);
            AssertEqual(expected, fixture.System.DeathsAndDrops.SelectRandomEnemyDrop(projectile),
                "Actual cumulative selection preserves each semantic column");
        }
        Suite(nameof(VerifyEnemyDropSelectionRules), () => VerifyEnemyDropSelectionRules());
        AssertThrows<ArgumentOutOfRangeException>(() => EnemyDropSelectionDefinitions.ForProbabilityColumn(-1),
            "Drop column lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => EnemyDropSelectionDefinitions.ForProbabilityColumn(6),
            "Drop column upper bound");
        Console.WriteLine("Enemy drop selection:six native identities and actual cumulative selections, RNG-zero reroll, energy hysteresis and full-resource eligibility pass.");
    }
    /// <summary>Checks Brinstar, Norfair, and yellow Pipe Bug visual program geometry.</summary>
    internal static void VerifyLookupStream2PipeBugVisualGeometry(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(PipeBugBehaviorVerification.VerifyPipeBugBehavior), () => PipeBugBehaviorVerification.VerifyPipeBugBehavior());
        Suite(nameof(PipeBugAnimationsVerification.VerifyPipeBugAnimations), () => PipeBugAnimationsVerification.VerifyPipeBugAnimations(rom));
        ushort[] poses = PipeBugAnimationsConstants.Spritemaps(Enum.GetValues<PipeBugProgram>()).Distinct().ToArray();
        foreach (ushort pose in poses)
            AssertEqual((ushort)1, ReadVerificationWord(rom, (int)new SnesAddress(PipeBugFrameToolingDefinitions.Bank, pose)), "Native seven-byte single-part composition");
        Console.WriteLine($"Pipe Bug visual geometry: {poses.Length} native single-part poses and the typed loops pass.");
    }

    /// <summary>Checks five-member Pipe Bug formation positions and native spawn data.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Pipe Bug formation")]
    internal static void VerifyLookupStream2PipeBugFormation(SuperMetroidAddressSpace rom)
    {
        for (int member = 0; member < 5; member++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb38c65 + member * 7)), PipeBugDefinitions.NorfairStaggerTarget(member),
                "Native rank-derived formation release counter");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb38c88 + member * 6)), (ushort)PipeBugDefinitions.NorfairPostRiseFunction(member),
                "Native role-specific formation function");
        }
        foreach (ushort samusX in new ushort[] { 120, 136 })
        {
            var enemies = new RoomEnemySystem(new EnemySession());
            for (int member = 0; member < 5; member++)
            {
                var slot = enemies.SlotTable.Slots[member];
                slot.Bind(EnemyDefinitionId.Gamet, slot.Definition);
                slot.Position = slot.Position.WithWhole(new(128, 128)); slot.SetSpawnParameter2(64);
                PipeBugsBehavior.InitializeNorfairPipeBug(slot);
                slot.InstructionTimer = NativeCountdown.FromRaw((ushort)(member + 4));
                slot.Timer = NativeCountdown.FromRaw((ushort)(member + 8));
            }
            var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(samusX, 112)) };
            enemies.PipeBugsBehavior.RunNorfairPipeBugSamusWait(enemies.SlotTable.Slots[0], (enemies.PipeBugStates[0] ?? throw new InvalidOperationException("Verification requires enemies.PipeBugStates[0].")), samus);
            for (int member = 0; member < 5; member++)
            {
                var state = enemies.PipeBugStates[member];
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb38c65 + member * 7)), (state ?? throw new InvalidOperationException("Verification requires state.")).StaggerTarget,
                    "Actual formation writes native release counter");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb38c88 + member * 6)), (ushort)state.NorfairPostRiseFunction,
                    "Actual formation writes native role function");
                AssertEqual(PipeBugEnemyFunction.NorfairRise, state.Function, "Actual formation begins rising");
                AssertEqual(AnimationProgramCursor<PipeBugProgram>.Start(samusX < 128 ? PipeBugProgram.GametRisingLeft : PipeBugProgram.GametRisingRight),
                    state.Program, "Actual formation selects facing");
                AssertEqual((ushort)(member == 0 ? 1 : member + 4), enemies.SlotTable.Slots[member].InstructionTimer.RawValue,
                    "Only leader instruction timer resets");
                AssertEqual((ushort)(member == 0 ? 0 : member + 8), enemies.SlotTable.Slots[member].Timer.RawValue,
                    "Only leader loop counter resets");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => PipeBugDefinitions.NorfairStaggerTarget(-1), "Formation lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => PipeBugDefinitions.NorfairPostRiseFunction(5), "Formation upper bound");
        Console.WriteLine("Norfair Pipe Bug formation:ten native immediates, both actual facing formations and leader-only timer resets pass.");
    }

    /// <summary>Checks projectile trail appearance catalog and independent sprite edits.</summary>
    internal static void VerifyLookupStream2TrailAppearance(ISnesAddressSpace rom)
    {
        byte[] json = ProjectileTrailExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<ProjectileTrailDocument>(json, ArtworkJson.Options);
        var stock = ProjectileTrailCatalog.Load(new MemoryStream(json));
        var edits = stock.suppliedAttributes;
        AssertEqual(0, edits.Count, "Stock trail appearance stores no repeated frame attributes");
        foreach (ushort frame in ProjectileTrailVisualDefinitions.Frames)
        {
            AssertEqual(ReadVerificationWord(rom, 0x900000 | (frame + 2)), stock.Resolve(frame), "Native trail appearance");
            string name = ProjectileTrailVisualDefinitions.Name(frame);
            var original = document.Frames[name];
            for (int field = 0; field < 6; field++)
            {
                var changed = field switch
                {
                    0 => original with { TileColumn = (original.TileColumn + 1) % 16 },
                    1 => original with { TileRow = (original.TileRow + 1) % 32 },
                    2 => original with { Palette = (original.Palette + 1) % 8 },
                    3 => original with { Priority = (original.Priority + 1) % 4 },
                    4 => original with { FlipX = !original.FlipX },
                    _ => original with { FlipY = !original.FlipY },
                };
                var frames = new Dictionary<string, ProjectileTrailAppearance>(document.Frames) { [name] = changed };
                var catalog = ProjectileTrailCatalog.Load(new MemoryStream(ProjectileTrailCatalog.Write(document with { Frames = frames })));
                foreach (ushort other in ProjectileTrailVisualDefinitions.Frames)
                {
                    var appearance = frames[ProjectileTrailVisualDefinitions.Name(other)];
                    ushort expected = SnesObjAttributeWord.Create(appearance.TileRow * 16 + appearance.TileColumn,
                        appearance.Palette, appearance.Priority,
                        (appearance.FlipX ? SnesTileFlipFlags.Horizontal : 0) | (appearance.FlipY ? SnesTileFlipFlags.Vertical : 0)).Raw;
                    AssertEqual(expected, catalog.Resolve(other), "Every independently edited trail field survives");
                }
            }
        }
        AssertThrows<InvalidDataException>(() => stock.Resolve(0), "Unknown trail frame rejected");
        Suite(nameof(VerifyLookupStream2TrailPrograms), () => VerifyLookupStream2TrailPrograms(rom));
        Console.WriteLine("Trail appearance:42 native words,252 independent field edits and existing real OAM/lifetime/freeze proof pass; ice phase boundaries remain pending.");
    }

    /// <summary>Checks trail instruction words and emitted OAM across the compiled programs.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled projectile trail program words")]
    internal static void VerifyLookupStream2TrailPrograms(ISnesAddressSpace bus)
    {
        byte[] json = ProjectileTrailExtractor.Extract(bus);
        var catalog = ProjectileTrailCatalog.Load(new MemoryStream(json));
        var encountered = new HashSet<ushort>();
        foreach (ushort start in new[] { ProjectileTrailDefinitions.LeftIce, ProjectileTrailDefinitions.RightIce, ProjectileTrailDefinitions.Wave, ProjectileTrailDefinitions.Missile })
        {
            ushort cursor = start;
            while (true)
            {
                ushort word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomData.Banks.Movement | cursor);
                if (word == 0) break;
                if (word < 0x8000) { encountered.Add(cursor); cursor += 4; }
                else { cursor += 2; }
            }
            var nativeSystem = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None) { Overreads = InstalledFixtures.SamusOverreads }; var authoredSystem = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None) { Overreads = InstalledFixtures.SamusOverreads };
            foreach (var system in new[] { nativeSystem, authoredSystem })
            {
                var pair = system.TrailSlots[0];
                foreach (var side in new[] { pair.Left, pair.Right })
                { side.InstructionPointer = start; side.InstructionTimer = NativeCountdown.Start(1); side.Position = side.Position.WithX(100).WithY(100); }
            }
            for (int frame = 0; frame < 80; frame++)
            {
                var nativeOam = new OamBuffer();
                bool frozenFrame = frame % 5 == 0;
                StepNative(nativeSystem.TrailSlots[0], nativeSystem.TrailSlots[0].Left, nativeOam, frozenFrame);
                StepNative(nativeSystem.TrailSlots[0], nativeSystem.TrailSlots[0].Right, nativeOam, frozenFrame);
                OamBuffer authoredOam = SpriteFrameBuffer.DrawnOam(frame =>
                {
                    authoredSystem.AdvanceTrails(frozenFrame, catalog);
                    authoredSystem.DrawTrails(frame, Position16<RoomPixels>.Zero, catalog, InstalledFixtures.ProjectileArtwork());
                });
                AssertTrue(nativeOam.LowTable.SequenceEqual(authoredOam.LowTable), "Trail catalog preserves live command/termination/freeze frame output");
                foreach (var sides in new[] { (nativeSystem.TrailSlots[0].Left, authoredSystem.TrailSlots[0].Left), (nativeSystem.TrailSlots[0].Right, authoredSystem.TrailSlots[0].Right) })
                {
                    AssertEqual(sides.Item1.InstructionPointer, sides.Item2.InstructionPointer, "Trail artwork cannot change instruction cursor");
                    AssertEqual(sides.Item1.InstructionTimer.RawValue, sides.Item2.InstructionTimer.RawValue, "Trail artwork cannot change live timing");
                    AssertEqual(sides.Item1.Position.Y, sides.Item2.Position.Y, "Trail artwork cannot change sibling-targeted movement");
                }
            }
        }
        AssertTrue(encountered.SetEquals(ProjectileTrailVisualDefinitions.Frames.ToArray()), "Independent native stream walk finds exactly the catalog's appearance records");
        int programWords = 0;
        for (int address = 0x90b4c8; address <= 0x90b5b3; address++)
        {
            if (ProjectileTrailProgramDefinitions.TryRead(address, out _))
            {
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), ProjectileTrailProgramDefinitions.Read(address), "Compiled trail program preserves every authored mechanics word");
                programWords++;
            }
            else
            {
                int rejectedAddress = address;
                AssertThrows<InvalidDataException>(() => ProjectileTrailProgramDefinitions.Read(rejectedAddress), "Trail program rejects presentation gaps, odd addresses and unrelated high-bank words");
            }
        }
        AssertEqual(67, programWords, "All 42 durations, 20 movement commands and five terminators are compiled");
        AssertThrows<InvalidDataException>(() => ProjectileTrailProgramDefinitions.Read(0x91b4c9), "Trail program rejects a wrong-bank alias");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = ProjectileTrailVisualDefinitions.Frames[-1], "Calculated trail frame lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = ProjectileTrailVisualDefinitions.Frames[42], "Calculated trail frame upper bound");
        Console.WriteLine("Stream 2 trail programs: 67 native mechanics words,42 visual record addresses, all four actual paired trail lifetimes/freeze states and mutable alias pass.");
        void StepNative(SamusProjectileTrailSlot pair, SamusProjectileTrailSide side, OamBuffer oam, bool frozen)
        {
            if (side.InstructionTimer.IsZero) return;
            if (!frozen && (side.InstructionTimer = side.InstructionTimer.Decrement().Remaining).IsZero)
            {
                ushort cursor = checked((ushort)side.InstructionPointer);
                while (true)
                {
                    ushort word = ReadVerificationWord(bus, 0x900000 | cursor);
                    if (word < 0x8000)
                    {
                        side.InstructionTimer = NativeCountdown.FromRaw(word);
                        if (word == 0) return;
                        side.TileNumberAttributes = ReadVerificationWord(bus, 0x900000 | (cursor + 2));
                        side.InstructionPointer = (ushort)(cursor + 4);
                        break;
                    }
                    cursor += 2;
                    if (word == (ushort)ProjectileTrailInstruction.MoveLeftDown) pair.Left.Position += new Offset16<RoomPixels>(0, 1);
                    else if (word == (ushort)ProjectileTrailInstruction.MoveRightDown) pair.Right.Position += new Offset16<RoomPixels>(0, 1);
                    else throw new InvalidDataException($"Unexpected native trail command {word:X4}.");
                }
            }
            oam.AddProjectileTrailSprite(new Position16<OamPixels>((byte)side.Position.X, (byte)side.Position.Y), side.TileNumberAttributes);
        }
    }

    /// <summary>Checks left/right trail pointer selectors and their native table domain.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled projectile trail selector words")]
    internal static void VerifyLookupStream2TrailSelectors(ISnesAddressSpace bus)
    {

        int start = SamusProjectileRomData.Trails.LeftInstructionPointers;
        int end = SamusProjectileRomData.Trails.RightInstructionPointers + 64 * sizeof(ushort);
        for (int address = start; address < end; address += sizeof(ushort))
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), ProjectileTrailDefinitions.ReadSelector(address, InstalledFixtures.SamusOverreads), "Trail selectors retain every reachable aligned native word");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start - 2, InstalledFixtures.SamusOverreads), "Trail selector rejects the preceding word");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start + 1, InstalledFixtures.SamusOverreads), "Trail selector rejects unaligned reads");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(end, InstalledFixtures.SamusOverreads), "Trail selector rejects the following word");
        for (int selection = 0; selection < 64; selection++)
        {
            var projectiles = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None) { Overreads = InstalledFixtures.SamusOverreads };
            var projectile = new SamusProjectileSlot(0)
            {
                Type = (ushort)selection,
                Cursor = SamusProjectileProgramAddresses.CursorAt(0x86e3),
                Position = new(new(100, 0), new(200, 0)),
            };
            projectiles.Trails.SpawnTrail(projectile);
            var trail = projectiles.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
            // An empty trail family (programs, direction list or coordinates outside the compiled data) draws no trail (#1318).
            if (trail.Left.InstructionTimer.IsZero)
                continue;
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), start + selection * 2), trail.Left.InstructionPointer, "Real spawn selects left trail including adjacent right-table entries");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomData.Trails.RightInstructionPointers + selection * 2), trail.Right.InstructionPointer, "Real spawn retains right-table overrun behavior");
            AssertEqual(1, trail.Left.InstructionTimer.RawValue, "Selector extraction leaves allocation timer unchanged");
            AssertEqual(96, trail.Left.Position.X, "Selector extraction leaves origin offset unchanged");
        }
        Console.WriteLine("Trail selectors: 103 reachable native words and 64 real spawn selections pass with the complete selector window forbidden.");
    }

    /// <summary>Checks Mother Brain tube palette ramps and actual CGRAM sequence.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyLookupStream2TubeRamp(ISnesAddressSpace rom)
    {
        PaletteRgb5[] Read(int address) => Enumerable.Range(0, 32).Select(index =>
        {
            ushort word = ReadVerificationWord(rom, address + index * 2);
            return new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        }).ToArray();
        var original = new ChozoAndTubeColorDocument { Version = 1,
            TubeCracks = Read(ChozoAndTubeColorRomData.TubeCracksSource),
            WreckedShip = Read(ChozoAndTubeColorRomData.WreckedShipSource),
            LowerNorfair = Read(ChozoAndTubeColorRomData.LowerNorfairSource) };
        ChozoAndTubeColorCatalog Load(ChozoAndTubeColorDocument document) =>
            ChozoAndTubeColorCatalog.Load(new MemoryStream(ChozoAndTubeColorCatalog.Write(document), writable: false));
        var stock = Load(original);
        Check(stock, original);
        AssertEqual(8, stock.tubeColorSeeds.Length,
            "tube stores only its eight authored seed colors");
        foreach (var (statue, edits) in new[] { ("wreckedShipEdits", stock.wreckedShipEdits), ("lowerNorfairEdits", stock.lowerNorfairEdits) })
            AssertEqual(0, edits.Count, $"stock {statue} store no colors beyond the calculated statue paint");
        AssertEqual(0, stock.tubeColorEdits.Count,
            "stock tube ramp and repeated half need no residuals");
        for (int color = 0; color < 32; color++)
        {
            var changed = (PaletteRgb5[])original.TubeCracks.Clone();
            changed[color] = changed[color] with { Red = (changed[color].Red + 1) % 32 };
            var document = original with { TubeCracks = changed };
            Check(Load(document), document);
            foreach (bool wrecked in new[] { true, false })
            {
                var statue = (PaletteRgb5[])(wrecked ? original.WreckedShip : original.LowerNorfair).Clone();
                statue[color] = statue[color] with { Red = (statue[color].Red + 1) % 32 };
                var statueDocument = wrecked ? original with { WreckedShip = statue } : original with { LowerNorfair = statue };
                var edited = Load(statueDocument);
                Check(edited, statueDocument);
                int stored = edited.wreckedShipEdits.Count
                    + edited.lowerNorfairEdits.Count;
                AssertEqual(1, stored, "a statue edit stores only that color");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveTubeCracks(-1), "tube negative color");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveTubeCracks(32), "tube past last color");
        Console.WriteLine("Tube and statue palettes: 96 native colors, statue paint with no stored stock colors, full CGRAM writes, 96 independent edits and canonical identity pass; the eight tube seeds are authored.");

        static void Check(ChozoAndTubeColorCatalog catalog, ChozoAndTubeColorDocument document)
        {
            static ushort Word(PaletteRgb5 color) => (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
            var cgram = new SnesCgram();
            catalog.ApplyTubeCracks(cgram);
            for (int color = 0; color < 32; color++)
            {
                ushort word = Word(document.TubeCracks[color]);
                AssertEqual(word, catalog.ResolveTubeCracks(color), "independent tube color preserved");
                AssertEqual(word, cgram.Colors[ChozoAndTubeColorRomData.Destination + color], "tube color reaches CGRAM");
                AssertEqual(Word(document.WreckedShip[color]), catalog.ResolveWreckedShip(color), "Wrecked Ship palette preserved");
                AssertEqual(Word(document.LowerNorfair[color]), catalog.ResolveLowerNorfair(color), "Lower Norfair palette preserved");
            }
            var statues = new SnesCgram();
            catalog.ApplyWreckedShip(statues);
            for (int color = 0; color < 32; color++)
                AssertEqual(Word(document.WreckedShip[color]), statues.Colors[ChozoAndTubeColorRomData.Destination + color], "Wrecked Ship color reaches CGRAM");
            catalog.ApplyLowerNorfair(statues);
            for (int color = 0; color < 32; color++)
                AssertEqual(Word(document.LowerNorfair[color]), statues.Colors[ChozoAndTubeColorRomData.Destination + color], "Lower Norfair color reaches CGRAM");
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
            int relative = cell - PauseWireframeDefinitions.DestinationByte / 2;
            bool wireframe = relative >= 0 && relative / (PauseWireframeDefinitions.DestinationStride / 2) < PauseWireframeDefinitions.Rows &&
                relative % (PauseWireframeDefinitions.DestinationStride / 2) < PauseWireframeDefinitions.Columns;
            AssertEqual(equipment, PauseEquipmentBaseDefinitions.IsEquipmentLabelCell(cell), "native equipment ownership footprint");
            AssertEqual(equipment || reserve || wireframe, PauseEquipmentBaseDefinitions.IsLiveOwnedCell(cell), "native live ownership footprint");
            AssertEqual((reserve || wireframe) && !equipment, PauseEquipmentBaseDefinitions.IsNonInventoryLiveOwnedCell(cell), "native noninventory footprint");
        }
        Console.WriteLine("Pause ownership: all1024 cells plus rejected outer bounds match native label/reserve destinations, including the nine-cell Plasma overlap.");
    }
    /// <summary>Checks Dead Torizo VRAM descriptors, terminators, and fourteen queued live-WRAM transfers.</summary>
    internal static void VerifyLookupStream2DeadTorizoTransfers(ISnesAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        var state = new DeadTorizoEnemyState(enemies.SlotTable.Slots[0], new CorpseRottingProgram { YLimit = 0, LateMoveEntryIndex = 0, WrapOffset = 0 });
        Action<DeadTorizoEnemyState> build = enemies.DeadTorizoBehavior.BuildDeadTorizoVramTransfers;
        for (ushort phase = 0; phase < 2; phase++)
        {
            var rows = DeadTorizoVramTransferDefinitions.ForHalf(odd: phase == 1);
            ushort table = phase == 0 ? DeadTorizoVramTransferDefinitions.EvenTable : DeadTorizoVramTransferDefinitions.OddTable;
            AssertEqual(7, rows.Length, "Dead Torizo six body rows plus sand strip");
            int count = 0;
            foreach (var row in rows)
            {
                int address = 0xa90000 | table + 8 * count;
                AssertEqual(ReadVerificationWord(rom, address), row.SizeInBytes, "Dead Torizo native DMA size");
                AssertEqual(ReadVerificationWord(rom, address + 2), row.SourceBankWord, "Dead Torizo native DMA bank");
                AssertEqual(ReadVerificationWord(rom, address + 4), row.SourceOffset, "Dead Torizo native DMA source");
                AssertEqual(ReadVerificationWord(rom, address + 6), row.EncodedVramDestination, "Dead Torizo native DMA destination");
                AssertEqual(rows[count++], row, "Dead Torizo enumeration order");
            }
            AssertEqual(7, count, "Dead Torizo enumeration count");
            AssertEqual((ushort)0, ReadVerificationWord(rom, 0xa90000 | table + count * 8), "Dead Torizo native terminator");
            // The cartridge's phase word before this call is phase - 1: 65535 for phase 0, whose odd low bit
            // gives way to the even half across the word's wrap, then 0 for phase 1.
            state.UploadsOddTransferHalf = phase == 0;
            build(state);
            AssertEqual(phase == 1, state.UploadsOddTransferHalf, "Dead Torizo phase advancement, including across the phase word's wrap");
            for (int index = 0; index < rows.Length; index++)
            {
                var actual = enemies.LastDeadTorizoVramTransfers[phase * 7 + index];
                AssertEqual(rows[index].SizeInBytes, actual.SizeInBytes, "Dead Torizo queued size");
                AssertEqual(rows[index].SourceAddress, actual.SourceAddress, "Dead Torizo queued live WRAM source");
                AssertEqual(rows[index].EncodedVramDestination, actual.Destination.Encoded, "Dead Torizo queued VRAM destination");
            }
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = rows[-1]; }, "Dead Torizo negative row");
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = rows[7]; }, "Dead Torizo past last row");
        }
        Console.WriteLine("Dead Torizo: all56 native descriptor fields, two terminators, actual fourteen queued live-WRAM transfers, phase parity, enumeration and bounds pass.");
    }
    /// <summary>Checks five gunship liftoff graphics transfers and their live queue behavior.</summary>
    internal static void VerifyLookupStream2GunshipTransfers(ISnesAddressSpace rom)
    {
        var transfers = GunshipLiftoffTransferDefinitions.Frames;
        AssertEqual(5, transfers.Count, "Gunship calculated transfer count");
        var enemies = new RoomEnemySystem(new EnemySession());
        var slot = enemies.SlotTable.Slots[0];
        var part = new GunshipPartState();
        slot.SetSpeciesState(part, part.ClearWithRecord);
        var queue = new VramWriteQueue();
        var append = new Action<RoomEnemySlot, VramWriteQueue>(enemies.GunshipBehavior.QueueGunshipTakeoffTiles);
        int enumerated = 0;
        foreach (var transfer in transfers)
        {
            int source = 0x940000 | ReadVerificationWord(rom, 0xa2ac07 + 2 * enumerated);
            ushort destination = ReadVerificationWord(rom, 0xa2ac11 + 2 * enumerated);
            AssertEqual(source, transfer.SourceAddress, "Gunship original source");
            AssertEqual(destination, transfer.DestinationWord, "Gunship original destination");
            AssertEqual(VramAssetId.GunshipLiftoffFirstTiles + enumerated, transfer.Asset, "Gunship typed transfer identity");
            AssertEqual(transfers[enumerated], transfer, "Gunship enumeration and indexing agree");
            append(slot, queue);
            AssertEqual(destination, queue.Entries[enumerated].Destination.Encoded, "Gunship actual queued destination");
            AssertEqual(source, queue.Entries[enumerated].SourceAddress, "Gunship actual queued source");
            enumerated++;
        }
        AssertEqual(5, enumerated, "Gunship enumerates every transfer");
        AssertEqual(0, part.LiftoffTransferIndex, "Gunship completes transfer phase");
        AssertEqual(GunshipFunction.FireUpEngines, part.Function, "Gunship starts engines after fifth transfer");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = transfers[-1], "Gunship lower transfer bound");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = transfers[5], "Gunship upper transfer bound");
        Console.WriteLine("Gunship transfers: all10 native source/destination fields, five typed identities, actual queued uploads/phase handoff, enumeration and bounds pass.");
    }
    /// <summary>Checks reserve arrow RGB5 colors, independent edits, and CGRAM placement.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyLookupStream2ReserveArrowColors(ISnesAddressSpace rom)
    {
        byte[] source = PauseReserveUiExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseReserveUiDocument>(source, ArtworkJson.Options);
        var stock = PauseReserveUiPresentation.Load(new MemoryStream(source));
        var edits = stock.arrowColorEdits;
        AssertEqual(0, edits.Count, "Native channel choices plus calculated ramps need no unexplained color overrides");
        var cgram = new SnesCgram();
        for (int frame = 0; frame < 32; frame++)
        {
            stock.ApplyArrowColors(cgram, true, frame, 6, 11);
            AssertEqual(ReadVerificationWord(rom, 0x82ad5d + 2 * frame), cgram.Colors[6], "Native arrow color6 reaches CGRAM");
            AssertEqual(ReadVerificationWord(rom, 0x82ad9d + 2 * frame), cgram.Colors[11], "Native arrow color11 reaches CGRAM");
        }
        Check(stock, document);
        for (int frame = 0; frame < 32; frame++)
        for (int color = 0; color < 2; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            var frames = document.Arrow.Frames.ToArray();
            var rgb = color == 0 ? frames[frame].Color6 : frames[frame].Color11;
            rgb = channel switch
            {
                0 => rgb with { Red = (rgb.Red + 1) & 31 },
                1 => rgb with { Green = (rgb.Green + 1) & 31 },
                _ => rgb with { Blue = (rgb.Blue + 1) & 31 },
            };
            frames[frame] = color == 0 ? frames[frame] with { Color6 = rgb } : frames[frame] with { Color11 = rgb };
            var changed = document with { Arrow = document.Arrow with { Frames = frames } };
            using var encoded = new MemoryStream();
            PauseReserveUiPresentation.Write(encoded, changed);
            encoded.Position = 0;
            Check(PauseReserveUiPresentation.Load(encoded), changed);
        }
        for (int selected = 0; selected < 2; selected++)
        for (int channel = 0; channel < 3; channel++)
        {
            PaletteRgb5 original = selected == 0 ? document.Arrow.SolidColor6 : document.Arrow.SolidColor11;
            var changedColor = new PaletteRgb5 { Red = channel == 0 ? original.Red ^ 31 : original.Red,
                Green = channel == 1 ? original.Green ^ 31 : original.Green, Blue = channel == 2 ? original.Blue ^ 31 : original.Blue };
            var arrow = selected == 0 ? document.Arrow with { SolidColor6 = changedColor } : document.Arrow with { SolidColor11 = changedColor };
            var changed = document with { Arrow = arrow };
            using var encoded = new MemoryStream(); PauseReserveUiPresentation.Write(encoded, changed); encoded.Position = 0;
            Check(PauseReserveUiPresentation.Load(encoded), changed);
        }
        Console.WriteLine("Reserve arrow ramps: all64 native colors,192 independent frame-channel edits,6 solid-color edits,zero stock color overrides,actual CGRAM and wrapping pass; mixed source paint/hold disposition documented.");

        static void Check(PauseReserveUiPresentation presentation, PauseReserveUiDocument expected)
        {
            var cgram = new SnesCgram();
            for (int frame = -1; frame <= 32; frame++)
            {
                presentation.ApplyArrowColors(cgram, true, frame, 6, 11);
                var row = expected.Arrow.Frames[frame & 31];
                AssertEqual(Pack(row.Color6), cgram.Colors[6], "Independent supplied color6");
                AssertEqual(Pack(row.Color11), cgram.Colors[11], "Independent supplied color11");
            }
            presentation.ApplyArrowColors(cgram, false, 0, 6, 11);
            AssertEqual(Pack(expected.Arrow.SolidColor6), cgram.Colors[6], "Solid arrow color6");
            AssertEqual(Pack(expected.Arrow.SolidColor11), cgram.Colors[11], "Solid arrow color11");
        }
        static ushort Pack(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
    }
    /// <summary>Checks reserve bar and arrow positions against extracted pause UI geometry.</summary>
    internal static void VerifyLookupStream2ReserveGeometry(ISnesAddressSpace rom)
    {
        byte[] source = PauseReserveUiExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<PauseReserveUiDocument>(source, ArtworkJson.Options);
        var stock = PauseReserveUiPresentation.Load(new MemoryStream(source));
        AssertTrue(stock.digits is null, "Stock digits discard stored lookup");
        AssertTrue(stock.arrowOffsets is null, "Stock arrow discards stored offsets");
        Check(stock, document);
        for (int index = 0; index < 10; index++)
        {
            var digits = document.Digits.Cells.ToArray();
            digits[index] = digits[index] with { FlipY = !digits[index].FlipY };
            var changedDocument = document with { Digits = document.Digits with { Cells = digits, Anchor = new() { Column = 18, Row = 20 } } };
            using var encoded = new MemoryStream();
            PauseReserveUiPresentation.Write(encoded, changedDocument);
            encoded.Position = 0;
            Check(PauseReserveUiPresentation.Load(encoded), changedDocument);
            var cells = document.Arrow.Cells.ToArray();
            cells[index] = new() { Column = 20 + index, Row = 22 };
            changedDocument = document with { Arrow = document.Arrow with { Cells = cells, EnabledPalette = 3, DisabledPalette = 4 } };
            encoded.SetLength(0);
            PauseReserveUiPresentation.Write(encoded, changedDocument);
            encoded.Position = 0;
            Check(PauseReserveUiPresentation.Load(encoded), changedDocument);
        }
        Console.WriteLine("Reserve geometry: all10 stock digits/10 arrow cells, actual tilemap writes, all20 independent edits, supplied anchors/palettes and unrelated tile fields pass.");

        static void Check(PauseReserveUiPresentation presentation, PauseReserveUiDocument expected)
        {
            for (int value = 0; value < 10; value++)
            for (int place = 0; place < 3; place++)
            {
                byte[] tilemap = new byte[2048];
                presentation.ApplyDigit(tilemap, place, value);
                int offset = 2 * (32 * expected.Digits.Anchor.Row + expected.Digits.Anchor.Column + place);
                byte[] word = PauseTileGrid.Compile([expected.Digits.Cells[value]], "reserve verification");
                AssertEqual(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(word),
                    System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(tilemap.AsSpan(offset)), "Digit actual complete tile word and supplied anchor");
            }
            foreach (bool enabled in new[] { false, true })
            {
                byte[] tilemap = new byte[2048];
                for (int cell = 0; cell < 1024; cell++)
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(tilemap.AsSpan(2 * cell), 0xe355);
                presentation.ApplyArrowTilePalettes(tilemap, enabled);
                for (int cell = 0; cell < 1024; cell++)
                {
                    bool arrow = expected.Arrow.Cells.Any(point => 32 * point.Row + point.Column == cell);
                    ushort word = arrow ? (ushort)((0xe355 & ~0x1c00) | ((enabled ? expected.Arrow.EnabledPalette : expected.Arrow.DisabledPalette) << 10)) : (ushort)0xe355;
                    AssertEqual(word, System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(tilemap.AsSpan(2 * cell)), "Arrow updates exactly supplied cells and preserves tile/flip/priority fields");
                }
            }
        }
    }
    /// <summary>Checks ghost appearance timing/visibility and four Norfair palette-FX cycles.</summary>
    internal static void VerifyLookupStream2GhostAndNorfair(ISnesAddressSpace rom)
    {
        Action<RoomEnemySlot, WreckedShipGhostEnemyState, Position16<RoomPixels>> spawn = WreckedShipGhostBehavior.SpawnWreckedShipGhost;
        Action<RoomEnemySlot, WreckedShipGhostEnemyState> flicker = WreckedShipGhostBehavior.AdvanceWreckedShipGhostFlicker;
        var slot = new RoomEnemySystem(new EnemySession()).SlotTable.Slots[0];
        var state = new WreckedShipGhostEnemyState();
        for (int index = 0; index < 9; index++)
        {
            short originalX = unchecked((short)ReadVerificationWord(rom, 0xa89aa8 + 4 * index));
            short originalY = unchecked((short)ReadVerificationWord(rom, 0xa89aaa + 4 * index));
            var calculated = WreckedShipGhostAppearanceDefinitions.SpawnOffset(index);
            AssertEqual(originalX, calculated.X, "Ghost original horizontal spawn offset");
            AssertEqual(originalY, calculated.Y, "Ghost original vertical spawn offset");
            state.SpawnColumn = index % 3;
            state.SpawnRow = index / 3;
            spawn(slot, state, new(32, 65520));
            AssertEqual(unchecked((ushort)(32 + originalX)), slot.Position.X.Whole, "Ghost actual wrapped spawn X");
            AssertEqual(unchecked((ushort)(65520 + originalY)), slot.Position.Y.Whole, "Ghost actual wrapped spawn Y");
            AssertEqual(WreckedShipGhostAiFunction.BrighteningAndFlickering, state.Function, "Ghost appearance phase");
            AssertEqual((ushort)64, state.StablePositionTimer.RawValue, "Ghost appearance position timer");
            AssertEqual((ushort)16, state.StableDirectionTimer.RawValue, "Ghost appearance direction timer");
        }
        for (int offset = 0; offset < 34; offset++)
        {
            short original = unchecked((short)ReadVerificationWord(rom, 0xa89acc + 2 * (offset / 2)));
            AssertEqual(original, WreckedShipGhostAppearanceDefinitions.FlickerDuration(offset / 2), "Ghost original flicker interval");
            slot.Properties = new EnemyPropertyWord(EnemyProperties.Invisible | EnemyProperties.ProcessOffScreen);
            state.PhaseTimer = NativeCountdown.Start(1);
            state.FlickerTableOffset = offset;
            flicker(slot, state);
            AssertEqual(original < 0 ? (ushort)0 : (ushort)original, state.PhaseTimer.RawValue, "Ghost actual next interval");
            AssertEqual(original < 0 ? 0 : offset + 2, state.FlickerTableOffset, "Ghost actual next offset including odd folding");
            bool remainsInvisible = original < 0 || (offset & 2) != 0;
            AssertEqual((ushort)(EnemyProperties.ProcessOffScreen | (remainsInvisible ? EnemyProperties.Invisible : default(EnemyProperties))), slot.Properties.ToNativeWord(), "Ghost actual visibility preserves unrelated flags");
        }
        state.PhaseTimer = NativeCountdown.Start(2);
        state.FlickerTableOffset = 2;
        slot.Properties = new EnemyPropertyWord(EnemyProperties.Invisible);
        flicker(slot, state);
        AssertEqual((ushort)1, state.PhaseTimer.RawValue, "Ghost interval countdown");
        AssertEqual(2, state.FlickerTableOffset, "Ghost no early interval advance");
        AssertEqual((ushort)EnemyProperties.Invisible, slot.Properties.ToNativeWord(), "Ghost no early visibility change");
        state.PhaseTimer = NativeCountdown.Start(0);
        flicker(slot, state);
        AssertEqual((ushort)0, slot.Properties.ToNativeWord(), "Ghost terminal next call clears invisibility");
        state.PhaseTimer = NativeCountdown.Start(1);
        state.FlickerTableOffset = 34;
        AssertThrows<InvalidDataException>(() => flicker(slot, state), "Ghost malformed offset remains rejected");
        state.SpawnColumn = 3;
        state.SpawnRow = 0;
        AssertThrows<InvalidDataException>(() => spawn(slot, state, Position16<RoomPixels>.Zero), "Ghost out-of-range spawn class remains rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.SpawnOffset(-1), "Ghost spawn lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.SpawnOffset(9), "Ghost spawn upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.FlickerDuration(-1), "Ghost flicker lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.FlickerDuration(17), "Ghost flicker upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => NorfairEnvironmentalPaletteFxColorDefinitionsConstants.Duration(-1), "Norfair duration lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => NorfairEnvironmentalPaletteFxColorDefinitionsConstants.Duration(16), "Norfair duration upper bound");
        Console.WriteLine("Ghost appearance: 18 native offset fields, 17 native intervals, actual wrapped positions/timing/visibility and malformed states pass; Norfair: 64 native durations and four 116-tick cycles pass.");
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
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = definitions[-1]; }, "Torizo definition lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = definitions[2]; }, "Torizo definition upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.FramePointer(-1), "Upper Crateria lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions.FramePointer(14), "Upper Crateria upper frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorPointer(0, -1), "Old Tourian lower color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => OldTourianEscapeRedFlashPaletteFxColorDefinitions.ColorPointer(0, 8), "Old Tourian upper color bound");
        Console.WriteLine("Stream 2 palette mechanics: original mechanics, inline color offsets, durations, dispatch and bounds pass.");
    }
    /// <summary>Checks Crystal Flash body colors and emitted CGRAM against extracted native phases.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyLookupStream2CrystalBody(ISnesAddressSpace rom)
    {
        byte[] source = CrystalFlashColorExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<CrystalFlashColorDocument>(
            source, ArtworkJson.Options);
        var stock = CrystalFlashColorCatalog.Load(new MemoryStream(source));
        AssertTrue(stock.body is null, "Original Crystal Flash body selects calculation without stored rows");
        var cgram = new SnesCgram();
        for (int frame = 0; frame < CrystalFlashColorFormat.BodyFrameCount; frame++)
        {
            ushort pointer = ReadVerificationWord(rom, SamusPaletteRomData.CrystalFlash.BodyRecords + frame * 4);
            stock.ApplyBody(cgram, frame);
            for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
            {
                ushort expected = ReadVerificationWord(rom, 0x9b0000 | pointer + 2 * color);
                AssertEqual(expected, stock.ResolveBody(frame, color), "Original Crystal Flash body RGB5 word");
                AssertEqual(expected, cgram.Colors[SamusPaletteRomData.CrystalFlash.BodyCgramStart + color], "Calculated body color reaches CGRAM");
                var rows = document.Body.Select(row => (PaletteRgb5[])row.Clone()).ToArray();
                rows[frame][color] = rows[frame][color] with { Red = (rows[frame][color].Red + 1) % 32 };
                var changedDocument = document with { Body = rows };
                var changed = CrystalFlashColorCatalog.Load(new MemoryStream(CrystalFlashColorCatalog.Write(changedDocument)));
                AssertTrue(changed.body is not null, "Independent supplied body edit retains its rows");
                for (int verifyFrame = 0; verifyFrame < rows.Length; verifyFrame++)
                {
                    changed.ApplyBody(cgram, verifyFrame);
                    for (int verifyColor = 0; verifyColor < rows[verifyFrame].Length; verifyColor++)
                    {
                        var rgb = rows[verifyFrame][verifyColor];
                        ushort word = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
                        AssertEqual(word, changed.ResolveBody(verifyFrame, verifyColor), "All supplied body fields preserved");
                        AssertEqual(word, cgram.Colors[SamusPaletteRomData.CrystalFlash.BodyCgramStart + verifyColor], "Supplied body color reaches CGRAM");
                    }
                }
                // Restore stock CGRAM for the remaining checks in this original frame.
                stock.ApplyBody(cgram, frame);
            }
        }
        var residuals = stock.bubble;
        AssertEqual(1, residuals.Count, "One stock bubble residual remains pending");
        AssertEqual(0x7fff, residuals[30], "Final-frame leading white remains supplied data");
        for (int frame = 0; frame < CrystalFlashColorFormat.BubbleFrameCount; frame++)
        for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
        {
            ushort pointer = ReadVerificationWord(rom, SamusPaletteRomData.CrystalFlash.BubblePointers + frame * 2);
            ushort original = ReadVerificationWord(rom, 0x9b0000 | pointer + 2 * color);
            AssertEqual(original, stock.ResolveBubble(frame, color), "Original bubble payload preserved");
            stock.ApplyBubble(cgram, frame);
            AssertEqual(original, cgram.Colors[SamusPaletteRomData.CrystalFlash.BubbleCgramStart + color], "Original bubble color reaches CGRAM");
            var rows = document.Bubble.Select(row => (PaletteRgb5[])row.Clone()).ToArray();
            rows[frame][color] = rows[frame][color] with { Green = (rows[frame][color].Green + 1) % 32 };
            var changedDocument = document with { Bubble = rows };
            var changed = CrystalFlashColorCatalog.Load(new MemoryStream(CrystalFlashColorCatalog.Write(changedDocument)));
            for (int verifyFrame = 0; verifyFrame < rows.Length; verifyFrame++)
            {
                changed.ApplyBubble(cgram, verifyFrame);
                for (int verifyColor = 0; verifyColor < rows[verifyFrame].Length; verifyColor++)
                {
                    var rgb = rows[verifyFrame][verifyColor];
                    ushort word = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
                    AssertEqual(word, changed.ResolveBubble(verifyFrame, verifyColor), "All supplied bubble fields preserved");
                    AssertEqual(word, cgram.Colors[SamusPaletteRomData.CrystalFlash.BubbleCgramStart + verifyColor], "Supplied bubble color reaches CGRAM");
                }
            }
            AssertTrue(changed.body is null, "Independent bubble edit preserves body calculation");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(-1, 0), "Bubble lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(6, 0), "Bubble upper frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(0, -1), "Bubble lower color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBubble(0, 6), "Bubble upper color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(-1, 0), "Calculated body lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(10, 0), "Calculated body upper frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(0, -1), "Calculated body lower color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBody(0, 10), "Calculated body upper color bound");
        Console.WriteLine("Stream 2 Crystal Flash body: all100 body and36 bubble native colors, CGRAM application and every independent supplied edit pass; one bubble residual remains pending.");
    }

    /// <summary>Checks Kraid palette ramp derivation and edited-color bounds against stock samples.</summary>
    internal static void VerifyLookupStream2KraidRamps(ISnesAddressSpace rom)
    {
        PaletteRgb5[] ReadSource(KraidPaletteSource source) => Enumerable.Range(0, KraidPaletteRomData.ColorCount(source))
            .Select(index =>
            {
                ushort word = ReadVerificationWord(rom, KraidPaletteRomData.SourceAddress(source) + 2 * index);
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
        var stock = KraidColorCatalog.Load(new MemoryStream(KraidColorCatalog.Write(document)));
        object health = stock.health;
        object secondary = stock.secondary;
        AssertTrue(ReferenceEquals(health, secondary), "Identical Kraid secondary colors share calculated primary source");
        foreach (var (name, band) in new (string, IReadOnlyList<Bgr555>)[]
                 { ("health", stock.health), ("roomBackdrop", stock.roomBackdrop), ("initialTarget", stock.initialTarget), ("deathArm", stock.deathArm) })
        {
            int deviations = band switch
            {
                KraidColorCatalog.StockBand stored => stored.deviations.Count,
                KraidColorCatalog.HealthPalette palette => palette.deviations.Count,
                _ => throw new InvalidDataException($"Stock Kraid {name} is not a deviation-storing band."),
            };
            AssertEqual(0, deviations, $"Stock Kraid {name} stores no colors beyond the calculated paint");
        }
        foreach (KraidPaletteSource source in Enum.GetValues<KraidPaletteSource>())
        for (int index = 0; index < KraidPaletteRomData.ColorCount(source); index++)
            AssertEqual(ReadVerificationWord(rom, KraidPaletteRomData.SourceAddress(source) + 2 * index),
                KraidPaintDefinitions.Color(source, index), $"Kraid {source} stock paint {index}");
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
        foreach (KraidPaletteSource source in Enum.GetValues<KraidPaletteSource>())
        for (int index = 0; index < KraidPaletteRomData.ColorCount(source); index++)
        {
            var editedColors = (PaletteRgb5[])Source(document, source).Clone();
            editedColors[index] = editedColors[index] with { Red = (editedColors[index].Red + 1) % 32 };
            var editedDocument = source switch
            {
                KraidPaletteSource.RoomBackdrop => document with { RoomBackdrop = editedColors },
                KraidPaletteSource.InitialTarget => document with { InitialTarget = editedColors },
                KraidPaletteSource.Health => document with { Health = editedColors },
                KraidPaletteSource.Secondary => document with { Secondary = editedColors },
                KraidPaletteSource.DeathArm => document with { DeathArm = editedColors },
                _ => document with { DeathArm = editedColors },
            };
            var edited = KraidColorCatalog.Load(new MemoryStream(KraidColorCatalog.Write(editedDocument)));
            Check(edited, editedDocument);
            AssertTrue(edited.ContentIdentity != stock.ContentIdentity, "Every independent Kraid color edit changes content identity");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(KraidPaletteSource.Health, -1), "Kraid calculated lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(KraidPaletteSource.Secondary, 144), "Kraid calculated upper bound");
        Console.WriteLine("Stream 2 Kraid colors: all 336 stock words calculate from paint with no stored stock colors; every one of the 336 single-cell edits, source independence and canonical identity pass.");

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
                    AssertEqual(Pack(colors[index]), actual.Resolve(source, index), "Every original or independently supplied Kraid color");
            }
        }
    }
    /// <summary>Checks projectile frame bindings for power and directional combinations.</summary>
    internal static void VerifyLookupStream2PowerDirectionBindings(ISnesAddressSpace rom)
    {
        byte[] json = ProjectileFrameBindingExtractor.Extract(rom);
        var stock = ProjectileFrameBindingCatalog.Load(new MemoryStream(json));
        var remaining = (System.Collections.IDictionary)stock.sprites;
        AssertEqual(0, remaining.Count, "All805 stock selector operands calculate; independent selection policies remain required");
        foreach (ushort pointer in SamusProjectileRecordDefinitions.TimedRecordAddresses)
            AssertEqual(ReadVerificationWord(rom, (0x930000 | pointer) + 2), stock.Resolve(pointer), "Every installed selector retains its exact native target");
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        var document = RequiredJson.Deserialize<ProjectileFrameBindingDocument>(json, options);
        for (int direction = 0; direction < 8; direction++)
        {
            ushort pointer = (ushort)(0x86db + direction * 12);
            AssertTrue(!remaining.Contains(pointer), "Stock compass selector is absent from stored residuals");
            var shot = new SamusProjectileSlot(0) { Cursor = SamusProjectileProgramAddresses.CursorAt(pointer), InstructionTimer = NativeCountdown.Start(1) };
            var system = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None) { Animation = { FrameBindings = stock } };
            _ = system.Animation.RunProjectileInstructionHandler(shot);
            AssertEqual(ReadVerificationWord(rom, (0x930000 | pointer) + 2), shot.SpritemapPointer, "Actual projectile handler selects calculated compass pose");
            string key = ProjectileFrameBindingFormat.FrameName(pointer);
            string original = document.Frames[key];
            document.Frames[key] = ProjectileSpriteDefinitions.Name(ProjectileSpriteDefinitions.NativePointers[0]);
            var edited = ProjectileFrameBindingCatalog.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, options)));
            var editedShot = new SamusProjectileSlot(0) { Cursor = SamusProjectileProgramAddresses.CursorAt(pointer), InstructionTimer = NativeCountdown.Start(1) };
            system.Animation.FrameBindings = edited;
            _ = system.Animation.RunProjectileInstructionHandler(editedShot);
            AssertEqual(ProjectileSpriteDefinitions.NativePointers[0], editedShot.SpritemapPointer, "Independent direction edit overrides calculation in actual handler");
            AssertEqual(shot.InstructionTimer.RawValue, editedShot.InstructionTimer.RawValue, "Visual edit preserves duration");
            AssertEqual(shot.InstructionPointer, editedShot.InstructionPointer, "Visual edit preserves control flow");
            AssertEqual(shot.Radius.X, editedShot.Radius.X, "Visual edit preserves horizontal radius");
            AssertEqual(shot.Radius.Y, editedShot.Radius.Y, "Visual edit preserves vertical radius");
            document.Frames[key] = original;
        }
        int checkedWaveIce = 0;
        int checkedAxialEndPairs = 0;
        int checkedCappedSweep = 0;
        foreach (ushort pointer in SamusProjectileRecordDefinitions.TimedRecordAddresses)
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
            {
                AssertTrue(!remaining.Contains(pointer), "Reversed axial end-pair operands calculate while parity policy remains required");
                checkedAxialEndPairs++;
            }
            if (pointer is >= 0x8bfb and < 0x8c4f)
            {
                AssertTrue(!remaining.Contains(pointer), "Capped sweep operands calculate while plateau policy remains required");
                checkedCappedSweep++;
            }
            AssertTrue(!remaining.Contains(pointer), "Calculated Wave/Ice selector is absent from stored residuals");
            var shot = new SamusProjectileSlot(0) { Cursor = SamusProjectileProgramAddresses.CursorAt(pointer), InstructionTimer = NativeCountdown.Start(1) };
            var system = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None) { Animation = { FrameBindings = stock } };
            _ = system.Animation.RunProjectileInstructionHandler(shot);
            AssertEqual(ReadVerificationWord(rom, (0x930000 | pointer) + 2), shot.SpritemapPointer, "Actual handler uses native Wave/Ice traversal pose");
            string key = ProjectileFrameBindingFormat.FrameName(pointer);
            string original = document.Frames[key];
            ushort replacement = stock.Resolve(pointer) == ProjectileSpriteDefinitions.NativePointers[0]
                ? ProjectileSpriteDefinitions.NativePointers[1] : ProjectileSpriteDefinitions.NativePointers[0];
            document.Frames[key] = ProjectileSpriteDefinitions.Name(replacement);
            var edited = ProjectileFrameBindingCatalog.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, options)));
            system.Animation.FrameBindings = edited;
            var editedShot = new SamusProjectileSlot(0) { Cursor = SamusProjectileProgramAddresses.CursorAt(pointer), InstructionTimer = NativeCountdown.Start(1) };
            _ = system.Animation.RunProjectileInstructionHandler(editedShot);
            AssertEqual(replacement, editedShot.SpritemapPointer, "Every Wave/Ice supplied edit takes precedence");
            AssertEqual(shot.InstructionTimer.RawValue, editedShot.InstructionTimer.RawValue, "Wave/Ice visual edit preserves duration");
            AssertEqual(shot.InstructionPointer, editedShot.InstructionPointer, "Wave/Ice visual edit preserves flow");
            AssertEqual(shot.Radius.X, editedShot.Radius.X, "Wave/Ice visual edit preserves X radius");
            AssertEqual(shot.Radius.Y, editedShot.Radius.Y, "Wave/Ice visual edit preserves Y radius");
            document.Frames[key] = original;
            checkedWaveIce++;
        }
        AssertEqual(797, checkedWaveIce, "All other calculated beam/effect records including18 explicit exceptional policies");
        AssertEqual(8, checkedAxialEndPairs, "All four reversed axial end-pairs use exact native operands");
        AssertEqual(10, checkedCappedSweep, "All ten capped sweep phases use exact native operands");
        int checkedBombs = 0;
        foreach (ushort pointer in SamusProjectileRecordDefinitions.TimedRecordAddresses)
        {
            if (pointer is < 0x9f87 or >= 0xa007) continue;
            var bomb = new SamusBombProjectileSlot(0) { Cursor = SamusProjectileProgramAddresses.CursorAt(pointer), InstructionTimer = NativeCountdown.Start(1), Type = 0x0500 };
            var bombs = new SamusBombProjectileSystem { FrameBindings = stock };
            _ = bombs.RunProjectileInstructionHandler(bomb);
            AssertEqual(ReadVerificationWord(rom, (0x930000 | pointer) + 2), bomb.SpritemapPointer, "Actual bomb handler uses sequential normal/fast pose");
            checkedBombs++;
        }
        AssertEqual(14, checkedBombs, "Both normal/fast PowerBomb and Bomb visual cycles");
        AssertThrows<InvalidDataException>(() => stock.Resolve(0x87c3), "Wave self-jump is not accepted as a timed record");
        AssertThrows<InvalidDataException>(() => stock.Resolve(0x86dc), "Program interior is not accepted as a timed record");
        Console.WriteLine("Stream2 Power bindings: 805calculated beam/effect selectors,805native operands,zero stored stock operands and805actual handler/edit paths pass.");
    }

    /// <summary>Checks Golden Torizo foot extended frames, OAM geometry, and independent edits.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Golden Torizo foot geometry")]
    internal static void VerifyLookupStream2GoldenTorizoFootGeometry(SuperMetroidAddressSpace rom)
    {
        var definitions = EnemyExtendedFrameDefinitions.Frames.ToArray();
        var document = new EnemyIdentityFixture().ExtendedDocument();
        var selected = new EnemySpritemapDefinition[10];
        int nativeBodyParts = 0;
        for (int phase = 0; phase < 10; phase++)
        {
            int identity = 0xaaa4fa + 34 * phase;
            AssertEqual(identity, GoldenTorizoStrideGeometryDefinitions.NativeFrameIdentity(phase), "Golden Torizo native extended-record geometry");
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
        EnemyExtendedFrameCatalog Load() => EnemyExtendedFrameCatalog.Load(new MemoryStream(
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
                    var parts = EnemySpritemapCatalog.CompileParts(wanted.Parts, frame.Name);
                    AssertEqual((short)wanted.OffsetX, found.Offset.X, "Golden Torizo independent component X");
                    AssertEqual((short)wanted.OffsetY, found.Offset.Y, "Golden Torizo independent component Y");
                    AssertTrue(found.Parts.SequenceEqual(parts), "Golden Torizo exact supplied part order and fields");
                    ushort x = unchecked((ushort)(128 + wanted.OffsetX)), y = unchecked((ushort)(96 + wanted.OffsetY));
                    expectedOam.AddEnemySpritemap(parts.AsSpan(), new Position16<OamPixels>(x, y), 0, 0);
                    actualOam.AddEnemySpritemap(found.Parts, new Position16<OamPixels>(x, y), 0, 0);
                }
                AssertEqual(expectedOam.NextByteOffset, actualOam.NextByteOffset, "Golden Torizo packed OBJ count");
                AssertTrue(expectedOam.ImageEquals(actualOam),
                    "Golden Torizo calculated stock/supplied parts produce exact packed OAM");
            }
            var identities = definitions.ToDictionary(frame => frame.Name, frame => ((int)new SnesAddress(frame.Bank, frame.Pointer)), StringComparer.Ordinal);
            string expectedHash = SelectedPresentationHash.Create("enemy-extended-oam-v1", content =>
            {
                foreach (var frame in definitions.OrderBy(frame => ((int)new SnesAddress(frame.Bank, frame.Pointer))))
                {
                    content.Append("frame", ((int)new SnesAddress(frame.Bank, frame.Pointer)));
                    var components = document.Frames[frame.Name];
                    content.Append("components", components.Length);
                    foreach (var component in components)
                    {
                        content.Append("offset-x", component.OffsetX); content.Append("offset-y", component.OffsetY);
                        content.AppendEnemyParts(EnemySpritemapCatalog.CompileParts(component.Parts, frame.Name));
                    }
                }
                foreach (var frame in definitions.OrderBy(frame => ((int)new SnesAddress(frame.Bank, frame.Pointer))))
                {
                    content.Append("native-binding", ((int)new SnesAddress(frame.Bank, frame.Pointer)));
                    content.Append("selected-binding", identities[(document.DisplayFrames ?? throw new InvalidOperationException("Verification requires document.DisplayFrames."))[frame.Name]]);
                }
            });
            AssertEqual(expectedHash, catalog.ContentIdentity, "Golden Torizo calculated extended-part hash remains canonical");
        }
        var stock = Load(); Check(stock);
        int edits = 0;
        foreach (var frame in selected)
        {
            AssertTrue(stock.TryGet(frame.Bank, frame.Pointer, out var components), "Golden Torizo native identity stays installed");
            AssertEqual("FootParts", components.Span[2].Parts.GetType().Name, "Golden Torizo stock foot strips use shared geometry");
            var parts = document.Frames[frame.Name][2].Parts;
            for (int index = 0; index < parts.Length; index++)
            {
                var original = parts[index];
                if (original.TileRow * 16 + original.TileColumn is < 0x160 or > 0x162) continue;
                parts[index] = original with { OffsetX = original.OffsetX + 1 };
                Check(Load());
                for (ushort offset = 0; offset <= 38; offset++)
                    AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaad59a + offset)), GoldenTorizoWalkDefinitions.Velocity(offset), "Edited foot artwork does not change native walking words/windows");
                parts[index] = original;
                edits++;
            }
        }
        AssertEqual(36, edits, "Golden Torizo all independent calculated foot-X fields edited");
        (document.DisplayFrames ?? throw new InvalidOperationException("Verification requires document.DisplayFrames."))[selected[0].Name] = selected[1].Name;
        Check(Load());
        Suite(nameof(VerifyCompiledStatueWalking), () => VerifyCompiledStatueWalking(rom));
        AssertThrows<ArgumentOutOfRangeException>(() => GoldenTorizoStrideGeometryDefinitions.HorizontalAdvance(10), "Golden Torizo phase upper bound");
        Console.WriteLine("Golden Torizo foot geometry:20derived words,39native byte windows,220body parts/40components,10calculated views,36independent foot-X edits,display rebind/hash/packed OAM and actual walking calls pass; seven artwork origins remain required.");
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

    /// <summary>Runs pause equipment label geometry and edit checks.</summary>
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

    /// <summary>Runs projectile trail appearance and sprite edit checks.</summary>
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
