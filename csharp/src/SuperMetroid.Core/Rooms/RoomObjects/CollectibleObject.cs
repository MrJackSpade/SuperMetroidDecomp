using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// An item or tank lying in the room (#1554). It holds its kind,
/// its presentation (exposed, in a Chozo orb or in a shot block), its phase and countdown as domain values, and advances
/// them on each update by its authored timing (<see cref="CollectibleTimingDefinitions"/>): an exposed item alternates
/// its two looks until Samus touches it; a Chozo orb cycles its shell until a shot breaks it; a shot block reveals its
/// item for a while and then conceals it again. A touch is collected on the next update, ahead of any countdown; the
/// item then waits for its pickup message and, once the message closes, shows its empty block and leaves, or leaves its
/// shot block empty and respawns it.
/// </summary>
/// <param name="kind">The item or tank.</param>
/// <param name="presentation">How the item lies: exposed, in a Chozo orb or in a shot block.</param>
/// <param name="blockPair">The terrain block pair an item that shows its own picture draws into; null for a tank.</param>
/// <param name="phase">The phase the item's setup leaves it in.</param>
internal sealed class CollectibleObject(
    InWorldCollectibleKind kind,
    CollectiblePresentation presentation,
    CollectibleBlockPairs.Pair? blockPair,
    CollectiblePhase phase) : RoomObjectBehaviour
{
    /// <summary>Inventory or capacity item granted when this item is collected.</summary>
    public InWorldCollectibleKind Kind { get; } = kind;
    /// <summary>Exposed, Chozo orb, or shot-block reveal behavior.</summary>
    public CollectiblePresentation Presentation { get; } = presentation;
    /// <summary>
    /// The terrain block pair an item that shows its own picture writes its frames into (<see cref="CollectibleBlockPairs"/>);
    /// null for a tank, whose frames are its kind's own blocks.
    /// </summary>
    public CollectibleBlockPairs.Pair? BlockPair { get; } = blockPair;
    /// <summary>Current reveal, visible, message, empty, or respawn phase.</summary>
    public CollectiblePhase Phase { get; set; } = phase;
    /// <summary>Updates remaining before the item's next image.</summary>
    public int Timer { get; set; }
    /// <summary>Which of its two alternating looks the item shows next while it lies visible (#1994).</summary>
    internal CollectibleLook Look { get; private set; }
    /// <summary>Where the item's burst, reveal, hiding or respawn stands (<see cref="CollectibleSequenceAnimations"/>).</summary>
    private AnimationPlace _sequencePlace = AnimationPlace.StartOfAnimation;
    /// <summary>Where a Chozo orb's shell animation stands (<see cref="CollectibleOrbAnimation"/>).</summary>
    internal AnimationPlace OrbPlace { get; set; } = AnimationPlace.StartOfAnimation;
    /// <summary>Remaining paired visible frames before a revealed item is concealed again.</summary>
    public int VisibleFramePairsRemaining { get; set; }
    /// <summary>Contact or projectile reaction pending for the item's next update.</summary>
    public bool Triggered { get; set; }
    /// <summary>Distinguishes a previously acquired concealed item during its reveal.</summary>
    public bool WasCollectedBeforeReveal { get; set; }

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        // Collision runs before the room-object update. The item trigger its setup wrote, for touch and shot, therefore
        // pre-empts an outstanding countdown on this same update; otherwise touching an item could leave Samus embedded
        // in its solid block for up to four extra updates.
        bool triggerPreemptsTimer = Triggered && Phase is
            CollectiblePhase.Visible or
            CollectiblePhase.ShotBlockVisible or
            CollectiblePhase.ChozoOrb or
            CollectiblePhase.ShotBlock or
            CollectiblePhase.CollectedShotBlock;
        if (!triggerPreemptsTimer && Timer > 0)
        {
            Timer--;
            if (Timer > 0)
                return;
        }

        switch (Phase)
        {
            case CollectiblePhase.AwaitingMessage:
                return;

            case CollectiblePhase.ResumeAfterMessage:
                FinishPickup(context.RoomObjects.Drawing, context.Level, place);
                return;

            case CollectiblePhase.EmptyAwaitingDelete:
                // The empty item ends by leaving the room.
                place.Delete();
                return;

            case CollectiblePhase.CollectedEmpty:
                context.Draw(place, RoomObjectCollectibleDrawDefinitions.Empty);
                place.Delete();
                return;

            case CollectiblePhase.Visible:
                if (Triggered)
                {
                    context.RoomObjects.CollectibleObjects.Acquire(this, place);
                    return;
                }
                DrawVisible(context, place);
                ChangeLook();
                Timer = CollectibleTimingDefinitions.VisibleImageUpdates;
                return;

            case CollectiblePhase.ShotBlockVisible:
                StepRevealed(place, context);
                return;

            case CollectiblePhase.ChozoOrb:
                if (Triggered)
                {
                    // Breaking the shell persists nothing; only the unused Chozo blocks record theirs.
                    Triggered = false;
                    Phase = CollectiblePhase.ChozoOrbBurst;
                    StartSequence(CollectibleSequence.OrbBurst, place, context);
                    return;
                }
                ShowNextOrbImage(place, context);
                return;

            case CollectiblePhase.ChozoOrbBurst:
                if (ShowNextImage(CollectibleSequence.OrbBurst, place, context))
                    return;
                Phase = CollectiblePhase.Visible;
                Look = CollectibleLook.Primary;
                if (Triggered)
                {
                    context.RoomObjects.CollectibleObjects.Acquire(this, place);
                    return;
                }
                DrawVisible(context, place);
                ChangeLook();
                Timer = CollectibleTimingDefinitions.VisibleImageUpdates;
                return;

            case CollectiblePhase.ShotBlock:
            case CollectiblePhase.CollectedShotBlock:
                if (!Triggered)
                    return;
                Triggered = false;
                WasCollectedBeforeReveal = Phase == CollectiblePhase.CollectedShotBlock;
                Phase = CollectiblePhase.ShotBlockReveal;
                StartSequence(CollectibleSequence.ShotBlockReveal, place, context);
                context.QueueSound(EnemySound.BlockCrumble, SoundRequestPriority.Normal);
                return;

            case CollectiblePhase.ShotBlockReveal:
                if (ShowNextImage(CollectibleSequence.ShotBlockReveal, place, context))
                    return;
                if (WasCollectedBeforeReveal)
                {
                    Phase = CollectiblePhase.CollectedShotBlockEmpty;
                    context.Draw(place, RoomObjectCollectibleDrawDefinitions.Empty);
                    Timer = CollectibleTimingDefinitions.CollectedShotBlockEmptyUpdates;
                    return;
                }
                Phase = CollectiblePhase.ShotBlockVisible;
                Look = CollectibleLook.Primary;
                VisibleFramePairsRemaining = CollectibleTimingDefinitions.RevealedImagePairs;
                StepRevealed(place, context);
                return;

            case CollectiblePhase.ShotBlockReconceal:
                if (ShowNextImage(CollectibleSequence.ShotBlockConceal, place, context))
                    return;
                // The shot block shows whole as its saved block returns over it on the same update.
                context.Draw(place, RoomObjectCollectibleDrawDefinitions.ShotBlockIntact);
                context.DrawRestoredBlock(place);
                Phase = CollectiblePhase.ShotBlock;
                return;

            case CollectiblePhase.CollectedShotBlockEmpty:
                Phase = CollectiblePhase.CollectedShotBlockRespawn;
                StartSequence(CollectibleSequence.ShotBlockRespawn, place, context);
                return;

            case CollectiblePhase.CollectedShotBlockRespawn:
                if (ShowNextImage(CollectibleSequence.ShotBlockRespawn, place, context))
                    return;
                context.DrawRestoredBlock(place);
                Phase = CollectiblePhase.CollectedShotBlock;
                return;

            default:
                throw new InvalidDataException($"Collectible {Kind} reached unsupported phase {Phase}.");
        }
    }

    /// <summary>
    /// The pickup message has closed: the item shows its empty block for one update and then leaves, or a shot block's
    /// item leaves its block empty for twenty-two eight-update intervals before it respawns.
    /// </summary>
    internal void FinishPickup(IRoomObjectDrawing drawing, RoomLevelData level, RoomObjectPlace place)
    {
        Ensure.NotNull(drawing);
        Ensure.NotNull(place);
        if (Presentation == CollectiblePresentation.ShotBlock)
        {
            Triggered = false;
            WasCollectedBeforeReveal = true;
            Phase = CollectiblePhase.CollectedShotBlockEmpty;
            drawing.Draw(level, place.Block, RoomObjectCollectibleDrawDefinitions.Empty);
            Timer = CollectibleTimingDefinitions.CollectedShotBlockEmptyUpdates;
            return;
        }

        // The empty item draws for one update; it leaves on the following update.
        drawing.Draw(level, place.Block, RoomObjectCollectibleDrawDefinitions.Empty);
        Phase = CollectiblePhase.EmptyAwaitingDelete;
        Timer = CollectibleTimingDefinitions.PickedUpEmptyUpdates;
    }

    /// <summary>
    /// A revealed item: collected when touched; otherwise it shows its next visible image, and after its last pair its shot
    /// block starts to conceal it again.
    /// </summary>
    private void StepRevealed(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (Triggered)
        {
            context.RoomObjects.CollectibleObjects.Acquire(this, place);
            return;
        }
        DrawVisible(context, place);
        if (Look == CollectibleLook.Alternate && --VisibleFramePairsRemaining == 0)
        {
            // The hiding's first image shows once this look's time is up.
            Phase = CollectiblePhase.ShotBlockReconceal;
            _sequencePlace = AnimationPlace.StartOfAnimation;
            Timer = CollectibleTimingDefinitions.RevealImageUpdates;
            return;
        }
        ChangeLook();
        Timer = CollectibleTimingDefinitions.VisibleImageUpdates;
    }

    /// <summary>Plays the orb's shell animation on to its next image, draws it and holds it for its updates.</summary>
    /// <exception cref="InvalidOperationException">The animation showed no image, which a looping animation of images never does.</exception>
    private void ShowNextOrbImage(RoomObjectPlace place, RoomObjectStepContext context)
    {
        AnimationPlace orbPlace = OrbPlace;
        AnimationShown<CollectibleOrbSegment, CollectibleDraw> shown = CollectibleOrbAnimation.Segments.PlayToNextFrame(
            CollectibleOrbAnimation.Pulsing, ref orbPlace, static _ => { });
        if (shown is not AnimationShown<CollectibleOrbSegment, CollectibleDraw>.Frame image)
            throw new InvalidOperationException($"The Chozo orb's animation showed {shown} instead of its next image.");
        OrbPlace = orbPlace;
        context.Draw(place, RoomObjectDraw.Of(image.Shown));
        Timer = image.Duration;
    }

    /// <summary>
    /// Draws the item's visible image: the terrain its kind and block pair write, and over it, for an item that shows its
    /// own picture, its kind's picture of that image (#1538).
    /// </summary>
    private void DrawVisible(RoomObjectStepContext context, RoomObjectPlace place)
    {
        context.Draw(place, CollectibleObjectDraws.VisibleFrame(Kind, Look, BlockPair));
        if (BlockPair is not null)
            context.Level.ShowCollectible(place.Block, new CollectiblePicture(Kind, Look));
    }

    /// <summary>Changes the visible item to its other look for its next image.</summary>
    private void ChangeLook() =>
        Look = Look == CollectibleLook.Primary ? CollectibleLook.Alternate : CollectibleLook.Primary;

    /// <summary>Plays <paramref name="sequence"/> from its first image, which shows on this update.</summary>
    /// <exception cref="InvalidOperationException">The animation showed no image.</exception>
    private void StartSequence(CollectibleSequence sequence, RoomObjectPlace place, RoomObjectStepContext context)
    {
        _sequencePlace = AnimationPlace.StartOfAnimation;
        if (!ShowNextImage(sequence, place, context))
            throw new InvalidOperationException($"The collectible's {sequence} ended before it showed an image.");
    }

    /// <summary>
    /// Plays <paramref name="sequence"/> on to its next image, draws it and holds it for its updates; false once the
    /// animation is over.
    /// </summary>
    /// <exception cref="InvalidOperationException">The animation showed neither an image nor its end.</exception>
    private bool ShowNextImage(CollectibleSequence sequence, RoomObjectPlace place, RoomObjectStepContext context)
    {
        switch (CollectibleSequenceAnimations.Segments.PlayToNextFrame(
            CollectibleSequenceAnimations.Animations[sequence], ref _sequencePlace, static _ => { }))
        {
            case AnimationShown<CollectibleSequence, CollectibleImage>.Frame image:
                context.Draw(place, image.Shown.Draw);
                Timer = image.Duration;
                return true;
            case AnimationShown<CollectibleSequence, CollectibleImage>.Ended:
                return false;
            case var shown:
                throw new InvalidOperationException($"The collectible's {sequence} showed {shown}.");
        }
    }
}
