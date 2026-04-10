import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { DatePipe } from '@angular/common';
import { PromptService } from '../../../core/services/prompt.service';
import { PromptDefinition } from '../../../models/prompt.model';
import { TagChipsComponent } from '../../../shared/components/tag-chips/tag-chips.component';

@Component({
  selector: 'app-prompt-detail',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatCardModule, MatChipsModule, MatDividerModule, DatePipe, TagChipsComponent],
  template: `
    @if (prompt) {
      <div class="detail-container">
        <div class="actions">
          <button mat-button (click)="goBack()">
            <mat-icon>arrow_back</mat-icon> Back
          </button>
          <div>
            <button mat-raised-button color="primary" (click)="edit()">
              <mat-icon>edit</mat-icon> Edit
            </button>
            <button mat-raised-button (click)="clonePrompt()">
              <mat-icon>content_copy</mat-icon> Clone
            </button>
            <button mat-raised-button color="warn" (click)="archivePrompt()">
              <mat-icon>archive</mat-icon> Archive
            </button>
          </div>
        </div>

        <mat-card>
          <mat-card-header>
            <mat-card-title>{{ prompt.title }}</mat-card-title>
            <mat-card-subtitle>
              Version {{ prompt.version }} &middot; {{ prompt.status }} &middot; {{ prompt.updatedUtc | date:'medium' }}
            </mat-card-subtitle>
          </mat-card-header>

          <mat-card-content>
            @if (prompt.summary) {
              <p class="summary">{{ prompt.summary }}</p>
              <mat-divider></mat-divider>
            }

            <div class="metadata">
              <div class="metadata-item">
                <strong>Tags:</strong>
                <app-tag-chips [tags]="prompt.tags"></app-tag-chips>
              </div>
              <div class="metadata-item">
                <strong>Categories:</strong>
                <app-tag-chips [tags]="prompt.categories"></app-tag-chips>
              </div>
              <div class="metadata-item">
                <strong>Models:</strong>
                <app-tag-chips [tags]="prompt.models"></app-tag-chips>
              </div>
              <div class="metadata-item">
                <strong>Style:</strong> {{ prompt.promptStyle }}
              </div>
              <div class="metadata-item">
                <strong>Language:</strong> {{ prompt.language }}
              </div>
            </div>

            <mat-divider></mat-divider>

            <h3>Prompt Content</h3>
            <pre class="prompt-content">{{ prompt.content }}</pre>

            @if (prompt.variables.length > 0) {
              <mat-divider></mat-divider>
              <h3>Variables</h3>
              <ul>
                @for (v of prompt.variables; track v.name) {
                  <li><strong>{{ v.name }}</strong> {{ v.required ? '(required)' : '(optional)' }}</li>
                }
              </ul>
            }

            @if (prompt.examples.length > 0) {
              <mat-divider></mat-divider>
              <h3>Examples</h3>
              @for (ex of prompt.examples; track $index) {
                <div class="example">
                  <p><strong>Input:</strong> {{ ex.input }}</p>
                  <p><strong>Output:</strong> {{ ex.outputSummary }}</p>
                </div>
              }
            }
          </mat-card-content>
        </mat-card>
      </div>
    }
  `,
  styles: [`
    .detail-container { padding: 24px; max-width: 900px; margin: 0 auto; }
    .actions { display: flex; justify-content: space-between; margin-bottom: 16px; }
    .actions div { display: flex; gap: 8px; }
    .summary { font-size: 1.1em; color: #555; margin: 16px 0; }
    .metadata { display: flex; flex-direction: column; gap: 8px; margin: 16px 0; }
    .metadata-item { display: flex; align-items: center; gap: 8px; }
    .prompt-content {
      background: #f5f5f5; padding: 16px; border-radius: 8px;
      white-space: pre-wrap; word-wrap: break-word; font-family: monospace;
    }
    .example { background: #fafafa; padding: 12px; border-radius: 4px; margin: 8px 0; }
    mat-divider { margin: 16px 0; }
  `]
})
export class PromptDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly promptService = inject(PromptService);

  prompt: PromptDefinition | null = null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.promptService.getById(id).subscribe(prompt => {
        this.prompt = prompt;
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/prompts']);
  }

  edit(): void {
    if (this.prompt) {
      this.router.navigate(['/prompts', this.prompt.promptId, 'edit']);
    }
  }

  clonePrompt(): void {
    if (this.prompt) {
      this.promptService.clone(this.prompt.promptId).subscribe(cloned => {
        this.router.navigate(['/prompts', cloned.promptId]);
      });
    }
  }

  archivePrompt(): void {
    if (this.prompt) {
      this.promptService.archive(this.prompt.promptId).subscribe(() => {
        this.router.navigate(['/prompts']);
      });
    }
  }
}
