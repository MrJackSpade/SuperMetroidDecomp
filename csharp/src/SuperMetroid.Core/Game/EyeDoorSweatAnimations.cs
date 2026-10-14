namespace SuperMetroid.Core.Game;

/// <summary>The eye-door sweat drop's typed programs (#1296), in place of its bank-$86 instruction lists.</summary>
internal static class EyeDoorSweatAnimations
{
    /// <summary>Updates each sweat pose lasts.</summary>
    private const int PoseDuration = 6;

    /// <summary>The timeline of <paramref name="program"/>.</summary>
    internal static AnimationTimeline<EyeDoorSweatFrame, EyeDoorSweatCue> Timeline(EyeDoorSweatProgram program) => program switch
    {
        EyeDoorSweatProgram.Falling => Falling,
        EyeDoorSweatProgram.HitFloor => HitFloor,
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "Undefined eye-door sweat program."),
    };

    /// <summary>The falling drop repeats its one pose every six updates until its movement finds the floor.</summary>
    private static AnimationTimeline<EyeDoorSweatFrame, EyeDoorSweatCue> Falling { get; } = new(
        AnimationStep<EyeDoorSweatFrame, EyeDoorSweatCue>.Show(EyeDoorSweatFrame.Drop, PoseDuration),
        AnimationStep<EyeDoorSweatFrame, EyeDoorSweatCue>.Loop);

    /// <summary>The splash stops the drop, shows its three poses for six updates each, then deletes it.</summary>
    private static AnimationTimeline<EyeDoorSweatFrame, EyeDoorSweatCue> HitFloor { get; } = new(
        AnimationStep<EyeDoorSweatFrame, EyeDoorSweatCue>.Publish(EyeDoorSweatCue.ClearPreInstruction),
        AnimationStep<EyeDoorSweatFrame, EyeDoorSweatCue>.Show(EyeDoorSweatFrame.SplashBegin, PoseDuration),
        AnimationStep<EyeDoorSweatFrame, EyeDoorSweatCue>.Show(EyeDoorSweatFrame.SplashSpread, PoseDuration),
        AnimationStep<EyeDoorSweatFrame, EyeDoorSweatCue>.Show(EyeDoorSweatFrame.SplashEnd, PoseDuration),
        AnimationStep<EyeDoorSweatFrame, EyeDoorSweatCue>.Publish(EyeDoorSweatCue.Delete),
        AnimationStep<EyeDoorSweatFrame, EyeDoorSweatCue>.Hold);

    /// <summary>Carries out the sweat cues on the projectile slot.</summary>
    internal static void RegisterCues(EnemyProjectileHandlerTable table) =>
        table.RegisterCue<EyeDoorSweatCue>((projectile, samus, cue) =>
        {
            switch (cue)
            {
                case EyeDoorSweatCue.ClearPreInstruction:
                    projectile.PreInstruction = (ushort)EnemyProjectilePreInstruction.RTS_868170;
                    return;
                case EyeDoorSweatCue.Delete:
                    projectile.Clear();
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(EyeDoorSweatCue)} {cue}.");
            }
        });
}