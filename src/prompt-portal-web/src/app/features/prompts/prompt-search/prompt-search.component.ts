import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { DatePipe } from '@angular/common';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { SearchService } from '../../../core/services/search.service';
import { SearchResult, FacetValue } from '../../../models/search.model';
import { TagChipsComponent } from '../../../shared/components/tag-chips/tag-chips.component';

@Component({
  selector: 'app-prompt-search',
  standalone: true,
  imports: [
    FormsModule, MatFormFieldModule, MatInputModule, MatButtonModule,
    MatIconModule, MatCardModule, MatCheckboxModule, MatPaginatorModule,
    DatePipe, TagChipsComponent
  ],
  template: `
    <div class="search-container">
      <div class="search-header">
        <mat-form-field appearance="outline" class="search-field">
          <mat-label>Search prompts</mat-label>
          <input matInput [(ngModel)]="query" (ngModelChange)="onQueryChange($event)"
                 placeholder="Search by title, content, or description...">
          <mat-icon matPrefix>search</mat-icon>
        </mat-form-field>
      </div>

      <div class="search-body">
        <aside class="facets">
          @if (result?.facets?.tags?.length) {
            <h3>Tags</h3>
            @for (facet of result!.facets!.tags; track facet.value) {
              <mat-checkbox [checked]="selectedTags.has(facet.value)"
                            (change)="toggleTag(facet.value)">
                {{ facet.value }} ({{ facet.count }})
              </mat-checkbox>
            }
          }

          @if (result?.facets?.categories?.length) {
            <h3>Categories</h3>
            @for (facet of result!.facets!.categories; track facet.value) {
              <mat-checkbox [checked]="selectedCategories.has(facet.value)"
                            (change)="toggleCategory(facet.value)">
                {{ facet.value }} ({{ facet.count }})
              </mat-checkbox>
            }
          }

          @if (result?.facets?.models?.length) {
            <h3>Models</h3>
            @for (facet of result!.facets!.models; track facet.value) {
              <mat-checkbox [checked]="selectedModels.has(facet.value)"
                            (change)="toggleModel(facet.value)">
                {{ facet.value }} ({{ facet.count }})
              </mat-checkbox>
            }
          }
        </aside>

        <main class="results">
          @if (result) {
            <p class="result-count">{{ result.total }} result(s)</p>

            @for (item of result.results; track item.promptId) {
              <mat-card class="result-card" (click)="viewPrompt(item.promptId)">
                <mat-card-header>
                  <mat-card-title>{{ item.title }}</mat-card-title>
                  <mat-card-subtitle>{{ item.updatedUtc | date:'medium' }}</mat-card-subtitle>
                </mat-card-header>
                <mat-card-content>
                  <p>{{ item.summary }}</p>
                  <app-tag-chips [tags]="item.tags"></app-tag-chips>
                </mat-card-content>
              </mat-card>
            }

            <mat-paginator [length]="result.total"
                           [pageSize]="pageSize"
                           [pageIndex]="page - 1"
                           (page)="onPage($event)"
                           [pageSizeOptions]="[10, 20, 50]">
            </mat-paginator>
          }
        </main>
      </div>
    </div>
  `,
  styles: [`
    .search-container { padding: 24px; }
    .search-field { width: 100%; }
    .search-body { display: flex; gap: 24px; }
    .facets { min-width: 220px; display: flex; flex-direction: column; gap: 4px; }
    .facets h3 { margin: 16px 0 8px; }
    .results { flex: 1; }
    .result-card { margin-bottom: 12px; cursor: pointer; }
    .result-card:hover { box-shadow: 0 4px 12px rgba(0,0,0,0.15); }
    .result-count { color: #666; margin-bottom: 12px; }
  `]
})
export class PromptSearchComponent implements OnInit {
  private readonly searchService = inject(SearchService);
  private readonly router = inject(Router);
  private readonly searchSubject = new Subject<string>();

  query = '';
  page = 1;
  pageSize = 20;
  result: SearchResult | null = null;
  selectedTags = new Set<string>();
  selectedCategories = new Set<string>();
  selectedModels = new Set<string>();

  ngOnInit(): void {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => this.executeSearch());

    this.executeSearch();
  }

  onQueryChange(value: string): void {
    this.searchSubject.next(value);
  }

  toggleTag(tag: string): void {
    this.selectedTags.has(tag) ? this.selectedTags.delete(tag) : this.selectedTags.add(tag);
    this.page = 1;
    this.executeSearch();
  }

  toggleCategory(cat: string): void {
    this.selectedCategories.has(cat) ? this.selectedCategories.delete(cat) : this.selectedCategories.add(cat);
    this.page = 1;
    this.executeSearch();
  }

  toggleModel(model: string): void {
    this.selectedModels.has(model) ? this.selectedModels.delete(model) : this.selectedModels.add(model);
    this.page = 1;
    this.executeSearch();
  }

  onPage(event: PageEvent): void {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.executeSearch();
  }

  viewPrompt(id: string): void {
    this.router.navigate(['/prompts', id]);
  }

  private executeSearch(): void {
    this.searchService.search({
      query: this.query || undefined,
      tags: this.selectedTags.size > 0 ? [...this.selectedTags] : undefined,
      categories: this.selectedCategories.size > 0 ? [...this.selectedCategories] : undefined,
      models: this.selectedModels.size > 0 ? [...this.selectedModels] : undefined,
      page: this.page,
      pageSize: this.pageSize
    }).subscribe(result => {
      this.result = result;
    });
  }
}
