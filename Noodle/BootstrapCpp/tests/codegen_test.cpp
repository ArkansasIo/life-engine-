#include "nudl/codegen.hpp"
#include "nudl/lexer.hpp"
#include "nudl/parser.hpp"
#include <cassert>
#include <string>
int main() {
 const std::string source="fn main() -> i64 { print(42); return 0; }";
 auto tokens=nudl::lex(source); nudl::Diagnostics diagnostics; nudl::Parser parser(std::move(tokens),diagnostics); auto program=parser.parse(); assert(diagnostics.ok()); auto generated=nudl::generate_source(*program,{nudl::TargetLanguage::Cpp}); assert(generated.find("int main()")!=std::string::npos); return 0;
}
