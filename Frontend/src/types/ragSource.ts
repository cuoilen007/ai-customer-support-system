export interface RagSource {
  sourceId: string;
  sourceType: string;
  relevanceScore: number;
  content: string;
}
