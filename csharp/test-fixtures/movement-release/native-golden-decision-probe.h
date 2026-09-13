// #617: original-CPU health/stun decisions, including RNG consumption and link writes.
#include "native-bounded-cpu.h"
int DiagnosticGoldenLifecycle(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  const uint16 kinds[]={0xad7a,0xaeb6,0xb1c0,0xb428};
  fprintf(f,"kind,facing,seed,frame,id,x,y,vx,vy,list,timer,map,damageEnabled\n");
  for(int k=0;k<4;k++) for(int facing=0;facing<2;facing++) for(int seed=0;seed<2;seed++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_ptr=0xb283;
    ProbeRunBounded(0x82de6f); ProbeRunBounded(0x82def2); ProbeRunBounded(0x82ea73);
    Enemy_Torizo *enemy=Get_Torizo(0);
    enemy->base.x_pos=256; enemy->base.y_pos=384;
    enemy->toriz_parameter_1=facing?0x8000:0; random_number=seed?4660:0;
    layer1_x_pos=layer1_y_pos=256; samus_x_pos=samus_y_pos=4096;
    eproj_enable_flag=0x8000;
    ProbeRunBoundedRegisters(0x868097,0,0,kinds[k]);
    for(int frame=0;frame<512;frame++) {
      ProbeRunBounded(0x868104);
      fprintf(f,"%u,%u,%u,%d",kinds[k],enemy->toriz_parameter_1,seed?4660:0,frame);
      if(!eproj_id[17]) fprintf(f,",0,0,0,0,0,0,0,0,0\n");
      else fprintf(f,",%u,%u,%u,%u,%u,%u,%u,%u,%u\n",eproj_id[17],eproj_x_pos[17],eproj_y_pos[17],
        eproj_x_vel[17],eproj_y_vel[17],eproj_instr_list_ptr[17],eproj_instr_timers[17],
        eproj_spritemap_ptr[17],(eproj_properties[17]&0x2000)==0);
      if(!eproj_id[17]) break;
    }
  }
  fclose(f); return 0;
}
int DiagnosticGoldenFlight(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  const uint16 seeds[]={0,31,256,4660};
  fprintf(f,"kind,facing,seed,frame,x,y,subX,subY,vx,vy,list,timer\n");
  for(int kind=0;kind<2;kind++) for(int facing=0;facing<2;facing++) for(int s=0;s<4;s++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_ptr=0xb283;
    ProbeRunBounded(0x82de6f); ProbeRunBounded(0x82def2); ProbeRunBounded(0x82ea73);
    Enemy_Torizo *enemy=Get_Torizo(0);
    enemy->base.x_pos=256; enemy->base.y_pos=384;
    enemy->toriz_parameter_1=facing?0x8000:0; random_number=seeds[s];
    eproj_radius[0]=0x0404; eproj_instr_timers[0]=1;
    ProbeRunBoundedRegisters(kind?0x86b328:0x86ac7c,0,0,0);
    uint16 initial=eproj_instr_list_ptr[0];
    for(int frame=0;frame<120;frame++) {
      ProbeRunBoundedRegisters(kind?0x86b38a:0x86acfa,0,0,0);
      fprintf(f,"%d,%u,%u,%d,%u,%u,%u,%u,%u,%u,%u,%u\n",kind,enemy->toriz_parameter_1,seeds[s],frame,
        eproj_x_pos[0],eproj_y_pos[0],eproj_x_subpos[0],eproj_y_subpos[0],
        eproj_x_vel[0],eproj_y_vel[0],eproj_instr_list_ptr[0],eproj_instr_timers[0]);
      if(eproj_instr_list_ptr[0]!=initial) break;
    }
  }
  fclose(f); return 0;
}
int DiagnosticGoldenAmmoDecisions(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  const uint16 ammo[]={0,31,32,33};
  const uint16 positions[]={0,1,2,15,16,255,256,511};
  fprintf(f,"ammo,x,frame,cursor,link,random\n");
  for(int a=0;a<4;a++) for(int x=0;x<8;x++) for(int frame=0;frame<32;frame++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    Enemy_Torizo *enemy=Get_Torizo(0);
    enemy->toriz_var_00=0x1234; random_number=0x5678;
    samus_missiles=ammo[a]; samus_x_pos=positions[x]; nmi_frame_counter_word=frame;
    ProbeRunBoundedRegisters(0xaad526,0,0,0xd000);
    fprintf(f,"%u,%u,%d,%u,%u,%u\n",ammo[a],positions[x],frame,
      g_snes->cpu->y,enemy->toriz_var_00,random_number);
  }
  fclose(f); return 0;
}
int DiagnosticGoldenSuperAim(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  const int offsets[]={-300,-64,0,64,300};
  fprintf(f,"routine,samusX,samusY,vx,vy\n");
  for(int left=0;left<2;left++) for(int x=0;x<5;x++) for(int y=0;y<5;y++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    eproj_x_pos[0]=512; eproj_y_pos[0]=512;
    samus_x_pos=512+offsets[x]; samus_y_pos=512+offsets[y];
    uint32 routine=left?0x86b272:0x86b269;
    ProbeRunBoundedRegisters(routine,0,0,0);
    fprintf(f,"%04X,%u,%u,%u,%u\n",routine&0xffff,samus_x_pos,samus_y_pos,eproj_x_vel[0],eproj_y_vel[0]);
  }
  fclose(f); return 0;
}
int DiagnosticGoldenProjectileInitializers(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  const uint32 routines[]={0x86ac7c,0x86ae15,0x86b001,0x86b1ce,0x86b328};
  fprintf(f,"routine,facing,seed,x,y,vx,vy,list,random\n");
  for(int r=0;r<5;r++) for(int facing=0;facing<2;facing++) for(int seed=0;seed<256;seed++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    Enemy_Torizo *enemy=Get_Torizo(0);
    enemy->base.x_pos=256; enemy->base.y_pos=384;
    enemy->toriz_parameter_1=facing?0x8000:0; random_number=seed;
    ProbeRunBoundedRegisters(routines[r],0,0,0);
    fprintf(f,"%04X,%u,%u,%u,%u,%u,%u,%u,%u\n",routines[r]&0xffff,
      enemy->toriz_parameter_1,seed,eproj_x_pos[0],eproj_y_pos[0],
      eproj_x_vel[0],eproj_y_vel[0],eproj_instr_list_ptr[0],random_number);
  }
  fclose(f); return 0;
}
int DiagnosticGoldenJumpDecisions(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output,"wx"); if (!f) return 4;
  const uint16 distances[]={31,32,111,112}, frames[]={359,360,361}, seeds[]={0,0x31};
  const uint32 routines[]={0xaad4ba,0xaad4fd};
  fprintf(f,"routine,samusX,facing,frames,counter,input,seed,cursor,random,afterCounter,vx,vy,gravity,timer\n");
  for(int r=0;r<2;r++) for(int d=0;d<4;d++) for(int side=0;side<2;side++)
  for(int facing=0;facing<2;facing++) for(int t=0;t<3;t++) for(int counter=7;counter<=8;counter++)
  for(int input=0;input<2;input++) for(int s=0;s<2;s++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    Enemy_Torizo *enemy=Get_Torizo(0);
    enemy->base.x_pos=256; enemy->toriz_parameter_1=facing?0x8000:0;
    enemy->toriz_var_07=frames[t]; enemy->toriz_var_09=counter;
    enemy->toriz_var_A=17; enemy->toriz_var_B=19; enemy->toriz_var_C=23;
    enemy->base.instruction_timer=29;
    samus_x_pos=256+(side?distances[d]:-distances[d]);
    joypad1_lastkeys=input?0x100:0; random_number=seeds[s];
    ProbeRunBoundedRegisters(routines[r],0,0,0xd000);
    fprintf(f,"%04X,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u,%u\n",routines[r]&0xffff,
      samus_x_pos,enemy->toriz_parameter_1,frames[t],counter,joypad1_lastkeys,seeds[s],g_snes->cpu->y,
      random_number,enemy->toriz_var_09,enemy->toriz_var_A,enemy->toriz_var_B,enemy->toriz_var_C,
      enemy->base.instruction_timer);
  }
  fclose(f); return 0;
}
int DiagnosticGoldenDistanceDecisions(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output,"wx"); if (!f) return 4;
  const uint16 distances[]={3,4,31,32,39,40,95,96};
  const uint16 poses[]={1,0x1d,0x79,0x7c};
  const uint16 seeds[]={0,0x31,0x100,0x1234};
  const uint32 routines[]={0xaad3ea,0xaad445};
  fprintf(f,"routine,samusX,facing,pose,seed,cursor,random,link,counter\n");
  for(int r=0;r<2;r++) for(int d=0;d<8;d++) for(int side=0;side<2;side++)
  for(int facing=0;facing<2;facing++) for(int p=0;p<4;p++) for(int s=0;s<4;s++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    Enemy_Torizo *enemy=Get_Torizo(0);
    enemy->base.x_pos=256; enemy->toriz_parameter_1=facing?0x8000:0;
    enemy->toriz_var_00=0x1234; enemy->toriz_var_09=9;
    samus_x_pos=256+(side?distances[d]:-distances[d]); samus_pose=poses[p];
    random_number=seeds[s];
    ProbeRunBoundedRegisters(routines[r],0,0,0xd000);
    fprintf(f,"%04X,%u,%u,%u,%u,%u,%u,%u,%u\n",routines[r]&0xffff,
      samus_x_pos,enemy->toriz_parameter_1,poses[p],seeds[s],g_snes->cpu->y,
      random_number,enemy->toriz_var_00,enemy->toriz_var_09);
  }
  fclose(f); return 0;
}
int DiagnosticGoldenDecisions(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  const uint16 healths[] = {1927,1928,1929,10799,10800,10801};
  const uint16 seeds[] = {0,1,2,3,0x31,0x61,0xff,0x100,0x1234,0xffff};
  const uint32 routines[] = {0xaad474,0xaad49b};
  fprintf(f,"routine,health,flags,seed,cursor,random,link,counter\n");
  for (int r=0;r<2;r++) for(int h=0;h<6;h++)
  for(int flag=0;flag<2;flag++) for(int s=0;s<10;s++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    Enemy_Torizo *enemy = Get_Torizo(0);
    enemy->base.health=healths[h]; enemy->toriz_parameter_2=flag ? 0x2000 : 0;
    enemy->toriz_var_00=0x1234; enemy->toriz_var_09=9; random_number=seeds[s];
    ProbeRunBoundedRegisters(routines[r],0,0,0xd000);
    fprintf(f,"%04X,%u,%u,%u,%u,%u,%u,%u\n",routines[r]&0xffff,healths[h],
      flag?0x2000:0,seeds[s],g_snes->cpu->y,random_number,enemy->toriz_var_00,enemy->toriz_var_09);
  }
  fclose(f); return 0;
}
