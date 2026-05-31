import { useState } from 'react'
import type { TechnologyCategory } from '../../models/Technology'
import './TechnologyPickerAccordion.css'

interface TechnologyPickerAccordionProps {
  categories: TechnologyCategory[]
  selectedIds: number[]
  /** offer: shows selection buttons only; profile: shows proficiency dots alongside each skill. */
  mode: 'offer' | 'profile'
  /** Called when a technology is toggled on or off. */
  onToggle: (id: number) => void
  /**
   * Profile mode only: returns the current UserTechnology for the given technologyId,
   * or undefined if not yet selected.
   */
  getSkill?: (technologyId: number) => { proficiency: number } | undefined
  /** Profile mode only: called when the user clicks a proficiency dot. */
  onProficiencyChange?: (technologyId: number, proficiency: number) => void
}

/**
 * Reusable collapsible category accordion for picking technologies.
 * Renders selection buttons in "offer" mode and adds interactive proficiency
 * dots in "profile" mode.
 */
export default function TechnologyPickerAccordion({
  categories,
  selectedIds,
  mode,
  onToggle,
  getSkill,
  onProficiencyChange,
}: TechnologyPickerAccordionProps) {
  const [expandedCategories, setExpandedCategories] = useState<Set<string>>(new Set())

  const toggleCategory = (name: string) => {
    setExpandedCategories(prev => {
      const next = new Set(prev)
      if (next.has(name)) next.delete(name)
      else next.add(name)
      return next
    })
  }

  return (
    <div className="tech-picker__categories">
      {categories.map(cat => {
        const isExpanded = expandedCategories.has(cat.category)
        const selectedCount = cat.technologies.filter(t => selectedIds.includes(t.id)).length

        return (
          <div key={cat.category}>
            <button
              type="button"
              className="tech-picker__category-header"
              onClick={() => toggleCategory(cat.category)}
            >
              <span>
                {cat.category}
                {selectedCount > 0 && (
                  <span className="tech-picker__category-count"> ({selectedCount})</span>
                )}
              </span>
              <span className={`tech-picker__chevron${isExpanded ? ' tech-picker__chevron--open' : ''}`}>
                ▸
              </span>
            </button>

            {isExpanded && (
              <div className="tech-picker__category-body">
                {mode === 'offer'
                  ? cat.technologies.map(tech => (
                      <button
                        key={tech.id}
                        type="button"
                        className={`tech-picker__skill-btn${selectedIds.includes(tech.id) ? ' tech-picker__skill-btn--active' : ''}`}
                        onClick={() => onToggle(tech.id)}
                      >
                        {tech.name}
                      </button>
                    ))
                  : cat.technologies.map(tech => {
                      const active = selectedIds.includes(tech.id)
                      const proficiency = getSkill?.(tech.id)?.proficiency ?? 3

                      return (
                        <button
                          key={tech.id}
                          type="button"
                          className={`tech-picker__skill-row${active ? ' tech-picker__skill-row--active' : ''}`}
                          onClick={() => onToggle(tech.id)}
                        >
                          <span>{tech.name}</span>
                          {active && (
                            <span
                              className="tech-picker__skill-dots"
                              onClick={e => e.stopPropagation()}
                            >
                              {Array.from({ length: 5 }, (_, i) => (
                                <span
                                  key={i}
                                  className={`tech-picker__skill-dot${i < proficiency ? ' tech-picker__skill-dot--filled' : ''}`}
                                  onClick={e => {
                                    e.stopPropagation()
                                    onProficiencyChange?.(tech.id, i + 1)
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
  )
}
