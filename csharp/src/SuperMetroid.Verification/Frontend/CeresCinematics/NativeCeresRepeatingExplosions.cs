using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Verification.Frontend.Intro;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>
/// An independent reader of the cartridge's repeating-explosions routine ($8B:C489), which the explosion spawner's list
/// installs after its 80-update wait (#2489). Each update it counts down the countdown the fade-in's completion seeded
/// ($8B:C33C) and, on reaching zero or below, spawns a repeating blast and reloads the countdown ($8B:C4A9).
/// </summary>
/// <remarks>
/// The routine shares its countdown with the cinematic's own timer, which the fade-in's completion seeds; the retail scene
/// completes its fade-in before the spawner's repeating stretch starts and its gunship flies away only after the spawner
/// has ended (<see cref="CeresSpawnerScheduleVerification"/>), so neither a reseed during the stretch nor the routine's
/// stop at the gunship's departure is read here.
/// </remarks>
/// <param name="cartridge">The retail cartridge whose countdown seed and reload are read.</param>
internal sealed class NativeCeresRepeatingExplosions(IImportCartridgeSource cartridge) : INativeCinematicRoutine<CeresExplosionSpawnerCue>
{
    /// <summary>The countdown the fade-in's completion seeds.</summary>
    private ushort countdown = cartridge.ReadCartridgeWord(0x8bc33c);

    /// <summary>The countdown a spawned blast reloads.</summary>
    private readonly ushort reload = cartridge.ReadCartridgeWord(0x8bc4a9);

    /// <inheritdoc/>
    public IReadOnlyList<CeresExplosionSpawnerCue> Step()
    {
        countdown = WrappingCounter.Decrement(countdown).Value;
        if ((short)countdown > 0)
            return [];
        countdown = reload;
        return [CeresExplosionSpawnerCue.SpawnRepeatingBlast];
    }
}
