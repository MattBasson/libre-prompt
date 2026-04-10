import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { PromptService } from '../../../core/services/prompt.service';

@Component({
  selector: 'app-prompt-editor',
  standalone: true,
  imports: [
    ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule,
    MatIconModule, MatCardModule, MatChipsModule, MatCheckboxModule, MatSelectModule
  ],
  template: `
    <div class="editor-container">
      <h1>{{ isEditMode ? 'Edit Prompt' : 'New Prompt' }}</h1>

      <form [formGroup]="form" (ngSubmit)="save()">
        <mat-card>
          <mat-card-content>
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Title</mat-label>
              <input matInput formControlName="title" placeholder="Enter prompt title">
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Summary</mat-label>
              <input matInput formControlName="summary" placeholder="Brief description">
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Prompt Content</mat-label>
              <textarea matInput formControlName="content" rows="12"
                        placeholder="Enter prompt content..." class="mono"></textarea>
            </mat-form-field>

            <div class="chip-fields">
              <mat-form-field appearance="outline">
                <mat-label>Tags</mat-label>
                <mat-chip-grid #tagGrid>
                  @for (tag of tags; track tag) {
                    <mat-chip-row (removed)="removeTag(tag)">
                      {{ tag }}
                      <button matChipRemove><mat-icon>cancel</mat-icon></button>
                    </mat-chip-row>
                  }
                </mat-chip-grid>
                <input placeholder="Add tag..."
                       [matChipInputFor]="tagGrid"
                       [matChipInputSeparatorKeyCodes]="separatorKeyCodes"
                       (matChipInputTokenEnd)="addTag($event)">
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Categories</mat-label>
                <mat-chip-grid #catGrid>
                  @for (cat of categories; track cat) {
                    <mat-chip-row (removed)="removeCategory(cat)">
                      {{ cat }}
                      <button matChipRemove><mat-icon>cancel</mat-icon></button>
                    </mat-chip-row>
                  }
                </mat-chip-grid>
                <input placeholder="Add category..."
                       [matChipInputFor]="catGrid"
                       [matChipInputSeparatorKeyCodes]="separatorKeyCodes"
                       (matChipInputTokenEnd)="addCategory($event)">
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Models</mat-label>
                <mat-chip-grid #modelGrid>
                  @for (model of models; track model) {
                    <mat-chip-row (removed)="removeModel(model)">
                      {{ model }}
                      <button matChipRemove><mat-icon>cancel</mat-icon></button>
                    </mat-chip-row>
                  }
                </mat-chip-grid>
                <input placeholder="Add model..."
                       [matChipInputFor]="modelGrid"
                       [matChipInputSeparatorKeyCodes]="separatorKeyCodes"
                       (matChipInputTokenEnd)="addModel($event)">
              </mat-form-field>
            </div>

            <div class="row">
              <mat-form-field appearance="outline">
                <mat-label>Style</mat-label>
                <mat-select formControlName="promptStyle">
                  <mat-option value="instructional">Instructional</mat-option>
                  <mat-option value="conversational">Conversational</mat-option>
                  <mat-option value="template">Template</mat-option>
                </mat-select>
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Language</mat-label>
                <input matInput formControlName="language" placeholder="en-GB">
              </mat-form-field>

              <mat-form-field appearance="outline">
                <mat-label>Visibility</mat-label>
                <mat-select formControlName="visibility">
                  <mat-option value="private">Private</mat-option>
                  <mat-option value="shared">Shared</mat-option>
                  <mat-option value="public">Public</mat-option>
                </mat-select>
              </mat-form-field>
            </div>

            <h3>Variables</h3>
            <div formArrayName="variables">
              @for (v of variablesArray.controls; track $index; let i = $index) {
                <div [formGroupName]="i" class="variable-row">
                  <mat-form-field appearance="outline">
                    <mat-label>Name</mat-label>
                    <input matInput formControlName="name">
                  </mat-form-field>
                  <mat-checkbox formControlName="required">Required</mat-checkbox>
                  <button mat-icon-button type="button" (click)="removeVariable(i)">
                    <mat-icon>delete</mat-icon>
                  </button>
                </div>
              }
            </div>
            <button mat-button type="button" (click)="addVariable()">
              <mat-icon>add</mat-icon> Add Variable
            </button>

            <h3>Examples</h3>
            <div formArrayName="examples">
              @for (ex of examplesArray.controls; track $index; let i = $index) {
                <div [formGroupName]="i" class="example-row">
                  <mat-form-field appearance="outline" class="full-width">
                    <mat-label>Input</mat-label>
                    <textarea matInput formControlName="input" rows="2"></textarea>
                  </mat-form-field>
                  <mat-form-field appearance="outline" class="full-width">
                    <mat-label>Output Summary</mat-label>
                    <textarea matInput formControlName="outputSummary" rows="2"></textarea>
                  </mat-form-field>
                  <button mat-icon-button type="button" (click)="removeExample(i)">
                    <mat-icon>delete</mat-icon>
                  </button>
                </div>
              }
            </div>
            <button mat-button type="button" (click)="addExample()">
              <mat-icon>add</mat-icon> Add Example
            </button>
          </mat-card-content>

          <mat-card-actions align="end">
            <button mat-button type="button" (click)="cancel()">Cancel</button>
            <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid">
              {{ isEditMode ? 'Update' : 'Create' }}
            </button>
          </mat-card-actions>
        </mat-card>
      </form>
    </div>
  `,
  styles: [`
    .editor-container { padding: 24px; max-width: 900px; margin: 0 auto; }
    .full-width { width: 100%; }
    .mono textarea { font-family: monospace; }
    .chip-fields { display: flex; gap: 16px; flex-wrap: wrap; }
    .chip-fields mat-form-field { flex: 1; min-width: 250px; }
    .row { display: flex; gap: 16px; flex-wrap: wrap; }
    .row mat-form-field { flex: 1; }
    .variable-row, .example-row { display: flex; align-items: center; gap: 8px; margin-bottom: 8px; }
    .variable-row mat-form-field { flex: 1; }
  `]
})
export class PromptEditorComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly promptService = inject(PromptService);

  readonly separatorKeyCodes = [ENTER, COMMA];
  isEditMode = false;
  promptId: string | null = null;
  tags: string[] = [];
  categories: string[] = [];
  models: string[] = [];

  form = this.fb.group({
    title: ['', Validators.required],
    summary: [''],
    content: ['', Validators.required],
    promptStyle: ['instructional'],
    language: ['en-GB'],
    visibility: ['private'],
    variables: this.fb.array([]),
    examples: this.fb.array([])
  });

  get variablesArray(): FormArray { return this.form.get('variables') as FormArray; }
  get examplesArray(): FormArray { return this.form.get('examples') as FormArray; }

  ngOnInit(): void {
    this.promptId = this.route.snapshot.paramMap.get('id');
    if (this.promptId) {
      this.isEditMode = true;
      this.promptService.getById(this.promptId).subscribe(prompt => {
        this.form.patchValue({
          title: prompt.title,
          summary: prompt.summary,
          content: prompt.content,
          promptStyle: prompt.promptStyle,
          language: prompt.language,
          visibility: prompt.visibility
        });
        this.tags = [...prompt.tags];
        this.categories = [...prompt.categories];
        this.models = [...prompt.models];
        prompt.variables.forEach(v => this.variablesArray.push(
          this.fb.group({ name: v.name, required: v.required })
        ));
        prompt.examples.forEach(e => this.examplesArray.push(
          this.fb.group({ input: e.input, outputSummary: e.outputSummary })
        ));
      });
    }
  }

  addTag(event: MatChipInputEvent): void {
    const value = (event.value || '').trim();
    if (value) this.tags.push(value);
    event.chipInput.clear();
  }

  removeTag(tag: string): void {
    this.tags = this.tags.filter(t => t !== tag);
  }

  addCategory(event: MatChipInputEvent): void {
    const value = (event.value || '').trim();
    if (value) this.categories.push(value);
    event.chipInput.clear();
  }

  removeCategory(cat: string): void {
    this.categories = this.categories.filter(c => c !== cat);
  }

  addModel(event: MatChipInputEvent): void {
    const value = (event.value || '').trim();
    if (value) this.models.push(value);
    event.chipInput.clear();
  }

  removeModel(model: string): void {
    this.models = this.models.filter(m => m !== model);
  }

  addVariable(): void {
    this.variablesArray.push(this.fb.group({ name: '', required: false }));
  }

  removeVariable(index: number): void {
    this.variablesArray.removeAt(index);
  }

  addExample(): void {
    this.examplesArray.push(this.fb.group({ input: '', outputSummary: '' }));
  }

  removeExample(index: number): void {
    this.examplesArray.removeAt(index);
  }

  save(): void {
    if (this.form.invalid) return;

    const value = this.form.value;
    const request = {
      title: value.title!,
      content: value.content!,
      summary: value.summary || undefined,
      tags: this.tags,
      categories: this.categories,
      models: this.models,
      promptStyle: value.promptStyle || undefined,
      language: value.language || undefined,
      visibility: value.visibility || undefined,
      variables: value.variables as any[],
      examples: value.examples as any[]
    };

    const obs = this.isEditMode && this.promptId
      ? this.promptService.update(this.promptId, request)
      : this.promptService.create(request);

    obs.subscribe(result => {
      this.router.navigate(['/prompts', result.promptId]);
    });
  }

  cancel(): void {
    if (this.promptId) {
      this.router.navigate(['/prompts', this.promptId]);
    } else {
      this.router.navigate(['/prompts']);
    }
  }
}
