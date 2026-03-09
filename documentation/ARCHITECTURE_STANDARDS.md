Project Guidelines & Coding Standards
This document defines the coding standards, conventions, and best practices for this codebase. AI assistants and developers should adhere to these guidelines when writing or reviewing code.

Table of Contents
Tech Stack
Project Structure
TypeScript Standards
React Conventions
Next.js Patterns
Component Architecture
Styling with Tailwind CSS
State Management
Data Fetching
Form Handling
Error Handling
Testing Standards
Performance Guidelines
Accessibility (a11y)
Security Practices
Git & Version Control
Code Review Checklist
Anti-Patterns to Avoid
Tech Stack
Category	Technology
Framework	Next.js 14+ (App Router)
Language	TypeScript 5+ (Strict Mode)
Styling	Tailwind CSS 3+
State Management	Zustand / React Context (local), TanStack Query (server)
Forms	React Hook Form + Zod
Testing	Vitest + React Testing Library + Playwright
Linting	ESLint + Prettier
Package Manager	pnpm (preferred) / npm
Project Structure
src/
├── app/                    # Next.js App Router pages and layouts
│   ├── (auth)/             # Route groups for authentication pages
│   ├── (dashboard)/        # Route groups for dashboard pages
│   ├── api/                # API routes (Route Handlers)
│   ├── layout.tsx          # Root layout
│   ├── page.tsx            # Home page
│   ├── error.tsx           # Error boundary
│   ├── loading.tsx         # Loading UI
│   └── not-found.tsx       # 404 page
│
├── components/
│   ├── ui/                 # Primitive/base components (Button, Input, Modal)
│   ├── forms/              # Form-specific components
│   ├── layouts/            # Layout components (Header, Footer, Sidebar)
│   └── features/           # Feature-specific components grouped by domain
│       ├── auth/
│       ├── dashboard/
│       └── settings/
│
├── hooks/                  # Custom React hooks
│   ├── use-debounce.ts
│   ├── use-local-storage.ts
│   └── use-media-query.ts
│
├── lib/                    # Utility functions and configurations
│   ├── utils.ts            # General utility functions
│   ├── api-client.ts       # API client configuration
│   ├── validations.ts      # Zod schemas
│   └── constants.ts        # App-wide constants
│
├── services/               # API service layer / data fetching functions
│   ├── auth.service.ts
│   └── user.service.ts
│
├── stores/                 # Global state stores (Zustand)
│   └── auth.store.ts
│
├── types/                  # TypeScript type definitions
│   ├── api.types.ts        # API response/request types
│   ├── common.types.ts     # Shared types
│   └── index.ts            # Type exports
│
├── styles/                 # Global styles
│   └── globals.css         # Tailwind directives and global CSS
│
└── config/                 # App configuration
    ├── site.ts             # Site metadata
    └── navigation.ts       # Navigation configuration
File Naming Conventions
Type	Convention	Example
Components	PascalCase	UserProfile.tsx
Component folders	PascalCase	UserProfile/index.tsx
Hooks	camelCase with use- prefix (kebab-case file)	use-auth.ts → useAuth
Utilities	kebab-case	format-date.ts
Types	kebab-case with .types	user.types.ts
Constants	kebab-case	api-endpoints.ts
Test files	Same name with .test	Button.test.tsx
TypeScript Standards
Strict Configuration
// tsconfig.json - Required compiler options
{
  "compilerOptions": {
    "strict": true,
    "noUncheckedIndexedAccess": true,
    "noImplicitReturns": true,
    "noFallthroughCasesInSwitch": true,
    "noImplicitOverride": true,
    "forceConsistentCasingInFileNames": true
  }
}
Type Definition Rules
// ✅ DO: Use interface for object shapes that may be extended
interface User {
  id: string;
  email: string;
  name: string;
  createdAt: Date;
}

// ✅ DO: Use type for unions, intersections, and computed types
type Status = 'idle' | 'loading' | 'success' | 'error';
type UserWithRole = User & { role: Role };

// ✅ DO: Use readonly for immutable data
interface Config {
  readonly apiUrl: string;
  readonly maxRetries: number;
}

// ✅ DO: Use `as const` for literal types
const ROUTES = {
  HOME: '/',
  DASHBOARD: '/dashboard',
  SETTINGS: '/settings',
} as const;

// ✅ DO: Prefer unknown over any, narrow types explicitly
function parseJSON(json: string): unknown {
  return JSON.parse(json);
}

// ✅ DO: Use generics for reusable type-safe functions
function getProperty<T, K extends keyof T>(obj: T, key: K): T[K] {
  return obj[key];
}

// ❌ DON'T: Use `any` - always find a more specific type
// ❌ DON'T: Use `@ts-ignore` - use `@ts-expect-error` with explanation if necessary
// ❌ DON'T: Use non-null assertion (!) without justification
Import Organization
// Order imports in this sequence, separated by blank lines:

// 1. React and Next.js imports
import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import Image from 'next/image';

// 2. Third-party libraries
import { useQuery } from '@tanstack/react-query';
import { z } from 'zod';

// 3. Internal aliases (@/)
import { Button } from '@/components/ui/Button';
import { useAuth } from '@/hooks/use-auth';
import { cn } from '@/lib/utils';

