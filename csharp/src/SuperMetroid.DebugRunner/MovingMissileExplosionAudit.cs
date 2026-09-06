using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Finds the reported travelling explosion, independently of firing-frame contact.</summary>
internal sealed class MovingMissileExplosionAudit(ISnesAddressSpace bus, bool failOnMovement)
{
    private readonly HashSet<ushort> explosionSprites = ReadExplosionSprites(bus);
    private readonly Dictionary<int, (ushort Type, ushort X, ushort Y)> previous = new();
    private ushort? previousRoom;

    public void Observe(int frame, ushort? room, SamusProjectileSystem projectiles)
    {
        if (room != previousRoom) previous.Clear();
        previousRoom = room;
        foreach (var slot in projectiles.Slots)
        {
            bool explosion = slot.PackedType.IsFamily(SamusProjectileFamily.MissileExplosion);
            bool flying = slot.PackedType.IsFamily(SamusProjectileFamily.Missile) ||
                slot.PackedType.IsFamily(SamusProjectileFamily.SuperMissile);
            if (slot.IsActive && previous.TryGetValue(slot.SlotIndex, out var old) &&
                (old.X != slot.XPosition || old.Y != slot.YPosition) &&
                projectiles.LastFrameResult.FiredSlot != slot.SlotIndex &&
                ((explosion && old.Type == slot.Type) ||
                 (flying && explosionSprites.Contains(slot.SpritemapPointer))))
            {
                string failure = $"MOVING EXPLOSION frame={frame} room={room:X4} slot={slot.SlotIndex} " +
                    $"type={old.Type:X4}->{slot.Type:X4} xy={old.X:X4},{old.Y:X4}->{slot.XPosition:X4},{slot.YPosition:X4} " +
                    $"map={slot.SpritemapPointer:X4} pre={slot.PreInstruction} instruction={slot.InstructionPointer:X4}.";
                Console.WriteLine(failure);
                if (failOnMovement) throw new InvalidDataException(failure);
            }
            if (slot.IsActive) previous[slot.SlotIndex] = (slot.Type, slot.XPosition, slot.YPosition);
            else previous.Remove(slot.SlotIndex);
        }
    }

    private static HashSet<ushort> ReadExplosionSprites(ISnesAddressSpace bus)
    {
        ushort Word(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
        var result = new HashSet<ushort>();
        foreach (int entry in new[] { SamusProjectileRomData.NonBeam.MissileExplosionInstructionPointer,
            SamusProjectileRomData.NonBeam.SuperMissileExplosionInstructionPointer })
        {
            ushort pointer = Word(entry);
            var visited = new HashSet<ushort>();
            while (visited.Add(pointer))
            {
                int address = SamusProjectileRomData.Banks.Projectile | pointer;
                ushort command = Word(address);
                if (command == SamusProjectileRomData.Instructions.Delete) break;
                if (command == SamusProjectileRomData.Instructions.GoTo) { pointer = Word(address + 2); continue; }
                if ((command & 0x8000) != 0) throw new InvalidDataException("Unknown explosion oracle instruction.");
                result.Add(Word(address + 2));
                pointer += 8;
            }
        }
        return result;
    }
}
