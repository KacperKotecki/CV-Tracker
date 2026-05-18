import type { JobOffer } from '../../models/JobOffer'
import OfferListItem from './OfferListItem'
import './OfferListPanel.css'

interface Props {
  offers: JobOffer[]
  selectedId: number | null
  search: string
  onSearchChange: (value: string) => void
  onSelect: (id: number) => void
  onAddOffer: () => void
}

const FILTER_CHIPS = ['Wszystkie', 'HR Screening', 'Zdalnie', 'B2B']

export default function OfferListPanel({ offers, selectedId, search, onSearchChange, onSelect, onAddOffer }: Props) {
  return (
    <div className="offer-list-panel">
      <div className="offer-list-panel__header">
        <input
          className="form-field offer-list-panel__search"
          type="search"
          value={search}
          onChange={e => onSearchChange(e.target.value)}
          placeholder="Szukaj po firmie lub stanowisku..."
        />
        <div className="offer-list-panel__chips">
          {FILTER_CHIPS.map(chip => (
            <span key={chip} className="offer-list-panel__chip">{chip}</span>
          ))}
        </div>
        <button className="btn offer-list-panel__add-btn" onClick={onAddOffer}>
          + Dodaj ofertę
        </button>
      </div>
      <div className="offer-list-panel__list">
        {offers.length === 0 && (
          <p className="offer-list-panel__empty">Brak ofert.</p>
        )}
        {offers.map(offer => (
          <OfferListItem
            key={offer.id}
            offer={offer}
            isSelected={offer.id === selectedId}
            onClick={() => onSelect(offer.id)}
          />
        ))}
      </div>
    </div>
  )
}
