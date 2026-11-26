using Engine.Actors;
using Engine.Actors.Components;
using Engine.Content;
using Engine.Input;
using Engine.Shared;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// Shows Samus: the timeline for what she is doing, the way she faces and where she aims, in the colours she shows
/// (<see cref="SamusColours"/>), with her flicker after a hit. It is stepped once per update, after she has acted, and only shows; nothing
/// in play reads it.
/// </summary>
/// <remarks>
/// Her timelines are named for her look, then the way she faces, then her aim: "standing_right_up". Where she has no
/// timeline of the whole name, the one without the aim is used, and failing that the one of the look alone. While
/// her energy is low, a timeline of any of those names with "_low" after it is used before them, and wearing the
/// varia or the gravity suit, one with "_suited" after it before those. A step's own offset places its picture, as
/// in any timeline.
/// <para>
/// As her look changes from one to another, a timeline named for both, then the way she faces, is played once
/// between them if her data has one: "falling_to_standing_right" is her landing. It is only shown; what she is doing
/// has already changed.
/// </para>
/// </remarks>
public partial class SamusVisual : Node2D
{
    /// <summary>How many updates pass between one echo being left and the next, how fast they close on her when she slows, and how fast they draw level.</summary>
    private const int EchoEvery = 4, EchoDepartureSpeed = 8, EchoLevelling = 2;

    private TimelinePlayer? _player;
    private string _playing = "";
    private string _look = "";
    private bool _passing;
    private TimelineStep? _shown;
    private readonly PixelPosition?[] _echoes = new PixelPosition?[2];
    private int _nextEcho;
    private int _echoesLeaving;

    /// <summary>Shows <paramref name="samus"/> as she is at the end of this update.</summary>
    public void Show(Samus samus)
    {
        ActorVisual? picture = GetNodeOrNull<ActorVisual>("Picture");
        if (picture is null)
            return;

        string look = SamusLooks.Of(samus);
        string name = TimelineFor(samus, look);
        if (look != _look)
        {
            // A change of look may pass through a picture or two of its own before the new look's are shown.
            // The pictures between may be their own for the way she holds her weapon as the look changes: her aim's,
            // or, aiming level with the attack held, the firing ones.
            string passing = $"{_look}_to_{look}_{(samus.Facing == Facing.Right ? "right" : "left")}";
            string held = samus.Aim != Aim.Forward ? Samus.AimName(samus.Aim) : samus.Input.IsHeld(GameAction.Attack) ? "firing" : "";
            if (held.Length > 0 && samus.Timelines.Has($"{passing}_{held}"))
                passing = $"{passing}_{held}";
            _passing = _look.Length > 0 && samus.Timelines.Has(passing);
            _look = look;
            if (_passing)
                name = passing;
            _playing = "";
        }
        else if (_passing)
        {
            if (_player is { IsOver: false })
                name = _playing;
            else
                _passing = false;
        }

        // Riding an elevator she is drawn on the even counts of the clock only.
        if (name.Length == 0 || (samus.State is RidingElevatorState && samus.Room.Session.Clock.Updates % 2 != 0))
        {
            _shown = null;
            picture.Show("");
            return;
        }

        if (name != _playing || _player is null)
        {
            _playing = name;
            _player = new TimelinePlayer(samus.Timelines.Get(name));
        }
        else if (!ShowsStride(samus))
        {
            _player.Step();
            if (_passing && _player.IsOver)
            {
                // The passing pictures are done within this update: the look's own begin at once.
                _passing = false;
                _playing = TimelineFor(samus, look);
                if (_playing.Length == 0)
                {
                    _shown = null;
                    picture.Show("");
                    return;
                }

                _player = new TimelinePlayer(samus.Timelines.Get(_playing));
            }
        }

        // Her animation's start is its first picture's first update, as on the cartridge, and is not stepped: so the
        // step the next update falls in is the one this update shows. Running, she shows the picture of the stride
        // play counts her boost by, so her stride quickens through the boost's stages as play's does.
        _shown = ShowsStride(samus) ? _player.Timeline.Steps[samus.StridePicture] : _player.Current;
        picture.Show(_shown);
        samus.Colours.ShowOn(picture);

        // After a hit she flickers, hidden on the clock's odd updates and shown on its even ones, but not while she
        // is thrown back or reads as holding a shine.
        picture.SetShown(!samus.IsInvincible || samus.Room.Session.Clock.Updates % 2 == 0 || samus.State is KnockedBackState || samus.Thrown is not null || samus.ReadsAsHoldingShine);
        StepEchoes(samus, picture);
    }

    /// <summary>Gives her pictures where they are loaded from. Called as she enters a room.</summary>
    public void Bind(ContentLibrary content)
    {
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Bind(content);
    }

