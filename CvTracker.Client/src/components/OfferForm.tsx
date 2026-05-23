import { useEffect, useState } from 'react'
import type { JobOffer } from '../../models/JobOffer'
import { applicationStatusOptions, type ApplicationStatus } from '../../models/ApplicationStatus'
import { contractTypeOptions } from '../../models/ContractType'
import { workModeOptions } from '../../models/WorkMode'
import { workLoadOptions } from '../../models/WorkLoad'
import OfferSkillPicker from './OfferSkillPicker'
import './OfferForm.css'
import './OfferSkillPicker.css'

interface ScrapedOffer {
  position: string | null
  salaryMin: number | null
  salaryMax: number | null
  contractType: string | null
  workMode: string | null
  workLoad: string | null
  requiredSkills: string[]
  companyName: string | null
  location: string | null
}

function toDateTimeLocal(value: string | null | undefined): string {
  if (!value) return ''
  return value.slice(0, 16)
}

interface Props {
  offer: JobOffer | null
  onSave: (dto: Partial<JobOffer>) => void
  onCancel: () => void
}

const emptyForm = {
  position: '',
  salaryMin: '',
  salaryMax: '',
  contractType: '',
  workMode: '',
  workLoad: '',
  companyName: '',
  location: '',
  sourceUrl: '',
  requiredSkills: [] as string[],
  appliedAt: '',
  followUpDate: '',
  recruiterName: '',
  recruiterContact: '',
  sentCvVersion: '',
  rejectionReason: '',
  status: applicationStatusOptions[0] as ApplicationStatus,
}

