import { useState, useEffect, useCallback } from 'react';
import SecurityApiService from '../services/securityApi';

export const useSecurityData = () => {
    const [metrics, setMetrics] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [lastUpdate, setLastUpdate] = useState(null);

    const fetchData = useCallback(async () => {
        try {
            setLoading(true);
            const [metricsData, scanResults] = await Promise.all([
                SecurityApiService.getSecurityMetrics(),
                SecurityApiService.getScanResults()
            ]);

            setMetrics({
                score: metricsData.score,
                critical: scanResults.filter(r => r.severity === 'Critical').length,
                high: scanResults.filter(r => r.severity === 'High').length,
                trend: metricsData.trend || []
            });
            
            setLastUpdate(new Date());
            setError(null);
        } catch (err) {
            setError(err.message);
            console.error('Error fetching security data:', err);
        } finally {
            setLoading(false);
        }
    }, []);

    const triggerScan = async () => {
        try {
            await SecurityApiService.triggerScan();
            await fetchData(); // Refresh data after scan
        } catch (err) {
            setError(err.message);
            console.error('Error triggering scan:', err);
        }
    };

    useEffect(() => {
        fetchData();
        const interval = setInterval(fetchData, 300000); // Refresh every 5 minutes
        return () => clearInterval(interval);
    }, [fetchData]);

    return {
        metrics,
        loading,
        error,
        lastUpdate,
        refreshData: fetchData,
        triggerScan
    };
};