using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Compares installed projectile-trail artwork with native OAM while checking animation and mutable-memory contracts.</summary>
    /// <param name="bus">Retail address space used to extract trail data and build the native rendering reference.</param>
    private static void VerifyProjectileTrailArtwork(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyProjectileTrailAtlas), () => VerifyProjectileTrailAtlas(bus));
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
                DrawNativeTrailFrame(bus, nativeSystem.TrailSlots[0], nativeOam, frozenFrame);
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
        int draws = 0;
        foreach (ushort frame in ProjectileTrailVisualDefinitions.Frames)
        foreach (ushort coordinate in new ushort[] { 0, 1, 255, 256, 65535 })
        foreach (int preceding in new[] { 0, 127, 128 })
        {
            ushort attributes = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomData.Banks.Movement | (frame + 2));
            var system = new SamusProjectileSystem();
            var side = system.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1].Left;
            side.InstructionPointer = (ushort)(frame + 4); side.InstructionTimer = 3;
            side.XPosition = coordinate; side.YPosition = coordinate; side.TileNumberAttributes = attributes;
            var native = new OamBuffer(); var authored = new OamBuffer();
            for (int i = 0; i < preceding; i++)
            { native.AddProjectileTrailSprite(0, 0, 0); authored.AddProjectileTrailSprite(0, 0, 0); }
            system.HandleTrailsAndDraw(new ProjectileCompositionForbiddenBus(), native, 0, 0, true);
            system.HandleTrailsAndDraw(new ProjectileCompositionForbiddenBus(), authored, 0, 0, true, catalog);
            AssertTrue(native.LowTable.SequenceEqual(authored.LowTable) && native.HighTable.SequenceEqual(authored.HighTable), "Trail catalog preserves native culling and complete OAM bytes");
            AssertEqual(native.NextByteOffset, authored.NextByteOffset, "Trail catalog retains OAM capacity behavior");
            AssertEqual(3, side.InstructionTimer, "Frozen draw cannot advance trail animation");
            AssertEqual(attributes, side.TileNumberAttributes, "Artwork selection does not mutate stored trail attributes");
            draws++;
        }
        var document = JsonNode.Parse(json)!;
        ushort first = ProjectileTrailVisualDefinitions.Frames[0];
        var part = document["frames"]![ProjectileTrailVisualDefinitions.Name(first)]!;
        part["flipX"] = !part["flipX"]!.GetValue<bool>();
        var edited = Load(document);
        Suite(nameof(VerifyRuntimeTrailBinding), () => VerifyRuntimeTrailBinding(bus, catalog, edited));
        AssertEqual((ushort)(catalog.Resolve(first) ^ 0x4000), edited.Resolve(first), "Trail flip edit changes only horizontal-flip bit");
        var animation = new SamusProjectileSystem();
        var trail = animation.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1].Left;
        trail.InstructionPointer = first; trail.InstructionTimer = 1; trail.XPosition = 100; trail.YPosition = 100;
        var frozen = new OamBuffer();
        animation.HandleTrailsAndDraw(new ProjectileCompositionForbiddenBus(), frozen, 0, 0, true, edited);
        AssertEqual(0, frozen.GetEntry(0).TileNumber, "New frozen trail retains native uninitialized tile rather than starting artwork early");
        animation.HandleTrailsAndDraw(bus, new OamBuffer(), 0, 0, false, catalog);
        var original = new OamBuffer(); var changed = new OamBuffer();
        animation.HandleTrailsAndDraw(new ProjectileCompositionForbiddenBus(), original, 0, 0, true, catalog);
        animation.HandleTrailsAndDraw(new ProjectileCompositionForbiddenBus(), changed, 0, 0, true, edited);
        AssertTrue(original.GetEntry(0).FlipX != changed.GetEntry(0).FlipX, "Edited trail flip reaches production draw");
        AssertEqual(1, trail.InstructionTimer, "Rebound art retains current timer");
        document["frames"]![ProjectileTrailVisualDefinitions.Name(first)]!["duration"] = 0;
        AssertThrows<InvalidDataException>(() => Load(document), "Trail art cannot change duration");
        document = JsonNode.Parse(json)!;
        document["frames"]!.AsObject().Remove(ProjectileTrailVisualDefinitions.Name(first));
        AssertThrows<InvalidDataException>(() => Load(document), "Missing trail art rejected");
        document = JsonNode.Parse(json)!;
        document["frames"]![ProjectileTrailVisualDefinitions.Name(first)]!["palette"] = 8;
        AssertThrows<InvalidDataException>(() => Load(document), "Invalid trail palette rejected");
        Console.WriteLine($"Trail artwork: {draws} native OAM comparisons, frozen-start preservation, live edit isolation and strict metadata rejection pass.");
        static ProjectileTrailCatalog Load(JsonNode document) => ProjectileTrailCatalog.Load(new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString())));
    }

    /// <summary>Advances the cartridge trail instruction stream and draws its current left and right trail sprites.</summary>
    /// <param name="rom">Address space supplying native instruction words.</param>
    /// <param name="pair">Paired trail state whose sides, positions, and instruction cursors are mutated.</param>
    /// <param name="oam">OAM buffer receiving visible trail sprites.</param>
    /// <param name="frozen">Whether instruction timers and commands are held for this rendered frame.</param>
    private static void DrawNativeTrailFrame(ISnesAddressSpace rom,
        SamusProjectileTrailSlot pair, OamBuffer oam, bool frozen)
    {
        ushort Word(ushort pointer) => (ushort)(rom.ReadByte(0x900000 | pointer) |
            rom.ReadByte(0x900000 | unchecked((ushort)(pointer + 1))) << 8);
        foreach (SamusProjectileTrailSide side in new[] { pair.Left, pair.Right })
        {
            if (side.InstructionTimer == 0) continue;
            if (!frozen && --side.InstructionTimer == 0)
            {
                ushort cursor = side.InstructionPointer;
                for (int operations = 0; ; operations++)
                {
                    AssertTrue(operations < 16, "native trail reaches a timed frame or terminator");
                    ushort command = Word(cursor);
                    if (command < 0x8000)
                    {
                        side.InstructionTimer = command;
                        if (command != 0)
                        {
                            side.TileNumberAttributes = Word(unchecked((ushort)(cursor + 2)));
                            side.InstructionPointer = unchecked((ushort)(cursor + 4));
                        }
                        break;
                    }
                    cursor += 2;
                    switch (command)
                    {
                        case 0xb525: pair.Left.YPosition++; break;
                        case 0xb587: pair.Right.YPosition++; break;
                        case 0xb5b3: pair.Left.YPosition--; break;
                        default: throw new InvalidDataException($"Unknown native trail command ${command:X4}.");
                    }
                }
                if (side.InstructionTimer == 0) continue;
            }
            if (side.XPosition < 256 && side.YPosition < 256)
                oam.AddProjectileTrailSprite((byte)side.XPosition, (byte)side.YPosition,
                    side.TileNumberAttributes);
        }
    }
    /// <summary>Checks runtime trail catalog binding, snapshot exclusion, host rebinding after restore, and preservation of saved timing.</summary>
    /// <param name="bus">Cartridge address space used to create the retail runtime fixture.</param>
    /// <param name="stock">Unedited catalog used as the native-equivalent appearance baseline.</param>
    /// <param name="edited">Catalog with a changed frame appearance used to verify runtime selection.</param>
    private static void VerifyRuntimeTrailBinding(ISnesAddressSpace bus, ProjectileTrailCatalog stock, ProjectileTrailCatalog edited)
    {
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.GameplayTimeFrozen = true;
        var side = runtime.Projectiles.TrailSlots[0].Left;
        side.InstructionPointer = (ushort)(ProjectileTrailVisualDefinitions.Frames[0] + 4);
        side.InstructionTimer = 3; side.TileNumberAttributes = stock.Resolve(ProjectileTrailVisualDefinitions.Frames[0]);
        side.XPosition = (ushort)(runtime.Camera!.XPosition + 100); side.YPosition = (ushort)(runtime.Camera.YPosition + 100);
        var game = CreateRetailGameFixture(bus);
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var runtimeField = game.GetType().GetField("runtime", flags)!;
        runtimeField.SetValue(game, runtime);
        byte[] Draw(SuperMetroid.Core.Runtime.SuperMetroidRuntime target)
        {
            target.Oam.BeginFrame();
            target.GetType().GetMethod("DrawGameplayActors", flags)!.Invoke(target, new object?[] { false, null, null, false });
            return target.Oam.LowTable.ToArray();
        }
        byte[] native = Draw(runtime);
        game.BindTrailArtwork(stock);
        AssertTrue(native.SequenceEqual(Draw(runtime)), "Runtime trail stock binding preserves native OAM");
        game.BindTrailArtwork(edited);
        AssertTrue(!native.SequenceEqual(Draw(runtime)), "Runtime actor pass consumes edited trail appearance");
        game.BindTrailArtwork(null);
        using var without = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(without, game);
        game.BindTrailArtwork(stock);
        using var with = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(with, game);
        AssertTrue(without.ToArray().SequenceEqual(with.ToArray()), "Trail catalogs are excluded from both frontend and runtime snapshots");
        with.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Frontend.SuperMetroidGame>(with);
        var restoredRuntime = (SuperMetroid.Core.Runtime.SuperMetroidRuntime)runtimeField.GetValue(restored)!;
        AssertTrue(restoredRuntime.TrailArtwork is null, "Restored trail catalog requires host rebind");
        BindRetailRuntimeFixture(restoredRuntime);
        restored.BindTrailArtwork(edited); game.BindTrailArtwork(edited);
        AssertTrue(Draw(runtime).SequenceEqual(Draw(restoredRuntime)), "Old state draws current selected trail appearance after rebind");
        AssertEqual(3, restoredRuntime.Projectiles.TrailSlots[0].Left.InstructionTimer, "Trail rebind preserves saved timing");
    }
    /// <summary>Checks that the bank-$90 low-half trail program alias reads through typed WRAM access rather than generic bus reads.</summary>
    private static void VerifyTrailMutableAlias()
    {
        var mirror = new TrailMutableMirrorBus();
        AssertEqual((ushort)0x3412, ProjectileTrailProgramDefinitions.Read(mirror, 0x900100),
            "bank-$90 low-half trail alias reads WRAM through the typed mutable interface");
        AssertEqual(2, mirror.WorkRamReads, "low-half trail word reads exactly two mutable bytes");
        Console.WriteLine("Projectile trail low-bank alias: two typed WRAM reads, no generic bus read.");
    }

    /// <summary>Test address space that exposes only two expected WRAM bytes and fails on untyped, SRAM, or write access.</summary>
    private sealed class TrailMutableMirrorBus : ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Number of typed WRAM byte requests made by the alias read.</summary>
        public int WorkRamReads { get; private set; }

        /// <summary>Routes cartridge reads to the fail-fast generic read path used to detect incorrect access.</summary>
        /// <param name="address">Cartridge address requested by the caller.</param>
        /// <returns>The result of the generic read path.</returns>
        /// <exception cref="InvalidOperationException">Any cartridge-byte read is untyped for this fixture.</exception>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects generic reads so the test can require the mutable-memory interface for the bank alias.</summary>
        /// <param name="address">Address requested by the caller.</param>
        /// <returns>No value; every request throws.</returns>
        /// <exception cref="InvalidOperationException">A generic address-space read was attempted.</exception>
        public static byte ReadByte(int address) =>
            throw new InvalidOperationException($"Untyped trail read at ${address:X6}.");

        /// <summary>Supplies the two low-half alias bytes expected by the compiled trail-word read.</summary>
        /// <param name="address">WRAM address requested by the reader.</param>
        /// <returns><c>$12</c> at <c>$900100</c> or <c>$34</c> at <c>$900101</c>.</returns>
        /// <exception cref="InvalidOperationException">The request is outside those two fixture addresses.</exception>
        public byte ReadWorkRamByte(int address)
        {
            WorkRamReads++;
            return address switch
            {
                0x900100 => 0x12,
                0x900101 => 0x34,
                _ => throw new InvalidOperationException($"Unexpected trail WRAM read ${address:X6}."),
            };
        }

        /// <summary>Rejects SRAM access because this alias fixture provides no save-memory bytes.</summary>
        /// <param name="address">Save-memory address requested by the caller.</param>
        /// <returns>No value; every request throws.</returns>
        /// <exception cref="InvalidOperationException">Any save-memory read was attempted.</exception>
        public byte ReadSaveRamByte(int address) =>
            throw new InvalidOperationException($"Unexpected trail SRAM read ${address:X6}.");

        /// <summary>Rejects all writes because the fixture is read-only.</summary>
        /// <param name="address">Address requested for writing.</param>
        /// <param name="value">Byte value the caller attempted to store.</param>
        /// <exception cref="InvalidOperationException">Any write was attempted.</exception>
        public void WriteByte(int address, byte value) =>
            throw new InvalidOperationException($"Unexpected trail write ${address:X6}.");
    }
}
