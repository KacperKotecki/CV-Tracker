import { useEffect, useState } from "react"
import JobOfferCard from "../components/JobOfferCard"
import type { JobOffer } from "../../models/JobOffer"
import { applicationStatusOptions } from "../../models/ApplicationStatus"
import StatusColumn from "../components/StatusColumn"


export default function OfferHomePage() {
    const [offers, setOffers] = useState<JobOffer[]>([])
    
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
                />
            ))}
        </div>
    )
}