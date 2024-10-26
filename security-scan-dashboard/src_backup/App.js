import React from 'react';
import SecurityDashboard from './components/security/SecurityDashboard';
import AuthProvider from './components/auth/AuthProvider';
import ProtectedRoute from './components/auth/ProtectedRoute';

function App() {
    return (
        <AuthProvider>
            <ProtectedRoute>
                <SecurityDashboard />
            </ProtectedRoute>
        </AuthProvider>
    );
}

export default App;