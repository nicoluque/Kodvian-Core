import { Component, EventEmitter, Output, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { NavigationService } from '../core/services/navigation.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [MatListModule, MatIconModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent {
  private readonly navigationService = inject(NavigationService);

  @Output() readonly itemSelected = new EventEmitter<void>();

  readonly items = this.navigationService.getItems();
}
