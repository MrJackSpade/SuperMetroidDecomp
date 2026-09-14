/* #425: controller-earned Slopekiller on original cartridge instructions. */
#include "../Common/CartridgeCpuFixture.h"
#include "../Common/MovementEntryPoints.h"
int main(int argc, char **argv) {
  if (argc != 2) { fprintf(stderr, "Usage: audit unheadered-retail-ROM > trace.csv\n"); return 2; }
  FILE *file = fopen(argv[1], "rb");
  if (!file || fread(rom, 1, sizeof(rom), file) != sizeof(rom) || fgetc(file) != EOF)
    Die("Expected unheadered 3 MiB ROM");
  fclose(file);
  printf("left,height,up,phase,frame,input,x,y,pose,movement,anim,timer,base,extra,accel,yspeed,ydir,incomingYSpeed\n");
  for (int left = 0; left < 2; left++)
  for (int height = 201; height <= 203; height++)
  for (int up = 19; up <= 21; up++)
  for (int phase = 0; phase <= 1; phase++) {
    memset(g_ram, 0, sizeof(g_ram));
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144 * 80 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    /* Flat landing followed by continuous 45-degree descent. Both direction
       cases include adjacent X pixels; do not assume native edge symmetry. */
    for (int x = 0; x < 144; x++) {
      int distance = left ? 59 - x : x - 68;
      int floor = 16 + (distance >= 0 ? distance : 0);
      for (int y = floor; y < 80; y++) {
        level_data[y * 144 + x] = distance >= 0 && y == floor ? 0x1000 : 0x8000;
        BTS[y * 144 + x] = distance >= 0 && y == floor ? (left ? 0x12 : 0x52) : 0;
      }
    }
    fx_y_pos = lava_acid_y_pos = 0xffff;
    equipped_items = collected_items = 4; game_state = 8;
    samus_health = samus_max_health = 99; enable_horiz_slope_coll = 3;
    samus_x_pos = samus_prev_x_pos = 1024 + (left ? -phase : phase);
    samus_y_pos = samus_prev_y_pos = height;
    samus_pose = samus_prev_pose = left ? 0x32 : 0x31;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_movement_type = samus_prev_movement_type = 8; samus_y_dir = 2;
    run(RefreshRadius);
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    uint16 previous = 0;
    for (int frame = 0; frame < 150; frame++) {
      nmi_frame_counter_word = nmi_frame_counter_byte = frame + 2;
      uint16 input = frame == up ? 0x800 : 0;
      if (frame >= 60) input |= left ? 0x200 : 0x100;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      run(InputPhase); run(InteractionPhase); samus_contact_damage_index = 0;
      uint32 incoming = (uint32)samus_y_speed << 16 | samus_y_subspeed;
      run(0x900000 | samus_movement_handler);
      unsigned stages[] = {AnimationPhase,TransitionPhase,CollisionPosePhase,ApplyPosePhase,
        PoseHistoryPhase,HurtPhase,CollisionPhase};
      for (int i = 0; i < 7; i++) run(stages[i]);
      printf("%d,%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%02X,%04X,%04X,%04X%04X,%04X%04X,%04X,%04X%04X,%04X,%08X\n",
        left,height,up,phase,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_movement_type,samus_anim_frame,samus_anim_frame_timer,
        samus_x_base_speed,samus_x_base_subspeed,samus_x_extra_run_speed,samus_x_extra_run_subspeed,
        samus_x_accel_mode,samus_y_speed,samus_y_subspeed,samus_y_dir,incoming);
    }
  }
  return 0;
}
