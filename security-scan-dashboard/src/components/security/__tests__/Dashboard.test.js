// src/components/security/__tests__/Dashboard.test.js
import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { Dashboard } from '../Dashboard';

// Mock the security API service
jest.mock('../../../services/securityApi', () => ({
  getSecurityMetrics: jest.fn().mockResolvedValue({
    score: 85,
    critical: 2,
    high: 5
  }),
  getScanResults: jest.fn().mockResolvedValue([])
}));

describe('Dashboard Component', () => {
  it('renders security score', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByText(/85/)).toBeInTheDocument();
    });
  });

  it('displays critical issues count', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByText(/2/)).toBeInTheDocument();
    });
  });

  it('displays high priority issues count', async () => {
    render(<Dashboard />);
    await waitFor(() => {
      expect(screen.getByText(/5/)).toBeInTheDocument();
    });
  });

  it('triggers scan when button clicked', async () => {
    render(<Dashboard />);
    const scanButton = screen.getByText(/Trigger Scan/i);
    fireEvent.click(scanButton);
    await waitFor(() => {
      expect(screen.getByText(/Scanning.../i)).toBeInTheDocument();
    });
  });
});