// 4. Relative imports (if necessary)
import { UserAvatar } from './UserAvatar';

// 5. Types (use `import type` for type-only imports)
import type { User } from '@/types';
React Conventions
Component Declaration
// ✅ DO: Use function declarations for components
// ✅ DO: Export named exports (not default exports)
// ✅ DO: Define props interface directly above component

interface ButtonProps {
  variant?: 'primary' | 'secondary' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  isLoading?: boolean;
  children: React.ReactNode;
  onClick?: () => void;
}

export function Button({
  variant = 'primary',
  size = 'md',
  isLoading = false,
  children,
  onClick,
}: ButtonProps) {
  return (
    <button
      className={cn(buttonVariants({ variant, size }))}
      onClick={onClick}
      disabled={isLoading}
    >
      {isLoading ? <Spinner /> : children}
    </button>
  );
}
Hooks Rules
// ✅ DO: Custom hooks must start with "use"
// ✅ DO: Extract complex logic into custom hooks
// ✅ DO: Return objects for multiple values (not arrays) for clarity

export function useUser(userId: string) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    // Fetch logic here
  }, [userId]);

  // Return object, not array
  return { user, isLoading, error };
}

// ✅ DO: Memoize expensive computations
const sortedItems = useMemo(
  () => items.sort((a, b) => a.name.localeCompare(b.name)),
  [items]
);

// ✅ DO: Memoize callbacks passed to child components
const handleSubmit = useCallback((data: FormData) => {
  // Submit logic
}, [dependency]);
Event Handlers
// ✅ DO: Prefix event handlers with "handle" or "on"
function SearchForm() {
  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    // Logic here
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    // Logic here
  };

  return (
    <form onSubmit={handleSubmit}>
      <input onChange={handleInputChange} />
    </form>
  );
}
Conditional Rendering
// ✅ DO: Use early returns for cleaner conditionals
function UserProfile({ user }: { user: User | null }) {
  if (!user) {
    return <EmptyState message="User not found" />;
  }

  return (
    <div>
      <h1>{user.name}</h1>
      <p>{user.email}</p>
    </div>
  );
}

// ✅ DO: Use ternary for simple inline conditionals
<Button>{isLoading ? 'Loading...' : 'Submit'}</Button>

// ✅ DO: Use && for presence checks (but be careful with falsy values)
{user && <UserAvatar user={user} />}

// ⚠️ CAREFUL: Avoid && with numbers (0 will render)
// ❌ Bad: {count && <span>{count}</span>}
// ✅ Good: {count > 0 && <span>{count}</span>}
Next.js Patterns
App Router Conventions
// app/dashboard/page.tsx
// ✅ DO: Use metadata export for SEO
import type { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Dashboard | MyApp',
  description: 'View your dashboard and analytics',
};

// ✅ DO: Default to Server Components
export default async function DashboardPage() {
  const data = await fetchDashboardData();

  return (
    <main>
      <h1>Dashboard</h1>
      <DashboardContent data={data} />
    </main>
  );
}
Client vs Server Components
// ✅ DO: Only use "use client" when necessary
// Client components needed for: hooks, event handlers, browser APIs

// components/ui/Button.tsx - Can be server component if no interactivity
export function Button({ children }: { children: React.ReactNode }) {
  return <button className="btn">{children}</button>;
}

// components/features/Counter.tsx - Needs "use client"
'use client';

import { useState } from 'react';

export function Counter() {
  const [count, setCount] = useState(0);
  return <button onClick={() => setCount(c => c + 1)}>{count}</button>;
}
Data Fetching Patterns
// ✅ DO: Fetch data in Server Components when possible
// app/users/page.tsx
async function getUsers(): Promise<User[]> {
  const res = await fetch('https://api.example.com/users', {
    next: { revalidate: 60 }, // Cache for 60 seconds
  });

  if (!res.ok) {
    throw new Error('Failed to fetch users');
  }

  return res.json();
}

export default async function UsersPage() {
  const users = await getUsers();
  return <UserList users={users} />;
}

// ✅ DO: Use loading.tsx for streaming/suspense
// app/users/loading.tsx
export default function Loading() {
  return <UserListSkeleton />;
}

// ✅ DO: Use error.tsx for error boundaries
// app/users/error.tsx
'use client';

export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <div>
      <h2>Something went wrong!</h2>
      <button onClick={reset}>Try again</button>
    </div>
  );
}
Route Handlers (API Routes)
// app/api/users/route.ts
import { NextRequest, NextResponse } from 'next/server';
import { z } from 'zod';

const createUserSchema = z.object({
  email: z.string().email(),
  name: z.string().min(2),
});

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();
    const validated = createUserSchema.parse(body);

    // Create user logic
    const user = await createUser(validated);

    return NextResponse.json(user, { status: 201 });
  } catch (error) {
    if (error instanceof z.ZodError) {
      return NextResponse.json(
        { error: 'Validation failed', details: error.errors },
        { status: 400 }
      );
    }

    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
}
Server Actions
// ✅ DO: Use Server Actions for mutations
// app/actions/user.actions.ts
'use server';

import { revalidatePath } from 'next/cache';
import { z } from 'zod';

