export type SkillLevel = 'Theory' | 'Beginner' | 'Junior' | 'Mid' | 'Senior' | 'Expert'

export const skillLevelOptions: SkillLevel[] = [
  'Theory', 'Beginner', 'Junior', 'Mid', 'Senior', 'Expert',
]

export const skillLevelLabels: Record<SkillLevel, string> = {
  Theory:   'Theory — Know by name',
  Beginner: 'Beginner — Personal projects',
  Junior:   'Junior — Simple tasks',
  Mid:      'Mid — Full independence',
  Senior:   'Senior — Complex problems',
  Expert:   'Expert — Architecture & mentoring',
}
