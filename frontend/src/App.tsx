import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Layout } from '@/components/layouts/Layout';
import { ProtectedRoute } from '@/components/auth/ProtectedRoute';
import { LoginPage } from '@/pages/LoginPage';
import { RegisterPage } from '@/pages/RegisterPage';
import { DashboardPage } from '@/pages/DashboardPage';
import { HealthRecordsListPage } from '@/pages/HealthRecordsListPage';
import { CreateHealthRecordPage } from '@/pages/CreateHealthRecordPage';
import { EditHealthRecordPage } from '@/pages/EditHealthRecordPage';
import { HealthRecordDetailPage } from '@/pages/HealthRecordDetailPage';
import { ROUTES } from '@/lib/constants';
import { useAuthStore } from '@/stores/auth.store';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
      staleTime: 5 * 60 * 1000, // 5 minutes
    },
  },
});

function App() {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Layout>
          <Routes>
            {/* Public routes */}
            <Route
              path={ROUTES.LOGIN}
              element={
                isAuthenticated ? (
                  <Navigate to={ROUTES.DASHBOARD} replace />
                ) : (
                  <LoginPage />
                )
              }
            />
            
            <Route
              path={ROUTES.REGISTER}
              element={
                isAuthenticated ? (
                  <Navigate to={ROUTES.DASHBOARD} replace />
                ) : (
                  <RegisterPage />
                )
              }
            />
            
            <Route
              path={ROUTES.HOME}
              element={
                isAuthenticated ? (
                  <Navigate to={ROUTES.DASHBOARD} replace />
                ) : (
                  <Navigate to={ROUTES.LOGIN} replace />
                )
              }
            />

            {/* Protected routes */}
            <Route element={<ProtectedRoute />}>
              <Route path={ROUTES.DASHBOARD} element={<DashboardPage />} />
              <Route path="/health-records" element={<HealthRecordsListPage />} />
              <Route path="/health-records/create" element={<CreateHealthRecordPage />} />
              <Route path="/health-records/edit/:id" element={<EditHealthRecordPage />} />
              <Route path="/health-records/:id" element={<HealthRecordDetailPage />} />
            </Route>
          </Routes>
        </Layout>
      </BrowserRouter>
    </QueryClientProvider>
  );
}

export default App;
