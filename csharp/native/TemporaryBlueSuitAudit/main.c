/* #429: earn temporary Blue Suit through native controller processing. */
#include "../Common/CartridgeCpuFixture.h"
#include "../Common/MovementEntryPoints.h"
enum { SamusPalettePhase = 0x91d6f7 };
int main(int argc, char **argv) {
  if (argc != 2) return 2;
  FILE *file = fopen(argv[1], "rb");
  if (!file || fread(rom, 1, sizeof(rom), file) != sizeof(rom) || fgetc(file) != EOF)
    Die("Expected unheadered 3 MiB ROM");
  fclose(file);
  printf("left,stop,aim,frame,input,x,y,pose,anim,timer,base,extra,boost,contact,shine,palette,yspeed,ydir\n");
  for (int left = 0; left < 2; left++)
  for (int stop = 60; stop <= 180; stop += 40)
  for (int aim = 0; aim < 4; aim++) {
    memset(g_ram, 0, sizeof(g_ram));
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144 * 80 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 144; x++) level_data[32 * 144 + x] = 0x8000;
    fx_y_pos = lava_acid_y_pos = 0xffff;
    equipped_items = collected_items = 0x2004; game_state = 8;
    samus_health = samus_max_health = 99;
    samus_x_pos = samus_prev_x_pos = left ? 2100 : 200;
    samus_y_pos = samus_prev_y_pos = 491;
    samus_pose = samus_prev_pose = left ? 2 : 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    run(RefreshRadius);
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    uint16 previous = 0;
    for (int frame = 0; frame < 400; frame++) {
      nmi_frame_counter_word = nmi_frame_counter_byte = frame + 2;
      uint16 input = frame < stop ? 0x8000 | (left ? 0x200 : 0x100) : aim * 0x10;
      if (frame == stop) input |= 0x400;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      run(InputPhase); run(InteractionPhase); samus_contact_damage_index = 0;
      run(0x900000 | samus_movement_handler);
      unsigned stages[] = {AnimationPhase,TransitionPhase,CollisionPosePhase,ApplyPosePhase,
        PoseHistoryPhase,HurtPhase,CollisionPhase,SamusPalettePhase};
      for (int i = 0; i < 8; i++) run(stages[i]);
      printf("%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%04X,%04X,%04X%04X,%04X%04X,%04X,%04X,%04X,%04X,%04X%04X,%04X\n",
        left,stop,aim,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_anim_frame,samus_anim_frame_timer,samus_x_base_speed,samus_x_base_subspeed,
        samus_x_extra_run_speed,samus_x_extra_run_subspeed,speed_boost_counter,samus_contact_damage_index,
        samus_shine_timer,timer_for_shine_timer,samus_y_speed,samus_y_subspeed,samus_y_dir);
    }
  }
  return 0;
}
