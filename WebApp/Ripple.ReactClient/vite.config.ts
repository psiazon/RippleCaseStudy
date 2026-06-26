import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    host: 'localhost',
    port: 7001
  },
  preview: {
    host: 'localhost',
    port: 7001
  }
});
