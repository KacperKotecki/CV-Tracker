import type { SkillLevel } from './SkillLevel'

export interface UserTechnology {
  id: number
  technologyId: number
  technologyName: string
  category: string
  level: SkillLevel
}

export interface UserTechnologyItemRequest {
  technologyId: number
  level: SkillLevel
}

export interface UpdateUserTechnologiesRequest {
  technologies: UserTechnologyItemRequest[]
}
