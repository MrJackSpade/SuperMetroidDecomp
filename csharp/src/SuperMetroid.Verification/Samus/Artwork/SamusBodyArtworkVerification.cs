using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Samus.Artwork;
namespace SuperMetroid.Verification.Samus.Artwork;
/// <summary>Checks installed Samus body, death, and arm-cannon artwork against retail selectors and rendering transfers.</summary>
internal static class SamusBodyArtworkVerification
{
    /// <summary>Checks every extracted selector and split character transfer of the repository installation against retail.</summary>
    internal static void VerifySamusBodyArtwork(ISnesAddressSpace bus)
    {
        // The installation's body characters and death tiles are the shared Samus sheet inputs, loaded once per process
        // with the sheets they compose (#1532).
        SamusBodyCharacterArtwork stockCharacters = SamusBodySheetFixtures.Inputs.SamusBody;
        SamusBodySources stock = stockCharacters.Body;
        // Her death colours are import data (#1637); the installation holds only the death steps the game reads.
        InstalledSamusDeathPalettes stockDeathPalettes = SamusDeathPaletteArtworkFiles.LoadForImport(
            SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.SamusBodyDirectoryName));
        AssertTrue(!File.Exists(Path.Combine(RepositoryInstallation.Installation.SamusBodyDirectory, SamusDeathPaletteArtworkFiles.ArtworkFileName)),
            "the installation holds none of Samus's death colours");
        SuperMetroid.Core.Samus.Death.SamusDeathSteps installedSteps = SamusDeathPaletteArtworkFiles.LoadSteps(RepositoryInstallation.Installation.SamusBodyDirectory);
        AssertEqual(stockDeathPalettes.Steps.ContentIdentity, installedSteps.ContentIdentity,
            "the installed death steps are the death palettes' explosion steps and whiteout colours");
        SamusDeathTileAtlas stockDeathTiles = SamusBodySheetFixtures.Inputs.DeathTiles;
        Suite(nameof(VerifySamusArmCannonArtwork), () => VerifySamusArmCannonArtwork(bus, stock));
        int definitions = 0;
        IReadOnlyDictionary<SamusPoseId, ushort> posePointers = stock.PosePointers;
        AssertEqual(SamusPoseNumbers.RecordedPoses.Count, posePointers.Count, "Samus body installs a frame list for each pose with a record and none for a record no pose names");
        for (int pose = 0; pose < SamusBodySources.PoseCount; pose++)
        {
            if (!SamusPoseNumbering.NamesPose(pose)) continue;
            int address = 0x92D94E + pose * 2;
            ushort native = CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
            AssertEqual(native, posePointers[SamusPoseNumbering.FromNumber(pose)], $"Samus body pose pointer {pose:X2}");
            int graphicsAddress = SamusSourceDefinitions.PoseDefinitions.Value +
                pose * SamusSourceDefinitions.PoseDefinitionByteCount + 4;
            AssertEqual(unchecked((sbyte)bus.ReadCartridgeByte(graphicsAddress)),
                stock.Gameplay.GraphicsYOffset(SamusPoseNumbering.FromNumber(pose)), $"Samus pose {pose:X2} visual Y origin");
        }
        for (int index = 0; index < SamusBodySources.FrameCount; index++)
        {
            int address = SamusBodySources.FirstFrameAddress + index * 4;
            SamusBodyFrameSelection frame = stock.Frames[index];
            AssertEqual(bus.ReadCartridgeByte(address), frame.TopSet, $"Samus frame {index} upper set");
            AssertEqual(bus.ReadCartridgeByte(address + 1), frame.TopPosition, $"Samus frame {index} upper position");
            AssertEqual(bus.ReadCartridgeByte(address + 2), frame.BottomSet, $"Samus frame {index} lower set");
            AssertEqual(bus.ReadCartridgeByte(address + 3), frame.BottomPosition, $"Samus frame {index} lower position");
        }
        for (int half = 0; half < 2; half++)
        {
            bool upper = half == 0;
            int setCount = upper ? SamusBodySources.TopSetCount : SamusBodySources.BottomSetCount;
            int pointerTable = upper ? 0x92D91E : 0x92D938;
            for (int set = 0; set < setCount; set++)
            {
                ushort nativePointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(pointerTable + set * 2);
                AssertEqual(nativePointer,
                    upper ? stock.TopSetPointers[set] : stock.BottomSetPointers[set],
                    $"Samus {(upper ? "top" : "bottom")} set {set} pointer");
                IReadOnlyList<SamusBodyTileTransfer> entries = upper ? stockCharacters.TopSet(set) : stockCharacters.BottomSet(set);
                for (int position = 0; position < entries.Count; position++)
                {
                    SamusBodyTileTransfer entry = entries[position];
                    int definitionAddress = stock.DefinitionAddress(upper ? SamusBodyHalf.Top : SamusBodyHalf.Bottom, (byte)set, (byte)position);
                    int source = (CartridgeImportSource.Require(bus).ReadCartridgeLong(definitionAddress));
                    ushort first = CartridgeImportSource.Require(bus).ReadCartridgeWord(definitionAddress + 3);
                    ushort second = CartridgeImportSource.Require(bus).ReadCartridgeWord(definitionAddress + 5);
                    AssertEqual(first, entry.FirstSize, $"Samus definition {definitionAddress:X6} first size");
                    AssertEqual(second, entry.SecondSize, $"Samus definition {definitionAddress:X6} second size");
                    // Planar rebuilds the definition's bytes on every read, so it is read once per definition.
                    ReadOnlySpan<byte> planar = entry.Planar.Span;
                    for (int i = 0; i < planar.Length; i++)
                        AssertEqual(bus.ReadCartridgeByte(source + i), planar[i],
                            $"Samus definition {definitionAddress:X6} tile byte {i:X3}");
                    definitions++;
                }
            }
        }
        AssertEqual(435, definitions, "complete Samus body DMA definition count");

