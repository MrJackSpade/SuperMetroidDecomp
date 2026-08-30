using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Exhaustively dispatches the power-bomb reaction reachable from every authored retail
/// enemy record. A passive lifecycle can execute for minutes without touching this bank-$A0
/// path, so it must not be treated as evidence that weapon damage and private reactions work.
/// </summary>
internal static partial class RetailEnemyExecutionAudit
{
    private const ushort DefaultVulnerabilityPointer = 0xec1c;

    public static int RunPowerBombCombat(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        RetailRoomState[] states = LoadNamedRetailStates();
        var testedVariants = new HashSet<PowerBombVariant>();
        var reachedDefinitions = new HashSet<ushort>();
        var immuneDefinitions = new HashSet<ushort>();
        var unavailableDefinitions = new HashSet<ushort>();
        var failures = new List<PowerBombFailure>();

        foreach (RetailRoomState state in states)
        {
            try
            {
                CartridgeRoomHeader defaultRoom = CartridgeRoomHeader.Load(bus, state.RoomPointer);
                CartridgeRoomState exactState = CartridgeRoomState.Load(bus, state.StatePointer);
                CartridgeRoomHeader room = defaultRoom with { State = exactState };
                CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
                LoadedRetailState initial = LoadState(bus, room, assets);

                for (int slotIndex = 0; slotIndex < initial.Enemies.EnemyCount; slotIndex++)
                {
                    RoomEnemySlot initialTarget = initial.Enemies.Slots[slotIndex];
                    if (initialTarget.EnemyDefinitionPointer is 0 or 0xffff)
                        continue;

                    var variant = new PowerBombVariant(
                        initialTarget.EnemyDefinitionPointer,
                        initialTarget.Parameter1,
                        initialTarget.Parameter2,
                        initialTarget.Properties,
                        initialTarget.ExtraProperties);
                    if (!testedVariants.Add(variant))
                        continue;

                    ushort vulnerabilityPointer =
                        initialTarget.Definition.VulnerabilityPointer != 0
                            ? initialTarget.Definition.VulnerabilityPointer
                            : DefaultVulnerabilityPointer;
                    byte vulnerability = bus.ReadByte(
                        0xb40000 | unchecked((ushort)(vulnerabilityPointer + 15)));
                    if ((vulnerability & 0x7f) == 0)
                    {
                        // Native returns before the callback for a zero multiplier. This is a
                        // proved immunity, not an unexecuted translated-reaction claim.
                        immuneDefinitions.Add(initialTarget.EnemyDefinitionPointer);
                        continue;
                    }

                    LoadedRetailState loaded = LoadState(bus, room, assets);
                    RoomEnemySlot target = loaded.Enemies.Slots[slotIndex];
                    if (target.EnemyDefinitionPointer != variant.Definition ||
                        target.Properties.HasAny(EnemyProperties.Deleted))
                    {
                        // Some one-shot palette/controller records delete themselves during
                        // initialization. They cannot enter the native power-bomb walker.
                        unavailableDefinitions.Add(variant.Definition);
                        continue;
                    }

                    // Preserve every companion slot and its ownership links, but move its
                    // collision origin outside the strict 32x24 ellipse. Deleting helpers can
                    // invalidate parent callbacks; mutating the cartridge record would be an
                    // even weaker fixture. Position isolation changes neither target state nor
                    // the callback selected from its real header.
                    for (int otherIndex = 0; otherIndex < loaded.Enemies.EnemyCount; otherIndex++)
                    {
                        if (otherIndex == slotIndex)
                            continue;
                        RoomEnemySlot companion = loaded.Enemies.Slots[otherIndex];
                        companion.XPosition = unchecked((ushort)(target.XPosition + 0x4000));
                        companion.YPosition = unchecked((ushort)(target.YPosition + 0x4000));
                    }

                    target.InvincibilityTimer = 0;
                    int reactions = loaded.Enemies.ResolveOrdinaryPowerBombHits(
                        bus,
                        target.XPosition,
                        target.YPosition,
                        explosionRadius: 32,
                        loaded.Samus);
                    if (reactions != 1)
                    {
                        throw new InvalidDataException(
                            $"Power-bomb vulnerability ${vulnerability:X2} admitted " +
                            $"definition ${variant.Definition:X4}, but the collision walker " +
                            $"reported {reactions} reactions.");
                    }
                    reachedDefinitions.Add(variant.Definition);
                }
            }
            catch (Exception exception)
            {
                failures.Add(new PowerBombFailure(state, exception));
            }
        }

        if (failures.Count != 0)
        {
            Console.Error.WriteLine(
                $"Retail enemy power-bomb audit found {failures.Count} failing room states:");
            foreach (IGrouping<string, PowerBombFailure> group in failures
                .GroupBy(failure =>
                    $"{failure.Exception.GetType().Name}: {failure.Exception.Message}")
                .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                Console.Error.WriteLine($"  {group.Key}");
                foreach (PowerBombFailure failure in group.Take(12))
                {
                    Console.Error.WriteLine(
                        $"    room/state $8F:{failure.State.RoomPointer:X4}/" +
                        $"${failure.State.StatePointer:X4} {failure.State.Symbol}");
                }
                if (group.Count() > 12)
                    Console.Error.WriteLine($"    ... and {group.Count() - 12} more states");
            }
            return 1;
        }

        Console.WriteLine(
            $"Retail enemy power-bomb audit passed: {testedVariants.Count} authored " +
            $"definition/parameter/property variants inspected; callbacks executed for " +
            $"{reachedDefinitions.Count} definitions, {immuneDefinitions.Count} definitions " +
            $"proved immune at vulnerability byte 15, and {unavailableDefinitions.Count} " +
            $"one-shot definitions were already deleted by their cartridge init AI.");
        return 0;
    }

    private readonly record struct PowerBombVariant(
        ushort Definition,
        ushort Parameter1,
        ushort Parameter2,
        ushort Properties,
        ushort ExtraProperties);

    private sealed record PowerBombFailure(
        RetailRoomState State,
        Exception Exception);
}