const updateProfileSchema = z.object({
  name: z.string().min(2),
  bio: z.string().max(500).optional(),
});

export async function updateProfile(formData: FormData) {
  const validated = updateProfileSchema.parse({
    name: formData.get('name'),
    bio: formData.get('bio'),
  });

  await db.user.update({
    where: { id: getCurrentUserId() },
    data: validated,
  });

  revalidatePath('/profile');
}
Component Architecture
Component Composition Pattern
// ✅ DO: Build composable component APIs

// components/ui/Card.tsx
interface CardProps {
  children: React.ReactNode;
  className?: string;
}

function Card({ children, className }: CardProps) {
  return (
    <div className={cn('rounded-lg border bg-card p-6', className)}>
      {children}
    </div>
  );
}

function CardHeader({ children, className }: CardProps) {
  return (
    <div className={cn('mb-4 space-y-1', className)}>
      {children}
    </div>
  );
}

function CardTitle({ children, className }: CardProps) {
  return (
    <h3 className={cn('text-lg font-semibold', className)}>
      {children}
    </h3>
  );
}

function CardContent({ children, className }: CardProps) {
  return <div className={cn('text-sm', className)}>{children}</div>;
}

// Export as compound component
export { Card, CardHeader, CardTitle, CardContent };

// Usage:
<Card>
  <CardHeader>
    <CardTitle>Welcome Back</CardTitle>
  </CardHeader>
  <CardContent>
    Your dashboard content here.
  </CardContent>
</Card>
Props Pattern with Variants (using CVA)
// ✅ DO: Use class-variance-authority for variant-based styling
import { cva, type VariantProps } from 'class-variance-authority';

const buttonVariants = cva(
  // Base styles
  'inline-flex items-center justify-center rounded-md font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 disabled:pointer-events-none disabled:opacity-50',
  {
    variants: {
      variant: {
        primary: 'bg-primary text-primary-foreground hover:bg-primary/90',
        secondary: 'bg-secondary text-secondary-foreground hover:bg-secondary/80',
        outline: 'border border-input bg-background hover:bg-accent',
        ghost: 'hover:bg-accent hover:text-accent-foreground',
        destructive: 'bg-destructive text-destructive-foreground hover:bg-destructive/90',
      },
      size: {
        sm: 'h-8 px-3 text-xs',
        md: 'h-10 px-4 text-sm',
        lg: 'h-12 px-6 text-base',
      },
    },
    defaultVariants: {
      variant: 'primary',
      size: 'md',
    },
  }
);

interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  isLoading?: boolean;
}

export function Button({
  variant,
  size,
  isLoading,
  className,
  children,
  ...props
}: ButtonProps) {
  return (
    <button
      className={cn(buttonVariants({ variant, size }), className)}
      disabled={isLoading}
      {...props}
    >
      {isLoading && <Spinner className="mr-2 h-4 w-4" />}
      {children}
    </button>
  );
}
Container/Presentational Pattern
// ✅ DO: Separate data fetching from presentation

// components/features/users/UserListContainer.tsx
// Container: Handles data fetching and state
export function UserListContainer() {
  const { data: users, isLoading, error } = useUsers();

  if (isLoading) return <UserListSkeleton />;
  if (error) return <ErrorMessage error={error} />;

  return <UserList users={users} />;
}

// components/features/users/UserList.tsx
// Presentational: Pure rendering, easily testable
interface UserListProps {
  users: User[];
}

export function UserList({ users }: UserListProps) {
  if (users.length === 0) {
    return <EmptyState message="No users found" />;
  }

  return (
    <ul className="space-y-4">
      {users.map(user => (
        <UserListItem key={user.id} user={user} />
      ))}
    </ul>
  );
}
Styling with Tailwind CSS
Utility Class Organization
// ✅ DO: Order Tailwind classes logically (Recommended order)
// Layout → Sizing → Spacing → Typography → Visual → Interactive → Responsive

<div
  className={cn(
    // Layout
    'flex flex-col items-center justify-between',
    // Sizing
    'h-full w-full max-w-md',
    // Spacing
    'gap-4 p-6',
    // Typography
    'text-sm font-medium text-gray-900',
    // Visual (backgrounds, borders, shadows)
    'rounded-lg border border-gray-200 bg-white shadow-sm',
    // Interactive
    'hover:border-gray-300 focus:outline-none focus:ring-2',
    // Responsive
    'sm:flex-row md:max-w-lg lg:p-8'
  )}
>
The cn() Utility
// lib/utils.ts
// ✅ DO: Always use cn() for conditional class merging
import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

// Usage:
<div
  className={cn(
    'base-classes here',
    isActive && 'active-classes',
    isDisabled && 'disabled-classes',
    className // Allow parent to override
  )}
/>
Responsive Design
// ✅ DO: Mobile-first responsive design
// Tailwind breakpoints: sm (640px), md (768px), lg (1024px), xl (1280px), 2xl (1536px)

<div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
  {/* Grid items */}
</div>

// ✅ DO: Use container queries when appropriate
<div className="@container">
  <div className="@md:flex-row flex flex-col">
    {/* Responds to container width, not viewport */}
  </div>
