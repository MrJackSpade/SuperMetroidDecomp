using System.Globalization;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static Program;
using System.Buffers.Binary;
using System.IO.Compression;
using System.Security.Cryptography;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static NativeSnapshotImportVerification;
using static RetailGameFixtureVerification;

internal static class FullMovieReplayVerification
{
    internal const string FullPlaythroughMoviePath = "csharp/test-fixtures/full-100-percent/Super Metroid 100%.smv";
    internal const string FullPlaythroughMovieSha256 = "4D0E6E671E11BD99439AE498210943711135E6CBE8F57D97B2C56815315AE07E";
    private const string LowPercentPlaythroughMoviePath = "csharp/test-fixtures/low-13-percent/13_speedbooster.lsmv";
    internal const string LowPercentPlaythroughMovieSha256 = "1D217BB073309AE44EA3D972C5EC3201BE0EC434C60F034E1E958F94FD1BCDA8";
    private const string DrunkPlaythroughMoviePath = "csharp/test-fixtures/issue-1275-movies/Samus drunk.smv";
    internal const string DrunkPlaythroughMovieSha256 = "6822B5D0F2D5CD81F7DB3E48E05E8EFE7F0EC2395669E0DDF80B7F5E8FC4DF17";
    private const string MotherBrainGlitchMoviePath = "csharp/test-fixtures/issue-1275-movies/mother brain glitch.smv";
    internal const string MotherBrainGlitchMovieSha256 = "C63BF68D4F24F378B2E3E3591281ABC158DDBAEBA113D35A63E3307A32D6E505";

    internal static void VerifyFullPlaythroughMovie(string traceDirectory, int? traceFromUpdate = null) =>
        VerifyPlaythroughMovie(
            ReplayMovie.Load("100%", FullPlaythroughMoviePath, FullPlaythroughMovieSha256),
            traceDirectory, traceFromUpdate);

    /// <summary>The 13% Speed Booster lsnes TAS, captured on bsnes v085.</summary>
    internal static void VerifyLowPercentPlaythroughMovie(string traceDirectory, int? traceFromUpdate = null) =>
        VerifyPlaythroughMovie(
            ReplayMovie.Load("13%", LowPercentPlaythroughMoviePath, LowPercentPlaythroughMovieSha256),
            traceDirectory, traceFromUpdate);

    /// <summary>The #1275 "Samus drunk" Snes9x 1.60 power-on playthrough.</summary>
    internal static void VerifyDrunkPlaythroughMovie(string traceDirectory, int? traceFromUpdate = null) =>
        VerifyPlaythroughMovie(
            ReplayMovie.Load("Samus drunk", DrunkPlaythroughMoviePath, DrunkPlaythroughMovieSha256),
            traceDirectory, traceFromUpdate);

    /// <summary>The #1275 Mother Brain glitch movie, which starts from a snapshot paused in her room.</summary>
    internal static void VerifyMotherBrainGlitchMovie(string traceDirectory, int? traceFromUpdate = null) =>
        VerifyPlaythroughMovie(
            ReplayMovie.Load("Mother Brain glitch", MotherBrainGlitchMoviePath, MotherBrainGlitchMovieSha256),
            traceDirectory, traceFromUpdate);

    /// <summary>
    /// Replays a supplied movie through production frontend and gameplay code. The only
    /// imported state is the movie's start: its power-on SRAM, or for a snapshot-start movie
    /// its snapshot. Every later update receives only its converted controller word, and
    /// native WRAM is read-only evidence.
    /// </summary>
    /// <param name="movie">The recorded movie whose starting state and controller timeline are replayed.</param>
    /// <param name="traceDirectory">Committed compact trace (see <see cref="ReplayTrace"/>) holding the native input-consumption timeline and read-only WRAM checkpoints for the supplied movie.</param>
    /// <param name="traceFromUpdate">
    /// Diagnostic only: from this update on, print port and native Samus kinematics so a
    /// divergence can be read field by field. Comparison and failure are unchanged.
    /// </param>
    private static void VerifyPlaythroughMovie(ReplayMovie movie, string traceDirectory, int? traceFromUpdate)
    {
        using var checkpoints = NativeMovieCheckpoints.Open(traceDirectory, movie.Bytes);
        int uncheckpointedUpdates = ReplayConvertedMovie(movie, checkpoints, traceFromUpdate, checkpoints.Updates.Count).UncheckpointedUpdates;
        checkpoints.AssertExhausted();
        Console.WriteLine($"{movie.Name} movie replay: {checkpoints.Updates.Count} updates across all {checkpoints.SourceFrameCount} source frames match" +
            (uncheckpointedUpdates == 0 ? "." : $"; {uncheckpointedUpdates} boot-entering dispatches have no native checkpoint of their own."));
    }

