namespace SuperMetroid.Core.Hardware;

/// <summary>
/// An upload over the OBJ characters an encounter makes after its room loads (#1392): the artwork it shows and the table
/// bytes it shows it in. A pre-coloured frame drawn over those bytes has a picture for each upload that can be there.
/// </summary>
/// <param name="Source">The artwork uploaded.</param>
/// <param name="FirstByte">The OBJ character table byte it shows from.</param>
/// <param name="ByteCount">The bytes it shows.</param>
public sealed record RuntimeCharacterUpload(ObjCharacterSource Source, int FirstByte, int ByteCount)
{
}
