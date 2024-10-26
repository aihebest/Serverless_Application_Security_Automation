// src/services/securityApi.js
import axios from 'axios';
import { msalInstance } from '../config/authConfig';
import { trackEvent, trackException } from './telemetryService';

class SecurityApiService {
  constructor() {
    this.baseURL = process.env.REACT_APP_API_BASE_URL;
    this.api = axios.create({
      baseURL: this.baseURL,
      headers: {
        'Content-Type': 'application/json'
      }
    });

    // Add request interceptor for auth and telemetry
    this.api.interceptors.request.use(async (config) => {
      try {
        const token = await this.getAuthToken();
        config.headers.Authorization = `Bearer ${token}`;
        trackEvent('ApiRequest', { endpoint: config.url });
        return config;
      } catch (error) {
        trackException(error, 'RequestInterceptor');
        return Promise.reject(error);
      }
    });

    // Add response interceptor for error handling
    this.api.interceptors.response.use(
      (response) => {
        trackEvent('ApiSuccess', { endpoint: response.config.url });
        return response;
      },
      (error) => {
        this.handleApiError(error);
        return Promise.reject(error);
      }
    );
  }

  async getAuthToken() {
    try {
      const account = msalInstance.getAllAccounts()[0];
      if (!account) throw new Error('No active account');

      const response = await msalInstance.acquireTokenSilent({
        scopes: [process.env.REACT_APP_API_SCOPE],
        account: account
      });
      
      return response.accessToken;
    } catch (error) {
      trackException(error, 'TokenAcquisition');
      throw new Error('Authentication failed');
    }
  }

  handleApiError(error) {
    const errorDetails = {
      endpoint: error.config?.url,
      status: error.response?.status,
      message: error.message
    };

    trackException(error, 'ApiError', errorDetails);

    if (error.response?.status === 401) {
      msalInstance.acquireTokenRedirect({
        scopes: [process.env.REACT_APP_API_SCOPE]
      });
    }
  }

  async getSecurityMetrics() {
    try {
      const response = await this.api.get('/api/security/metrics');
      return response.data;
    } catch (error) {
      throw this.formatError('Failed to fetch security metrics', error);
    }
  }

  async getScanResults(scanId = null) {
    try {
      const endpoint = scanId 
        ? `/api/security/scan-results/${scanId}`
        : '/api/security/scan-results';
      const response = await this.api.get(endpoint);
      return response.data;
    } catch (error) {
      throw this.formatError('Failed to fetch scan results', error);
    }
  }

  async triggerScan() {
    try {
      const response = await this.api.post('/api/security/scan');
      return response.data;
    } catch (error) {
      throw this.formatError('Failed to trigger security scan', error);
    }
  }

  async getSecurityTrend(days = 30) {
    try {
      const response = await this.api.get(`/api/security/trend?days=${days}`);
      return response.data;
    } catch (error) {
      throw this.formatError('Failed to fetch security trend', error);
    }
  }

  async exportReport(format, days) {
    try {
      const response = await this.api.get(
        `/api/security/export?format=${format}&days=${days}`,
        { responseType: 'blob' }
      );
      
      const fileName = this.getExportFileName(format);
      this.downloadFile(response.data, fileName, format);
      trackEvent('ExportSuccess', { format, days });
    } catch (error) {
      throw this.formatError('Failed to export report', error);
    }
  }

  formatError(message, error) {
    return {
      message,
      details: error.response?.data?.message || error.message,
      status: error.response?.status,
      timestamp: new Date().toISOString()
    };
  }

  getExportFileName(format) {
    const date = new Date().toISOString().split('T')[0];
    return `security-report-${date}.${format}`;
  }

  downloadFile(data, fileName, format) {
    const blob = new Blob([data], { type: this.getContentType(format) });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }

  getContentType(format) {
    const contentTypes = {
      csv: 'text/csv',
      pdf: 'application/pdf',
      excel: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      json: 'application/json'
    };
    return contentTypes[format] || 'application/octet-stream';
  }
}

const securityApiService = new SecurityApiService();
export default securityApiService;