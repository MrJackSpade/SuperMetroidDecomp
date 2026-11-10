using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>
/// One room sprite object: a program-driven decorative effect (dust, an explosion, a Metroid's shell, a Nuclear Waffle
/// link, one of Draygon's Evirs), created fresh each time one is spawned (#1591). Its spawner holds it when it moves,
/// recolours, freezes or ends it; the room's <see cref="RoomSpriteObjects"/> steps and draws it until it ends.
/// </summary>
public sealed class RoomSpriteObject
{
    /// <summary>Spawns an object of <paramref name="kind"/> at <paramref name="position"/>, showing its program's first frame.</summary>
    /// <param name="kind">What the object is, which selects the program it plays.</param>
    /// <param name="position">Where it stands, in whole room pixels.</param>
    /// <param name="graphics">The sheet characters and colours it draws with.</param>
    internal RoomSpriteObject(RoomSpriteObjectKind kind, Position16<RoomPixels> position, SpriteGraphics graphics)
    {
        Kind = kind;
        Position = FixedPosition<RoomPixels>.FromWhole(position);
        Graphics = graphics;
        Cursor = AnimationProgramCursor<RoomSpriteObjectProgram>.Start(RoomSpriteObjectDefinitions.Programs[kind]);
        AdvanceProgram();
    }

    /// <summary>What the object is, as its spawner chose it.</summary>
    public RoomSpriteObjectKind Kind { get; }

    /// <summary>Whether the object still runs its program; once it ends it never runs or shows again.</summary>
    public bool IsActive => Cursor is not null;

    /// <summary>
    /// The 16.16 room position, converted to screen coordinates by subtracting the camera when drawn. Most
    /// sprite objects move only in whole pixels and leave the fractions zero; Draygon's six burial Evirs move by
    /// fractional speeds, so discarding the fractions visibly changes their fan-in.
    /// </summary>
    public FixedPosition<RoomPixels> Position { get; internal set; }

    /// <summary>The sheet characters and colours the object draws with.</summary>
    public SpriteGraphics Graphics { get; internal set; }

    /// <summary>Where the object's program resumes, or null once it has ended.</summary>
    internal AnimationProgramCursor<RoomSpriteObjectProgram>? Cursor { get; private set; }

    /// <summary>Updates left showing <see cref="Frame"/> before the program moves on; not counted while the object <see cref="Holds"/>.</summary>
    public int Timer { get; private set; }

    /// <summary>Whether the program has reached its hold: the object shows <see cref="Frame"/> from then on until it ends.</summary>
    public bool Holds { get; private set; }

    /// <summary>The frame the object shows.</summary>
    internal RoomSpriteObjectFrame Frame { get; private set; }

    /// <summary>Suppresses program updates but leaves drawing intact.</summary>
    public bool UpdatesDisabled { get; internal set; }

    /// <summary>Ends the object: it stops running and showing.</summary>
    internal void End() => Cursor = null;

    /// <summary>Counts one update off the shown frame and moves the program on when it runs out, unless updates are disabled or the object holds.</summary>
    internal void Step()
    {
        if (!IsActive || UpdatesDisabled || Holds)
            return;
        if (--Timer == 0)
            AdvanceProgram();
    }

    /// <summary>
    /// Runs the program to its next frame: a timed frame installs its frame and duration, the hold keeps the frame for
    /// good, and delete ends the object.
    /// </summary>
    private void AdvanceProgram()
    {
        AnimationProgramCursor<RoomSpriteObjectProgram> cursor = Cursor ?? throw new InvalidOperationException(
            $"Sprite object {Kind} has no program to advance.");
        WrappingCountdown counter = default;
        AnimationProgramAdvance<RoomSpriteObjectFrame> advance =
            RoomSpriteObjectAnimations.Programs.Advance(ref cursor, ref counter, static (_, _) => null);
        switch (advance)
        {
            case AnimationProgramAdvance<RoomSpriteObjectFrame>.Show shown:
                Cursor = cursor;
                Frame = shown.Frame;
                Timer = shown.Duration;
                return;
            case AnimationProgramAdvance<RoomSpriteObjectFrame>.Hold:
                Cursor = cursor;
                Holds = true;
                return;
            case AnimationProgramAdvance<RoomSpriteObjectFrame>.Stop:
                End();
                return;
            default:
                throw new InvalidOperationException($"Sprite object program {cursor.Program} advanced with {advance}.");
        }
    }

    /// <summary>
    /// Points the object at <paramref name="program"/> past its first frame and shows that frame, keeping the current
    /// countdown: the frozen Metroid shell shows the program's first frame without reloading the timer, so the next
    /// expiry continues at the program's second step.
    /// </summary>
    /// <param name="program">The program the object plays from now on.</param>
    internal void ShowFirstFrameKeepingTimer(RoomSpriteObjectProgram program)
    {
        if (!IsActive)
            throw new InvalidOperationException($"Sprite object {Kind} has ended and shows no frame.");
        if (Holds)
            throw new InvalidOperationException($"Sprite object {Kind} holds its frame and has no countdown to keep.");
        AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue> first =
            RoomSpriteObjectAnimations.Programs.Steps(program)[0];
        if (first is not AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.ShowStep show)
            throw new InvalidOperationException($"Sprite object program {program} does not begin with a timed frame.");
        Cursor = new(program, 1);
        Frame = show.Frame;
    }
}
