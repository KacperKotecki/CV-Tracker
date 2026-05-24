import { useState } from 'react'
import { SKILL_CATEGORIES } from '../data/skillCategories'
import './OfferSkillPicker.css'

interface OfferSkillPickerProps {
  value: string[]
  onChange: (skills: string[]) => void
}

export default function OfferSkillPicker({ value, onChange }: OfferSkillPickerProps) {
  const [expandedCategories, setExpandedCategories] = useState<Set<string>>(new Set())

  const toggleCategory = (name: string) => {
    setExpandedCategories(prev => {
      const next = new Set(prev)
      if (next.has(name)) next.delete(name)
      else next.add(name)
      return next
    })
  }

  const toggleSkill = (skillName: string) => {
    if (value.includes(skillName)) {
      onChange(value.filter(s => s !== skillName))
    } else {
      onChange([...value, skillName])
    }
  }

  return (
    <div className="offer-skill-picker">
      {value.length > 0 && (
        <div className="offer-skill-picker__pills">
          {value.map(s => (
            <span key={s} className="offer-skill-picker__pill">
              {s}
              <button type="button" className="offer-skill-picker__pill-remove" onClick={() => toggleSkill(s)}>×</button>
            </span>
          ))}
        </div>
      )}
      <div className="offer-skill-picker__categories">
        {SKILL_CATEGORIES.map(cat => {
          const isExpanded = expandedCategories.has(cat.name)
          const selectedCount = cat.skills.filter(s => value.includes(s)).length
          return (
            <div key={cat.name} className="offer-skill-picker__category">
              <button
                type="button"
                className="offer-skill-picker__category-header"
                onClick={() => toggleCategory(cat.name)}
              >
                <span>
                  {cat.name}
                  {selectedCount > 0 && (
                    <span className="offer-skill-picker__category-count"> ({selectedCount})</span>
                  )}
                </span>
                <span className={`offer-skill-picker__chevron${isExpanded ? ' offer-skill-picker__chevron--open' : ''}`}>▸</span>
              </button>
              {isExpanded && (
                <div className="offer-skill-picker__category-body">
                  {cat.skills.map(skillName => (
                    <button
                      key={skillName}
                      type="button"
                      className={`offer-skill-picker__skill-btn${value.includes(skillName) ? ' offer-skill-picker__skill-btn--active' : ''}`}
                      onClick={() => toggleSkill(skillName)}
                    >
                      {skillName}
                    </button>
                  ))}
                </div>
              )}
            </div>
          )
        })}
      </div>
    </div>
  )
}
