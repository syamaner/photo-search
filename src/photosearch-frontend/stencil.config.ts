import { Config } from '@stencil/core';
import tailwindConfig from './tailwind.config';
import tailwind, { tailwindHMR } from 'stencil-tailwind-plugin';

import { sass } from '@stencil/sass';
// https://stenciljs.com/docs/config

export const config: Config = {
  globalStyle: 'src/global/app.css',
  globalScript: 'src/global/app.ts',
  taskQueue: 'async',
  outputTargets: [
    {
      type: 'www',
      // comment the following line to disable service workers in production
      serviceWorker: null,
      baseUrl: 'https://myapp.local/',
    },
  ],
  plugins: [
    sass(),
    tailwind({
      tailwindConf: tailwindConfig,
      tailwindCssPath: './src/styles/tailwind.css'
    }),
    tailwindHMR()
  ],
  devServer: {
    reloadStrategy: 'pageReload'
  },
  env: {
    API_BASE_URL: process.env.services__apiservice__http__0,
    MAP_TILE_SERVER: process.env.ConnectionStrings__OSMMapTileServer
  }
};
 
