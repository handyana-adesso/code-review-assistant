import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  base: "./",
  build: {
    outDir: "../CodeReviewAssistant/wwwroot", // build straight into the .NET app
    emptyOutDir: true,
  },
  server: {
    proxy: {
      "/api": "http://localhost:5099", // set to the http port `dotnet run` prints
    },
  },
});