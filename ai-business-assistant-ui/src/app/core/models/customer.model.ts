export interface Customer {
  customerId: number;
  name: string;
  email: string | null;
  phone: string | null;
  address: string | null;
  createdAt: string;
}

/** Body for POST /api/customers and PUT /api/customers/{id}. */
export interface CustomerPayload {
  name: string;
  email: string | null;
  phone: string | null;
  address: string | null;
}
