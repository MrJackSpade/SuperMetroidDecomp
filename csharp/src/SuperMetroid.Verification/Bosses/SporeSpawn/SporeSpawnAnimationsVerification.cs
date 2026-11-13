using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Audio;
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
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.MovieReplay.Verification;

/// <summary>
/// #1288, #1600: Spore Spawn's typed body and projectile animations against the retail cartridge. Each body segment
/// must match its native list beat by beat and each body animation the lists' control flow (the walk also yields each
/// beat's native instruction address, so replays and traces can compare typed playbacks with native pointers), and each
/// projectile animation must match its bank-$86 list update by update.
/// </summary>
internal static class SporeSpawnAnimationsVerification
{
    /// <summary>Every body program matches its native list, and every projectile program its bank-$86 list.</summary>
    internal static void VerifySporeSpawnAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<SporeSpawnBodySegment>().Length, BodyEntries.Length, "every typed Spore Spawn body program has a native entry");
        for (int index = 0; index < BodyEntries.Length; index++)
            AssertEqual((SporeSpawnBodySegment)index, BodyEntries[index].Program, "the Spore Spawn body programs are declared in authored order");
        AssertTrue(BodyEntries.Zip(BodyEntries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends), "native Spore Spawn body entries ascend");
        var body = BodyLists(cartridge);
        int updates = VerifyProjectilePrograms(rom);
        AssertEqual(SporeSpawnProjectileVisualDefinitions.ProjectileFrameCount, SporeSpawnProjectileVisualDefinitions.ProjectileFrames.Count,
            "the projectile catalog lists every frame");
        Console.WriteLine($"  Spore Spawn animations: {body.Count} body segments ({body.Values.Sum(list => list.BeatAddresses.Length)} beats) match bank $A5, " +
            $"and {ProjectileEntries.Length} projectile programs match bank $86 over {updates} updates.");
    }

    /// <summary>Each body segment's native entry, in authored order.</summary>
    private static NativeProgramEntry<SporeSpawnBodySegment>[] BodyListEntries =>
        [.. BodyEntries.Select(item => new NativeProgramEntry<SporeSpawnBodySegment>(item.Program, item.Entry))];

    /// <summary>
    /// Compares every body segment with its native list and every body animation with the lists' control flow, and
    /// returns each list's beat and control addresses, which translate a typed playback to the native pointer.
    /// </summary>
    internal static Dictionary<SporeSpawnBodySegment, NativeEnemyList<SporeSpawnBodySegment>> BodyLists(IImportCartridgeSource cartridge)
    {
        Dictionary<SporeSpawnBodySegment, NativeEnemyList<SporeSpawnBodySegment>> lists = NativeEnemyAnimationOracle.Lists(cartridge, BodyBankNumber,
            BodyListEntries, SporeSpawnAnimations.Segments, SporeSpawnFrameDefinitionsConstants.Spritemap,
            new NativeEnemyCueRules<SporeSpawnCue>(
                word => BodyInstructions.TryGetValue(word, out var instruction) ? instruction.Operands : null,
                (word, operands, cue, at) => CompareBodyCue(word, operands, cue, at)));
        NativeEnemyAnimationOracle.CompareAnimations(lists, SporeSpawnAnimations.StartingAt);
        return lists;
    }

    /// <summary>Compares one native body instruction's operands with the cue it became.</summary>
    private static void CompareBodyCue(ushort word, ushort[] operands, SporeSpawnCue cue, string at)
    {
        BodyInstructions[word].Cue.Check(cue, at);
        switch (cue)
        {
            case SporeSpawnCue.SetFunction setFunction:
                AssertEqual(operands[0], SporeSpawnNativeWords.SporeSpawnFunctions.Word(setFunction.Function), $"{at}: the cartridge's routine word names the cue's function");
                break;
            case SporeSpawnCue.SetMaxXRadiusAndAngleDelta sway:
                AssertEqual(operands[0], sway.MaximumXRadius, $"{at}: maximum X radius in pixels");
                AssertEqual(unchecked((short)operands[1]), sway.AngleDelta, $"{at}: signed angle delta");
                break;
            case SporeSpawnCue.SetSporeGeneration setSporeGeneration:
                AssertEqual(operands[0] != 0, setSporeGeneration.SporeGenerationSuppressed, $"{at}: spore generation stopped");
                break;
            case SporeSpawnCue.QueueSound queueSound:
                AssertEqual(SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, operands[0]), queueSound.Sound, $"{at}: library-two sound");
                break;
            case SporeSpawnCue.LoadDeathPalette loadDeathPalette:
                AssertEqual(operands[0], checked((ushort)(loadDeathPalette.DeathPaletteStep * SporeSpawnSourceDefinitions.ImageBytes)), $"{at}: the cartridge's byte offset names the cue's death image");
                break;
            case SporeSpawnCue.LoadDeathTargetPalette loadDeathTargetPalette:
                AssertEqual(operands[0], checked((ushort)(loadDeathTargetPalette.DeathPaletteStep * SporeSpawnSourceDefinitions.ImageBytes)), $"{at}: the cartridge's byte offset names the cue's death image");
                break;
            case SporeSpawnCue.Command:
                break;
            default:
                throw new InvalidOperationException($"Undefined Spore Spawn cue {cue}.");
        }
    }

    /// <summary>
    /// The spritemaps Spore Spawn's body segments show; <see cref="BodyLists"/> confirms each equals the
    /// native list's operand.
    /// </summary>
    internal static HashSet<ushort> BodySpritemaps() =>
        [.. BodyEntries.SelectMany(item => SporeSpawnAnimations.Segments.Beats(item.Program))
            .OfType<AnimationBeat<SporeSpawnFrame, SporeSpawnCue>.Frame>()
            .Select(frame => SporeSpawnFrameDefinitionsConstants.Spritemap(frame.Shown))];

    /// <summary>The frames Spore Spawn's body programs show, counting each native frame record once.</summary>
    internal static int BodyFrameRecordCount =>
        BodyEntries.Sum(item => SporeSpawnAnimations.Segments.Beats(item.Program).Count(beat => beat is AnimationBeat<SporeSpawnFrame, SporeSpawnCue>.Frame));

    /// <summary>The native instruction pointer of Spore Spawn's body, translated from its typed playback.</summary>
    internal static ushort NativeBodyInstruction(Dictionary<SporeSpawnBodySegment, NativeEnemyList<SporeSpawnBodySegment>> lists, SporeSpawnEnemyState state) =>
        NativeEnemyAnimationOracle.PointerOf(lists, BodyListEntries, SporeSpawnAnimations.StartingAt, state.BodyPlayback);

    /// <summary>
    /// Runs every projectile program on a real spore beside its native list, comparing timer, frame, deletion, the
    /// drop request and the harmless impact properties each update; returns the updates compared.
    /// </summary>
    private static int VerifyProjectilePrograms(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int updates = 0;
        foreach ((SporeSpawnProjectileSegment program, ushort entry) in ProjectileEntries)
        {
            RoomEnemySystem enemies = NewProjectileSystem();
            enemies.SporeSpawnBehavior.SpawnSporeSpawnSpore(new(128, 256));
            EnemyProjectile projectile = enemies.Projectiles.EnemyProjectiles.First(item => item.Kind == RoomEnemyProjectileKind.SporeSpawnSpore);
            AssertTrue(projectile.Animation is SporeSpawnProjectileAnimation started &&
                started.Playback == new AnimationPlayback<SporeSpawnProjectileSegment>(SporeSpawnProjectileAnimations.StartingWith(SporeSpawnProjectileSegment.Spore), AnimationPlace.StartOfAnimation),
                "the real spore producer starts the falling-spore program");
            projectile.StartAnimation(new SporeSpawnProjectileAnimation(program));
            var native = new NativeEnemyProjectileListReader(entry, projectile.FrameHold.RawValue);
            int drops = 0;
            for (int update = 0; update < 48 && !native.Deleted; update++, updates++)
            {
                enemies.Projectiles.AdvanceEnemyProjectileAnimations(projectile, new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()), Position16<RoomPixels>.Zero);
                native.Step(cartridge);
                drops += native.Callbacks.Count(callback => callback == NativeEnemyProjectileInstruction.Spores_SpawnEnemyDrops);
                string at = $"Spore Spawn projectile {program} update {update}";
                AssertEqual(!native.Deleted, projectile.IsActive, $"{at}: deletion");
                if (native.Deleted)
                    break;
                AssertEqual(native.Timer, projectile.FrameHold.RawValue, $"{at}: timer");
                AssertEqual(native.Operand, projectile.PresentationOperandAddress, $"{at}: frame");
            }
            AssertEqual(drops, enemies.ActiveEnemyDropPickups, $"Spore Spawn projectile {program}: drop requests");
            AssertEqual(program == SporeSpawnProjectileSegment.SporeShot, native.Deleted, $"Spore Spawn projectile {program}: only the shot deletes");
        }
        return updates;
    }

    /// <summary>
    /// A projectile system with Spore Spawn loaded in slot zero, whose spores take his colours, and whose spore drops have
    /// a Samus to read.
    /// </summary>
    private static RoomEnemySystem NewProjectileSystem()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.SporeSpawnBehavior.SporeSpawn = new SporeSpawnEnemyState(enemies.SlotTable.Slots[0]);
        enemies.VerificationServices().NextRandom = () => 1;
        enemies.DeathsAndDrops.BindSamus(new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()));
        return enemies;
    }

    /// <summary>Runs <paramref name="program"/> on an initialized Spore Spawn fixture for a fixed number of enemy frames.</summary>
    internal static RunSporeSpawnProgramResult RunSporeSpawnProgram(
        SporeSpawnBodySegment program,
        int frames,
        ImportEnemyArtwork? artwork = null,
        Action<RoomEnemySystem, RoomEnemySlot>? afterFrame = null)
    {
        ImportEnemyArtwork bound = artwork ?? InstalledFixtures.EnemyArtwork;
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(bound.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.BossArtwork = bound.Bosses;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.SporeSpawn, body.Definition);
        body.Bind(body.DefinitionId, default(RoomEnemyDefinition));
        body.Position = body.Position.WithWhole(new(128, 624));
        body.Health = 960;
        body.AnimationClock.StepOnNextUpdate();

        var state = new SporeSpawnEnemyState(body)
        {
            Function = SporeSpawnFunction.Idle,
            MovementCenter = body.Position.Whole,
            MaximumXRadius = 48,
            AngleDelta = 1,
            BodyPlayback = new AnimationPlayback<SporeSpawnBodySegment>(SporeSpawnAnimations.StartingWith(program), AnimationPlace.StartOfAnimation),
        };
        enemies.VerificationServices().NextRandom = () => 4660;
        enemies.DeathsAndDrops.BindSamus(new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Health = 99, MaxHealth = 99 });
        enemies.SporeSpawnBehavior.SporeSpawn = state;

        for (int frame = 0; frame < frames; frame++)
        {
            EnemyAnimationPlayer.AdvanceAnimation(body, samus: null, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
            afterFrame?.Invoke(enemies, body);
        }
        return new RunSporeSpawnProgramResult(enemies, body);
    }

    /// <summary>Reads one native bank-$A5 word for the independent reference trace.</summary>
    internal static ushort ReadSporeSpawnProgramWord(CartridgeImportAddressSpace source, ushort address) =>
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
