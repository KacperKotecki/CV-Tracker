import { useState } from "react"
import { useNavigate } from "react-router-dom"

export default function OfferDetailPage() {
    const navigate = useNavigate()
    const [form, setForm] = useState({ title: '', content: '' })
    
    return <div>Szczegóły oferty</div>
}