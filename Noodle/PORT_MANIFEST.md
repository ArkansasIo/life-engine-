# C# → Noodle source map

Inventory: 66 C# source files under `Sources/`. The C# implementation remains preserved as the reference. A matching `.nudl` target path is tracked for each source file; a file's existence does not imply translation completeness or runtime validation.

| C# source | Noodle target | Port status |
|---|---|---|
| `Sources/Ai/CellAi.cs` | `Noodle/Ai/CellAi.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Ai/Difficulty.cs` | `Noodle/Ai/Difficulty.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Attributes/Attributes.cs` | `Noodle/Attributes/Attributes.nudl` | Partial implementation; validate parity |
| `Sources/Biology/AnimalCatalogue.cs` | `Noodle/Biology/AnimalCatalogue.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Biology/AnimalKingdom.cs` | `Noodle/Biology/AnimalKingdom.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Biology/AnimalRanks.cs` | `Noodle/Biology/AnimalRanks.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Biology/Animals.cs` | `Noodle/Biology/Animals.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Biology/Biomes.cs` | `Noodle/Biology/Biomes.nudl` | Partial implementation; validate parity |
| `Sources/Biology/Biosphere.cs` | `Noodle/Biology/Biosphere.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Biology/Enums.cs` | `Noodle/Biology/Enums.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Biology/Genetics/HumanBiologyCatalog.cs` | `Noodle/Biology/Genetics/HumanBiologyCatalog.nudl` | Partial implementation; validate parity |
| `Sources/Biology/Genetics/MutationEngine.cs` | `Noodle/Biology/Genetics/MutationEngine.nudl` | Partial implementation; validate parity |
| `Sources/Biology/Genetics/MutationModels.cs` | `Noodle/Biology/Genetics/MutationModels.nudl` | Partial implementation; validate parity |
| `Sources/Biology/Genetics/SequenceValidator.cs` | `Noodle/Biology/Genetics/SequenceValidator.nudl` | Partial implementation; validate parity |
| `Sources/Biology/Kingdoms.cs` | `Noodle/Biology/Kingdoms.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Biology/NucleicAcid.cs` | `Noodle/Biology/NucleicAcid.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Biology/Organism.cs` | `Noodle/Biology/Organism.nudl` | Partial implementation; validate parity |
| `Sources/Biology/Taxonomy.cs` | `Noodle/Biology/Taxonomy.nudl` | Partial implementation; validate parity |
| `Sources/Biology/WolfPack.cs` | `Noodle/Biology/WolfPack.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Biology/WorldMap.cs` | `Noodle/Biology/WorldMap.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Combat/BattleLog.cs` | `Noodle/Combat/BattleLog.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Combat/Bestiary.cs` | `Noodle/Combat/Bestiary.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Combat/CombatEngine.cs` | `Noodle/Combat/CombatEngine.nudl` | Partial implementation; validate parity |
| `Sources/Combat/Combatant.cs` | `Noodle/Combat/Combatant.nudl` | Partial implementation; validate parity |
| `Sources/Combat/Effects.cs` | `Noodle/Combat/Effects.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Combat/EffectsCatalogue.cs` | `Noodle/Combat/EffectsCatalogue.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Configuration/Config.cs` | `Noodle/Configuration/Config.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Configuration/ConfigDefaults.cs` | `Noodle/Configuration/ConfigDefaults.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Configuration/ConfigFile.cs` | `Noodle/Configuration/ConfigFile.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Configuration/Scheduler.cs` | `Noodle/Configuration/Scheduler.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Configuration/UpdateSystem.cs` | `Noodle/Configuration/UpdateSystem.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Configuration/VersionInfo.cs` | `Noodle/Configuration/VersionInfo.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Data/Database.cs` | `Noodle/Data/Database.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Data/ExportBuilder.cs` | `Noodle/Data/ExportBuilder.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Data/SpreadsheetWriter.cs` | `Noodle/Data/SpreadsheetWriter.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Ecosystem/Ecosystem.cs` | `Noodle/Ecosystem/Ecosystem.nudl` | Partial implementation; validate parity |
| `Sources/Ecosystem/WorldFactory.cs` | `Noodle/Ecosystem/WorldFactory.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/FormIcon.cs` | `Noodle/Host/Forms/FormIcon.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/FormLife.Designer.cs` | `Noodle/Host/Forms/FormLife.Designer.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/FormLife.cs` | `Noodle/Host/Forms/FormLife.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Gui/AppContext.cs` | `Noodle/Host/Gui/AppContext.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Gui/MainForm.cs` | `Noodle/Host/Gui/MainForm.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Gui/ToolsPanel.cs` | `Noodle/Host/Gui/ToolsPanel.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Gui/WolfPanel.cs` | `Noodle/Host/Gui/WolfPanel.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Gui/WorldMapView.cs` | `Noodle/Host/Gui/WorldMapView.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Gui/WorldPanel.cs` | `Noodle/Host/Gui/WorldPanel.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/LifeGame.cs` | `Noodle/Core/LifeGame.nudl` | Partial implementation; validate parity |
| `Sources/LifeGrid.Designer.cs` | `Noodle/Host/Gui/LifeGrid.Designer.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/LifeGrid.cs` | `Noodle/Host/Gui/LifeGrid.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Program.cs` | `Noodle/Host/Program.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Properties/AssemblyInfo.cs` | `Noodle/Host/AssemblyInfo.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Properties/Resources.Designer.cs` | `Noodle/Host/Resources.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Properties/Settings.Designer.cs` | `Noodle/Host/Settings.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Scripting/Interpreter.cs` | `Noodle/Scripting/Interpreter.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Scripting/Lexer.cs` | `Noodle/Scripting/Lexer.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Scripting/Parser.cs` | `Noodle/Scripting/Parser.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Scripting/ScriptHost.cs` | `Noodle/Scripting/ScriptHost.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Scripting/ScriptManager.cs` | `Noodle/Scripting/ScriptManager.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Scripting/StandardLibrary.cs` | `Noodle/Scripting/StandardLibrary.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Scripting/Values.cs` | `Noodle/Scripting/Values.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Talents/TalentTree.cs` | `Noodle/Talents/TalentTree.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Tools/IconGenerator.cs` | `Noodle/Tools/IconGenerator.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Tools/SelfTest.cs` | `Noodle/Tests/SelfTest.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/Wolf/WolfLogic.cs` | `Noodle/Wolf/WolfLogic.nudl` | Generated contract/scaffold or partial implementation; validate parity |
| `Sources/World/DayNightCycle.cs` | `Noodle/World/DayNightCycle.nudl` | Partial implementation; validate parity |
| `Sources/World/Weather.cs` | `Noodle/World/Weather.nudl` | Partial implementation; validate parity |

## Host-specific behavior

WinForms UI, image resources, file-system operations, update networking, database providers, and spreadsheet binaries require explicit host adapters. The current Noodle bootstrap cannot compile all modules; Noodle-native compiler/runtime and host APIs remain incomplete. Do not remove C# reference code until the entire Noodle project compiles and deterministic parity tests pass.
