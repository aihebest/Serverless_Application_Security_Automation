import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import { FluentProvider, webLightTheme } from '@fluentui/react-components';
import App from './App';
import { initializeTelemetry } from './services/telemetryService';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <FluentProvider theme={webLightTheme}>
      <App />
    </FluentProvider>
  </React.StrictMode>
);