</div>
Dark Mode
// ✅ DO: Use CSS variables for theme colors (configured in tailwind.config)
// ✅ DO: Use the `dark:` variant for dark mode styles

<div className="bg-white text-gray-900 dark:bg-gray-900 dark:text-gray-100">
  <h1 className="text-primary dark:text-primary-dark">
    Themed heading
  </h1>
</div>

// tailwind.config.ts - Theme configuration example
const config = {
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: 'hsl(var(--primary))',
          foreground: 'hsl(var(--primary-foreground))',
        },
        // ... other semantic colors
      },
    },
  },
};
Tailwind Anti-Patterns
// ❌ DON'T: Use arbitrary values when Tailwind utilities exist
<div className="mt-[16px]" /> // Bad
<div className="mt-4" />      // Good

// ❌ DON'T: Repeat long class strings - extract components
// Bad: Copying "flex items-center gap-2 rounded-lg bg-blue-500 px-4 py-2..." everywhere
// Good: Create a <Button> component

// ❌ DON'T: Mix CSS-in-JS with Tailwind
// Pick one approach and stick with it

// ❌ DON'T: Use @apply excessively in CSS files
// Only use @apply for truly reusable base styles
State Management
Local State (useState)
// ✅ DO: Use useState for component-local state
function Toggle() {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <button onClick={() => setIsOpen(prev => !prev)}>
      {isOpen ? 'Close' : 'Open'}
    </button>
  );
}
Complex Local State (useReducer)
// ✅ DO: Use useReducer for complex state logic
interface State {
  status: 'idle' | 'loading' | 'success' | 'error';
  data: User | null;
  error: string | null;
}

type Action =
  | { type: 'FETCH_START' }
  | { type: 'FETCH_SUCCESS'; payload: User }
  | { type: 'FETCH_ERROR'; payload: string };

function reducer(state: State, action: Action): State {
  switch (action.type) {
    case 'FETCH_START':
      return { ...state, status: 'loading', error: null };
    case 'FETCH_SUCCESS':
      return { status: 'success', data: action.payload, error: null };
    case 'FETCH_ERROR':
      return { status: 'error', data: null, error: action.payload };
    default:
      return state;
  }
}
Global State (Zustand)
// stores/auth.store.ts
// ✅ DO: Use Zustand for global client state
import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (user: User, token: string) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      isAuthenticated: false,

      login: (user, token) =>
        set({ user, token, isAuthenticated: true }),

      logout: () =>
        set({ user: null, token: null, isAuthenticated: false }),
    }),
    {
      name: 'auth-storage', // localStorage key
      partialize: (state) => ({ token: state.token }), // Only persist token
    }
  )
);

// Usage in components:
function Profile() {
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);
  // ...
}
Server State (TanStack Query)
// hooks/use-users.ts
// ✅ DO: Use TanStack Query for server state
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { userService } from '@/services/user.service';

// Query keys factory
export const userKeys = {
  all: ['users'] as const,
  lists: () => [...userKeys.all, 'list'] as const,
  list: (filters: UserFilters) => [...userKeys.lists(), filters] as const,
  details: () => [...userKeys.all, 'detail'] as const,
  detail: (id: string) => [...userKeys.details(), id] as const,
};

export function useUsers(filters: UserFilters) {
  return useQuery({
    queryKey: userKeys.list(filters),
    queryFn: () => userService.getUsers(filters),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

export function useUser(id: string) {
  return useQuery({
    queryKey: userKeys.detail(id),
    queryFn: () => userService.getUser(id),
    enabled: !!id, // Only fetch if id exists
  });
}

export function useUpdateUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: userService.updateUser,
    onSuccess: (updatedUser) => {
      // Update the specific user in cache
      queryClient.setQueryData(userKeys.detail(updatedUser.id), updatedUser);
      // Invalidate lists to refetch
      queryClient.invalidateQueries({ queryKey: userKeys.lists() });
    },
  });
}
Data Fetching
API Client Setup
// lib/api-client.ts
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || '/api';

interface RequestConfig extends RequestInit {
  params?: Record<string, string>;
}

class ApiClient {
  private baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  private async request<T>(
    endpoint: string,
    config: RequestConfig = {}
  ): Promise<T> {
    const { params, ...init } = config;

    const url = new URL(`${this.baseUrl}${endpoint}`);
    if (params) {
      Object.entries(params).forEach(([key, value]) =>
        url.searchParams.append(key, value)
      );
    }

    const response = await fetch(url.toString(), {
      ...init,
      headers: {
        'Content-Type': 'application/json',
        ...init.headers,
      },
    });

    if (!response.ok) {
      throw new ApiError(response.status, await response.text());
    }

    return response.json();
  }

  get<T>(endpoint: string, config?: RequestConfig) {
    return this.request<T>(endpoint, { ...config, method: 'GET' });
  }

