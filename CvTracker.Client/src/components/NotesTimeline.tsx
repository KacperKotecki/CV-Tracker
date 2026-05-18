import { useState } from 'react'
import type { JobOfferNote } from '../../models/JobOfferNote'
import './NotesTimeline.css'

interface Props {
  notes: JobOfferNote[]
  offerId: number
  onAdd: (content: string, date: string) => void
  onDelete: (noteId: number) => void
}

function formatDate(value: string): string {
  return new Date(value).toLocaleString('pl-PL')
}

export default function NotesTimeline({ notes, onAdd, onDelete }: Props) {
  const [content, setContent] = useState('')
  const [date, setDate] = useState(() => new Date().toISOString().slice(0, 16))

  const handleSend = () => {
    if (!content.trim()) return
    onAdd(content, date)
    setContent('')
    setDate(new Date().toISOString().slice(0, 16))
  }

  return (
    <div className="notes-timeline">
      <h3 className="notes-timeline__title">Notatki</h3>

      <div className="notes-timeline__input-bar">
        <input
          type="datetime-local"
          className="form-field notes-timeline__date-input"
          value={date}
          onChange={e => setDate(e.target.value)}
        />
        <textarea
          className="form-field notes-timeline__textarea"
          rows={2}
          value={content}
          onChange={e => setContent(e.target.value)}
          placeholder="Treść notatki..."
        />
        <button className="btn" onClick={handleSend}>Wyślij</button>
      </div>

      <div className="notes-timeline__list">
        {notes.length === 0 && (
          <p className="notes-timeline__empty">Brak notatek.</p>
        )}
        {notes.map(note => (
          <div key={note.id} className="notes-timeline__item">
            <div className="notes-timeline__track">
              <div className="notes-timeline__dot" />
              <div className="notes-timeline__line" />
            </div>
            <div className="notes-timeline__body">
              <span className="notes-timeline__date">{formatDate(note.eventDate)}</span>
              <p className="notes-timeline__content">{note.content}</p>
              <button className="btn notes-timeline__delete-btn" onClick={() => onDelete(note.id)}>
                Usuń
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
