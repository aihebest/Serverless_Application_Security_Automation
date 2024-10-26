// src/components/auth/AuthProvider.js
// Remove the React import since it's already declared
import { MsalProvider } from "@azure/msal-react";
import { msalInstance } from '../../config/authConfig';

const AuthProvider = ({ children }) => {
  return (
    <MsalProvider instance={msalInstance}>
      {children}
    </MsalProvider>
  );
};

export default AuthProvider;