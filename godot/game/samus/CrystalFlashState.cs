using Engine.Actors;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// A crystal flash: at the edge of death, lying where a power bomb of hers lay as its blast ends, with the right actions
/// held, her ammunition is turned into energy. She rises a little, and one piece of ammunition at a time, missiles,
/// then super missiles, then power bombs, becomes energy. Nothing can hurt her meanwhile.
/// </summary>
public sealed class CrystalFlashState : PlayerState, IUntouchable, ISamusLook
{
    /// <summary>How many updates she rises for as the flash begins.</summary>
    private const int RiseUpdates = 10;

    /// <summary>How far she rises in each of them, in pixels.</summary>
    private const int RiseStep = 2;

    private static readonly string[] Ammunition = [Amounts.Missiles, Amounts.SuperMissiles, Amounts.PowerBombs];

    /// <summary>The actions that start a crystal flash: these held, and nothing else.</summary>
    private static readonly GameInput Chord = new([Actions.AngleUp, Actions.AngleDown, GameAction.Down, GameAction.Attack], []);

    private int _rising;
    private int _kind;
    private int _usedOfKind;

    /// <summary>
    /// Whether a crystal flash can start, as a power bomb's blast ends where she lies: the angle actions, down and the
    /// attack are held and nothing else, she is moving neither up nor down, her energy is low, her reserves are empty,
    /// and she has enough of every ammunition. What she is doing does not matter.
    /// </summary>
    public static bool CanStart(Samus samus)
    {
        Inventory inventory = samus.Inventory;
        return samus.Input.HoldsSameAs(Chord) &&
            samus.Velocity.Y.IsZero &&
            inventory.Amount(Amounts.Energy) < samus.CrystalFlashEnergyBelow &&
            inventory.Amount(Amounts.ReserveEnergy) == 0 &&
            Ammunition.All(kind => inventory.Amount(kind) >= samus.CrystalFlashAmmunition);
    }

    /// <inheritdoc/>
    public string Look => "crystal_flash";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <summary>A knockback waits while she is in this state, as the cartridge refuses it here and tries again each update of its time.</summary>
    public override bool HoldsOffInterruptions => true;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        var samus = (Samus)owner;
        owner.StopAcross();
        owner.Velocity = PixelVelocity.Zero;
        samus.SpendShine();
        samus.BeginCrystalFlashColouring();
        _rising = RiseUpdates;
    }

    /// <summary>A flash taken from her before its rise is done lets the bomb kept for its blast go.</summary>
    public override void Exit(Player owner) => ((Samus)owner).GiveUpCrystalFlashBlast();

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        var samus = (Samus)owner;
        if (_rising > 0)
        {
            _rising--;
            owner.Room.Mover.MoveDown(owner.Body, Pixels.FromWhole(-RiseStep), owner.BodyMoves);

            // The rise done, everything the room's library sounds is stopped, at the priority nothing turns away.
            if (_rising == 0)
            {
                owner.Room.Session.Sounds.Request(Sounds.RoomStop, channel: "room", priority: SoundPriority.Essential);

                // The flash's own blast begins where she has risen to.
                samus.BeginCrystalFlashBlast();
            }

            return null;
        }

        // Her ammunition is taken on the clock's cadence: the flash keeps no count of its own.
        if (samus.CrystalFlashDrainEvery > 0 && owner.Room.Session.Clock.Updates % samus.CrystalFlashDrainEvery != 0)
            return null;

        if (_kind < Ammunition.Length)
        {
            owner.Inventory.Lose(Ammunition[_kind], 1);
            // What her energy has no room for goes to her reserves, as her ship's refill does.
            EnergyRestore.Give(owner.Inventory, samus.CrystalFlashEnergyEach);
            _usedOfKind++;
            if (_usedOfKind >= samus.CrystalFlashAmmunition)
            {
                _kind++;
                _usedOfKind = 0;
            }

            return null;
        }

        // Done, its colouring runs on a little; cut short, it would run on until another colouring took its place.
        samus.FinishCrystalFlashColouring();
        return owner.StateAtRest();
    }
}
