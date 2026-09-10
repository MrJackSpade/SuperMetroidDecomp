#include "native-bounded-cpu.h"

// #454: bank-$94 contact publication followed by native elevator activation.
int DiagnosticElevatorGrab(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f,"up,parity,left,kind,x,input,contact,collision,probeY,status,pose,finalX,finalY\n");
  for(int up=0;up<2;up++) for(int parity=0;parity<2;parity++)
  for(int left=0;left<2;left++) for(int kind=0;kind<4;kind++)
  for(int x=120;x<152;x++) for(int input=0;input<3;input++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_width_in_blocks=16; room_height_in_blocks=32; room_size_in_blocks=1024;
    door_list_pointer=0x9b00; nmi_frame_counter_word=parity;
    for(int column=0;column<16;column++) level_data[256+column]=0x8000;
    level_data[264]=0x9000; BTS[264]=9;
    samus_pose=kind==0 ? (left?2:1) : kind==1 ? (left?10:9) : kind==2 ? (left?0x28:0x27) : (left?0x1a:0x19);
    samus_pose_x_dir=left?4:8; samus_prev_pose=samus_pose;
    samus_x_radius=5; samus_y_radius=RomPtr(0x91b629 + samus_pose * 8)[6];
    samus_x_pos=x; samus_y_pos=256-samus_y_radius; samus_health=samus_max_health=99;
    interactive_enemy_indexes[0]=0xffff;
    Enemy_Elevator *e=Get_Elevator(0); e->base.x_pos=136; e->base.y_pos=256;
    e->elevat_parameter_1=up; ProbeRunBounded(0xa394e6);
    *(uint16 *)(g_ram+0x12)=1; *(uint16 *)(g_ram+0x14)=0;
    ProbeRunBounded(0x949763);
    uint16 contact=elevator_flags, collision=samus_collision_flag, probe_y=samus_y_pos;
    joypad1_newkeys=input==0?0:input==1?(up?0x800:0x400):(up?0x400:0x800);
    ProbeRunBounded(0xa3952a);
    fprintf(f,"%d,%d,%d,%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
      up,parity,left,kind,x,input,contact,collision,probe_y,elevator_status,samus_pose,samus_x_pos,samus_y_pos);
  }
  fclose(f); return 0;
}
