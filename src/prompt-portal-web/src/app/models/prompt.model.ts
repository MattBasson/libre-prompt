export interface PromptDefinition {
  promptId: string;
  tenantId: string;
  title: string;
  slug: string;
  content: string;
  summary: string;
  tags: string[];
  categories: string[];
  models: string[];
  promptStyle: string;
  language: string;
  visibility: string;
  status: string;
  version: number;
  supersedesPromptId?: string;
  variables: PromptVariable[];
  examples: PromptExample[];
  createdBy: string;
  createdUtc: string;
  updatedUtc: string;
}

export interface PromptVariable {
  name: string;
  required: boolean;
}

export interface PromptExample {
  input: string;
  outputSummary: string;
}

export interface CreatePromptRequest {
  title: string;
  content: string;
  summary?: string;
  tags: string[];
  categories: string[];
  models: string[];
  promptStyle?: string;
  language?: string;
  visibility?: string;
  variables?: PromptVariable[];
  examples?: PromptExample[];
}

export interface UpdatePromptRequest extends CreatePromptRequest {}