  post<T>(endpoint: string, data: unknown, config?: RequestConfig) {
    return this.request<T>(endpoint, {
      ...config,
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  put<T>(endpoint: string, data: unknown, config?: RequestConfig) {
    return this.request<T>(endpoint, {
      ...config,
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  delete<T>(endpoint: string, config?: RequestConfig) {
    return this.request<T>(endpoint, { ...config, method: 'DELETE' });
  }
}

export const apiClient = new ApiClient(API_BASE_URL);
Service Layer Pattern
// services/user.service.ts
import { apiClient } from '@/lib/api-client';
import type { User, CreateUserInput, UpdateUserInput } from '@/types';

export const userService = {
  getUsers: (filters?: UserFilters) =>
    apiClient.get<User[]>('/users', { params: filters }),

  getUser: (id: string) =>
    apiClient.get<User>(`/users/${id}`),

  createUser: (data: CreateUserInput) =>
    apiClient.post<User>('/users', data),

  updateUser: ({ id, ...data }: UpdateUserInput & { id: string }) =>
    apiClient.put<User>(`/users/${id}`, data),

  deleteUser: (id: string) =>
    apiClient.delete<void>(`/users/${id}`),
};
Form Handling
React Hook Form + Zod Pattern
// ✅ DO: Use React Hook Form with Zod for type-safe forms
'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';

// Define schema
const loginSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
  rememberMe: z.boolean().default(false),
});

// Infer type from schema
type LoginFormData = z.infer<typeof loginSchema>;

export function LoginForm() {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
      rememberMe: false,
    },
  });

  const onSubmit = async (data: LoginFormData) => {
    try {
      await loginUser(data);
      // Handle success
    } catch (error) {
      // Handle error
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div>
        <label htmlFor="email" className="block text-sm font-medium">
          Email
        </label>
        <input
          id="email"
          type="email"
          {...register('email')}
          className={cn(
            'mt-1 block w-full rounded-md border px-3 py-2',
            errors.email && 'border-red-500'
          )}
        />
        {errors.email && (
          <p className="mt-1 text-sm text-red-600">{errors.email.message}</p>
        )}
      </div>

      <div>
        <label htmlFor="password" className="block text-sm font-medium">
          Password
        </label>
        <input
          id="password"
          type="password"
          {...register('password')}
          className={cn(
            'mt-1 block w-full rounded-md border px-3 py-2',
            errors.password && 'border-red-500'
          )}
        />
        {errors.password && (
          <p className="mt-1 text-sm text-red-600">{errors.password.message}</p>
        )}
      </div>

      <div className="flex items-center">
        <input
          id="rememberMe"
          type="checkbox"
          {...register('rememberMe')}
          className="h-4 w-4 rounded border"
        />
        <label htmlFor="rememberMe" className="ml-2 text-sm">
          Remember me
        </label>
      </div>

      <button
        type="submit"
        disabled={isSubmitting}
        className="w-full rounded-md bg-primary px-4 py-2 text-white disabled:opacity-50"
      >
        {isSubmitting ? 'Signing in...' : 'Sign in'}
      </button>
    </form>
  );
}
Reusable Form Components
// components/forms/FormField.tsx
import { useFormContext } from 'react-hook-form';

interface FormFieldProps {
  name: string;
  label: string;
  type?: string;
  placeholder?: string;
}

export function FormField({
  name,
  label,
  type = 'text',
  placeholder,
}: FormFieldProps) {
  const {
    register,
    formState: { errors },
  } = useFormContext();

  const error = errors[name];

  return (
    <div className="space-y-1">
      <label htmlFor={name} className="block text-sm font-medium">
        {label}
      </label>
      <input
        id={name}
        type={type}
        placeholder={placeholder}
        {...register(name)}
        className={cn(
          'w-full rounded-md border px-3 py-2',
          error && 'border-red-500'
        )}
      />
      {error && (
        <p className="text-sm text-red-600">
          {error.message as string}
        </p>
      )}
    </div>
  );
}
Error Handling
Error Classes
// lib/errors.ts
export class AppError extends Error {
  constructor(
    message: string,
    public code: string,
    public statusCode: number = 500
  ) {
    super(message);
    this.name = 'AppError';
  }
}

export class ApiError extends AppError {
  constructor(
    statusCode: number,
    message: string,
    public details?: unknown
  ) {
    super(message, 'API_ERROR', statusCode);
    this.name = 'ApiError';
  }
}

export class ValidationError extends AppError {
  constructor(
    message: string,
    public errors: Record<string, string[]>
  ) {
    super(message, 'VALIDATION_ERROR', 400);
    this.name = 'ValidationError';
  }
}

export class AuthenticationError extends AppError {
  constructor(message = 'Authentication required') {
    super(message, 'AUTHENTICATION_ERROR', 401);
    this.name = 'AuthenticationError';
  }
}
Error Boundary Component
// components/ErrorBoundary.tsx
'use client';

import { Component, type ReactNode } from 'react';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('Error caught by boundary:', error, errorInfo);
    // Log to error reporting service
  }

  render() {
    if (this.state.hasError) {
      return (
        this.props.fallback || (
          <div className="p-4 text-center">
            <h2>Something went wrong</h2>
            <button onClick={() => this.setState({ hasError: false, error: null })}>
              Try again
            </button>
          </div>
        )
      );
    }

    return this.props.children;
  }
}
Async Error Handling Pattern
// ✅ DO: Use a result type pattern for operations that can fail
type Result<T, E = Error> =
  | { success: true; data: T }
  | { success: false; error: E };

