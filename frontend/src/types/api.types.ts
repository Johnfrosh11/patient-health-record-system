// API Response types
export interface ApiResponse<T> {
  code: string;
  success: boolean;
  message: string;
  data?: T;
}

// Auth types
export interface User {
  userId: string;
  username: string;
  email: string;
  fullName: string;
  organizationId: string;
  isActive: boolean;
  createdDate: string;
  roles: string[];
}

export interface AuthResponse {
  userId: string;
  username: string;
  email: string;
  fullName: string;
  organizationId: string;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  roles: string[];
  permissions: string[];
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  organizationId: string; // Will be converted to GUID by backend
}

// Health Record types
export interface HealthRecord {
  healthRecordId: string;
  patientName: string;
  dateOfBirth: string;
  age: number;
  diagnosis: string;
  treatmentPlan: string;
  medicalHistory?: string;
  createdByUserId: string;
  createdBy: string;
  createdDate: string;
  lastModified?: string;
  canAccess: boolean;
}

export interface PaginatedHealthRecordsResponse {
  items: HealthRecord[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface CreateHealthRecordRequest {
  patientName: string;
  dateOfBirth: string;
  diagnosis: string;
  treatmentPlan: string;
  medicalHistory?: string;
}

export interface UpdateHealthRecordRequest {
  healthRecordId: string;
  patientName: string;
  dateOfBirth: string;
  diagnosis: string;
  treatmentPlan: string;
  medicalHistory?: string;
}

// Access Request types
export interface AccessRequest {
  accessRequestId: string;
  healthRecordId: string;
  patientName: string;
  requestingUserId: string;
  requestingUserName: string;
  reason: string;
  requestDate: string;
  status: string;
  reviewedBy?: string;
  reviewedDate?: string;
  reviewComment?: string;
  accessStartDateTime?: string;
  accessEndDateTime?: string;
  isActive: boolean;
}

export interface CreateAccessRequestRequest {
  healthRecordId: string;
  reason: string;
}

// Search/Pagination types
export interface SearchParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}
