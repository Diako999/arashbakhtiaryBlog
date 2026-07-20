import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// Single-deployable target: `npm run build` writes straight into the
// ASP.NET Core project's wwwroot, which Program.cs serves as static files
// with a SPA fallback to index.html. Dev mode instead proxies /api to the
// Kestrel process (see launchSettings.json) so `npm run dev` and `dotnet run`
// can run side by side without CORS.
export default defineConfig({
  plugins: [react(), tailwindcss()],
  build: {
    outDir: '../ArashBlog.Api/wwwroot',
    emptyOutDir: true,
  },
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5263',
        changeOrigin: true,
      },
    },
  },
})
