using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>The owner, address space and palette of one verify ceres alarm palette case.</summary>
/// <param name="Owner">The owner.</param>
/// <param name="AddressSpace">The address space.</param>
/// <param name="Palette">The palette.</param>
internal readonly record struct VerifyCeresAlarmPaletteCase(RoomEnemySystem Owner, ISnesAddressSpace AddressSpace, SnesCgram Palette);
