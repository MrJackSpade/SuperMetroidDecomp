namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What a grey cap's programs do besides drawing (#1296).</summary>
internal enum GreyDoorCue
{
    /// <summary>Becomes a blue cap when this door was opened before.</summary>
    BranchIfOpened,
    /// <summary>Links <see cref="GreyDoorProgram.Activate"/> for the unlock wake.</summary>
    LinkActivate,
    /// <summary>Installs the door's unlock-condition pre-instruction.</summary>
    ArmCondition,
    /// <summary>Links <see cref="GreyDoorProgram.Hit"/> for the shot wake.</summary>
    LinkHit,
    /// <summary>Installs the any-shot wake pre-instruction.</summary>
    ArmShotWake,
    /// <summary>Counts a hit; the first opens the door.</summary>
    CountHit,
    /// <summary>Queues the door-opening sound.</summary>
    QueueOpenSound,
    /// <summary>Queues the door-closing sound.</summary>
    QueueCloseSound,
    /// <summary>Bomb Torizo's door: repeats its closing until Samus has Bombs.</summary>
    RepeatUntilSamusHasBombs,
}
