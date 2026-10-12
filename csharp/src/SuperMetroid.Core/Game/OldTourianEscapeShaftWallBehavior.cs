namespace SuperMetroid.Core.Game;

/// <summary>The old Tourian escape-shaft wall species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class OldTourianEscapeShaftWallBehavior
{
    private readonly EnemyRoomBinding _room;
    private readonly EnemyProjectileEngine _projectiles;

    private readonly EnemyAudioQueue _audio;

    internal OldTourianEscapeShaftWallBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>
    /// Spawns enemy projectile $86:B4B1 for PLM pre-instruction $84:B927. Its initializer
    /// ($86:B49D) ignores the spawn parameter and places the explosion on the middle of the
    /// breaking wall, saving that point in Var0/Var1.
    /// </summary>
    public void SpawnOldTourianEscapeShaftWallExplosion()
    {
        _room.EnsureLoaded();
        RoomEnemyProjectileSlot? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return; // Native SpawnEproj returns carry set when all eighteen slots are live.

        // SpawnEnemyProjectileY_ParameterA_RoomGraphics uses graphics index zero.
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion,
            graphicsIndex: 0);
        projectile.Position = projectile.Position.WithWholeX(OldTourianEscapeShaftWallExplosionDefinitions.XPosition);
        projectile.Variable0 = OldTourianEscapeShaftWallExplosionDefinitions.XPosition;
        projectile.Position = projectile.Position.WithWholeY(OldTourianEscapeShaftWallExplosionDefinitions.YPosition);
        projectile.Variable1 = OldTourianEscapeShaftWallExplosionDefinitions.YPosition;
    }


    internal void RegisterWallExplosionSoundInstruction(EnemyProjectileHandlerTable table)
    {
        table.RegisterInstruction(EnemyProjectileInstruction.QueueSoundInY_Lib2_Max6, projectile => OldTourianEscapeShaftWallExplosionDefinitions.Owns(projectile.Kind),
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                _audio.QueueEnemySound(
                    OldTourianEscapeShaftWallExplosionDefinitions.ExplosionSound,
                    OldTourianEscapeShaftWallExplosionDefinitions.ExplosionSoundMaximum);
                cursor = unchecked((ushort)(cursor + 3));
                return EnemyProjectileInstructionOutcome.Continue;
            });
    }
}
