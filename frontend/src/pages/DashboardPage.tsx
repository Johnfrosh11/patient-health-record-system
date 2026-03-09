import { useAuthStore } from '@/stores/auth.store';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { useNavigate } from 'react-router-dom';

export function DashboardPage() {
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Welcome back, {user?.fullName}!
        </h1>
        <p className="mt-2 text-gray-600 dark:text-gray-400">
          Manage patient health records and access requests
        </p>
      </div>

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle>Health Records</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="mb-4 text-sm text-gray-600 dark:text-gray-400">
              View, create, and manage patient health records
            </p>
            <Button 
              variant="outline" 
              className="w-full"
              onClick={() => navigate('/health-records')}
            >
              View Records
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Create New Record</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="mb-4 text-sm text-gray-600 dark:text-gray-400">
              Add a new patient health record to the system
            </p>
            <Button 
              className="w-full"
              onClick={() => navigate('/health-records/create')}
            >
              Create Record
            </Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Your Profile</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="mb-4 text-sm text-gray-600 dark:text-gray-400 space-y-1">
              <p><span className="font-medium">Username:</span> {user?.username}</p>
              <p><span className="font-medium">Email:</span> {user?.email}</p>
              <p><span className="font-medium">Roles:</span> {user?.roles.join(', ') || 'None'}</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Quick Stats */}
      <Card>
        <CardHeader>
          <CardTitle>Quick Actions</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Button 
              variant="outline"
              onClick={() => navigate('/health-records')}
              className="h-20"
            >
              <div className="text-center">
                <div className="text-2xl mb-1">📋</div>
                <div>View All Records</div>
              </div>
            </Button>
            <Button 
              variant="outline"
              onClick={() => navigate('/health-records/create')}
              className="h-20"
            >
              <div className="text-center">
                <div className="text-2xl mb-1">➕</div>
                <div>Create New Record</div>
              </div>
            </Button>
            <Button 
              variant="outline"
              onClick={() => navigate('/health-records')}
              className="h-20"
            >
              <div className="text-center">
                <div className="text-2xl mb-1">🔍</div>
                <div>Search Records</div>
              </div>
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
