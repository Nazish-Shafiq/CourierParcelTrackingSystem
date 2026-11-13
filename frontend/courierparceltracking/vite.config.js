import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, '.', '')
  const apiProxyTarget = env.VITE_API_PROXY_TARGET || 'https://localhost:7075'
  const proxyUrl = new URL(apiProxyTarget)
  const isLocalHttpsTarget = proxyUrl.protocol === 'https:' &&
    ['localhost', '127.0.0.1', '[::1]'].includes(proxyUrl.hostname)

  return {
    plugins: [react()],
    server: {
      proxy: {
        '/api': {
          target: apiProxyTarget,
          changeOrigin: true,
          secure: !isLocalHttpsTarget,
        },
      },
    },
  }
})
