import { useIsAuthenticated, useMsal } from '@azure/msal-react';
import React from 'react';

const ProtectedRoute = ({ children }) => {
    const isAuthenticated = useIsAuthenticated();
    const { instance } = useMsal();

    if (!isAuthenticated) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-gray-50">
                <button
                    className="bg-blue-600 text-white px-4 py-2 rounded-md text-sm hover:bg-blue-700"
                    onClick={() => instance.loginRedirect()}
                >
                    Sign in to access dashboard
                </button>
            </div>
        );
    }

    return children;
};

export default ProtectedRoute;