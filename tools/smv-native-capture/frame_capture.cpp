// Cartridge frame capture: plays an unmodified SMV in Snes9x 1.60 and writes the pictures the console showed on the
// requested source frames as PNGs, for comparing the port's drawing with the cartridge's. Built beside the update
// capture from the same instrumented Snes9x checkout (see README.md); the instrumentation's observers do nothing here.
//
// Usage: frame_capture ROM MOVIE OUTPUT_DIRECTORY FRAMES
// FRAMES is a comma-separated list of source frames or ranges, a range optionally stepped: 1200,1300-1400,2000-3000/50.
// Beside each picture it writes frame-N.cgram, the console's 256 colour words as they stood after the frame, little-endian.
// and frame-N.apu, the four bytes the CPU last wrote to the sound processor's ports ($2140-$2143) by the frame's end,
// which hold the sound requests of each sound library the frame sent.
// and frame-N.aram, the sound processor's 64 KiB of RAM after the frame: its driver, samples, instruments and sounds.
// An optional audio range FIRST-LAST writes audio.wav, the console's stereo output while those frames play.
#include <windows.h>
#include <crtdbg.h>
#include <exception>
#include <stdexcept>
#include <filesystem>
#include <fstream>
#include <vector>
#include <set>
#include <string>
#include <sstream>
#include "libretro.h"
#include "snes9x.h"
#include "memmap.h"
#include "movie.h"
#include "snapshot.h"
#include "ppu.h"
#include "apu/bapu/snes/snes.hpp"
#include "zlib.h"

// The instrumented Snes9x calls these; frame capture records neither boundaries nor instructions.
bool RidleyTraceActive=false;
void RidleyObserveInstruction(unsigned) {}
void RidleyObserveBoundary(unsigned) {}

static retro_pixel_format pixelFormat=RETRO_PIXEL_FORMAT_0RGB1555;
static std::vector<unsigned char> lastFrame;
static unsigned lastWidth=0, lastHeight=0;

static bool env(unsigned command, void* data) {
 if(command==RETRO_ENVIRONMENT_SET_PIXEL_FORMAT) { pixelFormat=*static_cast<const retro_pixel_format*>(data); return true; }
 return false;
}

// Keeps the shown picture as 8-bit RGB; a duplicated frame (null data) keeps the previous picture.
static void video(const void* data,unsigned width,unsigned height,size_t pitch) {
 if(!data) return;
 lastWidth=width; lastHeight=height; lastFrame.assign(size_t(width)*height*3,0);
 for(unsigned y=0;y<height;y++) {
  const unsigned char* row=static_cast<const unsigned char*>(data)+y*pitch;
  for(unsigned x=0;x<width;x++) {
   unsigned char* out=&lastFrame[(size_t(y)*width+x)*3];
   if(pixelFormat==RETRO_PIXEL_FORMAT_XRGB8888) {
    const unsigned char* p=row+x*4; out[0]=p[2]; out[1]=p[1]; out[2]=p[0];
   } else {
    unsigned v=row[x*2]|row[x*2+1]<<8;
    unsigned r,g,b;
    if(pixelFormat==RETRO_PIXEL_FORMAT_RGB565) { r=(v>>11)&31; g=(v>>5)&63; b=v&31; out[1]=(unsigned char)(g<<2|g>>4); }
    else { r=(v>>10)&31; g=(v>>5)&31; b=v&31; out[1]=(unsigned char)(g<<3|g>>2); }
    out[0]=(unsigned char)(r<<3|r>>2); out[2]=(unsigned char)(b<<3|b>>2);
   }
  }
 }
}
// The console's stereo samples while the audio range plays, written to audio.wav.
static std::vector<int16_t> recorded;
static bool recording=false;
static size_t audio(const int16_t* samples,size_t frames) {
 if(recording) recorded.insert(recorded.end(),samples,samples+frames*2);
 return frames;
}

