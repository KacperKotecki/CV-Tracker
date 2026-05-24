export interface UserSkill {
  id: number
  skillId: number
  category: string
  skillName: string
  proficiency: number
}

export interface SkillItemRequest {
  skillId: number
  category: string
  skillName: string
  proficiency: number
}

export interface UpdateUserSkillsRequest {
  skills: SkillItemRequest[]
}