        SamusSpritemapSources sprites = stock.Spritemaps;
        for (int pose = 0; pose < SamusBodySources.PoseCount; pose++)
        {
            if (!SamusPoseNumbering.NamesPose(pose)) continue;
            int topAddress = SamusSpritemapSources.TopBaseAddress + pose * 2;
            int bottomAddress = SamusSpritemapSources.BottomBaseAddress + pose * 2;
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(topAddress),
                sprites.TopBase(SamusPoseNumbering.FromNumber(pose)), $"Samus pose {pose:X2} top OAM base");
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(bottomAddress),
                sprites.BottomBase(SamusPoseNumbering.FromNumber(pose)), $"Samus pose {pose:X2} bottom OAM base");
        }
        int nonzeroSpritemaps = 0;
        ushort[] nativePointers = [.. Enumerable.Range(0, SamusSpritemapSources.EntryCount).Select(index =>
            CartridgeImportSource.Require(bus).ReadCartridgeWord(SamusSpritemapSources.PointerTableAddress + index * 2))];
        SamusSpritemapId?[] nativeEntries = SamusSpritemapNative.Entries(nativePointers);
        for (int index = 0; index < SamusSpritemapSources.EntryCount; index++)
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
        // The installation holds each pose's and frame's placement, decoded once from the cartridge's tables (#1517).
        Dictionary<SamusPoseId, SamusBodyPosePlacement> expectedPlacements = SamusBodyPlacementDecoding.Decode(
            Enumerable.Range(0, SamusBodySources.PoseCount).Select(pose => (int)unchecked((sbyte)bus.ReadCartridgeByte(
                SamusSourceDefinitions.PoseDefinitions.Value + pose * SamusSourceDefinitions.PoseDefinitionByteCount + 4))).ToArray(),
            Enumerable.Range(0, SamusBodyPlacementSourceDefinitions.LandingByteCount).Select(i =>
                (int)bus.ReadCartridgeByte(SamusSourceDefinitions.LandingVerticalOffsets.Value + i)).ToArray(),
            Enumerable.Range(0, SamusBodyPlacementSourceDefinitions.PostureByteCount).Select(i =>
                (int)unchecked((sbyte)bus.ReadCartridgeByte(SamusSourceDefinitions.PostureTransitionVerticalOffsets.Value + i))).ToArray(),
            Enumerable.Range(0, SamusBodyPlacementSourceDefinitions.DrainedByteCount).Select(i =>
                (int)unchecked((sbyte)bus.ReadCartridgeByte(SamusSourceDefinitions.DrainedVerticalOffsets.Value + i))).ToArray());
        AssertEqual(expectedPlacements.Count, stock.Placements.Count, "Samus placements cover every pose with a record");
        foreach (KeyValuePair<SamusPoseId, SamusBodyPosePlacement> expected in expectedPlacements)
        {
            SamusBodyPosePlacement installed = stock.Placements[expected.Key];
            AssertEqual(expected.Value.GraphicsYOffset, installed.GraphicsYOffset, $"Samus pose {expected.Key} installed origin");
            AssertTrue(expected.Value.FrameRises is null ? installed.FrameRises is null
                : installed.FrameRises is { } rises && rises.SequenceEqual(expected.Value.FrameRises),
                $"Samus pose {expected.Key} installed frame placements");
        }

        // The installed selector and both VRAM halves must take the production path
        // without even reading one cartridge byte. This covers every authored pose's
        // initial frame, including all flashback/normal-gameplay pose identities.
        // These two production owners call the same indexed bank-$92 OAM routine as
        // Samus's body, but do not call Samus.Draw. Exercise both rather than inferring
        // their binding from the generic catalog's all-index byte-parity test.
        static SamusParticles OneParticle(int type, int step)
        {
            var particles = new SamusParticles();
            particles.Add(new SamusParticle(SamusAtmosphericEffectWordCodec.KindNumbered(type)
                ?? throw new InvalidDataException($"Effect number {type} names no particle."), step,
                SamusParticleStart.Shown(2), new(128, 128), Offset16<RoomPixels>.Zero));
            return particles;
        }
        foreach (byte type in new byte[] { 3, 5 })
        {
            SamusParticles nativeEffects = OneParticle(type, 0);
            SamusParticles installedEffects = OneParticle(type, 0);
            OamBuffer nativeEffectOam = SpriteFrameBuffer.DrawnOam(frame => nativeEffects.UpdateAndDraw(frame, new(0, 0), 128,
                SamusBodySheetFixtures.ForAtmosphere(stock.Spritemaps)));
            OamBuffer installedEffectOam = SpriteFrameBuffer.DrawnOam(frame => installedEffects.UpdateAndDraw(frame, new(0, 0), 128,
                SamusBodySheetFixtures.ForAtmosphere(stock.Spritemaps)));
            AssertTrue(nativeEffectOam.LastFinalizedSpriteCount > 0 &&
                nativeEffectOam.ImageEquals(installedEffectOam),
                $"atmospheric type {type} uses installed Samus OAM art with native parity");
        }
        foreach (byte type in new byte[] { 1, 4, 6, 7 })
        for (byte frame = 0; frame < SamusAtmosphericArtworkDefinitions.DirectAtmosphericFrameCount; frame++)
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
            SamusParticles nativeEffects = OneParticle(type, frame);
            SamusParticles installedEffects = OneParticle(type, frame);
            OamBuffer nativeEffectOam = SpriteFrameBuffer.DrawnOam(frame => nativeEffects.UpdateAndDraw(frame, new(0, 0), 128,
                SamusBodySheetFixtures.ForAtmosphere(atmosphere: stock.Atmosphere)));
            OamBuffer installedEffectOam = SpriteFrameBuffer.DrawnOam(frame => installedEffects.UpdateAndDraw(frame,
                Position16<RoomPixels>.Zero, 128, SamusBodySheetFixtures.ForAtmosphere(stock.Spritemaps, stock.Atmosphere)));
            AssertTrue(nativeEffectOam.LastFinalizedSpriteCount > 0 &&
                nativeEffectOam.ImageEquals(installedEffectOam) &&
                nativeEffects.All[0].Kind == installedEffects.All[0].Kind &&
                nativeEffects.All[0].AnimationStep == installedEffects.All[0].AnimationStep &&
                nativeEffects.All[0].StepUpdatesLeft == installedEffects.All[0].StepUpdatesLeft,
                $"direct atmospheric type {type} frame {frame} installed/native OAM and timing parity");
        }
        AssertTrue(!stock.Atmosphere.TryResolve(2, 0, out _),
            "atmospheric type two retains its retail pointer-zero address-space path");
        var deathSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        };
        deathSamus.DeathSequence.Begin(deathSamus, cameraPosition: new(0, 0));
        var deathCgram = new SamusColours();
        for (int frame = 0; deathSamus.DeathSequence.Phase != SamusDeathSequencePhase.SuitExplosion;
            frame++)
        {
            AssertTrue(frame < 200, "retail Samus death reaches the explosion draw phase");
            deathSamus.DeathSequence.Step(deathSamus, deathCgram, stock.Gameplay.DeathSteps);
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
                    stockDeathPalettes.SuitedColor(suit, palette, color),
                    $"death suit {suit} palette {palette} color {color}");
                AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(suitlessColorAt),
                    stockDeathPalettes.SuitlessColor(palette, color),
                    $"death suitless palette {palette} color {color}");
            }
        }
        for (int shade = 0; shade < SamusPaletteDefinitions.Death.WhiteoutShadeCount; shade++)
        {
            int at = SamusSourceDefinitions.DeathWhiteoutShades.Value + shade * sizeof(ushort);
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(at),
                stockDeathPalettes.WhiteoutColor(shade), $"death whiteout shade {shade}");
        }
        for (int frame = 0; frame < SamusDeathExplosionTimingDefinitions.StepCount; frame++)
            AssertEqual(bus.ReadCartridgeByte(SamusSourceDefinitions.DeathExplosionTimingAndPaletteIndices.Value +
                frame * 2 + 1), stockDeathPalettes.ExplosionPaletteIndex(frame),
                $"death explosion frame {frame} visual palette selector");
        for (int suit = 0; suit < InstalledSamusDeathPalettes.SuitCount; suit++)
        {
            ushort equipment = suit switch
            {
                1 => SamusEquipmentFlags.VariaSuit.ToEquipmentWord(),
                2 => SamusEquipmentFlags.GravitySuit.ToEquipmentWord(),
                _ => 0,
            };
            var nativeSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(equipment),
            };
            var installedSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(equipment),
            };
            nativeSamus.DeathSequence.Begin(nativeSamus, Position16<RoomPixels>.Zero);
            installedSamus.DeathSequence.Begin(installedSamus, Position16<RoomPixels>.Zero);
            var nativeColors = new SamusColours();
            var installedColors = new SamusColours();
            // Preflash still animates Samus's death pose. Begin comparison when
            // palette ownership passes to the independent bank-$9B sequence.
            for (int frame = 0; frame < 16; frame++)
            {
                nativeSamus.DeathSequence.Step(nativeSamus, nativeColors, stock.Gameplay.DeathSteps);
                installedSamus.DeathSequence.Step(installedSamus, installedColors, stock.Gameplay.DeathSteps);
            }
            int pairFrames = 0;
            for (int frame = 0; frame < 240; frame++)
            {
                SamusDeathSequenceStepResult nativeStep = nativeSamus.DeathSequence.Step(
                    nativeSamus, nativeColors, stock.Gameplay.DeathSteps);
                SamusDeathSequenceStepResult installedStep = installedSamus.DeathSequence.Step(
                    installedSamus, installedColors, stock.Gameplay.DeathSteps);
                // Samus's death colours are her shades and the whiteout a colour the display shows (#1512): each frame
                // her shown suit and suitless colours are one of the cartridge's death palette pairs for her suit, and the
                // whiteout one of the cartridge's whiteout shades.
                RgbColor[] shownSuit = installedColors.ShownSuit(InstalledFixtures.SamusShadeSources);
                RgbColor[] shownSuitless = installedColors.ShownSuitless(InstalledFixtures.SamusShadeSources);
                ushort[] suitWords = [.. shownSuit.Select(colour => colour.ToWord())];
                ushort[] suitlessWords = [.. shownSuitless.Select(colour => colour.ToWord())];
                int[] suitMatches = [.. Enumerable.Range(0, SamusPaletteDefinitions.Death.PaletteCount).Where(palette => CartridgeDeathColours(bus,
                    SamusSourceDefinitions.SuitPointers.Value + (suit * SamusPaletteDefinitions.Death.PaletteCount + palette) * sizeof(ushort)).SequenceEqual(suitWords))];
                int[] suitlessMatches = [.. Enumerable.Range(0, SamusPaletteDefinitions.Death.PaletteCount).Where(palette => CartridgeDeathColours(bus,
                    SamusSourceDefinitions.SuitlessPointers.Value + palette * sizeof(ushort)).SequenceEqual(suitlessWords))];
                // Until the sequence writes its first pair, her colours are the ones she had, as the cartridge's CGRAM is.
                bool pairWritten = installedColors.Shading.Shades.Any(shade => shade.Effect == SamusShades.DeathSuitless(0).Effect);
                if (pairWritten)
                    pairFrames++;
                bool pair = !pairWritten || suitMatches.Intersect(suitlessMatches).Any();
                AssertTrue(pair, pair ? "death colours are a cartridge pair"
                    : $"death suit {suit} frame {frame} shows one of the cartridge's death palette pairs (suit {Hex(suitWords)}; suitless "
                      + $"{Hex(suitlessWords)}; suit matches {string.Join(',', suitMatches)}; suitless matches {string.Join(',', suitlessMatches)})");
                AssertTrue(installedSamus.DeathSequence.Whiteout is not { } whiteout ||
                    Enumerable.Range(0, SamusPaletteDefinitions.Death.WhiteoutShadeCount).Any(shade =>
                        CartridgeImportSource.Require(bus).ReadCartridgeWord(SamusSourceDefinitions.DeathWhiteoutShades.Value + shade * sizeof(ushort))
                            == whiteout.ToWord()),
                    $"death suit {suit} frame {frame} whiteout is one of the cartridge's whiteout shades");
                AssertTrue(nativeSamus.DeathSequence.Whiteout == installedSamus.DeathSequence.Whiteout &&
                    nativeStep.PhaseAfterStep == installedStep.PhaseAfterStep &&
                    nativeSamus.DeathSequence.AnimationTimer ==
                        installedSamus.DeathSequence.AnimationTimer &&
                    nativeSamus.DeathSequence.AnimationCounter ==
                        installedSamus.DeathSequence.AnimationCounter,
                    $"death suit {suit} frame {frame} native/installed colors and timing");
                if (nativeStep.Completed)
                    break;
                AssertTrue(frame < 239, $"death suit {suit} terminates within 240 frames");
            }
            AssertTrue(pairFrames > 0, $"death suit {suit} shows the cartridge's death palette pairs");
        }
        for (int pose = 0; pose < SamusBodySources.PoseCount; pose++)
        {
            if (!SamusPoseNumbering.NamesPose(pose)) continue;
            // The half pictures the import composes a pose's frame zero of are the definition records the cartridge's
            // frame-zero selector names (#1465, #1585).
            SamusBodyFramePictures pictures = stock.Pictures(SamusPoseNumbering.FromNumber(pose), 0);
            SamusBodyFrameSelection selection = stock.Selection(SamusPoseNumbering.FromNumber(pose), 0);
            AssertEqual(SamusBodySources.Picture(stock.DefinitionRecord(SamusBodyHalf.Top, selection.TopSet, selection.TopPosition)),
                pictures.Top, $"Samus pose {pose:X2} composes its frame-zero top definition");
            AssertEqual(selection.NamesBottom
                    ? SamusBodySources.Picture(stock.DefinitionRecord(SamusBodyHalf.Bottom, selection.BottomSet, selection.BottomPosition)) : null,
                pictures.Bottom, $"Samus pose {pose:X2} composes its frame-zero bottom definition, or none when the frame names none");
        }

        Console.WriteLine("Samus body art: 253 poses, 1143 frames, 435 split DMAs, 1913 nonzero OAM indices and special draw offsets match retail.");
    }

    /// <summary>The sixteen colour words of the cartridge's death palette whose pointer lies at <paramref name="pointerAt"/>.</summary>
    private static ushort[] CartridgeDeathColours(ISnesAddressSpace bus, int pointerAt)
    {
        ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(pointerAt);
        return [.. Enumerable.Range(0, InstalledSamusDeathPalettes.ColorCount).Select(colour =>
            CartridgeImportSource.Require(bus).ReadCartridgeWord(SamusSourceDefinitions.PaletteBank.Value | pointer + colour * 2))];
    }

    /// <summary>Compares arm-cannon pose records and tiles to ROM, then checks guarded cover OAM and DMA output.</summary>
    private static void VerifySamusArmCannonArtwork(ISnesAddressSpace bus,
        SamusBodySources body)
    {
        SamusArmCannonSourceArtwork armCannon = body.ArmCannon;
        for (int pose = 0; pose < SamusBodySources.PoseCount; pose++)
        {
            if (!SamusPoseNumbering.NamesPose(pose)) continue;
            int source = SamusSourceDefinitions.PoseDrawingDataPointers.Value + pose * 2;
            ushort native = CartridgeImportSource.Require(bus).ReadCartridgeWord(source);
            AssertEqual(native, armCannon.PoseDrawingData(SamusPoseNumbering.FromNumber(pose)),
                $"arm-cannon pose {pose:X2} drawing pointer");
        }
        for (int offset = 0; offset < SamusArmCannonArtworkFormat.DrawingDataByteCount; offset++)
            AssertEqual(bus.ReadCartridgeByte(SamusSourceDefinitions.MovementBank.Value |
                    (SamusSourceDefinitions.ArmCannonDrawingData + offset)),
                armCannon.ReadDrawingByteAt((ushort)(SamusSourceDefinitions.ArmCannonDrawingData + offset)),
                $"arm-cannon drawing byte {offset:X3}");
        for (int direction = 0; direction < SamusProjectileDirectionCodec.Count; direction++)
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
        int drawingWindowEnd = SamusSourceDefinitions.MovementBank.Value |
            (SamusSourceDefinitions.ArmCannonDrawingData + SamusArmCannonArtworkFormat.DrawingDataByteCount);
        for (int pose = 0; pose < SamusBodySources.PoseCount; pose++)
        {
            // The typed placement gameplay reads holds what the pose's native descriptor does (#1463): its drawing mode,
            // its direction at frame zero and, when the direction byte's top bit is set, after it, then one signed X/Y
            // displacement per animation frame up to the end of the drawing window.
            int descriptor = SamusSourceDefinitions.MovementBank.Value |
                CartridgeImportSource.Require(bus).ReadCartridgeWord(SamusSourceDefinitions.PoseDrawingDataPointers.Value + pose * 2);
            int direction = bus.ReadCartridgeByte(descriptor);
            bool frameDependent = (direction & 0x80) != 0;
            if (!SamusPoseNumbering.NamesPose(pose)) continue;
            SamusArmCannonPlacement placement = body.ArmCannon.Gameplay.Placement(SamusPoseNumbering.FromNumber(pose));
            AssertEqual(SuperMetroid.AssetExtraction.Samus.SamusArmCannonDrawOrderCodec.FromDrawingMode(bus.ReadCartridgeByte(descriptor + 1)),
                placement.DrawOrder, $"arm-cannon pose {pose:X2} drawing mode");
            AssertEqual(SuperMetroid.AssetExtraction.Samus.SamusProjectileDirectionCodec.FromAimIndex(direction & 0x7f),
                placement.FirstDirection, $"arm-cannon pose {pose:X2} first direction");
            AssertEqual(frameDependent
                    ? SuperMetroid.AssetExtraction.Samus.SamusProjectileDirectionCodec.FromAimIndex(bus.ReadCartridgeByte(descriptor + 2) & 0x7f)
                    : null, placement.LaterDirection,
                $"arm-cannon pose {pose:X2} later direction");
            int displacements = descriptor + (frameDependent ? 4 : 2);
            AssertEqual((drawingWindowEnd - displacements) / 2, placement.FrameCount, $"arm-cannon pose {pose:X2} placed frames");
            for (int frame = 0; frame < placement.FrameCount; frame++)
                AssertEqual(Offset16<RoomPixels>.Of(unchecked((sbyte)bus.ReadCartridgeByte(displacements + frame * 2)),
                        unchecked((sbyte)bus.ReadCartridgeByte(displacements + frame * 2 + 1))), placement.Offset(frame),
                    $"arm-cannon pose {pose:X2} frame {frame} displacement");
        }
        foreach (ushort pointer in SamusArmCannonArtworkFormat.TileSourcePointers)
        {
            int source = SamusSourceDefinitions.CharacterDataBank.Value | pointer;
            AssertTrue(InstalledFixtures.SamusArmCannonCover.TryResolveTile(source,
                SamusArmCannonArtworkFormat.CoverTileByteCount, out var tile),
                $"arm-cannon character ${source:X6} extracted");
            for (int offset = 0; offset < tile.Length; offset++)
                AssertEqual(bus.ReadCartridgeByte(source + offset), tile.Span[offset],
                    $"arm-cannon character ${source:X6} byte {offset:X2}");
        }

        var nativeSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            AnimationFrame = 0, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), SelectedHudItem = HudItem.Missile,
        };
        var installedSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Pose = nativeSamus.Pose, AnimationFrame = nativeSamus.AnimationFrame,
            Position = FixedPosition<RoomPixels>.FromWhole(nativeSamus.Position.Whole),
            SelectedHudItem = nativeSamus.SelectedHudItem,
        };
        // The imported bytes above are the independent stock oracle. Both runtime
        // owners must now receive the installed catalog; no ROM fallback remains.
        nativeSamus.Presentation.BindBodyFixture(body);
        installedSamus.Presentation.BindBodyFixture(body);
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
        Console.WriteLine("Arm-cannon artwork: 253 pose pointers, drawing bytes, typed pose placements, ten direction selectors and twelve PNG tiles match retail; guarded cover draw and binding parity pass.");
    }

    /// <summary>Import-only oracle for Samus's bank-$92 OAM record format.</summary>
    private static void DrawImportedSamusSpritemap(ISnesAddressSpace bus, OamBuffer oam,
        ushort index, Position16<SpritePixels> origin)
    {
        int pointerAddress = SamusSpritemapSources.PointerTableAddress + index * 2;
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

    /// <summary>Colour words as four-digit hexadecimal, for a failure message.</summary>
    private static string Hex(ushort[] words) => string.Join(' ', words.Select(word => word.ToString("X4", System.Globalization.CultureInfo.InvariantCulture)));
}
