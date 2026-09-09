// #485: isolated retail CPU bomb-jump trajectory, not a full-room parity test.
// Include after native-release-probe.h in sm_rtl.c and dispatch before SDL starts.
#include "ida_types.h"
#include "enemy_types.h"
int DiagnosticMetroidBombJump(const char *rom) {
  int status = ProbeLoadRetailMovementRom(rom);
  if (status) return status;
  cpu_reset(g_snes->cpu);
  memset(g_ram, 0, sizeof(g_ram));
  g_snes->cpu->e = false;
  g_snes->cpu->sp = 0x1ff0;
  g_snes->cpu->dp = 0;
  room_width_in_blocks = 16;
  room_height_in_blocks = 16;
  interactive_enemy_indexes[0] = 0xffff;
  for (int i = 192; i < 208; i++) level_data[i] = 0x8000;
  fx_y_pos = lava_acid_y_pos = 0xffff;
  samus_x_pos = 128;
  samus_y_pos = 185;
  samus_x_radius = 7;
  samus_y_radius = 7;
  samus_pose = samus_prev_pose = 0x1d;
  samus_pose_x_dir = 8;
  samus_movement_type = 4;
  samus_y_subaccel = 0x1c00;
  bomb_jump_dir = 0x0802;
  RunAsmCode(0x90e025, 0, 0, 0, 0);
  printf("BOMB START y=%04X.%04X speed=%04X.%04X\n", samus_y_pos,
    samus_y_subpos, samus_y_speed, samus_y_subspeed);
  for (int frame = 0; frame < 12; frame++) {
    RunAsmCode(0x90e032, 0, 0, 0, 0);
    printf("BOMB MOVE frame=%d y=%04X.%04X speed=%04X.%04X\n", frame,
      samus_y_pos, samus_y_subpos, samus_y_speed, samus_y_subspeed);
  }
  // Focused interaction schedule: native projectile/Samus publication, enemy bomb
  // collision, attached positioning, movement, then bomb fuse/instruction update.
  // This intentionally excludes room AI and pose transitions; print rather than
  // claim an exhaustive full-game oracle from this constrained experiment.
  samus_y_pos = 185;
  samus_y_subpos = 0;
  samus_y_speed = samus_y_subspeed = bomb_jump_dir = 0;
  Enemy_Metroid *enemy = Get_Metroid(0);
  EnemyDef *definition = get_EnemyDef_A2(0xdd7f);
  // Extended enemy structs span discontiguous WRAM through padding; clearing the
  // whole struct would erase unrelated liquid state and invalidate the experiment.
  memset(&enemy->base, 0, sizeof(enemy->base));
  enemy->base.enemy_ptr = 0xdd7f;
  enemy->base.bank = 0xa3;
  enemy->base.spritemap_pointer = 0x8000;
  enemy->base.x_width = definition->x_radius;
  enemy->base.y_height = definition->y_radius;
  enemy->base.x_pos = 128;
  enemy->base.y_pos = 177;
  enemy->metroid_var_F = 2;
  cur_enemy_index = 0;
  bomb_counter = 1;
  projectile_index = 10;
  projectile_type[5] = 0x500;
  projectile_x_pos[5] = 128;
  projectile_y_pos[5] = 185;
  projectile_variables[5] = 60;
  RunAsmCode(0x9380a0, 0, 10, 0, 0);
  // Native missile-style initialization does not set damage/radii; the bomb
  // preinstruction populates its damage word through BombOrPowerBomb_Func1.
  int jumping = 0;
  for (int frame = 0; frame < 75; frame++) {
    RunAsmCode(0xa09785, 0, 0, 0, 0);
    RunAsmCode(0xa0a236, 0, 0, 0, 0);
    if (enemy->metroid_var_F == 2) {
      enemy->base.x_pos = samus_x_pos;
      enemy->base.y_pos = samus_y_pos - 8;
    }
    if (bomb_jump_dir && !jumping) {
      bomb_jump_dir |= 0x800;
      RunAsmCode(0x90e025, 0, 0, 0, 0);
      jumping = 1;
    } else if (jumping) {
      RunAsmCode(0x90e032, 0, 0, 0, 0);
    }
    projectile_index = 10;
    RunAsmCode(0x90b099, 0, 10, 0, 0);
    RunAsmCode(0x9381e9, 0, 0, 0, 0);
    printf("INTERACTION frame=%d y=%u enemyY=%u state=%u radius=%u/%u enemyRadius=%u/%u fuse=%u damage=%u\n",
      frame, samus_y_pos, enemy->base.y_pos, enemy->metroid_var_F,
      projectile_x_radius[5], projectile_y_radius[5], enemy->base.x_width,
      enemy->base.y_height, projectile_variables[5], projectile_damage[5]);
    if (enemy->metroid_var_F == 3) break;
  }
  return 0;
}
