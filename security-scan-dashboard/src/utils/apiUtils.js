// src/utils/apiUtils.js
import axios from 'axios';
import { trackException } from '../services/telemetryService';

export class RetryConfig {
  constructor(maxRetries = 3, baseDelay = 1000, maxDelay = 5000) {
    this.maxRetries = maxRetries;
    this.baseDelay = baseDelay;
    this.maxDelay = maxDelay;
  }

  getDelay(retryCount) {
    // Exponential backoff with jitter
    const exponentialDelay = Math.min(
      this.maxDelay,
      this.baseDelay * Math.pow(2, retryCount)
    );
    return exponentialDelay + Math.random() * 1000;
  }
}

export class ApiError extends Error {
  constructor(message, originalError, context = {}) {
    super(message);
    this.name = 'ApiError';
    this.originalError = originalError;
    this.status = originalError?.response?.status;
    this.context = context;
    this.timestamp = new Date().toISOString();
  }

  static isRetryable(error) {
    const status = error?.response?.status;
    return (
      !status || // Network errors
      status === 408 || // Request Timeout
      status === 429 || // Too Many Requests
      (status >= 500 && status <= 599) // Server errors
    );
  }
}

export const withRetry = async (operation, retryConfig = new RetryConfig()) => {
  let lastError;
  
  for (let retryCount = 0; retryCount <= retryConfig.maxRetries; retryCount++) {
    try {
      return await operation();
    } catch (error) {
      lastError = error;
      
      if (!ApiError.isRetryable(error) || retryCount === retryConfig.maxRetries) {
        throw error;
      }

      const delay = retryConfig.getDelay(retryCount);
      trackException(error, 'RetryableApiError', {
        retryCount,
        delay,
        endpoint: error.config?.url
      });

      await new Promise(resolve => setTimeout(resolve, delay));
    }
  }

  throw lastError;
};

export const createApiErrorHandler = (context) => {
  return (error) => {
    const apiError = new ApiError(
      error.message,
      error,
      { ...context, endpoint: error.config?.url }
    );
    
    trackException(apiError, 'ApiError', apiError.context);
    
    if (error.response?.status === 401) {
      // Handle authentication errors
      return Promise.reject(new ApiError('Authentication required', error, context));
    }
    
    return Promise.reject(apiError);
  };
};