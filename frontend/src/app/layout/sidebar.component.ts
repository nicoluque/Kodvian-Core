import { Component, EventEmitter, OnInit, Output, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { NavigationService } from '../core/services/navigation.service';
import { AuthSessionService } from '../core/auth/auth-session.service';
import { NavigationItem } from '../shared/models/navigation-item.model';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [MatListModule, MatIconModule, RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss'
})
export class SidebarComponent implements OnInit {
  private readonly navigationService = inject(NavigationService);
  private readonly authSession = inject(AuthSessionService);

  @Output() readonly itemSelected = new EventEmitter<void>();

  items: NavigationItem[] = [];

  ngOnInit(): void {
    this.authSession.ensureSessionLoaded().subscribe((user) => {
      this.items = this.navigationService.getItems(user);
    });
  }
}
