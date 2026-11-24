using Engine.Actors;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// A crystal flash: at the edge of death, with a power bomb laid and the right actions held, her ammunition is turned
/// into energy. She rises a little, and one piece of ammunition at a time, missiles, then super missiles, then power
/// bombs, becomes energy. Nothing can hurt her meanwhile.
/// </summary>
public sealed class CrystalFlashState : PlayerState, IUntouchable, ISamusLook
{
    /// <summary>How many updates she rises for as the flash begins.</summary>
    private const int RiseUpdates = 10;

    /// <summary>How far she rises in each of them, in pixels.</summary>
    private const int RiseStep = 2;

    private static readonly string[] Ammunition = [Amounts.Missiles, Amounts.SuperMissiles, Amounts.PowerBombs];

    private int _rising;
    private int _kind;
    private int _usedOfKind;
    private Countdown _wait;

    /// <summary>
    /// Whether a crystal flash can start: she is in the morph ball with a power bomb just laid, the angle actions, down
    /// and the attack are all held, her energy is low, her reserves are empty, and she has enough of every ammunition.
    /// </summary>
    public static bool CanStart(Samus samus)
    {
        GameInput input = samus.Input;
        Inventory inventory = samus.Inventory;
        return samus.Form == Player.Crawling &&
            input.IsHeld(Actions.AngleUp) && input.IsHeld(Actions.AngleDown) && input.IsHeld(GameAction.Down) && input.IsHeld(GameAction.Attack) &&
            samus.Weapons.LiveOf(Arsenal.PowerBomb).Count > 0 &&
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

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        var samus = (Samus)owner;
        owner.StopAcross();
        owner.Velocity = PixelVelocity.Zero;
        samus.SpendShine();
        _rising = RiseUpdates;
        _wait = Countdown.Of(samus.CrystalFlashDrainEvery);
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        var samus = (Samus)owner;
        if (_rising > 0)
        {
            _rising--;
            owner.Room.Mover.MoveDown(owner.Body, Pixels.FromWhole(-RiseStep), owner.BodyMoves);
            return null;
        }

        _wait = _wait.Stepped();
        if (_wait.IsRunning)
            return null;
        _wait = Countdown.Of(samus.CrystalFlashDrainEvery);

        if (_kind < Ammunition.Length)
        {
            owner.Inventory.Lose(Ammunition[_kind], 1);
            owner.Inventory.Add(Amounts.Energy, samus.CrystalFlashEnergyEach);
            _usedOfKind++;
            if (_usedOfKind >= samus.CrystalFlashAmmunition)
            {
                _kind++;
                _usedOfKind = 0;
            }

            return null;
        }

        return owner.StateAtRest();
    }
}
