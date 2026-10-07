# Controls

Every input LifeEngine accepts, on the mouse and on the keyboard.

---

## Contents

- [At a glance](#at-a-glance)
- [Mouse](#mouse)
  - [On the grid](#on-the-grid)
  - [On the sidebar](#on-the-sidebar)
  - [On the menu ribbon](#on-the-menu-ribbon)
- [Keyboard](#keyboard)
  - [Simulation](#simulation)
  - [Grid and view](#grid-and-view)
  - [Menus](#menus)
  - [Editing](#editing)
- [Menu reference](#menu-reference)
- [Sidebar reference](#sidebar-reference)
- [Status bar readout](#status-bar-readout)
- [Notes and gotchas](#notes-and-gotchas)

---

## At a glance

| I want to | Mouse | Keyboard |
| --- | --- | --- |
| Start or resume | Click **Start** | `F5` or `Space` |
| Pause | Click **Stop** | `F6`, `Space` or `Esc` |
| Advance one generation | Click **Step** | `F7` |
| Empty the grid | Click **Clear** | `Ctrl+Del` |
| Fill the grid randomly | Click **Randomize** | `Ctrl+R` |
| Go back to the start state | Click **Reset** | `Ctrl+Backspace` |
| Change the rules | Rules drop-down | Menu, Options → Rules |
| Change the grid size | Size drop-down | Menu, Simulation → Grid Size |
| Change the speed | Speed drop-down | Menu, Simulation → Speed |
| Zoom in | Click **+** or wheel up | `Ctrl++` |
| Zoom out | Click **−** or wheel down | `Ctrl+-` |
| Reset the zoom | — | `Ctrl+0` |
| Toggle grid lines | Options → Show Grid | `Ctrl+G` |
| Hide the sidebar | — | `Ctrl+B` |
| Draw a cell | Left click on the grid | — |
| Draw a line of cells | Left drag | — |
| Scroll the view | Right or middle drag | Arrow keys |

---

## Mouse

### On the grid

The grid is a `LifeGrid` control inside a scrolling panel. The control converts
screen pixels to cell coordinates like this:

```
cellX = pixelX * Columns / Grid.Width
cellY = pixelY * Rows   / Grid.Height
```

so it works correctly at any zoom level and at any grid size.

| Button / gesture | Action | Detail |
| --- | --- | --- |
| **Left click** | Toggle one cell | Flips the cell under the pointer between alive and dead. The population counter updates immediately. |
| **Left drag** | Paint | The state of the cell you first touched is remembered; dragging applies that same state, so a drag never toggles a cell twice. |
| **Right drag** | Pan | Scrolls the view in the direction the pointer moves. Useful when zoomed in past the panel size. |
| **Middle drag** | Pan | Identical to right drag, for three-button mice and trackballs. |
| **Wheel** | Zoom | Each notch is one zoom step, in or out. |
| **Double click** | None | Not bound; double clicking is two toggles. |

Coordinates are clamped, so dragging off the edge of the control does not flip a
random cell: `ToggleCellState` bounds-checks before writing.

**Tip.** Painting works while the simulation runs. Set the state of a cell and
the run will pick it up on the next generation.

### On the sidebar

The sidebar is a normal Windows Forms docked panel. Everything in it behaves the
way a Windows control should.

| Control | Interaction |
| --- | --- |
| Buttons | Click to run the command. Give focus with `Tab`, fire with `Space` or `Enter`. |
| Drop-downs | Click to open, then click an item. Click the selected item to step through in place. Type-ahead works: with the list open, typing a character jumps to the first matching entry. |
| Colour swatches | Click to open the Windows colour picker. Pick a colour and press OK; Cancel leaves the old colour. |
| Numeric fields | Arrow keys or the spinner buttons adjust by one. Typing replaces the value. |
| Statistics text | Selectable and scrollable, for copying the numbers out. |

Resizing the window re-lays the sidebar: the simulation group stays at the top,
statistics take the remaining space.

### On the menu ribbon

The ribbon is a `MenuStrip`, so it supports everything a Windows menu does.

| Gesture | Result |
| --- | --- |
| Click a top-level heading | Opens the menu; a second click closes it. |
| Hover across the headings while open | Switches to the hovered menu. |
| Click an item | Runs it and closes the menu. |
| Click a checkable item | Toggles it, leaves a tick. |
| Click a sub-menu parent | Opens the sub-menu to the side. |
| Click outside, or press `Esc` | Closes without choosing. |
| Point at an item | The status line shows what it does. |

---

## Keyboard

Shortcuts are handled two ways. Menu items advertise theirs through Windows'
own accelerator handling; the ones that must work regardless of which control
has focus are caught in the form's `ProcessCmdKey`.

### Simulation

| Key | Action | Notes |
| --- | --- | --- |
| `F5` | Start | Locks in the current grid as the start state, then begins running. |
| `F6` | Stop | Pauses. The grid is left exactly as it is. |
| `F7` | Step | Advances one generation and leaves the simulation paused. |
| `Space` | Start or Stop | Whichever applies to the current state. |
| `Esc` | Stop | Same as `F6`. |
| `Ctrl+R` | Randomize | Fills the grid at 15% density. |
| `Ctrl+Del` | Clear | Empties the grid, resets the generation counter, stops the run. |
| `Ctrl+Backspace` | Reset | Restores the state captured when Start was last pressed. |

### Grid and view

| Key | Action |
| --- | --- |
| `Ctrl++` (or `Ctrl+=`) | Zoom in one step, up to 5x. |
| `Ctrl+-` | Zoom out one step, down to 1x. |
| `Ctrl+0` | Return to 1x. |
| `Ctrl+G` | Show or hide the cell grid lines. |
| `Ctrl+B` | Show or hide the left sidebar. |
| `Left` `Right` `Up` `Down` | Scroll the grid view by one step. |
| `Page Up` / `Page Down` | Scroll the grid view by one page. |
| `Home` | Scroll back to the top-left corner of the grid. |
| `Tab` / `Shift+Tab` | Move focus forward or backward between the sidebar and the grid. |

Zoom is a straight multiplication by 1.5 per step. At 1x the grid exactly fills
the visible panel; at 5x it is five times as wide and tall and the panel grows
scrollbars.

### Menus

| Key | Action |
| --- | --- |
| `Alt` + underlined letter | Open that menu from anywhere. `Alt+F` for File, `Alt+S` for Simulation, `Alt+V` for View, `Alt+O` for Options, `Alt+H` for Help. |
| `Left` / `Right` | Move between top-level menus while one is open. |
| `Up` / `Down` | Move between items. |
| `Enter` | Activate the highlighted item. |
| `Right` on a sub-menu parent | Open the sub-menu. |
| `Left` | Close the sub-menu and return to the parent. |
| `Esc` | Close the menu without choosing. |
| `Alt` or `F10` | Move focus to the menu bar. |

### Editing

| Key | Action |
| --- | --- |
| `Tab` | Next control. |
| `Shift+Tab` | Previous control. |
| `Space` / `Enter` | Press the focused button. |
| `Alt+Down` | Open a focused drop-down. |
| `Esc` | Close a drop-down without changing the selection. |
| `Alt+F4` | Close the program. |

---

## Menu reference

### File

| Item | Shortcut | Effect |
| --- | --- | --- |
| Randomize | `Ctrl+R` | `LifeGame.Randomize(0.15)`. Generation resets to 0. |
| Clear | `Ctrl+Del` | `LifeGame.ClearGrid()`. Generation resets to 0, the run stops, Start is re-enabled. |
| Reset | `Ctrl+Backspace` | `LifeGame.ResetGrid()`. Restores the snapshot taken by the last Start. Generation resets to 0. |
| Exit | `Alt+F4` | Closes the program. |

### Simulation

| Item | Shortcut | Effect |
| --- | --- | --- |
| Start | `F5` | `SetStart()` then `timer.Start()`. Start greys out, Stop enables. |
| Stop | `F6` | `timer.Stop()`, then a repaint so the final state is shown. Stop greys out. |
| Step | `F7` | Stops the timer if it is running, advances one generation, repaints. |
| Speed ▸ | — | Sub-menu of 1, 2, 5, 10. Sets the timer interval to `(11 - speed) * 10` ms. |
| Grid Size ▸ | — | Sub-menu of 25, 50, 75, 100, 150, 200, 250. Builds a brand new `LifeGame` at that size and repaints. |

Changing the size discards the board. That is deliberate: a 100x100 board has no
meaningful mapping onto a 25x25 one.

### View

| Item | Shortcut | Effect |
| --- | --- | --- |
| Zoom In | `Ctrl++` | Multiplies the grid control's width and height by 1.5. |
| Zoom Out | `Ctrl+-` | Divides by 1.5. |
| Reset Zoom | `Ctrl+0` | Back to the base size. |
| Show Grid Lines | `Ctrl+G` | Toggles the one-pixel gutter between cells. Also a checkable menu item, so the tick reflects the state. |
| Show Sidebar | `Ctrl+B` | Hides or shows the left panel. The grid expands to fill the space. |

Zoom is capped at 5x and floored at 1x. The zoom counter in the status bar reads
1 to 5.

### Options

| Item | Effect |
| --- | --- |
| Rules ▸ | Checkable list of all 24 rulesets. Selecting one calls `LifeGame.ChangeRules` immediately, mid-run if a run is in progress. |
| Live Cell Colour | Colour picker for living cells. Default DeepSkyBlue. |
| Dead Cell Colour | Colour picker for empty cells. Default White. |
| History Cell Colour | Colour picker for cells that have been alive before. Default LightGreen. |

### Help

| Item | Effect |
| --- | --- |
| About | Version and licence. |

---

## Sidebar reference

| Group | Control | Same as |
| --- | --- | --- |
| **Simulation** | Start | Simulation → Start |
| | Stop | Simulation → Stop |
| | Step | Simulation → Step |
| | Reset | File → Reset |
| | Clear | File → Clear |
| | Randomize | File → Randomize |
| | Speed drop-down | Simulation → Speed |
| | Size drop-down | Simulation → Grid Size |
| **Rules** | Rules drop-down | Options → Rules |
| **Appearance** | Grid lines check box | View → Show Grid Lines |
| | Live / Dead / History swatches | Options → the three colour items |
| **Statistics** | Generation, population, live ratio, elapsed time, zoom | Read-only |

The sidebar and the menus are two faces of one set of commands. Changing a
setting from either place updates the other, because both drive the same
`FormLife` methods.

---

## Status bar readout

```
Ready | Rule: 23/3 - Conway's Life | Gen: 0 | Pop: 0 | Size: 100 | Speed: 10 | Zoom: 1x
```

| Field | Meaning |
| --- | --- |
| State | `Ready`, `Running` or `Paused`. |
| Rule | The active ruleset, `survive/birth` plus its name. |
| Gen | Generations elapsed since the last clear, resize or randomize. |
| Pop | Live cells in the current generation. |
| Size | Cells per side. |
| Speed | The speed preset, 1 to 10. |
| Zoom | The current magnification, 1x to 5x. |

---

## Notes and gotchas

**Reset only works after Start.** The snapshot that Reset restores is taken by
`SetStart()` when you press Start. If you have never started the simulation, the
snapshot is all-dead and Reset behaves like Clear. Randomize does not take a
snapshot.

**Changing the grid size stops the run.** The model is rebuilt from scratch, so
there is nothing to resume. The reference for the size range is
[README.md](../README.md); the default is 100 a side at speed 10.

**The grid is not wrapped.** Patterns that run off an edge are destroyed by it,
not carried around to the other side. This is the standard behaviour for Conway's
Life on a finite board and it matters for patterns that travel, such as gliders.

**Zoom does not change the cell count.** Zooming in makes cells bigger, not
generation slower or the board larger. To get more cells, change the size.

**Painting while paused.** Cells you toggle while paused are not part of the
start snapshot unless you press Start again, so a Reset will discard them.

**Right-drag panning needs something to pan.** At 1x the grid fits the panel
exactly and there is nowhere to scroll. Panning becomes useful from 2x up.

**Keyboard focus and the grid.** The grid is deliberately not a tab stop, so
`Tab` cycles through the sidebar controls without getting stuck on the board.
Arrow-key scrolling is handled by the form, not by the control.

**If a shortcut seems dead**, click the grid or the form body first and try
again. A focused drop-down swallows keys that would otherwise reach the form.
