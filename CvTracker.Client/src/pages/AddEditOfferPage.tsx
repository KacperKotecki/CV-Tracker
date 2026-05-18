import { useEffect, useState } from "react"
import { contractTypeOptions } from "../../models/ContractType"
import { workModeOptions } from "../../models/WorkMode"
import { workLoadOptions } from "../../models/WorkLoad"
import { useNavigate, useParams } from "react-router-dom"
import { applicationStatusOptions, type ApplicationStatus } from "../../models/ApplicationStatus"

interface ScrapedOffer {
  position: string | null
  salary: number | null
  contractType: string | null
  workMode: string | null
  workLoad: string | null
  skills: string | null
  ourRequirements: string | null
  whatWeOffer: string | null
  benefits: string | null
  companyName: string | null
  location: string | null
}

export default function AddEditOfferPage() {
  const { id } = useParams()

  const emptyForm = {
    position: '',
    salary: 0,
    contractType: '',
    workMode: '',
    workLoad: '',
    companyName: '',
    location: '',
    sourceUrl: '',
    skills: '',
    ourRequirements: '',
    whatWeOffer: '',
    benefits: '',
    status: applicationStatusOptions[0] as ApplicationStatus
  }

  const [form, setForm] = useState(emptyForm)
  const [offerUrl, setOfferUrl] = useState('')
  const [isScraping, setIsScraping] = useState(false)
  const navigate = useNavigate()

  const fetchOfferData = async () => {
    if (!offerUrl.trim()) return
    setIsScraping(true)
    try {
      const r = await fetch('/api/scrape', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ url: offerUrl })
      })
      if (!r.ok) {
        const msg = await r.text()
        window.alert(`Błąd: ${msg}`)
        return
      }
      const data: ScrapedOffer = await r.json()
      setForm(prev => ({
        ...prev,
        position:        data.position        ?? prev.position,
        salary:          data.salary          ?? prev.salary,
        contractType:    data.contractType    ?? prev.contractType,
        workMode:        data.workMode        ?? prev.workMode,
        workLoad:        data.workLoad        ?? prev.workLoad,
        companyName:     data.companyName     ?? prev.companyName,
        location:        data.location        ?? prev.location,
        skills:          data.skills          ?? prev.skills,
        ourRequirements: data.ourRequirements ?? prev.ourRequirements,
        whatWeOffer:     data.whatWeOffer     ?? prev.whatWeOffer,
        benefits:        data.benefits        ?? prev.benefits,
        sourceUrl:       offerUrl,
      }))
    } catch {
      window.alert('Błąd połączenia z serwerem.')
    } finally {
      setIsScraping(false)
    }
  }

  useEffect(() => {
    if (id != undefined) {
      fetch(`/api/JobApplications/${id}`)
        .then(r => r.json())
        .then(setForm)
    }
  }, [id])

