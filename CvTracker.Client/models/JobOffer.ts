import type { Company } from './Company'

export interface JobOffer {
  id: number
  position: string
  salary: number
  contractType: string
  workMode: string
  workLoad: string
  company: Company
  skills: string
  ourRequirements: string
  whatWeOffer: string
  benefits: string
}

