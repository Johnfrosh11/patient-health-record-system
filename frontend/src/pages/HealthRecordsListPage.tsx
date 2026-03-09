import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { healthRecordService } from '@/services/health-record.service';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card } from '@/components/ui/Card';
import type { HealthRecord } from '@/types/api.types';

export function HealthRecordsListPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');

  const { data, isLoading, error } = useQuery({
    queryKey: ['healthRecords', pageNumber, pageSize, searchTerm],
    queryFn: () => healthRecordService.getAll({ 
      pageNumber, 
      pageSize,
      searchTerm: searchTerm || undefined 
    }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => healthRecordService.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['healthRecords'] });
    },
  });

  const handleDelete = async (id: string, patientName: string) => {
    if (window.confirm(`Are you sure you want to delete the health record for "${patientName}"?`)) {
      try {
        await deleteMutation.mutateAsync(id);
        alert('Health record deleted successfully');
      } catch (err: any) {
        alert(err?.response?.data?.message || 'Failed to delete health record');
      }
    }
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setPageNumber(1);
  };

  const paginatedData = data?.data;
  const records = paginatedData?.items || [];

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">
            Health Records
          </h1>
          <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
            Manage patient health records
          </p>
        </div>
        <Button onClick={() => navigate('/health-records/create')}>
          + Create New Record
        </Button>
      </div>

      {/* Search Bar */}
      <Card className="mb-6">
        <form onSubmit={handleSearch} className="flex gap-4">
          <div className="flex-1">
            <Input
              type="text"
              placeholder="Search by patient name..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />
          </div>
          <Button type="submit">Search</Button>
          {searchTerm && (
            <Button
              type="button"
              variant="outline"
              onClick={() => {
                setSearchTerm('');
                setPageNumber(1);
              }}
            >
              Clear
            </Button>
          )}
        </form>
      </Card>

      {/* Loading State */}
      {isLoading && (
        <div className="text-center py-12">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-current border-r-transparent align-[-0.125em] motion-reduce:animate-[spin_1.5s_linear_infinite]" />
          <p className="mt-4 text-gray-600 dark:text-gray-400">Loading health records...</p>
        </div>
      )}

      {/* Error State */}
      {error && (
        <Card className="border-red-200 bg-red-50 dark:bg-red-900/10 dark:border-red-900">
          <p className="text-red-600 dark:text-red-400">
            Failed to load health records. Please try again.
          </p>
        </Card>
      )}

      {/* Empty State */}
      {!isLoading && !error && records.length === 0 && (
        <Card className="text-center py-12">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-2">
            No health records found
          </h3>
          <p className="text-gray-600 dark:text-gray-400 mb-6">
            {searchTerm ? 'Try adjusting your search criteria' : 'Create your first health record to get started'}
          </p>
          {!searchTerm && (
            <Button onClick={() => navigate('/health-records/create')}>
              Create Health Record
            </Button>
          )}
        </Card>
      )}

      {/* Records List */}
      {!isLoading && !error && records.length > 0 && (
        <>
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {records.map((record: HealthRecord) => (
              <Card key={record.healthRecordId} className="hover:shadow-lg transition-shadow">
                <div className="flex flex-col h-full">
                  <div className="flex-1">
                    <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-2">
                      {record.patientName}
                    </h3>
                    <div className="space-y-1 text-sm text-gray-600 dark:text-gray-400">
                      <p>
                        <span className="font-medium">Age:</span> {record.age} years
                      </p>
                      <p>
                        <span className="font-medium">DOB:</span>{' '}
                        {new Date(record.dateOfBirth).toLocaleDateString()}
                      </p>
                      <p>
                        <span className="font-medium">Diagnosis:</span> {record.diagnosis}
                      </p>
                      <p className="line-clamp-2">
                        <span className="font-medium">Treatment:</span> {record.treatmentPlan}
                      </p>
                      <p className="text-xs mt-2 pt-2 border-t border-gray-200 dark:border-gray-700">
                        Created: {new Date(record.createdDate).toLocaleDateString()}
                      </p>
                    </div>
                  </div>
                  
                  <div className="mt-4 flex gap-2">
                    <Button
                      size="sm"
                      variant="outline"
                      className="flex-1"
                      onClick={() => navigate(`/health-records/${record.healthRecordId}`)}
                    >
                      View
                    </Button>
                    <Button
                      size="sm"
                      variant="outline"
                      className="flex-1"
                      onClick={() => navigate(`/health-records/edit/${record.healthRecordId}`)}
                    >
                      Edit
                    </Button>
                    <Button
                      size="sm"
                      variant="destructive"
                      onClick={() => handleDelete(record.healthRecordId, record.patientName)}
                      isLoading={deleteMutation.isPending}
                    >
                      Delete
                    </Button>
                  </div>
                </div>
              </Card>
            ))}
          </div>

          {/* Pagination */}
          {paginatedData && paginatedData.totalPages > 1 && (
            <div className="mt-8 flex items-center justify-between border-t border-gray-200 dark:border-gray-700 pt-6">
              <div className="text-sm text-gray-600 dark:text-gray-400">
                Showing page {paginatedData.page} of {paginatedData.totalPages} ({paginatedData.totalCount} total records)
              </div>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  disabled={!paginatedData.hasPreviousPage}
                  onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                >
                  Previous
                </Button>
                <Button
                  variant="outline"
                  disabled={!paginatedData.hasNextPage}
                  onClick={() => setPageNumber((p) => p + 1)}
                >
                  Next
                </Button>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