// Writes the recorded samples as a 16-bit stereo WAV at the core's sample rate.
static void writeWav(const std::filesystem::path& path,unsigned rate) {
 std::ofstream out(path,std::ios::binary); out.exceptions(std::ios::failbit|std::ios::badbit);
 auto word=[&](uint32_t value,int bytes){ for(int i=0;i<bytes;i++) out.put((char)((value>>(8*i))&255)); };
 uint32_t data=uint32_t(recorded.size()*2);
 out.write("RIFF",4); word(36+data,4); out.write("WAVEfmt ",8); word(16,4); word(1,2); word(2,2); word(rate,4); word(rate*4,4); word(4,2); word(16,2);
 out.write("data",4); word(data,4); out.write((const char*)recorded.data(),data);
}
static void poll() {}
static int16_t input(unsigned,unsigned,unsigned,unsigned) { return 0; }

static void chunk(std::ofstream& out,const char* type,const std::vector<unsigned char>& body) {
 unsigned length=unsigned(body.size());
 unsigned char header[8]={(unsigned char)(length>>24),(unsigned char)(length>>16),(unsigned char)(length>>8),(unsigned char)length,
  (unsigned char)type[0],(unsigned char)type[1],(unsigned char)type[2],(unsigned char)type[3]};
 out.write((char*)header,8);
 if(length) out.write((const char*)body.data(),length);
 uLong crc=crc32(0,(const Bytef*)type,4);
 if(length) crc=crc32(crc,body.data(),length);
 unsigned char tail[4]={(unsigned char)(crc>>24),(unsigned char)(crc>>16),(unsigned char)(crc>>8),(unsigned char)crc};
 out.write((char*)tail,4);
}

static void writePng(const std::filesystem::path& path) {
 if(lastFrame.empty()) throw std::runtime_error("The console has shown no picture yet");
 std::vector<unsigned char> raw;
 raw.reserve(size_t(lastHeight)*(lastWidth*3+1));
 for(unsigned y=0;y<lastHeight;y++) {
  raw.push_back(0);
  raw.insert(raw.end(),lastFrame.begin()+size_t(y)*lastWidth*3,lastFrame.begin()+size_t(y+1)*lastWidth*3);
 }
 uLongf packedLength=compressBound(uLong(raw.size()));
 std::vector<unsigned char> packed(packedLength);
 if(compress2(packed.data(),&packedLength,raw.data(),uLong(raw.size()),6)!=Z_OK) throw std::runtime_error("PNG compression failed");
 packed.resize(packedLength);
 std::ofstream out(path,std::ios::binary); out.exceptions(std::ios::failbit|std::ios::badbit);
 const unsigned char signature[8]={137,80,78,71,13,10,26,10};
 out.write((const char*)signature,8);
 std::vector<unsigned char> ihdr={(unsigned char)(lastWidth>>24),(unsigned char)(lastWidth>>16),(unsigned char)(lastWidth>>8),(unsigned char)lastWidth,
  (unsigned char)(lastHeight>>24),(unsigned char)(lastHeight>>16),(unsigned char)(lastHeight>>8),(unsigned char)lastHeight,8,2,0,0,0};
 chunk(out,"IHDR",ihdr); chunk(out,"IDAT",packed); chunk(out,"IEND",{});
}

static std::set<unsigned> parseFrames(const std::string& spec) {
 std::set<unsigned> frames; std::stringstream items(spec); std::string item;
 while(std::getline(items,item,',')) {
  unsigned step=1; auto slash=item.find('/');
  if(slash!=std::string::npos) { step=std::stoul(item.substr(slash+1)); item=item.substr(0,slash); }
  if(step==0) throw std::runtime_error("A range's step must be positive");
  auto dash=item.find('-');
  if(dash==std::string::npos) { frames.insert(std::stoul(item)); continue; }
  unsigned first=std::stoul(item.substr(0,dash)), last=std::stoul(item.substr(dash+1));
  if(first>last) throw std::runtime_error("A range must not run backwards: "+item);
  for(unsigned frame=first;frame<=last;frame+=step) frames.insert(frame);
 }
 if(frames.empty()) throw std::runtime_error("No frames requested");
 return frames;
}

