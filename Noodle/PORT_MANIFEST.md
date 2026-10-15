# C# → Noodle port manifest

Source inventory captured from `master`: 66 C# files under `Sources/` (approximately 1.14 MB), plus project resources and non-C# data. The Noodle port is additive; C# files are preserved as the behavioral reference until the Noodle compiler/runtime and platform adapters are available.

## Porting rules

- Preserve public types, invariants, formulas, defaults, edge cases, and error behavior.
- Keep UI, operating-system, database-driver, and file-system behavior behind explicit host interfaces; do not pretend WinForms/OLE DB APIs exist in NUDL.
- Keep each Noodle module mapped to its source path.
- A module is not complete merely because a file exists. Update this manifest only after behavior has been translated and reviewed.
- NUDL source is currently uncompiled in this repository; compiler validation and parity tests are required before claiming runtime equivalence.

## Source modules

| C# source | Noodle target | Status |
|---|---|---|
| `Sources/Ai/CellAi.cs` | `Noodle/Ai/CellAi.nudl` | Not yet translated/reviewed |
| `Sources/Ai/Difficulty.cs` | `Noodle/Ai/Difficulty.nudl` | Not yet translated/reviewed |
| `Sources/Attributes/Attributes.cs` | `Noodle/Attributes/Attributes.nudl` | Not yet translated/reviewed |
| `Sources/Biology/AnimalCatalogue.cs` | `Noodle/Biology/AnimalCatalogue.nudl` | Not yet translated/reviewed |
| `Sources/Biology/AnimalKingdom.cs` | `Noodle/Biology/AnimalKingdom.nudl` | Not yet translated/reviewed |
| `Sources/Biology/AnimalRanks.cs` | `Noodle/Biology/AnimalRanks.nudl` | Not yet translated/reviewed |
| `Sources/Biology/Animals.cs` | `Noodle/Biology/Animals.nudl` | Not yet translated/reviewed |
| `Sources/Biology/Biomes.cs` | `Noodle/Biology/Biomes.nudl` | Not yet translated/reviewed |
| `Sources/Biology/Biosphere.cs` | `Noodle/Biology/Biosphere.nudl` | Not yet translated/reviewed |
| `Sources/Biology/Enums.cs` | `Noodle/Biology/Enums.nudl` | Not yet translated/reviewed |
| `Sources/Biology/Genetics/HumanBiologyCatalog.cs` | `Noodle/Biology/Genetics/HumanBiologyCatalog.nudl` | Not yet translated/reviewed |
| `Sources/Biology/Genetics/MutationEngine.cs` | `Noodle/Biology/Genetics/MutationEngine.nudl` | Not yet translated/reviewed |
| `Sources/Biology/Genetics/MutationModels.cs` | `Noodle/Biology/Genetics/MutationModels.nudl` | Not yet translated/reviewed |
| `Sources/Biology/Genetics/SequenceValidator.cs` | `Noodle/Biology/Genetics/SequenceValidator.nudl` | Not yet translated/reviewed |
| `Sources/Biology/Kingdoms.cs` | `Noodle/Biology/Kingdoms.nudl` | Not yet translated/reviewed |
| `Sources/Biology/NucleicAcid.cs` | `Noodle/Biology/NucleicAcid.nudl` | Not yet translated/reviewed |
| `Sources/Biology/Organism.cs` | `Noodle/Biology/Organism.nudl` | Not yet translated/reviewed |
| `Sources/Biology/Taxonomy.cs` | `Noodle/Biology/Taxonomy.nudl` | Not yet translated/reviewed |
| `Sources/Biology/WolfPack.cs` | `Noodle/Biology/WolfPack.nudl` | Not yet translated/reviewed |
| `Sources/Biology/WorldMap.cs` | `Noodle/Biology/WorldMap.nudl` | Not yet translated/reviewed |
| `Sources/Combat/BattleLog.cs` | `Noodle/Combat/BattleLog.nudl` | Not yet translated/reviewed |
| `Sources/Combat/Bestiary.cs` | `Noodle/Combat/Bestiary.nudl` | Not yet translated/reviewed |
| `Sources/Combat/CombatEngine.cs` | `Noodle/Combat/CombatEngine.nudl` | Not yet translated/reviewed |
| `Sources/Combat/Combatant.cs` | `Noodle/Combat/Combatant.nudl` | Not yet translated/reviewed |
| `Sources/Combat/Effects.cs` | `Noodle/Combat/Effects.nudl` | Not yet translated/reviewed |
| `Sources/Combat/EffectsCatalogue.cs` | `Noodle/Combat/EffectsCatalogue.nudl` | Not yet translated/reviewed |
| `Sources/Configuration/Config.cs` | `Noodle/Configuration/Config.nudl` | Not yet translated/reviewed |
| `Sources/Configuration/ConfigDefaults.cs` | `Noodle/Configuration/ConfigDefaults.nudl` | Not yet translated/reviewed |
| `Sources/Configuration/ConfigFile.cs` | `Noodle/Configuration/ConfigFile.nudl` | Not yet translated/reviewed |
| `Sources/Configuration/Scheduler.cs` | `Noodle/Configuration/Scheduler.nudl` | Not yet translated/reviewed |
| `Sources/Configuration/UpdateSystem.cs` | `Noodle/Configuration/UpdateSystem.nudl` | Not yet translated/reviewed |
| `Sources/Configuration/VersionInfo.cs` | `Noodle/Configuration/VersionInfo.nudl` | Not yet translated/reviewed |
| `Sources/Data/Database.cs` | `Noodle/Data/Database.nudl` | Not yet translated/reviewed |
| `Sources/Data/ExportBuilder.cs` | `Noodle/Data/ExportBuilder.nudl` | Not yet translated/reviewed |
| `Sources/Data/SpreadsheetWriter.cs` | `Noodle/Data/SpreadsheetWriter.nudl` | Not yet translated/reviewed |
| `Sources/Ecosystem/Ecosystem.cs` | `Noodle/Ecosystem/Ecosystem.nudl` | Not yet translated/reviewed |
| `Sources/Ecosystem/WorldFactory.cs` | `Noodle/Ecosystem/WorldFactory.nudl` | Not yet translated/reviewed |
| `Sources/FormIcon.cs` | `Noodle/Host/Forms/FormIcon.nudl` | Not yet translated/reviewed |
| `Sources/FormLife.Designer.cs` | `Noodle/Host/Forms/FormLife.Designer.nudl` | Not yet translated/reviewed |
| `Sources/FormLife.cs` | `Noodle/Host/Forms/FormLife.nudl` | Not yet translated/reviewed |
| `Sources/Gui/AppContext.cs` | `Noodle/Host/Gui/AppContext.nudl` | Not yet translated/reviewed |
| `Sources/Gui/MainForm.cs` | `Noodle/Host/Gui/MainForm.nudl` | Not yet translated/reviewed |
| `Sources/Gui/ToolsPanel.cs` | `Noodle/Host/Gui/ToolsPanel.nudl` | Not yet translated/reviewed |
| `Sources/Gui/WolfPanel.cs` | `Noodle/Host/Gui/WolfPanel.nudl` | Not yet translated/reviewed |
| `Sources/Gui/WorldMapView.cs` | `Noodle/Host/Gui/WorldMapView.nudl` | Not yet translated/reviewed |
| `Sources/Gui/WorldPanel.cs` | `Noodle/Host/Gui/WorldPanel.nudl` | Not yet translated/reviewed |
| `Sources/LifeGame.cs` | `Noodle/Core/LifeGame.nudl` | Not yet translated/reviewed |
| `Sources/LifeGrid.Designer.cs` | `Noodle/Host/Gui/LifeGrid.Designer.nudl` | Not yet translated/reviewed |
| `Sources/LifeGrid.cs` | `Noodle/Host/Gui/LifeGrid.nudl` | Not yet translated/reviewed |
| `Sources/Program.cs` | `Noodle/Host/Program.nudl` | Not yet translated/reviewed |
| `Sources/Properties/AssemblyInfo.cs` | `Noodle/Host/AssemblyInfo.nudl` | Not yet translated/reviewed |
| `Sources/Properties/Resources.Designer.cs` | `Noodle/Host/Resources.nudl` | Not yet translated/reviewed |
| `Sources/Properties/Settings.Designer.cs` | `Noodle/Host/Settings.nudl` | Not yet translated/reviewed |
| `Sources/Scripting/Interpreter.cs` | `Noodle/Scripting/Interpreter.nudl` | Not yet translated/reviewed |
| `Sources/Scripting/Lexer.cs` | `Noodle/Scripting/Lexer.nudl` | Not yet translated/reviewed |
| `Sources/Scripting/Parser.cs` | `Noodle/Scripting/Parser.nudl` | Not yet translated/reviewed |
| `Sources/Scripting/ScriptHost.cs` | `Noodle/Scripting/ScriptHost.nudl` | Not yet translated/reviewed |
| `Sources/Scripting/ScriptManager.cs` | `Noodle/Scripting/ScriptManager.nudl` | Not yet translated/reviewed |
| `Sources/Scripting/StandardLibrary.cs` | `Noodle/Scripting/StandardLibrary.nudl` | Not yet translated/reviewed |
| `Sources/Scripting/Values.cs` | `Noodle/Scripting/Values.nudl` | Not yet translated/reviewed |
| `Sources/Talents/TalentTree.cs` | `Noodle/Talents/TalentTree.nudl` | Not yet translated/reviewed |
| `Sources/Tools/IconGenerator.cs` | `Noodle/Tools/IconGenerator.nudl` | Not yet translated/reviewed |
| `Sources/Tools/SelfTest.cs` | `Noodle/Tests/SelfTest.nudl` | Not yet translated/reviewed |
| `Sources/Wolf/WolfLogic.cs` | `Noodle/Wolf/WolfLogic.nudl` | Not yet translated/reviewed |
| `Sources/World/DayNightCycle.cs` | `Noodle/World/DayNightCycle.nudl` | Not yet translated/reviewed |
| `Sources/World/Weather.cs` | `Noodle/World/Weather.nudl` | Not yet translated/reviewed |

## Non-C# project inputs

The project also contains SQL schemas/seeds, Lua examples, designer/resource files, images, release metadata, XML update configuration, documentation, and build configuration. These require language-appropriate treatment rather than mechanical conversion to NUDL.
