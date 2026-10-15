namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The eight bank-$84 Samus Eater plant draw lists, valued by native address.</summary>
internal enum SamusEaterDraw : ushort
{
    /// <summary>Native floor-idle draw list at $84:9E0D.</summary>
    FloorIdle = 0x9e0d,
    /// <summary>Native floor-chew-1 draw list at $84:9E45.</summary>
    FloorChew1 = 0x9e45,
    /// <summary>Native floor-chew-2 draw list at $84:9E61.</summary>
    FloorChew2 = 0x9e61,
    /// <summary>Native floor-chew-3 draw list at $84:9E7D.</summary>
    FloorChew3 = 0x9e7d,
    /// <summary>Native ceiling-idle draw list at $84:9E99.</summary>
    CeilingIdle = 0x9e99,
    /// <summary>Native ceiling-chew-1 draw list at $84:9ED1.</summary>
    CeilingChew1 = 0x9ed1,
    /// <summary>Native ceiling-chew-2 draw list at $84:9EED.</summary>
    CeilingChew2 = 0x9eed,
    /// <summary>Native ceiling-chew-3 draw list at $84:9F09.</summary>
    CeilingChew3 = 0x9f09,
}
