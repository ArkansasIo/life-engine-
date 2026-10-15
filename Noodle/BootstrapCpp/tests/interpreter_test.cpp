#include "nudl/interpreter.hpp"
#include "nudl/lexer.hpp"
#include "nudl/parser.hpp"
#include <cassert>
#include <string>
int main() {
 const std::string source="fn add(a: i64, b: i64) -> i64 { return a + b; } fn main() -> i64 { let result: i64 = add(20, 22); print(result); return 0; }";
 auto tokens=nudl::lex(source); nudl::Diagnostics diagnostics; nudl::Parser parser(std::move(tokens),diagnostics); auto program=parser.parse(); assert(diagnostics.ok()); nudl::Interpreter runtime(*program); auto value=runtime.run(); (void)value; return 0;
}
