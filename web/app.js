(() => {
  "use strict";
  const canvas = document.getElementById("life-grid");
  const ctx = canvas.getContext("2d");
  const cols = 90, rows = 60, count = cols * rows;
  let grid = new Uint8Array(count), next = new Uint8Array(count), history = new Uint8Array(count);
  let generation = 0, timer = null, lastChange = "—", steps = 0;
  const el = id => document.getElementById(id);
  const rules = {
    life: {born:[3], survive:[2,3], label:"B3 / S23"},
    highlife: {born:[3,6], survive:[2,3], label:"B36 / S23"},
    seeds: {born:[2], survive:[], label:"B2 / S"},
    daynight: {born:[3,6,7,8], survive:[3,4,6,7,8], label:"B3678 / S34678"}
  };
  function paint() {
    ctx.fillStyle = "#091116"; ctx.fillRect(0,0,canvas.width,canvas.height);
    const cw=canvas.width/cols, ch=canvas.height/rows;
    let alive=0;
    for(let y=0;y<rows;y++) for(let x=0;x<cols;x++) {
      const i=y*cols+x;
      if(grid[i]) { alive++; ctx.fillStyle="#8de0bb"; ctx.fillRect(x*cw+1,y*ch+1,Math.max(1,cw-2),Math.max(1,ch-2)); }
      else if(history[i]) { ctx.fillStyle="#203d3e"; ctx.fillRect(x*cw+1,y*ch+1,Math.max(1,cw-2),Math.max(1,ch-2)); }
    }
    el("alive-count").textContent=alive.toLocaleString();
    el("population-percent").textContent=(alive/count*100).toFixed(1)+"%";
    el("generation").textContent="GEN "+String(generation).padStart(6,"0");
    el("cell-count").textContent=count.toLocaleString();
    el("last-change").textContent=lastChange;
    el("rule-label").textContent=rules[el("ruleset").value].label;
  }
  function neighbours(x,y) {
    let n=0;
    for(let dy=-1;dy<=1;dy++) for(let dx=-1;dx<=1;dx++) {
      if(!dx&&!dy) continue;
      const xx=(x+dx+cols)%cols, yy=(y+dy+rows)%rows;
      n+=grid[yy*cols+xx];
    }
    return n;
  }
  function step() {
    const rule=rules[el("ruleset").value]; let changed=0;
    for(let y=0;y<rows;y++) for(let x=0;x<cols;x++) {
      const i=y*cols+x, n=neighbours(x,y), alive=grid[i]===1;
      const state=alive ? rule.survive.includes(n) : rule.born.includes(n);
      next[i]=state?1:0; if(next[i]!==grid[i]) changed++;
    }
    history.set(grid); [grid,next]=[next,grid]; generation++; steps++;
    lastChange=changed.toLocaleString()+" cells"; paint();
  }
  function stop() { if(timer) clearInterval(timer); timer=null; el("start").textContent="▶ Run"; el("run-status").textContent="SIMULATION PAUSED"; }
  function start() { if(timer){stop();return;} const ms=1000/Number(el("speed").value); timer=setInterval(step,ms); el("start").textContent="Ⅱ Pause"; el("run-status").textContent="SIMULATION RUNNING"; }
  function randomize() { stop(); grid.fill(0); history.fill(0); for(let i=0;i<count;i++) grid[i]=Math.random()<0.24?1:0; generation=0;lastChange="Random seed";paint(); }
  function clear() { stop();grid.fill(0);history.fill(0);generation=0;lastChange="Cleared";paint(); }
  canvas.addEventListener("click",ev=>{const r=canvas.getBoundingClientRect();const x=Math.floor((ev.clientX-r.left)/r.width*cols),y=Math.floor((ev.clientY-r.top)/r.height*rows);if(x>=0&&x<cols&&y>=0&&y<rows){const i=y*cols+x;grid[i]=grid[i]?0:1;history[i]=0;lastChange="Manual edit";paint();}});
  el("start").addEventListener("click",()=>{if(timer)stop();else start();});
  el("step").addEventListener("click",()=>{stop();step();});
  el("randomize").addEventListener("click",randomize);el("clear").addEventListener("click",clear);
  el("speed").addEventListener("input",()=>{el("speed-value").textContent=el("speed").value+" steps/s";if(timer){stop();start();}});
  el("ruleset").addEventListener("change",paint);
  async function loadBiosphere() {
    el("world-status").innerHTML='<span class="spinner"></span> Loading biosphere data…';
    try {
      const response=await fetch("./data/biosphere.json",{headers:{Accept:"application/json"},cache:"no-store"});
      if(!response.ok) throw new Error("HTTP "+response.status);
      const data=await response.json();
      el("biome-count").textContent=data.biomes.length;
      el("biome-list").innerHTML=data.biomes.map(b=>'<div class="biome-row"><span>'+escapeHtml(b.name)+'</span><span>'+b.organisms+' ORGANISMS</span></div>').join("");
      el("world-status").textContent="DATA LOADED · "+data.organisms.length+" ORGANISM RECORDS";
      el("data-source-status").textContent="DATA SOURCE: JSON / FETCH AJAX";
    } catch(error) {
      el("world-status").textContent="Could not load biosphere JSON. Run this site from a web server, not file://.";
      el("data-source-status").textContent="DATA SOURCE: UNAVAILABLE";
    }
  }
  function escapeHtml(s){return String(s).replace(/[&<>"']/g,c=>({"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#39;"}[c]));}
  randomize();loadBiosphere();el("refresh-data").addEventListener("click",loadBiosphere);
})();