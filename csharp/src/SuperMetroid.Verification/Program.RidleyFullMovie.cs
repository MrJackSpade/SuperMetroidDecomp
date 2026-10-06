using System.Buffers.Binary;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifySpringBallRelease()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RidleyMovieMemory.RidleyRoom);
        runtime.InitializeDebugGroundedSamus(79, 425, 16);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.SpringBall);
        samus.Pose = SamusPoseIds.SpringBallMovingLeftPose;
        samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
        samus.SetAnimationFrameFromSpecialHandler(5, 3);
        samus.XPosition = 79; samus.Kinematics.XSubposition = 0x8000;
        samus.YPosition = 425; samus.Kinematics.YSubposition = ushort.MaxValue;
        samus.HorizontalSpeed.BaseSpeed = 3; samus.HorizontalSpeed.BaseSubspeed = 0xc000;
        samus.HorizontalSpeed.AccelerationMode = 0;
        runtime.Controller1.Latch(0x0200);
        // Original movie source 1616 -> 1617: release Left while rolling.
        runtime.StepFrame(0);
        AssertEqual(0x004c4000u, samus.Kinematics.XFixed, "spring release retains native final displacement");
        AssertEqual(SamusPoseIds.SpringBallGroundLeftPose, samus.Pose, "spring release selects stationary pose immediately");
        AssertEqual(0u, samus.HorizontalSpeed.BaseFixed, "spring release clears base momentum after movement");
        AssertEqual((ushort)0, samus.HorizontalSpeed.AccelerationMode, "spring release clears acceleration mode");
        runtime.StepFrame(0);
        AssertEqual(0x004c4000u, samus.Kinematics.XFixed, "released spring ball stays stopped next update");
        Console.WriteLine("Spring Ball release: native movie displacement, pose and immediate momentum reset pass.");
    }

    private static void VerifyRidleyTailOffsets()
    {
        var tick = typeof(RoomEnemySystem).GetMethod("TickRidleyTailSegment", BindingFlags.Static | BindingFlags.NonPublic)!;
        var state = new RidleyEnemyState
        {
            TailSegments = Enumerable.Range(0, 7).Select(_ => new RidleyTailSegment()).ToArray(),
            IdealInterSegmentTailAngle = 16, TailAngleDelta = 2,
            TailMinimumClockwiseAngle = 0x3fc0, TailMaximumCounterClockwiseAngle = 0x4010,
            TailWhipTargetClockwiseAngle = ushort.MaxValue,
            TailWhipTargetCounterClockwiseAngle = ushort.MaxValue,
        };
        var segment = state.TailSegments[0];
        segment.Active = true; segment.StaggerAngle = 2;
        segment.XOffset = 7; segment.YOffset = 11;
        tick.Invoke(null, [state, 0]);
        AssertEqual((ushort)4, segment.StaggerAngle, "native stagger advances before returning");
        AssertEqual((ushort)7, segment.XOffset, "stagger retains X offset");
        AssertEqual((ushort)11, segment.YOffset, "stagger retains Y offset");
        segment.StaggerAngle = ushort.MaxValue; segment.Angle = 0x4000;
        segment.MovementDirection = 0x8000; segment.Distance = 0x0200;
        tick.Invoke(null, [state, 0]);
        AssertEqual((ushort)0x3ffe, segment.Angle, "native clockwise comparison decrements then restores one before storing");
        AssertEqual(ushort.MaxValue, segment.XOffset, "signed Mode 7 multiplication floors negative fraction");
        AssertEqual((ushort)1, segment.YOffset, "signed Mode 7 cosine product");
        state.TailWhipTargetClockwiseAngle = 0x4000;
        segment.Angle = 0x4000; segment.XOffset = 7; segment.YOffset = 11;
        tick.Invoke(null, [state, 0]);
        AssertTrue(!segment.Active, "root stops at whip target");
        AssertEqual((ushort)7, segment.XOffset, "stop retains X offset");
        AssertEqual((ushort)11, segment.YOffset, "stop retains Y offset");
        var child = state.TailSegments[1];
        segment.Active = true;
        child.Active = true; child.StaggerAngle = ushort.MaxValue;
        child.Angle = 0x4000; child.MovementDirection = 0x8000; child.Distance = 0x0800;
        tick.Invoke(null, [state, 1]);
        AssertTrue(child.Active, "moving predecessor prevents child deactivation");
        AssertEqual((ushort)0x3fc0, child.Angle, "blocked child clamps to native clockwise limit");
        Console.WriteLine("Ridley tail offsets: stagger/stop retention, clockwise arithmetic, signed multiplication and predecessor gate pass.");
    }

    private static void VerifyRidleyCenterFacing()
    {
        var method = typeof(RoomEnemySystem).GetMethod("SelectNorfairRidleyFacingInstruction",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        (ushort X, ushort Facing, bool Turn)[] cases = [
            (0x004a, 2, false), // Original movie frame 734: already facing inward.
            (0x00c0, 2, true), (0x004a, 0, true), (0x00c0, 0, false),
            (0x004a, 1, false), (0x007f, 2, false), (0x0080, 2, true),
            (0x014a, 2, false), // Native tests the low position byte, not full X >= 128.
        ];
        foreach (var sample in cases)
        {
            var slot = new RoomEnemySlot(0) { XPosition = sample.X,
                CurrentInstruction = RidleyMovieMemory.RidleyRightFlyingSleep,
                InstructionTimer = 7, Timer = 9 };
            var state = new RidleyEnemyState { FacingDirection = sample.Facing };
            method.Invoke(null, [slot, state]);
            ushort expected = !sample.Turn ? RidleyMovieMemory.RidleyRightFlyingSleep
                : sample.Facing == 0 ? RidleyInstructionProgramDefinitions.TurnFromLeftToRight
                : RidleyInstructionProgramDefinitions.TurnFromRightToLeft;
            AssertEqual(expected, slot.CurrentInstruction, $"native center-facing instruction at {sample.X:X4}, facing {sample.Facing}");
            AssertEqual(sample.Turn ? (ushort)2 : (ushort)7, slot.InstructionTimer, "turn timer changes only when native condition is met");
            AssertEqual(sample.Turn ? (ushort)0 : (ushort)9, slot.Timer, "loop counter is preserved when no turn is needed");
        }
        var hover = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyHover", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var hoverState = new RidleyEnemyState { FunctionTimer = 0, Function = RidleyAiFunction.NorfairHover };
        hover.Invoke(new RoomEnemySystem(), [new RoomEnemySlot(0), hoverState]);
        AssertEqual(ushort.MaxValue, hoverState.FunctionTimer, "native hover decrements zero before selecting next attack");
        AssertEqual(RidleyAiFunction.NorfairSelectAttack, hoverState.Function, "negative hover timer exits before movement");
        Console.WriteLine("Ridley center facing: movie trigger, both sides/directions, mid-turn and native low-byte boundary agree.");
    }

    private static void VerifyRidleyDoorEntry()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RidleyMovieMemory.SourceRoom);
        var level = runtime.LevelData!;
        bool found = false;
        for (int y = 0; y < level.HeightInBlocks && !found; y++)
        for (int x = 0; x < level.WidthInBlocks && !found; x++)
        {
            var block = level.GetCollisionBlock(x, y);
            if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
            var door = level.ResolveDoorCollision(bus, block.Behavior, 1, false);
            if (door.DestinationRoomPointer != RidleyMovieMemory.RidleyRoom) continue;
            level.ResolveDoorCollision(bus, block.Behavior, 1, true);
            found = true;
        }
        AssertTrue(found, "native Ridley entry door");
        // Original movie: accepted main-loop samples 155, 156, 157. The source
        // acid callback was installed long before the recorded door collision.
        runtime.RoomLayer3Fx.AdvanceHdmaSharedState(runtime.System, false);
        runtime.System.SetRandomNumber(0xd562);
        typeof(SuperMetroidRuntime).GetProperty(nameof(runtime.NmiFrameCounter))!.SetValue(runtime, (ushort)0xa4df);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.XPosition = 20; samus.Kinematics.XSubposition = 0x2000;
        samus.YPosition = 111; samus.Kinematics.YSubposition = 0x5bff;
        uint xFixed = samus.Kinematics.XFixed, yFixed = samus.Kinematics.YFixed;
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime.ActiveRoom!.State.Pointer);
        typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, SuperMetroidGameState.HitDoorBlock);
        game.Step(0);
        AssertEqual((ushort)0xef3a, runtime.System.RandomNumber, "native entry HDMA swap then RNG");
        AssertEqual((ushort)0xa4e0, runtime.NmiFrameCounter, "entry accepts exactly one NMI");
        AssertEqual(xFixed, samus.Kinematics.XFixed, "entry keeps Samus X fixed");
        AssertEqual(yFixed, samus.Kinematics.YFixed, "entry keeps Samus Y fixed");
        game.Step(0);
        AssertEqual((ushort)0x27bc, runtime.System.RandomNumber, "native sound-wait HDMA swap then RNG");
        AssertEqual((ushort)0xa4e1, runtime.NmiFrameCounter, "sound wait accepts exactly one NMI");
        AssertEqual(DoorTransitionPhase.FadeOutSourcePalette, game.DoorTransitionPhaseForVerification, "source fade begins after sound drain");
        var actor = runtime.Enemies.Slots[6];
        AssertEqual(PipeBugDefinitions.StrongBrinstarEnemyDefinition, actor.EnemyDefinitionPointer, "native source-room pipe bug");
        actor.CurrentInstruction = RidleyMovieMemory.PipeBugBeforeFadeInstruction;
        actor.InstructionTimer = 1;
        actor.SpritemapPointer = RidleyMovieMemory.PipeBugBeforeFadeSpritemap;
        game.Step(0);
        AssertEqual((ushort)0xadd4, runtime.System.RandomNumber, "first source fade advances native HDMA/RNG");
        AssertEqual((ushort)0xa4e2, runtime.NmiFrameCounter, "fade accepts exactly one NMI per update");
        AssertEqual(RidleyMovieMemory.PipeBugAfterFadeInstruction, actor.CurrentInstruction, "fade advances the native enemy instruction");
        AssertEqual(RidleyMovieMemory.PipeBugAfterFadeSpritemap, actor.SpritemapPointer, "fade changes to the native enemy sprite");
        AssertEqual((ushort)2, actor.InstructionTimer, "fade installs the native visual duration");
        int fadeSteps = 0;
        while (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.FadeOutSourcePalette && fadeSteps++ < 32)
            game.Step(0);
        AssertEqual(DoorTransitionPhase.LoadDoorHeader, game.DoorTransitionPhaseForVerification, "source palette fade finishes");
        AssertEqual((ushort)0xe19e, runtime.System.RandomNumber, "native update 172 fade endpoint RNG");
        // Original native input-boundary records 173..178. The first dispatch
        // still runs source HDMA; LoadDoorHeader disables it for the following ones.
        ushort[] nativeLoadingRandom = [0x1b76, 0x8a5f, 0xb4ec, 0x89ad, 0xb172, 0x784b];
        for (int index = 0; index < nativeLoadingRandom.Length; index++)
        {
            game.Step(0);
            AssertEqual(nativeLoadingRandom[index], runtime.System.RandomNumber, $"native loading RNG update {173 + index}");
            AssertEqual((ushort)(0xa4f1 + index), runtime.NmiFrameCounter, "loading accepts one NMI per update");
            if (index == 5)
            {
                AssertEqual(0x010e9000u, samus.Kinematics.XFixed, "native placement rebases and advances Samus while loading tiles");
                AssertEqual((ushort)0x00f8, runtime.Camera!.XPosition, "first loading IRQ moves camera four pixels");
            }
            if (index == 4)
            {
                AssertEqual(0x00135800u, samus.Kinematics.XFixed, "native scrolling setup moves Samus before destination placement");
                AssertEqual(yFixed, samus.Kinematics.YFixed, "left scrolling setup preserves perpendicular coordinate");
                AssertEqual((ushort)0x00fc, runtime.Camera!.XPosition, "native setup camera origin");
            }
        }
        game.Step(0); // The atomic destination loader must use the pre-setup source.
        AssertEqual(DoorTransitionPhase.WaitForDoorOpeningScroll, game.DoorTransitionPhaseForVerification, "loaded destination owns the opening trajectory");
        AssertEqual(0x010dc800u, samus.Kinematics.XFixed, "destination load carries both loading IRQ steps without restarting the scroll");
        int scrollCalls = 0;
        while (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.WaitForDoorOpeningScroll && scrollCalls < 64)
        {
            game.Step(0);
            scrollCalls++;
        }
        AssertEqual(61, scrollCalls, "left trajectory completes on its 61st remaining IRQ call");
        AssertEqual(DoorTransitionPhase.FinishDoorLoading, game.DoorTransitionPhaseForVerification, "post-scroll NMI remains inside the loading coroutine");
        AssertEqual(0x00de2000u, samus.Kinematics.XFixed, "native source frame 276 scrolling endpoint");
        game.Step(0);
        AssertEqual(DoorTransitionPhase.HandleAnimatedTiles, game.DoorTransitionPhaseForVerification, "loading alignment precedes music wait");
        AssertEqual((ushort)0x5a88, runtime.System.RandomNumber, "loading continuation does not advance outer RNG");
        AssertEqual(0x00d82000u, samus.Kinematics.XFixed, "native final doorway alignment preserves original subposition");
        AssertEqual(yFixed, samus.Kinematics.YFixed, "left door endpoint preserves perpendicular coordinate");
        game.Step(0);
        AssertEqual((ushort)0xc5b9, runtime.System.RandomNumber, "native animated-tile outer dispatch RNG");
        game.Step(0);
        AssertEqual((ushort)0xa1ea, runtime.System.RandomNumber, "native music-wait outer dispatch RNG");
        int musicWaits = 0;
        while (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.WaitForMusicQueue && musicWaits++ < 32)
            game.Step(0);
        AssertEqual(DoorTransitionPhase.HandleTransition, game.DoorTransitionPhaseForVerification, "native music queue reaches final door dispatch");
        ushort nmiBeforeNudge = runtime.NmiFrameCounter;
        ushort samusAnimation = samus.AnimationFrame;
        var ridley = runtime.Enemies.Slots[0];
        ushort bodyInstruction = ridley.CurrentInstruction;
        game.Step(0);
        AssertEqual(DoorTransitionPhase.BuildDestinationOam, game.DoorTransitionPhaseForVerification, "final nudge returns before first fade");
        AssertEqual(unchecked((ushort)(nmiBeforeNudge + 1)), runtime.NmiFrameCounter, "final transition accepts exactly one NMI");
        AssertEqual(samusAnimation, samus.AnimationFrame, "final transition does not animate Samus");
        AssertEqual(bodyInstruction, ridley.CurrentInstruction, "final transition does not advance destination enemy instructions");
        game.Step(0);
        AssertEqual(unchecked((ushort)(nmiBeforeNudge + 2)), runtime.NmiFrameCounter, "first fade accepts exactly one additional NMI");
        AssertEqual(samusAnimation, samus.AnimationFrame, "destination fade does not animate Samus");
        AssertEqual(RidleyMovieMemory.RidleyFirstFadeInstruction, ridley.CurrentInstruction, "first fade runs native Ridley instruction list");
        AssertEqual(RidleyMovieMemory.RidleyFirstFadeSpritemap, ridley.SpritemapPointer, "first fade publishes native Ridley sprite");
        AssertEqual((ushort)12, ridley.InstructionTimer, "first native fade visual duration");
        AssertEqual(RidleyAiFunction.WaitForDoorTransition, runtime.Enemies.Ridley!.Function, "native Ridley AI remains gated during fade visuals");
        AssertEqual((ushort)0, runtime.Enemies.Ridley.FunctionTimer, "fade does not consume the reveal countdown");
        game.Step(0);
        AssertEqual((ushort)11, ridley.InstructionTimer, "later fade updates continue enemy animation");
        int remainingFade = 0;
        while (game.GameState == SuperMetroidGameState.LoadingNextRoomB && remainingFade++ < 32)
        {
            game.Step(0);
            AssertEqual(RidleyAiFunction.WaitForDoorTransition, runtime.Enemies.Ridley.Function, "reveal timer remains gated through the final fade dispatch");
        }
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "fade releases ordinary gameplay");
        AssertTrue(!runtime.Enemies.EnemyDoorTransitionActive, "completed fade clears enemy gate");
        game.Step(0);
        AssertEqual(RidleyAiFunction.InitialDelay, runtime.Enemies.Ridley.Function, "first gameplay update begins reveal");
        AssertEqual((ushort)169, runtime.Enemies.Ridley.FunctionTimer, "native first gameplay update consumes one of 170 reveal ticks");
        Console.WriteLine("Ridley door entry/loading/fade: native positions, RNG, one NMI per dispatch, stationary Samus animation and continuing enemy visuals agree.");
    }

    private static void VerifyRidleyFullMovie(string directory)
    {
        byte[] movie = File.ReadAllBytes("csharp/test-fixtures/issue-1266-ridley/Ridley fight showcase.smv");
        AssertTrue(Convert.ToHexString(SHA256.HashData(movie)) == "7E12861DC56C5ABED12C2BFA2B00D24BFA418F49F2CE4C027D930CE9A3663F66", "original Ridley movie hash");
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "updates.json")));
        var root = manifest.RootElement;
        AssertEqual("super-metroid-gameplay-updates-v3", root.GetProperty("format").GetString()!, "converted replay format");
        AssertEqual(Convert.ToHexString(SHA256.HashData(movie)), root.GetProperty("movieSha256").GetString()!, "converted movie identity");
        AssertEqual(10890, root.GetProperty("sourceFrameCount").GetInt32(), "complete original movie coverage");
        var updates = root.GetProperty("updates").EnumerateArray().ToArray();
        int length = root.GetProperty("updateCount").GetInt32();
        AssertEqual(length, updates.Length, "converted update count");
        using var file = File.OpenRead(Path.Combine(directory, "update-boundaries.wram.gz"));
        AssertEqual(Convert.ToHexString(SHA256.HashData(file)), root.GetProperty("checkpointsSha256").GetString()!, "native checkpoint identity");
        file.Position = 0;
        using var trace = new GZipStream(file, CompressionMode.Decompress);
        var record = new byte[131080];
        int lastNativeRecord = -1;
        byte[] ReadFrame(int update)
        {
            int wantedRecord = update == 0 ? 0 : updates[update - 1].GetProperty("expectedRecord").GetInt32();
            AssertTrue(wantedRecord > lastNativeRecord, "native checkpoint order remains forward-only");
            while (lastNativeRecord < wantedRecord)
            {
                trace.ReadExactly(record);
                lastNativeRecord++;
            }
            int expectedFrame = update < length ? updates[update].GetProperty("sourceFrame").GetInt32() : 10890;
            AssertEqual(expectedFrame, BinaryPrimitives.ReadInt32LittleEndian(record), "native input-boundary frame");
            AssertEqual(update < length ? RidleyMovieMemory.ReadControllerInput : 0,
                BinaryPrimitives.ReadInt32LittleEndian(record.AsSpan(4)), "native accepted-input boundary or terminal");
            return record.AsSpan(8).ToArray();
        }
        byte[] memory = ReadFrame(0);
        ushort W(int address) => BinaryPrimitives.ReadUInt16LittleEndian(memory.AsSpan(address));
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.System.LoadCollectedItemBytes(memory.AsSpan(RidleyMovieMemory.CollectedItemBits, Bank80SystemState.ItemBitByteCount));
        runtime.System.LoadBossBytes(memory.AsSpan(RidleyMovieMemory.BossBits, Bank80SystemState.AreaCount));
        runtime.System.LoadEventBytes(memory.AsSpan(RidleyMovieMemory.Events, Bank80SystemState.EventByteCount));
        runtime.System.LoadOpenedDoorBytes(memory.AsSpan(RidleyMovieMemory.OpenedDoors, Bank80SystemState.DoorBitByteCount));
        runtime.LoadCartridgeRoomForDebug(W(RidleyMovieMemory.Room), W(RidleyMovieMemory.CameraX), W(RidleyMovieMemory.CameraY));
        // Preserve the native room's already-mutated doors and item blocks. Rebuilding
        // these from the pristine room header would no longer represent this movie frame.
        RoomLevelData level = runtime.LevelData ?? throw new InvalidDataException(
            "The native Ridley checkpoint did not load room collision data.");
        for (int index = 0; index < level.WidthInBlocks * level.HeightInBlocks; index++)
        {
            level.SetForegroundEntry(index, W(RidleyMovieMemory.Level + index * sizeof(ushort)));
            level.SetBehavior(index, memory[RidleyMovieMemory.Bts + index]);
        }

        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            "The native Ridley checkpoint did not load Samus.");
        samus.InputLocked = false;
        samus.EquippedItems = W(RidleyMovieMemory.Items);
        samus.EquippedBeams = W(RidleyMovieMemory.Beams);
        samus.Health = W(RidleyMovieMemory.Health);
        samus.MaxHealth = W(RidleyMovieMemory.MaxHealth);
        samus.Pose = (byte)W(RidleyMovieMemory.Pose);
        samus.XPosition = W(RidleyMovieMemory.X);
        samus.YPosition = W(RidleyMovieMemory.Y);
        samus.Kinematics.XSubposition = W(RidleyMovieMemory.XFraction);
        samus.Kinematics.YSubposition = W(RidleyMovieMemory.YFraction);
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.SetAnimationFrameFromSpecialHandler(W(RidleyMovieMemory.Animation), W(RidleyMovieMemory.AnimationTimer));
        samus.PoseHistory.PreviousPose = W(RidleyMovieMemory.PreviousPose);
        samus.PoseHistory.PreviousDirectionAndMovement = W(RidleyMovieMemory.PreviousDirection);
        samus.PoseHistory.LastDifferentPose = W(RidleyMovieMemory.LastDifferentPose);
        samus.PoseHistory.LastDifferentDirectionAndMovement = W(RidleyMovieMemory.LastDifferentDirection);
        samus.HorizontalSpeed.BaseSpeed = W(RidleyMovieMemory.BaseSpeed);
        samus.HorizontalSpeed.BaseSubspeed = W(RidleyMovieMemory.BaseFraction);
        samus.HorizontalSpeed.ExtraRunSpeed = W(RidleyMovieMemory.ExtraSpeed);
        samus.HorizontalSpeed.ExtraRunSubspeed = W(RidleyMovieMemory.ExtraFraction);
        samus.HorizontalSpeed.AccelerationMode = W(RidleyMovieMemory.AccelerationMode);
        samus.HorizontalSpeed.HasRunningMomentum = W(RidleyMovieMemory.Momentum) != 0;
        samus.HorizontalSpeed.SpeedBoostCounter = W(RidleyMovieMemory.BoostCounter);
        samus.Kinematics.YSpeed = W(RidleyMovieMemory.VerticalSpeed);
        samus.Kinematics.YSubspeed = W(RidleyMovieMemory.VerticalFraction);
        samus.Kinematics.YDirection = W(RidleyMovieMemory.VerticalDirection);

        // The snapshot was recorded after the entering door PLM deleted itself.
        // Restore the empty physical pool rather than executing fresh room-entry actors.
        var initialPlms = (Array)typeof(RoomPlmSystem).GetField("_slots", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(runtime.Plms)!;
        for (int index = 0; index < initialPlms.Length; index++)
        {
            AssertTrue(W(RidleyMovieMemory.PlmHeaders + index * 2) == 0, "native initial PLM pool is empty");
            object slot = initialPlms.GetValue(index)!;
            slot.GetType().GetProperty("Active")!.SetValue(slot, false);
        }
        // This is a one-time initial snapshot import. No native state is fed back during replay.
        runtime.System.SetRandomNumber(W(RidleyMovieMemory.Random));
        // Native frame zero already has the acid BG3 callback installed at $18F0.
        AssertTrue(W(RidleyMovieMemory.AcidHdmaPreInstruction) == RidleyMovieMemory.AcidHdmaCallback, "initial native acid HDMA callback");
        typeof(RoomLayer3FxState).GetField("lavaAcidBg3PreInstructionInstalled", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(runtime.RoomLayer3Fx, true);
        string[] slotWords = ["EnemyDefinitionPointer", "XPosition", "XSubposition", "YPosition", "YSubposition", "XRadius", "YRadius", "Properties", "ExtraProperties", "AiHandlerBits", "Health", "SpritemapPointer", "Timer", "CurrentInstruction", "InstructionTimer", "PaletteIndex", "VramTilesIndex", "Layer", "FlashTimer", "FrozenTimer", "InvincibilityTimer", "ShakeTimer", "FrameCounter"];
        for (int index = 0; index < runtime.Enemies.Slots.Count; index++)
        {
            var slot = runtime.Enemies.Slots[index];
            int address = RidleyMovieMemory.EnemyBase + index * 64;
            if (W(address) != 0)
                AssertTrue(slot.EnemyDefinitionPointer == W(address), "initial enemy species agrees with room population");
            for (int word = 0; word < slotWords.Length; word++)
                typeof(RoomEnemySlot).GetProperty(slotWords[word])!.SetValue(slot, W(address + word * 2));
            for (int word = 0; word < 6; word++)
                typeof(RoomEnemySlot).GetProperty("Variable" + (char)('A' + word))!.SetValue(slot, W(address + 48 + word * 2));
            if (runtime.Enemies.PipeBugStates[index] is { IsBrinstar: true } pipe)
            {
                pipe.SpawnX = slot.VariableB; pipe.SpawnY = slot.VariableC;
                pipe.DelayOrCounter = slot.VariableD;
                pipe.AnimationState = (PipeBugAnimationSelector)slot.VariableE;
                pipe.EmergenceTopY = W(RidleyMovieMemory.EnemyExtra + index * 64);
                pipe.InstalledAnimationState = (PipeBugAnimationSelector)W(RidleyMovieMemory.EnemyExtraPreviousAnimation + index * 64);
            }
        }
        typeof(SuperMetroidRuntime).GetProperty(nameof(runtime.NmiFrameCounter))!.SetValue(runtime, W(RidleyMovieMemory.NmiCounter));
        typeof(SuperMetroidRuntime).GetProperty(nameof(runtime.NmiFrameCounter8))!.SetValue(runtime, memory[RidleyMovieMemory.NmiCounterByte]);
        samus.CollectedItems = W(RidleyMovieMemory.CollectedItems); samus.CollectedBeams = W(RidleyMovieMemory.CollectedBeams);
        samus.Missiles = W(RidleyMovieMemory.Missiles); samus.MaxMissiles = W(RidleyMovieMemory.MaxMissiles);
        samus.SuperMissiles = W(RidleyMovieMemory.SuperMissiles); samus.MaxSuperMissiles = W(RidleyMovieMemory.MaxSuperMissiles);
        samus.PowerBombs = W(RidleyMovieMemory.PowerBombs); samus.MaxPowerBombs = W(RidleyMovieMemory.MaxPowerBombs);
        samus.PreviousHealthForHurtCheck = samus.Health;
        runtime.Controller1.Latch(W(RidleyMovieMemory.HeldInput));
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime.ActiveRoom!.State.Pointer);
        typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, (SuperMetroidGameState)W(RidleyMovieMemory.GameState));
        var audio = new CartridgeAudioRenderer(runtimeFixtureInstallation.Value.LoadAudio());
        ushort[]? pendingLoadedOwners = null;
        int loadingIntervals = 0;
        ushort[] CaptureLoadedOwners()
        {
            var words = new List<ushort> { runtime.System.RandomNumber };
            foreach (var actor in runtime.Enemies.Slots)
                words.AddRange([actor.EnemyDefinitionPointer, actor.XPosition, actor.XSubposition,
                    actor.YPosition, actor.YSubposition, actor.Health, actor.SpritemapPointer,
                    actor.CurrentInstruction, actor.InstructionTimer]);
            return words.ToArray();
        }
        for (int frame = 0; frame <= length; frame++)
        {
            if (frame != 0) memory = ReadFrame(frame);
            var mismatches = new List<string>();
            void Check(string name, ushort actual, int address)
            {
                ushort expected = W(address);
                if (actual != expected) mismatches.Add($"{name}: native={expected:X4} port={actual:X4}");
            }
            // Only the reference's proven hardware-upload NMI count is normalized;
            // gameplay state is never copied back into the production runtime.
            int excludedNmis = frame == 0 ? 0 : updates[frame - 1].GetProperty("excludedNmiAfter").GetInt32();
            ushort normalizedNmi = unchecked((ushort)(W(RidleyMovieMemory.NmiCounter) - excludedNmis));
            if (runtime.NmiFrameCounter != normalizedNmi)
                mismatches.Add($"Accepted gameplay NMI: native={normalizedNmi:X4} port={runtime.NmiFrameCounter:X4}");
            Check("Game state", (ushort)game.GameState, RidleyMovieMemory.GameState);
            Check("Enemy door gate", runtime.Enemies.EnemyDoorTransitionActive ? (ushort)1 : (ushort)0, RidleyMovieMemory.EnemyDoorTransition);
            // Native LoadDoorHeader publishes the destination room pointer before
            // loading its room/state data. The port keeps that identity in the pending
            // door while ActiveRoom still owns the source room's loaded data.
            ushort selectedRoom = game.DoorTransitionPhaseForVerification is
                DoorTransitionPhase.AlignSourceCamera or DoorTransitionPhase.FixDoorsMovingUp or
                DoorTransitionPhase.SetupNewRoom or DoorTransitionPhase.SetupScrolling or
                DoorTransitionPhase.PlaceSamusAndLoadTiles or DoorTransitionPhase.LoadMoreThingsAndOpenDoor
                ? (runtime.PendingDoorTransition ?? throw new InvalidDataException("Missing selected destination door")).DestinationRoomPointer
                : runtime.ActiveRoom!.Pointer;
            Check("Selected room", selectedRoom, RidleyMovieMemory.Room);
            Check("Camera X", runtime.Camera!.XPosition, RidleyMovieMemory.CameraX);
            Check("Camera Y", runtime.Camera.YPosition, RidleyMovieMemory.CameraY);
            Check("Samus X", samus.XPosition, RidleyMovieMemory.X);
            Check("Samus X fraction", samus.Kinematics.XSubposition, RidleyMovieMemory.XFraction);
            Check("Samus Y", samus.YPosition, RidleyMovieMemory.Y);
            Check("Samus Y fraction", samus.Kinematics.YSubposition, RidleyMovieMemory.YFraction);
            Check("Samus pose", samus.Pose, RidleyMovieMemory.Pose);
            Check("Samus animation", samus.AnimationFrame, RidleyMovieMemory.Animation);
            Check("Samus animation timer", samus.AnimationFrameTimer, RidleyMovieMemory.AnimationTimer);
            Check("Samus base speed", samus.HorizontalSpeed.BaseSpeed, RidleyMovieMemory.BaseSpeed);
            Check("Samus base fraction", samus.HorizontalSpeed.BaseSubspeed, RidleyMovieMemory.BaseFraction);
            Check("Samus extra speed", samus.HorizontalSpeed.ExtraRunSpeed, RidleyMovieMemory.ExtraSpeed);
            Check("Samus extra fraction", samus.HorizontalSpeed.ExtraRunSubspeed, RidleyMovieMemory.ExtraFraction);
            Check("Samus vertical speed", samus.Kinematics.YSpeed, RidleyMovieMemory.VerticalSpeed);
            Check("Samus vertical fraction", samus.Kinematics.YSubspeed, RidleyMovieMemory.VerticalFraction);
            Check("Samus vertical direction", samus.Kinematics.YDirection, RidleyMovieMemory.VerticalDirection);
            Check("Samus health", samus.Health, RidleyMovieMemory.Health);
            // The native CPU can still be decompressing source-room tiles while
            // IRQ scrolling advances; the port loads the destination atomically.
            // Align these owners at completed loading, not elapsed upload time.
            // Movement/input remain compared on EVERY IRQ interval. A stable owner
            // snapshot also proves the port does not run destination actors early.
            bool nativeLoading = game.GameState == SuperMetroidGameState.LoadingNextRoomB &&
                W(RidleyMovieMemory.DoorFunction) is RidleyMovieMemory.PlaceSamusLoadTiles or RidleyMovieMemory.LoadMoreThings;
            bool portWaitingForScroll = game.DoorTransitionPhaseForVerification is
                DoorTransitionPhase.WaitForDoorOpeningScroll or DoorTransitionPhase.FinishDoorLoading;
            bool deferLoadingOwners = nativeLoading && portWaitingForScroll;
            if (deferLoadingOwners)
            {
                ushort[] owners = CaptureLoadedOwners();
                if (pendingLoadedOwners is not null && !owners.AsSpan().SequenceEqual(pendingLoadedOwners))
                    throw new InvalidDataException("Destination RNG/enemy owners advanced while native hardware loading was still pending.");
                pendingLoadedOwners ??= owners;
                loadingIntervals++;
            }
            else
            {
                if (pendingLoadedOwners is not null)
                    AssertEqual(RidleyMovieMemory.HandleAnimTiles, W(RidleyMovieMemory.DoorFunction), "deferred destination owners reach native completed-loading boundary");
                Check("RNG", runtime.System.RandomNumber, RidleyMovieMemory.Random);
            }
            foreach (var actor in runtime.Enemies.Slots)
            {
                if (deferLoadingOwners) continue;
                int address = RidleyMovieMemory.EnemyBase + actor.NativeIndex;
                string owner = $"Enemy {actor.SlotIndex}";
                Check(owner + " identity", actor.EnemyDefinitionPointer, address);
                if (actor.EnemyDefinitionPointer == 0 || W(address) == 0) continue;
                Check(owner + " X", actor.XPosition, address + 2);
                Check(owner + " X fraction", actor.XSubposition, address + 4);
                Check(owner + " Y", actor.YPosition, address + 6);
                Check(owner + " Y fraction", actor.YSubposition, address + 8);
                Check(owner + " health", actor.Health, address + 20);
                Check(owner + " spritemap", actor.SpritemapPointer, address + 22);
                Check(owner + " instruction", actor.CurrentInstruction, address + 26);
                Check(owner + " instruction timer", actor.InstructionTimer, address + 28);
            }
            if (!deferLoadingOwners && runtime.Enemies.Ridley is { } ridleyState &&
                W(RidleyMovieMemory.EnemyBase) == RoomEnemySystem.NorfairRidleyDefinition)
            {
                Check("Ridley AI function", (ushort)ridleyState.Function, RidleyMovieMemory.RidleyFunction);
                Check("Ridley AI timer", ridleyState.FunctionTimer, RidleyMovieMemory.RidleyFunctionTimer);
                if (game.GameState == SuperMetroidGameState.MainGameplay)
                {
                    // Tail workspace becomes live after its first fade-owned composition.
                    Check("Ridley tail tip X", ridleyState.TailSegments[6].XPosition, RidleyMovieMemory.TailTipX);
                    Check("Ridley tail tip Y", ridleyState.TailSegments[6].YPosition, RidleyMovieMemory.TailTipY);
                }
            }
            if (mismatches.Count != 0)
            {
                Console.Error.WriteLine($"Room width={level.WidthInBlocks}, Samus radius={samus.Kinematics.XRadius}/{samus.Kinematics.YRadius}, speed={samus.HorizontalSpeed.BaseSpeed:X4}.{samus.HorizontalSpeed.BaseSubspeed:X4}+{samus.HorizontalSpeed.ExtraRunSpeed:X4}.{samus.HorizontalSpeed.ExtraRunSubspeed:X4}");
                for (int block = 0; block < level.WidthInBlocks * level.HeightInBlocks; block++)
                {
                    ushort expectedBlock = W(RidleyMovieMemory.Level + block * 2);
                    ushort actualBlock = level.ForegroundEntries.Span[block];
                    if (expectedBlock != actualBlock) Console.Error.WriteLine($"Block {block} ({block % level.WidthInBlocks},{block / level.WidthInBlocks}): native={expectedBlock:X4} port={actualBlock:X4}");
                }
                throw new InvalidDataException($"Full movie first divergence at update {frame} (SMV source frame {(frame == 0 ? 0 : updates[frame - 1].GetProperty("sourceFrame").GetInt32())}): " + string.Join("; ", mismatches));
            }
            if (!deferLoadingOwners && pendingLoadedOwners is not null)
            {
                Console.WriteLine($"Completed-loading RNG/enemy owners match after {loadingIntervals} IRQ intervals; movement/input checked throughout.");
                pendingLoadedOwners = null;
            }
            // Only the converted controller event enters production; reference memory is
            // read-only. An accepted input read during APU transfer is not another
            // gameplay update. Until its latch/counter effects are normalized, stop
            // explicitly instead of replaying hardware upload time as gameplay.
            if (frame < length)
            {
                if (updates[frame].GetProperty("timingClass").GetString() == "apu-upload-continuation")
                    throw new InvalidDataException($"SMV source frame {updates[frame].GetProperty("sourceFrame").GetInt32()} is an APU upload continuation; hardware-wait input normalization is not implemented.");
                var output = game.Step((ushort)updates[frame].GetProperty("input").GetInt32());
                audio.RenderFrame(output.AudioCommands);
                game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
            }
        }
        AssertTrue(pendingLoadedOwners is null, "no deferred loading comparison remains at movie end");
        AssertTrue(trace.ReadByte() == -1, "trace ends after movie terminal frame");
        Console.WriteLine($"Full movie input replay: {length} updates across all 10890 source frames match the currently instrumented fields.");
    }
}
