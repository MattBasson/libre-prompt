import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class MetadataService {
  private readonly http = inject(HttpClient);

  getTags(): Observable<string[]> {
    return this.http.get<string[]>('/api/metadata/tags');
  }

  getCategories(): Observable<string[]> {
    return this.http.get<string[]>('/api/metadata/categories');
  }
}
