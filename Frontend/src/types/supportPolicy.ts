export interface SupportPolicy {
  id: number;
  title: string;
  policyType: string;
  content: string;
  effectiveFrom: string;
  createdAt: string;
}

export interface SupportPolicyPayload {
  title: string;
  policyType: string;
  content: string;
  effectiveFrom: string;
}
