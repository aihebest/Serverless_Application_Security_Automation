// src/components/auth/__tests__/AuthProvider.test.js
import { render } from '@testing-library/react';
import { AuthProvider } from '../AuthProvider';
import { msalInstance } from '../../../config/authConfig';

test('AuthProvider renders children', () => {
  const { getByText } = render(
    <AuthProvider>
      <div>Test Child</div>
    </AuthProvider>
  );
  expect(getByText('Test Child')).toBeInTheDocument();
});