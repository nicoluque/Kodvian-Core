import { Component, EventEmitter, OnInit, Output, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { NavigationService } from '../core/services/navigation.service';
import { AuthSessionService } from '../core/auth/auth-session.service';
import { CurrentUser } from '../core/auth/auth.models';
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
  user: CurrentUser | null = null;

  ngOnInit(): void {
    this.authSession.ensureSessionLoaded().subscribe((user) => {
      this.user = user;
      this.items = this.navigationService.getItems(user);
    });
  }

  get userInitials(): string {
    const source = this.user?.fullName?.trim() || this.user?.email || '';
    const parts = source.split(/\s+/).filter(Boolean);
    if (parts.length >= 2) {
      return `${parts[0][0]}${parts[1][0]}`.toUpperCase();
    }

    return source.slice(0, 2).toUpperCase() || 'KC';
  }
}
