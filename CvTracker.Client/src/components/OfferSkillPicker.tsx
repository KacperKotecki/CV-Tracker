import { useState } from 'react'
import type { TechnologyCategory } from '../../models/Technology'
import './OfferSkillPicker.css'

interface OfferSkillPickerProps {
  categories: TechnologyCategory[]
  value: number[]
  onChange: (ids: number[]) => void
}

export default function OfferSkillPicker({ categories, value, onChange }: OfferSkillPickerProps) {
  const [expandedCategories, setExpandedCategories] = useState<Set<string>>(new Set())

  const toggleCategory = (name: string) => {
    setExpandedCategories(prev => {
      const next = new Set(prev)
      if (next.has(name)) next.delete(name)
      else next.add(name)
      return next
    })
  }

  const toggleSkill = (id: number) => {
    if (value.includes(id)) {
      onChange(value.filter(v => v !== id))
    } else {
      onChange([...value, id])
    }
  }

  // Build a flat map id→name for pill display
  const nameById = new Map<number, string>()
  for (const cat of categories) {
    for (const tech of cat.technologies) {
      nameById.set(tech.id, tech.name)
    }
  }

  return (
    <div className="offer-skill-picker">
      {value.length > 0 && (
        <div className="offer-skill-picker__pills">
          {value.map(id => (
            <span key={id} className="offer-skill-picker__pill">
              {nameById.get(id) ?? String(id)}
              <button type="button" className="offer-skill-picker__pill-remove" onClick={() => toggleSkill(id)}>×</button>
            </span>
          ))}
        </div>
      )}
      <div className="offer-skill-picker__categories">
        {categories.map(cat => {
          const isExpanded = expandedCategories.has(cat.category)
          const selectedCount = cat.technologies.filter(t => value.includes(t.id)).length
          return (
            <div key={cat.category} className="offer-skill-picker__category">
              <button
                type="button"
                className="offer-skill-picker__category-header"
                onClick={() => toggleCategory(cat.category)}
              >
                <span>
                  {cat.category}
                  {selectedCount > 0 && (
                    <span className="offer-skill-picker__category-count"> ({selectedCount})</span>
                  )}
                </span>
                <span className={`offer-skill-picker__chevron${isExpanded ? ' offer-skill-picker__chevron--open' : ''}`}>▸</span>
              </button>
              {isExpanded && (
                <div className="offer-skill-picker__category-body">
                  {cat.technologies.map(tech => (
                    <button
                      key={tech.id}
                      type="button"
                      className={`offer-skill-picker__skill-btn${value.includes(tech.id) ? ' offer-skill-picker__skill-btn--active' : ''}`}
                      onClick={() => toggleSkill(tech.id)}
                    >
                      {tech.name}
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
