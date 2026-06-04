import type { ApplicationStatus } from './ApplicationStatus'
import type { JobOfferNote } from './JobOfferNote'

export interface JobOffer {
  id: number
  position: string
  contractType: string
  workMode: string
  workLoad: string
  companyName: string | null
  location: string | null
  requiredSkillIds: number[]
  requiredSkillNames: string[]
  requiredSkillLevels: Record<number, string>
  requiredSkills?: Array<{ technologyId: number; requiredLevel: string }>
  matchScore: number | null
  sourceUrl?: string | null
  status: ApplicationStatus
  salaryMin: number | null
  salaryMax: number | null
  appliedAt: string | null
  followUpDate: string | null
  recruiterName: string | null
  recruiterContact: string | null
  sentCvVersion: string | null
  rejectionReason: string | null
  notes: JobOfferNote[]
}

