using Engine.Actors.Components;
using Engine.Shared;
using Godot;

namespace Game;

/// <summary>
/// The flare at the end of Samus's arm cannon. While she holds a charge: a glow that swells, and two rings of sparks
/// that close in on it, one slower than the other. While her grapple beam is out: the glow alone, where the line
/// leaves her. It is drawn in front of her, from the pictures of the beams she has switched on, in her colours.
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
/// <para>Its data, "charge_flare" in hers: "shows_from" and "sparks_from"; the timelines "main", and "slow_right",
/// "slow_left", "fast_right" and "fast_left" (a ring's two are the same steps with the pictures of each way she can
/// face), whose steps name pictures by their names alone; and "pictures", by the name of her beams, the "folder"
/// those pictures are in for them and where a picture's corner lies from the flare's place ("x", "y").</para>
/// </remarks>
public partial class SamusFlare : Node2D
{
    /// <summary>One part of the flare: its picture, its timeline for each way she faces, and how far along it is.</summary>
    private sealed class Part(ActorVisual picture, FrameTimeline right, FrameTimeline left)
    {
        public ActorVisual Picture { get; } = picture;

        public TimelinePlayer? Playing { get; private set; }

        public void Start() => Playing = new TimelinePlayer(right);

        public void StartAt(int step) => Playing = TimelinePlayer.Resuming(right, step, 0);

        public void End()
        {
            Playing = null;
            Picture.SetShown(false);
        }

        /// <summary>The name of the picture it is on, for the way she faces.</summary>
        public string Shown(bool facingRight) =>
            (facingRight ? right : left).Steps[(Playing ?? throw new InvalidOperationException("A part of the flare that has not started shows nothing.")).StepIndex].Picture;
    }

    private Part? _main, _slow, _fast, _grapple;
    private int _grappleFrom;
    private bool _bound;
    private int _showsFrom, _sparksFrom;
    private readonly Dictionary<string, (string Folder, Vector2I Offset)> _pictures = new(StringComparer.Ordinal);

    /// <summary>Reads the flare's data, and makes its pictures: the glow in front of the slow ring, in front of the fast one.</summary>
    public void ReadValues(Engine.Content.DataNode data)
    {
        ArgumentNullException.ThrowIfNull(data);
        _bound = false;
        (_showsFrom, _sparksFrom) = (data.Int("shows_from"), data.Int("sparks_from"));
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
            grapple.Playing!.Step();
            PixelPosition lies = samus.GrappleFlarePoint;
            grapple.Picture.SetShown(true);
            grapple.Picture.PictureOffset = pictures.Offset;
            grapple.Picture.Position = new Vector2(lies.X.Whole - samus.Position.X.Whole, lies.Y.Whole - samus.Position.Y.Whole);
            grapple.Picture.Show($"{pictures.Folder}/{grapple.Shown(facingRight: true)}.png");
            samus.Colours.ShowOn(grapple.Picture);
            return;
        }

        grapple.End();
        int held = samus.Weapons.ChargedFor;
        if (held == 0)
        {
            main.End();
            slow.End();
            fast.End();
            return;
        }

        if (held == 1 || main.Playing is null)
        {
            main.Start();
            slow.Start();
            fast.Start();
        }

        Vector2 at = new(samus.FlarePoint.X.Whole - samus.Position.X.Whole, samus.FlarePoint.Y.Whole - samus.Position.Y.Whole);
        bool armOut = samus.HoldsArmOut, facingRight = samus.Facing == Facing.Right;
        void Play(Part part, int from)
        {
            if (held < from)
            {
                part.Picture.SetShown(false);
                return;
            }

            part.Playing!.Step();
            part.Picture.SetShown(armOut);
            if (!armOut)
                return;
            part.Picture.PictureOffset = pictures.Offset;
            part.Picture.Position = at;
            part.Picture.Show($"{pictures.Folder}/{part.Shown(facingRight)}.png");
            samus.Colours.ShowOn(part.Picture);
        }

        Play(main, _showsFrom);
        Play(slow, _sparksFrom);
        Play(fast, _sparksFrom);
    }
}
