import './SkillsCard.css'

export interface SkillPillItem {
  id: number
  name: string
  levelLabel: string
}

interface SkillPillListProps {
  pills: SkillPillItem[]
  onRemove?: (id: number) => void
}

/**
 * Shared read-only (or removable) skill-pill strip.
 *
 * Renders the canonical `.skills-card__pill` markup that the Profile
 * SkillsCard uses, guaranteeing identical visuals in all job-offer views
 * (detail, add, edit) without duplicating markup or CSS.
 *
 * When `onRemove` is provided each pill gains a remove button; this covers
 * the OfferSkillPicker "selected skills" strip.
 */
export default function SkillPillList({ pills, onRemove }: SkillPillListProps) {
  if (pills.length === 0) return null

  return (
    <div className="skills-card__pills">
      {pills.map(pill => (
        <div key={pill.id} className="skills-card__pill">
          <span className="skills-card__pill-name">{pill.name}</span>
          {pill.levelLabel && (
            <span className="skills-card__pill-level">{pill.levelLabel}</span>
          )}
          {onRemove != null && (
            <button
              type="button"
              className="skills-card__pill-remove"
              onClick={() => onRemove(pill.id)}
              aria-label={`Usuń ${pill.name}`}
            >
              ×
            </button>
          )}
        </div>
      ))}
    </div>
  )
}
