using SuperMetroid.Core.Game;

/// <summary>Checks native common death's cleared record and separately owned explosion.</summary>
internal static class EnemyDeathAuditAssertions
{
    internal readonly record struct BeforeDeath(ushort Header, ushort X, ushort Y, ushort NativeIndex, bool Respawns, ushort Kills);

    public static BeforeDeath Capture(RoomEnemySystem enemies, RoomEnemySlot actor) =>
        new(actor.EnemyDefinitionPointer, actor.XPosition, actor.YPosition, actor.NativeIndex,
            actor.Properties.HasAny(EnemyProperties.RespawnIfKilled), enemies.EnemiesKilled);

    public static void Verify(RoomEnemySystem enemies, RoomEnemySlot actor, BeforeDeath before,
        string context, ushort expectedProperties = 0)
    {
        ushort expectedHeader = before.Respawns ? EnemyLifecycleDefinitions.RespawnPlaceholder : (ushort)0;
        if (actor.EnemyDefinitionPointer != expectedHeader || actor.Health != 0 ||
            actor.Properties != expectedProperties || actor.XPosition != 0 || actor.YPosition != 0 ||
            enemies.EnemiesKilled != unchecked((ushort)(before.Kills + 1)))
            throw new InvalidDataException($"{context}: native death clear differs: header={actor.EnemyDefinitionPointer:X4}/{expectedHeader:X4}, health={actor.Health}, properties={actor.Properties:X4}/{expectedProperties:X4}, position={actor.XPosition}/{actor.YPosition}, kills={enemies.EnemiesKilled}.");
        ushort index = (ushort)(before.NativeIndex | (before.Respawns ? 0x8000 : 0));
        var effects = enemies.EnemyProjectiles.Where(p => p.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion &&
            p.KilledEnemyNativeIndex == index && p.EnemyHeaderPointer == before.Header).ToArray();
        if (effects.Length != 1 || effects[0].XPosition != before.X || effects[0].YPosition != before.Y ||
            effects[0].GraphicsIndex != 0 || effects[0].InstructionTimer != 1)
            throw new InvalidDataException($"{context}: missing, duplicate or misplaced native death effect.");
    }
}
