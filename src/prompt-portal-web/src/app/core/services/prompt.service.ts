import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PromptDefinition, CreatePromptRequest, UpdatePromptRequest } from '../../models/prompt.model';

@Injectable({ providedIn: 'root' })
export class PromptService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/prompts';

  list(tag?: string, category?: string): Observable<PromptDefinition[]> {
    let params = new HttpParams();
    if (tag) params = params.set('tag', tag);
    if (category) params = params.set('category', category);
    return this.http.get<PromptDefinition[]>(this.baseUrl, { params });
  }

  getById(id: string): Observable<PromptDefinition> {
    return this.http.get<PromptDefinition>(`${this.baseUrl}/${id}`);
  }

  create(request: CreatePromptRequest): Observable<PromptDefinition> {
    return this.http.post<PromptDefinition>(this.baseUrl, request);
  }

  update(id: string, request: UpdatePromptRequest): Observable<PromptDefinition> {
    return this.http.put<PromptDefinition>(`${this.baseUrl}/${id}`, request);
  }

  clone(id: string, newTitle?: string): Observable<PromptDefinition> {
    return this.http.post<PromptDefinition>(`${this.baseUrl}/${id}/clone`, { newTitle });
  }

  archive(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/archive`, {});
  }
}
