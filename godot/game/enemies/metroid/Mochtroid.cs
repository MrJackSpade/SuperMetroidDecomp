using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Mochtroid: a failed copy of a Metroid. It homes in on Samus and, while it touches her, creeps along with her and
/// takes a bite of her energy every so often. It never clamps on, and shots hurt it as they would anything.
/// </summary>
/// <remarks>Its timelines are "free" and "attached".</remarks>
public partial class Mochtroid : Enemy
{
    /// <summary>The fastest it flies, in pixels an update.</summary>
    [Export, DataValue("fastest_flight")]
    public int FastestFlight { get; set; } = 3;

    /// <summary>How far it creeps toward Samus each update while it touches her, in pixels.</summary>
    [Export, DataValue("attached_speed")]
    public int AttachedSpeed { get; set; } = 1;

    /// <summary>How many updates of touching Samus pass between its bites.</summary>
    [Export, DataValue("bite_every")]
    public int BiteEvery { get; set; } = 80;

    /// <summary>How many updates pass between the sounds of it feeding.</summary>
    [Export, DataValue("feeding_sound_every")]
    public int FeedingSoundEvery { get; set; } = 8;

    /// <summary>The least energy Samus must have for its feeding to be heard.</summary>
    [Export, DataValue("feeding_sound_least_energy")]
    public int FeedingSoundLeastEnergy { get; set; } = 30;

    /// <summary>The sound of it feeding, by name.</summary>
    [Export, DataValue("feeding_sound")]
    public string FeedingSound { get; set; } = "";

    private bool _touching;
    private int _biteIn;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _biteIn = BiteEvery;
        Standing = Standing.InFront;
        Play("free");
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        _touching = true;
        Play("attached");
        if (_biteIn > 0)
            _biteIn--;
        Player samus = Room.Player;
        if (samus.BodyPowers.Any(power => power.IsActive))
        {
            base.Touch(part);
            samus.BeInvincibleFor(0);
            return;
        }

        if (Room.Session.EnemyPasses % FeedingSoundEvery == FeedingSoundEvery - 1
            && samus.Inventory.Amount(Inventory.Energy) >= FeedingSoundLeastEnergy)
            Sound(FeedingSound);
        if (_biteIn > 0)
            return;
        _biteIn = BiteEvery;
        samus.Sap(new Hit(ContactDamage, HitSide.Enemies, HitKind.Contact) { Source = this });
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        // Whether it touches Samus is found afresh each update, by its touch.
        bool touching = _touching;
        _touching = false;
        if (!touching)
        {
            Pixels fastest = Pixels.FromWhole(FastestFlight);
            Pixels down = Metroid.Limited(Velocity.Y - Metroid.Pull(Position.Y.Whole - Target.Y.Whole), fastest);
            Velocity = Velocity.WithY(MoveDownBy(down) ? Pixels.Zero : down);
            Pixels across = Metroid.Limited(Velocity.X - Metroid.Pull(Position.X.Whole - Target.X.Whole), fastest);
            Velocity = Velocity.WithX(MoveAcrossBy(across) ? Pixels.Zero : across);
            Play("free");
            return;
        }

        int toX = Target.X.Whole - Position.X.Whole, toY = Target.Y.Whole - Position.Y.Whole;
        Pixels creepAcross = Pixels.FromWhole(Math.Sign(toX) * AttachedSpeed), creepDown = Pixels.FromWhole(Math.Sign(toY) * AttachedSpeed);
        Velocity = new PixelVelocity(creepAcross, creepDown);
        MoveAcrossBy(creepAcross);
        MoveDownBy(creepDown);
    }
}
