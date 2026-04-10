import { Component, Input } from '@angular/core';
import { MatChipsModule } from '@angular/material/chips';

@Component({
  selector: 'app-tag-chips',
  standalone: true,
  imports: [MatChipsModule],
  template: `
    <mat-chip-set>
      @for (tag of tags; track tag) {
        <mat-chip [highlighted]="highlighted">{{ tag }}</mat-chip>
      }
    </mat-chip-set>
  `,
  styles: [`
    mat-chip-set { display: inline-flex; gap: 4px; }
  `]
})
export class TagChipsComponent {
  @Input() tags: string[] = [];
  @Input() highlighted = false;
}
