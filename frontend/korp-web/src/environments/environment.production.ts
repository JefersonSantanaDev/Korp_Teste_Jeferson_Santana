// Revisit this value when the app is containerized (Etapa 15) — the
// Inventory base URL a browser reaches differs from the Docker-internal
// service DNS name used for Billing -> Inventory calls.
export const environment = {
  production: true,
  inventoryApiBaseUrl: 'http://localhost:5001/api',
  billingApiBaseUrl: 'http://localhost:5002/api',
};
