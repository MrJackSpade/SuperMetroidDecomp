using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyLookupStream5MeltingTilemaps()
    {
        var oracle = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)), "Melt tilemap source revision");
        string directory = Path.Combine(Path.GetFullPath("csharp/test-temp"), "lookup-melt-maps-" + Guid.NewGuid().ToString("N"));
        try
        {
            SuperMetroid.AssetExtraction.EnemyTileArtworkFiles.Extract(oracle, directory, SuperMetroid.AssetExtraction.SupportedCartridge.Sha256);
            EnemyTileArtworkCatalog catalog = SuperMetroid.AssetExtraction.EnemyTileArtworkFiles.Load(directory, null);
            CrocomireMeltingArtwork art = catalog.CrocomireMelting!;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var native = new ushort[2][];
            for (int phase = 0; phase < 2; phase++)
            {
                int source = phase == 0 ? CrocomireMeltingArtworkAddresses.FirstTilemap : CrocomireMeltingArtworkAddresses.SecondTilemap;
                native[phase] = Enumerable.Range(0, 256).Select(i => ReadVerificationWord(oracle, source + i * 2)).ToArray();
                var calculated = new CrocomireMeltingTilemap(phase != 0, native[phase]);
                AssertEqual(0, ((Dictionary<int, ushort>)typeof(CrocomireMeltingTilemap).GetField("edits", flags)!.GetValue(calculated)!).Count,
                    "Every native melt tilemap word calculates without fallback");
                AssertTrue(calculated.Words().AsSpan().SequenceEqual(native[phase]), "All native melt words");
                AssertTrue(art.Tilemap(source).SequenceEqual(native[phase]), "Installed melt words");
                VerifyInstalledCrocomireMeltingTilemap(oracle, catalog, source, phase == 0
                    ? CrocomireInstructionProgramDefinitions.MeltingOneTopRow : CrocomireInstructionProgramDefinitions.MeltingTwoTopRow);
                for (int index = 0; index < 256; index++)
                {
                    ushort[] edited = (ushort[])native[phase].Clone(); edited[index] ^= 0xffff;
                    var independent = new CrocomireMeltingTilemap(phase != 0, edited);
                    edited[index] = native[phase][index];
                    ushort[] expected = (ushort[])native[phase].Clone(); expected[index] ^= 0xffff;
                    AssertTrue(independent.Words().AsSpan().SequenceEqual(expected), "Full-word edit owns input and preserves neighbors");
                    AssertTrue(calculated.Words().AsSpan().SequenceEqual(native[phase]), "Edited map does not mutate stock");
                }
                AssertThrows<InvalidDataException>(() => _ = new CrocomireMeltingTilemap(phase != 0, new ushort[255]), "Short melt map rejects");
            }
            var first = (RoomCharacterAtlas)typeof(CrocomireMeltingArtwork).GetField("first", flags)!.GetValue(art)!;
            var second = (RoomCharacterAtlas)typeof(CrocomireMeltingArtwork).GetField("second", flags)!.GetValue(art)!;
            string expectedIdentity = SelectedPresentationHash.Create("enemy-crocomire-melt-v1", content =>
            {
                content.Append("first", first.Transfer.Span); content.Append("second", second.Transfer.Span);
                content.AppendWords("first-map", native[0]); content.AppendWords("second-map", native[1]);
            });
            AssertEqual(expectedIdentity, art.ContentIdentity, "Original canonical melt identity");
            AssertThrows<InvalidDataException>(() => { _ = art.Tilemap(0); }, "Unknown map identity rejects");
            Console.WriteLine("Crocomire melt layouts: 512 native words, 512 isolated full-word edits, input ownership, both actual guarded BG2 uploads and canonical identity pass.");
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true); }
    }
    private static void VerifyLookupStream5SkeletonTransfers()
    {
        var oracle = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(oracle.Rom)),
            "Skeleton transfer oracle revision");
        string directory = Path.Combine(Path.GetFullPath("csharp/test-temp"),
            "lookup-skeleton-transfers-" + Guid.NewGuid().ToString("N"));
        try
        {
            SuperMetroid.AssetExtraction.EnemyTileArtworkFiles.Extract(oracle, directory,
                SuperMetroid.AssetExtraction.SupportedCartridge.Sha256);
            VerifyCrocomireSkeletonArtwork(oracle, directory,
                SuperMetroid.AssetExtraction.EnemyTileArtworkFiles.Load(directory, null));
            AssertThrows<InvalidDataException>(() => CrocomireSkeletonTransferDefinitions.TryGet(-1, out _),
                "Skeleton negative transfer index rejects");
            AssertThrows<IndexOutOfRangeException>(() => _ = CrocomireSkeletonTransferDefinitions.Frames[6],
                "Skeleton view excludes sentinel");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    private static void VerifyLookupStream5CeresRumble(SuperMetroidAddressSpace rom)
    {
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes("Super Metroid.smc"))), "Rumble native revision");
        for (int index = 0; index < 4; index++)
            AssertEqual((unchecked((short)ReadVerificationWord(rom, 0xa6f840 + 4 * index)),
                unchecked((short)ReadVerificationWord(rom, 0xa6f842 + 4 * index))),
                CeresDoorRumbleGeometryDefinitions.Offset(index), "All eight native anchor words");
        AssertThrows<IndexOutOfRangeException>(() => CeresDoorRumbleGeometryDefinitions.Offset(-1), "Rumble lower bound");
        AssertThrows<IndexOutOfRangeException>(() => CeresDoorRumbleGeometryDefinitions.Offset(4), "Rumble upper bound");
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        foreach (var origin in new[] { (X: (ushort)232, Y: (ushort)631), (X: (ushort)1, Y: (ushort)65530) })
        {
            var system = new RoomEnemySystem();
            var slot = system.Slots[0];
            slot.XPosition = origin.X; slot.YPosition = origin.Y;
            slot.VariableD = 48; slot.VariableE = 0; slot.VariableF = 0;
            system.CeresStatus = 2;
            int randomCalls = 0;
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(system,
                (Func<ushort>)(() => (ushort)(++randomCalls % 2 == 0 ? 0x4000 : 0)));
            var tick = typeof(RoomEnemySystem).GetMethod("RunCeresDoorRumbleAndExplosions", flags)!
                .CreateDelegate<Action<RoomEnemySlot>>(system);
            for (int call = 1; call <= 49; call++)
            {
                tick(slot);
                int emitted = Math.Min(10, (call + 4) / 5);
                AssertEqual(emitted, randomCalls, "One RNG call per selected anchor, none between emissions");
                var particles = system.EnemyProjectiles.Where(p => p.IsActive).ToArray();
                AssertEqual(emitted, particles.Length, "Exact independent effect allocation count");
                for (int ordinal = 0; ordinal < emitted; ordinal++)
                {
                    int index = 3 - ordinal % 4;
                    ushort x = unchecked((ushort)(origin.X + (short)ReadVerificationWord(rom, 0xa6f840 + index * 4)));
                    ushort y = unchecked((ushort)(origin.Y + (short)ReadVerificationWord(rom, 0xa6f842 + index * 4)));
                    ushort program = MiscDustProjectileDefinitions.InstructionList((ushort)(ordinal % 2 == 0 ? 12 : 3));
                    // Native pool allocation descends; identify each emission through its unique current ordinal below.
                    var particle = particles[emitted - ordinal - 1];
                    AssertEqual(x, particle.XPosition, "Actual produced native X including 16-bit wrap");
                    AssertEqual(y, particle.YPosition, "Actual produced native Y including 16-bit wrap");
                    AssertEqual(program, particle.InstructionPointer, "RNG changes animation only");
                }
                AssertEqual(call == 49, slot.Properties.HasAny(EnemyProperties.Invisible), "Hide only at destruction expiry");
            }
            AssertEqual((ushort)0x8000, system.CeresStatus, "Actual rotation handoff after last anchor");
            AssertEqual((ushort)0x25, system.LastCeresDoorSoundEffectLibrary2!.Value, "Emission sound preserved");
        }
        Console.WriteLine("Ceres rumble: eight native words, bounds, 98 actual calls, 20 emitted native positions/order/animations, wrap and expiry pass.");
    }
    private static void VerifyLookupStream5PhantoonRainWait(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        int totalTicks = 0;
        const int nextPattern = 3;
        foreach (int bucket in new[] { 2, 0, 1 })
        {
            var system = new RoomEnemySystem();
            var body = system.Slots[0];
            var eye = system.Slots[1];
            var state = new PhantoonEnemyState(body) { Eye = eye, Tentacles = system.Slots[2], Mouth = system.Slots[3] };
            int randomCalls = 0;
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(system,
                (Func<ushort>)(() => (ushort)(++randomCalls == 1 ? bucket : nextPattern)));
            var fade = typeof(RoomEnemySystem).GetMethod("RunPhantoonFlameRainFadeOut", flags)!
                .CreateDelegate<Action<RoomEnemySlot, PhantoonEnemyState, byte>>(system);
            var tick = typeof(RoomEnemySystem).GetMethod("RunPhantoonHiddenFlameRain", flags)!
                .CreateDelegate<Action<RoomEnemySlot, PhantoonEnemyState>>(system);
            body.XPosition = 111; body.YPosition = 99; body.VariableA = 42; body.VariableE = 17;
            body.VariableF = (ushort)PhantoonAiFunction.FadeOutDuringFlameRain;
            fade(body, state, 1); // Odd NMI leaves the independent fade incomplete.
            AssertEqual(0, randomCalls, "Incomplete fade does not choose hidden delay");
            AssertEqual((ushort)17, body.VariableE, "Incomplete fade preserves prior countdown");
            eye.VariableF = 1;
            fade(body, state, 0);
            ushort duration = ReadVerificationWord(rom, 0xa7cd63 + bucket * 2);
            AssertEqual(duration, body.VariableE, "Completed fade selects exact native hidden duration");
            AssertEqual((ushort)PhantoonAiFunction.SpawnFlameRain, body.VariableF, "Completed fade starts hidden waiting phase");
            for (int elapsed = 1; elapsed <= duration; elapsed++)
            {
                tick(body, state); totalTicks++;
                AssertEqual((ushort)(duration - elapsed), body.VariableE, "Hidden wait decrements exactly once");
                if (elapsed < duration)
                {
                    AssertEqual(1, randomCalls, "Next location is not chosen before delay expires");
                    AssertEqual((ushort)111, body.XPosition, "Hidden wait does not interpolate X");
                    AssertEqual((ushort)99, body.YPosition, "Hidden wait does not interpolate Y");
                    AssertEqual((ushort)42, body.VariableA, "Hidden wait does not advance path cursor");
                    AssertEqual(0, system.EnemyProjectiles.Count(projectile => projectile.IsActive), "No rain launches before expiry");
                }
            }
            AssertEqual(2, randomCalls, "Expiration consumes one separate placement RNG word");
            AssertEqual(ReadVerificationWord(rom, 0xa7cdad + nextPattern * 8), body.VariableA, "Exact delayed native path cursor");
            AssertEqual(ReadVerificationWord(rom, 0xa7cdaf + nextPattern * 8), body.XPosition, "Exact delayed native X");
            AssertEqual(ReadVerificationWord(rom, 0xa7cdb1 + nextPattern * 8), body.YPosition, "Exact delayed native Y");
            AssertEqual((ushort)PhantoonAiFunction.BecomeSolidAfterFlameRain, body.VariableF, "Native rain appearance handoff");
            AssertEqual((ushort)0, eye.VariableC, "Rain handoff clears fade counter");
            var flames = system.EnemyProjectiles.Where(projectile => projectile.IsActive).ToArray();
            AssertEqual(8, flames.Length, "Actual hidden expiry launches eight rain projectiles");
            foreach (var flame in flames)
            {
                AssertEqual(RoomEnemyProjectileKind.PhantoonDestroyableFlame, flame.Kind, "Actual rain projectile family");
                AssertEqual((ushort)40, flame.YPosition, "Actual rain starts at ceiling");
            }
        }
        AssertEqual(210, totalTicks, "Three exact native hidden wait lengths");
        Console.WriteLine("Phantoon rain wait:30/60/120 holds,210actual countdown calls,fade-gated selection/exact expiry RNG-placement and24real rain spawns pass.");
    }
    private static void VerifyLookupStream5PhantoonClosedEye(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        int totalTicks = 0;
        foreach (int bucket in new[] { 1, 2, 0 })
        foreach (bool reversed in new[] { false, true })
        {
            var system = new RoomEnemySystem();
            var body = system.Slots[0];
            var eye = system.Slots[1];
            var mouth = system.Slots[3];
            var state = new PhantoonEnemyState(body) { Eye = eye, Tentacles = system.Slots[2], Mouth = mouth };
            int randomCalls = 0;
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(system,
                (Func<ushort>)(() => { randomCalls++; return (ushort)(reversed ? 1 : 0); }));
            var begin = typeof(RoomEnemySystem).GetMethod("RunPhantoonPickFirstRoundPattern", flags)!
                .CreateDelegate<Action<RoomEnemySlot, PhantoonEnemyState, byte>>(system);
            var tick = typeof(RoomEnemySystem).GetMethod("RunPhantoonFirstRoundFigureEight", flags)!
                .CreateDelegate<Action<RoomEnemySlot, PhantoonEnemyState>>(system);
            body.XPosition = 128; body.YPosition = 96; body.VariableE = 1; body.Parameter2 = 1;
            begin(body, state, (byte)(bucket * 2));
            ushort duration = ReadVerificationWord(rom, 0xa7cd53 + bucket * 2);
            AssertEqual(duration, eye.VariableA, "Native first-round closed-eye duration");
            AssertEqual((ushort)(reversed ? 1 : 0), eye.VariableC, "Direction selection remains independent of duration bucket");
            // A valid unexpired casual-flame hold keeps this fixture specific to
            // the identified closed-eye countdown and its actual expiry producer.
            mouth.VariableB = (ushort)(duration + 1);
            for (int elapsed = 1; elapsed <= duration; elapsed++)
            {
                tick(body, state); totalTicks++;
                AssertEqual((ushort)(duration - elapsed), eye.VariableA, "Closed-eye timer decrements once despite movement speed/cursor");
                if (elapsed < duration)
                {
                    AssertEqual((ushort)PhantoonAiFunction.MoveInFigureEightThenOpenEye, body.VariableF, "No early eye opening");
                    AssertEqual(0, system.EnemyProjectiles.Count(projectile => projectile.IsActive), "No expiry spiral before selected duration");
                }
            }
            AssertEqual((ushort)PhantoonAiFunction.NoOperation, body.VariableF, "Selected duration stops moving phase");
            AssertEqual(PhantoonInstructionProgramDefinitions.EyeOpen, eye.CurrentInstruction, "Expiration starts exact eye-opening program");
            AssertEqual((ushort)1, eye.InstructionTimer, "Eye-opening program executes on next instruction step");
            AssertEqual((ushort)0, body.Parameter2, "Opening clears flame-rain trigger");
            AssertEqual((ushort)1, mouth.VariableB, "Casual hold still advances on every movement call");
            AssertEqual(1, randomCalls, "Only independent initial direction consumes RNG in this fixture");
            var flames = system.EnemyProjectiles.Where(projectile => projectile.IsActive).ToArray();
            AssertEqual(8, flames.Length, "Exact expiry spawns eight real spiral projectiles");
            foreach (var flame in flames)
            {
                AssertEqual(RoomEnemyProjectileKind.PhantoonDestroyableFlame, flame.Kind, "Expiry produces actual Phantoon flame family");
                AssertEqual(body.XPosition, flame.XPosition, "Spiral begins at current independently traversed body X");
                AssertEqual(unchecked((ushort)(body.YPosition + 16)), flame.YPosition, "Spiral begins at current body offset Y");
            }
        }
        AssertEqual(2280, totalTicks, "Three native closed-eye durations in both directions");
        Console.WriteLine("Phantoon closed eye:60/360/720 native waits,2280 actual moving countdown calls,both directions/exact eye-open handoffs and48 real spiral spawns pass.");
    }
    private static void VerifyLookupStream5CeresPlatform(SuperMetroidAddressSpace rom)
    {
        var document = new CeresDoorVisualDocument
        {
            Version = 1,
            Normal = Colors(CeresDoorVisualRomData.NormalColors,15),
            Escape = Colors(CeresDoorVisualRomData.EscapeColors,15),
            Animation = Enumerable.Range(0,8).Select(row => Colors(CeresDoorVisualRomData.AnimationColors +16*row,6)).ToArray(),
            Mode7DoorFrames = Enumerable.Range(0,2).Select(frame => Enumerable.Range(0,4)
                .Select(index => (int)rom.ReadByte(CeresDoorVisualRomData.Mode7FirstFrameSource +4*frame+index)).ToArray()).ToArray(),
        };
        byte[] planar = Enumerable.Range(0,CeresDoorVisualRomData.TileByteCount)
            .Select(index => rom.ReadByte(CeresDoorVisualRomData.TileSource+index)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar,4,RoomCharacterAtlasFormat.TileColumns,out int width,out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png,width,height,pixels,SnesGraphics.DiagnosticPalette(16));
        byte[] pngBytes = png.ToArray();
        CeresDoorVisualCatalog Load(CeresDoorVisualDocument value) => CeresDoorVisualCatalog.Load(
            new MemoryStream(pngBytes),new MemoryStream(CeresDoorVisualCatalog.Write(value)));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var stock = Load(document);
        CheckPlatformActor(stock, document);
        CheckHash(stock, document);
        AssertEqual(0, ((Dictionary<int,byte>)typeof(CeresDoorVisualCatalog).GetField("platformEdits", flags)!.GetValue(stock)!).Count, "No stored stock platform cells");
        for (int frame = 0; frame < 2; frame++)
        for (int cell = 0; cell < 4; cell++)
        {
            int[][] frames = document.Mode7DoorFrames.Select(row => row.ToArray()).ToArray();
            frames[frame][cell] ^= 255;
            var changed = document with { Mode7DoorFrames = frames };
            var edited = Load(changed);
            AssertEqual(1, ((Dictionary<int,byte>)typeof(CeresDoorVisualCatalog).GetField("platformEdits", flags)!.GetValue(edited)!).Count, "One independent platform override");
            CheckPlatformActor(edited, changed);
            CheckHash(edited, changed);
        }
        AssertThrows<IndexOutOfRangeException>(() => stock.LoadMode7DoorFrame(new SnesVram(), -1), "Platform lower frame domain");
        AssertThrows<IndexOutOfRangeException>(() => stock.LoadMode7DoorFrame(new SnesVram(), 2), "Platform upper frame domain");
        AssertThrows<ArgumentNullException>(() => stock.LoadMode7DoorFrame(null!, 0), "Platform preserves root null validation");
        AssertThrows<IndexOutOfRangeException>(() => CeresMode7TransferDefinitions.PlatformTile(0, -1), "Canonical platform lower column domain");
        AssertThrows<IndexOutOfRangeException>(() => CeresMode7TransferDefinitions.PlatformTile(0, 4), "Canonical platform upper column domain");
        Console.WriteLine("Ceres platform: eight native cells/eight independent edits,99 actual phase calls through wrap, high-byte/neighbor preservation, canonical hashes and domains pass.");
        PaletteRgb5[] Colors(int source,int count) => Enumerable.Range(0,count).Select(index =>
        {
            ushort word = ReadVerificationWord(rom,source+2*index);
            return new PaletteRgb5 { Red = word &31, Green = (word>>5)&31, Blue = (word>>10)&31 };
        }).ToArray();
        void CheckPlatformActor(CeresDoorVisualCatalog visual, CeresDoorVisualDocument expected)
        {
            var system = new RoomEnemySystem
            {
                TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
                    new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>(), ceresDoorVisual: visual),
            };
            var vram = new SnesVram();
            int destination = CeresDoorVisualRomData.Mode7DestinationWord;
            byte[] initial = Enumerable.Range(0, 12).Select(index => (byte)(index % 2 == 0 ? 0x5d : 0xa7)).ToArray();
            vram.LoadBytes((destination - 1) * 2, initial);
            typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(system, vram);
            typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(system, new SnesCgram());
            var slots = (RoomEnemySlot[])typeof(RoomEnemySystem).GetField("_slots", flags)!.GetValue(system)!;
            var animate = typeof(RoomEnemySystem).GetMethod("RunCeresDoorPaletteAnimation", flags)!;
            foreach (ushort tick in new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 0xfffe, 0xffff, 0 })
            {
                slots[0].FrameCounter = tick;
                animate.Invoke(system, null);
                int frame = (tick & 2) >> 1;
                for (int cell = 0; cell < 4; cell++)
                {
                    AssertEqual((ushort)(0xa700 | expected.Mode7DoorFrames[frame][cell]), vram.ReadWord(destination + cell), "Actual platform phase and character-byte preservation");
                    AssertEqual(rom.ReadByte(0xa6f918 + frame * 4 + cell), CeresMode7TransferDefinitions.PlatformTile(frame, cell), "Canonical platform cap/atlas geometry matches native");
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

    private static void VerifyLookupStream5PhantoonExposure(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        int ticks = 0;
        foreach (int bucket in new[] { 2, 1, 0 })
        foreach (bool shot in new[] { false, true })
        {
            var system = new RoomEnemySystem();
            var body = system.Slots[0];
            var eye = system.Slots[1];
            var tentacles = system.Slots[2];
            var state = new PhantoonEnemyState(body) { Eye = eye, Tentacles = tentacles, Mouth = system.Slots[3] };
            typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(system, (Func<ushort>)(() => (ushort)bucket));
            var open = typeof(RoomEnemySystem).GetMethod("BeginPhantoonEyeTracking", flags)!.CreateDelegate<Action<PhantoonEnemyState>>(system);
            var tick = typeof(RoomEnemySystem).GetMethod("RunPhantoonEyeTracking", flags)!.CreateDelegate<Action<RoomEnemySlot, PhantoonEnemyState, SamusState>>(system);
            body.XPosition = 128; body.YPosition = 96;
            open(state);
            ushort duration = ReadVerificationWord(rom, 0xa7cd41 + bucket * 2);
            AssertEqual(duration, body.VariableE, "Native selected vulnerability duration");
            AssertTrue((body.Properties & (ushort)EnemyProperties.IgnoreSamusCollision) == 0, "Eye opening enables collision");
            tentacles.VariableA = (ushort)(shot ? 1 : 0);
            tentacles.VariableB = 37;
            var samus = new SamusState { XPosition = 192, YPosition = 96 };
            for (int elapsed = 1; elapsed <= duration; elapsed++)
            {
                tick(body, state, samus); ticks++;
                AssertEqual((ushort)128, body.XPosition, "Exposure duration does not advance a spatial trajectory");
                AssertEqual((ushort)96, body.YPosition, "Exposure body remains at current Y");
                if (elapsed < duration)
                {
                    AssertEqual((ushort)(duration - elapsed), body.VariableE, "One exact countdown decrement per actor call");
                    AssertEqual((ushort)PhantoonAiFunction.EyeTracksSamus, body.VariableF, "No early exposure transition");
                    AssertTrue((body.Properties & (ushort)EnemyProperties.IgnoreSamusCollision) == 0, "Exposure remains collidable before expiry");
                    AssertEqual((ushort)37, tentacles.VariableB, "Round damage remains until expiration");
                }
            }
            AssertEqual((ushort)0, tentacles.VariableB, "Expiration resets accumulated round damage");
            if (shot)
            {
                AssertEqual((ushort)PhantoonAiFunction.BecomeSolidAndSwoop, body.VariableF, "Shot-triggered expiration selects swoop");
                AssertEqual(ReadVerificationWord(rom, 0xa7d620), body.VariableE, "Separate native swoop setup hold preserved");
                AssertEqual((ushort)0, tentacles.VariableA, "Expiration consumes swoop request");
            }
            else
            {
                AssertEqual((ushort)PhantoonAiFunction.NoOperation, body.VariableF, "Unhit expiration hands off to eye close");
                AssertEqual(PhantoonInstructionProgramDefinitions.InvulnerableBody, body.CurrentInstruction, "Native invulnerable body handoff");
                AssertEqual(PhantoonInstructionProgramDefinitions.EyeCloseAndPickNewPattern, eye.CurrentInstruction, "Native close/reselect eye handoff");
                AssertTrue((body.Properties & (ushort)EnemyProperties.IgnoreSamusCollision) != 0, "Expiration removes open-eye collision");
                AssertEqual((ushort)1, body.Parameter2, "Unhit expiration requests next rain branch");
            }
        }
        AssertEqual(210, ticks, "All three actual exposure lengths and both expiry branches");
        Console.WriteLine("Phantoon exposure: three native window lengths,210 actual countdown calls, exact collision/shot/close transitions and separate swoop hold pass.");
    }
    private static void VerifyLookupStream5CeresOverlayWords(SuperMetroidAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.CeresEscapeOverlayTilemapFiles.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<CeresEscapeOverlayTilemapDocument>(json, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!;
        CeresEscapeOverlayTilemapCatalog stock = Check(document);
        var edits = (System.Collections.IDictionary)typeof(CeresEscapeOverlayTilemapCatalog).GetField("edits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
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
            var system = new RoomEnemySystem
            {
                TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
                    new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>(),
                    ceresEscapeOverlayTilemaps: result),
            };
            var queue = new VramWriteQueue();
            typeof(RoomEnemySystem).GetMethod("QueueCeresEmergencyText", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(system, [queue]);
            AssertEqual(1, queue.Entries.Count, "Actual emergency producer queues one installed page");
            VramWriteEntry entry = queue.Entries[0];
            AssertEqual(ReadVerificationWord(rom, 0xa6c15d), entry.SizeInBytes, "Native emergency byte extent");
            AssertEqual(ReadVerificationWord(rom, 0xa6c162), entry.EncodedVramDestination, "Native emergency BG destination");
            AssertEqual(CeresEscapeOverlayTilemapDefinitions.Emergency.SourceAddress, entry.SourceAddress, "Actual emergency source identity");
            AssertTrue(result.TryResolve(entry.SourceAddress, entry.SizeInBytes, out var queued), "Actual emergency transfer resolves installed words");
            for (int index = 0; index < source.Pages["emergency"].Length; index++)
                AssertEqual(source.Pages["emergency"][index], System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(queued.Span[(index * 2)..]), "Actual queued emergency preserves independent edits");
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
    private static void VerifyLookupStream5CrocomireSharedPaint(SuperMetroidAddressSpace rom)
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
        AssertTrue(typeof(CrocomireColorCatalog).GetField("paint", BindingFlags.Instance | BindingFlags.NonPublic) is null, "Stock palette storage has been removed");
        var edits = (System.Collections.IDictionary)typeof(CrocomireColorCatalog).GetField("edits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertTrue(!typeof(CrocomirePaintDefinitions).GetFields(BindingFlags.Static | BindingFlags.NonPublic).Any(field => field.FieldType.IsArray), "Material calculation does not cache an output palette");
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
            Func<int, ushort>[] resolve = [result.ResolveFightBody, result.ResolveInitialWall, result.ResolveInitialProjectile, result.ResolveSkeletonArm, result.ResolveWallSpikes];
            var cgram = new SnesCgram(); result.ApplyInitial(cgram);
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
    private static void VerifyLookupStream5CeresSourcePages()
    {
        CeresEscapeTileSheetDefinition[] tilePages = [new(0xb7da00, 0x900, "ceres-escape-warning-tiles.png"), new(0xb0ba00, 0x600, "ceres-escape-door-tiles.png")];
        AssertTrue(tilePages.SequenceEqual(CeresEscapeTileArtworkDefinitions.All), "Original named character page order/source/extent/file");
        foreach (var page in tilePages)
        {
            AssertTrue(CeresEscapeTileArtworkDefinitions.Contains(page.SourceAddress, page.ByteCount), "Whole page admitted");
            AssertTrue(CeresEscapeTileArtworkDefinitions.Contains(page.SourceAddress + page.ByteCount - 1, 1), "Last byte admitted");
            AssertTrue(!CeresEscapeTileArtworkDefinitions.Contains(page.SourceAddress - 1, 1) && !CeresEscapeTileArtworkDefinitions.Contains(page.SourceAddress + page.ByteCount, 1), "Outside page rejected");
            AssertTrue(!CeresEscapeTileArtworkDefinitions.Contains(page.SourceAddress, 0), "Zero byte page rejected");
        }
        CeresEscapeOverlayTilemapDefinition[] overlays = [new("emergency", 0xa6c164, 9), new("japanese_0", 0xa6c3f4, 12), new("japanese_1", 0xa6c40c, 12), new("japanese_2", 0xa6c424, 11), new("japanese_3", 0xa6c43a, 11)];
        AssertTrue(overlays.SequenceEqual(CeresEscapeOverlayTilemapDefinitions.All), "Original named overlay order/source/extent");
        foreach (var page in overlays)
        {
            AssertTrue(CeresEscapeOverlayTilemapDefinitions.IsSource(page.SourceAddress, page.WordCount * 2), "Exact overlay extent admitted");
            AssertTrue(!CeresEscapeOverlayTilemapDefinitions.IsSource(page.SourceAddress, page.WordCount * 2 - 1), "Partial overlay extent rejected");
            AssertTrue(CeresEscapeOverlayTilemapDefinitions.ContainsByteAddress(page.SourceAddress) && CeresEscapeOverlayTilemapDefinitions.ContainsByteAddress(page.SourceAddress + page.WordCount * 2 - 1), "Overlay byte endpoints admitted");
        }
        AssertThrows<IndexOutOfRangeException>(() => _ = CeresEscapeTileArtworkDefinitions.All[-1], "Tile page lower bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = CeresEscapeTileArtworkDefinitions.All[2], "Tile page upper bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = CeresEscapeOverlayTilemapDefinitions.All[-1], "Overlay lower bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = CeresEscapeOverlayTilemapDefinitions.All[5], "Overlay upper bound");
        Console.WriteLine("Ceres source page cases: two character sheets/five overlays preserve exact identities, order, extents, enumeration and range domains.");
    }
    private static void VerifyLookupStream5MapLandmarkCases(SuperMetroidAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.MapLandmarkExtractor.Extract(rom);
        MapLandmarkLayout layout = MapLandmarkLayout.Load(new MemoryStream(extracted));
        var artwork = SuperMetroid.AssetExtraction.MapSpriteExtractor.Extract(rom);
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
            Markers = expectedIds.ToDictionary(id => id, id => layout.Get(id) with { X = layout.Get(id).X + 1, Y = layout.Get(id).Y + 1 }) });
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
            AssertEqual(count, bosses.Count, "Native boss slot count");
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
            AssertThrows<IndexOutOfRangeException>(() => _ = elevators[-1], "Elevator lower bound");
            AssertThrows<IndexOutOfRangeException>(() => _ = elevators[count], "Elevator upper bound");
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
            icons.BindLandmarks(adjustment == 0 ? layout : editedLayout); icons.BindSprites(sprites);
            var actual = new OamBuffer(); var expected = new OamBuffer();
            icons.DrawBossMarkers(actual, 7, 9);
            int pointer = Root(FileSelectMapIconRomData.BossLists, area), bits = defeated ? 1 : 0;
            if (pointer != 0)
                for (int i = 0; Read(pointer + i * 4) != ushort.MaxValue; i++, bits >>= 1)
                {
                    int x = Read(pointer + i * 4), y = Read(pointer + i * 4 + 2);
                    if (x == 0xfffe) continue;
                    if ((bits & 1) != 0)
                    {
                        Draw(FileSelectMapIconRomData.DefeatedBoss, x, y, FileSelectMapRomData.StationMarkerPalette);
                        Draw(FileSelectMapIconRomData.Boss, x, y, FileSelectMapIconRomData.DefeatedBossPalette);
                    }
                    else if (downloaded) Draw(FileSelectMapIconRomData.Boss, x, y, FileSelectMapRomData.StationMarkerPalette);
                }
            if (area != AreaId.Ceres)
            {
                icons.DrawAfterMarker(actual, 7, 9);
                if (area == AreaId.Crateria)
                {
                    int ship = Root(FileSelectMapRomData.SavePointMapPointers, area);
                    Draw(FileSelectMapIconRomData.Gunship, Read(ship), Read(ship + 2), FileSelectMapRomData.StationMarkerPalette);
                }
                if (downloaded)
                {
                    pointer = Root(FileSelectMapIconRomData.ElevatorLists, area);
                    for (int i = 0; Read(pointer + i * 6) != ushort.MaxValue; i++)
                        Draw((ushort)Read(pointer + i * 6 + 4), Read(pointer + i * 6), Read(pointer + i * 6 + 2), 0);
                }
            }
            AssertTrue(actual.LowTable.SequenceEqual(expected.LowTable) && actual.HighTable.SequenceEqual(expected.HighTable), "Actual native ordered landmark OAM");
            AssertEqual(defeated ? (byte)1 : (byte)0, system.GetBossBitsRaw(area), "Drawing preserves boss bits");
            AssertEqual(downloaded, system.HasAreaMap(area), "Drawing preserves map state");
            void Draw(ushort id, int x, int y, ushort palette) => sprites.Draw(id, expected, unchecked((ushort)(x + adjustment - 7)), unchecked((ushort)(y + adjustment - 9)), palette);
        }
        AssertThrows<ArgumentOutOfRangeException>(() => MapLandmarkDefinitions.Bosses((AreaId)7), "Invalid boss area");
        AssertThrows<ArgumentOutOfRangeException>(() => MapLandmarkDefinitions.Elevators(AreaId.Ceres), "Ceres has no elevator labels");
        AssertEqual(8, bossSlots, "Eight native boss slots including three unused");
        AssertEqual(17, elevatorSlots, "Seventeen native destination labels");
        Console.WriteLine("Map landmark cases: eight native boss slots, seventeen destinations, extraction/schema, 23 ordered IDs, bounds and 56 stock/edited ordered OAM/state cases pass.");
    }
    private static void VerifyLookupStream5PhantoonCollision(SuperMetroidAddressSpace rom)
    {
        int componentCount = 0;
        var lists = new HashSet<ushort>();
        foreach (EnemyBg2FrameDefinition frame in PhantoonBg2FrameDefinitions.Frames)
        {
            int root = 0xa70000 | frame.Pointer;
            var components = PhantoonCollisionDefinitions.ComponentsAt(frame.Pointer);
            AssertEqual((int)ReadVerificationWord(rom, root), components.Count, "Native component extent");
            for (int index = 0; index < components.Count; index++)
            {
                int record = root + 2 + index * 8;
                PhantoonCollisionComponent component = components[index];
                AssertEqual(unchecked((short)ReadVerificationWord(rom, record)), component.X, "Native component X");
                AssertEqual(unchecked((short)ReadVerificationWord(rom, record + 2)), component.Y, "Native component Y");
                AssertEqual(ReadVerificationWord(rom, record + 6), component.HitboxPointer, "Native collision identity");
                componentCount++;
                if (!lists.Add(component.HitboxPointer)) continue;
                int address = 0xa70000 | component.HitboxPointer;
                var hitboxes = PhantoonCollisionDefinitions.HitboxesAt(component.HitboxPointer);
                AssertEqual((int)ReadVerificationWord(rom, address), hitboxes.Count, "Native hitbox extent");
                for (int hitbox = 0; hitbox < hitboxes.Count; hitbox++)
                {
                    int native = address + 2 + hitbox * 12;
                    var expected = new PhantoonCollisionHitbox(unchecked((short)ReadVerificationWord(rom, native)),
                        unchecked((short)ReadVerificationWord(rom, native + 2)), unchecked((short)ReadVerificationWord(rom, native + 4)),
                        unchecked((short)ReadVerificationWord(rom, native + 6)), ReadVerificationWord(rom, native + 8), ReadVerificationWord(rom, native + 10));
                    AssertEqual(expected, hitboxes[hitbox], "Native shape and callback pair");
                }
                AssertThrows<IndexOutOfRangeException>(() => _ = hitboxes[-1], "Hitbox lower bound");
                AssertThrows<IndexOutOfRangeException>(() => _ = hitboxes[hitboxes.Count], "Hitbox upper bound");
            }
            AssertEqual(components.Count, components.ToArray().Length, "Component enumeration compatibility");
            AssertThrows<IndexOutOfRangeException>(() => _ = components[components.Count], "Component upper bound");
        }
        AssertEqual(25, componentCount, "All native components");
        AssertEqual(3, lists.Count, "All native hitbox lists");
        AssertEqual(PhantoonCollisionDefinitions.ShotAi, FindPhantoonHitboxCallback(null!, rom,
            PhantoonBg2FrameDefinitions.BodyFullHitbox, 128, 128, true), "Actual full-body shot callback");
        AssertEqual(PhantoonCollisionDefinitions.TouchAi, FindPhantoonHitboxCallback(null!, rom,
            PhantoonBg2FrameDefinitions.BodyFullHitbox, 128, 128, false), "Actual full-body touch callback");
        AssertEqual(PhantoonCollisionDefinitions.ShotAi, FindPhantoonHitboxCallback(null!, rom,
            PhantoonBg2FrameDefinitions.BodyEyeHitboxOnly, 128, 153, true), "Actual vulnerable eye callback");
        AssertEqual((ushort)0, FindPhantoonHitboxCallback(null!, rom,
            PhantoonBg2FrameDefinitions.BodyEyeHitboxOnly, 128, 128, true), "Eye-only frame excludes body collision");
        AssertThrows<InvalidDataException>(() => PhantoonCollisionDefinitions.ComponentsAt(0), "Frame domain");
        AssertThrows<InvalidDataException>(() => PhantoonCollisionDefinitions.HitboxesAt(0), "Hitbox domain");
        Console.WriteLine("Phantoon collision: 22 native frames, 25 components, all seven rectangles/callbacks, actual full-body/eye selection and bounds pass.");
    }
    private static void VerifyLookupStream5PhantoonFade(SuperMetroidAddressSpace rom)
    {
        byte[] bytes = SuperMetroid.AssetExtraction.PhantoonColorExtractor.Extract(rom);
        PhantoonColorDocument document = System.Text.Json.JsonSerializer.Deserialize<PhantoonColorDocument>(bytes,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase })!;
        PhantoonColorCatalog stock = Check(document);
        var edits = (System.Collections.IDictionary)typeof(PhantoonColorCatalog).GetField("fadeOutEdits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
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
        var healthEdits = (System.Collections.IDictionary)typeof(PhantoonColorCatalog).GetField("healthEdits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertEqual(0, healthEdits.Count, "All health colors calculate without stock overrides");
        AssertTrue(typeof(PhantoonColorCatalog).GetField("requiredHealthChannels", BindingFlags.Instance | BindingFlags.NonPublic) is null,
            "No per-channel native residual samples remain");
        AssertTrue(typeof(PhantoonColorCatalog).GetField("healthyPaint", BindingFlags.Instance | BindingFlags.NonPublic) is null,
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
        var powerEdits = (System.Collections.IDictionary)typeof(PhantoonColorCatalog).GetField("powerEdits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertTrue(typeof(PhantoonColorCatalog).GetField("requiredPowerColors", BindingFlags.Instance | BindingFlags.NonPublic) is null, "Power targets retain no stock color dictionary");
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
    private static void VerifyLookupStream5MapButtons(SuperMetroidAddressSpace rom)
    {
        var native = new ushort[4];
        for (int i = 0; i < native.Length; i++)
        {
            native[i] = ReadVerificationWord(rom, FileSelectMapRomData.ScrollArrows + i * 10 + 6);
            AssertEqual(i + 1, (int)ReadVerificationWord(rom, FileSelectMapRomData.ScrollArrows + i * 10 + 8), "Native direction identity");
            AssertEqual(native[i], MapScrollControls.ButtonFor((MapScrollDirection)(i + 1)), "Native directional mask");
            AssertEqual(native[i], MapScrollControls.Buttons[i], "Calculated ordered view");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => MapScrollControls.ButtonFor(MapScrollDirection.None), "No direction has no binding");
        AssertThrows<ArgumentOutOfRangeException>(() => MapScrollControls.ButtonFor((MapScrollDirection)5), "Invalid direction");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = MapScrollControls.Buttons[-1], "Lower index bound");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = MapScrollControls.Buttons[4], "Upper index bound");
        var state = new Bank80SystemState();
        state.MarkExploredMapTile(AreaId.Crateria, 0, 0);
        state.MarkExploredMapTile(AreaId.Crateria, 63, 31);
        var map = new LookupStream5ScrollMap();
        var field = typeof(FileSelectMapScroll).GetField("customButtons", BindingFlags.Instance | BindingFlags.NonPublic)!;
        AssertTrue(field.GetValue(new FileSelectMapScroll(rom, map, state, 240, 128)) is null, "Default stores no binding table");
        AssertTrue(field.GetValue(new FileSelectMapScroll(rom, map, state, 240, 128, native)) is null, "Native injection stores no duplicate table");
        ushort[] custom = [1, 2, 4, 8];
        for (int selected = 0; selected < 4; selected++)
        {
            var stock = new FileSelectMapScroll(rom, map, state, 240, 128);
            var edited = new FileSelectMapScroll(rom, map, state, 240, 128, custom);
            ushort startX = stock.Horizontal, startY = stock.Vertical;
            for (int tick = 1; tick <= 8; tick++)
            {
                AssertEqual(tick == 8, stock.Step(tick == 1 ? native[selected] : (ushort)0), "Stock sound boundary");
                AssertEqual(tick == 8, edited.Step(tick == 1 ? custom[selected] : (ushort)0), "Custom sound boundary");
                AssertEqual((stock.Horizontal, stock.Vertical, stock.Direction), (edited.Horizontal, edited.Vertical, edited.Direction), "Custom actual direction and trajectory");
                int delta = tick >= 4 ? 8 : 0;
                AssertEqual(unchecked((ushort)(startX + (selected == 0 ? -delta : selected == 1 ? delta : 0))), stock.Horizontal, "Horizontal pulse");
                AssertEqual(unchecked((ushort)(startY + (selected == 2 ? -delta : selected == 3 ? delta : 0))), stock.Vertical, "Vertical pulse");
                AssertEqual(tick == 8 ? MapScrollDirection.None : (MapScrollDirection)(selected + 1), stock.Direction, "Accepted direction survives release");
            }
        }
        var priority = new FileSelectMapScroll(rom, map, state, 240, 128, custom);
        custom[0] = 0;
        _ = priority.Step(15);
        AssertEqual(MapScrollDirection.Left, priority.Direction, "Injected bindings copied; left takes priority");
        AssertThrows<ArgumentException>(() => _ = new FileSelectMapScroll(rom, map, state, 240, 128, new ushort[3]), "Binding extent");
        Console.WriteLine("Map buttons: four native semantic cases, domain bounds, no stock storage, actual default/custom four-direction pulses, release and priority pass.");
    }

    private sealed class LookupStream5ScrollMap : IAreaMapView
    {
        public AreaId Area => AreaId.Crateria;
        public MapTileWord GetTile(int x, int y) => default;
        public bool IsDiscoverable(int x, int y) => true;
        public bool IsRevealedByMapStation(int x, int y) => false;
        public bool RevealsCellAbove(int x, int y) => false;
    }
    private static void VerifyLookupStream5ZebesStarFields(SuperMetroidAddressSpace rom)
    {
        var frames = new Dictionary<string, SpriteVisualPart[]>();
        foreach (var definition in CeresDestructionSpriteDefinitions.Frames)
            frames.Add(definition.Name, SuperMetroid.AssetExtraction.IntroCinematicSpriteFrameExtractor.Extract(
                rom, definition.Pointer, definition.StockPartCount, definition.Name));
        var document = new CeresDestructionSpriteDocument { Version = 1, Frames = frames };
        var stock = Load(document);
        int total = 0;
        foreach (var definition in CeresDestructionSpriteDefinitions.Frames)
        {
            if (!definition.Name.StartsWith("zebes-stars-", StringComparison.Ordinal)) continue;
            SpriteVisualPart[] parts = frames[definition.Name];
            Check(stock, definition.Pointer, parts);
            var compiled = IntroCinematicSpriteCompiler.Compile(parts, definition.Name);
            var calculated = ZebesStarGridParts.CalculateIfMatching(definition.Pointer, compiled);
            AssertTrue(typeof(SpriteComposition).GetField("parts", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(calculated) is ZebesStarGridParts, "Stock stars use calculated common fields");
            for (int index = 0; index < parts.Length; index++)
            {
                int source = 0x8c0000 | definition.Pointer + 2 + index * 5;
                CompiledSpritePart actual = calculated.Part(index);
                AssertEqual(Word(source), actual.X.Raw, "Native star X/size");
                AssertEqual(rom.ReadByte(source + 2), actual.Y, "Native star Y");
                AssertEqual((ushort)(Word(source + 3) & ~0x0e00), actual.Attributes.Raw,
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
                    Check(Load(document with { Frames = editedFrames }), definition.Pointer, editedParts);
                }
                total++;
            }
        }
        AssertEqual(29, total, "All four native star sheets checked");
        Console.WriteLine("Zebes stars:29 native parts,261 independent field edits, actual ordered OAM and full content identity pass; only decorative position/glyph triples are retained.");
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        static CeresDestructionSpritePresentation Load(CeresDestructionSpriteDocument value)
        {
            using var json = new MemoryStream();
            CeresDestructionSpritePresentation.Write(json, value);
            json.Position = 0;
            var result = CeresDestructionSpritePresentation.Load(json);
            var expected = value.Frames.ToDictionary(pair => CeresDestructionSpriteDefinitions.Frames
                .Single(definition => definition.Name == pair.Key).Pointer,
                pair => IntroCinematicSpriteCompiler.Compile(pair.Value, pair.Key));
            AssertEqual(SelectedPresentationHash.FromCompositions(nameof(CeresDestructionSpritePresentation), expected),
                result.ContentIdentity, "Selected star edits retain canonical composition identity");
            return result;
        }
        static void Check(CeresDestructionSpritePresentation catalog, ushort pointer, SpriteVisualPart[] parts)
        {
            var expected = IntroCinematicSpriteCompiler.Compile(parts, "star-reference");
            var actualOam = new OamBuffer();
            var expectedOam = new OamBuffer();
            actualOam.BeginFrame(); expectedOam.BeginFrame();
            catalog.Draw(pointer, actualOam, 128, 112, 0x0800, true);
            expected.DrawOnScreen(expectedOam, 128, 112, 0x0800);
            AssertEqual(expectedOam.NextByteOffset, actualOam.NextByteOffset, "Star OAM count/order");
            AssertTrue(expectedOam.LowTable.SequenceEqual(actualOam.LowTable), "Exact star low OAM");
            AssertTrue(expectedOam.HighTable.SequenceEqual(actualOam.HighTable), "Exact star high OAM");
        }
    }
    private static void VerifyLookupStream5StatueRamps(SuperMetroidAddressSpace rom)
    {
        byte[] nativeJson = SuperMetroid.AssetExtraction.TourianStatueColorExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<TourianStatueColorDocument>(nativeJson,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        var stock = Check(document);
        CheckResidualCount("baseColors", 0);
        CheckResidualCount("greyColors", 1);
        foreach (bool grey in new[] { false, true })
        {
            PaletteRgb5[] source = grey ? document.Grey : document.Base;
            for (int color = 0; color < source.Length; color++)
            for (int channel = 0; channel < 3; channel++)
            {
                var changed = (PaletteRgb5[])source.Clone();
                changed[color] = channel switch
                {
                    0 => changed[color] with { Red = changed[color].Red ^ 1 },
                    1 => changed[color] with { Green = changed[color].Green ^ 1 },
                    _ => changed[color] with { Blue = changed[color].Blue ^ 1 },
                };
                Check(grey ? document with { Grey = changed } : document with { Base = changed });
            }
        }
        AssertThrows<IndexOutOfRangeException>(() => stock.ResolveBase(-1), "Base ramp lower bound");
        AssertThrows<IndexOutOfRangeException>(() => stock.ResolveGrey(8), "Grey ramp upper bound");
        Console.WriteLine("Tourian statue ramps: all56 native palette colors,72 independent channel edits, actual CGRAM and canonical identities pass; one grey residual color and independent endpoints/outside colors remain pending.");

        void CheckResidualCount(string field, int expected)
        {
            object ramp = typeof(TourianStatueColorCatalog).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
            var edits = (Dictionary<int, ushort>)ramp.GetType().GetField("edits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(ramp)!;
            AssertEqual(expected, edits.Count, "Exact stock ramp residual count");
        }

        static ushort Pack(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);

        static TourianStatueColorCatalog Check(TourianStatueColorDocument expected)
        {
            var actual = TourianStatueColorCatalog.Load(new MemoryStream(TourianStatueColorCatalog.Write(expected), writable: false));
            var cgram = new SnesCgram();
            actual.ApplyEntrance(cgram);
            for (int color = 0; color < expected.Base.Length; color++)
            {
                AssertEqual(Pack(expected.Base[color]), actual.ResolveBase(color), "Exact independent base ramp color");
                AssertEqual(Pack(expected.Base[color]), cgram.Colors[TourianStatuePaletteRomData.BaseCgramIndex + color], "Actual base CGRAM");
                AssertEqual(Pack(expected.Statue[color]), cgram.Colors[TourianStatuePaletteRomData.StatueCgramIndex + color], "Unchanged statue CGRAM");
            }
            actual.ApplyGrey(cgram, 0);
            for (int color = 0; color < expected.Grey.Length; color++)
            {
                AssertEqual(Pack(expected.Grey[color]), actual.ResolveGrey(color), "Exact independent grey ramp color");
                AssertEqual(Pack(expected.Grey[color]), cgram.Colors[color], "Actual grey CGRAM");
            }
            for (int row = 0; row < expected.Eye.Length; row++)
            {
                actual.ApplyEye(cgram, (ushort)(row * 2));
                for (int color = 0; color < expected.Eye[row].Length; color++)
                    AssertEqual(Pack(expected.Eye[row][color]), cgram.Colors[TourianStatuePaletteRomData.EyeCgramIndex + color], "Unchanged eye CGRAM");
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

    private static void VerifyLookupStream5PhantoonMarkers(SuperMetroidAddressSpace rom)
    {
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes("Super Metroid.smc"))), "Marker reference ROM revision");
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        var body = enemies.Slots[0];
        var state = new PhantoonEnemyState(body)
        {
            Eye = enemies.Slots[1], Tentacles = enemies.Slots[2], Mouth = enemies.Slots[3],
        };
        var shot = typeof(RoomEnemySystem).GetMethod("ResolvePhantoonShotReaction", flags)!
            .CreateDelegate<Action<RoomEnemySlot, PhantoonEnemyState, ushort, ushort>>(enemies);
        ushort random = 0;
        int calls = 0;
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(enemies,
            (Func<ushort>)(() => { calls++; return random; }));
        for (int bucket = 0; bucket < 8; bucket++)
        {
            random = (ushort)(0xfff8 | bucket);
            body.Health = 1000;
            body.AiHandlerBits = 2;
            body.VariableF = (ushort)PhantoonAiFunction.EyeTracksSamus;
            body.VariableE = 60;
            state.Tentacles.VariableA = state.Tentacles.VariableB = 0;
            byte expected = rom.ReadByte(0xa7cda5 + bucket);
            AssertEqual(expected, PhantoonPatternDefinitions.ShotEyeMarkers[bucket], "Exact retained native RNG bucket");
            shot(body, state, 0x100, 1);
            AssertEqual((ushort)expected, state.Eye.VariableB, "Actual exposed eye marker");
            AssertEqual((ushort)bucket, state.Mouth.Parameter2, "Actual selected RNG pattern");
            AssertEqual((ushort)16, body.VariableE, "Actual shot window");
            AssertEqual(bucket + 1, calls, "One preserved RNG call per shot");
        }
        Console.WriteLine("Phantoon retained random markers: eight SHA-verified native bytes and eight actual bus-free shot writes preserve RNG, pattern and timing.");
    }


    private static void VerifyLookupStream5MagdollitePulse(SuperMetroidAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.MagdollitePaletteCycleExtractor.Extract(rom);
        var native = System.Text.Json.JsonSerializer.Deserialize<MagdollitePaletteCycleDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        MagdollitePaletteCycle stock = Check(native);
        var residuals = (Dictionary<int, ushort>)typeof(MagdollitePaletteCycle)
            .GetField("edits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
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
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ApplyFrame(new SnesCgram(), -1, 0),
            "Magdollite pulse lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.ApplyFrame(new SnesCgram(), 4, 0),
            "Magdollite pulse upper frame bound");
        Console.WriteLine("Magdollite pulse:16 native colors,48 independent RGB edits, actual CGRAM, canonical identity and frame bounds pass; four independent glow colors remain pending.");

        static MagdollitePaletteCycle Check(MagdollitePaletteCycleDocument expected)
        {
            var actual = MagdollitePaletteCycle.Load(new MemoryStream(MagdollitePaletteCycle.Write(expected), writable: false));
            var cgram = new SnesCgram();
            ushort[][] packed = expected.Frames.Select(row => row.Select(rgb =>
                (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10)).ToArray()).ToArray();
            for (int frame = 0; frame < packed.Length; frame++)
            {
                actual.ApplyFrame(cgram, frame, 144);
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
    private static void VerifyLookupStream5ZebetitePulse(SuperMetroidAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.ZebetiteColorExtractor.Extract(rom);
        var native = System.Text.Json.JsonSerializer.Deserialize<ZebetiteColorDocument>(json,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        ZebetiteColorCatalog stock = Check(native);
        var residuals = (Dictionary<int, ushort>)typeof(ZebetiteColorCatalog)
            .GetField("edits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
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
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Apply(new SnesCgram(), -1, 0),
            "Zebetite pulse lower frame bound");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Apply(new SnesCgram(), 8, 0),
            "Zebetite pulse upper frame bound");
        Console.WriteLine("Zebetite pulse:16 native colors,48 independent RGB edits, actual CGRAM, canonical identity and frame bounds pass; selected barrier-core paint disposition complete.");

        static ZebetiteColorCatalog Check(ZebetiteColorDocument expected)
        {
            var actual = ZebetiteColorCatalog.Load(new MemoryStream(ZebetiteColorCatalog.Write(expected), writable: false));
            var cgram = new SnesCgram();
            ushort[][] packed = expected.Frames.Select(row => row.Select(rgb =>
                (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10)).ToArray()).ToArray();
            for (int frame = 0; frame < packed.Length; frame++)
            {
                actual.Apply(cgram, frame, ZebetiteDefinitions.PaletteDestinationColor);
                for (int color = 0; color < packed[frame].Length; color++)
                    AssertEqual(packed[frame][color],
                        cgram.Colors[ZebetiteDefinitions.PaletteDestinationColor + color],
                        "Zebetite pulse exact selected CGRAM color");
            }
            string identity = SelectedPresentationHash.Create("ZebetiteColorCatalog-v1",
                content => content.AppendWordFrames("frames", packed));
            AssertEqual(identity, actual.ContentIdentity, "Zebetite pulse canonical identity");
            return actual;
        }
    }
    private static void VerifyLookupStream5YappingMawOffsets(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
        var begin = typeof(RoomEnemySystem).GetMethod("BeginYappingMawExtension", flags)!
            .CreateDelegate<Action<RoomEnemySlot,YappingMawEnemyState>>();
        var set = typeof(RoomEnemySystem).GetMethod("SetYappingMawHeldOffset", flags)!
            .CreateDelegate<Action<YappingMawEnemyState,int>>();
        var slot = new RoomEnemySystem().Slots[0];
        var state = new YappingMawEnemyState(slot) { DistanceToSamus = 32 };
        for (int direction = 0; direction < 8; direction++)
        {
            var offset = YappingMawRomData.HeldSamusOffset(direction);
            AssertEqual(Word(0xa8a0a7 + direction * 4), unchecked((ushort)offset.X), "Native held X");
            AssertEqual(Word(0xa8a0a9 + direction * 4), unchecked((ushort)offset.Y), "Native held Y");
            set(state, direction);
            Check(direction, "Actual held-offset callback");
        }
        for (ushort angle = 0; angle < 256; angle++)
        {
            state.AimAngle = angle;
            begin(slot, state);
            int direction = ((angle + 16) & 255) >> 5;
            AssertEqual((ushort)(direction * 2), state.DirectionTableByteOffset,
                "Actual angle quantization/wrap retained");
            Check(direction, "Actual begin-extension held offset");
        }
        AssertThrows<IndexOutOfRangeException>(() => YappingMawRomData.HeldSamusOffset(-1), "Held direction lower bound");
        AssertThrows<IndexOutOfRangeException>(() => YappingMawRomData.HeldSamusOffset(8), "Held direction upper bound");
        Console.WriteLine("Yapping Maw offsets:16 native words, eight actual held callbacks,256 angle-quantized begin-extension writes and direction bounds pass.");

        void Check(int direction, string context)
        {
            AssertEqual(Word(0xa8a0a7 + direction * 4), state.HeldSamusXOffset, context);
            AssertEqual(Word(0xa8a0a9 + direction * 4), state.HeldSamusYOffset, context);
        }
    }
    private static void VerifyLookupStream5EyeGeometry(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int direction = 0; direction < 4; direction++)
        {
            var expected = (unchecked((short)ReadVerificationWord(rom, 0xa890ca + direction * 2)),
                unchecked((short)ReadVerificationWord(rom, 0xa890d2 + direction * 2)),
                ReadVerificationWord(rom, 0xa890da + direction * 2));
            AssertEqual(expected, MorphBallEyeGeometryDefinitions.Mount(direction), "Native directional mount tuple");
            foreach (ushort origin in new ushort[] { 0, 65535 })
            {
                var enemies = new RoomEnemySystem();
                var slot = enemies.Slots[0];
                slot.EnemyDefinitionPointer = RoomEnemySystem.MorphBallEyeDefinition;
                slot.Parameter2 = (ushort)(0x8000 | direction);
                slot.XPosition = origin; slot.YPosition = origin;
                typeof(RoomEnemySystem).GetMethod("InitializeMorphBallEye", flags)!
                    .CreateDelegate<Action<RoomEnemySlot>>(enemies)(slot);
                AssertEqual(unchecked((ushort)(origin + expected.Item1)), slot.XPosition, "Actual mount X with native wrap");
                AssertEqual(unchecked((ushort)(origin + expected.Item2)), slot.YPosition, "Actual mount Y with native wrap");
                AssertEqual(expected.Item3, slot.CurrentInstruction, "Actual directional mount program");
                AssertEqual(MorphBallEyeAiFunction.MountNoOp, enemies.MorphBallEyeStates[0]!.Function, "Mount remains decorative");
            }
        }
        var owner = new RoomEnemySystem();
        var body = owner.Slots[1];
        body.EnemyDefinitionPointer = RoomEnemySystem.MorphBallEyeDefinition;
        typeof(RoomEnemySystem).GetMethod("InitializeMorphBallEye", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(owner)(body);
        owner.MorphBallEyeStates[1]!.ActivatedFlag = 1;
        owner.MorphBallEyeBeam.BodySlotIndex = 1;
        owner.MorphBallEyeBeam.Phase = MorphBallEyeBeamPhase.Full;
        owner.MorphBallEyeBeam.ColorIndex = 0xfff0;
        var step = typeof(RoomEnemySystem).GetMethod("StepFullMorphBallEyeBeam", flags)!
            .CreateDelegate<Action>(owner);
        for (int phase = 0; phase < 16; phase++)
        {
            var expected = (rom.ReadByte(0x88ea8b + phase * 4), rom.ReadByte(0x88ea8c + phase * 4));
            AssertEqual(expected, MorphBallEyeGeometryDefinitions.BeamColor(phase), "Native two-channel triangle");
            step();
            AssertEqual(expected.Item1, owner.MorphBallEyeBeam.Red, "Actual raw red COLDATA");
            AssertEqual(expected.Item2, owner.MorphBallEyeBeam.Green, "Actual raw green COLDATA");
            AssertEqual(rom.ReadByte(0x88ea8d + phase * 4), owner.MorphBallEyeBeam.Blue, "Actual zero blue intensity");
            AssertEqual((ushort)((phase + 1) & 15), owner.MorphBallEyeBeam.ColorIndex, "Actual masked cycling and wrap");
        }
        AssertThrows<InvalidDataException>(() => MorphBallEyeGeometryDefinitions.Mount(4), "Unsupported mount direction");
        AssertThrows<ArgumentOutOfRangeException>(() => MorphBallEyeGeometryDefinitions.BeamColor(16), "Beam phase bound");
        Console.WriteLine("Morph Ball eye: four native mount tuples, eight actual wrapped initializers,16 native beam colors and actual full-beam cycle pass.");
    }

    private static void VerifyLookupStream5MapHighlight(SuperMetroidAddressSpace rom)
    {
        var frames = Enumerable.Range(0, 14).Select(frame => new MapPaletteCycleFrame
        {
            DurationTicks = rom.ReadByte(MapAnimationRomData.PaletteTiming + frame * 3),
            Colors = Enumerable.Range(0, 16).Select(color =>
            {
                ushort word = ReadVerificationWord(rom, MapAnimationRomData.PaletteColors + (frame * 16 + color) * 2);
                return new PaletteRgb5 { Red = word & 31, Green = (word >> 5) & 31, Blue = (word >> 10) & 31 };
            }).ToArray(),
        }).ToArray();
        var original = new MapPaletteCycleDocument { Version = 1, Frames = frames };
        MapPaletteCycle stock = Check(original);
        AssertEqual(0, ((Dictionary<int, byte>)typeof(MapPaletteCycle).GetField("durationEdits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!).Count, "No stock duration lookup retained");
        AssertEqual(0, ((Dictionary<int, ushort>)typeof(MapPaletteCycle).GetField("reverseColorEdits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!).Count, "No stock reverse-frame lookup retained");
        AssertEqual(8, ((ushort[][])typeof(MapPaletteCycle).GetField("colors", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!).Length, "Only independent color phases remain");
        for (int frame = 0; frame < 14; frame++)
        {
            AssertEqual((byte)Math.Min(frame, 14 - frame), rom.ReadByte(MapAnimationRomData.PaletteTiming + frame * 3 + 1), "Native highlight phase traversal");
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
        AssertThrows<IndexOutOfRangeException>(() => stock.Duration(-1), "Highlight lower frame bound");
        AssertThrows<IndexOutOfRangeException>(() => stock.Apply(new SnesCgram(), 14, 0), "Highlight upper frame bound");
        Console.WriteLine("Map highlight: 224 native colors,14 timing/phase records, all independent color/hold edits and custom frame counts pass actual CGRAM application; eight color seed rows remain pending.");

        static MapPaletteCycle Check(MapPaletteCycleDocument document)
        {
            byte[] json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
            var cycle = MapPaletteCycle.Load(new MemoryStream(json, writable: false));
            AssertEqual(document.Frames.Length, cycle.FrameCount, "Custom highlight frame count");
            var cgram = new SnesCgram();
            for (int frame = 0; frame < cycle.FrameCount; frame++)
            {
                AssertEqual((byte)document.Frames[frame].DurationTicks, cycle.Duration(frame), "Independent highlight hold");
                cycle.Apply(cgram, frame, MapAnimationRomData.PaletteDestination);
                for (int color = 0; color < 16; color++)
                {
                    var rgb = document.Frames[frame].Colors[color];
                    AssertEqual((ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10),
                        cgram.Colors[MapAnimationRomData.PaletteDestination + color], "Independent highlight CGRAM color");
                }
            }
            return cycle;
        }
    }

    private static void VerifyLookupStream5SidehopperGeometry(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        ushort Word(int address) => ReadVerificationWord(rom, address);
        foreach (ushort variant in new ushort[] { 0, 2, ushort.MaxValue })
        {
            bool alternate = variant != 0;
            var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            var enemies = new RoomEnemySystem { TileArtwork = LookupStream5CorpseFixtureArtwork(rom) };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, memory);
            var initialize = typeof(RoomEnemySystem).GetMethod("InitializeDeadSidehopperGraphics", flags)!
                .CreateDelegate<Action<ushort>>(enemies);
            byte[] expected = new byte[0x800];
            for (int row = 0; row < 5; row++)
            {
                int native = (alternate ? 0xa9df08 : 0xa9dec1) + 14 * row;
                int source = Word(native + 2) - 0xc000;
                int destination = Word(native + 5) - 0x2000;
                int length = Word(native + 8) + 1;
                AssertEqual((source, destination, length),
                    DeadMonsterRottingDefinitions.SidehopperInitialCopy(variant, row), "Native Sidehopper MVN geometry");
                for (int i = 0; i < length; i++) expected[destination + i] = rom.ReadByte(0xb7c000 + source + i);
            }
            initialize(variant);
            for (int i = 0; i < expected.Length; i++)
                AssertEqual(expected[i], memory.ReadByte(0x7e2000 + i), "Actual installed Sidehopper graphics copy");

            // Native LDA abs,X operands and optional CMP #8 operands, independently read from each unrolled column.
            int[] sourceOperands = alternate
                ? [0xa9e56d, 0xa9e587, 0xa9e5a6, 0xa9e5c5, 0xa9e5e4]
                : [0xa9e476, 0xa9e495, 0xa9e4af, 0xa9e4c9, 0xa9e4e3];
            int[] minimumOperands = alternate ? [0, 0, 0xa9e59c, 0xa9e5bb, 0xa9e5da]
                : [0xa9e46c, 0xa9e48b, 0, 0, 0];
            for (int column = 0; column < 5; column++)
            {
                AssertEqual((Word(sourceOperands[column]) - 0x2000) / 2,
                    DeadMonsterRottingDefinitions.SidehopperColumnWordOffset(variant, column), "Native tile column word offset");
                AssertEqual(minimumOperands[column] == 0 ? 0 : (int)Word(minimumOperands[column]),
                    DeadMonsterRottingDefinitions.SidehopperColumnMinimumY(variant, column), "Native missing top-row clipping");
            }
            var state = new DeadSidehopperEnemyState(enemies.Slots[0], variant, 0, 0, 0, 0, 0, 0xe240, 0, 0, 0, 0, 148);
            var copy = typeof(RoomEnemySystem).GetMethod("CopyOrMoveDeadSidehopperPixelRow", flags)!
                .CreateDelegate<Action<DeadSidehopperEnemyState, ushort, bool>>(enemies);
            foreach (ushort y in new ushort[] { 0, 7, 8, 37, 38 })
            foreach (bool move in new[] { false, true })
            {
                for (int i = 0; i < expected.Length; i++)
                {
                    expected[i] = (byte)(i * 37 + i / 256);
                    memory.WriteByte(0x7e2000 + i, expected[i]);
                }
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
                copy(state, y, move);
                for (int i = 0; i < expected.Length; i++)
                    AssertEqual(expected[i], memory.ReadByte(0x7e2000 + i), "Actual Sidehopper pixel copy/move including clipping, wrap and terminal row");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => DeadMonsterRottingDefinitions.SidehopperInitialCopy(0, -1), "Copy row lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadMonsterRottingDefinitions.SidehopperInitialCopy(0, 5), "Copy row upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadMonsterRottingDefinitions.SidehopperColumnWordOffset(0, 5), "Column upper bound");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadMonsterRottingDefinitions.SidehopperColumnMinimumY(0, -1), "Column lower bound");
        Console.WriteLine("Sidehopper geometry: ten native MVN rows, ten column offsets/clips, installed artwork and actual copy/move boundary rows pass; nonzero variant semantics preserved.");
    }

    private static EnemyTileArtworkCatalog LookupStream5CorpseFixtureArtwork(SuperMetroidAddressSpace rom)
    {
        byte[] planar = Enumerable.Range(0,DeadTourianCorpseArtworkDefinitions.ByteCount)
            .Select(index => rom.ReadByte(DeadTourianCorpseArtworkDefinitions.SourceAddress+index)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar,4,RoomCharacterAtlasFormat.TileColumns,out int width,out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png,width,height,pixels,SnesGraphics.DiagnosticPalette(16));
        png.Position=0;
        RoomCharacterAtlas atlas = RoomCharacterAtlas.Load(png,planar.Length);
        return EnemyTileArtworkCatalog.FromArtworkForVerification(
            new Dictionary<ushort,RoomCharacterAtlas> { [RoomEnemySystem.DeadSidehopperDefinition] = atlas },
            new Dictionary<ushort,EnemyPaletteSheet>
            {
                [RoomEnemySystem.DeadSidehopperDefinition] = EnemyPaletteSheet.Load(new MemoryStream(
                    EnemyPaletteSheet.Write(new EnemyPaletteSheetDocument
                    {
                        Version=1,
                        Colors=Enumerable.Range(0,16).Select(_ => new PaletteRgb5 { Red=0,Green=0,Blue=0 }).ToArray(),
                    }))),
            },
            dmaSources: new Dictionary<ushort,int> { [RoomEnemySystem.DeadSidehopperDefinition] = DeadTourianCorpseArtworkDefinitions.SourceAddress });
    }
    private static void VerifyLookupStream5PhantoonRain(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int i = 0; i < 8; i++)
        {
            var placement = PhantoonPatternDefinitions.RainPlacement(i);
            AssertEqual(Word(0xa7cdad + i * 8), placement.Cursor, "Native rain figure-eight cursor");
            AssertEqual(Word(0xa7cdaf + i * 8), placement.X, "Native rain body X");
            AssertEqual(Word(0xa7cdb1 + i * 8), placement.Y, "Native rain body Y");
            AssertEqual((ushort)0, Word(0xa7cdb3 + i * 8), "Native unused rain record word");
            AssertEqual(rom.ReadByte(0xa7cfc2 + i), PhantoonPatternDefinitions.FirstRainColumns[i], "Native first rain column");
        }
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.NonPublic;
        var busField = typeof(RoomEnemySystem).GetField("_bus", flags)!;
        var randomField = typeof(RoomEnemySystem).GetField("_nextRandom", flags)!;
        var rainMethod = typeof(RoomEnemySystem).GetMethod("RunPhantoonHiddenFlameRain", flags)!;
        for (int high = 0; high < 1; high++)
        for (int pattern = 0; pattern < 8; pattern++)
        {
            var enemies = new RoomEnemySystem();
            busField.SetValue(enemies, new PhantoonPatternReadGuard(rom));
            ushort random = (ushort)((high << 8) | 0xf8 | pattern);
            int calls = 0;
            randomField.SetValue(enemies, (Func<ushort>)(() => { calls++; return random; }));
            var body = enemies.Slots[0];
            var eye = enemies.Slots[1];
            var state = new PhantoonEnemyState(body) { Eye = eye };
            body.VariableE = 1;
            eye.VariableC = 123;
            rainMethod.CreateDelegate<Action<RoomEnemySlot, PhantoonEnemyState>>(enemies)(body, state);
            AssertEqual(Word(0xa7cdad + pattern * 8), body.VariableA, "Real rain cursor handoff");
            AssertEqual(Word(0xa7cdaf + pattern * 8), body.XPosition, "Real rain body placement X");
            AssertEqual(Word(0xa7cdb1 + pattern * 8), body.YPosition, "Real rain body placement Y");
            AssertEqual((ushort)0, eye.VariableC, "Real rain direction reset");
            AssertEqual((ushort)PhantoonAiFunction.BecomeSolidAfterFlameRain, body.VariableF, "Real rain phase handoff");
            AssertEqual(1, calls, "Rain consumes one RNG word");
            var flames = enemies.EnemyProjectiles.Where(p => p.IsActive).OrderBy(p => p.XVelocity).ToArray();
            AssertEqual(8, flames.Length, "Actual rain population");
            AssertTrue(flames.All(flame => flame.XPosition != body.XPosition), "Calculated rain gap lies at the body X");
            for (int i = 0; i < 8; i++)
            {
                int column = (rom.ReadByte(0xa7cfc2 + pattern) + i) % 9;
                AssertEqual((ushort)rom.ReadByte(0x8698f7 + column), flames[i].XPosition, "Actual rain column order with wrap");
                AssertEqual((ushort)40, flames[i].YPosition, "Actual rain ceiling Y");
                AssertEqual((ushort)((i + 1) * 8), flames[i].XVelocity, "Actual staggered rain delay");
            }
        }

        (short X, short Y, ushort Direction)[] eyeTargets =
        [
            (0, -100, 0), (100, -100, 1), (100, 0, 2), (100, 100, 3),
            (0, 100, 4), (-100, 100, 6), (-100, 0, 7), (-100, -100, 8),
        ];
        for (ushort direction = 0; direction < 9; direction++)
        {
            AssertEqual(Word(0xa7d40d + direction * 2),
                PhantoonPatternDefinitions.EyeInstruction(direction),
                $"native Phantoon eye direction {direction}");
        }
        var eyeEnemies = new RoomEnemySystem();
        busField.SetValue(eyeEnemies, new PhantoonPatternReadGuard(rom));
        var pointEye = typeof(RoomEnemySystem).GetMethod("PointPhantoonEyeAtSamus", flags)!
            .CreateDelegate<Action<RoomEnemySlot, RoomEnemySlot, SamusState>>(eyeEnemies);
        RoomEnemySlot eyeBody = eyeEnemies.Slots[0];
        RoomEnemySlot trackingEye = eyeEnemies.Slots[1];
        eyeBody.XPosition = 0x4000;
        eyeBody.YPosition = 0x4000;
        foreach ((short x, short y, ushort direction) in eyeTargets)
        {
            pointEye(eyeBody, trackingEye, new SamusState
            {
                XPosition = unchecked((ushort)(eyeBody.XPosition + x)),
                YPosition = unchecked((ushort)(eyeBody.YPosition + y)),
            });
            AssertEqual(PhantoonPatternDefinitions.EyeInstruction(direction),
                trackingEye.CurrentInstruction,
                $"production Phantoon eye direction {direction}");
        }
        AssertThrows<InvalidDataException>(
            () => PhantoonPatternDefinitions.EyeInstruction(9),
            "Phantoon eye direction beyond authored table");
        AssertThrows<IndexOutOfRangeException>(() => _ = PhantoonPatternDefinitions.FirstRainColumns[-1], "rain pattern lower bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = PhantoonPatternDefinitions.FirstRainColumns[8], "rain pattern upper bound");
        Console.WriteLine("Stream 5 Phantoon: eight native rain columns, eight actual rain populations/gaps and nine eye selectors/eight actual octants pass with native tables forbidden; shot markers have a separately reviewed random-bucket exception.");
    }

    private static void VerifyLookupStream5CeresDoorRamp(SuperMetroidAddressSpace rom)
    {
        var document = new CeresDoorVisualDocument
        {
            Version = 1,
            Normal = Colors(CeresDoorVisualRomData.NormalColors,15),
            Escape = Colors(CeresDoorVisualRomData.EscapeColors,15),
            Animation = Enumerable.Range(0,8).Select(row => Colors(CeresDoorVisualRomData.AnimationColors +16*row,6)).ToArray(),
            Mode7DoorFrames = Enumerable.Range(0,2).Select(frame => Enumerable.Range(0,4)
                .Select(index => (int)rom.ReadByte(CeresDoorVisualRomData.Mode7FirstFrameSource +4*frame+index)).ToArray()).ToArray(),
        };
        byte[] planar = Enumerable.Range(0,CeresDoorVisualRomData.TileByteCount)
            .Select(index => rom.ReadByte(CeresDoorVisualRomData.TileSource+index)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar,4,RoomCharacterAtlasFormat.TileColumns,out int width,out int height);
        using var png = new MemoryStream();
        IndexedPng.Write(png,width,height,pixels,SnesGraphics.DiagnosticPalette(16));
        byte[] pngBytes = png.ToArray();
        CeresDoorVisualCatalog Load(CeresDoorVisualDocument value) => CeresDoorVisualCatalog.Load(
            new MemoryStream(pngBytes),new MemoryStream(CeresDoorVisualCatalog.Write(value)));
        var stock = Load(document);
        Check(stock,document);
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        AssertEqual(6,((ushort[])typeof(CeresDoorVisualCatalog).GetField("animationSeeds",flags)!.GetValue(stock)!).Length,"Ceres animation retains six unresolved seeds");
        AssertEqual(4,((Dictionary<int,ushort>)typeof(CeresDoorVisualCatalog).GetField("animationPhaseResiduals",flags)!.GetValue(stock)!).Count,"Ceres animation retains four unresolved phase colors");
        AssertEqual(0,((Dictionary<int,ushort>)typeof(CeresDoorVisualCatalog).GetField("animationRowEdits",flags)!.GetValue(stock)!).Count,"stock reverse phases require no row storage");
        AssertEqual(9, ((ushort[])typeof(CeresDoorVisualCatalog).GetField("escapeUniqueColors", flags)!
            .GetValue(stock)!).Length, "Nine independent escape colors remain pending");
        AssertEqual(0, ((Dictionary<int, ushort>)typeof(CeresDoorVisualCatalog).GetField("escapeSharedEdits", flags)!
            .GetValue(stock)!).Count, "Stock escape colors share six normal palette slots");
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
        AssertThrows<IndexOutOfRangeException>(() => stock.LoadAnimationColors(new SnesCgram(),-1),"Ceres animation lower bound");
        AssertThrows<IndexOutOfRangeException>(() => stock.LoadAnimationColors(new SnesCgram(),8),"Ceres animation upper bound");
        Console.WriteLine("Stream 5 Ceres door ramp:48 native colors,48 independently edited cells, actual CGRAM,90 independent setup-channel edits, six shared setup slots and canonical identities pass; seed colors and four phase residuals remain pending.");

        PaletteRgb5[] Colors(int source,int count) => Enumerable.Range(0,count).Select(index =>
        {
            ushort word = ReadVerificationWord(rom,source+2*index);
            return new PaletteRgb5 { Red = word &31, Green = (word>>5)&31, Blue = (word>>10)&31 };
        }).ToArray();
        static ushort Pack(PaletteRgb5 color) => (ushort)(color.Red | color.Green<<5 | color.Blue<<10);
        void Check(CeresDoorVisualCatalog actual,CeresDoorVisualDocument expected)
        {
            var cgram = new SnesCgram();
            for(int row =0;row<8;row++)
            {
                actual.LoadAnimationColors(cgram,row);
                for(int color=0;color<6;color++) AssertEqual(Pack(expected.Animation[row][color]),cgram.Colors[CeresDoorVisualRomData.AnimationTargetColor+color],"Ceres independent animation color");
            }
            actual.LoadNormalColors(cgram,0);
            for(int color=0;color<15;color++) AssertEqual(Pack(expected.Normal[color]),cgram.Colors[color],"Ceres normal palette unchanged");
            actual.LoadEscapeColors(cgram,0);
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
    private static void VerifyLookupStream5CorpseViews()
    {
        var expected = new List<DeadMonsterVramTransferDefinition>();
        foreach (ushort table in new ushort[] { 0xe0e0, 0xe10a, 0xe134, 0xe146, 0xe158, 0xe16a, 0xe17c, 0xe18e, 0xe1b0, 0xe1d2 })
        {
            var rows = DeadMonsterRottingDefinitions.ForTransferTable(table);
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
        AssertTrue(expected.SequenceEqual(DeadMonsterRottingDefinitions.AllTransfers), "audit DMA order matches all native lists");
        AssertThrows<InvalidDataException>(() => DeadMonsterRottingDefinitions.ForTransferTable(0xe0e1), "unaligned DMA list rejected");
        AssertThrows<InvalidDataException>(() => DeadMonsterRottingDefinitions.RotationOffset(0xe227, 0), "unknown corpse row layout rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadMonsterRottingDefinitions.RotationOffset(0xe226, 104), "corpse row past end rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadMonsterRottingDefinitions.SandSource(16), "sand source past end rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => DeadMonsterRottingDefinitions.SandDestination(16), "sand destination past end rejected");
    }
    private static void VerifyLookupStream5ActorLayouts(ISnesAddressSpace rom)
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
            return (layout, index => layout[index], layout.ContentIdentity);
        }, value => (value.Id, value.X, value.Y),
            (value, x) => x ? value with { X = (value.X + 1) & 65535 } : value with { Y = (value.Y + 1) & 65535 });
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
            return (layout, index => layout[index], layout.ContentIdentity);
        }, value => (value.Id, value.X, value.Y),
            (value, x) => x ? value with { X = (value.X + 1) & 65535 } : value with { Y = (value.Y + 1) & 65535 });
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
            return (layout, index => layout[index], layout.ContentIdentity);
        }, value => (value.Id, value.X, value.Y),
            (value, x) => x ? value with { X = (value.X + 1) & 65535 } : value with { Y = (value.Y + 1) & 65535 });
        Console.WriteLine("Ceres layouts: all14 original actor placements/28 coordinate operands, no stored stock rows, all28 independent coordinate edits, complete identity preservation and bounds pass.");

        static void Check<T>(T[] original,
            Func<T[], (object Layout, Func<int, T> Read, string Identity)> load,
            Func<T, (string Id, int X, int Y)> fields, Func<T, bool, T> edit)
        {
            var stock = load(original);
            var storage = stock.Layout.GetType().GetField("placements",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            AssertTrue(storage.GetValue(stock.Layout) is null, "Ceres stock placements select semantic actor dispatch");
            Verify(stock, original);
            for (int index = 0; index < original.Length; index++)
            foreach (bool horizontal in new[] { false, true })
            {
                T[] changed = original.ToArray();
                changed[index] = edit(changed[index], horizontal);
                var installed = load(changed);
                AssertTrue(storage.GetValue(installed.Layout) is not null, "Independent Ceres coordinate edit remains supplied");
                Verify(installed, changed);
            }
            AssertThrows<IndexOutOfRangeException>(() => stock.Read(-1), "Ceres layout lower bound");
            AssertThrows<IndexOutOfRangeException>(() => stock.Read(original.Length), "Ceres layout upper bound");

            void Verify((object Layout, Func<int, T> Read, string Identity) actual, T[] expected)
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
    private static void VerifyLookupStream5Initialization(ISnesAddressSpace rom)
    {
        for (ushort variant = 0; variant < 7; variant++)
        {
            var actual = CeresDoorInitializationDefinitions.For(variant);
            AssertEqual(ReadVerificationWord(rom, 0xa6f52c + 2 * variant), actual.InstructionList, "Ceres door original instruction dispatch");
            AssertEqual(ReadVerificationWord(rom, 0xa6f72b + 2 * variant), actual.MainFunction, "Ceres door original function dispatch");
        }
        for (ushort variant = 0; variant < 6; variant++)
        {
            var actual = CeresSteamDefinitions.Initialization((CeresSteamVariant)variant);
            AssertEqual(ReadVerificationWord(rom, 0xa6eff5 + 2 * variant), actual.InstructionList, "Ceres steam original instruction dispatch");
            AssertEqual(ReadVerificationWord(rom, 0xa6f001 + 2 * variant), (ushort)actual.Function, "Ceres steam original function dispatch");
        }
        for (int index = 0; index < 9; index++)
        {
            var actual = MagdollitePhaseDefinitions.Phase(index);
            AssertEqual(ReadVerificationWord(rom, 0xa8af55 + 2 * index), actual.DistanceThreshold, "Magdollite original rise threshold");
            AssertEqual(ReadVerificationWord(rom, 0xa8af67 + 2 * index), actual.BodyInstructionList, "Magdollite original body program");
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
    private static void VerifyLookupStream5PaletteEntries(ISnesAddressSpace rom)
    {
        CheckEntries(CeresCinematicLightPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 3);
        CheckEntries(CinematicGlowPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 2);
        CheckEntries(TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 2);
        CheckEntries(TourianStatueGreyPaletteFxProgramMechanicsDefinitions.All,
            entry => (entry.DefinitionPointer, entry.ProgramStart), 4);
        int lightWords = 0, glowWords = 0, redWords = 0, greyWords = 0;
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            ushort pointer = (ushort)address;
            if (CeresCinematicLightPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort light))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), light, "Ceres light original mechanics");
                lightWords++;
            }
            if (CinematicGlowPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort glow))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), glow, "Cinematic glow original mechanics");
                glowWords++;
            }
            if (TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort red))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), red, "Tourian red-flash original mechanics");
                redWords++;
            }
            if (TourianStatueGreyPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort grey))
            {
                AssertEqual(ReadVerificationWord(rom, 0x8d0000 | pointer), grey, "Tourian statue original mechanics");
                greyWords++;
            }
        }
        AssertEqual(44, lightWords, "Ceres light mechanics coverage");
        AssertEqual(64, glowWords, "Cinematic glow mechanics coverage");
        AssertEqual(50, redWords, "Tourian shared red mechanics coverage");
        AssertEqual(31, greyWords, "Tourian statue mechanics coverage");
        Console.WriteLine("Stream 5 palette entries: all eleven entry identities, original controls and collection bounds pass.");

        void CheckEntries<T>(IReadOnlyList<T> entries, Func<T, (ushort Definition, ushort Program)> project, int expectedCount)
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
    }    private static void VerifyLookupStream5DoorQuakeDecoding(SuperMetroidAddressSpace rom)
    {
        VerifyCeresDoorQuakeDefinitions(rom);
        byte[] stockJson = SuperMetroid.AssetExtraction.EnemySpritemapFiles.Extract(rom);
        var installed = EnemySpritemapCatalog.Load(new MemoryStream(stockJson, writable: false));
        var enemies = new RoomEnemySystem
        {
            CeresStatus = 1,
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
                new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>(), spritemaps: installed),
        };
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new CeresDoorQuakeReadGuard(rom));
        typeof(RoomEnemySystem).GetField("_ridleyState", flags)!.SetValue(enemies, new RidleyEnemyState { MovementAnimationEnabled = 0 });
        enemies.Slots[0].EnemyDefinitionPointer = EnemyDefinitionPointers.CeresRidley;
        var door = enemies.Slots[1];
        door.EnemyDefinitionPointer = CeresDoorInstructionProgramDefinitions.EnemyDefinitionPointer;
        door.VariableB = 1; door.YPosition = 80;
        foreach (ushort x in new ushort[] { 0, 1, 3, 255, 256, 511, ushort.MaxValue })
        foreach (ushort camera in new ushort[] { 0, ushort.MaxValue })
        for (ushort phase = 0; phase < 4; phase++)
        {
            door.XPosition = x; enemies.EarthquakeTimer = phase;
            var actual = new OamBuffer(); actual.BeginFrame();
            enemies.DrawCeresRidleyImmediateBabyAndDoor(actual, camera, 0); actual.FinalizeFrame();
            ushort native = (ushort)(rom.ReadByte(0xa6a321 + phase) | rom.ReadByte(0xa6a322 + phase) << 8);
            AssertEqual(native & 0x1ff, CeresDoorQuakeDefinitions.XOffset(phase) & 0x1ff,
                "Native overlapping high byte retains the same ninth coordinate bit");
            var expected = new OamBuffer(); expected.BeginFrame();
            DrawImportedEnemySpritemap(rom, expected, CeresDoorInstructionProgramDefinitions.Bank,
                CeresDoorInstructionProgramDefinitions.RidleyPrivateOverlaySpritemap,
                unchecked((ushort)(x - camera + native)), 80, EnemyPaletteBits.Palette2, 0);
            expected.FinalizeFrame();
            AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) && expected.HighTable.SequenceEqual(actual.HighTable),
                "Byte-decoded quake preserves actual low/high OAM through coordinate wrap");
        }
        Console.WriteLine("Ceres quake byte decoding:56 actual wrapped low/high OAM frames match full native overlapping-word reads.");
    }
    private static void VerifyLookupStream5CeresFlightPalette(SuperMetroidAddressSpace rom)
    {
        var colors = new PaletteRgb5[SnesCgram.ColorCount];
        for (int index = 0; index < colors.Length; index++)
        {
            ushort value = ReadVerificationWord(rom, CeresFlightRomData.Assets.Palette + index * sizeof(ushort));
            colors[index] = new PaletteRgb5 { Red = value & 31, Green = value >> 5 & 31, Blue = value >> 10 & 31 };
        }
        CeresFlightPalette stock = Check();
        var edits = (System.Collections.IDictionary)typeof(CeresFlightPalette).GetField("edits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertEqual(0, edits.Count, "Native shared sections and endpoint ramp need no residual corrections");
        int independent = ((System.Collections.IDictionary)typeof(CeresFlightPalette).GetField("colors", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(stock)!).Count;
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
        AssertThrows<ArgumentException>(() => stock.CopyTransferTo(new byte[SnesCgram.ByteCount - 1]), "Ceres palette output extent");
        IReadOnlyDictionary<string, byte[]> files = SuperMetroid.AssetExtraction.CeresFlightArtworkExtractor.Extract(rom);
        var catalog = CeresFlightArtworkCatalog.Load(Open(CeresFlightArtworkFormat.Mode7FileName),
            Open(CeresFlightArtworkFormat.MapFileName), Open(CeresFlightArtworkFormat.ObjectFileName),
            Open(CeresFlightPaletteFormat.FileName), Open(CeresFlightSpriteFormat.FileName), Open(CeresFlightActorLayoutFormat.FileName));
        byte[] nativeBytes = new byte[SnesCgram.ByteCount];
        for (int index = 0; index < nativeBytes.Length; index++) nativeBytes[index] = rom.ReadByte(CeresFlightRomData.Assets.Palette + index);
        string expectedHash = SelectedPresentationHash.Create(nameof(CeresFlightArtworkCatalog), content =>
        {
            content.Append("mode7-characters", catalog.Mode7Characters.Span);
            content.Append("mode7-maps", catalog.Mode7Maps.Span);
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
            using var json = new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions));
            CeresFlightPalette actual = CeresFlightPalette.Load(json);
            var cgram = new SnesCgram(); actual.LoadTo(cgram);
            Span<byte> bytes = stackalloc byte[SnesCgram.ByteCount]; actual.CopyTransferTo(bytes);
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
    private static void VerifyLookupStream5CeresNormalPaint(SuperMetroidAddressSpace rom)
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
            Mode7DoorFrames = Enumerable.Range(0, 2).Select(frame => Enumerable.Range(0, 4).Select(i => (int)rom.ReadByte(CeresDoorVisualRomData.Mode7FirstFrameSource + frame * 4 + i)).ToArray()).ToArray(),
        };
        byte[] planar = Enumerable.Range(0, CeresDoorVisualRomData.TileByteCount).Select(i => rom.ReadByte(CeresDoorVisualRomData.TileSource + i)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, RoomCharacterAtlasFormat.TileColumns, out int width, out int height);
        using var png = new MemoryStream(); IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        CeresDoorNormalPaintDefinitions stock = Check();
        AssertEqual(13, ((Dictionary<int, int>)typeof(CeresDoorNormalPaintDefinitions).GetField("paint", flags)!.GetValue(stock)!).Count, "Thirteen local material seeds remain after shared warm extraction");
        string[] warmSeeds = ["highlightBlue", "amberRed", "amberGreen", "red", "goldRed", "goldGreen", "goldBlue"];
        AssertTrue(warmSeeds.Order(StringComparer.Ordinal).SequenceEqual(typeof(CeresDoorWarmTargetPaintDefinitions).GetFields(flags)
            .Where(field => field.FieldType == typeof(int)).Select(field => field.Name).Order(StringComparer.Ordinal)), "Seven exact shared warm material inputs");
        AssertEqual(0, ((Dictionary<int, ushort>)typeof(CeresDoorWarmTargetPaintDefinitions).GetField("edits", flags)!.GetValue(stock.WarmTargets)!).Count, "Shared native warm shades need no overrides");
        AssertEqual(0, ((Dictionary<int, ushort>)typeof(CeresDoorNormalPaintDefinitions).GetField("edits", flags)!.GetValue(stock)!).Count, "Native calculated channels need no unexplained overrides");
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
        AssertThrows<IndexOutOfRangeException>(() => stock.ColorAt(-1), "Normal lower bound");
        AssertThrows<IndexOutOfRangeException>(() => stock.ColorAt(15), "Normal upper bound");
        Console.WriteLine("Ceres normal: fifteen native colors, thirteen local plus seven shared seeds/zero overrides,45 independent channel edits,92 actual initializer palette copies, hash and bounds pass.");

        CeresDoorNormalPaintDefinitions Check()
        {
            ushort Pack(PaletteRgb5 c) => (ushort)(c.Red | c.Green << 5 | c.Blue << 10);
            var basis = new CeresDoorNormalPaintDefinitions(document.Normal.Select(Pack).ToArray());
            var visual = CeresDoorVisualCatalog.Load(new MemoryStream(png.ToArray()), new MemoryStream(CeresDoorVisualCatalog.Write(document)));
            for (int color = 0; color < 15; color++) AssertEqual(Pack(document.Normal[color]), basis.ColorAt(color), "Every supplied normal channel remains independent");
            foreach (ushort variant in new ushort[] { 0, 3 })
            {
                var system = new RoomEnemySystem { TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>(), ceresDoorVisual: visual) };
                var cgram = new SnesCgram();
                typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(system, cgram);
                var slot = system.Slots[0]; slot.Parameter1 = variant;
                typeof(RoomEnemySystem).GetMethod("InitializeCeresDoor", flags)!.CreateDelegate<Action<RoomEnemySlot>>(system)(slot);
                int destination = variant == 3 ? CeresDoorVisualRomData.NormalTargetColor : CeresDoorVisualRomData.ActiveTargetColor;
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
    private static void VerifyLookupStream5CeresEscapePaint(SuperMetroidAddressSpace rom)
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
            Mode7DoorFrames = Enumerable.Range(0, 2).Select(frame => Enumerable.Range(0, 4).Select(i => (int)rom.ReadByte(CeresDoorVisualRomData.Mode7FirstFrameSource + frame * 4 + i)).ToArray()).ToArray(),
        };
        byte[] planar = Enumerable.Range(0, CeresDoorVisualRomData.TileByteCount).Select(i => rom.ReadByte(CeresDoorVisualRomData.TileSource + i)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, RoomCharacterAtlasFormat.TileColumns, out int width, out int height);
        using var png = new MemoryStream(); IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        CeresDoorEscapePaintDefinitions stock = Check();
        AssertEqual(4, ((int[])typeof(CeresDoorEscapePaintDefinitions).GetField("highlightAndEdgeLevels", flags)!.GetValue(stock)!).Length, "Four selected highlight/edge/outline levels");
        AssertEqual(0, ((Dictionary<int, ushort>)typeof(CeresDoorEscapePaintDefinitions).GetField("edits", flags)!.GetValue(stock)!).Count, "Native calculated channels need no unexplained overrides");
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
        AssertThrows<IndexOutOfRangeException>(() => stock.ColorAt(-1), "Escape lower bound");
        AssertThrows<IndexOutOfRangeException>(() => stock.ColorAt(15), "Escape upper bound");
        Console.WriteLine("Ceres escape: fifteen native colors, eight paint seeds/zero overrides,45 independent channel edits,92 actual escape initializer palette copies, hash and bounds pass.");

        CeresDoorEscapePaintDefinitions Check()
        {
            ushort Pack(PaletteRgb5 c) => (ushort)(c.Red | c.Green << 5 | c.Blue << 10);
            var basis = new CeresDoorEscapePaintDefinitions(document.Escape.Select(Pack).ToArray(), new CeresDoorNormalPaintDefinitions(document.Normal.Select(Pack).ToArray()));
            var visual = CeresDoorVisualCatalog.Load(new MemoryStream(png.ToArray()), new MemoryStream(CeresDoorVisualCatalog.Write(document)));
            for (int color = 0; color < 15; color++) AssertEqual(Pack(document.Escape[color]), basis.ColorAt(color), "Every supplied escape channel remains independent");
            foreach (ushort variant in new ushort[] { 0, 3 })
            {
                var system = new RoomEnemySystem { CeresStatus = 2, TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>(), ceresDoorVisual: visual) };
                var cgram = new SnesCgram();
                typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(system, cgram);
                var slot = system.Slots[0]; slot.Parameter1 = variant;
                typeof(RoomEnemySystem).GetMethod("InitializeCeresDoor", flags)!.CreateDelegate<Action<RoomEnemySlot>>(system)(slot);
                int destination = CeresDoorVisualRomData.ActiveTargetColor;
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
    private static void VerifyLookupStream5CeresBeaconPaint(SuperMetroidAddressSpace rom)
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
            Mode7DoorFrames = Enumerable.Range(0, 2).Select(frame => Enumerable.Range(0, 4).Select(i => (int)rom.ReadByte(CeresDoorVisualRomData.Mode7FirstFrameSource + frame * 4 + i)).ToArray()).ToArray(),
        };
        byte[] planar = Enumerable.Range(0, CeresDoorVisualRomData.TileByteCount).Select(i => rom.ReadByte(CeresDoorVisualRomData.TileSource + i)).ToArray();
        byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, RoomCharacterAtlasFormat.TileColumns, out int width, out int height);
        using var png = new MemoryStream(); IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        CeresDoorAnimationPaintDefinitions stock = Check(-1);
        AssertEqual(17, (int)typeof(CeresDoorAnimationPaintDefinitions).GetField("firstSeedBlue", flags)!.GetValue(stock)!, "Selected beacon blue");
        AssertEqual(14, (int)typeof(CeresDoorAnimationPaintDefinitions).GetField("dimMiddleAmberRed", flags)!.GetValue(stock)!, "Selected dim amber red");
        foreach (string field in new[] { "seedEdits", "edits" })
            AssertEqual(0, ((Dictionary<int, ushort>)typeof(CeresDoorAnimationPaintDefinitions).GetField(field, flags)!.GetValue(stock)!).Count, "All native beacon dependencies calculate without overrides");
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
        AssertThrows<IndexOutOfRangeException>(() => stock.ColorAt(-1, 0), "Beacon row lower bound");
        AssertThrows<IndexOutOfRangeException>(() => stock.ColorAt(8, 0), "Beacon row upper bound");
        AssertThrows<IndexOutOfRangeException>(() => stock.ColorAt(0, -1), "Beacon color lower bound");
        AssertThrows<IndexOutOfRangeException>(() => stock.ColorAt(0, 6), "Beacon color upper bound");
        Console.WriteLine("Ceres beacon:48native colors/zero stock overrides,144independent RGB edits,208actual row selections including64-tick cycle, untouched neighbors/hash/bounds pass.");

        CeresDoorAnimationPaintDefinitions Check(int editedRow)
        {
            ushort Pack(PaletteRgb5 c) => (ushort)(c.Red | c.Green << 5 | c.Blue << 10);
            var basis = new CeresDoorAnimationPaintDefinitions(document.Animation.Select(row => row.Select(Pack).ToArray()).ToArray(), new CeresDoorNormalPaintDefinitions(document.Normal.Select(Pack).ToArray()));
            var visual = CeresDoorVisualCatalog.Load(new MemoryStream(png.ToArray()), new MemoryStream(CeresDoorVisualCatalog.Write(document)));
            for (int row = 0; row < 8; row++) for (int color = 0; color < 6; color++)
                AssertEqual(Pack(document.Animation[row][color]), basis.ColorAt(row, color), "Every supplied beacon channel remains independent");
            var system = new RoomEnemySystem { TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>(), ceresDoorVisual: visual) };
            var cgram = new SnesCgram();
            typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(system, cgram);
            typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(system, new SnesVram());
            var tick = typeof(RoomEnemySystem).GetMethod("RunCeresDoorPaletteAnimation", flags)!.CreateDelegate<Action>(system);
            foreach (int counter in editedRow < 0 ? Enumerable.Range(0, 64) : new[] { editedRow * 8 })
            {
                system.Slots[0].FrameCounter = (ushort)counter; tick();
                int row = (counter & 0x38) >> 3;
                for (int color = 0; color < 6; color++) AssertEqual(Pack(document.Animation[row][color]), cgram.Colors[41 + color], "Actual native eight-tick row selection and six CGRAM writes");
                AssertEqual((ushort)0, cgram.Colors[40], "Beacon lower neighbor unchanged");
                AssertEqual((ushort)0, cgram.Colors[47], "Beacon upper neighbor unchanged");
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
}
