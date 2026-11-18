using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// Translates the typed playbacks of Ridley's room to and from the native instruction pointers a recording holds (#1287,
/// #1600), so replay comparisons keep checking the body's, fragments' and fireballs' animation state. Each beat sits at the
/// native instruction its ROM oracle matched it with; after a frame, both the playback and the native pointer rest on
/// the following instruction, and a play done after a list's last beat rests on the control instruction after it. A
/// check of the body's pointer alone builds <see cref="RidleyBodyNativeInstructionPointers"/> instead (#2419).
/// </summary>
internal sealed class RidleyNativeInstructionPointers
{
    /// <summary>Ridley's body's pointer translation.</summary>
    private readonly RidleyBodyNativeInstructionPointers body;
    /// <summary>Each fireball segment's beat and control addresses, each afterburn run's puff at its own list's tail.</summary>
    private readonly RidleyFireballNativeLists fireball;
    /// <summary>Each typed enemy-projectile family's pointer translation.</summary>
    private readonly Func<EnemyProjectileAnimation, ushort?>[] projectileFamilies;
    /// <summary>The source room's Pipe Bugs' pointer translation.</summary>
    private readonly PipeBugNativeInstructionPointers pipeBugs;

    /// <summary>Walks every typed Ridley segment and animation beside its native lists, failing on any mismatch.</summary>
    internal RidleyNativeInstructionPointers(IImportCartridgeSource cartridge)
    {
        body = new RidleyBodyNativeInstructionPointers(cartridge);
        RidleyNativeProgramOracle.CompareBreakupLists(cartridge);
        fireball = RidleyNativeProgramOracle.FireballLists(cartridge);
        projectileFamilies = [.. NativeEnemyProjectileFamilies.All.Select(family => family.NativePointers(cartridge))];
        pipeBugs = new PipeBugNativeInstructionPointers(cartridge);
    }

    /// <summary>The native instruction pointer of <paramref name="actor"/>, which must keep a translated cursor.</summary>
    /// <exception cref="InvalidOperationException">The actor keeps no cursor this translates.</exception>
    internal ushort Enemy(RoomEnemySystem enemies, RoomEnemyPlace actor) =>
        TranslatedEnemy(enemies, actor) ?? throw new InvalidOperationException(
            $"Enemy {actor.DefinitionId} in slot {actor.Ordinal} keeps no cursor a native instruction pointer translates.");

    /// <summary>
    /// The native instruction pointer of <paramref name="actor"/>: Ridley's body and breakup fragments and the Pipe Bugs
    /// translate their typed cursors; other actors keep their animation in their species state (#1471) and give null.
    /// </summary>
    internal ushort? TranslatedEnemy(RoomEnemySystem enemies, RoomEnemyPlace actor)
    {
        if (RidleyInteractionsConstants.IsRidleyDefinition(actor.DefinitionId))
            return body.Body(RequireRidley(enemies).Flight);
        if (actor.DefinitionId == EnemyDefinitionId.RidleyBreakupPiece)
        {
            // A fragment's frame sits at its list entry and its hold at the sleep after the frame record.
            return RidleyNativeProgramOracle.BreakupPointer(actor);
        }
        if (actor.PipeBugPlayback is { } pipeBug)
            return pipeBugs.Pointer(pipeBug);
        return null;
    }

    /// <summary>
    /// The native instruction pointer of <paramref name="projectile"/>: Ridley's fireballs and afterburns and every
    /// typed enemy-projectile family translate their typed cursors.
    /// </summary>
    internal ushort Projectile(EnemyProjectile projectile) => projectile.Animation switch
    {
        null => throw new InvalidOperationException($"Projectile {projectile.Kind} plays no program."),
        RidleyFireballAnimation animation => NativeEnemyAnimationOracle.PointerOf(fireball.For(animation.Playback.Animation), RidleyNativeProgramOracle.FireballListEntries,
            RidleyFireballAnimations.StartingAt, animation.Playback),
        { } animation => projectileFamilies.Select(family => family(animation)).SingleOrDefault(pointer => pointer is not null)
            ?? throw new InvalidOperationException(
                $"Projectile {projectile.Kind} runs a typed {animation.GetType().Name}, which has no native-pointer translation."),
    };

    /// <summary>Ridley's encounter state.</summary>
    private static RidleyEncounterState RequireRidley(RoomEnemySystem enemies) =>
        enemies.Ridley() ?? throw new InvalidOperationException("Ridley's actors require Ridley's encounter state.");
}
