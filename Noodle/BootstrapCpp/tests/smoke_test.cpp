#include "nudl/lexer.hpp"
#include "nudl/parser.hpp"
#include <cassert>
#include <string>
int main() {
 const std::string source="fn main() -> i64 { let x: i64 = 2 + 3; print(x); return 0; }";
 auto tokens=nudl::lex(source); nudl::Diagnostics diagnostics; nudl::Parser parser(std::move(tokens),diagnostics); auto program=parser.parse();
 assert(diagnostics.ok()); assert(program); assert(program->functions.size()==1); return 0;
}
