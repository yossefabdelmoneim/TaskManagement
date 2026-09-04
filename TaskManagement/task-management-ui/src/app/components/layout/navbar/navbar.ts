import { Component, effect, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isAuthenticated = signal(this.authService.isAuthenticated);
  readonly isAdmin = signal(this.authService.isAdmin);

  constructor() {
    effect(() => {
      this.isAuthenticated.set(this.authService.isAuthenticated);
      this.isAdmin.set(this.authService.isAdmin);
    });
  }

  async onLogout(): Promise<void> {
    await firstValueFrom(this.authService.logout()).catch(() => undefined);
    await this.router.navigate(['/login']);
  }
}