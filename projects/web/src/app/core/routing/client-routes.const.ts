export const ClientRoutes = {
  home: () => '/',
  customerDashboard: () => '/customer-dashboard',
  pilotDashboard: () => '/pilot-dashboard',
  admin: () => '/admin',
  unauthorized: () => '/unauthorized',
  
  auth: {
    signIn: () => '/signin',
    signUp: () => '/signup',
  },
  
  pilots: {
    list: () => '/pilots',
    detail: (id: string) => `/pilots/${id}`,
  },
  
  sites: {
    list: () => '/sites',
    detail: (id: string) => `/sites/${id}`,
  },
  
  booking: {
    new: () => '/new-booking',
  },
  
  deliveries: {
    list: () => '/my-deliveries',
    detail: (id: string) => `/deliveries/${id}`,
    searchCargo: () => '/search-cargo',
  },

  payments: {
    search: () => '/search-payments',
  },
  
  assignments: {
    list: () => '/assignments',
    detail: (id: string) => `/assignments/${id}`,
  },
  
  search: () => '/search',
} as const;

export function toRoutePath(url: string): string {
  return url.startsWith('/') ? url.slice(1) : url;
}

