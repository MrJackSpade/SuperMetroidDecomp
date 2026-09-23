// Generated from the pinned cartridge by tools/generate-library-background-programs.ps1.
// Fixed command operands only; graphics and tilemaps remain installed presentation assets.
namespace SuperMetroid.Core.Rooms;

public static partial class LibraryBackgroundProgramDefinitions
{
    private static readonly LibraryBackgroundProgram[] programs =
    [
        new(0xB76A, [
            new(LibraryBackgroundCommand.TransferForDoor, 0x8AC180, 0x4800, 0x0800, 0x8946),
            new(LibraryBackgroundCommand.TransferForDoor, 0x8AD180, 0x4800, 0x0800, 0x896A),
            new(LibraryBackgroundCommand.TransferForDoor, 0x8AB980, 0x4C00, 0x0800, 0x89B2),
            new(LibraryBackgroundCommand.TransferForDoor, 0x8AD180, 0x4800, 0x0800, 0x8AC6),
            new(LibraryBackgroundCommand.TransferForDoor, 0x8AB180, 0x4800, 0x0800, 0x88FE),
            new(LibraryBackgroundCommand.TransferForDoor, 0x8AC180, 0x4800, 0x0800, 0x890A),
        ], 0x0044),
        new(0xB7AE, [
            new(LibraryBackgroundCommand.TransferForDoor, 0x8AC180, 0x4800, 0x0800, 0x8A12),
            new(LibraryBackgroundCommand.TransferForDoor, 0x8AD980, 0x4800, 0x0800, 0x8AEA),
            new(LibraryBackgroundCommand.TransferForDoor, 0x8AB980, 0x4C00, 0x0800, 0xA18C),
            new(LibraryBackgroundCommand.TransferForDoor, 0x8AD980, 0x4800, 0x0800, 0xA1B0),
            new(LibraryBackgroundCommand.TransferForDoor, 0x8AB180, 0x4800, 0x0800, 0xA1E0),
            new(LibraryBackgroundCommand.TransferForDoor, 0x8AC980, 0x4C00, 0x0800, 0xA300),
        ], 0x0044),
        new(0xB7F2, [
            new(LibraryBackgroundCommand.TransferForDoor, 0x8AD980, 0x4800, 0x0800, 0x8A7E),
            new(LibraryBackgroundCommand.TransferForDoor, 0x8AD980, 0x4800, 0x0800, 0xA264),
        ], 0x0018),
        new(0xB80A, [
            new(LibraryBackgroundCommand.TransferToVram, 0x8AC180, 0x4800, 0x0800, 0x0000),
        ], 0x000B),
        new(0xB815, [
            new(LibraryBackgroundCommand.TransferToVramForKraid, 0x9AB200, 0x2000, 0x1000, 0x0000),
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9FA38, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4000, 0x1000, 0x0000),
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9FE3E, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x1000, 0x0000),
        ], 0x002B),
        new(0xB840, [
            new(LibraryBackgroundCommand.TransferToVramForKraid, 0x9AB200, 0x2000, 0x1000, 0x0000),
            new(LibraryBackgroundCommand.ClearBg2ForKraid, 0x000000, 0x0000, 0x0000, 0x0000),
        ], 0x000D),
        new(0xB84D, [
            new(LibraryBackgroundCommand.TransferToVram, 0x7E2000, 0x4800, 0x1000, 0x0000),
        ], 0x000B),
        new(0xB858, [
            new(LibraryBackgroundCommand.TransferToVram, 0x7E2000, 0x4800, 0x1000, 0x0000),
        ], 0x000B),
        new(0xB87E, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBA807E, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xB899, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBA82C4, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xB8B4, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBA8437, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xB8CF, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBA85BA, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xB8EA, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBA86FC, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xB905, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBA8780, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xB920, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBA8A49, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xB93B, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBA8ACD, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xB956, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBA8DBD, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBA37, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9C972, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBA52, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9CD01, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBA6D, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9CE9F, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBA88, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9CFF8, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBAA3, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9D1FB, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBABE, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9D38F, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBAD9, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9D3C5, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBAF4, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9D3FB, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBB45, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9D5D8, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBB60, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBAC4BC, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x1000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x87AD64, 0x6D00, 0x0600, 0x0000),
        ], 0x001B),
        new(0xBB7B, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9D715, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBBCC, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9E1B3, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBBE7, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9E61C, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBC02, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9E885, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBC38, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9EA80, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBC53, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9EBC7, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBC6E, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9EE52, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBCA4, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9F1C8, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x1000, 0x0000),
        ], 0x0012),
        new(0xBCEC, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9F94F, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBE3F, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9A634, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBE5A, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9A714, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBE90, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9A7A8, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBEAB, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9A83A, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBEC6, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9AC83, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBEE1, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9AEFF, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBEFC, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9B2F0, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBF17, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9B6BB, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBF32, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9BBA5, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBF4D, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9BF3B, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBF68, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9C26F, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xBF83, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xB9C5C8, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xE0FD, [
            new(LibraryBackgroundCommand.TransferToVram, 0x7E2000, 0x4800, 0x1000, 0x0000),
        ], 0x000B),
        new(0xE108, [
            new(LibraryBackgroundCommand.TransferToVram, 0x7E2000, 0x4800, 0x1000, 0x0000),
        ], 0x000B),
        new(0xE113, [
            new(LibraryBackgroundCommand.ClearBg2, 0x000000, 0x0000, 0x0000, 0x0000),
        ], 0x0004),
        new(0xE117, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBA8DE7, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xE14D, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBA9386, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xE168, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBA988D, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xE183, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBA9C35, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xE19E, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBA9F12, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xE1B9, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBAA119, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xE1D4, [
            new(LibraryBackgroundCommand.ClearBg2, 0x000000, 0x0000, 0x0000, 0x0000),
        ], 0x0004),
        new(0xE248, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBAA475, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x1000, 0x0000),
        ], 0x0012),
        new(0xE25A, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBAA69F, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x1000, 0x0000),
        ], 0x0012),
        new(0xE3E8, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBAAA78, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xE403, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBAADF0, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xE41E, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBAAFE6, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xE439, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBAB36B, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xE454, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBAB5D8, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xE46F, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBAB9A3, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xE48A, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBABDD9, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
        new(0xE4A5, [
            new(LibraryBackgroundCommand.DecompressToWorkRam, 0xBAC22A, 0x4000, 0x0000, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4800, 0x0800, 0x0000),
            new(LibraryBackgroundCommand.TransferToVram, 0x7E4000, 0x4C00, 0x0800, 0x0000),
        ], 0x001B),
    ];
}
