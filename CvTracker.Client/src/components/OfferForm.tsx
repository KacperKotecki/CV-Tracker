import { useEffect, useState } from 'react'
import type { JobOffer } from '../../models/JobOffer'
import type { TechnologyCategory } from '../../models/Technology'
import { applicationStatusOptions, type ApplicationStatus } from '../../models/ApplicationStatus'
import { contractTypeOptions } from '../../models/ContractType'
import { workModeOptions } from '../../models/WorkMode'
import { workLoadOptions } from '../../models/WorkLoad'
import OfferSkillPicker, { type OfferSkillItem } from './OfferSkillPicker'
import './OfferForm.css'

/** Shape returned by POST /api/parse. */
interface ScrapedOffer {
  position: string | null
  salaryMin: number | null
  salaryMax: number | null
  contractType: string | null
  workMode: string | null
  workLoad: string | null
  requiredSkillIds: number[]
  companyName: string | null
  location: string | null
}

function toDateTimeLocal(value: string | null | undefined): string {
  if (!value) return ''
  return value.slice(0, 16)
}

interface Props {
  offer: JobOffer | null
  categories: TechnologyCategory[]
  onSave: (dto: Partial<JobOffer>) => void
  onCancel: () => void
}

function makeEmptyForm() {
  const now = new Date()
  const pad = (n: number) => String(n).padStart(2, '0')
  const appliedAt = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}T${pad(now.getHours())}:${pad(now.getMinutes())}`
  return {
    position: '',
    salaryMin: '',
    salaryMax: '',
    contractType: 'UoP',
    workMode: 'OnSite',
    workLoad: 'FullTime',
    companyName: '',
    location: '',
    sourceUrl: '',
    requiredSkills: [] as OfferSkillItem[],
    appliedAt,
    followUpDate: '',
    recruiterName: '',
    recruiterContact: '',
    sentCvVersion: '',
    rejectionReason: '',
    status: applicationStatusOptions[0] as ApplicationStatus,
  }
}

export default function OfferForm({ offer, categories, onSave, onCancel }: Props) {
  const [form, setForm] = useState(makeEmptyForm)
  const [isPasting, setIsPasting] = useState(false)
  const [pasteError, setPasteError] = useState<string | null>(null)

  useEffect(() => {
    if (offer) {
      // Reconstruct requiredSkills from either the server-populated field or fall back
      // to requiredSkillIds with a default Mid level.
      const requiredSkills: OfferSkillItem[] = offer.requiredSkills
        ? offer.requiredSkills.map(s => ({ technologyId: s.technologyId, requiredLevel: s.requiredLevel }))
        : (offer.requiredSkillIds ?? []).map(id => ({
            technologyId: id,
            requiredLevel: offer.requiredSkillLevels?.[id] ?? 'Mid',
          }))

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
        requiredSkills,
        appliedAt:        toDateTimeLocal(offer.appliedAt),
        followUpDate:     toDateTimeLocal(offer.followUpDate),
        recruiterName:    offer.recruiterName     ?? '',
        recruiterContact: offer.recruiterContact  ?? '',
        sentCvVersion:    offer.sentCvVersion     ?? '',
        rejectionReason:  offer.rejectionReason   ?? '',
        status:           offer.status            ?? applicationStatusOptions[0],
      })
    } else {
      setForm(makeEmptyForm())
    }
  }, [offer])

  /**
   * Reads text from the system clipboard, validates length, sends it to
   * POST /api/parse, and merges the returned fields into the form state.
   * Non-null returned values overwrite the current form values.
   */
  const handlePaste = async () => {
    setPasteError(null)

    let text: string
    try {
      text = await navigator.clipboard.readText()
    } catch {
      setPasteError('Nie udało się odczytać schowka. Sprawdź uprawnienia przeglądarki.')
      return
    }

    // Client-side guard: reject obviously empty or binary-looking content.
    const trimmed = text.trim()
    if (!trimmed || trimmed.length < 50) {
      setPasteError('Schowek jest pusty lub tekst jest za krótki (min. 50 znaków).')
      return
    }

    // Reject text that looks like binary data (high ratio of non-printable chars).
    // eslint-disable-next-line no-control-regex
    const nonPrintable = (trimmed.match(/[^\u0009\u000A\u000D\u0020-\u007E\u00A0-\uFFFF]/g) ?? []).length
    if (nonPrintable / trimmed.length > 0.1) {
      setPasteError('Schowek zawiera dane binarne — wklej tekst ze strony oferty.')
      return
    }

    // Truncate at 20 000 chars before sending (server also truncates, but be explicit).
    const payload = trimmed.length > 20_000 ? trimmed.slice(0, 20_000) : trimmed

    setIsPasting(true)
    try {
      const r = await fetch('/api/parse', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ text: payload }),
      })
      if (!r.ok) {
        setPasteError(await r.text())
        return
      }
      const data: ScrapedOffer = await r.json()
      setForm(prev => {
        const parsedSkills: OfferSkillItem[] = data.requiredSkillIds?.length
          ? data.requiredSkillIds.map(id => ({ technologyId: id, requiredLevel: 'Mid' }))
          : prev.requiredSkills
        return {
          ...prev,
          position:      data.position      ?? prev.position,
          salaryMin:     data.salaryMin      != null ? String(data.salaryMin) : prev.salaryMin,
          salaryMax:     data.salaryMax      != null ? String(data.salaryMax) : prev.salaryMax,
          contractType:  data.contractType  ?? prev.contractType,
          workMode:      data.workMode      ?? prev.workMode,
          workLoad:      data.workLoad      ?? prev.workLoad,
          companyName:   data.companyName   ?? prev.companyName,
          location:      data.location      ?? prev.location,
          requiredSkills: parsedSkills,
        }
      })
    } catch {
      setPasteError('Błąd połączenia z serwerem.')
    } finally {
      setIsPasting(false)
    }
  }

  const handleSave = () => {
    if (!form.position.trim() || !form.companyName.trim()) {
      window.alert('Stanowisko i nazwa firmy są wymagane')
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
          <button className="btn" onClick={handleSave} disabled={isPasting}>
            Zapisz
          </button>
          <button className="btn-secondary" onClick={onCancel} disabled={isPasting}>
            Anuluj
          </button>
        </div>
      </div>

      <div className="offer-form__paste-row">
        <button className="btn" onClick={handlePaste} disabled={isPasting}>
          {isPasting ? 'Analizowanie...' : '📋 Wklej skopiowane'}
        </button>
        {pasteError && (
          <p className="offer-form__paste-error">{pasteError}</p>
        )}
      </div>

      <fieldset className="offer-form__fields" disabled={isPasting}>
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
            <OfferSkillPicker categories={categories} value={form.requiredSkills} onChange={v => setForm({ ...form, requiredSkills: v })} />
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
