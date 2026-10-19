using System.Collections.Immutable;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The projectile words the room teardown's projectile-data reset (<c>$82:E43A-$E472</c>) leaves in place: each slot's
/// position fraction words and its Spazer special-attack phase (<c>$0CA4</c>). The next room's projectile system starts
/// from them.
/// </summary>
internal sealed class SamusProjectileResidue
{
    /// <summary>The residue of a session's first room: every word clear.</summary>
    internal static SamusProjectileResidue None { get; } = new(
        [.. Enumerable.Repeat(new SlotResidue(FixedPosition<RoomPixels>.Zero, default), SamusProjectileSystem.SlotCount)]);

    /// <summary>Per-slot residue in native slot order.</summary>
    private readonly ImmutableArray<SlotResidue> _slots;

    /// <summary>Holds one residue per projectile slot.</summary>
    private SamusProjectileResidue(ImmutableArray<SlotResidue> slots) => _slots = slots;

    /// <summary>Captures what the outgoing room's projectile slots leave behind.</summary>
    internal static SamusProjectileResidue Of(SamusProjectileSystem previous) =>
        new([.. Ensure.NotNull(previous).Slots.Select(slot => new SlotResidue(slot.Position, slot.AuxiliaryPhase))]);

    /// <summary>Writes the residue into a new room's cleared slots.</summary>
    internal void ApplyTo(IReadOnlyList<SamusProjectileSlot> slots)
    {
        for (int index = 0; index < slots.Count; index++)
        {
            slots[index].Position = slots[index].Position.WithFractionsOf(_slots[index].Position);
            slots[index].AuxiliaryPhase = _slots[index].AuxiliaryPhase;
        }
    }

    /// <summary>One slot's surviving words: the fractions of its position and its Spazer phase.</summary>
    /// <param name="Position">The slot's position; only its fraction words survive.</param>
    /// <param name="AuxiliaryPhase">The slot's Spazer special-attack phase.</param>
    private readonly record struct SlotResidue(FixedPosition<RoomPixels> Position, SpazerComboPhase AuxiliaryPhase);
}
