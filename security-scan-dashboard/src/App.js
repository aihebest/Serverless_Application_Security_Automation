// src/App.js
import React from 'react';
import { BrowserRouter } from 'react-router-dom';
import { FluentProvider, webLightTheme } from '@fluentui/react-components';
import AuthProvider from './components/auth/AuthProvider';
// Fix the import path
import Dashboard from './components/security/Dashboard';

function App() {
  return (
    <FluentProvider theme={webLightTheme}>
      <BrowserRouter>
        <AuthProvider>
          <Dashboard />
        </AuthProvider>
      </BrowserRouter>
    </FluentProvider>
  );
}

export default App;