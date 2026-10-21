
namespace SuperMetroid.Core.Programs;

/// <summary>An instruction word and the address it is stored at.</summary>
/// <param name="Address">The address.</param>
/// <param name="Word">The word.</param>
internal readonly record struct AddressedWord(ushort Address, InstructionWord Word);
