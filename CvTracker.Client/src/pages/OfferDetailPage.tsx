import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import type { JobOffer } from '../../models/JobOffer'

export default function OfferDetailPage() {
  const { id } = useParams()
  const [offer, setOffer] = useState<JobOffer | null>(null)

  useEffect(() => {
    if (id) {
      fetch(`http://localhost:5161/api/JobApplications/${id}`)
        .then(r => r.json())
        .then(setOffer)
    }
  }, [id])

  if (!offer) return <div>Ładowanie...</div>

  return (
    <div>
      <h2>{offer.position}</h2>
      {offer.sourceUrl && (
        <p>
          <strong>Źródło:</strong>{' '}
          <a href={offer.sourceUrl} target="_blank" rel="noopener noreferrer">
            {offer.sourceUrl}
          </a>
        </p>
      )}
    </div>
  )
}