    /// <summary>
    /// Runs the converted updates 1 through <paramref name="lastUpdate"/>, comparing each with
    /// its native checkpoint and failing at the first divergence.
    /// </summary>
    /// <returns>The game and the native WRAM after <paramref name="lastUpdate"/>, which must have a checkpoint.</returns>
    internal static (SuperMetroidGame Game, NativeCheckpointMemory Memory, int UncheckpointedUpdates) ReplayConvertedMovie(
        ReplayMovie movie, NativeMovieCheckpoints checkpoints, int? traceFromUpdate, int lastUpdate)
    {
        var updates = checkpoints.Updates;

        NativeCheckpointMemory memory = checkpoints.ReadAfter(0) ?? throw new InvalidDataException(
            "The converted replay has no native checkpoint before its first update.");
        SuperMetroidGame game;
        if (movie.Snapshot is not null)
        {
            // The first input boundary completes the snapshot's frame. Its WRAM is the movie's
            // one initial-state import, alongside the snapshot's own cartridge SRAM.
            game = ImportNativeSnapshot(memory.ToWorkRam(), movie.InitialSaveRam).Game;
        }
        else
        {
            var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            movie.InitialSaveRam.CopyTo(bus.SaveRam);
            game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        }
        var audio = new CartridgeAudioRenderer(RepositoryInstallation.Installation.LoadAudio());

        Console.WriteLine($"Native first input boundary: source frame {updates[0].SourceFrame}, " +
            $"state {Word(memory, MovieDesyncMemory.GameState):X2}, RNG {Word(memory, MovieDesyncMemory.Random):X4}.");
        ushort[] frameInputs = movie.FrameInputs;
        var recentInputs = new Queue<string>();
        var uploadNmis = new EvidencedDoorMusicUploadNmis();
        game.DoorMusicUploadNmis = uploadNmis;
        var loaderProgress = new EvidencedDoorLoaderProgress();
        game.DoorLoaderProgress = loaderProgress;
        EvidencedSoundAcknowledgements? soundAcknowledgements = new(memory);
        CartridgeAudioAcknowledgements spcAcknowledgements = audio.ReadAcknowledgements();
        int excludedBefore = 0, uncheckpointedUpdates = 0;
        for (int update = 1; update <= lastUpdate; update++)
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
            NativeCheckpointMemory? nativeAfter = checkpoints.ReadAfter(update);
            // A dispatch that jumped into CommonBootSection has no checkpoint of its own, and
            // the clear erases the sound handler's waiting states before the next one. Both
            // updates therefore read the port's SPC model, as a message box's frames do.
            game.SetAudioAcknowledgements(nativeAfter is not null && soundAcknowledgements is not null
                ? soundAcknowledgements.ForUpdate(spcAcknowledgements, nativeAfter) : spcAcknowledgements);
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
            recentInputs.Enqueue($"{update}@{step.SourceFrame}:{step.Input:X4}/{step.Kind}");
            if (recentInputs.Count > 12) recentInputs.Dequeue();
            if (nativeAfter is null)
            {
                soundAcknowledgements = null;
                uncheckpointedUpdates++;
                continue;
            }
            memory = nativeAfter;
            soundAcknowledgements ??= new EvidencedSoundAcknowledgements(memory);
            soundAcknowledgements.Capture(memory);

            if (update >= traceFromUpdate)
                TraceMovieSamus(game, memory, update, step);
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
        if (lastUpdate != 0 && updates[lastUpdate - 1].ExpectedRecord is null)
            throw new InvalidOperationException($"Update {lastUpdate} has no native checkpoint to stop at.");
        return (game, memory, uncheckpointedUpdates);
    }

