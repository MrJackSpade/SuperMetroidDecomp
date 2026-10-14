namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Cartridge values of <see cref="CinematicCodePointers.Lists"/> that only verification reads.</summary>
internal static class CinematicCodePointersListsConstants
{
    /// <summary>Bank-$8B instruction list at $CE4B for the large Ceres explosion asteroids.</summary>
    public const ushort CeresExplosionLargeAsteroids = 0xce4b;
    /// <summary>Bank-$8B instruction list at $CC57 for the purple vortex behind Ceres.</summary>
    public const ushort CeresPurpleSpaceVortex = 0xcc57;
    /// <summary>Bank-$8B instruction list at $CC4F for the small Ceres asteroids.</summary>
    public const ushort CeresSmallAsteroids = 0xcc4f;
    /// <summary>Bank-$8B instruction list at $CDA3 for the Ceres star field.</summary>
    public const ushort CeresStars = 0xcda3;
    /// <summary>Bank-$8B instruction list at $CE0D that spawns intro Rinkas.</summary>
    public const ushort IntroRinkaSpawner = 0xce0d;
    /// <summary>Byte stride between consecutive Metroid-egg particle records.</summary>
    public const ushort MetroidEggParticleStride = 8;
    /// <summary>Bank-$8B instruction list at $CD69 for slime drops from the Metroid egg.</summary>
    public const ushort MetroidEggSlimeDrops = 0xcd69;

    /// <summary>Provides native cinematic instruction-list addresses for Ceres space scenes and intro particles.</summary>
    extension(CinematicCodePointers.Lists)
    {
        /// <inheritdoc cref="CinematicCodePointersListsConstants.CeresExplosionLargeAsteroids"/>
        internal static ushort CeresExplosionLargeAsteroids => CinematicCodePointersListsConstants.CeresExplosionLargeAsteroids;
        /// <inheritdoc cref="CinematicCodePointersListsConstants.CeresPurpleSpaceVortex"/>
        internal static ushort CeresPurpleSpaceVortex => CinematicCodePointersListsConstants.CeresPurpleSpaceVortex;
        /// <inheritdoc cref="CinematicCodePointersListsConstants.CeresSmallAsteroids"/>
        internal static ushort CeresSmallAsteroids => CinematicCodePointersListsConstants.CeresSmallAsteroids;
        /// <inheritdoc cref="CinematicCodePointersListsConstants.CeresStars"/>
        internal static ushort CeresStars => CinematicCodePointersListsConstants.CeresStars;
        /// <inheritdoc cref="CinematicCodePointersListsConstants.IntroRinkaSpawner"/>
        internal static ushort IntroRinkaSpawner => CinematicCodePointersListsConstants.IntroRinkaSpawner;
        /// <inheritdoc cref="CinematicCodePointersListsConstants.MetroidEggParticleStride"/>
        internal static ushort MetroidEggParticleStride => CinematicCodePointersListsConstants.MetroidEggParticleStride;
        /// <inheritdoc cref="CinematicCodePointersListsConstants.MetroidEggSlimeDrops"/>
        internal static ushort MetroidEggSlimeDrops => CinematicCodePointersListsConstants.MetroidEggSlimeDrops;
    }
}
