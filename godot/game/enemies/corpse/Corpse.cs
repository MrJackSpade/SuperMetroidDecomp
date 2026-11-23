using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// The husk of a creature the giant Metroid has drained. It is solid, and lies as it fell until something disturbs
/// it: Samus coming to rest against it, after a moment, or at once a shot, a bomb or her touch. Then it crumbles to
/// dust, row by row, and is gone.
/// </summary>
/// <remarks>Its data file names the timeline it lies in, "look", and how many "rows" of it there are to crumble.</remarks>
public partial class Corpse : Enemy
{
    /// <summary>The timeline it lies in.</summary>
    [Export, DataValue("look")]
    public string Look { get; set; } = "corpse";

    /// <summary>How many rows of it there are to crumble: the height of its picture.</summary>
    [Export, DataValue("rows")]
    public int Rows { get; set; } = 16;

    /// <summary>How many updates after Samus comes to rest against it that it starts to crumble.</summary>
    [Export, DataValue("disturbed_updates")]
    public int DisturbedUpdates { get; set; } = 16;

    /// <summary>The effect of the dust it crumbles into, by the data file of its timeline.</summary>
    [Export, DataValue("dust_effect")]
    public string DustEffect { get; set; } = "";

    /// <summary>The sound of it crumbling, by name.</summary>
    [Export, DataValue("dust_sound")]
    public string DustSound { get; set; } = "";

    /// <summary>Where its dust rises from, from its middle, in pixels.</summary>
    [Export]
    public Vector2I DustFrom { get; set; } = new(-14, 16);

    /// <summary>What has become of it.</summary>
    protected enum Phase
    {
        /// <summary>Lying undisturbed.</summary>
        Waiting,

        /// <summary>Disturbed, about to crumble.</summary>
        Disturbed,

        /// <summary>Crumbling.</summary>
        Crumbling,
    }

    private CorpseRot? _rot;
    private int _wait;

    /// <summary>What has become of it.</summary>
    protected Phase Now { get; private set; }

    /// <summary>Whether it can be disturbed at the moment. One that is not yet a husk says so here.</summary>
    protected virtual bool IsHusk => true;

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        if (!IsHusk)
            return base.TakeHit(hit, part);
        Crumble();
        return HitResult.Absorbed;
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (!IsHusk)
        {
            base.Touch(part);
            return;
        }

        Crumble();
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Now = Phase.Waiting;
        Play(Look);
    }

    /// <inheritdoc/>
    protected override void Behave() => StepHusk();

    /// <summary>Plays one update of it as a husk: waiting to be disturbed, or crumbling.</summary>
    protected void StepHusk()
    {
        switch (Now)
        {
            case Phase.Waiting:
                if (SamusRestsAgainstIt())
                    (Now, _wait) = (Phase.Disturbed, DisturbedUpdates);
                break;

            case Phase.Disturbed:
                if (--_wait <= 0)
                    Crumble();
                break;

            case Phase.Crumbling:
                StepCrumbling();
                break;

            default:
                throw new InvalidOperationException($"A corpse has no phase {Now}.");
        }
    }

    /// <summary>Starts it crumbling; nothing touches it from then on.</summary>
    protected void Crumble()
    {
        if (Now == Phase.Crumbling)
            return;
        Now = Phase.Crumbling;
        AlwaysActive = true;
        foreach (Collider part in GetChildren().OfType<Collider>())
            part.Enabled = false;
        _rot = new CorpseRot(Rows);
    }

    /// <summary>Crumbles it for one update. Returns whether any of it is left; with none left it is gone.</summary>
    protected bool StepCrumbling()
    {
        CorpseRot rot = _rot ?? throw new InvalidOperationException("The corpse is not crumbling.");
        bool left = rot.Step(LeaveDust);
        Visual?.ShowRows(rot.Rows);
        if (!left)
            Vanish();
        return left;
    }

    /// <summary>Leaves the puff of dust of a row reaching the ground.</summary>
    protected virtual void LeaveDust()
    {
        RandomDraw roll = Random.Last;
        int spread = roll.Part(1, 1) * 2 + roll.Part(3, 2) * 8;
        if (DustEffect.Length > 0)
            LeaveEffect(DustEffect, Position + PixelOffset.FromWhole(DustFrom.X + spread, DustFrom.Y));
        if (Room.Session.Clock.Updates % 8 == 0)
            Sound(DustSound);
    }

    /// <summary>Whether Samus is standing on it or pressed against it.</summary>
    private bool SamusRestsAgainstIt()
    {
        if (Solid is not { Enabled: true } solid)
            return false;
        PixelRect body = Room.Player.BodyRect;
        var reach = new PixelRect(body.Left - 1, body.Top - 1, body.Width + 2, body.Height + 2);
        return reach.Overlaps(solid.RoomRect);
    }
}