int main(int argc,char** argv) {
 SetErrorMode(SEM_FAILCRITICALERRORS|SEM_NOGPFAULTERRORBOX|SEM_NOOPENFILEERRORBOX);
 _set_abort_behavior(0,_WRITE_ABORT_MSG|_CALL_REPORTFAULT);
 _CrtSetReportMode(_CRT_ASSERT,_CRTDBG_MODE_FILE); _CrtSetReportFile(_CRT_ASSERT,_CRTDBG_FILE_STDERR);
 try {
  if(argc!=5 && argc!=6) throw std::runtime_error("usage: frame_capture ROM MOVIE OUTPUT_DIRECTORY FRAMES [AUDIO_FIRST-AUDIO_LAST]");
  unsigned audioFirst=1, audioLast=0;
  if(argc==6) {
   std::string range=argv[5]; auto dash=range.find('-');
   if(dash==std::string::npos) throw std::runtime_error("The audio range is FIRST-LAST");
   audioFirst=std::stoul(range.substr(0,dash)); audioLast=std::stoul(range.substr(dash+1));
   if(audioFirst>audioLast) throw std::runtime_error("The audio range must not run backwards");
  }
  std::filesystem::path output=argv[3]; std::filesystem::create_directories(output);
  std::set<unsigned> requested=parseFrames(argv[4]);
  retro_set_environment(env); retro_set_video_refresh(video); retro_set_audio_sample_batch(audio);
  retro_set_input_poll(poll); retro_set_input_state(input); retro_init();
  std::ifstream rom(argv[1],std::ios::binary); rom.exceptions(std::ios::badbit);
  if(!rom) throw std::runtime_error("ROM open failed");
  std::vector<char> bytes((std::istreambuf_iterator<char>(rom)),{});
  retro_game_info info={argv[1],bytes.data(),bytes.size(),nullptr};
  if(!retro_load_game(&info)) throw std::runtime_error("ROM load failed");
  int result=S9xMovieOpen(argv[2],true);
  if(result!=SUCCESS) throw std::runtime_error("Movie restore failed: "+std::to_string(result));
  const auto length=S9xMovieGetLength();
  unsigned last=*requested.rbegin();
  if(last>=length) throw std::runtime_error("Frame "+std::to_string(last)+" lies past the movie's "+std::to_string(length)+" frames");
  unsigned loops=0;
  // The picture a source frame shows is the one the console draws while it plays that frame's input.
  while(S9xMovieActive() && !requested.empty()) {
   unsigned frame=S9xMovieGetFrameCounter();
   recording=frame>=audioFirst && frame<=audioLast;
   retro_run();
   if(argc==6 && frame==audioLast) {
    retro_system_av_info av; retro_get_system_av_info(&av);
    writeWav(output/"audio.wav",unsigned(av.timing.sample_rate+0.5));
   }
   if(requested.erase(frame)) {
    writePng(output/("frame-"+std::to_string(frame)+".png"));
    std::ofstream colours(output/("frame-"+std::to_string(frame)+".cgram"),std::ios::binary);
    colours.exceptions(std::ios::failbit|std::ios::badbit);
    colours.write((const char*)PPU.CGDATA,sizeof(PPU.CGDATA));
    std::ofstream ports(output/("frame-"+std::to_string(frame)+".apu"),std::ios::binary);
    ports.exceptions(std::ios::failbit|std::ios::badbit);
    ports.write((const char*)SNES::cpu.registers,sizeof(SNES::cpu.registers));
    std::ofstream sound(output/("frame-"+std::to_string(frame)+".aram"),std::ios::binary);
    sound.exceptions(std::ios::failbit|std::ios::badbit);
    sound.write((const char*)SNES::smp.apuram,0x10000);
    fprintf(stderr,"Captured frame %u, room %04X\n",frame,Memory.RAM[0x79b]|Memory.RAM[0x79c]<<8);
   }
   if(++loops>length+300) throw std::runtime_error("Movie playback did not terminate");
  }
  if(!requested.empty()) throw std::runtime_error("The movie ended before frame "+std::to_string(*requested.begin()));
  retro_unload_game(); retro_deinit(); return 0;
 } catch(const std::exception& e) { fprintf(stderr,"%s\n",e.what()); return 1; }
 catch(...) { fprintf(stderr,"Unknown frame capture exception\n"); return 2; }
}
