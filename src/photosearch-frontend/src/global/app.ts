import {
  ConsoleSpanExporter,
  SimpleSpanProcessor,
} from '@opentelemetry/sdk-trace-base';
import { StackContextManager, WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { DocumentLoadInstrumentation } from '@opentelemetry/instrumentation-document-load';
import { registerInstrumentations } from '@opentelemetry/instrumentation';
import { FetchInstrumentation } from '@opentelemetry/instrumentation-fetch';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-proto';
import { UserInteractionInstrumentation } from '@opentelemetry/instrumentation-user-interaction';
import { LongTaskInstrumentation } from '@opentelemetry/instrumentation-long-task';

import { getWebAutoInstrumentations } from '@opentelemetry/auto-instrumentations-web';
import { XMLHttpRequestInstrumentation } from '@opentelemetry/instrumentation-xml-http-request';

import { Resource } from '@opentelemetry/resources';
import { Env } from '@stencil/core';
//https://www.honeycomb.io/blog/opentelemetry-browser-instrumentation
//https://github.com/open-telemetry/opentelemetry-js/tree/main/experimental/packages/opentelemetry-instrumentation-xml-http-request
//https://github.com/open-telemetry/opentelemetry-js-contrib/tree/main/plugins/web/opentelemetry-instrumentation-user-interaction
//https://github.com/open-telemetry/opentelemetry-js-contrib/tree/main/plugins/web/opentelemetry-instrumentation-long-task
//https://github.com/open-telemetry/opentelemetry-js-contrib/tree/main/plugins/web/opentelemetry-instrumentation-long-task

//OTEL_EXPORTER_OTLP_ENDPOINT
//OTEL_EXPORTER_OTLP_HEADERS
//OTEL_EXPORTER_OTLP_PROTOCOL grpc
//OTEL_EXPORTER_OTLP_PROTOCOL
//var attributes = Environment.GetEnvironmentVariable("OTEL_RESOURCE_ATTRIBUTES");
export default async () => {
  /**
   * The code to be executed should be placed within a default function that is
   * exported by the global script. Ensure all of the code in the global script
   * is wrapped in the function() that is exported.
   */
  const otlpOptions = {
    url: `${Env.OTEL_EXPORTER_OTLP_ENDPOINT}/v1/traces`,
    headers: parseDelimitedValues(Env.OTEL_EXPORTER_OTLP_HEADERS)
  };
  const attributes = parseDelimitedValues(Env.OTEL_RESOURCE_ATTRIBUTES);
  attributes["service.name"] = Env.OTEL_SERVICE_NAME;
  const provider = new WebTracerProvider({
    resource: new Resource(attributes),
  });

  // provider.addSpanProcessor(new SimpleSpanProcessor(new ConsoleSpanExporter()));
  provider.addSpanProcessor(new SimpleSpanProcessor(new OTLPTraceExporter(otlpOptions)));

  provider.register({
    contextManager: new StackContextManager(),
  });

  // Registering instrumentations
  registerInstrumentations({
    instrumentations: [
      // new DocumentLoadInstrumentation(),

    getWebAutoInstrumentations({      // load custom configuration for xml-http-request instrumentation
      '@opentelemetry/instrumentation-xml-http-request': {
        clearTimingResources: true,
      }}),
      new LongTaskInstrumentation({
        observerCallback: (span, longtaskEvent) => {
          span.setAttribute('location.pathname', window.location.pathname)
        }

      }),
      // new UserInteractionInstrumentation(),
      // new XMLHttpRequestInstrumentation({
      //   propagateTraceHeaderCorsUrls: [new RegExp('\\/api\\/*')], // Propagate trace header to all URLs
      // }),
      new FetchInstrumentation({
        propagateTraceHeaderCorsUrls: [new RegExp('\\/api\\/*')], // Propagate trace header to all URLs
        ignoreUrls: [new RegExp('\\/tile\\/*')], // Ignore all URLs
      })],
  });

  function parseDelimitedValues(s) {
    const headers = s.split(','); // Split by comma
    const result = {};

    headers.forEach(header => {
      const [key, value] = header.split('='); // Split by equal sign
      result[key.trim()] = value.trim(); // Add to the object, trimming spaces
    });


    return result;
  }
};
