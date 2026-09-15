#pragma once

#include "snes/cart.h"
#include "snes/dma.h"
#include "ida_types.h"

#ifdef _WIN32
__declspec(dllimport) unsigned int __stdcall SetErrorMode(unsigned int mode);
enum { kProbeNoCriticalErrorDialog = 1, kProbeNoFaultDialog = 2 };
#endif

extern bool g_calling_asm_from_c;

static uint16 spacetime_callback_y[8];
static uint16 spacetime_callback_x[8];
static uint32 spacetime_callback_dp[8];
static int spacetime_callback_count;

static int SpacetimeProbeLoadRetailRom(const char *rom) {
#ifdef _WIN32
  SetErrorMode(kProbeNoCriticalErrorDialog | kProbeNoFaultDialog);
#endif
  if (!SnesInit(rom))
    return 2;
  size_t rom_length = 0;
  uint8 *retail = ReadWholeFile(rom, &rom_length);
  size_t header = rom_length & 0x7fff;
  if (!retail || (header != 0 && header != 512) ||
      rom_length - header > g_snes->cart->romSize) {
    free(retail);
    fprintf(stderr, "Cartridge size mismatch in Spacetime probe.\n");
    return 3;
  }
  memcpy(g_snes->cart->rom, retail + header, rom_length - header);
  free(retail);
  return 0;
}
static void SpacetimeProbeRun(uint32 address) {
  Cpu *cpu = g_snes->cpu;
  uint16 old_sp = cpu->sp;
  uint16 old_pc = cpu->pc;
  uint16 old_dp = cpu->dp;
  uint8 old_db = cpu->db;
  g_ram[0x1ffff] = 1;
  cpu->db = cpu->k = address >> 16;
  cpu->pc = address;
  cpu->a = cpu->x = cpu->y = 0;
  cpu->mf = cpu->xf = false;
  cpu->spBreakpoint = cpu->sp;
  g_calling_asm_from_c = true;
  for (int budget = 10000000; g_calling_asm_from_c; budget--) {
    if (!budget) {
      fprintf(stderr, "Spacetime probe instruction budget at %02X:%04X.\n",
              cpu->k, cpu->pc);
      exit(6);
    }
    if ((((uint32)cpu->k << 16) | cpu->pc) == 0x90ad16 &&
        spacetime_callback_count < 8) {
      int callback = spacetime_callback_count++;
      spacetime_callback_y[callback] = cpu->y;
      spacetime_callback_x[callback] = cpu->x;
      spacetime_callback_dp[callback] =
          g_ram[0] | g_ram[1] << 8 | g_ram[2] << 16;
    }
    cpu_runOpcode(cpu);
    while (g_snes->dma->dmaBusy)
      dma_doDma(g_snes->dma);
  }
  cpu->sp = old_sp;
  cpu->pc = old_pc;
  cpu->dp = old_dp;
  cpu->db = old_db;
}

static uint32 SpacetimeProbeHash(const uint8 *data, size_t size) {
  uint32 hash = 2166136261u;
  for (size_t i = 0; i < size; i++) {
    hash ^= data[i];
    hash *= 16777619u;
  }
  return hash;
}

