#pragma once
/* Shared console-only original-ROM CPU adapter. No PPU or audio emulation. */
#include <stdio.h>
#include <stdlib.h>
#include <stdbool.h>
#include <stdint.h>
#include <string.h>
#include "../../../upstream-sm/src/snes/cpu.h"
#include "../../../upstream-sm/src/snes/snes.h"
#include "../../../upstream-sm/src/variables.h"

uint8 g_ram[0x20000];
static uint8 rom[0x300000];
static bool returned;
static unsigned multiplicand, dividend, quotient, remainder;
#ifdef CAPTURE_SAVE_RAM
/* Disposable LoROM battery RAM for native save/load diagnostic entry points. */
static uint8 fixture_sram[0x2000];
#endif
#ifdef CAPTURE_DMA_CHANNEL_REGISTERS
/* Some movement handlers allocate HDMA objects. Store their channel registers
   faithfully without claiming to execute DMA or render the resulting window. */
enum { DmaChannelRegistersStart = 0x4300, DmaChannelRegistersEnd = 0x4380 };
static uint8 dma_channel_registers[DmaChannelRegistersEnd - DmaChannelRegistersStart];
#endif
void Die(const char *message) { fprintf(stderr, "%s\n", message); exit(2); }
int CpuOpcodeHook(uint32 address) { fprintf(stderr, "Unexpected hook %06X\n", address); exit(2); }
bool HookedFunctionRts(int is_long) { (void)is_long; returned = true; return true; }
uint8 snes_cpuRead(Snes *snes, uint32 address) {
  (void)snes;
  unsigned bank = address >> 16, offset = address & 0xffff;
#ifdef CAPTURE_SAVE_RAM
  if ((bank & 0x7f) >= 0x70 && (bank & 0x7f) <= 0x7d && offset < 0x8000)
    return fixture_sram[offset & 0x1fff];
#endif
  if (bank == 0x7e || bank == 0x7f) return g_ram[address - 0x7e0000];
  if ((bank & 0x7f) < 0x40) {
    if (offset < 0x2000) return g_ram[offset];
#ifdef CAPTURE_DMA_CHANNEL_REGISTERS
    if (offset >= DmaChannelRegistersStart && offset < DmaChannelRegistersEnd)
      return dma_channel_registers[offset - DmaChannelRegistersStart];
#endif
    if (offset == 0x4214) return quotient;
    if (offset == 0x4215) return quotient >> 8;
    if (offset == 0x4216) return remainder;
    if (offset == 0x4217) return remainder >> 8;
  }
  unsigned index = ((bank & 0x7f) << 15) | (offset & 0x7fff);
  if (offset >= 0x8000 && index < sizeof(rom)) return rom[index];
  fprintf(stderr, "Unmapped read %06X\n", address); exit(2);
}
void snes_cpuWrite(Snes *snes, uint32 address, uint8 value) {
  (void)snes;
  unsigned bank = address >> 16, offset = address & 0xffff;
#ifdef CAPTURE_SAVE_RAM
  if ((bank & 0x7f) >= 0x70 && (bank & 0x7f) <= 0x7d && offset < 0x8000) {
    fixture_sram[offset & 0x1fff] = value; return;
  }
#endif
  if (bank == 0x7e || bank == 0x7f) { g_ram[address - 0x7e0000] = value; return; }
  if ((bank & 0x7f) < 0x40) {
    if (offset < 0x2000) { g_ram[offset] = value; return; }
#ifdef CAPTURE_DMA_CHANNEL_REGISTERS
    if (offset >= DmaChannelRegistersStart && offset < DmaChannelRegistersEnd) {
      dma_channel_registers[offset - DmaChannelRegistersStart] = value; return;
    }
#endif
    if (offset == 0x4202) { multiplicand = value; return; }
    if (offset == 0x4203) { remainder = multiplicand * value; return; }
    if (offset == 0x4204) { dividend = (dividend & 0xff00) | value; return; }
    if (offset == 0x4205) { dividend = (dividend & 0xff) | value << 8; return; }
    if (offset == 0x4206) {
      quotient = value ? dividend / value : 0xffff;
      remainder = value ? dividend % value : dividend; return;
    }
  }
  fprintf(stderr, "Unmapped write %06X\n", address); exit(2);
}
static void run(unsigned address) {
  Cpu *cpu = cpu_init(NULL, 0);
  cpu->pc = address; cpu->k = cpu->db = address >> 16;
  cpu->sp = cpu->spBreakpoint = 0x1ff0;
  returned = false;
  for (int n = 0; n < 1000000 && !returned; n++) cpu_runOpcode(cpu);
  cpu_free(cpu);
  if (!returned) Die("Movement exceeded instruction budget");
}
