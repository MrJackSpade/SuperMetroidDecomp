using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.FrontendRomFreeVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Samus.Artwork;
/// <summary>Checks installed Samus body, death, and arm-cannon artwork against retail selectors and rendering transfers.</summary>
internal static class SamusBodyArtworkVerification
{
    /// <summary>Checks every extracted selector and split character transfer against retail.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus body artwork")]
    internal static void VerifySamusBodyArtwork(ISnesAddressSpace bus, GameInstallation installation)
    {
        SamusBodyArtworkCatalog stock = installation.LoadSamusBodyArt();
        Suite(nameof(VerifySamusArmCannonArtwork), () => VerifySamusArmCannonArtwork(bus, stock));
        int definitions = 0;
        for (int pose = 0; pose < SamusBodyArtworkCatalog.PoseCount; pose++)
        {
            int address = 0x92D94E + pose * 2;
            ushort native = CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
            AssertEqual(native, stock.PosePointers[pose], $"Samus body pose pointer {pose:X2}");
            int graphicsAddress = SamusMovementRomData.Poses.Definitions +
                pose * SamusMovementRomData.Poses.DefinitionByteCount + 4;
            AssertEqual(unchecked((sbyte)bus.ReadCartridgeByte(graphicsAddress)),
                stock.GraphicsYOffset((SamusPoseId)pose), $"Samus pose {pose:X2} visual Y origin");
        }
        for (int index = 0; index < SamusBodyArtworkCatalog.FrameCount; index++)
        {
            int address = SamusBodyArtworkCatalog.FirstFrameAddress + index * 4;
            SamusBodyFrameSelection frame = stock.Frames[index];
            AssertEqual(bus.ReadCartridgeByte(address), frame.TopSet, $"Samus frame {index} upper set");
            AssertEqual(bus.ReadCartridgeByte(address + 1), frame.TopPosition, $"Samus frame {index} upper position");
            AssertEqual(bus.ReadCartridgeByte(address + 2), frame.BottomSet, $"Samus frame {index} lower set");
            AssertEqual(bus.ReadCartridgeByte(address + 3), frame.BottomPosition, $"Samus frame {index} lower position");
        }
        for (int half = 0; half < 2; half++)
        {
            bool upper = half == 0;
            int setCount = upper ? SamusBodyArtworkCatalog.TopSetCount : SamusBodyArtworkCatalog.BottomSetCount;
            int pointerTable = upper ? 0x92D91E : 0x92D938;
            for (int set = 0; set < setCount; set++)
            {
                ushort nativePointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(pointerTable + set * 2);
                AssertEqual(nativePointer,
                    upper ? stock.TopSetPointers[set] : stock.BottomSetPointers[set],
                    $"Samus {(upper ? "top" : "bottom")} set {set} pointer");
                IReadOnlyList<SamusBodyTileDefinition> entries = upper ? stock.TopSet(set) : stock.BottomSet(set);
                for (int position = 0; position < entries.Count; position++)
                {
                    SamusBodyTileDefinition entry = entries[position];
                    int definitionAddress = stock.DefinitionAddress(upper ? SamusBodyHalf.Top : SamusBodyHalf.Bottom, (byte)set, (byte)position);
                    int source = (CartridgeImportSource.Require(bus).ReadCartridgeLong(definitionAddress));
                    ushort first = CartridgeImportSource.Require(bus).ReadCartridgeWord(definitionAddress + 3);
                    ushort second = CartridgeImportSource.Require(bus).ReadCartridgeWord(definitionAddress + 5);
                    AssertEqual(source, entry.SourceAddress, $"Samus definition {definitionAddress:X6} source");
                    AssertEqual(first, entry.FirstSize, $"Samus definition {definitionAddress:X6} first size");
                    AssertEqual(second, entry.SecondSize, $"Samus definition {definitionAddress:X6} second size");
                    for (int i = 0; i < entry.Planar.Length; i++)
                        AssertEqual(bus.ReadCartridgeByte(source + i), entry.Planar.Span[i],
                            $"Samus definition {definitionAddress:X6} tile byte {i:X3}");
                    definitions++;
                }
            }
        }
        AssertEqual(435, definitions, "complete Samus body DMA definition count");

        SamusSpritemapArtworkCatalog sprites = stock.Spritemaps;
        for (int pose = 0; pose < SamusBodyArtworkCatalog.PoseCount; pose++)
        {
            int topAddress = SamusSpritemapArtworkCatalog.TopBaseAddress + pose * 2;
            int bottomAddress = SamusSpritemapArtworkCatalog.BottomBaseAddress + pose * 2;
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(topAddress),
                sprites.TopBase((SamusPoseId)pose), $"Samus pose {pose:X2} top OAM base");
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(bottomAddress),
                sprites.BottomBase((SamusPoseId)pose), $"Samus pose {pose:X2} bottom OAM base");
        }
        var spritemapGuard = NewFrontendCartridgeReadGuard(bus);
        int nonzeroSpritemaps = 0;
        ushort[] nativePointers = [.. Enumerable.Range(0, SamusSpritemapArtworkCatalog.PointerCount).Select(index =>
            CartridgeImportSource.Require(bus).ReadCartridgeWord(SamusSpritemapArtworkCatalog.PointerTableAddress + index * 2))];
        SamusSpritemapId?[] nativeEntries = SamusSpritemapNative.Entries(nativePointers);
        for (int index = 0; index < SamusSpritemapArtworkCatalog.PointerCount; index++)
        {
            ushort pointer = nativePointers[index];
            AssertEqual(nativeEntries[index], sprites.Entries[index], $"Samus OAM pointer index {index}");
            if (!sprites.TryGet((ushort)index, out SamusSpritemapDefinition? record))
            {
                AssertEqual((ushort)0, pointer, $"Samus OAM index {index} mutable-memory pointer");
                continue;
            }
            nonzeroSpritemaps++;
            int address = 0x920000 | pointer;
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(address),
                (ushort)record.Parts.Length, $"Samus OAM record ${pointer:X4} part count");
            for (int part = 0; part < record.Parts.Length; part++)
            {
                int at = address + 2 + part * 5;
                AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(at),
                    record.Parts[part].X.Raw, $"Samus OAM ${pointer:X4} part {part} X/size");
                AssertEqual(bus.ReadCartridgeByte(at + 2), record.Parts[part].Y.Raw,
                    $"Samus OAM ${pointer:X4} part {part} Y");
                AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(at + 3),
                    record.Parts[part].Attributes.Raw,
                    $"Samus OAM ${pointer:X4} part {part} attributes");
            }
            var nativeOam = new OamBuffer();
            nativeOam.BeginFrame();
            DrawImportedSamusSpritemap(bus, nativeOam, (ushort)index, new(127, 131));
            OamBuffer installedOam = SamusSpritemapDrawsAccess.SamusSpritemapOam((ushort)index,
                new Position16<ScreenPixels>(127, 131), SpriteFrameTestArtwork.Spritemaps(sprites), new SpriteSource(SpriteSourceKind.Samus));
            AssertTrue(nativeOam.ImageEquals(installedOam) &&
                nativeOam.NextByteOffset == installedOam.LastFinalizedSpriteCount * 4,
                $"Samus OAM index {index} installed/native staging parity");
        }
        AssertEqual(1913, nonzeroSpritemaps, "all nonzero Samus OAM pointer entries");
        int zeroIndex = Array.FindIndex(sprites.Entries.ToArray(), entry => entry is null);
        AssertTrue(zeroIndex >= 0, "retail Samus pointer table contains a zero entry");
        // Owner decision on #1318 row 11: a zero entry draws nothing instead of reading direct-page scratch.
        OamBuffer zeroOam = SamusSpritemapDrawsAccess.SamusSpritemapOam((ushort)zeroIndex,
            new Position16<ScreenPixels>(127, 131), SpriteFrameTestArtwork.Spritemaps(sprites), new SpriteSource(SpriteSourceKind.Samus));
        AssertEqual(0, zeroOam.LastFinalizedSpriteCount, "zero Samus pointer draws nothing");
        for (int i = 0; i < stock.LandingYOffsets.Length; i++)
            AssertEqual(bus.ReadCartridgeByte(SamusRenderingRomData.Body.LandingVerticalOffsets + i),
                stock.LandingYOffsets[i], $"Samus landing visual byte {i}");
        for (int i = 0; i < stock.PostureYOffsets.Length; i++)
            AssertEqual(unchecked((sbyte)bus.ReadCartridgeByte(
                SamusRenderingRomData.Body.PostureTransitionVerticalOffsets + i)),
                stock.PostureYOffsets[i], $"Samus posture visual byte {i}");
        for (int i = 0; i < stock.DrainedYOffsets.Length; i++)
            AssertEqual(unchecked((sbyte)bus.ReadCartridgeByte(
                SamusRenderingRomData.Body.DrainedVerticalOffsets + i)),
                stock.DrainedYOffsets[i], $"Samus drained visual byte {i}");

        // The installed selector and both VRAM halves must take the production path
        // without even reading one cartridge byte. This covers every authored pose's
        // initial frame, including all flashback/normal-gameplay pose identities.
        var guardedBus = NewFrontendCartridgeReadGuard(bus);
        // These two production owners call the same indexed bank-$92 OAM routine as
        // Samus's body, but do not call Samus.Draw. Exercise both rather than inferring
        // their binding from the generic catalog's all-index byte-parity test.
        foreach (byte type in new byte[] { 3, 5 })
        {
            var nativeEffects = new SamusAtmosphericEffectsState();
            var installedEffects = new SamusAtmosphericEffectsState();
            nativeEffects.SetSlot(0, type, 0, 2, new(128, 128));
            installedEffects.SetSlot(0, type, 0, 2, new(128, 128));
            OamBuffer nativeEffectOam = SpriteFrameBuffer.DrawnOam(frame => nativeEffects.UpdateAndDraw(frame, new(0, 0), 128,
                SamusBodySheetFixtures.ForAtmosphere(stock.Spritemaps)));
            OamBuffer installedEffectOam = SpriteFrameBuffer.DrawnOam(frame => installedEffects.UpdateAndDraw(frame, new(0, 0), 128,
                SamusBodySheetFixtures.ForAtmosphere(stock.Spritemaps)));
            AssertTrue(nativeEffectOam.LastFinalizedSpriteCount > 0 &&
                nativeEffectOam.ImageEquals(installedEffectOam),
                $"atmospheric type {type} uses installed Samus OAM art with native parity");
        }
        foreach (byte type in new byte[] { 1, 4, 6, 7 })
        for (byte frame = 0; frame < SamusMovementRomData.Environment.DirectAtmosphericFrameCount; frame++)
        {
            ushort pointer = type == 1
                ? SamusMovementRomData.Environment.TypeOneAtmosphericAttributes
                : SamusMovementRomData.Environment.SharedAtmosphericAttributes;
            int source = SamusMovementRomData.Banks.Movement | (pointer + frame * sizeof(ushort));
            ushort expectedAttributes = CartridgeImportSource.Require(bus).ReadCartridgeWord(source);
            AssertTrue(stock.Atmosphere.TryResolve(type, frame, out ushort installedAttributes),
                $"atmospheric type {type} frame {frame} has installed artwork");
            AssertEqual(expectedAttributes, installedAttributes,
                $"atmospheric type {type} frame {frame} imported exact native attributes");
            var nativeEffects = new SamusAtmosphericEffectsState();
            var installedEffects = new SamusAtmosphericEffectsState();
            nativeEffects.SetSlot(0, type, frame, 2, new(128, 128));
            installedEffects.SetSlot(0, type, frame, 2, new(128, 128));
            OamBuffer nativeEffectOam = SpriteFrameBuffer.DrawnOam(frame => nativeEffects.UpdateAndDraw(frame, new(0, 0), 128,
                SamusBodySheetFixtures.ForAtmosphere(atmosphere: stock.Atmosphere)));
            OamBuffer installedEffectOam = SpriteFrameBuffer.DrawnOam(frame => installedEffects.UpdateAndDraw(frame,
                Position16<RoomPixels>.Zero, 128, SamusBodySheetFixtures.ForAtmosphere(stock.Spritemaps, stock.Atmosphere)));
            AssertTrue(nativeEffectOam.LastFinalizedSpriteCount > 0 &&
                nativeEffectOam.ImageEquals(installedEffectOam) &&
                nativeEffects.Slots[0].FrameAndType == installedEffects.Slots[0].FrameAndType &&
                nativeEffects.Slots[0].AnimationTimer.RawValue == installedEffects.Slots[0].AnimationTimer.RawValue,
                $"direct atmospheric type {type} frame {frame} installed/native OAM and timing parity");
        }
        AssertTrue(!stock.Atmosphere.TryResolve(2, 0, out _),
            "atmospheric type two retains its retail pointer-zero address-space path");
        var deathSamus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        };
        deathSamus.DeathSequence.Begin(deathSamus, layer1: new(0, 0));
        var deathCgram = new SamusColourRig();
        for (int frame = 0; deathSamus.DeathSequence.Phase != SamusDeathSequencePhase.SuitExplosion;
            frame++)
        {
            AssertTrue(frame < 200, "retail Samus death reaches the explosion draw phase");
            deathSamus.DeathSequence.Step(deathSamus, deathCgram.Gameplay, deathCgram.Samus, stock.DeathPalettes);
        }
        OamBuffer nativeDeathOam = SpriteFrameBuffer.DrawnOam(frame =>
            deathSamus.DeathSequence.DrawExplosion(frame, SamusBodySheetFixtures.For(stock)));
        OamBuffer installedDeathOam = SpriteFrameBuffer.DrawnOam(frame =>
            deathSamus.DeathSequence.DrawExplosion(frame, SamusBodySheetFixtures.For(stock)));
        AssertTrue(nativeDeathOam.LastFinalizedSpriteCount > 0 &&
            nativeDeathOam.ImageEquals(installedDeathOam),
            "death explosion uses installed Samus OAM art with native parity");
        foreach (SamusDeathTileSegment segment in SamusSpecialSequenceRomData.Death.TileSegments)
        {
            AssertTrue(stock.DeathTiles.TryResolve(segment.SourceAddress,
                SamusSpecialSequenceRomData.Death.TileSegmentByteCount,
                out ReadOnlyMemory<byte> installedSegment),
                $"death tile segment ${segment.SourceAddress:X6} is installed");
            for (int offset = 0; offset < installedSegment.Length; offset++)
                AssertEqual(bus.ReadCartridgeByte(segment.SourceAddress + offset), installedSegment.Span[offset],
                    $"death tile segment ${segment.SourceAddress:X6} byte {offset:X3}");
            var installedQueue = new VramWriteQueue();
            installedQueue.Enqueue(SamusSpecialSequenceRomData.Death.TileSegmentByteCount,
                segment.SourceAddress, segment.Destination);
            var nativeDeathVram = new SnesVram();
            var installedDeathVram = new SnesVram();
            nativeDeathVram.ExecuteQueuedAssetWrite(
                Enumerable.Range(0, SamusSpecialSequenceRomData.Death.TileSegmentByteCount)
                    .Select(offset => bus.ReadCartridgeByte(segment.SourceAddress + offset)).ToArray(),
                segment.Destination.Encoded);
            installedQueue.DrainTo(installedDeathVram, new DeathTileAssetProvider(stock.DeathTiles), null, BgArtworkFixtures.For(installedDeathVram));
            AssertTrue(nativeDeathVram.ImageEquals(installedDeathVram),
                $"death tile segment ${segment.SourceAddress:X6} NMI transfer parity");
            int firstCharacter = (segment.Destination.Encoded - GameplayVramLayout.ObjectCharacters.Offset) /
                (SpriteArtwork.CharacterByteCount / sizeof(ushort));
            AssertTrue(stock.DeathTiles.Explosion.Characters.Slice(firstCharacter * SpriteArtwork.CharacterByteCount,
                    SamusSpecialSequenceRomData.Death.TileSegmentByteCount).SequenceEqual(installedSegment.Span),
                $"death tile segment ${segment.SourceAddress:X6} sits at its native characters in the explosion artwork");
        }
        for (int suit = 0; suit < SamusDeathPaletteArtworkCatalog.SuitCount; suit++)
        for (int palette = 0; palette < SamusPaletteRomData.Death.PaletteCount; palette++)
        {
            int pointerAt = SamusCartridgeTables.SuitPointers +
                (suit * SamusPaletteRomData.Death.PaletteCount + palette) * sizeof(ushort);
            ushort suitPointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(pointerAt);
            int suitlessAt = SamusCartridgeTables.SuitlessPointers + palette * sizeof(ushort);
            ushort suitlessPointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(suitlessAt);
            for (int color = 0; color < SamusDeathPaletteArtworkCatalog.ColorCount; color++)
            {
                int suitedAt = SamusPaletteRomData.Banks.Palette | suitPointer + color * 2;
                int suitlessColorAt = SamusPaletteRomData.Banks.Palette | suitlessPointer + color * 2;
                AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(suitedAt),
                    stock.DeathPalettes.SuitedColor(suit, palette, color),
                    $"death suit {suit} palette {palette} color {color}");
                AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(suitlessColorAt),
                    stock.DeathPalettes.SuitlessColor(palette, color),
                    $"death suitless palette {palette} color {color}");
            }
        }
        for (int shade = 0; shade < SamusPaletteRomData.Death.WhiteoutShadeCount; shade++)
        {
            int at = SamusPaletteRomData.Death.WhiteoutShades + shade * sizeof(ushort);
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(at),
                stock.DeathPalettes.WhiteoutColor(shade), $"death whiteout shade {shade}");
        }
        for (int frame = 0; frame < SamusDeathExplosionTimingDefinitions.RecordCount; frame++)
            AssertEqual(bus.ReadCartridgeByte(SamusPaletteRomData.Death.ExplosionTimingAndPaletteIndices +
                frame * 2 + 1), stock.DeathPalettes.ExplosionPaletteIndex(frame),
                $"death explosion frame {frame} visual palette selector");
        for (int suit = 0; suit < SamusDeathPaletteArtworkCatalog.SuitCount; suit++)
        {
            ushort equipment = suit switch
            {
                1 => SamusEquipmentFlags.VariaSuit.ToNativeWord(),
                2 => SamusEquipmentFlags.GravitySuit.ToNativeWord(),
                _ => 0,
            };
            var nativeSamus = new SamusState
            {
                Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), EquippedItems = (SamusEquipmentFlags)equipment,
            };
            var installedSamus = new SamusState
            {
                Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), EquippedItems = (SamusEquipmentFlags)equipment,
            };
            nativeSamus.DeathSequence.Begin(nativeSamus, Position16<RoomPixels>.Zero);
            installedSamus.DeathSequence.Begin(installedSamus, Position16<RoomPixels>.Zero);
            var nativeColors = new SamusColourRig();
            var installedColors = new SamusColourRig();
            // Preflash still animates Samus's death pose. Begin comparison when
            // palette ownership passes to the independent bank-$9B sequence.
            for (int frame = 0; frame < 16; frame++)
            {
                nativeSamus.DeathSequence.Step(nativeSamus, nativeColors.Gameplay, nativeColors.Samus, stock.DeathPalettes);
                installedSamus.DeathSequence.Step(installedSamus, installedColors.Gameplay, installedColors.Samus, stock.DeathPalettes);
            }
            for (int frame = 0; frame < 240; frame++)
            {
                SamusDeathSequenceStepResult nativeStep = nativeSamus.DeathSequence.Step(
                    nativeSamus, nativeColors.Gameplay, nativeColors.Samus, stock.DeathPalettes);
                SamusDeathSequenceStepResult installedStep = installedSamus.DeathSequence.Step(
                    installedSamus, installedColors.Gameplay, installedColors.Samus, stock.DeathPalettes);
                AssertTrue(new CgramPalettes(nativeColors).ImageEquals(new CgramPalettes(installedColors)) &&
                    nativeStep.PhaseAfterStep == installedStep.PhaseAfterStep &&
                    nativeSamus.DeathSequence.AnimationTimer.RawValue ==
                        installedSamus.DeathSequence.AnimationTimer.RawValue &&
                    nativeSamus.DeathSequence.AnimationCounter ==
                        installedSamus.DeathSequence.AnimationCounter,
                    $"death suit {suit} frame {frame} native/installed colors and timing");
                if (nativeStep.Completed)
                    break;
                AssertTrue(frame < 239, $"death suit {suit} terminates within 240 frames");
            }
        }
        for (int pose = 0; pose < SamusBodyArtworkCatalog.PoseCount; pose++)
        {
            var nativeTransfer = new SamusTileTransferState();
            var installedTransfer = new SamusTileTransferState();
            nativeTransfer.BindArtwork(stock);
            installedTransfer.BindArtwork(stock);
            nativeTransfer.SelectForPoseFrame((SamusPoseId)pose, 0);
            installedTransfer.SelectForPoseFrame((SamusPoseId)pose, 0);
            AssertEqual(nativeTransfer.TopDefinitionAddress, installedTransfer.TopDefinitionAddress,
                $"Samus pose {pose:X2} upper transfer pointer");
            AssertEqual(nativeTransfer.BottomDefinitionAddress, installedTransfer.BottomDefinitionAddress,
                $"Samus pose {pose:X2} lower transfer pointer");
            var nativeVram = new SnesVram();
            var installedVram = new SnesVram();
            nativeTransfer.TransferToVram(nativeVram);
            installedTransfer.TransferToVram(installedVram);
            AssertTrue(nativeVram.ImageEquals(installedVram),
                $"Samus pose {pose:X2} complete split VRAM transfer parity");
        }

        // An override changes pixels and selector data without changing animation timing.
        Directory.CreateDirectory(installation.SamusBodyOverrideDirectory);
        string stockPng = Path.Combine(installation.SamusBodyDirectory, "top-00.png");
        string overridePng = Path.Combine(installation.SamusBodyOverrideDirectory, "top-00.png");
        byte[] original = File.ReadAllBytes(stockPng);
        IndexedPngImage image = IndexedPng.Read(new MemoryStream(original, false), new(64, checked((ushort)(stock.TopSet(0).Count * 16))));
        byte[] pixels = (byte[])image.Pixels.Clone();
        pixels[0] = (byte)((pixels[0] + 1) & 15);
        using (var output = File.Create(overridePng))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), pixels, image.Colours);
        string selectedManifest = Path.Combine(installation.SamusBodyOverrideDirectory,
            SamusBodyArtworkFiles.ManifestFileName);
        JsonNode document = RequiredJson.Parse(File.ReadAllText(Path.Combine(
            installation.SamusBodyDirectory, SamusBodyArtworkFiles.ManifestFileName)));
        byte originalPosition = (byte)document.Required("frames").Required(0).Required("topPosition").GetValue<int>();
        document.Required("frames").Required(0)["topPosition"] = originalPosition == 0 ? 1 : 0;
        sbyte originalYOffset = (sbyte)document.Required("graphicsYOffsets").Required(1).GetValue<int>();
        document.Required("graphicsYOffsets")[1] = originalYOffset + 1;
        JsonNode poseOneRecord = document.Required("spritemaps").Required(
            SamusSpritemapNative.Ordinal(stock.Spritemaps.Entries[stock.Spritemaps.TopBase(SamusPoseId.FacingRightNormalPose)]));
        int originalPartX = poseOneRecord.Required("parts").Required(0).Required("x").GetValue<int>();
        poseOneRecord.Required("parts").Required(0)["x"] = originalPartX + 1;
        foreach (int owner in new[]
        {
            SamusSpritemapNative.Ordinal(stock.Spritemaps.Entries[0x018F]),
            SamusSpritemapNative.Ordinal(stock.Spritemaps.Entries[(deathSamus.DeathSequence.ExplosionSpritemapIndex ?? throw new InvalidOperationException("Verification requires deathSamus.DeathSequence.ExplosionSpritemapIndex."))]),
        })
        {
            JsonNode ownerRecord = document.Required("spritemaps").Required(owner);
            ownerRecord.Required("parts").Required(0)["x"] =
                ownerRecord.Required("parts").Required(0).Required("x").GetValue<int>() + 1;
        }
        byte originalLandingYOffset = (byte)document.Required("landingYOffsets").Required(0).GetValue<int>();
        document.Required("landingYOffsets")[0] = originalLandingYOffset + 1;
        File.WriteAllText(selectedManifest, document.ToJsonString());
        string atmosphereOverride = Path.Combine(installation.SamusBodyOverrideDirectory,
            SamusAtmosphericArtworkFiles.ArtworkFileName);
        JsonNode atmosphereDocument = RequiredJson.Parse(File.ReadAllText(Path.Combine(
            installation.SamusBodyDirectory, SamusAtmosphericArtworkFiles.ArtworkFileName)));
        int originalAttribute = atmosphereDocument.Required("typeOne").Required(0).GetValue<int>();
        atmosphereDocument.Required("typeOne")[0] = originalAttribute + 1;
        File.WriteAllText(atmosphereOverride, atmosphereDocument.ToJsonString());
        string deathOverride = Path.Combine(installation.SamusBodyOverrideDirectory,
            SamusDeathPaletteArtworkFiles.ArtworkFileName);
        JsonNode deathDocument = RequiredJson.Parse(File.ReadAllText(Path.Combine(
            installation.SamusBodyDirectory, SamusDeathPaletteArtworkFiles.ArtworkFileName)));
        int originalDeathColor = deathDocument.Required("suited").Required(0).Required(0).Required(0).GetValue<int>();
        deathDocument.Required("suited").Required(0).Required(0)[0] = originalDeathColor + 1;
        File.WriteAllText(deathOverride, deathDocument.ToJsonString());
        string deathTileOverride = Path.Combine(installation.SamusBodyOverrideDirectory,
            SamusDeathTileAtlasFormat.ArtworkFileName);
        IndexedPngImage deathTileImage = IndexedPng.Read(new MemoryStream(File.ReadAllBytes(
            Path.Combine(installation.SamusBodyDirectory,
                SamusDeathTileAtlasFormat.ArtworkFileName)), writable: false),
            new(SamusDeathTileAtlasFormat.Width, SamusDeathTileAtlasFormat.Height));
        byte[] editedDeathPixels = (byte[])deathTileImage.Pixels.Clone();
        editedDeathPixels[0] = (byte)((editedDeathPixels[0] + 1) & 15);
        using (var output = File.Create(deathTileOverride))
            IndexedPng.Write(output, new(deathTileImage.Size.X, deathTileImage.Size.Y),
                editedDeathPixels, deathTileImage.Colours);
        string armJsonOverride = Path.Combine(installation.SamusBodyOverrideDirectory,
            SamusArmCannonArtworkFormat.JsonFileName);
        JsonNode armDocument = RequiredJson.Parse(File.ReadAllText(Path.Combine(
            installation.SamusBodyDirectory, SamusArmCannonArtworkFormat.JsonFileName)));
        ushort armPosePointer = stock.ArmCannon.PoseDrawingData((int)SamusPoseId.FacingRightNormalPose);
        byte armSelector = stock.ArmCannon.ReadDrawingByteAt(armPosePointer);
        ushort armXAddress = unchecked((ushort)(armPosePointer +
            ((armSelector & 0x80) != 0 ? 4 : 2)));
        int armXIndex = armXAddress - SamusArmCannonArtworkFormat.DrawingDataStart;
        int stockArmX = armDocument.Required("drawingData").Required(armXIndex).GetValue<int>();
        armDocument.Required("drawingData")[armXIndex] = unchecked((byte)(stockArmX + 1));
        File.WriteAllText(armJsonOverride, armDocument.ToJsonString());
        string armPngOverride = Path.Combine(installation.SamusBodyOverrideDirectory,
            SamusArmCannonArtworkFormat.TileFileName);
        IndexedPngImage armImage = IndexedPng.Read(new MemoryStream(File.ReadAllBytes(
            Path.Combine(installation.SamusBodyDirectory,
                SamusArmCannonArtworkFormat.TileFileName)), writable: false),
            new(checked((ushort)(SamusArmCannonArtworkFormat.TileSourcePointers.Count * 8)), 8));
        ushort selectedArmTileSource = stock.ArmCannon.TileSource(armSelector & 0x7f, 3);
        int selectedArmTileIndex = SamusArmCannonArtworkFormat.TileSourcePointers.IndexOf(selectedArmTileSource);
        AssertTrue(selectedArmTileIndex >= 0,
            "standing Samus's fully open cover selects an extracted tile");
        byte[] armPixels = (byte[])armImage.Pixels.Clone();
        int selectedArmPixel = selectedArmTileIndex * 8;
        armPixels[selectedArmPixel] = (byte)((armPixels[selectedArmPixel] + 1) & 15);
        using (var output = File.Create(armPngOverride))
            IndexedPng.Write(output, new(armImage.Size.X, armImage.Size.Y), armPixels,
                armImage.Colours);
        SamusBodyArtworkCatalog replacement = installation.LoadSamusBodyArt();
        AssertTrue(stock.ArmCannon.ReadDrawingByteAt(armXAddress) !=
                replacement.ArmCannon.ReadDrawingByteAt(armXAddress),
            "edited arm-cannon JSON changes a pose OAM X offset");
        ReadOnlyMemory<byte> editedArmTile = default;
        AssertTrue(stock.ArmCannon.TryResolveTile(
                SamusRenderingRomData.Banks.CharacterData |
                    selectedArmTileSource, 32,
                out var stockArmTile) &&
            replacement.ArmCannon.TryResolveTile(
                SamusRenderingRomData.Banks.CharacterData |
                    selectedArmTileSource, 32,
                out editedArmTile) &&
            !stockArmTile.Span.SequenceEqual(editedArmTile.Span),
            "edited arm-cannon indexed PNG changes the production DMA character");
        var originalArmSamus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), SelectedHudItem = HudItem.Missile,
        };
        var editedArmSamus = new SamusState
        {
            Pose = originalArmSamus.Pose, Position = FixedPosition<RoomPixels>.FromWhole(originalArmSamus.Position.Whole), SelectedHudItem = originalArmSamus.SelectedHudItem,
        };
        originalArmSamus.Presentation.TileTransfers.BindBodyFixture(stock);
        editedArmSamus.Presentation.TileTransfers.BindBodyFixture(replacement);
        originalArmSamus.ArmCannon.Artwork = stock.ArmCannon;
        editedArmSamus.ArmCannon.Artwork = replacement.ArmCannon;
        for (int frame = 0; frame < 4; frame++)
        {
            originalArmSamus.ArmCannon.Update(originalArmSamus);
            editedArmSamus.ArmCannon.Update(editedArmSamus);
        }
        var originalArmFrame = new SpriteFrameBuffer();
        var editedArmFrame = new SpriteFrameBuffer();
        originalArmFrame.BeginFrame();
        editedArmFrame.BeginFrame();
        originalArmSamus.ArmCannon.Draw(
            originalArmFrame, originalArmSamus, Position16<RoomPixels>.Zero, 0);
        editedArmSamus.ArmCannon.Draw(
            editedArmFrame, editedArmSamus, Position16<RoomPixels>.Zero, 0);
        OamBuffer originalArmOam = originalArmFrame.FinalizeAsOam();
        OamBuffer editedArmOam = editedArmFrame.FinalizeAsOam();
        AssertTrue(originalArmOam.LastFinalizedSpriteCount == 1 && editedArmOam.LastFinalizedSpriteCount == 1,
            "stock and edited covers both emit a visible OBJ");
        int originalSignedX = unchecked((sbyte)stockArmX);
        int editedSignedX = unchecked((sbyte)(unchecked((byte)(stockArmX + 1))));
        AssertEqual(originalArmOam.GetEntry(0).Position.X + editedSignedX - originalSignedX,
            editedArmOam.GetEntry(0).Position.X,
            "edited arm-cannon pose offset moves the production OAM entry on screen");
        AssertEqual(SamusArmCannonArtworkFormat.TileSourcePointers.IndexOf(unchecked(selectedArmTileSource)),
            SheetFrameSources.CharacterDraw((ImageSpriteDraw)editedArmFrame.Draws[0]).Artwork.Id.Variant,
            "edited cannon PNG belongs to the displayed cover frame");
        AssertEqual(SheetFrameSources.CharacterDraw((ImageSpriteDraw)originalArmFrame.Draws[0]).Artwork.Id, SheetFrameSources.CharacterDraw((ImageSpriteDraw)editedArmFrame.Draws[0]).Artwork.Id,
            "cosmetic cover edits keep the displayed cover frame's identity");
        AssertTrue(!replacement.TopSet(0)[0].Planar.Span.SequenceEqual(stock.TopSet(0)[0].Planar.Span),
            "Samus body PNG override changes compiled tile bytes");
        AssertTrue(replacement.Frames[0].TopPosition != stock.Frames[0].TopPosition,
            "Samus body JSON override changes a visual frame selector");
        ushort poseOneIndex = stock.Spritemaps.TopBase(SamusPoseId.FacingRightNormalPose);
        OamBuffer stockSpritemapOam = SamusSpritemapDrawsAccess.SamusSpritemapOam(poseOneIndex,
            new Position16<ScreenPixels>(128, 128), SpriteFrameTestArtwork.Spritemaps(stock.Spritemaps), new SpriteSource(SpriteSourceKind.Samus));
        OamBuffer editedSpritemapOam = SamusSpritemapDrawsAccess.SamusSpritemapOam(poseOneIndex,
            new Position16<ScreenPixels>(128, 128), SpriteFrameTestArtwork.Spritemaps(replacement.Spritemaps), new SpriteSource(SpriteSourceKind.Samus));
        AssertEqual(unchecked((byte)(stockSpritemapOam.LowTable[0] + 1)),
            editedSpritemapOam.LowTable[0],
            "edited Samus spritemap part changes production OAM X by one pixel");
        var stockSplash = new SamusAtmosphericEffectsState();
        var editedSplash = new SamusAtmosphericEffectsState();
        stockSplash.SetSlot(0, 3, 0, 2, new(128, 128));
        editedSplash.SetSlot(0, 3, 0, 2, new(128, 128));
        OamBuffer stockSplashOam = SpriteFrameBuffer.DrawnOam(frame => stockSplash.UpdateAndDraw(frame, new(0, 0), 128,
            SamusBodySheetFixtures.ForAtmosphere(stock.Spritemaps)));
        OamBuffer editedSplashOam = SpriteFrameBuffer.DrawnOam(frame => editedSplash.UpdateAndDraw(frame, new(0, 0), 128,
            SamusBodySheetFixtures.ForAtmosphere(replacement.Spritemaps)));
        AssertEqual(unchecked((byte)(stockSplashOam.LowTable[0] + 1)),
            editedSplashOam.LowTable[0],
            "edited water-entry spritemap reaches the production atmospheric owner");
        var stockDirect = new SamusAtmosphericEffectsState();
        var editedDirect = new SamusAtmosphericEffectsState();
        stockDirect.SetSlot(0, 1, 0, 2, new(128, 128));
        editedDirect.SetSlot(0, 1, 0, 2, new(128, 128));
        OamBuffer stockDirectOam = SpriteFrameBuffer.DrawnOam(frame => stockDirect.UpdateAndDraw(frame,
            Position16<RoomPixels>.Zero, 128, SamusBodySheetFixtures.ForAtmosphere(stock.Spritemaps, stock.Atmosphere)));
        OamBuffer editedDirectOam = SpriteFrameBuffer.DrawnOam(frame => editedDirect.UpdateAndDraw(frame,
            Position16<RoomPixels>.Zero, 128, SamusBodySheetFixtures.ForAtmosphere(replacement.Spritemaps, replacement.Atmosphere)));
        AssertEqual(unchecked((byte)(stockDirectOam.LowTable[2] + 1)),
            editedDirectOam.LowTable[2],
            "edited atmospheric small-OBJ attribute reaches the production OAM byte");
        var editedDeathSamus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        };
        editedDeathSamus.DeathSequence.Begin(editedDeathSamus, Position16<RoomPixels>.Zero);
        var editedDeathColors = new SamusColourRig();
        for (int frame = 0; frame < 16; frame++)
            editedDeathSamus.DeathSequence.Step(editedDeathSamus,
                editedDeathColors.Gameplay, editedDeathColors.Samus, replacement.DeathPalettes);
        for (int frame = 0; frame < 60; frame++)
            editedDeathSamus.DeathSequence.Step(editedDeathSamus,
                editedDeathColors.Gameplay, editedDeathColors.Samus, replacement.DeathPalettes);
        AssertEqual((ushort)(originalDeathColor + 1),
            editedDeathColors.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart],
            "edited death suit color reaches the production CGRAM write");
        SamusDeathTileSegment firstDeathSegment = SamusSpecialSequenceRomData.Death.TileSegments[0];
        var stockDeathTransfer = new VramWriteQueue();
        var editedDeathTransfer = new VramWriteQueue();
        stockDeathTransfer.Enqueue(SamusSpecialSequenceRomData.Death.TileSegmentByteCount,
            firstDeathSegment.SourceAddress, firstDeathSegment.Destination);
        editedDeathTransfer.Enqueue(SamusSpecialSequenceRomData.Death.TileSegmentByteCount,
            firstDeathSegment.SourceAddress, firstDeathSegment.Destination);
        var stockDeathTilesVram = new SnesVram();
        var editedDeathTilesVram = new SnesVram();
        stockDeathTransfer.DrainTo(stockDeathTilesVram, new DeathTileAssetProvider(stock.DeathTiles), null, BgArtworkFixtures.For(stockDeathTilesVram));
        editedDeathTransfer.DrainTo(editedDeathTilesVram, new DeathTileAssetProvider(replacement.DeathTiles), null, BgArtworkFixtures.For(editedDeathTilesVram));
        AssertTrue(!stockDeathTilesVram.ImageEquals(editedDeathTilesVram),
            "edited death explosion PNG reaches the queued production VRAM transfer");
        OamBuffer stockDeathOam = SpriteFrameBuffer.DrawnOam(frame =>
            deathSamus.DeathSequence.DrawExplosion(frame, SamusBodySheetFixtures.For(stock)));
        OamBuffer editedDeathOam = SpriteFrameBuffer.DrawnOam(frame =>
            deathSamus.DeathSequence.DrawExplosion(frame, SamusBodySheetFixtures.For(replacement)));
        AssertEqual(unchecked((byte)(stockDeathOam.LowTable[0] + 1)),
            editedDeathOam.LowTable[0],
            "edited death explosion spritemap reaches the production sequence owner");
        var stockLandingSamus = new SamusState
        {
            Pose = SamusPoseId.NormalLandingRightPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        };
        var editedLandingSamus = new SamusState
        {
            Pose = SamusPoseId.NormalLandingRightPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        };
        stockLandingSamus.Presentation.TileTransfers.BindBodyFixture(stock);
        editedLandingSamus.Presentation.TileTransfers.BindBodyFixture(replacement);
        stockLandingSamus.Presentation.Draw(stockLandingSamus, new SpriteFrameBuffer(), layer1: new(0, 0));
        editedLandingSamus.Presentation.Draw(editedLandingSamus, new SpriteFrameBuffer(), layer1: new(0, 0));
        AssertEqual(unchecked((ushort)(stockLandingSamus.Presentation.SpritemapPosition.Y - 1)),
            editedLandingSamus.Presentation.SpritemapPosition.Y,
            "edited landing visual offset changes production OAM origin without a ROM read");
        var editedSamus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose };
        editedSamus.Presentation.TileTransfers.BindBodyFixture(replacement);
        AssertEqual((sbyte)(originalYOffset + 1),
            SamusPoseMetadata.ReadGraphicsYOffset(editedSamus),
            "edited Samus visual Y offset is used without a cartridge read");
        var stockSamus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
        stockSamus.Presentation.TileTransfers.BindBodyFixture(stock);
        editedSamus.Position = editedSamus.Position.WithWhole(stockSamus.Position.Whole);
        stockSamus.Presentation.Draw(stockSamus, new SpriteFrameBuffer(), layer1: new(0, 0));
        editedSamus.Presentation.Draw(editedSamus, new SpriteFrameBuffer(), layer1: new(0, 0));
        AssertEqual(unchecked((ushort)(stockSamus.Presentation.SpritemapPosition.Y - 1)),
            editedSamus.Presentation.SpritemapPosition.Y,
            "edited pose graphics Y offset shifts the actual Samus OAM origin one pixel");
        AssertEqual(unchecked((byte)originalYOffset),
            SamusPoseProjectileOriginDefinitions.ReadYOffset(SamusPoseId.FacingRightNormalPose),
            "editing Samus art never changes the compiled projectile collision correction");
        var originalSelector = new SamusTileTransferState();
        var editedSelector = new SamusTileTransferState();
        originalSelector.BindArtwork(stock);
        editedSelector.BindArtwork(replacement);
        originalSelector.SelectForPoseFrame(pose: SamusPoseId.FacingRightNormalPose, animationFrame: 0);
        editedSelector.SelectForPoseFrame(pose: SamusPoseId.FacingRightNormalPose, animationFrame: 0);
        AssertTrue(originalSelector.TopDefinitionAddress != editedSelector.TopDefinitionAddress,
            "edited Samus body JSON changes the production pose-$01 frame selector");
        var stockTransfer = new SamusTileTransferState();
        var editedTransfer = new SamusTileTransferState();
        stockTransfer.BindArtwork(stock);
        editedTransfer.BindArtwork(replacement);
        stockTransfer.SelectForPoseFrame(pose: SamusPoseId.MovingRightNormalPose, animationFrame: 0);
        editedTransfer.SelectForPoseFrame(pose: SamusPoseId.MovingRightNormalPose, animationFrame: 0);
        var stockPixels = new SnesVram();
        var editedPixels = new SnesVram();
        stockTransfer.TransferToVram(stockPixels);
        editedTransfer.TransferToVram(editedPixels);
        AssertTrue(!stockPixels.ImageEquals(editedPixels),
            "edited Samus body PNG changes the production VRAM transfer for pose $09");
        document.Required("topSetStarts")[0] = 1;
        File.WriteAllText(selectedManifest, document.ToJsonString());
        AssertThrows<InvalidDataException>(() => installation.LoadSamusBodyArt(),
            "Samus body override cannot change native set-pointer identity");
        document.Required("topSetStarts")[0] = stock.TopSetStarts[0];
        File.WriteAllText(selectedManifest, document.ToJsonString());
        // Installation repair must replace only the damaged stock copy. Player artwork
        // lives outside the atomically replaced content directory and must remain selected.
        string stockArmPath = Path.Combine(installation.SamusBodyDirectory,
            SamusArmCannonArtworkFormat.TileFileName);
        byte[] originalArmStock = File.ReadAllBytes(stockArmPath);
        File.WriteAllBytes(stockArmPath, [0]);
        GameInstallation repaired = GameAssetInstaller.EnsureInstalled(installation.Root)
            ?? throw new InvalidOperationException("Samus test installation disappeared during repair.");
        AssertTrue(File.ReadAllBytes(stockArmPath).SequenceEqual(originalArmStock),
            "repair restores the hash-checked stock arm-cannon PNG");
        SamusArmCannonArtworkCatalog reboundArm = repaired.LoadSamusBodyArt().ArmCannon;
        AssertTrue(reboundArm.ReadDrawingByteAt(armXAddress) != stock.ArmCannon.ReadDrawingByteAt(armXAddress),
            "repair preserves edited arm-cannon placement JSON");
        AssertTrue(reboundArm.TryResolveTile(SamusRenderingRomData.Banks.CharacterData |
                selectedArmTileSource, 32, out var reboundArmTile) &&
            reboundArmTile.Span.SequenceEqual(editedArmTile.Span),
            "repair preserves edited arm-cannon PNG pixels");
        Console.WriteLine("Samus body art: 253 poses, 1143 frames, 435 split DMAs, 1913 nonzero OAM indices and special draw offsets match retail; PNG/JSON overrides load.");
    }

    /// <summary>Compares arm-cannon pose records and tiles to ROM, then checks guarded cover OAM and DMA output.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus arm cannon artwork")]
    private static void VerifySamusArmCannonArtwork(ISnesAddressSpace bus,
        SamusBodyArtworkCatalog body)
    {
        SamusArmCannonArtworkCatalog armCannon = body.ArmCannon;
        for (int pose = 0; pose < SamusBodyArtworkCatalog.PoseCount; pose++)
        {
            int source = SamusCartridgeTables.PoseDrawingDataPointers + pose * 2;
            ushort native = CartridgeImportSource.Require(bus).ReadCartridgeWord(source);
            AssertEqual(native, armCannon.PoseDrawingData(pose),
                $"arm-cannon pose {pose:X2} drawing pointer");
        }
        for (int offset = 0; offset < SamusArmCannonArtworkFormat.DrawingDataByteCount; offset++)
            AssertEqual(bus.ReadCartridgeByte(SamusRenderingRomData.Banks.Movement |
                    (SamusArmCannonArtworkFormat.DrawingDataStart + offset)),
                armCannon.ReadDrawingByteAt((ushort)(SamusArmCannonArtworkFormat.DrawingDataStart + offset)),
                $"arm-cannon drawing byte {offset:X3}");
        for (int direction = 0; direction < SamusRenderingRomData.ArmCannon.DirectionCount; direction++)
        {
            int attrAddress = SamusRenderingRomData.ArmCannon.SpriteAttributes + direction * 2;
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(attrAddress),
                armCannon.SpriteAttributes(direction), $"arm-cannon direction {direction} OAM");
            int listAddress = SamusCartridgeTables.TileListPointers + direction * 2;
            int list = CartridgeImportSource.Require(bus).ReadCartridgeWord(listAddress);
            for (int frame = 0; frame < SamusArmCannonArtworkFormat.FramesPerDirection; frame++)
            {
                int word = SamusRenderingRomData.Banks.Movement | (list + frame * 2);
                AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(word),
                    armCannon.TileSource(direction, frame),
                    $"arm-cannon direction {direction} frame {frame} tile source");
            }
        }
        foreach (ushort pointer in SamusArmCannonArtworkFormat.TileSourcePointers)
        {
            int source = SamusRenderingRomData.Banks.CharacterData | pointer;
            AssertTrue(armCannon.TryResolveTile(source,
                SamusRenderingRomData.ArmCannon.TileUploadByteCount, out var tile),
                $"arm-cannon character ${source:X6} installed");
            for (int offset = 0; offset < tile.Length; offset++)
                AssertEqual(bus.ReadCartridgeByte(source + offset), tile.Span[offset],
                    $"arm-cannon character ${source:X6} byte {offset:X2}");
        }

        var nativeSamus = new SamusState
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            AnimationFrame = 0, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), SelectedHudItem = HudItem.Missile,
        };
        var installedSamus = new SamusState
        {
            Pose = nativeSamus.Pose, AnimationFrame = nativeSamus.AnimationFrame,
            Position = FixedPosition<RoomPixels>.FromWhole(nativeSamus.Position.Whole),
            SelectedHudItem = nativeSamus.SelectedHudItem,
        };
        // The imported bytes above are the independent stock oracle. Both runtime
        // owners must now receive the installed catalog; no ROM fallback remains.
        nativeSamus.Presentation.TileTransfers.BindBodyFixture(body);
        installedSamus.Presentation.TileTransfers.BindBodyFixture(body);
        nativeSamus.BindArmCannonFixture(armCannon);
        installedSamus.BindArmCannonFixture(armCannon);
        for (int frame = 0; frame < 4; frame++)
        {
            SamusArmCannonUpdateResult native = nativeSamus.ArmCannon.Update(nativeSamus);
            SamusArmCannonUpdateResult installed = installedSamus.ArmCannon.Update(
                installedSamus);
            AssertEqual(native, installed, $"installed arm-cannon update frame {frame}");
        }
        var nativeFrame = new SpriteFrameBuffer();
        var installedFrame = new SpriteFrameBuffer();
        nativeFrame.BeginFrame();
        installedFrame.BeginFrame();
        nativeSamus.ArmCannon.Draw(nativeFrame, nativeSamus, Position16<RoomPixels>.Zero, 0);
        installedSamus.ArmCannon.Draw(
            installedFrame, installedSamus, Position16<RoomPixels>.Zero, 0);
        AssertTrue(nativeFrame.FinalizeAsOam().LowTable.SequenceEqual(installedFrame.FinalizeAsOam().LowTable),
            "installed arm-cannon cover draws identical visible OAM");
        AssertEqual(SheetFrameSources.CharacterDraw((ImageSpriteDraw)nativeFrame.Draws[0]).Artwork.Id,
            SheetFrameSources.CharacterDraw((ImageSpriteDraw)installedFrame.Draws[0]).Artwork.Id,
            "installed arm-cannon cover shows the frame composed over the native cover character");
        Console.WriteLine("Arm-cannon artwork: 253 pose pointers, drawing bytes, ten direction selectors and twelve PNG tiles match retail; guarded cover draw and binding parity pass.");
    }

    /// <summary>Import-only oracle for Samus's bank-$92 OAM record format.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle drawing a Samus spritemap record straight from bank $92")]
    private static void DrawImportedSamusSpritemap(ISnesAddressSpace bus, OamBuffer oam,
        ushort index, Position16<OamPixels> origin)
    {
        int pointerAddress = SamusSpritemapArtworkCatalog.PointerTableAddress + index * 2;
        int recordAddress = 0x920000 | CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(pointerAddress);
        ushort partCount = CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(recordAddress);
        int partAddress = SnesAddressMath.AddWithinBank(recordAddress, 2);
        for (int part = 0; part < partCount; part++)
        {
            oam.AddProjectileSpritePart(new SnesSpritemapXWord(CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(partAddress)),
                bus.ReadCartridgeByte(SnesAddressMath.AddWithinBank(partAddress, 2)),
                CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(SnesAddressMath.AddWithinBank(partAddress, 3)), origin);
            partAddress = SnesAddressMath.AddWithinBank(partAddress, 5);
        }
    }

    /// <summary>Supplies installed death-animation tiles to the fixture's VRAM transfer path.</summary>
    internal sealed class DeathTileAssetProvider(SamusDeathTileAtlas tiles) :
        IVramAssetProvider, IInstalledArtworkTransferSource
    {
        /// <summary>Finds a death tile span by its native source address and transfer size.</summary>
        public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data) =>
            tiles.TryResolve(sourceAddress, byteCount, out data);

        /// <summary>Rejects typed assets because this fixture queues only native-address death tiles.</summary>
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
            throw new InvalidOperationException($"Death-tile fixture did not queue typed asset {asset}.");
    }
}
