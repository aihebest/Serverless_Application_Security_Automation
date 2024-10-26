import axios from 'axios';
import { msalInstance } from '../config/authConfig';

class SecurityApiService {
    constructor() {
        this.api = axios.create({
            baseURL: process.env.REACT_APP_API_BASE_URL,
            headers: {
                'Content-Type': 'application/json'
            }
        });

        // Add request interceptor for auth token
        this.api.interceptors.request.use(async (config) => {
            const account = msalInstance.getAllAccounts()[0];
            const token = await msalInstance.acquireTokenSilent({
                scopes: ['User.Read'],
                account: account
            });
            config.headers.Authorization = `Bearer ${token.accessToken}`;
            return config;
        });
    }

    async getScanResults() {
        try {
            const response = await this.api.get('/GetScanResults');
            return response.data;
        } catch (error) {
            console.error('Error fetching scan results:', error);
            throw error;
        }
    }

    async getSecurityMetrics() {
        try {
            const response = await this.api.get('/GetSecurityMetrics');
            return response.data;
        } catch (error) {
            console.error('Error fetching security metrics:', error);
            throw error;
        }
    }

    async triggerScan() {
        try {
            const response = await this.api.post('/TriggerScan');
            return response.data;
        } catch (error) {
            console.error('Error triggering scan:', error);
            throw error;
        }
    }
}

export default new SecurityApiService();