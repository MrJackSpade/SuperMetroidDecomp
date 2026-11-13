using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The crumble block a special block's reaction selects (<see cref="BlockReactionDefinitions.ContactCrumbles"/>): a block
/// that breaks up a few updates after Samus lands on it, by its shape and whether it grows back. Its values are arbitrary.
/// </summary>
public enum ContactCrumbleBlock
{
    /// <summary>One block that crumbles under Samus and grows back.</summary>
    Respawning1x1,

    /// <summary>Two blocks side by side that crumble under Samus and grow back.</summary>
    Respawning2x1,

    /// <summary>Two stacked blocks that crumble under Samus and grow back.</summary>
    Respawning1x2,

    /// <summary>A two-by-two square of blocks that crumbles under Samus and grows back.</summary>
    Respawning2x2,

    /// <summary>One block that crumbles under Samus for good.</summary>
    Permanent1x1,

    /// <summary>Two blocks side by side that crumble under Samus for good.</summary>
    Permanent2x1,

    /// <summary>Two stacked blocks that crumble under Samus for good.</summary>
    Permanent1x2,

    /// <summary>A two-by-two square of blocks that crumbles under Samus for good.</summary>
    Permanent2x2,
}
