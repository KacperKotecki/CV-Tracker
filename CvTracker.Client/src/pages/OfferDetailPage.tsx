import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import type { JobOffer } from '../../models/JobOffer'
import type { JobOfferNote } from '../../models/JobOfferNote'

function salaryRange(offer: JobOffer): string | null {
  const { salaryMin, salaryMax } = offer
  if (salaryMin != null && salaryMax != null)
    return `${salaryMin.toLocaleString('pl-PL')} – ${salaryMax.toLocaleString('pl-PL')} PLN`
  if (salaryMin != null) return `od ${salaryMin.toLocaleString('pl-PL')} PLN`
  if (salaryMax != null) return `do ${salaryMax.toLocaleString('pl-PL')} PLN`
  return null
}

function formatDate(value: string | null): string {
  if (!value) return '—'
  return new Date(value).toLocaleString('pl-PL')
}

export default function OfferDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [offer, setOffer] = useState<JobOffer | null>(null)
  const [notes, setNotes] = useState<JobOfferNote[]>([])
  const [noteContent, setNoteContent] = useState('')
  const [noteDate, setNoteDate] = useState(() => new Date().toISOString().slice(0, 16))

  useEffect(() => {
    if (id) {
      fetch(`http://localhost:5161/api/JobApplications/${id}`)
        .then(r => r.json())
        .then(setOffer)

      fetch(`http://localhost:5161/api/JobApplications/${id}/notes`)
        .then(r => r.json())
        .then(setNotes)
    }
  }, [id])

  const handleDeleteNote = async (noteId: number) => {
    const r = await fetch(`http://localhost:5161/api/JobApplications/${id}/notes/${noteId}`, {
      method: 'DELETE'
    })
    if (r.ok) {
      setNotes(prev => prev.filter(n => n.id !== noteId))
    }
  }

  const handleAddNote = async () => {
    if (!noteContent.trim()) return
    const r = await fetch(`http://localhost:5161/api/JobApplications/${id}/notes`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ eventDate: noteDate, content: noteContent })
    })
    if (r.ok) {
      const created: JobOfferNote = await r.json()
      setNotes(prev => [created, ...prev])
      setNoteContent('')
      setNoteDate(new Date().toISOString().slice(0, 16))
    }
  }

  if (!offer) return <div>Ładowanie...</div>

  const salary = salaryRange(offer)

  return (
    <div>
      <h2>{offer.position}</h2>
      <button className="btn" onClick={() => navigate(`/edit/${offer.id}`)}>Edytuj ofertę</button>

      {offer.sourceUrl && (
        <p>
          <strong>Źródło:</strong>{' '}
          <a href={offer.sourceUrl} target="_blank" rel="noopener noreferrer">
            {offer.sourceUrl}
          </a>
        </p>
      )}

      {salary && <p><strong>Wynagrodzenie:</strong> {salary}</p>}

      <p><strong>Firma:</strong> {offer.companyName ?? '—'}</p>
      <p><strong>Lokalizacja:</strong> {offer.location ?? '—'}</p>
      <p><strong>Typ umowy:</strong> {offer.contractType}</p>
      <p><strong>Tryb pracy:</strong> {offer.workMode}</p>
      <p><strong>Wymiar:</strong> {offer.workLoad}</p>
      <p><strong>Status:</strong> {offer.status}</p>
      {offer.skills && <p><strong>Umiejętności:</strong> {offer.skills}</p>}
      {offer.appliedAt && <p><strong>Data aplikacji:</strong> {formatDate(offer.appliedAt)}</p>}
      {offer.followUpDate && <p><strong>Follow-up:</strong> {formatDate(offer.followUpDate)}</p>}
      {offer.recruiterName && <p><strong>Rekruter:</strong> {offer.recruiterName}</p>}
      {offer.recruiterContact && <p><strong>Kontakt do rekrutera:</strong> {offer.recruiterContact}</p>}
      {offer.sentCvVersion && <p><strong>Wersja CV:</strong> {offer.sentCvVersion}</p>}
      {offer.rejectionReason && <p><strong>Powód odrzucenia:</strong> {offer.rejectionReason}</p>}

      <hr />
      <h3>Notatki</h3>

      <div>
        <input
          type="datetime-local"
          className="form-field"
          value={noteDate}
          onChange={e => setNoteDate(e.target.value)}
        />
        <textarea
          className="form-field"
          rows={3}
          value={noteContent}
          onChange={e => setNoteContent(e.target.value)}
          placeholder="Treść notatki..."
        />
        <button className="btn" onClick={handleAddNote}>Dodaj notatkę</button>
      </div>

      {notes.length === 0 && <p>Brak notatek.</p>}
      {notes.map(note => (
        <div key={note.id} style={{ borderBottom: '1px solid #ccc', padding: '8px 0' }}>
          <p style={{ margin: 0, fontSize: '0.85em', color: '#666' }}>{formatDate(note.eventDate)}</p>
          <p style={{ margin: '4px 0' }}>{note.content}</p>
          <button className="btn" onClick={() => handleDeleteNote(note.id)}>Usuń</button>
        </div>
      ))}
    </div>
  )
}
