# react-frontend — UI

[![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=black)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Vite](https://img.shields.io/badge/Vite-8-646CFF?logo=vite&logoColor=white)](https://vitejs.dev/)
[![Node.js](https://img.shields.io/badge/Node.js-LTS-339933?logo=nodedotjs&logoColor=white)](https://nodejs.org/)
[![ESLint](https://img.shields.io/badge/ESLint-9-4B32C3?logo=eslint&logoColor=white)](https://eslint.org/)

The **React + Vite (TypeScript)** single-screen UI for the Code Review Assistant.
You paste code, click **Run review**, and it renders the review returned by the
backend. It calls only **your** endpoint, `/api/review`.

> This is the frontend of the [Code Review Assistant](../README.md). See the root
> README for the full architecture, diagrams, and demo script.

---

## Requirements

| Tool | Why | Get it |
|------|-----|--------|
| **Node.js (LTS)** | builds & runs the frontend | https://nodejs.org |

---

## Scripts

```bash
npm install      # first time only
npm run dev      # start the Vite dev server (http://localhost:5173)
npm run build    # type-check + build into ../CodeReviewAssistant/wwwroot
npm run preview  # preview the production build locally
npm run lint     # run ESLint
```

---

## Two ways to run

### Build into the backend (single process)

```bash
npm install
npm run build     # outputs into ../CodeReviewAssistant/wwwroot
```

Then run the backend (`dotnet run`) — it serves the UI and the API from one origin.
No CORS, no dev server.

### Develop with hot reload

```bash
npm run dev       # open the Vite URL (:5173)
```

Vite proxies `/api` to the backend, so there's **no CORS**. Make sure the backend
is running (`dotnet run`) and the proxy target matches its http port.

> `npm run build` empties `wwwroot` (`emptyOutDir: true`).

---

## Configuration

The dev proxy is set in [vite.config.ts](vite.config.ts):

```ts
server: {
  proxy: {
    "/api": "http://localhost:5099", // set to the http port `dotnet run` prints
  },
},
```

The build output directory is also set there (`build.outDir`), pointing straight
into the .NET app's `wwwroot`.

---

## Project structure

```
react-frontend/
├─ index.html            # Vite entry HTML
├─ src/
│  ├─ App.tsx            # the single-screen app: code in → review out
│  ├─ main.tsx           # React root
│  ├─ App.css
│  └─ index.css
├─ vite.config.ts        # build output + dev proxy
├─ eslint.config.js      # ESLint flat config
└─ tsconfig*.json        # TypeScript configuration
```
