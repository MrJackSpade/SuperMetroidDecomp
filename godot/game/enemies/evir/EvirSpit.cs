using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// An Evir's spine ball. It sits in the Evir's grip until Samus comes near across, flies straight at where she was,
/// and once far out of view grows back in the grip.
/// </summary>
/// <remarks>
/// Its timelines are "idle" and "regrowing". The cues of "regrowing" are "regrow_start", which puts it a little way
/// behind the grip, "regrow_step", which brings it a pixel nearer, and "regrown", which leaves it ready again.
/// </remarks>
public partial class EvirSpit : Enemy
{
    /// <summary>How near across Samus must be to the Evir, in pixels, for it to be thrown.</summary>
    [Export, DataValue("throw_distance")]
    public int ThrowDistance { get; set; } = 128;

    /// <summary>How fast it flies, in pixels an update.</summary>
    [Export, DataValue("speed")]
    public int Speed { get; set; } = 4;

    /// <summary>How far out of view it must fly, in pixels, before it grows back.</summary>
    [Export, DataValue("regrow_margin")]
    public int RegrowMargin { get; set; } = 256;

    /// <summary>How far behind the grip it starts growing back, in pixels.</summary>
    [Export, DataValue("regrow_distance")]
    public int RegrowDistance { get; set; } = 8;

    private enum Doing
    {
        Held,
        Flying,
        GrowingBack,
    }

    private Doing _doing;
    private int _regrowAcross;

    /// <summary>The Evir that holds it.</summary>
    public Evir? Holder { get; set; }

    /// <summary>Whether it is on its way through the air.</summary>
    public bool IsFlying => _doing == Doing.Flying;

    /// <summary>Whether it is growing back in the Evir's grip.</summary>
    public bool IsGrowingBack => _doing == Doing.GrowingBack;

    /// <summary>Takes it out of the room along with the Evir that held it.</summary>
    public void GoWithHolder() => Vanish();

    /// <inheritdoc/>
    protected override void OnEnterRoom() => Play("idle");

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        Evir holder = TheHolder;
        switch (cue)
        {
            case "regrow_start":
                _regrowAcross = holder.Facing == Facing.Right ? -RegrowDistance : RegrowDistance;
                break;
            case "regrow_step":
                _regrowAcross += holder.Facing == Facing.Right ? 1 : -1;
                break;
            case "regrown":
                _doing = Doing.Held;
                break;
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Evir holder = TheHolder;
        switch (_doing)
        {
            case Doing.Held:
                // It is thrown from where it was put last update, before it is put in the grip again.
                if (Math.Abs(Target.X.Whole - holder.Position.X.Whole) >= ThrowDistance)
                {
                    PutAt(holder.Grip);
                    return;
                }

                // As the cartridge has it: the aim between whole pixels, counting up from right, and its speed laid
                // along it by the short product, across by the cosine and down by the sine a half turn on.
                int aim = CartridgeAngles.AimAngle(Target.X.Whole - Position.X.Whole, Target.Y.Whole - Position.Y.Whole);
                Velocity = new PixelVelocity(
                    CartridgeAngles.CircleCosine(aim, Speed), CartridgeAngles.CircleSine(aim + Turns.PerTurn / 2, Speed));
                Play("idle");
                _doing = Doing.Flying;
                Position += new PixelOffset(Velocity.X, Velocity.Y);
                return;

            case Doing.Flying:
                Play("idle");
                if (!IsInView(RegrowMargin) && !holder.IsFrozen)
                {
                    _doing = Doing.GrowingBack;
                    Play("regrowing", restart: true);
                }

                Position += new PixelOffset(Velocity.X, Velocity.Y);
                return;

            case Doing.GrowingBack:
                Play("regrowing");
                if (!holder.IsFrozen)
                    PutAt(holder.Grip + PixelOffset.FromWhole(_regrowAcross, 0));
                return;

            default:
                throw new InvalidOperationException($"Undefined Evir spine ball stage {_doing}.");
        }
    }

    /// <summary>Puts its whole pixels at <paramref name="place"/>'s: the part of a pixel its last flight left it at stays.</summary>
    private void PutAt(PixelPosition place) =>
        Position = new PixelPosition(Position.X.WithWhole(place.X.Whole), Position.Y.WithWhole(place.Y.Whole));

    /// <summary>The Evir that holds it.</summary>
    private Evir TheHolder => Holder ?? throw new InvalidOperationException($"The Evir spine ball \"{Name}\" has no Evir.");
}
