using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyTourianStatueUnlockDefinitions(SuperMetroid.AssetExtraction.CartridgeImportAddressSpace rom)
    {
        for (ushort parameter = 0; parameter <= 6; parameter += 2)
        {
            Position16<RoomPixels> position =
                TourianStatueUnlockDefinitions.EyePosition(parameter);
            AssertEqual(rom.ReadCartridgeWord((0x86b90e + parameter)), position.X,
                $"Tourian statue eye X parameter {parameter}");
            AssertEqual(rom.ReadCartridgeWord((0x86b916 + parameter)), position.Y,
                $"Tourian statue eye Y parameter {parameter}");
        }

        FieldInfo busField = typeof(RoomEnemySystem).GetField(
            "_bus", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo cgramField = typeof(RoomEnemySystem).GetField(
            "_cgram", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var guarded = NewTourianStatueEyeReadGuard(rom);
        for (ushort parameter = 0; parameter <= 6; parameter += 2)
        for (int soulValue = 0; soulValue < 2; soulValue++)
        {
            bool soul = soulValue != 0;
            var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
            busField.SetValue(enemies, guarded);
            cgramField.SetValue(enemies, new SnesCgram());
            enemies.SpawnTourianUnlockEffect(parameter, soul);

            Position16<RoomPixels> expected =
                TourianStatueUnlockDefinitions.EyePosition(parameter);
            RoomEnemyProjectileSlot projectile =
                enemies.EnemyProjectiles.Single(candidate => candidate.IsActive);
            AssertEqual(expected.X, projectile.Position.X.Whole,
                $"production Tourian unlock X {parameter}/{soul}");
            AssertEqual(expected.Y, projectile.Position.Y.Whole,
                $"production Tourian unlock Y {parameter}/{soul}");
            AssertEqual(soul ? TourianStatueRomData.Soul : TourianStatueRomData.EyeGlow,
                (ushort)projectile.Kind,
                $"production Tourian unlock kind {parameter}/{soul}");
            AssertEqual(soul ? 0xfc00 : 0,
                projectile.Velocity.Y.RawValue,
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
                        $"statue $87:{(int)definition.ObjectPointer:X4} leaves source " +
                        $"$87:{pointer:X4} presentation-owned");
                    int source = TourianStatueAnimatedTileArtworkDefinitions.SourceAddress(
                        definition, pointer);
                    byte[] native = ReadNativeTourianStatueFrame(rom, source, definition.TransferByteCount);
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

        bool forbidArtworkReads = false;
        var observedSourceOperands = new HashSet<ushort>();
        var guarded = new MutableCartridgeReadGuard(rom);
        guarded
            .Forbid(address => forbidArtworkReads &&
                    address >= TourianStatueAnimatedTileArtworkDefinitions.FirstSource &&
                    address < TourianStatueAnimatedTileArtworkDefinitions.SourceEnd,
                address => $"Installed Tourian statue artwork read cartridge byte ${address:X6}.")
            .Forbid(IsTourianStatueMechanicsByte,
                address => $"Production read compiled Tourian statue mechanics byte {SnesAddress.FromBusAddress(address)}.")
            .Observe(address => SnesAddress.FromBusAddress(address).Bank == (byte)(RoomFxRomData.Banks.AnimatedTiles >> 16),
                address =>
                {
                    ushort offset = SnesAddress.FromBusAddress(address).Offset;
                    foreach (TourianStatueAnimatedTileProgramDefinition definition in
                             TourianStatueAnimatedTileMechanicsDefinitions.All)
                    foreach (ushort sourcePointer in definition.SourceOperandPointers)
                    {
                        if (offset == sourcePointer || offset == unchecked((ushort)(sourcePointer + 1)))
                            observedSourceOperands.Add(sourcePointer);
                    }
                });
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
        forbidArtworkReads = true;

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
        AssertEqual(0, observedSourceOperands.Count,
            "installed Tourian sequence reads no frame-source operands");
        Console.WriteLine(
            $"Tourian statue animated tiles: 184 mechanics words and 36 art selections " +
            $"compiled, with ROM-free art transfer; release completes in {steps} calls.");
    }

    private static bool IsTourianStatueMechanicsByte(int address)
    {
        SnesAddress snesAddress = SnesAddress.FromBusAddress(address);
        return snesAddress.Bank == (byte)(RoomFxRomData.Banks.AnimatedTiles >> 16) &&
            TourianStatueAnimatedTileMechanicsDefinitions.All.Any(definition =>
                definition.TryReadMechanicsWord(snesAddress.Offset, out _) ||
                definition.TryReadMechanicsWord(unchecked((ushort)(snesAddress.Offset - 1)), out _));
    }

    private static bool TourianStatueGreyEventsAreSet(SuperMetroidRuntime runtime) =>
        runtime.System.HasEventRaw(0x0006) &&
        runtime.System.HasEventRaw(0x0007) &&
        runtime.System.HasEventRaw(0x0008) &&
        runtime.System.HasEventRaw(0x0009);

    private static CartridgeReadGuard NewTourianStatueEyeReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0x86b90e and < 0x86b91e,
            address => $"Tourian statue unlock attempted migrated eye-position read ${address:X6}.");

    [CpuBusBoundary("exact bus test: retail ROM oracle for Tourian statue animated-tile frame pixels")]
    private static byte[] ReadNativeTourianStatueFrame(IImportCartridgeSource rom, int source, int byteCount) =>
        RomDataReader.ReadFixedBank(rom, source, byteCount);
}
