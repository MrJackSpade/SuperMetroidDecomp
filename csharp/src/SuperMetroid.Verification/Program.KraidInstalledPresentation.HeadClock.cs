using System.Buffers.Binary;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static EnemyAnimationIsolationFixtureVerification;
using static EnemyTileArtworkKraidVerification;

/// <summary>Checks that replacement Kraid head artwork retains native instruction timing and mouth behavior.</summary>
internal static class KraidInstalledPresentationHeadClockVerification
{
    /// <summary>Replacing every head image must not replace the compiled clock, callback or mouth window.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyKraidInstalledHeadIsolation(string directory, EnemyTileArtworkCatalog stock)
    {
        string overrides = Path.Combine(directory, "kraid-head-clock-overrides");
        Directory.CreateDirectory(overrides);
        ushort[] pointers = KraidHeadInstructionDefinitions.All
            .Where(command => command.Kind == KraidHeadInstructionKind.Frame)
            .Select(command => command.Tilemap).Distinct().ToArray();
        for (int index = 0; index < pointers.Length; index++)
        {
            ReadOnlySpan<ushort> replacement = (stock.KraidBackground ?? throw new InvalidOperationException("Verification requires stock.KraidBackground.")).HeadWords(pointers[(index + 1) % pointers.Length]);
            var words = new byte[replacement.Length * sizeof(ushort)];
            for (int word = 0; word < replacement.Length; word++)
                BinaryPrimitives.WriteUInt16LittleEndian(words.AsSpan(word * 2),
                    (ushort)(replacement[word] ^ KraidBackgroundRomData.PriorityBit));
            File.WriteAllBytes(Path.Combine(overrides, KraidBackgroundArtworkFormat.HeadFileName(pointers[index])),
                KraidHeadTilemapAtlas.Encode(words));
        }
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrides);
        AssertTrue(stock.ContentIdentity != edited.ContentIdentity, "head replacement contributes to selected identity");
        AssertEqual(edited.ContentIdentity, EnemyTileArtworkFiles.Load(directory, overrides).ContentIdentity,
            "all four head replacements survive catalog reload");
        int frames = 0, ticks = 0, callbacks = 0, collisions = 0;
        var shot = new SamusProjectileSystem().Slots[0];
        shot.Radius = new(2, 2);
        var commands = new List<KraidHeadInstructionDefinition>();
        foreach (KraidHeadInstructionDefinition command in KraidHeadInstructionDefinitions.All)
        {
            commands.Add(command);
            if (command.Kind != KraidHeadInstructionKind.Terminate) continue;
            var baseline = new KraidHeadClockFixture(stock);
            var changed = new KraidHeadClockFixture(edited);
            baseline.Body.VariableB = changed.Body.VariableB = commands[0].Pointer;
            baseline.Body.VariableC = changed.Body.VariableC = 1;
            int nextEvent = 0, cursor = 0, soundCount = 0, uploadCount = 0;
            ushort outer = 0, inner = 0, physical = 0;
            var expectedStock = baseline.Vram.Bytes.ToArray();
            var expectedEdited = changed.Vram.Bytes.ToArray();
            ushort[] workingBody = baseline.State.BackgroundTilemapWords.ToArray();
            for (int tick = 0; ; tick++)
            {
                ushort stockResult = baseline.Step(), editedResult = changed.Step();
                ticks++;
                AssertEqual(stockResult, editedResult, "head interpretation result is not editable");
                AssertAnimationValues(baseline.Body, changed.Body, "Kraid head tick " + tick);
                AssertAnimationValues(baseline.State, changed.State, "Kraid head tick " + tick);
                AssertTrue(workingBody.SequenceEqual(baseline.State.BackgroundTilemapWords) &&
                    workingBody.SequenceEqual(changed.State.BackgroundTilemapWords),
                    "head DMA must not overwrite the WRAM body-map source");
                AssertEqual(baseline.Enemies.KraidBehavior.LastKraidSoundEffect, changed.Enemies.KraidBehavior.LastKraidSoundEffect,
                    "head art does not alter sound publication");
                if (tick == nextEvent)
                {
                    while (commands[cursor].Kind is KraidHeadInstructionKind.RoarSound or KraidHeadInstructionKind.DyingSound)
                    {
                        KraidHeadInstructionDefinition sound = commands[cursor++];
                        AssertEqual((byte)sound.SoundId, (baseline.Enemies.KraidBehavior.LastKraidSoundEffect ?? throw new InvalidOperationException("Verification requires baseline.Enemies.KraidBehavior.LastKraidSoundEffect.")).SoundEffect.Value,
                            "sound callback fires at the exact head admission tick");
                        if (sound.Kind == KraidHeadInstructionKind.RoarSound) soundCount++;
                        callbacks++;
                    }
                    KraidHeadInstructionDefinition admission = commands[cursor++];
                    if (admission.Kind == KraidHeadInstructionKind.Terminate)
                    {
                        AssertEqual(ushort.MaxValue, stockResult, "terminator on the exact sum-of-durations tick");
                        AssertEqual(admission.Pointer, baseline.Body.VariableB, "termination retains native cursor");
                        AssertEqual((ushort)0, baseline.Body.VariableC, "termination consumes the last timer tick");
                        AssertTrue(expectedStock.SequenceEqual(baseline.Vram.Bytes) &&
                            expectedEdited.SequenceEqual(changed.Vram.Bytes), "termination emits no tilemap upload");
                        break;
                    }
                    AssertEqual((ushort)1, stockResult, "frame admission result");
                    nextEvent += admission.Duration;
                    physical = admission.Tilemap; outer = admission.VulnerableHitbox; inner = admission.InvulnerableHitbox;
                    uploadCount++; frames++;
                    KraidHeadClockFixture.ExpectedUpload(expectedStock, (stock.KraidBackground ?? throw new InvalidOperationException("Verification requires stock.KraidBackground.")).HeadWords(physical));
                    KraidHeadClockFixture.ExpectedUpload(expectedEdited, (edited.KraidBackground ?? throw new InvalidOperationException("Verification requires edited.KraidBackground.")).HeadWords(physical));
                    AssertTrue(!baseline.Vram.ImageEquals(changed.Vram), "replacement head is actually visible");
                }
                AssertEqual((ushort)(nextEvent - tick), baseline.Body.VariableC, "native countdown on every hold tick");
                KraidHeadInstructionDefinition displayed = commands.Take(cursor).Last(item => item.Kind == KraidHeadInstructionKind.Frame);
                AssertEqual((ushort)(displayed.Pointer + 8), baseline.Body.VariableB, "head cursor holds until next event");
                AssertEqual(physical, baseline.State.CurrentHeadTilemap, "compiled physical head selector survives art replacement");
                AssertEqual(outer, baseline.State.VulnerableMouthHitbox, "outer mouth window follows compiled admission");
                AssertEqual(inner, baseline.State.InvulnerableMouthHitbox, "inner mouth window follows compiled admission");
                AssertEqual(soundCount, baseline.State.RoarRequestCount, "roar callback count on every tick");
                AssertEqual(uploadCount, baseline.State.HeadTilemapUploadCount, "no extra head upload during hold");
                AssertTrue(expectedStock.SequenceEqual(baseline.Vram.Bytes), "exact stock head DMA and untouched VRAM");
                AssertTrue(expectedEdited.SequenceEqual(changed.Vram.Bytes), "exact authored head DMA and untouched VRAM");
                // Actual mouth collision method, including the strict bottom-edge -1,
                // not a radius-only approximation or equality of endpoint state.
                foreach (ushort hitbox in new[] { outer, inner }.Where(value => value != ushort.MaxValue))
                for (int x = -8; x <= 48; x += 8)
                for (int y = -144; y <= -64; y += 8)
                {
                    shot.Position = shot.Position.WithWhole(new((ushort)(baseline.Body.Position.X.Whole + x), (ushort)(baseline.Body.Position.Y.Whole + y)));
                    (short left, short top, _, short bottom) = KraidMouthHitboxDefinitions.Resolve(hitbox);
                    bool expected = y - shot.Radius.Y - 1 < bottom && y + shot.Radius.Y >= top && x + shot.Radius.X >= left;
                    AssertEqual(expected, baseline.Hit(hitbox, shot), "stock native mouth geometry");
                    AssertEqual(expected, changed.Hit(hitbox, shot), "authored head does not move physical mouth geometry");
                    collisions++;
                }
            }
            commands.Clear();
        }
        AssertEqual(0, commands.Count, "all four native head programs terminate");
        Console.WriteLine($"PASS Kraid head isolation: {ticks} paired production ticks, {frames} exact admissions, " +
            $"{callbacks} timed sound callbacks, {collisions} mouth samples; four installed replacements retain native control/VRAM cadence.");
    }

    /// <summary>Owns a production Kraid enemy system and linked head slots for clock comparisons.</summary>
    internal sealed class KraidHeadClockFixture
    {
        /// <summary>Production enemy system executing head instructions.</summary>
        internal RoomEnemySystem Enemies { get; }
        /// <summary>Physical Kraid body slot whose variable words hold the head cursor and timer.</summary>
        internal RoomEnemySlot Body => Enemies.SlotTable.Slots[0];
        /// <summary>Kraid state holding selected head art and mouth hitbox identities.</summary>
        internal KraidEnemyState State { get; }
        /// <summary>Sentinel-filled VRAM receiving head tilemap uploads.</summary>
        internal SnesVram Vram { get; } = new();
        /// <summary>Cartridge-free address space used for the production head clock.</summary>
        internal SuperMetroid.Core.Hardware.SuperMetroidAddressSpace Memory { get; } =
            SuperMetroid.Core.Hardware.SuperMetroidAddressSpace.CreateWithoutCartridge();
        /// <summary>Bound production head-instruction dispatcher.</summary>
        private readonly Func<RoomEnemySlot, KraidEnemyState, ushort> step;
        /// <summary>Bound production mouth-versus-projectile overlap check.</summary>
        private readonly Func<RoomEnemySlot, ushort, SamusProjectileSlot, bool> hit;
        /// <summary>Bound production head tilemap transfer.</summary>
        private readonly Action<KraidEnemyState, ushort> transfer;

        /// <summary>Initializes Kraid with selected art and a sentinel VRAM image.</summary>
        [PpuLayoutBoundary("exact hardware test: fills VRAM with a sentinel image before the head transfer")]
        internal KraidHeadClockFixture(EnemyTileArtworkCatalog art)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Enemies = new RoomEnemySystem { TileArtwork = art };
            State = BuildKraidWorkingMap(art);
            Enemies.BindFixtureBus(Memory);
            Enemies.Presentation._vram = Vram;
            step = new Func<RoomEnemySlot, KraidEnemyState, ushort>(Enemies.KraidBehavior.ProcessKraidHeadInstruction);
            hit = Enemies.KraidMouthHitboxOverlapsShot;
            transfer = new Action<KraidEnemyState, ushort>(Enemies.KraidBehavior.TransferKraidHeadTilemap);
            Body.Position = Body.Position.WithWhole(new(512, 512));
            byte[] sentinel = new byte[SnesVram.ByteCount]; Array.Fill(sentinel, (byte)0xcd); Vram.LoadBytes(0, sentinel);
        }

        /// <summary>Advances one head instruction tick using the production interpreter.</summary>
        internal ushort Step() => step(Body, State);
        /// <summary>Uploads a selected head tilemap through production behavior.</summary>
        internal void Transfer(ushort pointer) => transfer(State, pointer);
        /// <summary>Tests a projectile against the selected physical mouth hitbox.</summary>
        internal bool Hit(ushort pointer, SamusProjectileSlot shot) => hit(Body, pointer, shot);
        /// <summary>Updates an expected VRAM image with one head tilemap DMA payload.</summary>
        internal static void ExpectedUpload(byte[] destination, ReadOnlySpan<ushort> words)
        {
            for (int word = 0; word < words.Length; word++)
                BinaryPrimitives.WriteUInt16LittleEndian(destination.AsSpan(KraidBackgroundRomData.LiveBg2TilemapWord * 2 + word * 2), words[word]);
        }
    }
}
