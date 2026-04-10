import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { SearchRequest, SearchResult } from '../../models/search.model';

@Injectable({ providedIn: 'root' })
export class SearchService {
  private readonly http = inject(HttpClient);

  search(request: SearchRequest): Observable<SearchResult> {
    return this.http.post<SearchResult>('/api/prompts/search', request);
  }
}
