import { useEffect, useState } from "react"
import { contractTypeOptions } from "../../models/ContractType"
import { workModeOptions } from "../../models/WorkMode"
import { workLoadOptions } from "../../models/WorkLoad"
import type { Company } from "../../models/Company"
import type { JobOffer } from "../../models/JobOffer"
import { useNavigate, useParams } from "react-router-dom"
import AddCompanyForm from "../components/AddCompanyForm"
import { applicationStatusOptions, type ApplicationStatus } from "../../models/ApplicationStatus"

interface AddJobOfferFormProps {
  companies: Company[]
  onJobOfferAdded: (jobOffer: JobOffer) => void
}

export default function AddEditOfferPage() {
  const { id } = useParams()

  let emptyForm = {
    position: '',
    salary: 0,
    contractType: '',
    workMode: '',
    workLoad: '',
    companyId: 0,
    skills: '',
    ourRequirements: '',
    whatWeOffer: '',
    benefits: '',
    status: applicationStatusOptions[0] as ApplicationStatus
  }


  

  const [form, setForm] = useState(emptyForm)
  const [companies, setCompanies] = useState<Company[]>([])
  const navigate = useNavigate()

  useEffect(() => {
    fetch('/api/companies')
      .then(r => r.json())
      .then(setCompanies)
    if (id != undefined) {
      fetch(`/api/JobApplications/${id}`)
        .then(r => r.json())
        .then(setForm)
    }
  }, [id])

const addEditJobOffer = async () => {
    if (Object.values(form).some(x => x === '' || x === 0)) {
      window.alert("Wszystkie pola są wymagane")
      return
    }
    if (form.salary < 0) {
      window.alert("Wynagrodzenie musi być większe niż 0")
      return
    }
    if (id != undefined) {

      await fetch(`/api/JobApplications/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          id: Number(id),
          position: form.position,
          salary: form.salary,
          contractType: form.contractType,
          workMode: form.workMode,
          workLoad: form.workLoad,
          companyId: form.companyId,
          skills: form.skills,
          ourRequirements: form.ourRequirements,
          whatWeOffer: form.whatWeOffer,
          benefits: form.benefits,
          status: form.status
        })
      })
    } else {
      await fetch('/api/JobApplications', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          position: form.position,
          salary: form.salary,
          contractType: form.contractType,
          workMode: form.workMode,
          workLoad: form.workLoad,
          companyId: form.companyId,
          skills: form.skills,
          ourRequirements: form.ourRequirements,
          whatWeOffer: form.whatWeOffer,
          benefits: form.benefits,
          status: form.status
        })
      })
    }
    navigate('/')
    setForm(emptyForm)
  }

  return (
    <>
      <div className="form-section">
        <h2>Dodaj ofertę pracy:</h2>
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
      <select className="form-field" value={form.companyId} onChange={(e) => setForm({ ...form, companyId: Number(e.target.value) })}>
        <option value={0}>Wybierz firmę</option>
        {companies.map(company => (
          <option key={company.id} value={company.id}>{company.companyName}</option>
        ))}
      </select>
      <textarea className="form-field" rows={2} value={form.skills} onChange={(e) => setForm({ ...form, skills: e.target.value })} placeholder="Umiejętności" />
      <textarea className="form-field" rows={3} value={form.ourRequirements} onChange={(e) => setForm({ ...form, ourRequirements: e.target.value })} placeholder="Nasze wymagania" />
      <textarea className="form-field" rows={3} value={form.whatWeOffer} onChange={(e) => setForm({ ...form, whatWeOffer: e.target.value })} placeholder="Co oferujemy" />
      <textarea className="form-field" rows={2} value={form.benefits} onChange={(e) => setForm({ ...form, benefits: e.target.value })} placeholder="Benefity" />
      <select className="form-field" value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value as ApplicationStatus })}>
        <option value=''>Wybierz status</option>
        {applicationStatusOptions.map(option => (
          <option key={option} value={option}>{option}</option>
        ))}
      </select>
        <button className="btn" onClick={addEditJobOffer}>
          {id ? 'Zaktualizuj' : 'Dodaj'}
        </button>
      </div>

      <AddCompanyForm onCompanyAdded={(company) => setCompanies(prev => [...prev, company])} />
    </>
  );
}