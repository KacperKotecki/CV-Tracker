import type { ApplicationStatus } from './ApplicationStatus'

export interface JobOffer {
  id: number
  position: string
  salary: number
  contractType: string
  workMode: string
  workLoad: string
  companyName: string | null
  location: string | null
  skills: string | null
  ourRequirements: string | null
  whatWeOffer: string | null
  benefits: string | null
  status: ApplicationStatus
}

