import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../../services/auth.service';
import { getApiErrorMessage } from '../../../utils/error.util';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly roleControl = this.fb.nonNullable.control('User');

  readonly form = this.fb.group({
    firstName: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    lastName: this.fb.nonNullable.control('', [Validators.required, Validators.maxLength(100)]),
    email: this.fb.nonNullable.control('', [Validators.required, Validators.email]),
    password: this.fb.nonNullable.control('', [Validators.required, Validators.minLength(6)]),
    role: this.roleControl
  });

  readonly loading = signal(false);
  readonly error = signal('');

  async onSubmit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set('');

    try {
      const { firstName, lastName, email, password, role } = this.form.getRawValue();
      await firstValueFrom(
        this.authService.register({ firstName, lastName, email, password, role })
      );
      await this.router.navigate(['/projects']);
    } catch (err) {
      this.error.set(getApiErrorMessage(err));
    } finally {
      this.loading.set(false);
    }
  }
}