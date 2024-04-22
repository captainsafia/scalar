import { findEntryPoints } from '@scalar/build-tooling'
import vue from '@vitejs/plugin-vue'
import { URL, fileURLToPath } from 'node:url'
import { defineConfig } from 'vite'
import svgLoader from 'vite-svg-loader'

import pkg from './package.json'

export default defineConfig({
  plugins: [vue(), (svgLoader as any)()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
      '@test': fileURLToPath(new URL('./test', import.meta.url)),
    },
    dedupe: ['vue'],
  },
  server: {
    open: './test/index.html',
    port: 9000,
  },
  build: {
    ssr: true,
    minify: false,
    target: 'esnext',
    lib: {
      entry: await findEntryPoints({ allowCss: true }),
      formats: ['es'],
    },
    rollupOptions: {
      external: [...Object.keys((pkg as any).peerDependencies || {})],
      output: {
        // Create a separate file for the dependency bundle
        manualChunks: (id) =>
          id.includes('node_modules') ? 'vendor' : undefined,
      },
    },
  },
})
