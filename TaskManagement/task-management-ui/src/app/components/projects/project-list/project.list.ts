import { Component, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { PagedResponse } from '../../../models/common.models';
import { Project } from '../../../models/project.models';
import { AuthService } from '../../../services/auth.service';
import { ProjectService } from '../../../services/project.service';
import { getApiErrorMessage } from '../../../utils/error.util';

@Component({
  selector: 'app-project-list',
  imports: [RouterLink, DatePipe],
  templateUrl: './project.list.html',
  styleUrl: './project.list.css'
})
export class ProjectList implements OnInit {
  readonly projects = signal<Project[]>([]);
  readonly meta = signal<PagedResponse<Project> | null>(null);

  readonly loading = signal(false);
  readonly error = signal('');
  readonly deleting = signal<number | null>(null);
  readonly isAdmin = signal(false);

  readonly page = signal(1);
  readonly search = signal('');
  readonly sortBy = signal<'name' | 'createdAt'>('createdAt');
  readonly sortDirection = signal<'asc' | 'desc'>('desc');

  readonly pageSize = 8;

  constructor(
    private readonly projectService: ProjectService,
    private readonly authService: AuthService
  ) {}

  ngOnInit(): void {
    this.isAdmin.set(this.authService.isAdmin);
    void this.loadProjects();
  }

  async loadProjects(): Promise<void> {
    this.loading.set(true);
    this.error.set('');

    try {
      const response = await firstValueFrom(
        this.projectService.getProjects({
          page: this.page(),
          pageSize: this.pageSize,
          search: this.search() || undefined,
          sortBy: this.sortBy() || undefined,
          sortDirection: this.sortDirection()
        })
      );
      this.projects.set(response.items);
      this.meta.set(response);
    } catch (err) {
      this.error.set(getApiErrorMessage(err));
      this.projects.set([]);
      this.meta.set(null);
    } finally {
      this.loading.set(false);
    }
  }

  onSearch(value: string): void {
    this.search.set(value.trim());
    this.page.set(1);
    void this.loadProjects();
  }

  onSortChange(sortBy: 'name' | 'createdAt', direction: 'asc' | 'desc'): void {
    this.sortBy.set(sortBy);
    this.sortDirection.set(direction);
    this.page.set(1);
    void this.loadProjects();
  }

  changePage(newPage: number): void {
    if (newPage < 1 || newPage > (this.meta()?.totalPages ?? 1)) {
      return;
    }
    this.page.set(newPage);
    void this.loadProjects();
  }

  async onDelete(project: Project): Promise<void> {
    const confirmed = confirm(`Delete project "${project.name}"? This cannot be undone.`);
    if (!confirmed) {
      return;
    }

    this.deleting.set(project.id);
    this.error.set('');

    try {
      await firstValueFrom(this.projectService.deleteProject(project.id));
      await this.loadProjects();
    } catch (err) {
      this.error.set(getApiErrorMessage(err));
    } finally {
      this.deleting.set(null);
    }
  }
}