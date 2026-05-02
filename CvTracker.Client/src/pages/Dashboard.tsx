import { useEffect, useState } from "react"
import type { JobOffer } from "../../models/JobOffer"
import { applicationStatusOptions, type ApplicationStatus } from "../../models/ApplicationStatus"
import StatusColumn from "../components/StatusColumn"


export default function OfferHomePage() {
    const [offers, setOffers] = useState<JobOffer[]>([])
    
    const handleDrop = (offerId: number, newStatus: ApplicationStatus) => {
        setOffers(prev =>
        prev.map(o => o.id === offerId ? { ...o, status: newStatus } : o)
    )
        fetch(`/api/jobapplications/${offerId}/status`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newStatus) })
    }
    
    useEffect(() => {
        fetch('/api/jobapplications').then(r => r.json()).then(setOffers)
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