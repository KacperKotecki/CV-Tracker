export interface UserTechnology {
  id: number
  technologyId: number
  technologyName: string
  category: string
  proficiency: number
}

export interface UserTechnologyItemRequest {
  technologyId: number
  proficiency: number
}

export interface UpdateUserTechnologiesRequest {
  technologies: UserTechnologyItemRequest[]
}
