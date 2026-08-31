import { PagedResult } from '../../../shared/models/api.models';
import { ProyectoFiltros, ProyectoListado } from '../../proyectos/models/proyectos.models';
import { KanbanColumn, TareaDetalle, TareaFiltros, TareaListado } from '../../tareas/models/tareas.models';

export type { PagedResult, ProyectoFiltros, ProyectoListado, KanbanColumn, TareaDetalle, TareaFiltros, TareaListado };

export interface MiTrabajoOverview {
  gitHubNotConnected: boolean;
  repositoryCount: number;
  totalIssuesCount: number;
  openIssuesCount: number;
  repositories: MiTrabajoRepositorio[];
  issues: MiTrabajoIssue[];
}

export interface MiTrabajoRepositorio {
  projectId: string;
  projectName: string;
  clientName: string;
  projectStatus: string;
  gitHubOwner: string;
  gitHubRepoName: string;
  fullName: string;
  gitHubRepoUrl?: string;
  gitHubRepoId?: number;
  openIssuesCount: number;
}

export interface MiTrabajoRepositoriosPage {
  gitHubNotConnected: boolean;
  items: MiTrabajoRepositorio[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

export type MiTrabajoIssueStatus = 'Open' | 'Closed';

export interface MiTrabajoIssue {
  id: string;
  projectId: string;
  projectName: string;
  title: string;
  repositoryFullName: string;
  gitHubIssueNumber: number;
  gitHubIssueUrl?: string;
  status: MiTrabajoIssueStatus | string;
  priority?: string;
  createdAt: string;
}

export interface MiTrabajoCreateIssueRequest {
  projectId: string;
  title: string;
  description?: string;
  priority?: 'Baja' | 'Media' | 'Alta' | 'Urgente' | '';
}

export interface MiTrabajoSyncResult {
  importedCount: number;
  updatedCount: number;
  skippedPullRequestsCount: number;
  repositoriesSynced: number;
}

export interface MiTrabajoIssueFiltros {
  pageNumber: number;
  pageSize: number;
  search?: string;
  projectId?: string;
  status?: 'Open' | 'Closed' | '';
  priority?: string;
}
