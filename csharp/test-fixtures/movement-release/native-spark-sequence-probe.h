#include "native-bounded-cpu.h"

// Ordered real projectile/instruction updates followed by the real crash handler.
// Launch/contact boundaries are seeded; no claim of a controller-driven route.
int DiagnosticSparkSequence(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  const int beams[]={0,1,2,4,8}, ages[]={0,580,640};
  fprintf(f,"beam,left,age,history,spark,frame,phase,count,index,x3,y3,draw3,x4,y4,draw4,types\n");
  for(int beam=0;beam<5;beam++) for(int left=0;left<2;left++) for(int age=0;age<3;age++) for(int history=0;history<2;history++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    samus_x_pos=samus_y_pos=128; samus_pose=left?0xca:0xc9; samus_pose_x_dir=left?4:8;
    equipped_beams=0x1000|beams[beam]; samus_power_bombs=2; hud_item_index=3;
    if(history) ProbeRunBounded(0x90d40d);
    if(beams[beam]) ProbeRunBounded(0x90ccc0);
    for(int i=0;i<ages[age];i++) ProbeRunBounded(0x90aece);
    for(int spark=0;spark<3;spark++) {
      samus_pose=left?0xca:0xc9; samus_health=29;
      ProbeRunBounded(0x90d2ba);
      for(int frame=0;frame<100;frame++) {
        ProbeRunBounded(0x90aece);
        int finish=samus_movement_handler==0xd40d;
        ProbeRunBounded(0x900000|samus_movement_handler);
        fprintf(f,"%d,%d,%d,%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,",
          beams[beam],left,ages[age],history,spark,frame,finish?0:samus_movement_handler,
          projectile_counter,speed_echoes_index,speed_echo_xpos[2],speed_echo_ypos[2],
          speed_echo_xspeed[2],speed_echo_xpos[3],speed_echo_ypos[3],speed_echo_xspeed[3]);
        for(int slot=0;slot<5;slot++) fprintf(f,"%04X",projectile_type[slot]);
        fprintf(f,"\n");
        if(finish) break;
        if(frame==99) { fclose(f); return 7; }
      }
      for(int frame=0;frame<20;frame++) ProbeRunBounded(0x90aece);
    }
  }
  fclose(f); return 0;
}
