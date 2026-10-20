using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTurretPose, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTurretFrame, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTurretCue>;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The turrets' and bullets' typed programs (#641), in place of their bank-$86 instruction lists.</summary>
internal static class MotherBrainTurretAnimations
{
    /// <summary>Frames in the impact smoke.</summary>
    internal const int ImpactPhaseCount = 5;

    /// <summary>
    /// Every turret program: each turret and bullet pose shows for one update and sleeps; the impact smoke takes palette
    /// zero, stops flying, shows four eight-update puffs and a 32-update fade, then deletes itself.
    /// </summary>
    internal static AnimationProgramSet<MotherBrainTurretPose, MotherBrainTurretFrame, MotherBrainTurretCue> Programs { get; } = new(
        new[] { MotherBrainTurretProgram.TurretPose, MotherBrainTurretProgram.BulletPose }
            .SelectMany(program => Enum.GetValues<MotherBrainTurretDirection>().Select(direction => new MotherBrainTurretPose(program, direction)))
            .Select(pose => KeyValuePair.Create(pose, StillPose(pose)))
            .Append(KeyValuePair.Create(MotherBrainTurretPose.BulletImpact, BulletImpact()))
            .ToDictionary());

    /// <summary>Shows the pose for one update and sleeps.</summary>
    private static Step[] StillPose(MotherBrainTurretPose pose) =>
        [Step.Show(new(pose.Program, (int)pose.Direction), 1), Step.Hold];

    /// <summary>The impact smoke's steps.</summary>
    private static Step[] BulletImpact() =>
    [
        Step.Publish(MotherBrainTurretCue.UsePaletteZero),
        Step.Publish(MotherBrainTurretCue.ClearPreInstruction),
        .. Enumerable.Range(0, ImpactPhaseCount).Select(phase =>
            Step.Show(new(MotherBrainTurretProgram.BulletImpact, phase), phase == ImpactPhaseCount - 1 ? 32 : 8)),
        Step.Publish(MotherBrainTurretCue.Delete),
        Step.Hold,
    ];
}
