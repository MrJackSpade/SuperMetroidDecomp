using SuperMetroid.Verification.Enemies.PipeBugs;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>
/// Translates Ridley's typed program cursors to and from the native instruction pointers a recording holds (#1287),
/// so replay comparisons keep checking the body's, fragments' and fireballs' animation state. Each typed step sits at
/// the native instruction its ROM oracle matched it with; after a frame, both the cursor and the native pointer rest on
/// the following instruction.
/// </summary>
internal sealed class RidleyNativeInstructionPointers
{
    /// <summary>Each body program's step addresses.</summary>
    private readonly Dictionary<RidleyBodyProgram, ushort[]> body;
    /// <summary>Each fireball program's step addresses.</summary>
    private readonly Dictionary<RidleyFireballProgram, ushort[]> fireball;
    /// <summary>Each typed enemy-projectile family's pointer translation.</summary>
    private readonly Func<EnemyProjectileAnimation, ushort?>[] projectileFamilies;
    /// <summary>The source room's Pipe Bugs' pointer translation.</summary>
    private readonly PipeBugNativeInstructionPointers pipeBugs;

    /// <summary>Walks every typed Ridley program beside its native list, failing on any mismatch.</summary>
    internal RidleyNativeInstructionPointers(IImportCartridgeSource cartridge)
    {
        body = RidleyAnimationsVerification.BodyStepAddresses(cartridge);
        RidleyAnimationsVerification.BreakupStepAddresses(cartridge);
        fireball = RidleyAnimationsVerification.FireballStepAddresses(cartridge);
        projectileFamilies = [.. NativeEnemyProjectileFamilies.All.Select(family => family.NativePointers(cartridge))];
        pipeBugs = new PipeBugNativeInstructionPointers(cartridge);
    }

    /// <summary>The native instruction pointer of Ridley's body, from <paramref name="state"/>'s body cursor.</summary>
    internal ushort Body(RidleyEnemyState state) => body[state.BodyProgram.Program][state.BodyProgram.Position];

    /// <summary>The native entry point of body <paramref name="program"/>.</summary>
    internal static ushort BodyEntry(RidleyBodyProgram program) =>
        RidleyNativeListAddresses.BodyEntries.Single(item => item.Program == program).Entry;

    /// <summary>
    /// The native instruction pointer of <paramref name="actor"/>: Ridley's body and breakup fragments and the Pipe Bugs
    /// translate their typed cursors; other actors still hold native pointers.
    /// </summary>
    internal ushort Enemy(RoomEnemySystem enemies, RoomEnemySlot actor)
    {
        if (RidleyBehavior.IsRidleyDefinition(actor.EnemyDefinitionPointer))
            return Body(RequireRidley(enemies));
        if (actor.EnemyDefinitionPointer == EnemyDefinitionId.RidleyExplosion)
        {
            if (!RequireRidley(enemies).BreakupPrograms.TryGetValue(actor.SlotIndex, out AnimationProgramCursor<RidleyBreakupFrame> cursor))
                throw new InvalidOperationException($"Ridley breakup fragment in slot {actor.SlotIndex} has no program.");
            // A fragment's frame sits at its list entry and its hold at the sleep after the frame record.
            return checked((ushort)(RidleyAnimationsVerification.BreakupList(cursor.Program) + (cursor.Position == 0 ? 0 : 4)));
        }
        if (enemies.PipeBugStates[actor.SlotIndex] is { } pipeBug)
            return pipeBugs.Pointer(pipeBug.Program);
        return actor.CurrentInstruction;
    }

    /// <summary>
    /// The native instruction pointer of <paramref name="projectile"/>: Ridley's fireballs and afterburns and every
    /// typed enemy-projectile family translate their typed cursors; projectiles still on native lists hold native pointers.
    /// </summary>
    internal ushort Projectile(RoomEnemyProjectileSlot projectile) => projectile.Animation switch
    {
        null => projectile.InstructionPointer,
        RidleyFireballAnimation animation => fireball[animation.Cursor.Program][animation.Cursor.Position],
        { } animation => projectileFamilies.Select(family => family(animation)).SingleOrDefault(pointer => pointer is not null)
            ?? throw new InvalidOperationException(
                $"Projectile {projectile.Kind} runs a typed {animation.GetType().Name}, which has no native-pointer translation."),
    };

    /// <summary>The body cursor a recording's native pointer <paramref name="address"/> stands for: the program entry when it is one.</summary>
    internal AnimationProgramCursor<RidleyBodyProgram> BodyCursorAt(ushort address)
    {
        if (RidleyNativeListAddresses.BodyProgramAt(address) is { } entry)
            return AnimationProgramCursor<RidleyBodyProgram>.Start(entry);
        var matches = body.SelectMany(program => program.Value.Select((step, position) => (program.Key, step, position)))
            .Where(item => item.step == address)
            .Select(item => new AnimationProgramCursor<RidleyBodyProgram>(item.Key, item.position))
            .ToList();
        return matches.Count == 1
            ? matches[0]
            : throw new InvalidDataException($"Native Ridley instruction pointer $A6:{address:X4} matches {matches.Count} typed steps.");
    }

    /// <summary>Ridley's encounter state.</summary>
    private static RidleyEnemyState RequireRidley(RoomEnemySystem enemies) =>
        enemies.RidleyEncounter.Ridley ?? throw new InvalidOperationException("Ridley's actors require Ridley's encounter state.");
}
