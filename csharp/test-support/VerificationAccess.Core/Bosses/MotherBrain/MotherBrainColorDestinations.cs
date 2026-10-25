namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Where the cartridge writes Mother Brain's colour images in its colour memory: verification's native view of the
/// colour sets the palette handlers fill.
/// </summary>
internal static class MotherBrainColorDestinations
{
    /// <summary>Byte destination $82: the first body background colour.</summary>
    public const int BodyColor = 65;
    /// <summary>Byte destination $122: the first brain and neck sprite colour; the fake-death fade writes here too.</summary>
    public const int BrainColor = 145;
    /// <summary>Byte destination $162: the first rear-leg sprite colour of the rainbow and health images.</summary>
    public const int RearLegColor = 177;
    /// <summary>Byte destination $168: the first rear-leg colour the drained and revival fades write, three inks into the rear-leg set.</summary>
    public const int DrainedRearLegColor = 180;
}
