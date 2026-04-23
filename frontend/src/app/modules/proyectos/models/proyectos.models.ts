export type { ApiResponse, PagedResult } from '../../../shared/models/api.models';

export type EstadoProyecto = 'Planificacion' | 'EnCurso' | 'Pausado' | 'Finalizado' | 'Cancelado' | 'Presupuestado';
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

export type ModoPagoContrato = 'Percentage' | 'FixedAmount';

export interface DesarrolladorExterno {
  id: string;
  fullName: string;
  email?: string;
  phone?: string;
  taxId?: string;
  notes?: string;
  isActive: boolean;
}

export interface ContratoDesarrollador {
  id: string;
  projectId: string;
  projectName: string;
  developerId: string;
  developerName: string;
  paymentMode: ModoPagoContrato;
  percentage?: number;
  agreedAmount?: number;
  startDate: string;
  endDate?: string;
  isActive: boolean;
  notes?: string;
}

export interface PagoDesarrollador {
  id: string;
  contractId: string;
  paymentDate: string;
  amount: number;
  periodYear: number;
  periodMonth: number;
  reference?: string;
  notes?: string;
  receipts: ComprobanteArchivo[];
}

export interface ComprobanteArchivo {
  id: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedByName: string;
  uploadedAt: string;
}

export interface DocumentoProyecto {
  id: string;
  title: string;
  type: TipoDocumentoProyecto;
  typeLabel: string;
  currentVersionId: string;
  currentVersionNumber: number;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedByName: string;
  uploadedAt: string;
}

export interface VersionDocumentoProyecto {
  id: string;
  versionNumber: number;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedByName: string;
  uploadedAt: string;
  notes?: string | null;
}

export interface TipoDocumentoProyectoItem {
  value: TipoDocumentoProyecto;
  label: string;
}

export type TipoDocumentoProyecto = 'Contract' | 'Scope' | 'Proposal' | 'Deliverable' | 'Legal' | 'Invoice' | 'General';

export interface DesarrolladorFormulario {
  fullName: string;
  email?: string;
  phone?: string;
  taxId?: string;
  notes?: string;
  isActive: boolean;
}

export interface ContratoDesarrolladorFormulario {
  developerId: string;
  paymentMode: ModoPagoContrato;
  percentage?: number | null;
  agreedAmount?: number | null;
  startDate: string;
  endDate?: string | null;
  isActive: boolean;
  notes?: string;
}

export interface PagoDesarrolladorFormulario {
  paymentDate: string;
  amount: number;
  periodYear: number;
  periodMonth: number;
  reference?: string;
  notes?: string;
}

export interface LedgerContratoMes {
  year: number;
  month: number;
  projectIncomeBase: number;
  dueAmount: number;
  paidAmount: number;
  balance: number;
}

export interface LedgerContrato {
  contractId: string;
  projectId: string;
  projectName: string;
  developerId: string;
  developerName: string;
  paymentMode: ModoPagoContrato;
  percentage?: number;
  agreedAmount?: number;
  totalDue: number;
  totalPaid: number;
  totalBalance: number;
  months: LedgerContratoMes[];
}
