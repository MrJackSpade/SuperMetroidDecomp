namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Editable visual block references for one compiled bank-$84 shot-block draw list.</summary>
/// <remarks>
/// Each value is only the low twelve bits of a level word: ten block-index bits and
/// two parent flips. Collision type, draw geometry, timers, and slot effects stay in
/// <see cref="RoomPlmShotBlockDrawDefinitions"/> and cannot be changed by this catalog.
/// </remarks>
/// <param name="DrawPointer">Compiled bank-$84 draw-list identity, not a PLM header or instruction-list pointer.</param>
/// <param name="Runs">Caller-owned run/word arrays in native draw order, with the compiled one- or two-run shape and one or two words per run; every word contains visual bits 0..11 only.</param>
public sealed record RoomPlmShotBlockVisualEntry(int DrawPointer, ushort[][] Runs);
