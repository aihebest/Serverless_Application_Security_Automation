# Serverless Security Automation for Cosmos DB

This project implements automated security scans for Azure Cosmos DB accounts using Azure Functions.

## Setup

1. Clone this repository
2. Set up Azure resources:
   - Azure Function App
   - Azure Cosmos DB account
3. Configure Function App settings:
   - COSMOS_DB_CONNECTION_STRING
   - AZURE_SUBSCRIPTION_ID
4. Deploy the functions to your Azure Function App

## Usage

### On-demand scan
Endpoint: `https://your-function-app.azurewebsites.net/api/SecurityScanTrigger`
Method: GET or POST
Query parameter: `resourceId` (Cosmos DB account resource ID)

### Automated daily scan
The `AutomatedScanTimerTrigger` function runs daily at midnight UTC.

## Security

This application uses Azure AD authentication. Ensure you have proper access before using the API.

## Maintenance

- Regularly update dependencies
- Review and enhance security checks periodically
