using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.FrontendRomFreeVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
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
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Samus.Artwork;
/// <summary>Checks installed Samus body, death, and arm-cannon artwork against retail selectors and rendering transfers.</summary>
internal static class SamusBodyArtworkVerification
{
    /// <summary>Checks every extracted selector and split character transfer against retail.</summary>
    internal static void VerifySamusBodyArtwork(ISnesAddressSpace bus, GameInstallation installation)
    {
        InstalledSamusBodyArtwork stock = installation.LoadSamusBodyArt();
        SamusDeathTileAtlas stockDeathTiles = SamusDeathTileArtworkFiles.Load(installation.SamusBodyDirectory, installation.SamusBodyOverrideDirectory);
        Suite(nameof(VerifySamusArmCannonArtwork), () => VerifySamusArmCannonArtwork(bus, stock));
        int definitions = 0;
        for (int pose = 0; pose < InstalledSamusBodyArtwork.PoseCount; pose++)
        {
            int address = 0x92D94E + pose * 2;
            ushort native = CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
            AssertEqual(native, stock.PosePointers[pose], $"Samus body pose pointer {pose:X2}");
            int graphicsAddress = SamusSourceDefinitions.PoseDefinitions.Value +
                pose * SamusSourceDefinitions.PoseDefinitionByteCount + 4;
            AssertEqual(unchecked((sbyte)bus.ReadCartridgeByte(graphicsAddress)),
                stock.GraphicsYOffset((SamusPoseId)pose), $"Samus pose {pose:X2} visual Y origin");
        }
        for (int index = 0; index < InstalledSamusBodyArtwork.FrameCount; index++)
        {
            int address = InstalledSamusBodyArtwork.FirstFrameAddress + index * 4;
            SamusBodyFrameSelection frame = stock.Frames[index];
            AssertEqual(bus.ReadCartridgeByte(address), frame.TopSet, $"Samus frame {index} upper set");
            AssertEqual(bus.ReadCartridgeByte(address + 1), frame.TopPosition, $"Samus frame {index} upper position");
            AssertEqual(bus.ReadCartridgeByte(address + 2), frame.BottomSet, $"Samus frame {index} lower set");
            AssertEqual(bus.ReadCartridgeByte(address + 3), frame.BottomPosition, $"Samus frame {index} lower position");
        }
        for (int half = 0; half < 2; half++)
        {
            bool upper = half == 0;
            int setCount = upper ? InstalledSamusBodyArtwork.TopSetCount : InstalledSamusBodyArtwork.BottomSetCount;
            int pointerTable = upper ? 0x92D91E : 0x92D938;
            for (int set = 0; set < setCount; set++)
            {
                ushort nativePointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(pointerTable + set * 2);
                AssertEqual(nativePointer,
                    upper ? stock.TopSetPointers[set] : stock.BottomSetPointers[set],
                    $"Samus {(upper ? "top" : "bottom")} set {set} pointer");
                IReadOnlyList<SamusBodyTileTransfer> entries = upper ? stock.TopSet(set) : stock.BottomSet(set);
                for (int position = 0; position < entries.Count; position++)
                {
                    SamusBodyTileTransfer entry = entries[position];
                    int definitionAddress = stock.DefinitionAddress(upper ? SamusBodyHalf.Top : SamusBodyHalf.Bottom, (byte)set, (byte)position);
                    int source = (CartridgeImportSource.Require(bus).ReadCartridgeLong(definitionAddress));
                    ushort first = CartridgeImportSource.Require(bus).ReadCartridgeWord(definitionAddress + 3);
                    ushort second = CartridgeImportSource.Require(bus).ReadCartridgeWord(definitionAddress + 5);
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

        InstalledSamusSpritemaps sprites = stock.Spritemaps;
        for (int pose = 0; pose < InstalledSamusBodyArtwork.PoseCount; pose++)
        {
            int topAddress = InstalledSamusSpritemaps.TopBaseAddress + pose * 2;
            int bottomAddress = InstalledSamusSpritemaps.BottomBaseAddress + pose * 2;
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(topAddress),
                sprites.TopBase((SamusPoseId)pose), $"Samus pose {pose:X2} top OAM base");
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(bottomAddress),
                sprites.BottomBase((SamusPoseId)pose), $"Samus pose {pose:X2} bottom OAM base");
        }
        var spritemapGuard = NewFrontendCartridgeReadGuard(bus);
        int nonzeroSpritemaps = 0;
        ushort[] nativePointers = [.. Enumerable.Range(0, InstalledSamusSpritemaps.EntryCount).Select(index =>
            CartridgeImportSource.Require(bus).ReadCartridgeWord(InstalledSamusSpritemaps.PointerTableAddress + index * 2))];
        SamusSpritemapId?[] nativeEntries = SamusSpritemapNative.Entries(nativePointers);
        for (int index = 0; index < InstalledSamusSpritemaps.EntryCount; index++)
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
                // The installed piece holds what the cartridge's five-byte record describes: the import codec decodes
                // the record, and its offset, size, character, palette row, priority and mirroring are compared.
                SpritePart expectedPart = new SpritemapPart(
                    new SnesSpritemapXWord(CartridgeImportSource.Require(bus).ReadCartridgeWord(at)),
                    new SnesSpritemapYByte(bus.ReadCartridgeByte(at + 2)),
                    new SnesObjAttributeWord(CartridgeImportSource.Require(bus).ReadCartridgeWord(at + 3))).ToPart();
                AssertEqual(expectedPart.Offset, record.Parts[part].Offset, $"Samus OAM ${pointer:X4} part {part} X/Y offset");
                AssertEqual(expectedPart.Large, record.Parts[part].Large, $"Samus OAM ${pointer:X4} part {part} size");
                AssertEqual(expectedPart, record.Parts[part], $"Samus OAM ${pointer:X4} part {part} character, palette, priority and flips");
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
            AssertEqual(bus.ReadCartridgeByte(SamusSourceDefinitions.LandingVerticalOffsets.Value + i),
                stock.LandingYOffsets[i], $"Samus landing visual byte {i}");
        for (int i = 0; i < stock.PostureYOffsets.Length; i++)
            AssertEqual(unchecked((sbyte)bus.ReadCartridgeByte(
                SamusSourceDefinitions.PostureTransitionVerticalOffsets.Value + i)),
                stock.PostureYOffsets[i], $"Samus posture visual byte {i}");
        for (int i = 0; i < stock.DrainedYOffsets.Length; i++)
            AssertEqual(unchecked((sbyte)bus.ReadCartridgeByte(
                SamusSourceDefinitions.DrainedVerticalOffsets.Value + i)),
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
            nativeEffects.SetSlot(0, (SamusAtmosphericEffectType)type, 0, 2, new(128, 128));
            installedEffects.SetSlot(0, (SamusAtmosphericEffectType)type, 0, 2, new(128, 128));
            OamBuffer nativeEffectOam = SpriteFrameBuffer.DrawnOam(frame => nativeEffects.UpdateAndDraw(frame, new(0, 0), 128,
                SamusBodySheetFixtures.ForAtmosphere(stock.Spritemaps)));
            OamBuffer installedEffectOam = SpriteFrameBuffer.DrawnOam(frame => installedEffects.UpdateAndDraw(frame, new(0, 0), 128,
                SamusBodySheetFixtures.ForAtmosphere(stock.Spritemaps)));
            AssertTrue(nativeEffectOam.LastFinalizedSpriteCount > 0 &&
                nativeEffectOam.ImageEquals(installedEffectOam),
                $"atmospheric type {type} uses installed Samus OAM art with native parity");
        }
        foreach (byte type in new byte[] { 1, 4, 6, 7 })
        for (byte frame = 0; frame < SamusMovementDefinitions.Environment.DirectAtmosphericFrameCount; frame++)
        {
            ushort pointer = type == 1
                ? SamusSourceDefinitions.TypeOneAtmosphericAttributes
                : SamusSourceDefinitions.SharedAtmosphericAttributes;
            int source = SamusSourceDefinitions.MovementBank.Value | (pointer + frame * sizeof(ushort));
            // The cartridge's attribute word is decoded by the import codec into the single piece it describes.
            SpritePart expectedAttributes = new SnesObjAttributeWord(CartridgeImportSource.Require(bus).ReadCartridgeWord(source)).ToSinglePart();
            AssertTrue(stock.Atmosphere.TryResolve(type, frame, out SpritePart installedAttributes),
                $"atmospheric type {type} frame {frame} has installed artwork");
            AssertEqual(expectedAttributes, installedAttributes,
                $"atmospheric type {type} frame {frame} imported the native character, palette row, priority and flips");
            var nativeEffects = new SamusAtmosphericEffectsState();
            var installedEffects = new SamusAtmosphericEffectsState();
            nativeEffects.SetSlot(0, (SamusAtmosphericEffectType)type, frame, 2, new(128, 128));
            installedEffects.SetSlot(0, (SamusAtmosphericEffectType)type, frame, 2, new(128, 128));
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
        // $9B:8000: the explosion's characters, in the order its spritemaps name them.
        ReadOnlySpan<byte> deathCharacters = stockDeathTiles.Explosion.Characters;
        for (int offset = 0; offset < deathCharacters.Length; offset++)
            AssertEqual(bus.ReadCartridgeByte(0x9b8000 + offset), deathCharacters[offset],
                $"death explosion character byte {offset:X4} is the native character byte");
        for (int suit = 0; suit < InstalledSamusDeathPalettes.SuitCount; suit++)
        for (int palette = 0; palette < SamusPaletteDefinitions.Death.PaletteCount; palette++)
        {
            int pointerAt = SamusSourceDefinitions.SuitPointers.Value +
                (suit * SamusPaletteDefinitions.Death.PaletteCount + palette) * sizeof(ushort);
            ushort suitPointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(pointerAt);
            int suitlessAt = SamusSourceDefinitions.SuitlessPointers.Value + palette * sizeof(ushort);
            ushort suitlessPointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(suitlessAt);
            for (int color = 0; color < InstalledSamusDeathPalettes.ColorCount; color++)
            {
                int suitedAt = SamusSourceDefinitions.PaletteBank.Value | suitPointer + color * 2;
                int suitlessColorAt = SamusSourceDefinitions.PaletteBank.Value | suitlessPointer + color * 2;
                AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(suitedAt),
                    stock.DeathPalettes.SuitedColor(suit, palette, color),
                    $"death suit {suit} palette {palette} color {color}");
                AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(suitlessColorAt),
                    stock.DeathPalettes.SuitlessColor(palette, color),
                    $"death suitless palette {palette} color {color}");
            }
        }
        for (int shade = 0; shade < SamusPaletteDefinitions.Death.WhiteoutShadeCount; shade++)
        {
            int at = SamusSourceDefinitions.DeathWhiteoutShades.Value + shade * sizeof(ushort);
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(at),
                stock.DeathPalettes.WhiteoutColor(shade), $"death whiteout shade {shade}");
        }
        for (int frame = 0; frame < SamusDeathExplosionTimingDefinitions.RecordCount; frame++)
            AssertEqual(bus.ReadCartridgeByte(SamusSourceDefinitions.DeathExplosionTimingAndPaletteIndices.Value +
                frame * 2 + 1), stock.DeathPalettes.ExplosionPaletteIndex(frame),
                $"death explosion frame {frame} visual palette selector");
        for (int suit = 0; suit < InstalledSamusDeathPalettes.SuitCount; suit++)
        {
            ushort equipment = suit switch
            {
                1 => SamusEquipmentFlags.VariaSuit.ToEquipmentWord(),
                2 => SamusEquipmentFlags.GravitySuit.ToEquipmentWord(),
                _ => 0,
            };
            var nativeSamus = new SamusState
            {
                Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(equipment),
            };
            var installedSamus = new SamusState
            {
                Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(equipment),
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
        for (int pose = 0; pose < InstalledSamusBodyArtwork.PoseCount; pose++)
        {
            var frames = new SamusBodyFrameState();
            frames.BindArtwork(stock);
            frames.Select((SamusPoseId)pose, 0);
            SamusBodyFrameSelection selection = stock.Frame((SamusPoseId)pose, 0);
            AssertEqual(stock.DefinitionRecord(SamusBodyHalf.Top, selection.TopSet, selection.TopPosition),
                frames.Shown.Top ?? -1, $"Samus pose {pose:X2} shows its frame-zero top definition");
            AssertEqual(selection.NamesBottom
                    ? stock.DefinitionRecord(SamusBodyHalf.Bottom, selection.BottomSet, selection.BottomPosition) : null,
                frames.Shown.Bottom, $"Samus pose {pose:X2} shows its frame-zero bottom definition, or none before any is named");
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
        int armXIndex = armXAddress - SamusSourceDefinitions.ArmCannonDrawingData;
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
        InstalledSamusBodyArtwork replacement = installation.LoadSamusBodyArt();
        SamusDeathTileAtlas replacementDeathTiles = SamusDeathTileArtworkFiles.Load(installation.SamusBodyDirectory, installation.SamusBodyOverrideDirectory);
        AssertTrue(stock.ArmCannon.ReadDrawingByteAt(armXAddress) !=
                replacement.ArmCannon.ReadDrawingByteAt(armXAddress),
            "edited arm-cannon JSON changes a pose OAM X offset");
        ReadOnlyMemory<byte> editedArmTile = default;
        AssertTrue(stock.ArmCannon.TryResolveTile(
                SamusSourceDefinitions.CharacterDataBank.Value |
                    selectedArmTileSource, 32,
                out var stockArmTile) &&
            replacement.ArmCannon.TryResolveTile(
                SamusSourceDefinitions.CharacterDataBank.Value |
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
        originalArmSamus.Presentation.BodyFrames.BindBodyFixture(stock);
        editedArmSamus.Presentation.BodyFrames.BindBodyFixture(replacement);
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
        stockSplash.SetSlot(0, SamusAtmosphericEffectType.DivingSplash, 0, 2, new(128, 128));
        editedSplash.SetSlot(0, SamusAtmosphericEffectType.DivingSplash, 0, 2, new(128, 128));
        OamBuffer stockSplashOam = SpriteFrameBuffer.DrawnOam(frame => stockSplash.UpdateAndDraw(frame, new(0, 0), 128,
            SamusBodySheetFixtures.ForAtmosphere(stock.Spritemaps)));
        OamBuffer editedSplashOam = SpriteFrameBuffer.DrawnOam(frame => editedSplash.UpdateAndDraw(frame, new(0, 0), 128,
            SamusBodySheetFixtures.ForAtmosphere(replacement.Spritemaps)));
        AssertEqual(unchecked((byte)(stockSplashOam.LowTable[0] + 1)),
            editedSplashOam.LowTable[0],
            "edited water-entry spritemap reaches the production atmospheric owner");
        var stockDirect = new SamusAtmosphericEffectsState();
        var editedDirect = new SamusAtmosphericEffectsState();
        stockDirect.SetSlot(0, SamusAtmosphericEffectType.FootstepSplash, 0, 2, new(128, 128));
        editedDirect.SetSlot(0, SamusAtmosphericEffectType.FootstepSplash, 0, 2, new(128, 128));
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
            editedDeathColors.Colors[SamusPaletteDefinitions.Common.SuitFirstColour],
            "edited death suit color reaches the production CGRAM write");
        AssertTrue(!stockDeathTiles.Explosion.Characters.SequenceEqual(replacementDeathTiles.Explosion.Characters),
            "edited death explosion PNG reaches the explosion characters its sheet is composed from");
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
        stockLandingSamus.Presentation.BodyFrames.BindBodyFixture(stock);
        editedLandingSamus.Presentation.BodyFrames.BindBodyFixture(replacement);
        stockLandingSamus.Presentation.Draw(stockLandingSamus, new SpriteFrameBuffer(), layer1: new(0, 0));
        editedLandingSamus.Presentation.Draw(editedLandingSamus, new SpriteFrameBuffer(), layer1: new(0, 0));
        AssertEqual(unchecked((ushort)(stockLandingSamus.Presentation.SpritemapPosition.Y - 1)),
            editedLandingSamus.Presentation.SpritemapPosition.Y,
            "edited landing visual offset changes production OAM origin without a ROM read");
        var editedSamus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose };
        editedSamus.Presentation.BodyFrames.BindBodyFixture(replacement);
        AssertEqual((sbyte)(originalYOffset + 1),
            SamusPoseMetadata.ReadGraphicsYOffset(editedSamus),
            "edited Samus visual Y offset is used without a cartridge read");
        var stockSamus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
        stockSamus.Presentation.BodyFrames.BindBodyFixture(stock);
        editedSamus.Position = editedSamus.Position.WithWhole(stockSamus.Position.Whole);
        stockSamus.Presentation.Draw(stockSamus, new SpriteFrameBuffer(), layer1: new(0, 0));
        editedSamus.Presentation.Draw(editedSamus, new SpriteFrameBuffer(), layer1: new(0, 0));
        AssertEqual(unchecked((ushort)(stockSamus.Presentation.SpritemapPosition.Y - 1)),
            editedSamus.Presentation.SpritemapPosition.Y,
            "edited pose graphics Y offset shifts the actual Samus OAM origin one pixel");
        AssertEqual(unchecked((byte)originalYOffset),
            SamusPoseProjectileOrigins.ReadYOffset(SamusPoseId.FacingRightNormalPose),
            "editing Samus art never changes the compiled projectile collision correction");
        var originalSelector = new SamusBodyFrameState();
        var editedSelector = new SamusBodyFrameState();
        originalSelector.BindArtwork(stock);
        editedSelector.BindArtwork(replacement);
        originalSelector.Select(pose: SamusPoseId.FacingRightNormalPose, animationFrame: 0);
        editedSelector.Select(pose: SamusPoseId.FacingRightNormalPose, animationFrame: 0);
        AssertTrue(originalSelector.TopDefinitionAddress != editedSelector.TopDefinitionAddress,
            "edited Samus body JSON changes the production pose-$01 frame selector");
        var stockFrames = new SamusBodyFrameState();
        var editedFrames = new SamusBodyFrameState();
        stockFrames.BindArtwork(stock);
        editedFrames.BindArtwork(replacement);
        stockFrames.Select(pose: SamusPoseId.MovingRightNormalPose, animationFrame: 0);
        editedFrames.Select(pose: SamusPoseId.MovingRightNormalPose, animationFrame: 0);
        AssertTrue(!stock.BodyArtwork(stockFrames.Shown.Top, stockFrames.Shown.Bottom).Characters.SequenceEqual(
                replacement.BodyArtwork(editedFrames.Shown.Top, editedFrames.Shown.Bottom).Characters),
            "edited Samus body PNG changes the picture production shows for pose $09");
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
        InstalledSamusArmCannonArtwork reboundArm = repaired.LoadSamusBodyArt().ArmCannon;
        AssertTrue(reboundArm.ReadDrawingByteAt(armXAddress) != stock.ArmCannon.ReadDrawingByteAt(armXAddress),
            "repair preserves edited arm-cannon placement JSON");
        AssertTrue(reboundArm.TryResolveTile(SamusSourceDefinitions.CharacterDataBank.Value |
                selectedArmTileSource, 32, out var reboundArmTile) &&
            reboundArmTile.Span.SequenceEqual(editedArmTile.Span),
            "repair preserves edited arm-cannon PNG pixels");
        Console.WriteLine("Samus body art: 253 poses, 1143 frames, 435 split DMAs, 1913 nonzero OAM indices and special draw offsets match retail; PNG/JSON overrides load.");
    }

    /// <summary>Compares arm-cannon pose records and tiles to ROM, then checks guarded cover OAM and DMA output.</summary>
    private static void VerifySamusArmCannonArtwork(ISnesAddressSpace bus,
        InstalledSamusBodyArtwork body)
    {
        InstalledSamusArmCannonArtwork armCannon = body.ArmCannon;
        for (int pose = 0; pose < InstalledSamusBodyArtwork.PoseCount; pose++)
        {
            int source = SamusSourceDefinitions.PoseDrawingDataPointers.Value + pose * 2;
            ushort native = CartridgeImportSource.Require(bus).ReadCartridgeWord(source);
            AssertEqual(native, armCannon.PoseDrawingData(pose),
                $"arm-cannon pose {pose:X2} drawing pointer");
        }
        for (int offset = 0; offset < SamusArmCannonArtworkFormat.DrawingDataByteCount; offset++)
            AssertEqual(bus.ReadCartridgeByte(SamusSourceDefinitions.MovementBank.Value |
                    (SamusSourceDefinitions.ArmCannonDrawingData + offset)),
                armCannon.ReadDrawingByteAt((ushort)(SamusSourceDefinitions.ArmCannonDrawingData + offset)),
                $"arm-cannon drawing byte {offset:X3}");
        for (int direction = 0; direction < SamusDrawingDefinitions.ArmCannon.DirectionCount; direction++)
        {
            int attrAddress = SamusSourceDefinitions.ArmCannonSpriteAttributes.Value + direction * 2;
            AssertEqual(new SnesObjAttributeWord(CartridgeImportSource.Require(bus).ReadCartridgeWord(attrAddress)).ToSinglePart(),
                armCannon.CoverAppearance(direction), $"arm-cannon direction {direction} cover character, palette row, priority and flips");
            int listAddress = SamusSourceDefinitions.TileListPointers.Value + direction * 2;
            int list = CartridgeImportSource.Require(bus).ReadCartridgeWord(listAddress);
            for (int frame = 0; frame < SamusArmCannonArtworkFormat.FramesPerDirection; frame++)
            {
                int word = SamusSourceDefinitions.MovementBank.Value | (list + frame * 2);
                AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(word),
                    armCannon.TileSource(direction, frame),
                    $"arm-cannon direction {direction} frame {frame} tile source");
            }
        }
        foreach (ushort pointer in SamusArmCannonArtworkFormat.TileSourcePointers)
        {
            int source = SamusSourceDefinitions.CharacterDataBank.Value | pointer;
            AssertTrue(armCannon.TryResolveTile(source,
                SamusDrawingDefinitions.ArmCannon.TileByteCount, out var tile),
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
        nativeSamus.Presentation.BodyFrames.BindBodyFixture(body);
        installedSamus.Presentation.BodyFrames.BindBodyFixture(body);
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
    private static void DrawImportedSamusSpritemap(ISnesAddressSpace bus, OamBuffer oam,
        ushort index, Position16<SpritePixels> origin)
    {
        int pointerAddress = InstalledSamusSpritemaps.PointerTableAddress + index * 2;
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
}
