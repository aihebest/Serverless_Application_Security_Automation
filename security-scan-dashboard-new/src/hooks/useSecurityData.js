// src/hooks/useSecurityData.js
import { useState, useEffect, useCallback } from 'react';
import { useMsal } from "@azure/msal-react";
import { trackEvent } from '../services/telemetryService';

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
  },
  '90': {
    trend: [
      { date: '2024-01-15', score: 65 },
      { date: '2024-02-15', score: 72 },
      { date: '2024-03-15', score: 78 },
      { date: '2024-04-15', score: 90 }
    ]
  }
};

export const useSecurityData = () => {
  const [metrics, setMetrics] = useState({
    score: 85,
    critical: 2,
    high: 5,
    trend: mockDataByRange['30'].trend
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [scanStatus, setScanStatus] = useState(null);
  const [scanProgress, setScanProgress] = useState(0);
  const [dateRange, setDateRange] = useState('30');

  const refreshData = useCallback(async () => {
    setLoading(true);
    try {
      // Simulate API call with different data based on range
      await new Promise(resolve => setTimeout(resolve, 1000));
      setMetrics(prev => ({
        ...prev,
        trend: mockDataByRange[dateRange]?.trend || prev.trend
      }));
      setError(null);
      trackEvent('DataRefreshSuccess', { range: dateRange });
    } catch (err) {
      setError(err.message);
      trackEvent('DataRefreshError', { error: err.message });
    } finally {
      setLoading(false);
    }
  }, [dateRange]);

  const triggerScan = useCallback(async () => {
    try {
      setScanStatus('starting');
      setScanProgress(0);
      trackEvent('ScanStarted');

      // Simulate scan progress
      for (let i = 0; i <= 100; i += 10) {
        await new Promise(resolve => setTimeout(resolve, 500));
        setScanProgress(i);
      }

      setScanStatus('completed');
      setMetrics(prev => ({
        ...prev,
        score: Math.min(100, prev.score + 5),
        critical: Math.max(0, prev.critical - 1)
      }));
      trackEvent('ScanCompleted');
    } catch (err) {
      setError('Scan failed: ' + err.message);
      setScanStatus('failed');
      trackEvent('ScanError', { error: err.message });
    }
  }, []);

  const exportData = useCallback(async (format = 'json') => {
    try {
      const data = {
        metrics,
        scanStatus,
        dateRange,
        exportDate: new Date().toISOString()
      };

      const content = format === 'csv' 
        ? convertToCSV(data)
        : JSON.stringify(data, null, 2);

      const blob = new Blob([content], { 
        type: format === 'csv' ? 'text/csv' : 'application/json'
      });
      
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `security-report-${new Date().toISOString()}.${format}`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);

      trackEvent('ExportSuccess', { format });
    } catch (err) {
      setError('Export failed: ' + err.message);
      trackEvent('ExportError', { error: err.message });
    }
  }, [metrics, scanStatus, dateRange]);

  // Update data when date range changes
  useEffect(() => {
    refreshData();
  }, [dateRange, refreshData]);

  return {
    metrics,
    loading,
    error,
    scanStatus,
    scanProgress,
    refreshData,
    triggerScan,
    exportData,
    setDateRange
  };
};

// Helper function for CSV conversion
const convertToCSV = (data) => {
  const headers = ['Date', 'Security Score', 'Critical Issues', 'High Priority Issues'];
  const rows = data.metrics.trend.map(point => [
    point.date,
    point.score,
    data.metrics.critical,
    data.metrics.high
  ]);

  return [
    headers.join(','),
    ...rows.map(row => row.join(','))
  ].join('\n');
};