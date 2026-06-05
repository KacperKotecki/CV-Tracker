import type { JobOffer } from '../../models/JobOffer'
import type { JobOfferNote } from '../../models/JobOfferNote'
import { applicationStatusOptions, type ApplicationStatus } from '../../models/ApplicationStatus'
import { skillLevelLabels, type SkillLevel } from '../../models/SkillLevel'
import { salaryRange } from '../utils/offerUtils'
import NotesTimeline from './NotesTimeline'
import SkillPillList from './SkillPillList'
import './OfferDetailView.css'

interface Props {
  offer: JobOffer
  notes: JobOfferNote[]
  onEdit: () => void
  onStatusChange: (id: number, status: ApplicationStatus) => void
  onAddNote: (content: string, date: string) => void
  onDeleteNote: (noteId: number) => void
}

function formatDate(value: string | null): string {
  if (!value) return '—'
  return new Date(value).toLocaleString('pl-PL')
}

export default function OfferDetailView({ offer, notes, onEdit, onStatusChange, onAddNote, onDeleteNote }: Props) {
  const salary = salaryRange(offer)

  // Build a pill list from the parallel requiredSkillIds / requiredSkillNames arrays,
  // mapping each level string through skillLevelLabels so the label is identical to
  // the Profile SkillsCard pills.
  const pills = (offer.requiredSkillIds ?? []).map((id, i) => {
    const rawLevel = offer.requiredSkillLevels?.[id]
    return {
      id,
      name: offer.requiredSkillNames?.[i] ?? String(id),
      levelLabel: rawLevel != null
        ? (skillLevelLabels[rawLevel as SkillLevel] ?? rawLevel)
        : '',
    }
  })

  return (
    <div className="offer-detail-view">
      <div className="offer-detail-view__header">
        <div className="offer-detail-view__title-block">
          <h1 className="offer-detail-view__position">{offer.position}</h1>
          <p className="offer-detail-view__subtitle">
            {offer.companyName ?? '—'}{offer.location ? ` · ${offer.location}` : ''}
          </p>
        </div>
        <div className="offer-detail-view__actions">
          {offer.sourceUrl && (
            <a
              className="btn offer-detail-view__source-btn"
              href={offer.sourceUrl}
              target="_blank"
              rel="noopener noreferrer"
            >
              Otwórz źródło
            </a>
          )}
          <button className="btn" onClick={onEdit}>Edytuj</button>
          <select
            className="form-field offer-detail-view__status-select"
            value={offer.status}
            onChange={e => onStatusChange(offer.id, e.target.value as ApplicationStatus)}
          >
            {applicationStatusOptions.map(s => (
              <option key={s} value={s}>{s}</option>
            ))}
          </select>
        </div>
      </div>

      <div className="offer-detail-view__grid">
        <div className="offer-detail-view__field">
          <span className="offer-detail-view__label">Wynagrodzenie</span>
          <span className="offer-detail-view__value">{salary}</span>
        </div>
        <div className="offer-detail-view__field">
          <span className="offer-detail-view__label">Tryb pracy</span>
          <span className="offer-detail-view__value">{offer.workMode}</span>
        </div>
        <div className="offer-detail-view__field">
          <span className="offer-detail-view__label">Data aplikacji</span>
          <span className="offer-detail-view__value">{formatDate(offer.appliedAt)}</span>
        </div>
        <div className="offer-detail-view__field">
          <span className="offer-detail-view__label">Typ umowy</span>
          <span className="offer-detail-view__value">{offer.contractType}</span>
        </div>
        <div className="offer-detail-view__field">
          <span className="offer-detail-view__label">Wymiar czasu</span>
          <span className="offer-detail-view__value">{offer.workLoad}</span>
        </div>
        {offer.recruiterName && (
          <div className="offer-detail-view__field">
            <span className="offer-detail-view__label">Rekruter</span>
            <span className="offer-detail-view__value">
              {offer.recruiterName}
              {offer.recruiterContact ? ` · ${offer.recruiterContact}` : ''}
            </span>
          </div>
        )}
      </div>

      {pills.length > 0 && (
        <div className="offer-detail-view__skills">
          <SkillPillList pills={pills} />
        </div>
      )}

      <NotesTimeline
        notes={notes}
        offerId={offer.id}
        onAdd={onAddNote}
        onDelete={onDeleteNote}
      />
    </div>
  )
}
