// src/components/security/Dashboard.js
import React from 'react';
import {
  Button,
  Card,
  Text,
  Spinner
} from "@fluentui/react-components";
import { 
  ArrowRotateClockwise24Regular, 
  ShieldCheckmark24Regular,
  ArrowDownload24Regular 
} from "@fluentui/react-icons";
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { useSecurityData } from '../../hooks/useSecurityData';

const MetricCard = ({ title, value, color }) => (
  <Card className="p-4">
    <div>
      <Text className="text-sm text-gray-600">{title}</Text>
      <Text 
        as="div" 
        className="text-3xl font-semibold mt-2"
        style={{ color }}
      >
        {value}
      </Text>
    </div>
  </Card>
);

const Dashboard = () => {
  const { 
    metrics, 
    loading, 
    error, 
    scanStatus,
    refreshData, 
    triggerScan, 
    exportData 
  } = useSecurityData();

  const isScanning = scanStatus === 'running' || scanStatus === 'starting';

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <Text as="h1" size={800} weight="bold">
          Security Dashboard
        </Text>
        <div className="flex gap-2">
          <Button
            appearance="subtle"
            icon={<ArrowRotateClockwise24Regular />}
            onClick={refreshData}
            disabled={loading || isScanning}
          >
            Refresh
          </Button>
          <Button
            appearance="primary"
            icon={<ShieldCheckmark24Regular />}
            onClick={triggerScan}
            disabled={loading || isScanning}
          >
            {isScanning ? 'Scanning...' : 'Trigger Scan'}
          </Button>
          <Button
            appearance="secondary"
            icon={<ArrowDownload24Regular />}
            onClick={() => exportData('json')}
            disabled={loading || isScanning}
          >
            Export
          </Button>
        </div>
      </div>

      {error && (
        <Card className="mb-4 bg-red-50">
          <Text className="text-red-600 p-4">{error}</Text>
        </Card>
      )}

      {loading ? (
        <div className="flex justify-center p-8">
          <Spinner label="Loading security data..." />
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
            <MetricCard 
              title="Security Score" 
              value={`${metrics.score}%`}
              color="#0078D4"
            />
            <MetricCard 
              title="Critical Issues" 
              value={metrics.critical}
              color="#d13438"
            />
            <MetricCard 
              title="High Priority" 
              value={metrics.high}
              color="#ffa500"
            />
          </div>

          <Card>
            <div className="p-4">
              <Text size="large" weight="semibold">Security Trend</Text>
              <div style={{ height: '400px' }} className="mt-4">
                <ResponsiveContainer>
                  <LineChart data={metrics.trend}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                    <XAxis 
                      dataKey="date"
                      tickFormatter={(value) => new Date(value).toLocaleDateString()}
                      stroke="#6b7280"
                    />
                    <YAxis domain={[0, 100]} stroke="#6b7280" />
                    <Tooltip
                      formatter={(value) => [`${value}%`, 'Security Score']}
                      labelFormatter={(label) => new Date(label).toLocaleDateString()}
                    />
                    <Line 
                      type="monotone" 
                      dataKey="score" 
                      stroke="#0078D4" 
                      strokeWidth={2}
                      dot={{ r: 4, fill: '#0078D4' }}
                      activeDot={{ r: 6, fill: '#0078D4' }}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </div>
            </div>
          </Card>
        </>
      )}
    </div>
  );
};

export default Dashboard;