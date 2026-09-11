#include "native-bounded-cpu.h"

// #512: execute the retail initializers and sprite handler, one actor at a time.
// This isolates animation/motion from music scheduling and the rest of the PPU.
int DiagnosticCeresActors(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  const uint16 definitions[] = { 0xcebb, 0xcec1, 0xcec7 };
  const int counts[] = { 5, 8, 4 };
  fprintf(f, "group,param,frame,active,x,xsub,y,ysub,map,oam,high\n");
  for (int group = 0; group < 3; group++) for (int param = 0; param < counts[group]; param++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    reg_M7X = 52; reg_M7Y = 48;
    ProbeRunBoundedRegisters(0x8b938a, param, 0, definitions[group]);
    for (int frame = 0; frame < 260; frame++) {
      ProbeRunBounded(0x8b93ef);
      memset(oam_ent, 0, 544); oam_next_ptr = 0;
      ProbeRunBounded(0x8b9746);
      fprintf(f, "%d,%d,%d,%d,%u,%u,%u,%u,%u,", group,param,frame,
        cinematicspr_instr_ptr[15] != 0, cinematicbg_arr7[15],cinematicspr_arr6[15],
        cinematicbg_arr8[15],cinematicspr_arr7[15],cinematicspr_whattodraw[15]);
      for (int byte = 0; byte < oam_next_ptr; byte++) fprintf(f, "%02X", ((uint8*)oam_ent)[byte]);
      fprintf(f, ",");
      for (int byte = 0; byte < 32; byte++) fprintf(f, "%02X", ((uint8*)oam_ext)[byte]);
      fprintf(f, "\n");
    }
  }
  fclose(f); return 0;
}

// Whole sprite population and phase handlers; the initial music wait is held at
// the managed no-audio fixture's fourteen calls, not claimed as an SPC comparison.
int DiagnosticCeresScene(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
  reg_M7X = 52; reg_M7Y = 48; cinematic_var8 = -44; cinematic_var10 = -112;
  cinematic_var6 = 256; reg_INIDISP = 0x80;
  screen_fade_delay = screen_fade_counter = 1; cinematic_function = 0xc2e4;
  ProbeRunBoundedRegisters(0x8b938a, 0, 0, 0xce7f);
  *(uint16*)(g_ram + 0x12) = 2; ProbeRunBoundedRegisters(0x8b93a2, 0, 0, 0xce8b);
  *(uint16*)(g_ram + 0x12) = 0; ProbeRunBoundedRegisters(0x8b93a2, 0, 0, 0xce91);
  ProbeRunBoundedRegisters(0x8b938a, 0, 0, 0xcf33);
  fprintf(f, "frame,function,zoom,bgx,bgy,brightness,oam,high\n");
  for (int frame = 0; frame < 600; frame++) {
    if (frame == 13) cinematic_function = 0xc2f1;
    else if (frame >= 14) ProbeRunBounded(0x8b0000 | cinematic_function);
    ProbeRunBounded(0x8b93ef);
    memset(oam_ent, 0, 544); oam_next_ptr = 0;
    ProbeRunBounded(0x8b9746);
    fprintf(f, "%d,%u,%u,%u,%u,%u,", frame,cinematic_function,cinematic_var6,
      cinematic_var8,cinematic_var10,reg_INIDISP & 15);
    for (int byte = 0; byte < oam_next_ptr; byte++) fprintf(f, "%02X", ((uint8*)oam_ent)[byte]);
    fprintf(f, ",");
    for (int byte = 0; byte < 32; byte++) fprintf(f, "%02X", ((uint8*)oam_ext)[byte]);
    fprintf(f, "\n");
  }
  fclose(f); return 0;
}
