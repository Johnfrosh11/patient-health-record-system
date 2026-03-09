export const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api/v1';

export const ROUTES = {
  HOME: '/',
  LOGIN: '/login',
  REGISTER: '/register',
  DASHBOARD: '/dashboard',
  HEALTH_RECORDS: '/health-records',
  HEALTH_RECORD_CREATE: '/health-records/create',
  HEALTH_RECORD_DETAIL: (id: string) => `/health-records/${id}`,
  ACCESS_REQUESTS: '/access-requests',
  PROFILE: '/profile',
} as const;

export const QUERY_KEYS = {
  AUTH: ['auth'],
  HEALTH_RECORDS: ['health-records'],
  HEALTH_RECORD_DETAIL: (id: string) => ['health-records', id],
  ACCESS_REQUESTS: ['access-requests'],
  USER_PROFILE: ['user', 'profile'],
} as const;
