using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Verification.Harness;

/// <summary>
/// #641: Botwoon's typed animations against the retail cartridge. A minimal reader walks the
/// native instruction lists the cartridge selects and the typed animation must match it frame by
/// frame: timers, drawn frames, collision radius, spit cues and the new-frame flag.
/// </summary>
internal static class BotwoonAnimationsVerification
{
    /// <summary>Selects nonpublic Botwoon behavior members exposed to this focused verification.</summary>
    private const BindingFlags BotwoonFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

    /// <summary><c>InstListPointers_Botwoon</c> at $B3:946B: moving lists, then eight hidden duplicates.</summary>
    private const int BotwoonMovingListTable = 0xb3946b;
    /// <summary>The eight hidden-head duplicates of the moving table at $B3:947B.</summary>
    private const int BotwoonHiddenListTable = 0xb3947b;
    /// <summary><c>InstListPointers_Botwoon_spit</c> at $B3:948B.</summary>
    private const int BotwoonSpitListTable = 0xb3948b;
    /// <summary><c>BotwoonsBodyTail_InstListPointers</c> at $86:E9F1, thirty-two selector entries.</summary>
    private const int BotwoonSegmentListTable = 0x86e9f1;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsSpit</c> at $86:EBAE.</summary>
    private const ushort BotwoonSpitList = 0xebae;

    /// <summary>Botwoon's private head opcodes ($B3:94C7-$9572) and what each does.</summary>
    private static readonly Dictionary<ushort, (int X, int Y)> BotwoonRadiusOpcodes = new()
    {
        [0x94c7] = (8, 16), [0x9507] = (8, 16), [0x9517] = (8, 16), [0x9557] = (8, 16),
        [0x94d7] = (12, 12), [0x94f7] = (12, 12), [0x9527] = (12, 12), [0x9547] = (12, 12),
        [0x94e7] = (16, 8), [0x9537] = (16, 8),
    };
    /// <summary>Native head-list command that marks the current frame as Botwoon's spit frame.</summary>
    private const ushort BotwoonSetSpittingFlagOpcode = 0x9567;
    /// <summary>Native head-list command that queues Botwoon's spit sound for comparison.</summary>
    private const ushort BotwoonQueueSpitSoundOpcode = 0x9572;

    /// <summary>Every head program the cartridge can select runs exactly like its native list.</summary>
    internal static void VerifyBotwoonHeadAnimations(SuperMetroidAddressSpace rom)
    {
        int compared = 0;
        var drawn = new HashSet<ushort>();
        for (int angle = 0; angle < 256; angle++)
        {
            ushort moving = CartridgeImportSource.Require(rom).ReadCartridgeWord((BotwoonMovingListTable + 2 * (angle >> 5)));
            CompareBotwoonHead(rom, moving, BotwoonHeadProgram.MovingToward((byte)angle), $"moving angle ${angle:X2}", drawn);
            ushort spitting = CartridgeImportSource.Require(rom).ReadCartridgeWord((BotwoonSpitListTable + 2 * (unchecked((byte)(angle + 16)) >> 5)));
            CompareBotwoonHead(rom, spitting, BotwoonHeadProgram.SpittingToward((byte)angle), $"spitting angle ${angle:X2}", drawn);
            compared += 2;
        }
        for (int octant = 0; octant < 8; octant++)
        {
            CompareBotwoonHead(rom, CartridgeImportSource.Require(rom).ReadCartridgeWord((BotwoonHiddenListTable + 2 * octant)), BotwoonHeadProgram.Hidden,
                $"hidden duplicate {octant}", drawn);
            compared++;
        }
        // Every drawn frame is exported artwork or the shared empty map (the resource audit requires the exports).
        AssertTrue(drawn.SetEquals(BotwoonVisualDefinitions.Frames().Select(frame => frame.Pointer)
                .Append(CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap)),
            "Botwoon's head animations draw exactly its sixteen exported frames and the empty map");
        Console.WriteLine($"  Botwoon head animations: {compared} cartridge selections match their native lists frame by frame.");
    }

