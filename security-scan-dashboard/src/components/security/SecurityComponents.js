// src/components/security/SecurityComponents.js
import React from 'react';
import { Card, Text, Badge } from '@fluentui/react-components';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

export const SecurityScore = ({ score }) => (
    <Card className="p-4">
        <Text size="large" weight="semibold">Security Score</Text>
        <div className="mt-4">
            <div className="relative pt-1">
                <div className="flex mb-2 items-center justify-between">
                    <div>
                        <span className="text-xs font-semibold inline-block py-1 px-2 uppercase rounded-full text-blue-600 bg-blue-200">
                            {score}%
                        </span>
                    </div>
                </div>
                <div className="overflow-hidden h-2 mb-4 text-xs flex rounded bg-blue-200">
                    <div
                        style={{ width: `${score}%` }}
                        className="shadow-none flex flex-col text-center whitespace-nowrap text-white justify-center bg-blue-500"
                    />
                </div>
            </div>
        </div>
    </Card>
);

export const SeverityMetrics = ({ metrics }) => (
    <Card className="p-4">
        <Text size="large" weight="semibold">Security Issues</Text>
        <div className="grid grid-cols-3 gap-4 mt-4">
            {Object.entries(metrics).map(([severity, count]) => (
                <div key={severity} className="text-center">
                    <Badge 
                        color={severity === 'critical' ? 'danger' : 
                              severity === 'high' ? 'warning' : 'info'}
                    >
                        {severity.toUpperCase()}
                    </Badge>
                    <Text size="large" className="block mt-2">{count}</Text>
                </div>
            ))}
        </div>
    </Card>
);

export const TrendChart = ({ data }) => (
    <Card className="p-4">
        <Text size="large" weight="semibold">Security Trend</Text>
        <div style={{ height: '300px' }} className="mt-4">
            <ResponsiveContainer>
                <LineChart data={data}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="date" />
                    <YAxis />
                    <Tooltip />
                    <Line type="monotone" dataKey="critical" stroke="#ff0000" />
                    <Line type="monotone" dataKey="high" stroke="#ffa500" />
                    <Line type="monotone" dataKey="medium" stroke="#00ff00" />
                </LineChart>
            </ResponsiveContainer>
        </div>
    </Card>
);