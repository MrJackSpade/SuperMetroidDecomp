namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The five ordinary projectile slots, the eighteen trail slots and the projectile counter $0CCE.</summary>
internal sealed class ProjectileSlotTable
{
    // `$93:83FF` is only the pointer-table entry that names the beam-explosion DATA
    // record (`$8679`). KillProjectileInner does not install that address. Its assembly
    // reads the instruction-list pointer stored two bytes into the data record, at
    // `$93:867B`. Treating `$83FF` itself as the list made the interpreter consume data
    // tables as eight-byte animation records: the first bogus "spritemap" was the real
    // list pointer, producing unrelated OBJ fragments, and no delete opcode was reached.

    /// <summary>Five indexed slots for live Samus shots; their occupancy does not replace the cartridge projectile counter.</summary>
    internal readonly SamusProjectileSlot[] _slots =
        Enumerable.Range(0, SamusProjectileSystem.SlotCount).Select(index => new SamusProjectileSlot(index)).ToArray();
    /// <summary>Eighteen independently indexed slots for the trails emitted by ordinary projectiles.</summary>
    internal readonly SamusProjectileTrailSlot[] _trailSlots =
        Enumerable.Range(0, SamusProjectileSystem.TrailSlotCount).Select(index => new SamusProjectileTrailSlot(index)).ToArray();

    /// <summary>WRAM <c>$0CCE</c>; maintained separately from free-slot scans by the ROM.</summary>
    public ushort ProjectileCounter { get; internal set; }
}