int DiagnosticSpacetime(const char *rom, const char *output) {
  int status = SpacetimeProbeLoadRetailRom(rom);
  if (status)
    return status;
  FILE *f = fopen(output, "wx");
  if (!f)
    return 4;

  cpu_reset(g_snes->cpu);
  memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false;
  g_snes->cpu->sp = 0x1ff0;
  g_snes->cpu->dp = 0;

  // Pit Room ($8F:975C), the Crateria/Old-Tourian room used by the public
  // demonstration. Load the native room header, selected state and level data.
  room_ptr = 0x975c;
  SpacetimeProbeRun(0x82de6f);
  SpacetimeProbeRun(0x82def2);
  SpacetimeProbeRun(0x82ea73);

  interactive_enemy_indexes[0] = 0xffff;
  fx_y_pos = lava_acid_y_pos = 0xffff;
  samus_health = samus_max_health = 999;
  samus_x_pos = samus_prev_x_pos = 384;
  samus_y_pos = samus_prev_y_pos = 139;
  samus_x_radius = 5;
  samus_y_radius = 21;
  samus_pose = samus_prev_pose = 1;
  samus_pose_x_dir = samus_prev_pose_x_dir = 8;
  samus_movement_type = samus_prev_movement_type = 0;
  samus_x_speed_table_pointer = 0x9f55;
  samus_input_handler = 0xe913;
  samus_movement_handler = 0xa337;
  grapple_beam_function = 0xc4f0;
  equipped_beams = 0x100e;
  collected_beams = 0x100f;
  button_config_shoot_x = 0x40;
  button_config_run_b = 0x8000;
  button_config_jump_a = 0x80;
  button_config_up = 0x800;
  button_config_down = 0x400;
  button_config_left = 0x200;
  button_config_right = 0x100;
  button_config_aim_up_R = 0x10;
  button_config_aim_down_L = 0x20;
  button_config_itemcancel_y = 0x4000;
  button_config_itemswitch = 0x2000;
  game_state = 8;

  // Make every persistent progression allocation observably nonzero. The beam's
  // unintended WRAM copy, not a test-side reset, must change these bytes.
  memset(g_ram + 0xd820, 0xa5, 0xf4);
  uint8 baseline[0xf4];
  memcpy(baseline, g_ram + 0xd820, sizeof(baseline));

  fprintf(f, "room=%04X width=%u height=%u scrolls=%u/%u samus=%u/%u\n",
          room_ptr, room_width_in_blocks, room_height_in_blocks,
          room_width_in_scrolls, room_height_in_scrolls, samus_x_pos, samus_y_pos);
  fprintf(f, "frame,input,pre,list,type,damage,count,cooldown,exitY,callbackCount,callbackX0,callbackY0,callbackDp0,callbackX1,callbackY1,callbackDp1,progressHash,changed,min,max,loadingState\n");

  for (int frame = 0; frame < 24; frame++) {
    uint16 input = frame == 0 ? 0x40 : 0;
    joypad1_lastkeys = input;
    joypad1_newkeys = input;
    samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
    samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
    spacetime_callback_count = 0;
    SpacetimeProbeRun(0x90e695);

    int changed = 0;
    int minimum = -1;
    int maximum = -1;
    for (int i = 0; i < (int)sizeof(baseline); i++) {
      if (g_ram[0xd820 + i] == baseline[i])
        continue;
      changed++;
      if (minimum < 0)
        minimum = 0xd820 + i;
      maximum = 0xd820 + i;
    }
    fprintf(f, "%d,%04X,%04X,%04X,%04X,%04X,%u,%04X,%04X,%d,%04X,%04X,%06X,%04X,%04X,%06X,%08X,%d,%04X,%04X,%04X\n",
            frame, input, projectile_bomb_pre_instructions[0],
            projectile_bomb_instruction_ptr[0], projectile_type[0],
            projectile_damage[0], projectile_counter, cooldown_timer,
            g_snes->cpu->y, spacetime_callback_count,
            spacetime_callback_count > 0 ? spacetime_callback_x[0] : 0xffff,
            spacetime_callback_count > 0 ? spacetime_callback_y[0] : 0xffff,
            spacetime_callback_count > 0 ? spacetime_callback_dp[0] : 0xffffff,
            spacetime_callback_count > 1 ? spacetime_callback_x[1] : 0xffff,
            spacetime_callback_count > 1 ? spacetime_callback_y[1] : 0xffff,
            spacetime_callback_count > 1 ? spacetime_callback_dp[1] : 0xffffff,
            SpacetimeProbeHash(g_ram + 0xd820, sizeof(baseline)), changed,
            minimum < 0 ? 0xffff : minimum, maximum < 0 ? 0xffff : maximum,
            g_ram[0xd914] | g_ram[0xd915] << 8);
  }

  fprintf(f, "progress=");
  for (int i = 0; i < (int)sizeof(baseline); i++)
    fprintf(f, "%02X", g_ram[0xd820 + i]);
  fprintf(f, "\n");
  fclose(f);
  return 0;
}
