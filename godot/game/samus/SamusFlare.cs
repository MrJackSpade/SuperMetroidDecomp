using Engine.Actors.Components;
using Engine.Shared;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// The flare at the end of Samus's arm cannon. While she holds a charge: a glow that swells, and two rings of sparks
/// that close in on it, one slower than the other. While her grapple beam is out: the glow alone, where the line
/// leaves her. With the hyper beam: the three counted back from where they stand, after each hyper shot. It is drawn
/// in front of her, from the pictures of the beams she has switched on, in her colours.
/// </summary>
/// <remarks>
/// <para>Its three parts each keep their own place in their own timeline. All three start as the charge starts, on
/// the first update it is held. Nothing of it shows or moves on until the charge has been held "shows_from" updates:
/// from then the glow moves on a step an update and shows; from "sparks_from" both rings do too. It moves on by her
/// updates, not by the charge's count, so it goes on through an update on which the charge stands still. It shows
/// only while her pose holds her arm out; hidden, it still moves on. A charge let go, fired or lost ends it.</para>
/// <para>The grapple's glow is on a count of its own. It is taken up where the glow's timeline loops, on the update
/// the line is fired, and moves on a step an update for as long as the line is out; the charge's flare neither shows
/// nor moves on meanwhile.</para>
/// <para>A part moves on before it is shown, so the first picture of each shows an update less than its step says,
/// as the cartridge's does.</para>
/// <para>With the hyper beam on, while she holds a charge or a hyper shot's count runs (<see cref="Samus.ChargeReaches"/>),
/// every part counts its timeline's steps back, through its loop as written and no further than its first step, each
/// step for "hyper_picture_updates" and the first for "hyper_first_picture_updates", and once past its first it shows
/// nothing. A hyper shot starts each part on its last step (<see cref="CountBackFromLast"/>); otherwise, as with a
/// charge kept through the gift of the hyper beam, each part is taken over where it stands, its step keeping one update
/// less of what it had left, and at least one. The fast ring stepping back onto its first picture ends the count and
/// whatever charge she held (<see cref="Samus.FlareCountedBack"/>); every part still shows on that update. Counted back,
/// the fast ring is in front of the slow one, in front of the glow.</para>
/// <para>Its data, "charge_flare" in hers: "shows_from" and "sparks_from"; "hyper_picture_updates" and
/// "hyper_first_picture_updates"; the timelines "main", and "slow_right", "slow_left", "fast_right" and "fast_left"
/// (a ring's two are the same steps with the pictures of each way she can face), whose steps name pictures by their
/// names alone; and "pictures", by the name of her beams, the "folder" those pictures are in for them and where a
/// picture's corner lies from the flare's place ("x", "y").</para>
/// </remarks>
public partial class SamusFlare : Node2D
{
    /// <summary>One part of the flare: its picture, its timeline for each way she faces, and how far along it is.</summary>
    private sealed class Part(ActorVisual picture, FrameTimeline right, FrameTimeline left)
    {
        public ActorVisual Picture { get; } = picture;

        public TimelinePlayer? Playing { get; private set; }

        /// <summary>The updates the step it is on has left as it plays forward, as its player counts them: what a count back takes over.</summary>
        private int _stepLeft;

        /// <summary>
        /// Its count back, while it counts back: the step it shows, or -1 once it has stepped back past its first and
        /// shows nothing, and the updates that step has left. Null while it plays forward.
        /// </summary>
        private (int Step, int Left)? _back;

        /// <summary>Whether it counts its steps back.</summary>
        public bool CountsBack => _back is not null;

        public void Start() => Begin(new TimelinePlayer(right), 0);

        public void StartAt(int step) => Begin(TimelinePlayer.Resuming(right, step, 0), step);

        private void Begin(TimelinePlayer playing, int step)
        {
            Playing = playing;
            _stepLeft = right.Steps[step].Updates;
            _back = null;
        }

        /// <summary>Moves it on by an update, forward.</summary>
        public void Step()
        {
            TimelinePlayer playing = Playing ?? throw new InvalidOperationException("A part of the flare that has not started does not move on.");
            playing.Step();
            if (--_stepLeft == 0)
                _stepLeft = right.Steps[playing.StepIndex].Updates;
        }

        public void End()
        {
            Playing = null;
            _back = null;
            Picture.SetShown(false);
        }

        /// <summary>Starts counting back from its last step, which it shows for <paramref name="updates"/>.</summary>
        public void CountBackFromLast(int updates) => _back = (right.Steps.Count - 1, updates);