    /// <summary>
    /// Leaves and moves her echoes. At full speed one of the two is left where she is every few updates, turn about,
    /// and while a shinespark flies, where she stood before it moved her. When she slows they close on her from behind,
    /// drawing level as they come, and are gone as they reach her. A shinespark's launch and its crash take them away.
    /// They are shown by the visuals "Echo1" and "Echo2", if her scene has them.
    /// </summary>
    private void StepEchoes(Samus samus, ActorVisual picture)
    {
        bool boosting = samus.IsBoosting && samus.State is not ShinesparkState;
        // Timed by the play time, not by the clock.
        bool due = samus.Room.Session.Progress.PlayUpdates % EchoEvery == 0;
        if (samus.State is ShinesparkState { IsWindingUp: false } spark)
        {
            if (spark.FlownFrom is not { } from)
            {
                (_echoes[0], _echoes[1], _nextEcho, _echoesLeaving) = (null, null, 0, 0);
            }
            else if (due)
            {
                _echoes[_nextEcho] = from;
                _nextEcho = 1 - _nextEcho;
            }
        }
        else if (samus.State is ShinesparkState && _echoesLeaving == 0)
        {
            // Winding up, echoes that are not closing on her stay where they were left.
        }
        else if (boosting && _echoesLeaving == 0)
        {
            if (due)
            {
                _echoes[_nextEcho] = samus.Position;
                _nextEcho = 1 - _nextEcho;
            }
        }
        else if (_echoes[0] is not null || _echoes[1] is not null)
        {
            if (_echoesLeaving == 0)
                _echoesLeaving = samus.Facing == Facing.Left ? -EchoDepartureSpeed : EchoDepartureSpeed;
            for (int echo = 0; echo < _echoes.Length; echo++)
            {
                if (_echoes[echo] is not { } at)
                    continue;
                int y = at.Y.Whole, level = samus.Position.Y.Whole;
                if (y != level)
                    y += y < level ? EchoLevelling : -EchoLevelling;
                int x = at.X.Whole + _echoesLeaving;
                bool reached = _echoesLeaving < 0 ? x < samus.Position.X.Whole : x >= samus.Position.X.Whole;
                _echoes[echo] = reached ? null : PixelPosition.FromWhole(x, y);
            }

            if (_echoes[0] is null && _echoes[1] is null)
                (_nextEcho, _echoesLeaving) = (0, 0);
        }
        else
        {
            _echoesLeaving = 0;
        }

        for (int echo = 0; echo < _echoes.Length; echo++)
        {
            if (GetNodeOrNull<ActorVisual>($"Echo{echo + 1}") is not { } shown)
                continue;
            if (_echoes[echo] is not { } at)
            {
                shown.Show("");
                continue;
            }

            ShowEchoOn(samus, shown, shows: true);
            shown.Position = new Vector2(at.X.Whole - samus.Position.X.Whole, at.Y.Whole - samus.Position.Y.Whole);
        }
    }

    /// <summary>
    /// Shows on <paramref name="echo"/> the whole picture her body last showed, in her colours: an echo of her repeats
    /// that and nothing of its own. It shows nothing when told not to, or before she has been shown.
    /// </summary>
    public void ShowEchoOn(Samus samus, ActorVisual echo, bool shows)
    {
        ArgumentNullException.ThrowIfNull(samus);
        ArgumentNullException.ThrowIfNull(echo);
        if (_shown is null || !shows)
        {
            echo.Show("");
            return;
        }

        echo.Show(_shown);
        samus.Colours.ShowOn(echo);
    }

    /// <summary>
    /// Shows her as her death has her, outside play's own update: the step given of one of her timelines, or the
    /// picture she last showed with none given, in her colours as they are now, whole and with no echo of her; or
    /// nothing of her at all.
    /// </summary>
    public void ShowDying(Samus samus, TimelineStep? step, bool shows)
    {
        ArgumentNullException.ThrowIfNull(samus);
        if (GetNodeOrNull<ActorVisual>("Picture") is not { } picture)
            return;
        if (step is not null)
            _shown = step;
        if (_shown is null || !shows)
        {
            picture.Show("");
        }
        else
        {
            picture.Show(_shown);
            samus.Colours.ShowOn(picture);
        }

        picture.SetShown(true);
        picture.Tint(Colors.White);
        ClearEchoes();
        foreach (string echo in (string[])["Echo1", "Echo2"])
            GetNodeOrNull<ActorVisual>(echo)?.Show("");
    }

    /// <summary>Whether her running look is shown by the picture of her stride play is at, rather than played by itself: while she runs, once any pictures passed through on the way into the run are done.</summary>
    private bool ShowsStride(Samus samus) => !_passing && samus.State is RunningState;

    /// <summary>Takes her echoes away at once, as switching the speed booster off on the pause screen does.</summary>
    public void ClearEchoes() => (_echoes[0], _echoes[1], _nextEcho, _echoesLeaving) = (null, null, 0, 0);


    /// <summary>The name of the timeline that shows her now, or empty when her data has none for it.</summary>
    private static string TimelineFor(Samus samus, string look)
    {
        string facing = samus.Facing == Facing.Right ? "right" : "left";
        string aim = samus.HoldName;
        // The timeline that says most exactly how she stands, faces and aims is the one shown: her aim's before the
        // one for the way she faces, that before the look's own. Each of those may have another in its stead for
        // the suit she wears (she faces out of the screen in another pose) or for being short of energy (she
        // breathes faster), which is tried first; such a one never takes the place of a more exact timeline, so
        // short of energy she still shows her aim.
        bool suited = samus.SuitWorn != "power", low = samus.Inventory.Amount(Amounts.Energy) <= samus.LowEnergy;
        foreach (string name in (string[])[$"{look}_{facing}_{aim}", $"{look}_{facing}", look])
        {
            string[] inItsStead = [name + "_suited_low", name + "_low", name + "_suited", name];
            bool[] applies = [suited && low, low, suited, true];
            for (int index = 0; index < inItsStead.Length; index++)
            {
                if (applies[index] && samus.Timelines.Has(inItsStead[index]))
                    return inItsStead[index];
            }
        }

        return "";
    }
}
