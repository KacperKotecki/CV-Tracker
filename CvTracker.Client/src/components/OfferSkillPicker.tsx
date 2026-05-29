import type { TechnologyCategory } from '../../models/Technology'
import TechnologyPickerAccordion from './TechnologyPickerAccordion'
import './OfferSkillPicker.css'

interface OfferSkillPickerProps {
  categories: TechnologyCategory[]
  value: number[]
  onChange: (ids: number[]) => void
}

export default function OfferSkillPicker({ categories, value, onChange }: OfferSkillPickerProps) {
  const toggleSkill = (id: number) => {
    if (value.includes(id)) {
      onChange(value.filter(v => v !== id))
    } else {
      onChange([...value, id])
    }
  }

  // Build a flat map id→name for pill display.
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
      <TechnologyPickerAccordion
        categories={categories}
        selectedIds={value}
        mode="offer"
        onToggle={toggleSkill}
      />
    </div>
  )
}
