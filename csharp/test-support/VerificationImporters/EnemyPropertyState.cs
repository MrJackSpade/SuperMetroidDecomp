using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The slot state a pair of native enemy property words describes (#1549): how the enemy meets Samus, what the room's
/// enemy passes run for it, whether it is present, whether it collides by its frames' hitboxes and whether its animation just
/// installed a frame. It exists only at the boundary where <see cref="EnemyPropertyWordCodec"/> converts the words; the
/// game holds the five parts on <see cref="RoomEnemySlot"/>.
/// </summary>
/// <param name="Contact">How the enemy meets Samus and her shots.</param>
/// <param name="Processing">What the room's enemy passes run for the enemy.</param>
/// <param name="Presence">Whether the enemy is drawn, occupies its slot and returns after a kill.</param>
/// <param name="CollidesByFrameHitboxes">Whether the enemy collides by its frames' hitboxes.</param>
/// <param name="NewInstructionFrame">Whether the enemy's animation program just installed a new timed frame.</param>
internal readonly record struct EnemyPropertyState(
    EnemyContact Contact,
    EnemyProcessing Processing,
    EnemyPresence Presence,
    bool CollidesByFrameHitboxes,
    bool NewInstructionFrame)
{
    /// <summary>The state that clears every part.</summary>
    public static EnemyPropertyState None => default;

    /// <summary>The five parts <paramref name="slot"/> holds.</summary>
    public static EnemyPropertyState Of(RoomEnemySlot slot)
    {
        ArgumentNullException.ThrowIfNull(slot);
        return new EnemyPropertyState(slot.Contact, slot.Processing, slot.Presence, slot.CollidesByFrameHitboxes, slot.AnimationClock.ShowsNewFrame);
    }

    /// <summary>Stores the five parts on <paramref name="slot"/>.</summary>
    public void ApplyTo(RoomEnemySlot slot)
    {
        ArgumentNullException.ThrowIfNull(slot);
        slot.Contact = Contact;
        slot.Processing = Processing;
        slot.Presence = Presence;
        slot.CollidesByFrameHitboxes = CollidesByFrameHitboxes;
        slot.AnimationClock.ShowsNewFrame = NewInstructionFrame;
    }
}
