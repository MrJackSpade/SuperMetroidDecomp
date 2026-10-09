using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>Checks every native statue-eye position and production eye/soul spawn against the compiled unlock data.</summary>
    /// <param name="rom">Import-only cartridge source used to compare the original bank-$86 coordinate words.</param>
    private static void VerifyTourianStatueUnlockDefinitions(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace rom)
    {
        for (ushort parameter = 0; parameter <= 6; parameter += 2)
        {
            TourianStatueEyePosition position =
                TourianStatueUnlockDefinitions.EyePosition(parameter);
            AssertEqual(ReadTourianEyeWord(rom, 0x86b90e + parameter), position.X,
                $"Tourian statue eye X parameter {parameter}");
            AssertEqual(ReadTourianEyeWord(rom, 0x86b916 + parameter), position.Y,
                $"Tourian statue eye Y parameter {parameter}");
        }

        FieldInfo busField = typeof(RoomEnemySystem).GetField(
            "_bus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo cgramField = typeof(RoomEnemySystem).GetField(
            "_cgram", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var guarded = new TourianStatueEyeReadGuard(rom);
        for (ushort parameter = 0; parameter <= 6; parameter += 2)
        for (int soulValue = 0; soulValue < 2; soulValue++)
        {
            bool soul = soulValue != 0;
            var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
            busField.SetValue(enemies, guarded);
            cgramField.SetValue(enemies, new SnesCgram());
            enemies.SpawnTourianUnlockEffect(parameter, soul);

            TourianStatueEyePosition expected =
                TourianStatueUnlockDefinitions.EyePosition(parameter);
            RoomEnemyProjectileSlot projectile =
                enemies.EnemyProjectiles.Single(candidate => candidate.IsActive);
            AssertEqual(expected.X, projectile.XPosition,
                $"production Tourian unlock X {parameter}/{soul}");
            AssertEqual(expected.Y, projectile.YPosition,
                $"production Tourian unlock Y {parameter}/{soul}");
            AssertEqual(soul ? TourianStatueRomData.Soul : TourianStatueRomData.EyeGlow,
                (ushort)projectile.Kind,
                $"production Tourian unlock kind {parameter}/{soul}");
            AssertEqual(soul ? 0xfc00 : 0,
                projectile.YVelocity,
                $"production Tourian unlock velocity {parameter}/{soul}");
        }

        foreach (ushort parameter in new ushort[] { 1, 3, 5, 7, ushort.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(
                () => TourianStatueUnlockDefinitions.EyePosition(parameter),
                $"invalid Tourian statue parameter {parameter}");
        }

        Suite(nameof(VerifyTourianStatueAnimatedTileMechanics), () => VerifyTourianStatueAnimatedTileMechanics(rom));

        Console.WriteLine(
            "Tourian statue eye positions: eight native words and all eight real eye/soul spawns pass with position tables forbidden.");
    }

    /// <summary>Compares compiled animated-tile mechanics and art sources with the cartridge, then confirms the retail release sequence needs no mechanics ROM reads.</summary>
    /// <param name="rom">Cartridge source for expected animated-tile records and presentation bytes.</param>
    private static void VerifyTourianStatueAnimatedTileMechanics(
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyTourianStatueArtworkSources), () => VerifyTourianStatueArtworkSources(rom));
        Suite(nameof(VerifyTourianStatueDescriptorFields), () => VerifyTourianStatueDescriptorFields(rom));
        Suite(nameof(VerifyTourianStatueProgramMappings), () => VerifyTourianStatueProgramMappings(rom));
        Suite(nameof(VerifyTourianStatueSpawnOrder), () => VerifyTourianStatueSpawnOrder(rom));
        int mechanicsWordCount = 0;
        int presentationWordCount = 0;
        RoomFxAnimatedTileAtlas artwork = RoomFxAnimatedTileAtlas.Load(
            new MemoryStream(RoomFxAnimatedTileAtlasExtractor.Extract(rom)));
        foreach (TourianStatueAnimatedTileProgramDefinition definition in
                 TourianStatueAnimatedTileMechanicsDefinitions.All)
        {
            mechanicsWordCount += 3; // Header fields are covered by the named descriptor proofs.

            for (int programOffset = 0; programOffset <= 0x66; programOffset += 2)
            {
                ushort pointer = unchecked((ushort)(definition.ProgramStart + programOffset));
                if (definition.SourceOperandPointers.Contains(pointer))
                {
                    AssertTrue(!definition.TryReadMechanicsWord(pointer, out _),
                        $"statue $87:{definition.ObjectPointer:X4} leaves source " +
                        $"$87:{pointer:X4} presentation-owned");
                    int source = TourianStatueAnimatedTileArtworkDefinitions.SourceAddress(
                        definition, pointer);
                    byte[] native = RomDataReader.ReadFixedBank(rom, source,
                        definition.TransferByteCount);
                    AssertTrue(artwork.TryResolve(source, native.Length,
                            out ReadOnlyMemory<byte> installed) &&
                        installed.Span.SequenceEqual(native),
                        $"statue frame $87:{pointer:X4} preserves every cartridge pixel byte");
                    presentationWordCount++;
                    continue;
                }

                mechanicsWordCount++;
            }
        }

        AssertEqual(4, TourianStatueAnimatedTileMechanicsDefinitions.All.Count(),
            "Tourian statue animated-tile object count");
        AssertEqual(184, mechanicsWordCount,
            "Tourian statue compiled mechanics word count");
        AssertEqual(36, presentationWordCount,
            "Tourian statue live presentation-source word count");

        var guarded = new TourianStatueMechanicsForbiddenBus(rom);
        var runtime = CreateRetailRuntimeFixture(guarded, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        for (int area = 1; area <= 4; area++)
            runtime.System.SetBossBits(area, BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(0xa66a);
        AssertTrue(runtime.TourianStatues.Enabled,
            "retail Tourian statue room enables the production sequence");
        runtime.VramWrites.DrainTo(runtime.Vram, ReferenceMutableMemory.From(guarded), runtime);
        guarded.ForbidArtworkReads = true;

        int steps = 0;
        for (; steps < 3000 && !TourianStatueGreyEventsAreSet(runtime); steps++)
        {
            runtime.TourianStatues.StepTiles(runtime);
            runtime.VramWrites.DrainTo(runtime.Vram, ReferenceMutableMemory.From(guarded), runtime);
        }

        AssertTrue(TourianStatueGreyEventsAreSet(runtime),
            "production Tourian sequence releases all four defeated-boss statues");
        AssertTrue(steps < 3000, "production Tourian sequence remains bounded");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production Tourian sequence performs no mechanics ROM reads");
        AssertEqual(0, guarded.ObservedSourceOperands.Count,
            "installed Tourian sequence reads no frame-source operands");
        Console.WriteLine(
            $"Tourian statue animated tiles: 184 mechanics words and 36 art selections " +
            $"compiled, with ROM-free art transfer; release completes in {steps} calls.");
    }

    /// <summary>Tests whether all four defeated-boss event bits released by the statue sequence are set.</summary>
    /// <param name="runtime">Runtime whose event word is checked.</param>
    /// <returns><see langword="true"/> when events $0006 through $0009 are all present.</returns>
    private static bool TourianStatueGreyEventsAreSet(SuperMetroidRuntime runtime) =>
        runtime.System.HasEventRaw(0x0006) &&
        runtime.System.HasEventRaw(0x0007) &&
        runtime.System.HasEventRaw(0x0008) &&
        runtime.System.HasEventRaw(0x0009);

    /// <summary>Reads one little-endian coordinate word from the original Tourian eye-position table.</summary>
    /// <param name="bus">Import address space supplying cartridge bytes.</param>
    /// <param name="address">Bus address of the word's low byte.</param>
    /// <returns>The low byte followed by the high byte as an unsigned word.</returns>
    private static ushort ReadTourianEyeWord(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Address-space guard that rejects runtime reads of the migrated statue eye-position table.</summary>
    /// <param name="source">Underlying cartridge source for all addresses outside the forbidden eye-word range.</param>
    private sealed class TourianStatueEyeReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes importer-style reads through the guarded bus so forbidden eye-position access is detected.</summary>
        /// <param name="address">Bus address requested by the caller.</param>
        /// <returns>The guarded byte value.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads of the migrated eye table and delegates every other address to the source.</summary>
        /// <param name="address">Bus address requested by the runtime.</param>
        /// <returns>The source byte when the address is outside the guarded range.</returns>
        /// <exception cref="InvalidOperationException">The runtime attempts to read a compiled eye-position word.</exception>
        public byte ReadByte(int address) =>
            address is >= 0x86b90e and < 0x86b91e
                ? throw new InvalidOperationException(
                    $"Tourian statue unlock attempted migrated eye-position read ${address:X6}.")
                : source.ReadByte(address);

        /// <summary>Forwards writes unchanged because this guard only observes reads.</summary>
        /// <param name="address">Destination bus address.</param>
        /// <param name="value">Byte to write to the underlying source.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    /// <summary>Tracks forbidden mechanics reads and source-operand access while supplying normal cartridge and WRAM storage.</summary>
    /// <param name="source">Underlying mutable address space used for permitted reads and writes.</param>
    private sealed class TourianStatueMechanicsForbiddenBus(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Number of attempted runtime reads of words that should be supplied by compiled mechanics definitions.</summary>
        public int ForbiddenReadAttempts { get; private set; }
        /// <summary>Enables rejection of reads from the statue artwork source range after installed-art setup is complete.</summary>
        public bool ForbidArtworkReads { get; set; }
        /// <summary>Mechanics source operands observed during execution, used to detect runtime table lookups.</summary>
        public HashSet<ushort> ObservedSourceOperands { get; } = [];

        /// <summary>Routes importer-style reads through mechanics and artwork guards.</summary>
        /// <param name="address">Bus address requested by the caller.</param>
        /// <returns>The guarded byte value.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Forwards a WRAM read to the underlying mutable memory.</summary>
        /// <param name="address">WRAM bus address to read.</param>
        /// <returns>The stored byte.</returns>
        public byte ReadWorkRamByte(int address) =>
            ((ISnesMutableMemory)source).ReadWorkRamByte(address);

        /// <summary>Forwards a save-RAM read to the underlying mutable memory.</summary>
        /// <param name="address">Save-RAM bus address to read.</param>
        /// <returns>The stored byte.</returns>
        public byte ReadSaveRamByte(int address) =>
            ((ISnesMutableMemory)source).ReadSaveRamByte(address);

        /// <summary>Rejects artwork reads when enabled, counts forbidden mechanics reads, records source-operand accesses, and delegates other reads.</summary>
        /// <param name="address">Bus address requested by the runtime.</param>
        /// <returns>The underlying source byte when no guard rejects the access.</returns>
        /// <exception cref="InvalidOperationException">An enabled artwork guard or forbidden mechanics access is encountered.</exception>
        public byte ReadByte(int address)
        {
            if (ForbidArtworkReads &&
                address >= TourianStatueAnimatedTileArtworkDefinitions.FirstSource &&
                address < TourianStatueAnimatedTileArtworkDefinitions.SourceEnd)
                throw new InvalidOperationException(
                    $"Installed Tourian statue artwork read cartridge byte ${address:X6}.");
            SnesAddress snesAddress = SnesAddress.FromBusAddress(address);
            if (snesAddress.Bank == (byte)(RoomFxRomData.Banks.AnimatedTiles >> 16))
            {
                foreach (TourianStatueAnimatedTileProgramDefinition definition in
                         TourianStatueAnimatedTileMechanicsDefinitions.All)
                {
                    if (definition.TryReadMechanicsWord(snesAddress.Offset, out _) ||
                        definition.TryReadMechanicsWord(
                            unchecked((ushort)(snesAddress.Offset - 1)), out _))
                    {
                        ForbiddenReadAttempts++;
                        throw new InvalidOperationException(
                            $"Production read compiled Tourian statue mechanics byte {snesAddress}.");
                    }

                    foreach (ushort sourcePointer in definition.SourceOperandPointers)
                    {
                        if (snesAddress.Offset == sourcePointer ||
                            snesAddress.Offset == unchecked((ushort)(sourcePointer + 1)))
                        {
                            ObservedSourceOperands.Add(sourcePointer);
                        }
                    }
                }
            }

            return source.ReadByte(address);
        }

        /// <summary>Forwards writes to the underlying mutable address space without filtering them.</summary>
        /// <param name="address">Destination bus address.</param>
        /// <param name="value">Byte to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
