export interface Project {
  id: number;
  name: string;
  description: string;
  createdAt: string;
}

export interface CreateProjectRequest {
  name: string;
  description: string;
}

export interface UpdateProjectRequest {
  name: string;
  description: string;
}

export interface ProjectQuery {
  page: number;
  pageSize: number;
  search?: string;
  sortBy?: 'name' | 'createdAt';
  sortDirection?: 'asc' | 'desc';
}