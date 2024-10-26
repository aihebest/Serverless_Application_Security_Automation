import React from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

const SecurityDashboard = () => {
    const [metrics] = React.useState({
        score: 75,
        critical: 2,
        high: 5,
        medium: 8,
        trend: [
            { date: '2024-01-01', score: 70 },
            { date: '2024-01-02', score: 75 },
            { date: '2024-01-03', score: 72 }
        ]
    });

    const getScoreColorClass = (score) => {
        if (score >= 80) return 'text-green-600';
        if (score >= 60) return 'text-yellow-600';
        return 'text-red-600';
    };

    return (
        <div className="min-h-screen bg-gray-50 py-6">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                {/* Header */}
                <div className="flex justify-between items-center mb-8">
                    <h1 className="text-3xl font-bold text-gray-900">Security Dashboard</h1>
                    <button className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg shadow-sm transition-colors duration-150">
                        Trigger New Scan
                    </button>
                </div>

                {/* Metrics Grid */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
                    {/* Security Score Card */}
                    <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-100">
                        <h2 className="text-lg font-medium text-gray-700 mb-2">Security Score</h2>
                        <div className={`text-4xl font-bold ${getScoreColorClass(metrics.score)}`}>
                            {metrics.score}%
                        </div>
                    </div>

                    {/* Critical Issues Card */}
                    <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-100">
                        <h2 className="text-lg font-medium text-gray-700 mb-2">Critical Issues</h2>
                        <div className="text-4xl font-bold text-red-600">
                            {metrics.critical}
                            <span className="text-base font-normal text-gray-500 ml-2">issues</span>
                        </div>
                    </div>

                    {/* High Priority Card */}
                    <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-100">
                        <h2 className="text-lg font-medium text-gray-700 mb-2">High Priority</h2>
                        <div className="text-4xl font-bold text-orange-500">
                            {metrics.high}
                            <span className="text-base font-normal text-gray-500 ml-2">alerts</span>
                        </div>
                    </div>
                </div>

                {/* Trend Chart */}
                <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-100 mb-8">
                    <h2 className="text-lg font-medium text-gray-700 mb-4">Security Trend</h2>
                    <div className="h-80">
                        <ResponsiveContainer width="100%" height="100%">
                            <LineChart data={metrics.trend}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />
                                <XAxis 
                                    dataKey="date" 
                                    stroke="#6B7280"
                                    tick={{ fill: '#6B7280' }}
                                />
                                <YAxis 
                                    stroke="#6B7280"
                                    tick={{ fill: '#6B7280' }}
                                    domain={[0, 100]}
                                />
                                <Tooltip 
                                    contentStyle={{ 
                                        background: '#ffffff', 
                                        border: '1px solid #E5E7EB',
                                        borderRadius: '6px',
                                        padding: '8px'
                                    }}
                                />
                                <Line 
                                    type="monotone" 
                                    dataKey="score" 
                                    stroke="#2563EB" 
                                    strokeWidth={2}
                                    dot={{ fill: '#2563EB', strokeWidth: 2 }}
                                    activeDot={{ r: 6 }}
                                />
                            </LineChart>
                        </ResponsiveContainer>
                    </div>
                </div>

                {/* Recent Alerts */}
                <div className="bg-white rounded-lg shadow-sm p-6 border border-gray-100">
                    <h2 className="text-lg font-medium text-gray-700 mb-4">Recent Alerts</h2>
                    <div className="space-y-4">
                        {metrics.critical > 0 && (
                            <div className="bg-red-50 border-l-4 border-red-500 p-4 rounded">
                                <div className="flex items-center">
                                    <div className="flex-shrink-0">
                                        <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                                            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                                        </svg>
                                    </div>
                                    <div className="ml-3">
                                        <h3 className="text-sm font-medium text-red-800">
                                            Critical: {metrics.critical} issues require immediate attention
                                        </h3>
                                    </div>
                                </div>
                            </div>
                        )}
                        {metrics.high > 0 && (
                            <div className="bg-orange-50 border-l-4 border-orange-500 p-4 rounded">
                                <div className="flex items-center">
                                    <div className="flex-shrink-0">
                                        <svg className="h-5 w-5 text-orange-400" viewBox="0 0 20 20" fill="currentColor">
                                            <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                                        </svg>
                                    </div>
                                    <div className="ml-3">
                                        <h3 className="text-sm font-medium text-orange-800">
                                            High Priority: {metrics.high} issues need review
                                        </h3>
                                    </div>
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default SecurityDashboard;