    private static void TraceMovieSamus(SuperMetroidGame game, NativeCheckpointMemory memory, int update, ConvertedMovieUpdate step)
    {
        var samus = game.RuntimeForVerification?.Player.Samus;
        string Native(int address) => Word(memory, address).ToString("X4");
        Console.WriteLine($"trace {update}@{step.SourceFrame} in={step.Input:X4} state={(ushort)game.GameState:X2}/{Native(MovieDesyncMemory.GameState)} " +
            $"door={game.DoorTransitionPhaseForVerification}/{Native(0x099c)} rng={game.DispatcherRandomNumber:X4}/{Native(MovieDesyncMemory.Random)}");
        if (PrivateState.Field<IntroCinematicState?>(game, "intro") is { } intro)
            Console.WriteLine($"  intro port={intro.Phase} native cinematic={Native(0x1f51)}");
        if (samus is null) return;
        Console.WriteLine($"  port   pose={(int)samus.Pose:X2} X={samus.Position.X.Whole:X4}.{samus.Kinematics.Position.X.Fraction:X4} Y={samus.Position.Y.Whole:X4}.{samus.Kinematics.Position.Y.Fraction:X4} " +
            $"vs={samus.Kinematics.YSpeed:X4}.{samus.Kinematics.YSubspeed:X4} total={samus.HorizontalSpeed.TotalSpeed:X4}.{samus.HorizontalSpeed.TotalSubspeed:X4} slope={(samus.Kinematics.PositionAdjustedBySlope ? 1 : 0)} base={samus.HorizontalSpeed.BaseSpeed:X4}.{samus.HorizontalSpeed.BaseSubspeed:X4} extra={samus.HorizontalSpeed.ExtraRunSpeed:X4}.{samus.HorizontalSpeed.ExtraRunSubspeed:X4} accel={samus.HorizontalSpeed.AccelerationMode:X4} anim={samus.AnimationFrame:X4}/{samus.AnimationFrameTimer:X4}");
        Console.WriteLine($"  native pose={Native(MovieDesyncMemory.SamusPose)} X={Native(MovieDesyncMemory.SamusX)}.{Native(MovieDesyncMemory.SamusXFraction)} Y={Native(MovieDesyncMemory.SamusY)}.{Native(MovieDesyncMemory.SamusYFraction)} " +
            $"vs={Native(0x0b2e)}.{Native(0x0b2c)} total={Native(0x0dbc)}.{Native(0x0dbe)} slope={Native(0x0dba)} base={Native(0x0b46)}.{Native(0x0b48)} extra={Native(0x0b42)}.{Native(0x0b44)} accel={Native(0x0b4a)} anim={Native(0x0a96)}/{Native(0x0a94)}");
        if (game.CeresDestructionForVerification is { } boom)
            Console.WriteLine($"  cinematic port={boom.Phase} native={Native(0x1f51)}");
        Console.WriteLine($"  nmi port={(game.RuntimeForVerification?.Nmi.NmiFrameCounter ?? game.MenuNmiFrameCounterForVerification):X4} native={Native(0x05b6)}");
        if (game.RuntimeForVerification?.EscapeTimer is { } escape)
            Console.WriteLine($"  escape port={escape.RawStatus:X4} {escape.MinutesBcd:X2}:{escape.SecondsBcd:X2}.{escape.CentisecondsBcd:X2} " +
                $"native={Native(0x0943)} {memory[0x0947]:X2}:{memory[0x0946]:X2}.{memory[0x0945]:X2}");
        Console.WriteLine("  aerial port  " + game.RuntimeForVerification?.Player.LastAerialSamusMovement);
        if (game.RuntimeForVerification is { } projectileRuntime)
        {
            Console.WriteLine("  sproj port   " + string.Join(" ", projectileRuntime.Projectiles.Slots
                .Where(slot => slot.Type != 0)
                .Select(slot => $"{slot.SlotIndex}:{slot.Type:X4}@{slot.Position.X.Whole:X4}.{slot.Position.X.Fraction:X4}/{slot.Position.Y.Whole:X4} r{slot.Radius.X:X}/{slot.Radius.Y:X} v{slot.Velocity.X.SignedRawValue:X4} d{slot.Direction:X}")));
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
            var portProjectiles = traceEnemies.Projectiles.EnemyProjectiles.Select((projectile, slot) => (projectile, slot))
                .Where(entry => entry.projectile.IsActive)
                .Select(entry => $"{entry.slot}:{(ushort)entry.projectile.Kind:X4}@{entry.projectile.Position.X.Whole:X}/{entry.projectile.Position.Y.Whole:X}" +
                    $"#{entry.projectile.InstructionPointer:X4}/{entry.projectile.InstructionTimer:X}");
            var nativeProjectiles = Enumerable.Range(0, 18).Where(slot => Word(memory, 0x1997 + 2 * slot) != 0)
                .Select(slot => $"{slot}:{Native(0x1997 + 2 * slot)}@{Word(memory, 0x1a4b + 2 * slot):X}/{Word(memory, 0x1a93 + 2 * slot):X}" +
                    $"#{Word(memory, 0x1b47 + 2 * slot):X4}/{Word(memory, 0x1b8f + 2 * slot):X}");
            Console.WriteLine("  eproj port   " + string.Join(" ", portProjectiles));
            Console.WriteLine("  eproj native " + string.Join(" ", nativeProjectiles));
            Console.WriteLine($"  ceres port={traceEnemies.RidleyEncounter.CeresStatus:X4} native={Native(0x093f)} " +
                $"nmi port={game.RuntimeForVerification!.Nmi.NmiFrameCounter:X4} native-raw={Native(0x05b6)}");
            foreach (var actor in traceEnemies.SlotTable.Slots.Where(slot => slot.EnemyDefinitionPointer != 0))
            {
                // $0F86 properties, $0F92 instruction list pointer, $0F94 instruction timer.
                int slotBase = MovieDesyncMemory.EnemyBase + actor.NativeIndex;
                Console.WriteLine($"  enemy {actor.NativeIndex / MovieDesyncMemory.EnemyStride} {(int)actor.EnemyDefinitionPointer:X4} " +
                    $"port inst={actor.CurrentInstruction:X4}/{actor.InstructionTimer:X4} prop={actor.Properties:X4} " +
                    $"native inst={Native(slotBase + 0x1a)}/{Native(slotBase + 0x1c)} prop={Native(slotBase + 0x0e)}");
            }
        }
        if (game.RuntimeForVerification?.Enemies.MotherBrainEncounter.MotherBrain?.BabyMetroid is { } baby)
        {
            // Baby Metroid cutscene actor: $0FA8+$40n function words; velocities at +2/+4.
            Console.WriteLine($"  baby port   pos={baby.Position.X.Whole:X4}.{baby.Position.X.Fraction:X4}/{baby.Position.Y.Whole:X4}.{baby.Position.Y.Fraction:X4} v={baby.Velocity.X.RawValue:X4}/{baby.Velocity.Y.RawValue:X4}");
            Console.WriteLine($"  baby native pos={Native(0x103a)}.{Native(0x103c)}/{Native(0x103e)}.{Native(0x1040)} v={Native(0x106a)}/{Native(0x106c)}");
        }
        if (game.RuntimeForVerification?.Enemies.RidleyEncounter.Ridley is { } ridley)
        {
            // $7E:2020: seven ten-word tail records; X/Y at +12/+14.
            string Port(int index) => $"{ridley.TailSegments[index].Position.X:X4}/{ridley.TailSegments[index].Position.Y:X4}";
            string NativeTail(int index) => $"{Native(0x2020 + index * 20 + 12)}/{Native(0x2020 + index * 20 + 14)}";
            Console.WriteLine("  tail port   " + string.Join(" ", Enumerable.Range(0, 7).Select(Port)) + $" fn={(ushort)ridley.Function:X4} tailfn={(int)ridley.TailFunctionIndex:X4} v={ridley.Velocity.X.RawValue:X4}/{ridley.Velocity.Y.RawValue:X4} getaway={ridley.Mode7TableByteIndex:X4}");
            Console.WriteLine("  tail native " + string.Join(" ", Enumerable.Range(0, 7).Select(NativeTail)) + $" fn={Native(0x0fa8)} tailfn={Native(0x2000)} v={Native(0x0faa)}/{Native(0x0fac)} getaway={Native(0x8026)}");
        }
    }

