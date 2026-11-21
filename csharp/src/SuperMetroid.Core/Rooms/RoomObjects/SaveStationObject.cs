using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A save station (#1554, #2484). It draws its pod once and stays still until Samus stands centred on its floor; that touch
/// asks the frontend whether to save, an accepted save flashes the pod's lights for their authored loops
/// (<see cref="StationAnimationDefinitions"/>) and then publishes the saved-game message, and the save is over once the
/// message closes or the answer is no. The pod and its flash each play on the shared animation player (#2480).
/// </summary>
/// <param name="savedAt">The load station the station saves at, as its placement named it.</param>
internal sealed class SaveStationObject(LoadStation savedAt) : RoomObjectBehaviour
{
    /// <summary>The load station the station saves at, which its activation events carry.</summary>
    internal LoadStation SavedAt { get; } = Ensure.NotNull(savedAt);

    /// <summary>The pod playing: its first image shows on the station's first update, and the pod stays still after it.</summary>
    private readonly AnimationPlayer<StationAnimation, StationDraw, NoAnimationCue> _pod =
        AnimationPlayer<StationAnimation, StationDraw, NoAnimationCue>.Starting(
            StationAnimationDefinitions.Segments, StationAnimationDefinitions.Animations[StationAnimation.SaveIdle]);

    /// <summary>The accepted save's flash playing, from the update the save is accepted.</summary>
    private readonly AnimationPlayer<StationAnimation, StationDraw, NoAnimationCue> _flash =
        AnimationPlayer<StationAnimation, StationDraw, NoAnimationCue>.Starting(
            StationAnimationDefinitions.Segments, StationAnimationDefinitions.Animations[StationAnimation.SaveFlash]);

    /// <summary>Where the station stands in a save, from Samus's accepted touch of its floor on.</summary>
    public SaveStationPhase SavePhase { get; private set; }

    /// <summary>Samus's accepted touch of the idle station's floor, which the station's next update takes (#2545).</summary>
    internal void Trigger() => SavePhase = SaveStationPhase.Touched;

    /// <summary>The save is over, declined at its confirmation or completed once its message closed.</summary>
    internal void EndSave() => SavePhase = SaveStationPhase.Idle;

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (SavePhase == SaveStationPhase.Touched)
        {
            SavePhase = SaveStationPhase.AwaitingConfirmation;
            context.Outcome.StationActed(new SaveConfirmationRequested(SaveActivation(place)));
        }

        if (StepSaveAnimation(place, context))
            return;

        // The station draws its pod once and then stays still until its floor asks to save.
        if (_pod.Shown is not null)
            return;
        StationDraw? shown = _pod.StepToNewFrame();
        if (_pod.IsResting)
            throw new InvalidDataException("The save station's pod came to rest instead of showing its image.");
        if (shown is { } image)
            context.Draw(place, StationObjectDraws.Family, image);
    }

    /// <summary>
    /// The accepted save's first image, drawn at <paramref name="block"/> on the update the save is accepted, which
    /// starts its loops.
    /// </summary>
    internal void StartSaveAnimation(IRoomObjectDrawing drawing, RoomLevelData level, Position16<RoomBlocks> block)
    {
        SavePhase = SaveStationPhase.Animating;
        _flash.Show(StationAnimationDefinitions.Animations[StationAnimation.SaveFlash]);
        StationObjectDraws.Family.Draw(level, block, Ensure.NotNull(drawing), _flash.Showing);
    }

    /// <summary>Runs the accepted save's flash until the saved-game message can be published.</summary>
    private bool StepSaveAnimation(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (SavePhase is SaveStationPhase.Idle or SaveStationPhase.AwaitingConfirmation)
            return false;
        if (SavePhase == SaveStationPhase.AwaitingCompletionMessageClose)
            return true;
        if (SavePhase != SaveStationPhase.Animating)
            throw new InvalidDataException($"Save station has unknown phase {SavePhase}.");

        if (_flash.StepToNewFrame() is { } shown)
        {
            context.Draw(place, StationObjectDraws.Family, shown);
            return true;
        }

        // After the last of its loops the flash is over, and the save publishes the saved-game message instead of
        // lighting the pod again.
        if (_flash.HasStopped)
        {
            SavePhase = SaveStationPhase.AwaitingCompletionMessageClose;
            context.Outcome.StationActed(new SaveCompleted(SaveActivation(place)));
        }
        return true;
    }

    /// <summary>The save station's activation: the station itself, the load station it saves at and its block.</summary>
    private SaveStationActivation SaveActivation(RoomObjectPlace place) => new(this, SavedAt, place.Block);
}
