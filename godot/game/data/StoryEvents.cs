namespace Game.Data;

/// <summary>The names of the story events: things that happen once and are remembered as facts in progress.</summary>
public static class StoryEvents
{
    /// <summary>Zebes has woken: the planet is no longer quiet.</summary>
    public const string ZebesAwake = "zebes_awake";

    /// <summary>The baby Metroid has drained the Sidehopper in Tourian.</summary>
    public const string BabyMetroidAteSidehopper = "baby_metroid_ate_sidehopper";

    /// <summary>Mother Brain's glass has been broken.</summary>
    public const string MotherBrainGlassDestroyed = "mother_brain_glass_destroyed";

    /// <summary>Phantoon's statue at the Tourian entrance has turned grey.</summary>
    public const string PhantoonStatueGrey = "phantoon_statue_grey";

    /// <summary>Ridley's statue at the Tourian entrance has turned grey.</summary>
    public const string RidleyStatueGrey = "ridley_statue_grey";

    /// <summary>Draygon's statue at the Tourian entrance has turned grey.</summary>
    public const string DraygonStatueGrey = "draygon_statue_grey";

    /// <summary>Kraid's statue at the Tourian entrance has turned grey.</summary>
    public const string KraidStatueGrey = "kraid_statue_grey";

    /// <summary>The way into Tourian is open.</summary>
    public const string TourianUnlocked = "tourian_unlocked";

    /// <summary>The glass tube into Maridia has been broken.</summary>
    public const string MaridiaTubeBroken = "maridia_tube_broken";

    /// <summary>The Chozo statue in Lower Norfair has lowered the acid.</summary>
    public const string LowerNorfairAcidLowered = "lower_norfair_acid_lowered";

    /// <summary>Shaktool has dug its path through.</summary>
    public const string ShaktoolClearedPath = "shaktool_cleared_path";

    /// <summary>The countdown to the destruction of Zebes has begun.</summary>
    public const string EscapeStarted = "escape_started";

    /// <summary>The animals were let out during the escape.</summary>
    public const string AnimalsEscaped = "animals_escaped";

    /// <summary>The first Metroid hall has been cleared.</summary>
    public const string FirstMetroidHallCleared = "first_metroid_hall_cleared";

    /// <summary>The first Metroid shaft has been cleared.</summary>
    public const string FirstMetroidShaftCleared = "first_metroid_shaft_cleared";

    /// <summary>The second Metroid hall has been cleared.</summary>
    public const string SecondMetroidHallCleared = "second_metroid_hall_cleared";

    /// <summary>The second Metroid shaft has been cleared.</summary>
    public const string SecondMetroidShaftCleared = "second_metroid_shaft_cleared";

    /// <summary>The music of the baby Metroid encounter has been heard once and is skipped after.</summary>
    public const string BabyMetroidMusicSkipped = "baby_metroid_music_skipped";

    /// <summary>Samus outran the rising lava after the speed booster.</summary>
    public const string OutranLavaQuake = "outran_lava_quake";

    /// <summary>Samus has been set down in Ceres.</summary>
    public const string CeresArrived = "ceres_arrived";

    /// <summary>Ridley has left Ceres with the capsule.</summary>
    public const string CeresRidleyEscaped = "ceres_ridley_escaped";

    /// <summary>Ceres's self-destruct has begun.</summary>
    public const string CeresSelfDestructStarted = "ceres_self_destruct_started";

    /// <summary>The way to Ceres's elevator has been blown open.</summary>
    public const string CeresElevatorDoorDestroyed = "ceres_elevator_door_destroyed";

    /// <summary>Every one of them.</summary>
    public static IReadOnlyList<string> All { get; } = [ZebesAwake, BabyMetroidAteSidehopper, MotherBrainGlassDestroyed, PhantoonStatueGrey, RidleyStatueGrey, DraygonStatueGrey, KraidStatueGrey, TourianUnlocked, MaridiaTubeBroken, LowerNorfairAcidLowered, ShaktoolClearedPath, EscapeStarted, AnimalsEscaped, FirstMetroidHallCleared, FirstMetroidShaftCleared, SecondMetroidHallCleared, SecondMetroidShaftCleared, BabyMetroidMusicSkipped, OutranLavaQuake, CeresArrived, CeresRidleyEscaped, CeresSelfDestructStarted, CeresElevatorDoorDestroyed];
}
