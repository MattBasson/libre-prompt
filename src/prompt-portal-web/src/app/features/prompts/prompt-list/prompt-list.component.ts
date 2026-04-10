import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { DatePipe } from '@angular/common';
import { PromptService } from '../../../core/services/prompt.service';
import { PromptDefinition } from '../../../models/prompt.model';
import { TagChipsComponent } from '../../../shared/components/tag-chips/tag-chips.component';

@Component({
  selector: 'app-prompt-list',
  standalone: true,
  imports: [MatTableModule, MatButtonModule, MatIconModule, MatCardModule, MatChipsModule, DatePipe, TagChipsComponent],
  template: `
    <div class="prompt-list-container">
      <div class="header">
        <h1>Prompts</h1>
        <button mat-raised-button color="primary" (click)="createNew()">
          <mat-icon>add</mat-icon> New Prompt
        </button>
      </div>

      <table mat-table [dataSource]="prompts" class="mat-elevation-z2">
        <ng-container matColumnDef="title">
          <th mat-header-cell *matHeaderCellDef>Title</th>
          <td mat-cell *matCellDef="let prompt">{{ prompt.title }}</td>
        </ng-container>

        <ng-container matColumnDef="tags">
          <th mat-header-cell *matHeaderCellDef>Tags</th>
          <td mat-cell *matCellDef="let prompt">
            <app-tag-chips [tags]="prompt.tags"></app-tag-chips>
          </td>
        </ng-container>

        <ng-container matColumnDef="categories">
          <th mat-header-cell *matHeaderCellDef>Categories</th>
          <td mat-cell *matCellDef="let prompt">
            <app-tag-chips [tags]="prompt.categories"></app-tag-chips>
          </td>
        </ng-container>

        <ng-container matColumnDef="status">
          <th mat-header-cell *matHeaderCellDef>Status</th>
          <td mat-cell *matCellDef="let prompt">{{ prompt.status }}</td>
        </ng-container>

        <ng-container matColumnDef="updatedUtc">
          <th mat-header-cell *matHeaderCellDef>Updated</th>
          <td mat-cell *matCellDef="let prompt">{{ prompt.updatedUtc | date:'medium' }}</td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let prompt; columns: displayedColumns;"
            class="clickable-row"
            (click)="viewPrompt(prompt)"></tr>
      </table>
    </div>
  `,
  styles: [`
    .prompt-list-container { padding: 24px; }
    .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
    table { width: 100%; }
    .clickable-row { cursor: pointer; }
    .clickable-row:hover { background-color: rgba(0, 0, 0, 0.04); }
  `]
})
export class PromptListComponent implements OnInit {
  private readonly promptService = inject(PromptService);
  private readonly router = inject(Router);

  prompts: PromptDefinition[] = [];
  displayedColumns = ['title', 'tags', 'categories', 'status', 'updatedUtc'];

  ngOnInit(): void {
    this.promptService.list().subscribe(prompts => {
      this.prompts = prompts;
    });
  }

  createNew(): void {
    this.router.navigate(['/prompts/new']);
  }

  viewPrompt(prompt: PromptDefinition): void {
    this.router.navigate(['/prompts', prompt.promptId]);
  }
}
