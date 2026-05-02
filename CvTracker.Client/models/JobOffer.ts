import type { Company } from './Company'
import type { SkillItem } from './SkillItem'

export interface JobOffer {
  id: number
  position: string
  salary: number
  contractType: string
  workMode: string
  workLoad: string
  company: Company
  skills: SkillItem[]
  ourRequirements: string
  whatWeOffer: string
  benefits: string
}

