import type { JobOffer } from "../../models/JobOffer";
import type { ApplicationStatus } from "../../models/ApplicationStatus";
import ApplicationCard from './ApplicationCard';
import './StatusColumn.css';

const statusLabels: Record<ApplicationStatus, string> = {
  Draft:                  'Draft',
  Applied:                'Applied',
  HRScreening:            'HR Screening',
  TechnicalInterview:     'Technical Interview',
  LiveCodingOrAssignment: 'Live Coding / Assignment',
  AwaitingFeedback:       'Awaiting Feedback',
  Rejected:               'Rejected',
  Accepted:               'Accepted',
  Ghosted:                'Ghosted',
}

interface StatusColumnProps {
  status: ApplicationStatus
  offers: JobOffer[]
  onDrop: (offerId: number, newStatus: ApplicationStatus) => void
}

export default function StatusColumn({ status, offers, onDrop }: StatusColumnProps) {
  return (
    <div className="status-column" 
        data-status={status}
        onDragOver={(e) => e.preventDefault()}
        onDrop={(e) => {
            const offerId = Number(e.dataTransfer.getData('offerId'))
            if (!Number.isFinite(offerId) || offerId <= 0) {
                return
            }

        onDrop(offerId, status)
  }}>
      <div className="status-column__header">
        <h2 className="status-column__title">{statusLabels[status]}</h2>
        <span className="status-column__count">{offers.length}</span>
      </div>

      <div className="status-column__cards">
        {offers.length === 0 ? (
          <p className="status-column__empty">Brak ofert</p>
        ) : (
          offers.map(offer => (
            <ApplicationCard key={offer.id} offer={offer} />
          ))
        )}
      </div>
    </div>
  );
}