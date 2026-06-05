import type { TechnologyCategory } from '../../models/Technology'
import { skillLevelLabels, type SkillLevel } from '../../models/SkillLevel'
import TechnologyPickerAccordion from './TechnologyPickerAccordion'
import SkillPillList from './SkillPillList'
import './OfferSkillPicker.css'

export interface OfferSkillItem {
  technologyId: number
  requiredLevel: string
}

interface OfferSkillPickerProps {
  categories: TechnologyCategory[]
  value: OfferSkillItem[]
  onChange: (skills: OfferSkillItem[]) => void
}

export default function OfferSkillPicker({ categories, value, onChange }: OfferSkillPickerProps) {
  const selectedIds = value.map(s => s.technologyId)

  const toggleSkill = (id: number) => {
    if (selectedIds.includes(id)) {
      onChange(value.filter(s => s.technologyId !== id))
    } else {
      onChange([...value, { technologyId: id, requiredLevel: 'Mid' }])
    }
  }

  const handleLevelChange = (id: number, level: SkillLevel) => {
    onChange(value.map(s => s.technologyId === id ? { ...s, requiredLevel: level } : s))
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
      <SkillPillList
        pills={value.map(skill => ({
          id: skill.technologyId,
          name: nameById.get(skill.technologyId) ?? String(skill.technologyId),
          levelLabel: skillLevelLabels[skill.requiredLevel as SkillLevel] ?? skill.requiredLevel,
        }))}
        onRemove={toggleSkill}
      />
      <TechnologyPickerAccordion
        categories={categories}
        selectedIds={selectedIds}
        mode="offer"
        onToggle={toggleSkill}
        getOfferSkillLevel={id => (value.find(s => s.technologyId === id)?.requiredLevel ?? 'Mid') as SkillLevel}
        onOfferLevelChange={handleLevelChange}
      />
    </div>
  )
}
