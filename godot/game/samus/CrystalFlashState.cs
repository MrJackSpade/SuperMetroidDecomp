using Engine.Actors;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// A crystal flash: at the edge of death, lying where a power bomb of hers lay as its blast ends, with the right actions
/// held, her ammunition is turned into energy. She is upright from its start, where she is and whatever is around her;
/// she rises a little, through anything; one piece of ammunition at a time, missiles, then super missiles, then power
/// bombs, becomes energy; and it finishes with her standing where it left her, in the air or not. It is no shield: what a hurt left her
/// is ended as it begins, as its rise ends, on every update of the taking and as it lets her go, so whatever touches
/// her while her ammunition is taken hurts her on every update, and nothing knocks her back.
/// </summary>
public sealed class CrystalFlashState : PlayerState, ISamusLook
{
    /// <summary>How many updates she rises for as the flash begins.</summary>
    private const int RiseUpdates = 10;

    /// <summary>How far she rises in each of them, in pixels.</summary>
    private const int RiseStep = 2;

    /// <summary>
    /// How many updates the finish goes on for after the one that takes the last of her ammunition: its three pictures
    /// end on the eighth, which shows her standing, and the ninth lets her go.
    /// </summary>
    private const int FinishUpdates = 9;

    private static readonly string[] Ammunition = [Amounts.Missiles, Amounts.SuperMissiles, Amounts.PowerBombs];

    /// <summary>The actions that start a crystal flash: these held, and nothing else.</summary>
    private static readonly GameInput Chord = new([Actions.AngleUp, Actions.AngleDown, GameAction.Down, GameAction.Attack], []);

    private int _rising;
    private int _kind;
    private int _usedOfKind;

    /// <summary>The height she had risen to as the rise ended, in whole pixels.</summary>
    private int _risenTo;

    /// <summary>How many updates of the finish are left; null until the last of her ammunition is taken.</summary>
    private int? _finishing;

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
    public string Look => _finishing is null ? "crystal_flash" : "crystal_flash_finish";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <summary>
    /// She is upright from the flash's start, her middle where it was, with the upright body at once: the cartridge
    /// feels for no room, and sets the flash pose's size as it takes it up.
    /// </summary>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(Player.Standing, Unfitted: true, AtOnce: true);

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        var samus = (Samus)owner;
        owner.StopAcross();
        owner.Velocity = PixelVelocity.Zero;
        samus.SpendShine();
        samus.BeginCrystalFlashColouring();
        owner.BeInvincibleFor(0);
        _rising = RiseUpdates;
    }

    /// <summary>A flash taken from her before its rise is done lets the bomb kept for its blast go.</summary>
    public override void Exit(Player owner) => ((Samus)owner).GiveUpCrystalFlashBlast();

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        var samus = (Samus)owner;
        if (_finishing is { } left)
        {
            // Moved from the height she rose to, she is set down a pixel on each update of the finish.
            if (owner.Position.Y.Whole != _risenTo)
                owner.Position += PixelOffset.FromWhole(0, 1);
            if (left > 1)
            {
                _finishing = left - 1;
                return null;
            }

            // Done, its colouring runs on a little; cut short, it would run on until another colouring took its place.
            // She is let go standing, as the cartridge's finish leaves her in its standing pose, in the air too: from
            // there she falls as a standing Samus with nothing under her does, from her next update.
            samus.FinishCrystalFlashColouring();
            owner.BeInvincibleFor(0);
            return new StandingState();
        }

        if (_rising > 0)
        {
            // The rise is a plain change of where she is, by whole pixels: nothing stops it.
            _rising--;
            owner.Position += PixelOffset.FromWhole(0, -RiseStep);

            // The rise done, everything the room's library sounds is stopped, at the priority nothing turns away.
            if (_rising == 0)
            {
                _risenTo = owner.Position.Y.Whole;
                owner.Room.Session.Sounds.Request(Sounds.RoomStop, channel: "room", priority: SoundPriority.Essential);

                // The flash's own blast begins where she has risen to.
                samus.BeginCrystalFlashBlast();
                owner.BeInvincibleFor(0);
            }

            return null;
        }

        owner.BeInvincibleFor(0);

        // Her ammunition is taken on the clock's cadence: the flash keeps no count of its own.
        if (samus.CrystalFlashDrainEvery > 0 && owner.Room.Session.Clock.Updates % samus.CrystalFlashDrainEvery != 0)
            return null;

        owner.Inventory.Lose(Ammunition[_kind], 1);
        // What her energy has no room for goes to her reserves, as her ship's refill does.
        EnergyRestore.Give(owner.Inventory, samus.CrystalFlashEnergyEach);
        _usedOfKind++;
        if (_usedOfKind < samus.CrystalFlashAmmunition)
            return null;
        _kind++;
        _usedOfKind = 0;

        // The update that takes the last of her ammunition turns to the finish.
        if (_kind == Ammunition.Length)
            _finishing = FinishUpdates;
        return null;
    }
}
