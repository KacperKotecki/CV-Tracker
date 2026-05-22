export interface UserSkill {
  id: number
  category: string
  skillName: string
  proficiency: number
}

export interface SkillItemRequest {
  category: string
  skillName: string
  proficiency: number
}

export interface UpdateUserSkillsRequest {
  skills: SkillItemRequest[]
}
