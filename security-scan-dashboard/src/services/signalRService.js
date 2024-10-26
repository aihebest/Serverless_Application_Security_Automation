// src/services/signalRService.js
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { ApplicationInsights } from '@microsoft/applicationinsights-web';

const appInsights = new ApplicationInsights({
  config: {
    connectionString: process.env.REACT_APP_APPINSIGHTS_CONNECTION_STRING,
    enableAutoRouteTracking: true
  }
});

class SignalRService {
  constructor() {
    this.connection = null;
  }

  async createConnection() {
    try {
      this.connection = new HubConnectionBuilder()
        .withUrl(`${process.env.REACT_APP_API_BASE_URL}/securityHub`)
        .configureLogging(LogLevel.Information)
        .withAutomaticReconnect()
        .build();

      await this.connection.start();
      appInsights.trackEvent({ name: 'SignalRConnected' });
      console.log('SignalR Connected');
      
      return this.connection;
    } catch (err) {
      appInsights.trackException({ exception: err });
      console.error('SignalR Connection Error: ', err);
      throw err;
    }
  }

  getConnection() {
    return this.connection;
  }

  async stopConnection() {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }
}

export default new SignalRService();