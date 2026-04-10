import { Routes } from '@angular/router';
import { PromptListComponent } from './features/prompts/prompt-list/prompt-list.component';
import { PromptDetailComponent } from './features/prompts/prompt-detail/prompt-detail.component';
import { PromptEditorComponent } from './features/prompts/prompt-editor/prompt-editor.component';
import { PromptSearchComponent } from './features/prompts/prompt-search/prompt-search.component';

export const routes: Routes = [
  { path: '', redirectTo: 'prompts', pathMatch: 'full' },
  { path: 'prompts', component: PromptListComponent },
  { path: 'prompts/new', component: PromptEditorComponent },
  { path: 'prompts/:id', component: PromptDetailComponent },
  { path: 'prompts/:id/edit', component: PromptEditorComponent },
  { path: 'search', component: PromptSearchComponent },
];
