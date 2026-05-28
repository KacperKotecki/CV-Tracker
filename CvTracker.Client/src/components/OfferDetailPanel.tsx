import type { JobOffer } from '../../models/JobOffer'
import type { JobOfferNote } from '../../models/JobOfferNote'
import type { ApplicationStatus } from '../../models/ApplicationStatus'
import type { TechnologyCategory } from '../../models/Technology'
import OfferDetailView from './OfferDetailView'
import OfferForm from './OfferForm'
import './OfferDetailPanel.css'

type PanelMode = 'empty' | 'detail' | 'edit' | 'add'

interface Props {
  mode: PanelMode
  offer: JobOffer | null
  notes: JobOfferNote[]
  categories: TechnologyCategory[]
  onEdit: () => void
  onSave: (dto: Partial<JobOffer>) => void
  onCancel: () => void
  onStatusChange: (id: number, status: ApplicationStatus) => void
  onAddNote: (content: string, date: string) => void
  onDeleteNote: (noteId: number) => void
}

export default function OfferDetailPanel({
  mode,
  offer,
  notes,
  categories,
  onEdit,
  onSave,
  onCancel,
  onStatusChange,
  onAddNote,
  onDeleteNote,
}: Props) {
  if (mode === 'empty') {
    return (
      <div className="offer-detail-panel offer-detail-panel--empty">
        <p>Wybierz ofertę z listy</p>
      </div>
    )
  }

  if (mode === 'add') {
    return (
      <div className="offer-detail-panel">
        <OfferForm offer={null} categories={categories} onSave={onSave} onCancel={onCancel} />
      </div>
    )
  }

  if (mode === 'edit' && offer) {
    return (
      <div className="offer-detail-panel">
        <OfferForm offer={offer} categories={categories} onSave={onSave} onCancel={onCancel} />
      </div>
    )
  }

  if (mode === 'detail' && offer) {
    return (
      <div className="offer-detail-panel">
        <OfferDetailView
          offer={offer}
          notes={notes}
          onEdit={onEdit}
          onStatusChange={onStatusChange}
          onAddNote={onAddNote}
          onDeleteNote={onDeleteNote}
        />
      </div>
    )
  }

  return null
}
