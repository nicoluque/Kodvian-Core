export type { ApiResponse, PagedResult } from '../../../shared/models/api.models';

export type EstadoProyecto = 'Planificacion' | 'EnCurso' | 'Pausado' | 'Finalizado' | 'Cancelado';
export type PrioridadProyecto = 'Baja' | 'Media' | 'Alta' | 'Urgente';

export interface ProyectoListado {
  id: string;
  name: string;
  clientId: string;
  clientName: string;
  responsibleId?: string;
  responsibleName?: string;
  status: EstadoProyecto;
  priority: PrioridadProyecto;
  startDate?: string;
  estimatedDeliveryDate?: string;
  progressPercentage: number;
  isActive: boolean;
}

export interface ProyectoDetalle {
  id: string;
  clientId: string;
  clientName: string;
  name: string;
  description?: string;
  responsibleId?: string;
  responsibleName?: string;
  status: EstadoProyecto;
  priority: PrioridadProyecto;
  startDate?: string;
  estimatedDeliveryDate?: string;
  closingDate?: string;
  budget?: number;
  progressPercentage: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface ProyectoFormulario {
  clientId: string;
  name: string;
  description?: string;
  responsibleId?: string | null;
  status: EstadoProyecto;
  priority: PrioridadProyecto;
  startDate?: string | null;
  estimatedDeliveryDate?: string | null;
  closingDate?: string | null;
  budget?: number | null;
  progressPercentage: number;
  isActive: boolean;
}

export interface ProyectoFiltros {
  pageNumber: number;
  pageSize: number;
  search?: string;
  clientId?: string;
  status?: EstadoProyecto | '';
  priority?: PrioridadProyecto | '';
}

export interface LookupItem {
  id: string;
  name: string;
}

export interface ProyectoLookups {
  clients: LookupItem[];
  responsibles: LookupItem[];
}
