namespace Game.Data;

/// <summary>The names of the pieces of music, each the name of its file in the music folder.</summary>
public static class Music
{
    /// <summary>Samus's fanfare.</summary>
    public const string SamusFanfare = "samus_fanfare";

    /// <summary>The fanfare of collecting an item.</summary>
    public const string ItemFanfare = "item_fanfare";

    /// <summary>An elevator ride.</summary>
    public const string Elevator = "elevator";

    /// <summary>The hall before a Chozo statue.</summary>
    public const string StatueHall = "statue_hall";

    /// <summary>The opening of the title sequence.</summary>
    public const string TitleOpening = "title_opening";

    /// <summary>The title screen.</summary>
    public const string TitleScreen = "title_screen";

    /// <summary>Crateria on first arriving.</summary>
    public const string CrateriaArrival = "crateria_arrival";

    /// <summary>Crateria once Zebes has woken.</summary>
    public const string CrateriaArrivalZebesAwake = "crateria_arrival_zebes_awake";

    /// <summary>Blue Brinstar on first arriving.</summary>
    public const string BlueBrinstarArrival = "blue_brinstar_arrival";

    /// <summary>Lower Crateria.</summary>
    public const string LowerCrateria = "lower_crateria";

    /// <summary>The statues at the Tourian entrance.</summary>
    public const string TourianStatues = "tourian_statues";

    /// <summary>Samus's theme.</summary>
    public const string SamusTheme = "samus_theme";

    /// <summary>Green Brinstar.</summary>
    public const string GreenBrinstar = "green_brinstar";

    /// <summary>Red Brinstar.</summary>
    public const string RedBrinstar = "red_brinstar";

    /// <summary>Upper Norfair.</summary>
    public const string UpperNorfair = "upper_norfair";

    /// <summary>Lower Norfair.</summary>
    public const string LowerNorfair = "lower_norfair";

    /// <summary>East Maridia.</summary>
    public const string EastMaridia = "east_maridia";

    /// <summary>West Maridia.</summary>
    public const string WestMaridia = "west_maridia";

    /// <summary>The Wrecked Ship without power.</summary>
    public const string WreckedShipPoweredDown = "wrecked_ship_powered_down";

    /// <summary>The Wrecked Ship with power.</summary>
    public const string WreckedShipPoweredUp = "wrecked_ship_powered_up";

    /// <summary>Tourian.</summary>
    public const string Tourian = "tourian";

    /// <summary>The piece that is silence.</summary>
    public const string SilentSong = "silent_song";

    /// <summary>The fight with Mother Brain.</summary>
    public const string MotherBrainBattle = "mother_brain_battle";

    /// <summary>The fights with Ridley and with Draygon.</summary>
    public const string RidleyAndDraygonBattle = "ridley_and_draygon_battle";

    /// <summary>A Torizo waking.</summary>
    public const string TorizoAwakening = "torizo_awakening";

    /// <summary>The escape.</summary>
    public const string Escape = "escape";

    /// <summary>The fights with Kraid and with Phantoon.</summary>
    public const string KraidAndPhantoonBattle = "kraid_and_phantoon_battle";

    /// <summary>A boss room before its fight.</summary>
    public const string BossRoomTension = "boss_room_tension";

    /// <summary>A fight with a mini-boss.</summary>
    public const string MiniBossBattle = "mini_boss_battle";

    /// <summary>The baby Metroid.</summary>
    public const string BabyMetroidTheme = "baby_metroid_theme";

    /// <summary>The flight to Ceres.</summary>
    public const string CeresFlight = "ceres_flight";

    /// <summary>Ceres Station.</summary>
    public const string CeresStation = "ceres_station";

    /// <summary>Ceres exploding.</summary>
    public const string CeresExplosion = "ceres_explosion";

    /// <summary>Ceres exploding with Samus still aboard.</summary>
    public const string CeresExplosionWithSamusAboard = "ceres_explosion_with_samus_aboard";

    /// <summary>Zebes exploding.</summary>
    public const string ZebesExplosion = "zebes_explosion";

    /// <summary>The introduction.</summary>
    public const string Introduction = "introduction";

    /// <summary>Samus's death.</summary>
    public const string SamusDeath = "samus_death";

    /// <summary>The credits.</summary>
    public const string Credits = "credits";

    /// <summary>The last Metroid is in captivity.</summary>
    public const string LastMetroidInCaptivity = "last_metroid_in_captivity";

    /// <summary>The galaxy is at peace.</summary>
    public const string GalaxyAtPeace = "galaxy_at_peace";

    /// <summary>Every one of them.</summary>
    public static IReadOnlyList<string> All { get; } = [SamusFanfare, ItemFanfare, Elevator, StatueHall, TitleOpening, TitleScreen, CrateriaArrival, CrateriaArrivalZebesAwake, BlueBrinstarArrival, LowerCrateria, TourianStatues, SamusTheme, GreenBrinstar, RedBrinstar, UpperNorfair, LowerNorfair, EastMaridia, WestMaridia, WreckedShipPoweredDown, WreckedShipPoweredUp, Tourian, SilentSong, MotherBrainBattle, RidleyAndDraygonBattle, TorizoAwakening, Escape, KraidAndPhantoonBattle, BossRoomTension, MiniBossBattle, BabyMetroidTheme, CeresFlight, CeresStation, CeresExplosion, CeresExplosionWithSamusAboard, ZebesExplosion, Introduction, SamusDeath, Credits, LastMetroidInCaptivity, GalaxyAtPeace];
}
