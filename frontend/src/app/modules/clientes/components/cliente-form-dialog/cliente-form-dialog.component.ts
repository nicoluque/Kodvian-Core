import { Component, Inject, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { finalize } from 'rxjs';

import { CityOption, CountryOption, RegionOption } from '../../../../core/models/location.models';
import { LocationService } from '../../../../core/services/location.service';
import { ClienteDetalle, ClienteFormulario, ESTADO_CLIENTE_OPTIONS, EstadoCliente } from '../../models/clientes.models';

interface ClienteFormDialogData {
  cliente?: ClienteDetalle;
}

@Component({
  selector: 'app-cliente-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule
  ],
  templateUrl: './cliente-form-dialog.component.html',
  styleUrl: './cliente-form-dialog.component.scss'
})
export class ClienteFormDialogComponent {
  private static readonly MANUAL_OPTION = '__manual__';

  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<ClienteFormDialogComponent>);
  private readonly locationService = inject(LocationService);

  readonly estados = ESTADO_CLIENTE_OPTIONS;
  readonly data: ClienteFormDialogData;

  countries: CountryOption[] = [];
  regions: RegionOption[] = [];
  cities: CityOption[] = [];

  loadingRegions = false;
  loadingCities = false;

  private readonly initialCountryName: string;
  private readonly initialRegionName: string;
  private readonly initialCityName: string;

  readonly form = this.fb.group({
    commercialName: ['', [Validators.required, Validators.maxLength(200)]],
    legalName: ['', [Validators.maxLength(220)]],
    taxId: ['', [Validators.maxLength(20)]],
    contactName: ['', [Validators.maxLength(120)]],
    contactEmail: ['', [Validators.email, Validators.maxLength(120)]],
    contactPhone: ['', [Validators.maxLength(40)]],
    address: ['', [Validators.maxLength(180)]],
    countrySelection: [''],
    countryManual: ['', [Validators.maxLength(120)]],
    provinceSelection: [''],
    provinceManual: ['', [Validators.maxLength(120)]],
    citySelection: [''],
    cityManual: ['', [Validators.maxLength(120)]],
    status: ['Prospecto' as EstadoCliente, [Validators.required]],
    serviceType: ['', [Validators.maxLength(120)]],
    monthlyAmount: [null as number | null, [Validators.min(0)]],
    billingDay: [null as number | null, [Validators.min(1), Validators.max(31)]],
    notes: ['', [Validators.maxLength(1000)]],
    isActive: [true]
  });

  constructor(@Inject(MAT_DIALOG_DATA) data: ClienteFormDialogData | null) {
    this.data = data ?? {};
    this.initialCountryName = this.data.cliente?.country?.trim() ?? '';
    this.initialRegionName = this.data.cliente?.province?.trim() ?? '';
    this.initialCityName = this.data.cliente?.city?.trim() ?? '';

    if (this.data.cliente) {
      this.form.patchValue({
        commercialName: this.data.cliente.commercialName,
        legalName: this.data.cliente.legalName ?? '',
        taxId: this.data.cliente.taxId ?? '',
        contactName: this.data.cliente.contactName ?? '',
        contactEmail: this.data.cliente.contactEmail ?? '',
        contactPhone: this.data.cliente.contactPhone ?? '',
        address: this.data.cliente.address ?? '',
        status: this.data.cliente.status,
        serviceType: this.data.cliente.serviceType ?? '',
        monthlyAmount: this.data.cliente.monthlyAmount ?? null,
        billingDay: this.data.cliente.billingDay ?? null,
        notes: this.data.cliente.notes ?? '',
        isActive: this.data.cliente.isActive
      });
    }

    this.cargarPaises();
  }

  get isCountryManual(): boolean {
    return this.form.value.countrySelection === ClienteFormDialogComponent.MANUAL_OPTION;
  }

  get isProvinceManual(): boolean {
    return this.form.value.provinceSelection === ClienteFormDialogComponent.MANUAL_OPTION;
  }

  get isCityManual(): boolean {
    return this.form.value.citySelection === ClienteFormDialogComponent.MANUAL_OPTION;
  }

  onCountrySelectionChange(value: string): void {
    this.form.patchValue({ provinceSelection: '', provinceManual: '', citySelection: '', cityManual: '' });
    this.regions = [];
    this.cities = [];

    if (!value || value === ClienteFormDialogComponent.MANUAL_OPTION) {
      return;
    }

    this.cargarProvincias(value);
  }

  onProvinceSelectionChange(value: string): void {
    this.form.patchValue({ citySelection: '', cityManual: '' });
    this.cities = [];

    const countryCode = this.form.value.countrySelection ?? '';
    if (!value || value === ClienteFormDialogComponent.MANUAL_OPTION) {
      return;
    }

    if (!countryCode || countryCode === ClienteFormDialogComponent.MANUAL_OPTION) {
      return;
    }

    this.cargarCiudades(countryCode, value);
  }

  guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();

    const country = this.resolveLocationValue(raw.countrySelection ?? '', raw.countryManual ?? '', this.countries);
    const province = this.resolveLocationValue(raw.provinceSelection ?? '', raw.provinceManual ?? '', this.regions);
    const city = this.resolveLocationValue(raw.citySelection ?? '', raw.cityManual ?? '', this.cities);

    this.dialogRef.close({
      commercialName: raw.commercialName ?? '',
      legalName: raw.legalName || undefined,
      taxId: raw.taxId || undefined,
      contactName: raw.contactName || undefined,
      contactEmail: raw.contactEmail || undefined,
      contactPhone: raw.contactPhone || undefined,
      address: raw.address || undefined,
      city: city || undefined,
      province: province || undefined,
      country: country || undefined,
      status: raw.status ?? 'Prospecto',
      serviceType: raw.serviceType || undefined,
      monthlyAmount: raw.monthlyAmount,
      billingDay: raw.billingDay,
      notes: raw.notes || undefined,
      isActive: !!raw.isActive
    } as ClienteFormulario);
  }

  private cargarPaises(): void {
    this.locationService.getCountries().subscribe({
      next: (data) => {
        this.countries = data;
        this.aplicarPaisInicial();
      },
      error: () => {
        this.countries = [];
        this.form.patchValue({ countrySelection: ClienteFormDialogComponent.MANUAL_OPTION, countryManual: this.initialCountryName });
      }
    });
  }

  private cargarProvincias(countryCode: string): void {
    this.loadingRegions = true;
    this.locationService
      .getRegions(countryCode)
      .pipe(finalize(() => (this.loadingRegions = false)))
      .subscribe({
        next: (data) => {
          this.regions = data;
          this.aplicarProvinciaInicial();
        },
        error: () => {
          this.regions = [];
        }
      });
  }

  private cargarCiudades(countryCode: string, regionCode: string): void {
    this.loadingCities = true;
    this.locationService
      .getCities(countryCode, regionCode)
      .pipe(finalize(() => (this.loadingCities = false)))
      .subscribe({
        next: (data) => {
          this.cities = data;
          this.aplicarCiudadInicial();
        },
        error: () => {
          this.cities = [];
        }
      });
  }

  private aplicarPaisInicial(): void {
    if (!this.initialCountryName) {
      const argentina = this.countries.find((x) => x.code === 'AR');
      if (argentina) {
        this.form.patchValue({ countrySelection: argentina.code, countryManual: '' });
        this.cargarProvincias(argentina.code);
      }
      return;
    }

    const matched = this.findByName(this.countries, this.initialCountryName);
    if (matched) {
      this.form.patchValue({ countrySelection: matched.code, countryManual: '' });
      this.cargarProvincias(matched.code);
      return;
    }

    this.form.patchValue({
      countrySelection: ClienteFormDialogComponent.MANUAL_OPTION,
      countryManual: this.initialCountryName
    });
  }

  private aplicarProvinciaInicial(): void {
    if (!this.initialRegionName) {
      return;
    }

    const matched = this.findByName(this.regions, this.initialRegionName);
    if (matched) {
      this.form.patchValue({ provinceSelection: matched.code, provinceManual: '' });

      const countryCode = this.form.value.countrySelection ?? '';
      if (countryCode && countryCode !== ClienteFormDialogComponent.MANUAL_OPTION) {
        this.cargarCiudades(countryCode, matched.code);
      }

      return;
    }

    this.form.patchValue({
      provinceSelection: ClienteFormDialogComponent.MANUAL_OPTION,
      provinceManual: this.initialRegionName
    });
  }

  private aplicarCiudadInicial(): void {
    if (!this.initialCityName) {
      return;
    }

    const matched = this.findByName(this.cities, this.initialCityName);
    if (matched) {
      this.form.patchValue({ citySelection: matched.code, cityManual: '' });
      return;
    }

    this.form.patchValue({
      citySelection: ClienteFormDialogComponent.MANUAL_OPTION,
      cityManual: this.initialCityName
    });
  }

  private findByName(options: { name: string; code: string }[], value: string): { name: string; code: string } | undefined {
    const normalized = value.trim().toLowerCase();
    return options.find((x) => x.name.trim().toLowerCase() === normalized);
  }

  private resolveLocationValue(selection: string, manualValue: string, options: { code: string; name: string }[]): string {
    if (!selection) {
      return '';
    }

    if (selection === ClienteFormDialogComponent.MANUAL_OPTION) {
      return manualValue.trim();
    }

    return options.find((x) => x.code === selection)?.name ?? '';
  }
}
