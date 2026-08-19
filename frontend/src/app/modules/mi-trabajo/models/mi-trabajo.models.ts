import { PagedResult } from '../../../shared/models/api.models';
import { ProyectoFiltros, ProyectoListado } from '../../proyectos/models/proyectos.models';
import { KanbanColumn, TareaDetalle, TareaFiltros, TareaListado } from '../../tareas/models/tareas.models';

export type { PagedResult, ProyectoFiltros, ProyectoListado, KanbanColumn, TareaDetalle, TareaFiltros, TareaListado };

export interface MiTrabajoOverview {
  projects: ProyectoListado[];
  tasks: TareaListado[];
}
