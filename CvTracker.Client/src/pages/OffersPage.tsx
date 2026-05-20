import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import type { JobOffer } from '../../models/JobOffer'
import type { JobOfferNote } from '../../models/JobOfferNote'
import type { ApplicationStatus } from '../../models/ApplicationStatus'
import OfferListPanel from '../components/OfferListPanel'
import OfferDetailPanel from '../components/OfferDetailPanel'
import './OffersPage.css'

type PanelMode = 'empty' | 'detail' | 'edit' | 'add'

export default function OffersPage() {
  const [offers, setOffers] = useState<JobOffer[]>([])
  const [search, setSearch] = useState('')
  const [searchParams, setSearchParams] = useSearchParams()

  const selectedIdParam = searchParams.get('id')
  const selectedId = selectedIdParam ? Number(selectedIdParam) : null
  const selectedOffer = selectedId != null ? (offers.find(o => o.id === selectedId) ?? null) : null

  const [panelMode, setPanelMode] = useState<PanelMode>(selectedId != null ? 'detail' : 'empty')

  useEffect(() => {
    fetch('/api/jobapplications')
      .then(r => r.json())
      .then(setOffers)
  }, [])

  const handleSelectOffer = (id: number) => {
    setSearchParams({ id: String(id) })
    setPanelMode('detail')
  }

  const handleAddOffer = () => {
    setSearchParams({})
    setPanelMode('add')
  }

  const handleEditOffer = () => {
    setPanelMode('edit')
  }

  const handleSaveOffer = async (dto: Partial<JobOffer>) => {
    if (dto.id != null) {
      // PUT — returns 204 NoContent
      const r = await fetch(`/api/jobapplications/${dto.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(dto),
      })
      if (!r.ok) {
        const msg = await r.text()
        window.alert(`Błąd zapisu: ${msg}`)
        return
      }
      setOffers(prev =>
        prev.map(o => o.id === dto.id ? { ...o, ...dto, notes: o.notes } as JobOffer : o)
      )
      setPanelMode('detail')
    } else {
      // POST — returns 201 with created offer
      const r = await fetch('/api/jobapplications', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(dto),
      })
      if (!r.ok) {
        const msg = await r.text()
        window.alert(`Błąd zapisu: ${msg}`)
        return
      }
      const created: JobOffer = await r.json()
      setOffers(prev => [created, ...prev])
      setSearchParams({ id: String(created.id) })
      setPanelMode('detail')
    }
  }

  const handleCancelEdit = () => {
    if (selectedId != null) {
      setPanelMode('detail')
    } else {
      setPanelMode('empty')
    }
  }

  const handleStatusChange = async (id: number, status: ApplicationStatus) => {
    const r = await fetch(`/api/jobapplications/${id}/status`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(status),
    })
    if (r.ok) {
      setOffers(prev => prev.map(o => o.id === id ? { ...o, status } : o))
    }
  }

  const handleAddNote = async (content: string, date: string) => {
    if (selectedId == null) return
    const r = await fetch(`/api/jobapplications/${selectedId}/notes`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ eventDate: date, content }),
    })
    if (r.ok) {
      const created: JobOfferNote = await r.json()
      setOffers(prev => prev.map(o =>
        o.id === selectedId
          ? { ...o, notes: [created, ...o.notes] }
          : o
      ))
    }
  }

  const handleDeleteNote = async (noteId: number) => {
    if (selectedId == null) return
    const r = await fetch(`/api/jobapplications/${selectedId}/notes/${noteId}`, {
      method: 'DELETE',
    })
    if (r.ok) {
      setOffers(prev => prev.map(o =>
        o.id === selectedId
          ? { ...o, notes: o.notes.filter(n => n.id !== noteId) }
          : o
      ))
    }
  }

  const filteredOffers = offers.filter(o => {
    const q = search.toLowerCase()
    return (
      (o.companyName ?? '').toLowerCase().includes(q) ||
      o.position.toLowerCase().includes(q)
    )
  })

  return (
    <div className="offers-page">
      <div className="offers-page__left">
        <OfferListPanel
          offers={filteredOffers}
          selectedId={selectedId}
          search={search}
          onSearchChange={setSearch}
          onSelect={handleSelectOffer}
          onAddOffer={handleAddOffer}
        />
      </div>
      <div className="offers-page__right">
        <OfferDetailPanel
          mode={panelMode}
          offer={selectedOffer}
          notes={selectedOffer?.notes ?? []}
          onEdit={handleEditOffer}
          onSave={handleSaveOffer}
          onCancel={handleCancelEdit}
          onStatusChange={handleStatusChange}
          onAddNote={handleAddNote}
          onDeleteNote={handleDeleteNote}
        />
      </div>
    </div>
  )
}
