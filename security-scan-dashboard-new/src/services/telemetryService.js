// src/services/telemetryService.js
import { ApplicationInsights } from '@microsoft/applicationinsights-web';

const appInsights = new ApplicationInsights({
  config: {
    connectionString: process.env.REACT_APP_APPINSIGHTS_CONNECTION_STRING,
    enableAutoRouteTracking: true,
    enableCorsCorrelation: true,
    enableRequestHeaderTracking: true,
    enableResponseHeaderTracking: true,
  }
});

export const initializeTelemetry = () => {
  appInsights.loadAppInsights();
  appInsights.trackPageView();
};

export const trackEvent = (name, properties) => {
  appInsights.trackEvent({ name, properties });
};

export const trackException = (error, severityLevel) => {
  appInsights.trackException({ error, severityLevel });
};

export default appInsights;