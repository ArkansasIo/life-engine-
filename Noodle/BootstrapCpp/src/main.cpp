#include "nudl/codegen.hpp"
#include "nudl/interpreter.hpp"
#include "nudl/lexer.hpp"
#include "nudl/parser.hpp"
#include <fstream>
#include <iostream>
#include <sstream>
#include <stdexcept>
#include <string>
using namespace nudl;
static TargetLanguage parse_target(const std::string& s) {
 if (s=="cpp"||s=="c++") return TargetLanguage::Cpp;
 if (s=="c") return TargetLanguage::C;
 if (s=="rust") return TargetLanguage::Rust;
 if (s=="python") return TargetLanguage::Python;
 if (s=="js"||s=="javascript") return TargetLanguage::JavaScript;
 if (s=="ts"||s=="typescript") return TargetLanguage::TypeScript;
 if (s=="csharp"||s=="cs") return TargetLanguage::CSharp;
 if (s=="java") return TargetLanguage::Java;
 if (s=="swift") return TargetLanguage::Swift;
 throw std::runtime_error("unknown target: "+s);
}
static int usage() {
 std::cerr << "Noodle / NUDL bootstrap\n"
           << "  noodle run <file.nudl>\n"
           << "  noodle compile <file.nudl> [--target cpp] [--emit output.cpp]\n"
           << "  noodle check <file.nudl>\n";
 return 2;
}
int main(int argc,char**argv) {
 if(argc<3) return usage();
 const std::string mode=argv[1], file=argv[2];
 std::string target_name="cpp", output;
 for(int i=3;i<argc;i++) {
  std::string arg=argv[i];
  if(arg=="--target" && i+1<argc) target_name=argv[++i];
  else if(arg=="--emit" && i+1<argc) output=argv[++i];
  else { std::cerr<<"noodle: unknown option "<<arg<<"\n"; return 2; }
 }
 std::ifstream input(file,std::ios::binary);
 if(!input) { std::cerr<<"noodle: cannot open "<<file<<"\n"; return 2; }
 std::ostringstream buffer; buffer<<input.rdbuf();
 try {
  auto tokens=lex(buffer.str()); Diagnostics diagnostics;
  Parser parser(std::move(tokens),diagnostics); auto program=parser.parse();
  if(!diagnostics.ok()) {
   for(const auto& e:diagnostics.errors()) std::cerr<<file<<":"<<e.line<<":"<<e.column<<": error: "<<e.message<<"\n";
   return 1;
  }
  if(mode=="check") return 0;
  if(mode=="run") {
   Interpreter runtime(*program); runtime.run(); return 0;
  }
  if(mode=="compile") {
   const auto generated=generate_source(*program,{parse_target(target_name)});
   if(output.empty()) std::cout<<generated;
   else { std::ofstream out(output,std::ios::binary); if(!out){std::cerr<<"noodle: cannot write "<<output<<"\n";return 2;} out<<generated; }
   return 0;
  }
  return usage();
 } catch(const std::exception& e) {
  std::cerr<<"noodle: "<<e.what()<<"\n"; return 1;
 }
}
