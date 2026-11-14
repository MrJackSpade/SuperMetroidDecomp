using SuperMetroid.Diagnostics.Input;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Bosses.Crocomire;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Verification.Enemies.Magdollite;
using SuperMetroid.Verification.Enemies.MorphBallEye;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Areas.Ceres.CeresDoorQuakeDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireMeltingDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.Artwork.CrocomireSkeletonArtworkVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkPhantoonBg2FramesVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Ceres;
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
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
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
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Assets;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Fx;
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
        ImportEnemyArtwork catalog = InstalledFixtures.EnemyArtwork;
        CrocomireMeltingArtwork art = (catalog.Bosses.CrocomireMelting ?? throw new InvalidOperationException("Verification requires catalog.CrocomireMelting."));
        var native = new ushort[2][];
        for (int phase = 0; phase < 2; phase++)
        {
            CrocomireMeltingIllustration source = phase == 0 ? CrocomireMeltingIllustration.First : CrocomireMeltingIllustration.Second;
            native[phase] = Enumerable.Range(0, 256).Select(i => ReadVerificationWord(oracle, SuperMetroid.AssetExtraction.Enemies.CrocomireMeltingSources.Tilemap(source) + i * 2)).ToArray();
            AssertTrue(BackgroundTileWordBytes.Words((art ?? throw new InvalidOperationException("Verification requires art.")).Tilemap(source)).SequenceEqual(native[phase]), "Installed melt words");
            VerifyInstalledCrocomireMeltingTilemap(oracle, catalog, source, phase == 0
                ? CrocomireBodySegment.MeltingOneTopRow : CrocomireBodySegment.MeltingTwoTopRow);
        }
        var first = art.first;
        var second = art.second;
        string expectedIdentity = SelectedPresentationHash.Create("enemy-crocomire-melt-v2", content =>
        {
            content.Append("first", first.Characters.Span); content.Append("second", second.Characters.Span);
            content.AppendTiles("first-map", BackgroundTileWordBytes.Tiles(native[0])); content.AppendTiles("second-map", BackgroundTileWordBytes.Tiles(native[1]));
        });
        AssertEqual(expectedIdentity, art.ContentIdentity, "Original canonical melt identity");
        AssertThrows<InvalidDataException>(() => { _ = art.Tilemap(CrocomireMeltingIllustration.Second + 1); }, "Unknown map identity rejects");
        Console.WriteLine("Crocomire melt layouts: 512 native words, both actual guarded BG2 uploads and canonical identity pass.");
    }
    /// <summary>Checks Crocomire skeleton VRAM transfer staging and destinations.</summary>
    internal static void VerifyLookupStream5SkeletonTransfers()
    {
        var oracle = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)),
            "Skeleton transfer oracle revision");
        VerifyCrocomireSkeletonArtwork(oracle, InstalledFixtures.EnemyArtwork);
        AssertThrows<InvalidDataException>(() => CrocomireSkeletonArtwork.IsChunk(-1),
            "Skeleton negative transfer index rejects");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = CrocomireSourceDefinitions.SkeletonChunk(6),
            "Skeleton view excludes sentinel");
    }

    /// <summary>Compares Ceres rumble and falling debris emissions with the native room routine.</summary>
    internal static void VerifyLookupStream5CeresRumble(CartridgeImportAddressSpace rom)
    {
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes("Super Metroid.smc"))), "Rumble native revision");
        for (int index = 0; index < 4; index++)
            AssertEqual(new Offset16<RoomPixels>(unchecked((short)ReadVerificationWord(rom, 0xa6f840 + 4 * index)),
                unchecked((short)ReadVerificationWord(rom, 0xa6f842 + 4 * index))),
                CeresDoorRumble.Offset(index), "All eight native anchor words");
        AssertThrows<ArgumentOutOfRangeException>(() => CeresDoorRumble.Offset(-1), "Rumble lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => CeresDoorRumble.Offset(4), "Rumble upper bound");
        foreach (Position16<RoomPixels> origin in new Position16<RoomPixels>[] { new(232, 631), new(1, 65530) })
        {
            var system = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
            var slot = system.SlotTable.Slots[0];
            slot.Position = slot.Position.WithWhole(origin);
            var doorState = new CeresDoorEnemyState { RumbleCallsLeft = 48 };
            slot.SetSpeciesState(doorState);
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
                    MiscDustProjectileSegment program = MiscDustProjectiles.Animation(ordinal % 2 == 0 ? MiscDustAnimation.Smoke : MiscDustAnimation.SmallExplosion);
                    // Native pool allocation descends; identify each emission through its unique current ordinal below.
                    var particle = particles[emitted - ordinal - 1];
                    AssertEqual(x, particle.Position.X.Whole, "Actual produced native X including 16-bit wrap");
                    AssertEqual(y, particle.Position.Y.Whole, "Actual produced native Y including 16-bit wrap");
                    AssertTrue(particle.Animation is MiscDustProjectileAnimation started && started.PlayingSegment == program, "RNG changes animation only");
                }
                AssertEqual(call == 49, slot.Presence.HasAny(EnemyPresence.Hidden), "Hide only at destruction expiry");
            }
            AssertEqual(CeresEscapeStatus.ElevatorDoorDestroyed, system.RidleyEncounter.CeresStatus, "Actual rotation handoff after last anchor");
            AssertTrue(system.Audio.SoundRequests.Any(request => request.SoundEffect == EnemySound.BigExplosion), "Emission sound preserved");
        }
        Console.WriteLine("Ceres rumble: eight native words, bounds, 98 actual calls, 20 emitted native positions/order/animations, wrap and expiry pass.");
    }
    /// <summary>Checks Phantoon's rain-phase wait against the cartridge countdown.</summary>
    internal static void VerifyLookupStream5PhantoonRainWait(CartridgeImportAddressSpace rom)
    {
        int totalTicks = 0;
        const int nextPattern = 3;
        foreach (int bucket in new[] { 2, 0, 1 })
        {
            var system = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
            var body = system.SlotTable.Slots[0];
            var eye = system.SlotTable.Slots[1];
            var state = new PhantoonEnemyState(body) { Eye = eye, Tentacles = system.SlotTable.Slots[2], Mouth = system.SlotTable.Slots[3] };
            system.PhantoonBehavior.Combat.Phantoon = state;
            int randomCalls = 0;
            system.VerificationServices().NextRandom = () => (ushort)(++randomCalls == 1 ? bucket : nextPattern);
            // The fade writes Phantoon's palette rows toward its installed fade colors.
            system.BossArtwork = InstalledFixtures.BossArtwork;
            Action<PhantoonEnemyState> fade = system.PhantoonBehavior.Patterns.RunPhantoonFlameRainFadeOut;
            Action<RoomEnemySlot, PhantoonEnemyState> tick = system.PhantoonBehavior.Patterns.RunPhantoonHiddenFlameRain;
            body.Position = body.Position.WithWhole(new(111, 99)); state.BodyState.PathCursor = 42; state.BodyState.PhaseTimer = 17;
            state.BodyState.Phase = PhantoonPhase.FadeOutDuringFlameRain;
            // He fades out of the colouring the fight shows him in (#1512), his health band's.
            state.Colours.Show(PhantoonBodyLook.Health(0));
            fade(state); // The eye's independent fade has not completed yet.
            AssertEqual(0, randomCalls, "Incomplete fade does not choose hidden delay");
            AssertEqual(17, state.BodyState.PhaseTimer, "Incomplete fade preserves prior countdown");
            state.EyeState.FadeComplete = true;
            fade(state);
            ushort duration = ReadVerificationWord(rom, 0xa7cd63 + bucket * 2);
            AssertEqual(duration, state.BodyState.PhaseTimer, "Completed fade selects exact native hidden duration");
            AssertEqual(PhantoonPhase.SpawnFlameRain, state.BodyState.Phase, "Completed fade starts hidden waiting phase");
            for (int elapsed = 1; elapsed <= duration; elapsed++)
            {
                tick(body, state); totalTicks++;
                AssertEqual((ushort)(duration - elapsed), state.BodyState.PhaseTimer, "Hidden wait decrements exactly once");
                if (elapsed < duration)
                {
                    AssertEqual(1, randomCalls, "Next location is not chosen before delay expires");
                    AssertEqual((ushort)111, body.Position.X.Whole, "Hidden wait does not interpolate X");
                    AssertEqual((ushort)99, body.Position.Y.Whole, "Hidden wait does not interpolate Y");
                    AssertEqual(42, state.BodyState.PathCursor, "Hidden wait does not advance path cursor");
                    AssertEqual(0, system.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive), "No rain launches before expiry");
                }
            }
            AssertEqual(2, randomCalls, "Expiration consumes one separate placement RNG word");
            AssertEqual(ReadVerificationWord(rom, 0xa7cdad + nextPattern * 8), state.BodyState.PathCursor, "Exact delayed native path cursor");
            AssertEqual(ReadVerificationWord(rom, 0xa7cdaf + nextPattern * 8), body.Position.X.Whole, "Exact delayed native X");
            AssertEqual(ReadVerificationWord(rom, 0xa7cdb1 + nextPattern * 8), body.Position.Y.Whole, "Exact delayed native Y");
            AssertEqual(PhantoonPhase.BecomeSolidAfterFlameRain, state.BodyState.Phase, "Native rain appearance handoff");
            AssertEqual(false, state.EyeState.FigureEightReversed, "Rain handoff clears fade counter");
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
    internal static void VerifyLookupStream5PhantoonClosedEye(CartridgeImportAddressSpace rom)
    {
        int totalTicks = 0;
        foreach (int bucket in new[] { 1, 2, 0 })
        foreach (bool reversed in new[] { false, true })
        {
            var system = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
            var body = system.SlotTable.Slots[0];
            var eye = system.SlotTable.Slots[1];
            var mouth = system.SlotTable.Slots[3];
            var state = new PhantoonEnemyState(body) { Eye = eye, Tentacles = system.SlotTable.Slots[2], Mouth = mouth };
            system.PhantoonBehavior.Combat.Phantoon = state;
            int randomCalls = 0;
            system.VerificationServices().NextRandom = () => { randomCalls++; return (ushort)(reversed ? 1 : 0); };
            Action<PhantoonEnemyState> begin = system.PhantoonBehavior.Patterns.RunPhantoonPickFirstRoundPattern;
            Action<RoomEnemySlot, PhantoonEnemyState> tick = system.PhantoonBehavior.Patterns.RunPhantoonFirstRoundFigureEight;
            // The duration bucket is the enemy frame's NMI counter bits 1-2, not a random draw.
            system.FrameContext.SeedFrame((ushort)(bucket << 1), InstalledFixtures.CreateProjectileSystem(), new SuperMetroid.Core.Samus.Palettes.SamusColours());
            body.Position = body.Position.WithWhole(new(128, 96)); state.BodyState.PhaseTimer = 1; state.BodyState.FlameRainRequested = true;
            begin(state);
            ushort duration = ReadVerificationWord(rom, 0xa7cd53 + bucket * 2);
            AssertEqual(duration, state.EyeState.ClosedTimer, "Native first-round closed-eye duration");
            AssertEqual(reversed, state.EyeState.FigureEightReversed, "Direction selection remains independent of duration bucket");
            // A valid unexpired casual-flame hold keeps this fixture specific to
            // the identified closed-eye countdown and its actual expiry producer.
            state.MouthState.CasualFlameHold = duration + 1;
            for (int elapsed = 1; elapsed <= duration; elapsed++)
            {
                tick(body, state); totalTicks++;
                AssertEqual((ushort)(duration - elapsed), state.EyeState.ClosedTimer, "Closed-eye timer decrements once despite movement speed/cursor");
                if (elapsed < duration)
                {
                    AssertEqual(PhantoonPhase.MoveInFigureEightThenOpenEye, state.BodyState.Phase, "No early eye opening");
                    AssertEqual(0, system.Projectiles.EnemyProjectiles.Count(projectile => projectile.IsActive), "No expiry spiral before selected duration");
                }
            }
            AssertEqual((PhantoonPhase?)null, state.BodyState.Phase, "Selected duration stops moving phase");
            AssertEqual(PhantoonPartSegment.EyeOpen, state.PlayingProgram(eye), "Expiration starts exact eye-opening program");
            AssertEqual((ushort)1, eye.AnimationClock.Hold.RawValue, "Eye-opening program executes on next instruction step");
            AssertEqual(false, state.BodyState.FlameRainRequested, "Opening clears flame-rain trigger");
            AssertEqual(1, state.MouthState.CasualFlameHold, "Casual hold still advances on every movement call");
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

    /// <summary>
    /// Compares the Ceres platform's strips against retail words (#1467): the platform actor draws the light or dark strip's
    /// picture by its phase, and the import pictures each strip from exactly the cartridge's four cells.
    /// </summary>
    internal static void VerifyLookupStream5CeresPlatform(CartridgeImportAddressSpace rom)
    {
        var document = new CeresDoorVisualDocument
        {
            Version = 1,
            Normal = Colors(CeresSourceDefinitions.Door.NormalColors,15),
            Escape = Colors(CeresSourceDefinitions.Door.EscapeColors,15),
            Animation = Enumerable.Range(0,8).Select(row => Colors(CeresSourceDefinitions.Door.AnimationColors +16*row,6)).ToArray(),
        };
        byte[] planar = Enumerable.Range(0,CeresDoorVisualFormat.CharacterBytes)
            .Select(index => rom.ReadCartridgeByte(CeresSourceDefinitions.Door.Characters+index)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar,4,RoomCharacterAtlasFormat.TileColumns,out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png,size,pixels,SnesGraphics.DiagnosticPalette(16));
        byte[] pngBytes = png.ToArray();
        var stock = CeresDoorVisualFiles.Read(new MemoryStream(pngBytes),new MemoryStream(CeresDoorVisualFiles.Write(document)));
        CeresFloorAnimationFrame[] strips = [CeresFloorAnimationFrame.ElevatorLight, CeresFloorAnimationFrame.ElevatorDark];
        for (int frame = 0; frame < strips.Length; frame++)
        {
            IReadOnlyDictionary<int, int> cells = CeresFloorDrawings.Cells(strips[frame]);
            for (int cell = 0; cell < 4; cell++)
                AssertEqual<int>(rom.ReadCartridgeByte(CeresSourceDefinitionsDoorConstants.PlatformFrames + frame * 4 + cell), cells[12 * 128 + 14 + cell],
                    "The import pictures the platform strip's native cells");
        }
        RoomFloor floor = RoomBackgroundsFixtures.CeresFloor();
        var system = new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() })), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new StandaloneRoomEnemyServices(() => 0) { Floor = floor });
        var slots = (RoomEnemySlot[])system.SlotTable.Slots;
        foreach (ushort tick in new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 0xfffe, 0xffff, 0 })
        {
            slots[0].FrameCounter = tick;
            system.CeresDoorBehavior.RunCeresDoorPaletteAnimation(slots[0]);
            AssertEqual(strips[(tick & 2) >> 1], floor.Shown[CeresFloorPart.ElevatorPlatform], "Actual platform phase draws its strip over the floor");
        }
        string identity = SelectedPresentationHash.Create("enemy-ceres-door-v1",content =>
        {
            content.Append("tiles",planar);
            content.AppendWords("normal",document.Normal.Select(Pack).ToArray());
            content.AppendWords("escape",document.Escape.Select(Pack).ToArray());
            content.AppendWordFrames("animation",document.Animation.Select(row=>row.Select(Pack).ToArray()).ToArray());
        });
        AssertEqual(identity,stock.ContentIdentity,"Ceres calculated colors preserve canonical resource identity");
        Console.WriteLine("Ceres platform: eight native cells pictured, actual phase calls through wrap draw their strips, canonical hash passes.");
        PaletteRgb5[] Colors(int source,int count) => Enumerable.Range(0,count).Select(index =>
        {
            ushort word = ReadVerificationWord(rom,source+2*index);
            return new PaletteRgb5 { Red = word &31, Green = (word>>5)&31, Blue = (word>>10)&31 };
        }).ToArray();
        static ushort Pack(PaletteRgb5 color) => (ushort)(color.Red | color.Green<<5 | color.Blue<<10);
    }
    /// <summary>Checks Phantoon's exposed-body phase and related authored visual state.</summary>
    internal static void VerifyLookupStream5PhantoonExposure(CartridgeImportAddressSpace rom)
    {
        int ticks = 0;
        foreach (int bucket in new[] { 2, 1, 0 })
        foreach (bool shot in new[] { false, true })
        {
            var system = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
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
            AssertEqual(duration, state.BodyState.PhaseTimer, "Native selected vulnerability duration");
            AssertTrue(!body.Contact.HasAny(EnemyContact.IgnoresSamus), "Eye opening enables collision");
            state.TentaclesState.SwoopRequested = shot;
            state.TentaclesState.RoundDamage = 37;
            var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(new(192, 96)) };
            for (int elapsed = 1; elapsed <= duration; elapsed++)
            {
                tick(body, state, samus); ticks++;
                AssertEqual((ushort)128, body.Position.X.Whole, "Exposure duration does not advance a spatial trajectory");
                AssertEqual((ushort)96, body.Position.Y.Whole, "Exposure body remains at current Y");
                if (elapsed < duration)
                {
                    AssertEqual((ushort)(duration - elapsed), state.BodyState.PhaseTimer, "One exact countdown decrement per actor call");
                    AssertEqual(PhantoonPhase.EyeTracksSamus, state.BodyState.Phase, "No early exposure transition");
                    AssertTrue(!body.Contact.HasAny(EnemyContact.IgnoresSamus), "Exposure remains collidable before expiry");
                    AssertEqual(37, state.TentaclesState.RoundDamage, "Round damage remains until expiration");
                }
            }
            AssertEqual(0, state.TentaclesState.RoundDamage, "Expiration resets accumulated round damage");
            if (shot)
            {
                AssertEqual(PhantoonPhase.BecomeSolidAndSwoop, state.BodyState.Phase, "Shot-triggered expiration selects swoop");
                AssertEqual(ReadVerificationWord(rom, 0xa7d620), state.BodyState.PhaseTimer, "Separate native swoop setup hold preserved");
                AssertEqual(false, state.TentaclesState.SwoopRequested, "Expiration consumes swoop request");
            }
            else
            {
                AssertEqual((PhantoonPhase?)null, state.BodyState.Phase, "Unhit expiration hands off to eye close");
                AssertEqual(PhantoonPartSegment.InvulnerableBody, state.PlayingProgram(body), "Native invulnerable body handoff");
                AssertEqual(PhantoonPartSegment.EyeCloseAndPickNewPattern, state.PlayingProgram(eye), "Native close/reselect eye handoff");
                AssertTrue(body.Contact.HasAny(EnemyContact.IgnoresSamus), "Expiration removes open-eye collision");
                AssertEqual(true, state.BodyState.FlameRainRequested, "Unhit expiration requests next rain branch");
            }
        }
        AssertEqual(210, ticks, "All three actual exposure lengths and both expiry branches");
        Console.WriteLine("Phantoon exposure: three native window lengths,210 actual countdown calls, exact collision/shot/close transitions and separate swoop hold pass.");
    }
    /// <summary>Compares Ceres overlay tile words and their installed draw order with retail data.</summary>
    internal static void VerifyLookupStream5CeresOverlayWords(CartridgeImportAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Areas.Ceres.CeresEscapeOverlayTilemapFiles.Extract(rom);
        var document = RequiredJson.Deserialize<CeresEscapeOverlayTilemapDocument>(json, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        CeresEscapeOverlays stock = Check(document);
        int words = 0;
        foreach (var page in CeresEscapeOverlayPages.All)
        {
            AssertTrue(stock.TryResolve(page, out ReadOnlyMemory<BackgroundTile> installed), "Every subtitle page is installed");
            for (int index = 0; index < page.WordCount; index++)
            {
                ushort native = ReadVerificationWord(rom, CeresEscapeNativeSources.OverlaySource(page) + index * 2);
                AssertEqual(native, installed.Span[index].PackedWord, "Native installed subtitle glyph/style");
                words++;
            }
            AssertTrue(!stock.TryResolveNative(CeresEscapeNativeSources.OverlaySource(page), page.WordCount * 2 - 1, out _), "Changed transfer extent rejected");
        }
        AssertTrue(!stock.TryResolve(default, out _), "Unknown subtitle page rejected");
        AssertEqual(55, words, "Native subtitle word count");
        Console.WriteLine("Ceres overlay glyphs: 55 native words, exact original hash and transfer domains pass.");

        CeresEscapeOverlays Check(CeresEscapeOverlayTilemapDocument source)
        {
            var result = CeresEscapeOverlays.Load(new MemoryStream(CeresEscapeOverlays.Write(source)));
            var system = new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() })), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new SuperMetroid.Verification.Enemies.StandaloneRoomEnemyServices(() => throw new InvalidOperationException("The warning fixture generates no random numbers.")));
            system.CeresRidleyBehavior.Getaway.DrawCeresEmergencyText();
            int first = ReadVerificationWord(rom, 0xa6c162) - GameplayVramLayout.Bg1TilemapWord;
            int count = ReadVerificationWord(rom, 0xa6c15d) / sizeof(ushort);
            // The getaway writes the title's picture by name (#1466); the words it stands for are the page's.
            System.Collections.Immutable.ImmutableArray<int> shownWords = system.RidleyEncounter.CeresWarning.WordsIn(result);
            AssertEqual(count, shownWords.Count(word => word != RoomWrittenOverlay.Unwritten), "Native emergency word extent");
            AssertEqual(source.Pages["emergency"].Length, count, "Emergency page keeps its native extent");
            for (int index = 0; index < count; index++)
                AssertEqual(source.Pages["emergency"][index].PackedWord, shownWords[first + index], "Actual shown emergency words");
            foreach (var page in CeresEscapeOverlayPages.All)
            {
                byte[] expected = new byte[page.WordCount * 2];
                for (int index = 0; index < page.WordCount; index++)
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(index * 2), source.Pages[page.Name][index].PackedWord);
                AssertTrue(result.TryResolve(page, out var actual) && BackgroundTileWordBytes.Of(actual.Span).AsSpan().SequenceEqual(expected), "Glyph words and neighboring data");
            }
            // The pages are keyed by name since #1415, not by cartridge source, each framed by its tiles' fields (#1422).
            string expectedIdentity = SelectedPresentationHash.Create("enemy-ceres-escape-overlay-v2", content =>
            {
                content.Append("shared", ReadOnlySpan<byte>.Empty);
                foreach (var page in CeresEscapeOverlayPages.All)
                {
                    content.Append("source", System.Text.Encoding.UTF8.GetBytes(page.Name));
                    content.AppendTiles("tiles", source.Pages[page.Name]);
                }
            });
            AssertEqual(expectedIdentity, result.ContentIdentity, "Overlay hash framing by page name");
            return result;
        }
    }
    /// <summary>Checks Crocomire wall and spike palette transfers where their CGRAM destinations overlap.</summary>
    internal static void VerifyLookupStream5CrocomireSharedPaint(CartridgeImportAddressSpace rom)
    {
        int[] sources = [CrocomireSourceDefinitions.FightBodyColors, CrocomireSourceDefinitions.InitialWallColors,
            CrocomireSourceDefinitions.InitialProjectileColors, CrocomireSourceDefinitions.SkeletonArmColors, CrocomireSourceDefinitions.WallSpikesColors];
        // The initial projectile band is sprite set 5's sixteen colours: the port does not carry the transfer's seventeenth
        // word, which lands on sprite set 6's transparent colour 0 (#1436).
        int[] counts = [8, 17, 16, 16, 16];
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
        _ = Check();
        Console.WriteLine("Crocomire installed colours: 73 native colors, actual CGRAM, hash and bounds pass.");

        CrocomireColorCatalog Check()
        {
            var document = new CrocomireColorDocument { Version = 1, FightBody = bands[0], InitialWall = bands[1], InitialProjectile = bands[2], SkeletonArm = bands[3], WallSpikes = bands[4] };
            var result = CrocomireColorFile.Read(new MemoryStream(CrocomireColorFile.Write(document)));
            Func<int, RgbColor>[] resolve = [result.ResolveFightBody, result.ResolveInitialWall, result.ResolveInitialProjectile, result.ResolveSkeletonArm, result.ResolveWallSpikes];
            // The encounter's first load shows the wall's colours on the spawned objects' sheets (#1512): the import's wall
            // shade holds the initial wall's first sixteen colours in the wall's row.
            SheetShade wall = BossSheetShades.SpawnedObjects(InstalledFixtures.BossArtwork with { CrocomireColors = result }, EnemyDefinitionId.Crocomire)
                .Single(shade => shade.Shade == SuperMetroid.Core.Bosses.Crocomire.Artwork.CrocomireShades.Wall);
            for (int ink = 0; ink < wall.Colours.Length; ink++)
                AssertEqual(resolve[1](ink), wall.Colours[ink], "Initial wall load shows the supplied colours");
            string expectedHash = SelectedPresentationHash.Create("CrocomireColorCatalog-v1", content =>
            {
                for (int band = 0; band < bands.Length; band++)
                {
                    ushort[] words = new ushort[counts[band]];
                    for (int color = 0; color < words.Length; color++)
                    {
                        PaletteRgb5 rgb = bands[band][color];
                        words[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
                        AssertEqual(words[color], resolve[band](color), "Native word");
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
        // Core's pages carry their extent and file (#1415); extraction's sources place them where the catalog did before.
        TileSheetDefinition[] tilePages = [new(0x900, "ceres-escape-warning-tiles.png"), new(0x600, "ceres-escape-door-tiles.png")];
        AssertTrue(tilePages.SequenceEqual(CeresEscapeTileArtworkDefinitions.All), "Original named character page order/extent/file");
        int[] tileSources = [0xb7da00, 0xb0ba00];
        for (int index = 0; index < tilePages.Length; index++)
        {
            TileSheetDefinition page = tilePages[index];
            int source = tileSources[index];
            AssertEqual(source, CeresEscapeNativeSources.PageSource(index), "Extraction reads the page from its original source");
            AssertTrue(CeresEscapeTileArtworkDefinitionsTooling.Contains(source, page.ByteCount), "Whole page admitted");
            AssertTrue(CeresEscapeTileArtworkDefinitionsTooling.Contains(source + page.ByteCount - 1, 1), "Last byte admitted");
            AssertTrue(!CeresEscapeTileArtworkDefinitionsTooling.Contains(source - 1, 1) && !CeresEscapeTileArtworkDefinitionsTooling.Contains(source + page.ByteCount, 1), "Outside page rejected");
            AssertTrue(!CeresEscapeTileArtworkDefinitionsTooling.Contains(source, 0), "Zero byte page rejected");
        }
        // Each picture's corner is the room pixel of the foreground word the cartridge writes it at.
        CeresEscapeWarningPicture[] overlays = [new("emergency", CornerOfWord(0x0cb)), new("japanese_0", CornerOfWord(0x28a)), new("japanese_1", CornerOfWord(0x2aa)), new("japanese_2", CornerOfWord(0x2ca)), new("japanese_3", CornerOfWord(0x2ea))];
        AssertTrue(overlays.SequenceEqual(CeresEscapeOverlayPages.All), "Original named overlay order/place");
        AssertTrue(overlays.Select(page => page.WordCount).SequenceEqual([9, 12, 12, 11, 11]), "Original overlay extents");
        int[] overlaySources = [0xa6c164, 0xa6c3f4, 0xa6c40c, 0xa6c424, 0xa6c43a];
        for (int index = 0; index < overlays.Length; index++)
        {
            CeresEscapeWarningPicture page = overlays[index];
            int source = overlaySources[index];
            AssertEqual(source, CeresEscapeNativeSources.OverlaySource(page), "Extraction reads the overlay from its original source");
            AssertTrue(CeresEscapeWarningPicturesTooling.IsSource(source, page.WordCount * 2), "Exact overlay extent admitted");
            AssertTrue(!CeresEscapeWarningPicturesTooling.IsSource(source, page.WordCount * 2 - 1), "Partial overlay extent rejected");
            AssertTrue(CeresEscapeWarningPictures.ContainsByteAddress(source) && CeresEscapeWarningPictures.ContainsByteAddress(source + page.WordCount * 2 - 1), "Overlay byte endpoints admitted");
        }
        AssertThrows<IndexOutOfRangeException>(() => _ = CeresEscapeTileArtworkDefinitions.All[-1], "Tile page lower bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = CeresEscapeTileArtworkDefinitions.All[2], "Tile page upper bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = CeresEscapeOverlayPages.All[-1], "Overlay lower bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = CeresEscapeOverlayPages.All[5], "Overlay upper bound");
        Console.WriteLine("Ceres source page cases: two character sheets/five overlays preserve exact identities, order, extents, enumeration and range domains.");

        // The foreground pixel of the top-left corner of foreground word `word`.
        static Position16<PlanePixels> CornerOfWord(int word) => SuperMetroid.Core.Rooms.Backgrounds.TwoScreenTilemapCells.CornerAt(word);
    }
    /// <summary>Checks authored area-map landmark tiles and their native reveal cases.</summary>
    internal static void VerifyLookupStream5MapLandmarkCases(CartridgeImportAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Frontend.Map.MapLandmarkExtractor.Extract(rom);
        MapLandmarkPositions layout = MapLandmarkFile.Read(new MemoryStream(extracted));
        var artwork = SuperMetroid.AssetExtraction.Frontend.Map.MapSpriteExtractor.Extract(rom);
        MapSpriteCompositions sprites = MapSpriteArtwork.Read(new MemoryStream(artwork[MapSpriteFormat.JsonFile]), new MemoryStream(artwork[MapSpriteFormat.PngFile]));
        int Read(int pointer) => ReadVerificationWord(rom, MapSourceDefinitions.Menu.ObjectBank | pointer);
        int Root(int table, AreaId area) => ReadVerificationWord(rom, table + SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(area) * 2);
        string[] expectedIds = ["Crateria.Elevator.ToBrinstarWest", "Crateria.Elevator.ToBrinstarCentral", "Crateria.Elevator.ToBrinstarEast",
            "Crateria.Elevator.ToWreckedShip", "Crateria.Elevator.ToMaridia",
            "Boss.Kraid", "Brinstar.Elevator.ToCrateriaWest", "Brinstar.Elevator.ToCrateriaCentral", "Brinstar.Elevator.ToCrateriaEast",
            "Brinstar.Elevator.ToMaridia", "Brinstar.Elevator.ToNorfair",
            "Boss.Ridley", "Norfair.Elevator.ToBrinstar", "Boss.Phantoon", "WreckedShip.Elevator.ToCrateriaWest", "WreckedShip.Elevator.ToCrateriaEast",
            "Boss.Draygon", "Maridia.Elevator.ToCrateria", "Maridia.Elevator.ToBrinstarWest", "Maridia.Elevator.ToBrinstarEast",
            "Tourian.Elevator.ToCrateria", "Boss.CeresRidley", "Crateria.Gunship"];
        AssertTrue(expectedIds.SequenceEqual(MapLandmarkFile.Ids()), "Original public identity and manifest order");
        int bossSlots = 0, elevatorSlots = 0;
        foreach (AreaId area in AreaIds.All)
        {
            var markers = MapLandmarks.BossMarkers(area);
            int pointer = Root(MapSourceDefinitions.Landmarks.BossLists, area);
            int count = 0, shown = 0;
            if (pointer != 0)
                while (Read(pointer + count * 4) != ushort.MaxValue)
                {
                    if (Read(pointer + count * 4) != 0xfffe)
                    {
                        // The cartridge's first slot shows the area boss's flag.
                        AssertEqual(0, count, "Native used boss slot is the area boss's");
                        MapBossMarker marker = markers[shown++];
                        AssertEqual(DefeatedBosses.AreaBoss, marker.Boss, "Named marker shows the area boss");
                        AssertEqual(Read(pointer + count * 4), layout.Get(marker.Id).X, "Native semantic boss X");
                        AssertEqual(Read(pointer + count * 4 + 2), layout.Get(marker.Id).Y, "Native semantic boss Y");
                    }
                    count++;
                }
            AssertEqual(shown, markers.Length, "Every named marker stands in a used native slot");
            bossSlots += count;
            if (area == AreaId.Ceres) continue;
            var elevators = MapLandmarks.Elevators(area);
            pointer = Root(MapSourceDefinitions.Landmarks.ElevatorLists, area);
            count = 0;
            while (Read(pointer + count * 6) != ushort.MaxValue)
            {
                var label = elevators[count];
                AssertEqual(Read(pointer + count * 6 + 4), MapSpriteOrdinals.Spritemaps.Word(MapLandmarks.ElevatorPicture(label.Destination)), "Native destination identity");
                AssertEqual(Read(pointer + count * 6), layout.Get(label.Id).X, "Native elevator X");
                AssertEqual(Read(pointer + count * 6 + 2), layout.Get(label.Id).Y, "Native elevator Y");
                count++;
            }
            AssertEqual(count, elevators.Length, "Native elevator count");
            elevatorSlots += count;
        }
        foreach (AreaId area in AreaIds.All)
        foreach (bool downloaded in new[] { false, true })
        foreach (bool defeated in new[] { false, true })
        {
            var system = new SharedGameState(new SharedRandom());
            if (downloaded) system.SetAreaMapAcquired(area);
            if (defeated) system.RecordBossesDefeated(area, DefeatedBosses.AreaBoss);
            var icons = new FileSelectMapIcons(system, area);
            icons.BindLandmarks(layout); icons.BindSprites(MenuSheetFixtures.MapSprites(sprites));
            var actual = ScreenSpritesFixtures.Blank(); var expected = ScreenSpritesFixtures.Blank();
            icons.DrawBossMarkers(actual, new(7, 9));
            int pointer = Root(MapSourceDefinitions.Landmarks.BossLists, area), bits = defeated ? 1 : 0;
            if (pointer != 0)
                for (int i = 0; Read(pointer + i * 4) != ushort.MaxValue; i++, bits >>= 1)
                {
                    int x = Read(pointer + i * 4), y = Read(pointer + i * 4 + 2);
                    if (x == 0xfffe) continue;
                    if ((bits & 1) != 0)
                    {
                        Draw(MapSpriteId.MarkerDefeatedBoss, x, y, MapSpriteColouring.Markers);
                        Draw(MapSpriteId.MarkerBoss, x, y, MapSpriteColouring.DefeatedBossMarker);
                    }
                    else if (downloaded) Draw(MapSpriteId.MarkerBoss, x, y, MapSpriteColouring.Markers);
                }
            if (area != AreaId.Ceres)
            {
                icons.DrawAfterMarker(actual, new(7, 9));
                if (area == AreaId.Crateria)
                {
                    int ship = Root((int)FrontendSourceDefinitions.SavePointMapPointers, area);
                    Draw(MapSpriteId.MarkerGunship, Read(ship), Read(ship + 2), MapSpriteColouring.Markers);
                }
                if (downloaded)
                {
                    pointer = Root(MapSourceDefinitions.Landmarks.ElevatorLists, area);
                    for (int i = 0; Read(pointer + i * 6) != ushort.MaxValue; i++)
                        Draw(MapSpriteOrdinals.Spritemaps.Decode((ushort)Read(pointer + i * 6 + 4), "map spritemap ordinal"), Read(pointer + i * 6), Read(pointer + i * 6 + 2), MapSpriteColouring.Labels);
                }
            }
            actual.FinishFrame(); expected.FinishFrame();
            AssertTrue(actual.ImageEquals(expected), "Actual native ordered landmark OAM");
            AssertEqual(defeated ? DefeatedBosses.AreaBoss : DefeatedBosses.None, system.BossesDefeatedIn(area), "Drawing preserves boss bits");
            AssertEqual(downloaded, system.HasAreaMap(area), "Drawing preserves map state");
            void Draw(MapSpriteId id, int x, int y, MapSpriteColouring colouring) => MapSpriteSheet.Draw(MenuSheetFixtures.MapSprites(sprites), id, expected, new(unchecked((ushort)(x - 7)), unchecked((ushort)(y - 9))), MapSpriteScreen.FileSelect, colouring);
        }
        AssertThrows<ArgumentOutOfRangeException>(() => MapLandmarks.BossMarkers((AreaId)7), "Invalid boss area");
        AssertThrows<ArgumentOutOfRangeException>(() => MapLandmarks.Elevators(AreaId.Ceres), "Ceres has no elevator labels");
        AssertEqual(8, bossSlots, "Eight native boss slots including three unused");
        AssertEqual(17, elevatorSlots, "Seventeen native destination labels");
        Console.WriteLine("Map landmark cases: eight native boss slots, seventeen destinations, extraction/schema, 23 ordered IDs, bounds and 28 stock ordered OAM/state cases pass.");
    }
    /// <summary>Checks Phantoon's vulnerable collision state against its native phase data.</summary>
    internal static void VerifyLookupStream5PhantoonCollision(CartridgeImportAddressSpace rom)
    {
        int componentCount = 0;
        var lists = new HashSet<ushort>();
        foreach (EnemyFrameName frame in PhantoonBg2FrameDefinitions.Frames)
        {
            int root = 0xa70000 | frame.Pointer;
            var components = PhantoonCollision.ComponentsAt(frame.Pointer);
            AssertEqual(ReadVerificationWord(rom, root), components.Length, "Native component extent");
            for (int index = 0; index < components.Length; index++)
            {
                int record = root + 2 + index * 8;
                EnemyCollisionComponent component = components[index];
                AssertEqual(unchecked((short)ReadVerificationWord(rom, record)), component.Offset.X, "Native component X");
                AssertEqual(unchecked((short)ReadVerificationWord(rom, record + 2)), component.Offset.Y, "Native component Y");
                componentCount++;
                // The ROM names the component's hitbox list by address; the oracle decodes the list it names, and
                // every compiled component must hold exactly those rectangles and reactions, so a component holding
                // another list fails.
                ushort listPointer = ReadVerificationWord(rom, record + 6);
                lists.Add(listPointer);
                int address = 0xa70000 | listPointer;
                IReadOnlyList<EnemyCollisionHitbox> hitboxes = component.Hitboxes;
                AssertEqual(ReadVerificationWord(rom, address), hitboxes.Count, "Native hitbox extent");
                for (int hitbox = 0; hitbox < hitboxes.Count; hitbox++)
                {
                    int native = address + 2 + hitbox * 12;
                    LookupStream5Expected expected = new LookupStream5Expected(unchecked((short)ReadVerificationWord(rom, native)),
                        unchecked((short)ReadVerificationWord(rom, native + 2)), unchecked((short)ReadVerificationWord(rom, native + 4)),
                        unchecked((short)ReadVerificationWord(rom, native + 6)),
                        EnemyReactionAccess.Of(0xa7, ReadVerificationWord(rom, native + 8), shot: false),
                        EnemyReactionAccess.Of(0xa7, ReadVerificationWord(rom, native + 10), shot: true));
                    EnemyCollisionHitbox actual = hitboxes[hitbox];
                    LookupStream5Expected compiled = new LookupStream5Expected(actual.Left, actual.Top, actual.Right, actual.Bottom,
                        actual.Touch, actual.Shot);
                    AssertEqual(expected, compiled, "Native shape and callback pair");
                }
            }
            AssertEqual(components.Length, components.ToArray().Length, "Component enumeration compatibility");
            AssertThrows<IndexOutOfRangeException>(() => _ = components[components.Length], "Component upper bound");
        }
        AssertEqual(25, componentCount, "All native components");
        AssertEqual(3, lists.Count, "All native hitbox lists");
        AssertEqual((EnemyReaction?)PhantoonReactions.Shot, FindPhantoonHitboxReaction(null, (ushort)PhantoonCollisionFrame.BodyFullHitbox, new(128, 128), true), "Actual full-body shot callback");
        AssertEqual((EnemyReaction?)PhantoonReactions.Touch, FindPhantoonHitboxReaction(null, (ushort)PhantoonCollisionFrame.BodyFullHitbox, new(128, 128), false), "Actual full-body touch callback");
        AssertEqual((EnemyReaction?)PhantoonReactions.Shot, FindPhantoonHitboxReaction(null, (ushort)PhantoonCollisionFrame.BodyEyeHitboxOnly, new(128, 153), true), "Actual vulnerable eye callback");
        AssertEqual<EnemyReaction?>(null, FindPhantoonHitboxReaction(null, (ushort)PhantoonCollisionFrame.BodyEyeHitboxOnly, new(128, 128), true), "Eye-only frame excludes body collision");
        AssertThrows<InvalidDataException>(() => PhantoonCollision.ComponentsAt(0), "Frame domain");
        Console.WriteLine("Phantoon collision: 22 native frames, 25 components, all seven rectangles/callbacks, actual full-body/eye selection and bounds pass.");
    }
    /// <summary>Compares Phantoon fade color rows and transition timing with cartridge palettes.</summary>
    internal static void VerifyLookupStream5PhantoonFade(CartridgeImportAddressSpace rom)
    {
        byte[] bytes = SuperMetroid.AssetExtraction.Bosses.PhantoonColorExtractor.Extract(rom);
        PhantoonColorDocument document = RequiredJson.Deserialize<PhantoonColorDocument>(bytes,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
        PhantoonColorCatalog stock = Check(document);
        for (int color = 0; color < PhantoonColorFormat.FadeOutCount; color++)
        {
            AssertEqual(ReadVerificationWord(rom, PhantoonSourceDefinitions.FadeOutColors + color * 2), stock.ResolveFadeOut(color), "Native black fade endpoint");
        }
        for (int band = 0; band < PhantoonColorDefinitions.HealthBandCount; band++)
        for (int color = 0; color < PhantoonColorFormat.HealthBandColorCount; color++)
        {
            AssertEqual(ReadVerificationWord(rom, PhantoonSourceDefinitions.HealthBandColors + (band * 16 + color) * 2), stock.ResolveHealth(band, color), "Native health RGB equality");
        }
        for (int color = 0; color < PhantoonSourceDefinitions.PowerOnColourCount; color++)
        {
            AssertEqual(ReadVerificationWord(rom, PhantoonSourceDefinitions.PowerOnColors + color * 2), stock.ResolvePowerOnAtNative(color), "Native power shade equality");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolvePowerOn(WreckedShipPoweredPart.Shared, -1), "Power lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolvePowerOn(WreckedShipPoweredPart.Lights, 16), "Power upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolvePowerOn((WreckedShipPoweredPart)Enum.GetValues<WreckedShipPoweredPart>().Length, 0), "Power rejects a part the power-on does not fade");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveHealth(-1, 0), "Health band lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveHealth(8, 0), "Health band upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveHealth(0, -1), "Health color lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveHealth(0, 16), "Health color upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveFadeOut(-1), "Fade lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveFadeOut(16), "Fade upper bound");
        Console.WriteLine("Phantoon colors: 16 native fade targets, 128 health colors, 112 power colors, the hash and bounds pass.");

        static PhantoonColorCatalog Check(PhantoonColorDocument source)
        {
            PhantoonColorCatalog result = PhantoonColorFile.Read(new MemoryStream(PhantoonColorFile.Write(source), writable: false));
            ushort[] Pack(PaletteRgb5[] colors) => colors.Select(c => (ushort)(c.Red | c.Green << 5 | c.Blue << 10)).ToArray();
            ushort[] fade = Pack(source.FadeOut);
            for (int color = 0; color < fade.Length; color++) AssertEqual(fade[color], result.ResolveFadeOut(color), "Fade input");
            for (int band = 0; band < source.HealthBands.Length; band++)
            {
                ushort[] values = Pack(source.HealthBands[band]);
                for (int color = 0; color < values.Length; color++) AssertEqual(values[color], result.ResolveHealth(band, color), "Health channels");
            }
            ushort[] power = Pack(source.PowerOn);
            for (int color = 0; color < power.Length; color++) AssertEqual(power[color], result.ResolvePowerOnAtNative(color), "Power paint preserved");
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
    internal static void VerifyLookupStream5MapButtons(CartridgeImportAddressSpace rom)
    {
        var native = new ControllerButton[4];
        for (int i = 0; i < native.Length; i++)
        {
            native[i] = ControllerWordCodec.FromJoypadWord(ReadVerificationWord(rom, MapSourceDefinitions.FileSelectMap.ScrollArrows + i * 10 + 6), "native map arrow mask");
            AssertEqual(i + 1, (int)ReadVerificationWord(rom, MapSourceDefinitions.FileSelectMap.ScrollArrows + i * 10 + 8), "Native direction identity");
            AssertEqual(native[i], MapScrollControls.ButtonFor((MapScrollDirection)(i + 1)), "Native directional mask");
            AssertEqual(native[i], MapScrollControls.Buttons[i], "Calculated ordered view");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => MapScrollControls.ButtonFor(MapScrollDirection.None), "No direction has no binding");
        AssertThrows<ArgumentOutOfRangeException>(() => MapScrollControls.ButtonFor((MapScrollDirection)5), "Invalid direction");
        var state = new SharedGameState(new SharedRandom());
        state.MarkExploredMapTile(AreaId.Crateria, new(0, 0));
        state.MarkExploredMapTile(AreaId.Crateria, new(63, 31));
        var map = new LookupStream5ScrollMap();
        for (int selected = 0; selected < 4; selected++)
        {
            var stock = new FileSelectMapScroll(map, state, new(240, 128));
            ushort startX = stock.Scroll.X, startY = stock.Scroll.Y;
            for (int tick = 1; tick <= 8; tick++)
            {
                AssertEqual(tick == 8, stock.Step(tick == 1 ? native[selected] : ControllerButton.None), "Stock sound boundary");
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
        /// <summary>Supplies a blank cell so scroll assertions do not depend on map art.</summary>
        public MapCell Cell(Position16<MapTiles> cell) => default;
        /// <summary>Admits every fixture cell for scroll selection.</summary>
        public bool IsDiscoverable(Position16<MapTiles> cell) => true;
        /// <summary>Leaves station-driven visibility out of this scroll fixture.</summary>
        public bool IsRevealedByMapStation(Position16<MapTiles> cell) => false;
        /// <summary>Disables the special upper-cell reveal path in this fixture.</summary>
        public bool RevealsCellAbove(Position16<MapTiles> cell) => false;
    }

    /// <summary>Compares Zebes starfield sprite positions and attributes with the native OAM composition.</summary>
    internal static void VerifyLookupStream5ZebesStarFields(CartridgeImportAddressSpace rom)
    {
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        foreach (var definition in CeresDestructionSpriteFrameDefinitions.Frames)
            frames.Add(definition.Name, SuperMetroid.AssetExtraction.Frontend.Intro.IntroCinematicSpriteFrameExtractor.Extract(
                rom, CinematicSpritemapSources.Of(definition.Pose), definition.StockPartCount, definition.Name));
        var document = new CeresDestructionSpriteDocument { Version = CeresDestructionSpriteFormat.Version, Frames = frames };
        var stock = Load(document);
        int total = 0;
        foreach (var definition in CeresDestructionSpriteFrameDefinitions.Frames)
        {
            if (!definition.Name.StartsWith("zebes-stars-", StringComparison.Ordinal)) continue;
            SpriteVisualPart[] parts = frames[definition.Name];
            Check(stock, definition.Pose, parts);
            var compiled = IntroCinematicSpriteCompiler.Compile(parts, definition.Name);
            for (int index = 0; index < parts.Length; index++)
            {
                int source = 0x8c0000 | CinematicSpritemapSources.Of(definition.Pose) + 2 + index * 5;
                CompiledSpritePart actual = compiled.Part(index);
                // The star has no packed record: the native five-byte record is decoded by the import pipeline's codec
                // and compared by offset, size, character, priority and mirroring, its palette row cleared because the
                // scene replaces it.
                var nativeStar = new SpritemapPart(new SnesSpritemapXWord(CartridgeImportSource.Require(rom).ReadCartridgeWord(source)),
                    new SnesSpritemapYByte(rom.ReadCartridgeByte(source + 2)),
                    new SnesObjAttributeWord(CartridgeImportSource.Require(rom).ReadCartridgeWord(source + 3)).WithPaletteIndex(0)).ToPart();
                AssertEqual(nativeStar, actual.Part, "Native star offset, size, character, priority and mirroring excluding replaced source palette");
                total++;
            }
        }
        AssertEqual(29, total, "All four native star sheets checked");
        Console.WriteLine("Zebes stars:29 native parts, actual ordered OAM and full content identity pass.");
        static CeresDestructionSpritePresentation Load(CeresDestructionSpriteDocument value)
        {
            using var json = new MemoryStream();
            CeresDestructionSpritePresentation.Write(json, value);
            json.Position = 0;
            var result = CeresDestructionSpritePresentation.Load(json);
            var expected = value.Frames.ToDictionary(pair => CeresDestructionSpriteFrameDefinitions.Frames
                .Single(definition => definition.Name == pair.Key).Pose,
                pair => IntroCinematicSpriteCompiler.Compile(pair.Value, pair.Key));
            AssertEqual(CinematicSpriteArtwork.ContentIdentity(nameof(CeresDestructionSpritePresentation), expected),
                result.ContentIdentity, "Stars retain canonical composition identity");
            return result;
        }
        static void Check(CeresDestructionSpritePresentation catalog, CinematicSpritePose pose, SpriteVisualPart[] parts)
        {
            var expected = IntroCinematicSpriteCompiler.Compile(parts, "star-reference");
            var actualOam = ScreenSpritesFixtures.Blank();
            var expectedOam = new OamBuffer();
            actualOam.BeginFrame(); expectedOam.BeginFrame();
            CinematicSheetFixtures.Drawer(catalog).Draw(pose, actualOam, new(128, 112), CeresShades.Drawn(CeresSpriteColouring.SpaceObjects));
            expected.DrawOnScreen(expectedOam, new(128, 112), 0x0800);
            actualOam.FinishFrame(); expectedOam.FinalizeFrame();
            AssertEqual(expectedOam.LastFinalizedSpriteCount, actualOam.LastFinalizedSpriteCount, "Star OAM count/order");
            AssertTrue(expectedOam.LowTable.SequenceEqual(actualOam.LowTable), "Exact star low OAM");
            AssertTrue(expectedOam.HighTable.SequenceEqual(actualOam.HighTable), "Exact star high OAM");
        }
    }
    /// <summary>Checks Tourian statue palette ramps and their CGRAM destinations against native colors.</summary>
    internal static void VerifyLookupStream5StatueRamps(CartridgeImportAddressSpace rom)
    {
        byte[] nativeJson = SuperMetroid.AssetExtraction.TourianStatueColorExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<TourianStatueColorDocument>(nativeJson,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var stock = Check(document);
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveEye(0, 4), "Eye color cannot cross into next row");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveStatue(16), "Statue upper bound");        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveBase(-1), "Base ramp lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ResolveGrey(8), "Grey ramp upper bound");
        Console.WriteLine("Tourian statue ramps: all56 native palette colors kept as installed, actual CGRAM and canonical identities pass.");

        static ushort Pack(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);

        static RgbColor Shown(PaletteRgb5 rgb) => new FadeLevelColour(rgb.Red, rgb.Green, rgb.Blue).ToColour();

        static TourianStatuePalettes Check(TourianStatueColorDocument expected)
        {
            var actual = TourianStatueColorFile.Read(new MemoryStream(TourianStatueColorFile.Write(expected), writable: false));
            // The installed colours the import pre-colours the statue scene's shades from (#1512).
            for (int color = 0; color < expected.Base.Length; color++)
            {
                AssertEqual(Pack(expected.Base[color]), actual.ResolveBase(color), "Exact base ramp color");
                AssertEqual(Shown(expected.Base[color]), actual.BaseColor(color), "Installed base colour");
                AssertEqual(Shown(expected.Statue[color]), actual.StatueColor(color), "Installed statue colour");
            }
            for (int color = 0; color < expected.Grey.Length; color++)
            {
                AssertEqual(Pack(expected.Grey[color]), actual.ResolveGrey(color), "Exact grey ramp color");
                AssertEqual(Shown(expected.Grey[color]), actual.GreyColor(color), "Installed grey colour");
            }
            for (int row = 0; row < expected.Eye.Length; row++)
            {
                for (int color = 0; color < expected.Eye[row].Length; color++)
                    AssertEqual(Shown(expected.Eye[row][color]), actual.ResolveEye(row, color), "Installed eye glow colour");
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

    /// <summary>Checks that every one of Phantoon's eight shot draws takes one random number and shortens his window.</summary>
    internal static void VerifyLookupStream5PhantoonMarkers()
    {
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes("Super Metroid.smc"))), "Marker reference ROM revision");
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
        var body = enemies.SlotTable.Slots[0];
        var state = new PhantoonEnemyState(body)
        {
            Eye = enemies.SlotTable.Slots[1], Tentacles = enemies.SlotTable.Slots[2], Mouth = enemies.SlotTable.Slots[3],
        };
        Action<RoomEnemySlot, PhantoonEnemyState, ushort, int> shot = (shotSlot, shotPhantoon, shotType, shotDamage) =>
            enemies.PhantoonBehavior.Combat.ResolvePhantoonShotReaction(shotSlot, shotPhantoon, SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(shotType), shotDamage);
        ushort random = 0;
        int calls = 0;
        enemies.VerificationServices().NextRandom = () => { calls++; return random; };
        for (int bucket = 0; bucket < 8; bucket++)
        {
            random = (ushort)(0xfff8 | bucket);
            body.Health = 1000;
            body.AiStates = EnemyAiStates.Hurt;
            state.BodyState.Phase = PhantoonPhase.EyeTracksSamus;
            state.BodyState.PhaseTimer = 60;
            state.TentaclesState.SwoopRequested = false;
            state.TentaclesState.RoundDamage = 0;
            shot(body, state, 0x100, 1);
            AssertEqual(16, state.BodyState.PhaseTimer, "Actual shot window");
            AssertEqual(bucket + 1, calls, "One preserved RNG call per shot");
        }
        Console.WriteLine("Phantoon shot draws: eight actual bus-free shots preserve RNG and timing.");
    }

    /// <summary>Checks Magdollite's palette pulse frames and the eight-frame update cadence.</summary>
    internal static void VerifyLookupStream5MagdollitePulse(CartridgeImportAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Enemies.MagdollitePaletteCycleExtractor.Extract(rom);
        var native = RequiredJson.Deserialize<MagdollitePaletteCycleDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        MagdollitePaletteCycle stock = Check(native);
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(-1, 0),
            "Magdollite pulse lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(4, 0),
            "Magdollite pulse upper frame bound");
        Console.WriteLine("Magdollite pulse:16 native colors, actual CGRAM, canonical identity and frame bounds pass; four independent glow colors remain pending.");

        static MagdollitePaletteCycle Check(MagdollitePaletteCycleDocument expected)
        {
            var actual = MagdollitePaletteCycleFile.Read(new MemoryStream(MagdollitePaletteCycleFile.Write(expected), writable: false));
            ushort[][] packed = expected.Frames.Select(row => row.Select(rgb =>
                (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10)).ToArray()).ToArray();
            for (int frame = 0; frame < expected.Frames.Length; frame++)
            {
                for (int color = 0; color < expected.Frames[frame].Length; color++)
                    AssertEqual(new FadeLevelColour(expected.Frames[frame][color].Red, expected.Frames[frame][color].Green, expected.Frames[frame][color].Blue).ToColour(),
                        actual.Resolve(frame, color), "Magdollite pulse exact installed glow colour");
            }
            string identity = SelectedPresentationHash.Create("MagdollitePaletteCycle-v1",
                content => content.AppendWordFrames("frames", packed));
            AssertEqual(identity, actual.ContentIdentity, "Magdollite pulse canonical identity");
            return actual;
        }
    }
    /// <summary>Checks Zebetite color pulses against the installed frame sequence and CGRAM positions.</summary>
    internal static void VerifyLookupStream5ZebetitePulse(CartridgeImportAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Bosses.MotherBrain.ZebetiteColorExtractor.Extract(rom);
        var native = RequiredJson.Deserialize<ZebetiteColorDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        ZebetiteColorCatalog stock = Check(native);
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(-1, 0),
            "Zebetite pulse lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(8, 0),
            "Zebetite pulse upper frame bound");
        Console.WriteLine("Zebetite pulse:16 native colors, actual CGRAM, canonical identity and frame bounds pass; selected barrier-core paint disposition complete.");

        static ZebetiteColorCatalog Check(ZebetiteColorDocument expected)
        {
            var actual = ZebetiteColorFile.Read(new MemoryStream(ZebetiteColorFile.Write(expected), writable: false));
            ushort[][] packed = expected.Frames.Select(row => row.Select(rgb =>
                (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10)).ToArray()).ToArray();
            for (int frame = 0; frame < packed.Length; frame++)
                for (int color = 0; color < packed[frame].Length; color++)
                    AssertEqual(packed[frame][color], actual.Resolve(frame, color).ToWord(),
                        "Zebetite pulse exact selected colour");
            string identity = SelectedPresentationHash.Create("ZebetiteColorCatalog-v1",
                content => content.AppendWordFrames("frames", packed));
            AssertEqual(identity, actual.ContentIdentity, "Zebetite pulse canonical identity");
            return actual;
        }
    }

    /// <summary>Compares Yapping Maw's animation offsets with native address and displacement tables.</summary>
    internal static void VerifyLookupStream5YappingMawOffsets(CartridgeImportAddressSpace rom)
    {
        Action<RoomEnemySlot,YappingMawEnemyState> begin = YappingMawBehavior.BeginYappingMawExtension;
        Action<YappingMawEnemyState,YappingMawAim> set = YappingMawBehavior.SetYappingMawHeldOffset;
        var slot = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom()).SlotTable.Slots[0];
        var state = new YappingMawEnemyState(slot) { DistanceToSamus = 32 };
        // The cartridge's held-offset table lists the aims clockwise from up, the order of the aim sequence.
        for (int direction = 0; direction < 8; direction++)
        {
            YappingMawAim aim = YappingMawAnimationDefinitions.Aims.At(direction);
            var offset = YappingMawAttackGeometry.HeldSamusOffset(aim);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa8a0a7 + direction * 4)), unchecked((ushort)offset.X), $"Native held X aiming {aim}");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa8a0a9 + direction * 4)), unchecked((ushort)offset.Y), $"Native held Y aiming {aim}");
            set(state, aim);
            Check(direction, "Actual held-offset callback");
        }
        for (ushort angle = 0; angle < 256; angle++)
        {
            state.AimAngle = new Bearing(checked((byte)angle));
            begin(slot, state);
            int direction = unchecked((byte)(angle + 16)) >> 5;
            AssertEqual(YappingMawAnimationDefinitions.Aims.At(direction), state.Aim,
                "Actual angle quantization/wrap retained");
            Check(direction, "Actual begin-extension held offset");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => YappingMawAttackGeometry.HeldSamusOffset((YappingMawAim)Enum.GetValues<YappingMawAim>().Length), "Held aim beyond the eight aims");
        Console.WriteLine("Yapping Maw offsets:16 native words, eight actual held callbacks,256 angle-quantized begin-extension writes and direction bounds pass.");

        void Check(int direction, string context)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa8a0a7 + direction * 4)), unchecked((ushort)state.HeldSamusOffset.X), context);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa8a0a9 + direction * 4)), unchecked((ushort)state.HeldSamusOffset.Y), context);
        }
    }

    /// <summary>Checks eye-door geometry and active hitbox dimensions against retail records.</summary>
    internal static void VerifyLookupStream5EyeGeometry(CartridgeImportAddressSpace rom)
    {
        for (int direction = 0; direction < 4; direction++)
        {
            var expected = new LookupStream5Expected2(unchecked((short)ReadVerificationWord(rom, 0xa890ca + direction * 2)),
                unchecked((short)ReadVerificationWord(rom, 0xa890d2 + direction * 2)),
                ReadVerificationWord(rom, 0xa890da + direction * 2));
            var mountSettings = (MorphBallEyeSpawnSettings)EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Eye, PopulationWords.Parameters(0, 0x8000 | direction));
            var mount = MorphBallEyeGeometry.Mount(mountSettings.MountSide);
            AssertEqual(expected, new LookupStream5Expected2(mount.Offset.X, mount.Offset.Y, NativeEnemyAnimationOracle.EntryOf(MorphBallEyeNativeListAddresses.Entries, mount.Animation)),
                "Native directional mount tuple");
            foreach (ushort origin in new ushort[] { 0, 65535 })
            {
                var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
                var slot = enemies.SlotTable.Slots[0];
                slot.Bind(EnemyDefinitionId.Eye, slot.Definition);
                slot.SetSpawnParameter2(EnemyDefinitionId.Eye, (ushort)(0x8000 | direction));
                slot.Position = slot.Position.WithWhole(new(origin, origin));
                new Action<RoomEnemySlot>(enemies.MorphBallEyeBehavior.InitializeMorphBallEye)(slot);
                AssertEqual(unchecked((ushort)(origin + expected.First)), slot.Position.X.Whole, "Actual mount X with native wrap");
                AssertEqual(unchecked((ushort)(origin + expected.Second)), slot.Position.Y.Whole, "Actual mount Y with native wrap");
                MorphBallEyeEnemyState eye = enemies.SlotTable.Slots[0].SpeciesState<MorphBallEyeEnemyState>() ?? throw new InvalidOperationException("The mount has no state.");
                AssertEqual(new AnimationPlayback<MorphBallEyeSegment>(MorphBallEyeAnimations.StartingWith(mount.Animation), AnimationPlace.StartOfAnimation),
                    eye.Playback,
                    "Actual directional mount program");
                AssertEqual(MorphBallEyeAction.MountNoOp, eye.Action, "Mount remains decorative");
            }
        }
        var owner = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        var body = owner.SlotTable.Slots[1];
        body.Bind(EnemyDefinitionId.Eye, body.Definition);
        body.SetSpawnSettings(new MorphBallEyeSpawnSettings(HorizontalDirection.Right, MorphBallEyePart.Eye, MorphBallEyeMountSide.Left));
        new Action<RoomEnemySlot>(owner.MorphBallEyeBehavior.InitializeMorphBallEye)(body);
        (body.SpeciesState<MorphBallEyeEnemyState>() ?? throw new InvalidOperationException("Verification requires the eye body's state.")).IsActivated = true;
        owner.MorphBallEyeBehavior.MorphBallEyeBeam.Body = body;
        owner.MorphBallEyeBehavior.MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Full;
        owner.MorphBallEyeBehavior.MorphBallEyeBeam.ColourStep = 0;
        Action step = owner.MorphBallEyeBehavior.StepFullMorphBallEyeBeam;
        for (int phase = 0; phase < 16; phase++)
        {
            // The cartridge's colour bytes carry each channel's intensity in their low five bits.
            var expected = new FadeLevelColour(rom.ReadCartridgeByte(0x88ea8b + phase * 4) & FadeLevelColour.FullLevel,
                rom.ReadCartridgeByte(0x88ea8c + phase * 4) & FadeLevelColour.FullLevel,
                rom.ReadCartridgeByte(0x88ea8d + phase * 4) & FadeLevelColour.FullLevel).ToColour();
            AssertEqual(expected, MorphBallEyeGeometry.BeamColor(phase), "Native two-channel triangle intensities");
            step();
            AssertEqual(expected, owner.MorphBallEyeBehavior.MorphBallEyeBeam.Color, "Actual beam colour, zero blue intensity included");
            AssertEqual((ushort)((phase + 1) & 15), owner.MorphBallEyeBehavior.MorphBallEyeBeam.ColourStep, "Actual cycling and wrap after the sixteenth colour");
        }
        AssertThrows<InvalidDataException>(() => EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Eye, PopulationWords.Parameters(0, 0x8004)),
            "the import rejects a mount direction past the four sides");
        AssertThrows<ArgumentOutOfRangeException>(() => MorphBallEyeGeometry.BeamColor(16), "Beam phase bound");
        Console.WriteLine("Morph Ball eye: four native mount tuples, eight actual wrapped initializers,16 native beam colors and actual full-beam cycle pass.");
    }

    /// <summary>Checks the map highlight cycle's holds and highlight colours against the cartridge's.</summary>
    internal static void VerifyLookupStream5MapHighlight(CartridgeImportAddressSpace rom)
    {
        var frames = Enumerable.Range(0, 14).Select(frame => new MapPaletteCycleFrame
        {
            DurationTicks = rom.ReadCartridgeByte(MapSourceDefinitions.Animation.PaletteTiming + frame * 3),
            Highlight = Highlight(frame),
        }).ToArray();
        var original = new MapPaletteCycleDocument { Version = MapPaletteCycleFile.Version, Frames = frames };
        // The extraction states each frame's look as the colour its highlight shows: both of the frame's highlight colours.
        MapPaletteCycleDocument extracted = System.Text.Json.JsonSerializer.Deserialize<MapPaletteCycleDocument>(
            SuperMetroid.AssetExtraction.Frontend.Map.MapPaletteCycleExtractor.Extract(rom), ArtworkJson.Options)
            ?? throw new InvalidDataException("The extracted map highlight cycle is null.");
        AssertTrue(extracted.Frames.SequenceEqual(frames), "the extracted highlight cycle holds each frame's hold and highlight colour");
        for (int frame = 0; frame < 14; frame++)
            AssertEqual(Highlight(frame), Colour(frame, 11), "both of a frame's highlight colours are the same colour");
        MapPaletteCycle stock = Check(original);
        for (int frame = 0; frame < 14; frame++)
        {
            AssertEqual((byte)Math.Min(frame, 14 - frame), rom.ReadCartridgeByte(MapSourceDefinitions.Animation.PaletteTiming + frame * 3 + 1), "Native highlight phase traversal");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Duration(-1), "Highlight lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Highlight(14), "Highlight upper frame bound");
        Console.WriteLine("Map highlight: 14 native highlight colours and timing/phase records resolve to each frame's highlight shade colour.");

        PaletteRgb5 Highlight(int frame) => Colour(frame, 6);

        PaletteRgb5 Colour(int frame, int color)
        {
            ushort word = ReadVerificationWord(rom, MapSourceDefinitions.Animation.PaletteColors + (frame * 16 + color) * 2);
            return new PaletteRgb5 { Red = word & 31, Green = (word >> 5) & 31, Blue = (word >> 10) & 31 };
        }

        static MapPaletteCycle Check(MapPaletteCycleDocument document)
        {
            byte[] json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
            var cycle = MapPaletteCycleFile.Read(new MemoryStream(json, writable: false));
            AssertEqual(document.Frames.Length, cycle.FrameCount, "Highlight frame count");
            AssertEqual(document.Frames.Length, cycle.Timing.FrameCount, "Highlight timing frame count");
            for (int frame = 0; frame < cycle.FrameCount; frame++)
            {
                AssertEqual((byte)document.Frames[frame].DurationTicks, cycle.Duration(frame), "Highlight hold");
                AssertEqual(cycle.Duration(frame), cycle.Timing.Duration(frame), "Gameplay steps the highlight by the installed hold");
                // Each frame's highlight colour is the one the import pre-colours the frame's highlight shade from (#1512).
                var rgb = document.Frames[frame].Highlight;
                AssertEqual(new FadeLevelColour(rgb.Red, rgb.Green, rgb.Blue).ToColour(), cycle.Highlight(frame), "Highlight shade colour");
            }
            return cycle;
        }
    }

    /// <summary>Compares the Sidehopper corpse's native staging copies with the staging oracle's, over installed artwork.</summary>
    internal static void VerifyLookupStream5SidehopperGeometry(CartridgeImportAddressSpace rom)
    {
        ushort Word(int address) => ReadVerificationWord(rom, address);
        foreach (ushort variant in new ushort[] { 0, 2, ushort.MaxValue })
        {
            bool alternate = variant != 0;
            ReadOnlyMemory<byte> atlas = LookupStream5CorpseFixtureArtwork(rom).TilesOf(CorpseSheets.DeadMonsterSheet);
            AssertEqual(CorpseSheets.DeadMonsterSheetByteCount, atlas.Length, "Sidehopper corpse fixture art resolves its sheet");
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
            // The staging oracle stages the corpse as the cartridge does (#1392).
            CorpseStaging corpseTiles = CorpseStaging.Sidehopper(atlas.Span, variant);
            for (int row = 0; row < 5; row++)
            {
                CorpseGraphicsCopy copied = CorpseStagingGeometryDefinitions.SidehopperInitialCopy(variant, row);
                byte[] staged = Staged(corpseTiles, copied.DestinationOffset, copied.Length);
                for (int i = 0; i < copied.Length; i++)
                    AssertEqual(expected[copied.DestinationOffset + i], staged[i], "Actual installed Sidehopper graphics copy");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => CorpseStagingGeometryDefinitions.SidehopperInitialCopy(0, -1), "Copy row lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => CorpseStagingGeometryDefinitions.SidehopperInitialCopy(0, 5), "Copy row upper bound");
        Console.WriteLine("Sidehopper geometry: ten native MVN rows and installed artwork staging pass; nonzero variant semantics preserved.");

        static byte[] Staged(CorpseStaging staging, int offset, int length) =>
            staging.Upload(new CorpseRotRow(0, CorpseWorkMemory.EnemyBg2Staging + offset, length, 0));
    }

    /// <summary>Reads the retail Sidehopper corpse character rows for an independent artwork fixture.</summary>
    private static byte[] ReadSidehopperCorpseRomBytes(ISnesAddressSpace rom, int address, int count) =>
        Enumerable.Range(0, count).Select(offset => rom.ReadCartridgeByte(address + offset)).ToArray();

    /// <summary>Builds a corpse-art fixture with cartridge Sidehopper palette and tile bytes.</summary>
    internal static EnemyTileSourceArtwork LookupStream5CorpseFixtureArtwork(CartridgeImportAddressSpace rom)
    {
        byte[] planar = Enumerable.Range(0,CorpseSheets.DeadMonsterSheetByteCount)
            .Select(index => rom.ReadCartridgeByte(SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.TileDataSource(rom, CorpseSheets.DeadMonsterSheet)+index)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar,4,RoomCharacterAtlasFormat.TileColumns,out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png,size,pixels,SnesGraphics.DiagnosticPalette(16));
        png.Position=0;
        RoomCharacterAtlas atlas = RoomCharacterAtlasPng.Read(png,planar.Length);
        return EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas> { [EnemyDefinitionId.CorpseSidehopper] = atlas }, Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>
            {
                [EnemyDefinitionId.CorpseSidehopper] = EnemyPaletteSheetFile.Read(new MemoryStream(
                    EnemyPaletteSheetFile.Write(new EnemyPaletteSheetDocument
                    {
                        Version=1,
                        Colors=Enumerable.Range(0,16).Select(_ => new PaletteRgb5 { Red=0,Green=0,Blue=0 }).ToArray(),
                    }))),
            } });
    }

    /// <summary>Checks Phantoon rain sprites and their room-state timing against retail sources.</summary>
    internal static void VerifyLookupStream5PhantoonRain(CartridgeImportAddressSpace rom)
    {
        for (int i = 0; i < 8; i++)
        {
            var placement = PhantoonFlameRain.RainPlacement(i);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdad + i * 8)), placement.Cursor, "Native rain figure-eight cursor");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdaf + i * 8)), placement.Position.X, "Native rain body X");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdb1 + i * 8)), placement.Position.Y, "Native rain body Y");
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdb3 + i * 8)), "Native unused rain record word");
            AssertEqual(rom.ReadCartridgeByte(0xa7cfc2 + i), PhantoonFlameRain.FirstRainColumns[i], "Native first rain column");
        }
        for (int high = 0; high < 1; high++)
        for (int pattern = 0; pattern < 8; pattern++)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            ushort random = LittleEndianBytes.Word(unchecked((byte)(0xf8 | pattern)), unchecked((byte)high));
            int calls = 0;
            enemies.VerificationServices().NextRandom = () => { calls++; return random; };
            var body = enemies.SlotTable.Slots[0];
            var eye = enemies.SlotTable.Slots[1];
            var state = new PhantoonEnemyState(body) { Eye = eye };
            enemies.PhantoonBehavior.Combat.Phantoon = state;
            state.BodyState.PhaseTimer = 1;
            state.EyeState.FigureEightReversed = true;
            new Action<RoomEnemySlot, PhantoonEnemyState>(enemies.PhantoonBehavior.Patterns.RunPhantoonHiddenFlameRain)(body, state);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdad + pattern * 8)), state.BodyState.PathCursor, "Real rain cursor handoff");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdaf + pattern * 8)), body.Position.X.Whole, "Real rain body placement X");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa7cdb1 + pattern * 8)), body.Position.Y.Whole, "Real rain body placement Y");
            AssertEqual(false, state.EyeState.FigureEightReversed, "Real rain direction reset");
            AssertEqual(PhantoonPhase.BecomeSolidAfterFlameRain, state.BodyState.Phase, "Real rain phase handoff");
            AssertEqual(1, calls, "Rain consumes one RNG word");
            var flames = enemies.Projectiles.EnemyProjectiles.Where(p => p.IsActive).OrderBy(p => p.StateOf<PhantoonRainFlameState>().FallDelay).ToArray();
            AssertEqual(8, flames.Length, "Actual rain population");
            AssertTrue(flames.All(flame => flame.Position.X.Whole != body.Position.X.Whole), "Calculated rain gap lies at the body X");
            for (int i = 0; i < 8; i++)
            {
                int column = (rom.ReadCartridgeByte(0xa7cfc2 + pattern) + i) % 9;
                AssertEqual(rom.ReadCartridgeByte(0x8698f7 + column), flames[i].Position.X.Whole, "Actual rain column order with wrap");
                AssertEqual((ushort)40, flames[i].Position.Y.Whole, "Actual rain ceiling Y");
                AssertEqual((i + 1) * 8, flames[i].StateOf<PhantoonRainFlameState>().FallDelay, "Actual staggered rain delay");
            }
        }

        EyeTarget[] eyeTargets = PhantoonEyeTargets.All;
        PhantoonEyeTargets.VerifyGazeProgramsAgainstCartridge(CartridgeImportSource.Require(rom));
        var eyeEnemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot, PhantoonEnemyState, SamusState> pointEye = PhantoonPatterns.PointPhantoonEyeAtSamus;
        RoomEnemySlot eyeBody = eyeEnemies.SlotTable.Slots[0];
        RoomEnemySlot trackingEye = eyeEnemies.SlotTable.Slots[1];
        var eyeState = new PhantoonEnemyState(eyeBody) { Eye = trackingEye, Tentacles = eyeEnemies.SlotTable.Slots[2], Mouth = eyeEnemies.SlotTable.Slots[3] };
        eyeBody.Position = eyeBody.Position.WithWhole(new(0x4000, 0x4000));
        foreach ((short x, short y, CompassDirection direction, _) in eyeTargets)
        {
            pointEye(eyeBody, eyeState, new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(eyeBody.Position.X.Whole + x)), unchecked((ushort)(eyeBody.Position.Y.Whole + y)))),
            });
            AssertEqual(PhantoonEyeGaze.EyeAnimation(direction),
                eyeState.PlayingProgram(trackingEye),
                $"production Phantoon eye direction {direction}");
        }
        AssertThrows<InvalidDataException>(
            () => PhantoonEyeGaze.EyeAnimation((CompassDirection)Enum.GetValues<CompassDirection>().Length),
            "Phantoon eye direction beyond the eight directions");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = PhantoonFlameRain.FirstRainColumns[-1], "rain pattern lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = PhantoonFlameRain.FirstRainColumns[8], "rain pattern upper bound");
        Console.WriteLine("Stream 5 Phantoon: eight native rain columns, eight actual rain populations/gaps and nine eye selectors/eight actual octants pass with native tables forbidden; shot markers have a separately reviewed random-bucket exception.");
    }

    /// <summary>Checks Ceres door alarm ramp colors and their physical CGRAM slots.</summary>
    internal static void VerifyLookupStream5CeresDoorRamp(CartridgeImportAddressSpace rom)
    {
        var document = new CeresDoorVisualDocument
        {
            Version = 1,
            Normal = Colors(CeresSourceDefinitions.Door.NormalColors,15),
            Escape = Colors(CeresSourceDefinitions.Door.EscapeColors,15),
            Animation = Enumerable.Range(0,8).Select(row => Colors(CeresSourceDefinitions.Door.AnimationColors +16*row,6)).ToArray(),
        };
        byte[] planar = Enumerable.Range(0,CeresDoorVisualFormat.CharacterBytes)
            .Select(index => rom.ReadCartridgeByte(CeresSourceDefinitions.Door.Characters+index)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar,4,RoomCharacterAtlasFormat.TileColumns,out Extent16<ImagePixels> size);
        using var png = new MemoryStream();
        IndexedPng.Write(png,size,pixels,SnesGraphics.DiagnosticPalette(16));
        byte[] pngBytes = png.ToArray();
        CeresDoorSourceArtwork Load(CeresDoorVisualDocument value) => CeresDoorVisualFiles.Read(
            new MemoryStream(pngBytes),new MemoryStream(CeresDoorVisualFiles.Write(value)));
        var stock = Load(document);
        Check(stock,document);
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Visuals.AnimationColor(-1, 0),"Ceres animation lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Visuals.AnimationColor(8, 0),"Ceres animation upper bound");
        Console.WriteLine("Stream 5 Ceres door ramp:48 native colors and the canonical identity pass.");

        PaletteRgb5[] Colors(int source,int count) => Enumerable.Range(0,count).Select(index =>
        {
            ushort word = ReadVerificationWord(rom,source+2*index);
            return new PaletteRgb5 { Red = word &31, Green = (word>>5)&31, Blue = (word>>10)&31 };
        }).ToArray();
        static ushort Pack(PaletteRgb5 color) => (ushort)(color.Red | color.Green<<5 | color.Blue<<10);
        void Check(CeresDoorSourceArtwork source,CeresDoorVisualDocument expected)
        {
            CeresDoorVisuals actual = source.Visuals;
            static RgbColor Shown(PaletteRgb5 rgb) => new FadeLevelColour(rgb.Red, rgb.Green, rgb.Blue).ToColour();
            for(int row =0;row<8;row++)
            {
                for(int color=0;color<6;color++) AssertEqual(Shown(expected.Animation[row][color]),actual.AnimationColor(row,color),"Ceres animation color");
            }
            for(int color=0;color<15;color++) AssertEqual(Shown(expected.Normal[color]),actual.NormalColor(color),"Ceres normal palette unchanged");
            for(int color=0;color<15;color++) AssertEqual(Shown(expected.Escape[color]),actual.EscapeColor(color),"Ceres escape palette unchanged");
            string identity = SelectedPresentationHash.Create("enemy-ceres-door-v1",content =>
            {
                content.Append("tiles",planar);
                content.AppendWords("normal",expected.Normal.Select(Pack).ToArray());
                content.AppendWords("escape",expected.Escape.Select(Pack).ToArray());
                content.AppendWordFrames("animation",expected.Animation.Select(row=>row.Select(Pack).ToArray()).ToArray());
            });
            AssertEqual(identity,source.ContentIdentity,"Ceres installed colors preserve canonical resource identity");
        }
    }
    /// <summary>Checks the extraction's corpse row upload lists and the sand geometry bounds.</summary>
    internal static void VerifyLookupStream5CorpseViews()
    {
        var expected = new List<CorpseRowUpload>();
        foreach (ushort table in new ushort[] { 0xe0e0, 0xe10a, 0xe134, 0xe146, 0xe158, 0xe16a, 0xe17c, 0xe18e, 0xe1b0, 0xe1d2 })
        {
            var rows = DeadMonsterRowUploadDefinitions.ForList(CorpseNativeAddresses.TransferTableAt(table));
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
        AssertTrue(expected.SequenceEqual(CorpseNativeAddresses.AllTransferTables.SelectMany(kind => DeadMonsterRowUploadDefinitions.ForList(kind))),
            "every kind's row uploads, in native list order");
        AssertThrows<InvalidDataException>(() => CorpseNativeAddresses.TransferTableAt(0xe0e1), "unaligned row upload list rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => CorpseStagingGeometryDefinitions.SandSource(16), "sand source past end rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => CorpseStagingGeometryDefinitions.SandDestination(16), "sand destination past end rejected");
    }
    /// <summary>Compares compiled enemy actor layout arrays with their cartridge source bytes.</summary>
    internal static void VerifyLookupStream5ActorLayouts(ISnesAddressSpace rom)
    {
        var flight = CeresCinematicSourceDefinitions.RearViewPlacements.Select(source =>
            new CeresFlightActorPlacement { Role = source.Role,
                X = CoordinateAxis.AsSigned(ReadVerificationWord(rom, 0x8b0000 | source.FirstOperand)),
                Y = CoordinateAxis.AsSigned(ReadVerificationWord(rom, 0x8b0000 | source.SecondOperand)) }).ToArray();
        Check(flight, "CeresFlightActorLayout", values =>
        {
            using var encoded = new MemoryStream();
            CeresFlightActorLayoutFile.Write(encoded, new() { Version = CeresFlightActorLayoutFormat.Version, Actors = values });
            encoded.Position = 0;
            var layout = CeresFlightActorLayoutFile.Read(encoded);
            return new LookupStream5Load<CeresRearViewActorRole, CeresFlightActorPlacement>(role => layout[role], layout.ContentIdentity);
        }, value => value.Role, value => new LookupStream5Field(value.Role.ToString(), value.X, value.Y));
        var reveal = CeresCinematicSourceDefinitions.ZebesPlacements.Select(source =>
            new CeresRevealActorPlacement { Role = source.Role,
                X = CoordinateAxis.AsSigned(ReadVerificationWord(rom, 0x8b0000 | source.FirstOperand)),
                Y = CoordinateAxis.AsSigned(ReadVerificationWord(rom, 0x8b0000 | source.SecondOperand)) }).ToArray();
        Check(reveal, "CeresRevealActorLayout", values =>
        {
            using var encoded = new MemoryStream();
            CeresRevealActorLayoutFile.Write(encoded, new() { Version = CeresRevealActorLayoutFormat.Version, Actors = values });
            encoded.Position = 0;
            var layout = CeresRevealActorLayoutFile.Read(encoded);
            return new LookupStream5Load<ZebesRevealActorRole, CeresRevealActorPlacement>(role => layout[role], layout.ContentIdentity);
        }, value => value.Role, value => new LookupStream5Field(value.Role.ToString(), value.X, value.Y));
        // The large and small asteroids start where the rear view's do; the stationary vortex has its own column.
        CeresFlightActorPlacement Inherited(CeresRearViewActorRole role) => flight.Single(placement => placement.Role == role);
        CeresDestructionActorPlacement[] destruction =
        [
            new() { Role = CeresDestructionActorRole.LargeAsteroids,
                X = Inherited(CeresRearViewActorRole.LargeAsteroids).X, Y = Inherited(CeresRearViewActorRole.LargeAsteroids).Y },
            new() { Role = CeresDestructionActorRole.SmallAsteroids,
                X = Inherited(CeresRearViewActorRole.SmallAsteroids).X, Y = Inherited(CeresRearViewActorRole.SmallAsteroids).Y },
            new() { Role = CeresDestructionActorRole.Vortex,
                X = CoordinateAxis.AsSigned(ReadVerificationWord(rom, 0x8bbfa6)), Y = Inherited(CeresRearViewActorRole.Vortex).Y },
        ];
        Check(destruction, "CeresDestructionActorLayout", values =>
        {
            using var encoded = new MemoryStream();
            CeresDestructionActorLayoutFile.Write(encoded, new() { Version = CeresDestructionActorLayoutFormat.Version, Actors = values });
            encoded.Position = 0;
            var layout = CeresDestructionActorLayoutFile.Read(encoded);
            return new LookupStream5Load<CeresDestructionActorRole, CeresDestructionActorPlacement>(role => layout[role], layout.ContentIdentity);
        }, value => value.Role, value => new LookupStream5Field(value.Role.ToString(), value.X, value.Y));
        // #1627: the layouts hold signed screen pixels. A placement left of the screen stands at its wrapped screen position,
        // the signed range's ends are accepted and a coordinate past either end is rejected.
        CeresFlightActorPlacement left = flight[0] with { X = -32 };
        AssertEqual(65504, (int)left.Position.X, "a placement 32 pixels left of the screen stands at its wrapped screen position");
        _ = ReadFlight([flight[0] with { X = -32768, Y = 32767 }, .. flight[1..]]);
        AssertThrows<InvalidDataException>(() => ReadFlight([flight[0] with { X = 32768 }, .. flight[1..]]), "a coordinate past the signed range's top is rejected");
        AssertThrows<InvalidDataException>(() => ReadFlight([flight[0] with { Y = -32769 }, .. flight[1..]]), "a coordinate past the signed range's bottom is rejected");
        Console.WriteLine("Ceres layouts: all14 original actor placements/28 coordinate operands, complete identity preservation and bounds pass.");

        static CeresFlightActorPlacements ReadFlight(CeresFlightActorPlacement[] values)
        {
            using var encoded = new MemoryStream();
            CeresFlightActorLayoutFile.Write(encoded, new() { Version = CeresFlightActorLayoutFormat.Version, Actors = values });
            encoded.Position = 0;
            return CeresFlightActorLayoutFile.Read(encoded);
        }

        // Each layout's identity keeps the layout name it was installed under; #1452 renamed the types to the placements
        // they hold and kept those names.
        static void Check<TRole, T>(T[] original, string identityName,
            Func<T[], LookupStream5Load<TRole, T>> load,
            Func<T, TRole> roleOf,
            Func<T, LookupStream5Field> fields)
            where TRole : struct, Enum
        {
            var stock = load(original);
            Verify(stock, original);
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Read((TRole)Enum.ToObject(typeof(TRole), -1)), "Ceres layout undefined role below");
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Read((TRole)Enum.ToObject(typeof(TRole), Enum.GetValues<TRole>().Length)),
                "Ceres layout undefined role above");

            void Verify(LookupStream5Load<TRole, T> actual, T[] expected)
            {
                foreach (T placement in expected)
                    AssertEqual(fields(placement), fields(actual.Read(roleOf(placement))), "Ceres complete selected placement");
                // The identity covers each role's placement in role order.
                T[] byRole = [.. expected.OrderBy(roleOf)];
                string identity = SelectedPresentationHash.Create(identityName, content =>
                {
                    content.Append("actors", byRole.Length);
                    foreach (T placement in byRole)
                    {
                        var value = fields(placement);
                        content.Append("role", System.Text.Encoding.UTF8.GetBytes(value.Role));
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
            var actual = CeresDoorInitializations.For(SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.CeresDoor(variant));
            AssertEqual(ReadVerificationWord(rom, 0xa6f52c + 2 * variant), NativeEnemyAnimationOracle.EntryOf(CeresDoorNativeListAddresses.Entries, actual.Animation), "Ceres door original instruction dispatch");
            AssertEqual(ReadVerificationWord(rom, 0xa6f72b + 2 * variant), CeresNativeWords.DoorActions.Word(actual.MainAction), "Ceres door original function dispatch");
        }
        // The vents' ways in the order of the cartridge's tables, which a vent's first spawn parameter indexes.
        CeresSteamVariant[] steamVariants = [CeresSteamVariant.Up, CeresSteamVariant.Left, CeresSteamVariant.Down, CeresSteamVariant.Right, CeresSteamVariant.RotatingElevatorLeft, CeresSteamVariant.RotatingElevatorRight];
        for (int variant = 0; variant < steamVariants.Length; variant++)
        {
            var actual = CeresSteamInitializations.Initialization(steamVariants[variant]);
            AssertEqual(ReadVerificationWord(rom, 0xa6eff5 + 2 * variant), NativeEnemyAnimationOracle.EntryOf(CeresSteamNativeListAddresses.Entries, actual.Animation), "Ceres steam original instruction dispatch");
            AssertEqual(ReadVerificationWord(rom, 0xa6f001 + 2 * variant), CeresNativeWords.SteamActions.Word(actual.Action), "Ceres steam original function dispatch");
        }
        for (int index = 0; index < 9; index++)
        {
            var actual = MagdolliteRise.Phase(index);
            AssertEqual(ReadVerificationWord(rom, 0xa8af55 + 2 * index), actual.DistanceThreshold, "Magdollite original rise threshold");
            AssertEqual(ReadVerificationWord(rom, 0xa8af67 + 2 * index), NativeEnemyAnimationOracle.EntryOf(MagdolliteNativeListAddresses.Entries, actual.BodyAnimation), "Magdollite original body program");
            AssertEqual(ReadVerificationWord(rom, 0xa8af79 + 2 * index), actual.OverlayYOffset, "Magdollite original overlay offset");
        }
        AssertThrows<InvalidDataException>(() => CeresDoorInitializations.For(SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.CeresDoor(7)), "Ceres door upper bound");
        AssertThrows<InvalidDataException>(() => CeresDoorInitializations.For(SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.CeresDoor(ushort.MaxValue)), "Ceres door full-word rejection");
        AssertThrows<InvalidDataException>(() => CeresSteamInitializations.Initialization((CeresSteamVariant)6), "Ceres steam upper bound");
        AssertThrows<InvalidDataException>(() => CeresSteamInitializations.Initialization((CeresSteamVariant)ushort.MaxValue), "Ceres steam full-word rejection");
        AssertThrows<InvalidDataException>(() => MagdolliteRise.Phase(-1), "Magdollite lower bound");
        AssertThrows<InvalidDataException>(() => MagdolliteRise.Phase(9), "Magdollite upper bound");
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
        // The glow programs are a plain list since #1418, which has no indexer of its own to bound.
        CheckEntries(CinematicGlowPaletteFxColorDefinitions.All,
            entry => new Project(entry.DefinitionPointer, entry.ProgramStart), 2);
        CheckEntries(TourianEscapeSharedRedFlashPaletteFxColorToolingDefinitions.All,
            entry => new Project(entry.DefinitionPointer, entry.ProgramStart), 2);
        CheckComputedBounds(TourianEscapeSharedRedFlashPaletteFxColorToolingDefinitions.All, 2);
        CheckEntries(TourianStatueGreyPaletteFxColorToolingDefinitions.All,
            entry => new Project(entry.DefinitionPointer, entry.ProgramStart), 4);
        CheckComputedBounds(TourianStatueGreyPaletteFxColorToolingDefinitions.All, 4);
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
        }
        // A list that computes its entries rejects an index outside them itself.
        static void CheckComputedBounds<T>(IReadOnlyList<T> entries, int expectedCount)
        {
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

    /// <summary>Checks the Ceres door quake's offsets against the authored ROM words.</summary>
    internal static void VerifyLookupStream5DoorQuakeDecoding(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyCeresDoorQuakeDefinitions), () => VerifyCeresDoorQuakeDefinitions(rom));
        byte[] stockJson = SuperMetroid.AssetExtraction.Enemies.EnemySpritemapFiles.Extract(rom);
        var installed = EnemySpritemapDocuments.Load(new MemoryStream(stockJson, writable: false));
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(SpawnedObjectFixtures.InstalledTilesWith(installed)), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            RidleyEncounter = { CeresStatus = CeresEscapeStatus.RidleyEscaped },
        };
        enemies.RidleyEncounter.Ridley = new RidleyEnemyState { MovementAnimationEnabled = false };
        enemies.SlotTable.Slots[0].Bind(EnemyDefinitionId.RidleyCeres, enemies.SlotTable.Slots[0].Definition);
        // Ceres Ridley's scene draws from his room's sheets.
        SpawnedObjectFixtures.LoadSetDrawing(enemies, EnemyDefinitionId.RidleyCeres);
        var door = enemies.SlotTable.Slots[1];
        door.Bind(EnemyDefinitionId.CeresDoor, door.Definition);
        var doorState = new CeresDoorEnemyState { HandsDrawingToRidley = true };
        door.SetSpeciesState(doorState);
        door.Position = door.Position.WithWholeY(80);
        foreach (ushort x in new ushort[] { 0, 1, 3, 255, 256, 511, ushort.MaxValue })
        foreach (ushort camera in new ushort[] { 0, ushort.MaxValue })
        for (ushort phase = 0; phase < 4; phase++)
        {
            door.Position = door.Position.WithWholeX(x); enemies.Earthquake.Start(NativeEarthquakeTypes.Shake(29), phase);
            var actual = new OamBuffer(); actual.BeginFrame();
            actual.AppendDrawn(frame => enemies.CeresRidleyBehavior.BabyScene.DrawCeresRidleyImmediateBabyAndDoor(frame, new(camera, 0))); actual.FinalizeFrame();
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa6a321 + phase * 2);
            AssertEqual(unchecked((short)native), CeresDoorQuake.XOffset(phase),
                "Quake offset is the authored word of its phase");
            var expected = new OamBuffer(); expected.BeginFrame();
            DrawImportedEnemySpritemap(rom, expected, new NativeEnemySpritemapDraw { Bank = CeresDoorFrameDefinitionsConstants.Bank, Pointer = CeresDoorFrameDefinitionsConstants.RidleyPrivateOverlaySpritemap, Origin = new(unchecked((ushort)(x - camera + native)), 80), PaletteBits = SnesObjAttributeWord.Create(tileNumber: 0, paletteIndex: 2, priority: 0).PaletteBits, BaseTileIndex = 0 });
            expected.FinalizeFrame();
            AssertTrue(expected.ImageEquals(actual),
                $"Authored quake preserves actual low/high OAM through coordinate wrap (x {x}, camera {camera}, phase {phase}: {expected.FirstDifference(actual)})");
        }
        Console.WriteLine("Ceres quake decoding: 56 actual wrapped low/high OAM frames match the authored offset words.");
    }

    /// <summary>Checks the Ceres flight scene's transferred colors and CGRAM destinations.</summary>
    internal static void VerifyLookupStream5CeresFlightPalette(CartridgeImportAddressSpace rom)
    {
        var colors = new PaletteRgb5[SnesCgram.ColorCount];
        for (int index = 0; index < colors.Length; index++)
        {
            ushort value = ReadVerificationWord(rom, CeresCinematicSourceDefinitions.FlightPalette + index * sizeof(ushort));
            colors[index] = new PaletteRgb5 { Red = value & 31, Green = value >> 5 & 31, Blue = value >> 10 & 31 };
        }
        CeresFlightPalette stock = Check();
        // The import reads the palette as its background colours then its sprite colours (#1512); the game keeps its backdrop.
        AssertEqual(SnesCgram.ColorCount, stock.Colours.Length, "Ceres palette holds every colour of both owners");
        AssertEqual(stock.Colours[0], stock.Backdrop, "Ceres palette's backdrop is its first background colour");
        AssertEqual(CgramPalettes.ByteCount, ScreenColourWords.Words(stock.BackgroundColours.AsSpan(), stock.SpriteColours.AsSpan()).Length,
            "Ceres palette serializes every colour of both owners");
        IReadOnlyDictionary<string, byte[]> files = SuperMetroid.AssetExtraction.Areas.Ceres.CeresFlightArtworkExtractor.Extract(rom);
        var catalog = CeresFlightCharacters.Read(Open(CeresFlightArtworkFileFormat.FloorFileName),
            Open(CeresFlightArtworkFileFormat.ObjectFileName), Open(CeresFlightArtworkFileFormat.MapFileName),
            Open(CeresFlightSpriteFormat.FileName),
            CeresFlightSourceArtwork.Read(stock.Backdrop, Open(CeresFlightActorLayoutFormat.FileName)));
        // The installed documents' identity covers the backdrop and placements; the import's adds the maps and the
        // compositions, which are import data like the characters (#1636, #1638, #1693).
        string documentsHash = SelectedPresentationHash.Create("CeresFlightArtworkCatalog-v4", content =>
        {
            content.AppendColors("backdrop", [stock.Backdrop]);
            content.Append("actors", Convert.FromHexString(catalog.Flight.Actors.ContentIdentity));
        });
        string expectedHash = SelectedPresentationHash.Create("CeresFlightImportArtwork-v2", content =>
        {
            content.Append("documents", Convert.FromHexString(documentsHash));
            content.Append("mode7-maps", catalog.FloorMaps.Span);
            content.Append("sprites", Convert.FromHexString(catalog.Sprites.ContentIdentity));
        });
        AssertEqual(expectedHash, catalog.ContentIdentity, "Ceres flight canonical hash covers its backdrop; its colours are the import's alone (#1544)");
        Console.WriteLine("Ceres flight palette:256 native colors,actual CGRAM,byte serialization and catalog identity pass.");

        MemoryStream Open(string name) => new(files[name], writable: false);
        CeresFlightPalette Check()
        {
            var document = new CeresFlightPaletteDocument { Version = 1, Colors = colors };
            using var json = new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options));
            CeresFlightPalette actual = CeresFlightPaletteFile.Read(json);
            RgbColor[] selected = [.. actual.Colours];
            RgbColor[] loaded = [.. actual.BackgroundColours, .. actual.SpriteColours];
            ReadOnlySpan<byte> bytes = ScreenColourWords.Words(actual.BackgroundColours.AsSpan(), actual.SpriteColours.AsSpan());
            for (int index = 0; index < colors.Length; index++)
            {
                PaletteRgb5 color = colors[index];
                ushort expected = (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
                AssertEqual(expected, selected[index], "Every selected color");
                AssertEqual(expected, loaded[index], "Installed colours reach the colours the import pre-colours the scenes in");
                AssertEqual(expected, System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bytes.Slice(index * sizeof(ushort))), "Serialized colors retain exact native byte order");
            }
            return actual;
        }
    }

    /// <summary>Checks Ceres normal room paint against native colors in their actual CGRAM positions.</summary>
    internal static void VerifyLookupStream5CeresNormalPaint(CartridgeImportAddressSpace rom)
    {
        PaletteRgb5[] Colors(int source, int count) => Enumerable.Range(0, count).Select(i =>
        {
            ushort w = ReadVerificationWord(rom, source + 2 * i);
            return new PaletteRgb5 { Red = w & 31, Green = w >> 5 & 31, Blue = w >> 10 };
        }).ToArray();
        var document = new CeresDoorVisualDocument
        {
            Version = 1, Normal = Colors(CeresSourceDefinitions.Door.NormalColors, 15), Escape = Colors(CeresSourceDefinitions.Door.EscapeColors, 15),
            Animation = Enumerable.Range(0, 8).Select(row => Colors(CeresSourceDefinitions.Door.AnimationColors + 16 * row, 6)).ToArray(),
        };
        byte[] planar = Enumerable.Range(0, CeresDoorVisualFormat.CharacterBytes).Select(i => rom.ReadCartridgeByte(CeresSourceDefinitions.Door.Characters + i)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
        using var png = new MemoryStream(); IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        CeresDoorVisuals stock = Check();
        AssertThrows<ArgumentOutOfRangeException>(() => stock.NormalColor(-1), "Normal lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.NormalColor(15), "Normal upper bound");
        Console.WriteLine("Ceres normal: fifteen native colors,two actual initializers, hash and bounds pass.");

        CeresDoorVisuals Check()
        {
            ushort Pack(PaletteRgb5 c) => (ushort)(c.Red | c.Green << 5 | c.Blue << 10);
            var visual = CeresDoorVisualFiles.Read(new MemoryStream(png.ToArray()), new MemoryStream(CeresDoorVisualFiles.Write(document)));
            for (int color = 0; color < 15; color++) AssertEqual(Pack(document.Normal[color]), visual.Visuals.NormalColor(color), "Every installed normal channel");
            foreach (ushort variant in new ushort[] { 0, 3 })
            {
                var system = new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() })), RoomEnemyCompatibilityFixtures.ForNewRoom());
                var slot = system.SlotTable.Slots[0]; slot.SetSpawnParameter1(EnemyDefinitionId.CeresDoor, variant);
                new Action<RoomEnemySlot>(system.CeresDoorBehavior.InitializeCeresDoor)(slot);
                // The initializer shows the normal colours as the door's shade, which the import paints from this paint (#1512).
                AssertEqual(CeresDoorShades.InNormalColours, slot.Shading, "Actual normal/target initializer shows the normal colours");
            }
            string identity = SelectedPresentationHash.Create("enemy-ceres-door-v1", content =>
            {
                content.Append("tiles", planar); content.AppendWords("normal", document.Normal.Select(Pack).ToArray()); content.AppendWords("escape", document.Escape.Select(Pack).ToArray());
                content.AppendWordFrames("animation", document.Animation.Select(row => row.Select(Pack).ToArray()).ToArray());
            });
            AssertEqual(identity, visual.ContentIdentity, "Normal colours keep the escape/animation and canonical hash");
            return visual.Visuals;
        }
    }

    /// <summary>Checks Ceres escape paint's highlights, edges and outlines after palette composition.</summary>
    internal static void VerifyLookupStream5CeresEscapePaint(CartridgeImportAddressSpace rom)
    {
        PaletteRgb5[] Colors(int source, int count) => Enumerable.Range(0, count).Select(i =>
        {
            ushort w = ReadVerificationWord(rom, source + 2 * i);
            return new PaletteRgb5 { Red = w & 31, Green = w >> 5 & 31, Blue = w >> 10 };
        }).ToArray();
        var document = new CeresDoorVisualDocument
        {
            Version = 1, Normal = Colors(CeresSourceDefinitions.Door.NormalColors, 15), Escape = Colors(CeresSourceDefinitions.Door.EscapeColors, 15),
            Animation = Enumerable.Range(0, 8).Select(row => Colors(CeresSourceDefinitions.Door.AnimationColors + 16 * row, 6)).ToArray(),
        };
        byte[] planar = Enumerable.Range(0, CeresDoorVisualFormat.CharacterBytes).Select(i => rom.ReadCartridgeByte(CeresSourceDefinitions.Door.Characters + i)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
        using var png = new MemoryStream(); IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        CeresDoorVisuals stock = Check();
        AssertThrows<ArgumentOutOfRangeException>(() => stock.EscapeColor(-1), "Escape lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.EscapeColor(15), "Escape upper bound");
        Console.WriteLine("Ceres escape: fifteen native colors,two actual escape initializers, hash and bounds pass.");

        CeresDoorVisuals Check()
        {
            ushort Pack(PaletteRgb5 c) => (ushort)(c.Red | c.Green << 5 | c.Blue << 10);
            var visual = CeresDoorVisualFiles.Read(new MemoryStream(png.ToArray()), new MemoryStream(CeresDoorVisualFiles.Write(document)));
            for (int color = 0; color < 15; color++) AssertEqual(Pack(document.Escape[color]), visual.Visuals.EscapeColor(color), "Every installed escape channel");
            foreach (ushort variant in new ushort[] { 0, 3 })
            {
                var system = new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() })), RoomEnemyCompatibilityFixtures.ForNewRoom()) { RidleyEncounter = { CeresStatus = CeresEscapeStatus.SelfDestructStarted }};
                var slot = system.SlotTable.Slots[0]; slot.SetSpawnParameter1(EnemyDefinitionId.CeresDoor, variant);
                new Action<RoomEnemySlot>(system.CeresDoorBehavior.InitializeCeresDoor)(slot);
                // Once the self-destruct starts, the initializer shows the escaping colours as the door's shade (#1512).
                AssertEqual(CeresDoorShades.InEscapingColours, slot.Shading, "Actual escape initializer shows the escaping colours");
            }
            string identity = SelectedPresentationHash.Create("enemy-ceres-door-v1", content =>
            {
                content.Append("tiles", planar); content.AppendWords("normal", document.Normal.Select(Pack).ToArray()); content.AppendWords("escape", document.Escape.Select(Pack).ToArray());
                content.AppendWordFrames("animation", document.Animation.Select(row => row.Select(Pack).ToArray()).ToArray());
            });
            AssertEqual(identity, visual.ContentIdentity, "Escape colours keep the normal/animation and canonical hash");
            return visual.Visuals;
        }
    }

    /// <summary>Checks Ceres beacon light colors against the native palette ramp.</summary>
    internal static void VerifyLookupStream5CeresBeaconPaint(CartridgeImportAddressSpace rom)
    {
        PaletteRgb5[] Colors(int source, int count) => Enumerable.Range(0, count).Select(i =>
        {
            ushort w = ReadVerificationWord(rom, source + 2 * i);
            return new PaletteRgb5 { Red = w & 31, Green = w >> 5 & 31, Blue = w >> 10 };
        }).ToArray();
        var document = new CeresDoorVisualDocument
        {
            Version = 1, Normal = Colors(CeresSourceDefinitions.Door.NormalColors, 15), Escape = Colors(CeresSourceDefinitions.Door.EscapeColors, 15),
            Animation = Enumerable.Range(0, 8).Select(row => Colors(CeresSourceDefinitions.Door.AnimationColors + 16 * row, 6)).ToArray(),
        };
        byte[] planar = Enumerable.Range(0, CeresDoorVisualFormat.CharacterBytes).Select(i => rom.ReadCartridgeByte(CeresSourceDefinitions.Door.Characters + i)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, RoomCharacterAtlasFormat.TileColumns, out Extent16<ImagePixels> size);
        using var png = new MemoryStream(); IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
        CeresDoorVisuals stock = Check();
        AssertThrows<ArgumentOutOfRangeException>(() => stock.AnimationColor(-1, 0), "Beacon row lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.AnimationColor(8, 0), "Beacon row upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.AnimationColor(0, -1), "Beacon color lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.AnimationColor(0, 6), "Beacon color upper bound");
        Console.WriteLine("Ceres beacon:48native colors,the64-tick row selection cycle, hash and bounds pass.");

        CeresDoorVisuals Check()
        {
            ushort Pack(PaletteRgb5 c) => (ushort)(c.Red | c.Green << 5 | c.Blue << 10);
            var visual = CeresDoorVisualFiles.Read(new MemoryStream(png.ToArray()), new MemoryStream(CeresDoorVisualFiles.Write(document)));
            for (int row = 0; row < 8; row++) for (int color = 0; color < 6; color++)
                AssertEqual(Pack(document.Animation[row][color]), visual.Visuals.AnimationColor(row, color), "Every installed beacon channel");
            var system = new RoomEnemySystem(EnemySessionFixtures.WithInstalledFrames(EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() })), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new StandaloneRoomEnemyServices(() => 0) { Floor = RoomBackgroundsFixtures.CeresFloor() });
            system.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
            RoomEnemySlot door = system.SlotTable.Slots[0];
            foreach (int counter in Enumerable.Range(0, 64))
            {
                door.FrameCounter = (ushort)counter; system.CeresDoorBehavior.RunCeresDoorPaletteAnimation(door);
                int row = (counter & 0x38) >> 3;
                // The tick names the glow step the room's pictures show, which the import paints from this row (#1512).
                AssertEqual(row, system.CeresDoorBehavior.ElevatorRoomGlowStep, "Actual native eight-tick row selection");
            }
            string identity = SelectedPresentationHash.Create("enemy-ceres-door-v1", content =>
            {
                content.Append("tiles", planar); content.AppendWords("normal", document.Normal.Select(Pack).ToArray()); content.AppendWords("escape", document.Escape.Select(Pack).ToArray());
                content.AppendWordFrames("animation", document.Animation.Select(row => row.Select(Pack).ToArray()).ToArray());
            });
            AssertEqual(identity, visual.ContentIdentity, "Beacon colours keep the normal/escape/platform and canonical hash");
            return visual.Visuals;
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
        Suite(nameof(VerifyLookupStream5PhantoonMarkers), () => VerifyLookupStream5PhantoonMarkers());
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
