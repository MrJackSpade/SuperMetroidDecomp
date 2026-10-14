using SuperMetroid.Core.Frontend;

/// <summary>Verification access to <see cref="CeresExplosionDefinitions"/> members production does not use.</summary>
internal static class CeresExplosionDefinitionsAccess
{
    /// <summary>Exposes the native explosion spawner definition used by verification.</summary>
    extension(CeresExplosionDefinitions)
    {
        /// <summary><c>$8B:CF33</c>, invisible actor whose list owns the three-wave schedule.</summary>
        /// <remarks>Native list $8B:CE35..CE4A owns the three-wave schedule translated by StepCeresActors. The native list is translated to EventsAtFrame, with a mutable repeat countdown reset by fade completion. Initial/final instruction timing and repeat-before-final ordering are independently verified under #1165.</remarks>
        internal static CeresExplosionActorDefinition SpawnerActor =>
            new(0x93d9, 0xce35);
    }
}