    /// <summary>The AI selects the typed program the cartridge would, and reinstalling one does not restart it.</summary>
    internal static void VerifyBotwoonAnimationSelection(SuperMetroidAddressSpace rom)
    {
        Action<RoomEnemySlot, BotwoonEnemyState> animateHead = BotwoonBehavior.AnimateBotwoonHeadFromMovement;
        Action<RoomEnemySystem, RoomEnemySlot, BotwoonEnemyState, SamusState> aimHead = (owner, a0, a1, a2) => owner.BotwoonBehavior.AimBotwoonAtSamus(a0, a1, a2);
        (short X, short Y)[] octantVectors =
            [(0, -100), (100, -100), (100, 0), (100, 100), (0, 100), (-100, 100), (-100, 0), (-100, -100)];
        for (int octant = 0; octant < octantVectors.Length; octant++)
        {
            (short dx, short dy) = octantVectors[octant];
            (RoomEnemySystem movingEnemies, RoomEnemySlot movingHead, BotwoonEnemyState movingState) = NewBotwoonHead();
            movingHead.Position = movingHead.Position.WithWhole(new(unchecked((ushort)(1000 + dx)), unchecked((ushort)(1000 + dy))));
            movingState.HeadHistory[3] = new Position16<RoomPixels>(1000, 1000);
            movingState.InsideHole = false;
            animateHead(movingHead, movingState);
            AssertEqual(new BotwoonHeadProgram(BotwoonHeadProgramKind.Moving, BotwoonAnimations.FacingOfOctant(octant)),
                movingState.HeadProgram, $"moving head toward octant {octant}");
            CompareBotwoonHead(rom, CartridgeImportSource.Require(rom).ReadCartridgeWord((BotwoonMovingListTable + 2 * octant)), movingEnemies, movingHead,
                movingState, $"AI-selected moving octant {octant}");

            (RoomEnemySystem spitEnemies, RoomEnemySlot spitHead, BotwoonEnemyState spitState) = NewBotwoonHead();
            spitState.Function = BotwoonEnemyFunction.SpitWhileHidden;
            spitHead.Position = spitHead.Position.WithWhole(new(1000, 1000));
            var samus = new SamusState
            {
                Position = FixedPosition<RoomPixels>.FromWhole(new(unchecked((ushort)(1000 + dx)), unchecked((ushort)(1000 + dy)))),
            };
            aimHead(spitEnemies, spitHead, spitState, samus);
            AssertEqual(new BotwoonHeadProgram(BotwoonHeadProgramKind.Spitting, BotwoonAnimations.FacingOfOctant(octant)),
                spitState.HeadProgram, $"spitting head toward octant {octant}");
            // Outside a hidden spit the aim fires a three-shot volley at once. The spit angle is a quarter turn minus
            // the aim and the volley spreads a sixteenth of a turn either side of it, all wrapping around the turn.
            (RoomEnemySystem volleyEnemies, RoomEnemySlot volleyHead, BotwoonEnemyState volleyState) = NewBotwoonHead();
            volleyHead.Position = volleyHead.Position.WithWhole(new(1000, 1000));
            aimHead(volleyEnemies, volleyHead, volleyState, samus);
            byte aim = EnemyMath.CalculateCartridgeAngle(new(dx, dy));
            byte spitAngle = unchecked((byte)(64 - aim));
            AssertEqual(SnesAngle.FromTableIndex(spitAngle), volleyState.SpitAngle, $"spit angle toward octant {octant} wraps around the turn");
            int[] expectedDirections = [unchecked((byte)(spitAngle - 16)), spitAngle, unchecked((byte)(spitAngle + 16))];
            int[] actualDirections = volleyEnemies.Projectiles.EnemyProjectiles
                .Where(projectile => projectile.Kind == RoomEnemyProjectileKind.BotwoonSpit)
                .Select(projectile => projectile.DirectionParameter)
                .Order()
                .ToArray();
            AssertTrue(expectedDirections.Order().SequenceEqual(actualDirections),
                $"volley toward octant {octant} spreads around the wrapped spit angle");
        }

        // Travelling inside the wall hides the head; an unchanged program keeps its timer.
        (_, RoomEnemySlot head, BotwoonEnemyState state) = NewBotwoonHead();
        head.Position = head.Position.WithWhole(new(1010, 1000));
        state.HeadHistory[3] = new Position16<RoomPixels>(1000, 1000);
        state.InsideHole = false;
        animateHead(head, state);
        head.InstructionTimer = NativeCountdown.Start(7);
        state.HeadHistory[3] = new Position16<RoomPixels>(1000, 1000);
        animateHead(head, state);
        AssertEqual((ushort)7, head.InstructionTimer.RawValue, "reinstalling Botwoon's unchanged head program keeps it running");
        state.InsideHole = true;
        state.HeadHistory[3] = new Position16<RoomPixels>(1000, 1000);
        animateHead(head, state);
        AssertEqual(BotwoonHeadProgram.Hidden, state.HeadProgram, "Botwoon's head hides inside the wall");
        AssertEqual((ushort)1, head.InstructionTimer.RawValue, "a changed Botwoon head program restarts on the next update");

        for (ushort selector = 0; selector < 64; selector += 2)
        {
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord((BotwoonSegmentListTable + selector));
            _ = BotwoonAnimations.Segment(BotwoonSegmentProgram.FromSelector(selector));
            AssertTrue(native != 0, $"Botwoon segment selector ${selector:X2} has a native list");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => BotwoonSegmentProgram.FromSelector(1), "odd Botwoon segment selector");
        AssertThrows<ArgumentOutOfRangeException>(() => BotwoonSegmentProgram.FromSelector(64), "Botwoon segment selector past the table");
        AssertThrows<ArgumentOutOfRangeException>(() => BotwoonAnimations.FacingOfOctant(8), "Botwoon octant past the circle");
        Console.WriteLine("  Botwoon animation selection: AI head choices, hiding and reinstallation follow the cartridge.");
    }

