import type { UserTechnology } from './UserSkill'

export interface UserProfile {
  id: number
  firstName: string | null
  lastName: string | null
  location: string | null
  linkedInUrl: string | null
  gitHubUrl: string | null
  websiteUrl: string | null
  avatarUrl: string | null
  resumeFileName: string | null
  resumeUrl: string | null
  skills: UserTechnology[]
}

export interface UpdateUserProfileRequest {
  firstName: string | null
  lastName: string | null
  location: string | null
  linkedInUrl: string | null
  gitHubUrl: string | null
  websiteUrl: string | null
}
