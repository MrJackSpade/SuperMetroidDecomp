using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.OldTourianWall;

/// <summary>The old Tourian escape-shaft wall species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
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
    /// breaking wall.
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
            graphics: default);
        projectile.StartAnimation(new OldTourianWallExplosionAnimation(OldTourianWallExplosionProgram.Start));
        projectile.Position = projectile.Position.WithWholeX(OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.XPosition);
        projectile.Position = projectile.Position.WithWholeY(OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.YPosition);
    }


    /// <summary>Registers the handler that carries out the wall explosion program's commands.</summary>
    internal void AddWallExplosionCues(EnemyProjectileHandlerTable table) =>
        table.AddCue<OldTourianWallExplosionCue>((context, cue) => ApplyWallExplosionCue(context.Projectile, cue));

    /// <summary>Carries out one command of the wall explosion's program.</summary>
    private void ApplyWallExplosionCue(RoomEnemyProjectileSlot projectile, OldTourianWallExplosionCue cue)
    {
        switch (cue)
        {
            case OldTourianWallExplosionCue.ClearPreInstruction:
                projectile.PreInstruction = EnemyProjectilePreInstruction.RTS_868170;
                break;
            case OldTourianWallExplosionCue.Scatter:
                _projectiles.Scatter(projectile, OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.Scatter);
                break;
            case OldTourianWallExplosionCue.ExplosionSound:
                _audio.QueueEnemySound(
                    OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.ExplosionSound,
                    OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.ExplosionSoundMaximum);
                break;
            case OldTourianWallExplosionCue.Delete:
                projectile.Clear();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(OldTourianWallExplosionCue)} {cue}.");
        }
    }
}
