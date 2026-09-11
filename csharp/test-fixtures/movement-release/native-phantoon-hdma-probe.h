#include "native-bounded-cpu.h"

// Palette fade wrappers include their even-NMI gate and completion latch. Keep this
// separate from wave sampling: ordinary reappearances/fades do not spawn wave HDMA.
static int DiagnosticPhantoonFadeCsv(const char *output) {
  char path[1024];
  if (snprintf(path, sizeof(path), "%s.fade.csv", output) >= sizeof(path)) return 5;
  FILE *f = fopen(path, "wx"); if (!f) return 4;
  const int health_values[] = {1, 312, 313, 2496, 2500};
  const int denominators[] = {1, 12};
  fprintf(f, "fadeIn,denominator,health,frame,numerator,complete,color,value\n");
  for (int fade_in = 0; fade_in <= 1; fade_in++)
  for (int d = 0; d < 2; d++) for (int h = 0; h < 5; h++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    enemy_data[0].health = health_values[h];
    int band = (health_values[h] - 1) / 312; if (band > 7) band = 7;
    uint32 palette_address = 0xa7cb41 + band * 32;
    if (!fade_in) memcpy(&palette_buffer[112], RomFixedPtr(palette_address), 32);
    for (int frame = 0; frame < 40; frame++) {
      nmi_frame_counter_word = frame;
      ProbeRunBoundedRegisters(fade_in ? 0xa7d486 : 0xa7d464, denominators[d], 0, 0);
      for (int color = 0; color < 16; color++)
        fprintf(f, "%d,%d,%d,%d,%d,%d,%d,%d\n", fade_in, denominators[d],
          health_values[h], frame, enemy_data[1].ai_var_E, enemy_data[1].ai_preinstr,
          color, palette_buffer[112 + color]);
    }
  }
  fclose(f); return 0;
}

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
  fclose(f);
  char lifecycle_path[1024];
  if (snprintf(lifecycle_path, sizeof(lifecycle_path), "%s.lifecycle.csv", output) >= sizeof(lifecycle_path)) return 5;
  f = fopen(lifecycle_path, "wx"); if (!f) return 4;
  cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
  Get_Phantoon(0)->phant_var_E = 1;
  // Execute the actual materialization transition, including SpawnHdmaObject.
  ProbeRunBoundedRegisters(0xa7d508, 0, 0, 0);
  fprintf(f, "frame,mode,pending,amplitude,phase,timer,list,channel,scroll0\n");
  for (int frame = -1; frame < 8; frame++) {
    // Controlled disable after several live updates exercises the sleep/delete tail.
    if (frame == 5) enemy_data[1].parameter_1 = 0;
    if (frame >= 0 && hdma_object_channels_bitmask[0]) {
      hdma_object_index = 0;
      ProbeRunBoundedRegisters(0x88851c, 0, 0, 0);
    }
    fprintf(f, "%d,%d,%d,%d,%d,%d,%d,%d,%d\n", frame,
      enemy_data[1].parameter_1, enemy_data[2].parameter_1,
      enemy_data[3].ai_var_D, hdma_object_A[0], hdma_object_instruction_timers[0],
      hdma_object_instruction_list_pointers[0], hdma_object_channels_bitmask[0],
      g_ram[0x9100] | g_ram[0x9101] << 8);
  }
  fclose(f); return DiagnosticPhantoonFadeCsv(output);
}
