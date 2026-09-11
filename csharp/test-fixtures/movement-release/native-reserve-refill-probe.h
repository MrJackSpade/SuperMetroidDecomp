#include "native-bounded-cpu.h"

// Original 65816 routines, with fresh constructed state for each refill. No ROM
// or captured player state is embedded in this source. CSV output stays local.
int DiagnosticReserveRefill(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  char sprite_path[1024];
  if (snprintf(sprite_path, sizeof(sprite_path), "%s.oam.csv", output) >= sizeof(sprite_path)) { fclose(f); return 5; }
  FILE *sprites = fopen(sprite_path, "wx"); if (!sprites) { fclose(f); return 4; }
  fprintf(f, "manual,supply,frame,health,reserve,delay,hudTens,hudOnes,auto0,auto1,auto2,auto3,auto4,auto5\n");
  const int supplies[] = {2, 21, 99, 199};
  for (int manual = 0; manual <= 1; manual++) for (int s = 0; s < 4; s++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    samus_health = manual ? 20 : 0; samus_max_health = 99;
    samus_reserve_health = supplies[s]; samus_max_reserve_health = 200;
    reserve_health_mode = manual ? 2 : 1; time_is_frozen_flag = 1;
    pausemenu_equipment_category_item = 0x100;
    for (int frame = 0; frame < 220; frame++) {
      nmi_frame_counter_word = frame; nmi_frame_counter_byte = frame;
      joypad1_newkeys = frame == 0 ? 0x80 : 0;
      ProbeRunBoundedRegisters(manual ? 0x82af4f : 0x82dc31, 0, 0, 0);
      ProbeRunBoundedRegisters(0x809b44, 0, 0, 0);
      fprintf(f, "%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d,%d\n",
        manual, supplies[s], frame, samus_health, samus_reserve_health,
        pausemenu_reserve_tank_delay_ctr, hud_tilemap[70], hud_tilemap[71],
        hud_tilemap[8], hud_tilemap[9], hud_tilemap[40], hud_tilemap[41], hud_tilemap[72], hud_tilemap[73]);
      // These queues normally drain at NMI; keep this bounded routine experiment
      // from filling the WRAM queue while retaining the actual native HUD writer.
      vram_write_queue_tail = 0;
      if (manual) {
        oam_next_ptr = 0; memset(g_ram + 0x370, 0, 0x220);
        ProbeRunBoundedRegisters(0x82b2aa, 0, 0, 0);
        fprintf(sprites, "%d,%d,%d", supplies[s], frame, oam_next_ptr / 4);
        for (int b = 0; b < 0x220; b++) fprintf(sprites, ",%d", g_ram[0x370 + b]);
        fprintf(sprites, "\n");
      }
      if (!samus_reserve_health) break;
    }
  }
  fclose(sprites); fclose(f); return 0;
}
