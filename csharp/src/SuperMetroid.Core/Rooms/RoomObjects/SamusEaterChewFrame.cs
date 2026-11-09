namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>One image of a Brinstar plant's chew (<see cref="SamusEaterObjectDefinitions"/>).</summary>
/// <param name="Image">What the plant shows.</param>
/// <param name="Updates">How many updates the image stays.</param>
/// <param name="BiteBefore">What the chew does to the held Samus just before the image shows.</param>
internal sealed record SamusEaterChewFrame(SamusEaterDraw Image, int Updates, SamusEaterBite BiteBefore);
