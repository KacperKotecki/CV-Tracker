export interface Technology {
  id: number
  name: string
  category: string
}

export interface TechnologyCategory {
  category: string
  technologies: Technology[]
}
