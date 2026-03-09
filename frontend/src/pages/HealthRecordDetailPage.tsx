import { useQuery } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { healthRecordService } from '@/services/health-record.service';
import { Button } from '@/components/ui/Button';
import { Card } from '@/components/ui/Card';

export function HealthRecordDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data, isLoading, error } = useQuery({
    queryKey: ['healthRecord', id],
    queryFn: () => healthRecordService.getById(id!),
    enabled: !!id,
  });

  if (isLoading) {
    return (
      <div className="container mx-auto py-8 px-4">
        <div className="text-center py-12">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-current border-r-transparent" />
          <p className="mt-4 text-gray-600 dark:text-gray-400">Loading health record...</p>
        </div>
      </div>
    );
  }

  if (error || !data?.data) {
    return (
      <div className="container mx-auto py-8 px-4 max-w-3xl">
        <Card className="border-red-200 bg-red-50 dark:bg-red-900/10 dark:border-red-900">
          <p className="text-red-600 dark:text-red-400 mb-4">
            Failed to load health record. Please try again.
          </p>
          <Button onClick={() => navigate('/health-records')}>
            Back to Records
          </Button>
        </Card>
      </div>
    );
  }

  const record = data.data;

  return (
    <div className="container mx-auto py-8 px-4 max-w-3xl">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">
            Health Record Details
          </h1>
          <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
            View patient health record information
          </p>
        </div>
        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={() => navigate(`/health-records/edit/${record.healthRecordId}`)}
          >
            Edit
          </Button>
          <Button
            variant="outline"
            onClick={() => navigate('/health-records')}
          >
            Back
          </Button>
        </div>
      </div>

      <div className="space-y-6">
        {/* Patient Information */}
        <Card>
          <h2 className="text-xl font-semibold text-gray-900 dark:text-gray-100 mb-4">
            Patient Information
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="text-sm font-medium text-gray-500 dark:text-gray-400">
                Patient Name
              </label>
              <p className="mt-1 text-lg text-gray-900 dark:text-gray-100">
                {record.patientName}
              </p>
            </div>
            <div>
              <label className="text-sm font-medium text-gray-500 dark:text-gray-400">
                Age
              </label>
              <p className="mt-1 text-lg text-gray-900 dark:text-gray-100">
                {record.age} years old
              </p>
            </div>
            <div className="md:col-span-2">
              <label className="text-sm font-medium text-gray-500 dark:text-gray-400">
                Date of Birth
              </label>
              <p className="mt-1 text-lg text-gray-900 dark:text-gray-100">
                {new Date(record.dateOfBirth).toLocaleDateString('en-US', {
                  year: 'numeric',
                  month: 'long',
                  day: 'numeric',
                })}
              </p>
            </div>
          </div>
        </Card>

        {/* Medical Information */}
        <Card>
          <h2 className="text-xl font-semibold text-gray-900 dark:text-gray-100 mb-4">
            Medical Information
          </h2>
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium text-gray-500 dark:text-gray-400">
                Diagnosis
              </label>
              <p className="mt-1 text-gray-900 dark:text-gray-100 whitespace-pre-wrap">
                {record.diagnosis}
              </p>
            </div>
            <div>
              <label className="text-sm font-medium text-gray-500 dark:text-gray-400">
                Treatment Plan
              </label>
              <p className="mt-1 text-gray-900 dark:text-gray-100 whitespace-pre-wrap">
                {record.treatmentPlan}
              </p>
            </div>
            {record.medicalHistory && (
              <div>
                <label className="text-sm font-medium text-gray-500 dark:text-gray-400">
                  Medical History
                </label>
                <p className="mt-1 text-gray-900 dark:text-gray-100 whitespace-pre-wrap">
                  {record.medicalHistory}
                </p>
              </div>
            )}
          </div>
        </Card>

        {/* Record Metadata */}
        <Card>
          <h2 className="text-xl font-semibold text-gray-900 dark:text-gray-100 mb-4">
            Record Information
          </h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="text-sm font-medium text-gray-500 dark:text-gray-400">
                Created By
              </label>
              <p className="mt-1 text-gray-900 dark:text-gray-100">
                {record.createdBy}
              </p>
            </div>
            <div>
              <label className="text-sm font-medium text-gray-500 dark:text-gray-400">
                Created Date
              </label>
              <p className="mt-1 text-gray-900 dark:text-gray-100">
                {new Date(record.createdDate).toLocaleString('en-US', {
                  year: 'numeric',
                  month: 'long',
                  day: 'numeric',
                  hour: '2-digit',
                  minute: '2-digit',
                })}
              </p>
            </div>
            {record.lastModified && (
              <div className="md:col-span-2">
                <label className="text-sm font-medium text-gray-500 dark:text-gray-400">
                  Last Modified
                </label>
                <p className="mt-1 text-gray-900 dark:text-gray-100">
                  {new Date(record.lastModified).toLocaleString('en-US', {
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric',
                    hour: '2-digit',
                    minute: '2-digit',
                  })}
                </p>
              </div>
            )}
            <div className="md:col-span-2">
              <label className="text-sm font-medium text-gray-500 dark:text-gray-400">
                Record ID
              </label>
              <p className="mt-1 text-sm font-mono text-gray-600 dark:text-gray-400">
                {record.healthRecordId}
              </p>
            </div>
          </div>
        </Card>

        {/* Actions */}
        <div className="flex gap-4 justify-end pt-4">
          <Button
            variant="outline"
            onClick={() => navigate('/health-records')}
          >
            Back to List
          </Button>
          <Button
            onClick={() => navigate(`/health-records/edit/${record.healthRecordId}`)}
          >
            Edit Record
          </Button>
        </div>
      </div>
    </div>
  );
}
