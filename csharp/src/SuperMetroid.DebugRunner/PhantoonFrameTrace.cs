using SuperMetroid.Core.Runtime;

/// <summary>The 158 numeric fields shared with the original-CPU Doppler captures.</summary>
internal static class PhantoonFrameTrace
{
    public static int[] Capture(SuperMetroidRuntime runtime, int start, int cadence, int movement, int frame, ushort input)
    {
        var samus = runtime.Samus!;
        var phantoon = runtime.Enemies.Phantoon!;
        var body = phantoon.Body;
        int[] prefix = [start, cadence, movement, frame, input, runtime.TimeIsFrozen ? 1 : 0,
            body.Health, body.InvincibilityTimer, body.FlashTimer, body.SpritemapPointer,
            body.XPosition, body.YPosition, samus.XPosition, samus.YPosition,
            samus.Pose, samus.SelectedHudItem, samus.Health, body.VariableF,
            samus.Kinematics.XSubposition, samus.Kinematics.YSubposition];
        return prefix.Concat(runtime.Projectiles.Slots.SelectMany(p => p.Type != 0
                ? new int[] { p.Type, p.XPosition, p.YPosition, p.XSubposition, p.YSubposition, p.Direction, p.Damage }
                : new int[7]))
            .Concat(runtime.Enemies.EnemyProjectiles.SelectMany(p => p.IsActive
                ? new int[] { (ushort)p.Kind, p.XPosition, p.YPosition, p.XSubposition, p.YSubposition }
                : new int[5]))
            .Concat(new int[] { body.XSubposition, body.YSubposition,
                phantoon.Tentacles!.VariableC, phantoon.Tentacles.VariableD, phantoon.Tentacles.VariableE,
                body.VariableE, body.CurrentInstruction, body.InstructionTimer,
                phantoon.Eye!.CurrentInstruction, phantoon.Eye.InstructionTimer,
                phantoon.Tentacles.VariableB, samus.PreviousDrawNewInput, runtime.BombProjectiles.CooldownTimer })
            .ToArray();
    }
}
