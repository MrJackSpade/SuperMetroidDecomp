namespace Game.Data;

/// <summary>
/// The names of the sounds this game's code asks for, each the name of its file in the sounds folder: the old game's
/// name for the sound, after which of its three libraries it is in ("samus_", "enemy_" or "room_"). A sound a data
/// file names (a weapon's shot, an enemy's cry) is named there and not here.
/// </summary>
public static class Sounds
{
    /// <summary>Stops everything Samus's library is sounding. A stop is asked for like a sound and waits its turn like one.</summary>
    public const string SamusStop = "samus_stop";

    /// <summary>Stops a spin jump's sound.</summary>
    public const string SpinJumpStop = "samus_stop_spin_jump";

    /// <summary>A spin jump's sound taken up again as she leaves water.</summary>
    public const string SpinJumpResumed = "samus_resumed_spin_jump";

    /// <summary>Stops the speed booster's sound, or an elevator's.</summary>
    public const string SpeedBoosterStop = "room_stop_speed_booster_or_elevator";

    /// <summary>Stops everything the enemies' library is sounding.</summary>
    public const string EnemyStop = "enemy_stop";

    /// <summary>Stops everything the room's library is sounding.</summary>
    public const string RoomStop = "room_stop";

    /// <summary>A power bomb's blast.</summary>
    public const string PowerBombExplosion = "samus_power_bomb_explosion";

    /// <summary>A spin jump turning over.</summary>
    public const string SpinJump = "samus_spin_jump";

    /// <summary>A spin jump with the space jump.</summary>
    public const string SpaceJump = "samus_space_jump";

    /// <summary>A spin jump with the screw attack.</summary>
    public const string ScrewAttack = "samus_screw_attack";

    /// <summary>A spin jump under water.</summary>
    public const string UnderwaterSpinJump = "samus_underwater_space_jump";

    /// <summary>A jump off a wall.</summary>
    public const string WallJump = "samus_spin_jump";

    /// <summary>Landing on the ground.</summary>
    public const string Land = "room_landing";

    /// <summary>Landing hard from a long fall.</summary>
    public const string HardLand = "room_hard_landing";

    /// <summary>Samus being hurt.</summary>
    public const string SamusHurt = "samus_samus_damaged";

    /// <summary>The warning while energy is low.</summary>
    public const string LowEnergy = "room_low_health_beep";

    /// <summary>The speed booster reaching full speed.</summary>
    public const string SpeedBooster = "room_speed_booster";

    /// <summary>A shine being stored.</summary>
    public const string StoredShine = "room_stored_shinespark";

    /// <summary>A shinespark launching.</summary>
    public const string Shinespark = "room_shinespark";

    /// <summary>A shinespark ending against something.</summary>
    public const string ShinesparkEnd = "room_shinespark_end";

    /// <summary>The beam charging.</summary>
    public const string Charging = "samus_charging_beam";

    /// <summary>The beam's charge going on sounding after something silenced it.</summary>
    public const string ChargingResumed = "samus_resume_charging_beam";

    /// <summary>The selected item changing.</summary>
    public const string ItemSelect = "samus_hud_weapon_select";

    /// <summary>The grapple beam firing.</summary>
    public const string GrappleFire = "samus_grapple_start";

    /// <summary>The grapple beam latching on.</summary>
    public const string GrappleLatch = "samus_grappling";

    /// <summary>The grapple beam letting go.</summary>
    public const string GrappleRelease = "samus_grapple_end";

    /// <summary>The X-ray scope switching on.</summary>
    public const string Xray = "samus_xray_start";

    /// <summary>The X-ray scope switching off.</summary>
    public const string XrayEnd = "samus_stop_xray";

    /// <summary>A crystal flash.</summary>
    public const string CrystalFlash = "samus_power_bomb_explosion";

    /// <summary>Energy being refilled a point at a time.</summary>
    public const string EnergyRefill = "room_incremental_energy";

    /// <summary>Plunging into a liquid, or leaving one.</summary>
    public const string Splash = "enemy_splash_in";

    /// <summary>Something coming out of a liquid.</summary>
    public const string SplashOut = "enemy_splash_out";

    /// <summary>Being burnt by lava or acid.</summary>
    public const string LiquidDamage = "enemy_liquid_damage";

    /// <summary>A door cap opening.</summary>
    public const string DoorOpen = "room_door_opened";

    /// <summary>A door cap closing.</summary>
    public const string DoorClose = "room_door_closed";

    /// <summary>A shot stopped by something it cannot hurt.</summary>
    public const string ShotDeflected = "enemy_dud_shot";

    /// <summary>A small enemy being destroyed.</summary>
    public const string EnemyKilled = "enemy_enemy_killed";

    /// <summary>Samus's body striking an enemy.</summary>
    public const string EnemyContactKill = "enemy_contact_kill";

    /// <summary>An enemy being frozen.</summary>
    public const string EnemyFrozen = "room_enemy_frozen";

    /// <summary>A block crumbling.</summary>
    public const string BlockCrumble = "enemy_block_crumble";

    /// <summary>A block being broken.</summary>
    public const string BlockBreak = "enemy_contact_block_break";

    /// <summary>A save station working.</summary>
    public const string SaveStation = "samus_saving";

    /// <summary>A refill station working.</summary>
    public const string RefillStation = "enemy_station_engaged";

    /// <summary>A map station working.</summary>
    public const string MapStation = "enemy_station_engaged";

    /// <summary>An elevator moving.</summary>
    public const string Elevator = "room_elevator";

    /// <summary>The ground shaking.</summary>
    public const string Earthquake = "enemy_lavaquake";

    /// <summary>An explosion.</summary>
    public const string Explosion = "enemy_small_explosion";

    /// <summary>A large explosion.</summary>
    public const string BigExplosion = "enemy_big_explosion";

    /// <summary>A menu's cursor moving.</summary>
    public const string MenuMove = "samus_menu_cursor";

    /// <summary>A menu's choice being accepted.</summary>
    public const string MenuAccept = "samus_menu_confirm";

    /// <summary>A letter of narration appearing.</summary>
    public const string Typing = "room_typewriter";

    /// <summary>A foot of her run coming down.</summary>
    public const string Footsteps = "room_footsteps";

    /// <summary>A suit becoming hers.</summary>
    public const string SuitAcquired = "enemy_suit_acquired";

    /// <summary>Every one of them.</summary>
    public static IReadOnlyList<string> All { get; } =
    [
        SpinJump, SpaceJump, ScrewAttack, UnderwaterSpinJump, WallJump, Land, HardLand, SamusHurt, LowEnergy, SpeedBooster,
        StoredShine, Shinespark, ShinesparkEnd, Charging, ItemSelect, GrappleFire, GrappleLatch, GrappleRelease, Xray, XrayEnd,
        CrystalFlash, EnergyRefill, Splash, SplashOut, LiquidDamage, DoorOpen, DoorClose, ShotDeflected, EnemyKilled, EnemyFrozen,
        BlockCrumble, BlockBreak, SaveStation, RefillStation, MapStation, Elevator, Earthquake, Explosion,
        BigExplosion, MenuMove, MenuAccept, Typing, SamusStop, SpinJumpStop, EnemyStop, RoomStop, PowerBombExplosion,
        SpeedBoosterStop, ChargingResumed, EnemyContactKill, SpinJumpResumed, Footsteps, SuitAcquired,
    ];
}
