# LifeEngine example scripts.
#
# These are ordinary files in the scripting language. They are loaded at start-up
# from the Scripts folder, in alphabetical order, and they share one interpreter,
# so a class declared in one file can be used by a later one.
#
# Run them from the Script menu, or call them from another script by name.

# ---------------------------------------------------------------------------
# 01-hello.lua
# The smallest useful script: it talks, and it reads the state of the program.
# ---------------------------------------------------------------------------

print("LifeEngine scripting")
print("grid is " .. gridsize() .. " cells a side")
print("generation " .. generation())
print("population " .. population())
print("rules are " .. rules.get())

# A table is both a list and a record.
spawns = { "block", "beehive", "glider", "blinker" }

print("known spawns:")

for name in spawns do
  print("  " .. name)
end

# ---------------------------------------------------------------------------
# 02-patterns.lua
# A class: a named pattern that knows how to stamp itself onto the grid.
#
# Classes are declared with "class ... end", inherit with "extends", and get
# their fields from the class at construction time.
# ---------------------------------------------------------------------------

class Pattern
  width = 0
  height = 0
  cells = {}

  function init(name)
    self.name = name
  end

  function add(dx, dy)
    table.append(self.cells, { x = dx, y = dy })
    return self
  end

  function size()
    return len(self.cells)
  end

  function stamp(originX, originY)
    for cell in self.cells do
      grid.set(originX + cell.x, originY + cell.y, true)
    end
    return self.size()
  end

  function describe()
    return self.name .. " (" .. self.size() .. " cells)"
  end
end

# A block is a 2x2 square: it never changes.
class Block extends Pattern
  function init()
    super.init("Block")
    self.add(0, 0)
    self.add(1, 0)
    self.add(0, 1)
    self.add(1, 1)
  end
end

# A blinker is three in a row: it flips between a row and a column.
class Blinker extends Pattern
  function init()
    super.init("Blinker")
    self.add(0, 0)
    self.add(1, 0)
    self.add(2, 0)
  end
end

# A glider walks one cell diagonally every four generations.
class Glider extends Pattern
  function init()
    super.init("Glider")
    self.add(1, 0)
    self.add(2, 1)
    self.add(0, 2)
    self.add(1, 2)
    self.add(2, 2)
  end
end

# ---------------------------------------------------------------------------
# 03-grid.lua
# Drawing with the grid functions, and watching what happens.
# ---------------------------------------------------------------------------

function clearAndDraw()
  grid.clear()

  block = new Block()
  blinker = new Blinker()
  glider = new Glider()

  print("stamping " .. block.describe())
  print("stamping " .. blinker.describe())
  print("stamping " .. glider.describe())

  block.stamp(5, 5)
  blinker.stamp(20, 5)
  glider.stamp(10, 20)

  return population()
end

function measureGrowth(generations)
  before = population()

  sim.run(generations)

  after = population()

  print("after " .. generations .. " generations: " ..
        before .. " -> " .. after .. " cells")
  print("generation is now " .. sim.generation())

  return after
end

# ---------------------------------------------------------------------------
# 04-analysis.lua
# Reading the board back: density, neighbours, and the bounding box of a shape.
# ---------------------------------------------------------------------------

function density()
  total = grid.size() * grid.size()

  if total == 0 then
    return 0
  end

  return population() / total
end

function boundingBox()
  minX = 99999
  minY = 99999
  maxX = -1
  maxY = -1
  found = 0

  size = grid.size()

  for y in rangeOf(0, size - 1) do
    for x in rangeOf(0, size - 1) do
      if grid.get(x, y) then
        found = found + 1

        if x < minX then minX = x end
        if y < minY then minY = y end
        if x > maxX then maxX = x end
        if y > maxY then maxY = y end
      end
    end
  end

  if found == 0 then
    return nil
  end

  return { minX = minX, minY = minY, maxX = maxX, maxY = maxY, count = found }
end

function crowdedCells()
  count = 0
  size = grid.size()

  for y in rangeOf(0, size - 1) do
    for x in rangeOf(0, size - 1) do
      if grid.neighbours(x, y) > 5 then
        count = count + 1
      end
    end
  end

  return count
end

# ---------------------------------------------------------------------------
# 05-simulation.lua
# The entry point. A manager calls this after the scripts are loaded.
# ---------------------------------------------------------------------------

function main()
  print("--- analysis ---")
  print("density before: " .. math.round(density(), 4))

  clearAndDraw()

  box = boundingBox()

  if box != nil then
    print("live cells span x " .. box.minX .. " to " .. box.maxX ..
          ", y " .. box.minY .. " to " .. box.maxY)
  end

  print("crowded cells: " .. crowdedCells())

  measureGrowth(4)

  print("density after: " .. math.round(density(), 4))

  sim.step()
  print("stepped once more, now generation " .. sim.generation())

  return true
end