async function safeAsync<T>(
  promise: Promise<T>
): Promise<Result<T>> {
  try {
    const data = await promise;
    return { success: true, data };
  } catch (error) {
    return {
      success: false,
      error: error instanceof Error ? error : new Error(String(error)),
    };
  }
}

// Usage:
const result = await safeAsync(fetchUser(id));

if (!result.success) {
  console.error('Failed to fetch user:', result.error);
  return;
}

const user = result.data; // Type-safe access
Testing Standards
Component Testing
// components/ui/Button.test.tsx
import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { Button } from './Button';

describe('Button', () => {
  it('renders children correctly', () => {
    render(<Button>Click me</Button>);
    expect(screen.getByRole('button', { name: /click me/i })).toBeInTheDocument();
  });

  it('calls onClick when clicked', () => {
    const handleClick = vi.fn();
    render(<Button onClick={handleClick}>Click me</Button>);

    fireEvent.click(screen.getByRole('button'));
    expect(handleClick).toHaveBeenCalledTimes(1);
  });

  it('shows loading state', () => {
    render(<Button isLoading>Submit</Button>);
    expect(screen.getByRole('button')).toBeDisabled();
  });

  it('applies variant classes correctly', () => {
    render(<Button variant="destructive">Delete</Button>);
    expect(screen.getByRole('button')).toHaveClass('bg-destructive');
  });
});
Hook Testing
// hooks/use-counter.test.ts
import { renderHook, act } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { useCounter } from './use-counter';

describe('useCounter', () => {
  it('initializes with default value', () => {
    const { result } = renderHook(() => useCounter());
    expect(result.current.count).toBe(0);
  });

  it('initializes with provided value', () => {
    const { result } = renderHook(() => useCounter(10));
    expect(result.current.count).toBe(10);
  });

  it('increments count', () => {
    const { result } = renderHook(() => useCounter());

    act(() => {
      result.current.increment();
    });

    expect(result.current.count).toBe(1);
  });
});
Integration Testing
// __tests__/login.integration.test.tsx
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { LoginPage } from '@/app/(auth)/login/page';

// Mock the auth service
vi.mock('@/services/auth.service', () => ({
  authService: {
    login: vi.fn(),
  },
}));

describe('Login Page', () => {
  it('successfully logs in with valid credentials', async () => {
    const user = userEvent.setup();
    render(<LoginPage />);

    await user.type(screen.getByLabelText(/email/i), 'test@example.com');
    await user.type(screen.getByLabelText(/password/i), 'password123');
    await user.click(screen.getByRole('button', { name: /sign in/i }));

    await waitFor(() => {
      expect(authService.login).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'password123',
      });
    });
  });

  it('displays validation errors for invalid input', async () => {
    const user = userEvent.setup();
    render(<LoginPage />);

    await user.click(screen.getByRole('button', { name: /sign in/i }));

    await waitFor(() => {
      expect(screen.getByText(/invalid email/i)).toBeInTheDocument();
    });
  });
});
Test File Organization
src/
├── components/
│   └── ui/
│       ├── Button.tsx
│       └── Button.test.tsx      # Co-located unit tests
├── hooks/
│   ├── use-auth.ts
│   └── use-auth.test.ts
└── __tests__/                    # Integration/E2E tests
    ├── integration/
    │   └── login.test.tsx
    └── e2e/
        └── user-flow.spec.ts    # Playwright tests
Performance Guidelines
Code Splitting
// ✅ DO: Lazy load heavy components
import dynamic from 'next/dynamic';

// With loading state
const HeavyChart = dynamic(
  () => import('@/components/features/analytics/Chart'),
  {
    loading: () => <ChartSkeleton />,
    ssr: false, // Disable SSR for client-only components
  }
);

// ✅ DO: Use Next.js route-based code splitting (automatic with App Router)
Image Optimization
// ✅ DO: Always use next/image for images
import Image from 'next/image';

<Image
  src="/hero.jpg"
  alt="Hero image"
  width={1200}
  height={600}
  priority // For above-the-fold images
  placeholder="blur"
  blurDataURL={blurDataUrl}
/>

// ✅ DO: Use responsive images
<Image
  src="/product.jpg"
  alt="Product"
  fill
  sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw"
  className="object-cover"
/>
Memoization Guidelines
// ✅ DO: Memoize expensive calculations
const sortedAndFilteredData = useMemo(() => {
  return data
    .filter(item => item.status === 'active')
    .sort((a, b) => b.date - a.date);
}, [data]);

// ✅ DO: Memoize callbacks passed to children
const handleItemClick = useCallback((id: string) => {
  setSelectedId(id);
}, []);

// ✅ DO: Use React.memo for pure components that receive complex props
const ExpensiveList = memo(function ExpensiveList({ items }: Props) {
  return (
    <ul>
      {items.map(item => (
        <li key={item.id}>{item.name}</li>
      ))}
    </ul>
  );
});

// ❌ DON'T: Over-memoize simple operations or primitives
// The cost of memoization can exceed the benefit
Bundle Size
// ✅ DO: Import only what you need
import { format, parseISO } from 'date-fns'; // Not: import * as dateFns

// ✅ DO: Use dynamic imports for large libraries
const { PDFDocument } = await import('pdf-lib');

