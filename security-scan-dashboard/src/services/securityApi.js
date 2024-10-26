// src/services/securityApi.js
import axios from 'axios';
import { authTokenManager } from './authTokenManager';
import { withRetry, RetryConfig, createApiErrorHandler } from '../utils/apiUtils';
import { trackEvent, trackException } from './telemetryService';

class SecurityApiService {
  constructor() {
    this.baseURL = process.env.REACT_APP_API_BASE_URL;
    this.retryConfig = new RetryConfig(3, 1000, 5000);
    
    this.api = axios.create({
      baseURL: this.baseURL,
      headers: {
        'Content-Type': 'application/json'
      }
    });

    this.setupInterceptors();
  }

  setupInterceptors() {
    this.api.interceptors.request.use(
      async (config) => {
        const token = await authTokenManager.getAccessToken();
        config.headers.Authorization = `Bearer ${token}`;
        trackEvent('ApiRequest', { endpoint: config.url });
        return config;
      },
      error => {
        trackException(error, 'RequestInterceptor');
        return Promise.reject(error);
      }
    );

    this.api.interceptors.response.use(
      response => {
        trackEvent('ApiSuccess', { endpoint: response.config.url });
        return response;
      },
      async error => {
        if (error.response?.status === 401) {
          try {
            // Try to get a new token
            const token = await authTokenManager.getAccessToken(true);
            error.config.headers.Authorization = `Bearer ${token}`;
            return this.api.request(error.config);
          } catch (refreshError) {
            return Promise.reject(refreshError);
          }
        }
        return Promise.reject(error);
      }
    );
  }

  async getSecurityMetrics() {
    return withRetry(
      async () => {
        const response = await this.api.get('/api/security/metrics');
        return response.data;
      },
      this.retryConfig
    ).catch(createApiErrorHandler({ context: 'getSecurityMetrics' }));
  }

  async getScanResults(scanId = null) {
    return withRetry(
      async () => {
        const endpoint = scanId 
          ? `/api/security/scan-results/${scanId}`
          : '/api/security/scan-results';
        const response = await this.api.get(endpoint);
        return response.data;
      },
      this.retryConfig
    ).catch(createApiErrorHandler({ context: 'getScanResults', scanId }));
  }

  async triggerScan() {
    return withRetry(
      async () => {
        const response = await this.api.post('/api/security/scan');
        return response.data;
      },
      this.retryConfig
    ).catch(createApiErrorHandler({ context: 'triggerScan' }));
  }

  // ... rest of your existing methods with similar retry and error handling
}

const securityApiService = new SecurityApiService();
export default securityApiService;