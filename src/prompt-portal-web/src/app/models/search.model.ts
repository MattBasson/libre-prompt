export interface SearchRequest {
  query?: string;
  tags?: string[];
  categories?: string[];
  models?: string[];
  status?: string;
  page: number;
  pageSize: number;
}

export interface SearchResult {
  results: SearchResultItem[];
  total: number;
  page: number;
  pageSize: number;
  facets: SearchFacets;
}

export interface SearchResultItem {
  promptId: string;
  title: string;
  summary: string;
  tags: string[];
  categories: string[];
  models: string[];
  status: string;
  score: number;
  updatedUtc: string;
}

export interface SearchFacets {
  tags: FacetValue[];
  categories: FacetValue[];
  models: FacetValue[];
}

export interface FacetValue {
  value: string;
  count: number;
}
