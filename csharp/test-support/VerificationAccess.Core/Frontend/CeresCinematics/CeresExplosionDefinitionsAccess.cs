using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Verification access to <see cref="CeresExplosionDefinitions"/> members production does not use.</summary>
internal static class CeresExplosionDefinitionsAccess
{
    /// <summary>Exposes the native explosion spawner definition used by verification.</summary>
    extension(CeresExplosionDefinitions)
    {
        /// <summary><c>$8B:CF33</c>, invisible actor whose list owns the three-wave schedule.</summary>
        /// <remarks>Its typed program (native list $8B:CE35) owns the three-wave schedule translated by StepCeresActors, as EventsAtFrame, with a mutable repeat countdown reset by fade completion. Initial/final instruction timing and repeat-before-final ordering are independently verified under #1165.</remarks>
        internal static CeresExplosionActorDefinition SpawnerActor =>
            new(0x93d9, CinematicSpriteProgram.CeresExplosionSpawner);
    }
}
