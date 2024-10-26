import React, { useState, useEffect, useCallback } from 'react';
import { useMsal } from "@azure/msal-react";
import { 
  Card, 
  Text, 
  Button, 
  DataGrid, 
  DataGridBody, 
  DataGridRow, 
  DataGridCell,
  ProgressBar,
  Badge
} from '@fluentui/react-components';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import axios from 'axios';

// Security Score Component
const SecurityScore = ({ score }) => (
  <Card className="p-4">
    <Text size="large" weight="semibold">Security Score</Text>
    <div className="mt-4">
      <ProgressBar
        value={score}
        max={100}
        style={{
          backgroundColor: score > 80 ? '#4CAF50' : score > 60 ? '#FFA726' : '#F44336'
        }}
      />
      <Text>{score}%</Text>
    </div>
  </Card>
);

// Security Metrics Component
const SecurityMetrics = ({ metrics }) => (
  <Card className="p-4">
    <Text size="large" weight="semibold">Security Issues</Text>
    <div className="grid grid-cols-3 gap-4 mt-4">
      <div>
        <Badge color="danger">Critical</Badge>
        <Text size="large">{metrics.critical || 0}</Text>
      </div>
      <div>
        <Badge color="warning">High</Badge>
        <Text size="large">{metrics.high || 0}</Text>
      </div>
      <div>
        <Badge color="info">Medium</Badge>
        <Text size="large">{metrics.medium || 0}</Text>
      </div>
    </div>
  </Card>
);

// Compliance Status Component
const ComplianceStatus = ({ checks }) => (
  <Card className="p-4">
    <Text size="large" weight="semibold">Compliance Status</Text>
    <div className="mt-4">
      {checks.map((check, index) => (
        <div key={index} className="flex items-center justify-between mb-2">
          <Text>{check.category}</Text>
          <Badge 
            color={check.status ? 'success' : 'danger'}
          >
            {check.status ? 'Compliant' : 'Non-Compliant'}
          </Badge>
        </div>
      ))}
    </div>
  </Card>
);

// Main Dashboard Component
const Dashboard = () => {
  const { instance, accounts } = useMsal();
  const [scanResults, setScanResults] = useState([]);
  const [loading, setLoading] = useState(false);
  const [securityMetrics, setSecurityMetrics] = useState({
    score: 0,
    critical: 0,
    high: 0,
    medium: 0,
    trend: []
  });
  const [complianceChecks, setComplianceChecks] = useState([]);

  // Fetch security metrics
  const fetchSecurityMetrics = useCallback(async () => {
    try {
      const token = await instance.acquireTokenSilent({
        scopes: ["api://your-api-client-id/user_impersonation"],
        account: accounts[0]
      });

      const response = await axios.get('your-function-url/api/GetSecurityMetrics', {
        headers: { 'Authorization': `Bearer ${token.accessToken}` }
      });

      setSecurityMetrics(response.data);
    } catch (error) {
      console.error('Error fetching security metrics:', error);
    }
  }, [instance, accounts]);

  // Fetch scan results
  const fetchScanResults = useCallback(async () => {
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
      
      // Update compliance checks from scan results
      if (response.data.length > 0) {
        setComplianceChecks(response.data[0].complianceChecks || []);
      }
    } catch (error) {
      console.error('Error fetching scan results:', error);
    }
    setLoading(false);
  }, [instance, accounts]);

  // Trigger security scan
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
      // Refresh data
      await Promise.all([fetchSecurityMetrics(), fetchScanResults()]);
    } catch (error) {
      console.error('Error triggering scan:', error);
      alert('Error triggering scan');
    }
  };

  // Initialize and refresh data
  useEffect(() => {
    fetchSecurityMetrics();
    fetchScanResults();
  }, [fetchSecurityMetrics, fetchScanResults]);

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Security Scan Dashboard</h1>
        <div className="space-x-4">
          <Button onClick={triggerScan}>Trigger New Scan</Button>
          <Button onClick={() => {
            fetchSecurityMetrics();
            fetchScanResults();
          }}>Refresh Data</Button>
        </div>
      </div>

      {/* Metrics Overview */}
      <div className="grid grid-cols-3 gap-6 mb-6">
        <SecurityScore score={securityMetrics.score} />
        <SecurityMetrics metrics={securityMetrics} />
        <ComplianceStatus checks={complianceChecks} />
      </div>

      {/* Security Score Trend */}
      <Card className="mb-6 p-4">
        <Text size="large" weight="semibold" className="mb-4">Security Score Trend</Text>
        <div style={{ height: '300px' }}>
          <ResponsiveContainer>
            <LineChart data={securityMetrics.trend}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="date" />
              <YAxis domain={[0, 100]} />
              <Tooltip />
              <Line type="monotone" dataKey="score" stroke="#0078D4" />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </Card>

      {/* Scan Results Table */}
      <Card className="p-4">
        <Text size="large" weight="semibold" className="mb-4">Recent Scan Results</Text>
        {loading ? (
          <Text>Loading scan results...</Text>
        ) : (
          <DataGrid>
            <DataGridBody>
              {scanResults.map((result, index) => (
                <DataGridRow key={index}>
                  <DataGridCell>{result.id}</DataGridCell>
                  <DataGridCell>{result.resourceName}</DataGridCell>
                  <DataGridCell>{new Date(result.scanTime).toLocaleString()}</DataGridCell>
                  <DataGridCell>
                    <Badge color="danger">{result.severityCounts?.High || 0}</Badge>
                  </DataGridCell>
                  <DataGridCell>
                    <Badge color="warning">{result.severityCounts?.Medium || 0}</Badge>
                  </DataGridCell>
                  <DataGridCell>
                    <Badge color="info">{result.severityCounts?.Low || 0}</Badge>
                  </DataGridCell>
                </DataGridRow>
              ))}
            </DataGridBody>
          </DataGrid>
        )}
      </Card>
    </div>
  );
};

export default Dashboard;