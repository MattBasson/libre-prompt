import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule, MatIconModule],
  template: `
    <mat-toolbar color="primary">
      <span class="brand">Prompt Portal</span>
      <nav>
        <a mat-button routerLink="/prompts" routerLinkActive="active">
          <mat-icon>list</mat-icon> Prompts
        </a>
        <a mat-button routerLink="/search" routerLinkActive="active">
          <mat-icon>search</mat-icon> Search
        </a>
      </nav>
    </mat-toolbar>
    <router-outlet />
  `,
  styles: [`
    mat-toolbar {
      display: flex;
      gap: 16px;
    }
    .brand {
      font-weight: 600;
      margin-right: 24px;
    }
    nav {
      display: flex;
      gap: 4px;
    }
    .active {
      background-color: rgba(255, 255, 255, 0.15);
    }
  `]
})
export class AppComponent {}