    private static ushort Word(NativeCheckpointMemory memory, int address) => memory.Word(address);

    /// <summary>
    /// Compares the gameplay-outcome fields that reveal desynchronization. Door loading
    /// is atomic in the port, so room-owned state is compared only once gameplay resumes.
    /// </summary>
    private static List<string> CompareMovieDesyncState(SuperMetroidGame game, NativeCheckpointMemory memory)
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
        if (runtime?.Player.Samus is not { } samus) return mismatches;
        Check("Samus X", samus.Position.X.Whole, MovieDesyncMemory.SamusX);
        Check("Samus X fraction", samus.Kinematics.Position.X.Fraction, MovieDesyncMemory.SamusXFraction);
        Check("Samus Y", samus.Position.Y.Whole, MovieDesyncMemory.SamusY);
        Check("Samus Y fraction", samus.Kinematics.Position.Y.Fraction, MovieDesyncMemory.SamusYFraction);
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

        Check("Samus pose", (ushort)samus.Pose, MovieDesyncMemory.SamusPose);
        Check("Room", runtime.Room.ActiveRoom!.Pointer, MovieDesyncMemory.Room);
        Check("Camera X", runtime.Room.Camera!.PixelPosition.X, MovieDesyncMemory.CameraX);
        Check("Camera Y", runtime.Room.Camera.PixelPosition.Y, MovieDesyncMemory.CameraY);
        Check("Camera X fraction", runtime.Room.Camera.Position.X.Fraction, MovieDesyncMemory.CameraXSubposition);
        Check("Camera Y fraction", runtime.Room.Camera.Position.Y.Fraction, MovieDesyncMemory.CameraYSubposition);
        Check("Accepted NMI", runtime.Nmi.NmiFrameCounter, MovieDesyncMemory.NmiCounter);
        if (runtime.Enemies.RidleyEncounter.Ridley is { } ridley)
            for (int index = 0; index < ridley.TailSegments.Length; index++)
            {
                int tail = MovieDesyncMemory.RidleyTailSegments + index * MovieDesyncMemory.RidleyTailSegmentStride;
                // Inactive records hold stale words the cartridge never reads.
                bool nativeActive = (Word(memory, tail) & 0x8000) != 0;
                if (nativeActive != ridley.TailSegments[index].Active)
                    mismatches.Add($"Ridley tail {index} active: native={nativeActive} port={ridley.TailSegments[index].Active}");
                if (!nativeActive && !ridley.TailSegments[index].Active) continue;
                Check($"Ridley tail {index} X", ridley.TailSegments[index].Position.X, tail + MovieDesyncMemory.RidleyTailXOffset);
                Check($"Ridley tail {index} Y", ridley.TailSegments[index].Position.Y, tail + MovieDesyncMemory.RidleyTailYOffset);
            }
        var projectiles = runtime.Enemies.Projectiles.EnemyProjectiles;
        if (projectiles.Count != MovieDesyncMemory.EnemyProjectileSlots)
            throw new InvalidOperationException($"Port has {projectiles.Count} enemy-projectile slots.");
        // The fresh-Ceres pad ($86:A387) and concealer ($86:A395) live in their own arrival
        // model, which owns native slots $22 and $20 while they exist; the pool leaves them free.
        var arrival = runtime.Room.CeresElevatorArrival;
        void CheckArrivalSlot(int slot, bool active, ushort id, ushort y)
        {
            int offset = 2 * slot;
            if (projectiles[slot].IsActive)
                mismatches.Add($"Enemy projectile {slot} is allocated while the Ceres arrival owns it");
            Check($"Enemy projectile {slot} ID", active ? id : (ushort)0, MovieDesyncMemory.EnemyProjectileIds + offset);
            if (!active) return;
            Check($"Enemy projectile {slot} X", arrival!.PadPosition.X, MovieDesyncMemory.EnemyProjectileX + offset);
            Check($"Enemy projectile {slot} Y", y, MovieDesyncMemory.EnemyProjectileY + offset);
        }
        bool arrivalLive = arrival is { IsComplete: false };
        if (arrivalLive)
        {
            CheckArrivalSlot(17, arrival!.PadActive, 0xa387, arrival!.PadPosition.Y);
            CheckArrivalSlot(16, arrival!.PlatformActive, 0xa395, arrival!.PlatformPosition.Y);
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
                Check($"Enemy projectile {slot} X", projectile.Position.X.Whole, MovieDesyncMemory.EnemyProjectileX + offset);
            Check($"Enemy projectile {slot} Y", projectile.Position.Y.Whole, MovieDesyncMemory.EnemyProjectileY + offset);
        }
        foreach (var actor in runtime.Enemies.SlotTable.Slots)
        {
            int address = MovieDesyncMemory.EnemyBase + actor.NativeIndex;
            string owner = $"Enemy {actor.SlotIndex}";
            Check(owner + " identity", (ushort)actor.EnemyDefinitionPointer, address + MovieDesyncMemory.EnemyIdentityOffset);
            if (actor.EnemyDefinitionPointer == EnemyDefinitionId.None) continue;
            Check(owner + " X", actor.Position.X.Whole, address + MovieDesyncMemory.EnemyXOffset);
            Check(owner + " X fraction", actor.Position.X.Fraction, address + MovieDesyncMemory.EnemyXFractionOffset);
            Check(owner + " Y", actor.Position.Y.Whole, address + MovieDesyncMemory.EnemyYOffset);
            Check(owner + " Y fraction", actor.Position.Y.Fraction, address + MovieDesyncMemory.EnemyYFractionOffset);
            Check(owner + " health", actor.Health, address + MovieDesyncMemory.EnemyHealthOffset);
        }
        return mismatches;
    }

    [VerificationCommand("--lsmv-playthrough-movie", "lowTraceDirectory")]
    internal static int LsmvPlaythroughMovieCommand(string lowTraceDirectory)
    {
     VerifyLowPercentPlaythroughMovie(lowTraceDirectory); return 0; 
    }

    [VerificationCommand("--full-playthrough-movie", "fullTraceDirectory")]
    internal static int FullPlaythroughMovieCommand(string fullTraceDirectory)
    {
     VerifyFullPlaythroughMovie(fullTraceDirectory); return 0; 
    }

    [VerificationCommand("--drunk-playthrough-movie", "drunkTraceDirectory")]
    internal static int DrunkPlaythroughMovieCommand(string drunkTraceDirectory)
    {
     VerifyDrunkPlaythroughMovie(drunkTraceDirectory); return 0; 
    }

    [VerificationCommand("--mother-brain-glitch-movie", "glitchTraceDirectory")]
    internal static int MotherBrainGlitchMovieCommand(string glitchTraceDirectory)
    {
     VerifyMotherBrainGlitchMovie(glitchTraceDirectory); return 0; 
    }

    [VerificationCommand("--lsmv-playthrough-movie", "traceDirectory", "--trace-from", "traceFromUpdate")]
    internal static int LsmvPlaythroughMovieTraceCommand(string traceDirectory, string traceFromUpdate)
    {
        VerifyLowPercentPlaythroughMovie(traceDirectory, int.Parse(traceFromUpdate, CultureInfo.InvariantCulture));
        return 0;
    }

    [VerificationCommand("--full-playthrough-movie", "traceDirectory", "--trace-from", "traceFromUpdate")]
    internal static int FullPlaythroughMovieTraceCommand(string traceDirectory, string traceFromUpdate)
    {
        VerifyFullPlaythroughMovie(traceDirectory, int.Parse(traceFromUpdate, CultureInfo.InvariantCulture));
        return 0;
    }

    [VerificationCommand("--drunk-playthrough-movie", "traceDirectory", "--trace-from", "traceFromUpdate")]
    internal static int DrunkPlaythroughMovieTraceCommand(string traceDirectory, string traceFromUpdate)
    {
        VerifyDrunkPlaythroughMovie(traceDirectory, int.Parse(traceFromUpdate, CultureInfo.InvariantCulture));
        return 0;
    }

    [VerificationCommand("--mother-brain-glitch-movie", "traceDirectory", "--trace-from", "traceFromUpdate")]
    internal static int MotherBrainGlitchMovieTraceCommand(string traceDirectory, string traceFromUpdate)
    {
        VerifyMotherBrainGlitchMovie(traceDirectory, int.Parse(traceFromUpdate, CultureInfo.InvariantCulture));
        return 0;
    }
}