export default function OfferForm({ offer, onSave, onCancel }: Props) {
  const [form, setForm] = useState(emptyForm)
  const [offerUrl, setOfferUrl] = useState('')
  const [isScraping, setIsScraping] = useState(false)

  useEffect(() => {
    if (offer) {
      setForm({
        position:         offer.position         ?? '',
        salaryMin:        offer.salaryMin         != null ? String(offer.salaryMin) : '',
        salaryMax:        offer.salaryMax         != null ? String(offer.salaryMax) : '',
        contractType:     offer.contractType      ?? '',
        workMode:         offer.workMode          ?? '',
        workLoad:         offer.workLoad          ?? '',
        companyName:      offer.companyName       ?? '',
        location:         offer.location          ?? '',
        sourceUrl:        offer.sourceUrl         ?? '',
        requiredSkills:   offer.requiredSkills     ?? [],
        appliedAt:        toDateTimeLocal(offer.appliedAt),
        followUpDate:     toDateTimeLocal(offer.followUpDate),
        recruiterName:    offer.recruiterName     ?? '',
        recruiterContact: offer.recruiterContact  ?? '',
        sentCvVersion:    offer.sentCvVersion     ?? '',
        rejectionReason:  offer.rejectionReason   ?? '',
        status:           offer.status            ?? applicationStatusOptions[0],
      })
    } else {
      setForm(emptyForm)
    }
  }, [offer])

  const handleScrape = async () => {
    if (!offerUrl.trim()) return
    setIsScraping(true)
    try {
      const r = await fetch('/api/scrape', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ url: offerUrl }),
      })
      if (!r.ok) {
        const msg = await r.text()
        window.alert(`Błąd: ${msg}`)
        return
      }
      const data: ScrapedOffer = await r.json()
      setForm(prev => ({
        ...prev,
        position:     data.position     ?? prev.position,
        salaryMin:    data.salaryMin    != null ? String(data.salaryMin) : prev.salaryMin,
        salaryMax:    data.salaryMax    != null ? String(data.salaryMax) : prev.salaryMax,
        contractType: data.contractType ?? prev.contractType,
        workMode:     data.workMode     ?? prev.workMode,
        workLoad:     data.workLoad     ?? prev.workLoad,
        companyName:  data.companyName  ?? prev.companyName,
        location:     data.location     ?? prev.location,
        requiredSkills: data.requiredSkills?.length ? data.requiredSkills : prev.requiredSkills,
        sourceUrl:    offerUrl,
      }))
    } catch {
      window.alert('Błąd połączenia z serwerem.')
    } finally {
      setIsScraping(false)
    }
  }

  const handleSave = () => {
    if (!form.position.trim() || !form.contractType || !form.workMode || !form.workLoad) {
      window.alert('Stanowisko, typ umowy, tryb i wymiar pracy są wymagane')
      return
    }
    onSave({
      id:               offer?.id,
      position:         form.position,
      salaryMin:        form.salaryMin    !== '' ? Number(form.salaryMin)  : null,
      salaryMax:        form.salaryMax    !== '' ? Number(form.salaryMax)  : null,
      contractType:     form.contractType,
      workMode:         form.workMode,
      workLoad:         form.workLoad,
      companyName:      form.companyName      || null,
      location:         form.location         || null,
      sourceUrl:        form.sourceUrl        || null,
      requiredSkills:   form.requiredSkills,
      appliedAt:        form.appliedAt        || null,
      followUpDate:     form.followUpDate     || null,
      recruiterName:    form.recruiterName    || null,
      recruiterContact: form.recruiterContact || null,
      sentCvVersion:    form.sentCvVersion    || null,
      rejectionReason:  form.rejectionReason  || null,
      status:           form.status,
    })
  }

  return (
    <div className="offer-form">
      <div className="offer-form__header">
        <h2 className="offer-form__title">{offer ? 'Edytuj ofertę' : 'Nowa oferta'}</h2>
        <div className="offer-form__header-actions">
          <button className="btn" onClick={handleSave} disabled={isScraping}>
            Zapisz
          </button>
          <button className="btn-secondary" onClick={onCancel} disabled={isScraping}>
            Anuluj
          </button>
        </div>
      </div>

      <div className="offer-form__scrape-row">
        <input
          className="form-field offer-form__url-input"
          type="url"
          value={offerUrl}
          onChange={e => setOfferUrl(e.target.value)}
          placeholder="Wklej URL oferty, aby pobrać dane..."
          disabled={isScraping}
        />
        <button className="btn" onClick={handleScrape} disabled={isScraping}>
          {isScraping ? 'Pobieranie...' : 'Pobierz dane'}
        </button>
      </div>

      <fieldset className="offer-form__fields" disabled={isScraping}>
        <div className="offer-form__grid">
          <input className="form-field" type="text" value={form.position} onChange={e => setForm({ ...form, position: e.target.value })} placeholder="Stanowisko *" />
          <input className="form-field" type="text" value={form.companyName} onChange={e => setForm({ ...form, companyName: e.target.value })} placeholder="Nazwa firmy" />
          <input className="form-field" type="text" value={form.location} onChange={e => setForm({ ...form, location: e.target.value })} placeholder="Lokalizacja" />
          <select className="form-field" value={form.contractType} onChange={e => setForm({ ...form, contractType: e.target.value })}>
            <option value=''>Wybierz typ umowy *</option>
            {contractTypeOptions.map(o => <option key={o} value={o}>{o}</option>)}
          </select>
          <select className="form-field" value={form.workMode} onChange={e => setForm({ ...form, workMode: e.target.value })}>
            <option value=''>Wybierz tryb pracy *</option>
            {workModeOptions.map(o => <option key={o} value={o}>{o}</option>)}
          </select>
          <select className="form-field" value={form.workLoad} onChange={e => setForm({ ...form, workLoad: e.target.value })}>
            <option value=''>Wybierz wymiar czasu *</option>
            {workLoadOptions.map(o => <option key={o} value={o}>{o}</option>)}
          </select>
          <input className="form-field" type="number" value={form.salaryMin} onChange={e => setForm({ ...form, salaryMin: e.target.value })} placeholder="Wynagrodzenie od (PLN)" />
          <input className="form-field" type="number" value={form.salaryMax} onChange={e => setForm({ ...form, salaryMax: e.target.value })} placeholder="Wynagrodzenie do (PLN)" />
          <select className="form-field" value={form.status} onChange={e => setForm({ ...form, status: e.target.value as ApplicationStatus })}>
            {applicationStatusOptions.map(o => <option key={o} value={o}>{o}</option>)}
          </select>
          <input className="form-field" type="url" value={form.sourceUrl} onChange={e => setForm({ ...form, sourceUrl: e.target.value })} placeholder="URL źródłowy" />
          <input className="form-field" type="text" value={form.recruiterName} onChange={e => setForm({ ...form, recruiterName: e.target.value })} placeholder="Imię rekrutera" />
          <input className="form-field" type="text" value={form.recruiterContact} onChange={e => setForm({ ...form, recruiterContact: e.target.value })} placeholder="Kontakt do rekrutera" />
          <div className="offer-form__col-full">
            <OfferSkillPicker value={form.requiredSkills} onChange={v => setForm({ ...form, requiredSkills: v })} />
          </div>
          <div className="offer-form__col-full">
            <label className="offer-form__label">Data aplikacji</label>
            <input className="form-field" type="datetime-local" value={form.appliedAt} onChange={e => setForm({ ...form, appliedAt: e.target.value })} />
          </div>
          <input className="form-field" type="text" value={form.sentCvVersion} onChange={e => setForm({ ...form, sentCvVersion: e.target.value })} placeholder="Wersja CV" />
          <input className="form-field" type="text" value={form.rejectionReason} onChange={e => setForm({ ...form, rejectionReason: e.target.value })} placeholder="Powód odrzucenia" />
        </div>
      </fieldset>
    </div>
  )
}
