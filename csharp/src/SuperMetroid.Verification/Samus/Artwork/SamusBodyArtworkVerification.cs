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
using SuperMetroid.Core.Samus.Colours;
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
        // The import decodes the cartridge's body tables once into what each pose's picture frames show (#2556); each decoded
        // frame's half pictures, placement and compositions are the ones the cartridge's tables select for that frame.
        Dictionary<SamusPoseId, SamusBodyPosePlacement> expectedPlacements = SamusBodyPlacementDecoding.Decode(
            Enumerable.Range(0, SamusBodyTableDecoding.PoseCount).Select(pose => (int)unchecked((sbyte)bus.ReadCartridgeByte(
                SamusSourceDefinitions.PoseDefinitions.Value + pose * SamusSourceDefinitions.PoseDefinitionByteCount + 4))).ToArray(),
            Enumerable.Range(0, SamusBodyPlacementSourceDefinitions.LandingByteCount).Select(i =>
                (int)bus.ReadCartridgeByte(SamusSourceDefinitions.LandingVerticalOffsets.Value + i)).ToArray(),
            Enumerable.Range(0, SamusBodyPlacementSourceDefinitions.PostureByteCount).Select(i =>
                (int)unchecked((sbyte)bus.ReadCartridgeByte(SamusSourceDefinitions.PostureTransitionVerticalOffsets.Value + i))).ToArray(),
            Enumerable.Range(0, SamusBodyPlacementSourceDefinitions.DrainedByteCount).Select(i =>
                (int)unchecked((sbyte)bus.ReadCartridgeByte(SamusSourceDefinitions.DrainedVerticalOffsets.Value + i))).ToArray());
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        SamusSpritemapSources sprites = stock.Spritemaps;
        SamusShownFrames shownFrames = InstalledFixtures.SamusShownFrameAnalysis.Installed();
        int decodedFrames = 0, drawnHalves = 0;
        // Each half picture and composition is compared with the cartridge once, at the first frame that shows it; every
        // later frame showing it must select the same definition record or spritemap.
        var pictureRecords = new Dictionary<SamusBodyPicture, int>();
        var compositionPointers = new Dictionary<ushort, SamusSpritemapDefinition?>();
        for (int pose = 0; pose < SamusBodyTableDecoding.PoseCount; pose++)
        {
            if (!SamusPoseNumbering.NamesPose(pose)) continue;
            SamusPoseId named = SamusPoseNumbering.FromNumber(pose);
            int graphicsAddress = SamusSourceDefinitions.PoseDefinitions.Value + pose * SamusSourceDefinitions.PoseDefinitionByteCount + 4;
            AssertEqual(unchecked((sbyte)bus.ReadCartridgeByte(graphicsAddress)), stock.Gameplay.ArtOrigin(named), $"Samus pose {pose:X2} visual Y origin");
            int frameList = (cartridge.ReadCartridgeWord(SamusSourceDefinitions.AnimationDefinitionListPointers.Value + pose * 2) - SamusSourceDefinitions.FirstBodyFrame) / 4;
            SamusPoseAnimation animation = SamusAnimationDelays.AnimationForPose(named);
            IEnumerable<int> pictured = shownFrames.PictureFrames.ContainsKey(named)
                ? shownFrames.Pictures(named).Select(frame => SamusAnimationLayout.Offset(animation, frame)) : [];
            AssertTrue(stock.Frames[named].Keys.All(pictured.Contains), $"Samus pose {pose:X2} decodes only the frames its animation pictures");
            foreach ((int frame, SamusBodyDecodedFrame decoded) in stock.Frames[named])
            {
                int selector = SamusSourceDefinitions.BodyFrames.Value + (frameList + frame) * 4;
                void Picture(int groups, int set, int position, SamusBodyPicture picture, string what)
                {
                    int record = cartridge.ReadCartridgeWord(groups + set * 2) + position * SamusSourceDefinitions.BodyDefinitionBytes;
                    if (pictureRecords.TryAdd(picture, record))
                        AssertTransfer(bus, record, stockCharacters.Transfer(picture), what);
                    else
                        AssertEqual(pictureRecords[picture], record, $"{what} is the definition its picture names");
                }
                Picture(SamusSourceDefinitions.TopDefinitionListPointers.Value, bus.ReadCartridgeByte(selector), bus.ReadCartridgeByte(selector + 1),
                    decoded.Pictures.Top, $"Samus pose {pose:X2} frame {frame} top picture");
                int bottomSet = bus.ReadCartridgeByte(selector + 2);
                AssertEqual(bottomSet != byte.MaxValue, decoded.Pictures.Bottom is not null, $"Samus pose {pose:X2} frame {frame} names a bottom picture as its selector does");
                if (decoded.Pictures.Bottom is { } bottomPicture)
                    Picture(SamusSourceDefinitions.BottomDefinitionListPointers.Value, bottomSet, bus.ReadCartridgeByte(selector + 3),
                        bottomPicture, $"Samus pose {pose:X2} frame {frame} bottom picture");
                AssertTrue(expectedPlacements[named].TryFrameRise(frame, out int rise) && rise == decoded.OriginRise,
                    $"Samus pose {pose:X2} frame {frame} carries its native placement");
                foreach (SamusBodyHalf half in new[] { SamusBodyHalf.Top, SamusBodyHalf.Bottom })
                {
                    if (half == SamusBodyHalf.Bottom && !SamusBodyCompositions.ShowsBottomHalf(named, frame))
                        continue;
                    var halfFrame = new SamusBodyHalfFrame(named, half, frame);
                    int index = SamusSpritemapNative.Index(halfFrame);
                    ushort pointer = cartridge.ReadCartridgeWord(SamusSourceDefinitions.SpritemapPointers.Value + index * 2);
                    SamusSpritemapDefinition? definition = sprites.Definition(halfFrame);
                    if (!compositionPointers.TryAdd(pointer, definition))
                    {
                        AssertTrue(ReferenceEquals(compositionPointers[pointer], definition), $"Samus {halfFrame} shows the composition its spritemap names");
                        continue;
                    }
                    AssertComposition(bus, pointer, definition, $"Samus {halfFrame}");
                    var nativeOam = new OamBuffer();
                    nativeOam.BeginFrame();
                    if (pointer != 0)
                        DrawImportedSamusSpritemap(bus, nativeOam, (ushort)index, new(127, 131));
                    OamBuffer installedOam = SamusSpritemapDrawsAccess.SamusSpritemapOam(
                        sprites.TryGetFrame(halfFrame, out SpriteFrame? shownHalf) ? shownHalf : null,
                        new Position16<ScreenPixels>(127, 131), SpriteFrameTestArtwork.Spritemaps(sprites), new SpriteSource(SpriteSourceKind.Samus));
                    AssertTrue(nativeOam.ImageEquals(installedOam) && nativeOam.NextByteOffset == installedOam.LastFinalizedSpriteCount * 4,
                        $"Samus {halfFrame} installed/native staging parity");
                    drawnHalves++;
                }
                decodedFrames++;
            }
        }
        foreach (HorizontalDirection facing in SamusDeathSheetComposer.Facings)
        foreach (SamusDeathExplosionFrame frame in SamusDeathSheetDefinitions.Explosion)
        {
            string name = SamusDeathSheetDefinitions.ExplosionFrame(facing, frame);
            AssertComposition(bus, cartridge.ReadCartridgeWord(SamusSourceDefinitions.SpritemapPointers.Value +
                SamusDeathSheetComposerConstants.ExplosionSpritemap(facing, frame) * 2), sprites.ExplosionDefinition(name), name);
        }
        void AtmosphereEntries(IReadOnlyList<SamusAtmosphereFrame> effect, int first)
        {
            for (int frame = 0; frame < effect.Count; frame++)
            {
                string name = SamusAtmosphereSheetDefinitions.FrameName(effect[frame]);
                AssertComposition(bus, cartridge.ReadCartridgeWord(SamusSourceDefinitions.SpritemapPointers.Value + (first + frame) * 2),
                    sprites.AtmosphereDefinition(name), name);
            }
        }
        // $92:808D entries $18F and $186: the diving splash's and bubbles' first compositions; their later frames follow.
        AtmosphereEntries(SamusAtmosphereSheetDefinitions.DivingSplash, 399);
        AtmosphereEntries(SamusAtmosphereSheetDefinitions.Bubbles, 390);

        // The installed selector and both VRAM halves must take the production path
        // without even reading one cartridge byte. This covers every authored pose's
        // initial frame, including all flashback/normal-gameplay pose identities.
        // These two production owners call the same indexed bank-$92 OAM routine as
        // Samus's body, but do not call Samus.Draw. Exercise both rather than inferring
        // their binding from the generic catalog's all-index byte-parity test.
        static SamusParticles OneParticle(int type, int step)
        {
            var particles = new SamusParticles();
            particles.Add(SamusParticle.AtStep(SamusAtmosphericEffectWordCodec.KindNumbered(type)
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
            AssertTrue(SamusAtmosphericTypeCodec.TryDecode(type, out SamusAtmosphericPictures pictures), $"atmospheric type {type} draws single-piece pictures");
            AssertTrue(stock.Atmosphere.TryResolve(pictures, frame, out SpritePart installedAttributes),
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
        AssertTrue(!SamusAtmosphericTypeCodec.TryDecode(2, out _),
            "atmospheric type two retains its retail pointer-zero address-space path");
        var deathSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
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
        for (int palette = 0; palette < SamusShadeDefinitions.Death.ExplosionShadeCount; palette++)
        {
            int pointerAt = SamusSourceDefinitions.SuitPointers.Value +
                (suit * SamusShadeDefinitions.Death.ExplosionShadeCount + palette) * sizeof(ushort);
            ushort suitPointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(pointerAt);
            int suitlessAt = SamusSourceDefinitions.SuitlessPointers.Value + palette * sizeof(ushort);
            ushort suitlessPointer = CartridgeImportSource.Require(bus).ReadCartridgeWord(suitlessAt);
            for (int color = 0; color < InstalledSamusDeathPalettes.ColorCount; color++)
            {
                int suitedAt = SamusSourceDefinitions.PaletteBank.Value | suitPointer + color * 2;
                int suitlessColorAt = SamusSourceDefinitions.PaletteBank.Value | suitlessPointer + color * 2;
                AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(suitedAt),
                    stockDeathPalettes.SuitedColor(SuperMetroid.AssetExtraction.Samus.SamusSuitListLayout.Order[suit], palette, color),
                    $"death suit {suit} palette {palette} color {color}");
                AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(suitlessColorAt),
                    stockDeathPalettes.SuitlessColor(palette, color),
                    $"death suitless palette {palette} color {color}");
            }
        }
        for (int shade = 0; shade < SamusShadeDefinitions.Death.WhiteoutShadeCount; shade++)
        {
            int at = SamusSourceDefinitions.DeathWhiteoutShades.Value + shade * sizeof(ushort);
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(at),
                stockDeathPalettes.WhiteoutColor(shade), $"death whiteout shade {shade}");
        }
        for (int frame = 0; frame < SuperMetroid.Core.Samus.Death.SamusDeathExplosionTimingDefinitionsConstants.RecordCount; frame++)
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
            var nativeSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
            {
                Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(equipment),
            };
            var installedSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
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
                int[] suitMatches = [.. Enumerable.Range(0, SamusShadeDefinitions.Death.ExplosionShadeCount).Where(palette => CartridgeDeathColours(bus,
                    SamusSourceDefinitions.SuitPointers.Value + (suit * SamusShadeDefinitions.Death.ExplosionShadeCount + palette) * sizeof(ushort)).SequenceEqual(suitWords))];
                int[] suitlessMatches = [.. Enumerable.Range(0, SamusShadeDefinitions.Death.ExplosionShadeCount).Where(palette => CartridgeDeathColours(bus,
                    SamusSourceDefinitions.SuitlessPointers.Value + palette * sizeof(ushort)).SequenceEqual(suitlessWords))];
                // Until the sequence writes its first pair, her colours are the ones she had, as the cartridge's CGRAM is.
                bool pairWritten = installedColors.Shading.Shades.Any(shade => shade.Effect == SamusShades.DeathSuitless(SuperMetroid.Core.Samus.Death.SamusDeathShade.First).Effect);
                if (pairWritten)
                    pairFrames++;
                bool pair = !pairWritten || suitMatches.Intersect(suitlessMatches).Any();
                AssertTrue(pair, pair ? "death colours are a cartridge pair"
                    : $"death suit {suit} frame {frame} shows one of the cartridge's death palette pairs (suit {Hex(suitWords)}; suitless "
                      + $"{Hex(suitlessWords)}; suit matches {string.Join(',', suitMatches)}; suitless matches {string.Join(',', suitlessMatches)})");
                AssertTrue(installedSamus.DeathSequence.Whiteout is not { } whiteout ||
                    Enumerable.Range(0, SamusShadeDefinitions.Death.WhiteoutShadeCount).Any(shade =>
                        CartridgeImportSource.Require(bus).ReadCartridgeWord(SamusSourceDefinitions.DeathWhiteoutShades.Value + shade * sizeof(ushort))
                            == whiteout.ToWord()),
                    $"death suit {suit} frame {frame} whiteout is one of the cartridge's whiteout shades");
                AssertTrue(nativeSamus.DeathSequence.Whiteout == installedSamus.DeathSequence.Whiteout &&
                    nativeSamus.DeathSequence.Phase == installedSamus.DeathSequence.Phase &&
                    nativeSamus.DeathSequence.Flash.UpdatesLeft == installedSamus.DeathSequence.Flash.UpdatesLeft &&
                    nativeSamus.DeathSequence.Flash.Shown == installedSamus.DeathSequence.Flash.Shown &&
                    nativeSamus.DeathSequence.Flashing.Age == installedSamus.DeathSequence.Flashing.Age &&
                    nativeSamus.DeathSequence.Explosion?.UpdatesLeft == installedSamus.DeathSequence.Explosion?.UpdatesLeft &&
                    nativeSamus.DeathSequence.ExplosionFrame == installedSamus.DeathSequence.ExplosionFrame &&
                    nativeColors.Beams == installedColors.Beams,
                    $"death suit {suit} frame {frame} native/installed colors and timing");
                if (nativeStep.Completed)
                    break;
                AssertTrue(frame < 239, $"death suit {suit} terminates within 240 frames");
            }
            AssertTrue(pairFrames > 0, $"death suit {suit} shows the cartridge's death palette pairs");
        }
        Console.WriteLine($"Samus body art: {decodedFrames} decoded pose frames and {drawnHalves} distinct compositions, the explosion and atmosphere compositions and special draw offsets match retail.");
    }

    /// <summary>
    /// Requires <paramref name="transfer"/> to be the body definition at bank-$92 <paramref name="record"/>: its two transfer
    /// sizes and characters.
    /// </summary>
    private static void AssertTransfer(ISnesAddressSpace bus, int record, SamusBodyTileTransfer transfer, string what)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        int definitionAddress = 0x920000 | record;
        int source = cartridge.ReadCartridgeLong(definitionAddress);
        AssertEqual(cartridge.ReadCartridgeWord(definitionAddress + 3), (ushort)transfer.FirstSize, $"{what} first size");
        AssertEqual(cartridge.ReadCartridgeWord(definitionAddress + 5), (ushort)transfer.SecondSize, $"{what} second size");
        ReadOnlySpan<byte> planar = transfer.Planar.Span;
        for (int i = 0; i < planar.Length; i++)
            AssertEqual(bus.ReadCartridgeByte(source + i), planar[i], $"{what} tile byte {i:X3}");
    }

    /// <summary>
    /// Requires <paramref name="record"/> to be the composition the cartridge's spritemap at bank-$92
    /// <paramref name="pointer"/> describes, or none for a zero pointer, which draws nothing (owner decision on #1318).
    /// </summary>
    private static void AssertComposition(ISnesAddressSpace bus, ushort pointer, SamusSpritemapDefinition? record, string what)
    {
        if (pointer == 0)
        {
            AssertTrue(record is null, $"{what} shows no composition for a zero pointer");
            return;
        }
        if (record is null)
            throw new InvalidOperationException($"{what} shows no composition, but the cartridge's pointer is ${pointer:X4}.");
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        int address = 0x920000 | pointer;
        AssertEqual(cartridge.ReadCartridgeWord(address), (ushort)record.Parts.Length, $"{what} part count");
        for (int part = 0; part < record.Parts.Length; part++)
        {
            int at = address + 2 + part * 5;
            // The decoded piece holds what the cartridge's five-byte record describes: the import codec decodes the record,
            // and its offset, size, character, palette row, priority and mirroring are compared.
            SpritePart expectedPart = new SpritemapPart(new SnesSpritemapXWord(cartridge.ReadCartridgeWord(at)),
                new SnesSpritemapYByte(bus.ReadCartridgeByte(at + 2)), new SnesObjAttributeWord(cartridge.ReadCartridgeWord(at + 3))).ToPart();
            AssertEqual(expectedPart, record.Parts[part], $"{what} part {part} offset, size, character, palette, priority and flips");
        }
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
        for (int pose = 0; pose < SamusBodyTableDecoding.PoseCount; pose++)
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
                armCannon.CoverAppearance(SuperMetroid.AssetExtraction.Samus.SamusProjectileDirectionCodec.FromAimIndex(direction)), $"arm-cannon direction {direction} cover character, palette row, priority and flips");
            int listAddress = SamusSourceDefinitions.TileListPointers.Value + direction * 2;
            int list = CartridgeImportSource.Require(bus).ReadCartridgeWord(listAddress);
            for (int frame = 0; frame < SamusArmCannonArtworkFormatConstants.FramesPerDirection; frame++)
            {
                int word = SamusSourceDefinitions.MovementBank.Value | (list + frame * 2);
                AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(word),
                    armCannon.TileSource(direction, frame),
                    $"arm-cannon direction {direction} frame {frame} tile source");
            }
        }
        int drawingWindowEnd = SamusSourceDefinitions.MovementBank.Value |
            (SamusSourceDefinitions.ArmCannonDrawingData + SamusArmCannonArtworkFormat.DrawingDataByteCount);
        for (int pose = 0; pose < SamusBodyTableDecoding.PoseCount; pose++)
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
                        unchecked((sbyte)bus.ReadCartridgeByte(displacements + frame * 2 + 1))), placement.OffsetNumbered(SamusPoseNumbering.FromNumber(pose), frame),
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

        var nativeSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            AnimationNumber = 0, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), SelectedHudItem = HudItem.Missile,
        };
        var installedSamus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Pose = nativeSamus.Pose, AnimationFrame = nativeSamus.AnimationFrame,
            Position = FixedPosition<RoomPixels>.FromWhole(nativeSamus.Position.Whole),
            SelectedHudItem = nativeSamus.SelectedHudItem,
        };
        // The imported bytes above are the independent stock oracle. Both runtime
        // owners must now receive the installed catalog; no ROM fallback remains.
        nativeSamus.BindBodyFixture(body);
        installedSamus.BindBodyFixture(body);
        nativeSamus.BindArmCannonFixture(armCannon);
        installedSamus.BindArmCannonFixture(armCannon);
        for (int frame = 0; frame < 4; frame++)
        {
            nativeSamus.ArmCannon.Update(nativeSamus);
            installedSamus.ArmCannon.Update(installedSamus);
            AssertEqual(
                (nativeSamus.ArmCannon.Frame, nativeSamus.ArmCannon.DrawOrder),
                (installedSamus.ArmCannon.Frame, installedSamus.ArmCannon.DrawOrder),
                $"installed arm-cannon update frame {frame}");
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
        AssertEqual(SheetFrameSources.CharacterDraw(nativeFrame.Draws[0]).Artwork.Id,
            SheetFrameSources.CharacterDraw(installedFrame.Draws[0]).Artwork.Id,
            "installed arm-cannon cover shows the frame composed over the native cover character");
        Console.WriteLine("Arm-cannon artwork: 253 pose pointers, drawing bytes, typed pose placements, ten direction selectors and twelve PNG tiles match retail; guarded cover draw and binding parity pass.");
    }

    /// <summary>Import-only oracle for Samus's bank-$92 OAM record format.</summary>
    private static void DrawImportedSamusSpritemap(ISnesAddressSpace bus, OamBuffer oam,
        ushort index, Position16<ScreenPixels> origin)
    {
        int pointerAddress = SamusSourceDefinitions.SpritemapPointers.Value + index * 2;
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

    /// <summary>Runs the Samus body artwork checks against the repository installation.</summary>
    private static void VerifyInstalledSamusBodyArtwork() =>
        VerifySamusBodyArtwork(SuperMetroid.Verification.Harness.RepositoryInputsVerification.LoadRepositoryRom());

    /// <summary>Runs the Samus body artwork checks.</summary>
    [SuperMetroid.Verification.Harness.VerificationCommand("--samus-body-artwork")]
    internal static int SamusBodyArtworkCommand()
    {
        Suite(nameof(VerifySamusBodyArtwork), VerifyInstalledSamusBodyArtwork);
        return 0;
    }
}
