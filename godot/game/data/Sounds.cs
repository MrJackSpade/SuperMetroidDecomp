namespace Game.Data;

/// <summary>
/// The names of the sounds this game's code asks for, each the name of its file in the sounds folder. A sound a data
/// file names (a weapon's shot, an enemy's cry) is named there and not here.
/// </summary>
public static class Sounds
{
    /// <summary>A jump leaving the ground.</summary>
    public const string Jump = "jump";

    /// <summary>A spin jump turning over.</summary>
    public const string SpinJump = "spin_jump";

    /// <summary>A spin jump with the space jump.</summary>
    public const string SpaceJump = "space_jump";

    /// <summary>A spin jump with the screw attack.</summary>
    public const string ScrewAttack = "screw_attack";

    /// <summary>A spin jump under water.</summary>
    public const string UnderwaterSpinJump = "underwater_spin_jump";

    /// <summary>A jump off a wall.</summary>
    public const string WallJump = "wall_jump";

    /// <summary>Landing on the ground.</summary>
    public const string Land = "land";

    /// <summary>Landing hard from a long fall.</summary>
    public const string HardLand = "hard_land";

    /// <summary>Samus being hurt.</summary>
    public const string SamusHurt = "samus_hurt";

    /// <summary>The warning while energy is low.</summary>
    public const string LowEnergy = "low_energy";

    /// <summary>The speed booster reaching full speed.</summary>
    public const string SpeedBooster = "speed_booster";

    /// <summary>A shine being stored.</summary>
    public const string StoredShine = "stored_shine";

    /// <summary>A shinespark launching.</summary>
    public const string Shinespark = "shinespark";

    /// <summary>A shinespark ending against something.</summary>
    public const string ShinesparkEnd = "shinespark_end";

    /// <summary>Changing into the morph ball.</summary>
    public const string Morph = "morph";

    /// <summary>The beam charging.</summary>
    public const string Charging = "charging";

    /// <summary>The selected item changing.</summary>
    public const string ItemSelect = "item_select";

    /// <summary>The grapple beam firing.</summary>
    public const string GrappleFire = "grapple_fire";

    /// <summary>The grapple beam latching on.</summary>
    public const string GrappleLatch = "grapple_latch";

    /// <summary>The grapple beam letting go.</summary>
    public const string GrappleRelease = "grapple_release";

    /// <summary>The X-ray scope switching on.</summary>
    public const string Xray = "xray";

    /// <summary>The X-ray scope switching off.</summary>
    public const string XrayEnd = "xray_end";

    /// <summary>A crystal flash.</summary>
    public const string CrystalFlash = "crystal_flash";

    /// <summary>Energy being refilled a point at a time.</summary>
    public const string EnergyRefill = "energy_refill";

    /// <summary>Plunging into a liquid, or leaving one.</summary>
    public const string Splash = "splash";

    /// <summary>Being burnt by lava or acid.</summary>
    public const string LiquidDamage = "liquid_damage";

    /// <summary>A door cap opening.</summary>
    public const string DoorOpen = "door_open";

    /// <summary>A door cap closing.</summary>
    public const string DoorClose = "door_close";

    /// <summary>A shot stopped by something it cannot hurt.</summary>
    public const string ShotDeflected = "shot_deflected";

    /// <summary>A small enemy being destroyed.</summary>
    public const string EnemyKilled = "enemy_killed";

    /// <summary>A large enemy being destroyed.</summary>
    public const string BigEnemyKilled = "big_enemy_killed";

    /// <summary>An enemy being frozen.</summary>
    public const string EnemyFrozen = "enemy_frozen";

    /// <summary>A pickup being collected.</summary>
    public const string PickupCollected = "pickup_collected";

    /// <summary>A block crumbling.</summary>
    public const string BlockCrumble = "block_crumble";

    /// <summary>A block being broken.</summary>
    public const string BlockBreak = "block_break";

    /// <summary>A save station working.</summary>
    public const string SaveStation = "save_station";

    /// <summary>A refill station working.</summary>
    public const string RefillStation = "refill_station";

    /// <summary>A map station working.</summary>
    public const string MapStation = "map_station";

    /// <summary>An elevator moving.</summary>
    public const string Elevator = "elevator";

    /// <summary>The ground shaking.</summary>
    public const string Earthquake = "earthquake";

    /// <summary>An explosion.</summary>
    public const string Explosion = "explosion";

    /// <summary>A large explosion.</summary>
    public const string BigExplosion = "big_explosion";

    /// <summary>A menu's cursor moving.</summary>
    public const string MenuMove = "menu_move";

    /// <summary>A menu's choice being accepted.</summary>
    public const string MenuAccept = "menu_accept";

    /// <summary>A menu being backed out of.</summary>
    public const string MenuCancel = "menu_cancel";

    /// <summary>A letter of narration appearing.</summary>
    public const string Typing = "typing";

    /// <summary>The escape's siren.</summary>
    public const string Siren = "siren";

    /// <summary>Every one of them.</summary>
    public static IReadOnlyList<string> All { get; } =
    [
        Jump, SpinJump, SpaceJump, ScrewAttack, UnderwaterSpinJump, WallJump, Land, HardLand, SamusHurt, LowEnergy, SpeedBooster,
        StoredShine, Shinespark, ShinesparkEnd, Morph, Charging, ItemSelect, GrappleFire, GrappleLatch, GrappleRelease, Xray, XrayEnd,
        CrystalFlash, EnergyRefill, Splash, LiquidDamage, DoorOpen, DoorClose, ShotDeflected, EnemyKilled, BigEnemyKilled, EnemyFrozen,
        PickupCollected, BlockCrumble, BlockBreak, SaveStation, RefillStation, MapStation, Elevator, Earthquake, Explosion,
        BigExplosion, MenuMove, MenuAccept, MenuCancel, Typing, Siren,
    ];
}
