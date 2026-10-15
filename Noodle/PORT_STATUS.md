# Noodle port status

This is a behavioral migration, not a file-extension conversion. The repository contains 66 C# source files (approximately 1.14 MB), including platform-dependent UI and database adapters. Files marked partial are not complete translations.

## Initial modules added

- `Noodle/Core/LifeGame.nudl` — cellular automaton state and stepping; partial, compiler/runtime APIs need validation.
- `Noodle/Attributes/Attributes.nudl` — attribute score, grade, training; partial, compare against the full C# class.
- `Noodle/World/DayNightCycle.nudl` — normalized time/day model; partial.
- `Noodle/World/Weather.nudl` — weather state and validation; partial.
- `Noodle/Ecosystem/Ecosystem.nudl` — population validation and logistic growth helper; partial, not a full ecosystem port.
- `Noodle/Combat/Combatant.nudl` — simplified combatant state, damage, and healing; partial.
- `Noodle/Combat/CombatEngine.nudl` — basic attack resolution; partial, not equivalent to the complete C# combat pipeline.
- `Noodle/Ai/Difficulty.nudl` — starter difficulty profiles; partial, constants must be reconciled with C#.
- `Noodle/Configuration/ConfigDefaults.nudl` — core simulation defaults; partial, not a complete configuration system.
- `Noodle/Scripting/Lexer.nudl` — token/error contract only; not a complete lexer.
- Existing genetics files — initial synthetic mutation port, not compiler-verified.

## Still required

Translate and review all 66 source modules, including animal taxonomy/catalogues, biosphere/biomes/world map, wolf behavior and packs, complete combat effects/bestiary/logging, AI decision logic, configuration persistence/scheduler/updater, database and export implementations, full lexer/parser/interpreter/standard library, talents, GUI host interfaces, entry point, and tests/tools.

WinForms, OLE DB, file-system, and image-resource behavior need explicit Noodle host interfaces or a compatible runtime; NUDL source alone cannot replace those operating-system integrations.

## Validation gates

1. Confirm the intended NUDL 0.3 grammar and standard-library API.
2. Compile every `.nudl` file.
3. Port C# tests and add behavior-parity cases for simulation, combat, population, configuration, and script evaluation.
4. Compare deterministic seeds and edge cases against the C# reference.
5. Do not remove or replace C# sources until Noodle builds and behavior parity passes.
