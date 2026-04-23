export type { ApiResponse } from '../../../shared/models/api.models';

export interface DesarrolladorExterno {
  id: string;
  fullName: string;
  email?: string;
  phone?: string;
  taxId?: string;
  notes?: string;
  isActive: boolean;
}

export interface DesarrolladorFormulario {
  fullName: string;
  email?: string;
  phone?: string;
  taxId?: string;
  notes?: string;
  isActive: boolean;
}

export interface ResumenContratoDesarrollador {
  contractId: string;
  projectId: string;
  projectName: string;
  paymentMode: 'Percentage' | 'FixedAmount' | string;
  percentage?: number;
  agreedAmount?: number;
  totalDue: number;
  totalPaid: number;
  totalBalance: number;
  isUpToDate: boolean;
  lastPaymentDate?: string;
  isActive: boolean;
}
