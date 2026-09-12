#include "native-bounded-cpu.h"

// The original game-state-eight owner runs the entire encounter. Only initial
// room/player state and physical input words are supplied by this diagnostic.
int DiagnosticPhantoonEnrage(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "fire,initialHealth,control,frame,input,frozen,health,timer,flash,map,x,y,samusX,samusY,pose,hud,samusHealth,function,samusSubX,samusSubY");
  for (int p = 0; p < 5; p++) fprintf(f, ",s%d_type,s%d_x,s%d_y,s%d_subX,s%d_subY,s%d_direction,s%d_damage",p,p,p,p,p,p,p);
  for (int p = 0; p < 18; p++) fprintf(f, ",p%d_id,p%d_x,p%d_y,p%d_subX,p%d_subY",p,p,p,p,p);
  fprintf(f, ",bodySubX,bodySubY,swoopVX,swoopVY,swoopTargetX\n");
  static const int fires[] = {1576,1577,1578};
  static const int healths[] = {500,700,2500};
  for (int test = 0; test < 9; test++) {
    int fire = fires[test % 3], initialHealth = healths[test / 3];
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_ptr = 0xcd13; random_number = 0x61;
    ProbeRunBounded(0x82de6f); ProbeRunBounded(0x82def2); ProbeRunBounded(0x82ea73);
    // The authored grey door changes projectile collision at the room's left
    // edge. Spawn it through the real room-PLM owner, not a synthetic solid tile.
    ProbeRunBoundedRegisters(0x84846a, 0, 0xc2b3, 0);
    ProbeRunBounded(0x8483ad);
    ProbeRunBounded(0xa08a1e); ProbeRunBounded(0xa08a9e);
    if (num_enemies_in_room != 4) { fclose(f); return 8; }
    fx_y_pos = lava_acid_y_pos = 0xffff;
    equipped_items = collected_items = 0x8001;
    equipped_beams = collected_beams = 0;
    gEnemyData(0)->health = initialHealth;
    game_state = 8; reg_INIDISP = 15;
    samus_health = samus_max_health = 999;
    samus_missiles = samus_max_missiles = 10;
    samus_super_missiles = samus_max_super_missiles = 10;
    samus_x_pos = samus_prev_x_pos = 128; samus_y_pos = samus_prev_y_pos = 187;
    layer1_x_pos = 32; layer1_y_pos = 21;
    samus_pose = samus_prev_pose = 1; samus_pose_x_dir = samus_prev_pose_x_dir = 8;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    frame_handler_alfa = 0xe695; frame_handler_beta = 0xe725;
    frame_handler_gamma = 0xe90e; samus_draw_handler = 0xeb52;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    hdma_objects_enable_flag = 0x8000;
    ProbeRunBounded(0x868000); ProbeRunBounded(0x89ab82);
    uint16 previous = 0; int firstHit = -1;
    for (int frame = -2; frame < 1700; frame++) {
      nmi_frame_counter_word = nmi_frame_counter_byte = frame + 3;
      uint16 input = frame == -2 ? 0x2000 : frame == -1 ? 0 : 0x800;
      if (frame >= 1460 && frame < 1480) input = 0x100;
      if (frame >= 1548 && frame < 1566) input |= 0x80;
      if (frame == 1565 || frame == fire) input |= 0x40;
      if (frame == 1566) input |= 0x2000;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      oam_next_ptr = vram_write_queue_tail = vram_read_queue_tail = 0;
      EnemyData *body = gEnemyData(0); uint16 health = body->health;
      ProbeRunBounded(0x8884b9); ProbeRunBounded(0x808111); ProbeRunBounded(0x828b44);
      fprintf(f, "%d,%d,%d,%d,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u",
        fire,initialHealth,0,frame,input,time_is_frozen_flag,body->health,body->invincibility_timer,
        body->flash_timer,body->spritemap_pointer,body->x_pos,body->y_pos,
        samus_x_pos,samus_y_pos,samus_pose,hud_item_index,samus_health,Get_Phantoon(0)->phant_var_F,
        samus_x_subpos,samus_y_subpos);
      for (int p = 0; p < 5; p++) {
        if (!projectile_type[p]) fprintf(f, ",0,0,0,0,0,0,0");
        else fprintf(f, ",%u,%u,%u,%u,%u,%u,%u",projectile_type[p],projectile_x_pos[p],projectile_y_pos[p],
          projectile_bomb_x_subpos[p],projectile_bomb_y_subpos[p],projectile_dir[p],projectile_damage[p]);
      }
      for (int p = 0; p < 18; p++) {
        if (!eproj_id[p]) fprintf(f, ",0,0,0,0,0");
        else fprintf(f, ",%u,%u,%u,%u,%u",eproj_id[p],eproj_x_pos[p],eproj_y_pos[p],eproj_x_subpos[p],eproj_y_subpos[p]);
      }
      fprintf(f, ",%u,%u,%u,%u,%u\n", body->x_subpos,body->y_subpos,
        Get_Phantoon(0x80)->phant_var_C,Get_Phantoon(0x80)->phant_var_D,Get_Phantoon(0x80)->phant_var_E);
      fflush(f);
    }
  }
  fclose(f); return 0;
}
