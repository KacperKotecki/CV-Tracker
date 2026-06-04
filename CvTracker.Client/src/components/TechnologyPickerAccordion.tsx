import { useState } from 'react'
import type { TechnologyCategory } from '../../models/Technology'
import { skillLevelOptions, type SkillLevel } from '../../models/SkillLevel'
import './TechnologyPickerAccordion.css'

interface TechnologyPickerAccordionProps {
  categories: TechnologyCategory[]
  selectedIds: number[]
  /** offer: shows selection buttons only; profile: shows level select alongside each skill. */
  mode: 'offer' | 'profile'
  /** Called when a technology is toggled on or off. */
  onToggle: (id: number) => void
  /**
   * Profile mode only: returns the current UserTechnology for the given technologyId,
   * or undefined if not yet selected.
   */
  getSkill?: (technologyId: number) => { level: SkillLevel } | undefined
  /** Profile mode only: called when the user changes the skill level. */
  onLevelChange?: (technologyId: number, level: SkillLevel) => void
  /** Offer mode only: returns the current required level for the given technologyId. */
  getOfferSkillLevel?: (id: number) => SkillLevel
  /** Offer mode only: called when the required level changes for a skill. */
  onOfferLevelChange?: (id: number, level: SkillLevel) => void
}

/**
 * Reusable collapsible category accordion for picking technologies.
 * Renders selection buttons in "offer" mode and adds interactive level
 * selects in "profile" mode.
 */
export default function TechnologyPickerAccordion({
  categories,
  selectedIds,
  mode,
  onToggle,
  getSkill,
  onLevelChange,
  getOfferSkillLevel,
  onOfferLevelChange,
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
                  ? cat.technologies.map(tech => {
                      const active = selectedIds.includes(tech.id)
                      const currentLevel = getOfferSkillLevel?.(tech.id) ?? 'Mid'
                      return (
                        <div key={tech.id} className="tech-picker__offer-row">
                          <button
                            type="button"
                            className={`tech-picker__skill-btn${active ? ' tech-picker__skill-btn--active' : ''}`}
                            onClick={() => onToggle(tech.id)}
                          >
                            {tech.name}
                          </button>
                          {active && (
                            <select
                              className="tech-picker__level-select"
                              value={currentLevel}
                              onClick={e => e.stopPropagation()}
                              onChange={e => {
                                e.stopPropagation()
                                onOfferLevelChange?.(tech.id, e.target.value as SkillLevel)
                              }}
                            >
                              {skillLevelOptions.map(lvl => (
                                <option key={lvl} value={lvl}>{lvl}</option>
                              ))}
                            </select>
                          )}
                        </div>
                      )
                    })
                  : cat.technologies.map(tech => {
                      const active = selectedIds.includes(tech.id)
                      const currentLevel = getSkill?.(tech.id)?.level ?? 'Mid'

                      return (
                        <button
                          key={tech.id}
                          type="button"
                          className={`tech-picker__skill-row${active ? ' tech-picker__skill-row--active' : ''}`}
                          onClick={() => onToggle(tech.id)}
                        >
                          <span>{tech.name}</span>
                          {active && (
                            <select
                              className="tech-picker__level-select"
                              value={currentLevel}
                              onClick={e => e.stopPropagation()}
                              onChange={e => {
                                e.stopPropagation()
                                onLevelChange?.(tech.id, e.target.value as SkillLevel)
                              }}
                            >
                              {skillLevelOptions.map(lvl => (
                                <option key={lvl} value={lvl}>{lvl}</option>
                              ))}
                            </select>
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
