import { useNavigate } from "react-router-dom";
import type { JobOffer } from "../../models/JobOffer";
import './StatusCard.css';

interface ApplicationCardProps {
  offer: JobOffer
}

function salaryRange(offer: JobOffer): string | null {
  const { salaryMin, salaryMax } = offer
  if (salaryMin != null && salaryMax != null)
    return `${salaryMin.toLocaleString('pl-PL')} – ${salaryMax.toLocaleString('pl-PL')} PLN`
  if (salaryMin != null) return `od ${salaryMin.toLocaleString('pl-PL')} PLN`
  if (salaryMax != null) return `do ${salaryMax.toLocaleString('pl-PL')} PLN`
  return null
}

export default function ApplicationCard({ offer }: ApplicationCardProps) {
  const navigate = useNavigate()
  const salary = salaryRange(offer)

  return (
    <div className="status-card"
        draggable={true}
        onClick={() => navigate(`/offer/${offer.id}`)}
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
        ? <p className="status-card__position"><a href={offer.sourceUrl} target="_blank" rel="noopener noreferrer" onClick={e => e.stopPropagation()}>{offer.position}</a></p>
        : <p className="status-card__position">{offer.position}</p>}
      <p className="status-card__company">{offer.companyName}</p>
      {salary && (
        <p className="status-card__salary">
          {salary}
        </p>
      )}
      <div className="status-card__tags">
        {offer.contractType && <span className="tag">{offer.contractType}</span>}
        {offer.workMode && <span className="tag">{offer.workMode}</span>}
      </div>
    </div>
  );
}