const addEditJobOffer = async () => {
    if (!form.position.trim() || form.salary <= 0 || !form.contractType || !form.workMode || !form.workLoad) {
      window.alert("Stanowisko, wynagrodzenie, typ umowy, tryb i wymiar pracy są wymagane")
      return
    }

    try {
      const url = id != undefined ? `/api/JobApplications/${id}` : '/api/JobApplications'
      const method = id != undefined ? 'PUT' : 'POST'

      const r = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          id: id != undefined ? Number(id) : undefined,
          position: form.position,
          salary: form.salary,
          contractType: form.contractType,
          workMode: form.workMode,
          workLoad: form.workLoad,
          companyName: form.companyName || null,
          location: form.location || null,
          sourceUrl: form.sourceUrl || null,
          skills: form.skills,
          ourRequirements: form.ourRequirements,
          whatWeOffer: form.whatWeOffer,
          benefits: form.benefits,
          status: form.status
        })
      })

      if (!r.ok) {
        const msg = await r.text()
        window.alert(`Błąd zapisu: ${msg}`)
        return
      }
    } catch {
      window.alert('Błąd połączenia z serwerem.')
      return
    }

    navigate('/')
    setForm(emptyForm)
  }

  return (
    <>
      <div className="form-section">
        <h2>Pobierz dane z oferty:</h2>
        <div style={{ display: 'flex', gap: '8px', alignItems: 'center' }}>
          <input
            className="form-field"
            style={{ marginBottom: 0, flex: 1 }}
            type="url"
            value={offerUrl}
            onChange={(e) => setOfferUrl(e.target.value)}
            placeholder="https://..."
            disabled={isScraping}
          />
          <button
            className="btn"
            onClick={fetchOfferData}
            disabled={isScraping}
            style={{ marginTop: 0, whiteSpace: 'nowrap' }}
          >
            {isScraping ? 'Pobieranie...' : 'Pobierz dane'}
          </button>
        </div>
      </div>

      <div className="form-section">
        <h2>Dodaj ofertę pracy:</h2>
        <fieldset disabled={isScraping} style={{ border: 'none', padding: 0, margin: 0 }}>
      <input className="form-field" type="text" value={form.position} onChange={(e) => setForm({ ...form, position: e.target.value })} placeholder="Stanowisko" />
      <input className="form-field" type="number" value={form.salary} onChange={(e) => setForm({ ...form, salary: Number(e.target.value) })} placeholder="Wypłata" />
      <select className="form-field" value={form.contractType} onChange={(e) => setForm({ ...form, contractType: e.target.value })}>
        <option value=''>Wybierz typ umowy</option>
        {contractTypeOptions.map(option => (
          <option key={option} value={option}>{option}</option>
        ))}
      </select>
      <select className="form-field" value={form.workMode} onChange={(e) => setForm({ ...form, workMode: e.target.value })}>
        <option value=''>Wybierz tryb pracy</option>
        {workModeOptions.map(option => (
          <option key={option} value={option}>{option}</option>
        ))}
      </select>
      <select className="form-field" value={form.workLoad} onChange={(e) => setForm({ ...form, workLoad: e.target.value })}>
        <option value=''>Wybierz wymiar czasu</option>
        {workLoadOptions.map(option => (
          <option key={option} value={option}>{option}</option>
        ))}
      </select>
      <input className="form-field" type="text" value={form.companyName} onChange={(e) => setForm({ ...form, companyName: e.target.value })} placeholder="Nazwa firmy" />
      <input className="form-field" type="text" value={form.location} onChange={(e) => setForm({ ...form, location: e.target.value })} placeholder="Lokalizacja (np. Warszawa, Zdalnie)" />
      <input className="form-field" type="url" value={form.sourceUrl ?? ''} onChange={(e) => setForm({ ...form, sourceUrl: e.target.value })} placeholder="Source URL (np. https://...)" />
      <textarea className="form-field" rows={2} value={form.skills ?? ''} onChange={(e) => setForm({ ...form, skills: e.target.value })} placeholder="Umiejętności" />
      <textarea className="form-field" rows={3} value={form.ourRequirements ?? ''} onChange={(e) => setForm({ ...form, ourRequirements: e.target.value })} placeholder="Nasze wymagania" />
      <textarea className="form-field" rows={3} value={form.whatWeOffer ?? ''} onChange={(e) => setForm({ ...form, whatWeOffer: e.target.value })} placeholder="Co oferujemy" />
      <textarea className="form-field" rows={2} value={form.benefits ?? ''} onChange={(e) => setForm({ ...form, benefits: e.target.value })} placeholder="Benefity" />
      <select className="form-field" value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value as ApplicationStatus })}>
        {applicationStatusOptions.map(option => (
          <option key={option} value={option}>{option}</option>
        ))}
      </select>
        <button className="btn" onClick={addEditJobOffer}>
          {id ? 'Zaktualizuj' : 'Dodaj'}
        </button>
        </fieldset>
      </div>
    </>
  );
}