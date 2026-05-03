import { useEffect, useState } from "react"
import type { JobOffer } from "../../models/JobOffer"
import { applicationStatusOptions, type ApplicationStatus } from "../../models/ApplicationStatus"
import StatusColumn from "../components/StatusColumn"


export default function Dashboard() {
    const [offers, setOffers] = useState<JobOffer[]>([])
    
    const handleDrop = async (offerId: number, newStatus: ApplicationStatus) => {
        const response = await fetch(`/api/jobapplications/${offerId}/status`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newStatus) })
        
        if(!response.ok) {
            window.alert("Nie udało się zaktualizować statusu oferty")
            return
        }
        
        setOffers(prev =>
        prev.map(o => o.id === offerId ? { ...o, status: newStatus } : o)
    )
       
    }
    
    useEffect(() => {
        const load = async () => {
            const response = await fetch('/api/jobapplications')
            if (!response.ok) {
                window.alert('Nie udało się załadować ofert')
                return
            }
            setOffers(await response.json())
        }
        load()
    }, [])
    return (
        <div className="status-board">
            {applicationStatusOptions.map(status => (
                <StatusColumn
                    key={status}
                    status={status}
                    offers={offers.filter(o => o.status === status)}
                    onDrop={handleDrop}
                />
            ))}
        </div>
    )
}