import { Component, OnInit, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ProjectService } from '../../../services/project.service';
import { getApiErrorMessage } from '../../../utils/error.util';

@Component({
  selector: 'app-project-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './project.form.html',
  styleUrl: './project.form.css'
})
export class ProjectForm implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly projectService = inject(ProjectService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]]
  });

  readonly isEdit = signal(false);
  readonly projectId = signal<number | null>(null);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal('');

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.isEdit.set(true);
      this.projectId.set(Number(idParam));
      void this.loadProject();
    }
  }

  async loadProject(): Promise<void> {
    const id = this.projectId();
    if (id === null) {
      return;
    }

    this.loading.set(true);
    this.error.set('');

    try {
      const project = await firstValueFrom(this.projectService.getProjectById(id));
      this.form.patchValue({
        name: project.name,
        description: project.description
      });
    } catch (err) {
      this.error.set(getApiErrorMessage(err));
    } finally {
      this.loading.set(false);
    }
  }

  async onSubmit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set('');

    const payload = this.form.getRawValue();

    try {
      if (this.isEdit()) {
        const id = this.projectId();
        if (id === null) {
          throw new Error('Missing project id.');
        }
        await firstValueFrom(this.projectService.updateProject(id, payload));
      } else {
        await firstValueFrom(this.projectService.createProject(payload));
      }
      await this.router.navigate(['/projects']);
    } catch (err) {
      this.error.set(getApiErrorMessage(err));
    } finally {
      this.saving.set(false);
    }
  }
}