// ✅ DO: Analyze bundle with Next.js built-in analyzer
// next.config.js
const withBundleAnalyzer = require('@next/bundle-analyzer')({
  enabled: process.env.ANALYZE === 'true',
});
Accessibility (a11y)
Semantic HTML
// ✅ DO: Use semantic HTML elements
<header>
  <nav aria-label="Main navigation">
    <ul>
      <li><a href="/">Home</a></li>
      <li><a href="/about">About</a></li>
    </ul>
  </nav>
</header>

<main>
  <article>
    <h1>Page Title</h1>
    <section aria-labelledby="section-heading">
      <h2 id="section-heading">Section Title</h2>
      <p>Content...</p>
    </section>
  </article>
</main>

<footer>
  {/* Footer content */}
</footer>
Interactive Elements
// ✅ DO: Ensure all interactive elements are accessible
<button
  type="button"
  onClick={handleClick}
  aria-label="Close dialog"  // When text isn't descriptive enough
  aria-expanded={isOpen}     // For expandable elements
  aria-pressed={isActive}    // For toggle buttons
>
  <XIcon aria-hidden="true" />
</button>

// ✅ DO: Make custom components keyboard accessible
function CustomSelect({ options, value, onChange }: Props) {
  return (
    <div
      role="listbox"
      tabIndex={0}
      aria-activedescendant={`option-${value}`}
      onKeyDown={handleKeyDown}
    >
      {options.map(option => (
        <div
          key={option.id}
          id={`option-${option.id}`}
          role="option"
          aria-selected={option.id === value}
          onClick={() => onChange(option.id)}
        >
          {option.label}
        </div>
      ))}
    </div>
  );
}
Form Accessibility
// ✅ DO: Associate labels with inputs
<div>
  <label htmlFor="email">Email address</label>
  <input
    id="email"
    type="email"
    aria-describedby="email-error email-hint"
    aria-invalid={!!errors.email}
  />
  <p id="email-hint" className="text-sm text-gray-500">
    We'll never share your email
  </p>
  {errors.email && (
    <p id="email-error" role="alert" className="text-sm text-red-600">
      {errors.email.message}
    </p>
  )}
</div>
Focus Management
// ✅ DO: Manage focus for modals and dialogs
function Modal({ isOpen, onClose, children }: ModalProps) {
  const modalRef = useRef<HTMLDivElement>(null);
  const previousFocusRef = useRef<HTMLElement | null>(null);

  useEffect(() => {
    if (isOpen) {
      previousFocusRef.current = document.activeElement as HTMLElement;
      modalRef.current?.focus();
    } else {
      previousFocusRef.current?.focus();
    }
  }, [isOpen]);

  return (
    <div
      ref={modalRef}
      role="dialog"
      aria-modal="true"
      aria-labelledby="modal-title"
      tabIndex={-1}
    >
      <h2 id="modal-title">Modal Title</h2>
      {children}
    </div>
  );
}
Security Practices
Input Sanitization
// ✅ DO: Validate all inputs with Zod
const userInputSchema = z.object({
  name: z.string().min(1).max(100).trim(),
  email: z.string().email(),
  bio: z.string().max(500).optional(),
});

// ✅ DO: Sanitize HTML content if you must render it
import DOMPurify from 'dompurify';

function SafeHTML({ content }: { content: string }) {
  const sanitized = DOMPurify.sanitize(content, {
    ALLOWED_TAGS: ['p', 'b', 'i', 'em', 'strong', 'a'],
    ALLOWED_ATTR: ['href'],
  });

  return <div dangerouslySetInnerHTML={{ __html: sanitized }} />;
}
Environment Variables
// ✅ DO: Validate environment variables at build time
// lib/env.ts
import { z } from 'zod';

const envSchema = z.object({
  DATABASE_URL: z.string().url(),
  NEXTAUTH_SECRET: z.string().min(32),
  NEXT_PUBLIC_API_URL: z.string().url(),
});

export const env = envSchema.parse({
  DATABASE_URL: process.env.DATABASE_URL,
  NEXTAUTH_SECRET: process.env.NEXTAUTH_SECRET,
  NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL,
});

// ❌ DON'T: Expose sensitive keys to the client
// Only NEXT_PUBLIC_* variables are available client-side
Authentication
// ✅ DO: Protect API routes
// middleware.ts
import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

