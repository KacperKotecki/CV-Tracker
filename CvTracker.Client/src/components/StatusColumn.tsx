import type { JobOffer } from "../../models/JobOffer";
import type { ApplicationStatus } from "../../models/ApplicationStatus";
import './StatusColumn.css';
import './StatusCard.css';

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
}

export default function StatusColumn({ status, offers }: StatusColumnProps) {
  return (
    <div className="status-column" data-status={status}>
      <div className="status-column__header">
        <h2 className="status-column__title">{statusLabels[status]}</h2>
        <span className="status-column__count">{offers.length}</span>
      </div>

      <div className="status-column__cards">
        {offers.length === 0 ? (
          <p className="status-column__empty">Brak ofert</p>
        ) : (
          offers.map(offer => (
            <div key={offer.id} className="status-card">
              <p className="status-card__position">{offer.position}</p>
              <p className="status-card__company">{offer.company?.companyName}</p>
              {offer.salary ? (
                <p className="status-card__salary">
                  {offer.salary.toLocaleString('pl-PL')} PLN
                </p>
              ) : null}
              <div className="status-card__tags">
                {offer.contractType && <span className="tag">{offer.contractType}</span>}
                {offer.workMode && <span className="tag">{offer.workMode}</span>}
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}