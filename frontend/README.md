# Patient Health Record System - Frontend

Modern React + TypeScript frontend for the Patient Health Record System, built with Vite and following enterprise coding standards.

## Tech Stack

- **Framework:** React 18 + TypeScript 5 (Strict Mode)
- **Build Tool:** Vite
- **Styling:** Tailwind CSS 3
- **State Management:** Zustand (global) + TanStack Query (server state)
- **Forms:** React Hook Form + Zod validation
- **Routing:** React Router DOM v6
- **HTTP Client:** Axios

## Project Structure

```
src/
├── components/
│   ├── ui/              # Primitive UI components (Button, Input, Card)
│   ├── layouts/         # Layout components (Header, Footer)
│   └── auth/            # Authentication components
├── pages/               # Page components
├── services/            # API service layer
├── stores/              # Zustand stores
├── hooks/               # Custom React hooks
├── lib/                 # Utilities and configurations
│   ├── api-client.ts    # Axios client
│   ├── utils.ts         # Helper functions
│   └── constants.ts     # App constants
├── types/               # TypeScript types
└── styles/              # Global styles
```

## Getting Started

### Prerequisites

- Node.js 18+ and npm/pnpm

### Installation

```bash
# Install dependencies
npm install

# Copy environment file
cp .env.example .env

# Start development server
npm run dev
```

### Environment Variables

Create a `.env` file:

```env
VITE_API_URL=http://localhost:5000/api/v1
```

## Available Scripts

- `npm run dev` - Start development server (http://localhost:3000)
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## Features

- ✅ JWT Authentication with automatic token management
- ✅ Protected routes with route guards
- ✅ Global state management with Zustand
- ✅ Server state caching with TanStack Query
- ✅ Type-safe API client with Axios interceptors
- ✅ Responsive design with Tailwind CSS
- ✅ Dark mode support
- ✅ Form validation with Zod schemas
- ✅ Component composition patterns
- ✅ Loading and error states

## Coding Standards

This project follows enterprise-grade coding standards as defined in `CLAUDE 1.md`:

- **TypeScript:** Strict mode, no `any`, proper type inference
- **Components:** Function declarations, named exports, compound patterns
- **Styling:** Tailwind CSS with `cn()` utility for class merging
- **State:** Zustand for global, TanStack Query for server, useState for local
- **File Naming:** PascalCase for components, kebab-case for utilities
- **Import Order:** React → Third-party → Internal → Relative → Types

## API Integration

The frontend integrates with the .NET backend API:

- **Base URL:** `http://localhost:5000/api/v1`
- **Auth Endpoints:** `/Auth/login`, `/Auth/register`, `/Auth/logout`
- **Health Records:** `/HealthRecords/*`
- **Access Requests:** `/AccessRequests/*`

## License

MIT
