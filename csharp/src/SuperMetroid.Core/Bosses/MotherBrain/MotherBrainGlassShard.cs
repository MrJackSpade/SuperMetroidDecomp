namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Which way a glass shard projectile flies: its bank-$86 spawn parameter (#1296).</summary>
internal enum MotherBrainGlassShard : ushort
{
    /// <summary>Flies right.</summary>
    Right = 0,
    /// <summary>Flies left.</summary>
    Left = 2,
    /// <summary>Falls from the centre.</summary>
    Centre = 4,
}
