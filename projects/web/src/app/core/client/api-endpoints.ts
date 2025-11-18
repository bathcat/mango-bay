export const API_ENDPOINTS = {
  auth: {
    signUp: () => '/auth/web/signup',
    signIn: () => '/auth/web/signin',
    refresh: () => '/auth/web/refresh',
    signOut: () => '/auth/web/signout',
  },
  deliveries: {
    create: () => '/deliveries',
    get: (id: string) => `/deliveries/${id}`,
    calculateCost: () => '/deliveries/calculate-cost',
    myDeliveries: () => '/deliveries/my-deliveries',
    myAssignments: () => '/deliveries/my-assignments',
    byPilot: (pilotId: string) => `/deliveries/pilot/${pilotId}`,
    cancel: (id: string) => `/deliveries/${id}`,
    updateStatus: (id: string) => `/deliveries/${id}/status`,
    search: () => '/deliveries/search',
  },
  pilots: {
    list: () => '/pilots',
    get: (id: string) => `/pilots/${id}`,
  },
  sites: {
    list: () => '/sites',
    get: (id: string) => `/sites/${id}`,
    create: () => '/sites',
    update: (id: string) => `/sites/${id}`,
    delete: (id: string) => `/sites/${id}`,
    uploadImage: (id: string) => `/sites/${id}/image`,
  },
  reviews: {
    create: () => '/reviews',
    get: (id: string) => `/reviews/${id}`,
    byPilot: (pilotId: string) => `/reviews/pilot/${pilotId}`,
    byDelivery: (deliveryId: string) => `/reviews/delivery/${deliveryId}`,
    update: (id: string) => `/reviews/${id}`,
    delete: (id: string) => `/reviews/${id}`,
  },
  customers: {
    create: () => '/customers',
    get: (id: string) => `/customers/${id}`,
    update: (id: string) => `/customers/${id}`,
  },
  payments: {
    get: (id: string) => `/payments/${id}`,
    byDelivery: (deliveryId: string) => `/payments/by-delivery/${deliveryId}`,
    searchByCardholders: () => '/payments/search-by-cardholders',
  },
  proofs: {
    upload: (deliveryId: string) => `/proofs/deliveries/${deliveryId}/upload`,
    get: (deliveryId: string) => `/proofs/deliveries/${deliveryId}`,
    image: (deliveryId: string) => `/proofs/deliveries/${deliveryId}/image`,
  },
} as const;

