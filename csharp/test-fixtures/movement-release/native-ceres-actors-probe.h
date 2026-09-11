#include "native-bounded-cpu.h"

// #512: execute the retail initializers and sprite handler, one actor at a time.
// This isolates animation/motion from music scheduling and the rest of the PPU.
int DiagnosticCeresActors(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  const uint16 definitions[] = { 0xcebb, 0xcec1, 0xcec7 };
  const int counts[] = { 5, 8, 4 };
  fprintf(f, "group,param,frame,active,x,xsub,y,ysub,map\n");
  for (int group = 0; group < 3; group++) for (int param = 0; param < counts[group]; param++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    reg_M7X = 52; reg_M7Y = 48;
    ProbeRunBoundedRegisters(0x8b938a, param, 0, definitions[group]);
    for (int frame = 0; frame < 260; frame++) {
      ProbeRunBounded(0x8b93ef);
      fprintf(f, "%d,%d,%d,%d,%u,%u,%u,%u,%u\n", group,param,frame,
        cinematicspr_instr_ptr[15] != 0, cinematicbg_arr7[15],cinematicspr_arr6[15],
        cinematicbg_arr8[15],cinematicspr_arr7[15],cinematicspr_whattodraw[15]);
    }
  }
  fclose(f); return 0;
}
