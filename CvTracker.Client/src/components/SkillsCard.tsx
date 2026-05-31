import type { UserTechnology } from '../../models/UserSkill'
import type { TechnologyCategory } from '../../models/Technology'
import TechnologyPickerAccordion from './TechnologyPickerAccordion'
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

/**
 * Displays the user's skill pills in read-only mode and a TechnologyPickerAccordion
 * in edit mode. The accordion state (expanded categories) lives inside the accordion.
 */
export default function SkillsCard({
  skills,
  categories,
  isEditing,
  onEditToggle,
  onSkillToggle,
  onProficiencyChange,
  onCancel,
}: SkillsCardProps) {
  const selectedIds = skills.map(s => s.technologyId)

  const getSkill = (technologyId: number): { proficiency: number } | undefined =>
    skills.find(s => s.technologyId === technologyId)

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
        <TechnologyPickerAccordion
          categories={categories}
          selectedIds={selectedIds}
          mode="profile"
          onToggle={id => { void onSkillToggle(id) }}
          getSkill={getSkill}
          onProficiencyChange={(id, proficiency) => { void onProficiencyChange(id, proficiency) }}
        />
      </div>
    </div>
  )
}
