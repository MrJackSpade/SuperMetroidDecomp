using SuperMetroid.Core.Frontend;
using System.Reflection;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream2YardGroups()
    {
        var frames = YardVisualDefinitions.Frames();
        AssertEqual(104, frames.Length, "Yard semantic groups preserve complete registry");
        string identity = string.Join("\n", frames.Select(frame => $"{frame.Bank:X2}:{frame.Pointer:X4}:{frame.Name}"));
        string hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(identity)));
        AssertEqual("2E09CB87CB8C4106F823834D08281C250FB4294981A52F4FE9F83BD983732954", hash,
            "Yard exact pre-change native bank/pointer/name/order identity");
        Console.WriteLine("Yard groups: 38 semantic program roles preserve all104 native frame identities/names/order.");
    }

    private static void VerifyLookupStream2ChozoFootGeometry(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        int[] nativeIdentities = [0xaae943, 0xaae9ae, 0xaaea1e, 0xaaea8e, 0xaaeafe, 0xaaeb69, 0xaaebd9, 0xaaec49];
        var definitions = EnemySpritemapDefinitions.Frames.ToArray();
        var document = new EnemySpritemapDocument
        {
            Version = (int)EnemySpritemapSchema.Current,
            Frames = definitions.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>(), StringComparer.Ordinal),
            DisplayFrames = definitions.ToDictionary(frame => frame.Name, frame => frame.Name, StringComparer.Ordinal),
        };
        var selected = definitions.Where(frame => nativeIdentities.Contains((frame.Bank << 16) | frame.Pointer)).ToArray();
        AssertEqual(8, selected.Length, "Eight native Chozo stride composition identities");
        for (int pose = 0; pose < nativeIdentities.Length; pose++)
            AssertEqual(nativeIdentities[pose], ChozoStrideGeometryDefinitions.NativePoseIdentity(pose), "Chozo counted-record identity geometry");
        foreach (var frame in selected)
        {
            int source = (frame.Bank << 16) | frame.Pointer;
            document.Frames[frame.Name] = Enumerable.Range(0, Word(source)).Select(index =>
            {
                int entry = source + 2 + index * 5;
                var x = new SnesSpritemapXWord(Word(entry));
                var attributes = new SnesObjAttributeWord(Word(entry + 3));
                return new SpriteVisualPart
                {
                    OffsetX = x.SignedOffset, OffsetY = unchecked((sbyte)rom.ReadByte(entry + 2)), Size = x.IsLarge ? 16 : 8,
                    TileColumn = attributes.TileNumber % 16, TileRow = attributes.TileNumber / 16,
                    Palette = attributes.PaletteIndex, Priority = attributes.Priority,
                    FlipX = attributes.FlipHorizontally, FlipY = attributes.FlipVertically,
                };
            }).ToArray();
        }
        EnemySpritemapCatalog Load() => EnemySpritemapCatalog.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions)));
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
                foreach (var frame in definitions.OrderBy(frame => (frame.Bank << 16) | frame.Pointer))
                {
                    content.Append("frame", (frame.Bank << 16) | frame.Pointer);
                    content.AppendEnemyParts(EnemySpritemapCatalog.CompileParts(document.Frames[frame.Name], frame.Name));
                }
                foreach (var frame in definitions.OrderBy(frame => (frame.Bank << 16) | frame.Pointer))
                {
                    content.Append("native-binding", (frame.Bank << 16) | frame.Pointer);
                    content.Append("selected-binding", (frame.Bank << 16) | frame.Pointer);
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
                AssertEqual(ChozoCarryMotionDefinitions.Read(10).Velocity, (short)-0x300, "Chozo edited art does not move carry mechanics");
                AssertEqual(ChozoCarryMotionDefinitions.Read(12).Velocity, (short)-0xe00, "Chozo edited art does not alter late support displacement");
            }
        }
        Suite(nameof(VerifyCompiledStatueWalking), () => VerifyCompiledStatueWalking(rom));
        AssertThrows<ArgumentOutOfRangeException>(() => ChozoStrideGeometryDefinitions.SupportFootX(4), "Chozo support-phase upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => ChozoStrideGeometryDefinitions.NativePoseIdentity(-1), "Chozo pose lower bound");
        Console.WriteLine("Chozo shared geometry:174 native parts,eight stock views,16 independent foot-X edits,hash/display order and existing96-word/actual carry checks pass; shape and two movement-policy inputs remain documented.");
    }
    private static void VerifyLookupStream2GhostPalette(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        var cgram = new SnesCgram();
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, cgram);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.PaletteIndex = 0;
        var state = new WreckedShipGhostEnemyState(slot) { PhaseTimer = 2 };
        var brightening = typeof(RoomEnemySystem).GetMethod("RunWreckedShipGhostBrightening", flags)!
            .CreateDelegate<Action<RoomEnemySlot, WreckedShipGhostEnemyState>>(enemies);
        var fade = typeof(RoomEnemySystem).GetMethod("StepWreckedShipGhostPaletteTowardTarget", flags)!
            .CreateDelegate<Func<RoomEnemySlot, WreckedShipGhostEnemyState, int>>(enemies);
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
            int color = (rom.ReadByte(address) >> bit & 1) | (rom.ReadByte(address + 1) >> bit & 1) << 1 |
                (rom.ReadByte(address + 16) >> bit & 1) << 2 | (rom.ReadByte(address + 17) >> bit & 1) << 3;
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
            int color = (rom.ReadByte(address) >> bit & 1) | (rom.ReadByte(address + 1) >> bit & 1) << 1 |
                (rom.ReadByte(address + 16) >> bit & 1) << 2 | (rom.ReadByte(address + 17) >> bit & 1) << 3;
            kagoUsage[color]++;
        }
        AssertTrue(kagoUsage.SequenceEqual(expectedKagoUsage), "Exact native Kago pixel-label usage supports each shared paint role; slot0 is transparent");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.PaletteColor(-1), "Ghost color lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.PaletteColor(16), "Ghost color upper bound");
        Console.WriteLine("Ghost palette:16 native targets and shared-source words,actual white-flash target copy,32 full component-fade steps,and native pixel-slot usage pass; calculated channels preserve the specified source paint and transparent payload.");
    }

    private static void VerifyLookupStream2TourianAccentCadence(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyOldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions), () => VerifyOldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions(rom));
        foreach (var definition in OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.All)
        {
            ushort[] nativeDurations = new ushort[15];
            int cycle = 0;
            for (int frame = 0; frame < 15; frame++)
            {
                nativeDurations[frame] = ReadVerificationWord(rom, 0x8d0000 | definition.FramePointer(frame));
                AssertEqual(nativeDurations[frame], OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.Duration(frame), "Exact native authored pass dwell");
                cycle += nativeDurations[frame];
                for (int color = 0; color < 3; color++)
                    AssertEqual(ReadVerificationWord(rom, 0x8d0000 | definition.ColorPointer(frame % 5, color)),
                        ReadVerificationWord(rom, 0x8d0000 | definition.ColorPointer(frame, color)), "Three sweeps select identical RGB stages despite distinct cadence");
            }
            AssertEqual(64, cycle, "Native pass cadence sums to its exact loop period");
            var guarded = new PaletteFxMechanicsForbiddenBus(rom);
            var actual = new RoomPaletteFxSystem();
            actual.SpawnDefinition(guarded, definition.DefinitionPointer, equippedItems: 0);
            var cgram = new SnesCgram();
            var colors = new ReferencePaletteFxColorSource(guarded);
            int frameIndex = 0, elapsed = 0;
            for (int tick = 0; tick <= cycle; tick++)
            {
                if (elapsed == nativeDurations[frameIndex]) { frameIndex = (frameIndex + 1) % 15; elapsed = 0; }
                actual.Step(guarded, cgram, colors, 0, 0, false, false);
                for (int color = 0; color < 3; color++)
                    AssertEqual(ReadVerificationWord(rom, 0x8d0000 | definition.ColorPointer(frameIndex, color)),
                        cgram.Colors[definition.ColorByteIndex / 2 + color], "Actual CGRAM follows each exact native hold and loop boundary");
                elapsed++;
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts, "Cadence execution avoids native mechanics reads");
        }
        AssertThrows<IndexOutOfRangeException>(() => OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.Duration(-1), "Accent duration lower domain");
        AssertThrows<IndexOutOfRangeException>(() => OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.Duration(15), "Accent duration upper domain");
        Console.WriteLine("Old Tourian accent cadence:30 native durations,90 repeated color words,68 mechanics words and130 actual per-tick CGRAM/loop states pass with mechanics reads blocked.");
    }
    private static void VerifyLookupStream2NinjaProgramLayout()
    {
        AssertEqual(308, NinjaSpacePirateInstructionProgramDefinitions.MechanicsWordCount, "Complete native Ninja mechanics count");
        AssertEqual(140, NinjaSpacePirateInstructionProgramDefinitions.PresentationWordCount, "Complete native Ninja visual count");
        ushort previous = 0;
        for (int index = 0; index < 308; index++)
        {
            ushort address = NinjaSpacePirateInstructionProgramDefinitions.MechanicsWord(index).Address;
            AssertTrue(address > previous, "Calculated Ninja controls preserve strict unique native order");
            previous = address;
        }
        previous = 0;
        for (int index = 0; index < 140; index++)
        {
            ushort address = NinjaSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(address > previous, "Calculated Ninja selectors preserve strict unique native order");
            previous = address;
        }
        AssertThrows<IndexOutOfRangeException>(() => NinjaSpacePirateInstructionProgramDefinitions.MechanicsWord(-1), "Ninja control lower bound");
        AssertThrows<IndexOutOfRangeException>(() => NinjaSpacePirateInstructionProgramDefinitions.MechanicsWord(308), "Ninja control upper bound");
        AssertThrows<IndexOutOfRangeException>(() => NinjaSpacePirateInstructionProgramDefinitions.PresentationWordAddress(-1), "Ninja selector lower bound");
        AssertThrows<IndexOutOfRangeException>(() => NinjaSpacePirateInstructionProgramDefinitions.PresentationWordAddress(140), "Ninja selector upper bound");
        Suite(nameof(VerifyNinjaSpacePirateInstructionProgramDefinitions), () => VerifyNinjaSpacePirateInstructionProgramDefinitions());
    }

    private static void VerifyLookupStream2ProjectileIdentityGeometry(ISnesAddressSpace rom)
    {
        var expected = new SortedSet<ushort>();
        foreach (ushort record in SamusProjectileRadiusDefinitions.TimedRecordPointers)
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
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var frames = (System.Collections.IDictionary)typeof(ProjectileSpriteCatalog).GetField("frames", fields)!.GetValue(selected)!;
        int calculatedCompositions = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.SingleBeamParts)
                calculatedCompositions++;
        AssertEqual(12, calculatedCompositions, "Eight Power and four Ice compositions calculate without cached part arrays");
        int calculatedQuads = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.ChargedBeamParts)
                calculatedQuads++;
        AssertEqual(19, calculatedQuads, "Nineteen charged Power/Ice quadrant compositions calculate without cached part arrays");
        int calculatedWaves = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.WaveParts)
                calculatedWaves++;
        AssertEqual(33, calculatedWaves, "All33 stock Wave compositions share directional geometry without cached parts");
        int calculatedLobes = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.VerticalChargedWaveParts)
                calculatedLobes++;
        AssertEqual(20, calculatedLobes, "Both charged-Wave families share centered/vertical lobe geometry without cached parts");
        int calculatedMissiles = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.AxialSuperMissileParts)
                calculatedMissiles++;
        AssertEqual(4, calculatedMissiles, "Four SuperMissile axial poses share centered strip geometry without cached parts");
        int calculatedDiagonalMissiles = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.DiagonalMissileParts)
                calculatedDiagonalMissiles++;
        AssertEqual(8, calculatedDiagonalMissiles, "Eight diagonal missile poses share reflected footprint geometry without cached part arrays");
        int calculatedSimpleEffects = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.SimpleEffectParts)
                calculatedSimpleEffects++;
        AssertEqual(8, calculatedSimpleEffects, "Four centered Bomb poses and four mirrored beam explosions calculate without cached parts");
        int calculatedHorizontalLobes = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.HorizontalChargedWaveParts)
                calculatedHorizontalLobes++;
        AssertEqual(16, calculatedHorizontalLobes, "Sixteen horizontal charged Wave lobes calculate geometry with exact native corner order");
        int calculatedSpazerSeeds = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.SpazerSeedParts)
                calculatedSpazerSeeds++;
        AssertEqual(8, calculatedSpazerSeeds, "Eight Spazer seed poses share tile/reflection geometry and one required diagonal origin");
        int calculatedSpazerSpreads = 0;
        foreach (SpriteComposition composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.SpazerDiagonalSpreadParts)
                calculatedSpazerSpreads++;
        AssertEqual(16, calculatedSpazerSpreads, "Sixteen diagonal Spazer spreads calculate repeated seed lanes");
        int calculatedAxialSpreads = 0;
        foreach (var composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.SpazerAxialSpreadParts)
                calculatedAxialSpreads++;
        AssertEqual(19, calculatedAxialSpreads, "Nineteen axial Spazer spreads calculate repeated lanes with exact traversal");
        int calculatedChargedSpazer = 0;
        foreach (var composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.HorizontalChargedSpazerParts)
                calculatedChargedSpazer++;
        AssertEqual(6, calculatedChargedSpazer, "Six horizontal charged Spazer compositions calculate strip adjacency and lane geometry");
        int calculatedVerticalSpazer = 0;
        foreach (var composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.VerticalChargedSpazerSpreadParts)
                calculatedVerticalSpazer++;
        AssertEqual(4, calculatedVerticalSpazer, "Four vertical charged Spazer spreads calculate cell adjacency and mirrored side columns");
        int calculatedSpazerStartup = 0;
        foreach (var composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.SpazerAxialStartupParts)
                calculatedSpazerStartup++;
        AssertEqual(12, calculatedSpazerStartup, "Twelve axial Spazer startup poses calculate centered single/pair geometry");
        int calculatedDiagonalStartup = 0;
        foreach (var composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.SpazerDiagonalStartupParts)
                calculatedDiagonalStartup++;
        AssertEqual(6, calculatedDiagonalStartup, "Six diagonal Spazer startup poses calculate centered pair-strip geometry");
        int calculatedAlternateStartup = 0, calculatedPlasmaCores = 0;
        foreach (var composition in frames.Values)
        {
            object? parts = typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition);
            if (parts is ProjectileSpriteDefinitions.AlternateDiagonalStartupParts) calculatedAlternateStartup++;
            if (parts is ProjectileSpriteDefinitions.PlasmaStartupCoreParts) calculatedPlasmaCores++;
        }
        AssertEqual(4, calculatedAlternateStartup, "Four alternate diagonal startup poses calculate rotated/reflected endcap adjacency");
        AssertEqual(8, calculatedPlasmaCores, "Eight Plasma startup cores calculate centered cell geometry");
        int calculatedPlasmaShort = 0;
        foreach (var composition in frames.Values)
            if (typeof(SpriteComposition).GetField("parts", fields)!.GetValue(composition) is ProjectileSpriteDefinitions.HorizontalPlasmaWaveShortParts)
                calculatedPlasmaShort++;
        AssertEqual(5, calculatedPlasmaShort, "Five horizontal PlasmaWave Short poses calculate cell and lobe geometry");
        var firstCharged = (SpriteComposition)frames[(ushort)0xec3e]!;
        AssertTrue(typeof(SpriteComposition).GetField("parts", fields)!.GetValue(firstCharged) is CompiledSpritePart[], "Distinct initial charged-Power ordering stays explicitly supplied/pending");
        foreach (ushort pointer in expected)
        {
            var native = new OamBuffer();
            var actual = new OamBuffer();
            DrawImportedProjectileSpritemap(rom, native, pointer, 255, 255);
            selected.Draw(pointer, actual, 255, 255);
            AssertTrue(native.LowTable.SequenceEqual(actual.LowTable) && native.HighTable.SequenceEqual(actual.HighTable), "All417 calculated/remaining identities extract and draw exact native OAM");
        }
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        var document = System.Text.Json.JsonSerializer.Deserialize<ProjectileSpriteDocument>(json, options)!;
        const ushort superMissilePointer = 0xadd5;
        string superMissileName = ProjectileSpriteDefinitions.Name(superMissilePointer);
        var originalMissilePart = document.Frames[superMissileName][1];
        document.Frames[superMissileName][1] = originalMissilePart with { OffsetX = originalMissilePart.OffsetX + 1 };
        var editedMissile = ProjectileSpriteCatalog.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, options)));
        var originalMissileOam = new OamBuffer();
        var editedMissileOam = new OamBuffer();
        selected.Draw(superMissilePointer, originalMissileOam, 255, 255);
        editedMissile.Draw(superMissilePointer, editedMissileOam, 255, 255);
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
        selected.Draw(editedPointer, baselineOam, 255, 255);
        edited.Draw(editedPointer, editedOam, 255, 255);
        AssertEqual(unchecked((byte)(baselineOam.LowTable[0] + 1)), editedOam.LowTable[0], "Independent composition edit reaches calculated identity");
        document.Frames[editedName][0] = before;
        var ownedOam = new OamBuffer();
        edited.Draw(editedPointer, ownedOam, 255, 255);
        AssertTrue(ownedOam.LowTable.SequenceEqual(editedOam.LowTable), "Loaded composition owns independent edited content");
        const ushort quadPointer = 0xed9e;
        string quadName = ProjectileSpriteDefinitions.Name(quadPointer);
        var originalQuadPart = document.Frames[quadName][0];
        document.Frames[quadName][0] = originalQuadPart with { FlipX = !originalQuadPart.FlipX };
        var editedQuadCatalog = ProjectileSpriteCatalog.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, options)));
        var nativeQuad = new OamBuffer();
        var editedQuad = new OamBuffer();
        selected.Draw(quadPointer, nativeQuad, 255, 255);
        editedQuadCatalog.Draw(quadPointer, editedQuad, 255, 255);
        byte[] expectedQuad = nativeQuad.LowTable.ToArray();
        expectedQuad[3] ^= 0x40;
        AssertTrue(expectedQuad.AsSpan().SequenceEqual(editedQuad.LowTable) && nativeQuad.HighTable.SequenceEqual(editedQuad.HighTable), "Independent one-quadrant reflection edit changes only its exact emitted OAM attribute bit");
        const ushort wavePointer = 0xaea4;
        string waveName = ProjectileSpriteDefinitions.Name(wavePointer);
        var wavePart = document.Frames[waveName][0];
        document.Frames[waveName][0] = wavePart with { OffsetY = wavePart.OffsetY + 1 };
        var editedWaveCatalog = ProjectileSpriteCatalog.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, options)));
        var nativeWave = new OamBuffer();
        var editedWave = new OamBuffer();
        selected.Draw(wavePointer, nativeWave, 255, 255);
        editedWaveCatalog.Draw(wavePointer, editedWave, 255, 255);
        byte[] expectedWave = nativeWave.LowTable.ToArray();
        expectedWave[1]++;
        AssertTrue(expectedWave.AsSpan().SequenceEqual(editedWave.LowTable) && nativeWave.HighTable.SequenceEqual(editedWave.HighTable), "Independent diagonal Wave Y edit changes exactly its emitted coordinate and preserves X ninth bit");
        var flare = ChargeFlareSpriteCatalog.Load(new MemoryStream(ChargeFlareSpriteExtractor.Extract(rom)));
        for (ushort selector = 0; selector < ChargeFlareSpriteDefinitions.Selectors.Length; selector++)
        {
            var native = new OamBuffer();
            var actual = new OamBuffer();
            DrawImportedProjectileSpritemap(rom, native, ChargeFlareSpriteDefinitions.Selectors[selector], 255, 255);
            flare.Draw(selector, actual, 255, 255);
            AssertTrue(native.LowTable.SequenceEqual(actual.LowTable) && native.HighTable.SequenceEqual(actual.HighTable), "Existing flare span caller preserves exact extracted OAM");
        }
        AssertThrows<IndexOutOfRangeException>(() => _ = ProjectileSpriteDefinitions.NativePointers[-1], "Projectile identity lower bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = ProjectileSpriteDefinitions.NativePointers[417], "Projectile identity upper bound");
        Console.WriteLine("Projectile identity geometry:417 exact identities from805 native selectors,48 physical startup records,417 actual extracted OAM draws,independent composition edit/ownership,all existing flare selectors and bounds pass;all417 identities and 208 stock beam/missile/effect compositions calculate; independent frame selection/composition/art inputs remain pending.");
    }
    private static void VerifyLookupStream2EnvironmentalCatalogs(CartridgeImportAddressSpace rom)
    {
        var norfair = NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All;
        var accents = OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.All;
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
        Suite(nameof(VerifyNorfairEnvironmentalPaletteFxProgramMechanicsDefinitions), () => VerifyNorfairEnvironmentalPaletteFxProgramMechanicsDefinitions(rom));
        Suite(nameof(VerifyOldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions), () => VerifyOldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions(rom));
        Console.WriteLine("Environmental catalogs:six native definition/color/loop bindings,292mechanics words,16heat phase bytes,actual complete/repeating cycles,live colors,enumeration/order and bounds pass; accent timing and independent colors remain pending.");
    }
    private static void VerifyLookupStream2BackdropGeometry(SuperMetroidAddressSpace rom)
    {
        byte[] json = PauseBackdropExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<PauseBackdropDocument>(json, MapPresentationFormat.JsonOptions)!;
        var stock = PauseBackdropPresentation.Load(new MemoryStream(json));
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
        var areaResiduals = (Dictionary<int, ushort>[])typeof(PauseBackdropPresentation).GetField("areas", fields)!.GetValue(stock)!;
        var buttonResiduals = (Dictionary<int, ushort>)typeof(PauseBackdropPresentation).GetField("buttons", fields)!.GetValue(stock)!;
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
        AssertTrue(nativeButtons.AsSpan().SequenceEqual(stock.CreateButtonTilemap()), "All512native button-template words");
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
            AssertTrue(nativeButtons.AsSpan().SequenceEqual(selected.CreateButtonTilemap()), "Area edit does not propagate into independent button template");
        }
        foreach (int edit in new[] { 0, 8 * 32 + 2, 9 * 32 + 5, 10 * 32 + 29 })
        {
            var cells = document.Buttons.ToArray();
            cells[edit] = cells[edit] with { FlipY = !cells[edit].FlipY, Palette = 5 };
            using var output = new MemoryStream();
            PauseBackdropPresentation.Write(output, document with { Buttons = cells });
            var selected = PauseBackdropPresentation.Load(new MemoryStream(output.ToArray()));
            AssertTrue(PauseTileGrid.Compile(cells, "Edited buttons").AsSpan().SequenceEqual(selected.CreateButtonTilemap()), "Independent control-template edit");
            ConfirmArea(selected, AreaId.Crateria, PauseTileGrid.Compile(document.Areas[AreaId.Crateria.ToString()], "Unchanged area"));
        }
        var invalidVram = new SnesVram();
        AssertThrows<ArgumentOutOfRangeException>(() => stock.LoadTo(invalidVram, -1, AreaId.Crateria), "Backdrop negative VRAM destination");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.LoadTo(invalidVram, SnesVram.ByteCount - 2047, AreaId.Crateria), "Backdrop end-of-VRAM bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.LoadTo(invalidVram, 0, (AreaId)7), "Backdrop area bound");
        AssertTrue(invalidVram.Bytes.ToArray().All(value => value == 0), "Invalid load fails before any VRAM mutation");
        Console.WriteLine($"Backdrop geometry:7680native words, exact{areaResiduals.Sum(cells => cells.Count)}area/{buttonResiduals.Count}button residuals,53actual VRAM loads,10independent edits and bounds pass; selected layout/art/style inputs remain required.");

        static void ConfirmArea(PauseBackdropPresentation presentation, AreaId area, byte[] native)
        {
            var vram = new SnesVram();
            byte[] expected = Enumerable.Repeat((byte)0x55, SnesVram.ByteCount).ToArray();
            vram.LoadBytes(0, expected);
            native.CopyTo(expected, 0x7000);
            presentation.LoadTo(vram, 0x7000, area);
            AssertTrue(expected.AsSpan().SequenceEqual(vram.Bytes), "Actual area load preserves every selected word and untouched VRAM byte");
        }
    }
    private static void VerifyLookupStream2EquipmentBaseGeometry(SuperMetroidAddressSpace rom)
    {
        byte[] json = PauseEquipmentBaseExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<PauseEquipmentBaseDocument>(json, MapPresentationFormat.JsonOptions)!;
        var stock = PauseEquipmentBasePresentation.Load(new MemoryStream(json));
        byte[] native = new byte[2048];
        for (int cell = 0; cell < 1024; cell++)
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(native.AsSpan(cell * 2),
                ReadVerificationWord(rom, 0xb6e800 + cell * 2));
        AssertTrue(native.AsSpan().SequenceEqual(stock.CreateTilemap()), "Calculated equipment template preserves all1024native words");
        var residuals = (Dictionary<int, ushort>)typeof(PauseEquipmentBasePresentation)
            .GetField("remainingCells", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
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
            AssertTrue(expected.AsSpan().SequenceEqual(selected.CreateTilemap()), "Independent selected glyph/palette/flip edit preserves every other cell and mirror partner");
            ConfirmRebind(selected, expected);
        }
        byte[] mutable = stock.CreateTilemap();
        mutable[0] ^= 1;
        AssertTrue(native.AsSpan().SequenceEqual(stock.CreateTilemap()), "Mutable menu state does not alter immutable template");
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
                AssertTrue(expected.AsSpan().SequenceEqual(actual), "Actual base refresh preserves live ownership and arrow palette while applying selected artwork");
            }
        }
    }
    private static void VerifyLookupStream2WireframeMirrors(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        byte[] json = PauseWireframeExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<PauseWireframeDocument>(json,
            MapPresentationFormat.JsonOptions)!;
        var stock = PauseWireframePresentation.Load(new MemoryStream(json));
        int pieceCells = 0, stockOverrides = 0;
        foreach (PauseWireframeKind kind in Enum.GetValues<PauseWireframeKind>())
        {
            int pointer = Word(PauseWireframeDefinitions.Pointers + (int)kind * 2) | 0x820000;
            var native = new byte[PauseWireframeDefinitions.Cells * 2];
            for (int cell = 0; cell < PauseWireframeDefinitions.Cells; cell++)
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(native.AsSpan(cell * 2), Word(pointer + cell * 2));
            Confirm(stock, kind, native, "Actual mirrored stock wireframe preserves every native tile word");
            const BindingFlags privateFields = BindingFlags.Instance | BindingFlags.NonPublic;
            object frame = ((Array)typeof(PauseWireframePresentation).GetField("frames", privateFields)!.GetValue(stock)!).GetValue((int)kind)!;
            var edits = (Dictionary<int, ushort>)frame.GetType().GetField("edits", privateFields)!.GetValue(frame)!;
            stockOverrides += edits.Count;
            for (int cell = 0; cell < PauseWireframeDefinitions.Cells; cell++)
            {
                ushort actualNative = Word(pointer + cell * 2);
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
            AssertTrue(expected.AsSpan().SequenceEqual(actual), context);
        }
    }

    private static void VerifyLookupStream2ReserveLabels(SuperMetroidAddressSpace rom)
    {
        byte[] json = PauseReserveUiExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<PauseReserveUiDocument>(json,
            MapPresentationFormat.JsonOptions)!;
        int words = 0;
        foreach ((string name, PauseReserveLabelVisual label) in document.Labels)
        {
            byte[] native = PauseTileGrid.Compile(label.Cells, name);
            int nativeOffset = (label.Anchor.Row * 32 + label.Anchor.Column) * 2;
            AssertEqual(nativeOffset, PauseReserveUiDefinitions.StockLabelOffset(name), "Native reserve text destination");
            for (int cell = 0; cell < label.Cells.Length; cell++)
            {
                AssertEqual(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(cell * 2)),
                    PauseReserveUiDefinitions.StockLabelWord(name, cell), "Native reserve glyph/palette/priority");
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
                    selected.ApplyLabel(actual, name, preserve);
                    AssertTrue(expected.AsSpan().SequenceEqual(actual), "Actual reserve label patch preserves edits, attribute policy and surrounding bytes");
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
            moved.ApplyLabel(movedActual, name);
            AssertTrue(movedExpected.AsSpan().SequenceEqual(movedActual), "Independent reserve label placement edit");
        }
        AssertEqual(22, words, "All native reserve label words");
        var stock = PauseReserveUiPresentation.Load(new MemoryStream(json));
        AssertThrows<InvalidDataException>(() => stock.ApplyLabel(new byte[2048], "unknown"), "Unknown reserve label rejected");
        AssertThrows<ArgumentException>(() => stock.ApplyLabel(new byte[1], "Manual"), "Short reserve destination rejected");
        AssertThrows<IndexOutOfRangeException>(() => PauseReserveUiDefinitions.StockLabelWord("Auto", 4), "Reserve glyph upper bound rejected");
        Console.WriteLine("Reserve labels:22 native words/four destinations,22 independent cell edits,56 actual attribute-preserving/ordinary/moved patches and bounds pass; independent glyph/arrow inputs remain required.");
    }

    private static void VerifyLookupStream2EquipmentLabels(SuperMetroidAddressSpace rom)
    {
        byte[] json = PauseEquipmentLabelExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<PauseEquipmentLabelDocument>(json,
            MapPresentationFormat.JsonOptions)!;
        var stock = PauseEquipmentLabelPresentation.Load(new MemoryStream(json));
        int words = 0;
        foreach ((string key, PauseEquipmentLabel label) in document.Labels)
        {
            byte[] native = PauseTileGrid.Compile(label.Cells, key);
            AssertEqual((label.Row * 32 + label.Column) * 2,
                PauseEquipmentLabelDefinitions.StockDestinationByte(key), "Native semantic label placement");
            for (int cell = 0; cell < label.Cells.Length; cell++)
            {
                AssertEqual(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(native.AsSpan(cell * 2)),
                    PauseEquipmentLabelDefinitions.StockWord(key, cell), "Native packed glyph fragment");
                words++;
            }
        }
        int changedCells = 0;
        foreach (var identity in PauseEquipmentLabelDefinitions.Labels())
        {
            string key = identity.Key;
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
                    AssertTrue(expected.AsSpan().SequenceEqual(actual), "Actual label patch/disabled recolor preserves every cell and surrounding byte");
                }
                if (edit >= 0) changedCells++;
            }
        }
        byte[] overrun = new byte[32 * 32 * 2];
        stock.ApplyLabel(overrun, PauseEquipmentCategory.Beams, 4, 9, false);
        PauseEquipmentLabel plasma = document.Labels[PauseEquipmentLabelDefinitions.PlasmaKey];
        PauseEquipmentLabel varia = document.Labels[PauseEquipmentLabelDefinitions.VariaKey];
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
        AssertTrue(movedExpected.AsSpan().SequenceEqual(movedActual),
            "Independent moved label retains its edited destination and untouched original cell");
        AssertEqual(115, words, "All native equipment glyph words");
        AssertEqual(106, changedCells, "All ordinary label glyph cells edited independently");
        AssertThrows<ArgumentOutOfRangeException>(() => PauseEquipmentLabelDefinitions.StockWord("unknown", 0), "Unknown label rejected");
        AssertThrows<IndexOutOfRangeException>(() => PauseEquipmentLabelDefinitions.StockWord("Beam.Ice", -1), "Negative label cell rejected");
        AssertThrows<IndexOutOfRangeException>(() => PauseEquipmentLabelDefinitions.StockWord("Beam.Ice", 5), "Label cell upper bound rejected");
        Console.WriteLine("Pause equipment labels:115 native words/15placements,106 ordinary and five Hyper cell edits, disabled recoloring, exact Plasma/Varia overrun, source hashes and bounds pass; glyph artwork remains independent.");
    }

    private static void VerifyLookupStream2EquipmentBlank(SuperMetroidAddressSpace rom)
    {
        byte[] nativeJson = PauseEquipmentLabelExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<PauseEquipmentLabelDocument>(
            nativeJson, MapPresentationFormat.JsonOptions)!;
        for (int cell = 0; cell < PauseEquipmentLabelDefinitions.EquipmentWords; cell++)
        {
            int address = 0x820000 | (PauseEquipmentLabelDefinitions.BlankSource + cell * 2);
            AssertEqual((byte)0, rom.ReadByte(address), "Native blank low byte");
            AssertEqual((byte)0, rom.ReadByte(address + 1), "Native blank high byte");
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
            var edits = (Dictionary<int, ushort>)typeof(PauseEquipmentLabelPresentation)
                .GetField("blankEdits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(presentation)!;
            AssertEqual(edited < 0 ? 0 : 1, edits.Count, "Only explicit blank edits are stored");
            foreach (bool hyper in new[] { false, true })
            {
                byte[] actual = Enumerable.Repeat((byte)0x55, 32 * 32 * 2).ToArray();
                byte[] expected = actual.ToArray();
                byte[] blankBytes = PauseTileGrid.Compile(blank, "Reference blank");
                foreach (var identity in PauseEquipmentLabelDefinitions.Labels())
                {
                    PauseEquipmentLabel label = selected.Labels[identity.Key];
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
                AssertTrue(expected.AsSpan().SequenceEqual(actual),
                    "Actual uncollected inventory/Hyper blank writes preserve every edited cell and untouched byte");
            }
        }
        Console.WriteLine("Pause equipment blank: nine native empty cells, stock/no stored payload, nine independent edits, serialized hashes and20 actual inventory/Hyper tilemap writes pass.");
    }

    private static void VerifyLookupStream2DeadTorizoGeometry(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var expected = new byte[0x1000];
        for (int row = 0; row < 12; row++)
        {
            int cursor = 0xa9de18 + row * 14;
            int source = Word(cursor + 2) - 0xa800;
            int destination = Word(cursor + 5) - 0x2000;
            int length = Word(cursor + 8) + 1;
            AssertEqual(new DeadTorizoGraphicsCopy(source, destination, length),
                DeadTorizoGeometryDefinitions.InitialCopy(row), "Dead Torizo native MVN descriptor");
            for (int offset = 0; offset < length; offset++)
                expected[destination + offset] = rom.ReadByte(0xb7a800 + source + offset);
        }
        // The crop is the tile union of the native stationary composition, not
        // an occupancy bounding box over the unrelated parts elsewhere in its atlas.
        var visibleTiles = new HashSet<int>();
        int partCount = Word(0xa9d6e2);
        AssertEqual(25, partCount, "Dead Torizo native stationary part count");
        for (int part = 0; part < partCount; part++)
        {
            int address = 0xa9d6e4 + part * 5;
            int tile = (Word(address + 3) & 0x1ff) - 0x100;
            int extent = (Word(address) & 0x8000) == 0 ? 1 : 2;
            int nativeX = Word(address) & 0x1ff;
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
            int first = (Word(cursor + 2) - 0xa800) / 32;
            int count = (Word(cursor + 8) + 1) / 32;
            for (int tile = first; tile < first + count; tile++) copiedTiles.Add(tile);
        }
        AssertEqual(97, visibleTiles.Count, "Dead Torizo native visible tile count");
        AssertEqual(true, visibleTiles.SetEquals(copiedTiles),
            "Dead Torizo twelve native MVNs copy exactly the 25-part stationary composition");
        int[] nativeColumns = [0xa9e280,0xa9e29f,0xa9e2be,0xa9e2d8,0xa9e2f2,
            0xa9e30c,0xa9e326,0xa9e340,0xa9e35a,0xa9e379];
        int[] minimumY = [Word(0xa9e276),Word(0xa9e295),Word(0xa9e2b4),0,0,0,0,0,0,Word(0xa9e36f)];
        for (int column = 0; column < 10; column++)
        {
            AssertEqual((Word(nativeColumns[column]) - 0x2000) / 2,
                DeadTorizoGeometryDefinitions.ColumnWordOffset(column), "Dead Torizo native column word displacement");
            AssertEqual(minimumY[column], DeadTorizoGeometryDefinitions.ColumnMinimumY(column),
                "Dead Torizo native clipping boundary");
        }
        byte[] planar = Enumerable.Range(0, DeadTorizoArtworkDefinitions.ByteCount)
            .Select(index => rom.ReadByte(DeadTorizoArtworkDefinitions.SourceAddress + index)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, RoomCharacterAtlasFormat.TileColumns,
            out int width, out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        png.Position = 0;
        // An independently edited display composition is deliberately unrelated to
        // stock tile coverage. Native staging must still copy the same artwork rows.
        var displayParts = new Dictionary<int, EnemySpritemapParts>
        {
            [0xa9d6e2] = EnemySpritemapParts.FromOwnedArray([new(SnesSpritemapXWord.Create(70, false), 33,
                SnesObjAttributeWord.Create(0, 7, 3, SnesTileFlipFlags.Horizontal))]),
        };
        var displayBindings = new Dictionary<int, int> { [0xa9d6e2] = 0xa9d6e2 };
        var display = (EnemySpritemapCatalog)typeof(EnemySpritemapCatalog)
            .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
                [typeof(Dictionary<int, EnemySpritemapParts>), typeof(Dictionary<int, int>)], null)!
            .Invoke([displayParts, displayBindings]);
        AssertTrue(display.TryGetDisplay(0xa9, 0xd6e2, out EnemySpritemapParts selectedDisplay),
            "Edited Dead Torizo display is selected");
        var editedOam = new OamBuffer();
        editedOam.AddEnemySpritemap(selectedDisplay, 128, 128, 0, 0);
        AssertEqual(4, editedOam.NextByteOffset, "Edited display draws one small part instead of25stock parts");
        AssertEqual((byte)198, editedOam.LowTable[0], "Edited display origin reaches actual OAM");
        var artwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
            new Dictionary<EnemyDefinitionId, RoomCharacterAtlas> { [EnemyDefinitionId.CorpseTorizo] = RoomCharacterAtlas.Load(png, planar.Length) },
            new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>
            {
                [EnemyDefinitionId.CorpseTorizo] = EnemyPaletteSheet.Load(new MemoryStream(
                    EnemyPaletteSheet.Write(new EnemyPaletteSheetDocument
                    {
                        Version = 1,
                        Colors = Enumerable.Range(0,16).Select(_ => new PaletteRgb5 { Red=0, Green=0, Blue=0 }).ToArray(),
                    }))),
            },
            spritemaps: display,
            dmaSources: new Dictionary<EnemyDefinitionId, int> { [EnemyDefinitionId.CorpseTorizo] = DeadTorizoArtworkDefinitions.SourceAddress });
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, rom);
        for (int index = 0; index < expected.Length; index++) rom.WriteByte(0x7e2000 + index, 0);
        typeof(RoomEnemySystem).GetMethod("InitializeDeadTorizoGraphics", flags)!
            .CreateDelegate<Action>(enemies)();
        Compare(expected, "Actual Dead Torizo installed-art crop");

        var state = new DeadTorizoEnemyState(enemies.Slots[0],0,0,0,0,0xe226,0,96,95,94,0x134);
        var moveRow = typeof(RoomEnemySystem).GetMethod("CopyOrMoveDeadTorizoPixelRow", flags)!
            .CreateDelegate<Action<DeadTorizoEnemyState,ushort,bool>>(enemies);
        foreach (bool move in new[] { false, true })
        for (ushort y = 0; y < 96; y++)
        {
            for (int index = 0; index < expected.Length; index++)
            {
                expected[index] = (byte)(index * 73 ^ index >> 3);
                rom.WriteByte(0x7e2000 + index, expected[index]);
            }
            int source = Word(0xa9e226 + y / 8 * 2) + (y & 7) * 2;
            int destination = source + ((y & 7) >= 6 ? 0x134 : 0) + 2;
            for (int column = 0; column < 10; column++)
            {
                if (y < minimumY[column]) continue;
                int nativeOffset = Word(nativeColumns[column]) - 0x2000;
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
            moveRow(state, y, move);
            Compare(expected, "Actual Dead Torizo column row copy/move");
        }
        AssertThrows<IndexOutOfRangeException>(() => DeadTorizoGeometryDefinitions.InitialCopy(-1), "Crop negative row rejected");
        AssertThrows<IndexOutOfRangeException>(() => DeadTorizoGeometryDefinitions.InitialCopy(12), "Crop row upper bound rejected");
        AssertThrows<IndexOutOfRangeException>(() => DeadTorizoGeometryDefinitions.ColumnMinimumY(-1), "Crop negative column rejected");
        AssertThrows<IndexOutOfRangeException>(() => DeadTorizoGeometryDefinitions.ColumnMinimumY(10), "Crop column upper bound rejected");
        Console.WriteLine("Dead Torizo geometry:12 native MVNs/25-part OAM exact97-tile union,10 column operands/clip limits, actual installed-art staging and192 actual row copy/move operations pass; crop derives from immutable stock composition independently of edited display OAM; artwork ownership remains explicit.");

        void Compare(byte[] bytes, string context)
        {
            for (int index = 0; index < bytes.Length; index++)
                AssertEqual(bytes[index], rom.ReadByte(0x7e2000 + index), context);
        }
    }
    private static void VerifyLookupStream2CrawlerRamps(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
        var reset = typeof(RoomEnemySystem).GetMethod("ResetCrawlerVelocitiesFromProperties", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>();
        var setYard = typeof(RoomEnemySystem).GetMethod("SetYardCrawlingVelocities", flags)!
            .CreateDelegate<Action<RoomEnemySlot, YardEnemyState, ushort>>();
        var slot = new RoomEnemySystem().Slots[0];
        var yard = new YardEnemyState(slot);
        for (ushort parameter = 0; parameter < 32; parameter++)
        {
            ushort expected = Word(0xa3e5f0 + parameter * 2);
            AssertEqual(expected, Word(0xa3cca2 + parameter * 2), "Native Yard/crawler magnitude agreement");
            AssertEqual(expected, CrawlerSpeedDefinitions.ForParameter(parameter), "Native calculated speed ramp");
            slot.Parameter1 = parameter;
            for (ushort property = 0; property < 4; property++)
            {
                slot.Properties = (ushort)(0xa000 | property);
                reset(slot);
                AssertEqual(property == 0 ? unchecked((ushort)-expected) : expected, slot.VariableA,
                    "Actual crawler reset X magnitude");
                AssertEqual(property == 2 ? unchecked((ushort)-expected) : expected, slot.VariableB,
                    "Actual crawler reset Y magnitude");
            }
            for (ushort direction = 0; direction < 8; direction++)
            {
                setYard(slot, yard, direction);
                int nativeDirection = 0xa3cd82 + direction * 8;
                AssertEqual(unchecked((ushort)((expected ^ Word(nativeDirection)) + Word(nativeDirection + 2))),
                    yard.CrawlingXVelocity, "Actual Yard reset X magnitude");
                AssertEqual(unchecked((ushort)((expected ^ Word(nativeDirection + 4)) + Word(nativeDirection + 6))),
                    yard.CrawlingYVelocity, "Actual Yard reset Y magnitude");
            }
        }
        AssertThrows<InvalidDataException>(() => CrawlerSpeedDefinitions.ForParameter(32), "Crawler parameter upper bound");
        Console.WriteLine("Crawler speed ramps: both 32 native words and 384 actual crawler/Yard velocity resets pass; irregular gaps/terminal choices remain pending.");
    }
    private static void VerifyLookupStream2LavaJumperLayout(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyNorfairLavaJumperInstructionProgramDefinitions), () => VerifyNorfairLavaJumperInstructionProgramDefinitions(rom));
        int visual = 0;
        int cursor = 0xbe3c;
        while (cursor < 0xbe86)
        {
            ushort word = ReadVerificationWord(rom, 0xa20000 | cursor);
            if (word < 0x8000)
            {
                ushort operand = (ushort)(cursor + 2);
                AssertEqual(operand, NorfairLavaJumperInstructionProgramDefinitions.PresentationWordAddress(visual++), "Lava-jumper calculated pose address follows native instruction widths");
                AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xa2, operand, out ushort selector), "Lava-jumper pose has an installed selector");
                AssertEqual(ReadVerificationWord(rom, 0xa20000 | operand), selector, "Lava-jumper selector matches exact native operand");
                cursor += 4;
            }
            else cursor += word is 0x8123 or 0x8110 or 0x80ed ? 4 : 2;
        }
        AssertEqual(0xbe86, cursor, "Lava-jumper native walker stops before velocity data");
        AssertEqual(14, visual, "All fourteen lava-jumper visual operands independently confirmed");
        Console.WriteLine("Lava-jumper calculated layout:23 native controls,14 native selectors, actual parent/follower programs and handshake pass; seven pose holds remain pending.");
    }
    private static void VerifyLookupStream2ChozoLayout(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyChozoStatueInstructionProgramDefinitions), () => VerifyChozoStatueInstructionProgramDefinitions(rom));
        int visual = 0;
        int mechanics = 0;
        foreach ((int start, int end) in new[] { (0xe39d, 0xe429), (0xe457, 0xe57f) })
        {
            int cursor = start;
            while (cursor < end)
            {
                ushort word = ReadVerificationWord(rom, 0xaa0000 | cursor);
                mechanics++;
                if (word < 0x8000)
                {
                    ushort operand = (ushort)(cursor + 2);
                    AssertEqual(operand, ChozoStatueInstructionProgramDefinitions.PresentationWordAddress(visual++),
                        "Calculated Chozo visual address follows independent native instruction widths");
                    AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xaa, operand, out ushort selector), "Chozo pose resolves its installed selector");
                    AssertEqual(ReadVerificationWord(rom, 0xaa0000 | operand), selector, "Chozo installed selector equals its native operand");
                    cursor += 4;
                }
                else
                {
                    bool operand = word is 0x806b or 0x8123 or 0x8110 or 0xe5d8 or 0xe58f;
                    if (operand) mechanics++;
                    cursor += operand ? 4 : 2;
                }
            }
            AssertEqual(end, cursor, "Chozo native walk terminates exactly before callback code");
        }
        AssertEqual(166, mechanics, "Independent native walk counts all Chozo mechanics words");
        AssertEqual(52, visual, "Independent native walk counts every Chozo pose");
        AssertThrows<ArgumentOutOfRangeException>(() => ChozoStatueInstructionProgramDefinitions.MechanicsWord(166), "Chozo mechanics upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => ChozoStatueInstructionProgramDefinitions.PresentationWordAddress(-1), "Chozo visual lower bound");
        Console.WriteLine("Chozo calculated layouts:166 native controls,52 independently decoded native visual operands, exact list boundaries and existing allocation checks pass; hold/footstep payloads remain pending.");
    }
    private static void VerifyLookupStream2HorizontalCameraTargets(ISnesAddressSpace rom)
    {
        var storage = new byte[RoomScrollGrid.StorageByteCount];
        Array.Fill(storage, (byte)1);
        var grid = RoomScrollGrid.LoadCompiled(new TestAddressSpace(), storage, 3, 2);
        for (ushort mode = 0; mode <= 6; mode += 2)
        foreach (byte facing in new byte[] { 4, 8 })
        for (int reversal = 0; reversal < 4; reversal++)
        {
            var context = new HorizontalCameraContext(
                reversal == 1 ? (ushort)1 : (ushort)0,
                reversal == 2 ? SamusMovementType.Moonwalking : 0,
                reversal == 3 ? (ushort)1 : (ushort)0, facing, mode);
            bool right = reversal == 0 ? facing != 4 : facing == 4;
            ushort nativeOffset = ReadVerificationWord(rom, (right ? 0x90963f : 0x909647) + mode);
            AssertEqual(nativeOffset, HorizontalCameraTargetDefinitions.Offset(mode, right), "Native mode/facing target offset");
            var camera = new ScrollBoundaryCamera(grid);
            camera.SetPosition(100, 0);
            camera.TrackMovedSamusHorizontally(new(200, 0, 100, 0), new(204, 0, 100, 0), context);
            AssertEqual(unchecked((ushort)(204 - nativeOffset)), camera.IdealXPosition, "Actual camera target preserves mode and reversal");
            AssertEqual((ushort)5, camera.CameraXSpeed, "Target dispatch preserves movement speed calculation");
        }
        var invalidCamera = new ScrollBoundaryCamera(grid);
        AssertThrows<ArgumentOutOfRangeException>(() => invalidCamera.TrackMovedSamusHorizontally(
            new(200, 0, 100, 0), new(204, 0, 100, 0), new(0, 0, 0, 8, 1)), "Native mode domain remains validated before tracking");
        Console.WriteLine("Horizontal camera targets:eight native offsets and32 actual mode/facing/reversal targets preserve geometry, speed and domain validation.");
    }
    private static void VerifyLookupStream2TorizoPageDispatch(ISnesAddressSpace rom)
    {
        TorizoInstructionTileSheetDefinition[] original =
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
            native[ordinal] = Enumerable.Range(0, page.ByteCount).Select(offset => rom.ReadByte(page.SourceAddress + offset)).ToArray();
            byte[] pixels = SnesGraphics.DecodePlanarTiles(native[ordinal], 4,
                Math.Min(RoomCharacterAtlasFormat.TileColumns, page.ByteCount / RoomCharacterAtlasFormat.BytesPerTile), out int width, out int height);
            using var png = new MemoryStream();
            IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
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
        AssertThrows<IndexOutOfRangeException>(() => _ = TorizoInstructionVramArtworkDefinitions.All[-1], "Torizo page lower bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = TorizoInstructionVramArtworkDefinitions.All[8], "Torizo page upper bound");
        Console.WriteLine("Torizo page dispatch:eight exact ordered identities, native decoded page resolution/bounds and canonical content hash pass.");
    }
    private static void VerifyLookupStream2KagoFrameGeometry(ISnesAddressSpace rom)
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
    private static void VerifyLookupStream2KraidLintInitialization(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        var state = new KraidEnemyState();
        typeof(RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(enemies, (Func<ushort>)(() => 1));
        var finish = typeof(RoomEnemySystem).GetMethod("FinishKraidGrowth", flags)!
            .CreateDelegate<Action<RoomEnemySlot, KraidEnemyState>>(enemies);
        var align = typeof(RoomEnemySystem).GetMethod("AlignKraidPart", flags)!
            .CreateDelegate<Action<RoomEnemySlot, KraidPartState>>(enemies);
        var body = enemies.Slots[0];
        body.XPosition = 200; body.YPosition = 300;
        for (int slot = 2; slot <= 4; slot++) enemies.Slots[slot].VariableB = 17;
        finish(body, state);
        for (int slot = 2; slot <= 4; slot++)
        {
            var lint = enemies.Slots[slot];
            ushort expected = Word(0xa7a916 + (slot - 2) * 2);
            AssertEqual(expected, KraidLintInitializationDefinitions.InitialDelay((KraidLintPart)slot), "Native per-part initial delay");
            AssertEqual(expected, lint.VariableF, "Actual post-growth timer write");
            AssertEqual((ushort)0, lint.VariableB, "Actual lint growth resets extension");
            AssertEqual(KraidAiFunction.LintProduce, state.Parts[slot].NextFunction, "Actual lint continuation");
            for (int remaining = expected - 1; remaining >= 0; remaining--)
            {
                align(lint, state.Parts[slot]);
                AssertEqual((ushort)remaining, lint.VariableF, "Actual per-part countdown");
                AssertEqual((ushort)(remaining == 0 ? KraidAiFunction.LintProduce : KraidAiFunction.AlignPartToKraid),
                    lint.VariableA, "Actual lint transition occurs on its own final tick");
                AssertEqual(unchecked((ushort)(body.XPosition - lint.XRadius)), lint.XPosition, "Alignment continues during delay");
            }
        }
        foreach (KraidLintPart invalid in new[] { (KraidLintPart)0, (KraidLintPart)1, (KraidLintPart)5 })
            AssertThrows<InvalidOperationException>(() => KraidLintInitializationDefinitions.InitialDelay(invalid), "Only lint slots have launch policy");
        Console.WriteLine("Kraid lint initialization: three native policies, actual slot writes and all 512 countdown transitions pass.");
    }
    private static void VerifyLookupStream2DropSelection(SuperMetroidAddressSpace rom)
    {
        // Native first record weights60,60,60,5,60,10 yield cumulative thresholds
        //60,120,180,185,245,255 when all resources are eligible.
        ushort[] randomValues = [1, 61, 121, 181, 186, 246];
        for (int column = 0; column < 6; column++)
        {
            EnemyPickupKind expected = (EnemyPickupKind)rom.ReadByte(0x86f25e + column);
            AssertEqual(expected, EnemyDropSelectionDefinitions.ForProbabilityColumn(column),
                "Native probability-column pickup identity");
            var samus = CreateDropTestSamus();
            samus.Health = 50;
            samus.MaxMissiles = samus.MaxSuperMissiles = samus.MaxPowerBombs = 5;
            var fixture = CreateEnemyDropFixture(samus, [randomValues[column]]);
            var projectile = fixture.System.EnemyProjectiles[0];
            projectile.ItemDropChancesPointerOverride = NativeDropChancePointer;
            AssertEqual(expected, fixture.System.SelectRandomEnemyDrop(projectile),
                "Actual cumulative selection preserves each semantic column");
        }
        Suite(nameof(VerifyEnemyDropSelectionRules), () => VerifyEnemyDropSelectionRules());
        AssertThrows<IndexOutOfRangeException>(() => EnemyDropSelectionDefinitions.ForProbabilityColumn(-1),
            "Drop column lower bound");
        AssertThrows<IndexOutOfRangeException>(() => EnemyDropSelectionDefinitions.ForProbabilityColumn(6),
            "Drop column upper bound");
        Console.WriteLine("Enemy drop selection:six native identities and actual cumulative selections, RNG-zero reroll, energy hysteresis and full-resource eligibility pass.");
    }
    private static void VerifyLookupStream2PipeBugVisualGeometry(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyBrinstarPipeBugInstructionProgramDefinitions), () => VerifyBrinstarPipeBugInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyNorfairPipeBugInstructionProgramDefinitions), () => VerifyNorfairPipeBugInstructionProgramDefinitions(rom));
        Suite(nameof(VerifyYellowPipeBugInstructionProgramDefinitions), () => VerifyYellowPipeBugInstructionProgramDefinitions(rom));
        for (int index = 0; index < BrinstarPipeBugInstructionProgramDefinitionsTooling.PresentationWordCount; index++)
            Check(index < 28 ? EnemyDefinitionId.Zeb : EnemyDefinitionId.Zebbo,
                BrinstarPipeBugInstructionProgramDefinitionsTooling.PresentationWordAddress(index));
        for (int index = 0; index < NorfairPipeBugInstructionProgramDefinitionsTooling.PresentationWordCount; index++)
            Check(EnemyDefinitionId.Gamet, NorfairPipeBugInstructionProgramDefinitionsTooling.PresentationWordAddress(index));
        for (int index = 0; index < YellowPipeBugInstructionProgramDefinitionsTooling.PresentationWordCount; index++)
            Check(EnemyDefinitionId.Geega, YellowPipeBugInstructionProgramDefinitionsTooling.PresentationWordAddress(index));
        Console.WriteLine("Pipe Bug visual geometry:88 native selectors/single-part headers, adjacent mechanics rejection and existing actual variant program checks pass.");

        void Check(EnemyDefinitionId enemy, ushort address)
        {
            ushort expected = ReadVerificationWord(rom, 0xb30000 | address);
            AssertEqual(expected, PipeBugVisualDefinitions.FrameAt(enemy, address), "Native Pipe Bug visual selector");
            AssertEqual((ushort)1, ReadVerificationWord(rom, 0xb30000 | expected), "Native seven-byte single-part composition");
            for (int delta = -2; delta <= 2; delta++)
            {
                if (delta == 0) continue;
                ushort invalid = unchecked((ushort)(address + delta));
                AssertThrows<InvalidDataException>(() => PipeBugVisualDefinitions.FrameAt(enemy, invalid), "Nonvisual adjacent bytes rejected");
            }
        }
    }
    private static void VerifyLookupStream2PipeBugFormation(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo initialize = typeof(RoomEnemySystem).GetMethod("InitializeNorfairPipeBug", flags)!;
        MethodInfo wait = typeof(RoomEnemySystem).GetMethod("RunNorfairPipeBugSamusWait", flags)!;
        for (int member = 0; member < 5; member++)
        {
            AssertEqual(Word(0xb38c65 + member * 7), PipeBugDefinitions.NorfairStaggerTarget(member),
                "Native rank-derived formation release counter");
            AssertEqual(Word(0xb38c88 + member * 6), (ushort)PipeBugDefinitions.NorfairPostRiseFunction(member),
                "Native role-specific formation function");
        }
        foreach (ushort samusX in new ushort[] { 120, 136 })
        {
            var enemies = new RoomEnemySystem();
            for (int member = 0; member < 5; member++)
            {
                var slot = enemies.Slots[member];
                slot.EnemyDefinitionPointer = EnemyDefinitionId.Gamet;
                slot.XPosition = 128; slot.YPosition = 128; slot.Parameter2 = 64;
                initialize.Invoke(enemies, [slot]);
                slot.InstructionTimer = (ushort)(member + 4);
                slot.Timer = (ushort)(member + 8);
            }
            var samus = new SamusState { XPosition = samusX, YPosition = 112 };
            wait.Invoke(enemies, [enemies.Slots[0], enemies.PipeBugStates[0]!, samus]);
            for (int member = 0; member < 5; member++)
            {
                var state = enemies.PipeBugStates[member]!;
                AssertEqual(Word(0xb38c65 + member * 7), state.StaggerTarget,
                    "Actual formation writes native release counter");
                AssertEqual(Word(0xb38c88 + member * 6), (ushort)state.NorfairPostRiseFunction,
                    "Actual formation writes native role function");
                AssertEqual(PipeBugEnemyFunction.NorfairRise, state.Function, "Actual formation begins rising");
                AssertEqual(samusX < 128 ? NorfairPipeBugInstructionProgramDefinitions.RisingLeft
                    : NorfairPipeBugInstructionProgramDefinitions.RisingRight,
                    enemies.Slots[member].CurrentInstruction, "Actual formation selects facing");
                AssertEqual((ushort)(member == 0 ? 1 : member + 4), enemies.Slots[member].InstructionTimer,
                    "Only leader instruction timer resets");
                AssertEqual((ushort)(member == 0 ? 0 : member + 8), enemies.Slots[member].Timer,
                    "Only leader loop counter resets");
            }
        }
        AssertThrows<IndexOutOfRangeException>(() => PipeBugDefinitions.NorfairStaggerTarget(-1), "Formation lower bound");
        AssertThrows<IndexOutOfRangeException>(() => PipeBugDefinitions.NorfairPostRiseFunction(5), "Formation upper bound");
        Console.WriteLine("Norfair Pipe Bug formation:ten native immediates, both actual facing formations and leader-only timer resets pass.");
    }
    private static void VerifyLookupStream2GoldenControl(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyGoldenTorizoJumpLandingDefinitions), () => VerifyGoldenTorizoJumpLandingDefinitions(rom));
        int address = 0xc9cb;
        for (int index = 0; index < 7; index++)
        {
            InstructionMechanicsWord actual =
                GoldenTorizoInitialInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual((ushort)address, actual.Address, "Golden initial native instruction cursor");
            ushort native = (ushort)(rom.ReadByte(0xaa0000 | address) |
                rom.ReadByte(0xaa0000 | (address + 1)) << 8);
            AssertEqual(native, actual.Value, "Golden initial native control word");
            AssertTrue(GoldenTorizoInitialInstructionProgramDefinitions.TryReadMechanicsWord(
                actual.Address, out ushort selected) && selected == native,
                "Golden initial runtime word dispatch");
            for (int byteIndex = 0; byteIndex < 2; byteIndex++)
                AssertTrue(GoldenTorizoInitialInstructionProgramDefinitions.IsCompiledMechanicsByte(
                    0xaa0000 | (address + byteIndex)), "Golden initial control byte ownership");
            address += native == 0x814b ? 9 : native < 0x8000 ? 4 : 2;
        }
        AssertEqual(0xc9e2, address, "Golden initial terminal sleep boundary");
        ushort operand = GoldenTorizoInitialInstructionProgramDefinitions.InitialFrameOperand;
        AssertTrue(CompiledEnemyVisualSelectors.TryGet(0xaa, operand, out ushort visual),
            "Golden initial installed sprite selector");
        AssertEqual((ushort)(rom.ReadByte(0xaa0000 | operand) |
            rom.ReadByte(0xaa0000 | (operand + 1)) << 8), visual,
            "Golden initial exact native sprite selection");
        AssertTrue(!GoldenTorizoInitialInstructionProgramDefinitions.TryReadMechanicsWord(operand, out _),
            "Golden initial visual operand excluded from mechanics");
        AssertTrue(!GoldenTorizoInitialInstructionProgramDefinitions.TryReadMechanicsWord((ushort)address, out _),
            "Golden initial next program excluded from mechanics");
        AssertThrows<IndexOutOfRangeException>(() => GoldenTorizoInitialInstructionProgramDefinitions.MechanicsWord(7),
            "Golden initial control index boundary");
        AssertThrows<IndexOutOfRangeException>(() => GoldenTorizoJumpLandingInstructionProgramDefinitions.MechanicsWord(20),
            "Golden landing control index boundary");
        Console.WriteLine("Golden Torizo control dispatch:7 initial words,20 landing words, native instruction positions, exact installed initial sprite and bounded ownership pass.");
    }
    private static void VerifyLookupStream2TrailAppearance(ISnesAddressSpace rom)
    {
        byte[] json = ProjectileTrailExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<ProjectileTrailDocument>(json, MapPresentationFormat.JsonOptions)!;
        var stock = ProjectileTrailCatalog.Load(new MemoryStream(json));
        var edits = (Dictionary<ushort, ushort>)typeof(ProjectileTrailCatalog)
            .GetField("suppliedAttributes", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
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
    private static void VerifyLookupStream2TrailPrograms(ISnesAddressSpace bus)
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
            var nativeSystem = new SamusProjectileSystem(); var authoredSystem = new SamusProjectileSystem();
            foreach (var system in new[] { nativeSystem, authoredSystem })
            {
                var pair = system.TrailSlots[0];
                foreach (var side in new[] { pair.Left, pair.Right })
                { side.InstructionPointer = start; side.InstructionTimer = 1; side.XPosition = 100; side.YPosition = 100; }
            }
            for (int frame = 0; frame < 80; frame++)
            {
                var nativeOam = new OamBuffer(); var authoredOam = new OamBuffer();
                bool frozenFrame = frame % 5 == 0;
                StepNative(nativeSystem.TrailSlots[0], nativeSystem.TrailSlots[0].Left, nativeOam, frozenFrame);
                StepNative(nativeSystem.TrailSlots[0], nativeSystem.TrailSlots[0].Right, nativeOam, frozenFrame);
                authoredSystem.HandleTrailsAndDraw(new ProjectileCompositionForbiddenBus(), authoredOam, 0, 0, frozenFrame, catalog);
                AssertTrue(nativeOam.LowTable.SequenceEqual(authoredOam.LowTable), "Trail catalog preserves live command/termination/freeze frame output");
                foreach (var sides in new[] { (nativeSystem.TrailSlots[0].Left, authoredSystem.TrailSlots[0].Left), (nativeSystem.TrailSlots[0].Right, authoredSystem.TrailSlots[0].Right) })
                {
                    AssertEqual(sides.Item1.InstructionPointer, sides.Item2.InstructionPointer, "Trail artwork cannot change instruction cursor");
                    AssertEqual(sides.Item1.InstructionTimer, sides.Item2.InstructionTimer, "Trail artwork cannot change live timing");
                    AssertEqual(sides.Item1.YPosition, sides.Item2.YPosition, "Trail artwork cannot change sibling-targeted movement");
                }
            }
        }
        AssertTrue(encountered.SetEquals(ProjectileTrailVisualDefinitions.Frames.ToArray()), "Independent native stream walk finds exactly the catalog's appearance records");
        int programWords = 0;
        for (int address = 0x90b4c8; address <= 0x90b5b3; address++)
        {
            if (ProjectileTrailProgramDefinitions.TryRead(address, out _))
            {
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), ProjectileTrailProgramDefinitions.Read(bus, address), "Compiled trail program preserves every authored mechanics word");
                programWords++;
            }
            else
            {
                int rejectedAddress = address;
                AssertThrows<InvalidDataException>(() => ProjectileTrailProgramDefinitions.Read(bus, rejectedAddress), "Trail program rejects presentation gaps, odd addresses and unrelated high-bank words");
            }
        }
        AssertEqual(67, programWords, "All 42 durations, 20 movement commands and five terminators are compiled");
        AssertThrows<InvalidDataException>(() => ProjectileTrailProgramDefinitions.Read(bus, 0x91b4c9), "Trail program rejects a wrong-bank alias");
        Suite(nameof(VerifyTrailMutableAlias), () => VerifyTrailMutableAlias());
        AssertThrows<IndexOutOfRangeException>(() => _ = ProjectileTrailVisualDefinitions.Frames[-1], "Calculated trail frame lower bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = ProjectileTrailVisualDefinitions.Frames[42], "Calculated trail frame upper bound");
        Console.WriteLine("Stream 2 trail programs: 67 native mechanics words,42 visual record addresses, all four actual paired trail lifetimes/freeze states and mutable alias pass.");
        void StepNative(SamusProjectileTrailSlot pair, SamusProjectileTrailSide side, OamBuffer oam, bool frozen)
        {
            if (side.InstructionTimer == 0) return;
            if (!frozen && --side.InstructionTimer == 0)
            {
                ushort cursor = side.InstructionPointer;
                while (true)
                {
                    ushort word = ReadVerificationWord(bus, 0x900000 | cursor);
                    if (word < 0x8000)
                    {
                        side.InstructionTimer = word;
                        if (word == 0) return;
                        side.TileNumberAttributes = ReadVerificationWord(bus, 0x900000 | (cursor + 2));
                        side.InstructionPointer = (ushort)(cursor + 4);
                        break;
                    }
                    cursor += 2;
                    if (word == (ushort)ProjectileTrailInstruction.MoveLeftDown) pair.Left.YPosition++;
                    else if (word == (ushort)ProjectileTrailInstruction.MoveRightDown) pair.Right.YPosition++;
                    else throw new InvalidDataException($"Unexpected native trail command {word:X4}.");
                }
            }
            oam.AddProjectileTrailSprite((byte)side.XPosition, (byte)side.YPosition, side.TileNumberAttributes);
        }
    }

    private static void VerifyLookupStream2TrailSelectors(ISnesAddressSpace bus)
    {

        int start = SamusProjectileRomData.Trails.LeftInstructionPointers;
        int end = SamusProjectileRomData.Trails.RightInstructionPointers + 64 * sizeof(ushort);
        for (int address = start; address < end; address += sizeof(ushort))
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address), ProjectileTrailDefinitions.ReadSelector(address), "Trail selectors retain every reachable aligned native word");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start - 2), "Trail selector rejects the preceding word");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(start + 1), "Trail selector rejects unaligned reads");
        AssertThrows<InvalidDataException>(() => ProjectileTrailDefinitions.ReadSelector(end), "Trail selector rejects the following word");
        var spawn = typeof(SamusProjectileSystem).GetMethod("SpawnTrail", BindingFlags.NonPublic | BindingFlags.Instance)!;
        for (int selection = 0; selection < 64; selection++)
        {
            var projectiles = new SamusProjectileSystem();
            var projectile = new SamusProjectileSlot(0)
            {
                Type = (ushort)selection,
                InstructionPointer = 0x86e3,
                XPosition = 100,
                YPosition = 200,
            };
            spawn.Invoke(projectiles, [new TrailSelectorGuard(start, end), projectile]);
            var trail = projectiles.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), start + selection * 2), trail.Left.InstructionPointer, "Real spawn selects left trail including adjacent right-table entries");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomData.Trails.RightInstructionPointers + selection * 2), trail.Right.InstructionPointer, "Real spawn retains right-table overrun behavior");
            AssertEqual(1, trail.Left.InstructionTimer, "Selector extraction leaves allocation timer unchanged");
            AssertEqual(96, trail.Left.XPosition, "Selector extraction leaves origin offset unchanged");
        }
        Console.WriteLine("Trail selectors: 103 reachable native words and 64 real spawn selections pass with the complete selector window forbidden.");
    }

    private static void VerifyLookupStream2TubeRamp(ISnesAddressSpace rom)
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
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        AssertEqual(8, ((Bgr555[])typeof(ChozoAndTubeColorCatalog).GetField("tubeColorSeeds", flags)!.GetValue(stock)!).Length,
            "tube stores only its eight authored seed colors");
        foreach (string statue in new[] { "wreckedShipEdits", "lowerNorfairEdits" })
            AssertEqual(0, ((System.Collections.IDictionary)typeof(ChozoAndTubeColorCatalog).GetField(statue, flags)!.GetValue(stock)!).Count,
                $"stock {statue} store no colors beyond the calculated statue paint");
        AssertEqual(0, ((System.Collections.IDictionary)typeof(ChozoAndTubeColorCatalog).GetField("tubeColorEdits", flags)!.GetValue(stock)!).Count,
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
                int stored = ((System.Collections.IDictionary)typeof(ChozoAndTubeColorCatalog).GetField("wreckedShipEdits", flags)!.GetValue(edited)!).Count
                    + ((System.Collections.IDictionary)typeof(ChozoAndTubeColorCatalog).GetField("lowerNorfairEdits", flags)!.GetValue(edited)!).Count;
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
    private static void VerifyLookupStream2PauseOwnership(ISnesAddressSpace rom)
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
    private static void VerifyLookupStream2DeadTorizoTransfers(ISnesAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        var state = new DeadTorizoEnemyState(enemies.Slots[0], 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        var build = typeof(RoomEnemySystem).GetMethod("BuildDeadTorizoVramTransfers",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .CreateDelegate<Action<DeadTorizoEnemyState>>(enemies);
        for (ushort phase = 0; phase < 2; phase++)
        {
            var rows = DeadTorizoVramTransferDefinitions.ForPhase(phase);
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
            state.VramTransferPhase = unchecked((ushort)(phase - 1));
            build(state);
            AssertEqual(phase, state.VramTransferPhase, "Dead Torizo phase advancement");
            for (int index = 0; index < rows.Length; index++)
            {
                var actual = enemies.LastDeadTorizoVramTransfers[phase * 7 + index];
                AssertEqual(rows[index].SizeInBytes, actual.SizeInBytes, "Dead Torizo queued size");
                AssertEqual(rows[index].SourceAddress, actual.SourceAddress, "Dead Torizo queued live WRAM source");
                AssertEqual(rows[index].EncodedVramDestination, actual.EncodedVramDestination, "Dead Torizo queued VRAM destination");
                AssertEqual(rows[index], DeadTorizoVramTransferDefinitions.ForPhase((ushort)(phase + 2))[index], "Dead Torizo phase parity");
            }
            AssertThrows<IndexOutOfRangeException>(() => { _ = rows[-1]; }, "Dead Torizo negative row");
            AssertThrows<IndexOutOfRangeException>(() => { _ = rows[7]; }, "Dead Torizo past last row");
        }
        Console.WriteLine("Dead Torizo: all56 native descriptor fields, two terminators, actual fourteen queued live-WRAM transfers, phase parity, enumeration and bounds pass.");
    }
    private static void VerifyLookupStream2GunshipTransfers(ISnesAddressSpace rom)
    {
        var transfers = GunshipLiftoffTransferDefinitions.Frames;
        AssertEqual(5, transfers.Count, "Gunship calculated transfer count");
        var enemies = new RoomEnemySystem();
        var slot = enemies.Slots[0];
        var queue = new VramWriteQueue();
        var append = typeof(RoomEnemySystem).GetMethod("QueueGunshipTakeoffTiles",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot, VramWriteQueue>>(enemies);
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
            AssertEqual(destination, queue.Entries[enumerated].EncodedVramDestination, "Gunship actual queued destination");
            AssertEqual(source, queue.Entries[enumerated].SourceAddress, "Gunship actual queued source");
            enumerated++;
        }
        AssertEqual(5, enumerated, "Gunship enumerates every transfer");
        AssertEqual((ushort)0, slot.VariableB, "Gunship completes transfer phase");
        AssertEqual((ushort)GunshipFunction.FireUpEngines, slot.VariableF, "Gunship starts engines after fifth transfer");
        AssertThrows<IndexOutOfRangeException>(() => _ = transfers[-1], "Gunship lower transfer bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = transfers[5], "Gunship upper transfer bound");
        Console.WriteLine("Gunship transfers: all10 native source/destination fields, five typed identities, actual queued uploads/phase handoff, enumeration and bounds pass.");
    }
    private static void VerifyLookupStream2ReserveArrowColors(ISnesAddressSpace rom)
    {
        byte[] source = PauseReserveUiExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<PauseReserveUiDocument>(source, MapPresentationFormat.JsonOptions)!;
        var stock = PauseReserveUiPresentation.Load(new MemoryStream(source));
        var edits = (Dictionary<int, Bgr555>)typeof(PauseReserveUiPresentation)
            .GetField("arrowColorEdits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
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
    private static void VerifyLookupStream2ReserveGeometry(ISnesAddressSpace rom)
    {
        byte[] source = PauseReserveUiExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<PauseReserveUiDocument>(source, MapPresentationFormat.JsonOptions)!;
        var stock = PauseReserveUiPresentation.Load(new MemoryStream(source));
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        AssertTrue(typeof(PauseReserveUiPresentation).GetField("digits", fields)!.GetValue(stock) is null, "Stock digits discard stored lookup");
        AssertTrue(typeof(PauseReserveUiPresentation).GetField("arrowOffsets", fields)!.GetValue(stock) is null, "Stock arrow discards stored offsets");
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
    private static void VerifyLookupStream2GhostAndNorfair(ISnesAddressSpace rom)
    {
        const System.Reflection.BindingFlags methods = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
        var spawn = typeof(RoomEnemySystem).GetMethod("SpawnWreckedShipGhost", methods)!
            .CreateDelegate<Action<RoomEnemySlot, WreckedShipGhostEnemyState, ushort, ushort>>();
        var flicker = typeof(RoomEnemySystem).GetMethod("AdvanceWreckedShipGhostFlicker", methods)!
            .CreateDelegate<Action<RoomEnemySlot, WreckedShipGhostEnemyState>>();
        var slot = new RoomEnemySystem().Slots[0];
        var state = new WreckedShipGhostEnemyState(slot);
        for (int index = 0; index < 9; index++)
        {
            short originalX = unchecked((short)ReadVerificationWord(rom, 0xa89aa8 + 4 * index));
            short originalY = unchecked((short)ReadVerificationWord(rom, 0xa89aaa + 4 * index));
            var calculated = WreckedShipGhostAppearanceDefinitions.SpawnOffset(index);
            AssertEqual(originalX, calculated.X, "Ghost original horizontal spawn offset");
            AssertEqual(originalY, calculated.Y, "Ghost original vertical spawn offset");
            state.HorizontalMovementClass = (ushort)(4 * (index % 3));
            state.VerticalMovementClass = (ushort)(12 * (index / 3));
            spawn(slot, state, 32, 65520);
            AssertEqual(unchecked((ushort)(32 + originalX)), slot.XPosition, "Ghost actual wrapped spawn X");
            AssertEqual(unchecked((ushort)(65520 + originalY)), slot.YPosition, "Ghost actual wrapped spawn Y");
            AssertEqual(WreckedShipGhostAiFunction.BrighteningAndFlickering, state.Function, "Ghost appearance phase");
            AssertEqual((ushort)64, state.StablePositionTimer, "Ghost appearance position timer");
            AssertEqual((ushort)16, state.StableDirectionTimer, "Ghost appearance direction timer");
        }
        for (int offset = 0; offset < 34; offset++)
        {
            short original = unchecked((short)ReadVerificationWord(rom, 0xa89acc + 2 * (offset / 2)));
            AssertEqual(original, WreckedShipGhostAppearanceDefinitions.FlickerDuration(offset / 2), "Ghost original flicker interval");
            slot.Properties = (ushort)(EnemyProperties.Invisible | EnemyProperties.ProcessOffScreen);
            state.PhaseTimer = 1;
            state.FlickerTableOffset = (ushort)offset;
            flicker(slot, state);
            AssertEqual(original < 0 ? (ushort)0 : (ushort)original, state.PhaseTimer, "Ghost actual next interval");
            AssertEqual(original < 0 ? (ushort)0 : (ushort)(offset + 2), state.FlickerTableOffset, "Ghost actual next offset including odd folding");
            bool remainsInvisible = original < 0 || (offset & 2) != 0;
            AssertEqual((ushort)(EnemyProperties.ProcessOffScreen | (remainsInvisible ? EnemyProperties.Invisible : default(EnemyProperties))), slot.Properties, "Ghost actual visibility preserves unrelated flags");
        }
        state.PhaseTimer = 2;
        state.FlickerTableOffset = 2;
        slot.Properties = (ushort)EnemyProperties.Invisible;
        flicker(slot, state);
        AssertEqual((ushort)1, state.PhaseTimer, "Ghost interval countdown");
        AssertEqual((ushort)2, state.FlickerTableOffset, "Ghost no early interval advance");
        AssertEqual((ushort)EnemyProperties.Invisible, slot.Properties, "Ghost no early visibility change");
        state.PhaseTimer = 0;
        flicker(slot, state);
        AssertEqual((ushort)0, slot.Properties, "Ghost terminal next call clears invisibility");
        state.PhaseTimer = 1;
        state.FlickerTableOffset = 34;
        AssertThrows<InvalidDataException>(() => flicker(slot, state), "Ghost malformed offset remains rejected");
        state.HorizontalMovementClass = 1;
        state.VerticalMovementClass = 0;
        AssertThrows<InvalidDataException>(() => spawn(slot, state, 0, 0), "Ghost unaligned spawn class remains rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.SpawnOffset(-1), "Ghost spawn lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.SpawnOffset(9), "Ghost spawn upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.FlickerDuration(-1), "Ghost flicker lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => WreckedShipGhostAppearanceDefinitions.FlickerDuration(17), "Ghost flicker upper bound");
        foreach (var definition in NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All)
        {
            int total = 0;
            for (int frame = 0; frame < 16; frame++)
            {
                ushort pointer = (ushort)(definition.FramePointer(frame) + (definition.PublishesHeatPhase ? 3 : 0));
                ushort original = ReadVerificationWord(rom, 0x8d0000 | pointer);
                AssertTrue(definition.TryReadMechanicsWord(pointer, out ushort duration), "Norfair duration mechanic resolves");
                AssertEqual(original, duration, "Norfair original duration");
                total += duration;
            }
            AssertEqual(116, total, "Norfair exact cycle duration");
        }
        AssertThrows<IndexOutOfRangeException>(() => NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.Duration(-1), "Norfair duration lower bound");
        AssertThrows<IndexOutOfRangeException>(() => NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.Duration(16), "Norfair duration upper bound");
        Console.WriteLine("Ghost appearance: 18 native offset fields, 17 native intervals, actual wrapped positions/timing/visibility and malformed states pass; Norfair: 64 native durations and four 116-tick cycles pass.");
    }
    private static void VerifyLookupStream2PaletteMechanics(ISnesAddressSpace rom)
    {
        int upperWords = 0, oldWords = 0, bellyWords = 0;
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            ushort pointer = (ushort)address;
            if (UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort upper))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), upper, "Upper Crateria mechanics original word");
                upperWords++;
            }
            if (OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort old))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), old, "Old Tourian mechanics original word");
                oldWords++;
            }
            if (TorizoBellyPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort belly))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), belly, "Torizo belly mechanics original word");
                bellyWords++;
            }
        }
        AssertEqual(32, upperWords, "Upper Crateria complete mechanics coverage");
        AssertEqual(60, oldWords, "Old Tourian complete mechanics coverage");
        AssertEqual(36, bellyWords, "Torizo belly complete mechanics coverage");
        int upperDuration = 0;
        for (int frame = 0; frame < 14; frame++)
        {
            ushort pointer = UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FramePointer(frame);
            AssertEqual((ushort)(0xfd01 + 18 * frame), pointer, "Upper Crateria frame address");
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort duration);
            upperDuration += duration;
            for (int color = 0; color < 8; color++)
            {
                int[] nativeOffsets = [2, 4, 6, 10, 12, 14, 16, 20];
                ushort actual = OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer(frame, color);
                AssertEqual((ushort)(0xfa6d + 24 * frame + nativeOffsets[color]), actual, "Old Tourian color address around inline skips");
                AssertTrue(!OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(actual, out _), "Supplied old Tourian colors remain live");
            }
        }
        AssertEqual(63, upperDuration, "Upper Crateria cycle duration");
        var definitions = TorizoBellyPaletteFxProgramMechanicsDefinitions.All;
        AssertEqual(2, definitions.Count, "Both Torizo programs enumerated");
        for (int index = 0; index < definitions.Count; index++)
        {
            var definition = definitions[index];
            AssertEqual((TorizoBellyPaletteOwner)index, definition.Owner, "Torizo program owner ordering");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | definition.DefinitionPointer + 2), definition.ProgramStart, "Original Torizo definition list operand");
            int total = 0;
            for (int frame = 0; frame < 6; frame++)
            {
                definition.TryReadMechanicsWord(definition.FramePointer(frame), out ushort duration);
                total += duration;
                for (int color = 0; color < 3; color++)
                    AssertTrue(!definition.TryReadMechanicsWord(definition.ColorPointer(frame, color), out _), "Supplied Torizo colors remain live");
            }
            AssertEqual(52, total, "Torizo belly cycle duration");
            AssertThrows<ArgumentOutOfRangeException>(() => definition.FramePointer(6), "Torizo frame upper bound");
            AssertThrows<ArgumentOutOfRangeException>(() => definition.FramePointer(-1), "Torizo frame lower bound");
        }
        int enumerated = 0;
        foreach (var definition in definitions)
        {
            AssertEqual(definitions[enumerated].Owner, definition.Owner, "Torizo enumerated owner");
            enumerated++;
        }
        AssertEqual(2, enumerated, "Torizo definition enumeration");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = definitions[-1]; }, "Torizo definition lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = definitions[2]; }, "Torizo definition upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FramePointer(-1), "Upper Crateria lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FramePointer(14), "Upper Crateria upper frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer(0, -1), "Old Tourian lower color bound");
        AssertThrows<ArgumentOutOfRangeException>(() => OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorPointer(0, 8), "Old Tourian upper color bound");
        Console.WriteLine("Stream 2 palette mechanics: original mechanics, inline color offsets, durations, dispatch and bounds pass.");
    }
    private static void VerifyLookupStream2CrystalBody(ISnesAddressSpace rom)
    {
        byte[] source = CrystalFlashColorExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<CrystalFlashColorDocument>(
            source, MapPresentationFormat.JsonOptions)!;
        var stock = CrystalFlashColorCatalog.Load(new MemoryStream(source));
        var storedBody = typeof(CrystalFlashColorCatalog).GetField("body",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        AssertTrue(storedBody.GetValue(stock) is null, "Original Crystal Flash body selects calculation without stored rows");
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
                AssertTrue(storedBody.GetValue(changed) is not null, "Independent supplied body edit retains its rows");
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
        var storedBubble = typeof(CrystalFlashColorCatalog).GetField("bubble",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        var residuals = (Dictionary<int, Bgr555>)storedBubble.GetValue(stock)!;
        AssertEqual(1, residuals.Count, "One stock bubble residual remains pending");
        AssertEqual((ushort)0x7fff, residuals[30], "Final-frame leading white remains supplied data");
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
            AssertTrue(storedBody.GetValue(changed) is null, "Independent bubble edit preserves body calculation");
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
    }    private static void VerifyLookupStream2KraidRamps(ISnesAddressSpace rom)
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
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object health = typeof(KraidColorCatalog).GetField("health", fields)!.GetValue(stock)!;
        object secondary = typeof(KraidColorCatalog).GetField("secondary", fields)!.GetValue(stock)!;
        AssertTrue(ReferenceEquals(health, secondary), "Identical Kraid secondary colors share calculated primary source");
        foreach (string name in new[] { "health", "roomBackdrop", "initialTarget", "deathArm" })
        {
            object band = typeof(KraidColorCatalog).GetField(name, fields)!.GetValue(stock)!;
            var deviations = (System.Collections.IDictionary)band.GetType().GetField("deviations", fields)!.GetValue(band)!;
            AssertEqual(0, deviations.Count, $"Stock Kraid {name} stores no colors beyond the calculated paint");
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
    private static void VerifyLookupStream2PowerDirectionBindings(ISnesAddressSpace rom)
    {
        byte[] json = ProjectileFrameBindingExtractor.Extract(rom);
        var stock = ProjectileFrameBindingCatalog.Load(new MemoryStream(json));
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var remaining = (System.Collections.IDictionary)typeof(ProjectileFrameBindingCatalog).GetField("sprites", fields)!.GetValue(stock)!;
        AssertEqual(0, remaining.Count, "All805 stock selector operands calculate; independent selection policies remain required");
        foreach (ushort pointer in SamusProjectileRadiusDefinitions.TimedRecordPointers)
            AssertEqual(ReadVerificationWord(rom, (0x930000 | pointer) + 2), stock.Resolve(pointer), "Every installed selector retains its exact native target");
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
        var document = System.Text.Json.JsonSerializer.Deserialize<ProjectileFrameBindingDocument>(json, options)!;
        for (int direction = 0; direction < 8; direction++)
        {
            ushort pointer = (ushort)(0x86db + direction * 12);
            AssertTrue(!remaining.Contains(pointer), "Stock compass selector is absent from stored residuals");
            var shot = new SamusProjectileSlot(0) { InstructionPointer = pointer, InstructionTimer = 1 };
            var system = new SamusProjectileSystem { FrameBindings = stock };
            _ = system.RunProjectileInstructionHandler(shot);
            AssertEqual(ReadVerificationWord(rom, (0x930000 | pointer) + 2), shot.SpritemapPointer, "Actual projectile handler selects calculated compass pose");
            string key = ProjectileFrameBindingFormat.FrameName(pointer);
            string original = document.Frames[key];
            document.Frames[key] = ProjectileSpriteDefinitions.Name(ProjectileSpriteDefinitions.NativePointers[0]);
            var edited = ProjectileFrameBindingCatalog.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, options)));
            var editedShot = new SamusProjectileSlot(0) { InstructionPointer = pointer, InstructionTimer = 1 };
            system.FrameBindings = edited;
            _ = system.RunProjectileInstructionHandler(editedShot);
            AssertEqual(ProjectileSpriteDefinitions.NativePointers[0], editedShot.SpritemapPointer, "Independent direction edit overrides calculation in actual handler");
            AssertEqual(shot.InstructionTimer, editedShot.InstructionTimer, "Visual edit preserves duration");
            AssertEqual(shot.InstructionPointer, editedShot.InstructionPointer, "Visual edit preserves control flow");
            AssertEqual(shot.XRadius, editedShot.XRadius, "Visual edit preserves horizontal radius");
            AssertEqual(shot.YRadius, editedShot.YRadius, "Visual edit preserves vertical radius");
            document.Frames[key] = original;
        }
        int checkedWaveIce = 0;
        int checkedAxialEndPairs = 0;
        int checkedCappedSweep = 0;
        foreach (ushort pointer in SamusProjectileRadiusDefinitions.TimedRecordPointers)
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
            var shot = new SamusProjectileSlot(0) { InstructionPointer = pointer, InstructionTimer = 1 };
            var system = new SamusProjectileSystem { FrameBindings = stock };
            _ = system.RunProjectileInstructionHandler(shot);
            AssertEqual(ReadVerificationWord(rom, (0x930000 | pointer) + 2), shot.SpritemapPointer, "Actual handler uses native Wave/Ice traversal pose");
            string key = ProjectileFrameBindingFormat.FrameName(pointer);
            string original = document.Frames[key];
            ushort replacement = stock.Resolve(pointer) == ProjectileSpriteDefinitions.NativePointers[0]
                ? ProjectileSpriteDefinitions.NativePointers[1] : ProjectileSpriteDefinitions.NativePointers[0];
            document.Frames[key] = ProjectileSpriteDefinitions.Name(replacement);
            var edited = ProjectileFrameBindingCatalog.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, options)));
            system.FrameBindings = edited;
            var editedShot = new SamusProjectileSlot(0) { InstructionPointer = pointer, InstructionTimer = 1 };
            _ = system.RunProjectileInstructionHandler(editedShot);
            AssertEqual(replacement, editedShot.SpritemapPointer, "Every Wave/Ice supplied edit takes precedence");
            AssertEqual(shot.InstructionTimer, editedShot.InstructionTimer, "Wave/Ice visual edit preserves duration");
            AssertEqual(shot.InstructionPointer, editedShot.InstructionPointer, "Wave/Ice visual edit preserves flow");
            AssertEqual(shot.XRadius, editedShot.XRadius, "Wave/Ice visual edit preserves X radius");
            AssertEqual(shot.YRadius, editedShot.YRadius, "Wave/Ice visual edit preserves Y radius");
            document.Frames[key] = original;
            checkedWaveIce++;
        }
        AssertEqual(797, checkedWaveIce, "All other calculated beam/effect records including18 explicit exceptional policies");
        AssertEqual(8, checkedAxialEndPairs, "All four reversed axial end-pairs use exact native operands");
        AssertEqual(10, checkedCappedSweep, "All ten capped sweep phases use exact native operands");
        var runBomb = typeof(SamusBombProjectileSystem).GetMethod("RunProjectileInstructionHandler",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        int checkedBombs = 0;
        foreach (ushort pointer in SamusProjectileRadiusDefinitions.TimedRecordPointers)
        {
            if (pointer is < 0x9f87 or >= 0xa007) continue;
            var bomb = new SamusBombProjectileSlot(0) { InstructionPointer = pointer, InstructionTimer = 1, Type = 0x0500 };
            var bombs = new SamusBombProjectileSystem { FrameBindings = stock };
            _ = runBomb.Invoke(bombs, [bomb]);
            AssertEqual(ReadVerificationWord(rom, (0x930000 | pointer) + 2), bomb.SpritemapPointer, "Actual bomb handler uses sequential normal/fast pose");
            checkedBombs++;
        }
        AssertEqual(14, checkedBombs, "Both normal/fast PowerBomb and Bomb visual cycles");
        AssertThrows<InvalidDataException>(() => stock.Resolve(0x87c3), "Wave self-jump is not accepted as a timed record");
        AssertThrows<InvalidDataException>(() => stock.Resolve(0x86dc), "Program interior is not accepted as a timed record");
        Console.WriteLine("Stream2 Power bindings: 805calculated beam/effect selectors,805native operands,zero stored stock operands and805actual handler/edit paths pass.");
    }
    private static void VerifyLookupStream2GoldenTorizoFootGeometry(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var definitions = EnemyExtendedFrameDefinitions.Frames.ToArray();
        var document = new EnemyIdentityFixture().ExtendedDocument();
        var selected = new EnemyExtendedFrameDefinition[10];
        int nativeBodyParts = 0;
        for (int phase = 0; phase < 10; phase++)
        {
            int identity = 0xaaa4fa + 34 * phase;
            AssertEqual(identity, GoldenTorizoStrideGeometryDefinitions.NativeFrameIdentity(phase), "Golden Torizo native extended-record geometry");
            var frame = definitions.Single(frame => ((frame.Bank << 16) | frame.Pointer) == identity);
            selected[phase] = frame;
            AssertEqual((ushort)4, Word(identity), "Golden Torizo four native visual components");
            var components = new EnemyExtendedVisualComponent[4];
            for (int component = 0; component < 4; component++)
            {
                int record = identity + 2 + component * 8;
                int source = 0xaa0000 | Word(record + 4);
                if (component == 2) nativeBodyParts += Word(source);
                var parts = Enumerable.Range(0, Word(source)).Select(index =>
                {
                    int entry = source + 2 + index * 5;
                    var x = new SnesSpritemapXWord(Word(entry));
                    var attributes = new SnesObjAttributeWord(Word(entry + 3));
                    return new SpriteVisualPart
                    {
                        OffsetX = x.SignedOffset, OffsetY = unchecked((sbyte)rom.ReadByte(entry + 2)), Size = x.IsLarge ? 16 : 8,
                        TileColumn = attributes.TileNumber % 16, TileRow = attributes.TileNumber / 16,
                        Palette = attributes.PaletteIndex, Priority = attributes.Priority,
                        FlipX = attributes.FlipHorizontally, FlipY = attributes.FlipVertically,
                    };
                }).ToArray();
                components[component] = new EnemyExtendedVisualComponent
                { OffsetX = unchecked((short)Word(record)), OffsetY = unchecked((short)Word(record + 2)), Parts = parts };
            }
            document.Frames[frame.Name] = components;
        }
        AssertEqual(220, nativeBodyParts, "Golden Torizo ten native body-composition extents");
        EnemyExtendedFrameCatalog Load() => EnemyExtendedFrameCatalog.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions)));
        void Check(EnemyExtendedFrameCatalog catalog)
        {
            foreach (var frame in selected)
            {
                AssertTrue(catalog.TryGetDisplay(frame.Bank, frame.Pointer, out var actual), "Golden Torizo installed display binding");
                var expected = document.Frames[document.DisplayFrames![frame.Name]];
                AssertEqual(expected.Length, actual.Length, "Golden Torizo component order/extent");
                var expectedOam = new OamBuffer(); expectedOam.BeginFrame();
                var actualOam = new OamBuffer(); actualOam.BeginFrame();
                for (int component = 0; component < expected.Length; component++)
                {
                    var wanted = expected[component];
                    var found = actual.Span[component];
                    var parts = EnemySpritemapCatalog.CompileParts(wanted.Parts, frame.Name);
                    AssertEqual((short)wanted.OffsetX, found.OffsetX, "Golden Torizo independent component X");
                    AssertEqual((short)wanted.OffsetY, found.OffsetY, "Golden Torizo independent component Y");
                    AssertTrue(found.Parts.SequenceEqual(parts), "Golden Torizo exact supplied part order and fields");
                    ushort x = unchecked((ushort)(128 + wanted.OffsetX)), y = unchecked((ushort)(96 + wanted.OffsetY));
                    expectedOam.AddEnemySpritemap(parts.AsSpan(), x, y, 0, 0);
                    actualOam.AddEnemySpritemap(found.Parts, x, y, 0, 0);
                }
                AssertEqual(expectedOam.NextByteOffset, actualOam.NextByteOffset, "Golden Torizo packed OBJ count");
                AssertTrue(expectedOam.LowTable.SequenceEqual(actualOam.LowTable) && expectedOam.HighTable.SequenceEqual(actualOam.HighTable),
                    "Golden Torizo calculated stock/supplied parts produce exact packed OAM");
            }
            var identities = definitions.ToDictionary(frame => frame.Name, frame => (frame.Bank << 16) | frame.Pointer, StringComparer.Ordinal);
            string expectedHash = SelectedPresentationHash.Create("enemy-extended-oam-v1", content =>
            {
                foreach (var frame in definitions.OrderBy(frame => (frame.Bank << 16) | frame.Pointer))
                {
                    content.Append("frame", (frame.Bank << 16) | frame.Pointer);
                    var components = document.Frames[frame.Name];
                    content.Append("components", components.Length);
                    foreach (var component in components)
                    {
                        content.Append("offset-x", component.OffsetX); content.Append("offset-y", component.OffsetY);
                        content.AppendEnemyParts(EnemySpritemapCatalog.CompileParts(component.Parts, frame.Name));
                    }
                }
                foreach (var frame in definitions.OrderBy(frame => (frame.Bank << 16) | frame.Pointer))
                {
                    content.Append("native-binding", (frame.Bank << 16) | frame.Pointer);
                    content.Append("selected-binding", identities[document.DisplayFrames![frame.Name]]);
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
                    AssertEqual(Word(0xaad59a + offset), GoldenTorizoWalkDefinitions.Velocity(offset), "Edited foot artwork does not change native walking words/windows");
                parts[index] = original;
                edits++;
            }
        }
        AssertEqual(36, edits, "Golden Torizo all independent calculated foot-X fields edited");
        document.DisplayFrames![selected[0].Name] = selected[1].Name;
        Check(Load());
        Suite(nameof(VerifyCompiledStatueWalking), () => VerifyCompiledStatueWalking(rom));
        AssertThrows<ArgumentOutOfRangeException>(() => GoldenTorizoStrideGeometryDefinitions.HorizontalAdvance(10), "Golden Torizo phase upper bound");
        Console.WriteLine("Golden Torizo foot geometry:20derived words,39native byte windows,220body parts/40components,10calculated views,36independent foot-X edits,display rebind/hash/packed OAM and actual walking calls pass; seven artwork origins remain required.");
    }
}
