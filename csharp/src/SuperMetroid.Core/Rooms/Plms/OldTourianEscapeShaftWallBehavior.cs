using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The old Tourian escape-shaft wall species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class OldTourianEscapeShaftWallBehavior
{
    /// <summary>Loaded-room dependency required before the wall controller can spawn its explosion.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Native-equivalent eighteen-slot enemy projectile allocator for the wall explosion.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Enemy sound queue used by the wall explosion's bank-$86 instruction.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds room, projectile, and audio owners needed by the old Tourian fake-wall controller.</summary>
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
        projectile.Position = projectile.Position.WithWholeX(OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.XPosition);
        projectile.Variable0 = OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.XPosition;
        projectile.Position = projectile.Position.WithWholeY(OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.YPosition);
        projectile.Variable1 = OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.YPosition;
    }


    /// <summary>Registers the wall explosion's library-two sound opcode only for the owned projectile kind, then advances its cursor.</summary>
    internal void RegisterWallExplosionSoundInstruction(EnemyProjectileHandlerTable table)
    {
        table.RegisterInstruction(EnemyProjectileInstruction.QueueSoundInY_Lib2_Max6, projectile => OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.Owns(projectile.Kind),
            (RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor) =>
            {
                _audio.QueueEnemySound(
                    OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.ExplosionSound,
                    OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.ExplosionSoundMaximum);
                cursor = unchecked((ushort)(cursor + 3));
                return EnemyProjectileInstructionOutcome.Continue;
            });
    }
}
