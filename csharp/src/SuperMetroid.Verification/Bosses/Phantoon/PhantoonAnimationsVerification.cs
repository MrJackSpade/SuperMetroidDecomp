using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Phantoon;

using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core;

/// <summary>
/// #1286: Phantoon's typed body, eye, tentacle, mouth and flame programs against the retail cartridge. The part
/// programs must match the native bank-$A7 lists step by step and run their callbacks as the native interpreter
/// does; the flames and the pre-Phantoon room projectile must match their bank-$86 lists update by update.
/// </summary>
internal static class PhantoonAnimationsVerification
{
    /// <summary>Creates the four linked Phantoon parts and a deterministic random source.</summary>
    internal static RoomEnemySystem CreatePhantoonSystem()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        RoomEnemySlot eye = enemies.SlotTable.Slots[1];
        RoomEnemySlot tentacles = enemies.SlotTable.Slots[2];
        RoomEnemySlot mouth = enemies.SlotTable.Slots[3];
        body.Bind(EnemyDefinitionId.PhantoonBody, body.Definition);
        eye.Bind(EnemyDefinitionId.PhantoonEye, eye.Definition);
        tentacles.Bind(EnemyDefinitionId.PhantoonTentacles, tentacles.Definition);
        mouth.Bind(EnemyDefinitionId.PhantoonMouth, mouth.Definition);
        enemies.PhantoonBehavior.Phantoon = new PhantoonEnemyState(body)
        {
            Eye = eye,
            Tentacles = tentacles,
            Mouth = mouth,
        };
        return enemies;
    }

    /// <summary>The program a Phantoon part is running.</summary>
    internal static PhantoonPartProgram PartProgram(RoomEnemySystem enemies, RoomEnemySlot part) =>
        RequireState(enemies).PartProgram(part).Program;

    /// <summary>The system's Phantoon state.</summary>
    private static PhantoonEnemyState RequireState(RoomEnemySystem enemies) =>
        enemies.PhantoonBehavior.Phantoon ?? throw new InvalidOperationException("Verification requires Phantoon's state.");

    /// <summary>
    /// Every part program's typed steps equal its native list, and the eye-opening and mouth programs run their
    /// callbacks through the production interpreter as the native lists do.
    /// </summary>
    internal static void VerifyPhantoonPartAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<PhantoonPartProgram>().Length, PartEntries.Length, "every typed Phantoon part program has a native entry");
        for (int index = 0; index < PartEntries.Length; index++)
            AssertEqual((PhantoonPartProgram)index, PartEntries[index].Program, "the Phantoon part programs are declared in authored order");
        AssertTrue(PartEntriesAscend, "native Phantoon part entries ascend");

        int stepCount = 0;
        foreach ((PhantoonPartProgram program, ushort entry) in PartEntries)
            stepCount += CompareProgram(cartridge, program, entry);
        for (int ordinal = 0; ordinal < 8; ordinal++)
            AssertEqual(PhantoonPartProgram.EyeLookingUp + ordinal, PhantoonAnimationsConstants.Gaze(ordinal), $"gaze {ordinal} selects its program");

        VerifyEyeOpening();
        VerifyMouthFlame();
        Console.WriteLine($"  Phantoon part programs: {PartEntries.Length} programs and {stepCount} steps match bank $A7; eye-opening and mouth callbacks run as the native lists do.");
    }

    /// <summary>Compares one typed part program with its native list and returns its typed step count.</summary>
    private static int CompareProgram(IImportCartridgeSource cartridge, PhantoonPartProgram program, ushort entry)
    {
        var steps = PhantoonAnimations.Programs.Steps(program);
        ushort cursor = entry;
        int index = 0;
        for (int command = 0; command < 64; command++)
        {
            string at = $"Phantoon {program} step {index}";
            if (cursor != entry && PartProgramAt(cursor) is { } next)
            {
                AssertEqual(AnimationProgramStepKind.FallInto, steps[index].Kind, $"{at}: runs on into the next authored program");
                AssertEqual(next, steps[index].Target, $"{at}: fall-through target");
                AssertEqual(steps.Count, index + 1, $"{at}: fall-through ends the program");
                return steps.Count;
            }
            int address = (PartBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                    AssertEqual(AnimationProgramStepKind.Show, steps[index].Kind, $"{at}: frame");
                    AssertEqual(word, steps[index].Duration, $"{at}: duration");
                    AssertEqual(operand, PhantoonPartFrameSpritemapsConstants.Spritemap(steps[index].Frame), $"{at}: spritemap");
                    cursor = unchecked((ushort)(cursor + 4));
                    index++;
                    continue;
                case NativeEnemyListDefinitions.WordKind.GoTo:
                    AssertEqual(AnimationProgramStepKind.GoTo, steps[index].Kind, $"{at}: goto");
                    AssertEqual(PartProgramAt(operand), steps[index].Target, $"{at}: goto target");
                    AssertEqual(steps.Count, index + 1, $"{at}: goto ends the program");
                    return steps.Count;
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    AssertEqual(AnimationProgramStepKind.Hold, steps[index].Kind, $"{at}: sleep");
                    AssertEqual(steps.Count, index + 1, $"{at}: sleep ends the program");
                    return steps.Count;
                case NativeEnemyListDefinitions.WordKind.Callback:
                    AssertEqual(CallFunctionInY, word, $"{at}: Phantoon's callbacks use the common call-function command");
                    AssertTrue(PartCallbacks.TryGetValue(operand, out PhantoonPartCue cue), $"{at}: native routine ${operand:X4} is known");
                    AssertEqual(AnimationProgramStepKind.Cue, steps[index].Kind, $"{at}: callback");
                    AssertEqual(cue, steps[index].Cue, $"{at}: cue");
                    cursor = unchecked((ushort)(cursor + 4));
                    index++;
                    continue;
                case NativeEnemyListDefinitions.WordKind.Stop:
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                case NativeEnemyListDefinitions.WordKind.Wait:
                    throw new InvalidDataException($"{at}: Phantoon's part lists use no stops, counters or waits.");
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)}.");
            }
        }
        throw new InvalidDataException($"Phantoon {program} ran past its native list.");
    }

    /// <summary>Runs one part update, optionally expiring the current frame first.</summary>
    private static void Pass(RoomEnemySlot part, bool forceExpiry = false)
    {
        if (forceExpiry)
            part.InstructionTimer = NativeCountdown.Start(1);
        EnemyInstructionInterpreter.ProcessInstructions(part, samus: null, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
    }

    /// <summary>
    /// The opened eye's callbacks queue the materialization sound and install the eye-only body hitbox, while the
    /// list's sleep keeps the open eye on its own program, as the native sleep overwrites the callback's install.
    /// </summary>
    private static void VerifyEyeOpening()
    {
        RoomEnemySystem enemies = CreatePhantoonSystem();
        PhantoonEnemyState state = RequireState(enemies);
        RoomEnemySlot eye = state.Eye;
        state.PartProgram(eye) = AnimationProgramCursor<PhantoonPartProgram>.Start(PhantoonPartProgram.EyeOpen);
        eye.InstructionTimer = NativeCountdown.Start(1);
        for (int frame = 0; frame < 3; frame++)
        {
            Pass(eye, forceExpiry: true);
            AssertEqual(PhantoonPartFrameSpritemapsConstants.Spritemap(new PhantoonPartFrame(PhantoonPartFrameKind.OpeningEye, frame)), eye.SpritemapPointer, $"opening eye frame {frame}");
        }
        AssertTrue(state.LastMaterializationSound is null, "the materialization sound waits for the last frame to expire");
        Pass(eye, forceExpiry: true);
        AssertTrue(state.LastMaterializationSound.HasValue, "the opened eye queues its materialization sound");
        AssertEqual(PhantoonPartProgram.EyeHitboxBody, PartProgram(enemies, state.Body), "the opened eye installs the eye-only body hitbox");
        AssertEqual(PhantoonPartProgram.EyeOpen, PartProgram(enemies, eye), "the list's sleep keeps the eye on its opened program");
        AssertEqual(PhantoonPartFrameSpritemapsConstants.Spritemap(new PhantoonPartFrame(PhantoonPartFrameKind.OpeningEye, 2)), eye.SpritemapPointer, "the opened eye keeps its last frame");
        AssertEqual((ushort)1, eye.InstructionTimer.RawValue, "the callback's one-update timer survives the sleep");
    }

    /// <summary>The mouth's flame program spawns one casual flame and runs on into its initial frame.</summary>
    private static void VerifyMouthFlame()
    {
        RoomEnemySystem enemies = CreatePhantoonSystem();
        PhantoonEnemyState state = RequireState(enemies);
        RoomEnemySlot mouth = state.Mouth;
        state.PartProgram(mouth) = AnimationProgramCursor<PhantoonPartProgram>.Start(PhantoonPartProgram.MouthSpawnFlame);
        Pass(mouth, forceExpiry: true);
        Pass(mouth, forceExpiry: true);
        AssertEqual(0, ActiveFlames(enemies), "the mouth opens before it spawns");
        Pass(mouth, forceExpiry: true);
        AssertEqual(1, ActiveFlames(enemies), "the mouth spawns one casual flame");
        AssertEqual(PhantoonPartProgram.MouthInitial, PartProgram(enemies, mouth), "the mouth runs on into its initial program");
        AssertEqual(PhantoonPartFrameSpritemapsConstants.Spritemap(new PhantoonPartFrame(PhantoonPartFrameKind.Mouth, 0)), mouth.SpritemapPointer, "the mouth shows its closed frame at once");
    }

    /// <summary>Active Phantoon destroyable flames.</summary>
    private static int ActiveFlames(RoomEnemySystem enemies) =>
        enemies.Projectiles.EnemyProjectiles.Count(item => item.IsActive && item.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame);

    /// <summary>
    /// The real flame producers start the typed programs their native lists start, and every flame program matches
    /// its bank-$86 list update by update: frames, timers, the falling loop counter, the drop request and deletion.
    /// </summary>
    internal static void VerifyPhantoonFlameAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        VerifyFlameProducers();
        foreach ((PhantoonFlameProgram program, ushort entry) in FlameEntries)
        {
            RoomEnemySystem enemies = NewFlameSystem();
            RoomEnemyProjectileSlot flame = SpawnDestroyable(enemies, PhantoonFlameSpawnType.Enraged);
            flame.StartAnimation(new PhantoonFlameAnimation(program));
            var native = new NativeEnemyProjectileListDefinitions.Reader(entry, flame.InstructionTimer.RawValue);
            int drops = 0;
            for (int update = 0; update < 64 && !native.Deleted; update++)
            {
                enemies.Projectiles.ProcessEnemyProjectileInstructions(flame, new SamusState(), Position16<RoomPixels>.Zero);
                native.Step(cartridge);
                drops += native.Callbacks.Count;
                string at = $"Phantoon flame {program} update {update}";
                AssertEqual(!native.Deleted, flame.IsActive, $"{at}: deletion");
                if (native.Deleted)
                    break;
                AssertEqual(native.Timer, flame.InstructionTimer.RawValue, $"{at}: timer");
                AssertEqual(native.Operand, flame.PresentationOperandAddress, $"{at}: frame");
                AssertEqual(native.Counter, flame.GeneralTimer, $"{at}: loop counter");
            }
            AssertEqual(drops, enemies.PhantoonFlameDropRequests.Count, $"Phantoon flame {program}: drop requests");
            bool deletes = program is PhantoonFlameProgram.CasualResting or PhantoonFlameProgram.Dying or PhantoonFlameProgram.RainImpact
                or PhantoonFlameProgram.Delete or PhantoonFlameProgram.DestroyableShot;
            AssertEqual(deletes, native.Deleted, $"Phantoon flame {program}: deletion matches the native list");
        }

        AssertEqual(PhantoonFlameVisualDefinitions.ProjectileFrameCount, PhantoonFlameVisualDefinitions.ProjectileFrames.Count, "the flame catalog lists every frame");
        VerifyPrePhantoonRoomAnimation(cartridge);
        Console.WriteLine($"  Phantoon flame programs: {FlameEntries.Length} programs match bank $86 every update, with the falling counter, drop request and deletion.");
    }

    /// <summary>A projectile system whose drops have a Samus to read.</summary>
    private static RoomEnemySystem NewFlameSystem()
    {
        RoomEnemySystem enemies = CreatePhantoonSystem();
        enemies.VerificationServices().NextRandom = () => 128;
        enemies.DeathsAndDrops.BindSamus(new SamusState { Health = 99, MaxHealth = 99 });
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Position = body.Position.WithWhole(new(128, 96));
        return enemies;
    }

    /// <summary>Spawns one destroyable flame through the real producer.</summary>
    private static RoomEnemyProjectileSlot SpawnDestroyable(RoomEnemySystem enemies, PhantoonFlameSpawnType type)
    {
        AssertTrue(enemies.PhantoonBehavior.SpawnPhantoonDestroyableFlame(enemies.SlotTable.Slots[0], type, 0), $"the real {type} producer allocates a flame");
        return enemies.Projectiles.EnemyProjectiles.Single(item => item.IsActive && item.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame);
    }

    /// <summary>The starting, casual and rage producers start the programs their native initializers install.</summary>
    private static void VerifyFlameProducers()
    {
        RoomEnemySystem starting = NewFlameSystem();
        AssertTrue(starting.PhantoonBehavior.SpawnPhantoonStartingFlame(starting.SlotTable.Slots[0], 0), "the real starting producer allocates a flame");
        AssertStarted(starting.Projectiles.EnemyProjectiles.Single(item => item.Kind == RoomEnemyProjectileKind.PhantoonStartingFlame),
            PhantoonFlameProgram.StartingFlame, "starting flame");
        RoomEnemySystem casual = NewFlameSystem();
        AssertStarted(SpawnDestroyable(casual, PhantoonFlameSpawnType.Casual), PhantoonFlameProgram.CasualFalling0, "casual flame");
        RoomEnemySystem rage = NewFlameSystem();
        AssertStarted(SpawnDestroyable(rage, PhantoonFlameSpawnType.Enraged), PhantoonFlameProgram.DestroyableIdle, "rage flame");
    }

    /// <summary>Requires <paramref name="flame"/> to run <paramref name="program"/> from its start.</summary>
    private static void AssertStarted(RoomEnemyProjectileSlot flame, PhantoonFlameProgram program, string label) =>
        AssertTrue(flame.Animation is PhantoonFlameAnimation started && started.Cursor == AnimationProgramCursor<PhantoonFlameProgram>.Start(program),
            $"the {label} starts the {program} program");

    /// <summary>The pre-Phantoon room projectile's blank hold and deletion match $86:A3AA update by update.</summary>
    private static void VerifyPrePhantoonRoomAnimation(IImportCartridgeSource cartridge)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var scroll = new BackgroundScrollState();
        enemies.PrePhantoonRoomBehavior.SpawnPrePhantoonRoomProjectile(scroll);
        RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles.Single(item => item.Kind == RoomEnemyProjectileKind.PrePhantoonRoom);
        AssertTrue(projectile.Animation is PrePhantoonRoomAnimation, "the room setup starts the typed room animation");
        var native = new NativeEnemyProjectileListDefinitions.Reader(PrePhantoonRoomEntry, projectile.InstructionTimer.RawValue);
        for (int update = 0; update < 40 && !native.Deleted; update++)
        {
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, null, Position16<RoomPixels>.Zero);
            native.Step(cartridge);
            AssertEqual(!native.Deleted, projectile.IsActive, $"pre-Phantoon room update {update}: deletion");
            if (native.Deleted)
                break;
            AssertEqual(native.Timer, projectile.InstructionTimer.RawValue, $"pre-Phantoon room update {update}: timer");
            // The native operand selects the empty projectile spritemap, which has no installed program frame.
            AssertEqual(cartridge.ReadCartridgeWord(NativeEnemyProjectileListDefinitions.ProjectileBank | native.Operand), projectile.SpritemapPointer,
                $"pre-Phantoon room update {update}: spritemap");
            AssertTrue(projectile.Frame == EnemyProjectileFrameId.Blank, $"pre-Phantoon room update {update}: the blank frame, no program frame");
        }
        AssertTrue(native.Deleted, "the pre-Phantoon room projectile deletes itself");
    }

    /// <summary>Runs Phantoon's part program parity check against the repository cartridge.</summary>
    [VerificationCommand("--phantoon-part-animations")]
    internal static int PhantoonPartAnimationsCommand()
    {
        Suite(nameof(VerifyPhantoonPartAnimations), () => VerifyPhantoonPartAnimations(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>Runs Phantoon's flame program parity check against the repository cartridge.</summary>
    [VerificationCommand("--phantoon-flame-animations")]
    internal static int PhantoonFlameAnimationsCommand()
    {
        Suite(nameof(VerifyPhantoonFlameAnimations), () => VerifyPhantoonFlameAnimations(LoadRepositoryRom()));
        return 0;
    }
}
