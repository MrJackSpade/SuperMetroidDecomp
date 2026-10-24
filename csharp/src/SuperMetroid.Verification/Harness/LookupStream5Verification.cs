using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.World.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Callbacks;
using SuperMetroid.Core.Areas;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Verification.Enemies.Magdollite;
using SuperMetroid.Verification.Enemies.MorphBallEye;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Input;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Areas.Ceres.CeresDoorQuakeDefinitionsVerification;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireMeltingDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.Artwork.CrocomireSkeletonArtworkVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkPhantoonBg2FramesVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Map;

using SuperMetroid.Verification.Bosses.Phantoon;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Areas.Ceres;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Harness;

/// <summary>Cartridge-oracle checks for Ceres, boss, projectile and map visual definitions in lookup stream five.</summary>
internal static class LookupStream5Verification
{
    /// <summary>Compares compiled melting-room tilemap changes with their native transfer words.</summary>
    internal static void VerifyLookupStream5MeltingTilemaps()
    {
        var oracle = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Melt tilemap source revision");
        string directory = Path.Combine(Path.GetFullPath("csharp/test-temp"), "lookup-melt-maps-" + Guid.NewGuid().ToString("N"));
        try
        {
            SuperMetroid.AssetExtraction.Enemies.EnemyTileArtworkFiles.Extract(oracle, directory, SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256);
            InstalledEnemyArtwork catalog = SuperMetroid.AssetExtraction.Enemies.EnemyTileArtworkFiles.Load(directory, null);
            CrocomireMeltingArtwork art = (catalog.Bosses.CrocomireMelting ?? throw new InvalidOperationException("Verification requires catalog.CrocomireMelting."));
            var native = new ushort[2][];
            for (int phase = 0; phase < 2; phase++)
            {
                CrocomireMeltingTilemapAddress source = phase == 0 ? CrocomireMeltingTilemapAddress.FirstTilemap : CrocomireMeltingTilemapAddress.SecondTilemap;
                native[phase] = Enumerable.Range(0, 256).Select(i => ReadVerificationWord(oracle, (int)source + i * 2)).ToArray();
                var calculated = new CrocomireMeltingTilemap((phase != 0) ? CrocomireMeltingIllustration.Second : CrocomireMeltingIllustration.First, native[phase]);
                AssertEqual(0, calculated.edits.Count,
                    "Every native melt tilemap word calculates without fallback");
                AssertTrue(calculated.Words().SequenceEqual(native[phase]), "All native melt words");
                AssertTrue((art ?? throw new InvalidOperationException("Verification requires art.")).Tilemap(source).SequenceEqual(native[phase]), "Installed melt words");
                VerifyInstalledCrocomireMeltingTilemap(oracle, catalog, source, phase == 0
                    ? CrocomireBodyProgram.MeltingOneTopRow : CrocomireBodyProgram.MeltingTwoTopRow);
                for (int index = 0; index < 256; index++)
                {
                    ushort[] edited = (ushort[])native[phase].Clone(); edited[index] ^= 0xffff;
                    var independent = new CrocomireMeltingTilemap((phase != 0) ? CrocomireMeltingIllustration.Second : CrocomireMeltingIllustration.First, edited);
                    edited[index] = native[phase][index];
                    ushort[] expected = (ushort[])native[phase].Clone(); expected[index] ^= 0xffff;
                    AssertTrue(independent.Words().SequenceEqual(expected), "Full-word edit owns input and preserves neighbors");
                    AssertTrue(calculated.Words().SequenceEqual(native[phase]), "Edited map does not mutate stock");
                }
                AssertThrows<InvalidDataException>(() => _ = new CrocomireMeltingTilemap((phase != 0) ? CrocomireMeltingIllustration.Second : CrocomireMeltingIllustration.First, new ushort[255]), "Short melt map rejects");
            }
            var first = (art ?? throw new InvalidOperationException("Verification requires art.")).first;
            var second = art.second;
            string expectedIdentity = SelectedPresentationHash.Create("enemy-crocomire-melt-v1", content =>
            {
                content.Append("first", first.Characters.Span); content.Append("second", second.Characters.Span);
                content.AppendWords("first-map", native[0]); content.AppendWords("second-map", native[1]);
            });
            AssertEqual(expectedIdentity, art.ContentIdentity, "Original canonical melt identity");
            AssertThrows<InvalidDataException>(() => { _ = art.Tilemap(0); }, "Unknown map identity rejects");
            Console.WriteLine("Crocomire melt layouts: 512 native words, 512 isolated full-word edits, input ownership, both actual guarded BG2 uploads and canonical identity pass.");
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
    }
    /// <summary>Checks Crocomire skeleton VRAM transfer staging and destinations.</summary>
    internal static void VerifyLookupStream5SkeletonTransfers()
    {
        var oracle = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)),
            "Skeleton transfer oracle revision");
        string directory = Path.Combine(Path.GetFullPath("csharp/test-temp"),
            "lookup-skeleton-transfers-" + Guid.NewGuid().ToString("N"));
        try
        {
            SuperMetroid.AssetExtraction.Enemies.EnemyTileArtworkFiles.Extract(oracle, directory,
                SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256);
            VerifyCrocomireSkeletonArtwork(oracle, directory,
                SuperMetroid.AssetExtraction.Enemies.EnemyTileArtworkFiles.Load(directory, null));
            AssertThrows<InvalidDataException>(() => CrocomireSkeletonTransferDefinitions.TryGet(-1, out _),
                "Skeleton negative transfer index rejects");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = CrocomireSkeletonTransferDefinitions.Frames[6],
                "Skeleton view excludes sentinel");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>Compares Ceres rumble and falling debris emissions with the native room routine.</summary>
    internal static void VerifyLookupStream5CeresRumble(SuperMetroidAddressSpace rom)
    {
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes("Super Metroid.smc"))), "Rumble native revision");
        for (int index = 0; index < 4; index++)
            AssertEqual(new Offset16<RoomPixels>(unchecked((short)ReadVerificationWord(rom, 0xa6f840 + 4 * index)),
                unchecked((short)ReadVerificationWord(rom, 0xa6f842 + 4 * index))),
                CeresDoorRumbleGeometryDefinitions.Offset(index), "All eight native anchor words");
        AssertThrows<ArgumentOutOfRangeException>(() => CeresDoorRumbleGeometryDefinitions.Offset(-1), "Rumble lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => CeresDoorRumbleGeometryDefinitions.Offset(4), "Rumble upper bound");
        foreach (Position16<RoomPixels> origin in new Position16<RoomPixels>[] { new(232, 631), new(1, 65530) })
        {
            var system = new RoomEnemySystem(new EnemySession(), services: new VerificationRoomEnemyServices(null));
            var slot = system.SlotTable.Slots[0];
            slot.Position = slot.Position.WithWhole(origin);
            var doorState = new CeresDoorEnemyState { RumbleCallsLeft = 48 };
            slot.SetSpeciesState(doorState, doorState.ClearWithRecord);
            system.RidleyEncounter.CeresStatus = CeresEscapeStatus.SelfDestructStarted;
            int randomCalls = 0;
            system.VerificationServices().NextRandom = () => (ushort)(++randomCalls % 2 == 0 ? 0x4000 : 0);
            Action<RoomEnemySlot> tick = system.CeresDoorBehavior.RunCeresDoorRumbleAndExplosions;
            for (int call = 1; call <= 49; call++)
            {
                tick(slot);
                int emitted = Math.Min(10, (call + 4) / 5);
                AssertEqual(emitted, randomCalls, "One RNG call per selected anchor, none between emissions");
                var particles = system.Projectiles.EnemyProjectiles.Where(p => p.IsActive).ToArray();
                AssertEqual(emitted, particles.Length, "Exact independent effect allocation count");
                for (int ordinal = 0; ordinal < emitted; ordinal++)
                {
                    int index = 3 - ordinal % 4;
                    ushort x = unchecked((ushort)(origin.X + (short)ReadVerificationWord(rom, 0xa6f840 + index * 4)));
                    ushort y = unchecked((ushort)(origin.Y + (short)ReadVerificationWord(rom, 0xa6f842 + index * 4)));
                    MiscDustProjectileProgram program = MiscDustProjectileDefinitions.Program(ordinal % 2 == 0 ? MiscDustAnimation.Smoke : MiscDustAnimation.SmallExplosion);
                    // Native pool allocation descends; identify each emission through its unique current ordinal below.
                    var particle = particles[emitted - ordinal - 1];
                    AssertEqual(x, particle.Position.X.Whole, "Actual produced native X including 16-bit wrap");
                    AssertEqual(y, particle.Position.Y.Whole, "Actual produced native Y including 16-bit wrap");
                    AssertTrue(particle.Animation is MiscDustProjectileAnimation started && started.Cursor.Program == program, "RNG changes animation only");
                }
                AssertEqual(call == 49, slot.Properties.HasAny(EnemyProperties.Invisible), "Hide only at destruction expiry");
            }
            AssertEqual(CeresEscapeStatus.ElevatorDoorDestroyed, system.RidleyEncounter.CeresStatus, "Actual rotation handoff after last anchor");
            AssertEqual(SoundEffectLibrary2Sounds.CeresDoorExplosion, (system.CeresDoorBehavior.LastCeresDoorSoundEffect ?? throw new InvalidOperationException("Verification requires system.CeresDoorBehavior.LastCeresDoorSoundEffect.")), "Emission sound preserved");
        }
        Console.WriteLine("Ceres rumble: eight native words, bounds, 98 actual calls, 20 emitted native positions/order/animations, wrap and expiry pass.");
    }
    /// <summary>Checks Phantoon's rain-phase wait against the cartridge countdown.</summary>
    internal static void VerifyLookupStream5PhantoonRainWait(SuperMetroidAddressSpace rom)
    {
        int totalTicks = 0;
        const int nextPattern = 3;
        foreach (int bucket in new[] { 2, 0, 1 })
        {
            var system = new RoomEnemySystem(new EnemySession(), services: new VerificationRoomEnemyServices(null));
            var body = system.SlotTable.Slots[0];
            var eye = system.SlotTable.Slots[1];
            var state = new PhantoonEnemyState(body) { Eye = eye, Tentacles = system.SlotTable.Slots[2], Mouth = system.SlotTable.Slots[3] };
            system.PhantoonBehavior.Combat.Phantoon = state;
            int randomCalls = 0;
            system.VerificationServices().NextRandom = () => (ushort)(++randomCalls == 1 ? bucket : nextPattern);
            // The fade writes Phantoon's palette rows toward its installed fade colors.
            system.Presentation.BindCgram(new GameplayColours(new SpriteColourSets()));
            system.TileArtwork = InstalledFixtures.EnemyTiles;
            system.BossArtwork = InstalledFixtures.BossArtwork;
            system.AreaArtwork = InstalledFixtures.AreaArtwork;
            Action<RoomEnemySlot, PhantoonEnemyState> fade = system.PhantoonBehavior.Patterns.RunPhantoonFlameRainFadeOut;
            Action<RoomEnemySlot, PhantoonEnemyState> tick = system.PhantoonBehavior.Patterns.RunPhantoonHiddenFlameRain;
            body.Position = body.Position.WithWhole(new(111, 99)); state.Parts[body.SlotIndex].PathCursor = 42; state.Parts[body.SlotIndex].FunctionTimer = 17;
            state.Parts[body.SlotIndex].Function = PhantoonAiFunction.FadeOutDuringFlameRain;
            fade(body, state); // The eye's independent fade has not completed yet.
            AssertEqual(0, randomCalls, "Incomplete fade does not choose hidden delay");
            AssertEqual(17, state.Parts[body.SlotIndex].FunctionTimer, "Incomplete fade preserves prior countdown");
            state.Parts[eye.SlotIndex].FadeComplete = true;
            fade(body, state);
            ushort duration = ReadVerificationWord(rom, 0xa7cd63 + bucket * 2);
            AssertEqual(duration, state.Parts[body.SlotIndex].FunctionTimer, "Completed fade selects exact native hidden duration");
            AssertEqual(PhantoonAiFunction.SpawnFlameRain, state.Parts[body.SlotIndex].Function, "Completed fade starts hidden waiting phase");
            for (int elapsed = 1; elapsed <= duration; elapsed++)
            {
                tick(body, state); totalTicks++;
                AssertEqual((ushort)(duration - elapsed), state.Parts[body.SlotIndex].FunctionTimer, "Hidden wait decrements exactly once");
                if (elapsed < duration)
                {
                    AssertEqual(1, randomCalls, "Next location is not chosen before delay expires");
                    AssertEqual((ushort)111, body.Position.X.Whole, "Hidden wait does not interpolate X");
                    AssertEqual((ushort)99, body.Position.Y.Whole, "Hidden wait does not interpolate Y");
                    AssertEqual(42, state.Parts[body.SlotIndex].PathCursor, "Hidden wait does not advance path cursor");
                    AssertEqual(0, system.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive), "No rain launches before expiry");
                }
            }
            AssertEqual(2, randomCalls, "Expiration consumes one separate placement RNG word");
            AssertEqual(ReadVerificationWord(rom, 0xa7cdad + nextPattern * 8), state.Parts[body.SlotIndex].PathCursor, "Exact delayed native path cursor");
            AssertEqual(ReadVerificationWord(rom, 0xa7cdaf + nextPattern * 8), body.Position.X.Whole, "Exact delayed native X");
            AssertEqual(ReadVerificationWord(rom, 0xa7cdb1 + nextPattern * 8), body.Position.Y.Whole, "Exact delayed native Y");
            AssertEqual(PhantoonAiFunction.BecomeSolidAfterFlameRain, state.Parts[body.SlotIndex].Function, "Native rain appearance handoff");
            AssertEqual(false, state.Parts[eye.SlotIndex].FigureEightReversed, "Rain handoff clears fade counter");
            var flames = system.Projectiles.EnemyProjectiles.Where(projectile => projectile.IsActive).ToArray();
            AssertEqual(8, flames.Length, "Actual hidden expiry launches eight rain projectiles");
            foreach (var flame in flames)
            {
                AssertEqual(RoomEnemyProjectileKind.PhantoonDestroyableFlame, flame.Kind, "Actual rain projectile family");
                AssertEqual((ushort)40, flame.Position.Y.Whole, "Actual rain starts at ceiling");
            }
        }
        AssertEqual(210, totalTicks, "Three exact native hidden wait lengths");
        Console.WriteLine("Phantoon rain wait:30/60/120 holds,210actual countdown calls,fade-gated selection/exact expiry RNG-placement and24real rain spawns pass.");
    }
    /// <summary>Checks the closed-eye countdown and its actual expiry producer while casual-flame hold remains valid.</summary>
    internal static void VerifyLookupStream5PhantoonClosedEye(SuperMetroidAddressSpace rom)
    {
        int totalTicks = 0;
        foreach (int bucket in new[] { 1, 2, 0 })
        foreach (bool reversed in new[] { false, true })
        {
            var system = new RoomEnemySystem(new EnemySession(), services: new VerificationRoomEnemyServices(null));
            var body = system.SlotTable.Slots[0];
            var eye = system.SlotTable.Slots[1];
            var mouth = system.SlotTable.Slots[3];
            var state = new PhantoonEnemyState(body) { Eye = eye, Tentacles = system.SlotTable.Slots[2], Mouth = mouth };
            system.PhantoonBehavior.Combat.Phantoon = state;
            int randomCalls = 0;
            system.VerificationServices().NextRandom = () => { randomCalls++; return (ushort)(reversed ? 1 : 0); };
            Action<RoomEnemySlot, PhantoonEnemyState> begin = system.PhantoonBehavior.Patterns.RunPhantoonPickFirstRoundPattern;
            Action<RoomEnemySlot, PhantoonEnemyState> tick = system.PhantoonBehavior.Patterns.RunPhantoonFirstRoundFigureEight;
            // The duration bucket is the enemy frame's NMI counter bits 1-2, not a random draw.
            system.FrameContext.SeedNmiFrameCounter((ushort)(bucket << 1));
            body.Position = body.Position.WithWhole(new(128, 96)); state.Parts[body.SlotIndex].FunctionTimer = 1; state.Parts[body.SlotIndex].FlameRainRequested = true;
            begin(body, state);
            ushort duration = ReadVerificationWord(rom, 0xa7cd53 + bucket * 2);
            AssertEqual(duration, state.Parts[eye.SlotIndex].ClosedTimer, "Native first-round closed-eye duration");
            AssertEqual(reversed, state.Parts[eye.SlotIndex].FigureEightReversed, "Direction selection remains independent of duration bucket");
            // A valid unexpired casual-flame hold keeps this fixture specific to
            // the identified closed-eye countdown and its actual expiry producer.
            state.Parts[mouth.SlotIndex].CasualFlameHold = (ushort)(duration + 1);
            for (int elapsed = 1; elapsed <= duration; elapsed++)
            {
                tick(body, state); totalTicks++;
                AssertEqual((ushort)(duration - elapsed), state.Parts[eye.SlotIndex].ClosedTimer, "Closed-eye timer decrements once despite movement speed/cursor");
                if (elapsed < duration)
                {
                    AssertEqual(PhantoonAiFunction.MoveInFigureEightThenOpenEye, state.Parts[body.SlotIndex].Function, "No early eye opening");
                    AssertEqual(0, system.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive), "No expiry spiral before selected duration");
                }
            }
            AssertEqual(PhantoonAiFunction.NoOperation, state.Parts[body.SlotIndex].Function, "Selected duration stops moving phase");
            AssertEqual(PhantoonPartProgram.EyeOpen, state.PartProgram(eye).Program, "Expiration starts exact eye-opening program");
            AssertEqual((ushort)1, eye.InstructionTimer.RawValue, "Eye-opening program executes on next instruction step");
            AssertEqual(false, state.Parts[body.SlotIndex].FlameRainRequested, "Opening clears flame-rain trigger");
            AssertEqual(1, state.Parts[mouth.SlotIndex].CasualFlameHold, "Casual hold still advances on every movement call");
            AssertEqual(1, randomCalls, "Only independent initial direction consumes RNG in this fixture");
            var flames = system.Projectiles.EnemyProjectiles.Where(projectile => projectile.IsActive).ToArray();
            AssertEqual(8, flames.Length, "Exact expiry spawns eight real spiral projectiles");
            foreach (var flame in flames)
            {
                AssertEqual(RoomEnemyProjectileKind.PhantoonDestroyableFlame, flame.Kind, "Expiry produces actual Phantoon flame family");
                AssertEqual(body.Position.X.Whole, flame.Position.X.Whole, "Spiral begins at current independently traversed body X");
                AssertEqual(unchecked((ushort)(body.Position.Y.Whole + 16)), flame.Position.Y.Whole, "Spiral begins at current body offset Y");
            }
        }
        AssertEqual(2280, totalTicks, "Three native closed-eye durations in both directions");
        Console.WriteLine("Phantoon closed eye:60/360/720 native waits,2280 actual moving countdown calls,both directions/exact eye-open handoffs and48 real spiral spawns pass.");
    }

    /// <summary>Compares Ceres platform graphics and VRAM placement against retail words.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres platform")]
    internal static void VerifyLookupStream5CeresPlatform(SuperMetroidAddressSpace rom)
    {
        var document = new CeresDoorVisualDocument
        {
            Version = 1,
            Normal = Colors(CeresDoorVisualRomData.NormalColors,15),
            Escape = Colors(CeresDoorVisualRomData.EscapeColors,15),
            Animation = Enumerable.Range(0,8).Select(row => Colors(CeresDoorVisualRomData.AnimationColors +16*row,6)).ToArray(),
            Mode7DoorFrames = Enumerable.Range(0,2).Select(frame => Enumerable.Range(0,4)
                .Select(index => (int)rom.ReadCartridgeByte(CeresDoorVisualRomData.Mode7FirstFrameSource +4*frame+index)).ToArray()).ToArray(),
        };
        byte[] planar = Enumerable.Range(0,CeresDoorVisualRomData.TileByteCount)
            .Select(index => rom.ReadCartridgeByte(CeresDoorVisualRomData.TileSource+index)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar,4,RoomCharacterAtlasFormat.TileColumns,out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png,size,pixels,SnesGraphics.DiagnosticPalette(16));
        byte[] pngBytes = png.ToArray();
        CeresDoorVisualCatalog Load(CeresDoorVisualDocument value) => CeresDoorVisualCatalog.Load(
            new MemoryStream(pngBytes),new MemoryStream(CeresDoorVisualCatalog.Write(value)));
        var stock = Load(document);
        CheckPlatformActor(stock, document);
        CheckHash(stock, document);
        AssertEqual(0, stock.platformEdits.Count, "No stored stock platform cells");
        for (int frame = 0; frame < 2; frame++)
        for (int cell = 0; cell < 4; cell++)
        {
            int[][] frames = document.Mode7DoorFrames.Select(row => row.ToArray()).ToArray();
            frames[frame][cell] ^= 255;
            var changed = document with { Mode7DoorFrames = frames };
            var edited = Load(changed);
            AssertEqual(1, edited.platformEdits.Count, "One independent platform override");
            CheckPlatformActor(edited, changed);
            CheckHash(edited, changed);
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Mode7DoorFrame(-1), "Platform lower frame domain");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Mode7DoorFrame(2), "Platform upper frame domain");
        AssertThrows<ArgumentOutOfRangeException>(() => CeresMode7TransferDefinitions.PlatformTile(0, -1), "Canonical platform lower column domain");
        AssertThrows<ArgumentOutOfRangeException>(() => CeresMode7TransferDefinitions.PlatformTile(0, 4), "Canonical platform upper column domain");
        Console.WriteLine("Ceres platform: eight native cells/eight independent edits,99 actual phase calls through wrap, high-byte/neighbor preservation, canonical hashes and domains pass.");
        PaletteRgb5[] Colors(int source,int count) => Enumerable.Range(0,count).Select(index =>
        {
            ushort word = ReadVerificationWord(rom,source+2*index);
            return new PaletteRgb5 { Red = word &31, Green = (word>>5)&31, Blue = (word>>10)&31 };
        }).ToArray();
        void CheckPlatformActor(CeresDoorVisualCatalog visual, CeresDoorVisualDocument expected)
        {
            var system = new RoomEnemySystem(new EnemySession())
            {
                TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), AreaArtwork = new AreaArtworkCatalog { CeresDoorVisual = visual },
            };
            var vram = new SnesVram();
            int destination = CeresDoorVisualRomData.Mode7MapCell;
            byte[] initial = Enumerable.Range(0, 12).Select(index => (byte)(index % 2 == 0 ? 0x5d : 0xa7)).ToArray();
            vram.LoadBytes((destination - 1) * 2, initial);
            system.Presentation.BindBackgrounds(BgArtworkFixtures.For(vram));
            system.Presentation.BindCgram(new GameplayColours(new SpriteColourSets()));
            var slots = (RoomEnemySlot[])system.SlotTable.Slots;
            foreach (ushort tick in new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 0xfffe, 0xffff, 0 })
            {
                slots[0].FrameCounter = tick;
                system.CeresDoorBehavior.RunCeresDoorPaletteAnimation();
                int frame = (tick & 2) >> 1;
                for (int cell = 0; cell < 4; cell++)
                {
                    AssertEqual((ushort)(0xa700 | expected.Mode7DoorFrames[frame][cell]), vram.ReadWord(destination + cell), "Actual platform phase and character-byte preservation");
                    AssertEqual(rom.ReadCartridgeByte(0xa6f918 + frame * 4 + cell), CeresMode7TransferDefinitions.PlatformTile(frame, cell), "Canonical platform cap/atlas geometry matches native");
                }
                AssertEqual((ushort)0xa75d, vram.ReadWord(destination - 1), "Platform preceding map/character unchanged");
                AssertEqual((ushort)0xa75d, vram.ReadWord(destination + 4), "Platform following map/character unchanged");
            }
        }        static ushort Pack(PaletteRgb5 color) => (ushort)(color.Red | color.Green<<5 | color.Blue<<10);
        void CheckHash(CeresDoorVisualCatalog actual, CeresDoorVisualDocument expected)
        {
            string identity = SelectedPresentationHash.Create("enemy-ceres-door-v1",content =>
            {
                content.Append("tiles",planar);
                content.AppendWords("normal",expected.Normal.Select(Pack).ToArray());
                content.AppendWords("escape",expected.Escape.Select(Pack).ToArray());
                content.AppendWordFrames("animation",expected.Animation.Select(row=>row.Select(Pack).ToArray()).ToArray());
                content.Append("mode7-frames",expected.Mode7DoorFrames.Length);
                foreach(var frame in expected.Mode7DoorFrames) content.Append("mode7-frame",frame.Select(value=>(byte)value).ToArray());
            });
            AssertEqual(identity,actual.ContentIdentity,"Ceres calculated colors preserve canonical resource identity");
        }
    }

    /// <summary>Checks Phantoon's exposed-body phase and related authored visual state.</summary>
    internal static void VerifyLookupStream5PhantoonExposure(SuperMetroidAddressSpace rom)
    {
        int ticks = 0;
        foreach (int bucket in new[] { 2, 1, 0 })
        foreach (bool shot in new[] { false, true })
        {
            var system = new RoomEnemySystem(new EnemySession(), services: new VerificationRoomEnemyServices(null));
            var body = system.SlotTable.Slots[0];
            var eye = system.SlotTable.Slots[1];
            var tentacles = system.SlotTable.Slots[2];
            var state = new PhantoonEnemyState(body) { Eye = eye, Tentacles = tentacles, Mouth = system.SlotTable.Slots[3] };
            system.PhantoonBehavior.Combat.Phantoon = state;
            system.VerificationServices().NextRandom = () => (ushort)bucket;
            Action<PhantoonEnemyState> open = system.PhantoonBehavior.BeginPhantoonEyeTracking;
            Action<RoomEnemySlot, PhantoonEnemyState, SamusState> tick = PhantoonPatterns.RunPhantoonEyeTracking;
            body.Position = body.Position.WithWhole(new(128, 96));
            open(state);
            ushort duration = ReadVerificationWord(rom, 0xa7cd41 + bucket * 2);
            AssertEqual(duration, state.Parts[body.SlotIndex].FunctionTimer, "Native selected vulnerability duration");
            AssertTrue(!body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision), "Eye opening enables collision");
            state.Parts[tentacles.SlotIndex].SwoopRequested = shot;
            state.Parts[tentacles.SlotIndex].RoundDamage = 37;
            var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(192, 96)) };
            for (int elapsed = 1; elapsed <= duration; elapsed++)
            {
                tick(body, state, samus); ticks++;
                AssertEqual((ushort)128, body.Position.X.Whole, "Exposure duration does not advance a spatial trajectory");
                AssertEqual((ushort)96, body.Position.Y.Whole, "Exposure body remains at current Y");
                if (elapsed < duration)
                {
                    AssertEqual((ushort)(duration - elapsed), state.Parts[body.SlotIndex].FunctionTimer, "One exact countdown decrement per actor call");
                    AssertEqual(PhantoonAiFunction.EyeTracksSamus, state.Parts[body.SlotIndex].Function, "No early exposure transition");
                    AssertTrue(!body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision), "Exposure remains collidable before expiry");
                    AssertEqual(37, state.Parts[tentacles.SlotIndex].RoundDamage, "Round damage remains until expiration");
                }
            }
            AssertEqual(0, state.Parts[tentacles.SlotIndex].RoundDamage, "Expiration resets accumulated round damage");
            if (shot)
            {
                AssertEqual(PhantoonAiFunction.BecomeSolidAndSwoop, state.Parts[body.SlotIndex].Function, "Shot-triggered expiration selects swoop");
                AssertEqual(ReadVerificationWord(rom, 0xa7d620), state.Parts[body.SlotIndex].FunctionTimer, "Separate native swoop setup hold preserved");
                AssertEqual(false, state.Parts[tentacles.SlotIndex].SwoopRequested, "Expiration consumes swoop request");
            }
            else
            {
                AssertEqual(PhantoonAiFunction.NoOperation, state.Parts[body.SlotIndex].Function, "Unhit expiration hands off to eye close");
                AssertEqual(PhantoonPartProgram.InvulnerableBody, state.PartProgram(body).Program, "Native invulnerable body handoff");
                AssertEqual(PhantoonPartProgram.EyeCloseAndPickNewPattern, state.PartProgram(eye).Program, "Native close/reselect eye handoff");
                AssertTrue(body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision), "Expiration removes open-eye collision");
                AssertEqual(true, state.Parts[body.SlotIndex].FlameRainRequested, "Unhit expiration requests next rain branch");
            }
        }
        AssertEqual(210, ticks, "All three actual exposure lengths and both expiry branches");
        Console.WriteLine("Phantoon exposure: three native window lengths,210 actual countdown calls, exact collision/shot/close transitions and separate swoop hold pass.");
    }
    /// <summary>Compares Ceres overlay tile words and their installed draw order with retail data.</summary>
    internal static void VerifyLookupStream5CeresOverlayWords(SuperMetroidAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Areas.Ceres.CeresEscapeOverlayTilemapFiles.Extract(rom);
        var document = RequiredJson.Deserialize<CeresEscapeOverlayTilemapDocument>(json, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        CeresEscapeOverlayTilemapCatalog stock = Check(document);
        var edits = stock.edits;
        AssertEqual(0, edits.Count, "All native Ceres glyph words calculate without overrides");
        int words = 0;
        foreach (var page in CeresEscapeOverlayTilemapDefinitions.All)
        {
            for (int index = 0; index < page.WordCount; index++)
            {
                ushort native = ReadVerificationWord(rom, page.SourceAddress + index * 2);
                AssertEqual(native, CeresEscapeOverlayTilemapDefinitions.StockWord(page, index), "Native calculated subtitle glyph/style");
                document.Pages[page.Name][index] = (ushort)(native ^ ushort.MaxValue);
                _ = Check(document);
                document.Pages[page.Name][index] = native;
                words++;
            }
            AssertTrue(!stock.TryResolve(page.SourceAddress, page.WordCount * 2 - 1, out _), "Changed transfer extent rejected");
            AssertThrows<ArgumentOutOfRangeException>(() => CeresEscapeOverlayTilemapDefinitions.StockWord(page, -1), "Glyph lower bound");
            AssertThrows<ArgumentOutOfRangeException>(() => CeresEscapeOverlayTilemapDefinitions.StockWord(page, page.WordCount), "Glyph upper bound");
        }
        AssertTrue(!stock.TryResolve(0, 2, out _), "Unknown subtitle source rejected");
        AssertEqual(55, words, "Native subtitle word count");
        Console.WriteLine("Ceres overlay glyphs: 55 native words, zero stock overrides, 55 independent full-word edits, exact original hash and transfer domains pass.");

        CeresEscapeOverlayTilemapCatalog Check(CeresEscapeOverlayTilemapDocument source)
        {
            var result = CeresEscapeOverlayTilemapCatalog.Load(new MemoryStream(CeresEscapeOverlayTilemapCatalog.Write(source)));
            var overlay = new RecordedBg1Overlay();
            var system = new RoomEnemySystem(new EnemySession(), services: overlay.Services())
            {
                TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), AreaArtwork = new AreaArtworkCatalog { CeresEscapeOverlayTilemaps = result },
            };
            system.CeresRidleyBehavior.Getaway.DrawCeresEmergencyText();
            int first = ReadVerificationWord(rom, 0xa6c162) - GameplayVramLayout.Bg1TilemapWord;
            int count = ReadVerificationWord(rom, 0xa6c15d) / sizeof(ushort);
            AssertEqual(count, overlay.Overlay.Words.Count(word => word != RoomTilemapOverlay.Unwritten), "Native emergency word extent");
            AssertEqual(source.Pages["emergency"].Length, count, "Edited emergency page keeps its native extent");
            for (int index = 0; index < count; index++)
                AssertEqual(source.Pages["emergency"][index], overlay.Overlay.Words[first + index], "Actual shown emergency preserves independent edits");
            var transfers = new Dictionary<int, byte[]>();
            foreach (var page in CeresEscapeOverlayTilemapDefinitions.All)
            {
                byte[] expected = new byte[page.WordCount * 2];
                for (int index = 0; index < page.WordCount; index++)
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(index * 2), source.Pages[page.Name][index]);
                AssertTrue(result.TryResolve(page.SourceAddress, expected.Length, out var actual) && actual.Span.SequenceEqual(expected), "Independent glyph word and neighboring data preservation");
                transfers.Add(page.SourceAddress, expected);
            }
            AssertEqual(SelectedPresentationHash.FromTransfers("enemy-ceres-escape-overlay-v1", transfers, bytes => bytes), result.ContentIdentity, "Original overlay hash framing");
            return result;
        }
    }
    /// <summary>Checks Crocomire wall and spike palette transfers where their CGRAM destinations overlap.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyLookupStream5CrocomireSharedPaint(SuperMetroidAddressSpace rom)
    {
        int[] sources = [CrocomirePaletteRomData.FightBodySource, CrocomirePaletteRomData.InitialWallSource,
            CrocomirePaletteRomData.InitialProjectileSource, CrocomirePaletteRomData.SkeletonArmSource, CrocomirePaletteRomData.WallSpikesSource];
        int[] counts = [8, 17, 17, 16, 16];
        int[] destinations = [112, 160, 208, 144, 176];
        string[] labels = ["fightBody", "initialWall", "initialProjectile", "skeletonArm", "wallSpikes"];
        PaletteRgb5[][] bands = new PaletteRgb5[5][];
        for (int band = 0; band < bands.Length; band++)
        {
            bands[band] = new PaletteRgb5[counts[band]];
            for (int color = 0; color < counts[band]; color++)
            {
                ushort native = ReadVerificationWord(rom, sources[band] + color * 2);
                bands[band][color] = new PaletteRgb5 { Red = native & 31, Green = native >> 5 & 31, Blue = native >> 10 & 31 };
            }
        }
        CrocomireColorCatalog stock = Check();
        AssertTrue(!MetadataShape.DeclaresField(typeof(CrocomireColorCatalog), "paint", BindingFlags.Instance | BindingFlags.NonPublic), "Stock palette storage has been removed");
        var edits = stock.edits;
        AssertTrue(!MetadataShape.Fields(typeof(CrocomirePaintDefinitions), BindingFlags.Static | BindingFlags.NonPublic).Any(field => field.FieldType.IsArray), "Material calculation does not cache an output palette");
        AssertEqual(0, edits.Count, "All 74 stock inks calculate without overrides");
        for (int band = 0; band < bands.Length; band++)
            for (int color = 0; color < counts[band]; color++)
            {
                PaletteRgb5 original = bands[band][color];
                for (int channel = 0; channel < 3; channel++)
                {
                    bands[band][color] = new PaletteRgb5 { Red = channel == 0 ? original.Red ^ 31 : original.Red,
                        Green = channel == 1 ? original.Green ^ 31 : original.Green, Blue = channel == 2 ? original.Blue ^ 31 : original.Blue };
                    _ = Check();
                }
                bands[band][color] = original;
            }
        Console.WriteLine("Crocomire material paint: 74 native colors, 222 independent channel edits, zero stock palette/overrides, actual CGRAM, hash and bounds pass.");

        CrocomireColorCatalog Check()
        {
            var document = new CrocomireColorDocument { Version = 1, FightBody = bands[0], InitialWall = bands[1], InitialProjectile = bands[2], SkeletonArm = bands[3], WallSpikes = bands[4] };
            var result = CrocomireColorCatalog.Load(new MemoryStream(CrocomireColorCatalog.Write(document)));
            Func<int, Bgr555>[] resolve = [result.ResolveFightBody, result.ResolveInitialWall, result.ResolveInitialProjectile, result.ResolveSkeletonArm, result.ResolveWallSpikes];
            var cgram = new GameplayColours(new SpriteColourSets()); result.ApplyInitial(cgram);
            for (int band = 1; band <= 2; band++)
                for (int ink = 0; ink < counts[band]; ink++)
                    AssertEqual(resolve[band](ink), cgram.Colors[destinations[band] + ink], "Initial seventeen-word copy preserves its independently supplied overlap");
            result.ApplyFightBody(cgram); result.ApplySkeletonArm(cgram); result.ApplyWallSpikes(cgram);
            string expectedHash = SelectedPresentationHash.Create("CrocomireColorCatalog-v1", content =>
            {
                for (int band = 0; band < bands.Length; band++)
                {
                    ushort[] words = new ushort[counts[band]];
                    for (int color = 0; color < words.Length; color++)
                    {
                        PaletteRgb5 rgb = bands[band][color];
                        words[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
                        AssertEqual(words[color], resolve[band](color), "Independent native/edit word");
                        // Wall's overlapping destination176 is subsequently replaced by the spike transfer.
                        if (band != 1 || color != 16) AssertEqual(words[color], cgram.Colors[destinations[band] + color], "Calculated paint reaches CGRAM");
                    }
                    content.AppendWords(labels[band], words);
                    int selected = band;
                    AssertThrows<ArgumentOutOfRangeException>(() => resolve[selected](-1), "Paint lower domain");
                    AssertThrows<ArgumentOutOfRangeException>(() => resolve[selected](counts[selected]), "Paint upper domain");
                }
            });
            AssertEqual(expectedHash, result.ContentIdentity, "Original five-band canonical hash framing");
            return result;
        }
    }
    /// <summary>Checks the Ceres visual transfer source pages selected by compiled definitions.</summary>
    internal static void VerifyLookupStream5CeresSourcePages()
    {
        TileSheetDefinition[] tilePages = [new(0xb7da00, 0x900, "ceres-escape-warning-tiles.png"), new(0xb0ba00, 0x600, "ceres-escape-door-tiles.png")];
        AssertTrue(tilePages.SequenceEqual(CeresEscapeTileArtworkDefinitions.All), "Original named character page order/source/extent/file");
        foreach (var page in tilePages)
        {
            AssertTrue(CeresEscapeTileArtworkDefinitionsTooling.Contains(page.SourceAddress, page.ByteCount), "Whole page admitted");
            AssertTrue(CeresEscapeTileArtworkDefinitionsTooling.Contains(page.SourceAddress + page.ByteCount - 1, 1), "Last byte admitted");
            AssertTrue(!CeresEscapeTileArtworkDefinitionsTooling.Contains(page.SourceAddress - 1, 1) && !CeresEscapeTileArtworkDefinitionsTooling.Contains(page.SourceAddress + page.ByteCount, 1), "Outside page rejected");
            AssertTrue(!CeresEscapeTileArtworkDefinitionsTooling.Contains(page.SourceAddress, 0), "Zero byte page rejected");
        }
        CeresEscapeOverlayTilemapDefinition[] overlays = [new("emergency", 0xa6c164, 9, 0x0cb), new("japanese_0", 0xa6c3f4, 12, 0x28a), new("japanese_1", 0xa6c40c, 12, 0x2aa), new("japanese_2", 0xa6c424, 11, 0x2ca), new("japanese_3", 0xa6c43a, 11, 0x2ea)];
        AssertTrue(overlays.SequenceEqual(CeresEscapeOverlayTilemapDefinitions.All), "Original named overlay order/source/extent");
        foreach (var page in overlays)
        {
            AssertTrue(CeresEscapeOverlayTilemapDefinitionsTooling.IsSource(page.SourceAddress, page.WordCount * 2), "Exact overlay extent admitted");
            AssertTrue(!CeresEscapeOverlayTilemapDefinitionsTooling.IsSource(page.SourceAddress, page.WordCount * 2 - 1), "Partial overlay extent rejected");
            AssertTrue(CeresEscapeOverlayTilemapDefinitions.ContainsByteAddress(page.SourceAddress) && CeresEscapeOverlayTilemapDefinitions.ContainsByteAddress(page.SourceAddress + page.WordCount * 2 - 1), "Overlay byte endpoints admitted");
        }
        AssertThrows<IndexOutOfRangeException>(() => _ = CeresEscapeTileArtworkDefinitions.All[-1], "Tile page lower bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = CeresEscapeTileArtworkDefinitions.All[2], "Tile page upper bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = CeresEscapeOverlayTilemapDefinitions.All[-1], "Overlay lower bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = CeresEscapeOverlayTilemapDefinitions.All[5], "Overlay upper bound");
        Console.WriteLine("Ceres source page cases: two character sheets/five overlays preserve exact identities, order, extents, enumeration and range domains.");
    }
    /// <summary>Checks authored area-map landmark tiles and their native reveal cases.</summary>
    internal static void VerifyLookupStream5MapLandmarkCases(SuperMetroidAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Frontend.Map.MapLandmarkExtractor.Extract(rom);
        MapLandmarkLayout layout = MapLandmarkLayout.Load(new MemoryStream(extracted));
        var artwork = SuperMetroid.AssetExtraction.Frontend.Map.MapSpriteExtractor.Extract(rom);
        MapSpriteCatalog sprites = MapSpriteCatalog.Load(new MemoryStream(artwork[MapSpriteFormat.JsonFile]), new MemoryStream(artwork[MapSpriteFormat.PngFile]));
        int Read(int pointer) => ReadVerificationWord(rom, FileSelectMapRomData.MenuObjectBank | pointer);
        int Root(int table, AreaId area) => ReadVerificationWord(rom, table + (int)area * 2);
        string[] expectedIds = ["Crateria.Elevator.0", "Crateria.Elevator.1", "Crateria.Elevator.2", "Crateria.Elevator.3", "Crateria.Elevator.4",
            "Boss.Kraid", "Brinstar.Elevator.0", "Brinstar.Elevator.1", "Brinstar.Elevator.2", "Brinstar.Elevator.3", "Brinstar.Elevator.4",
            "Boss.Ridley", "Norfair.Elevator.0", "Boss.Phantoon", "WreckedShip.Elevator.0", "WreckedShip.Elevator.1",
            "Boss.Draygon", "Maridia.Elevator.0", "Maridia.Elevator.1", "Maridia.Elevator.2", "Tourian.Elevator.0", "Boss.CeresRidley", "Crateria.Gunship"];
        AssertTrue(expectedIds.SequenceEqual(MapLandmarkDefinitions.AllIds()), "Original public identity and manifest order");
        using var editedBytes = new MemoryStream();
        MapLandmarkLayout.Write(editedBytes, new MapLandmarkDocument { Version = MapLandmarkFormat.Version,
            Markers = expectedIds.ToDictionary(id => id, id => layout.Get(id) with { X = (ushort)(layout.Get(id).X + 1), Y = (ushort)(layout.Get(id).Y + 1) }) });
        editedBytes.Position = 0;
        MapLandmarkLayout editedLayout = MapLandmarkLayout.Load(editedBytes);
        int bossSlots = 0, elevatorSlots = 0;
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            var bosses = MapLandmarkDefinitions.Bosses(area);
            int pointer = Root(FileSelectMapIconRomData.BossLists, area);
            int count = 0;
            if (pointer != 0)
                while (Read(pointer + count * 4) != ushort.MaxValue)
                {
                    string? id = bosses[count];
                    AssertEqual(Read(pointer + count * 4) == 0xfffe, id is null, "Native unused boss slot");
                    if (id is not null)
                    {
                        AssertEqual(Read(pointer + count * 4), layout.Get(id).X, "Native semantic boss X");
                        AssertEqual(Read(pointer + count * 4 + 2), layout.Get(id).Y, "Native semantic boss Y");
                    }
                    count++;
                }
            AssertEqual(count, bosses.Length, "Native boss slot count");
            AssertEqual(count, bosses.ToArray().Length, "Boss enumeration count");
            AssertThrows<IndexOutOfRangeException>(() => _ = bosses[-1], "Boss lower bound");
            AssertThrows<IndexOutOfRangeException>(() => _ = bosses[count], "Boss upper bound");
            bossSlots += count;
            if (area == AreaId.Ceres) continue;
            var elevators = MapLandmarkDefinitions.Elevators(area);
            pointer = Root(FileSelectMapIconRomData.ElevatorLists, area);
            count = 0;
            while (Read(pointer + count * 6) != ushort.MaxValue)
            {
                var label = elevators[count];
                AssertEqual(Read(pointer + count * 6 + 4), (int)MapLandmarkDefinitions.ElevatorSpritemap(label.Destination), "Native destination identity");
                AssertEqual(Read(pointer + count * 6), layout.Get(label.Id).X, "Native elevator X");
                AssertEqual(Read(pointer + count * 6 + 2), layout.Get(label.Id).Y, "Native elevator Y");
                count++;
            }
            AssertEqual(count, elevators.Count, "Native elevator count");
            AssertEqual(count, elevators.ToArray().Length, "Elevator enumeration count");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = elevators[-1], "Elevator lower bound");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = elevators[count], "Elevator upper bound");
            elevatorSlots += count;
        }
        foreach (AreaId area in Enum.GetValues<AreaId>())
        foreach (int adjustment in new[] { 0, 1 })
        foreach (bool downloaded in new[] { false, true })
        foreach (bool defeated in new[] { false, true })
        {
            var system = new Bank80SystemState();
            if (downloaded) system.SetAreaMapAcquired(area);
            if (defeated) system.SetBossBits(area, (BossBits)1);
            var icons = new FileSelectMapIcons(system, area);
            icons.BindLandmarks(adjustment == 0 ? layout : editedLayout); icons.BindSprites(MenuSheetFixtures.MapSprites(sprites));
            var actual = ScreenSpritesFixtures.Blank(); var expected = ScreenSpritesFixtures.Blank();
            icons.DrawBossMarkers(actual, new(7, 9));
            int pointer = Root(FileSelectMapIconRomData.BossLists, area), bits = defeated ? 1 : 0;
            if (pointer != 0)
                for (int i = 0; Read(pointer + i * 4) != ushort.MaxValue; i++, bits >>= 1)
                {
                    int x = Read(pointer + i * 4), y = Read(pointer + i * 4 + 2);
                    if (x == 0xfffe) continue;
                    if ((bits & 1) != 0)
                    {
                        Draw(MapSpriteId.MarkerDefeatedBoss, x, y, FileSelectMapRomData.StationMarkerPalette);
                        Draw(MapSpriteId.MarkerBoss, x, y, FileSelectMapIconRomData.DefeatedBossPalette);
                    }
                    else if (downloaded) Draw(MapSpriteId.MarkerBoss, x, y, FileSelectMapRomData.StationMarkerPalette);
                }
            if (area != AreaId.Ceres)
            {
                icons.DrawAfterMarker(actual, new(7, 9));
                if (area == AreaId.Crateria)
                {
                    int ship = Root(FrontendCartridgeTables.SavePointMapPointers, area);
                    Draw(MapSpriteId.MarkerGunship, Read(ship), Read(ship + 2), FileSelectMapRomData.StationMarkerPalette);
                }
                if (downloaded)
                {
                    pointer = Root(FileSelectMapIconRomData.ElevatorLists, area);
                    for (int i = 0; Read(pointer + i * 6) != ushort.MaxValue; i++)
                        Draw((MapSpriteId)Read(pointer + i * 6 + 4), Read(pointer + i * 6), Read(pointer + i * 6 + 2), 0);
                }
            }
            actual.FinishFrame(); expected.FinishFrame();
            AssertTrue(actual.ImageEquals(expected), "Actual native ordered landmark OAM");
            AssertEqual(defeated ? (byte)1 : (byte)0, system.GetBossBitsRaw(area), "Drawing preserves boss bits");
            AssertEqual(downloaded, system.HasAreaMap(area), "Drawing preserves map state");
            void Draw(MapSpriteId id, int x, int y, ushort palette) => MapSpriteSheet.Draw(MenuSheetFixtures.MapSprites(sprites), id, expected, new(unchecked((ushort)(x + adjustment - 7)), unchecked((ushort)(y + adjustment - 9))), palette);
        }
        AssertThrows<ArgumentOutOfRangeException>(() => MapLandmarkDefinitions.Bosses((AreaId)7), "Invalid boss area");
        AssertThrows<ArgumentOutOfRangeException>(() => MapLandmarkDefinitions.Elevators(AreaId.Ceres), "Ceres has no elevator labels");
        AssertEqual(8, bossSlots, "Eight native boss slots including three unused");
        AssertEqual(17, elevatorSlots, "Seventeen native destination labels");
        Console.WriteLine("Map landmark cases: eight native boss slots, seventeen destinations, extraction/schema, 23 ordered IDs, bounds and 56 stock/edited ordered OAM/state cases pass.");
    }
    /// <summary>Checks Phantoon's vulnerable collision state against its native phase data.</summary>
    internal static void VerifyLookupStream5PhantoonCollision(SuperMetroidAddressSpace rom)
    {
        int componentCount = 0;
        var lists = new HashSet<ushort>();
        foreach (EnemySpritemapDefinition frame in PhantoonBg2FrameDefinitions.Frames)
        {
            int root = 0xa70000 | frame.Pointer;
            var components = PhantoonCollisionDefinitions.ComponentsAt(frame.Pointer);
            AssertEqual(ReadVerificationWord(rom, root), components.Length, "Native component extent");
            for (int index = 0; index < components.Length; index++)
            {
                int record = root + 2 + index * 8;
                PhantoonCollisionComponent component = components[index];
                AssertEqual(unchecked((short)ReadVerificationWord(rom, record)), component.Offset.X, "Native component X");
                AssertEqual(unchecked((short)ReadVerificationWord(rom, record + 2)), component.Offset.Y, "Native component Y");
                componentCount++;
                // The ROM names the component's hitbox list by address; the oracle decodes the list it names, and
                // every compiled component must hold exactly those rectangles and reactions, so a component holding
                // another list fails.
                ushort listPointer = ReadVerificationWord(rom, record + 6);
                lists.Add(listPointer);
                int address = 0xa70000 | listPointer;
                IReadOnlyList<PhantoonCollisionHitbox> hitboxes = component.Hitboxes;
                AssertEqual(ReadVerificationWord(rom, address), hitboxes.Count, "Native hitbox extent");
                for (int hitbox = 0; hitbox < hitboxes.Count; hitbox++)
                {
                    int native = address + 2 + hitbox * 12;
                    LookupStream5Expected expected = new LookupStream5Expected(unchecked((short)ReadVerificationWord(rom, native)),
                        unchecked((short)ReadVerificationWord(rom, native + 2)), unchecked((short)ReadVerificationWord(rom, native + 4)),
                        unchecked((short)ReadVerificationWord(rom, native + 6)), ReadVerificationWord(rom, native + 8), ReadVerificationWord(rom, native + 10));
                    PhantoonCollisionHitbox actual = hitboxes[hitbox];
                    LookupStream5Expected compiled = new LookupStream5Expected(actual.Left, actual.Top, actual.Right, actual.Bottom,
                        PhantoonHitboxCallbackCodec.ToCallbackWord(actual.Touch), PhantoonHitboxCallbackCodec.ToCallbackWord(actual.Shot));
                    AssertEqual(expected, compiled, "Native shape and callback pair");
                }
            }
            AssertEqual(components.Length, components.ToArray().Length, "Component enumeration compatibility");
            AssertThrows<IndexOutOfRangeException>(() => _ = components[components.Length], "Component upper bound");
        }
        AssertEqual(25, componentCount, "All native components");
        AssertEqual(3, lists.Count, "All native hitbox lists");
        AssertEqual(EnemyAiCodePointers.BankA7.PhantoonShot.Offset, FindPhantoonHitboxCallback(null, (ushort)PhantoonCollisionFrame.BodyFullHitbox, new(128, 128), true), "Actual full-body shot callback");
        AssertEqual(EnemyAiCodePointers.BankA7.PhantoonTouch.Offset, FindPhantoonHitboxCallback(null, (ushort)PhantoonCollisionFrame.BodyFullHitbox, new(128, 128), false), "Actual full-body touch callback");
        AssertEqual(EnemyAiCodePointers.BankA7.PhantoonShot.Offset, FindPhantoonHitboxCallback(null, (ushort)PhantoonCollisionFrame.BodyEyeHitboxOnly, new(128, 153), true), "Actual vulnerable eye callback");
        AssertEqual((ushort)0, FindPhantoonHitboxCallback(null, (ushort)PhantoonCollisionFrame.BodyEyeHitboxOnly, new(128, 128), true), "Eye-only frame excludes body collision");
        AssertThrows<InvalidDataException>(() => PhantoonCollisionDefinitions.ComponentsAt(0), "Frame domain");
        Console.WriteLine("Phantoon collision: 22 native frames, 25 components, all seven rectangles/callbacks, actual full-body/eye selection and bounds pass.");
    }
    /// <summary>Compares Phantoon fade color rows and transition timing with cartridge palettes.</summary>
    internal static void VerifyLookupStream5PhantoonFade(SuperMetroidAddressSpace rom)
    {
        byte[] bytes = SuperMetroid.AssetExtraction.Bosses.PhantoonColorExtractor.Extract(rom);
        PhantoonColorDocument document = RequiredJson.Deserialize<PhantoonColorDocument>(bytes,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
        PhantoonColorCatalog stock = Check(document);
        var edits = stock.fadeOutEdits;
        AssertEqual(0, edits.Count, "Stock fade stores no target table");
        for (int color = 0; color < PhantoonColorRomData.FadeOutCount; color++)
        {
            AssertEqual(ReadVerificationWord(rom, PhantoonColorRomData.FadeOutSource + color * 2), stock.ResolveFadeOut(color), "Native black fade endpoint");
            for (int channel = 0; channel < 3; channel++)
            {
                document.FadeOut[color] = new PaletteRgb5 { Red = channel == 0 ? 31 : 0, Green = channel == 1 ? 31 : 0, Blue = channel == 2 ? 31 : 0 };
                _ = Check(document);
            }
            document.FadeOut[color] = new PaletteRgb5 { Red = 0, Green = 0, Blue = 0 };
        }
        var healthEdits = stock.healthEdits;
        AssertEqual(0, healthEdits.Count, "All health colors calculate without stock overrides");
        AssertTrue(!MetadataShape.DeclaresField(typeof(PhantoonColorCatalog), "requiredHealthChannels", BindingFlags.Instance | BindingFlags.NonPublic),
            "No per-channel native residual samples remain");
        AssertTrue(!MetadataShape.DeclaresField(typeof(PhantoonColorCatalog), "healthyPaint", BindingFlags.Instance | BindingFlags.NonPublic),
            "No stock healthy palette remains stored");
        for (int band = 0; band < PhantoonColorRomData.HealthBandCount; band++)
        for (int color = 0; color < PhantoonColorRomData.HealthBandColorCount; color++)
        {
            AssertEqual(ReadVerificationWord(rom, PhantoonColorRomData.HealthBandsSource + (band * 16 + color) * 2), stock.ResolveHealth(band, color), "Native health RGB equality");
            PaletteRgb5 original = document.HealthBands[band][color];
            for (int channel = 0; channel < 3; channel++)
            {
                document.HealthBands[band][color] = new PaletteRgb5
                {
                    Red = channel == 0 ? original.Red ^ 31 : original.Red,
                    Green = channel == 1 ? original.Green ^ 31 : original.Green,
                    Blue = channel == 2 ? original.Blue ^ 31 : original.Blue,
                };
                _ = Check(document);
            }
            document.HealthBands[band][color] = original;
        }
        var powerEdits = stock.powerEdits;
        AssertTrue(!MetadataShape.DeclaresField(typeof(PhantoonColorCatalog), "requiredPowerColors", BindingFlags.Instance | BindingFlags.NonPublic), "Power targets retain no stock color dictionary");
        AssertEqual(0, powerEdits.Count, "Native power targets have no unexplained overrides");
        for (int color = 0; color < PhantoonColorRomData.PowerOnCount; color++)
        {
            AssertEqual(ReadVerificationWord(rom, PhantoonColorRomData.PowerOnSource + color * 2), stock.ResolvePowerOn(color), "Native power shade equality");
            PaletteRgb5 original = document.PowerOn[color];
            for (int channel = 0; channel < 3; channel++)
            {
                document.PowerOn[color] = new PaletteRgb5
                {
                    Red = channel == 0 ? original.Red ^ 31 : original.Red,
                    Green = channel == 1 ? original.Green ^ 31 : original.Green,
                    Blue = channel == 2 ? original.Blue ^ 31 : original.Blue,
                };
                _ = Check(document);
            }
            document.PowerOn[color] = original;
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolvePowerOn(-1), "Power lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolvePowerOn(112), "Power upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveHealth(-1, 0), "Health band lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveHealth(8, 0), "Health band upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveHealth(0, -1), "Health color lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveHealth(0, 16), "Health color upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveFadeOut(-1), "Fade lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveFadeOut(16), "Fade upper bound");
        Console.WriteLine("Phantoon colors: 16 native fade targets, 128 health colors, zero stored healthy palette/residual samples or stock overrides, 112 power colors, 768 independent channel edits, hashes and bounds pass.");

        static PhantoonColorCatalog Check(PhantoonColorDocument source)
        {
            PhantoonColorCatalog result = PhantoonColorCatalog.Load(new MemoryStream(PhantoonColorCatalog.Write(source), writable: false));
            ushort[] Pack(PaletteRgb5[] colors) => colors.Select(c => (ushort)(c.Red | c.Green << 5 | c.Blue << 10)).ToArray();
            ushort[] fade = Pack(source.FadeOut);
            for (int color = 0; color < fade.Length; color++) AssertEqual(fade[color], result.ResolveFadeOut(color), "Independent fade input");
            for (int band = 0; band < source.HealthBands.Length; band++)
            {
                ushort[] values = Pack(source.HealthBands[band]);
                for (int color = 0; color < values.Length; color++) AssertEqual(values[color], result.ResolveHealth(band, color), "Independent health channels and unaffected targets");
            }
            ushort[] power = Pack(source.PowerOn);
            for (int color = 0; color < power.Length; color++) AssertEqual(power[color], result.ResolvePowerOn(color), "Power paint preserved");
            string hash = SelectedPresentationHash.Create("PhantoonColorCatalog-v1", content =>
            {
                content.AppendWords("fadeOut", fade);
                content.AppendWords("powerOn", Pack(source.PowerOn));
                content.AppendWordFrames("healthBands", source.HealthBands.Select(Pack).ToArray());
            });
            AssertEqual(hash, result.ContentIdentity, "Original canonical hash framing");
            return result;
        }
    }
    /// <summary>Checks map screen button prompts and tile choices against retail definitions.</summary>
    internal static void VerifyLookupStream5MapButtons(SuperMetroidAddressSpace rom)
    {
        var native = new SnesButton[4];
        for (int i = 0; i < native.Length; i++)
        {
            native[i] = (SnesButton)ReadVerificationWord(rom, FileSelectMapRomData.ScrollArrows + i * 10 + 6);
            AssertEqual(i + 1, (int)ReadVerificationWord(rom, FileSelectMapRomData.ScrollArrows + i * 10 + 8), "Native direction identity");
            AssertEqual(native[i], MapScrollControls.ButtonFor((MapScrollDirection)(i + 1)), "Native directional mask");
            AssertEqual(native[i], MapScrollControls.Buttons[i], "Calculated ordered view");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => MapScrollControls.ButtonFor(MapScrollDirection.None), "No direction has no binding");
        AssertThrows<ArgumentOutOfRangeException>(() => MapScrollControls.ButtonFor((MapScrollDirection)5), "Invalid direction");
        var state = new Bank80SystemState();
        state.MarkExploredMapTile(AreaId.Crateria, new(0, 0));
        state.MarkExploredMapTile(AreaId.Crateria, new(63, 31));
        var map = new LookupStream5ScrollMap();
        for (int selected = 0; selected < 4; selected++)
        {
            var stock = new FileSelectMapScroll(map, state, new(240, 128));
            ushort startX = stock.Scroll.X, startY = stock.Scroll.Y;
            for (int tick = 1; tick <= 8; tick++)
            {
                AssertEqual(tick == 8, stock.Step(tick == 1 ? native[selected] : SnesButton.None), "Stock sound boundary");
                int delta = tick >= 4 ? 8 : 0;
                AssertEqual(unchecked((ushort)(startX + (selected == 0 ? -delta : selected == 1 ? delta : 0))), stock.Scroll.X, "Horizontal pulse");
                AssertEqual(unchecked((ushort)(startY + (selected == 2 ? -delta : selected == 3 ? delta : 0))), stock.Scroll.Y, "Vertical pulse");
                AssertEqual(tick == 8 ? MapScrollDirection.None : (MapScrollDirection)(selected + 1), stock.Direction, "Accepted direction survives release");
            }
        }
        var priority = new FileSelectMapScroll(map, state, new(240, 128));
        _ = priority.Step((native[0] | native[1] | native[2] | native[3]));
        AssertEqual(MapScrollDirection.Left, priority.Direction, "Left takes priority among simultaneous directions");
        Console.WriteLine("Map buttons: four native semantic cases, domain bounds, four-direction pulses, release and priority pass.");
    }

    /// <summary>Always-discoverable Crateria map fixture that isolates scroll behavior from reveal state.</summary>
    internal sealed class LookupStream5ScrollMap : IAreaMapView
    {
        /// <summary>Uses Crateria's area-map coordinate domain.</summary>
        public AreaId Area => AreaId.Crateria;
        /// <summary>Supplies a blank tile so scroll assertions do not depend on map art.</summary>
        public MapTileWord GetTile(Position16<MapTiles> cell) => default;
        /// <summary>Admits every fixture cell for scroll selection.</summary>
        public bool IsDiscoverable(Position16<MapTiles> cell) => true;
        /// <summary>Leaves station-driven visibility out of this scroll fixture.</summary>
        public bool IsRevealedByMapStation(Position16<MapTiles> cell) => false;
        /// <summary>Disables the special upper-cell reveal path in this fixture.</summary>
        public bool RevealsCellAbove(Position16<MapTiles> cell) => false;
    }

    /// <summary>Compares Zebes starfield sprite positions and attributes with the native OAM composition.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Zebes star fields")]
    internal static void VerifyLookupStream5ZebesStarFields(SuperMetroidAddressSpace rom)
    {
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        foreach (var definition in CeresDestructionSpriteDefinitions.Frames)
            frames.Add(definition.Name, SuperMetroid.AssetExtraction.Frontend.Intro.IntroCinematicSpriteFrameExtractor.Extract(
                rom, CinematicSpritemapAddresses.Of(definition.Pose), definition.StockPartCount, definition.Name));
        var document = new CeresDestructionSpriteDocument { Version = 1, Frames = frames };
        var stock = Load(document);
        int total = 0;
        foreach (var definition in CeresDestructionSpriteDefinitions.Frames)
        {
            if (!definition.Name.StartsWith("zebes-stars-", StringComparison.Ordinal)) continue;
            SpriteVisualPart[] parts = frames[definition.Name];
            Check(stock, definition.Pose, parts);
            var compiled = IntroCinematicSpriteCompiler.Compile(parts, definition.Name);
            var calculated = ZebesStarGridParts.CalculateIfMatching(definition.Backdrop, compiled);
            AssertTrue(calculated.parts is ZebesStarGridParts, "Stock stars use calculated common fields");
            for (int index = 0; index < parts.Length; index++)
            {
                int source = 0x8c0000 | CinematicSpritemapAddresses.Of(definition.Pose) + 2 + index * 5;
                CompiledSpritePart actual = calculated.Part(index);
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(source), actual.X.Raw, "Native star X/size");
                AssertEqual(rom.ReadCartridgeByte(source + 2), actual.Y.Raw, "Native star Y");
                AssertEqual((ushort)(CartridgeImportSource.Require(rom).ReadCartridgeWord((source + 3)) & ~0x0e00), actual.Attributes.Raw,
                    "Native star attributes excluding replaced source palette");
                for (int field = 0; field < 9; field++)
                {
                    SpriteVisualPart value = parts[index];
                    SpriteVisualPart edit = field switch
                    {
                        0 => value with { OffsetX = value.OffsetX + 1 },
                        1 => value with { OffsetY = value.OffsetY + 1 },
                        2 => value with { TileColumn = value.TileColumn ^ 1 },
                        3 => value with { TileRow = value.TileRow ^ 1 },
                        4 => value with { Size = 16 },
                        5 => value with { Priority = 1 },
                        6 => value with { Palette = 2 },
                        7 => value with { FlipX = true },
                        _ => value with { FlipY = true },
                    };
                    var editedParts = (SpriteVisualPart[])parts.Clone();
                    editedParts[index] = edit;
                    var editedFrames = new Dictionary<string, SpriteVisualPart[]>(frames)
                        { [definition.Name] = editedParts };
                    Check(Load(document with { Frames = editedFrames }), definition.Pose, editedParts);
                }
                total++;
            }
        }
        AssertEqual(29, total, "All four native star sheets checked");
        Console.WriteLine("Zebes stars:29 native parts,261 independent field edits, actual ordered OAM and full content identity pass; only decorative position/glyph triples are retained.");
        static CeresDestructionSpritePresentation Load(CeresDestructionSpriteDocument value)
        {
            using var json = new MemoryStream();
            CeresDestructionSpritePresentation.Write(json, value);
            json.Position = 0;
            var result = CeresDestructionSpritePresentation.Load(json);
            var expected = value.Frames.ToDictionary(pair => CeresDestructionSpriteDefinitions.Frames
                .Single(definition => definition.Name == pair.Key).Pose,
                pair => IntroCinematicSpriteCompiler.Compile(pair.Value, pair.Key));
            AssertEqual(CinematicSpriteArtwork.ContentIdentity(nameof(CeresDestructionSpritePresentation), expected),
                result.ContentIdentity, "Selected star edits retain canonical composition identity");
            return result;
        }
        static void Check(CeresDestructionSpritePresentation catalog, CinematicSpritePose pose, SpriteVisualPart[] parts)
        {
            var expected = IntroCinematicSpriteCompiler.Compile(parts, "star-reference");
            var actualOam = ScreenSpritesFixtures.Blank();
            var expectedOam = new OamBuffer();
            actualOam.BeginFrame(); expectedOam.BeginFrame();
            CinematicSheetFixtures.Drawer(catalog).Draw(pose, actualOam, new(128, 112), 0x0800, true);
            expected.DrawOnScreen(expectedOam, new(128, 112), 0x0800);
            actualOam.FinishFrame(); expectedOam.FinalizeFrame();
            AssertEqual(expectedOam.LastFinalizedSpriteCount, actualOam.LastFinalizedSpriteCount, "Star OAM count/order");
            AssertTrue(expectedOam.LowTable.SequenceEqual(actualOam.LowTable), "Exact star low OAM");
            AssertTrue(expectedOam.HighTable.SequenceEqual(actualOam.HighTable), "Exact star high OAM");
        }
    }
    /// <summary>Checks Tourian statue palette ramps and their CGRAM destinations against native colors.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyLookupStream5StatueRamps(SuperMetroidAddressSpace rom)
    {
        byte[] nativeJson = SuperMetroid.AssetExtraction.TourianStatueColorExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<TourianStatueColorDocument>(nativeJson,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var stock = Check(document);
        foreach (var ramp in new[] { stock.baseColors, stock.statueColors, stock.eyeColors, stock.greyColors }) CheckResidualCount(ramp.edits, 0);
        for (int domain = 0; domain < 4; domain++)
        {
            PaletteRgb5[] source = domain switch
            {
                0 => document.Base, 1 => document.Statue, 2 => document.Eye.SelectMany(row => row).ToArray(), _ => document.Grey,
            };
            for (int color = 0; color < source.Length; color++)
            for (int channel = 0; channel < 3; channel++)
            {
                var changed = (PaletteRgb5[])source.Clone();
                changed[color] = channel switch
                {
                    0 => changed[color] with { Red = changed[color].Red ^ 31 },
                    1 => changed[color] with { Green = changed[color].Green ^ 31 },
                    _ => changed[color] with { Blue = changed[color].Blue ^ 31 },
                };
                Check(domain switch
                {
                    0 => document with { Base = changed },
                    1 => document with { Statue = changed },
                    2 => document with { Eye = Enumerable.Range(0, 4).Select(row => changed.Skip(row * 4).Take(4).ToArray()).ToArray() },
                    _ => document with { Grey = changed },
                });
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveEye(0, 4), "Eye color cannot cross into next row");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveStatue(16), "Statue upper bound");        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBase(-1), "Base ramp lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveGrey(8), "Grey ramp upper bound");
        Console.WriteLine("Tourian statue ramps: all56 native palette colors,168 independent full-range channel edits, zero stock overrides, actual CGRAM and canonical identities pass; selected material paint scope complete.");

        void CheckResidualCount(Dictionary<ColourCell, Bgr555> edits, int expected) =>
            AssertEqual(expected, edits.Count, "Exact stock ramp residual count");

        static ushort Pack(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);

        static TourianStatueColorCatalog Check(TourianStatueColorDocument expected)
        {
            var actual = TourianStatueColorCatalog.Load(new MemoryStream(TourianStatueColorCatalog.Write(expected), writable: false));
            var cgram = new GameplayColours(new SpriteColourSets());
            actual.ApplyEntrance(cgram);
            for (int color = 0; color < expected.Base.Length; color++)
            {
                AssertEqual(Pack(expected.Base[color]), actual.ResolveBase(color), "Exact independent base ramp color");
                AssertEqual(Pack(expected.Base[color]), cgram.Colors[TourianStatuePaletteRomDataConstants.BaseCgramIndex + color], "Actual base CGRAM");
                AssertEqual(Pack(expected.Statue[color]), cgram.Colors[TourianStatuePaletteRomDataConstants.StatueCgramIndex + color], "Unchanged statue CGRAM");
            }
            actual.ApplyGrey(cgram.BackgroundSets.Set(0).Slice(0, expected.Grey.Length));
            for (int color = 0; color < expected.Grey.Length; color++)
            {
                AssertEqual(Pack(expected.Grey[color]), actual.ResolveGrey(color), "Exact independent grey ramp color");
                AssertEqual(Pack(expected.Grey[color]), cgram.Colors[color], "Actual grey CGRAM");
            }
            for (int row = 0; row < expected.Eye.Length; row++)
            {
                actual.ApplyEye(cgram, (ushort)(row * 2));
                for (int color = 0; color < expected.Eye[row].Length; color++)
                    AssertEqual(Pack(expected.Eye[row][color]), cgram.Colors[TourianStatuePaletteRomDataConstants.EyeCgramIndex + color], "Unchanged eye CGRAM");
            }
            string identity = SelectedPresentationHash.Create("TourianStatueColorCatalog-v1", hash =>
            {
                hash.AppendWords("baseColors", expected.Base.Select(Pack).ToArray());
                hash.AppendWords("statueColors", expected.Statue.Select(Pack).ToArray());
                hash.AppendWords("greyColors", expected.Grey.Select(Pack).ToArray());
                hash.AppendWordFrames("eyeColors", expected.Eye.Select(row => row.Select(Pack).ToArray()).ToArray());
            });
            AssertEqual(identity, actual.ContentIdentity, "Exact canonical selected palette identity");
            return actual;
        }
    }

    /// <summary>Compares Phantoon marker addresses and visual selectors with cartridge data.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Phantoon markers")]
    internal static void VerifyLookupStream5PhantoonMarkers(SuperMetroidAddressSpace rom)
    {
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes("Super Metroid.smc"))), "Marker reference ROM revision");
        var enemies = new RoomEnemySystem(new EnemySession(), services: new VerificationRoomEnemyServices(null));
        var body = enemies.SlotTable.Slots[0];
        var state = new PhantoonEnemyState(body)
        {
            Eye = enemies.SlotTable.Slots[1], Tentacles = enemies.SlotTable.Slots[2], Mouth = enemies.SlotTable.Slots[3],
        };
        Action<RoomEnemySlot, PhantoonEnemyState, ushort, int> shot = enemies.PhantoonBehavior.Combat.ResolvePhantoonShotReaction;
        ushort random = 0;
        int calls = 0;
        enemies.VerificationServices().NextRandom = () => { calls++; return random; };
        for (int bucket = 0; bucket < 8; bucket++)
        {
            random = (ushort)(0xfff8 | bucket);
            body.Health = 1000;
            body.AiStates = EnemyAiStates.Hurt;
            state.Parts[body.SlotIndex].Function = PhantoonAiFunction.EyeTracksSamus;
            state.Parts[body.SlotIndex].FunctionTimer = 60;
            state.Parts[state.Tentacles.SlotIndex].SwoopRequested = false;
            state.Parts[state.Tentacles.SlotIndex].RoundDamage = 0;
            byte expected = rom.ReadCartridgeByte(0xa7cda5 + bucket);
            AssertEqual(expected, PhantoonPatternDefinitions.ShotEyeMarkers[bucket], "Exact retained native RNG bucket");
            shot(body, state, 0x100, 1);
            AssertEqual(expected, state.Parts[state.Eye.SlotIndex].ShotMarker, "Actual exposed eye marker");
            AssertEqual((ushort)bucket, state.Parts[state.Mouth.SlotIndex].ShotPattern, "Actual selected RNG pattern");
            AssertEqual(16, state.Parts[body.SlotIndex].FunctionTimer, "Actual shot window");
            AssertEqual(bucket + 1, calls, "One preserved RNG call per shot");
        }
        Console.WriteLine("Phantoon retained random markers: eight SHA-verified native bytes and eight actual bus-free shot writes preserve RNG, pattern and timing.");
    }

    /// <summary>Checks Magdollite's palette pulse frames and the eight-frame update cadence.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyLookupStream5MagdollitePulse(SuperMetroidAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Enemies.MagdollitePaletteCycleExtractor.Extract(rom);
        var native = RequiredJson.Deserialize<MagdollitePaletteCycleDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        MagdollitePaletteCycle stock = Check(native);
        var residuals = stock.edits;
        AssertEqual(0, residuals.Count, "Native Magdollite pulse requires no stored frame residuals");
        for (int frame = 0; frame < native.Frames.Length; frame++)
        for (int color = 0; color < native.Frames[frame].Length; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            var rows = native.Frames.Select(row => (PaletteRgb5[])row.Clone()).ToArray();
            PaletteRgb5 before = rows[frame][color];
            rows[frame][color] = channel switch
            {
                0 => before with { Red = before.Red ^ 1 },
                1 => before with { Green = before.Green ^ 1 },
                _ => before with { Blue = before.Blue ^ 1 },
            };
            Check(native with { Frames = rows });
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ApplyFrame(new SpriteColourSets().Set(0).Slice(0, 4), -1),
            "Magdollite pulse lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ApplyFrame(new SpriteColourSets().Set(0).Slice(0, 4), 4),
            "Magdollite pulse upper frame bound");
        Console.WriteLine("Magdollite pulse:16 native colors,48 independent RGB edits, actual CGRAM, canonical identity and frame bounds pass; four independent glow colors remain pending.");

        static MagdollitePaletteCycle Check(MagdollitePaletteCycleDocument expected)
        {
            var actual = MagdollitePaletteCycle.Load(new MemoryStream(MagdollitePaletteCycle.Write(expected), writable: false));
            var cgram = new GameplayColours(new SpriteColourSets());
            ushort[][] packed = expected.Frames.Select(row => row.Select(rgb =>
                (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10)).ToArray()).ToArray();
            for (int frame = 0; frame < packed.Length; frame++)
            {
                actual.ApplyFrame(cgram.SpriteSets.Set(1).Slice(0, 4), frame);
                for (int color = 0; color < packed[frame].Length; color++)
                    AssertEqual(packed[frame][color],
                        cgram.Colors[144 + color],
                        "Magdollite pulse exact selected CGRAM color");
            }
            string identity = SelectedPresentationHash.Create("MagdollitePaletteCycle-v1",
                content => content.AppendWordFrames("frames", packed));
            AssertEqual(identity, actual.ContentIdentity, "Magdollite pulse canonical identity");
            return actual;
        }
    }
    /// <summary>Checks Zebetite color pulses against the installed frame sequence and CGRAM positions.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyLookupStream5ZebetitePulse(SuperMetroidAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Bosses.MotherBrain.ZebetiteColorExtractor.Extract(rom);
        var native = RequiredJson.Deserialize<ZebetiteColorDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        ZebetiteColorCatalog stock = Check(native);
        var residuals = stock.edits;
        AssertEqual(0, residuals.Count, "Native Zebetite pulse requires no stored frame residuals");
        for (int frame = 0; frame < native.Frames.Length; frame++)
        for (int color = 0; color < native.Frames[frame].Length; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            var rows = native.Frames.Select(row => (PaletteRgb5[])row.Clone()).ToArray();
            PaletteRgb5 before = rows[frame][color];
            rows[frame][color] = channel switch
            {
                0 => before with { Red = before.Red ^ 31 },
                1 => before with { Green = before.Green ^ 31 },
                _ => before with { Blue = before.Blue ^ 31 },
            };
            Check(native with { Frames = rows });
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Apply(new SpriteColourSets().Set(0).Slice(0, 2), -1),
            "Zebetite pulse lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Apply(new SpriteColourSets().Set(0).Slice(0, 2), 8),
            "Zebetite pulse upper frame bound");
        Console.WriteLine("Zebetite pulse:16 native colors,48 independent RGB edits, actual CGRAM, canonical identity and frame bounds pass; selected barrier-core paint disposition complete.");

        static ZebetiteColorCatalog Check(ZebetiteColorDocument expected)
        {
            var actual = ZebetiteColorCatalog.Load(new MemoryStream(ZebetiteColorCatalog.Write(expected), writable: false));
            var cgram = new GameplayColours(new SpriteColourSets());
            ushort[][] packed = expected.Frames.Select(row => row.Select(rgb =>
                (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10)).ToArray()).ToArray();
            for (int frame = 0; frame < packed.Length; frame++)
            {
                actual.Apply(ZebetiteDefinitions.PulseColours.In(cgram), frame);
                for (int color = 0; color < packed[frame].Length; color++)
                    AssertEqual(packed[frame][color],
                        cgram.Colors[ColourSetRunPositions.First(ZebetiteDefinitions.PulseColours) + color],
                        "Zebetite pulse exact selected CGRAM color");
            }
            string identity = SelectedPresentationHash.Create("ZebetiteColorCatalog-v1",
                content => content.AppendWordFrames("frames", packed));
            AssertEqual(identity, actual.ContentIdentity, "Zebetite pulse canonical identity");
            return actual;
        }
    }

    /// <summary>Compares Yapping Maw's animation offsets with native address and displacement tables.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Yapping Maw offsets")]
    internal static void VerifyLookupStream5YappingMawOffsets(SuperMetroidAddressSpace rom)
    {
        Action<RoomEnemySlot,YappingMawEnemyState> begin = YappingMawBehavior.BeginYappingMawExtension;
        Action<YappingMawEnemyState,int> set = YappingMawBehavior.SetYappingMawHeldOffset;
        var slot = new RoomEnemySystem(new EnemySession()).SlotTable.Slots[0];
        var state = new YappingMawEnemyState(slot) { DistanceToSamus = 32 };
        for (int direction = 0; direction < 8; direction++)
        {
            var offset = YappingMawRomData.HeldSamusOffset(direction);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa8a0a7 + direction * 4)), unchecked((ushort)offset.X), "Native held X");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa8a0a9 + direction * 4)), unchecked((ushort)offset.Y), "Native held Y");
            set(state, direction);
            Check(direction, "Actual held-offset callback");
        }
        for (ushort angle = 0; angle < 256; angle++)
        {
            state.AimAngle = new SnesBearing(checked((byte)angle));
            begin(slot, state);
            int direction = unchecked((byte)(angle + 16)) >> 5;
            AssertEqual(direction, state.DirectionIndex,
                "Actual angle quantization/wrap retained");
            Check(direction, "Actual begin-extension held offset");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => YappingMawRomData.HeldSamusOffset(-1), "Held direction lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => YappingMawRomData.HeldSamusOffset(8), "Held direction upper bound");
        Console.WriteLine("Yapping Maw offsets:16 native words, eight actual held callbacks,256 angle-quantized begin-extension writes and direction bounds pass.");

        void Check(int direction, string context)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa8a0a7 + direction * 4)), unchecked((ushort)state.HeldSamusOffset.X), context);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa8a0a9 + direction * 4)), unchecked((ushort)state.HeldSamusOffset.Y), context);
        }
    }

    /// <summary>Checks eye-door geometry and active hitbox dimensions against retail records.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Eye geometry")]
    internal static void VerifyLookupStream5EyeGeometry(SuperMetroidAddressSpace rom)
    {
        for (int direction = 0; direction < 4; direction++)
        {
            var expected = new LookupStream5Expected2(unchecked((short)ReadVerificationWord(rom, 0xa890ca + direction * 2)),
                unchecked((short)ReadVerificationWord(rom, 0xa890d2 + direction * 2)),
                ReadVerificationWord(rom, 0xa890da + direction * 2));
            var mount = MorphBallEyeGeometryDefinitions.Mount(direction);
            AssertEqual(expected, new LookupStream5Expected2(mount.Offset.X, mount.Offset.Y, NativeEnemyProgramOracle.EntryOf(MorphBallEyeNativeListAddresses.Entries, mount.Program)),
                "Native directional mount tuple");
            foreach (ushort origin in new ushort[] { 0, 65535 })
            {
                var enemies = new RoomEnemySystem(new EnemySession());
                var slot = enemies.SlotTable.Slots[0];
                slot.Bind(EnemyDefinitionId.Eye, slot.Definition);
                slot.SetSpawnParameter2((ushort)(0x8000 | direction));
                slot.Position = slot.Position.WithWhole(new(origin, origin));
                new Action<RoomEnemySlot>(enemies.MorphBallEyeBehavior.InitializeMorphBallEye)(slot);
                AssertEqual(unchecked((ushort)(origin + expected.First)), slot.Position.X.Whole, "Actual mount X with native wrap");
                AssertEqual(unchecked((ushort)(origin + expected.Second)), slot.Position.Y.Whole, "Actual mount Y with native wrap");
                MorphBallEyeEnemyState eye = enemies.MorphBallEyeBehavior.MorphBallEyeStates[0] ?? throw new InvalidOperationException("The mount has no state.");
                AssertEqual(AnimationProgramCursor<MorphBallEyeProgram>.Start(mount.Program),
                    eye.Program,
                    "Actual directional mount program");
                AssertEqual(MorphBallEyeAiFunction.MountNoOp, eye.Function, "Mount remains decorative");
            }
        }
        var owner = new RoomEnemySystem(new EnemySession());
        var body = owner.SlotTable.Slots[1];
        body.Bind(EnemyDefinitionId.Eye, body.Definition);
        new Action<RoomEnemySlot>(owner.MorphBallEyeBehavior.InitializeMorphBallEye)(body);
        (owner.MorphBallEyeBehavior.MorphBallEyeStates[1] ?? throw new InvalidOperationException("Verification requires owner.MorphBallEyeBehavior.MorphBallEyeStates[1].")).IsActivated = true;
        owner.MorphBallEyeBehavior.MorphBallEyeBeam.BodySlotIndex = 1;
        owner.MorphBallEyeBehavior.MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Full;
        owner.MorphBallEyeBehavior.MorphBallEyeBeam.ColorIndex = 0xfff0;
        Action step = owner.MorphBallEyeBehavior.StepFullMorphBallEyeBeam;
        for (int phase = 0; phase < 16; phase++)
        {
            var expected = new RedGreenLevels(rom.ReadCartridgeByte(0x88ea8b + phase * 4), rom.ReadCartridgeByte(0x88ea8c + phase * 4));
            AssertEqual(expected, MorphBallEyeGeometryDefinitions.BeamColor(phase), "Native two-channel triangle");
            step();
            AssertEqual(expected.Red, owner.MorphBallEyeBehavior.MorphBallEyeBeam.Red, "Actual raw red COLDATA");
            AssertEqual(expected.Green, owner.MorphBallEyeBehavior.MorphBallEyeBeam.Green, "Actual raw green COLDATA");
            AssertEqual(rom.ReadCartridgeByte(0x88ea8d + phase * 4), owner.MorphBallEyeBehavior.MorphBallEyeBeam.Blue, "Actual zero blue intensity");
            AssertEqual((ushort)((phase + 1) & 15), owner.MorphBallEyeBehavior.MorphBallEyeBeam.ColorIndex, "Actual masked cycling and wrap");
        }
        AssertThrows<InvalidDataException>(() => MorphBallEyeGeometryDefinitions.Mount(4), "Unsupported mount direction");
        AssertThrows<ArgumentOutOfRangeException>(() => MorphBallEyeGeometryDefinitions.BeamColor(16), "Beam phase bound");
        Console.WriteLine("Morph Ball eye: four native mount tuples, eight actual wrapped initializers,16 native beam colors and actual full-beam cycle pass.");
    }

    /// <summary>Checks the map highlight palette cycle against cartridge colors and installed CGRAM slots.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for map highlight")]
    internal static void VerifyLookupStream5MapHighlight(SuperMetroidAddressSpace rom)
    {
        var frames = Enumerable.Range(0, 14).Select(frame => new MapPaletteCycleFrame
        {
            DurationTicks = rom.ReadCartridgeByte(MapAnimationRomData.PaletteTiming + frame * 3),
            Colors = Enumerable.Range(0, 16).Select(color =>
            {
                ushort word = ReadVerificationWord(rom, MapAnimationRomData.PaletteColors + (frame * 16 + color) * 2);
                return new PaletteRgb5 { Red = word & 31, Green = (word >> 5) & 31, Blue = (word >> 10) & 31 };
            }).ToArray(),
        }).ToArray();
        var original = new MapPaletteCycleDocument { Version = 1, Frames = frames };
        MapPaletteCycle stock = Check(original);
        AssertEqual(0, stock.durationEdits.Count, "No stock duration lookup retained");
        AssertEqual(0, stock.reverseColorEdits.Count, "No stock reverse-frame lookup retained");
        AssertEqual(8, stock.colors.Length, "Only independent color phases remain");
        for (int frame = 0; frame < 14; frame++)
        {
            AssertEqual((byte)Math.Min(frame, 14 - frame), rom.ReadCartridgeByte(MapAnimationRomData.PaletteTiming + frame * 3 + 1), "Native highlight phase traversal");
            var durationEdit = (MapPaletteCycleFrame[])frames.Clone();
            durationEdit[frame] = frames[frame] with { DurationTicks = 254 };
            Check(original with { Frames = durationEdit });
            for (int color = 0; color < 16; color++)
            {
                var changed = (MapPaletteCycleFrame[])frames.Clone();
                var changedColors = (PaletteRgb5[])frames[frame].Colors.Clone();
                changedColors[color] = changedColors[color] with { Red = changedColors[color].Red ^ 1 };
                changed[frame] = frames[frame] with { Colors = changedColors };
                Check(original with { Frames = changed });
            }
        }
        foreach (int count in new[] { 1, 15, 255 })
            Check(original with { Frames = Enumerable.Range(0, count).Select(i => frames[i % 14]).ToArray() });
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Duration(-1), "Highlight lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Apply(new ScreenPalette().SpriteRow(MapScreenDefinitions.HighlightSpriteRow), 14), "Highlight upper frame bound");
        Console.WriteLine("Map highlight: 224 native colors,14 timing/phase records, all independent color/hold edits and custom frame counts pass actual CGRAM application; eight color seed rows remain pending.");

        static MapPaletteCycle Check(MapPaletteCycleDocument document)
        {
            byte[] json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
            var cycle = MapPaletteCycle.Load(new MemoryStream(json, writable: false));
            AssertEqual(document.Frames.Length, cycle.FrameCount, "Custom highlight frame count");
            var cgram = new ScreenPalette();
            for (int frame = 0; frame < cycle.FrameCount; frame++)
            {
                AssertEqual((byte)document.Frames[frame].DurationTicks, cycle.Duration(frame), "Independent highlight hold");
                cycle.Apply(cgram.SpriteRow(MapScreenDefinitions.HighlightSpriteRow), frame);
                for (int color = 0; color < 16; color++)
                {
                    var rgb = document.Frames[frame].Colors[color];
                    AssertEqual((ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10),
                        cgram.Colors[ScreenColourPositions.Sprite(MapScreenDefinitions.HighlightSpriteRow) + color], "Independent highlight CGRAM color");
                }
            }
            return cycle;
        }
    }

    /// <summary>Compares Sidehopper motion and corpse geometry operands across the native unrolled columns.</summary>
    internal static void VerifyLookupStream5SidehopperGeometry(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => ReadVerificationWord(rom, address);
        foreach (ushort variant in new ushort[] { 0, 2, ushort.MaxValue })
        {
            bool alternate = variant != 0;
            AssertTrue(LookupStream5CorpseFixtureArtwork(rom).TryResolve(CorpseSheetDefinitions.DeadMonsterSheetAddress,
                CorpseSheetDefinitions.DeadMonsterSheetByteCount, out ReadOnlyMemory<byte> atlas), "Sidehopper corpse fixture art resolves its sheet");
            byte[] expected = new byte[0x800];
            for (int row = 0; row < 5; row++)
            {
                int native = (alternate ? 0xa9df08 : 0xa9dec1) + 14 * row;
                int source = Word(native + 2) - 0xc000;
                int destination = Word(native + 5) - 0x2000;
                int length = Word(native + 8) + 1;
                AssertEqual(new CorpseGraphicsCopy(source, destination, length),
                    CorpseStagingGeometryDefinitions.SidehopperInitialCopy(variant, row), "Native Sidehopper MVN geometry");
                ReadSidehopperCorpseRomBytes(rom, 0xb7c000 + source, length).CopyTo(expected, destination);
            }
            // The extraction stages the corpse as the cartridge does and rots it to picture each step (#1392).
            CorpseStaging corpseTiles = CorpseStaging.Sidehopper(atlas.Span, variant);
            for (int row = 0; row < 5; row++)
            {
                CorpseGraphicsCopy copied = CorpseStagingGeometryDefinitions.SidehopperInitialCopy(variant, row);
                byte[] staged = Staged(corpseTiles, copied.DestinationOffset, copied.Length);
                for (int i = 0; i < copied.Length; i++)
                    AssertEqual(expected[copied.DestinationOffset + i], staged[i], "Actual installed Sidehopper graphics copy");
            }

            // Native LDA abs,X operands and optional CMP #8 operands, independently read from each unrolled column.
            int[] sourceOperands = alternate
                ? [0xa9e56d, 0xa9e587, 0xa9e5a6, 0xa9e5c5, 0xa9e5e4]
                : [0xa9e476, 0xa9e495, 0xa9e4af, 0xa9e4c9, 0xa9e4e3];
            int[] minimumOperands = alternate ? [0, 0, 0xa9e59c, 0xa9e5bb, 0xa9e5da]
                : [0xa9e46c, 0xa9e48b, 0, 0, 0];
            for (int column = 0; column < 5; column++)
            {
                AssertEqual((Word(sourceOperands[column]) - 0x2000) / 2,
                    CorpseStagingGeometryDefinitions.SidehopperColumnWordOffset(variant, column), "Native tile column word offset");
                AssertEqual(minimumOperands[column] == 0 ? 0 : Word(minimumOperands[column]),
                    CorpseStagingGeometryDefinitions.SidehopperColumnMinimumY(variant, column), "Native missing top-row clipping");
            }
            CorpseRowGeometry geometry = CorpseRowGeometry.Sidehopper(variant, wrap: 148);
            foreach (ushort y in new ushort[] { 0, 7, 8, 37, 38 })
            foreach (bool move in new[] { false, true })
            {
                for (int i = 0; i < expected.Length; i++)
                    expected[i] = (byte)(i * 37 + i / 256);
                corpseTiles.Copy(expected, new CorpseGraphicsCopy(0, 0, expected.Length));
                int sourceRow = Word(0xa9e240 + (y >> 3) * 2) + (y & 7) * 2;
                int destinationRow = sourceRow + ((y & 7) >= 6 ? 148 : 0);
                for (int column = 0; column < 5; column++)
                {
                    int minimum = minimumOperands[column] == 0 ? 0 : Word(minimumOperands[column]);
                    if (y < minimum) continue;
                    int columnBase = Word(sourceOperands[column]) - 0x2000;
                    foreach (int plane in new[] { 0, 16 })
                    {
                        int source = sourceRow + columnBase + plane;
                        int destination = destinationRow + columnBase + plane + 2;
                        if (y < 38)
                        {
                            expected[destination] = expected[source];
                            expected[destination + 1] = expected[source + 1];
                        }
                        if (move) { expected[source] = 0; expected[source + 1] = 0; }
                    }
                }
                corpseTiles.MoveRow(geometry, y, move ? CorpseRowOperation.Move : CorpseRowOperation.Copy);
                byte[] moved = Staged(corpseTiles, 0, expected.Length);
                for (int i = 0; i < expected.Length; i++)
                    AssertEqual(expected[i], moved[i], "Actual Sidehopper pixel copy/move including clipping, wrap and terminal row");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => CorpseStagingGeometryDefinitions.SidehopperInitialCopy(0, -1), "Copy row lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => CorpseStagingGeometryDefinitions.SidehopperInitialCopy(0, 5), "Copy row upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => CorpseStagingGeometryDefinitions.SidehopperColumnWordOffset(0, 5), "Column upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => CorpseStagingGeometryDefinitions.SidehopperColumnMinimumY(0, -1), "Column lower bound");
        Console.WriteLine("Sidehopper geometry: ten native MVN rows, ten column offsets/clips, installed artwork and actual copy/move boundary rows pass; nonzero variant semantics preserved.");

        static byte[] Staged(CorpseStaging staging, int offset, int length) =>
            staging.Upload(new CorpseRotRow(0, EnemyStagingDefinitions.Bg2NativeAddress + offset, length, 0));
    }

    /// <summary>Reads the retail Sidehopper corpse character rows for an independent artwork fixture.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Sidehopper corpse artwork rows")]
    private static byte[] ReadSidehopperCorpseRomBytes(ISnesAddressSpace rom, int address, int count) =>
        Enumerable.Range(0, count).Select(offset => rom.ReadCartridgeByte(address + offset)).ToArray();

    /// <summary>Builds a corpse-art fixture with cartridge Sidehopper palette and tile bytes.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the dead Tourian corpse fixture artwork")]
    internal static EnemyTileArtworkCatalog LookupStream5CorpseFixtureArtwork(SuperMetroidAddressSpace rom)
    {
        byte[] planar = Enumerable.Range(0,CorpseSheetDefinitions.DeadMonsterSheetByteCount)
            .Select(index => rom.ReadCartridgeByte(CorpseSheetDefinitions.DeadMonsterSheetAddress+index)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar,4,RoomCharacterAtlasFormat.TileColumns,out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png,size,pixels,SnesGraphics.DiagnosticPalette(16));
        png.Position=0;
        RoomCharacterAtlas atlas = RoomCharacterAtlas.Load(png,planar.Length);
        return EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas> { [EnemyDefinitionId.CorpseSidehopper] = atlas }, Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>
            {
                [EnemyDefinitionId.CorpseSidehopper] = EnemyPaletteSheet.Load(new MemoryStream(
                    EnemyPaletteSheet.Write(new EnemyPaletteSheetDocument
                    {
                        Version=1,
                        Colors=Enumerable.Range(0,16).Select(_ => new PaletteRgb5 { Red=0,Green=0,Blue=0 }).ToArray(),
                    }))),
            }, DmaSources = new Dictionary<EnemyDefinitionId, int> { [EnemyDefinitionId.CorpseSidehopper] = CorpseSheetDefinitions.DeadMonsterSheetAddress } });
    }

    /// <summary>Checks Phantoon rain sprites and their room-state timing against retail sources.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Phantoon rain")]
    internal static void VerifyLookupStream5PhantoonRain(SuperMetroidAddressSpace rom)
    {
        for (int i = 0; i < 8; i++)
        {
            var placement = PhantoonPatternDefinitions.RainPlacement(i);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdad + i * 8)), placement.Cursor, "Native rain figure-eight cursor");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdaf + i * 8)), placement.Position.X, "Native rain body X");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdb1 + i * 8)), placement.Position.Y, "Native rain body Y");
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdb3 + i * 8)), "Native unused rain record word");
            AssertEqual(rom.ReadCartridgeByte(0xa7cfc2 + i), PhantoonPatternDefinitions.FirstRainColumns[i], "Native first rain column");
        }
        for (int high = 0; high < 1; high++)
        for (int pattern = 0; pattern < 8; pattern++)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            ushort random = SnesLittleEndian.Word(unchecked((byte)(0xf8 | pattern)), unchecked((byte)high));
            int calls = 0;
            enemies.VerificationServices().NextRandom = () => { calls++; return random; };
            var body = enemies.SlotTable.Slots[0];
            var eye = enemies.SlotTable.Slots[1];
            var state = new PhantoonEnemyState(body) { Eye = eye };
            enemies.PhantoonBehavior.Combat.Phantoon = state;
            state.Parts[body.SlotIndex].FunctionTimer = 1;
            state.Parts[eye.SlotIndex].FigureEightReversed = true;
            new Action<RoomEnemySlot, PhantoonEnemyState>(enemies.PhantoonBehavior.Patterns.RunPhantoonHiddenFlameRain)(body, state);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdad + pattern * 8)), state.Parts[body.SlotIndex].PathCursor, "Real rain cursor handoff");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdaf + pattern * 8)), body.Position.X.Whole, "Real rain body placement X");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdb1 + pattern * 8)), body.Position.Y.Whole, "Real rain body placement Y");
            AssertEqual(false, state.Parts[eye.SlotIndex].FigureEightReversed, "Real rain direction reset");
            AssertEqual(PhantoonAiFunction.BecomeSolidAfterFlameRain, state.Parts[body.SlotIndex].Function, "Real rain phase handoff");
            AssertEqual(1, calls, "Rain consumes one RNG word");
            var flames = enemies.Projectiles.EnemyProjectiles.Where(p => p.IsActive).OrderBy(p => p.Velocity.X.RawValue).ToArray();
            AssertEqual(8, flames.Length, "Actual rain population");
            AssertTrue(flames.All(flame => flame.Position.X.Whole != body.Position.X.Whole), "Calculated rain gap lies at the body X");
            for (int i = 0; i < 8; i++)
            {
                int column = (rom.ReadCartridgeByte(0xa7cfc2 + pattern) + i) % 9;
                AssertEqual(rom.ReadCartridgeByte(0x8698f7 + column), flames[i].Position.X.Whole, "Actual rain column order with wrap");
                AssertEqual((ushort)40, flames[i].Position.Y.Whole, "Actual rain ceiling Y");
                AssertEqual((ushort)((i + 1) * 8), flames[i].Velocity.X.RawValue, "Actual staggered rain delay");
            }
        }

        EyeTarget[] eyeTargets =
        [
            new EyeTarget(0, -100, 0), new EyeTarget(100, -100, 1), new EyeTarget(100, 0, 2), new EyeTarget(100, 100, 3),
            new EyeTarget(0, 100, 4), new EyeTarget(-100, 100, 6), new EyeTarget(-100, 0, 7), new EyeTarget(-100, -100, 8),
        ];
        for (ushort direction = 0; direction < 9; direction++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7d40d + direction * 2)),
                PhantoonNativeListAddresses.PartEntry(PhantoonPatternDefinitions.EyeProgram(direction)),
                $"native Phantoon eye direction {direction}");
        }
        var eyeEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot, PhantoonEnemyState, SamusState> pointEye = PhantoonPatterns.PointPhantoonEyeAtSamus;
        RoomEnemySlot eyeBody = eyeEnemies.SlotTable.Slots[0];
        RoomEnemySlot trackingEye = eyeEnemies.SlotTable.Slots[1];
        var eyeState = new PhantoonEnemyState(eyeBody) { Eye = trackingEye, Tentacles = eyeEnemies.SlotTable.Slots[2], Mouth = eyeEnemies.SlotTable.Slots[3] };
        eyeBody.Position = eyeBody.Position.WithWhole(new(0x4000, 0x4000));
        foreach ((short x, short y, ushort direction) in eyeTargets)
        {
            pointEye(eyeBody, eyeState, new SamusState
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(eyeBody.Position.X.Whole + x)), unchecked((ushort)(eyeBody.Position.Y.Whole + y)))),
            });
            AssertEqual(PhantoonPatternDefinitions.EyeProgram(direction),
                eyeState.PartProgram(trackingEye).Program,
                $"production Phantoon eye direction {direction}");
        }
        AssertThrows<InvalidDataException>(
            () => PhantoonPatternDefinitions.EyeProgram(9),
            "Phantoon eye direction beyond authored table");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = PhantoonPatternDefinitions.FirstRainColumns[-1], "rain pattern lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = PhantoonPatternDefinitions.FirstRainColumns[8], "rain pattern upper bound");
        Console.WriteLine("Stream 5 Phantoon: eight native rain columns, eight actual rain populations/gaps and nine eye selectors/eight actual octants pass with native tables forbidden; shot markers have a separately reviewed random-bucket exception.");
    }

    /// <summary>Checks Ceres door alarm ramp colors and their physical CGRAM slots.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres door ramp")]
    internal static void VerifyLookupStream5CeresDoorRamp(SuperMetroidAddressSpace rom)
    {
        var document = new CeresDoorVisualDocument
        {
            Version = 1,
            Normal = Colors(CeresDoorVisualRomData.NormalColors,15),
            Escape = Colors(CeresDoorVisualRomData.EscapeColors,15),
            Animation = Enumerable.Range(0,8).Select(row => Colors(CeresDoorVisualRomData.AnimationColors +16*row,6)).ToArray(),
            Mode7DoorFrames = Enumerable.Range(0,2).Select(frame => Enumerable.Range(0,4)
                .Select(index => (int)rom.ReadCartridgeByte(CeresDoorVisualRomData.Mode7FirstFrameSource +4*frame+index)).ToArray()).ToArray(),
        };
        byte[] planar = Enumerable.Range(0,CeresDoorVisualRomData.TileByteCount)
            .Select(index => rom.ReadCartridgeByte(CeresDoorVisualRomData.TileSource+index)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar,4,RoomCharacterAtlasFormat.TileColumns,out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png,size,pixels,SnesGraphics.DiagnosticPalette(16));
        byte[] pngBytes = png.ToArray();
        CeresDoorVisualCatalog Load(CeresDoorVisualDocument value) => CeresDoorVisualCatalog.Load(
            new MemoryStream(pngBytes),new MemoryStream(CeresDoorVisualCatalog.Write(value)));
        var stock = Load(document);
        Check(stock,document);
        var animation = stock.animation;
        var escape = stock.escape;
        AssertEqual(0, animation.seedEdits.Count,
            "stock animation seeds derive from the normal paint");
        AssertEqual(0, animation.edits.Count,
            "stock animation phases require no cell storage");
        AssertEqual(0, escape.edits.Count,
            "stock escape colors derive from the normal paint");
        for (int palette = 0; palette < 2; palette++)
        for (int color = 0; color < 15; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            var colors = (PaletteRgb5[])(palette == 0 ? document.Normal : document.Escape).Clone();
            PaletteRgb5 before = colors[color];
            colors[color] = channel switch
            {
                0 => before with { Red = before.Red ^ 1 },
                1 => before with { Green = before.Green ^ 1 },
                _ => before with { Blue = before.Blue ^ 1 },
            };
            var changed = palette == 0 ? document with { Normal = colors } : document with { Escape = colors };
            Check(Load(changed), changed);
        }
        for (int edited = 0; edited <48; edited++)
        {
            var rows = document.Animation.Select(row => row.ToArray()).ToArray();
            var original = rows[edited/6][edited%6];
            rows[edited/6][edited%6] = new PaletteRgb5 { Red = original.Red ^1, Green = original.Green, Blue = original.Blue };
            var changed = document with { Animation = rows };
            Check(Load(changed),changed);
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.LoadAnimationColors(new BackgroundColourSets(),-1),"Ceres animation lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.LoadAnimationColors(new BackgroundColourSets(),8),"Ceres animation upper bound");
        Console.WriteLine("Stream 5 Ceres door ramp:48 native colors,48 independently edited cells, actual CGRAM,90 independent setup-channel edits, six shared setup slots and canonical identities pass; stock seeds, phases and escape colors store no edits.");

        PaletteRgb5[] Colors(int source,int count) => Enumerable.Range(0,count).Select(index =>
        {
            ushort word = ReadVerificationWord(rom,source+2*index);
            return new PaletteRgb5 { Red = word &31, Green = (word>>5)&31, Blue = (word>>10)&31 };
        }).ToArray();
        static ushort Pack(PaletteRgb5 color) => (ushort)(color.Red | color.Green<<5 | color.Blue<<10);
        void Check(CeresDoorVisualCatalog actual,CeresDoorVisualDocument expected)
        {
            var cgram = new GameplayColours(new SpriteColourSets());
            for(int row =0;row<8;row++)
            {
                actual.LoadAnimationColors(cgram.BackgroundSets,row);
                for(int color=0;color<6;color++) AssertEqual(Pack(expected.Animation[row][color]),cgram.Colors[CeresDoorVisualRomDataConstants.AnimationTargetColor+color],"Ceres independent animation color");
            }
            actual.LoadNormalColors(cgram.BackgroundSets.All.Slice(0, 15));
            for(int color=0;color<15;color++) AssertEqual(Pack(expected.Normal[color]),cgram.Colors[color],"Ceres normal palette unchanged");
            actual.LoadEscapeColors(cgram.BackgroundSets.All.Slice(0, 15));
            for(int color=0;color<15;color++) AssertEqual(Pack(expected.Escape[color]),cgram.Colors[color],"Ceres escape palette unchanged");
            string identity = SelectedPresentationHash.Create("enemy-ceres-door-v1",content =>
            {
                content.Append("tiles",planar);
                content.AppendWords("normal",expected.Normal.Select(Pack).ToArray());
                content.AppendWords("escape",expected.Escape.Select(Pack).ToArray());
                content.AppendWordFrames("animation",expected.Animation.Select(row=>row.Select(Pack).ToArray()).ToArray());
                content.Append("mode7-frames",expected.Mode7DoorFrames.Length);
                foreach(var frame in expected.Mode7DoorFrames) content.Append("mode7-frame",frame.Select(value=>(byte)value).ToArray());
            });
            AssertEqual(identity,actual.ContentIdentity,"Ceres calculated colors preserve canonical resource identity");
        }
    }
    /// <summary>Checks the extraction's corpse row upload lists and the corpse geometry bounds.</summary>
    internal static void VerifyLookupStream5CorpseViews()
    {
        var expected = new List<CorpseRowUpload>();
        foreach (ushort table in new ushort[] { 0xe0e0, 0xe10a, 0xe134, 0xe146, 0xe158, 0xe16a, 0xe17c, 0xe18e, 0xe1b0, 0xe1d2 })
        {
            var rows = DeadMonsterRowUploadDefinitions.ForKind(CorpseNativeAddresses.TransferTableAt(table));
            int index = 0;
            foreach (var row in rows)
            {
                AssertEqual(rows[index++], row, "corpse DMA enumeration retains indexed order");
                expected.Add(row);
            }
            AssertEqual(rows.Length, index, "corpse DMA enumeration count");
            AssertThrows<IndexOutOfRangeException>(() => { _ = rows[-1]; }, "corpse DMA negative index");
            AssertThrows<IndexOutOfRangeException>(() => { _ = rows[rows.Length]; }, "corpse DMA past end");
        }
        AssertTrue(expected.SequenceEqual(CorpseNativeAddresses.AllTransferTables.SelectMany(kind => DeadMonsterRowUploadDefinitions.ForKind(kind))),
            "every kind's row uploads, in native list order");
        AssertThrows<InvalidDataException>(() => CorpseNativeAddresses.TransferTableAt(0xe0e1), "unaligned row upload list rejected");
        AssertThrows<InvalidDataException>(() => DeadMonsterRowUploadDefinitions.ForKind(DeadTorizoCorpseDefinitions.Kind), "the Torizo corpse uploads by halves");
        AssertThrows<InvalidDataException>(() => SuperMetroid.Verification.Enemies.Corpses.CorpseRowLayouts.RotationOffset(0xe227, 0), "unknown corpse row layout rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => SuperMetroid.Verification.Enemies.Corpses.CorpseRowLayouts.RotationOffset(0xe226, 104), "corpse row past end rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => CorpseStagingGeometryDefinitions.SandSource(16), "sand source past end rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => CorpseStagingGeometryDefinitions.SandDestination(16), "sand destination past end rejected");
    }
    /// <summary>Compares compiled enemy actor layout arrays with their cartridge source bytes.</summary>
    internal static void VerifyLookupStream5ActorLayouts(ISnesAddressSpace rom)
    {
        var flight = Enumerable.Range(0, 5).Select(index =>
        {
            var source = CeresFlightActorDefinitions.RearViewPlacementSource(index);
            return new CeresFlightActorPlacement { Id = source.Id,
                X = ReadVerificationWord(rom, 0x8b0000 | source.XAddress),
                Y = ReadVerificationWord(rom, 0x8b0000 | source.YAddress) };
        }).ToArray();
        Check(flight, values =>
        {
            using var encoded = new MemoryStream();
            CeresFlightActorLayout.Write(encoded, new() { Version = 1, Actors = values });
            encoded.Position = 0;
            var layout = CeresFlightActorLayout.Load(encoded);
            return new LookupStream5Load<CeresFlightActorPlacement>(layout, layout.placements, index => layout[index], layout.ContentIdentity);
        }, value => new LookupStream5Field(value.Id, value.X, value.Y),
            (value, x) => x ? value with { X = unchecked((ushort)(value.X + 1)) } : value with { Y = unchecked((ushort)(value.Y + 1)) });
        var reveal = Enumerable.Range(0, 6).Select(index =>
        {
            var source = CeresDestructionActorDefinitions.ZebesPlacementSource(index);
            return new CeresRevealActorPlacement { Id = source.Id,
                X = ReadVerificationWord(rom, 0x8b0000 | source.XAddress),
                Y = ReadVerificationWord(rom, 0x8b0000 | source.YAddress) };
        }).ToArray();
        Check(reveal, values =>
        {
            using var encoded = new MemoryStream();
            CeresRevealActorLayout.Write(encoded, new() { Version = 1, Actors = values });
            encoded.Position = 0;
            var layout = CeresRevealActorLayout.Load(encoded);
            return new LookupStream5Load<CeresRevealActorPlacement>(layout, layout.placements, index => layout[index], layout.ContentIdentity);
        }, value => new LookupStream5Field(value.Id, value.X, value.Y),
            (value, x) => x ? value with { X = unchecked((ushort)(value.X + 1)) } : value with { Y = unchecked((ushort)(value.Y + 1)) });
        var destruction = Enumerable.Range(0, 3).Select(index =>
        {
            var inherited = flight[index == 0 ? 0 : index == 1 ? 2 : 3];
            return new CeresDestructionActorPlacement { Id = CeresDestructionActorDefinitions.InitialPlacementId(index),
                X = index == 2 ? ReadVerificationWord(rom, 0x8bbfa6) : inherited.X, Y = inherited.Y };
        }).ToArray();
        Check(destruction, values =>
        {
            using var encoded = new MemoryStream();
            CeresDestructionActorLayout.Write(encoded, new() { Version = 1, Actors = values });
            encoded.Position = 0;
            var layout = CeresDestructionActorLayout.Load(encoded);
            return new LookupStream5Load<CeresDestructionActorPlacement>(layout, layout.placements, index => layout[index], layout.ContentIdentity);
        }, value => new LookupStream5Field(value.Id, value.X, value.Y),
            (value, x) => x ? value with { X = unchecked((ushort)(value.X + 1)) } : value with { Y = unchecked((ushort)(value.Y + 1)) });
        Console.WriteLine("Ceres layouts: all14 original actor placements/28 coordinate operands, no stored stock rows, all28 independent coordinate edits, complete identity preservation and bounds pass.");

        static void Check<T>(T[] original,
            Func<T[], LookupStream5Load<T>> load,
            Func<T, LookupStream5Field> fields, Func<T, bool, T> edit)
        {
            var stock = load(original);
            AssertTrue(stock.Placements is null, "Ceres stock placements select semantic actor dispatch");
            Verify(stock, original);
            for (int index = 0; index < original.Length; index++)
            foreach (bool horizontal in new[] { false, true })
            {
                T[] changed = original.ToArray();
                changed[index] = edit(changed[index], horizontal);
                var installed = load(changed);
                AssertTrue(installed.Placements is not null, "Independent Ceres coordinate edit remains supplied");
                Verify(installed, changed);
            }
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Read(-1), "Ceres layout lower bound");
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Read(original.Length), "Ceres layout upper bound");

            void Verify(LookupStream5Load<T> actual, T[] expected)
            {
                for (int index = 0; index < expected.Length; index++)
                    AssertEqual(fields(expected[index]), fields(actual.Read(index)), "Ceres complete selected placement");
                string identity = SelectedPresentationHash.Create(actual.Layout.GetType().Name, content =>
                {
                    content.Append("actors", expected.Length);
                    foreach (T placement in expected)
                    {
                        var value = fields(placement);
                        content.Append("id", System.Text.Encoding.UTF8.GetBytes(value.Id));
                        content.Append("x", value.X); content.Append("y", value.Y);
                    }
                });
                AssertEqual(identity, actual.Identity, "Ceres canonical selected content identity");
            }
        }
    }
    /// <summary>Checks compiled lookup initialization words against banked cartridge values.</summary>
    internal static void VerifyLookupStream5Initialization(ISnesAddressSpace rom)
    {
        for (ushort variant = 0; variant < 7; variant++)
        {
            var actual = CeresDoorInitializationDefinitions.For(variant);
            AssertEqual(ReadVerificationWord(rom, 0xa6f52c + 2 * variant), NativeEnemyProgramOracle.EntryOf(CeresDoorNativeListAddresses.Entries, actual.Program), "Ceres door original instruction dispatch");
            AssertEqual(ReadVerificationWord(rom, 0xa6f72b + 2 * variant), (ushort)actual.MainFunction, "Ceres door original function dispatch");
        }
        for (ushort variant = 0; variant < 6; variant++)
        {
            var actual = CeresSteamDefinitions.Initialization((CeresSteamVariant)variant);
            AssertEqual(ReadVerificationWord(rom, 0xa6eff5 + 2 * variant), NativeEnemyProgramOracle.EntryOf(CeresSteamNativeListAddresses.Entries, actual.Program), "Ceres steam original instruction dispatch");
            AssertEqual(ReadVerificationWord(rom, 0xa6f001 + 2 * variant), (ushort)actual.Function, "Ceres steam original function dispatch");
        }
        for (int index = 0; index < 9; index++)
        {
            var actual = MagdollitePhaseDefinitions.Phase(index);
            AssertEqual(ReadVerificationWord(rom, 0xa8af55 + 2 * index), actual.DistanceThreshold, "Magdollite original rise threshold");
            AssertEqual(ReadVerificationWord(rom, 0xa8af67 + 2 * index), NativeEnemyProgramOracle.EntryOf(MagdolliteNativeListAddresses.Entries, actual.BodyProgram), "Magdollite original body program");
            AssertEqual(ReadVerificationWord(rom, 0xa8af79 + 2 * index), actual.OverlayYOffset, "Magdollite original overlay offset");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => CeresDoorInitializationDefinitions.For(7), "Ceres door upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => CeresDoorInitializationDefinitions.For(ushort.MaxValue), "Ceres door full-word rejection");
        AssertThrows<InvalidDataException>(() => CeresSteamDefinitions.Initialization((CeresSteamVariant)6), "Ceres steam upper bound");
        AssertThrows<InvalidDataException>(() => CeresSteamDefinitions.Initialization((CeresSteamVariant)ushort.MaxValue), "Ceres steam full-word rejection");
        AssertThrows<InvalidDataException>(() => MagdollitePhaseDefinitions.Phase(-1), "Magdollite lower bound");
        AssertThrows<InvalidDataException>(() => MagdollitePhaseDefinitions.Phase(9), "Magdollite upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = CeresEscapeVramTransferDefinitions.All[-1]; }, "Ceres DMA list lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => { _ = CeresEscapeVramTransferDefinitions.All[19]; }, "Ceres DMA list upper bound");
        Console.WriteLine("Stream 5 initialization: all 53 native selector/phase words and rejected domains pass.");
    }
    /// <summary>Checks Ceres light and other palette entry identities against native definition/program pairs.</summary>
    internal static void VerifyLookupStream5PaletteEntries(ISnesAddressSpace rom)
    {
        // Ceres light entries carry no identity; their native definition/program pairs are
        // the compiled owner constants, checked against the original definition list.
        CheckEntryCount(CeresCinematicLightPaletteFxColorDefinitionsTooling.All, 3);
        foreach ((ushort definition, ushort program) in new[]
        {
            new VerifyLookupStream5PaletteEntriesCase(CeresCinematicLightPaletteFxColorDefinitions.GunshipEngineDefinitionPointer,
                CeresCinematicLightPaletteFxColorDefinitionsConstants.GunshipEngineProgramStart),
            new VerifyLookupStream5PaletteEntriesCase(CeresCinematicLightPaletteFxColorDefinitions.SpriteNavigationLightsDefinitionPointer,
                CeresCinematicLightPaletteFxColorDefinitionsConstants.SpriteNavigationLightsProgramStart),
            new VerifyLookupStream5PaletteEntriesCase(CeresCinematicLightPaletteFxColorDefinitions.BackgroundNavigationLightsDefinitionPointer,
                CeresCinematicLightPaletteFxColorDefinitionsConstants.BackgroundNavigationLightsProgramStart),
        })
        {
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | definition + 2), program,
                "Original palette definition list pointer");
        }
        CheckEntries(CinematicGlowPaletteFxColorDefinitions.All,
            entry => new Project(entry.DefinitionPointer, entry.ProgramStart), 2);
        CheckEntries(TourianEscapeSharedRedFlashPaletteFxColorToolingDefinitions.All,
            entry => new Project(entry.DefinitionPointer, entry.ProgramStart), 2);
        CheckEntries(TourianStatueGreyPaletteFxColorToolingDefinitions.All,
            entry => new Project(entry.DefinitionPointer, entry.ProgramStart), 4);
        Console.WriteLine("Stream 5 palette entries: all eleven entry identities, original controls and collection bounds pass.");

        void CheckEntries<T>(IReadOnlyList<T> entries, Func<T, Project> project, int expectedCount)
        {
            AssertEqual(expectedCount, entries.Count, "Palette entry count");
            int index = 0;
            foreach (T entry in entries)
            {
                var identity = project(entry);
                AssertEqual(identity, project(entries[index]), "Palette entry enumeration order");
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | identity.Definition + 2), identity.Program, "Original palette definition list pointer");
                index++;
            }
            AssertEqual(expectedCount, index, "All palette entries enumerated");
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = entries[-1]; }, "Palette entries lower bound");
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = entries[expectedCount]; }, "Palette entries upper bound");
        }
        void CheckEntryCount<T>(IReadOnlyList<T> entries, int expectedCount)
        {
            AssertEqual(expectedCount, entries.Count, "Palette entry count");
            int index = 0;
            foreach (T entry in entries)
            {
                AssertEqual(entry, entries[index], "Palette entry enumeration order");
                index++;
            }
            AssertEqual(expectedCount, index, "All palette entries enumerated");
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = entries[-1]; }, "Palette entries lower bound");
            AssertThrows<ArgumentOutOfRangeException>(() => { _ = entries[expectedCount]; }, "Palette entries upper bound");
        }
    }

    /// <summary>Checks the Ceres door quake's byte-indexed offsets against original ROM words.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Ceres door earthquake offsets")]
    internal static void VerifyLookupStream5DoorQuakeDecoding(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyCeresDoorQuakeDefinitions), () => VerifyCeresDoorQuakeDefinitions(rom));
        byte[] stockJson = SuperMetroid.AssetExtraction.Enemies.EnemySpritemapFiles.Extract(rom);
        var installed = EnemySpritemapDocuments.Load(new MemoryStream(stockJson, writable: false));
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            RidleyEncounter = { CeresStatus = CeresEscapeStatus.RidleyEscaped },
            TileArtwork = SpawnedObjectFixtures.InstalledTilesWith(installed),
        };
        enemies.RidleyEncounter.Ridley = new RidleyEnemyState { MovementAnimationEnabled = false };
        enemies.SlotTable.Slots[0].Bind(EnemyDefinitionId.RidleyCeres, enemies.SlotTable.Slots[0].Definition);
        // Ceres Ridley's scene draws from his room's sheets.
        SpawnedObjectFixtures.LoadSetDrawing(enemies, EnemyDefinitionId.RidleyCeres);
        var door = enemies.SlotTable.Slots[1];
        door.Bind(EnemyDefinitionId.CeresDoor, door.Definition);
        var doorState = new CeresDoorEnemyState { HandsDrawingToRidley = true };
        door.SetSpeciesState(doorState, doorState.ClearWithRecord);
        door.Position = door.Position.WithWholeY(80);
        foreach (ushort x in new ushort[] { 0, 1, 3, 255, 256, 511, ushort.MaxValue })
        foreach (ushort camera in new ushort[] { 0, ushort.MaxValue })
        for (ushort phase = 0; phase < 4; phase++)
        {
            door.Position = door.Position.WithWholeX(x); enemies.Earthquake.EarthquakeTimer = WrappingCountdown.FromRaw(phase);
            var actual = new OamBuffer(); actual.BeginFrame();
            actual.AppendDrawn(frame => enemies.CeresRidleyBehavior.BabyScene.DrawCeresRidleyImmediateBabyAndDoor(frame, new(camera, 0))); actual.FinalizeFrame();
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa6a321 + phase);
            AssertEqual(native & 0x1ff, CeresDoorQuakeDefinitions.XOffset(phase) & 0x1ff,
                "Native overlapping high byte retains the same ninth coordinate bit");
            var expected = new OamBuffer(); expected.BeginFrame();
            DrawImportedEnemySpritemap(rom, expected, new NativeEnemySpritemapDraw { Bank = CeresDoorFrameDefinitionsConstants.Bank, Pointer = CeresDoorFrameDefinitionsConstants.RidleyPrivateOverlaySpritemap, Origin = new(unchecked((ushort)(x - camera + native)), 80), PaletteBits = EnemyPaletteBits.Palette2, BaseTileIndex = 0 });
            expected.FinalizeFrame();
            AssertTrue(expected.ImageEquals(actual),
                "Byte-decoded quake preserves actual low/high OAM through coordinate wrap");
        }
        Console.WriteLine("Ceres quake byte decoding:56 actual wrapped low/high OAM frames match full native overlapping-word reads.");
    }

    /// <summary>Checks the Ceres flight scene's transferred colors and CGRAM destinations.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres flight palette")]
    internal static void VerifyLookupStream5CeresFlightPalette(SuperMetroidAddressSpace rom)
    {
        var colors = new PaletteRgb5[SnesCgram.ColorCount];
        for (int index = 0; index < colors.Length; index++)
        {
            ushort value = ReadVerificationWord(rom, CeresFlightRomData.Assets.Palette + index * sizeof(ushort));
            colors[index] = new PaletteRgb5 { Red = value & 31, Green = value >> 5 & 31, Blue = value >> 10 & 31 };
        }
        CeresFlightPalette stock = Check();
        var edits = stock.edits;
        AssertEqual(0, edits.Count, "Native shared sections and endpoint ramp need no residual corrections");
        int independent = stock.colors.Count;
        for (int index = 0; index < colors.Length; index++)
        {
            PaletteRgb5 original = colors[index];
            for (int channel = 0; channel < 3; channel++)
            {
                colors[index] = new PaletteRgb5
                {
                    Red = channel == 0 ? original.Red ^ 1 : original.Red,
                    Green = channel == 1 ? original.Green ^ 1 : original.Green,
                    Blue = channel == 2 ? original.Blue ^ 1 : original.Blue,
                };
                _ = Check();
            }
            colors[index] = original;
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ColorAt(-1), "Ceres palette lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ColorAt(SnesCgram.ColorCount), "Ceres palette upper bound");
        AssertThrows<ArgumentException>(() => stock.CopyColorWordsTo(new byte[CeresFlightPalette.ColorWordBytes - 1]), "Ceres palette output extent");
        IReadOnlyDictionary<string, byte[]> files = SuperMetroid.AssetExtraction.Areas.Ceres.CeresFlightArtworkExtractor.Extract(rom);
        var catalog = CeresFlightArtworkCatalog.Load(Open(CeresFlightArtworkFormat.FloorFileName),
            Open(CeresFlightArtworkFormat.MapFileName), Open(CeresFlightArtworkFormat.ObjectFileName),
            Open(CeresFlightPaletteFormat.FileName), Open(CeresFlightSpriteFormat.FileName), Open(CeresFlightActorLayoutFormat.FileName));
        byte[] nativeBytes = new byte[CgramPalettes.ByteCount];
        for (int index = 0; index < nativeBytes.Length; index++) nativeBytes[index] = rom.ReadCartridgeByte(CeresFlightRomData.Assets.Palette + index);
        string expectedHash = SelectedPresentationHash.Create(nameof(CeresFlightArtworkCatalog), content =>
        {
            content.Append("mode7-characters", catalog.FloorCharacters.Span);
            content.Append("mode7-maps", catalog.FloorMaps.Span);
            content.Append("object-characters", catalog.ObjectCharacters.Span);
            content.Append("palette", nativeBytes);
            content.Append("sprites", Convert.FromHexString(catalog.Sprites.ContentIdentity));
            content.Append("actors", Convert.FromHexString(catalog.Actors.ContentIdentity));
        });
        AssertEqual(expectedHash, catalog.ContentIdentity, "Ceres flight canonical hash retains original palette byte framing");
        Console.WriteLine($"Ceres flight palette:256 native colors,768 independent channel edits,actual CGRAM,byte serialization and catalog identity pass; {independent} independent color inputs remain required.");

        MemoryStream Open(string name) => new(files[name], writable: false);
        CeresFlightPalette Check()
        {
            var document = new CeresFlightPaletteDocument { Version = 1, Colors = colors };
            using var json = new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options));
            CeresFlightPalette actual = CeresFlightPalette.Load(json);
            var cgram = new ScreenPalette(); actual.LoadTo(cgram);
            Span<byte> bytes = stackalloc byte[CeresFlightPalette.ColorWordBytes]; actual.CopyColorWordsTo(bytes);
            for (int index = 0; index < colors.Length; index++)
            {
                PaletteRgb5 color = colors[index];
                ushort expected = (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
                AssertEqual(expected, actual.ColorAt(index), "Every selected color remains independent");
                AssertEqual(expected, cgram.Colors[index], "Calculated colors reach actual CGRAM");
                AssertEqual(expected, System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(index * sizeof(ushort))), "Serialized colors retain exact native byte order");
            }
            return actual;
        }
    }

    /// <summary>Checks Ceres normal room paint against native colors in their actual CGRAM positions.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres normal paint")]
    internal static void VerifyLookupStream5CeresNormalPaint(SuperMetroidAddressSpace rom)
    {
        PaletteRgb5[] Colors(int source, int count) => Enumerable.Range(0, count).Select(i =>
        {
            ushort w = ReadVerificationWord(rom, source + 2 * i);
            return new PaletteRgb5 { Red = w & 31, Green = w >> 5 & 31, Blue = w >> 10 };
        }).ToArray();
        var document = new CeresDoorVisualDocument
        {
            Version = 1, Normal = Colors(CeresDoorVisualRomData.NormalColors, 15), Escape = Colors(CeresDoorVisualRomData.EscapeColors, 15),
            Animation = Enumerable.Range(0, 8).Select(row => Colors(CeresDoorVisualRomData.AnimationColors + 16 * row, 6)).ToArray(),
            Mode7DoorFrames = Enumerable.Range(0, 2).Select(frame => Enumerable.Range(0, 4).Select(i => (int)rom.ReadCartridgeByte(CeresDoorVisualRomData.Mode7FirstFrameSource + frame * 4 + i)).ToArray()).ToArray(),
        };
        byte[] planar = Enumerable.Range(0, CeresDoorVisualRomData.TileByteCount).Select(i => rom.ReadCartridgeByte(CeresDoorVisualRomData.TileSource + i)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
        using var png = new MemoryStream(); IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        CeresDoorNormalPaintDefinitions stock = Check();
        AssertEqual(13, stock.paint.Count, "Thirteen local material seeds remain after shared warm extraction");
        string[] warmSeeds = ["highlightBlue", "amberRed", "amberGreen", "red", "goldRed", "goldGreen", "goldBlue"];
        AssertTrue(warmSeeds.Order(StringComparer.Ordinal).SequenceEqual(MetadataShape.Fields(typeof(CeresDoorWarmTargetPaintDefinitions), flags)
            .Where(field => field.FieldType == typeof(int)).Select(field => field.Name).Order(StringComparer.Ordinal)), "Seven exact shared warm material inputs");
        AssertEqual(0, stock.WarmTargets.edits.Count, "Shared native warm shades need no overrides");
        AssertEqual(0, stock.edits.Count, "Native calculated channels need no unexplained overrides");
        for (int color = 0; color < 15; color++)
        {
            PaletteRgb5 before = document.Normal[color];
            for (int channel = 0; channel < 3; channel++)
            {
                document.Normal[color] = new PaletteRgb5 { Red = channel == 0 ? before.Red ^ 31 : before.Red, Green = channel == 1 ? before.Green ^ 31 : before.Green, Blue = channel == 2 ? before.Blue ^ 31 : before.Blue };
                _ = Check();
            }
            document.Normal[color] = before;
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ColorAt(-1), "Normal lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ColorAt(15), "Normal upper bound");
        Console.WriteLine("Ceres normal: fifteen native colors, thirteen local plus seven shared seeds/zero overrides,45 independent channel edits,92 actual initializer palette copies, hash and bounds pass.");

        CeresDoorNormalPaintDefinitions Check()
        {
            ushort Pack(PaletteRgb5 c) => (ushort)(c.Red | c.Green << 5 | c.Blue << 10);
            var basis = new CeresDoorNormalPaintDefinitions(ToColors(document.Normal.Select(Pack).ToArray()));
            var visual = CeresDoorVisualCatalog.Load(new MemoryStream(png.ToArray()), new MemoryStream(CeresDoorVisualCatalog.Write(document)));
            for (int color = 0; color < 15; color++) AssertEqual(Pack(document.Normal[color]), basis.ColorAt(color), "Every supplied normal channel remains independent");
            foreach (ushort variant in new ushort[] { 0, 3 })
            {
                var system = new RoomEnemySystem(new EnemySession()) { TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), AreaArtwork = new AreaArtworkCatalog { CeresDoorVisual = visual } };
                var cgram = new SamusColourRig();
                system.Presentation.BindCgram(cgram.Gameplay);
                var slot = system.SlotTable.Slots[0]; slot.SetSpawnParameter1(variant);
                new Action<RoomEnemySlot>(system.CeresDoorBehavior.InitializeCeresDoor)(slot);
                int destination = variant == 3 ? CeresDoorVisualRomDataConstants.NormalTargetColor : CeresDoorVisualRomDataConstants.ActiveTargetColor;
                for (int color = 0; color < 15; color++) AssertEqual(Pack(document.Normal[color]), cgram.Colors[destination + color], "Actual normal/target initializer copy");
            }
            string identity = SelectedPresentationHash.Create("enemy-ceres-door-v1", content =>
            {
                content.Append("tiles", planar); content.AppendWords("normal", document.Normal.Select(Pack).ToArray()); content.AppendWords("escape", document.Escape.Select(Pack).ToArray());
                content.AppendWordFrames("animation", document.Animation.Select(row => row.Select(Pack).ToArray()).ToArray());
                content.Append("mode7-frames", 2); foreach (var frame in document.Mode7DoorFrames) content.Append("mode7-frame", frame.Select(value => (byte)value).ToArray());
            });
            AssertEqual(identity, visual.ContentIdentity, "Normal changes preserve independently installed escape/animation and canonical hash");
            return basis;
        }
    }

    /// <summary>Checks Ceres escape paint's highlights, edges and outlines after palette composition.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres escape paint")]
    internal static void VerifyLookupStream5CeresEscapePaint(SuperMetroidAddressSpace rom)
    {
        PaletteRgb5[] Colors(int source, int count) => Enumerable.Range(0, count).Select(i =>
        {
            ushort w = ReadVerificationWord(rom, source + 2 * i);
            return new PaletteRgb5 { Red = w & 31, Green = w >> 5 & 31, Blue = w >> 10 };
        }).ToArray();
        var document = new CeresDoorVisualDocument
        {
            Version = 1, Normal = Colors(CeresDoorVisualRomData.NormalColors, 15), Escape = Colors(CeresDoorVisualRomData.EscapeColors, 15),
            Animation = Enumerable.Range(0, 8).Select(row => Colors(CeresDoorVisualRomData.AnimationColors + 16 * row, 6)).ToArray(),
            Mode7DoorFrames = Enumerable.Range(0, 2).Select(frame => Enumerable.Range(0, 4).Select(i => (int)rom.ReadCartridgeByte(CeresDoorVisualRomData.Mode7FirstFrameSource + frame * 4 + i)).ToArray()).ToArray(),
        };
        byte[] planar = Enumerable.Range(0, CeresDoorVisualRomData.TileByteCount).Select(i => rom.ReadCartridgeByte(CeresDoorVisualRomData.TileSource + i)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
        using var png = new MemoryStream(); IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        CeresDoorEscapePaintDefinitions stock = Check();
        // The highlight/edge/outline levels live in the surface paint the escape palette composes.
        object surface = stock.surface;
        AssertEqual(4, (((CeresDoorEscapeSurfacePaintDefinitions)surface).highlightAndEdgeLevels).Length, "Four selected highlight/edge/outline levels");
        AssertEqual(0, stock.edits.Count, "Native calculated channels need no unexplained overrides");
        AssertEqual(0, (((CeresDoorEscapeSurfacePaintDefinitions)surface).edits).Count, "Native calculated surface channels need no unexplained overrides");
        for (int color = 0; color < 15; color++)
        {
            PaletteRgb5 before = document.Escape[color];
            for (int channel = 0; channel < 3; channel++)
            {
                document.Escape[color] = new PaletteRgb5 { Red = channel == 0 ? before.Red ^ 31 : before.Red, Green = channel == 1 ? before.Green ^ 31 : before.Green, Blue = channel == 2 ? before.Blue ^ 31 : before.Blue };
                _ = Check();
            }
            document.Escape[color] = before;
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ColorAt(-1), "Escape lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ColorAt(15), "Escape upper bound");
        Console.WriteLine("Ceres escape: fifteen native colors, eight paint seeds/zero overrides,45 independent channel edits,92 actual escape initializer palette copies, hash and bounds pass.");

        CeresDoorEscapePaintDefinitions Check()
        {
            ushort Pack(PaletteRgb5 c) => (ushort)(c.Red | c.Green << 5 | c.Blue << 10);
            var basis = new CeresDoorEscapePaintDefinitions(ToColors(document.Escape.Select(Pack).ToArray()), new CeresDoorNormalPaintDefinitions(ToColors(document.Normal.Select(Pack).ToArray())));
            var visual = CeresDoorVisualCatalog.Load(new MemoryStream(png.ToArray()), new MemoryStream(CeresDoorVisualCatalog.Write(document)));
            for (int color = 0; color < 15; color++) AssertEqual(Pack(document.Escape[color]), basis.ColorAt(color), "Every supplied escape channel remains independent");
            foreach (ushort variant in new ushort[] { 0, 3 })
            {
                var system = new RoomEnemySystem(new EnemySession()) { RidleyEncounter = { CeresStatus = CeresEscapeStatus.SelfDestructStarted }, TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), AreaArtwork = new AreaArtworkCatalog { CeresDoorVisual = visual } };
                var cgram = new SamusColourRig();
                system.Presentation.BindCgram(cgram.Gameplay);
                var slot = system.SlotTable.Slots[0]; slot.SetSpawnParameter1(variant);
                new Action<RoomEnemySlot>(system.CeresDoorBehavior.InitializeCeresDoor)(slot);
                int destination = CeresDoorVisualRomDataConstants.ActiveTargetColor;
                for (int color = 0; color < 15; color++) AssertEqual(Pack(document.Escape[color]), cgram.Colors[destination + color], "Actual escape initializer copy");
            }
            string identity = SelectedPresentationHash.Create("enemy-ceres-door-v1", content =>
            {
                content.Append("tiles", planar); content.AppendWords("normal", document.Normal.Select(Pack).ToArray()); content.AppendWords("escape", document.Escape.Select(Pack).ToArray());
                content.AppendWordFrames("animation", document.Animation.Select(row => row.Select(Pack).ToArray()).ToArray());
                content.Append("mode7-frames", 2); foreach (var frame in document.Mode7DoorFrames) content.Append("mode7-frame", frame.Select(value => (byte)value).ToArray());
            });
            AssertEqual(identity, visual.ContentIdentity, "Escape changes preserve independently installed normal/animation and canonical hash");
            return basis;
        }
    }

    /// <summary>Checks Ceres beacon light colors against the native palette ramp.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres beacon paint")]
    internal static void VerifyLookupStream5CeresBeaconPaint(SuperMetroidAddressSpace rom)
    {
        PaletteRgb5[] Colors(int source, int count) => Enumerable.Range(0, count).Select(i =>
        {
            ushort w = ReadVerificationWord(rom, source + 2 * i);
            return new PaletteRgb5 { Red = w & 31, Green = w >> 5 & 31, Blue = w >> 10 };
        }).ToArray();
        var document = new CeresDoorVisualDocument
        {
            Version = 1, Normal = Colors(CeresDoorVisualRomData.NormalColors, 15), Escape = Colors(CeresDoorVisualRomData.EscapeColors, 15),
            Animation = Enumerable.Range(0, 8).Select(row => Colors(CeresDoorVisualRomData.AnimationColors + 16 * row, 6)).ToArray(),
            Mode7DoorFrames = Enumerable.Range(0, 2).Select(frame => Enumerable.Range(0, 4).Select(i => (int)rom.ReadCartridgeByte(CeresDoorVisualRomData.Mode7FirstFrameSource + frame * 4 + i)).ToArray()).ToArray(),
        };
        byte[] planar = Enumerable.Range(0, CeresDoorVisualRomData.TileByteCount).Select(i => rom.ReadCartridgeByte(CeresDoorVisualRomData.TileSource + i)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
        using var png = new MemoryStream(); IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        CeresDoorAnimationPaintDefinitions stock = Check(-1);
        AssertEqual(17, stock.firstSeedBlue, "Selected beacon blue");
        AssertEqual(14, stock.dimMiddleAmberRed, "Selected dim amber red");
        foreach (Dictionary<ColourCell, Bgr555> overrides in new[] { stock.seedEdits, stock.edits })
            AssertEqual(0, overrides.Count, "All native beacon dependencies calculate without overrides");
        for (int row = 0; row < 8; row++)
        for (int color = 0; color < 6; color++)
        {
            PaletteRgb5 before = document.Animation[row][color];
            for (int channel = 0; channel < 3; channel++)
            {
                document.Animation[row][color] = new PaletteRgb5 { Red = channel == 0 ? before.Red ^ 31 : before.Red, Green = channel == 1 ? before.Green ^ 31 : before.Green, Blue = channel == 2 ? before.Blue ^ 31 : before.Blue };
                _ = Check(row);
            }
            document.Animation[row][color] = before;
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ColorAt(-1, 0), "Beacon row lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ColorAt(8, 0), "Beacon row upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ColorAt(0, -1), "Beacon color lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ColorAt(0, 6), "Beacon color upper bound");
        Console.WriteLine("Ceres beacon:48native colors/zero stock overrides,144independent RGB edits,208actual row selections including64-tick cycle, untouched neighbors/hash/bounds pass.");

        CeresDoorAnimationPaintDefinitions Check(int editedRow)
        {
            ushort Pack(PaletteRgb5 c) => (ushort)(c.Red | c.Green << 5 | c.Blue << 10);
            var basis = new CeresDoorAnimationPaintDefinitions(ToColors(document.Animation.Select(row => row.Select(Pack).ToArray()).ToArray()), new CeresDoorNormalPaintDefinitions(ToColors(document.Normal.Select(Pack).ToArray())));
            var visual = CeresDoorVisualCatalog.Load(new MemoryStream(png.ToArray()), new MemoryStream(CeresDoorVisualCatalog.Write(document)));
            for (int row = 0; row < 8; row++) for (int color = 0; color < 6; color++)
                AssertEqual(Pack(document.Animation[row][color]), basis.ColorAt(row, color), "Every supplied beacon channel remains independent");
            var system = new RoomEnemySystem(new EnemySession(), services: new StandaloneRoomEnemyServices(() => 0) { FloorMap = new FloorMap(new byte[FloorMap.Cells], default, RoomBackgroundsFixtures.FloorTiles()) }) { TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), AreaArtwork = new AreaArtworkCatalog { CeresDoorVisual = visual } };
            var cgram = new SamusColourRig();
            system.Presentation.BindCgram(cgram.Gameplay);
            system.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
            Action tick = system.CeresDoorBehavior.RunCeresDoorPaletteAnimation;
            foreach (int counter in editedRow < 0 ? Enumerable.Range(0, 64) : new[] { editedRow * 8 })
            {
                system.SlotTable.Slots[0].FrameCounter = (ushort)counter; tick();
                int row = (counter & 0x38) >> 3;
                for (int color = 0; color < 6; color++) AssertEqual(Pack(document.Animation[row][color]), cgram.Colors[41 + color], "Actual native eight-tick row selection and six CGRAM writes");
                AssertEqual(0, cgram.Colors[40], "Beacon lower neighbor unchanged");
                AssertEqual(0, cgram.Colors[47], "Beacon upper neighbor unchanged");
            }
            string identity = SelectedPresentationHash.Create("enemy-ceres-door-v1", content =>
            {
                content.Append("tiles", planar); content.AppendWords("normal", document.Normal.Select(Pack).ToArray()); content.AppendWords("escape", document.Escape.Select(Pack).ToArray());
                content.AppendWordFrames("animation", document.Animation.Select(row => row.Select(Pack).ToArray()).ToArray());
                content.Append("mode7-frames", 2); foreach (var frame in document.Mode7DoorFrames) content.Append("mode7-frame", frame.Select(value => (byte)value).ToArray());
            });
            AssertEqual(identity, visual.ContentIdentity, "Beacon edits preserve normal/escape/platform and canonical hash");
            return basis;
        }
    }

    /// <summary>Runs the Ceres normal room-paint cartridge comparison.</summary>
    [VerificationCommand("--lookup-stream5-ceres-normal-paint")]
    internal static int LookupStream5CeresNormalPaintCommand()
    {
        Suite(nameof(VerifyLookupStream5CeresNormalPaint), () => VerifyLookupStream5CeresNormalPaint(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the Ceres escape surface-paint comparison.</summary>
    [VerificationCommand("--lookup-stream5-ceres-escape-paint")]
    internal static int LookupStream5CeresEscapePaintCommand()
    {
        Suite(nameof(VerifyLookupStream5CeresEscapePaint), () => VerifyLookupStream5CeresEscapePaint(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the Ceres beacon color-ramp comparison.</summary>
    [VerificationCommand("--lookup-stream5-ceres-beacon-paint")]
    internal static int LookupStream5CeresBeaconPaintCommand()
    {
        Suite(nameof(VerifyLookupStream5CeresBeaconPaint), () => VerifyLookupStream5CeresBeaconPaint(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the Ceres rumble and falling debris check.</summary>
    [VerificationCommand("--lookup-stream5-ceres-rumble")]
    internal static int LookupStream5CeresRumbleCommand()
    {
        Suite(nameof(VerifyLookupStream5CeresRumble), () => VerifyLookupStream5CeresRumble(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the Zebes starfield OAM comparison.</summary>
    [VerificationCommand("--lookup-stream5-zebes-star-fields")]
    internal static int LookupStream5ZebesStarFieldsCommand()
    {
        Suite(nameof(VerifyLookupStream5ZebesStarFields), () => VerifyLookupStream5ZebesStarFields(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the Tourian statue palette-ramp comparison.</summary>
    [VerificationCommand("--lookup-stream5-statue-ramps")]
    internal static int LookupStream5StatueRampsCommand()
    {
        Suite(nameof(VerifyLookupStream5StatueRamps), () => VerifyLookupStream5StatueRamps(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the Phantoon visual-marker address check.</summary>
    [VerificationCommand("--lookup-stream5-phantoon-markers")]
    internal static int LookupStream5PhantoonMarkersCommand()
    {
        Suite(nameof(VerifyLookupStream5PhantoonMarkers), () => VerifyLookupStream5PhantoonMarkers(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the Magdollite palette-pulse check.</summary>
    [VerificationCommand("--lookup-stream5-magdollite-pulse")]
    internal static int LookupStream5MagdollitePulseCommand()
    {
        Suite(nameof(VerifyLookupStream5MagdollitePulse), () => VerifyLookupStream5MagdollitePulse(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the Zebetite palette-pulse check.</summary>
    [VerificationCommand("--lookup-stream5-zebetite-pulse")]
    internal static int LookupStream5ZebetitePulseCommand()
    {
        Suite(nameof(VerifyLookupStream5ZebetitePulse), () => VerifyLookupStream5ZebetitePulse(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs Yapping Maw's native offset comparison.</summary>
    [VerificationCommand("--lookup-stream5-yapping-maw-offsets")]
    internal static int LookupStream5YappingMawOffsetsCommand()
    {
        Suite(nameof(VerifyLookupStream5YappingMawOffsets), () => VerifyLookupStream5YappingMawOffsets(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the eye-door geometry comparison.</summary>
    [VerificationCommand("--lookup-stream5-eye-geometry")]
    internal static int LookupStream5EyeGeometryCommand()
    {
        Suite(nameof(VerifyLookupStream5EyeGeometry), () => VerifyLookupStream5EyeGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the map highlight CGRAM comparison.</summary>
    [VerificationCommand("--lookup-stream5-map-highlight")]
    internal static int LookupStream5MapHighlightCommand()
    {
        Suite(nameof(VerifyLookupStream5MapHighlight), () => VerifyLookupStream5MapHighlight(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs Sidehopper and corpse-geometry source checks.</summary>
    [VerificationCommand("--lookup-stream5-sidehopper-geometry")]
    internal static int LookupStream5SidehopperGeometryCommand()
    {
        Suite(nameof(VerifyLookupStream5SidehopperGeometry), () => VerifyLookupStream5SidehopperGeometry(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the Phantoon rain sprite comparison.</summary>
    [VerificationCommand("--lookup-stream5-phantoon-rain")]
    internal static int LookupStream5PhantoonRainCommand()
    {
        Suite(nameof(VerifyLookupStream5PhantoonRain), () => VerifyLookupStream5PhantoonRain(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the Ceres door palette-ramp comparison.</summary>
    [VerificationCommand("--lookup-stream5-ceres-door-ramp")]
    internal static int LookupStream5CeresDoorRampCommand()
    {
        Suite(nameof(VerifyLookupStream5CeresDoorRamp), () => VerifyLookupStream5CeresDoorRamp(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>Runs the Crocomire skeleton VRAM-transfer comparison.</summary>
    [VerificationCommand("--lookup-stream5-skeleton-transfers")]
    internal static int LookupStream5SkeletonTransfersCommand()
    {
     VerifyLookupStream5SkeletonTransfers(); return 0; 
    }

    /// <summary>Runs melting-room tilemap transfer checks.</summary>
    [VerificationCommand("--lookup-stream5-melting-tilemaps")]
    internal static int LookupStream5MeltingTilemapsCommand()
    {
     VerifyLookupStream5MeltingTilemaps(); return 0; 
    }
}
