using SuperMetroid.Diagnostics.Input;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// Replays a supplied movie through production frontend and gameplay code. The only imported state is the movie's
/// start: its power-on SRAM, or for a snapshot-start movie its snapshot. Every later update receives only its converted
/// controller word, and native WRAM is read-only evidence: each checkpointed update is compared with its native
/// checkpoint and the replay fails at the first divergence.
/// </summary>
/// <remarks>
/// The host runs each frame: an update's input, or one frame of a message box the update opened. Bank $85's message
/// box runs inside its dispatch on lag frames, polling the joypad registers rather than accepting NMIs, so the box spans
/// the update's remaining source frames; the port shows it one frame per step, each with that source frame's input.
/// </remarks>
internal sealed class ConvertedTimelineReplay : IConvertedMovieReplay
{
    /// <summary>The recorded movie whose starting state and controller timeline are replayed.</summary>
    private readonly ReplayMovie movie;

    /// <summary>The native input-consumption timeline and read-only WRAM checkpoints of the movie.</summary>
    private readonly NativeMovieCheckpoints checkpoints;

    /// <summary>Diagnostic only: from this update on, port and native state are printed field by field.</summary>
    private readonly int? traceFromUpdate;

    /// <summary>The last update the replay runs; it must have a native checkpoint.</summary>
    private readonly int lastUpdate;

    /// <summary>This movie's own comparisons after each checkpointed update, reported with the shared ones.</summary>
    private readonly IConvertedMovieChecks? movieChecks;

    /// <summary>When supplied, the host records every update's picture and sound with it.</summary>
    private readonly IMovieRecorder? video;

    /// <summary>The update whose input frame the host is running, or <see langword="null"/> between frames and during message-box frames.</summary>
    private int? pendingUpdate;

    /// <summary>The source frame of <see cref="pendingUpdate"/>'s input.</summary>
    private int pendingSourceFrame;

    /// <summary>Starts a replay of <paramref name="movie"/> from its native first input boundary.</summary>
    /// <param name="movie">The recorded movie whose starting state and controller timeline are replayed.</param>
    /// <param name="checkpoints">The movie's native checkpoints; the replay takes ownership.</param>
    /// <param name="traceFromUpdate">Diagnostic only: from this update on, print port and native state.</param>
    /// <param name="lastUpdate">The last update to run; it must have a native checkpoint.</param>
    /// <param name="movieChecks">This movie's own checks, or <see langword="null"/>.</param>
    /// <param name="video">When supplied, the host records every update's picture and sound with it.</param>
    internal ConvertedTimelineReplay(ReplayMovie movie, NativeMovieCheckpoints checkpoints, int? traceFromUpdate, int lastUpdate,
        IConvertedMovieChecks? movieChecks, IMovieRecorder? video)
    {
        if (video is not null && movie.Snapshot is not null)
            throw new NotSupportedException("Video export covers power-on movies only.");
        this.movie = movie;
        this.checkpoints = checkpoints;
        this.traceFromUpdate = traceFromUpdate;
        this.lastUpdate = lastUpdate;
        this.movieChecks = movieChecks;
        this.video = video;
        Memory = checkpoints.ReadAfter(0) ?? throw new InvalidDataException(
            "The converted replay has no native checkpoint before its first update.");
    }

    /// <inheritdoc/>
    public SuperMetroidGame Game
    {
        get => field ?? throw new InvalidOperationException("The replay has not imported its initial state.");
        private set;
    }

    /// <summary>The native WRAM after the last checkpointed update completed so far.</summary>
    internal NativeCheckpointMemory Memory { get; private set; }

    /// <summary>The updates completed so far, each with its message-box frames, reset and comparison.</summary>
    internal int CompletedUpdates { get; private set; }

    /// <summary>Boot-entering dispatches completed so far that have no native checkpoint of their own.</summary>
    internal int UncheckpointedUpdates { get; private set; }

