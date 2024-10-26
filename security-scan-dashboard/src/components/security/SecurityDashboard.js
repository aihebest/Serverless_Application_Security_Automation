import React from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

const SecurityDashboard = () => {
    const [metrics] = React.useState({
        score: 75,
        critical: 2,
        high: 5,
        trend: [
            { date: '2024-01-01', score: 70 },
            { date: '2024-01-02', score: 75 },
            { date: '2024-01-03', score: 72 }
        ]
    });

    return (
        <div className="min-h-screen bg-gray-50 p-2">
            <div className="max-w-6xl mx-auto">
                {/* Header */}
                <div className="flex justify-between items-center mb-2">
                    <h1 className="text-base font-semibold text-gray-900">Security Dashboard</h1>
                    <button className="bg-blue-600 hover:bg-blue-700 text-white px-2 py-0.5 rounded text-xs">
                        Trigger New Scan
                    </button>
                </div>

                {/* Metrics Grid */}
                <div className="grid grid-cols-3 gap-2 mb-2">
                    {/* Your existing metrics components */}
                </div>

                {/* Chart */}
                <div className="bg-white rounded p-2 shadow-sm mb-2">
                    <h2 className="text-xs text-gray-600 mb-2">Security Trend</h2>
                    <div className="h-32">
                        <ResponsiveContainer width="100%" height="100%">
                            <LineChart data={metrics.trend}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#eee" />
                                <XAxis dataKey="date" tick={{ fontSize: 10 }} />
                                <YAxis domain={[0, 100]} tick={{ fontSize: 10 }} />
                                <Tooltip />
                                <Line type="monotone" dataKey="score" stroke="#2563EB" strokeWidth={1} />
                            </LineChart>
                        </ResponsiveContainer>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default SecurityDashboard;