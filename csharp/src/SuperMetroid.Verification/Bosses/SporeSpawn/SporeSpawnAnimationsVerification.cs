using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Bosses.SporeSpawn.SporeSpawnNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.SporeSpawn;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// #1288: Spore Spawn's typed body and projectile programs against the retail cartridge. Each body program must
/// match its native list step by step (the walk also yields each step's native instruction address, so replays and
/// traces can compare typed cursors with native pointers), and each projectile program must match its bank-$86 list
/// update by update.
/// </summary>
internal static class SporeSpawnAnimationsVerification
{
    /// <summary>Every body program matches its native list, and every projectile program its bank-$86 list.</summary>
    internal static void VerifySporeSpawnAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<SporeSpawnBodyProgram>().Length, BodyEntries.Length, "every typed Spore Spawn body program has a native entry");
        for (int index = 0; index < BodyEntries.Length; index++)
            AssertEqual((SporeSpawnBodyProgram)index, BodyEntries[index].Program, "the Spore Spawn body programs are declared in authored order");
        AssertTrue(BodyEntries.Zip(BodyEntries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends), "native Spore Spawn body entries ascend");
        var body = BodyStepAddresses(cartridge);
        int updates = VerifyProjectilePrograms(rom);
        AssertEqual(SporeSpawnProjectileVisualDefinitions.ProjectileFrameCount, SporeSpawnProjectileVisualDefinitions.ProjectileFrames.Count,
            "the projectile catalog lists every frame");
        Console.WriteLine($"  Spore Spawn programs: {body.Count} body programs ({body.Values.Sum(steps => steps.Length)} steps) match bank $A5, " +
            $"and {ProjectileEntries.Length} projectile programs match bank $86 over {updates} updates.");
    }

    /// <summary>Compares every body program with its native list and returns each typed step's native address.</summary>
    internal static Dictionary<SporeSpawnBodyProgram, ushort[]> BodyStepAddresses(IImportCartridgeSource cartridge) =>
        BodyEntries.ToDictionary(item => item.Program, item => CompareBodyProgram(cartridge, item.Program, item.Entry));

    /// <summary>Compares one body program with its native list and returns each typed step's native address.</summary>
    private static ushort[] CompareBodyProgram(IImportCartridgeSource cartridge, SporeSpawnBodyProgram program, ushort entry)
    {
        var steps = SporeSpawnAnimations.Programs.Steps(program);
        var addresses = new ushort[steps.Count];
        ushort cursor = entry;
        for (int index = 0; index < steps.Count; index++)
        {
            string at = $"Spore Spawn {program} step {index}";
            addresses[index] = cursor;
            var step = steps[index];
            if (cursor != entry && BodyProgramAt(cursor) is { } next)
            {
                var fallInto = AssertIs<AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.FallIntoStep>(step, $"{at}: runs on into the next authored program");
                AssertEqual(next, fallInto.Target, $"{at}: fall-through target");
                AssertEqual(steps.Count, index + 1, $"{at}: fall-through ends the program");
                return addresses;
            }
            int address = (BodyBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                    var show = AssertIs<AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.ShowStep>(step, $"{at}: frame");
                    AssertEqual(word, show.Duration, $"{at}: duration");
                    AssertEqual(operand, SporeSpawnFrameDefinitionsConstants.Spritemap(show.Frame), $"{at}: spritemap");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    AssertTrue(step is AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.HoldStep, $"{at}: sleep");
                    AssertEqual(steps.Count, index + 1, $"{at}: sleep ends the program");
                    return addresses;
                case NativeEnemyListDefinitions.WordKind.GoTo:
                    var goTo = AssertIs<AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.GoToStep>(step, $"{at}: goto");
                    AssertEqual(BodyProgramAt(operand), goTo.Target, $"{at}: goto target");
                    AssertEqual(steps.Count, index + 1, $"{at}: goto ends the program");
                    return addresses;
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                    var setCounter = AssertIs<AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.SetCounterStep>(step, $"{at}: counter load");
                    AssertEqual(operand, setCounter.Count, $"{at}: counter");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                    var repeat = AssertIs<AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.RepeatWhileCountingStep>(step, $"{at}: counted repeat");
                    AssertEqual(BodyProgramAt(operand), repeat.Target, $"{at}: repeat target");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Wait:
                    var wait = AssertIs<AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.WaitStep>(step, $"{at}: wait");
                    AssertEqual(operand, wait.Duration, $"{at}: wait length");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    AssertTrue(BodyInstructions.TryGetValue(word, out var instruction), $"{at}: native instruction ${word:X4} is known");
                    var cueStep = AssertIs<AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.CueStep>(step, $"{at}: instruction");
                    SporeSpawnCue cue = cueStep.Cue;
                    instruction.Cue.Check(cue, at);
                    switch (cue)
                    {
                        case SporeSpawnCue.SetFunction setFunction:
                            AssertEqual(operand, SporeSpawnNativeWords.SporeSpawnFunctions.Word(setFunction.Function), $"{at}: the cartridge's routine word names the cue's function");
                            break;
                        case SporeSpawnCue.SetMaxXRadiusAndAngleDelta sway:
                            AssertEqual(operand, sway.MaximumXRadius, $"{at}: maximum X radius in pixels");
                            AssertEqual(unchecked((short)cartridge.ReadCartridgeWord(address + 4)), sway.AngleDelta, $"{at}: signed angle delta");
                            break;
                        case SporeSpawnCue.SetSporeGeneration setSporeGeneration:
                            AssertEqual(operand != 0, setSporeGeneration.SporeGenerationSuppressed, $"{at}: spore generation stopped");
                            break;
                        case SporeSpawnCue.QueueSound queueSound:
                            AssertEqual(SoundEffectCommands.Decode(SoundEffectLibrary.Library2, operand), queueSound.Sound, $"{at}: library-two sound");
                            break;
                        case SporeSpawnCue.LoadDeathPalette loadDeathPalette:
                            AssertEqual(operand, checked((ushort)(loadDeathPalette.DeathPaletteStep * SporeSpawnSourceDefinitions.ImageBytes)), $"{at}: the cartridge's byte offset names the cue's death image");
                            break;
                        case SporeSpawnCue.LoadDeathTargetPalette loadDeathTargetPalette:
                            AssertEqual(operand, checked((ushort)(loadDeathTargetPalette.DeathPaletteStep * SporeSpawnSourceDefinitions.ImageBytes)), $"{at}: the cartridge's byte offset names the cue's death image");
                            break;
                        case SporeSpawnCue.Command:
                            break;
                        default:
                            throw new InvalidOperationException($"Undefined Spore Spawn cue {cue}.");
                    }
                    cursor = unchecked((ushort)(cursor + 2 + 2 * instruction.Operands));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Stop:
                    throw new InvalidDataException($"{at}: Spore Spawn's body lists use no stops.");
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)}.");
            }
        }
        throw new InvalidDataException($"Spore Spawn {program} has steps past its native list.");
    }

    /// <summary>
    /// The spritemaps Spore Spawn's body programs show; <see cref="CompareBodyProgram"/> confirms each equals the
    /// native list's operand.
    /// </summary>
    internal static HashSet<ushort> BodySpritemaps() =>
        [.. BodyEntries.SelectMany(item => SporeSpawnAnimations.Programs.Steps(item.Program))
            .OfType<AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.ShowStep>()
            .Select(step => SporeSpawnFrameDefinitionsConstants.Spritemap(step.Frame))];

    /// <summary>The frames Spore Spawn's body programs show, counting each native frame record once.</summary>
    internal static int BodyFrameRecordCount =>
        BodyEntries.Sum(item => SporeSpawnAnimations.Programs.Steps(item.Program).Count(step => step is AnimationProgramStep<SporeSpawnBodyProgram, SporeSpawnFrame, SporeSpawnCue>.ShowStep));

    /// <summary>The native instruction pointer of Spore Spawn's body, translated from its typed cursor.</summary>
    internal static ushort NativeBodyInstruction(Dictionary<SporeSpawnBodyProgram, ushort[]> steps, SporeSpawnEnemyState state) =>
        steps[state.BodyProgram.Program][state.BodyProgram.Position];

    /// <summary>
    /// Runs every projectile program on a real spore beside its native list, comparing timer, frame, deletion, the
    /// drop request and the harmless impact properties each update; returns the updates compared.
    /// </summary>
    private static int VerifyProjectilePrograms(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int updates = 0;
        foreach ((SporeSpawnProjectileProgram program, ushort entry) in ProjectileEntries)
        {
            RoomEnemySystem enemies = NewProjectileSystem();
            enemies.SporeSpawnBehavior.SpawnSporeSpawnSpore(new(128, 256));
            RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles.First(item => item.Kind == RoomEnemyProjectileKind.SporeSpawnSpore);
            AssertTrue(projectile.Animation is SporeSpawnProjectileAnimation started &&
                started.Cursor == AnimationProgramCursor<SporeSpawnProjectileProgram>.Start(SporeSpawnProjectileProgram.Spore),
                "the real spore producer starts the falling-spore program");
            projectile.StartAnimation(new SporeSpawnProjectileAnimation(program));
            var native = new NativeEnemyProjectileListDefinitions.Reader(entry, projectile.InstructionTimer.RawValue);
            int drops = 0;
            for (int update = 0; update < 48 && !native.Deleted; update++, updates++)
            {
                enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, new SamusState(), Position16<RoomPixels>.Zero);
                native.Step(cartridge);
                drops += native.Callbacks.Count(callback => callback == NativeEnemyProjectileInstruction.Spores_SpawnEnemyDrops);
                string at = $"Spore Spawn projectile {program} update {update}";
                AssertEqual(!native.Deleted, projectile.IsActive, $"{at}: deletion");
                if (native.Deleted)
                    break;
                AssertEqual(native.Timer, projectile.InstructionTimer.RawValue, $"{at}: timer");
                AssertEqual(native.Operand, projectile.PresentationOperandAddress, $"{at}: frame");
            }
            AssertEqual(drops, enemies.SporeSpawnDropRequests.Count, $"Spore Spawn projectile {program}: drop requests");
            AssertEqual(program == SporeSpawnProjectileProgram.SporeShot, native.Deleted, $"Spore Spawn projectile {program}: only the shot deletes");
        }
        return updates;
    }

    /// <summary>A projectile system whose spore drops have a Samus to read.</summary>
    private static RoomEnemySystem NewProjectileSystem()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 1;
        enemies.DeathsAndDrops.BindSamus(new SamusState());
        return enemies;
    }

    /// <summary>Runs <paramref name="program"/> on an initialized Spore Spawn fixture for a fixed number of enemy frames.</summary>
    internal static RunSporeSpawnProgramResult RunSporeSpawnProgram(
        SporeSpawnBodyProgram program,
        int frames,
        InstalledEnemyArtwork? artwork = null,
        Action<RoomEnemySystem, RoomEnemySlot>? afterFrame = null)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        InstalledEnemyArtwork bound = artwork ?? InstalledFixtures.EnemyArtwork;
        enemies.TileArtwork = bound.Enemies;
        enemies.BossArtwork = bound.Bosses;
        enemies.AreaArtwork = bound.Areas;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.SporeSpawn, body.Definition);
        body.Bind(body.DefinitionId, default(RoomEnemyDefinition));
        body.Position = body.Position.WithWhole(new(128, 624));
        body.Health = 960;
        body.InstructionTimer = WrappingCountdown.Start(1);

        var state = new SporeSpawnEnemyState(body)
        {
            Function = SporeSpawnFunction.Idle,
            MovementCenter = body.Position.Whole,
            MaximumXRadius = 48,
            AngleDelta = 1,
            BodyProgram = AnimationProgramCursor<SporeSpawnBodyProgram>.Start(program),
        };
        enemies.Presentation.BindColours(new GameplayColours(new SpriteColourSets()));
        enemies.VerificationServices().NextRandom = () => 4660;
        enemies.DeathsAndDrops.BindSamus(new SamusState { Health = 99, MaxHealth = 99 });
        enemies.SporeSpawnBehavior.SporeSpawn = state;

        for (int frame = 0; frame < frames; frame++)
        {
            EnemyInstructionInterpreter.ProcessInstructions(body, samus: null, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
            afterFrame?.Invoke(enemies, body);
        }
        return new RunSporeSpawnProgramResult(enemies, body);
    }

    /// <summary>Reads one native bank-$A5 word for the independent reference trace.</summary>
    internal static ushort ReadSporeSpawnProgramWord(SuperMetroidAddressSpace source, ushort address) =>
        CartridgeImportSource.Require(source).ReadCartridgeWord((BodyBankNumber << 16) | address);

    /// <summary>A guard over the cartridge that rejects any production read of Spore Spawn's native body lists.</summary>
    internal static MutableCartridgeReadGuard NewSporeSpawnInstructionReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address is >= BodyListsStart and < BodyListsEnd,
            address => $"Production read Spore Spawn's native body list at ${address:X6}.");
        return guard;
    }

    /// <summary>Runs Spore Spawn's program parity check against the repository cartridge.</summary>
    [VerificationCommand("--spore-spawn-animations")]
    internal static int SporeSpawnAnimationsCommand()
    {
        Suite(nameof(VerifySporeSpawnAnimations), () => VerifySporeSpawnAnimations(LoadRepositoryRom()));
        return 0;
    }
}
