import sys
from pathlib import Path
import unittest

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / "tools"))
import rom_analyze


class MappingTests(unittest.TestCase):
    def test_lorom_mirrors(self):
        self.assertEqual(rom_analyze.lorom_to_offset(0x00841C), 0x041C)
        self.assertEqual(rom_analyze.lorom_to_offset(0x80841C), 0x041C)
        self.assertEqual(rom_analyze.lorom_to_offset(0x8F8000), 0x078000)

    def test_offset_to_canonical_address(self):
        self.assertEqual(rom_analyze.offset_to_lorom(0x041C), 0x80841C)
        self.assertEqual(rom_analyze.offset_to_lorom(0x8000), 0x818000)

    def test_rejects_low_half_of_ordinary_bank(self):
        with self.assertRaises(ValueError):
            rom_analyze.lorom_to_offset(0x807FFF)


class DecoderTests(unittest.TestCase):
    def test_rep_changes_immediate_width(self):
        # REP #$30; LDA #$1234; LDX #$5678
        rom = bytes(0x423) + bytes.fromhex("C2 30 A9 34 12 A2 78 56")
        decoded = list(rom_analyze.disassemble(rom, 0x808423, 3))
        self.assertEqual(decoded[1][1], bytes.fromhex("A9 34 12"))
        self.assertEqual(decoded[2][1], bytes.fromhex("A2 78 56"))

    def test_relative_target_wraps_within_bank(self):
        self.assertEqual(rom_analyze.format_operand("rel8", b"\xfd", 0x80_9000), "$808FFF")

    def test_opcode_table_is_complete(self):
        self.assertEqual(len(rom_analyze.OPCODES), 256)


if __name__ == "__main__":
    unittest.main()
