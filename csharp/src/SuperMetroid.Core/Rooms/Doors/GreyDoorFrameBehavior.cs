namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The pre-instructions a grey cap installs (#1296); the unlock checks are selected by the door's condition. The
/// values are arbitrary (#1414).
/// </summary>
internal enum GreyDoorFrameBehavior
{
    /// <summary>No pre-instruction.</summary>
    None = 1,
    /// <summary>PLM frame behaviour: goto link instruction if shot.</summary>
    WakeWhenShot = 2,
    /// <summary>PLM frame behaviour: goto link instruction if area boss dead.</summary>
    WakeIfAreaBossDead = 3,
    /// <summary>PLM frame behaviour: goto link instruction if area Mini boss is dead.</summary>
    WakeIfAreaMiniBossDead = 4,
    /// <summary>PLM frame behaviour: goto link instruction if area Torizo is dead.</summary>
    WakeIfAreaTorizoDead = 5,
    /// <summary>PLM frame behaviour: goto link inst, set Zebes awake event if enemies dead.</summary>
    WakeIfEnemiesDead = 6,
    /// <summary>The door never unlocks.</summary>
    PlayDudSound = 7,
    /// <summary>PLM frame behaviour: goto link inst if Tourian statue finished processing.</summary>
    WakeIfTourianStatueFinished = 8,
    /// <summary>PLM frame behaviour: goto link instruction if critters escaped.</summary>
    WakeIfCrittersEscaped = 9,
}
