namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The four Spore Spawn ceiling draws (#2241), named for what each shows.</summary>
internal enum SporeSpawnCeilingDraw
{
    /// <summary>Clear the ceiling after defeat or crumble.</summary>
    Clear,
    /// <summary>The ceiling's blocks cracked, as its crumble starts: the room's first crumbling block.</summary>
    CeilingCracked,
    /// <summary>The ceiling's blocks breaking apart: the room's second crumbling block.</summary>
    CeilingCrumbling,
    /// <summary>The ceiling's blocks crumbled to debris, before they clear: the room's third crumbling block.</summary>
    CeilingCrumbled,
}
