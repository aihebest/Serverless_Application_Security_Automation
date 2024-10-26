// src/services/authTokenManager.js
import { msalInstance } from '../config/authConfig';
import { trackEvent, trackException } from './telemetryService';

class AuthTokenManager {
  constructor() {
    this.tokenCache = new Map();
    this.tokenAcquisitions = new Map();
  }

  async getAccessToken(forceRefresh = false) {
    const scopes = [process.env.REACT_APP_API_SCOPE];
    const account = msalInstance.getAllAccounts()[0];

    if (!account) {
      trackException(new Error('No active account'), 'TokenAcquisition');
      await this.handleAuthenticationRequired();
      return this.getAccessToken();
    }

    try {
      // Check if we have a pending token acquisition
      const pendingAcquisition = this.tokenAcquisitions.get(account.homeAccountId);
      if (pendingAcquisition) {
        return pendingAcquisition;
      }

      // Get cached token if available and not forced refresh
      const cachedToken = this.tokenCache.get(account.homeAccountId);
      if (!forceRefresh && cachedToken && this.isTokenValid(cachedToken)) {
        return cachedToken.accessToken;
      }

      // Create new token acquisition promise
      const acquisitionPromise = this.acquireNewToken(account, scopes);
      this.tokenAcquisitions.set(account.homeAccountId, acquisitionPromise);

      const token = await acquisitionPromise;
      this.tokenAcquisitions.delete(account.homeAccountId);
      return token;

    } catch (error) {
      this.tokenAcquisitions.delete(account.homeAccountId);
      throw error;
    }
  }

  async acquireNewToken(account, scopes) {
    try {
      const response = await msalInstance.acquireTokenSilent({
        scopes,
        account,
        forceRefresh: true
      });

      this.tokenCache.set(account.homeAccountId, {
        accessToken: response.accessToken,
        expiresOn: response.expiresOn,
      });

      trackEvent('TokenAcquisitionSuccess');
      return response.accessToken;

    } catch (error) {
      trackException(error, 'TokenAcquisition');
      
      if (this.isInteractionRequired(error)) {
        await this.handleAuthenticationRequired();
        return this.getAccessToken();
      }
      
      throw error;
    }
  }

  isTokenValid(tokenInfo) {
    if (!tokenInfo.expiresOn) return false;
    // Add 5-minute buffer before expiration
    const expirationBuffer = 5 * 60 * 1000;
    return new Date(tokenInfo.expiresOn).getTime() - expirationBuffer > Date.now();
  }

  isInteractionRequired(error) {
    return (
      error.name === 'InteractionRequiredAuthError' ||
      error.name === 'BrowserAuthError' ||
      error.message.includes('interaction_required') ||
      error.message.includes('login_required')
    );
  }

  async handleAuthenticationRequired() {
    trackEvent('InteractionRequired');
    await msalInstance.acquireTokenRedirect({
      scopes: [process.env.REACT_APP_API_SCOPE],
      prompt: 'select_account'
    });
  }

  clearTokenCache() {
    this.tokenCache.clear();
    this.tokenAcquisitions.clear();
  }
}

export const authTokenManager = new AuthTokenManager();