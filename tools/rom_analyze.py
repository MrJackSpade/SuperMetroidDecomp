#!/usr/bin/env python3
"""Small, dependency-free SNES ROM inspector and 65C816 linear disassembler."""

from __future__ import annotations

import argparse
import hashlib
from dataclasses import dataclass
from pathlib import Path
import sys


EXPECTED_SHA256 = "12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72"
LOROM_HEADER = 0x7FC0


def u16(data: bytes, offset: int) -> int:
    return data[offset] | data[offset + 1] << 8


def lorom_to_offset(address: int) -> int:
    """Map a bank:address integer to its LoROM file offset."""
    bank, addr = (address >> 16) & 0xFF, address & 0xFFFF
    if addr < 0x8000:
        raise ValueError(f"${bank:02X}:{addr:04X} is not in the ordinary LoROM ROM window")
    return (bank & 0x7F) * 0x8000 + (addr & 0x7FFF)


def offset_to_lorom(offset: int) -> int:
    if offset < 0:
        raise ValueError("negative ROM offset")
    return (0x80 + offset // 0x8000) << 16 | 0x8000 | offset % 0x8000


def parse_address(text: str) -> int:
    cleaned = text.strip().replace("$", "").replace(":", "")
    value = int(cleaned, 16)
    if value <= 0xFFFF:
        value |= 0x800000
    return value


@dataclass(frozen=True)
class Header:
    title: str
    map_mode: int
    cartridge_type: int
    rom_size_code: int
    sram_size_code: int
    region: int
    developer: int
    version: int
    complement: int
    checksum: int

    @classmethod
    def from_rom(cls, rom: bytes) -> "Header":
        h = LOROM_HEADER
        return cls(
            rom[h : h + 21].decode("ascii", errors="replace").rstrip(),
            rom[h + 0x15], rom[h + 0x16], rom[h + 0x17], rom[h + 0x18],
            rom[h + 0x19], rom[h + 0x1A], rom[h + 0x1B],
            u16(rom, h + 0x1C), u16(rom, h + 0x1E),
        )


VECTOR_OFFSETS = {
    "native_cop": 0x7FE4,
    "native_brk": 0x7FE6,
    "native_abort": 0x7FE8,
    "native_nmi": 0x7FEA,
    "native_irq": 0x7FEE,
    "emulation_cop": 0x7FF4,
    "emulation_abort": 0x7FF8,
    "emulation_nmi": 0x7FFA,
    "emulation_reset": 0x7FFC,
    "emulation_irq_brk": 0x7FFE,
}


# Addressing modes determine operand size and presentation. Immediate M/X widths
# are state-dependent; all other sizes are fixed.
ROWS = [
"BRK:i8 ORA:idx COP:i8 ORA:sr TSB:dp ORA:dp ASL:dp ORA:idp PHP:imp ORA:immM ASL:acc PHD:imp TSB:abs ORA:abs ASL:abs ORA:long",
"BPL:rel8 ORA:idy ORA:ind ORA:sry TRB:dp ORA:dpx ASL:dpx ORA:idpy CLC:imp ORA:absy INC:acc TCS:imp TRB:abs ORA:absx ASL:absx ORA:longx",
"JSR:abs AND:idx JSL:long AND:sr BIT:dp AND:dp ROL:dp AND:idp PLP:imp AND:immM ROL:acc PLD:imp BIT:abs AND:abs ROL:abs AND:long",
"BMI:rel8 AND:idy AND:ind AND:sry BIT:dpx AND:dpx ROL:dpx AND:idpy SEC:imp AND:absy DEC:acc TSC:imp BIT:absx AND:absx ROL:absx AND:longx",
"RTI:imp EOR:idx WDM:i8 EOR:sr MVP:block EOR:dp LSR:dp EOR:idp PHA:imp EOR:immM LSR:acc PHK:imp JMP:abs EOR:abs LSR:abs EOR:long",
"BVC:rel8 EOR:idy EOR:ind EOR:sry MVN:block EOR:dpx LSR:dpx EOR:idpy CLI:imp EOR:absy PHY:imp TCD:imp JML:long EOR:absx LSR:absx EOR:longx",
"RTS:imp ADC:idx PER:rel16 ADC:sr STZ:dp ADC:dp ROR:dp ADC:idp PLA:imp ADC:immM ROR:acc RTL:imp JMP:indabs ADC:abs ROR:abs ADC:long",
"BVS:rel8 ADC:idy ADC:ind ADC:sry STZ:dpx ADC:dpx ROR:dpx ADC:idpy SEI:imp ADC:absy PLY:imp TDC:imp JMP:indx ADC:absx ROR:absx ADC:longx",
"BRA:rel8 STA:idx BRL:rel16 STA:sr STY:dp STA:dp STX:dp STA:idp DEY:imp BIT:immM TXA:imp PHB:imp STY:abs STA:abs STX:abs STA:long",
"BCC:rel8 STA:idy STA:ind STA:sry STY:dpx STA:dpx STX:dpy STA:idpy TYA:imp STA:absy TXS:imp TXY:imp STZ:abs STA:absx STZ:absx STA:longx",
"LDY:immX LDA:idx LDX:immX LDA:sr LDY:dp LDA:dp LDX:dp LDA:idp TAY:imp LDA:immM TAX:imp PLB:imp LDY:abs LDA:abs LDX:abs LDA:long",
"BCS:rel8 LDA:idy LDA:ind LDA:sry LDY:dpx LDA:dpx LDX:dpy LDA:idpy CLV:imp LDA:absy TSX:imp TYX:imp LDY:absx LDA:absx LDX:absy LDA:longx",
"CPY:immX CMP:idx REP:i8 CMP:sr CPY:dp CMP:dp DEC:dp CMP:idp INY:imp CMP:immM DEX:imp WAI:imp CPY:abs CMP:abs DEC:abs CMP:long",
"BNE:rel8 CMP:idy CMP:ind CMP:sry PEI:ind CMP:dpx DEC:dpx CMP:idpy CLD:imp CMP:absy PHX:imp STP:imp JMP:indlong CMP:absx DEC:absx CMP:longx",
"CPX:immX SBC:idx SEP:i8 SBC:sr CPX:dp SBC:dp INC:dp SBC:idp INX:imp SBC:immM NOP:imp XBA:imp CPX:abs SBC:abs INC:abs SBC:long",
"BEQ:rel8 SBC:idy SBC:ind SBC:sry PEA:i16 SBC:dpx INC:dpx SBC:idpy SED:imp SBC:absy PLX:imp XCE:imp JSR:indx SBC:absx INC:absx SBC:longx",
]
OPCODES = [tuple(cell.split(":")) for row in ROWS for cell in row.split()]

MODE_SIZE = {
    "imp": 0, "acc": 0, "i8": 1, "dp": 1, "dpx": 1, "dpy": 1,
    "idx": 1, "idy": 1, "ind": 1, "idp": 1, "idpy": 1, "sr": 1,
    "sry": 1, "rel8": 1, "abs": 2, "absx": 2, "absy": 2,
    "indabs": 2, "indx": 2, "indlong": 2, "rel16": 2, "i16": 2,
    "block": 2, "long": 3, "longx": 3,
}


def operand_size(mode: str, m8: bool, x8: bool) -> int:
    if mode == "immM":
        return 1 if m8 else 2
    if mode == "immX":
        return 1 if x8 else 2
    return MODE_SIZE[mode]


def format_operand(mode: str, raw: bytes, pc: int) -> str:
    if not raw:
        return "A" if mode == "acc" else ""
    value = int.from_bytes(raw, "little")
    if mode in ("i8", "immM", "immX"):
        return f"#${value:0{len(raw) * 2}X}"
    if mode == "i16": return f"#${value:04X}"
    if mode == "dp": return f"${value:02X}"
    if mode == "dpx": return f"${value:02X},X"
    if mode == "dpy": return f"${value:02X},Y"
    if mode == "idx": return f"(${value:02X},X)"
    if mode == "idy": return f"(${value:02X}),Y"
    if mode == "ind": return f"(${value:02X})"
    if mode == "idp": return f"[${value:02X}]"
    if mode == "idpy": return f"[${value:02X}],Y"
    if mode == "sr": return f"${value:02X},S"
    if mode == "sry": return f"(${value:02X},S),Y"
    if mode == "abs": return f"${value:04X}"
    if mode == "absx": return f"${value:04X},X"
    if mode == "absy": return f"${value:04X},Y"
    if mode == "indabs": return f"(${value:04X})"
    if mode == "indx": return f"(${value:04X},X)"
    if mode == "indlong": return f"[${value:04X}]"
    if mode == "long": return f"${value >> 16:02X}:{value & 0xFFFF:04X}"
    if mode == "longx": return f"${value >> 16:02X}:{value & 0xFFFF:04X},X"
    if mode == "block": return f"${raw[0]:02X},${raw[1]:02X}"
    if mode == "rel8":
        delta = value - 0x100 if value & 0x80 else value
        return f"${(pc & 0xFF0000) | ((pc + 2 + delta) & 0xFFFF):06X}"
    if mode == "rel16":
        delta = value - 0x10000 if value & 0x8000 else value
        return f"${(pc & 0xFF0000) | ((pc + 3 + delta) & 0xFFFF):06X}"
    raise AssertionError(mode)


def disassemble(rom: bytes, start_address: int, count: int):
    offset = lorom_to_offset(start_address)
    address = start_address
    m8 = x8 = True  # Reset begins in emulation mode, which forces both flags.
    for _ in range(count):
        if offset >= len(rom): break
        opcode = rom[offset]
        mnemonic, mode = OPCODES[opcode]
        size = operand_size(mode, m8, x8)
        instruction = rom[offset : offset + 1 + size]
        if len(instruction) != 1 + size: break
        operand = format_operand(mode, instruction[1:], address)
        yield address, instruction, mnemonic, operand, m8, x8
        if mnemonic == "REP":
            mask = instruction[1]
            if mask & 0x20: m8 = False
            if mask & 0x10: x8 = False
        elif mnemonic == "SEP":
            mask = instruction[1]
            if mask & 0x20: m8 = True
            if mask & 0x10: x8 = True
        offset += len(instruction)
        address = (address & 0xFF0000) | ((address + len(instruction)) & 0xFFFF)


def print_info(rom: bytes) -> None:
    header = Header.from_rom(rom)
    sha256 = hashlib.sha256(rom).hexdigest()
    nominal_rom = 1 << (header.rom_size_code + 10)
    sram = 0 if not header.sram_size_code else 1 << (header.sram_size_code + 10)
    print(f"Title:       {header.title}")
    print(f"ROM bytes:   {len(rom):,} ({len(rom) * 8 // (1024 * 1024)} Mbit)")
    print(f"SHA-256:     {sha256}")
    print(f"Known NTSC:  {'yes' if sha256 == EXPECTED_SHA256 else 'NO'}")
    print(f"Map mode:    ${header.map_mode:02X} (FastROM LoROM)")
    print(f"Cart type:   ${header.cartridge_type:02X} (ROM + SRAM + battery)")
    print(f"ROM header:  {nominal_rom:,} bytes nominal (actual image is a valid 24-Mbit size)")
    print(f"SRAM:        {sram:,} bytes")
    print(f"Region:      ${header.region:02X} (Japan; shared Japan/USA program)")
    print(f"Version:     1.{header.version}")
    print(f"Checksum:    ${header.checksum:04X} / complement ${header.complement:04X}")
    print("Vectors:")
    for name, offset in VECTOR_OFFSETS.items():
        print(f"  {name:18} ${u16(rom, offset):04X}")


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("rom", type=Path)
    commands = parser.add_subparsers(dest="command", required=True)
    commands.add_parser("info", help="print internal header, identity, and vectors")
    dis = commands.add_parser("disasm", help="linearly disassemble from a LoROM address")
    dis.add_argument("--address", type=parse_address, default=parse_address("00:841C"))
    dis.add_argument("--count", type=int, default=40)
    args = parser.parse_args(argv)
    rom = args.rom.read_bytes()
    if len(rom) % 0x8000 != 0:
        parser.error("ROM has a copier header or is not an integral number of LoROM banks")
    if args.command == "info":
        print_info(rom)
    else:
        for address, raw, mnemonic, operand, m8, x8 in disassemble(rom, args.address, args.count):
            encoded = raw.hex(" ").upper().ljust(11)
            flags = f"M={'8' if m8 else '16'} X={'8' if x8 else '16'}"
            print(f"{address >> 16:02X}:{address & 0xFFFF:04X}  {encoded} {mnemonic:<4} {operand:<14} ; {flags}")
    return 0


if __name__ == "__main__":
    sys.exit(main())

