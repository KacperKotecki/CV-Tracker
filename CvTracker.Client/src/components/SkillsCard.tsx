import { useState } from 'react'
import type { UserTechnology } from '../../models/UserSkill'
import type { TechnologyCategory } from '../../models/Technology'
import './SkillsCard.css'

interface SkillsCardProps {
  skills: UserTechnology[]
  categories: TechnologyCategory[]
  isEditing: boolean
  onEditToggle: () => void
  onSkillToggle: (technologyId: number) => Promise<void>
  onProficiencyChange: (technologyId: number, proficiency: number) => Promise<void>
  onCancel: () => void
}

export default function SkillsCard({
  skills,
  categories,
  isEditing,
  onEditToggle,
  onSkillToggle,
  onProficiencyChange,
  onCancel,
}: SkillsCardProps) {
  const [expandedCategories, setExpandedCategories] = useState<Set<string>>(new Set())

  const toggleCategory = (name: string) => {
    setExpandedCategories(prev => {
      const next = new Set(prev)
      if (next.has(name)) {
        next.delete(name)
      } else {
        next.add(name)
      }
      return next
    })
  }

  const getSkill = (technologyId: number): UserTechnology | undefined =>
    skills.find(s => s.technologyId === technologyId)

  const isSelected = (technologyId: number): boolean =>
    getSkill(technologyId) !== undefined

  const getProficiency = (technologyId: number): number =>
    getSkill(technologyId)?.proficiency ?? 3

  if (!isEditing) {
    return (
      <div className="profile-card">
        <div className="profile-card__header">
          <h2 className="profile-card__title">Umiejętności</h2>
          <button className="profile-card__edit-btn" onClick={onEditToggle} title="Edytuj umiejętności">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
              <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
              <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
            </svg>
          </button>
        </div>
        {skills.length === 0 ? (
          <div className="skills-card__empty">
            Brak umiejętności — kliknij ołówek, aby dodać
          </div>
        ) : (
          <div className="skills-card__pills">
            {skills.map(skill => (
              <div key={skill.technologyId} className="skills-card__pill">
                <span className="skills-card__pill-name">{skill.technologyName}</span>
                <span className="skills-card__dots">
                  {Array.from({ length: 5 }, (_, i) => (
                    <span
                      key={i}
                      className={`skills-card__dot${i < skill.proficiency ? ' skills-card__dot--filled' : ''}`}
                    />
                  ))}
                </span>
              </div>
            ))}
          </div>
        )}
      </div>
    )
  }

  return (
    <div className="profile-card">
      <div className="profile-card__header">
        <h2 className="profile-card__title">Edytuj umiejętności</h2>
        <button className="profile-card__done-btn" onClick={onCancel}>
          Gotowe
        </button>
      </div>
      <div className="skills-card__editor">
        {categories.map(cat => {
          const isExpanded = expandedCategories.has(cat.category)
          const selectedInCategory = skills.filter(s => s.category === cat.category).length

          return (
            <div key={cat.category} className="skills-card__category">
              <button
                className="skills-card__category-header"
                onClick={() => toggleCategory(cat.category)}
                type="button"
              >
                <span>
                  {cat.category}
                  {selectedInCategory > 0 && (
                    <span className="skills-card__category-count"> ({selectedInCategory})</span>
                  )}
                </span>
                <span className={`skills-card__chevron${isExpanded ? ' skills-card__chevron--open' : ''}`}>
                  ▸
                </span>
              </button>
              {isExpanded && (
                <div className="skills-card__category-body">
                  {cat.technologies.map(tech => {
                    const active = isSelected(tech.id)
                    const proficiency = getProficiency(tech.id)

                    return (
                      <button
                        key={tech.id}
                        type="button"
                        className={`skills-card__skill-row${active ? ' skills-card__skill-row--active' : ''}`}
                        onClick={() => { void onSkillToggle(tech.id) }}
                      >
                        <span>{tech.name}</span>
                        {active && (
                          <span className="skills-card__dots" onClick={e => e.stopPropagation()}>
                            {Array.from({ length: 5 }, (_, i) => (
                              <span
                                key={i}
                                className={`skills-card__skill-dot${i < proficiency ? ' skills-card__skill-dot--filled' : ''}`}
                                onClick={e => {
                                  e.stopPropagation()
                                  void onProficiencyChange(tech.id, i + 1)
                                }}
                              />
                            ))}
                          </span>
                        )}
                      </button>
                    )
                  })}
                </div>
              )}
            </div>
          )
        })}
      </div>
    </div>
  )
}
