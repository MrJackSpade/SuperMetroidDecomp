namespace SuperMetroid.Core.Game;

/// <summary>The save station electricity's typed program (#1296), in place of its bank-$86 instruction list.</summary>
internal static class SaveStationElectricityAnimations
{
    /// <summary>Frames in one electricity cycle.</summary>
    internal const int PhaseCount = 8;
    /// <summary>Cycles the NTSC cartridge plays before deleting the electricity.</summary>
    private const int CycleCount = 20;

    /// <summary>Twenty cycles of the eight one-update frames, then the electricity is deleted.</summary>
    internal static AnimationTimeline<SaveStationElectricityFrame, SaveStationElectricityCue> Timeline { get; } = new(
    [
        .. Enumerable.Range(0, CycleCount).SelectMany(_ => Enumerable.Range(0, PhaseCount).Select(phase =>
            AnimationStep<SaveStationElectricityFrame, SaveStationElectricityCue>.Show(new(phase), 1))),
        AnimationStep<SaveStationElectricityFrame, SaveStationElectricityCue>.Publish(SaveStationElectricityCue.Delete),
        AnimationStep<SaveStationElectricityFrame, SaveStationElectricityCue>.Hold,
    ]);

    /// <summary>Carries out the electricity's cue on the projectile slot.</summary>
    internal static void RegisterCues(EnemyProjectileHandlerTable table) =>
        table.RegisterCue<SaveStationElectricityCue>((projectile, samus, cue) =>
        {
            switch (cue)
            {
                case SaveStationElectricityCue.Delete:
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(SaveStationElectricityCue)} {cue}.");
            }
        });
}