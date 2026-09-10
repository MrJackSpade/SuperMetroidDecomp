#pragma once
#include "snes/dma.h"
extern bool g_calling_asm_from_c;

// Same subroutine boundary as RunAsmCode, with an instruction budget so an
// invalid diagnostic setup cannot leave the headless CPU running indefinitely.
// The host entrypoint must separately suppress explicit SDL error dialogs.
static bool ProbeRunBoundedUntil(uint32 address, uint16 a, uint16 x, uint16 y, uint32 stop) {
  Cpu *cpu = g_snes->cpu;
  uint16 oldSp = cpu->sp, oldPc = cpu->pc, oldDp = cpu->dp;
  uint8 oldDb = cpu->db;
  g_ram[0x1ffff] = 1;
  cpu->db = cpu->k = address >> 16; cpu->pc = address;
  cpu->a = a; cpu->x = x; cpu->y = y; cpu->mf = cpu->xf = false;
  cpu->spBreakpoint = cpu->sp; g_calling_asm_from_c = true;
  bool stopped = false;
  for (int budget = 10000000; g_calling_asm_from_c; budget--) {
    if (stop && (((uint32)cpu->k << 16) | cpu->pc) == stop) {
      stopped = true;
      g_calling_asm_from_c = false;
      break;
    }
    if (!budget) {
      fprintf(stderr, "Probe instruction budget at %02X:%04X (entry %06X, state %04X).\n",
        cpu->k, cpu->pc, address, game_state);
      exit(6);
    }
    cpu_runOpcode(cpu);
    while (g_snes->dma->dmaBusy) dma_doDma(g_snes->dma);
  }
  cpu->sp = oldSp; cpu->pc = oldPc; cpu->dp = oldDp; cpu->db = oldDb;
  return stopped;
}

static void ProbeRunBoundedRegisters(uint32 address, uint16 a, uint16 x, uint16 y) {
  ProbeRunBoundedUntil(address, a, x, y, 0);
}

static void ProbeRunBounded(uint32 address) {
  ProbeRunBoundedRegisters(address, 0, 0, 0);
}
