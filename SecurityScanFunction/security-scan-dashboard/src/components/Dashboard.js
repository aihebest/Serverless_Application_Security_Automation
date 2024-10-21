import React, { useState, useEffect } from 'react';
import { useMsal } from "@azure/msal-react";
import { Button, DataGrid, DataGridBody, DataGridRow, DataGridCell } from '@fluentui/react-components';
import axios from 'axios';

const Dashboard = () => {
  const { instance, accounts } = useMsal();
  const [scanResults, setScanResults] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    fetchScanResults();
  }, []);

  const fetchScanResults = async () => {
    setLoading(true);
    try {
      const token = await instance.acquireTokenSilent({
        scopes: ["api://your-api-client-id/user_impersonation"],
        account: accounts[0]
      });

      const response = await axios.get('your-function-url/api/GetScanResults', {
        headers: { 'Authorization': `Bearer ${token.accessToken}` }
      });
      setScanResults(response.data);
    } catch (error) {
      console.error('Error fetching scan results:', error);
    }
    setLoading(false);
  };

  const triggerScan = async () => {
    try {
      const token = await instance.acquireTokenSilent({
        scopes: ["api://your-api-client-id/user_impersonation"],
        account: accounts[0]
      });

      await axios.post('your-function-url/api/SecurityScanTrigger', {}, {
        headers: { 'Authorization': `Bearer ${token.accessToken}` }
      });
      alert('Scan triggered successfully');
      fetchScanResults();
    } catch (error) {
      console.error('Error triggering scan:', error);
      alert('Error triggering scan');
    }
  };

  return (
    <div>
      <h1>Security Scan Dashboard</h1>
      <Button onClick={triggerScan}>Trigger New Scan</Button>
      <Button onClick={fetchScanResults}>Refresh Results</Button>
      {loading ? (
        <p>Loading...</p>
      ) : (
        <DataGrid>
          <DataGridBody>
            {scanResults.map((result, index) => (
              <DataGridRow key={index}>
                <DataGridCell>{result.id}</DataGridCell>
                <DataGridCell>{result.resourceName}</DataGridCell>
                <DataGridCell>{result.scanTime}</DataGridCell>
                <DataGridCell>{result.severityCounts.High}</DataGridCell>
                <DataGridCell>{result.severityCounts.Medium}</DataGridCell>
                <DataGridCell>{result.severityCounts.Low}</DataGridCell>
              </DataGridRow>
            ))}
          </DataGridBody>
        </DataGrid>
      )}
    </div>
  );
};

export default Dashboard;