    /// <summary>Every body, tail, hidden and spit animation runs exactly like its native bank-$86 list.</summary>
    internal static void VerifyBotwoonProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        void Pass(RoomEnemyProjectileSlot projectile) => enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, null, Position16<RoomPixels>.Zero);

        for (ushort selector = 0; selector < 64; selector += 2)
        {
            var projectile = new RoomEnemyProjectileSlot(0) { Kind = RoomEnemyProjectileKind.BotwoonBody };
            projectile.StartAnimation(BotwoonProjectileAnimation.ForSegment(BotwoonSegmentProgram.FromSelector(selector)));
            CompareBotwoonProjectile(rom, CartridgeImportSource.Require(rom).ReadCartridgeWord((BotwoonSegmentListTable + selector)), projectile, Pass,
                $"segment selector ${selector:X2}");
        }

        Action<RoomEnemySlot, byte, ushort> spawnSpit = enemies.BotwoonBehavior.SpawnBotwoonSpit;
        spawnSpit(enemies.SlotTable.Slots[0], 0x40, 0x0180);
        RoomEnemyProjectileSlot spit = enemies.Projectiles.EnemyProjectiles.First(item => item.Kind == RoomEnemyProjectileKind.BotwoonSpit);
        AssertTrue(spit.Animation is BotwoonProjectileAnimation { Segment: null }, "the real spit producer starts the typed spit animation");
        AssertTrue(spit.Animation is not null, "a typed spit keeps no native instruction pointer");
        CompareBotwoonProjectile(rom, BotwoonSpitList, spit, Pass, "spit");

        // A changed selector restarts the segment; an unchanged one keeps running.
        Action<RoomEnemyProjectileSlot, int> animate = enemies.BotwoonBehavior.AnimateBotwoonBodySegment;
        var segment = new RoomEnemyProjectileSlot(1) { Kind = RoomEnemyProjectileKind.BotwoonBody, DirectionParameter = 0 };
        segment.StartAnimation(BotwoonProjectileAnimation.ForSegment(BotwoonSegmentProgram.FromSelector(0)));
        Pass(segment);
        Pass(segment);
        animate(segment, 0);
        AssertEqual((ushort)7, segment.InstructionTimer.RawValue, "an unchanged Botwoon segment program keeps its timer");
        segment.DirectionParameter = 2;
        animate(segment, 0);
        AssertEqual((ushort)1, segment.InstructionTimer.RawValue, "a changed Botwoon segment program restarts");
        Pass(segment);
        AssertEqual(EnemyProjectileFrames.ProgramFrameKey(BotwoonVisualDefinitions.ShownProjectileFrames[new(BotwoonProjectileFrameKind.Body, BotwoonFacing.UpRight, 0)]),
            segment.PresentationOperandAddress, "the restarted segment shows its first frame");

        // Landing hands the segment to the shared native big-explosion list.
        Action<RoomEnemySlot, BotwoonEnemyState, ushort> spawnBody = enemies.BotwoonBehavior.SpawnBotwoonBodySegment;
        Action<RoomEnemyProjectileSlot, int> runBody = enemies.BotwoonBehavior.RunBotwoonBodyPreInstruction;
        RoomEnemySlot head = enemies.SlotTable.Slots[0];
        var state = new BotwoonEnemyState();
        enemies.BotwoonBehavior._botwoonState = state;
        spawnBody(head, state, 2);
        RoomEnemyProjectileSlot body = enemies.Projectiles.EnemyProjectiles.First(item => item.Kind == RoomEnemyProjectileKind.BotwoonBody);
        AssertEqual(BotwoonSegmentProgram.Hidden, ((BotwoonProjectileAnimation)(body.Animation ?? throw new InvalidOperationException("Verification requires body.Animation."))).Segment,
            "the real body producer starts a non-tail segment hidden");
        body.Position = body.Position.WithWholeY(200);
        body.Velocity = body.Velocity.WithX(new SnesSignedEightEight((ushort)BotwoonBodyFunction.Falling));
        runBody(body, 0);
        AssertEqual((ushort)BotwoonBodyFunction.Landed, body.Velocity.X.RawValue, "a landed body installs the cartridge RTS state");
        AssertTrue(body.Animation is MiscDustProjectileAnimation { Cursor.Program: MiscDustProjectileProgram.BigExplosion },
            "a landed body runs the shared misc-dust big explosion");
        Pass(body);

        Console.WriteLine("  Botwoon projectile animations: 32 segment selectors and the spit match their native lists; landing hands off to the shared explosion.");
    }

    /// <summary>Creates a head slot with Botwoon's state attached to its owning enemy system.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Head, BotwoonEnemyState State) NewBotwoonHead()
    {
        var enemies = new RoomEnemySystem();
        RoomEnemySlot head = enemies.SlotTable.Slots[0];
        head.Bind(EnemyDefinitionId.Botwoon, head.Definition);
        var state = new BotwoonEnemyState();
        enemies.BotwoonBehavior._botwoonState = state;
        return (enemies, head, state);
    }

    /// <summary>Starts a typed head program from its first instruction and compares it with the selected cartridge list.</summary>
    private static void CompareBotwoonHead(SuperMetroidAddressSpace rom, ushort nativeList, BotwoonHeadProgram program, string label,
        HashSet<ushort> drawn)
    {
        (RoomEnemySystem enemies, RoomEnemySlot head, BotwoonEnemyState state) = NewBotwoonHead();
        state.HeadProgram = program;
        state.HeadAnimationPosition = 0;
        head.InstructionTimer = NativeCountdown.Start(1);
        CompareBotwoonHead(rom, nativeList, enemies, head, state, label, drawn);
    }

    /// <summary>Runs the typed head beside a native-list reader for seventy updates and compares every observable.</summary>
    private static void CompareBotwoonHead(SuperMetroidAddressSpace rom, ushort nativeList, RoomEnemySystem enemies,
        RoomEnemySlot head, BotwoonEnemyState state, string label, HashSet<ushort>? drawn = null)
    {
        var native = new NativeBotwoonHead(nativeList, head.InstructionTimer.RawValue, head.Radius);
        for (int update = 0; update < 70; update++)
        {
            enemies.BotwoonBehavior.LastBotwoonSoundEffect = null;
            EnemyInstructionInterpreter.ProcessInstructions(head, samus: null, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
            bool nativeSound = native.Step(rom);
            string at = $"Botwoon {label} update {update}";
            AssertEqual(native.Timer, head.InstructionTimer.RawValue, $"{at}: timer");
            AssertEqual(native.Spritemap, head.SpritemapPointer, $"{at}: frame");
            if (native.NewFrame)
                drawn?.Add(head.SpritemapPointer);
            AssertEqual(native.Radius, head.Radius, $"{at}: radius");
            AssertEqual(native.NewFrame, head.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame), $"{at}: new-frame flag");
            AssertEqual(native.SpitFrame, state.SpitFrameReached, $"{at}: spit frame");
            AssertEqual(nativeSound ? BotwoonSoundDefinitions.Spit : (SoundEffectId?)null, enemies.BotwoonBehavior.LastBotwoonSoundEffect, $"{at}: spit sound");
        }
    }

    /// <summary>Runs a typed projectile beside a native-list reader for forty passes and compares timer and frame.</summary>
    private static void CompareBotwoonProjectile(SuperMetroidAddressSpace rom, ushort nativeList, RoomEnemyProjectileSlot projectile,
        Action<RoomEnemyProjectileSlot> pass, string label)
    {
        ushort cursor = nativeList, timer = projectile.InstructionTimer.RawValue, operand = 0;
        for (int update = 0; update < 40; update++)
        {
            pass(projectile);
            ushort old = timer;
            timer = NativeWordCounter.Decrement(timer).Value;
            if (old == 1)
            {
                for (int command = 0; command < 8; command++)
                {
                    ushort word = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x860000 | cursor));
                    if (word < 0x8000)
                    {
                        timer = word;
                        operand = unchecked((ushort)(cursor + 2));
                        cursor = unchecked((ushort)(cursor + 4));
                        break;
                    }
                    if (word == (ushort)EnemyProjectileInstruction.GotoY)
                    {
                        cursor = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x860000 | unchecked((ushort)(cursor + 2))));
                        continue;
                    }
                    AssertEqual((ushort)EnemyProjectileInstruction.Sleep, word, $"Botwoon {label}: native list uses only frames, goto and sleep");
                    timer = 0;
                    break;
                }
            }
            string at = $"Botwoon {label} pass {update}";
            AssertEqual(timer, projectile.InstructionTimer.RawValue, $"{at}: timer");
            AssertEqual(operand, projectile.PresentationOperandAddress, $"{at}: frame");
        }
    }

    /// <summary>A native bank-$B3 head list, read from the cartridge with the enemy interpreter's timer gate.</summary>
    internal sealed class NativeBotwoonHead(ushort cursor, ushort timer, Extent16<RoomPixels> radius)
    {
        /// <summary>Current bank-$B3 instruction offset in the native reference list.</summary>
        private ushort cursor = cursor;

        /// <summary>Updates remaining before the next native instruction is consumed.</summary>
        internal ushort Timer { get; private set; } = timer;
        /// <summary>Spritemap selected by the latest native frame instruction.</summary>
        internal ushort Spritemap { get; private set; }
        /// <summary>Collision extent after the latest native radius command.</summary>
        internal Extent16<RoomPixels> Radius { get; private set; } = radius;
        /// <summary>Whether this update consumed a new native frame instruction.</summary>
        internal bool NewFrame { get; private set; }
        /// <summary>Whether a native command reached Botwoon's spit frame.</summary>
        internal bool SpitFrame { get; private set; }

        /// <summary>Runs one update; returns whether the spit sound was queued.</summary>
        internal bool Step(SuperMetroidAddressSpace rom)
        {
            ushort old = Timer;
            Timer = NativeWordCounter.Decrement(Timer).Value;
            if (old != 1)
            {
                NewFrame = false;
                return false;
            }
            bool sound = false;
            for (int command = 0; command < 8; command++)
            {
                ushort word = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb30000 | cursor));
                if (word < 0x8000)
                {
                    Timer = word;
                    Spritemap = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb30000 | unchecked((ushort)(cursor + 2))));
                    cursor = unchecked((ushort)(cursor + 4));
                    NewFrame = true;
                    return sound;
                }
                if (word == (ushort)CommonEnemyInstruction.Sleep)
                    return sound;
                if (BotwoonRadiusOpcodes.TryGetValue(word, out var extent))
                    Radius = new((ushort)extent.X, (ushort)extent.Y);
                else if (word == BotwoonSetSpittingFlagOpcode)
                    SpitFrame = true;
                else if (word == BotwoonQueueSpitSoundOpcode)
                    sound = true;
                else
                    throw new InvalidDataException($"Botwoon native head opcode ${word:X4} is outside the compared set.");
                cursor = unchecked((ushort)(cursor + 2));
            }
            throw new InvalidDataException("A Botwoon native head list ran eight commands without a frame.");
        }
    }

    /// <summary>Runs the projectile instruction parity check against the repository cartridge.</summary>
    [VerificationCommand("--botwoon-projectile-instruction-mechanics")]
    internal static int BotwoonProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyBotwoonProjectileAnimations), () => VerifyBotwoonProjectileAnimations(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>Runs the head instruction parity check against the repository cartridge.</summary>
    [VerificationCommand("--botwoon-instruction-mechanics")]
    internal static int BotwoonInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyBotwoonHeadAnimations), () => VerifyBotwoonHeadAnimations(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>Checks that AI-selected head and segment programs match the cartridge's choices.</summary>
    [VerificationCommand("--botwoon-instruction-definitions")]
    internal static int BotwoonInstructionDefinitionsCommand()
    {
        Suite(nameof(VerifyBotwoonAnimationSelection), () => VerifyBotwoonAnimationSelection(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
