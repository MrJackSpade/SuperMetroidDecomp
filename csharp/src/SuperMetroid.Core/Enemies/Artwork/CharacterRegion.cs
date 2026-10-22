namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// A run of OBJ character table bytes that each upload a graphics set's encounters make after its room loads covers whole
/// or not at all (#1392); a frame drawn over it has a picture for each upload that can show there.
/// </summary>
/// <param name="FirstByte">The region's first table byte.</param>
/// <param name="ByteCount">Its bytes.</param>
public sealed record CharacterRegion(int FirstByte, int ByteCount);
