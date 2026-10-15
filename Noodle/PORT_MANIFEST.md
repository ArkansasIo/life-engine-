# C# → Noodle source map

Every C# file under `Sources/` has a corresponding Noodle target path. C# is preserved as the behavioral reference until compiler validation and parity tests pass.

| C# source | Noodle target | Status |
|---|---|---|
| `Sources/Biology/AnimalCatalogue.cs` | `Noodle/Biology/AnimalCatalogue.nudl` | Scaffold/partial; verify parity |
| `Sources/Biology/AnimalKingdom.cs` | `Noodle/Biology/AnimalKingdom.nudl` | Scaffold/partial; verify parity |
| `Sources/Biology/AnimalRanks.cs` | `Noodle/Biology/AnimalRanks.nudl` | Scaffold/partial; verify parity |
| `Sources/Biology/Animals.cs` | `Noodle/Biology/Animals.nudl` | Scaffold/partial; verify parity |
| `Sources/Biology/Biosphere.cs` | `Noodle/Biology/Biosphere.nudl` | Scaffold/partial; verify parity |
| `Sources/Biology/Enums.cs` | `Noodle/Biology/Enums.nudl` | Scaffold/partial; verify parity |
| `Sources/Biology/Kingdoms.cs` | `Noodle/Biology/Kingdoms.nudl` | Scaffold/partial; verify parity |
| `Sources/Biology/NucleicAcid.cs` | `Noodle/Biology/NucleicAcid.nudl` | Scaffold/partial; verify parity |
| `Sources/Biology/WolfPack.cs` | `Noodle/Biology/WolfPack.nudl` | Scaffold/partial; verify parity |
| `Sources/Combat/BattleLog.cs` | `Noodle/Combat/BattleLog.nudl` | Scaffold/partial; verify parity |
| `Sources/Combat/Bestiary.cs` | `Noodle/Combat/Bestiary.nudl` | Scaffold/partial; verify parity |
| `Sources/Combat/Effects.cs` | `Noodle/Combat/Effects.nudl` | Scaffold/partial; verify parity |
| `Sources/Combat/EffectsCatalogue.cs` | `Noodle/Combat/EffectsCatalogue.nudl` | Scaffold/partial; verify parity |
| `Sources/Configuration/Config.cs` | `Noodle/Configuration/Config.nudl` | Scaffold/partial; verify parity |
| `Sources/Configuration/ConfigFile.cs` | `Noodle/Configuration/ConfigFile.nudl` | Scaffold/partial; verify parity |
| `Sources/Configuration/Scheduler.cs` | `Noodle/Configuration/Scheduler.nudl` | Scaffold/partial; verify parity |
| `Sources/Configuration/UpdateSystem.cs` | `Noodle/Configuration/UpdateSystem.nudl` | Scaffold/partial; verify parity |
| `Sources/Configuration/VersionInfo.cs` | `Noodle/Configuration/VersionInfo.nudl` | Scaffold/partial; verify parity |
| `Sources/Data/ExportBuilder.cs` | `Noodle/Data/ExportBuilder.nudl` | Scaffold/partial; verify parity |
| `Sources/Data/SpreadsheetWriter.cs` | `Noodle/Data/SpreadsheetWriter.nudl` | Scaffold/partial; verify parity |
| `Sources/Ecosystem/WorldFactory.cs` | `Noodle/Ecosystem/WorldFactory.nudl` | Scaffold/partial; verify parity |
| `Sources/Host/Forms/FormIcon.cs` | `Noodle/Host/Forms/FormIcon.nudl` | Scaffold/partial; verify parity |
| `Sources/Host/Forms/FormLife.Designer.cs` | `Noodle/Host/Forms/FormLife.Designer.nudl` | Scaffold/partial; verify parity |
| `Sources/Host/Forms/FormLife.cs` | `Noodle/Host/Forms/FormLife.nudl` | Scaffold/partial; verify parity |
| `Sources/Host/Gui/AppContext.cs` | `Noodle/Host/Gui/AppContext.nudl` | Scaffold/partial; verify parity |
| `Sources/Host/Gui/MainForm.cs` | `Noodle/Host/Gui/MainForm.nudl` | Scaffold/partial; verify parity |
| `Sources/Host/Gui/ToolsPanel.cs` | `Noodle/Host/Gui/ToolsPanel.nudl` | Scaffold/partial; verify parity |
| `Sources/Host/Gui/WolfPanel.cs` | `Noodle/Host/Gui/WolfPanel.nudl` | Scaffold/partial; verify parity |
| `Sources/Host/Gui/WorldMapView.cs` | `Noodle/Host/Gui/WorldMapView.nudl` | Scaffold/partial; verify parity |
| `Sources/Host/Gui/WorldPanel.cs` | `Noodle/Host/Gui/WorldPanel.nudl` | Scaffold/partial; verify parity |
| `Sources/Host/Gui/LifeGrid.Designer.cs` | `Noodle/Host/Gui/LifeGrid.Designer.nudl` | Scaffold/partial; verify parity |
| `Sources/Host/Gui/LifeGrid.cs` | `Noodle/Host/Gui/LifeGrid.nudl` | Scaffold/partial; verify parity |
| `Sources/Host/Program.cs` | `Noodle/Host/Program.nudl` | Scaffold/partial; verify parity |
| `Sources/Host/AssemblyInfo.cs` | `Noodle/Host/AssemblyInfo.nudl` | Scaffold/partial; verify parity |
| `Sources/Host/Resources.cs` | `Noodle/Host/Resources.nudl` | Scaffold/partial; verify parity |
| `Sources/Host/Settings.cs` | `Noodle/Host/Settings.nudl` | Scaffold/partial; verify parity |
| `Sources/Scripting/Interpreter.cs` | `Noodle/Scripting/Interpreter.nudl` | Scaffold/partial; verify parity |
| `Sources/Scripting/ScriptHost.cs` | `Noodle/Scripting/ScriptHost.nudl` | Scaffold/partial; verify parity |
| `Sources/Scripting/ScriptManager.cs` | `Noodle/Scripting/ScriptManager.nudl` | Scaffold/partial; verify parity |
| `Sources/Scripting/StandardLibrary.cs` | `Noodle/Scripting/StandardLibrary.nudl` | Scaffold/partial; verify parity |
| `Sources/Tools/IconGenerator.cs` | `Noodle/Tools/IconGenerator.nudl` | Scaffold/partial; verify parity |
| `Sources/Tests/SelfTest.cs` | `Noodle/Tests/SelfTest.nudl` | Scaffold/partial; verify parity |

Existing modules include the cellular automaton, attributes, world clock/weather, ecosystem, combatant/engine, difficulty/config defaults, scripting lexer/parser/value contracts, database seam, talents, wolf logic, and compiler/runtime scaffolds. Their status remains partial. Host adapters are contracts until backed by a compatible runtime.
