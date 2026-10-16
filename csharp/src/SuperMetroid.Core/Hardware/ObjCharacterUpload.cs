namespace SuperMetroid.Core.Hardware;

/// <summary>One upload shown in the OBJ character table.</summary>
/// <param name="FirstByte">Table byte the shown range starts at.</param>
/// <param name="ByteCount">Bytes shown.</param>
/// <param name="Source">The owned artwork or memory bytes uploaded.</param>
/// <param name="SourceOffset">Source byte the shown range starts at.</param>
public readonly record struct ObjCharacterUpload(int FirstByte, int ByteCount, ObjCharacterSource Source, int SourceOffset);
