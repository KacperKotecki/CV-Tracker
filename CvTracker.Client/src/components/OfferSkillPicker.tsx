import { useState } from 'react'
import { SKILL_CATEGORIES } from '../data/skillCategories'
import type { SkillRef } from '../../models/JobOffer'
import './OfferSkillPicker.css'

interface OfferSkillPickerProps {
  value: SkillRef[]
  onChange: (skills: SkillRef[]) => void
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
    if (value.some(s => s.name === skillName)) {
      onChange(value.filter(s => s.name !== skillName))
    } else {
      onChange([...value, { id: 0, name: skillName }])
    }
  }

  return (
    <div className="offer-skill-picker">
      {value.length > 0 && (
        <div className="offer-skill-picker__pills">
          {value.map(s => (
            <span key={s.name} className="offer-skill-picker__pill">
              {s.name}
              <button type="button" className="offer-skill-picker__pill-remove" onClick={() => toggleSkill(s.name)}>×</button>
            </span>
          ))}
        </div>
      )}
      <div className="offer-skill-picker__categories">
        {SKILL_CATEGORIES.map(cat => {
          const isExpanded = expandedCategories.has(cat.name)
          const selectedCount = cat.skills.filter(s => value.some(r => r.name === s)).length
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
                      className={`offer-skill-picker__skill-btn${value.some(s => s.name === skillName) ? ' offer-skill-picker__skill-btn--active' : ''}`}
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
