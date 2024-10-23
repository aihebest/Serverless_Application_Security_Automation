// Create new file: src/components/security/SecurityDashboard.js
import React from 'react';
import { Card, Text, ProgressBar } from '@fluentui/react-components';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip } from 'recharts';

// Security Score Trend Component
export const SecurityScoreTrend = ({ data }) => (
  <Card className="p-4 mb-4">
    <Text size="large" weight="semibold">Security Score Trend</Text>
    <div className="mt-4" style={{ height: '200px' }}>
      <LineChart width={500} height={200} data={data}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="date" />
        <YAxis domain={[0, 100]} />
        <Tooltip />
        <Line type="monotone" dataKey="score" stroke="#0078D4" />
      </LineChart>
    </div>
  </Card>
);

// Vulnerability Summary Component
export const VulnerabilitySummary = ({ vulnerabilities }) => (
  <Card className="p-4 mb-4">
    <Text size="large" weight="semibold">Vulnerability Summary</Text>
    <div className="mt-4 grid grid-cols-2 gap-4">
      <div>
        <Text>Critical</Text>
        <ProgressBar 
          value={vulnerabilities.critical} 
          max={100}
          style={{ backgroundColor: '#D92C20' }}
        />
      </div>
      <div>
        <Text>High</Text>
        <ProgressBar 
          value={vulnerabilities.high} 
          max={100}
          style={{ backgroundColor: '#FA8C16' }}
        />
      </div>
    </div>
  </Card>
);

// Compliance Status Component
export const ComplianceStatus = ({ statuses }) => (
  <Card className="p-4 mb-4">
    <Text size="large" weight="semibold">Compliance Status</Text>
    <div className="mt-4">
      {statuses.map((status, index) => (
        <div key={index} className="mb-2">
          <Text>{status.name}</Text>
          <div className="flex items-center">
            <div 
              className="w-3 h-3 rounded-full mr-2"
              style={{ 
                backgroundColor: status.compliant ? '#52C41A' : '#D92C20'
              }}
            />
            <Text>{status.compliant ? 'Compliant' : 'Non-Compliant'}</Text>
          </div>
        </div>
      ))}
    </div>
  </Card>
);