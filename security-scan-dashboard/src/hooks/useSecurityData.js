// src/hooks/useSecurityData.js
import { useState, useEffect, useCallback, useRef } from 'react';
import { useMsal } from "@azure/msal-react";
import securityApiService from '../services/securityApi';
import signalRService from '../services/signalRService';
import { trackEvent, trackException } from '../services/telemetryService';

// Mock data for development fallback
const mockDataByRange = {
  '7': {
    trend: [
      { date: '2024-04-09', score: 82 },
      { date: '2024-04-11', score: 85 },
      { date: '2024-04-13', score: 88 },
      { date: '2024-04-15', score: 90 }
    ]
  },
  '30': {
    trend: [
      { date: '2024-03-15', score: 75 },
      { date: '2024-03-22', score: 78 },
      { date: '2024-03-29', score: 82 },
      { date: '2024-04-05', score: 85 },
      { date: '2024-04-12', score: 88 },
      { date: '2024-04-15', score: 90 }
    ]
  }
};

const DEFAULT_METRICS = {
  score: 85,
  critical: 2,
  high: 5,
  trend: mockDataByRange['30'].trend
};

export const useSecurityData = () => {
    const [metrics, setMetrics] = useState(DEFAULT_METRICS);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [lastUpdate, setLastUpdate] = useState(null);
    const [scanStatus, setScanStatus] = useState(null);
    const [scanProgress, setScanProgress] = useState(0);
    const [dateRange, setDateRange] = useState('30');
    const [isUsingMockData, setIsUsingMockData] = useState(false);

    const signalRConnectionRef = useRef(null);
    const refreshTimerRef = useRef(null);

    const fetchData = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);

            // Try to fetch real data
            try {
                const [metricsData, scanResults, trendData] = await Promise.all([
                    securityApiService.getSecurityMetrics(),
                    securityApiService.getScanResults(),
                    securityApiService.getSecurityTrend(parseInt(dateRange))
                ]);

                setMetrics({
                    score: metricsData.score,
                    critical: scanResults.filter(r => r.severity === 'Critical').length,
                    high: scanResults.filter(r => r.severity === 'High').length,
                    trend: trendData || [],
                    lastScanId: metricsData.lastScanId
                });
                
                setIsUsingMockData(false);
            } catch (apiError) {
                console.warn('API not available, using mock data:', apiError);
                // Fallback to mock data
                setMetrics(prev => ({
                    ...prev,
                    trend: mockDataByRange[dateRange]?.trend || prev.trend
                }));
                setIsUsingMockData(true);
            }
            
            setLastUpdate(new Date());
        } catch (err) {
            setError('Failed to fetch security data');
            trackException(err, 'DataFetch');
        } finally {
            setLoading(false);
        }
    }, [dateRange]);

    const setupSignalRConnection = useCallback(async () => {
        if (isUsingMockData) return; // Don't attempt SignalR if using mock data

        try {
            if (!signalRConnectionRef.current) {
                const connection = await signalRService.createConnection();
                
                connection.on('ScanProgressUpdate', (data) => {
                    setScanProgress(data.progress);
                    setScanStatus(data.status);
                    if (data.status === 'completed') {
                        fetchData();
                    }
                });

                connection.on('MetricsUpdate', (data) => {
                    setMetrics(prev => ({
                        ...prev,
                        ...data
                    }));
                    setLastUpdate(new Date());
                });

                signalRConnectionRef.current = connection;
            }
        } catch (err) {
            console.warn('SignalR connection failed, falling back to polling:', err);
            // Don't set error state - just fall back to polling
        }
    }, [fetchData, isUsingMockData]);

    const triggerScan = async () => {
        try {
            setScanStatus('starting');
            setScanProgress(0);
            
            if (isUsingMockData) {
                // Simulate scan with mock data
                for (let i = 0; i <= 100; i += 20) {
                    await new Promise(resolve => setTimeout(resolve, 500));
                    setScanProgress(i);
                }
                setScanStatus('completed');
                setMetrics(prev => ({
                    ...prev,
                    score: Math.min(100, prev.score + 5),
                    critical: Math.max(0, prev.critical - 1)
                }));
                return { scanId: 'mock-scan-' + Date.now() };
            }

            const result = await securityApiService.triggerScan();
            setScanStatus('in-progress');
            return result;
        } catch (err) {
            setScanStatus('failed');
            setError('Scan failed: ' + err.message);
            throw err;
        }
    };

    // Initialize data and connections
    useEffect(() => {
        fetchData();
        
        // Set up polling as fallback
        refreshTimerRef.current = setInterval(fetchData, 300000); // 5 minutes

        return () => {
            clearInterval(refreshTimerRef.current);
            if (signalRConnectionRef.current && !isUsingMockData) {
                signalRService.stopConnection();
                signalRConnectionRef.current = null;
            }
        };
    }, [fetchData]);

    // Handle real-time updates setup
    useEffect(() => {
        if (!isUsingMockData) {
            setupSignalRConnection();
        }
    }, [setupSignalRConnection, isUsingMockData]);

    // Handle date range changes
    useEffect(() => {
        fetchData();
    }, [dateRange, fetchData]);

    return {
        metrics,
        loading,
        error,
        lastUpdate,
        scanStatus,
        scanProgress,
        dateRange,
        isUsingMockData,
        refreshData: fetchData,
        triggerScan,
        setDateRange
    };
};