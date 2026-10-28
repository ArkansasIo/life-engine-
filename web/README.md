# LifeEngine Web Edition 2.0.0

A thematic, static HTML/CSS/TypeScript browser adaptation of the LifeEngine cellular automata and biosphere concept.

## Features
- Responsive dark ecological research-lab interface.
- Cellular automata canvas with Conway's Life, HighLife, Seeds and Day & Night rule profiles.
- Run/pause, single-step, randomize, clear, adjustable speed and click-to-toggle cells.
- JSON biosphere dataset loaded asynchronously with browser Fetch API (AJAX).
- No runtime server, database, login, API key or third-party JavaScript framework required.

## Run locally
Serve this directory over HTTP (the JSON Fetch request may be blocked when opening `index.html` directly via `file://`).

With Python installed:
```sh
python -m http.server 8080 --directory web
```
Then open http://localhost:8080.

## TypeScript build
From the repository root, run:
```sh
npm install
npm run build:web
```
The TypeScript source is `web/src/main.ts`; the current static runtime is `web/app.js`. The repository build command type-checks and emits the TypeScript output to `web/dist/main.js` to avoid overwriting the hand-maintained runtime file.

## Static deployment
The `web/` directory can be served by GitHub Pages, any static host, or a CDN. If the repository Pages workflow currently publishes `docs/`, point the workflow's upload path to `web/` or copy the web files into its published directory before expecting the new interface to appear on the live site. This commit alone does not prove that a Pages deployment has completed.
