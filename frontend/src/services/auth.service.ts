import { apiClient } from '@/lib/api-client';
import type {
  ApiResponse,
  AuthResponse,
  LoginRequest,
  RegisterRequest,
} from '@/types/api.types';

export const authService = {
  async login(credentials: LoginRequest): Promise<ApiResponse<AuthResponse>> {
    return apiClient.post('/Auth/login', credentials);
  },

  async register(data: RegisterRequest): Promise<ApiResponse<AuthResponse>> {
    return apiClient.post('/Auth/register', data);
  },

  async refreshToken(refreshToken: string): Promise<ApiResponse<AuthResponse>> {
    return apiClient.post('/Auth/refresh-token', { refreshToken });
  },

  async logout(): Promise<ApiResponse<boolean>> {
    return apiClient.post('/Auth/logout');
  },
};
