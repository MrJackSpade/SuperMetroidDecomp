#include "native-bounded-cpu.h"

// Original 65816 wave setup/update, not the upstream C reimplementation.
int DiagnosticPhantoonHdma(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  const int amplitudes[] = {0, 0x40, 0x100, 0x340, 0xc00, 0xffff};
  fprintf(f, "mode,amplitude,frame,phase,index,scroll\n");
  for (int mode = 1; mode <= 2; mode++) for (int a = 0; a < 6; a++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    enemy_data[2].parameter_1 = mode;
    enemy_data[3].ai_var_D = amplitudes[a];
    enemy_data[3].ai_preinstr = 8;
    reg_BG2HOFS = 0xffa8;
    ProbeRunBoundedRegisters(0x88e4bd, 0, 0, 0);
    for (int frame = 0; frame < 40; frame++) {
      hdma_object_index = 0;
      ProbeRunBoundedRegisters(0x88e567, 0, 0, 0);
      int count = (mode & 1) ? 128 : 64;
      for (int i = 0; i < count; i++) {
        int p = 0x9100 + 2 * i;
        fprintf(f, "%d,%d,%d,%d,%d,%d\n", mode, amplitudes[a], frame,
          hdma_object_A[0], i, g_ram[p] | g_ram[p+1] << 8);
      }
    }
  }
  fclose(f); return 0;
}
