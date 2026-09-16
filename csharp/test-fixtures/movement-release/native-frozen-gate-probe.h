#include "native-bounded-cpu.h"
#include "ida_types.h"
#include "enemy_types.h"

static int FrozenGateLoadRetailRom(const char *rom) {
  if (!SnesInit(rom)) return 2;
  size_t rom_length = 0;
  uint8 *retail = ReadWholeFile(rom, &rom_length);
  size_t header = rom_length & 0x7fff;
  if (!retail || (header != 0 && header != 512) ||
      rom_length - header > g_snes->cart->romSize) {
    free(retail);
    return 3;
  }
  memcpy(g_snes->cart->rom, retail + header, rom_length - header);
  free(retail);
  return 0;
}

// #407: original-CPU Caterpillar frozen-Zero gate-column clipping boundary.
// The actual room, closed-gate PLM and terrain are retained. Only the long setup
// which walks a retail Zero to the gate is collapsed into its final frozen body.
int DiagnosticFrozenGate(const char *rom, const char *output) {
  int status = FrozenGateLoadRetailRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "distance,enemyX,frozen,collided,finalX,gateWoke,gatePre,gateList\n");

  static const struct { uint16 distance, enemy_x, frozen; } cases[] = {
    {6, 621, 1}, // Future hitboxes are tangent: terrain wins.
    {7, 621, 1}, // One more pixel reaches the frozen enemy first and enters the gate.
    {7, 622, 1}, // Adjacent enemy placement is tangent again.
    {7, 621, 0}, // An unfrozen, non-solid Zero cannot bypass terrain.
  };
  const uint8 *state = RomFixedPtr(0x8fa32f);
  uint32 level = state[0] | state[1] << 8 | state[2] << 16;
  uint16 population = state[20] | state[21] << 8;
  for (int test = 0; test < 4; test++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0;
    room_ptr = 0xa322;
    room_width_in_blocks = 48; room_height_in_blocks = 128;
    room_width_in_scrolls = 3; room_height_in_scrolls = 8;
    room_size_in_blocks = 48 * 128 * 2; area_index = 1;
    DecompressToMem(level, g_ram + 0x10000);
    memcpy(BTS, (uint8 *)level_data + room_size_in_blocks, 48 * 128);
    layer1_x_pos = 512; layer1_y_pos = 768; plm_flag = 0x8000;
    for (uint16 entry = population; ; entry += 6) {
      uint32 address = 0x8f0000 | entry;
      if (!GET_WORD(RomFixedPtr(address))) break;
      ProbeRunBoundedRegisters(0x84846a, 0, entry, 0);
    }
    int gate = -1;
    for (int i = 0; i < 40; i++) if (plm_header_ptr[i] == 0xc82a) gate = i;
    if (gate < 0) { fclose(f); return 8; }
    ProbeRunBounded(0x8485b4); ProbeRunBounded(0x8485b4);
    if (plm_pre_instrs[gate] != 0xbb6b || plm_instr_list_ptrs[gate] != 0xbc44) {
      fclose(f); return 9;
    }

    samus_x_pos = samus_prev_x_pos = 602;
    samus_x_subpos = samus_prev_x_subpos = 0;
    samus_y_pos = samus_prev_y_pos = 860;
    samus_y_subpos = samus_prev_y_subpos = 0;
    samus_x_radius = 5; samus_y_radius = 21;
    samus_pose = samus_prev_pose = 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = 8;
    samus_movement_type = samus_prev_movement_type = 0;
    samus_x_speed_table_pointer = 0x9f55;
    samus_collision_direction = 1;

    EnemyData *enemy = gEnemyData(0);
    enemy->enemy_ptr = 0xd7bf; enemy->bank = 0xa3;
    enemy->x_pos = cases[test].enemy_x; enemy->y_pos = 860;
    enemy->x_width = enemy->y_height = 8;
    enemy->properties = 0; enemy->frozen_timer = cases[test].frozen;
    interactive_enemy_indexes_write_ptr = 2;
    interactive_enemy_indexes[0] = 0; interactive_enemy_indexes[1] = 0xffff;

    *(uint16 *)&g_ram[0x12] = cases[test].distance;
    *(uint16 *)&g_ram[0x14] = 0;
    ProbeRunBounded(0x9093b1);
    uint16 collided = samus_collision_flag;
    ProbeRunBounded(0x8485b4);
    // The wake pre-instruction advances the list to $BC46, then the same PLM
    // handler invocation consumes that zero-delay instruction and leaves $BC51.
    bool woke = plm_pre_instrs[gate] == 0xbba3;
    fprintf(f, "%u,%u,%u,%u,%u,%u,%04X,%04X\n",
      cases[test].distance, cases[test].enemy_x, cases[test].frozen,
      collided, samus_x_pos, woke, plm_pre_instrs[gate],
      plm_instr_list_ptrs[gate]);
  }
  fclose(f); return 0;
}
