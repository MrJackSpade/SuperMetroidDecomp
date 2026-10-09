using System.Buffers.Binary;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Replacing every head image must not replace the compiled clock, callback or mouth window.</summary>
    private static void VerifyKraidInstalledHeadIsolation(string directory, EnemyTileArtworkCatalog stock)
    {
        string overrides = Path.Combine(directory, "kraid-head-clock-overrides");
        Directory.CreateDirectory(overrides);
        ushort[] pointers = KraidHeadInstructionDefinitions.All.ToArray()
            .Where(command => command.Kind == KraidHeadInstructionKind.Frame)
            .Select(command => command.Tilemap).Distinct().ToArray();
        for (int index = 0; index < pointers.Length; index++)
        {
            ReadOnlySpan<ushort> replacement = stock.KraidBackground!.HeadWords(pointers[(index + 1) % pointers.Length]);
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
        shot.XRadius = 2; shot.YRadius = 2;
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
                AssertTrue(workingBody.AsSpan().SequenceEqual(baseline.State.BackgroundTilemapWords) &&
                    workingBody.AsSpan().SequenceEqual(changed.State.BackgroundTilemapWords),
                    "head DMA must not overwrite the WRAM body-map source");
                AssertEqual(baseline.Enemies.LastKraidSoundEffect, changed.Enemies.LastKraidSoundEffect,
                    "head art does not alter sound publication");
                if (tick == nextEvent)
                {
                    while (commands[cursor].Kind is KraidHeadInstructionKind.RoarSound or KraidHeadInstructionKind.DyingSound)
                    {
                        KraidHeadInstructionDefinition sound = commands[cursor++];
                        AssertEqual((byte)sound.SoundId, baseline.Enemies.LastKraidSoundEffect!.Value.SoundEffect.Value,
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
                        AssertTrue(expectedStock.AsSpan().SequenceEqual(baseline.Vram.Bytes) &&
                            expectedEdited.AsSpan().SequenceEqual(changed.Vram.Bytes), "termination emits no tilemap upload");
                        break;
                    }
                    AssertEqual((ushort)1, stockResult, "frame admission result");
                    nextEvent += admission.Duration;
                    physical = admission.Tilemap; outer = admission.VulnerableHitbox; inner = admission.InvulnerableHitbox;
                    uploadCount++; frames++;
                    KraidHeadClockFixture.ExpectedUpload(expectedStock, stock.KraidBackground!.HeadWords(physical));
                    KraidHeadClockFixture.ExpectedUpload(expectedEdited, edited.KraidBackground!.HeadWords(physical));
                    AssertTrue(!baseline.Vram.Bytes.SequenceEqual(changed.Vram.Bytes), "replacement head is actually visible");
                }
                AssertEqual((ushort)(nextEvent - tick), baseline.Body.VariableC, "native countdown on every hold tick");
                KraidHeadInstructionDefinition displayed = commands.Take(cursor).Last(item => item.Kind == KraidHeadInstructionKind.Frame);
                AssertEqual((ushort)(displayed.Pointer + 8), baseline.Body.VariableB, "head cursor holds until next event");
                AssertEqual(physical, baseline.State.CurrentHeadTilemap, "compiled physical head selector survives art replacement");
                AssertEqual(outer, baseline.State.VulnerableMouthHitbox, "outer mouth window follows compiled admission");
                AssertEqual(inner, baseline.State.InvulnerableMouthHitbox, "inner mouth window follows compiled admission");
                AssertEqual(soundCount, baseline.State.RoarRequestCount, "roar callback count on every tick");
                AssertEqual(uploadCount, baseline.State.HeadTilemapUploadCount, "no extra head upload during hold");
                AssertTrue(expectedStock.AsSpan().SequenceEqual(baseline.Vram.Bytes), "exact stock head DMA and untouched VRAM");
                AssertTrue(expectedEdited.AsSpan().SequenceEqual(changed.Vram.Bytes), "exact authored head DMA and untouched VRAM");
                // Actual mouth collision method, including the strict bottom-edge -1,
                // not a radius-only approximation or equality of endpoint state.
                foreach (ushort hitbox in new[] { outer, inner }.Where(value => value != ushort.MaxValue))
                for (int x = -8; x <= 48; x += 8)
                for (int y = -144; y <= -64; y += 8)
                {
                    shot.XPosition = (ushort)(baseline.Body.XPosition + x); shot.YPosition = (ushort)(baseline.Body.YPosition + y);
                    (short left, short top, _, short bottom) = KraidMouthHitboxes.Resolve(hitbox);
                    bool expected = y - shot.YRadius - 1 < bottom && y + shot.YRadius >= top && x + shot.XRadius >= left;
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

    /// <summary>
    /// Provides a cartridge-free room-enemy instance and reflected entry points for checking Kraid's
    /// compiled head animation against installed artwork.
    /// </summary>
    private sealed class KraidHeadClockFixture
    {
        /// <summary>Room-enemy system configured with the artwork catalog under verification.</summary>
        internal RoomEnemySystem Enemies { get; }
        /// <summary>The first enemy slot, which the head animation routines use for Kraid's body state.</summary>
        internal RoomEnemySlot Body => Enemies.Slots[0];
        /// <summary>Mutable Kraid working state, including the body map and head-animation outputs.</summary>
        internal KraidEnemyState State { get; }
        /// <summary>VRAM image mutated by the production head-tilemap transfer routine.</summary>
        internal SnesVram Vram { get; } = new();
        /// <summary>Cartridge-free address space backing the fixture's room-enemy system.</summary>
        internal SuperMetroid.Core.Hardware.SuperMetroidAddressSpace Memory { get; } =
            SuperMetroid.Core.Hardware.SuperMetroidAddressSpace.CreateWithoutCartridge();
        /// <summary>Bound production delegate for advancing one Kraid head instruction.</summary>
        private readonly Func<RoomEnemySlot, KraidEnemyState, ushort> step;
        /// <summary>Production mouth-hitbox query used to check collisions against the selected head frame.</summary>
        private readonly Func<RoomEnemySlot, ushort, SamusProjectileSlot, bool> hit;
        /// <summary>Bound production delegate that transfers a selected head tilemap into VRAM.</summary>
        private readonly Action<KraidEnemyState, ushort> transfer;

        /// <summary>
        /// Builds Kraid's working map and binds the production animation, transfer, and collision routines
        /// to a room-enemy system using the supplied artwork.
        /// </summary>
        /// <param name="art">Installed artwork catalog used to construct the fixture's Kraid state.</param>
        internal KraidHeadClockFixture(EnemyTileArtworkCatalog art)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Enemies = new RoomEnemySystem { TileArtwork = art };
            State = BuildKraidWorkingMap(art);
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(Enemies, Memory);
            typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(Enemies, Vram);
            step = typeof(RoomEnemySystem).GetMethod("ProcessKraidHeadInstruction", flags)!
                .CreateDelegate<Func<RoomEnemySlot, KraidEnemyState, ushort>>(Enemies);
            hit = Enemies.KraidMouthHitboxOverlapsShot;
            transfer = typeof(RoomEnemySystem).GetMethod("TransferKraidHeadTilemap", flags)!
                .CreateDelegate<Action<KraidEnemyState, ushort>>(Enemies);
            Body.XPosition = 512; Body.YPosition = 512;
            byte[] sentinel = new byte[SnesVram.ByteCount]; Array.Fill(sentinel, (byte)0xcd); Vram.LoadBytes(0, sentinel);
        }

        /// <summary>Advances the head instruction processor once for the fixture's body and state.</summary>
        internal ushort Step() => step(Body, State);
        /// <summary>Transfers the requested compiled head tilemap through the production routine.</summary>
        /// <param name="pointer">Native tilemap pointer selected by the active head instruction.</param>
        internal void Transfer(ushort pointer) => transfer(State, pointer);
        /// <summary>Tests a projectile against the production geometry for the selected mouth hitbox.</summary>
        /// <param name="pointer">Identifier of the vulnerable or invulnerable mouth hitbox.</param>
        /// <param name="shot">Projectile slot whose position and radii define the tested bounds.</param>
        internal bool Hit(ushort pointer, SamusProjectileSlot shot) => hit(Body, pointer, shot);
        /// <summary>Applies the expected little-endian tilemap words at the BG2 tilemap VRAM destination.</summary>
        /// <param name="destination">Full VRAM byte image to update in place.</param>
        /// <param name="words">Tilemap words expected from one head-frame upload.</param>
        internal static void ExpectedUpload(byte[] destination, ReadOnlySpan<ushort> words)
        {
            for (int word = 0; word < words.Length; word++)
                BinaryPrimitives.WriteUInt16LittleEndian(destination.AsSpan(KraidBackgroundRomData.LiveBg2TilemapWord * 2 + word * 2), words[word]);
        }
    }
}
