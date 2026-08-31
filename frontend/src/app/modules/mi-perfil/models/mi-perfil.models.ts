export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  role: string;
  developerId?: string;
  gitHubConnected: boolean;
  gitHubUsername?: string;
  gitHubConnectedAt?: string;
}
