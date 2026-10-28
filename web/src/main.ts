type RuleProfile = { born: number[]; survive: number[]; label: string };
type BiomeRecord = { id: string; name: string; organisms: number; status: string; color?: string };
type BiosphereData = { version: string; source: string; biomes: BiomeRecord[]; organisms: { id: string; name: string; biome: string; role: string }[] };

const canvas = document.querySelector<HTMLCanvasElement>("#life-grid");
if (!canvas) throw new Error("Missing #life-grid canvas.");
const ctx = canvas.getContext("2d");
if (!ctx) throw new Error("Canvas 2D context is unavailable.");

const cols = 90;
const rows = 60;
const count = cols * rows;
let grid = new Uint8Array(count);
let next = new Uint8Array(count);
let history = new Uint8Array(count);
let generation = 0;
let timer: number | undefined;
let lastChange = "—";

const get = <T extends HTMLElement>(id: string): T => {
  const element = document.getElementById(id);
  if (!element) throw new Error(`Missing required element: #${id}`);
  return element as T;
};
const rules: Record<string, RuleProfile> = {
  life: { born: [3], survive: [2, 3], label: "B3 / S23" },
  highlife: { born: [3, 6], survive: [2, 3], label: "B36 / S23" },
  seeds: { born: [2], survive: [], label: "B2 / S" },
  daynight: { born: [3, 6, 7, 8], survive: [3, 4, 6, 7, 8], label: "B3678 / S34678" }
};

function paint(): void {
  ctx!.fillStyle = "#091116";
  ctx!.fillRect(0, 0, canvas!.width, canvas!.height);
  const cw = canvas!.width / cols;
  const ch = canvas!.height / rows;
  let alive = 0;
  for (let y = 0; y < rows; y++) for (let x = 0; x < cols; x++) {
    const i = y * cols + x;
    if (grid[i]) {
      alive++;
      ctx!.fillStyle = "#8de0bb";
      ctx!.fillRect(x * cw + 1, y * ch + 1, Math.max(1, cw - 2), Math.max(1, ch - 2));
    } else if (history[i]) {
      ctx!.fillStyle = "#203d3e";
      ctx!.fillRect(x * cw + 1, y * ch + 1, Math.max(1, cw - 2), Math.max(1, ch - 2));
    }
  }
  get("alive-count").textContent = alive.toLocaleString();
  get("population-percent").textContent = (alive / count * 100).toFixed(1) + "%";
  get("generation").textContent = "GEN " + String(generation).padStart(6, "0");
  get("cell-count").textContent = count.toLocaleString();
  get("last-change").textContent = lastChange;
  get("rule-label").textContent = rules[(get<HTMLSelectElement>("ruleset")).value].label;
}
function neighbours(x: number, y: number): number {
  let n = 0;
  for (let dy = -1; dy <= 1; dy++) for (let dx = -1; dx <= 1; dx++) {
    if (!dx && !dy) continue;
    n += grid[((y + dy + rows) % rows) * cols + ((x + dx + cols) % cols)];
  }
  return n;
}
function step(): void {
  const rule = rules[get<HTMLSelectElement>("ruleset").value];
  let changed = 0;
  for (let y = 0; y < rows; y++) for (let x = 0; x < cols; x++) {
    const i = y * cols + x;
    const alive = grid[i] === 1;
    const state = alive ? rule.survive.includes(neighbours(x, y)) : rule.born.includes(neighbours(x, y));
    next[i] = state ? 1 : 0;
    if (next[i] !== grid[i]) changed++;
  }
  history.set(grid);
  [grid, next] = [next, grid];
  generation++;
  lastChange = changed.toLocaleString() + " cells";
  paint();
}
function stop(): void {
  if (timer !== undefined) window.clearInterval(timer);
  timer = undefined;
  get("start").textContent = "▶ Run";
  get("run-status").textContent = "SIMULATION PAUSED";
}
function start(): void {
  if (timer !== undefined) { stop(); return; }
  timer = window.setInterval(step, 1000 / Number((get<HTMLInputElement>("speed")).value));
  get("start").textContent = "Ⅱ Pause";
  get("run-status").textContent = "SIMULATION RUNNING";
}
function randomize(): void {
  stop(); grid.fill(0); history.fill(0);
  for (let i = 0; i < count; i++) grid[i] = Math.random() < 0.24 ? 1 : 0;
  generation = 0; lastChange = "Random seed"; paint();
}
function clearGrid(): void {
  stop(); grid.fill(0); history.fill(0); generation = 0; lastChange = "Cleared"; paint();
}
function escapeHtml(value: string): string {
  return value.replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]!));
}
async function loadBiosphere(): Promise<void> {
  get("world-status").innerHTML = '<span class="spinner"></span> Loading biosphere data…';
  try {
    // Fetch is the browser-native AJAX interface. No server framework is required.
    const response = await fetch("./data/biosphere.json", { headers: { Accept: "application/json" }, cache: "no-store" });
    if (!response.ok) throw new Error("HTTP " + response.status);
    const data = await response.json() as BiosphereData;
    get("biome-count").textContent = String(data.biomes.length);
    get("biome-list").innerHTML = data.biomes.map(b => '<div class="biome-row"><span>' + escapeHtml(b.name) + '</span><span>' + b.organisms + ' ORGANISMS</span></div>').join("");
    get("world-status").textContent = "DATA LOADED · " + data.organisms.length + " ORGANISM RECORDS";
    get("data-source-status").textContent = "DATA SOURCE: JSON / FETCH AJAX";
  } catch {
    get("world-status").textContent = "Could not load biosphere JSON. Serve this folder over HTTP; file:// blocks fetch in many browsers.";
    get("data-source-status").textContent = "DATA SOURCE: UNAVAILABLE";
  }
}
canvas.addEventListener("click", event => {
  const bounds = canvas!.getBoundingClientRect();
  const pointer = event as MouseEvent;
  const x = Math.floor((pointer.clientX - bounds.left) / bounds.width * cols);
  const y = Math.floor((pointer.clientY - bounds.top) / bounds.height * rows);
  if (x >= 0 && x < cols && y >= 0 && y < rows) {
    const i = y * cols + x;
    grid[i] = grid[i] ? 0 : 1; history[i] = 0; lastChange = "Manual edit"; paint();
  }
});
get("start").addEventListener("click", () => timer === undefined ? start() : stop());
get("step").addEventListener("click", () => { stop(); step(); });
get("randomize").addEventListener("click", randomize);
get("clear").addEventListener("click", clearGrid);
get<HTMLInputElement>("speed").addEventListener("input", () => {
  get("speed-value").textContent = get<HTMLInputElement>("speed").value + " steps/s";
  if (timer !== undefined) { stop(); start(); }
});
get<HTMLSelectElement>("ruleset").addEventListener("change", paint);
get("refresh-data").addEventListener("click", () => void loadBiosphere());
randomize();
void loadBiosphere();
