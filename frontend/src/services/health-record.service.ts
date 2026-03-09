import { apiClient } from '@/lib/api-client';
import type {
  ApiResponse,
  HealthRecord,
  PaginatedHealthRecordsResponse,
  CreateHealthRecordRequest,
  UpdateHealthRecordRequest,
  SearchParams,
} from '@/types/api.types';

export const healthRecordService = {
  async getAll(params?: SearchParams): Promise<ApiResponse<PaginatedHealthRecordsResponse>> {
    return apiClient.get('/HealthRecords/all', { params });
  },

  async getById(id: string): Promise<ApiResponse<HealthRecord>> {
    return apiClient.get(`/HealthRecords/${id}`);
  },

  async create(data: CreateHealthRecordRequest): Promise<ApiResponse<HealthRecord>> {
    return apiClient.post('/HealthRecords/create', data);
  },

  async update(data: UpdateHealthRecordRequest): Promise<ApiResponse<HealthRecord>> {
    return apiClient.put('/HealthRecords/update', data);
  },

  async delete(id: string): Promise<ApiResponse<{ id: string; patientName: string; message: string }>> {
    return apiClient.delete(`/HealthRecords/${id}`);
  },
};
