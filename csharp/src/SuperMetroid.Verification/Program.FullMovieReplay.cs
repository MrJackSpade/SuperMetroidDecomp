using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Repository fixture path for the power-on 100-percent playthrough recording replayed by verification.</summary>
    private const string FullPlaythroughMoviePath = "csharp/test-fixtures/full-100-percent/Super Metroid 100%.smv";
    /// <summary>SHA-256 digest used to ensure the 100-percent replay fixture has not changed.</summary>
    private const string FullPlaythroughMovieSha256 = "4D0E6E671E11BD99439AE498210943711135E6CBE8F57D97B2C56815315AE07E";
    /// <summary>Repository fixture path for the 13-percent Speed Booster lsnes TAS replayed by verification.</summary>
    private const string LowPercentPlaythroughMoviePath = "csharp/test-fixtures/low-13-percent/13_speedbooster.lsmv";
    /// <summary>SHA-256 digest used to ensure the 13-percent replay fixture has not changed.</summary>
    private const string LowPercentPlaythroughMovieSha256 = "1D217BB073309AE44EA3D972C5EC3201BE0EC434C60F034E1E958F94FD1BCDA8";

    /// <summary>Loads the pinned 100-percent movie and replays its converted inputs through the production game.</summary>
    /// <param name="traceDirectory">The native input-consumption timeline and read-only WRAM checkpoints for this recording.</param>
    /// <param name="traceFromUpdate">Optional update index from which diagnostic Samus state is printed.</param>
    private static void VerifyFullPlaythroughMovie(string traceDirectory, int? traceFromUpdate = null) =>
        VerifyPlaythroughMovie(
            ReplayMovie.Load("100%", FullPlaythroughMoviePath, FullPlaythroughMovieSha256),
            traceDirectory, traceFromUpdate);

    /// <summary>The 13% Speed Booster lsnes TAS, captured on bsnes v085.</summary>
    private static void VerifyLowPercentPlaythroughMovie(string traceDirectory, int? traceFromUpdate = null) =>
        VerifyPlaythroughMovie(
            ReplayMovie.Load("13%", LowPercentPlaythroughMoviePath, LowPercentPlaythroughMovieSha256),
            traceDirectory, traceFromUpdate);

    /// <summary>
    /// Replays a supplied power-on movie through production frontend and gameplay
    /// code. The only imported state is the movie's own power-on SRAM; every later update
    /// receives only its converted controller word, and native WRAM is read-only evidence.
    /// </summary>
    /// <param name="movie">The recorded movie whose power-on SRAM and controller timeline are replayed.</param>
    /// <param name="traceDirectory">Directory containing the native input-consumption timeline and read-only WRAM checkpoints for the supplied movie.</param>
    /// <param name="traceFromUpdate">
    /// Diagnostic only: from this update on, print port and native Samus kinematics so a
    /// divergence can be read field by field. Comparison and failure are unchanged.
    /// </param>
    private static void VerifyPlaythroughMovie(ReplayMovie movie, string traceDirectory, int? traceFromUpdate)
    {
        using var checkpoints = NativeMovieCheckpoints.Open(traceDirectory, movie.Bytes);
        var updates = checkpoints.Updates;

        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        movie.PowerOnSaveRam.CopyTo(bus.SaveRam);
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        var audio = new CartridgeAudioRenderer(RepositoryInstallation.Installation.LoadAudio());

        byte[] memory = checkpoints.ReadAfter(0);
        Console.WriteLine($"Native first input boundary: source frame {updates[0].SourceFrame}, " +
            $"state {Word(memory, MovieDesyncMemory.GameState):X2}, RNG {Word(memory, MovieDesyncMemory.Random):X4}.");
        ushort[] frameInputs = movie.FrameInputs;
        var recentInputs = new Queue<string>();
        var uploadNmis = new EvidencedDoorMusicUploadNmis();
        game.DoorMusicUploadNmis = uploadNmis;
        var loaderProgress = new EvidencedDoorLoaderProgress();
        game.DoorLoaderProgress = loaderProgress;
        var soundAcknowledgements = new EvidencedSoundAcknowledgements(memory);
        CartridgeAudioAcknowledgements spcAcknowledgements = audio.ReadAcknowledgements();
        int excludedBefore = 0;
        for (int update = 1; update <= updates.Count; update++)
        {
            ConvertedMovieUpdate step = updates[update - 1];
            // Upload-wait NMIs that follow this update's input are accepted inside its dispatch.
            uploadNmis.Expect(update, step.ExcludedNmiAfter - excludedBefore);
            excludedBefore = step.ExcludedNmiAfter;
            loaderProgress.Expect(step.DoorLoaderCompletedEnemySlots);
            if (update >= traceFromUpdate && game.RuntimeForVerification is { } tracedRuntime)
                tracedRuntime.System.RandomCallObserver = () => Console.WriteLine(
                    "  rng call: " + string.Join(" <- ", new System.Diagnostics.StackTrace(2)
                        .GetFrames().Take(4).Select(frame => frame.GetMethod()?.Name)));
            // The last controller read of an eliminated music wait is still input the
            // cartridge consumed: it decides whether this update sees a new press.
            if (step.HardwareWaitLatch is { } latched)
                game.AcceptDoorMusicWaitControllerRead(latched);
            memory = checkpoints.ReadAfter(update);
            game.SetAudioAcknowledgements(soundAcknowledgements.ForUpdate(spcAcknowledgements, memory));
            // A confirmation box can continue from the previous dispatch into this
            // NMI continuation; its MessageBox_Routine entry belongs to that dispatch.
            bool boxContinues = game.RuntimeForVerification?.MessageBox.IsActive == true;
            var output = game.Step(step.Input);
            uploadNmis.AssertSettled();
            audio.RenderFrame(output.AudioCommands);
            spcAcknowledgements = audio.ReadAcknowledgements();
            // Bank $85's message box runs inside this dispatch on lag frames, polling the
            // joypad registers rather than accepting NMIs, so the box spans this update's
            // remaining source frames. The port shows it one frame per step.
            if (game.RuntimeForVerification?.MessageBox.IsActive == true)
            {
                int endSourceFrame = update < updates.Count ? updates[update].SourceFrame : checkpoints.SourceFrameCount;
                // The capture records MessageBox_Routine's entry frame. When the dispatch's
                // gameplay overran its own frame first, those lag frames read no controller;
                // the box's own polling starts that many frames after the dispatch's input.
                if (boxContinues && step.MessageBoxStartFrame is not null)
                    throw new InvalidDataException(
                        $"Native dispatch {update} opened a new message box while the port's box was still open.");
                int boxStartFrame = boxContinues ? step.SourceFrame : step.MessageBoxStartFrame ??
                    throw new InvalidDataException(
                        $"The port opened a message box in update {update}, but the native capture records " +
                        "no MessageBox_Routine entry in that dispatch.");
                int frame = boxStartFrame + 1;
                // MessageBox_Routine returns ($85:80BA) on the box's last frame. Its dispatch
                // can run on past it (a save writes SRAM into a lag frame); those frames are
                // the dispatch's hardware lag, not box frames, so the port steps none of them.
                int boxFrameLimit = step.MessageBoxEndFrame is { } nativeBoxEnd ? nativeBoxEnd + 1 : endSourceFrame;
                // The box's own HandleSounds calls span frames the capture does not split,
                // so they read the port's SPC model frame by frame.
                game.SetAudioAcknowledgements(spcAcknowledgements);
                for (; frame < boxFrameLimit && game.RuntimeForVerification!.MessageBox.IsActive; frame++)
                {
                    output = game.Step(frameInputs[frame]);
                    audio.RenderFrame(output.AudioCommands);
                    spcAcknowledgements = audio.ReadAcknowledgements();
                    game.SetAudioAcknowledgements(spcAcknowledgements);
                }
                if (step.MessageBoxEndFrame is { } nativeEnd)
                {
                    if (game.RuntimeForVerification!.MessageBox.IsActive || frame != nativeEnd + 1)
                        throw new InvalidDataException(
                            $"The port's message box closed after source frame {frame - 1}, but native dispatch {update} " +
                            $"(source frames {step.SourceFrame}-{endSourceFrame}) returned from it at {nativeEnd}.");
                }
                // A confirmation box reads the controller itself ($85:84BA), which the native
                // capture records as a following NMI-continuation update. The box then
                // legitimately stays open into that update and continues there.
                else if (!game.RuntimeForVerification!.MessageBox.IsActive || frame != endSourceFrame ||
                    update >= updates.Count || updates[update].Kind != "nmi-continuation")
                    throw new InvalidDataException(
                        $"The port's message box closed after source frame {frame - 1}, but native dispatch {update} " +
                        $"(source frames {step.SourceFrame}-{endSourceFrame}) keeps it open past {endSourceFrame - 1}.");
            }
            else if (step.MessageBoxStartFrame is { } nativeBoxFrame)
            {
                throw new InvalidDataException(
                    $"Native dispatch {update} opened a message box at source frame {nativeBoxFrame}; the port did not.");
            }
            else if (step.MessageBoxEndFrame is { } nativeBoxReturn)
            {
                throw new InvalidDataException(
                    $"Native dispatch {update} returned from a message box at source frame {nativeBoxReturn}; the port had none open.");
            }
            soundAcknowledgements.Capture(memory);

            if (update >= traceFromUpdate)
                TraceMovieSamus(game, memory, update, step);
            recentInputs.Enqueue($"{update}@{step.SourceFrame}:{step.Input:X4}/{step.Kind}");
            if (recentInputs.Count > 12) recentInputs.Dequeue();
            List<string> mismatches = CompareMovieDesyncState(game, memory);
            if (mismatches.Count != 0)
            {
                Console.Error.WriteLine("Recent converted inputs: " + string.Join(", ", recentInputs));
                throw new InvalidDataException(
                    $"{movie.Name} movie first divergence after update {update} (source frame {step.SourceFrame}, " +
                    $"native state {Word(memory, MovieDesyncMemory.GameState):X2}, port state {(ushort)game.GameState:X2}): " +
                    string.Join("; ", mismatches));
            }
            if (update % 10000 == 0)
                Console.WriteLine($"Update {update}/{updates.Count} (source frame {step.SourceFrame}) matches; state {(ushort)game.GameState:X2}.");
        }
        checkpoints.AssertExhausted();
        Console.WriteLine($"{movie.Name} movie replay: {updates.Count} updates across all {checkpoints.SourceFrameCount} source frames match.");
    }

    /// <summary>Prints port and native gameplay state for one replay update to help inspect a reported divergence.</summary>
    /// <param name="game">The production game instance immediately after processing the converted update.</param>
    /// <param name="memory">The read-only native WRAM checkpoint after that update.</param>
    /// <param name="update">The one-based update number associated with the trace line.</param>
    /// <param name="step">Input, source-frame, and continuation metadata for the converted update.</param>
    private static void TraceMovieSamus(SuperMetroidGame game, byte[] memory, int update, ConvertedMovieUpdate step)
    {
        var samus = game.RuntimeForVerification?.Samus;
        string Native(int address) => Word(memory, address).ToString("X4");
        Console.WriteLine($"trace {update}@{step.SourceFrame} in={step.Input:X4} state={(ushort)game.GameState:X2}/{Native(MovieDesyncMemory.GameState)} " +
            $"door={game.DoorTransitionPhaseForVerification}/{Native(0x099c)} rng={game.DispatcherRandomNumber:X4}/{Native(MovieDesyncMemory.Random)}");
        if (samus is null) return;
        Console.WriteLine($"  port   pose={samus.Pose:X2} X={samus.XPosition:X4}.{samus.Kinematics.XSubposition:X4} Y={samus.YPosition:X4}.{samus.Kinematics.YSubposition:X4} " +
            $"vs={samus.Kinematics.YSpeed:X4}.{samus.Kinematics.YSubspeed:X4} total={samus.HorizontalSpeed.TotalSpeed:X4}.{samus.HorizontalSpeed.TotalSubspeed:X4} slope={(samus.Kinematics.PositionAdjustedBySlope ? 1 : 0)} base={samus.HorizontalSpeed.BaseSpeed:X4}.{samus.HorizontalSpeed.BaseSubspeed:X4} extra={samus.HorizontalSpeed.ExtraRunSpeed:X4}.{samus.HorizontalSpeed.ExtraRunSubspeed:X4} accel={samus.HorizontalSpeed.AccelerationMode:X4}");
        Console.WriteLine($"  native pose={Native(MovieDesyncMemory.SamusPose)} X={Native(MovieDesyncMemory.SamusX)}.{Native(MovieDesyncMemory.SamusXFraction)} Y={Native(MovieDesyncMemory.SamusY)}.{Native(MovieDesyncMemory.SamusYFraction)} " +
            $"vs={Native(0x0b2e)}.{Native(0x0b2c)} total={Native(0x0dbc)}.{Native(0x0dbe)} slope={Native(0x0dba)} base={Native(0x0b46)}.{Native(0x0b48)} extra={Native(0x0b42)}.{Native(0x0b44)} accel={Native(0x0b4a)}");
        if (game.CeresDestructionForVerification is { } boom)
            Console.WriteLine($"  cinematic port={boom.Phase} native={Native(0x1f51)}");
        Console.WriteLine($"  nmi port={(game.RuntimeForVerification?.NmiFrameCounter ?? game.MenuNmiFrameCounterForVerification):X4} native={Native(0x05b6)}");
        Console.WriteLine("  aerial port  " + game.RuntimeForVerification?.LastAerialSamusMovement);
        if (game.RuntimeForVerification is { } projectileRuntime)
        {
            Console.WriteLine("  sproj port   " + string.Join(" ", projectileRuntime.Projectiles.Slots
                .Where(slot => slot.Type != 0)
                .Select(slot => $"{slot.SlotIndex}:{slot.Type:X4}@{slot.XPosition:X4}.{slot.XSubposition:X4}/{slot.YPosition:X4} r{slot.XRadius:X}/{slot.YRadius:X} v{slot.XVelocity:X4} d{slot.Direction:X}")));
            Console.WriteLine("  sproj native " + string.Join(" ", Enumerable.Range(0, 5)
                .Where(slot => Native(0x0c18 + 2 * slot) != "0000")
                .Select(slot => $"{slot}:{Native(0x0c18 + 2 * slot)}@{Native(0x0b64 + 2 * slot)}.{Native(0x0b8c + 2 * slot)}/{Native(0x0b78 + 2 * slot)} r{Native(0x0bb4 + 2 * slot)}/{Native(0x0bc8 + 2 * slot)} v{Native(0x0bf0 + 2 * slot)} d{Native(0x0c04 + 2 * slot)}")));
        }
        Console.WriteLine("  music port   " + game.AudioForVerification.MusicQueueForVerification());
        Console.WriteLine($"  music native {Word(memory, 0x063b) / 2:X}/{Word(memory, 0x0639) / 2:X} t={Word(memory, 0x063f):X} d=" +
            string.Join(",", Enumerable.Range(0, 8).Select(slot => Word(memory, 0x0629 + 2 * slot).ToString("X"))));
        string Queues(Func<int, (byte, byte, byte)> read) => string.Join(" ",
            Enumerable.Range(0, 3).Select(queue => read(queue)).Select(q => $"{q.Item1:X}/{q.Item2:X}/{q.Item3}"));
        Console.WriteLine("  sfx port   " + Queues(queue =>
        {
            var q = game.AudioForVerification.SoundQueueForVerification(queue);
            return (q.Start, q.Next, q.State);
        }));
        Console.WriteLine("  sfx native " + Queues(queue =>
            (memory[0x0643 + queue], memory[0x0646 + queue], memory[0x0649 + queue])));
        if (game.RuntimeForVerification?.Enemies is { } traceEnemies)
        {
            // $1997 IDs, $1A4B X, $1A93 Y: one word per slot.
            var portProjectiles = traceEnemies.EnemyProjectiles.Select((projectile, slot) => (projectile, slot))
                .Where(entry => entry.projectile.IsActive)
                .Select(entry => $"{entry.slot}:{(ushort)entry.projectile.Kind:X4}@{entry.projectile.XPosition:X}/{entry.projectile.YPosition:X}" +
                    $"#{entry.projectile.InstructionPointer:X4}/{entry.projectile.InstructionTimer:X}");
            var nativeProjectiles = Enumerable.Range(0, 18).Where(slot => Word(memory, 0x1997 + 2 * slot) != 0)
                .Select(slot => $"{slot}:{Native(0x1997 + 2 * slot)}@{Word(memory, 0x1a4b + 2 * slot):X}/{Word(memory, 0x1a93 + 2 * slot):X}" +
                    $"#{Word(memory, 0x1b47 + 2 * slot):X4}/{Word(memory, 0x1b8f + 2 * slot):X}");
            Console.WriteLine("  eproj port   " + string.Join(" ", portProjectiles));
            Console.WriteLine("  eproj native " + string.Join(" ", nativeProjectiles));
            Console.WriteLine($"  ceres port={traceEnemies.CeresStatus:X4} native={Native(0x093f)} " +
                $"nmi port={game.RuntimeForVerification!.NmiFrameCounter:X4} native-raw={Native(0x05b6)}");
            foreach (var actor in traceEnemies.Slots.Where(slot => slot.EnemyDefinitionPointer != 0))
            {
                // $0F86 properties, $0F92 instruction list pointer, $0F94 instruction timer.
                int slotBase = MovieDesyncMemory.EnemyBase + actor.NativeIndex;
                Console.WriteLine($"  enemy {actor.NativeIndex / MovieDesyncMemory.EnemyStride} {actor.EnemyDefinitionPointer:X4} " +
                    $"port inst={actor.CurrentInstruction:X4}/{actor.InstructionTimer:X4} prop={actor.Properties:X4} " +
                    $"native inst={Native(slotBase + 0x1a)}/{Native(slotBase + 0x1c)} prop={Native(slotBase + 0x0e)}");
            }
        }
        if (game.RuntimeForVerification?.Enemies.Ridley is { } ridley)
        {
            // $7E:2020: seven ten-word tail records; X/Y at +12/+14.
            string Port(int index) => $"{ridley.TailSegments[index].XPosition:X4}/{ridley.TailSegments[index].YPosition:X4}";
            string NativeTail(int index) => $"{Native(0x2020 + index * 20 + 12)}/{Native(0x2020 + index * 20 + 14)}";
            Console.WriteLine("  tail port   " + string.Join(" ", Enumerable.Range(0, 7).Select(Port)) + $" fn={(ushort)ridley.Function:X4} tailfn={ridley.TailFunctionIndex:X4} v={ridley.HorizontalVelocity:X4}/{ridley.VerticalVelocity:X4} getaway={ridley.Mode7TableByteIndex:X4}");
            Console.WriteLine("  tail native " + string.Join(" ", Enumerable.Range(0, 7).Select(NativeTail)) + $" fn={Native(0x0fa8)} tailfn={Native(0x2000)} v={Native(0x0faa)}/{Native(0x0fac)} getaway={Native(0x8026)}");
        }
    }

    /// <summary>Reads one little-endian 16-bit word from a captured native memory image.</summary>
    /// <param name="memory">The native memory snapshot containing the requested bytes.</param>
    /// <param name="address">The byte offset of the word in the snapshot.</param>
    /// <returns>The unsigned word formed from the low byte followed by the high byte.</returns>
    private static ushort Word(byte[] memory, int address) =>
        BinaryPrimitives.ReadUInt16LittleEndian(memory.AsSpan(address));

    /// <summary>
    /// Compares the gameplay-outcome fields that reveal desynchronization. Door loading
    /// is atomic in the port, so room-owned state is compared only once gameplay resumes.
    /// </summary>
    private static List<string> CompareMovieDesyncState(SuperMetroidGame game, byte[] memory)
    {
        var mismatches = new List<string>();
        void Check(string name, ushort actual, int address)
        {
            ushort expected = Word(memory, address);
            if (actual != expected) mismatches.Add($"{name}: native={expected:X4} port={actual:X4}");
        }

        Check("Game state", (ushort)game.GameState, MovieDesyncMemory.GameState);
        bool doorTransition = game.GameState is SuperMetroidGameState.HitDoorBlock or
            SuperMetroidGameState.LoadingNextRoomA or SuperMetroidGameState.LoadingNextRoomB;
        if (!doorTransition)
            Check("RNG", game.DispatcherRandomNumber, MovieDesyncMemory.Random);

        // Native loading leaves Samus half-initialized across its NMI waits; the port
        // loads atomically. The following state compares the completed result.
        if (game.GameState is SuperMetroidGameState.SetUpNewGame or SuperMetroidGameState.LoadingGameData)
            return mismatches;
        var runtime = game.RuntimeForVerification;
        if (runtime?.Samus is not { } samus) return mismatches;
        Check("Samus X", samus.XPosition, MovieDesyncMemory.SamusX);
        Check("Samus X fraction", samus.Kinematics.XSubposition, MovieDesyncMemory.SamusXFraction);
        Check("Samus Y", samus.YPosition, MovieDesyncMemory.SamusY);
        Check("Samus Y fraction", samus.Kinematics.YSubposition, MovieDesyncMemory.SamusYFraction);
        Check("Samus health", samus.Health, MovieDesyncMemory.Health);
        Check("Samus max health", samus.MaxHealth, MovieDesyncMemory.MaxHealth);
        Check("Equipped items", samus.EquippedItems, MovieDesyncMemory.EquippedItems);
        Check("Collected items", samus.CollectedItems, MovieDesyncMemory.CollectedItems);
        Check("Equipped beams", samus.EquippedBeams, MovieDesyncMemory.EquippedBeams);
        Check("Collected beams", samus.CollectedBeams, MovieDesyncMemory.CollectedBeams);
        Check("Missiles", samus.Missiles, MovieDesyncMemory.Missiles);
        Check("Max missiles", samus.MaxMissiles, MovieDesyncMemory.MaxMissiles);
        Check("Super missiles", samus.SuperMissiles, MovieDesyncMemory.SuperMissiles);
        Check("Max super missiles", samus.MaxSuperMissiles, MovieDesyncMemory.MaxSuperMissiles);
        Check("Power bombs", samus.PowerBombs, MovieDesyncMemory.PowerBombs);
        Check("Max power bombs", samus.MaxPowerBombs, MovieDesyncMemory.MaxPowerBombs);
        Check("Reserve capacity", samus.MaxReserveEnergy, MovieDesyncMemory.MaxReserve);
        Check("Reserve energy", samus.ReserveEnergy, MovieDesyncMemory.Reserve);
        if (game.GameState != SuperMetroidGameState.MainGameplay) return mismatches;

        Check("Samus pose", samus.Pose, MovieDesyncMemory.SamusPose);
        Check("Room", runtime.ActiveRoom!.Pointer, MovieDesyncMemory.Room);
        Check("Camera X", runtime.Camera!.XPosition, MovieDesyncMemory.CameraX);
        Check("Camera Y", runtime.Camera.YPosition, MovieDesyncMemory.CameraY);
        Check("Camera X fraction", runtime.Camera.XSubposition, MovieDesyncMemory.CameraXSubposition);
        Check("Camera Y fraction", runtime.Camera.YSubposition, MovieDesyncMemory.CameraYSubposition);
        Check("Accepted NMI", runtime.NmiFrameCounter, MovieDesyncMemory.NmiCounter);
        if (runtime.Enemies.Ridley is { } ridley)
            for (int index = 0; index < ridley.TailSegments.Length; index++)
            {
                int tail = MovieDesyncMemory.RidleyTailSegments + index * MovieDesyncMemory.RidleyTailSegmentStride;
                // Inactive records hold stale words the cartridge never reads.
                bool nativeActive = (Word(memory, tail) & 0x8000) != 0;
                if (nativeActive != ridley.TailSegments[index].Active)
                    mismatches.Add($"Ridley tail {index} active: native={nativeActive} port={ridley.TailSegments[index].Active}");
                if (!nativeActive && !ridley.TailSegments[index].Active) continue;
                Check($"Ridley tail {index} X", ridley.TailSegments[index].XPosition, tail + MovieDesyncMemory.RidleyTailXOffset);
                Check($"Ridley tail {index} Y", ridley.TailSegments[index].YPosition, tail + MovieDesyncMemory.RidleyTailYOffset);
            }
        var projectiles = runtime.Enemies.EnemyProjectiles;
        if (projectiles.Count != MovieDesyncMemory.EnemyProjectileSlots)
            throw new InvalidOperationException($"Port has {projectiles.Count} enemy-projectile slots.");
        // The fresh-Ceres pad ($86:A387) and concealer ($86:A395) live in their own arrival
        // model, which owns native slots $22 and $20 while they exist; the pool leaves them free.
        var arrival = runtime.CeresElevatorArrival;
        void CheckArrivalSlot(int slot, bool active, ushort id, ushort y)
        {
            int offset = 2 * slot;
            if (projectiles[slot].IsActive)
                mismatches.Add($"Enemy projectile {slot} is allocated while the Ceres arrival owns it");
            Check($"Enemy projectile {slot} ID", active ? id : (ushort)0, MovieDesyncMemory.EnemyProjectileIds + offset);
            if (!active) return;
            Check($"Enemy projectile {slot} X", arrival!.XPosition, MovieDesyncMemory.EnemyProjectileX + offset);
            Check($"Enemy projectile {slot} Y", y, MovieDesyncMemory.EnemyProjectileY + offset);
        }
        bool arrivalLive = arrival is { IsComplete: false };
        if (arrivalLive)
        {
            CheckArrivalSlot(17, arrival!.PadActive, 0xa387, arrival!.PadYPosition);
            CheckArrivalSlot(16, arrival!.PlatformActive, 0xa395, arrival!.PlatformYPosition);
        }
        for (int slot = 0; slot < (arrivalLive ? 16 : projectiles.Count); slot++)
        {
            // The port's projectile kind is the native bank-$86 header pointer.
            var projectile = projectiles[slot];
            int offset = 2 * slot;
            Check($"Enemy projectile {slot} ID", projectile.IsActive ? (ushort)projectile.Kind : (ushort)0,
                MovieDesyncMemory.EnemyProjectileIds + offset);
            if (!projectile.IsActive) continue;
            // $86:8097 leaves the whole X word unwritten and the n00b-tube bubble's init only
            // stores its X in Var1; $86:D8DF copies it on its first run. Until then native X
            // is whatever the aliased RAM held (slot 3's X is CinematicFrameCounter $1A51).
            // The bubble has no contact radius, so that residue only places one drawn frame.
            bool bubbleXUnwritten = projectile.Kind == SuperMetroid.Core.Game.RoomEnemyProjectileKind.NoobTubeReleasedAirBubble &&
                Word(memory, MovieDesyncMemory.EnemyProjectileX + offset) !=
                Word(memory, MovieDesyncMemory.EnemyProjectileVar1 + offset);
            if (!bubbleXUnwritten)
                Check($"Enemy projectile {slot} X", projectile.XPosition, MovieDesyncMemory.EnemyProjectileX + offset);
            Check($"Enemy projectile {slot} Y", projectile.YPosition, MovieDesyncMemory.EnemyProjectileY + offset);
        }
        foreach (var actor in runtime.Enemies.Slots)
        {
            int address = MovieDesyncMemory.EnemyBase + actor.NativeIndex;
            string owner = $"Enemy {actor.SlotIndex}";
            Check(owner + " identity", actor.EnemyDefinitionPointer, address + MovieDesyncMemory.EnemyIdentityOffset);
            if (actor.EnemyDefinitionPointer == 0) continue;
            Check(owner + " X", actor.XPosition, address + MovieDesyncMemory.EnemyXOffset);
            Check(owner + " X fraction", actor.XSubposition, address + MovieDesyncMemory.EnemyXFractionOffset);
            Check(owner + " Y", actor.YPosition, address + MovieDesyncMemory.EnemyYOffset);
            Check(owner + " Y fraction", actor.YSubposition, address + MovieDesyncMemory.EnemyYFractionOffset);
            Check(owner + " health", actor.Health, address + MovieDesyncMemory.EnemyHealthOffset);
        }
        return mismatches;
    }
}