export function middleware(request: NextRequest) {
  const token = request.cookies.get('session-token');

  if (!token && request.nextUrl.pathname.startsWith('/dashboard')) {
    return NextResponse.redirect(new URL('/login', request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/dashboard/:path*', '/api/protected/:path*'],
};
Git & Version Control
Commit Message Convention (Conventional Commits)
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
Types: - feat: New feature - fix: Bug fix - docs: Documentation changes - style: Code style changes (formatting, semicolons, etc.) - refactor: Code refactoring (no feature or bug fix) - perf: Performance improvements - test: Adding or updating tests - chore: Build process, dependencies, or tooling changes

Examples:

feat(auth): add OAuth2 login with Google

fix(ui): resolve button alignment issue on mobile

docs(readme): update installation instructions

refactor(api): extract validation logic into middleware

chore(deps): upgrade React to v18.3
Branch Naming
<type>/<ticket-id>-<short-description>

Examples:
feat/PROJ-123-user-authentication
fix/PROJ-456-cart-total-calculation
refactor/PROJ-789-api-client
Pull Request Guidelines
Title: Use conventional commit format
Description: Include:
What changed and why
Screenshots/videos for UI changes
Testing instructions
Related tickets/issues
Size: Keep PRs small and focused (<400 lines when possible)
Reviews: Require at least one approval before merging
Code Review Checklist
Before submitting or approving code, verify:

Functionality
[ ] Code works as expected
[ ] Edge cases are handled
[ ] No regressions introduced
Code Quality
[ ] Follows project conventions
[ ] No unnecessary complexity
[ ] DRY (Don't Repeat Yourself)
[ ] Functions are small and focused
TypeScript
[ ] No any types without justification
[ ] Proper type definitions
[ ] No type assertions without comments
Performance
[ ] No unnecessary re-renders
[ ] Large lists are virtualized
[ ] Images are optimized
Security
[ ] User input is validated
[ ] No sensitive data exposed
[ ] Authentication/authorization checked
Testing
[ ] Unit tests for new logic
[ ] Integration tests for features
[ ] Tests are meaningful (not just for coverage)
Accessibility
[ ] Semantic HTML used
[ ] Keyboard navigation works
[ ] Screen reader friendly
Anti-Patterns to Avoid
React Anti-Patterns
// ❌ DON'T: Mutate state directly
const [items, setItems] = useState([]);
items.push(newItem); // Bad!
setItems([...items, newItem]); // Good

// ❌ DON'T: Use array index as key for dynamic lists
{items.map((item, index) => <Item key={index} />)} // Bad!
{items.map(item => <Item key={item.id} />)} // Good

// ❌ DON'T: Create components inside components
function Parent() {
  // This creates a new component on every render!
  const Child = () => <div>Child</div>; // Bad!
  return <Child />;
}

// ❌ DON'T: Use useEffect for derived state
const [items, setItems] = useState([]);
const [filteredItems, setFilteredItems] = useState([]);

useEffect(() => {
  setFilteredItems(items.filter(i => i.active)); // Bad!
}, [items]);

// Good: Compute during render
const filteredItems = items.filter(i => i.active);
// Or use useMemo if expensive
const filteredItems = useMemo(() => items.filter(i => i.active), [items]);

// ❌ DON'T: Fetch data in useEffect without cleanup
useEffect(() => {
  fetch('/api/data').then(r => r.json()).then(setData); // Bad!
}, []);

// Good: Handle race conditions
useEffect(() => {
  let cancelled = false;
  fetch('/api/data')
    .then(r => r.json())
    .then(data => {
      if (!cancelled) setData(data);
    });
  return () => { cancelled = true; };
}, []);
// Better: Use TanStack Query or SWR
TypeScript Anti-Patterns
// ❌ DON'T: Use `any`
const data: any = fetchData(); // Bad!
const data: unknown = fetchData(); // Good, then narrow the type

// ❌ DON'T: Use type assertions without validation
const user = data as User; // Bad!
// Good: Validate with Zod
const user = userSchema.parse(data);

// ❌ DON'T: Ignore TypeScript errors
// @ts-ignore // Bad!
// @ts-expect-error - Reason why this is necessary // Better, but still avoid

// ❌ DON'T: Use `!` without justification
const name = user!.name; // Bad!
// Good: Handle null case
const name = user?.name ?? 'Anonymous';
Next.js Anti-Patterns
// ❌ DON'T: Use 'use client' unnecessarily
// Only add when you need hooks, event handlers, or browser APIs

// ❌ DON'T: Fetch data client-side when server-side is possible
// Bad: useEffect + fetch in client component
// Good: Async server component with direct fetch

// ❌ DON'T: Import server-only code in client components
// This will leak server code to the client bundle

// ❌ DON'T: Use next/router in App Router
import { useRouter } from 'next/router'; // Bad!
import { useRouter } from 'next/navigation'; // Good for App Router
CSS/Tailwind Anti-Patterns
// ❌ DON'T: Use arbitrary values when utilities exist
className="mt-[16px]" // Bad!
className="mt-4"      // Good

// ❌ DON'T: Override Tailwind with inline styles
style={{ marginTop: '16px' }} // Bad!

// ❌ DON'T: Create long, unreadable class strings without organization
className="flex items-center justify-between p-4 bg-white rounded-lg shadow-md border border-gray-200 hover:shadow-lg transition-shadow duration-200 cursor-pointer" // Hard to read

// Good: Use cn() and organize logically
className={cn(
  'flex items-center justify-between',
  'rounded-lg border border-gray-200 bg-white p-4 shadow-md',
  'cursor-pointer transition-shadow duration-200 hover:shadow-lg'
)}
Quick Reference
Import Order
React/Next.js
Third-party libraries
Internal modules (@/)
Relative imports
Types (import type)
Component Structure
TypeScript interface/types
Component function
Hooks (useState, useEffect, custom)
Event handlers
Helper functions
Return JSX
File Naming
Components: PascalCase.tsx
Hooks: use-kebab-case.ts
Utils: kebab-case.ts
Types: kebab-case.types.ts
Last updated: 2024 Maintained by: Engineering Team