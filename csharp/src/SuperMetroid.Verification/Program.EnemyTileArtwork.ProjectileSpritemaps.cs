using System.Text.Json;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledEnemyProjectileSpritemaps(
        ISnesAddressSpace bus, string directory, EnemyTileArtworkCatalog stock)
    {
        EnemyProjectileSpritemapCatalog installed = stock.ProjectileSpritemaps
            ?? throw new InvalidDataException("Installed enemy-projectile compositions are missing.");
        foreach ((ushort pointer, _) in EnemyProjectileSpritemapDefinitions.Frames)
        foreach ((ushort originX, ushort originY, ushort graphicsIndex) in new[]
                 {
                     ((ushort)0x0080, (ushort)0x0004, (ushort)0x0000),
                     ((ushort)0x01ff, (ushort)0x00fc, (ushort)0x0a04),
                 })
        {
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            nativeOam.BeginFrame();
            installedOam.BeginFrame();
            bool onScreen = (originY & 0xff00) == 0;
            nativeOam.AddEnemyProjectileSpritemap(bus, pointer,
                originX, originY, graphicsIndex, onScreen);
            installedOam.AddEnemySpritemap(installed.Get(pointer).Span,
                originX, originY,
                unchecked((ushort)(graphicsIndex & 0xff00)),
                unchecked((byte)graphicsIndex),
                clipVerticalWrap: true, originYIsOnScreen: onScreen);
            AssertEqual(nativeOam.NextByteOffset, installedOam.NextByteOffset,
                $"installed $8D:{pointer:X4} OAM part count");
            AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                       nativeOam.HighTable.SequenceEqual(installedOam.HighTable),
                $"installed $8D:{pointer:X4} OAM pixels, wrapping, tile base and palette");
        }

        foreach ((ushort operand, ushort pointer) in new[]
                 {
                     (SkreeMetareeParticleVisualDefinitions.SkreeOperand,
                         SkreeMetareeParticleVisualDefinitions.SkreeComposition),
                     (SkreeMetareeParticleVisualDefinitions.MetareeOperand,
                         SkreeMetareeParticleVisualDefinitions.MetareeComposition),
                 })
        {
            ushort nativePointer = unchecked((ushort)(
                bus.ReadByte(0x860000 | operand) |
                bus.ReadByte(0x860000 | unchecked((ushort)(operand + 1))) << 8));
            AssertEqual(nativePointer, SkreeMetareeParticleVisualDefinitions.Resolve(operand),
                $"particle visual operand $86:{operand:X4} matches the pinned cartridge");
            AssertEqual(nativePointer, pointer,
                $"particle visual operand $86:{operand:X4} selects its editable composition");
        }
        VerifyBurstVisuals(stock);
        VerifySharedProgramVisuals(stock);

        var nativeSamus = new SamusState { XPosition = 0x0080, YPosition = 0 };
        var installedSamus = new SamusState { XPosition = 0x0080, YPosition = 0 };
        var nativeArrival = new CeresElevatorArrivalState(bus, nativeSamus);
        var installedArrival = new CeresElevatorArrivalState(
            new EnemyProjectileVisualReadGuard(bus), installedSamus, installed);
        for (int frame = 0; frame < 3; frame++)
        {
            nativeArrival.Step(nativeSamus);
            installedArrival.Step(installedSamus);
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            nativeOam.BeginFrame();
            installedOam.BeginFrame();
            nativeArrival.Draw(nativeOam, 0, 0);
            installedArrival.Draw(installedOam, 0, 0);
            AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                       nativeOam.HighTable.SequenceEqual(installedOam.HighTable),
                $"Ceres elevator arrival frame {frame} draws from installed $8D compositions");
        }

        string overrides = Path.Combine(directory, "eproj-overrides");
        Directory.CreateDirectory(overrides);
        EnemyProjectileSpritemapDocument document =
            JsonSerializer.Deserialize<EnemyProjectileSpritemapDocument>(
                File.ReadAllBytes(Path.Combine(directory,
                    EnemyProjectileSpritemapDefinitions.FileName)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        SpriteVisualPart[] pad = document.Frames["ceres_elevator_pad_0"];
        pad[0] = pad[0] with { OffsetX = pad[0].OffsetX + 1 };
        SpriteVisualPart[] skree = document.Frames["skree_debris"];
        skree[0] = skree[0] with { OffsetX = skree[0].OffsetX + 1 };
        EnemyProjectilePresentationFrameDefinition editableProgramFrame =
            EnemyProjectileInstructionMechanicsDefinitions.VisualFrames[0];
        SpriteVisualPart[] shared = document.ProgramFrames![editableProgramFrame.Name];
        shared[0] = shared[0] with { OffsetX = shared[0].OffsetX + 1 };
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName),
            EnemyProjectileSpritemapCatalog.Write(document));
        EnemyTileArtworkCatalog editedArt = EnemyTileArtworkFiles.Load(directory, overrides);
        EnemyProjectileSpritemapCatalog edited = editedArt.ProjectileSpritemaps!;
        var originalOam = new OamBuffer();
        var editedOam = new OamBuffer();
        originalOam.BeginFrame();
        editedOam.BeginFrame();
        originalOam.AddEnemySpritemap(installed.Get(0xb1ba).Span,
            0x0080, 0x0004, 0, 0, clipVerticalWrap: true);
        editedOam.AddEnemySpritemap(edited.Get(0xb1ba).Span,
            0x0080, 0x0004, 0, 0, clipVerticalWrap: true);
        AssertEqual((byte)(originalOam.LowTable[0] + 1), editedOam.LowTable[0],
            "enemy-projectile composition override moves the live Ceres pad OAM part");
        OamBuffer stockBurst = DrawBurst(stock,
            SkreeMetareeParticleInstructionProgramDefinitions.Skree, bus);
        OamBuffer editedBurst = DrawBurst(editedArt,
            SkreeMetareeParticleInstructionProgramDefinitions.Skree, bus);
        AssertTrue(!stockBurst.LowTable.SequenceEqual(editedBurst.LowTable),
            "edited Skree debris composition changes production burst OAM");
        AssertTrue(!DrawSharedFrame(stock, editableProgramFrame.OperandAddress, bus)
                .LowTable.SequenceEqual(DrawSharedFrame(editedArt,
                    editableProgramFrame.OperandAddress,
                    new EnemyProjectileVisualReadGuard(bus)).LowTable),
            "edited shared projectile frame changes production OAM without a ROM visual read");
        var incompleteCurrent = document.Frames
            .Where(entry => entry.Key != "skree_debris")
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
        byte[] invalidCurrentJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = EnemyProjectileSpritemapDefinitions.Version,
                Frames = incompleteCurrent,
                ProgramFrames = document.ProgramFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        AssertThrows<InvalidDataException>(
            () => EnemyProjectileSpritemapCatalog.Load(new MemoryStream(invalidCurrentJson),
                installed),
            "current projectile compositions cannot silently omit Skree debris");
        var incompleteProgramFrames = document.ProgramFrames!
            .Where(entry => entry.Key != editableProgramFrame.Name)
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
        byte[] invalidProgramJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = EnemyProjectileSpritemapDefinitions.Version,
                Frames = document.Frames,
                ProgramFrames = incompleteProgramFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        AssertThrows<InvalidDataException>(
            () => EnemyProjectileSpritemapCatalog.Load(new MemoryStream(invalidProgramJson),
                installed),
            "current projectile compositions cannot silently omit a shared program frame");

        var legacyFrames = document.Frames
            .Where(entry => entry.Key.StartsWith("ceres_elevator_", StringComparison.Ordinal))
            .ToDictionary(entry => entry.Key, entry => entry.Value, StringComparer.Ordinal);
        byte[] legacyJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = 1,
                Frames = legacyFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        AssertThrows<InvalidDataException>(
            () => EnemyProjectileSpritemapCatalog.Load(new MemoryStream(legacyJson)),
            "a partial version-one composition cannot stand alone as stock content");
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), legacyJson);
        EnemyTileArtworkCatalog migrated = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawBurst(migrated,
                SkreeMetareeParticleInstructionProgramDefinitions.Skree, bus)
            .LowTable.SequenceEqual(stockBurst.LowTable),
            "version-one Ceres override inherits stock Skree debris composition");
        var migratedPad = new OamBuffer();
        migratedPad.BeginFrame();
        migratedPad.AddEnemySpritemap(migrated.ProjectileSpritemaps!.Get(0xb1ba).Span,
            0x0080, 0x0004, 0, 0, clipVerticalWrap: true);
        AssertEqual(editedOam.LowTable[0], migratedPad.LowTable[0],
            "version-one Ceres composition edit survives the version-three catalog upgrade");
        byte[] versionTwoJson = JsonSerializer.SerializeToUtf8Bytes(
            new EnemyProjectileSpritemapDocument
            {
                Version = 2,
                Frames = document.Frames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        File.WriteAllBytes(Path.Combine(overrides,
            EnemyProjectileSpritemapDefinitions.FileName), versionTwoJson);
        EnemyTileArtworkCatalog migratedV2 = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(DrawBurst(migratedV2,
                SkreeMetareeParticleInstructionProgramDefinitions.Skree, bus)
            .LowTable.SequenceEqual(editedBurst.LowTable),
            "version-two edits retain the Skree debris frame");
        AssertTrue(DrawSharedFrame(stock, editableProgramFrame.OperandAddress, bus)
                .LowTable.SequenceEqual(DrawSharedFrame(migratedV2,
                    editableProgramFrame.OperandAddress,
                    new EnemyProjectileVisualReadGuard(bus)).LowTable),
            "version-two edits inherit stock shared-program frames");

        void VerifySharedProgramVisuals(EnemyTileArtworkCatalog artwork)
        {
            ReadOnlySpan<EnemyProjectilePresentationFrameDefinition> frames =
                EnemyProjectileInstructionMechanicsDefinitions.VisualFrames;
            AssertTrue(frames.Length > 150,
                "compiled shared Mother Brain/dust programs expose their complete visual frame set");
            foreach (EnemyProjectilePresentationFrameDefinition frame in frames)
            {
                ushort nativePointer = unchecked((ushort)(
                    bus.ReadByte(0x860000 | frame.OperandAddress) |
                    bus.ReadByte(0x860000 | unchecked((ushort)(frame.OperandAddress + 1))) << 8));
                foreach ((ushort originX, ushort originY) in new[]
                         {
                             ((ushort)128, (ushort)96),
                             ((ushort)0x01ff, (ushort)0x00fc),
                         })
                {
                    var nativeOam = new OamBuffer();
                    var installedOam = new OamBuffer();
                    nativeOam.BeginFrame();
                    installedOam.BeginFrame();
                    nativeOam.AddEnemyProjectileSpritemap(bus, nativePointer,
                        originX, originY, 0x0a04, originYIsOnScreen: true);
                    installedOam.AddEnemySpritemap(
                        installed.GetProgramFrame(frame.OperandAddress).Span,
                        originX, originY, 0x0a00, 4,
                        clipVerticalWrap: true, originYIsOnScreen: true);
                    AssertTrue(nativeOam.NextByteOffset == installedOam.NextByteOffset &&
                        nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                        nativeOam.HighTable.SequenceEqual(installedOam.HighTable),
                        $"installed shared frame $86:{frame.OperandAddress:X4} matches ROM OAM at {originX:X4},{originY:X4}");
                }
                AssertTrue(DrawSharedFrame(null, frame.OperandAddress, bus)
                        .LowTable.SequenceEqual(DrawSharedFrame(artwork,
                            frame.OperandAddress,
                            new EnemyProjectileVisualReadGuard(bus)).LowTable),
                    $"shared frame $86:{frame.OperandAddress:X4} runs without visual ROM reads");
            }
        }

        void VerifyBurstVisuals(EnemyTileArtworkCatalog artwork)
        {
            foreach (ushort program in new[]
                     {
                         SkreeMetareeParticleInstructionProgramDefinitions.Skree,
                         SkreeMetareeParticleInstructionProgramDefinitions.Metaree,
                     })
            {
                OamBuffer native = DrawBurst(null, program, bus);
                OamBuffer installedBurst = DrawBurst(artwork, program,
                    new EnemyProjectileVisualReadGuard(bus));
                AssertTrue(native.NextByteOffset > 0 &&
                    native.NextByteOffset == installedBurst.NextByteOffset &&
                    native.LowTable.SequenceEqual(installedBurst.LowTable) &&
                    native.HighTable.SequenceEqual(installedBurst.HighTable),
                    $"installed Skree/Metaree burst ${program:X4} draws stock OAM without visual ROM reads");
            }
        }

        static OamBuffer DrawBurst(EnemyTileArtworkCatalog? artwork, ushort program,
            ISnesAddressSpace source)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, source);
            string producer = program == SkreeMetareeParticleInstructionProgramDefinitions.Skree
                ? "SpawnSkreeParticleBurst" : "SpawnMetareeParticleBurst";
            var spawn = typeof(RoomEnemySystem).GetMethod(producer, flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(enemies);
            var process = typeof(RoomEnemySystem).GetMethod(
                "ProcessEnemyProjectileInstructions", flags)!;
            var enemy = new RoomEnemySlot(0)
            {
                XPosition = 128, YPosition = 96,
                VramTilesIndex = 0x0200, PaletteIndex = 0x0c00,
            };
            spawn(enemy);
            foreach (RoomEnemyProjectileSlot particle in enemies.EnemyProjectiles
                         .Where(particle => particle.IsActive))
            {
                particle.InstructionTimer = 1;
                process.Invoke(enemies, [particle, new SamusState(), (ushort)0, (ushort)0]);
            }
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawEnemyProjectiles(oam, cameraX: 0, cameraY: 0);
            return oam;
        }

        static OamBuffer DrawSharedFrame(EnemyTileArtworkCatalog? artwork,
            ushort operand, ISnesAddressSpace source)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, source);
            RoomEnemyProjectileSlot slot = enemies.EnemyProjectiles[0];
            slot.Kind = RoomEnemyProjectileKind.MiscDustExplosion;
            slot.XPosition = 128;
            slot.YPosition = 96;
            slot.GraphicsIndex = 0x0a04;
            slot.InstructionPointer = unchecked((ushort)(operand - 2));
            slot.InstructionTimer = 1;
            typeof(RoomEnemySystem).GetMethod(
                "ProcessEnemyProjectileInstructions", flags)!
                .Invoke(enemies, [slot, new SamusState(), (ushort)0, (ushort)0]);
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawEnemyProjectiles(oam, 0, 0);
            return oam;
        }
    }

    private sealed class EnemyProjectileVisualReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            ushort low = unchecked((ushort)address);
            if ((address & 0xff0000) == 0x8d0000 ||
                (address & 0xff0000) == 0x860000 &&
                (EnemyProjectileInstructionMechanicsDefinitions.IsVisualOperand(low) ||
                 EnemyProjectileInstructionMechanicsDefinitions.IsVisualOperand(
                     unchecked((ushort)(low - 1))) ||
                 low is SkreeMetareeParticleVisualDefinitions.SkreeOperand or
                    SkreeMetareeParticleVisualDefinitions.MetareeOperand ||
                 low == SkreeMetareeParticleVisualDefinitions.SkreeOperand + 1 ||
                 low == SkreeMetareeParticleVisualDefinitions.MetareeOperand + 1))
                throw new InvalidOperationException(
                    $"Installed enemy projectile reread visual byte ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
