import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { UserProfile } from './models/mi-perfil.models';
import { MiPerfilService } from './services/mi-perfil.service';

@Component({
  selector: 'app-mi-perfil-page',
  standalone: true,
  imports: [MatCardModule, MatButtonModule, MatIconModule, MatSnackBarModule, DatePipe],
  templateUrl: './mi-perfil-page.component.html',
  styleUrl: './mi-perfil-page.component.scss'
})
export class MiPerfilPageComponent implements OnInit {
  private readonly miPerfilService = inject(MiPerfilService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  perfil: UserProfile | null = null;
  cargando = false;
  desconectando = false;

  ngOnInit(): void {
    this.manejarQueryOAuth();
    this.cargarPerfil();
  }

  cargarPerfil(): void {
    this.cargando = true;
    this.miPerfilService.obtenerPerfil().subscribe({
      next: (perfil) => {
        this.perfil = perfil;
        this.cargando = false;
      },
      error: () => {
        this.perfil = null;
        this.cargando = false;
        this.snackBar.open('No se pudo cargar tu perfil', 'Cerrar', { duration: 3500 });
      }
    });
  }

  conectarGitHub(): void {
    window.location.assign(this.miPerfilService.getConnectUrl());
  }

  desconectarGitHub(): void {
    this.desconectando = true;
    this.miPerfilService.desconectarGitHub().subscribe({
      next: () => {
        this.desconectando = false;
        this.snackBar.open('Cuenta de GitHub desconectada correctamente', 'Cerrar', { duration: 3000 });
        this.cargarPerfil();
      },
      error: (error) => {
        this.desconectando = false;
        this.snackBar.open(error?.error?.message ?? 'No se pudo desconectar GitHub', 'Cerrar', { duration: 3500 });
      }
    });
  }

  private manejarQueryOAuth(): void {
    const connected = this.route.snapshot.queryParamMap.get('connected');
    const error = this.route.snapshot.queryParamMap.get('error');

    if (connected === 'true') {
      this.snackBar.open('GitHub se conectó correctamente', 'Cerrar', { duration: 3500 });
      void this.router.navigate([], { relativeTo: this.route, queryParams: {}, replaceUrl: true });
      return;
    }

    if (connected === 'false') {
      const mensaje =
        error === 'disabled'
          ? 'La integración con GitHub está deshabilitada'
          : 'No se pudo completar la conexión con GitHub';
      this.snackBar.open(mensaje, 'Cerrar', { duration: 4000 });
      void this.router.navigate([], { relativeTo: this.route, queryParams: {}, replaceUrl: true });
    }
  }
}
