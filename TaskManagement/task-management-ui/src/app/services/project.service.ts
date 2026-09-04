import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PagedResponse } from '../models/common.models';
import { CreateProjectRequest, Project, ProjectQuery, UpdateProjectRequest } from '../models/project.models';

@Injectable({ providedIn: 'root' })
export class ProjectService {
  private readonly baseUrl = `${environment.apiUrl}/projects`;

  constructor(private readonly http: HttpClient) {}

  getProjects(query: ProjectQuery): Observable<PagedResponse<Project>> {
    let params = new HttpParams()
      .set('page', query.page.toString())
      .set('pageSize', query.pageSize.toString());

    if (query.search) {
      params = params.set('search', query.search);
    }
    if (query.sortBy) {
      params = params.set('sortBy', query.sortBy);
    }
    if (query.sortDirection) {
      params = params.set('sortDirection', query.sortDirection);
    }

    return this.http.get<PagedResponse<Project>>(this.baseUrl, { params });
  }

  getProjectById(id: number): Observable<Project> {
    return this.http.get<Project>(`${this.baseUrl}/${id}`);
  }

  createProject(payload: CreateProjectRequest): Observable<Project> {
    return this.http.post<Project>(this.baseUrl, payload);
  }

  updateProject(id: number, payload: UpdateProjectRequest): Observable<Project> {
    return this.http.put<Project>(`${this.baseUrl}/${id}`, payload);
  }

  deleteProject(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}