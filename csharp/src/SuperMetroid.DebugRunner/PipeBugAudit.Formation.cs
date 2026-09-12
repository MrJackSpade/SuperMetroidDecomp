using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class PipeBugAudit
{
    private static void VerifyNorfairFormationTimerOwnership(
        LoadedPipeBugs loaded, RoomEnemySlot leader, RoomEnemySlot victim, bool respawns)
    {
        if (victim.EnemyDefinitionPointer != (respawns ? EnemyLifecycleDefinitions.RespawnPlaceholder : 0) ||
            victim.AiBank != (respawns ? 0xa3 : 0) || victim.Health != 0 || victim.Properties != 0 ||
            victim.XPosition != 0 || victim.YPosition != 0 ||
            victim.InstructionTimer != 0 || victim.Timer != 0)
            throw new InvalidDataException("Norfair respawning member did not retain the native cleared placeholder record.");

        var survivors = loaded.Enemies.Slots.Take(5).Where(s => s != victim).ToArray();
        foreach (var actor in survivors)
        {
            actor.InstructionTimer = (ushort)(21 + actor.SlotIndex);
            actor.Timer = (ushort)(41 + actor.SlotIndex);
        }
        ushort delayBefore = RequireState(loaded.Enemies, leader).DelayOrCounter;
        // Invoke the exact native routine boundary: the later enemy instruction
        // dispatcher would otherwise consume timers and hide which slot owns a reset.
        var trigger = typeof(RoomEnemySystem).GetMethod("RunNorfairPipeBugSamusWait",
            BindingFlags.Instance | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot, PipeBugEnemyState, SamusState>>(loaded.Enemies);
        trigger(leader, RequireState(loaded.Enemies, leader), loaded.Samus);
        ushort[] nativeTargets = [104, 96, 88, 112, 120];
        PipeBugEnemyFunction[] nativeFunctions =
        [
            PipeBugEnemyFunction.NorfairLeaderStagger,
            PipeBugEnemyFunction.NorfairUpperNearStagger,
            PipeBugEnemyFunction.NorfairUpperFarStagger,
            PipeBugEnemyFunction.NorfairLowerNearStagger,
            PipeBugEnemyFunction.NorfairLowerFarStagger,
        ];
        foreach (var actor in loaded.Enemies.Slots.Take(5))
        {
            ushort expectedInstructionTimer = actor == leader ? (ushort)1
                : actor == victim ? (ushort)0 : (ushort)(21 + actor.SlotIndex);
            ushort expectedLoopTimer = actor == leader || actor == victim ? (ushort)0
                : (ushort)(41 + actor.SlotIndex);
            if (actor.InstructionTimer != expectedInstructionTimer || actor.Timer != expectedLoopTimer)
                throw new InvalidDataException($"Norfair formation timer ownership differs at slot {actor.SlotIndex}: instruction={actor.InstructionTimer}/{expectedInstructionTimer}, loop={actor.Timer}/{expectedLoopTimer}.");
            var state = RequireState(loaded.Enemies, actor);
            if (state.Function != PipeBugEnemyFunction.NorfairRise ||
                state.StaggerTarget != nativeTargets[actor.SlotIndex] ||
                state.NorfairPostRiseFunction != nativeFunctions[actor.SlotIndex])
                throw new InvalidDataException($"Norfair formation raw writes differ at physical slot {actor.SlotIndex}.");
        }
        if (RequireState(loaded.Enemies, leader).DelayOrCounter != unchecked((ushort)(delayBefore + 1)))
            throw new InvalidDataException("Norfair formation did not increment only the leader's delay.");
    }

    private static void VerifyPipeBugDeath(LoadedPipeBugs loaded, RoomEnemySlot actor,
        ushort originalHeader, ushort deathX, ushort deathY, bool respawns, ushort expectedProperties)
    {
        if (actor.EnemyDefinitionPointer != (respawns ? EnemyLifecycleDefinitions.RespawnPlaceholder : 0) ||
            actor.Health != 0 || actor.Properties != expectedProperties ||
            actor.XPosition != 0 || actor.YPosition != 0 || loaded.Enemies.EnemiesKilled != 1)
            throw new InvalidDataException($"Pipe Bug death record mismatch: header={actor.EnemyDefinitionPointer:X4}, health={actor.Health}, properties={actor.Properties:X4}, position={actor.XPosition}/{actor.YPosition}, kills={loaded.Enemies.EnemiesKilled}.");
        var effects = loaded.Enemies.EnemyProjectiles
            .Where(p => p.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion).ToArray();
        ushort expectedIndex = (ushort)(actor.NativeIndex | (respawns ? 0x8000 : 0));
        if (effects.Length != 1 || effects[0].EnemyHeaderPointer != originalHeader ||
            effects[0].KilledEnemyNativeIndex != expectedIndex || effects[0].XPosition != deathX ||
            effects[0].YPosition != deathY || effects[0].GraphicsIndex != 0 || effects[0].InstructionTimer != 1)
            throw new InvalidDataException("Pipe Bug did not publish one correctly identified and positioned respawn/death effect.");
    }
}
