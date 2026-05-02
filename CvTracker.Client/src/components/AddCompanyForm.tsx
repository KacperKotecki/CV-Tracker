import { useState } from "react"
import type { Company } from "../../models/Company"

interface AddCompanyFormProps {
  onCompanyAdded: (company: Company) => void
}

export default function AddCompanyForm({ onCompanyAdded }: AddCompanyFormProps) {
  const emptyForm = {
    companyName: '',
    companyAddress: ''
  }
  const [form, setForm] = useState(emptyForm)

  const addCompany = async () => {
    if (Object.values(form).some(x => x === '')) {
      window.alert("Wszystkie pola są wymagane")
      return
    }
    const response = await fetch('http://localhost:5211/api/companies', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ companyName: form.companyName, companyAddress: form.companyAddress })
    })
    const company: Company = await response.json()
    onCompanyAdded(company)
    setForm(emptyForm)
  }

  return (
    <div className="form-section">
      <h2>Dodaj firmę:</h2>
      <input className="form-field" type="text" value={form.companyName} onChange={(e) => setForm({ ...form, companyName: e.target.value })} placeholder="Nazwa firmy" />
      <input className="form-field" type="text" value={form.companyAddress} onChange={(e) => setForm({ ...form, companyAddress: e.target.value })} placeholder="Adres firmy" />
      <button className="btn" onClick={addCompany}>Dodaj firmę</button>
    </div>
  )
}
