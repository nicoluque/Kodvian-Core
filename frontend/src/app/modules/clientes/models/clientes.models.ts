export type { ApiResponse, PagedResult } from '../../../shared/models/api.models';

export interface ClienteListado {
  id: string;
  commercialName: string;
  contactName?: string;
  contactEmail?: string;
  status: EstadoCliente;
  monthlyAmount?: number;
  isActive: boolean;
}

export interface ClienteDetalle {
  id: string;
  commercialName: string;
  legalName?: string;
  taxId?: string;
  contactName?: string;
  contactEmail?: string;
  contactPhone?: string;
  address?: string;
  city?: string;
  province?: string;
  country?: string;
  status: EstadoCliente;
  serviceType?: string;
  monthlyAmount?: number;
  billingDay?: number;
  notes?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface ClienteFormulario {
  commercialName: string;
  legalName?: string;
  taxId?: string;
  contactName?: string;
  contactEmail?: string;
  contactPhone?: string;
  address?: string;
  city?: string;
  province?: string;
  country?: string;
  status: EstadoCliente;
  serviceType?: string;
  monthlyAmount?: number | null;
  billingDay?: number | null;
  notes?: string;
  isActive: boolean;
}

export type EstadoCliente = 'Prospecto' | 'Activo' | 'Pausado' | 'Finalizado';

export interface ClientesFiltros {
  search?: string;
  status?: EstadoCliente | '';
  pageNumber: number;
  pageSize: number;
}
