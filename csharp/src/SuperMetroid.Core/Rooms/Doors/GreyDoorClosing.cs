namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>How a grey cap closes once Samus has passed (#2383).</summary>
internal enum GreyDoorClosing
{
    /// <summary>An ordinary grey cap, which closes for good at once.</summary>
    AtOnce,

    /// <summary>Bomb Torizo's cap, held open until Samus has the Bombs.</summary>
    AfterBombs,
}
