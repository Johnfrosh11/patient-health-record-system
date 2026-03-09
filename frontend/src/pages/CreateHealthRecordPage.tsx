import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { healthRecordService } from '@/services/health-record.service';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card } from '@/components/ui/Card';
import type { CreateHealthRecordRequest } from '@/types/api.types';

export function CreateHealthRecordPage() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState<CreateHealthRecordRequest>({
    patientName: '',
    dateOfBirth: '',
    diagnosis: '',
    treatmentPlan: '',
    medicalHistory: '',
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  const createMutation = useMutation({
    mutationFn: (data: CreateHealthRecordRequest) => healthRecordService.create(data),
    onSuccess: () => {
      alert('Health record created successfully!');
      navigate('/health-records');
    },
    onError: (error: any) => {
      alert(error?.response?.data?.message || 'Failed to create health record');
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

    // Clean up data before sending
    const cleanData: CreateHealthRecordRequest = {
      patientName: formData.patientName.trim(),
      dateOfBirth: formData.dateOfBirth,
      diagnosis: formData.diagnosis.trim(),
      treatmentPlan: formData.treatmentPlan.trim(),
      medicalHistory: formData.medicalHistory?.trim() || undefined,
    };

    createMutation.mutate(cleanData);
  };

  const handleInputChange = (field: keyof CreateHealthRecordRequest, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    // Clear error when user starts typing
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: '' }));
    }
  };

  return (
    <div className="container mx-auto py-8 px-4 max-w-2xl">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900 dark:text-gray-100">
          Create Health Record
        </h1>
        <p className="mt-2 text-sm text-gray-600 dark:text-gray-400">
          Add a new patient health record to the system
        </p>
      </div>

      <Card>
        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Patient Name */}
          <Input
            label="Patient Name *"
            type="text"
            placeholder="Enter patient full name"
            value={formData.patientName}
            onChange={(e) => handleInputChange('patientName', e.target.value)}
            error={errors.patientName}
            required
          />

          {/* Date of Birth */}
          <Input
            label="Date of Birth *"
            type="date"
            value={formData.dateOfBirth}
            onChange={(e) => handleInputChange('dateOfBirth', e.target.value)}
            error={errors.dateOfBirth}
            required
            max={new Date().toISOString().split('T')[0]}
          />

          {/* Diagnosis */}
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

          {/* Treatment Plan */}
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

          {/* Medical History (Optional) */}
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

          {/* Form Actions */}
          <div className="flex gap-4 pt-4 border-t border-gray-200 dark:border-gray-700">
            <Button
              type="button"
              variant="outline"
              onClick={() => navigate('/health-records')}
              disabled={createMutation.isPending}
              className="flex-1"
            >
              Cancel
            </Button>
            <Button
              type="submit"
              isLoading={createMutation.isPending}
              className="flex-1"
            >
              Create Health Record
            </Button>
          </div>
        </form>
      </Card>
    </div>
  );
}
