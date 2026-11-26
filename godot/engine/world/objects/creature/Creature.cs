using Engine.Actors.Components;
using Engine.Actors.Movement;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.World.Objects;

/// <summary>
/// A harmless animal that moves by its own behaviour. It cannot be hurt and hurts nothing. A particular creature
/// supplies only its behaviour; the base shows the timelines its data names, passes it their cues, and moves it
/// through terrain.
/// </summary>
public abstract partial class Creature : RoomObject, IMoves
{
    private TimelinePlayer? _animation;

    /// <inheritdoc/>
    public PixelVelocity Velocity { get; set; }

    /// <inheritdoc/>
    public Facing Facing { get; set; }

    /// <inheritdoc/>
    public Collider MovingBody =>
        EntityNode.ColliderOf(this, ColliderRole.Body) ?? throw new InvalidOperationException($"The creature \"{Name}\" has no body collider.");

    /// <summary>The timelines its data names.</summary>
    public TimelineSet Timelines { get; private set; } = TimelineSet.None;

    /// <summary>The name of the timeline it is showing.</summary>
    protected string Playing { get; private set; } = "";

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        Timelines = TimelineSet.ReadFrom(data);
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Behave();
        StepAnimation();
    }

    /// <summary>This creature's own behaviour for one update.</summary>
    protected abstract void Behave();

    /// <summary>
    /// Called for each cue its timeline reaches that is its own. The base acts on one itself: "sound:" and a sound's
    /// name asks for that sound.
    /// </summary>
    protected virtual void OnCue(string cue)
    {
    }

    /// <summary>Shows the timeline named <paramref name="name"/> from its start. Naming the one showing changes nothing unless <paramref name="restart"/> is asked for.</summary>
    protected void Play(string name, bool restart = false)
    {
        if (name == Playing && !restart && _animation is not null)
            return;
        Playing = name;
        _animation = new TimelinePlayer(Timelines.Get(name));
        ShowPicture();
    }

    /// <summary>Asks for a sound; an empty name asks for none.</summary>
    protected void Sound(string sound)
    {
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "objects");
    }

    /// <summary>Moves it across by <paramref name="distance"/> and returns whether something stopped it.</summary>
    protected bool MoveAcrossBy(Pixels distance, SlopeTreatment slopes = SlopeTreatment.Wall) =>
        Room.Mover.MoveAcross(MovingBody, distance, MoveOptions.AmongBlocks with { Slopes = slopes }).Blocked;

    /// <summary>Moves it down by <paramref name="distance"/>, upward when negative, and returns whether something stopped it.</summary>
    protected bool MoveDownBy(Pixels distance, SlopeTreatment slopes = SlopeTreatment.Wall) =>
        Room.Mover.MoveDown(MovingBody, distance, MoveOptions.AmongBlocks with { Slopes = slopes }).Blocked;

    /// <summary>Shows or hides it.</summary>
    protected void SetShown(bool shown)
    {
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.SetShown(shown);
    }

    /// <summary>Moves its timeline on by one update and acts on the cues it reaches.</summary>
    private void StepAnimation()
    {
        if (_animation is not { } playing)
            return;
        foreach (string cue in playing.Step())
        {
            if (cue.StartsWith("sound:", StringComparison.Ordinal))
                Sound(cue["sound:".Length..]);
            else
                OnCue(cue);

            // A cue that started another timeline ends this one's turn.
            if (_animation != playing)
                return;
        }

        ShowPicture();
    }

    private void ShowPicture()
    {
        if (_animation is null)
            return;
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Show(_animation.Shown);
    }
}
