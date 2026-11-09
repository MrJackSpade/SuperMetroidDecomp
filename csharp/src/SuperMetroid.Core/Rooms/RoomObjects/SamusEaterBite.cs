namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What a Brinstar plant's chew does to the held Samus just before an image shows.</summary>
internal enum SamusEaterBite
{
    /// <summary>Nothing: the image only shows.</summary>
    None,
    /// <summary>The chew sound plays, then the chew deals its damage.</summary>
    SoundAndDamage,
    /// <summary>The chew deals its damage.</summary>
    Damage,
}
