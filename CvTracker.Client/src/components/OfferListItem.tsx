import type { JobOffer } from '../../models/JobOffer'
import { salaryRange, formatRelativeDate, statusColor } from '../utils/offerUtils'
import './OfferListItem.css'

interface Props {
  offer: JobOffer
  isSelected: boolean
  onClick: () => void
}

export default function OfferListItem({ offer, isSelected, onClick }: Props) {
  const salary = salaryRange(offer)

  return (
    <button
      className={`offer-list-item${isSelected ? ' offer-list-item--selected' : ''}`}
      onClick={onClick}
    >
      <div className="offer-list-item__row1">
        <span className="offer-list-item__company">{offer.companyName ?? '—'}</span>
        <span className="offer-list-item__date">{formatRelativeDate(offer.appliedAt)}</span>
      </div>
      <div className="offer-list-item__position">{offer.position}</div>
      <div className="offer-list-item__row3">
        <span
          className="offer-list-item__dot"
          style={{ background: statusColor(offer.status) }}
        />
        <span className="offer-list-item__salary">{salary}</span>
      </div>
    </button>
  )
}