        /// <summary>Starts counting back from the step it stands on, which keeps one update less of what it had left, and at least one.</summary>
        public void CountBackFromHere()
        {
            if (Playing is null)
                Start();
            _back = (Playing!.StepIndex, Math.Max(_stepLeft - 1, 1));
        }

        /// <summary>
        /// Counts back by an update: once its step's updates run out it steps back to the step before, shown for
        /// <paramref name="stepUpdates"/>, or for <paramref name="firstStepUpdates"/> when that is the first. Returns
        /// whether it stepped back onto its first step.
        /// </summary>
        public bool StepBack(int stepUpdates, int firstStepUpdates)
        {
            (int Step, int Left) back = _back ?? throw new InvalidOperationException("A part of the flare that is not counting back does not step back.");
            if (back.Step < 0)
                return false;
            if (--back.Left > 0)
            {
                _back = back;
                return false;
            }

            int before = back.Step - 1;
            _back = (before, before == 0 ? firstStepUpdates : stepUpdates);
            return before == 0;
        }

        /// <summary>The name of the picture it is on as it plays forward, for the way she faces.</summary>
        public string Shown(bool facingRight) =>
            (facingRight ? right : left).Steps[(Playing ?? throw new InvalidOperationException("A part of the flare that has not started shows nothing.")).StepIndex].Picture;

        /// <summary>The name of the picture it is on as it counts back, for the way she faces; null once it has stepped back past its first.</summary>
        public string? ShownBack(bool facingRight)
        {
            (int Step, int Left) back = _back ?? throw new InvalidOperationException("A part of the flare that is not counting back shows nothing of a count back.");
            return back.Step < 0 ? null : (facingRight ? right : left).Steps[back.Step].Picture;
        }
    }

    private Part? _main, _slow, _fast, _grapple;
    private int _grappleFrom;
    private bool _bound;
    private int _showsFrom, _sparksFrom;
    private int _backStepUpdates, _backFirstStepUpdates;
    private readonly Dictionary<string, (string Folder, Vector2I Offset)> _pictures = new(StringComparer.Ordinal);

    /// <summary>Reads the flare's data, and makes its pictures: the glow in front of the slow ring, in front of the fast one.</summary>
    public void ReadValues(Engine.Content.DataNode data)
    {
        ArgumentNullException.ThrowIfNull(data);
        _bound = false;
        (_showsFrom, _sparksFrom) = (data.Int("shows_from"), data.Int("sparks_from"));
        (_backStepUpdates, _backFirstStepUpdates) = (data.Int("hyper_picture_updates"), data.Int("hyper_first_picture_updates"));
        if (_backStepUpdates < 1 || _backFirstStepUpdates < 1)
            throw data.Problem("gives a step of the hyper beam's count back fewer than one update");
        _pictures.Clear();
        foreach ((string beams, Engine.Content.DataNode pictures) in data.Get("pictures").Entries())
            _pictures[beams] = (pictures.Text("folder"), new Vector2I(pictures.Int("x"), pictures.Int("y")));
        foreach (Node old in GetChildren())
        {
            RemoveChild(old);
            old.QueueFree();
        }

        Part Made(string name, string right, string left)
        {
            (FrameTimeline toRight, FrameTimeline toLeft) = (FrameTimeline.Read(data.Get(right)), FrameTimeline.Read(data.Get(left)));
            if (toRight.Steps.Count != toLeft.Steps.Count)
                throw data.Problem($"gives \"{right}\" {toRight.Steps.Count} steps and \"{left}\" {toLeft.Steps.Count}; a part's two timelines are the same steps");
            var picture = new ActorVisual { Name = name, Visible = false };
            AddChild(picture);
            return new Part(picture, toRight, toLeft);
        }

        // What is added later is drawn in front.
        _fast = Made("FastSparks", "fast_right", "fast_left");
        _slow = Made("SlowSparks", "slow_right", "slow_left");
        _main = Made("Glow", "main", "main");
        _grapple = Made("GrappleGlow", "main", "main");
        _grappleFrom = FrameTimeline.Read(data.Get("main")).LoopsTo
            ?? throw data.Problem("gives \"main\" no step it loops to, which is where the grapple's glow takes it up");
    }

    /// <summary>A hyper shot: every part of the flare counts back from its last step.</summary>
    public void CountBackFromLast()
    {
        if (_main is not { } main || _slow is not { } slow || _fast is not { } fast)
            throw new InvalidOperationException("Samus's flare counts a hyper shot back before it has read its data.");
        foreach (Part part in (Part[])[fast, slow, main])
            part.CountBackFromLast(_backStepUpdates);
    }

