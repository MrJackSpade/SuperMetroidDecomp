using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.Ui;

/// <summary>
/// One thing shown in a cinematic: a picture at a place on the screen that plays a timeline, drifts if it is given a
/// speed, and is over when a timeline that does not loop has played out. Its cinematic steps it once an update and
/// acts on the cues it reports. Its picture can be shown with some of its colours changed, as any visual's can.
/// </summary>
/// <remarks>
/// It shows nothing until its first step. Each step of its timeline begins on one call and lasts that step's
/// updates; the cues of a step are reported by the call that begins it. A step with no picture hides it.
/// </remarks>
public partial class CinematicSprite : Node2D, IRecoloured
{
    /// <inheritdoc/>
    public void Recolour(string source, IReadOnlyList<ColourSwap> swaps) => _visual?.Recolour(source, swaps);

    /// <inheritdoc/>
    public bool ShowsPicturesIn(string place) => _visual?.ShowsPicturesIn(place) ?? false;

    private FrameTimeline? _timeline;
    private ActorVisual? _visual;
    private int _step = -1;
    private int _left;
    private int _wait;

    /// <summary>A sprite that will play <paramref name="timeline"/> at <paramref name="at"/>.</summary>
    public static CinematicSprite Of(ContentLibrary content, FrameTimeline timeline, PixelPosition at)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(timeline);
        var sprite = new CinematicSprite { Name = "Sprite", Place = at, _timeline = timeline };
        sprite._visual = new ActorVisual { Name = "Visual" };
        sprite.AddChild(sprite._visual);
        sprite._visual.Bind(content);
        sprite.ShowPlace();
        return sprite;
    }

    /// <summary>Where it is on the screen.</summary>
    public PixelPosition Place { get; set; }

    /// <summary>How far it drifts each update.</summary>
    public PixelVelocity Velocity { get; set; }

    /// <summary>
    /// The width of the loop it drifts round, in pixels: it is shown as many pixels across as it is along the loop,
    /// the far half of the loop lying to the left of the screen. Zero for one that drifts in a line.
    /// </summary>
    public int LoopWidth { get; set; }

    /// <summary>Whether its timeline, one that does not loop, has played out.</summary>
    public bool IsOver { get; private set; }

    /// <summary>Makes it do nothing for its next <paramref name="updates"/> steps.</summary>
    public void Delay(int updates)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(updates);
        _wait = updates;
    }

    /// <summary>Starts it on another timeline, from that timeline's first step.</summary>
    public void Play(FrameTimeline timeline)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        (_timeline, _step, _left, IsOver) = (timeline, -1, 0, false);
    }

    /// <summary>Tints its picture; white leaves it as it is.</summary>
    public void Tint(Color colour) => Modulate = colour;

    /// <summary>Plays one update and returns the cues of the step it began, if it began one.</summary>
    public IReadOnlyList<string> Step()
    {
        FrameTimeline timeline = _timeline ?? throw new InvalidOperationException("The sprite has no timeline.");
        if (IsOver)
            return [];
        Place += Velocity.OverOneUpdate;
        ShowPlace();
        if (_wait > 0)
        {
            _wait--;
            return [];
        }

        if (_step >= 0 && --_left > 0)
            return [];
        int next = _step + 1;
        if (next >= timeline.Steps.Count)
        {
            if (timeline.LoopsTo is not int loop)
            {
                IsOver = true;
                _visual?.Show("");
                return [];
            }

            next = loop;
        }

        _step = next;
        TimelineStep step = timeline.Steps[next];
        _left = step.Updates;
        _visual?.Show(step);
        return step.Cues;
    }

    /// <summary>Puts the node where its place says, round its loop if it has one.</summary>
    public void ShowPlace()
    {
        int across = Place.X.Whole;
        if (LoopWidth > 0)
        {
            int along = ((across % LoopWidth) + LoopWidth) % LoopWidth;
            across = along < LoopWidth / 2 ? along : along - LoopWidth;
        }

        Position = new Vector2(across, Place.Y.Whole);
    }
}
