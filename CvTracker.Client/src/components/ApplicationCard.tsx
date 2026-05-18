import type { JobOffer } from "../../models/JobOffer";
import './StatusCard.css';

interface ApplicationCardProps {
  offer: JobOffer
}

export default function ApplicationCard({ offer }: ApplicationCardProps) {
  return (
    <div className="status-card"
        draggable={true}
        onDragStart={(e) => {
            e.dataTransfer.setData('offerId', String(offer.id))
            setTimeout(() => {
                e.currentTarget.classList.add('dragging')
            }, 0)
        }}
        onDragEnd={(e) => {
            e.currentTarget.classList.remove('dragging')
        }}>
      {offer.sourceUrl
        ? <p className="status-card__position"><a href={offer.sourceUrl} target="_blank" rel="noopener noreferrer">{offer.position}</a></p>
        : <p className="status-card__position">{offer.position}</p>}
      <p className="status-card__company">{offer.companyName}</p>
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
  );
}


