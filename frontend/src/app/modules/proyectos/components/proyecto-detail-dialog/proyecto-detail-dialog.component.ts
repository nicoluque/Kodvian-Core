import { CommonModule, CurrencyPipe } from '@angular/common';
import { Component, Inject, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';

import { AuthSessionService } from '../../../../core/auth/auth-session.service';
import { DocumentoProyecto, ProyectoDetalle, TipoDocumentoProyecto, TipoDocumentoProyectoItem, VersionDocumentoProyecto } from '../../models/proyectos.models';
import { ProyectosService } from '../../services/proyectos.service';

const PROJECTS_DOCUMENTS_READ = 'projects.documents.read';
const PROJECTS_DOCUMENTS_WRITE = 'projects.documents.write';
const PROJECTS_DOCUMENTS_DELETE = 'projects.documents.delete';

@Component({
  selector: 'app-proyecto-detail-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe, MatDialogModule, MatButtonModule],
  templateUrl: './proyecto-detail-dialog.component.html',
  styleUrl: './proyecto-detail-dialog.component.scss'
})
export class ProyectoDetailDialogComponent implements OnInit {
  private readonly proyectosService = inject(ProyectosService);
  private readonly authSession = inject(AuthSessionService);
  private readonly snackBar = inject(MatSnackBar);

  documentos: DocumentoProyecto[] = [];
  tiposDocumento: TipoDocumentoProyectoItem[] = [];
  versionesPorDocumento: Record<string, VersionDocumentoProyecto[]> = {};
  historialAbierto: Record<string, boolean> = {};
  cargandoDocumentos = false;
  subiendoDocumento = false;
  subiendoVersionId: string | null = null;
  cargandoVersiones: Record<string, boolean> = {};

  puedeLeerDocumentos = false;
  puedeEscribirDocumentos = false;
  puedeEliminarDocumentos = false;

  nuevoDocumento = {
    title: '',
    type: 'General' as TipoDocumentoProyecto,
    notes: ''
  };

  notasVersiones: Record<string, string> = {};

  constructor(@Inject(MAT_DIALOG_DATA) public readonly proyecto: ProyectoDetalle) {}

  ngOnInit(): void {
    this.authSession.ensureSessionLoaded().subscribe((user) => {
      const permissions = user?.permissions ?? [];
      this.puedeLeerDocumentos = permissions.includes(PROJECTS_DOCUMENTS_READ);
      this.puedeEscribirDocumentos = permissions.includes(PROJECTS_DOCUMENTS_WRITE);
      this.puedeEliminarDocumentos = permissions.includes(PROJECTS_DOCUMENTS_DELETE);

      if (!this.puedeLeerDocumentos) {
        return;
      }

      this.cargarDocumentos();
      this.cargarTiposDocumento();
    });
  }

  valor(texto?: string | number | null): string {
    return texto === null || texto === undefined || texto === '' ? '-' : String(texto);
  }

  mostrarEstado(estado: string): string {
    if (estado === 'Planificacion') return 'Planificación';
    if (estado === 'Presupuestado') return 'Presupuestado';
    if (estado === 'EnCurso') return 'En curso';
    return estado;
  }

  seleccionarArchivo(input: HTMLInputElement): void {
    input.click();
  }

  onArchivoSeleccionado(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    if (!this.nuevoDocumento.title.trim()) {
      this.snackBar.open('Debes indicar un título para el documento', 'Cerrar', { duration: 3200 });
      input.value = '';
      return;
    }

    this.subiendoDocumento = true;
    this.proyectosService
      .subirDocumentoProyecto(this.proyecto.id, {
        file,
        title: this.nuevoDocumento.title,
        type: this.nuevoDocumento.type,
        notes: this.nuevoDocumento.notes
      })
      .subscribe({
        next: (documento) => {
          this.subiendoDocumento = false;
          input.value = '';
          this.nuevoDocumento.notes = '';
          this.documentos = [documento, ...this.documentos.filter((d) => d.id !== documento.id)];
          this.snackBar.open('Documento cargado correctamente', 'Cerrar', { duration: 3000 });
        },
        error: (error) => {
          this.subiendoDocumento = false;
          input.value = '';
          this.snackBar.open(error?.error?.message ?? 'No se pudo cargar el documento', 'Cerrar', { duration: 3500 });
        }
      });
  }

  onNuevaVersionSeleccionada(documento: DocumentoProyecto, event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    this.subiendoVersionId = documento.id;
    this.proyectosService
      .subirVersionDocumentoProyecto(this.proyecto.id, documento.id, {
        file,
        notes: this.notasVersiones[documento.id]
      })
      .subscribe({
        next: (documentoActualizado) => {
          this.subiendoVersionId = null;
          input.value = '';
          this.notasVersiones[documento.id] = '';
          this.documentos = this.documentos.map((item) => (item.id === documentoActualizado.id ? documentoActualizado : item));
          if (this.historialAbierto[documento.id]) {
            this.cargarVersiones(documento.id);
          }
          this.snackBar.open('Nueva versión cargada correctamente', 'Cerrar', { duration: 3000 });
        },
        error: (error) => {
          this.subiendoVersionId = null;
          input.value = '';
          this.snackBar.open(error?.error?.message ?? 'No se pudo cargar la versión', 'Cerrar', { duration: 3500 });
        }
      });
  }

  descargarDocumento(documento: DocumentoProyecto): void {
    const url = `/api/projects/${this.proyecto.id}/documents/${documento.id}`;
    window.open(url, '_blank', 'noopener');
  }

  descargarVersion(documento: DocumentoProyecto, version: VersionDocumentoProyecto): void {
    const url = `/api/projects/${this.proyecto.id}/documents/${documento.id}/versions/${version.id}`;
    window.open(url, '_blank', 'noopener');
  }

  toggleHistorial(documentoId: string): void {
    this.historialAbierto[documentoId] = !this.historialAbierto[documentoId];
    if (this.historialAbierto[documentoId]) {
      this.cargarVersiones(documentoId);
    }
  }

  eliminarDocumento(documento: DocumentoProyecto): void {
    const confirmed = window.confirm(`Se eliminará el documento ${documento.title}. ¿Continuar?`);
    if (!confirmed) {
      return;
    }

    this.proyectosService.eliminarDocumentoProyecto(this.proyecto.id, documento.id).subscribe({
      next: () => {
        this.documentos = this.documentos.filter((item) => item.id !== documento.id);
        delete this.versionesPorDocumento[documento.id];
        delete this.historialAbierto[documento.id];
        this.snackBar.open('Documento eliminado correctamente', 'Cerrar', { duration: 3000 });
      },
      error: (error) => {
        this.snackBar.open(error?.error?.message ?? 'No se pudo eliminar el documento', 'Cerrar', { duration: 3500 });
      }
    });
  }

  formatBytes(size: number): string {
    if (size < 1024) return `${size} B`;
    if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} KB`;
    return `${(size / (1024 * 1024)).toFixed(2)} MB`;
  }

  private cargarDocumentos(): void {
    this.cargandoDocumentos = true;
    this.proyectosService.obtenerDocumentosProyecto(this.proyecto.id).subscribe({
      next: (data) => {
        this.documentos = data;
        this.cargandoDocumentos = false;
      },
      error: () => {
        this.documentos = [];
        this.cargandoDocumentos = false;
        this.snackBar.open('No se pudieron cargar los documentos del proyecto', 'Cerrar', { duration: 3500 });
      }
    });
  }

  private cargarTiposDocumento(): void {
    this.proyectosService.obtenerTiposDocumentoProyecto().subscribe({
      next: (tipos) => {
        this.tiposDocumento = tipos;
      },
      error: () => {
        this.tiposDocumento = [{ value: 'General', label: 'General' }];
      }
    });
  }

  private cargarVersiones(documentoId: string): void {
    this.cargandoVersiones[documentoId] = true;
    this.proyectosService.obtenerVersionesDocumentoProyecto(this.proyecto.id, documentoId).subscribe({
      next: (versiones) => {
        this.versionesPorDocumento[documentoId] = versiones;
        this.cargandoVersiones[documentoId] = false;
      },
      error: () => {
        this.versionesPorDocumento[documentoId] = [];
        this.cargandoVersiones[documentoId] = false;
        this.snackBar.open('No se pudo cargar el historial de versiones', 'Cerrar', { duration: 3200 });
      }
    });
  }
}
