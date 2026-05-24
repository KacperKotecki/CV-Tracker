import { useEffect, useRef, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import type { JobOffer } from '../../models/JobOffer'

type PollingState = 'polling' | 'done' | 'error'

const POLL_INTERVAL_MS = 2_000
const POLL_TIMEOUT_MS  = 60_000

/**
 * Displays a loading spinner while the backend scrapes a job offer.
 * Polls `GET /api/jobapplications/:id` every 2 s and navigates to the offer
 * detail view when status leaves `ScrapingInProgress`.
 * Times out after 60 s and shows a manual fallback link.
 */
export default function ScrapingPage() {
  const { id }     = useParams<{ id: string }>()
  const navigate   = useNavigate()
  const [state, setState] = useState<PollingState>('polling')
  const [sourceUrl, setSourceUrl] = useState<string | null>(null)

  // Use refs for the interval and elapsed-time counter so they are stable
  // across re-renders without triggering effects.
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null)
  const elapsedRef  = useRef(0)

  useEffect(() => {
    if (!id) {
      setState('error')
      return
    }

    const poll = async () => {
      elapsedRef.current += POLL_INTERVAL_MS

      if (elapsedRef.current > POLL_TIMEOUT_MS) {
        clearInterval(intervalRef.current!)
        setState('error')
        return
      }

      try {
        const res = await fetch(`/api/jobapplications/${id}`)
        if (!res.ok) {
          clearInterval(intervalRef.current!)
          setState('error')
          return
        }

        const offer: JobOffer = await res.json()

        // Store the original URL so we can show a fallback link on timeout.
        if (offer.sourceUrl && sourceUrl === null) {
          setSourceUrl(offer.sourceUrl)
        }

        if (offer.status !== 'ScrapingInProgress') {
          clearInterval(intervalRef.current!)
          setState('done')
          // Navigate to offer detail (OffersPage with the offer selected).
          navigate(`/?id=${id}`)
        }
      } catch {
        clearInterval(intervalRef.current!)
        setState('error')
      }
    }

    intervalRef.current = setInterval(poll, POLL_INTERVAL_MS)

    return () => {
      if (intervalRef.current !== null) {
        clearInterval(intervalRef.current)
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id])

  if (state === 'done') {
    return null
  }

  if (state === 'error') {
    return (
      <div style={{ textAlign: 'center', marginTop: '4rem' }}>
        <h2>Pobieranie nie powiodło się lub minął limit czasu</h2>
        <p>Spróbuj otworzyć ofertę ręcznie lub dodać ją bez pobierania danych.</p>
        {sourceUrl && (
          <a href={sourceUrl} target="_blank" rel="noopener noreferrer">
            Otwórz ofertę w nowej karcie
          </a>
        )}
        <br />
        <button onClick={() => navigate('/')} style={{ marginTop: '1rem' }}>
          Wróć do listy
        </button>
      </div>
    )
  }

  return (
    <div style={{ textAlign: 'center', marginTop: '4rem' }}>
      <div
        aria-label="Ładowanie"
        style={{
          width: '3rem',
          height: '3rem',
          border: '4px solid #e0e0e0',
          borderTopColor: '#3b82f6',
          borderRadius: '50%',
          animation: 'spin 0.8s linear infinite',
          margin: '0 auto 1.5rem',
        }}
      />
      <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
      <h2>Pobieranie danych oferty…</h2>
      <p>To może potrwać kilka sekund.</p>
    </div>
  )
}
