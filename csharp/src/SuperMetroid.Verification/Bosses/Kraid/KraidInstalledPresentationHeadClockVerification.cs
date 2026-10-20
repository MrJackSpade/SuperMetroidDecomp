using System.Buffers.Binary;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyAnimationIsolationFixtureVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks that replacement Kraid head artwork retains native instruction timing and mouth behavior.</summary>
internal static class KraidInstalledPresentationHeadClockVerification
{
    /// <summary>Replacing every head image must not replace the compiled clock, callback or mouth window.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyKraidInstalledHeadIsolation(CartridgeImportAddressSpace source, string directory, EnemyTileArtworkCatalog stock)
    {
        var native = new KraidHeadNativeCursors(CartridgeImportSource.Require(source));
        string overrides = Path.Combine(directory, "kraid-head-clock-overrides");
        Directory.CreateDirectory(overrides);
        ushort[] pointers = [.. KraidHeadFrameDefinitions.PresentationKeys];
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
        var shot = new SamusProjectileSystem(new BeamChargeState(), SamusProjectileResidue.None).Slots[0];
        shot.Radius = new(2, 2);
        int programsRun = 0;
        foreach (KraidHeadProgram program in Enum.GetValues<KraidHeadProgram>())
        {
            List<KraidHeadNativeRecord> commands = [.. native.Records.Where(record => record.Cursor.Program == program)];
            var baseline = new KraidHeadClockFixture(stock);
            var changed = new KraidHeadClockFixture(edited);
            baseline.State.HeadProgram = changed.State.HeadProgram = commands[0].Cursor;
            baseline.State.Parts[0].HeadTimer = changed.State.Parts[0].HeadTimer = 1;
            int nextEvent = 0, cursor = 0, soundCount = 0, uploadCount = 0;
            ushort outer = 0, inner = 0, physical = 0;
            ushort[] expectedStock = baseline.Shown();
            ushort[] expectedEdited = changed.Shown();
            (KraidBodyPage Top, KraidBodyPage Bottom) workingBody = (baseline.State.WorkingTop, baseline.State.WorkingBottom);
            for (int tick = 0; ; tick++)
            {
                ushort stockResult = baseline.Step(), editedResult = changed.Step();
                ticks++;
                AssertEqual(stockResult, editedResult, "head interpretation result is not editable");
                AssertAnimationValues(baseline.Body, changed.Body, "Kraid head tick " + tick);
                AssertAnimationValues(baseline.State, changed.State, "Kraid head tick " + tick);
                AssertEqual(baseline.State.DisplayedHeadFrame, changed.State.DisplayedHeadFrame,
                    "Kraid head tick " + tick + " displayed head frame is not editable");
                AssertTrue(workingBody == (baseline.State.WorkingTop, baseline.State.WorkingBottom) &&
                    workingBody == (changed.State.WorkingTop, changed.State.WorkingBottom),
                    "head DMA must not overwrite the WRAM body-map source");
                AssertEqual(baseline.Enemies.KraidBehavior.LastKraidSoundEffect, changed.Enemies.KraidBehavior.LastKraidSoundEffect,
                    "head art does not alter sound publication");
                if (tick == nextEvent)
                {
                    while (!commands[cursor].IsPose && !commands[cursor].IsTerminator)
                    {
                        KraidHeadNativeRecord sound = commands[cursor++];
                        AssertEqual((byte)sound.Sound, (baseline.Enemies.KraidBehavior.LastKraidSoundEffect ?? throw new InvalidOperationException("Verification requires baseline.Enemies.KraidBehavior.LastKraidSoundEffect.")).SoundEffect.Value,
                            "sound callback fires at the exact head admission tick");
                        if (sound.Opcode == KraidHeadNativeCursors.RoarCryCall) soundCount++;
                        callbacks++;
                    }
                    KraidHeadNativeRecord admission = commands[cursor++];
                    if (admission.IsTerminator)
                    {
                        AssertEqual(ushort.MaxValue, stockResult, "terminator on the exact sum-of-durations tick");
                        AssertEqual(admission.Cursor, baseline.State.HeadProgram, "termination retains native cursor");
                        AssertEqual(0, baseline.State.Parts[0].HeadTimer, "termination consumes the last timer tick");
                        AssertTrue(expectedStock.SequenceEqual(baseline.Shown()) &&
                            expectedEdited.SequenceEqual(changed.Shown()), "termination emits no tilemap upload");
                        break;
                    }
                    AssertEqual((ushort)1, stockResult, "frame admission result");
                    nextEvent += admission.Opcode;
                    physical = admission.Tilemap; outer = admission.OuterShape; inner = admission.InnerShape;
                    uploadCount++; frames++;
                    KraidHeadClockFixture.ExpectedUpload(expectedStock, (stock.KraidBackground ?? throw new InvalidOperationException("Verification requires stock.KraidBackground.")).HeadWords(physical));
                    KraidHeadClockFixture.ExpectedUpload(expectedEdited, (edited.KraidBackground ?? throw new InvalidOperationException("Verification requires edited.KraidBackground.")).HeadWords(physical));
                    AssertTrue(!baseline.Shown().SequenceEqual(changed.Shown()), "replacement head is actually visible");
                }
                AssertEqual((ushort)(nextEvent - tick), baseline.State.Parts[0].HeadTimer, "native countdown on every hold tick");
                KraidHeadNativeRecord displayed = commands.Take(cursor).Last(item => item.IsPose);
                AssertEqual(native.CursorAt((ushort)(displayed.Pointer + 8)), baseline.State.HeadProgram, "head cursor holds until next event");
                KraidHeadFrame shown = baseline.State.DisplayedHeadFrame
                    ?? throw new InvalidOperationException("Verification requires a displayed Kraid head frame.");
                AssertEqual(physical, KraidHeadFrameDefinitions.PresentationKey(shown), "compiled physical head selector survives art replacement");
                AssertEqual(outer, (ushort)shown.VulnerableShape, "outer mouth window follows compiled admission");
                AssertEqual(inner, shown.InvulnerableShape is { } innerShape ? (ushort)innerShape : ushort.MaxValue,
                    "inner mouth window follows compiled admission");
                AssertEqual(soundCount, baseline.State.RoarRequestCount, "roar callback count on every tick");
                AssertEqual(uploadCount, baseline.State.HeadTilemapUploadCount, "no extra head upload during hold");
                AssertTrue(expectedStock.SequenceEqual(baseline.Shown()), "exact stock head and untouched body");
                AssertTrue(expectedEdited.SequenceEqual(changed.Shown()), "exact authored head and untouched body");
                // Actual mouth collision method, including the strict bottom-edge -1,
                // not a radius-only approximation or equality of endpoint state.
                foreach (ushort hitbox in new[] { outer, inner }.Where(value => value != ushort.MaxValue))
                for (int x = -8; x <= 48; x += 8)
                for (int y = -144; y <= -64; y += 8)
                {
                    shot.Position = shot.Position.WithWhole(new((ushort)(baseline.Body.Position.X.Whole + x), (ushort)(baseline.Body.Position.Y.Whole + y)));
                    (short left, short top, _, short bottom) = KraidMouthHitboxDefinitionsConstants.Resolve(hitbox);
                    bool expected = y - shot.Radius.Y - 1 < bottom && y + shot.Radius.Y >= top && x + shot.Radius.X >= left;
                    AssertEqual(expected, baseline.Hit(hitbox, shot), "stock native mouth geometry");
                    AssertEqual(expected, changed.Hit(hitbox, shot), "authored head does not move physical mouth geometry");
                    collisions++;
                }
            }
            programsRun++;
        }
        AssertEqual(4, programsRun, "all four native head programs terminate");
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
        /// <summary>Cartridge-free address space used for the production head clock.</summary>
        internal SuperMetroid.Core.Hardware.SuperMetroidAddressSpace Memory { get; } =
            SuperMetroid.Core.Hardware.SuperMetroidAddressSpace.CreateWithoutCartridge();
        /// <summary>Bound production head-instruction dispatcher.</summary>
        private readonly Func<RoomEnemySlot, KraidEnemyState, ushort> step;
        /// <summary>Bound production mouth-versus-projectile overlap check.</summary>
        private readonly Func<RoomEnemySlot, ushort, SamusProjectileSlot, bool> hit;
        /// <summary>The selected art, from which the shown body resolves.</summary>
        private readonly EnemyTileArtworkCatalog art;

        /// <summary>Initializes Kraid's background with selected art.</summary>
        internal KraidHeadClockFixture(EnemyTileArtworkCatalog art)
        {
            this.art = art;
            Enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = art };
            State = new KraidEnemyState();
            Enemies.KraidBehavior.InitializeKraidBackground(State);
            step = new Func<RoomEnemySlot, KraidEnemyState, ushort>(Enemies.KraidBehavior.ProcessKraidHeadInstruction);
            hit = CoreAccess.KraidMouthHitboxOverlapsShot;
            Body.Position = Body.Position.WithWhole(new(512, 512));
        }

        /// <summary>Advances one head instruction tick using the production interpreter.</summary>
        internal ushort Step() => step(Body, State);
        /// <summary>The BG2 tilemap words Kraid's body frame shows.</summary>
        internal ushort[] Shown() => KraidBodyFixtures.Shown(art, State);
        /// <summary>Tests a projectile against the selected physical mouth hitbox.</summary>
        internal bool Hit(ushort pointer, SamusProjectileSlot shot) => hit(Body, pointer, shot);
        /// <summary>Updates an expected shown tilemap with one head tilemap's words over the upper page.</summary>
        internal static void ExpectedUpload(ushort[] destination, ReadOnlySpan<ushort> words) => words.CopyTo(destination);
    }
}
