import React from 'react';
import { useIsAuthenticated, useMsal } from '@azure/msal-react';

const AuthenticatedRoute = ({ children }) => {
    const isAuthenticated = useIsAuthenticated();
    const { instance } = useMsal();

    if (!isAuthenticated) {
        return (
            <div className="min-h-screen flex items-center justify-center bg-gray-50">
                <div className="text-center">
                    <h2 className="text-xl font-semibold mb-4">Please sign in</h2>
                    <button
                        onClick={() => instance.loginRedirect()}
                        className="bg-blue-600 text-white px-4 py-2 rounded-md text-sm hover:bg-blue-700"
                    >
                        Sign in with Azure AD
                    </button>
                </div>
            </div>
        );
    }

    return children;
};

export default AuthenticatedRoute;