    /// <summary>Plays one update of the flare, once her weapons have counted the charge and she is where the update leaves her.</summary>
    public void Step(Samus samus)
    {
        ArgumentNullException.ThrowIfNull(samus);
        if (_main is not { } main || _slow is not { } slow || _fast is not { } fast || _grapple is not { } grapple)
            return;
        if (!_bound)
        {
            // Its pictures come from the room's content, which it first has once she is in a room.
            foreach (Part part in (Part[])[main, slow, fast, grapple])
                part.Picture.Bind(samus.Room.Content);
            _bound = true;
        }

        string beams = samus.BeamsSwitchedOn;
        if (!_pictures.TryGetValue(beams, out (string Folder, Vector2I Offset) pictures))
            throw new InvalidOperationException($"Samus's data has no flare for the beams \"{beams}\".");
        if (samus.Grapple is { ShowsFlare: true })
        {
            // The charge's flare stands as it is, unseen, while the grapple's shows.
            main.Picture.SetShown(false);
            slow.Picture.SetShown(false);
            fast.Picture.SetShown(false);
            if (grapple.Playing is null)
                grapple.StartAt(_grappleFrom);
            grapple.Step();
            PixelPosition lies = samus.GrappleFlarePoint;
            grapple.Picture.SetShown(true);
            grapple.Picture.PictureOffset = pictures.Offset;
            grapple.Picture.Position = new Vector2(lies.X.Whole - samus.Position.X.Whole, lies.Y.Whole - samus.Position.Y.Whole);
            grapple.Picture.Show($"{pictures.Folder}/{grapple.Shown(facingRight: true)}.png");
            samus.Colours.ShowOn(grapple.Picture);
            return;
        }

        grapple.End();
        if (!samus.ChargeReaches(1))
        {
            main.End();
            slow.End();
            fast.End();
            return;
        }

        Vector2 at = new(samus.FlarePoint.X.Whole - samus.Position.X.Whole, samus.FlarePoint.Y.Whole - samus.Position.Y.Whole);
        // Moonwalking, the sparks are the other side's: the cartridge's moonwalk poses are set down as facing away
        // from the way she aims, and its flare takes its sparks by the pose's facing.
        bool armOut = samus.HoldsArmOut, facingRight = (samus.Facing == Facing.Right) != (samus.State is MoonwalkingState);
        void Show(Part part, string? picture)
        {
            part.Picture.SetShown(armOut && picture is not null);
            if (!armOut || picture is null)
                return;
            part.Picture.PictureOffset = pictures.Offset;
            part.Picture.Position = at;
            part.Picture.Show($"{pictures.Folder}/{picture}.png");
            samus.Colours.ShowOn(part.Picture);
        }

        if (samus.Inventory.IsOn(Equipment.HyperBeam))
        {
            // Every part counts back, the fast ring first; the gift of the hyper beam takes each over where it stands.
            foreach (Part part in (Part[])[fast, slow, main])
            {
                if (!part.CountsBack)
                    part.CountBackFromHere();
            }

            bool ended = fast.StepBack(_backStepUpdates, _backFirstStepUpdates);
            slow.StepBack(_backStepUpdates, _backFirstStepUpdates);
            main.StepBack(_backStepUpdates, _backFirstStepUpdates);
            Stack(main, slow, fast);
            foreach (Part part in (Part[])[fast, slow, main])
                Show(part, part.ShownBack(facingRight));
            if (ended)
                samus.FlareCountedBack();
            return;
        }

        int held = samus.Weapons.ChargedFor;
        if (held == 1 || main.Playing is null)
        {
            main.Start();
            slow.Start();
            fast.Start();
        }

        void Play(Part part, int from)
        {
            if (held < from)
            {
                part.Picture.SetShown(false);
                return;
            }

            part.Step();
            Show(part, armOut ? part.Shown(facingRight) : null);
        }

        Stack(fast, slow, main);
        Play(main, _showsFrom);
        Play(slow, _sparksFrom);
        Play(fast, _sparksFrom);
    }

    /// <summary>Puts the three parts' pictures in the order given, from the one furthest back to the one in front.</summary>
    private void Stack(Part back, Part middle, Part front)
    {
        MoveChild(back.Picture, 0);
        MoveChild(middle.Picture, 1);
        MoveChild(front.Picture, 2);
    }
}