    /// <inheritdoc/>
    public IEnumerable<ControllerButton?> Frames()
    {
        var updates = checkpoints.Updates;
        NativeCheckpointMemory memory = Memory;
        // The replay's hardware-timing evidence is fixed for the game's lifetime, across console resets.
        var uploadNmis = new EvidencedDoorMusicLoadFrames();
        var loaderProgress = new EvidencedRoomLoadProgress();
        var soundAcknowledgements = new EvidencedSoundAcknowledgements(memory);
        var hostServices = new SuperMetroidGameHostServices
        {
            DoorMusicLoadFrames = uploadNmis,
            RoomLoadProgress = loaderProgress,
            SoundAcceptance = soundAcknowledgements,
            GameplayFrames = CapturedGameplayFrames.Instance,
        };
        if (movie.Snapshot is not null)
        {
            // The first input boundary completes the snapshot's frame. Its WRAM is the movie's
            // one initial-state import, alongside the snapshot's own cartridge SRAM.
            Game = NativeSnapshotImport.ImportNativeSnapshot(memory.ToWorkRam(), movie.InitialSaveRam, hostServices).Game;
        }
        else
        {
            var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            Game = InstalledFixtures.CreateGame(SuperMetroid.Diagnostics.Saves.SaveRamCodec.Decode(movie.InitialSaveRam, InstalledFixtures.Maps),
                renderGameplayFrames: video is not null, hostServices: hostServices);
            video?.Attach(Game);
        }

        Console.WriteLine($"Native first input boundary: source frame {updates[0].SourceFrame}, " +
            $"state {MovieDesyncComparison.Word(memory, MovieDesyncMemory.GameState):X2}, RNG {MovieDesyncComparison.Word(memory, MovieDesyncMemory.Random):X4}.");
        // The opening import is complete: the host may run the first update.
        yield return null;

        ushort[] frameInputs = movie.FrameInputs;
        var recentInputs = new Queue<string>();
        int excludedBefore = 0;
        for (int update = 1; update <= lastUpdate; update++)
        {
            ConvertedMovieUpdate step = updates[update - 1];
            // Upload-wait NMIs that follow this update's input are accepted inside its dispatch.
            uploadNmis.Expect(update, step.ExcludedNmiAfter - excludedBefore);
            excludedBefore = step.ExcludedNmiAfter;
            loaderProgress.Expect(step.DoorLoaderCompletedEnemySlots);
            if (update >= traceFromUpdate && Game.RuntimeForVerification is { } tracedRuntime)
                tracedRuntime.System.Random.CallObserver = () => Console.WriteLine(
                    "  rng call: " + string.Join(" <- ", new System.Diagnostics.StackTrace(2)
                        .GetFrames().Take(4).Select(frame => frame.GetMethod()?.Name)));
            // The last controller read of an eliminated music wait is still input the
            // cartridge consumed: it decides whether this update sees a new press.
            if (step.HardwareWaitLatch is { } latched)
                Game.AcceptDoorMusicWaitControllerRead(ControllerWordCodec.FromJoypadWord(latched, $"movie update {update} wait latch"));
            NativeCheckpointMemory? nativeAfter = checkpoints.ReadAfter(update);
            // A dispatch that jumped into CommonBootSection has no checkpoint of its own, and
            // the clear erases the sound handler's waiting states before the next one. Both
            // updates therefore read the session's SPC model, as a message box's frames do.
            soundAcknowledgements.Expect(nativeAfter);
            // A confirmation box can continue from the previous dispatch into this
            // NMI continuation; its MessageBox_Routine entry belongs to that dispatch.
            bool boxContinues = Game.RuntimeForVerification?.MessageBox.IsActive == true;
            // The host runs this update's input; a production failure in it is named after the update.
            pendingUpdate = update;
            pendingSourceFrame = step.SourceFrame;
            yield return ControllerWordCodec.FromJoypadWord(step.Input, $"movie update {update}");
            pendingUpdate = null;
            uploadNmis.AssertSettled();
            // Bank $85's message box runs inside this dispatch on lag frames, polling the
            // joypad registers rather than accepting NMIs, so the box spans this update's
            // remaining source frames. The port shows it one frame per step.
            if (Game.RuntimeForVerification?.MessageBox.IsActive == true)
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
                // so they read the session's SPC model frame by frame.
                soundAcknowledgements.Expect(null);
                for (; frame < boxFrameLimit && Game.GameplayRuntimeForVerification.MessageBox.IsActive; frame++)
                {
                    // The previous frame is complete; the host runs the box's next frame.
                    yield return null;
                    yield return ControllerWordCodec.FromJoypadWord(frameInputs[frame], $"movie frame {frame}");
                }
                if (step.MessageBoxEndFrame is { } nativeEnd)
                {
                    if (Game.GameplayRuntimeForVerification.MessageBox.IsActive || frame != nativeEnd + 1)
                        throw new InvalidDataException(
                            $"The port's message box closed after source frame {frame - 1}, but native dispatch {update} " +
                            $"(source frames {step.SourceFrame}-{endSourceFrame}) returned from it at {nativeEnd}.");
                }
                // A confirmation box reads the controller itself ($85:84BA), which the native
                // capture records as a following NMI-continuation update. The box then
                // legitimately stays open into that update and continues there.
                else if (!Game.GameplayRuntimeForVerification.MessageBox.IsActive || frame != endSourceFrame ||
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
            CompleteUpdate(update, step, nativeAfter, soundAcknowledgements, hostServices, recentInputs);
            // The update is complete: the host may run the next frame.
            yield return null;
        }
        if (lastUpdate != 0 && updates[lastUpdate - 1].ExpectedRecord is null)
            throw new InvalidOperationException($"Update {lastUpdate} has no native checkpoint to stop at.");
    }

    /// <summary>
    /// Completes update <paramref name="update"/> after its frames ran: a console reset the movie pressed before the next
    /// update's read, and the comparison with the native checkpoint that follows the update.
    /// </summary>
    private void CompleteUpdate(int update, ConvertedMovieUpdate step, NativeCheckpointMemory? nativeAfter,
        EvidencedSoundAcknowledgements soundAcknowledgements, SuperMetroidGameHostServices hostServices, Queue<string> recentInputs)
    {
        var updates = checkpoints.Updates;
        CompletedUpdates = update;
        // The movie pressed the console's reset button before the next update's read. The
        // cartridge reboots keeping only its SRAM, so the port starts a new game on the same
        // cartridge; the post-boot checkpoint at that read completes this update.
        bool consoleReset = update < updates.Count && updates[update].ConsoleReset;
        if (consoleReset)
        {
            Game = InstalledFixtures.CreateGame(Game.Saves, renderGameplayFrames: video is not null, hostServices: hostServices);
            video?.Attach(Game);
            Console.WriteLine($"{movie.Name} movie: console reset before update {update + 1} " +
                $"(source frame {updates[update].SourceFrame}); the port reboots on the same cartridge.");
        }
        recentInputs.Enqueue($"{update}@{step.SourceFrame}:{step.Input:X4}/{step.Kind}");
        if (recentInputs.Count > 12) recentInputs.Dequeue();
        if (nativeAfter is null)
        {
            soundAcknowledgements.Invalidate();
            UncheckpointedUpdates++;
            return;
        }
        NativeCheckpointMemory memory = nativeAfter;
        Memory = memory;
        soundAcknowledgements.Capture(memory);
        // Like a power-on movie's first record, the read after a reset precedes CommonBootSection,
        // which the port's reboot performs within the next update; that update is compared instead.
        if (consoleReset)
            return;

        if (update >= traceFromUpdate)
            MovieDesyncComparison.TraceMovieSamus(Game, memory, update, step);
        List<string> mismatches = MovieDesyncComparison.CompareMovieDesyncState(Game, memory);
        if (movieChecks is not null)
            mismatches.AddRange(movieChecks.Compare(Game, update, memory));
        if (mismatches.Count != 0)
        {
            Console.Error.WriteLine("Recent converted inputs: " + string.Join(", ", recentInputs));
            throw new InvalidDataException(
                $"{movie.Name} movie first divergence after update {update} (source frame {step.SourceFrame}, " +
                $"native state {MovieDesyncComparison.Word(memory, MovieDesyncMemory.GameState):X2}, port state {GameStateNumberCodec.Encode(Game.GameState):X2}): " +
                string.Join("; ", mismatches));
        }
        if (update % 10000 == 0)
            Console.WriteLine($"Update {update}/{updates.Count} (source frame {step.SourceFrame}) matches; state {Game.GameState}.");
    }

    /// <inheritdoc/>
    public string Finish()
    {
        checkpoints.AssertExhausted();
        var report = new List<string>
        {
            $"{movie.Name} movie replay: {checkpoints.Updates.Count} updates across all {checkpoints.SourceFrameCount} source frames match" +
            (UncheckpointedUpdates == 0 ? "." : $"; {UncheckpointedUpdates} boot-entering dispatches have no native checkpoint of their own."),
        };
        if (movieChecks is not null)
            report.AddRange(movieChecks.Finish());
        return string.Join(Environment.NewLine, report);
    }

    /// <inheritdoc/>
    public Exception? NamedProductionFailure(Exception exception) =>
        pendingUpdate is { } update && exception is InvalidOperationException or NotSupportedException or InvalidDataException
            // Name the update a production failure happened on; the original stays the inner exception.
            ? new InvalidOperationException(
                $"{movie.Name} movie update {update} (source frame {pendingSourceFrame}) failed in production code{RoomAtFailure()}.", exception)
            : null;

    /// <summary>
    /// The room state loaded when production code failed and the room objects active in it, each with its header, block
    /// and behaviour, so a failure names what the room was doing (#1636); empty when no room is loaded.
    /// </summary>
    private string RoomAtFailure()
    {
        if (Game.RuntimeForVerification?.Room.Scope is not { } scope)
            return string.Empty;
        string[] objects = [.. scope.RoomObjects.SlotTable.InProcessingOrder.Where(static slot => slot.Active)
            .Select(slot => $"{slot.Behaviour?.GetType().Name ?? "no behaviour"} at {slot.Block} (terrain now " +
                $"{scope.LevelData.GetCollisionBlock(slot.Block)}; restores {slot.RestoreTerrain?.ToString() ?? "nothing"})")];
        return $" in room state {scope.Room.State.Name}; active room objects: {(objects.Length == 0 ? "none" : string.Join(", ", objects))}";
    }

    /// <inheritdoc/>
    public void Dispose() => checkpoints.Dispose();
}
