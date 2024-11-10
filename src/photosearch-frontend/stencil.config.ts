import { Config } from '@stencil/core';
import tailwindConfig from './tailwind.config';
import tailwind, { tailwindHMR } from 'stencil-tailwind-plugin';
import nodePolyfills from 'rollup-plugin-node-polyfills';

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
  // rollupPlugins: {
  //   after: [
  //     nodePolyfills(),
  //   ]
  // },
  plugins: [
    // babel({
    //   plugins: ['transform-class-properties', 'transform-object-rest-spread', '@babel/plugin-transform-runtime'],
    //   include: ['node_modules/zone.js/**'],
    // }),
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
    MAP_TILE_SERVER: process.env.ConnectionStrings__OSMMapTileServer,
    OTEL_EXPORTER_OTLP_ENDPOINT: process.env.OTEL_EXPORTER_OTLP_ENDPOINT,
    OTEL_EXPORTER_OTLP_HEADERS: process.env.OTEL_EXPORTER_OTLP_HEADERS,
    OTEL_RESOURCE_ATTRIBUTES: process.env.OTEL_RESOURCE_ATTRIBUTES,
    OTEL_SERVICE_NAME: process.env.OTEL_SERVICE_NAME
  }
};
 
 
