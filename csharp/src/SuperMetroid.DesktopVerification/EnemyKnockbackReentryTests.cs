using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyEnemyKnockbackReentry()
    {
        var loaded = DebuggerFixtureLoader.Load("draygon-body-position", 0);
        var runtime = loaded.Game.RuntimeForVerification!;
        var samus = runtime.Samus!;
        samus.Health = 500;
        samus.KnockbackActive = true;
        samus.KnockbackDirection = 1;
        samus.InvincibilityTimer = 0;
        ushort speed = samus.Kinematics.YSpeed;
        // Isolate the exact shared damage producer in #383 from enemy hitbox selection.
        // The cartridge permits damage while a prior movement handler remains installed.
        var damage = typeof(RoomEnemySystem).GetMethod("ApplyNormalEnemyTouchDamage", BindingFlags.Instance | BindingFlags.NonPublic)!;
        damage.Invoke(runtime.Enemies, new object[] { samus, (ushort)0, (ushort)20, samus.XPosition });
        if (samus.Health >= 500 || samus.InvincibilityTimer != 96 || samus.KnockbackTimer != 5 ||
            !samus.KnockbackActive || samus.KnockbackDirection != 1 || samus.Kinematics.YSpeed != speed)
            throw new InvalidOperationException("Enemy touch must publish damage/timers without restarting installed knockback.");
        Console.WriteLine("Enemy touch reentry: damage and native request published; installed knockback retained.");
    }
}
