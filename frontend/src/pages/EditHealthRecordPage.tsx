import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { healthRecordService } from '@/services/health-record.service';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card } from '@/components/ui/Card';
import type { UpdateHealthRecordRequest } from '@/types/api.types';

export function EditHealthRecordPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  // Redirect if no id
  useEffect(() => {
    if (!id) {
      navigate('/health-records');
    }
  }, [id, navigate]);
  
  const [formData, setFormData] = useState<UpdateHealthRecordRequest>({
    healthRecordId: '',
    patientName: '',
    dateOfBirth: '',
    diagnosis: '',
    treatmentPlan: '',
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  // Fetch existing record
  const { data, isLoading, error } = useQuery({
    queryKey: ['healthRecord', id],
    queryFn: () => {
      if (!id) throw new Error('No ID provided');
      return healthRecordService.getById(id);
    },
    enabled: !!id,
  });

  // Populate form when data is loaded
  useEffect(() => {
    if (data?.data && id) {
      const record = data.data;
      const dobParts = record.dateOfBirth.split('T');
      const formattedDate = dobParts[0] || record.dateOfBirth;
      
      setFormData({
        healthRecordId: id,
        patientName: record.patientName,
        dateOfBirth: formattedDate,
        diagnosis: record.diagnosis,
        treatmentPlan: record.treatmentPlan,
        ...(record.medicalHistory && { medicalHistory: record.medicalHistory }),
      });
    }
  }, [data, id]);

  const updateMutation = useMutation({
    mutationFn: (data: UpdateHealthRecordRequest) => healthRecordService.update(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['healthRecords'] });
      queryClient.invalidateQueries({ queryKey: ['healthRecord', id] });
      alert('Health record updated successfully!');
      navigate('/health-records');
    },
    onError: (error: any) => {
      alert(error?.response?.data?.message || 'Failed to update health record');
    },
  });

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!formData.patientName.trim()) {
      newErrors.patientName = 'Patient name is required';
    } else if (formData.patientName.trim().length < 2) {
      newErrors.patientName = 'Patient name must be at least 2 characters';
    }

    if (!formData.dateOfBirth) {
      newErrors.dateOfBirth = 'Date of birth is required';
    } else {
      const dob = new Date(formData.dateOfBirth);
      const today = new Date();
      if (dob > today) {
        newErrors.dateOfBirth = 'Date of birth cannot be in the future';
      }
    }

    if (!formData.diagnosis.trim()) {
      newErrors.diagnosis = 'Diagnosis is required';
    } else if (formData.diagnosis.trim().length < 3) {
      newErrors.diagnosis = 'Diagnosis must be at least 3 characters';
    }

    if (!formData.treatmentPlan.trim()) {
      newErrors.treatmentPlan = 'Treatment plan is required';
    } else if (formData.treatmentPlan.trim().length < 5) {
      newErrors.treatmentPlan = 'Treatment plan must be at least 5 characters';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validate()) {
      return;
    }

    const cleanData: UpdateHealthRecordRequest = {
      healthRecordId: formData.healthRecordId,
      patientName: formData.patientName.trim(),
      dateOfBirth: formData.dateOfBirth,
      diagnosis: formData.diagnosis.trim(),
      treatmentPlan: formData.treatmentPlan.trim(),
      medicalHistory: formData.medicalHistory?.trim() || undefined,
    };

    updateMutation.mutate(cleanData);
  };

  const handleInputChange = (field: keyof UpdateHealthRecordRequest, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: '' }));
    }
  };

  if (isLoading) {
    return (
      <div className="container mx-auto py-8 px-4 max-w-2xl">
        <div className="text-center py-12">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-current border-r-transparent" />
          <p className="mt-4 text-gray-600 dark:text-gray-400">Loading health record...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto py-8 px-4 max-w-2xl">
        <Card className="border-red-200 bg-red-50 dark:bg-red-900/10 dark:border-red-900">
          <p className="text-red-600 dark:text-red-400">
            Failed to load health record. Please try again.
          </p>
          <Button onClick={() => navigate('/health-records')} className="mt-4">
            Back to Records
          </Button>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8 px-4 max-w-2xl">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Edit Health Record
        </h1>
        <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
          Update patient health record information
        </p>
      </div>

      <Card>
        <form onSubmit={handleSubmit} className="space-y-6">
          <Input
            label="Patient Name *"
            type="text"
            placeholder="Enter patient full name"
            value={formData.patientName}
            onChange={(e) => handleInputChange('patientName', e.target.value)}
            error={errors.patientName}
            required
          />

          <Input
            label="Date of Birth *"
            type="date"
            value={formData.dateOfBirth}
            onChange={(e) => handleInputChange('dateOfBirth', e.target.value)}
            error={errors.dateOfBirth}
            required
            max={new Date().toISOString().split('T')[0]}
          />

          <div>
            <label className="mb-2 block text-sm font-medium text-gray-700 dark:text-gray-300">
              Diagnosis *
            </label>
            <textarea
              className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              placeholder="Enter patient diagnosis"
              value={formData.diagnosis}
              onChange={(e) => handleInputChange('diagnosis', e.target.value)}
              required
            />
            {errors.diagnosis && (
              <p className="mt-1 text-sm text-destructive">{errors.diagnosis}</p>
            )}
          </div>

          <div>
            <label className="mb-2 block text-sm font-medium text-gray-700 dark:text-gray-300">
              Treatment Plan *
            </label>
            <textarea
              className="flex min-h-[100px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              placeholder="Enter treatment plan details"
              value={formData.treatmentPlan}
              onChange={(e) => handleInputChange('treatmentPlan', e.target.value)}
              required
            />
            {errors.treatmentPlan && (
              <p className="mt-1 text-sm text-destructive">{errors.treatmentPlan}</p>
            )}
          </div>

          <div>
            <label className="mb-2 block text-sm font-medium text-gray-700 dark:text-gray-300">
              Medical History (Optional)
            </label>
            <textarea
              className="flex min-h-[120px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
              placeholder="Enter patient medical history (optional)"
              value={formData.medicalHistory}
              onChange={(e) => handleInputChange('medicalHistory', e.target.value)}
            />
          </div>

          <div className="flex gap-4 pt-4 border-t border-gray-200 dark:border-gray-700">
            <Button
              type="button"
              variant="outline"
              onClick={() => navigate('/health-records')}
              disabled={updateMutation.isPending}
              className="flex-1"
            >
              Cancel
            </Button>
            <Button
              type="submit"
              isLoading={updateMutation.isPending}
              className="flex-1"
            >
              Update Health Record
            </Button>
          </div>
        </form>
      </Card>
    </div>
  );
}
